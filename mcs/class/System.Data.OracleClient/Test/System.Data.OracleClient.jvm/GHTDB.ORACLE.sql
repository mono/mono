DROP INDEX GHTDB.categoryname;
DROP INDEX GHTDB.city;
DROP INDEX GHTDB.companyname_1;
DROP INDEX GHTDB.postalcode_2;
DROP INDEX GHTDB.region;
DROP INDEX GHTDB.lastname;
DROP INDEX GHTDB.postalcode_1;
DROP INDEX GHTDB.categoryid;
DROP INDEX GHTDB.productname;
DROP INDEX GHTDB.suppliersproducts;
DROP INDEX GHTDB.companyname;
DROP INDEX GHTDB.postalcode;

DROP TABLE GHTDB.TYPES_SIMPLE;
DROP TABLE GHTDB.TYPES_EXTENDED;
DROP TABLE GHTDB.TYPES_SPECIFIC;
DROP TABLE GHTDB.categories;
DROP TABLE GHTDB.customercustomerdemo;
DROP TABLE GHTDB.customerdemographics;
DROP TABLE GHTDB.customers;
DROP TABLE GHTDB.employees;
DROP TABLE GHTDB.employeeterritories;
DROP TABLE GHTDB.gh_emptytable;
DROP TABLE GHTDB."Order Details";
DROP TABLE GHTDB.orders;
DROP TABLE GHTDB.products;
DROP TABLE GHTDB.region;
DROP TABLE GHTDB.Results;
DROP TABLE GHTDB.shippers;
DROP TABLE GHTDB.shoppingcart;
DROP TABLE GHTDB.suppliers;
DROP TABLE GHTDB.territories;

DROP SEQUENCE GHTDB.s_463_1_reviews;
DROP SEQUENCE GHTDB.s_464_1_shoppingcart;
DROP SEQUENCE GHTDB.s_59_1_products;
DROP SEQUENCE GHTDB.s_60_1_orders;
DROP SEQUENCE GHTDB.s_62_1_suppliers;
DROP SEQUENCE GHTDB.s_63_1_shippers;
DROP SEQUENCE GHTDB.s_65_1_employees;
DROP SEQUENCE GHTDB.s_68_1_categories;

DROP USER GHTDB CASCADE;

CREATE USER "GHTDB"
    IDENTIFIED BY "GHTDB" DEFAULT TABLESPACE "USERS"
    TEMPORARY TABLESPACE "TEMP" ;

GRANT CONNECT, RESOURCE, CREATE TABLE  TO "GHTDB";
ALTER USER GHTDB quota unlimited on USERS;

CREATE OR REPLACE  PACKAGE "GHTDB"."GHTPKG"  AS
	TYPE RCT1 IS REF CURSOR;
	IDENTITY INTEGER;
	
	procedure ghsp_pkgAmbig(res out rct1);
	procedure ghsp_inPkg(CustomerIdPrm IN CHAR, result OUT RCT1);
END;
/

CREATE TABLE GHTDB.TYPES_SIMPLE (
	ID CHAR(10) NULL,
	T_NUMBER NUMBER(10) NULL, 
	T_LONG LONG NULL, 
	T_FLOAT FLOAT(10) NULL, 
	T_VARCHAR VARCHAR2(10) NULL, 
	T_NVARCHAR NVARCHAR2(10) NULL, 
	T_CHAR CHAR(10) NULL,
	T_NCHAR NCHAR(10) NULL
)
  TABLESPACE USERS
/



CREATE TABLE GHTDB.TYPES_EXTENDED (
	ID CHAR(10) NULL,
	T_RAW RAW(10) NULL, 
	T_LONGRAW LONG RAW NULL, 
	T_DATE DATE NULL, 
	T_BLOB BLOB NULL, 
	T_CLOB CLOB NULL, 
	T_NCLOB NCLOB NULL
)
  TABLESPACE USERS
/

CREATE TABLE GHTDB.TYPES_SPECIFIC (
	ID	CHAR(10) NULL,
	T_ROWID ROWID NULL, 
	T_UROWID UROWID NULL, 
	T_BFILE BFILE NULL, 
	T_XMLTYPE SYS.XMLTYPE NULL
)
  TABLESPACE USERS 
/

CREATE TABLE GHTDB.categories (
  categoryid   NUMBER(10,0) NOT NULL,
  categoryname VARCHAR2(30) NOT NULL,
  description  CLOB         NULL
)
  TABLESPACE USERS
/


CREATE INDEX GHTDB.categoryname
  ON GHTDB.categories (
    categoryname
  )
  TABLESPACE USERS
/

CREATE TABLE GHTDB.customercustomerdemo (
  customerid     CHAR(10) NOT NULL,
  customertypeid CHAR(20) NOT NULL
)
  TABLESPACE USERS
/


ALTER TABLE GHTDB.customercustomerdemo
  ADD CONSTRAINT pk_customercustomerdemo UNIQUE (
    customerid,
    customertypeid
  )
/

CREATE TABLE GHTDB.customerdemographics (
  customertypeid CHAR(20) NOT NULL,
  customerdesc   CLOB     NULL
)
  TABLESPACE USERS
/


ALTER TABLE GHTDB.customerdemographics
  ADD CONSTRAINT pk_customerdemographics UNIQUE (
    customertypeid
  )
/

CREATE TABLE GHTDB.customers (
  customerid   CHAR(10)      NOT NULL,
  companyname  VARCHAR2(80)  NOT NULL,
  contactname  VARCHAR2(60)  NULL,
  contacttitle VARCHAR2(60)  NULL,
  address      VARCHAR2(120) NULL,
  city         VARCHAR2(30)  NULL,
  region       VARCHAR2(30)  NULL,
  postalcode   VARCHAR2(20)  NULL,
  country      VARCHAR2(30)  NULL,
  phone        VARCHAR2(48)  NULL,
  fax          VARCHAR2(48)  NULL
)
  TABLESPACE USERS
/


CREATE INDEX GHTDB.city
  ON GHTDB.customers (
    city
  )
/

CREATE INDEX GHTDB.companyname_1
  ON GHTDB.customers (
    companyname
  )
/

CREATE INDEX GHTDB.postalcode_2
  ON GHTDB.customers (
    postalcode
  )
/

CREATE INDEX GHTDB.region
  ON GHTDB.customers (
    region
  )
/

ALTER TABLE GHTDB.customers
  ADD CONSTRAINT uk_customers_customerid UNIQUE (
    customerid
  )
/

CREATE TABLE GHTDB.employees (
  employeeid      NUMBER(10,0)  NOT NULL,
  lastname        VARCHAR2(40)  NOT NULL,
  firstname       VARCHAR2(20)  NOT NULL,
  title           VARCHAR2(60)  NULL,
  titleofcourtesy VARCHAR2(50)  NULL,
  birthdate       DATE          NULL,
  hiredate        DATE          NULL,
  address         VARCHAR2(120) NULL,
  city            VARCHAR2(30)  NULL,
  region          VARCHAR2(30)  NULL,
  postalcode      VARCHAR2(20)  NULL,
  country         VARCHAR2(30)  NULL,
  homephone       VARCHAR2(48)  NULL,
  extension       VARCHAR2(8)   NULL,
  notes           CLOB          NULL,
  reportsto       NUMBER(10,0)  NULL,
  photopath       VARCHAR2(510) NULL,
  mycolumn        NUMBER(10,0)  NULL
)
  TABLESPACE USERS
/


CREATE INDEX GHTDB.lastname
  ON GHTDB.employees (
    lastname
  )
  TABLESPACE USERS
/

CREATE INDEX GHTDB.postalcode_1
  ON GHTDB.employees (
    postalcode
  )
  TABLESPACE USERS
/

ALTER TABLE GHTDB.employees
  ADD CONSTRAINT pk_employees UNIQUE (
    employeeid
  )
/

CREATE TABLE GHTDB.employeeterritories (
  employeeid  NUMBER(10,0) NOT NULL,
  territoryid VARCHAR2(40) NOT NULL
)
  TABLESPACE USERS
/



CREATE TABLE GHTDB.gh_emptytable (
  col1 INTEGER      NULL,
  col2 VARCHAR2(50) NULL
)
  TABLESPACE USERS
/


CREATE TABLE GHTDB."Order Details" (
  orderid   NUMBER(10,0) NOT NULL,
  productid NUMBER(10,0) NOT NULL,
  unitprice NUMBER(19,4) NOT NULL,
  quantity  NUMBER(5,0)  NOT NULL,
  discount  FLOAT(126)   NOT NULL
)
  TABLESPACE USERS
/


CREATE TABLE GHTDB.orders (
  orderid        NUMBER(10,0)  NOT NULL,
  customerid     NCHAR(5)      NULL,
  employeeid     NUMBER(10,0)  NULL,
  orderdate      DATE          NULL,
  requireddate   DATE          NULL,
  shippeddate    DATE          NULL,
  shipvia        NUMBER(10,0)  NULL,
  freight        NUMBER(19,4)  NULL,
  shipname       NVARCHAR2(40) NULL,
  shipaddress    NVARCHAR2(60) NULL,
  shipcity       NVARCHAR2(15) NULL,
  shipregion     NVARCHAR2(15) NULL,
  shippostalcode NVARCHAR2(10) NULL,
  shipcountry    NVARCHAR2(15) NULL
)
  TABLESPACE USERS
/

ALTER TABLE GHTDB.orders
  ADD CONSTRAINT orders_uk11075049410018 UNIQUE (
    orderid
  )
/

ALTER TABLE GHTDB.orders
  ADD CONSTRAINT orders_fk21075049699981 FOREIGN KEY (
    orderid
  ) REFERENCES GHTDB.orders (
    orderid
  )
/

CREATE TABLE GHTDB.products (
  productid       NUMBER(10,0) NOT NULL,
  productname     VARCHAR2(80) NOT NULL,
  supplierid      NUMBER(10,0) NULL,
  categoryid      NUMBER(10,0) NULL,
  quantityperunit VARCHAR2(40) NULL,
  unitprice       NUMBER(19,4) DEFAULT (0) NULL,
  unitsinstock    NUMBER(5,0)  DEFAULT (0) NULL,
  unitsonorder    NUMBER(5,0)  DEFAULT (0) NULL,
  reorderlevel    NUMBER(5,0)  DEFAULT (0) NULL,
  discontinued    NUMBER(1,0)  DEFAULT (0) NOT NULL
)
  TABLESPACE USERS
/


ALTER TABLE GHTDB.products
  ADD CONSTRAINT ck_products_unitprice CHECK (
    UnitPrice >= 0
  )
/

ALTER TABLE GHTDB.products
  ADD CONSTRAINT ck_reorderlevel CHECK (
    ReorderLevel >= 0
  )
/

ALTER TABLE GHTDB.products
  ADD CONSTRAINT ck_unitsinstock CHECK (
    UnitsInStock >= 0
  )
/

ALTER TABLE GHTDB.products
  ADD CONSTRAINT ck_unitsonorder CHECK (
    UnitsOnOrder >= 0
  )
/

CREATE INDEX GHTDB.categoryid
  ON GHTDB.products (
    categoryid
  )
/

CREATE INDEX GHTDB.productname
  ON GHTDB.products (
    productname
  )
/

CREATE INDEX GHTDB.suppliersproducts
  ON GHTDB.products (
    supplierid
  )
/

CREATE TABLE GHTDB.region (
  regionid          NUMBER(10,0) NOT NULL,
  regiondescription CHAR(100)    NOT NULL
)
  TABLESPACE USERS
/


CREATE TABLE GHTDB.Results (
  employeeid      NUMBER(10,0)  NOT NULL,
  lastname        VARCHAR2(20)  NOT NULL,
  firstname       VARCHAR2(10)  NOT NULL,
  title           VARCHAR2(30)  NULL,
  titleofcourtesy VARCHAR2(25)  NULL,
  birthdate       DATE          NULL,
  hiredate        DATE          NULL,
  address         VARCHAR2(60)  NULL,
  city            VARCHAR2(15)  NULL,
  region          VARCHAR2(15)  NULL,
  postalcode      VARCHAR2(10)  NULL,
  country         VARCHAR2(15)  NULL,
  homephone       VARCHAR2(24)  NULL,
  extension       VARCHAR2(4)   NULL,
  notes           CLOB          NULL,
  reportsto       NUMBER(10,0)  NULL,
  photopath       VARCHAR2(255) NULL,
  mycolumn        NUMBER(10,0)  NULL
)
  TABLESPACE USERS
/


CREATE TABLE GHTDB.shippers (
  shipperid   NUMBER(10,0) NOT NULL,
  companyname VARCHAR2(80) NOT NULL,
  phone       VARCHAR2(48) NULL
)
  TABLESPACE USERS
/


CREATE TABLE GHTDB.shoppingcart (
  recordid    NUMBER(10,0)  NOT NULL,
  cartid      VARCHAR2(100) NULL,
  quantity    NUMBER(10,0)  DEFAULT (1) NOT NULL,
  productid   NUMBER(10,0)  NOT NULL,
  datecreated DATE          DEFAULT (SYSDATE) NOT NULL
)
  TABLESPACE USERS
/


CREATE TABLE GHTDB.suppliers (
  supplierid   NUMBER(10,0)  NOT NULL,
  companyname  VARCHAR2(80)  NOT NULL,
  contactname  VARCHAR2(60)  NULL,
  contacttitle VARCHAR2(60)  NULL,
  address      VARCHAR2(120) NULL,
  city         VARCHAR2(30)  NULL,
  region       VARCHAR2(30)  NULL,
  postalcode   VARCHAR2(20)  NULL,
  country      VARCHAR2(30)  NULL,
  phone        VARCHAR2(48)  NULL,
  fax          VARCHAR2(48)  NULL,
  homepage     CLOB          NULL
)
  TABLESPACE USERS
/


CREATE INDEX GHTDB.companyname
  ON GHTDB.suppliers (
    companyname
  )
  TABLESPACE USERS
/

CREATE INDEX GHTDB.postalcode
  ON GHTDB.suppliers (
    postalcode
  )
  TABLESPACE USERS
/

CREATE TABLE GHTDB.territories (
  territoryid          VARCHAR2(40) NOT NULL,
  territorydescription CHAR(100)    NOT NULL,
  regionid             NUMBER(10,0) NOT NULL
)
  TABLESPACE USERS
/


CREATE SEQUENCE GHTDB.s_463_1_reviews
  MINVALUE 1
  MAXVALUE 999999999999999999999999999
  INCREMENT BY 1
  NOCYCLE
  NOORDER
  CACHE 20
/

CREATE SEQUENCE GHTDB.s_464_1_shoppingcart
  MINVALUE 1
  MAXVALUE 999999999999999999999999999
  INCREMENT BY 1
  NOCYCLE
  NOORDER
  CACHE 20
/

CREATE SEQUENCE GHTDB.s_59_1_products
  MINVALUE 1
  MAXVALUE 999999999999999999999999999
  INCREMENT BY 1
  NOCYCLE
  NOORDER
  CACHE 20
/

CREATE SEQUENCE GHTDB.s_60_1_orders
  MINVALUE 1
  MAXVALUE 999999999999999999999999999
  INCREMENT BY 1
  NOCYCLE
  NOORDER
  CACHE 20
/

CREATE SEQUENCE GHTDB.s_62_1_suppliers
  MINVALUE 1
  MAXVALUE 999999999999999999999999999
  INCREMENT BY 1
  NOCYCLE
  NOORDER
  CACHE 20
/

CREATE SEQUENCE GHTDB.s_63_1_shippers
  MINVALUE 1
  MAXVALUE 999999999999999999999999999
  INCREMENT BY 1
  NOCYCLE
  NOORDER
  CACHE 20
/

CREATE SEQUENCE GHTDB.s_65_1_employees
  MINVALUE 1
  MAXVALUE 999999999999999999999999999
  INCREMENT BY 1
  NOCYCLE
  NOORDER
  CACHE 20
/

CREATE SEQUENCE GHTDB.s_68_1_categories
  MINVALUE 1
  MAXVALUE 999999999999999999999999999
  INCREMENT BY 1
  NOCYCLE
  NOORDER
  CACHE 20
/

CREATE OR REPLACE TRIGGER GHTDB.tr_s_464_1_shoppingcart
BEFORE INSERT
ON GHTDB.shoppingcart
FOR EACH ROW
BEGIN
 IF (:new.RECORDID IS NULL) THEN
  SELECT S_464_1_SHOPPINGCART.nextval
  INTO :NEW.RECORDID
  FROM dual;
 END IF;
END;
/

CREATE OR REPLACE TRIGGER GHTDB.tr_s_59_1_products
BEFORE INSERT
ON GHTDB.products
FOR EACH ROW
BEGIN
 IF (:new.PRODUCTID IS NULL) THEN
  SELECT S_59_1_PRODUCTS.nextval
  INTO :NEW.PRODUCTID
  FROM dual;
 END IF;
END;
/

CREATE OR REPLACE TRIGGER GHTDB.tr_s_62_1_suppliers
BEFORE INSERT
ON GHTDB.suppliers
FOR EACH ROW
BEGIN
 IF (:new.SUPPLIERID IS NULL) THEN
  SELECT S_62_1_SUPPLIERS.nextval
  INTO :NEW.SUPPLIERID
  FROM dual;
 END IF;
END;
/

CREATE OR REPLACE TRIGGER GHTDB.tr_s_63_1_shippers
BEFORE INSERT
ON GHTDB.shippers
FOR EACH ROW
BEGIN
 IF (:new.SHIPPERID IS NULL) THEN
  SELECT S_63_1_SHIPPERS.nextval
  INTO :NEW.SHIPPERID
  FROM dual;
 END IF;
END;
/

CREATE OR REPLACE TRIGGER GHTDB.tr_s_68_1_categories
BEFORE INSERT
ON GHTDB.categories
FOR EACH ROW
BEGIN
 IF (:new.CATEGORYID IS NULL) THEN
  SELECT S_68_1_CATEGORIES.nextval
  INTO :NEW.CATEGORYID
  FROM dual;
 END IF;
END;
/

CREATE OR REPLACE VIEW GHTDB.current_product_list (
  productid,
  productname
) AS
SELECT  Product_List.ProductID, Product_List.ProductName
 FROM Products Product_List
	WHERE
	(
	(( Product_List.Discontinued ) = 0))
/

CREATE OR REPLACE VIEW GHTDB.customer_and_suppliers_by_cit (
  city,
  companyname,
  contactname,
  relationship
) AS
SELECT  City, CompanyName, ContactName, 'Customers' Relationship
 FROM Customers UNION SELECT  City, CompanyName, ContactName, 'Suppliers'
 FROM Suppliers
/

CREATE OR REPLACE VIEW GHTDB.products_above_average_price (
  productname,
  unitprice
) AS
SELECT  Products.ProductName, Products.UnitPrice
 FROM Products
	WHERE Products.UnitPrice > (
		SELECT  AVG(UnitPrice)
		 FROM Products  )
/

CREATE OR REPLACE  PACKAGE BODY "GHTDB"."GHTPKG"  AS

--Procedure declerations:
	procedure ghsp_pkgAmbig(res out rct1) is
  begin
	   --Return a value which indocates that the procedure 'ghsp_pkgAmbig' was called not from within GHTPKG.
     declare IN_GHTPKG varchar2(4) := 'TRUE';
     begin
  	   open res for
  	   select IN_GHTPKG as IN_PKG from dual;
	   end;
  end;

  procedure ghsp_inPkg(CustomerIdPrm in char, result out rct1) is
  begin
  OPEN result FOR
  SELECT * FROM Customers where CustomerId=CustomerIdPrm;
  end;
  
END;
/

CREATE OR REPLACE  PROCEDURE "GHTDB"."GH_MULTIRECORDSETS" (
    RCT_Employees OUT GHTPKG.RCT1,
    RCT_Customers OUT GHTPKG.RCT1,
    RCT_Orders OUT GHTPKG.RCT1
 )
 IS
 -- Declare cursor
    m_RCT_Employees GHTPKG.RCT1;
    m_RCT_Customers GHTPKG.RCT1;
    m_RCT_Orders GHTPKG.RCT1;
 BEGIN
    OPEN m_RCT_Employees FOR
    SELECT EmployeeId, LastName FROM Employees where EmployeeId in (1,2) ORDER BY EMPLOYEEID ASC;

    OPEN m_RCT_Customers FOR
    SELECT CustomerId, CompanyName,ContactName FROM Customers  where CustomerId in ('MORGK','NORTS') ORDER BY CustomerId ASC;
    -- return empty result set
    OPEN m_RCT_Orders FOR
    SELECT OrderId, ShipAddress,ShipVia, ShipCity FROM Orders where OrderId=-1  ;

    RCT_Employees := m_RCT_Employees;
    RCT_Customers := m_RCT_Customers;
    RCT_Orders := m_RCT_Orders;
END;
/

CREATE OR REPLACE  PROCEDURE "GHTDB"."GH_INOUT1" (
INPARAM IN VARCHAR DEFAULT NULL,
OUTPARAM OUT INTEGER 
)
AS
L_INPARAM VARCHAR(30) := INPARAM;
BEGIN
OUTPARAM := 100;
END;
/

CREATE OR REPLACE  PROCEDURE "GHTDB"."GH_CREATETABLE"  AS
Begin
  --craete a temporary table
  execute immediate 'Create global temporary Table ghtdb.temp_tbl (Col1 int,Col2 int)';
  --insert values to the table
  execute immediate 'insert into ghtdb.temp_tbl values (11,12)';
  execute immediate 'insert into ghtdb.temp_tbl values (21,22)';
  execute immediate 'insert into ghtdb.temp_tbl values (31,32)';
  --execute select on the created table
  execute immediate 'select col1 as Value1, col2 as Value2 from ghtdb.temp_tbl';
  execute immediate 'drop table temp_tbl';
end;
/

CREATE OR REPLACE  PROCEDURE "GHTDB"."GH_REFCURSOR1" (
    RCT_Employees OUT GHTPKG.RCT1
 )
 IS
    m_RCT_Employees GHTPKG.RCT1;
 BEGIN
    OPEN m_RCT_Employees FOR
    SELECT EmployeeId, LastName FROM GHTDB.Employees where EmployeeId = 1;

    RCT_Employees := m_RCT_Employees;
END;
/

CREATE OR REPLACE  PROCEDURE "GHTDB"."GH_REFCURSOR2" (
    IN_EMPLOYEEID INTEGER,
    RCT_Employees OUT GHTPKG.RCT1
 )
 IS
    m_RCT_Employees GHTPKG.RCT1;
 BEGIN
    OPEN m_RCT_Employees FOR
    SELECT EmployeeId, LastName FROM GHTDB.Employees where EmployeeId = IN_EMPLOYEEID;

    RCT_Employees := m_RCT_Employees;
END;
/

CREATE OR REPLACE  PROCEDURE "GHTDB"."GH_REFCURSOR3" (
    IN_LASTNAME VARCHAR2,
    RCT_Employees OUT GHTPKG.RCT1
 )
 IS
    m_RCT_Employees GHTPKG.RCT1;
 BEGIN
    OPEN m_RCT_Employees FOR
    SELECT EmployeeId, LastName FROM GHTDB.Employees where LastName = IN_LASTNAME;

    RCT_Employees := m_RCT_Employees;
END;
/


CREATE OR REPLACE PROCEDURE GHTDB.GHSP_TYPES_SIMPLE_1 (
T_NUMBER   IN NUMBER,
T_LONG     IN LONG,
T_FLOAT    IN FLOAT,
T_VARCHAR  IN VARCHAR2,
T_NVARCHAR IN NVARCHAR2,
T_CHAR     IN CHAR,
T_NCHAR    IN NCHAR,
RESULT     OUT GHTPKG.RCT1
)
AS
BEGIN
OPEN RESULT FOR
SELECT T_NUMBER AS T_NUMBER, T_LONG AS T_LONG, T_FLOAT AS T_FLOAT, T_VARCHAR AS T_VARCHAR, T_NVARCHAR AS T_NVARCHAR, T_CHAR AS T_CHAR, T_NCHAR AS T_NCHAR FROM DUAL;
RETURN;
END;
/



CREATE OR REPLACE PROCEDURE GHTDB.GHSP_TYPES_SIMPLE_2 (
T_NUMBER   IN OUT NUMBER,
T_LONG     IN OUT LONG,
T_FLOAT    IN OUT FLOAT,
T_VARCHAR  IN OUT VARCHAR2,
T_NVARCHAR IN OUT NVARCHAR2,
T_CHAR     IN OUT CHAR,
T_NCHAR    IN OUT NCHAR
)
AS
BEGIN
T_NUMBER := T_NUMBER * 2;
T_LONG := UPPER(T_LONG);
T_FLOAT := T_FLOAT * 2;
T_VARCHAR := UPPER(T_VARCHAR);
T_NVARCHAR := UPPER(T_NVARCHAR);
T_CHAR := UPPER(T_CHAR);
T_NCHAR := UPPER(T_NCHAR);
END;
/



CREATE OR REPLACE PROCEDURE GHTDB.GHSP_TYPES_SIMPLE_3 (
ID1        IN CHAR,
P_NUMBER   OUT NUMBER,
P_LONG     OUT LONG,
P_FLOAT    OUT FLOAT,
P_VARCHAR  OUT VARCHAR2,
P_NVARCHAR OUT NVARCHAR2,
P_CHAR     OUT CHAR,
P_NCHAR    OUT NCHAR
)
AS
BEGIN
SELECT T_NUMBER, T_LONG, T_FLOAT, T_VARCHAR, T_NVARCHAR, T_CHAR, T_NCHAR INTO P_NUMBER, P_LONG, P_FLOAT, P_VARCHAR, P_NVARCHAR, P_CHAR, P_NCHAR FROM TYPES_SIMPLE WHERE ID = ID1;
RETURN;
END;
/



CREATE OR REPLACE  PROCEDURE GHTDB.GHSP_TYPES_SIMPLE_4  (
ID1        IN CHAR,
RESULT     OUT GHTPKG.RCT1,
RESULT1    OUT GHTPKG.RCT1,
RESULT2    OUT GHTPKG.RCT1
)
IS

m_result  GHTPKG.RCT1;
m_result1  GHTPKG.RCT1;
m_result2  GHTPKG.RCT1;

BEGIN

insert into TYPES_SIMPLE(ID,T_NUMBER) values (ID1,50);

OPEN m_result FOR
SELECT * FROM TYPES_SIMPLE where ID = ID1;

update TYPES_SIMPLE set T_NUMBER=60 where Id = ID1;

OPEN m_result1 FOR
SELECT * FROM TYPES_SIMPLE where ID = ID1;

delete from TYPES_SIMPLE where ID = ID1;

OPEN m_result2 FOR
SELECT * FROM TYPES_SIMPLE where ID = ID1;

RESULT := m_result;
RESULT1 := m_result1;
RESULT2 := m_result2;

END;
/



CREATE OR REPLACE PROCEDURE   GHTDB.GHSP_TYPES_SIMPLE_5(
RESULT     OUT GHTPKG.RCT1
)AS

BEGIN
  DECLARE
    T_NUMBER    NUMBER(10)    := 21;
    T_LONG      LONG          := 'abcdefghijklmnopqrstuvwxyz1234567890~!@#$%^&*()_+-=[]\|;:,./<>? ';
    T_FLOAT     FLOAT(10)     := 1.234;
    T_VARCHAR   VARCHAR2(10)  := 'qwertasdfg';
    T_NVARCHAR  NVARCHAR2(10) := 'qwertasdfg';
    T_CHAR      CHAR(10)      := 'abcdefghij';
    T_NCHAR     NCHAR(10)     := 'abcdefghij';

  BEGIN
  OPEN RESULT FOR
  SELECT T_NUMBER,T_LONG, T_FLOAT, T_VARCHAR, T_NVARCHAR, T_CHAR, T_NCHAR FROM DUAL;
  RETURN;
  END;

END;
/

CREATE OR REPLACE  PROCEDURE "GHTDB"."GHSP_PKGAMBIG"  (
 res out ghtpkg.rct1
)
as
begin
	 --Return a value which indocates that the procedure 'ghsp_pkgAmbig' was called not from within GHTPKG.
   declare IN_GHTPKG varchar2(5) := 'FALSE';
   begin
  	 open res for
	   select IN_GHTPKG as IN_PKG from dual;
	 end;
end;
/

CREATE OR REPLACE  PROCEDURE "GHTDB"."GH_DUMMY"  (
 EmployeeIDPrm in NUMBER,
 result     OUT GHTPKG.RCT1
)
as
begin
 OPEN result FOR
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
  FROM GHTDB.EMPLOYEES where EmployeeID > EmployeeIDPrm;
RETURN;
end;
/

GRANT DELETE,INSERT,SELECT,UPDATE ON GHTDB.categories TO PUBLIC;
GRANT DELETE,INSERT,SELECT,UPDATE ON GHTDB.current_product_list TO PUBLIC;
GRANT DELETE,INSERT,SELECT,UPDATE ON GHTDB.customercustomerdemo TO PUBLIC;
GRANT DELETE,INSERT,SELECT,UPDATE ON GHTDB.customerdemographics TO PUBLIC;
GRANT DELETE,INSERT,SELECT,UPDATE ON GHTDB.customers TO PUBLIC;
GRANT EXECUTE ON GHTDB.GH_MULTIRECORDSETS TO PUBLIC;
--GRANT EXECUTE ON GHTDB.custordersorders TO PUBLIC;
GRANT DELETE,INSERT,SELECT,UPDATE ON GHTDB.employees TO PUBLIC;
GRANT DELETE,INSERT,SELECT,UPDATE ON GHTDB.employeeterritories TO PUBLIC;
GRANT DELETE,INSERT,SELECT,UPDATE ON GHTDB.products TO PUBLIC;
GRANT DELETE,INSERT,SELECT,UPDATE ON GHTDB.products_above_average_price TO PUBLIC;
GRANT DELETE,INSERT,SELECT,UPDATE ON GHTDB.region TO PUBLIC;
--GRANT EXECUTE ON salesbycategory TO PUBLIC;
GRANT DELETE,INSERT,SELECT,UPDATE ON GHTDB.suppliers TO PUBLIC;
--GRANT EXECUTE ON ten_most_expensive_products TO PUBLIC;
GRANT DELETE,INSERT,SELECT,UPDATE ON GHTDB.territories TO PUBLIC;

---------------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------------
DROP TABLE GHTDB_EX.CUSTOMERS;
DROP TABLE GHTDB_EX.categories;

DROP USER GHTDB_EX CASCADE;

CREATE USER "GHTDB_EX"
    IDENTIFIED BY "GHTDB_EX" DEFAULT TABLESPACE "USERS"
    TEMPORARY TABLESPACE "TEMP" ;

GRANT CONNECT, RESOURCE TO "GHTDB_EX";
ALTER USER GHTDB_EX quota unlimited on USERS;

CREATE OR REPLACE PACKAGE GHTDB_EX.GHTPKG
AS
	TYPE RCT1 IS REF CURSOR;
	IDENTITY INTEGER;
END;
/

CREATE TABLE GHTDB_EX.CUSTOMERS (
  customerid   CHAR(10)      NOT NULL,
  companyname  VARCHAR2(80)  NULL,
  contactname  VARCHAR2(60)  NULL,
  contacttitle VARCHAR2(60)  NULL,
  address      VARCHAR2(120) NULL,
  city         VARCHAR2(30)  NULL,
  region       VARCHAR2(30)  NULL,
  postalcode   VARCHAR2(20)  NULL,
  country      VARCHAR2(30)  NULL,
  phone        VARCHAR2(48)  NULL,
  fax          VARCHAR2(48)  NULL,
  picture      VARCHAR2(48)  NULL
)
  TABLESPACE USERS
/

CREATE TABLE GHTDB_EX.categories (
  categoryid   VARCHAR2(20) NOT NULL,
  categoryname VARCHAR2(30) NOT NULL,
  description  CLOB         NULL
)
  TABLESPACE USERS
/


CREATE OR REPLACE  PROCEDURE "GHTDB_EX"."GH_DUMMY"  
    (CustomerIdPrm IN CHAR,
result     OUT GHTPKG.RCT1)
AS
BEGIN
OPEN result FOR
SELECT * FROM Customers where CustomerID=CustomerIdPrm;
RETURN;
END;
/

GRANT DELETE,INSERT,SELECT,UPDATE ON GHTDB_EX.CUSTOMERS TO PUBLIC;
GRANT DELETE,INSERT,SELECT,UPDATE ON GHTDB_EX.CATEGORIES TO PUBLIC;
GRANT EXECUTE ON GHTDB_EX.gh_dummy TO PUBLIC;



COMMIT;

exit
