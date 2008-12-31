use monotest
go

-- =================================== OBJECT EMPLOYEE ============================
-- TABLE : INT_FAMILY
-- data with id > 6000 is not gaurenteed to be read-only.
if exists (select name from sysobjects where 
	name = 'numeric_family' and type = 'U')
	drop table numeric_family
go

create table numeric_family (
	id int PRIMARY KEY NOT NULL,
	type_bit bit NOT NULL,
	type_tinyint tinyint NULL,
	type_smallint smallint NULL,
	type_int int NULL,
	type_bigint numeric (38,0) NULL,
	type_decimal decimal (38, 0) NULL,
	type_numeric numeric (38, 0) NULL,
	type_money money NULL,
	type_smallmoney smallmoney NULL)

grant all on numeric_family to monotester
go

insert into numeric_family values (1,1,255,32767,2147483647,9223372036854775807,1000,1000,922337203685477.5807,214748.3647)
insert into numeric_family values (2,0,0,-32768,-2147483648,-9223372036854775808,-1000,-1000,-922337203685477.5808,-214748.3648)
insert into numeric_family values (3,0,0,0,0,0,0,0,0,0)
insert into numeric_family values (4,0,null,null,null,null,null,null,null,null)
go

-- =================================== END OBJECT EMPLOYEE ========================

-- =================================== OBJECT BINARY_FAMILY =========================
-- TABLE : BINARY FAMILY
-- data with id > 6000 is not gaurenteed to be read-only.
if exists (select name from sysobjects where 
	name = 'binary_family' and type = 'U')
	drop table binary_family
go

create table binary_family (
	id int PRIMARY KEY NOT NULL,
	type_binary binary (8) NULL,
	type_varbinary varbinary (255) NULL,
	type_blob image NULL,
	type_tinyblob image NULL,
	type_mediumblob image NULL,
	type_longblob_image image NULL)

grant all on binary_family to monotester
go

insert into binary_family (id, type_binary, type_varbinary, type_blob, type_tinyblob, type_mediumblob, type_longblob_image) values (
	1,
	convert (binary, '5'),
	convert (varbinary, '0123456789012345678901234567890123456789012345678901234567890123456789'), 
	convert (image, '66666666'),
	convert (image, '777777'), 
	convert (image, '888888'),
	convert (image, '999999')
);
insert into binary_family (id, type_binary, type_varbinary, type_blob, type_tinyblob, type_mediumblob, type_longblob_image) values (
	2,
	convert (binary, '03423051'),
	convert (varbinary, '98765432101234'), 
	convert (image, '06666666'),
	convert (image, '077077'), 
	convert (image, '088088'),
	convert (image, '099099')
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


-- =================================== OBJECT EMPLOYEE ============================
-- TABLE : EMPLOYEE
-- data with id above 6000 is not gaurenteed to be read-only.
if exists (select name from sysobjects where 
	name = 'employee' and type = 'U')
	drop table employee
go

create table employee ( 
	id int PRIMARY KEY NOT NULL, 
	fname varchar (50) NOT NULL,
	lname varchar (50) NULL,
	dob datetime NOT NULL,
	doj datetime NOT NULL,
	email varchar (50) NULL)
go

grant all privileges on employee to monotester

insert into employee values (1, 'suresh', 'kumar', '1978-08-22', '2001-03-12', 'suresh@gmail.com')
insert into employee values (2, 'ramesh', 'rajendran', '1977-02-15', '2005-02-11', 'ramesh@yahoo.com')
insert into employee values (3, 'venkat', 'ramakrishnan', '1977-06-12', '2003-12-11', 'ramesh@yahoo.com')
insert into employee values (4, 'ramu', 'dhasarath', '1977-02-15', '2005-02-11', 'ramesh@yahoo.com')
go

-- STORED PROCEDURES
-- SP : sp_clean_employee_table
if exists (select name from sysobjects where 
	name = 'sp_clean_employee_table' and type = 'P')
	drop procedure sp_clean_employee_table
go

create procedure sp_clean_employee_table
as 
begin
	delete from employee where id > 6000
end
go

grant all on sp_clean_employee_table to monotester
go

-- SP : sp_get_age
if exists (select name from sysobjects where 
	name = 'sp_get_age' and type = 'P')
	drop procedure sp_get_age
go

create procedure sp_get_age (
	@fname varchar (50),
	@age int output)
as 
begin
	select @age = datediff (day, dob, getdate ()) from employee where fname like @fname
	return @age
end
go

grant all on sp_get_age to monotester
go


-- =================================== END OBJECT EMPLOYEE ============================
