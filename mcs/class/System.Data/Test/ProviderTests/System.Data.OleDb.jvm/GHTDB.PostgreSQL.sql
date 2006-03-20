DROP FUNCTION GHSP_TYPES_SIMPLE_1(BOOL, INT2, INT4, INT8, NUMERIC, FLOAT4, FLOAT8, VARCHAR, CHAR, NCHAR);
DROP FUNCTION GHSP_TYPES_SIMPLE_4(VARCHAR);
DROP FUNCTION GHSP_TYPES_SIMPLE_5();
DROP FUNCTION GH_DUMMY(NUMERIC);
DROP FUNCTION GH_REFCURSOR1();
DROP FUNCTION GH_REFCURSOR2(integer);
DROP FUNCTION GH_REFCURSOR3(varchar);
DROP FUNCTION gh_createtable();
DROP FUNCTION GH_MULTIRECORDSETS();

DROP VIEW products_above_average_price;
DROP VIEW current_product_list;

DROP TABLE categories;
DROP TABLE customercustomerdemo;
DROP TABLE customerdemographics;
DROP TABLE customers;
DROP TABLE employees;
DROP TABLE employeeterritories;
DROP TABLE gh_emptytable;
DROP TABLE "Order Details";
DROP TABLE orders;
DROP TABLE products;
DROP TABLE region;
DROP TABLE Results;
DROP TABLE shippers;
DROP TABLE shoppingcart;
DROP TABLE suppliers;
DROP TABLE territories;
DROP TABLE TYPES_SIMPLE;
DROP TABLE TYPES_EXTENDED;
DROP TABLE TYPES_SPECIFIC;


-- Create tables
----------------------------------------------------------------
CREATE TABLE categories (
  categoryid   serial unique NOT NULL,
  categoryname VARCHAR(30) NOT NULL,
  description  VARCHAR(4000)         NULL
)
WITHOUT OIDS;


CREATE INDEX categoryname
  ON categories (
    categoryname
);

CREATE TABLE customercustomerdemo (
  customerid     CHAR(10) NOT NULL,
  customertypeid CHAR(20) NOT NULL
)
WITHOUT OIDS;


ALTER TABLE customercustomerdemo
  ADD CONSTRAINT pk_customercustomerdemo UNIQUE (
    customerid,
    customertypeid
);

CREATE TABLE customerdemographics (
  customertypeid CHAR(20) NOT NULL,
  customerdesc   VARCHAR(4000)     NULL
)
WITHOUT OIDS;


ALTER TABLE customerdemographics
  ADD CONSTRAINT pk_customerdemographics UNIQUE (
    customertypeid
);

CREATE TABLE customers (
  customerid   CHAR(10)      NOT NULL,
  companyname  VARCHAR(80)  NOT NULL,
  contactname  VARCHAR(60)  NULL,
  contacttitle VARCHAR(60)  NULL,
  address      VARCHAR(120) NULL,
  city         VARCHAR(30)  NULL,
  region       VARCHAR(30)  NULL,
  postalcode   VARCHAR(20)  NULL,
  country      VARCHAR(30)  NULL,
  phone        VARCHAR(48)  NULL,
  fax          VARCHAR(48)  NULL
)
WITHOUT OIDS;

CREATE INDEX city
  ON customers (
    city
);

CREATE INDEX companyname_1
  ON customers (
    companyname
);

CREATE INDEX postalcode_2
  ON customers (
    postalcode
);

CREATE INDEX ix_region
  ON customers (
    region
);

ALTER TABLE customers
  ADD CONSTRAINT uk_customers_customerid UNIQUE (
    customerid
);

CREATE TABLE employees (
  employeeid      numeric(10,0)  NOT NULL,
  lastname        VARCHAR(40)  NOT NULL,
  firstname       VARCHAR(20)  NOT NULL,
  title           VARCHAR(60)  NULL,
  titleofcourtesy VARCHAR(50)  NULL,
  birthdate       Timestamp    NULL,
  hiredate        Timestamp    NULL,
  address         VARCHAR(120) NULL,
  city            VARCHAR(30)  NULL,
  region          VARCHAR(30)  NULL,
  postalcode      VARCHAR(20)  NULL,
  country         VARCHAR(30)  NULL,
  homephone       VARCHAR(48)  NULL,
  extension       VARCHAR(8)   NULL,
  notes           VARCHAR(4000)          NULL,
  reportsto       numeric(10,0)  NULL,
  photopath       VARCHAR(510) NULL,
  mycolumn        numeric(10,0)  NULL
)
WITHOUT OIDS;


CREATE INDEX lastname
  ON employees (
    lastname
);

CREATE INDEX postalcode_1
  ON employees (
    postalcode
);

ALTER TABLE employees
  ADD CONSTRAINT pk_employees UNIQUE (
    employeeid
);


CREATE TABLE employeeterritories (
  employeeid  numeric(10,0) NOT NULL,
  territoryid VARCHAR(40) NOT NULL
)
WITHOUT OIDS;


CREATE TABLE gh_emptytable (
  col1 INTEGER      NULL,
  col2 VARCHAR(50) NULL
)
WITHOUT OIDS;


CREATE TABLE "Order Details" (
  orderid   numeric(10,0) NOT NULL,
  productid numeric(10,0) NOT NULL,
  unitprice numeric(19,4) NOT NULL,
  quantity  numeric(5,0)  NOT NULL,
  discount  FLOAT(53)   NOT NULL
)
WITHOUT OIDS;


CREATE TABLE orders (
  orderid        numeric(10,0)  NOT NULL,
  customerid     NCHAR(5)      NULL,
  employeeid     numeric(10,0)  NULL,
  orderdate      DATE          NULL,
  requireddate   DATE          NULL,
  shippeddate    DATE          NULL,
  shipvia        numeric(10,0)  NULL,
  freight        numeric(19,4)  NULL,
  shipname       VARCHAR(40) NULL,
  shipaddress    VARCHAR(60) NULL,
  shipcity       VARCHAR(15) NULL,
  shipregion     VARCHAR(15) NULL,
  shippostalcode VARCHAR(10) NULL,
  shipcountry    VARCHAR(15) NULL
)
WITHOUT OIDS;

ALTER TABLE orders
  ADD CONSTRAINT orders_uk11075049410018 UNIQUE (
    orderid
);


ALTER TABLE orders
  ADD CONSTRAINT orders_fk21075049699981 FOREIGN KEY (
    orderid
  ) REFERENCES orders (
    orderid
);


CREATE TABLE products (
  productid       serial unique NOT NULL,
  productname     VARCHAR(80) NOT NULL,
  supplierid      numeric(10,0) NULL,
  categoryid      numeric(10,0) NULL,
  quantityperunit VARCHAR(40) NULL,
  unitprice       numeric(19,4) DEFAULT (0) NULL,
  unitsinstock    numeric(5,0)  DEFAULT (0) NULL,
  unitsonorder    numeric(5,0)  DEFAULT (0) NULL,
  reorderlevel    numeric(5,0)  DEFAULT (0) NULL,
  discontinued    numeric(1,0)  DEFAULT (0) NOT NULL
)
WITHOUT OIDS;


ALTER TABLE products
  ADD CONSTRAINT ck_products_unitprice CHECK (
    UnitPrice >= 0
);


ALTER TABLE products
  ADD CONSTRAINT ck_reorderlevel CHECK (
    ReorderLevel >= 0
);


ALTER TABLE products
  ADD CONSTRAINT ck_unitsinstock CHECK (
    UnitsInStock >= 0
);


ALTER TABLE products
  ADD CONSTRAINT ck_unitsonorder CHECK (
    UnitsOnOrder >= 0
);


CREATE INDEX categoryid
  ON products (
    categoryid
);


CREATE INDEX productname
  ON products (
    productname
);


CREATE INDEX suppliersproducts
  ON products (
    supplierid
);


CREATE TABLE region (
  regionid          numeric(10,0) NOT NULL,
  regiondescription CHAR(100)    NOT NULL
)
WITHOUT OIDS;


CREATE TABLE Results (
  employeeid      numeric(10,0)  NOT NULL,
  lastname        VARCHAR(20)  NOT NULL,
  firstname       VARCHAR(10)  NOT NULL,
  title           VARCHAR(30)  NULL,
  titleofcourtesy VARCHAR(25)  NULL,
  birthdate       DATE          NULL,
  hiredate        DATE          NULL,
  address         VARCHAR(60)  NULL,
  city            VARCHAR(15)  NULL,
  region          VARCHAR(15)  NULL,
  postalcode      VARCHAR(10)  NULL,
  country         VARCHAR(15)  NULL,
  homephone       VARCHAR(24)  NULL,
  extension       VARCHAR(4)   NULL,
  notes           VARCHAR(4000)          NULL,
  reportsto       numeric(10,0)  NULL,
  photopath       VARCHAR(255) NULL,
  mycolumn        numeric(10,0)  NULL
)
WITHOUT OIDS;


CREATE TABLE shippers (
  shipperid   serial unique NOT NULL,
  companyname VARCHAR(80) NOT NULL,
  phone       VARCHAR(48) NULL
)
WITHOUT OIDS;


CREATE TABLE shoppingcart (
  recordid    serial unique  NOT NULL,
  cartid      VARCHAR(100) NULL,
  quantity    numeric(10,0)  DEFAULT (1) NOT NULL,
  productid   numeric(10,0)  NOT NULL,
  datecreated TIMESTAMP  DEFAULT (CURRENT_TIMESTAMP) NOT NULL
)
WITHOUT OIDS;


CREATE TABLE suppliers (
  supplierid   serial unique  NOT NULL,
  companyname  VARCHAR(80)  NOT NULL,
  contactname  VARCHAR(60)  NULL,
  contacttitle VARCHAR(60)  NULL,
  address      VARCHAR(120) NULL,
  city         VARCHAR(30)  NULL,
  region       VARCHAR(30)  NULL,
  postalcode   VARCHAR(20)  NULL,
  country      VARCHAR(30)  NULL,
  phone        VARCHAR(48)  NULL,
  fax          VARCHAR(48)  NULL,
  homepage     VARCHAR(4000)          NULL
)
WITHOUT OIDS;


CREATE INDEX companyname
  ON suppliers (
    companyname
);

CREATE INDEX postalcode
  ON suppliers (
    postalcode
);

CREATE TABLE territories (
  territoryid          VARCHAR(40) NOT NULL,
  territorydescription CHAR(100)    NOT NULL,
  regionid             numeric(10,0) NOT NULL
)
WITHOUT OIDS;

CREATE TABLE TYPES_SIMPLE (
  id char(10),
  t_bool bool,
  t_int2 int2,
  t_int4 int4,
  t_int8 int8,
  t_numeric numeric(10),
  t_float4 float4,
  t_float8 float8,
  t_varchar varchar(50),
  t_char char(10),
  t_nchar char(10)
)
WITHOUT OIDS;

CREATE TABLE TYPES_EXTENDED (
  id char(10),
  t_bytea bytea,
  t_date date,
  t_text text,
  t_time time,
  t_timestamp timestamp
)
WITHOUT OIDS;

CREATE TABLE TYPES_SPECIFIC (
  id char(10),
  t_bit bit(1),
  t_box box,
  t_cidr cidr,
  t_circle circle,
  t_inet inet,
  t_interval interval (6),  
  t_line line,
  t_lseg lseg,
  t_macaddr macaddr,
  t_money money,
  t_path path,
  t_point point,
  t_polygon polygon,
  t_serial serial,
  t_bigserial bigserial,
  t_timetz timetz,  
  t_timestamptz timestamptz
)
WITHOUT OIDS;

-- Create views
----------------------------------------------------------------
CREATE OR REPLACE VIEW products_above_average_price (
  productname,
  unitprice
) AS
SELECT  Products.ProductName, Products.UnitPrice
 FROM Products
	WHERE Products.UnitPrice > (
		SELECT  AVG(UnitPrice)
		 FROM Products  );


CREATE OR REPLACE VIEW current_product_list (
  productid,
  productname
) AS
SELECT  Product_List.ProductID, Product_List.ProductName
 FROM Products Product_List
	WHERE
	(
	(( Product_List.Discontinued ) = 0));


-- Create functions
----------------------------------------------------------------
CREATE FUNCTION GH_MULTIRECORDSETS() RETURNS SETOF refcursor AS $$
DECLARE 
	rct1 refcursor;
	rct2 refcursor;
	rct3 refcursor;
BEGIN
	OPEN rct1 FOR
	SELECT EmployeeId, LastName FROM Employees where EmployeeId in (1,2) ORDER BY EMPLOYEEID ASC;
	RETURN NEXT rct1;

	OPEN rct2 FOR
	SELECT CustomerId, CompanyName,ContactName FROM Customers  where CustomerId in ('MORGK','NORTS') ORDER BY CustomerId ASC;
	RETURN NEXT rct2;

	OPEN rct3 FOR
	SELECT OrderId, ShipAddress,ShipVia, ShipCity FROM Orders where OrderId=-1  ;
	RETURN NEXT rct3;
	RETURN;
END;
$$ LANGUAGE 'plpgsql';

CREATE FUNCTION gh_createtable() RETURNS void AS $$
BEGIN
  --craete a temporary table
  execute 'Create Table temp_tbl (Col1 int,Col2 int)';
  --insert values to the table
  execute 'insert into temp_tbl values (11,12)';
  execute 'insert into temp_tbl values (21,22)';
  execute 'insert into temp_tbl values (31,32)';
  --execute select on the created table
  execute 'select col1 as Value1, col2 as Value2 from temp_tbl';
  execute 'drop table temp_tbl';
  RETURN;
END;
$$ LANGUAGE 'plpgsql';

CREATE FUNCTION GH_REFCURSOR1() RETURNS refcursor AS $$
DECLARE 
	rct1 refcursor;
BEGIN
	OPEN rct1 FOR
	SELECT EmployeeId, LastName FROM Employees where EmployeeId = 1;
	RETURN rct1;
END;
$$ LANGUAGE 'plpgsql';


CREATE FUNCTION GH_REFCURSOR2(integer) RETURNS refcursor AS $$
DECLARE 
	rct1 refcursor;
BEGIN
	OPEN rct1 FOR
	SELECT EmployeeId, LastName FROM Employees where EmployeeId = $1;
	RETURN rct1;
END;
$$ LANGUAGE 'plpgsql';

CREATE FUNCTION GH_REFCURSOR3(varchar) RETURNS refcursor AS $$
DECLARE 
	rct1 refcursor;
BEGIN
	OPEN rct1 FOR
	SELECT EmployeeId, LastName FROM Employees where LastName = $1;
	RETURN rct1;
END;
$$ LANGUAGE 'plpgsql';

CREATE FUNCTION GHSP_TYPES_SIMPLE_1(BOOL, INT2, INT4, INT8, NUMERIC, FLOAT4, FLOAT8, VARCHAR, CHAR, NCHAR)  RETURNS refcursor AS $$
DECLARE 
	rct1 refcursor;
BEGIN
	OPEN rct1 FOR
	SELECT $1 as T_BOOL, $2 as T_INT2, $3 as T_INT4, $4 as T_INT8, $5 as T_NUMERIC, $6 as T_FLOAT4, $7 as T_FLOAT8, $8 as T_VARCHAR, $9 as T_CHAR, $10 as T_NCHAR;
	RETURN rct1;
END
$$  LANGUAGE 'plpgsql';

CREATE FUNCTION GHSP_TYPES_SIMPLE_4(VARCHAR) RETURNS SETOF refcursor AS $$
DECLARE
	rct1 refcursor;
	rct2 refcursor;
	rct3 refcursor;
BEGIN

	insert into TYPES_SIMPLE(ID,t_numeric) values ($1,50);
	
	OPEN rct1 FOR
	SELECT * FROM TYPES_SIMPLE where ID = $1;
	RETURN NEXT rct1;

	update TYPES_SIMPLE set t_numeric=60 where Id = $1;

	OPEN rct2 FOR
	SELECT * FROM TYPES_SIMPLE where ID = $1;
	RETURN NEXT rct2;

	delete from TYPES_SIMPLE where ID = $1;

	OPEN rct3 FOR
	SELECT * FROM TYPES_SIMPLE where ID = $1;
	RETURN NEXT rct3;

	RETURN;
END;
$$ LANGUAGE 'plpgsql';

CREATE FUNCTION GHSP_TYPES_SIMPLE_5() RETURNS refcursor AS $$
DECLARE 
   rct1 refcursor;
   T_BOOL    boolean        :=true;
   T_INT2      int2           := 21;
   T_INT4      int4           := 30000;
   T_INT8      int8           := 30001;
   T_NUMERIC	NUMERIC(10)    := 100000;
   T_FLOAT4    	FLOAT4      := 7.23157;
   T_FLOAT8		FLOAT8         := 7.123456;
   T_VARCHAR   	VARCHAR(10)    := 'qwertasdfg';
   T_CHAR	CHAR(10)       := 'abcdefghij';
   T_NCHAR	NCHAR(10)       := 'klmnopqrst';
 
BEGIN
  OPEN rct1 FOR
  SELECT T_BOOL, T_INT2, T_INT4, T_INT8, T_NUMERIC, T_FLOAT4, T_FLOAT8, T_VARCHAR, T_CHAR, T_NCHAR;
  RETURN rct1;
END;
$$ LANGUAGE 'plpgsql';


CREATE FUNCTION GH_DUMMY(NUMERIC) RETURNS refcursor AS $$
DECLARE 
   rct1 refcursor;
 
BEGIN
  OPEN rct1 FOR
  SELECT EMPLOYEEID,
         LASTNAME, 
         FIRSTNAME, 
         TITLE, 
         TITLEOFCOURTESY, 
         BIRTHDATE, 
         HIREDATE, 
         ADDRESS, 
         CITY, 
         REGION, 
         POSTALCODE, 
         COUNTRY, 
         HOMEPHONE, 
         EXTENSION, 
         REPORTSTO, 
         PHOTOPATH, 
         MYCOLUMN
  	FROM EMPLOYEES where EmployeeID > $1;
  RETURN rct1;
END;
$$ LANGUAGE 'plpgsql';