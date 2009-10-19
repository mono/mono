CREATE TABLE Sessions
(
  SessionId       Text(80)  NOT NULL,
  ApplicationName Text(255) NOT NULL,
  Created         DateTime  NOT NULL,
  Expires         DateTime  NOT NULL,
  LockDate        DateTime  NOT NULL,
  LockId          Integer   NOT NULL,
  Timeout         Integer   NOT NULL,
  Locked          YesNo     NOT NULL,
  SessionItems    Memo,
  Flags           Integer   NOT NULL,
      CONSTRAINT PKSessions PRIMARY KEY (SessionId, ApplicationName)
);
