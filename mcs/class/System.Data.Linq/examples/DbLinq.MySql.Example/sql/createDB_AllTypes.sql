####################################################################
## script to create a database which exercises all known MySql types
####################################################################

DROP DATABASE IF EXISTS `AllTypes`;

CREATE DATABASE `AllTypes`;

USE `AllTypes`;

GRANT ALL ON AllTypes.*  TO 'LinqUser'@'%';
FLUSH PRIVILEGES;

####################################################################
CREATE TABLE `AllIntTypes` (
  `int` INTEGER UNSIGNED NOT NULL AUTO_INCREMENT,
  `intN` INTEGER UNSIGNED,
  `boolean` BOOLEAN NOT NULL DEFAULT 0,
  `boolN` BOOLEAN,
  `byte` TINYINT UNSIGNED NOT NULL DEFAULT 0,
  `byteN` TINYINT UNSIGNED,
  `short` MEDIUMINT UNSIGNED NOT NULL DEFAULT 0,
  `shortN` MEDIUMINT UNSIGNED NULL,
  `smallInt` SMALLINT UNSIGNED NOT NULL DEFAULT 0,
  `smallIntN` SMALLINT UNSIGNED,
  `tinyIntU` TINYINT(1) UNSIGNED NOT NULL DEFAULT 0,
  `tinyIntUN` TINYINT(1) UNSIGNED NULL DEFAULT 0,
  `tinyIntS` TINYINT(1) SIGNED DEFAULT 0,
  `bigInt` BIGINT NOT NULL,
  `bigIntN` BIGINT NULL,
  `DbLinq_EnumTest` SMALLINT UNSIGNED NOT NULL,
  PRIMARY KEY(`int`)
)
ENGINE = InnoDB
COMMENT = 'Tests mapping of many MySQL types to CSharp types';

####################################################################
CREATE TABLE `FloatTypes` (
  `id1` INTEGER NOT NULL AUTO_INCREMENT,
  `double` DOUBLE NOT NULL DEFAULT 0,
  `doubleN` DOUBLE,
  `decimal` DECIMAL NOT NULL DEFAULT 0,
  `decimalN` DECIMAL,
  `float` FLOAT NOT NULL DEFAULT 0,
  `floatN` FLOAT,
  `numeric` NUMERIC NOT NULL DEFAULT 0,
  `numericN` NUMERIC,
  `real` REAL NOT NULL DEFAULT 0,
  `realN` REAL,
  PRIMARY KEY(`id1`)
)
ENGINE = InnoDB
COMMENT = 'Tests mapping of many MySQL types to CSharp types';


####################################################################
CREATE TABLE `OtherTypes` (
  `id1` INTEGER NOT NULL AUTO_INCREMENT,
  `blob` BLOB NOT NULL,
  `blobN` BLOB,
  `DateTime` DATETIME NOT NULL DEFAULT 0,
  `DateTimeN` DATETIME,
  `char` CHAR NOT NULL DEFAULT '',
  `charN` CHAR,
  `text` TEXT NOT NULL,
  `textN` TEXT,
  `rainbow` ENUM ('red', 'orange', 'yellow') NOT NULL,
  `DbLinq_guid_test` CHAR(36),
  `DbLinq_guid_test2` BINARY(16) NOT NULL,
  PRIMARY KEY(`id1`)
)
ENGINE = InnoDB
COMMENT = 'Tests mapping of many MySQL types to CSharp types';

####################################################################
CREATE TABLE ParsingData (
 id1 INTEGER NOT NULL AUTO_INCREMENT,
 dateTimeStr VARCHAR(20),
 PRIMARY KEY(id1)
)
ENGINE = InnoDB
COMMENT = 'Tests DateTime.ParseExact on DB varchars';
  

INSERT INTO AllIntTypes (`intN`,`boolean`, `BIGINT`, `byte`, `DbLinq_EnumTest`)
VALUES (1,'2', -9223372036854775808, 7, 2);

INSERT INTO FloatTypes (`double`,`decimal`,`float`)
VALUES (1.1, 2.2, 3.3);

INSERT INTO OtherTypes (`blob`,`text`, rainbow, DbLinq_guid_test, DbLinq_guid_test2)
VALUES ( REPEAT("\0",(8)), 'text', 'red', 'E5F61BB0-38BA-4116-841D-C7E5AAA137A2', REPEAT("\0\1",8) );

INSERT INTO ParsingData (dateTimeStr) VALUES ('2008.12.31');