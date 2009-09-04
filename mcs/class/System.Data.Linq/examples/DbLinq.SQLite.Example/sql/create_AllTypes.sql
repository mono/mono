--####################################################################
--
--####################################################################

--####################################################################
CREATE TABLE IF NOT EXISTS [AllTypes] (
  [int] INTEGER PRIMARY KEY ,
  [intN] INTEGER UNSIGNED,
  [double] DOUBLE NOT NULL DEFAULT 0,
  [doubleN] DOUBLE,
  [decimal] DECIMAL NOT NULL DEFAULT 0,
  [decimalN] DECIMAL,
  [blob] BLOB NOT NULL,
  [blobN] BLOB,
  [boolean] BOOLEAN NOT NULL DEFAULT 0,
  [boolN] BOOLEAN,
  [byte] TINYINT UNSIGNED NOT NULL DEFAULT 0,
  [byteN] TINYINT UNSIGNED,
  [DateTime] DATETIME NOT NULL DEFAULT 0,
  [DateTimeN] DATETIME,
  [float] FLOAT NOT NULL DEFAULT 0,
  [floatN] FLOAT,
  [char] CHAR NOT NULL DEFAULT '',
  [charN] CHAR,
  [text] TEXT NOT NULL,
  [textN] TEXT,
  [short] MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
  [shortN] MEDIUMINT UNSIGNED,
  [numeric] NUMERIC NOT NULL DEFAULT 0,
  [numericN] NUMERIC,
  [real] REAL NOT NULL DEFAULT 0,
  [realN] REAL,
  [smallInt] SMALLINT UNSIGNED NOT NULL DEFAULT 0,
  [smallIntN] SMALLINT UNSIGNED,
  [tinyIntU] TINYINT UNSIGNED NOT NULL DEFAULT 0,
  [tinyIntUN] TINYINT UNSIGNED NULL DEFAULT 0,
  [tinyIntS] TINYINT SIGNED DEFAULT 0,
  [DbLinq_EnumTest] SMALLINT UNSIGNED NOT NULL
);


--####################################################################
--## populate tables with seed data
--####################################################################


DELETE FROM AllTypes;
INSERT INTO AllTypes (
               [intN] ,
  [double] ,   [doubleN] ,
  [decimal] ,  [decimalN] ,
  [blob] ,     [blobN] ,
  [boolean] ,  [boolN] ,
  [byte] ,     [byteN] ,
  [DateTime] , [DateTimeN] ,
  [float] ,    [floatN] ,
  [char] ,     [charN] ,
  [text] ,     [textN] ,
  [short] ,    [shortN] ,
  [numeric] ,  [numericN] ,
  [real] ,     [realN] ,
  [smallInt] , [smallIntN],
  [DbLinq_EnumTest]
)
VALUES(         null,
  2,            null, 
  3,            null,
  'aa',         null, 
  1,            null,
  8,            null, 
  '2007-12-14', null,
  4,            null, 
  'c',          null,
  'text',       null, 
  127,          null,
  999.9,        null, 
  998.9,        null,
  16000,        null, 
  1);
