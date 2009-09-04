--############################################################
-- this database contains pathological cases discovered by Andrus (Tallinn, Estonia)

--####################################################################
--## create database
--####################################################################
DROP DATABASE IF EXISTS Andrus;
CREATE DATABASE Andrus WITH OWNER = "LinqUser";

\connect andrus
--####################################################################
--## create tables
--####################################################################
DROP TABLE t1 CASCADE;
DROP TABLE t2;
DROP TABLE t3;
DROP DOMAIN ebool;

--# problem 1. foreign key relation generates duplicate propery if property exists.
--# problem 2. Column name 'private' causes error
CREATE TABLE t1 ( private int primary key);
CREATE TABLE t2 ( f1 int references t1,
  f2 int references t1 );

CREATE DOMAIN ebool AS bool DEFAULT false NOT NULL;

--# problem 3. GetHashCode() does not check null ids for reference types
--# problem 4. pgsql domain types are not recognized in mappings ('ebool' above)
CREATE TABLE t3 ( t3ID varchar(5) primary key, 
my_ebool ebool,
t3 integer); --check for 'Content' field

/*
CREATE TABLE t4 
ALTER TABLE t4 ALTER kasutaja SET NOT NULL,
ALTER t4 SET NOT NULL,
ADD constraint t4_kasutaja_firmanr_unique
UNIQUE (t4,kasutaja,firmanr);

--this causes exception in sqlmetal:
--Missing data from 'constraint_column_usage' for foreign key
--kasgrupp_kasutaja_firmanr_unique
*/

create table tCompositePK
(
  f1 int,
  f2 varchar(5),
  f3 int,  
  primary key(f1,f2)
);

-- there was a bug where this table could not be updated - quotes missing in the UPDATE statement around the PK.
create table char_pk ( col1 char primary key, val1 int);

-- this is for testing of vertical Partitioning / Discriminator.
create table Employee
(
  employeeID SERIAL NOT NULL,
  employeeType int NOT NULL,
  employeeName varchar(99),
  startDate date NULL,
  PRIMARY KEY(employeeID)
);

--####################################################################
--## make sure permissions are set
--####################################################################
grant all on char_pk to "LinqUser";
grant all on employee to "LinqUser";
grant all on t1 to "LinqUser";
grant all on t2 to "LinqUser";
grant all on t3 to "LinqUser";
grant all on tcompositepk to "LinqUser";


INSERT INTO Employee (employeeType, employeeName) VALUES (0, 'Pavel');
INSERT INTO Employee (employeeType, employeeName) VALUES (1, 'Piotr');
INSERT INTO Employee (employeeType, employeeName) VALUES (1, 'Jana');
INSERT INTO Employee (employeeType, employeeName) VALUES (2, 'Ladislav');

COMMIT;




