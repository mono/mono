--####################################################################
-- Script to create Oracle version of the Northwind test DB
--
-- this script was tested on Oracle XE - so it does not contain any 'CREATE DATABASE' statements.
--####################################################################
CREATE TABLE "Region" (
  "RegionID" INTEGER NOT NULL,
  "RegionDescription" VARCHAR(50) NOT NULL,
  PRIMARY KEY("RegionID")
);

CREATE TABLE "Territories" (
  "TerritoryID" VARCHAR(20) NOT NULL,
  "TerritoryDescription" VARCHAR(50) NOT NULL,
  "RegionID" INTEGER NOT NULL,
  PRIMARY KEY("TerritoryID"),
  FOREIGN KEY ("RegionID") REFERENCES "Region" ("RegionID")
);

--####################################################################
CREATE TABLE "Categories" (
  "CategoryID" INTEGER  NOT NULL,
  "CategoryName" VARCHAR(15) NOT NULL,
  "Description" VARCHAR(500) NULL,
  "Picture" BLOB NULL,
  PRIMARY KEY("CategoryID")
);

CREATE TABLE "Suppliers" (
  "SupplierID" INTEGER  NOT NULL,
  "CompanyName" VARCHAR(40) NOT NULL,
  "ContactName" VARCHAR(30) NULL,
  "ContactTitle" VARCHAR(30) NULL,
  "Address" VARCHAR(60) NULL,
  "City" VARCHAR(15) NULL,
  "Region" VARCHAR(15) NULL,
  "PostalCode" VARCHAR(10) NULL,
  "Country" VARCHAR(15) NULL,
  "Phone" VARCHAR(24) NULL,
  "Fax" VARCHAR(24) NULL,
  PRIMARY KEY("SupplierID")
);
--####################################################################

CREATE TABLE "Products" (
  "ProductID" INTEGER NOT NULL,
  "ProductName" VARCHAR(40) NOT NULL,
  "SupplierID" INTEGER NULL,
  "CategoryID" INTEGER NULL,
  "QuantityPerUnit" VARCHAR(20) NULL,
  "UnitPrice" DECIMAL NULL,
  "UnitsInStock" SMALLINT NULL,
  "UnitsOnOrder" SMALLINT NULL,
  "ReorderLevel" SMALLINT NULL,
  "Discontinued" NUMBER(1) NOT NULL, --'bool' field
  PRIMARY KEY("ProductID"),
  FOREIGN KEY ("CategoryID") REFERENCES "Categories" ("CategoryID"),
  FOREIGN KEY ("SupplierID") REFERENCES "Suppliers" ("SupplierID")
);
  
CREATE TABLE "Customers" (
  "CustomerID" VARCHAR(5) NOT NULL,
  "CompanyName" VARCHAR(40) NOT NULL,
  "ContactName" VARCHAR(30) NULL,
  "ContactTitle" VARCHAR(30) NULL,
  "Address" VARCHAR(60) NULL,
  "City" VARCHAR(15) NULL,
  "Region" VARCHAR(15) NULL,
  "PostalCode" VARCHAR(10) NULL,
  "Country" VARCHAR(15) NULL,
  "Phone" VARCHAR(24) NULL,
  "Fax" VARCHAR(24) NULL,
  PRIMARY KEY("CustomerID")
);

--####################################################################
CREATE TABLE "Employees" (
  "EmployeeID" INTEGER NOT NULL,
  "LastName" VARCHAR(20) NOT NULL,
  "FirstName" VARCHAR(10) NOT NULL,
  "Title" VARCHAR(30) NULL,
  "BirthDate" DATE NULL,
  "HireDate" DATE NULL,
  "Address" VARCHAR(60) NULL,
  "City" VARCHAR(15) NULL,
  "Region" VARCHAR(15) NULL,
  "PostalCode" VARCHAR(10) NULL,
  "Country" VARCHAR(15) NULL,
  "HomePhone" VARCHAR(24) NULL,
  "Photo" BLOB NULL,
  "Notes" VARCHAR(100) NULL,
  "TitleOfCourtesy" VARCHAR(25) NULL,
  "PhotoPath" VARCHAR (255) NULL,
  "Extension" VARCHAR(5) NULL,
  "ReportsTo" INTEGER NULL,
  PRIMARY KEY("EmployeeID"),
  FOREIGN KEY ("ReportsTo")  REFERENCES "Employees" ("EmployeeID")
);

CREATE TABLE "EmployeeTerritories" (
  "EmployeeID" INTEGER NOT NULL,
  "TerritoryID" VARCHAR(20) NOT NULL,
  PRIMARY KEY("EmployeeID","TerritoryID"),
  FOREIGN KEY ("EmployeeID") REFERENCES "Employees" ("EmployeeID"),
  FOREIGN KEY ("TerritoryID") REFERENCES "Territories" ("TerritoryID")
);

--####################################################################
CREATE TABLE "Orders" (
  "OrderID" INTEGER NOT NULL,
  "CustomerID" VARCHAR(5) NULL,
  "EmployeeID" INTEGER NULL,
  "OrderDate" DATE NULL,
  "RequiredDate" DATE NULL,
  "ShippedDate" DATE NULL,
  "ShipVia" INT NULL,
  "Freight" DECIMAL NULL,
  "ShipName" VARCHAR(40) NULL,
  "ShipAddress" VARCHAR(60) NULL,
  "ShipCity" VARCHAR(15) NULL,
  "ShipRegion" VARCHAR(15) NULL,
  "ShipPostalCode" VARCHAR(10) NULL,
  "ShipCountry" VARCHAR(15) NULL,
  PRIMARY KEY("OrderID"),
  FOREIGN KEY ("CustomerID") REFERENCES "Customers" ("CustomerID"),
  FOREIGN KEY ("EmployeeID") REFERENCES "Employees" ("EmployeeID")
);

--####################################################################
CREATE TABLE "OrderDetails" (
  "OrderID" INTEGER NOT NULL,
  "ProductID" INTEGER NOT NULL,
  "UnitPrice" DECIMAL NOT NULL,
  "Quantity" SMALLINT NOT NULL,
  "Discount" FLOAT NOT NULL,
  PRIMARY KEY("OrderID","ProductID"),
  FOREIGN KEY ("OrderID") REFERENCES "Orders" ("OrderID"),
  FOREIGN KEY ("ProductID") REFERENCES "Products" ("ProductID")
);

--####################################################################
CREATE SEQUENCE Region_seq      START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Categories_seq  START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Suppliers_seq   START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Products_seq    START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Orders_seq      START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Employees_seq   START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Territories_seq START WITH 1    INCREMENT BY 1;
 
--####################################################################

CREATE OR REPLACE TRIGGER Employees_Trigger
BEFORE INSERT ON "Employees" 
FOR EACH ROW
BEGIN
   IF (:new."EmployeeID" IS NULL) THEN
        SELECT Employees_seq.NEXTVAL INTO :new."EmployeeID" FROM DUAL;
   END IF;
END;
/

CREATE OR REPLACE TRIGGER Products_Trigger
BEFORE INSERT ON "Products" 
FOR EACH ROW
BEGIN
   IF (:new."ProductID" IS NULL) THEN
        SELECT Products_seq.NEXTVAL INTO :new."ProductID" FROM DUAL;
   END IF;
END;
/

CREATE OR REPLACE TRIGGER Categories_Trigger
BEFORE INSERT ON "Categories" 
FOR EACH ROW
BEGIN
   IF (:new."CategoryID" IS NULL) THEN
        SELECT Categories_seq.NEXTVAL INTO :new."CategoryID" FROM DUAL;
   END IF;
END;
/

CREATE OR REPLACE TRIGGER Region_Trigger
BEFORE INSERT ON "Region" 
FOR EACH ROW
BEGIN
   IF (:new."RegionID" IS NULL) THEN
        SELECT Region_seq.NEXTVAL INTO :new."RegionID" FROM DUAL;
   END IF;
END;
/

CREATE OR REPLACE TRIGGER Suppliers_Trigger
BEFORE INSERT ON "Suppliers" 
FOR EACH ROW
BEGIN
   IF (:new."SupplierID" IS NULL) THEN
        SELECT Suppliers_seq.NEXTVAL INTO :new."SupplierID" FROM DUAL;
   END IF;
END;
/

CREATE OR REPLACE TRIGGER Orders_Trigger
BEFORE INSERT ON "Orders" 
FOR EACH ROW
BEGIN
   IF (:new."OrderID" IS NULL) THEN
        SELECT Orders_seq.NEXTVAL INTO :new."OrderID" FROM DUAL;
   END IF;
END;
/

CREATE OR REPLACE TRIGGER Territories_Trigger
BEFORE INSERT ON "Territories" 
FOR EACH ROW
BEGIN
   IF (:new."TerritoryID" IS NULL) THEN
        SELECT Territories_seq.NEXTVAL INTO :new."TerritoryID" FROM DUAL;
   END IF;
END;
/

--####################################################################
Insert INTO "Categories" ("CategoryID", "CategoryName","Description")
values (Categories_seq.NextVal, 'Beverages',	'Soft drinks, coffees, teas, beers, and ales');
Insert INTO "Categories" ("CategoryID", "CategoryName","Description")
values (Categories_seq.NextVal, 'Condiments','Sweet and savory sauces, relishes, spreads, and seasonings');
Insert INTO "Categories" ("CategoryID", "CategoryName","Description")
values (Categories_seq.NextVal, 'Seafood','Seaweed and fish');

--####################################################################
INSERT INTO "Region" ("RegionDescription") VALUES ('North America');
INSERT INTO "Region" ("RegionDescription") VALUES ('Europe');

--####################################################################
INSERT INTO "Territories" ("TerritoryID", "TerritoryDescription", "RegionID") VALUES ('US.Northwest', 'Northwest', 1);

--####################################################################
insert INTO "Customers" ("CustomerID", "CompanyName","ContactName","Country","PostalCode","City")
values ('AIRBU', 'airbus','jacques','France','10000','Paris');
insert INTO "Customers" ("CustomerID", "CompanyName","ContactName","Country","PostalCode","City")
values ('BT___','BT','graeme','U.K.','E14','London');

insert INTO "Customers" ("CustomerID", "CompanyName","ContactName","Country","PostalCode","City")
values ('ATT__','ATT','bob','USA','10021','New York');
insert INTO "Customers" ("CustomerID", "CompanyName","ContactName","Country","PostalCode","City")
values ('UKMOD', 'MOD','(secret)','U.K.','E14','London');

insert INTO "Customers" ("CustomerID", "CompanyName","ContactName", "ContactTitle", "Country","PostalCode","City", "Phone")
values ('ALFKI', 'Alfreds Futterkiste','Maria Anders','Sales Representative','Germany','12209','Berlin','030-0074321');

insert INTO "Customers" ("CustomerID", "CompanyName","ContactName", "ContactTitle", "Country", "PostalCode", "Address", "City", "Phone", "Fax")
values ('BONAP', 'Bon app''','Laurence Lebihan','Owner','France','13008','12, rue des Bouchers','Marseille','91.24.45.40', '91.24.45.41');

insert INTO "Customers" ("CustomerID", "CompanyName","ContactName", "ContactTitle", "Country","PostalCode","City", "Phone")
values ('WARTH', 'Wartian Herkku','Pirkko Koskitalo','Accounting Manager','Finland','90110','Oulu','981-443655');

--####################################################################
insert INTO "Suppliers" ("SupplierID", "CompanyName", "ContactName", "ContactTitle", "Address", "City", "Region", "Country")
VALUES (Suppliers_seq.Nextval, 'alles AG', 'Harald Reitmeyer', 'Prof', 'Fischergasse 8', 'Heidelberg', 'B-W', 'Germany');

insert INTO "Suppliers" ("SupplierID", "CompanyName", "ContactName", "ContactTitle", "Address", "City", "Region", "Country")
VALUES (Suppliers_seq.Nextval, 'Microsoft', 'Mr Allen', 'Monopolist', '1 MS', 'Redmond', 'WA', 'USA');

INSERT INTO "Suppliers" ("SupplierID", "CompanyName", "ContactName", "ContactTitle", "Address", "City", "Region", "PostalCode", "Country", "Phone", "Fax")
VALUES (Suppliers_seq.Nextval, 'Pavlova, Ltd.', 'Ian Devling', 'Marketing Manager', '74 Rose St. Moonie Ponds', 'Melbourne', 'Victoria', '3058', 'Australia', '(03) 444-2343', '(03) 444-6588');


insert INTO "Products" ("ProductID", "ProductName","SupplierID", "QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES (Products_seq.nextval, 'Pen',1, 10,     12, 2,  0);
insert INTO "Products" ("ProductID", "ProductName","SupplierID", "QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES (Products_seq.nextval, 'Bicycle',1, 1,  6, 0,  0);
insert INTO "Products" ("ProductID", "ProductName","QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES (Products_seq.nextval, 'Phone',3,    7, 0,  0);
insert INTO "Products" ("ProductID", "ProductName","QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES (Products_seq.nextval, 'SAM',1,      51, 11, 0);
insert INTO "Products" ("ProductID", "ProductName","QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES (Products_seq.nextval, 'iPod',0,     11, 0, 0);
insert INTO "Products" ("ProductID", "ProductName","QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES (Products_seq.nextval, 'Toilet Paper',2,  0, 3, 1);
insert INTO "Products" ("ProductID", "ProductName","QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES (Products_seq.nextval, 'Fork',5,   111, 0, 0);
insert INTO "Products" ("ProductID", "ProductName","SupplierID", "QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES (Products_seq.nextval, 'Linq Book',2, 1, 0, 26, 0);
INSERT INTO "Products" ("ProductID", "ProductName","SupplierID", "QuantityPerUnit","UnitPrice",  "UnitsInStock","UnitsOnOrder","Discontinued")
VALUES (Products_seq.nextval, 'Carnarvon Tigers', 3,'16 kg pkg.',62.50,  42, 0, 0);

--####################################################################
insert INTO "Employees" ("EmployeeID", "LastName","FirstName","Title","BirthDate","HireDate","Address","City","ReportsTo","Country","HomePhone")
VALUES (Employees_seq.nextval, 'Fuller','Andrew','Vice President, Sales',to_date('01-01-1964','dd-mm-yyyy'),to_date('01-01-1989','dd-mm-yyyy'), '908 W. Capital Way','Tacoma',NULL,'USA','(111)222333');

insert INTO "Employees" ("EmployeeID", "LastName","FirstName","Title","BirthDate","HireDate","Address","City","ReportsTo","Country","HomePhone")
VALUES (Employees_seq.nextval, 'Davolio','Nancy','Sales Representative',to_date('01-01-1964','dd-mm-yyyy'),to_date('01-01-1994','dd-mm-yyyy'),'507 - 20th Ave. E.  Apt. 2A','Seattle',1,'USA','(444)555666');

insert INTO "Employees" ("EmployeeID", "LastName","FirstName","Title","BirthDate","HireDate","Address","City","ReportsTo","Country","HomePhone")
VALUES (Employees_seq.nextval, 'Builder','Bob','Handyman',to_date('01-01-1964','dd-mm-yyyy'),to_date('01-01-1964','dd-mm-yyyy'),'666 dark street','Seattle',2,'USA','(777)888999');

--####################################################################
INSERT INTO "EmployeeTerritories" ("EmployeeID", "TerritoryID") VALUES (2, 'US.Northwest');


--####################################################################
--truncate table Orders;
--
insert INTO "Orders" ("OrderID", "CustomerID", "EmployeeID", "OrderDate", "Freight")
Values (Orders_seq.NextVal, 'AIRBU', 1, sysdate, 21.3);

insert INTO "Orders" ("OrderID", "CustomerID", "EmployeeID", "OrderDate", "Freight")
Values (Orders_seq.NextVal, 'BT___', 1, sysdate, 11.1);

insert INTO "Orders" ("OrderID", "CustomerID", "EmployeeID", "OrderDate", "Freight")
Values (Orders_seq.NextVal, 'BT___', 1, sysdate, 11.5);

insert INTO "Orders" ("OrderID", "CustomerID", "EmployeeID", "OrderDate", "Freight")
Values (Orders_seq.NextVal, 'UKMOD', 1, sysdate, 32.5);

insert INTO "Orders" ("OrderID", "CustomerID", "EmployeeID", "OrderDate", "RequiredDate", "ShippedDate", "Freight", "ShipName", "ShipAddress", "ShipCity", "ShipCountry")
Values (Orders_seq.NextVal, 'BONAP', 1, to_date('1996-10-16', 'yyyy-mm-dd'), to_date('1996-11-27', 'yyyy-mm-dd'), to_date('1996-10-21', 'yyyy-mm-dd'), 10.21, 'Bon app''', '12, rue des Bouchers', 'Marseille', 'France' );

INSERT INTO "OrderDetails" ("OrderID", "ProductID", "UnitPrice", "Quantity", "Discount")
VALUES (1,2, 33, 5, 11);

INSERT INTO "OrderDetails" ("OrderID", "ProductID", "UnitPrice", "Quantity",   "Discount")
VALUES (5,9, 50, 20,   0.05); --## CanarvonTigers for customer BONAP


--####################################################################

CREATE OR REPLACE FUNCTION NORTHWIND.HELLO0
RETURN varchar
  IS
BEGIN
  return 'hello0';
END;
/

CREATE OR REPLACE FUNCTION NORTHWIND.HELLO1
(s varchar)
RETURN varchar
  IS
BEGIN
  return 'Hello, ' || s || '!';
END;
/

CREATE OR REPLACE FUNCTION NORTHWIND.HELLO2
(s varchar, s2 number)
RETURN varchar
  IS
BEGIN
  return 'Hello, ' || s || '!';
END;
/

CREATE OR REPLACE FUNCTION NORTHWIND.GETORDERCOUNT
(custId varchar)
RETURN number
  IS
count1 number;
BEGIN
SELECT COUNT(*) INTO count1 FROM "Orders" WHERE "CustomerID"=custId;
RETURN count1;
END;
/

CREATE OR REPLACE PROCEDURE NORTHWIND.SP_SELORDERS
(s varchar, s2 out number)
  IS    
BEGIN
select 22 into s2 from dual;
END;
/

COMMIT;

EXIT

