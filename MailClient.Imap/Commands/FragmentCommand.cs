using MailClient.Imap.Common;
using MailClient.Imap.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MailClient.Imap.Commands
{
    internal static class FragmentCommand
    {
        internal static bool Connect(byte[] data)
        {
            return Encoding.ASCII.GetString(data) == "* OK IMAP4rev1 Service Ready\r\n";
        }

        public static bool Bad(string data)
        {
            string[] frag = data.Trim().Split(" ");

            return frag[1] == "BAD" || frag[1] == "NO";
        }

        public static bool Authenticate(byte[] data)
        {
            string[] frag = Encoding.UTF8.GetString(data).Trim().Split(" ");

            return frag[1] == "OK" && frag[2] == CommandType.LOGIN.ToString();
        }

        public static Folder SelectFolder(byte[] data)
        {
            string[] lines = Encoding.ASCII.GetString(data).Trim().Split("\r\n");
            int exists = 0, recent = 0, unseen = 0, uidValidity = 0, uidNext = 0;
            List<MessageFlag> flags = new(), permanentFlags = new();
            FolderAccess access = FolderAccess.ReadOnly;

            _ = Parallel.ForEach(lines, line =>
              {
                  if (line.Contains("EXISTS"))
                  {
                      exists = int.Parse(lines[0].Split(' ')[1]);
                      return;
                  }

                  if (line.Contains("RECENT"))
                  {
                      recent = int.Parse(line.Split(' ')[1]);
                      return;
                  }

                  if (line.Contains("UNSEEN"))
                  {
                      unseen = int.Parse(line.Split('[', ']')[1].Split(' ')[1]);
                      return;
                  }

                  if (line.Contains("UIDVALIDITY"))
                  {
                      uidValidity = int.Parse(line.Split('[', ']')[1].Split(' ')[1]);
                      return;
                  }

                  if (line.Contains("UIDNEXT"))
                  {
                      uidNext = int.Parse(line.Split('[', ']')[1].Split(' ')[1]);
                      return;
                  }

                  if (line.Contains("PERMANENTFLAGS"))
                  {
                      string[] fls = line[22..(line.Length - 3)].Replace("\\", string.Empty).Split(' ');
                      foreach (string fl in fls)
                      {
                          permanentFlags.Add((MessageFlag)Enum.Parse(typeof(MessageFlag), fl, true));
                      }
                      return;
                  }

                  if (line.Contains("FLAGS"))
                  {
                      string[] fls = line[9..(line.Length - 1)].Replace("\\", string.Empty).Split(' ');
                      foreach (string fl in fls)
                      {
                          flags.Add((MessageFlag)Enum.Parse(typeof(MessageFlag), fl, true));
                      }
                      return;
                  }

                  if (line.Contains("READ-WRITE"))
                  {
                      access = FolderAccess.ReadWrite;
                      return;
                  }

                  if (line.Contains("READ-ONLY"))
                  {
                      access = FolderAccess.ReadOnly;
                      return;
                  }
              });

            return new Folder(exists, recent, uidValidity, uidNext, access, flags.ToArray(), permanentFlags.ToArray());
        }

        public static List<int> Search(byte[] data)
        {
            List<int> uids = new();
            string[] lines = Encoding.ASCII.GetString(data).Trim().Split("\r\n");
            string[] stringData = lines[0].Split(' ');

            return new List<int>(stringData[2..].Select(x => int.Parse(x)).ToArray());
        }

        public static string[] List(byte[] data)
        {
            List<string> names = new();
            string[] lines = Encoding.ASCII.GetString(data).Trim().Split("\r\n");

            Regex regex = new(@"""\w+""");

            for (int i = 0; i < lines.Length - 1; i++)
            {
                Match match = regex.Match(lines[i]);
                if (match.Success)
                {
                    names.Add(match.Value.Replace("\"", ""));
                }
                else
                {
                    names.Add(lines[i].Split(' ')[4]);
                }
            }

            return names.ToArray();
        }

        public static Message FetchMessage(byte[] data)
        {
            Message message = new();

            string mailPattern = @"(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)";

            string tempData = Encoding.UTF8.GetString(data).Trim();

            string infoLine = Regex.Match(tempData, @"\* .+ FETCH(.[^\r\n]+)+").Value;

            message.Uid = long.Parse(Regex.Match(infoLine, @"UID [0-9]+").Value.Split(' ')[1]);

            message.Flags = new(Regex.Match(infoLine, @"FLAGS \(.*\)").Value.Split(new char[] { ' ', '\\', '(', ')' }, StringSplitOptions.RemoveEmptyEntries)[1..]
                                 .Select(x => (MessageFlag)Enum.Parse(typeof(MessageFlag), x, true)));

            string body = tempData[(infoLine.Length + 2)..];

            string[] lines = body.Split("\r\n");

            int trash = 0, line = 0;
            for (; line < lines.Length; line++)
            {
                string temp = lines[line];
                if (Regex.IsMatch(temp, @".+From") || !Regex.IsMatch(temp, @"From.+"))
                    trash += temp.Length + 2;
                else
                    break;
            }

            body = body[trash..];
            lines = lines[line..];
            trash = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string temp = lines[i];
                trash += temp.Length + 2;

                if (Regex.IsMatch(temp, @"From.+"))
                {
                    message.From = Regex.Match(temp, mailPattern).Value;
                    continue;
                }

                if (Regex.IsMatch(temp, @"To.+"))
                {
                    message.To = Regex.Match(temp, mailPattern).Value;
                    continue;
                }

                if (Regex.IsMatch(temp, @"Date.+"))
                {
                    message.DateTime = DateTime.Parse(temp[6..], System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);
                    continue;
                }

                if (Regex.IsMatch(temp, @"Subject.+"))
                {
                    if (Regex.IsMatch(temp, @"=\?.+\?.+\?.*\?="))
                    {
                        string subject = string.Empty;
                        Match tempMatch;
                        int j = i;
                        while ((tempMatch = Regex.Match(lines[j], @"=\?.+\?.+\?.*\?=")).Success)
                        {
                            string tempSubject = tempMatch.Value[1..(tempMatch.Value.Length - 1)];
                            string[] info = tempSubject.Split('?', StringSplitOptions.RemoveEmptyEntries);
                            subject += ConvertToString.FromString(info[2], info[1], info[0]);
                            j += 1;
                        }
                        message.Subject = subject;
                    }
                    else
                        message.Subject = temp[9..];

                    continue;
                }

                if (Regex.IsMatch(temp, @"Content-Type.+"))
                {
                    line = i;
                    trash -= temp.Length + 2;
                    break;
                }
            }

            body = body[trash..(body.Length - lines.Last().Length - 3)];

            string contentType;
            Match match;
            if ((match = Regex.Match(body, @"Content-Type.*\r\n\t.*\r\n")).Success)
            {
                contentType = match.Value.Replace("\r\n\t", string.Empty);
            }
            else
            {
                match = Regex.Match(body, @"Content-Type.*\r\n");
                contentType = match.Value.Replace(" ", string.Empty);
            }

            // if message contains single part
            if (!contentType.Contains("multipart"))
            {
                message.Body = Body.SinglePartBody(body, contentType, match.Value.Length);
            }
            else
            {
                message.Body = Body.MultiPartBody(body, contentType, match.Value.Length);
            }

            message.SetBodyPreview(message.Body);

            return message;
        }
    }
}