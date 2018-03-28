/*
=========================================================================================
MySQL_5.sql
Author: Amit Biswas (amit@amitbiswas.com)

This sql script performs the same operations as "mysql.sql" but some sql commands
have been changed either to fix bugs or to comply with MySQL Server 5.0
15-Dec-2007

Changes:
--------
In numeric_family, the unsigned attribute was added to column type_tinyint to allow it to store the value 255,
Reason: tinyint is normally signed and stores from -128 to 127 (http://dev.mysql.com/doc/refman/5.0/en/numeric-types.html)

In numeric_family, the column type_double was declared as float NULL which cannot store the value 1.79E+308, hence it changed it to float (53)
Reason: MySQL supports the optional precision specification but the precision value is used only
to determine storage size. A precision from 0 to 23 results in a four-byte single-precision FLOAT column.
A precision from 24 to 53 results in an eight-byte double-precision DOUBLE column.
(http://dev.mysql.com/doc/refman/5.0/en/numeric-types.html)

In binary_family, the column type_binary was declared as binary NULL which cannot store the value 555555, hence changed it to binary (8)
Reason: In case of binary (and varbinary) fields the length indicates bytes, not characters. (http://dev.mysql.com/doc/refman/5.0/en/binary-varbinary.html)
(ERROR: Data too long for column 'type_binary')

In datetime_family, the column type_smalldatetime was declared as timestamp NULL which cannot store the year 2079, hence changed it to 2037-12-31 23:59:00
Reason: The range of timestamp is '1970-01-01 00:00:01' to 2037, (http://dev.mysql.com/doc/refman/5.0/en/date-and-time-type-overview.html)
(ERROR: Incorrect datetime value: '2079-06-06 23:59:00' for column 'type_smalldatetime')

Stored Procedures:
------------------
Modified the "Create Procedure" statement
Reason: the existing statement doesnt work in MySQL Administrator, MySQL 5.0.27

Removed the "Return" statement in the stored procedure sp_get_age
Reason: "Return" is only allowed in a function not in a procedure, u can use "INTO" instead

===========================================================================================
*/

/*
=================================== OBJECT NUMERIC_FAMILY =========================
-- TABLE : NUMERIC_FAMILY
-- data with id > 6000 is not gaurenteed to be read-only.
*/

drop table if exists numeric_family;


create table `numeric_family` (
	`id` int NOT NULL,
	`type_bit` bit NULL,
	`type_tinyint` tinyint unsigned NULL,
	`type_smallint` smallint NULL,
	`type_int` int NULL,
	`type_bigint` bigint NULL,
	`type_decimal1` decimal (38, 0) NULL,
	`type_decimal2` decimal (10, 3) NULL,
	`type_numeric1` numeric (38, 0) NULL,
	`type_numeric2` numeric (10, 3) NULL,
	`type_money` numeric (38,0) NULL,
	`type_smallmoney` numeric (12,0) NULL,
  `type_float` float(24) NULL,
  `type_double` float (53) NULL,
  `type_autoincrement` int PRIMARY KEY AUTO_INCREMENT NOT NULL);

insert into numeric_family (id, type_bit, type_tinyint, type_smallint, type_int, type_bigint, type_decimal1, type_decimal2, type_numeric1, type_numeric2, type_money, type_smallmoney, type_float, type_double)
	values (1, 1, 255, 32767, 2147483647, 9223372036854775807, 1000, 4456.432, 1000, 4456.432, 922337203685477.5807, 214748.3647, 3.40E+38, 1.79E+308);
insert into numeric_family (id, type_bit, type_tinyint, type_smallint, type_int, type_bigint, type_decimal1, type_decimal2, type_numeric1, type_numeric2, type_money, type_smallmoney, type_float, type_double)
	values (2, 0, 0, -32768, -2147483648, -9223372036854775808, -1000, -4456.432, -1000, -4456.432, -922337203685477.5808, -214748.3648, -3.40E+38, -1.79E+308);
insert into numeric_family (id, type_bit, type_tinyint, type_smallint, type_int, type_bigint, type_decimal1, type_decimal2, type_numeric1, type_numeric2, type_money, type_smallmoney, type_float, type_double)
	values (3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
insert into numeric_family (id, type_bit, type_tinyint, type_smallint, type_int, type_bigint, type_decimal1, type_decimal2, type_numeric1, type_numeric2, type_money, type_smallmoney, type_float, type_double)
	values (4, null, null, null, null, null, null, null, null, null, null, null, null, null);

/*
-- =================================== END OBJECT NUMERIC_FAMILY ========================


-- =================================== OBJECT BINARY_FAMILY =========================
-- TABLE : BINARY_FAMILY
-- data with id > 6000 is not gaurenteed to be read-only.
*/

drop table if exists binary_family;

create table `binary_family` (
	`id` int PRIMARY KEY NOT NULL,
	`type_binary` binary (8),
	`type_varbinary` varbinary (255) NULL,
	`type_blob` blob NULL,
	`type_tinyblob` tinyblob NULL,
	`type_mediumblob` mediumblob NULL,
	`type_longblob_image` longblob NULL);

insert into binary_family (id, type_binary, type_varbinary, type_blob, type_tinyblob, type_mediumblob, type_longblob_image) values (
	1,
	'5',
	0x303132333435363738393031323334353637383930313233343536373839004453, 
	0x3256004422,
	0x3A56004422, 
	0x2B87002233,
	0x4D84002332
);
insert into binary_family (id, type_binary, type_varbinary, type_blob, type_tinyblob, type_mediumblob, type_longblob_image) values (
	2,
	0x0033340033303531,
	0x003938373635003332313031323334, 
	0x0066066697006606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066698,
	0x0056334422, 
	0x0087342233,
	0x0084352332
);
insert into binary_family (id, type_binary, type_varbinary, type_blob, type_tinyblob, type_mediumblob, type_longblob_image) values (
	3,
	'',
	'', 
	'',
	'', 
	'',
	''
);
insert into binary_family (id, type_binary, type_varbinary, type_blob, type_tinyblob, type_mediumblob, type_longblob_image) values (
	4,null,null,null,null,null,null);

/*
-- =================================== END OBJECT BINARY_FAMILY ========================


-- =================================== OBJECT STRING_FAMILY============================
-- TABLE : string_family 
-- data with id above 6000 is not gaurenteed to be read-only.
*/

drop table if exists string_family;

create table `string_family` (
	`id` int PRIMARY KEY NOT NULL,
	`type_char` char(10) NULL,
	`type_nchar` char(10) CHARACTER SET ucs2 COLLATE ucs2_general_ci NULL,
	`type_varchar` varchar(10) NULL,
	`type_nvarchar` varchar(10) CHARACTER SET ucs2 COLLATE ucs2_general_ci NULL,
	`type_text` text NULL,
	`type_ntext` longtext CHARACTER SET ucs2 COLLATE ucs2_general_ci NULL);

insert into string_family values (1, 'char', 'nchभाr', 'varchar', 'nvभारतr', 'text', 'ntभाxt');
insert into string_family values (2, '0123456789', '0123456789', 'varchar ', 'nvभारतr ', 'longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext ', 'ntभाxt ');
insert into string_family values (3, '', '', '', '', '', '');
insert into string_family values (4, null, null, null, null, null, null);

/*
-- =================================== END OBJECT STRING_FAMILY ========================


-- =================================== OBJECT DATETIME_FAMILY============================
-- TABLE : datetime_family
-- data with id above 6000 is not gaurenteed to be read-only.
*/

drop table if exists datetime_family;

create table `datetime_family` (
        `id` int PRIMARY KEY NOT NULL,
        `type_smalldatetime` timestamp NULL,
        `type_datetime` datetime NULL,
        `type_time` time NULL,
        `type_date` date NULL);

insert into `datetime_family` values (1,'2037-12-31 23:59:00','9999-12-31 23:59:59','23:58:59','9999-12-31');
insert into `datetime_family` values (4,null,null,null,null);

/*
-- =================================== END OBJECT DATETIME_FAMILY========================


-- =================================== OBJECT EMPLOYEE ============================
-- TABLE : EMPLOYEE
-- data with id above 6000 is not gaurenteed to be read-only.
*/

drop table if exists employee;

create table `employee` (
	`id` int PRIMARY KEY NOT NULL,
	`fname` varchar (50) NOT NULL,
	`lname` varchar (50),
	`dob` datetime NOT NULL,
	`doj` datetime NOT NULL,
	`email` varchar (50));

insert into `employee` values (1, 'suresh', 'kumar', '1978-08-22', '2001-03-12', 'suresh@gmail.com');
insert into `employee` values (2, 'ramesh', 'rajendran', '1977-02-15', '2005-02-11', 'ramesh@yahoo.com');
insert into `employee` values (3, 'venkat', 'ramakrishnan', '1977-06-12', '2003-12-11', 'ramesh@yahoo.com');
insert into `employee` values (4, 'ramu', 'dhasarath', '1977-02-15', '2005-02-11', 'ramesh@yahoo.com');

/*
-- STORED PROCEDURES
-- SP : sp_clean_person_table
*/

delimiter //
drop procedure if exists sp_clean_employee_table
//
CREATE PROCEDURE `sp_clean_employee_table`()
begin
	delete from employee where `id` > 6000;
end
//

/*
-- SP : sp_get_age
*/

drop procedure if exists sp_get_age
//
create procedure sp_get_age (fname varchar (50), OUT age int)
begin
  select age=datediff (`dob`, now()) from `employee` where `fname` like fname;
end
//
/*
-- =================================== END OBJECT EMPLOYEE ============================
*/