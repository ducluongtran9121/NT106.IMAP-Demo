using System;

namespace MailClient.Imap
{
    [Serializable]
    public class ConnectionException : Exception
    {
        public ConnectionException()
        {
        }

        public ConnectionException(string message)
            : base(message)
        {
        }
    }

    [Serializable]
    public class AuthenticationException : Exception
    {
        public AuthenticationException()
        {
        }

        public AuthenticationException(string message)
            : base(message)
        {
        }
    }

    [Serializable]
    public class WriteDataException : Exception
    {
        public WriteDataException()
        {
        }

        public WriteDataException(string message)
            : base(message)
        {
        }
    }

    [Serializable]
    public class ReadDataException : Exception
    {
        public ReadDataException()
        {
        }

        public ReadDataException(string message)
            : base(message)
        {
        }
    }
}