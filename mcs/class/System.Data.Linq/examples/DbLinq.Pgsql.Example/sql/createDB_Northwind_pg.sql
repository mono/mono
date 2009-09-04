--####################################################################
--script to create PostgreSql version of the Northwind test DB
--####################################################################

DROP DATABASE IF EXISTS "Northwind";
CREATE DATABASE "Northwind" WITH OWNER = "LinqUser";

\connect "Northwind"; 


CREATE TABLE "Region" (
  "RegionID" SERIAL NOT NULL,
  "RegionDescription" VARCHAR(50) NOT NULL,
  PRIMARY KEY("RegionID")
);

CREATE TABLE "Territories" (
  "TerritoryID" VARCHAR(20) NOT NULL,
  "TerritoryDescription" VARCHAR(50) NOT NULL,
  "RegionID" INTEGER NOT NULL,
  PRIMARY KEY("TerritoryID"),
  CONSTRAINT "FK_Terr_Region" FOREIGN KEY ("RegionID") REFERENCES "Region"("RegionID")
);

--####################################################################
CREATE TABLE "Categories" (
  "CategoryID" SERIAL NOT NULL,
  "CategoryName" VARCHAR(15) NOT NULL,
  "Description" TEXT NULL,
  "Picture" BYTEA,
  PRIMARY KEY("CategoryID")
);

CREATE TABLE "Suppliers" (
  "SupplierID" SERIAL NOT NULL,
  "CompanyName" VARCHAR(40) NOT NULL DEFAULT '',
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
  "ProductID" SERIAL NOT NULL,
  "ProductName" VARCHAR(40) NOT NULL DEFAULT '',
  "SupplierID" INTEGER NULL,
  "CategoryID" INTEGER NULL,
  "QuantityPerUnit" VARCHAR(20) NULL,
  "UnitPrice" DECIMAL NULL,
  "UnitsInStock" SMALLINT NULL,
  "UnitsOnOrder" SMALLINT NULL,
  "ReorderLevel" SMALLINT NULL,
  "Discontinued" BOOL NOT NULL,
  PRIMARY KEY("ProductID"),
  CONSTRAINT "FK_prod_catg" FOREIGN KEY ("CategoryID") REFERENCES "Categories"("CategoryID"),
  CONSTRAINT "FK_prod_supp" FOREIGN KEY ("SupplierID") REFERENCES "Suppliers"("SupplierID")
  );
  
CREATE TABLE "Customers" (
  "CustomerID" VARCHAR(5) NOT NULL,
  "CompanyName" VARCHAR(40) NOT NULL,
  "ContactName" VARCHAR(30) NOT NULL ,
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
CREATE TABLE "Shippers" (
  "ShipperID" SERIAL NOT NULL,
  "CompanyName" VARCHAR(40) NOT NULL,
  "Phone" VARCHAR(24) NULL,
  PRIMARY KEY("ShipperID")
);

--####################################################################
CREATE TABLE "Employees" (
  "EmployeeID" SERIAL NOT NULL,
  "LastName" VARCHAR(20) NOT NULL,
  "FirstName" VARCHAR(10) NOT NULL,
  "Title" VARCHAR(30) NULL,
  "BirthDate" DATE NULL,
  "HireDate" TIMESTAMP NULL,
  "Address" VARCHAR(60) NULL,
  "City" VARCHAR(15) NULL,
  "Region" VARCHAR(15) NULL,
  "PostalCode" VARCHAR(10) NULL,
  "Country" VARCHAR(15) NULL,
  "HomePhone" VARCHAR(24) NULL,
  "Photo" BYTEA,
  "Notes" TEXT NULL,
  "TitleOfCourtesy" VARCHAR(25) NULL,
  "PhotoPath" VARCHAR (255) NULL,
  "Extension" VARCHAR(5) NULL,
  "ReportsTo" INTEGER NULL,
  CONSTRAINT "FK_Emp_ReportsToEmp" FOREIGN KEY ("ReportsTo") REFERENCES "Employees"("EmployeeID"),
  PRIMARY KEY("EmployeeID")
);

--####################################################################
CREATE TABLE  "EmployeeTerritories"  (
   "EmployeeID"  INTEGER NOT NULL          REFERENCES "Employees"("EmployeeID"),
   "TerritoryID"  VARCHAR(20) NOT NULL     REFERENCES "Territories"("TerritoryID"),
  PRIMARY KEY("EmployeeID","TerritoryID")
);


CREATE TABLE "Orders" (
  "OrderID" SERIAL NOT NULL,
  "CustomerID" VARCHAR(5) NOT NULL,
  "EmployeeID" INTEGER NULL,
  "OrderDate" TIMESTAMP NULL,
  "RequiredDate" TIMESTAMP NULL,
  "ShippedDate" TIMESTAMP NULL,
  "ShipVia" INT NULL,
  "Freight" DECIMAL NULL,
  "ShipName" VARCHAR(40) NULL,
  "ShipAddress" VARCHAR(60) NULL,
  "ShipCity" VARCHAR(15) NULL,
  "ShipRegion" VARCHAR(15) NULL,
  "ShipPostalCode" VARCHAR(10) NULL,
  "ShipCountry" VARCHAR(15) NULL,
  PRIMARY KEY("OrderID"),
  CONSTRAINT fk_order_customer FOREIGN KEY ("CustomerID") REFERENCES "Customers"("CustomerID"),
  CONSTRAINT fk_order_product FOREIGN KEY ("EmployeeID") REFERENCES "Employees"("EmployeeID")
);

CREATE TABLE "OrderDetails" (
  "OrderID" INT NOT NULL              REFERENCES "Orders"("OrderID"),
  "ProductID" INT NOT NULL            REFERENCES "Products"("ProductID"),
  "UnitPrice" decimal,
  "Quantity" INT,
  "Discount" float,
  PRIMARY KEY ("OrderID","ProductID")
);

--####################################################################
--## make sure permissions are set
--####################################################################
grant all on "Categories" to "LinqUser";
grant all on "Customers" to "LinqUser";
grant all on "Employees" to "LinqUser";
grant all on "EmployeeTerritories" to "LinqUser";
grant all on "OrderDetails" to "LinqUser";
grant all on "Orders" to "LinqUser";
grant all on "Products" to "LinqUser";
grant all on "Region" to "LinqUser";
grant all on "Shippers" to "LinqUser";
grant all on "Suppliers" to "LinqUser";
grant all on "Territories" to "LinqUser";
--sequences
grant all on "Products_ProductID_seq" to "LinqUser";
--####################################################################
--## populate tables with seed data
--####################################################################
truncate table "Categories" CASCADE;
Insert INTO "Categories" ("CategoryName","Description")
values ('Beverages',	'Soft drinks, coffees, teas, beers, and ales');
Insert INTO "Categories" ("CategoryName","Description")
values ('Condiments','Sweet and savory sauces, relishes, spreads, and seasonings');
Insert INTO "Categories" ("CategoryName","Description")
values ('Seafood','Seaweed and fish');

INSERT INTO "Region" ("RegionDescription") VALUES ('North America');
INSERT INTO "Region" ("RegionDescription") VALUES ('Europe');

INSERT INTO "Territories" ("TerritoryID","TerritoryDescription", "RegionID") VALUES ('US.Northwest', 'Northwest', 1);


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

insert INTO "Customers" ("CustomerID", "CompanyName","ContactName", "ContactTitle", "Country","PostalCode","Address","City", "Phone", "Fax")
values ('BONAP', 'Bon app''','Laurence Lebihan','Owner','France','13008','12, rue des Bouchers','Marseille','91.24.45.40', '91.24.45.41');

insert INTO "Customers" ("CustomerID", "CompanyName","ContactName", "ContactTitle", "Country","PostalCode","City", "Phone")
values ('WARTH', 'Wartian Herkku','Pirkko Koskitalo','Accounting Manager','Finland','90110','Oulu','981-443655');

--truncate table Orders; -- must be truncated before Products
truncate table "Products" CASCADE;

insert INTO "Suppliers" ("CompanyName", "ContactName", "ContactTitle", "Address", "City", "Region", "Country")
VALUES ('alles AG', 'Harald Reitmeyer', 'Prof', 'Fischergasse 8', 'Heidelberg', 'B-W', 'Germany');

insert INTO "Suppliers" ("CompanyName", "ContactName", "ContactTitle", "Address", "City", "Region", "Country")
VALUES ('Microsoft', 'Mr Allen', 'Monopolist', '1 MS', 'Redmond', 'WA', 'USA');

INSERT INTO "Suppliers" ("CompanyName", "ContactName", "ContactTitle", "Address", "City", "Region", "PostalCode", "Country", "Phone", "Fax")
VALUES ('Pavlova, Ltd.', 'Ian Devling', 'Marketing Manager', '74 Rose St. Moonie Ponds', 'Melbourne', 'Victoria', '3058', 'Australia', '(03) 444-2343', '(03) 444-6588');

--#################################################################################
insert INTO "Products" ("ProductName","SupplierID", "QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES ('Pen',1, 10,     12, 2,  '0');
insert INTO "Products" ("ProductName","SupplierID", "QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES ('Bicycle',1, 1,  6, 0,  '0');
insert INTO "Products" ("ProductName","QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES ('Phone',3,    7, 0,  '0');
insert INTO "Products" ("ProductName","QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES ('SAM',1,      51, 11, '0');
insert INTO "Products" ("ProductName","QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES ('iPod',0,     11, 0, '0');
insert INTO "Products" ("ProductName","QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES ('Toilet Paper',2,  0, 3, '1');
insert INTO "Products" ("ProductName","QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES ('Fork',5,   111, 0, '0');
insert INTO "Products" ("ProductName","SupplierID", "QuantityPerUnit","UnitsInStock","UnitsOnOrder","Discontinued")
VALUES ('Linq Book',2, 1, 0, 26, '0');

INSERT INTO "Products" ("ProductName","SupplierID", "QuantityPerUnit","UnitPrice",  "UnitsInStock","UnitsOnOrder","Discontinued")
VALUES ('Carnarvon Tigers', 3,'16 kg pkg.',62.50,  42, 0, false);


insert INTO  "Employees"  ("LastName","FirstName","Title","BirthDate","HireDate","Address","City","ReportsTo","Country","HomePhone")
VALUES ('Fuller','Andrew','Vice President, Sales','19540101','19890101', '908 W. Capital Way','Tacoma',NULL,'USA','(111)222333');

insert INTO  "Employees"  ("LastName","FirstName","Title","BirthDate","HireDate","Address","City","ReportsTo","Country","HomePhone")
VALUES ('Davolio','Nancy','Sales Representative','19640101','19940101','507 - 20th Ave. E.  Apt. 2A','Seattle',1,'USA','(444)555666');

insert INTO  "Employees"  ("LastName","FirstName","Title","BirthDate","HireDate","Address","City","ReportsTo","Country","HomePhone")
VALUES ('Builder','Bob','Handyman','19640101','19940101','666 dark street','Seattle',2,'USA','(777)888999');

insert into "EmployeeTerritories" ("EmployeeID","TerritoryID")
values (2,'US.Northwest');

--####################################################################
truncate table "Orders" CASCADE;
insert INTO "Orders" ("CustomerID", "EmployeeID", "OrderDate", "Freight")
Values ('AIRBU', 1, now(), 21.3);

insert INTO "Orders" ("CustomerID", "EmployeeID", "OrderDate", "Freight")
Values ('BT___', 1, now(), 11.1);

insert INTO "Orders" ("CustomerID", "EmployeeID", "OrderDate", "Freight")
Values ('BT___', 1, now(), 11.5);

insert INTO "Orders" ("CustomerID", "EmployeeID", "OrderDate", "Freight")
Values ('UKMOD', 1, now(), 32.5);

insert INTO "Orders" ("CustomerID", "EmployeeID", "OrderDate", "RequiredDate", "ShippedDate", "Freight", "ShipName", "ShipAddress",
"ShipCity", "ShipCountry")
Values ('BONAP', 1, '1996-10-16', '1996-11-27', '1996-10-21', 10.21, 'Bon app''', '12, rue des Bouchers', 'Marseille', 'France' );

INSERT INTO "OrderDetails" ("OrderID", "ProductID", "UnitPrice", "Quantity", "Discount")
VALUES (1,2, 33, 5, 11);

INSERT INTO "OrderDetails" ("OrderID", "ProductID", "UnitPrice", "Quantity",   "Discount")
VALUES (5,9, 50, 20,   0.05); --## CanarvonTigers

CREATE FUNCTION hello0() RETURNS text AS $$ 
  BEGIN RETURN 'hello0'; END;
$$ LANGUAGE plpgsql;

-- contatenates strings. test case: select hello2('aa','bb')
CREATE OR REPLACE FUNCTION hello1(name text) RETURNS text AS $$ 
  BEGIN RETURN 'hello,' || name || '!'; END;
$$ LANGUAGE plpgsql;
CREATE OR REPLACE FUNCTION hello2(name text, unused text) RETURNS text AS $$ 
  BEGIN RETURN 'hello,' || name || '!'; SELECT * FROM customer; END;
$$ LANGUAGE plpgsql;


-- count orders for given CustomerID. test case: select getOrderCount(1)
CREATE OR REPLACE FUNCTION "getOrderCount"("custId" VARCHAR) RETURNS INT AS $$
DECLARE
  count1 INTEGER;
BEGIN
  SELECT COUNT(*) INTO count1 FROM "Orders" WHERE "CustomerID"="custId";
  RETURN count1;
END;
$$ LANGUAGE plpgsql;

COMMIT;








