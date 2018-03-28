-- =================================== OBJECT NUMERIC_FAMILY============================
-- TABLE : NUMERIC_FAMILY
-- data with id > 6000 is not gaurenteed to be read-only.
if exists (select name from sysobjects where
	name = 'numeric_family' and type = 'U')
	drop table numeric_family;
go

create table numeric_family (
	id int PRIMARY KEY NOT NULL,
	type_bit bit NULL,
	type_tinyint tinyint NULL,
	type_smallint smallint NULL,
	type_int int NULL,
	type_bigint bigint NULL,
	type_decimal1 decimal(38,0) NULL,
	type_decimal2 decimal(10,3) NULL,
	type_numeric1 numeric(38,0) NULL,
	type_numeric2 numeric(10,3) NULL,
	type_money money NULL,
	type_smallmoney smallmoney NULL,
	type_float real NULL,
	type_double float NULL,
	type_autoincrement int identity (2, 3));
go


insert into numeric_family (id, type_bit, type_tinyint, type_smallint, type_int, type_bigint, type_decimal1, type_decimal2, type_numeric1, type_numeric2, type_money, type_smallmoney, type_float, type_double)
	values (1, 1, 255, 32767, 2147483647, 9223372036854775807, 1000, 4456.432, 1000, 4456.432, 922337203685477.5807, 214748.3647, 3.40E+38, 1.79E+308);
insert into numeric_family (id, type_bit, type_tinyint, type_smallint, type_int, type_bigint, type_decimal1, type_decimal2, type_numeric1, type_numeric2, type_money, type_smallmoney, type_float, type_double)
	values (2, 0, 0, -32768, -2147483648, -9223372036854775808, -1000, -4456.432, -1000, -4456.432, -922337203685477.5808, -214748.3648, -3.40E+38, -1.79E+308);
insert into numeric_family (id, type_bit, type_tinyint, type_smallint, type_int, type_bigint, type_decimal1, type_decimal2, type_numeric1, type_numeric2, type_money, type_smallmoney, type_float, type_double)
	values (3, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
insert into numeric_family (id, type_bit, type_tinyint, type_smallint, type_int, type_bigint, type_decimal1, type_decimal2, type_numeric1, type_numeric2, type_money, type_smallmoney, type_float, type_double)
	values (4, null, null, null, null, null, null, null, null, null, null, null, null, null);
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
	type_binary binary (8) NULL,
	type_varbinary varbinary (255) NULL,
	type_blob image NULL,
	type_tinyblob image NULL,
	type_mediumblob image NULL,
	type_longblob_image image NULL,
	type_timestamp timestamp NULL);
go

insert into binary_family (id, type_binary, type_varbinary, type_blob, type_tinyblob, type_mediumblob, type_longblob_image) values (
	1,
	convert (binary, '5'),
	convert (varbinary(255), 0x303132333435363738393031323334353637383930313233343536373839004453),
	convert (image, 0x3256004422),
	convert (image, 0x3A56004422),
	convert (image, 0x2B87002233),
	convert (image, 0x4D84002332)
);
insert into binary_family (id, type_binary, type_varbinary, type_blob, type_tinyblob, type_mediumblob, type_longblob_image) values (
	2,
	convert (binary, 0x0033340033303531),
	convert (varbinary, 0x003938373635003332313031323334),
	convert (image, 0x0066066697006606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066697066606669706660666970666066698),
	convert (image, 0x0056334422),
	convert (image, 0x0087342233),
	convert (image, 0x0084352332)
);
insert into binary_family (id, type_binary, type_varbinary, type_blob, type_tinyblob, type_mediumblob, type_longblob_image) values (
	3,
	convert (binary, ''),
	convert (varbinary, ''),
	convert (image, ''),
	convert (image, ''), 
	convert (image, ''),
	convert (image, '')
);
insert into binary_family (id, type_binary, type_varbinary, type_blob, type_tinyblob, type_mediumblob, type_longblob_image) values (
	4,null,null,null,null,null,null);
go

-- =================================== END OBJECT BINARY_FAMILY ========================


-- =================================== OBJECT STRING_FAMILY============================
-- TABLE : string_family 
-- data with id above 6000 is not gaurenteed to be read-only.
if exists (select name from sysobjects where
	name = 'string_family' and type = 'U')
	drop table string_family;
go

create table string_family (
	id int PRIMARY KEY NOT NULL,
	type_guid uniqueidentifier NULL,
	type_char char(10) NULL,
	type_nchar nchar(10) NULL,
	type_varchar varchar(10) NULL,
	type_nvarchar nvarchar(10) NULL,
	type_text text NULL,
	type_ntext ntext NULL);
go

insert into string_family values (1, 'd222a130-6383-4d36-ac5e-4e6b2591aabf', 'char', N'nchभाr', 'varchar', N'nvभारतr', 'text', N'ntभाxt');
insert into string_family values (2, '1c47dd1d-891b-47e8-aac8-f36608b31bc5', '0123456789', '0123456789', 'varchar ', N'nvभारतr ', 'longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext longtext ', N'ntभाxt ');
insert into string_family values (3, '3c47dd1d-891b-47e8-aac8-f36608b31bc5', '', '', '', '', '', '');
insert into string_family values (4, null, null, null, null, null, null, null);
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

go
insert into datetime_family values (1,'2037-12-31 23:59:00','9999-12-31 23:59:59:997');
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

insert into employee values (1, 'suresh', 'kumar', '1978-08-22', '2001-03-12', 'suresh@gmail.com');
insert into employee values (2, 'ramesh', 'rajendran', '1977-02-15', '2005-02-11', 'ramesh@yahoo.com');
insert into employee values (3, 'venkat', 'ramakrishnan', '1977-06-12', '2003-12-11', 'ramesh@yahoo.com');
insert into employee values (4, 'ramu', 'dhasarath', '1977-02-15', '2005-02-11', 'ramesh@yahoo.com');

go

-- STORED PROCEDURES

-- SP : sp_clean_employee_table
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

-- SP : sp_326182a
if exists (select name from sysobjects where
	name = 'sp_326182a' and type = 'P')
	drop procedure sp_326182a;
go

CREATE procedure sp_326182a (
	@param0 int out,
	@param1 int out,
	@param2 int out,
	@param3 int out)
as
begin
	set @param0 = 100
	set @param1 = 101
	set @param2 = 102
	set @param3 = 103
	return 2
end
go

-- SP: sp_326182b

if exists (select name from sysobjects where
	name = 'sp_326182b' and type = 'P')
	drop procedure sp_326182b;
go

CREATE procedure sp_326182b (
	@param0 int = 9,
	@param1 decimal (5, 2) out,
	@param2 varchar (12))
as
begin
	set @param1 = (@param0 + @param1 + 2)
	return 666
end