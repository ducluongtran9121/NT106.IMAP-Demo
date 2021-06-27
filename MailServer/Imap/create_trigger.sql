CREATE TRIGGER update_MailBox_after_insert_mail 
AFTER INSERT ON MailInfo
BEGIN
	UPDATE MailBoxInfo 
	SET mailexists = mailexists +1,
		recent = recent +1,
		uidnext = uidnext+1,
		firstunseen = 	CASE
							WHEN firstunseen = 0 AND NEW.seen = 1 THEN (SELECT count(*)from MailInfo where user = NEW.user AND mailboxname = NEW.mailboxname)
							ELSE firstunseen
						END
	WHERE user = NEW.user
	AND name = NEW.mailboxname;
END;

CREATE TRIGGER update_MailBox_default_attribute_with_name
AFTER INSERT ON MailBoxInfo
BEGIN
	UPDATE MailBox 
	SET sent = 
		CASE
			WHEN name LIKE 'sent items' THEN 1
			ELSE 0
  		END,
 		trash = 
 		CASE
 			WHEN name LIKE '%deleted items' THEN 1
 			ELSE 0 		END,
		inbox = 
 		CASE
 			WHEN name LIKE '%inbox' THEN 1
 			ELSE 0
 		END,
		drafts = 
		CASE
			WHEN name LIKE '%drafts' THEN 1
			ELSE 0
		END
	WHERE user = NEW.user AND name = NEW.name;
END;

CREATE TRIGGER update_MailBox_firstunseen_after_update_mail_seen_flag 
AFTER UPDATE OF seen ON MailInfo
BEGIN
	UPDATE MailBoxInfo 
	SET firstunseen = 	(
		WITH var AS (
			WITH var1 AS (SELECT ROW_NUMBER () OVER (ORDER BY uid ) RowNum,seen FROM MailInfo WHERE user = NEW.user and mailboxname = NEW.mailboxname)
			SELECT MIN(RowNum) AS MinRow FROM var1 WHERE seen=0)
		SELECT 
			CASE
				WHEN MinRow IS NULL THEN 0
				ELSE MinRow
			END result
		FROM var)
	WHERE user = NEW.user
	AND name = NEW.mailboxname;
END;

CREATE TRIGGER update_MailBox_recent_after_update_mail_recent_flag 
AFTER UPDATE OF recent ON MailInfo
BEGIN
	UPDATE MailBoxInfo 
	SET recent = (SELECT count(*) FROM MailInfo WHERE user = NEW.user AND mailboxname = NEW.mailboxname AND recent = 1)
	WHERE user = NEW.user
	AND name = NEW.mailboxname;
END;

CREATE TRIGGER update_MailBox_after_delete_mail 
AFTER DELETE ON MailInfo
BEGIN
	UPDATE MailBoxInfo 
	SET mailexists = mailexists - 1,
		recent = recent - OLD.recent,
		firstunseen = (
				WITH var AS (
					WITH var1 AS (SELECT ROW_NUMBER () OVER (ORDER BY uid ) RowNum,seen FROM MailInfo WHERE user = OLD.user and mailboxname = OLD.mailboxname)
					SELECT MIN(RowNum) AS MinRow FROM var1 WHERE seen=0)
				SELECT 
					CASE
						WHEN MinRow IS NULL THEN 0
						ELSE MinRow
					END result
				FROM var)
	WHERE user = OLD.user
	AND name = OLD.mailboxname;
END