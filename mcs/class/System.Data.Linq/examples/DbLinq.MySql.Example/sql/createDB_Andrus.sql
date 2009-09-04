####################################################################
## script to create Andrus DB, a test DB which exercises:
## 1) Composite PKs
## 2) Vertical Partitioning
## 3) some pathological mapping situations (mapping from sql schema to DBML to C#)
## most of these cases were discovered by Andrus (Tallinn, Estonia).
####################################################################

DROP DATABASE IF EXISTS `Andrus`;

CREATE DATABASE `Andrus`;

USE `Andrus`;

## note - LinqUser must already be created.
GRANT Select, Insert, Update, Delete, EXECUTE ON `Andrus`.* TO 'LinqUser'@'%';
FLUSH PRIVILEGES;

# problem 1. foreign key relation generates duplicate propery if property exists.
# problem 2. Column name 'private' causes error
CREATE TABLE t1 ( private int primary key) ENGINE = InnoDB;


## here both columns reference table t1
CREATE TABLE t2 ( f1 int references t1,
  f2 int references t1 ) ENGINE = InnoDB;

## a little composite PK playground...
create table tCompositePK
(
  f1 int,
  f2 varchar(5),
  f3 int,  
  primary key(f1,f2)
)
ENGINE = InnoDB;

## there was a bug where this table could not be updated - quotes missing in the UPDATE statement around the PK.
create table char_pk ( col1 char primary key, val1 int);

## this is for testing of vertical Partitioning / Discriminator.
## EmployeeType controls which derived class we instantiate.
create table Employee
(
  employeeID INTEGER NOT NULL AUTO_INCREMENT,
  employeeType int NOT NULL,
  employeeName varchar(99),
  startDate date NULL,
  PRIMARY KEY(employeeID)
)
ENGINE = InnoDB;

INSERT INTO Employee (employeeType, employeeName) VALUES (0, 'Pavel');
INSERT INTO Employee (employeeType, employeeName) VALUES (1, 'Piotr');
INSERT INTO Employee (employeeType, employeeName) VALUES (1, 'Jana');
INSERT INTO Employee (employeeType, employeeName) VALUES (2, 'Ladislav');

COMMIT;



