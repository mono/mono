use monotest;

-- =================================== OBJECT NUMERIC_FAMILY============================
-- TABLE : NUMERIC_FAMILY
-- data with id > 6000 is not gaurenteed to be read-only.
if exists (select name from sysobjects where 
	name = 'numeric_family' and type = 'U')
	drop table numeric_family;
go

create table numeric_family(
	id int PRIMARY KEY NOT NULL,
        type_bit bit NULL,
        type_tinyint tinyint NULL,
        type_smallint smallint NULL,
        type_int int NULL,
        type_bigint bigint NULL,
        type_decimal decimal(38,0) NULL,
        type_numeric numeric(38,0) NULL,
        type_money money NULL,
        type_smallmoney smallmoney NULL,
        type_float real NULL,
        type_double float NULL);
go

grant all privileges on numeric_family to monotester;
go

insert into numeric_family values (1,1,255,32767,2147483647,9223372036854775807,1000,1000,922337203685477.5807,214748.3647,3.40E+38,1.79E+308);
insert into numeric_family values (2,0,0,-32768,-2147483648,-9223372036854775808,-1000,-1000,-922337203685477.5808,-214748.3648,-3.40E+38,-1.79E+308);
insert into numeric_family values (3,0,0,0,0,0,0,0,0,0,0,0);
insert into numeric_family values (4,null,null,null,null,null,null,null,null,null,null,null);
go
-- =================================== END OBJECT NUMERIC_FAMILY ========================

-- =================================== OBJECT BINARY_FAMILY =========================
-- TABLE : BINARY_FAMILY
-- data with id > 6000 is not gaurenteed to be read-only.
if exists (select name from sysobjects where 
	name = 'binary_family' and type = 'U')
	drop table binary_family;
go

create table binary_family (
        id int PRIMARY KEY NOT NULL,
        type_binary binary NULL,
        type_varbinary varbinary (255) NULL,
        type_blob image NULL,
        type_tinyblob image NULL,
        type_mediumblob image NULL,
        type_longblob_image image NULL);
go

grant all privileges on binary_family to monotester;
go

insert into binary_family values (1, convert (binary, '5'), convert (varbinary, '0123456789012345678901234567890123456789012345678901234567890123456789'), 
					convert (image, '66666666'), convert (image, '777777'), 
					convert (image, '888888'), convert (image, '999999'));
--insert into binary_family values (2,
--insert into binary_family values (3,
insert into binary_family values (4,null,null,null,null,null,null);
go

-- =================================== END OBJECT BINARY_FAMILY ========================


-- =================================== OBJECT STRING_FAMILY============================
-- TABLE : string_family 
-- data with id above 6000 is not gaurenteed to be read-only.
if exists (select name from sysobjects where 
	name = 'string_family' and type = 'U')
	drop table string_family;
go

create table string_family(
	id int PRIMARY KEY NOT NULL,
        type_guid uniqueidentifier NULL,
        type_char char(10) NULL,
        type_varchar varchar(10) NULL,
        type_text text NULL,
        type_ntext ntext NULL);
go

grant all privileges on string_family to monotester;
go

insert into string_family values (1,newid(),'char','varchar','text','ntext');
insert into string_family values (4,null,null,null,null,null);
go
-- =================================== END OBJECT STRING_FAMILY ========================


-- =================================== OBJECT DATETIME_FAMILY============================
-- TABLE : datetime_family
-- data with id above 6000 is not gaurenteed to be read-only.

if exists (select name from sysobjects where 
	name = 'datetime_family' and type = 'U')
	drop table datetime_family;
go

create table datetime_family (
        id int PRIMARY KEY NOT NULL,
        type_smalldatetime smalldatetime NULL,
        type_datetime datetime NULL);

grant all privileges on datetime_family to monotester;
go
insert into datetime_family values (1,'2079-06-06 23:59:00','9999-12-31 23:59:59:00');
insert into datetime_family values (4,null,null);
go

-- =================================== END OBJECT DATETIME_FAMILY========================

-- =================================== OBJECT EMPLOYEE ============================
-- TABLE : EMPLOYEE
-- data with id above 6000 is not gaurenteed to be read-only.
if exists (select name from sysobjects where 
	name = 'employee' and type = 'U')
	drop table employee;
go

create table employee ( 
	id int PRIMARY KEY NOT NULL, 
	fname varchar (50) NOT NULL,
	lname varchar (50) NULL,
	dob datetime NOT NULL,
	doj datetime NOT NULL,
	email varchar (50) NULL);
go

grant all privileges on employee to monotester;

go

insert into employee values (1, 'suresh', 'kumar', '1978-08-22', '2001-03-12', 'suresh@gmail.com');
insert into employee values (2, 'ramesh', 'rajendran', '1977-02-15', '2005-02-11', 'ramesh@yahoo.com');
insert into employee values (3, 'venkat', 'ramakrishnan', '1977-06-12', '2003-12-11', 'ramesh@yahoo.com');
insert into employee values (4, 'ramu', 'dhasarath', '1977-02-15', '2005-02-11', 'ramesh@yahoo.com');

go

-- STORED PROCEDURES
-- SP : sp_clean_person_table
if exists (select name from sysobjects where 
	name = 'sp_clean_employee_table' and type = 'P')
	drop procedure sp_clean_employee_table;
go

create procedure sp_clean_employee_table
as 
begin
	delete from employee where id > 6000;
end
go


-- SP : sp_get_age
if exists (select name from sysobjects where 
	name = 'sp_get_age' and type = 'P')
	drop procedure sp_get_age;
go

create procedure sp_get_age (
	@fname varchar (50),
	@age int output)
as 
begin
	select @age = datediff (day, dob, getdate ()) from employee where fname like @fname;
	return @age;
end
go

-- =================================== END OBJECT EMPLOYEE ============================
