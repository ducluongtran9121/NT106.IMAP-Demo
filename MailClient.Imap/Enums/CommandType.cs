﻿namespace MailClient.Imap.Enums
{
    public enum CommandType
    {
        NONE,
        LOGIN,
        FETCH,
        STARTTLS,
        SELECT,
        EXAMINE,
        CREATE,
        DELETE,
        RENAME,
        SUBSCRIBE,
        UNSUBSCRIBE,
        NOOP,
        LIST,
        LSUB,
        STATUS,
        APPEND,
        CHECK,
        CLOSE,
        EXPURE,
        SEARCH,
        STORE,
        COPY,
        UID,
        XLIST,
        LOGOUT
    }
}