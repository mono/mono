/*####################################################################
  ## script to create FirbirdSql version of the Northwind test DB
  ####################################################################*/

/*DROP USER IF EXISTS 'LinqUser'@'%'; */
/*DELETE FROM mysql.user WHERE User='LinqUser';*/
/*DROP USER 'LinqUser'@'%';*/


/*## create user LinqUser, password: 'linq2'
CREATE USER 'LinqUser'@'%'
  IDENTIFIED BY PASSWORD '*247E8BFCE2F07F00D7FD773390A282540001077B';*/

/*## give our new user full permissions on new database:
GRANT ALL ON Northwind.*  TO 'LinqUser'@'%';
FLUSH PRIVILEGES;*/

DROP TRIGGER Trg_Orders_Insert;
DROP TRIGGER Trg_Employees_Insert;
DROP TRIGGER Trg_Shippers_Insert;
DROP TRIGGER Trg_Products_Insert;
DROP TRIGGER Trg_Suppliers_Insert;
DROP TRIGGER Trg_Categories_Insert;
DROP TRIGGER Trg_Region_Insert;

DROP GENERATOR Gen_Orders_OrderID;
DROP GENERATOR Gen_Employees_EmployeeID;
DROP GENERATOR Gen_Shippers_ShipperID;
DROP GENERATOR Gen_Products_ProductID;
DROP GENERATOR Gen_Suppliers_SupplierID;
DROP GENERATOR Gen_Categories_CategoryID;
DROP GENERATOR Gen_Region_RegionID;

DROP TABLE "Order Details";
DROP TABLE Orders;
DROP TABLE EmployeeTerritories;
DROP TABLE Employees;
DROP TABLE Shippers;
DROP TABLE Customers;
DROP TABLE Products;
DROP TABLE Suppliers;
DROP TABLE Categories;
DROP TABLE Territories;
DROP TABLE Region;

/*####################################################################
  ## create tables
  ####################################################################*/

CREATE TABLE Region (
  RegionID INTEGER NOT NULL,
  RegionDescription VARCHAR(50) NOT NULL,
  PRIMARY KEY(RegionID)
)
;

CREATE TABLE Territories (
  TerritoryID VARCHAR(20) NOT NULL,
  TerritoryDescription VARCHAR(50) NOT NULL,
  RegionID INTEGER NOT NULL,
  PRIMARY KEY(TerritoryID)
)
;

alter table Territories
   add constraint FK_Terr_Region foreign key (RegionID)
      references Region (RegionID);

/*####################################################################*/
CREATE TABLE Categories (
  CategoryID INTEGER  NOT NULL,
  CategoryName VARCHAR(15) NOT NULL,
  Description BLOB SUB_TYPE 1,
  Picture BLOB,
  PRIMARY KEY(CategoryID)
)
;

CREATE TABLE Suppliers (
  SupplierID INTEGER  NOT NULL,
  CompanyName VARCHAR(40) DEFAULT '' NOT NULL,
  ContactName VARCHAR(30),
  ContactTitle VARCHAR(30),
  Address VARCHAR(60),
  City VARCHAR(15),
  Region VARCHAR(15),
  PostalCode VARCHAR(10),
  Country VARCHAR(15),
  Phone VARCHAR(24),
  Fax VARCHAR(24),
  PRIMARY KEY(SupplierID)
)
;

/*####################################################################*/
CREATE TABLE Products (
  ProductID INTEGER NOT NULL,
  ProductName VARCHAR(40) DEFAULT '' NOT NULL,
  SupplierID INTEGER,
  CategoryID INTEGER,
  QuantityPerUnit VARCHAR(20),
  UnitPrice DECIMAL,
  UnitsInStock SMALLINT,
  UnitsOnOrder SMALLINT,
  ReorderLevel SMALLINT,
  Discontinued SMALLINT NOT NULL,
  PRIMARY KEY(ProductID)
);

alter table Products
   add constraint FK_prod_catg foreign key (CategoryID)
      references Categories (CategoryID);
alter table Products
   add constraint FK_prod_supp foreign key (SupplierID)
      references Suppliers (SupplierID);

/*####################################################################*/
CREATE TABLE Customers (
  CustomerID VARCHAR(5) NOT NULL,
  CompanyName VARCHAR(40) DEFAULT '' NOT NULL,
  ContactName VARCHAR(30),
  ContactTitle VARCHAR(30),
  Address VARCHAR(60),
  City VARCHAR(15),
  Region VARCHAR(15),
  PostalCode VARCHAR(10),
  Country VARCHAR(15),
  Phone VARCHAR(24),
  Fax VARCHAR(24),
  PRIMARY KEY(CustomerID)
)
;

/*####################################################################*/
CREATE TABLE Shippers (
  ShipperID INTEGER NOT NULL,
  CompanyName VARCHAR(40) NOT NULL,
  Phone VARCHAR(24),
  PRIMARY KEY(ShipperID)
)
;

/*####################################################################*/
CREATE TABLE Employees (
  EmployeeID INTEGER NOT NULL,
  LastName VARCHAR(20) NOT NULL,
  FirstName VARCHAR(10) NOT NULL,
  Title VARCHAR(30),
  BirthDate TIMESTAMP,
  HireDate TIMESTAMP,
  Address VARCHAR(60),
  City VARCHAR(15),
  Region VARCHAR(15),
  PostalCode VARCHAR(10),
  Country VARCHAR(15),
  HomePhone VARCHAR(24),
  Photo BLOB,
  Notes BLOB SUB_TYPE 1,
  TitleOfCourtesy VARCHAR(25),
  PhotoPath VARCHAR (255),
  Extension VARCHAR(5),
  ReportsTo INTEGER,
  PRIMARY KEY(EmployeeID)
)
;

alter table Employees
   add constraint FK_Emp_ReportsToEmp foreign key (ReportsTo)
      references Employees (EmployeeID);

/*####################################################################*/
CREATE TABLE EmployeeTerritories (
  EmployeeID INTEGER NOT NULL,
  TerritoryID VARCHAR(20) NOT NULL,
  PRIMARY KEY(EmployeeID,TerritoryID)
)
;

alter table EmployeeTerritories
   add constraint FK_empTerr_emp foreign key (EmployeeID)
      references Employees (EmployeeID);
alter table EmployeeTerritories
   add constraint FK_empTerr_terr foreign key (TerritoryID)
      references Territories (TerritoryID);


/*####################################################################*/
CREATE TABLE Orders (
  OrderID INTEGER NOT NULL,
  CustomerID VARCHAR(5),
  EmployeeID INTEGER,
  OrderDate TIMESTAMP,
  RequiredDate TIMESTAMP,
  ShippedDate TIMESTAMP,
  ShipVia INTEGER,
  Freight DECIMAL,
  ShipName VARCHAR(40),
  ShipAddress VARCHAR(60),
  ShipCity VARCHAR(15),
  ShipRegion VARCHAR(15),
  ShipPostalCode VARCHAR(10),
  ShipCountry VARCHAR(15),
  PRIMARY KEY(OrderID)
)
;

alter table Orders
   add constraint FK_orders_cust foreign key (CustomerID)
      references Customers (CustomerID);
alter table Orders
   add constraint FK_orders_emp foreign key (EmployeeID)
      references Employees (EmployeeID);
alter table Orders
   add constraint FK_orders_ship foreign key (ShipVia)
      references Shippers (ShipperID);

/*####################################################################*/
CREATE TABLE "Order Details" (
  OrderID INTEGER NOT NULL,
  ProductID INTEGER NOT NULL,
  UnitPrice DECIMAL NOT NULL,
  Quantity SMALLINT NOT NULL,
  Discount FLOAT NOT NULL,
  PRIMARY KEY(OrderID,ProductID)
)
;

alter table "Order Details"
   add constraint FK_orderDet_ord foreign key (OrderID)
      references Orders (OrderID);
alter table "Order Details"
   add constraint FK_orderDet_prod foreign key (ProductID)
      references Products (ProductID);

CREATE GENERATOR Gen_Region_RegionID;

SET TERM ^ ;
CREATE TRIGGER Trg_Region_Insert for Region
    ACTIVE BEFORE INSERT POSITION 0
AS
BEGIN
    NEW.RegionID = GEN_ID(GEN_Region_RegionID, 1);
END^
SET TERM ; ^


CREATE GENERATOR Gen_Categories_CategoryID;

SET TERM ^ ;
CREATE TRIGGER Trg_Categories_Insert for Categories
    ACTIVE BEFORE INSERT POSITION 0
AS
BEGIN
    NEW.CategoryID = GEN_ID(Gen_Categories_CategoryID, 1);
END^
SET TERM ; ^


CREATE GENERATOR Gen_Suppliers_SupplierID;

SET TERM ^ ;
CREATE TRIGGER Trg_Suppliers_Insert for Suppliers
    ACTIVE BEFORE INSERT POSITION 0
AS
BEGIN
    NEW.SupplierID = GEN_ID(Gen_Suppliers_SupplierID, 1);
END^
SET TERM ; ^


CREATE GENERATOR Gen_Products_ProductID;

SET TERM ^ ;
CREATE TRIGGER Trg_Products_Insert for Products
    ACTIVE BEFORE INSERT POSITION 0
AS
BEGIN
    NEW.ProductID = GEN_ID(Gen_Products_ProductID, 1);
END^
SET TERM ; ^


CREATE GENERATOR Gen_Shippers_ShipperID;

SET TERM ^ ;
CREATE TRIGGER Trg_Shippers_Insert for Shippers
    ACTIVE BEFORE INSERT POSITION 0
AS
BEGIN
    NEW.ShipperID = GEN_ID(Gen_Shippers_ShipperID, 1);
END^
SET TERM ; ^


CREATE GENERATOR Gen_Employees_EmployeeID;

SET TERM ^ ;
CREATE TRIGGER Trg_Employees_Insert for Employees
    ACTIVE BEFORE INSERT POSITION 0
AS
BEGIN
    NEW.EmployeeID = GEN_ID(Gen_Employees_EmployeeID, 1);
END^
SET TERM ; ^


CREATE GENERATOR Gen_Orders_OrderID;

SET TERM ^ ;
CREATE TRIGGER Trg_Orders_Insert for Orders
    ACTIVE BEFORE INSERT POSITION 0
AS
BEGIN
    NEW.OrderID = GEN_ID(Gen_Orders_OrderID, 1);
END^
SET TERM ; ^











/*####################################################################
  ## populate tables with seed data
  ####################################################################*/
set generator Gen_Region_RegionID to 0;
set generator Gen_Categories_CategoryID to 0;
set generator Gen_Suppliers_SupplierID to 0;
set generator Gen_Products_ProductID to 0;
set generator Gen_Shippers_ShipperID to 0;
set generator Gen_Employees_EmployeeID to 0;
set generator Gen_Orders_OrderID to 0;

DELETE FROM "Order Details";
DELETE FROM Orders;
DELETE FROM EmployeeTerritories;
DELETE FROM Employees;
DELETE FROM Shippers;
DELETE FROM Customers;
DELETE FROM Products;
DELETE FROM Suppliers;
DELETE FROM Categories;
DELETE FROM Territories;
DELETE FROM Region;

Insert INTO Categories (CategoryName,Description)
values ('Beverages',    'Soft drinks, coffees, teas, beers, and ales');

Insert INTO Categories (CategoryName,Description)
values ('Condiments','Sweet and savory sauces, relishes, spreads, and seasonings');

Insert INTO Categories (CategoryName,Description)
values ('Seafood','Seaweed and fish');


INSERT INTO Region (RegionDescription) VALUES ('North America');
INSERT INTO Region (RegionDescription) VALUES ('Europe');

INSERT INTO Territories (TerritoryID,TerritoryDescription, RegionID) VALUES ('US.Northwest', 'Northwest', 1);

insert INTO Customers (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('AIRBU', 'airbus','jacques','France','10000','Paris');
insert INTO Customers (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('BT___','BT','graeme','U.K.','E14','London');

insert INTO Customers (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('ATT__','ATT','bob','USA','10021','New York');
insert INTO Customers (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('UKMOD', 'MOD','(secret)','U.K.','E14','London');

insert INTO Customers (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,City, Phone)
values ('ALFKI', 'Alfreds Futterkiste','Maria Anders','Sales Representative','Germany','12209','Berlin','030-0074321');

insert INTO Customers (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,Address,City, Phone, Fax)
values ('BONAP', 'Bon app''','Laurence Lebihan','Owner','France','13008','12, rue des Bouchers','Marseille','91.24.45.40', '91.24.45.41');

insert INTO Customers (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,City, Phone)
values ('WARTH', 'Wartian Herkku','Pirkko Koskitalo','Accounting Manager','Finland','90110','Oulu','981-443655');

delete from Suppliers;

insert INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region, Country)
VALUES ('alles AG', 'Harald Reitmeyer', 'Prof', 'Fischergasse 8', 'Heidelberg', 'B-W', 'Germany');

insert INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region, Country)
VALUES ('Microsoft', 'Mr Allen', 'Monopolist', '1 MS', 'Redmond', 'WA', 'USA');

INSERT INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region, PostalCode, Country, Phone, Fax)
VALUES ('Pavlova, Ltd.', 'Ian Devling', 'Marketing Manager', '74 Rose St. Moonie Ponds', 'Melbourne', 'Victoria', '3058', 'Australia', '(03) 444-2343', '(03) 444-6588');


insert INTO Products (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Pen',1, 10,     12, 2,  0);
insert INTO Products (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Bicycle',1, 1,  6, 0,  0);
insert INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Phone',3,    7, 0,  0);
insert INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('SAM',1,      51, 11, 0);
insert INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('iPod',0,     11, 0, 0);
insert INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Toilet Paper',2,  0, 3, 1);
insert INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Fork',5,   111, 0, 0);
insert INTO Products (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Linq Book',2, 1, 0, 26, 0);
INSERT INTO Products (ProductName,SupplierID, QuantityPerUnit,UnitPrice,  UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Carnarvon Tigers', 3,'16 kg pkg.',62.50,  42, 0, 0);

delete from Employees;

insert INTO Employees (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo,Country,HomePhone)
VALUES ('Fuller','Andrew','Vice President, Sales','1954-01-01','1989-01-01', '908 W. Capital Way','Tacoma',NULL,'USA','(111)222333');

insert INTO Employees (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo,Country,HomePhone)
VALUES ('Davolio','Nancy','Sales Representative','1964-01-01','1994-01-01','507 - 20th Ave. E.  Apt. 2A','Seattle',1,'USA','(444)555666');

insert INTO Employees (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo,Country,HomePhone)
VALUES ('Builder','Bob','Handyman','1964-01-01','1994-01-01','666 dark street','Seattle',2,'USA','(777)888999');

insert into employeeTerritories (EmployeeID,TerritoryID)
values (2,'US.Northwest');

/*####################################################################*/
delete from Orders;
insert INTO Orders (CustomerID, EmployeeID, OrderDate, Freight)
Values ('AIRBU', 1, CURRENT_TIMESTAMP, 21.3);

insert INTO Orders (CustomerID, EmployeeID, OrderDate, Freight)
Values ('BT___', 1, CURRENT_TIMESTAMP, 11.1);

insert INTO Orders (CustomerID, EmployeeID, OrderDate, Freight)
Values ('BT___', 1, CURRENT_TIMESTAMP, 11.5);

insert INTO Orders (CustomerID, EmployeeID, OrderDate, Freight)
Values ('UKMOD', 1, CURRENT_TIMESTAMP, 32.5);

insert INTO Orders (CustomerID, EmployeeID, OrderDate, RequiredDate, ShippedDate, Freight, ShipName, ShipAddress, ShipCity, ShipCountry)
Values ('BONAP', 1, '1996-10-16', '1996-11-27', '1996-10-21', 10.21, 'Bon app''', '12, rue des Bouchers', 'Marseille', 'France' );

INSERT INTO "Order Details" (OrderID, ProductID, UnitPrice, Quantity, Discount)
VALUES (1,2, 33, 5, 11);

INSERT INTO "Order Details" (OrderID, ProductID, UnitPrice, Quantity,   Discount)
VALUES (5,9, 50, 20,   0.05); /* CanarvonTigers */

/*####################################################################
## create stored procs
####################################################################*/
/* we also need some functions to test the -sprocs option **/
/*CREATE FUNCTION hello0() RETURNS char(20) RETURN 'hello0';
CREATE FUNCTION hello1(s CHAR(20)) RETURNS char(30) RETURN CONCAT('Hello, ',s,'!');
CREATE FUNCTION hello2(s CHAR(20),s2 int) RETURNS char(30) RETURN CONCAT('Hello, ',s,'!');

DELIMITER $$

DROP FUNCTION IF EXISTS getOrderCount $$
CREATE FUNCTION getOrderCount(custId VARCHAR(5)) RETURNS INT
BEGIN
DECLARE count1 int;
SELECT COUNT(*) INTO count1 FROM Orders WHERE CustomerID=custId;
RETURN count1;
END $$

DROP PROCEDURE IF EXISTS sp_selOrders $$
CREATE PROCEDURE sp_selOrders(s CHAR(20), OUT s2 int)
BEGIN
set s2 = 22;
select * from orders;
END $$

DROP PROCEDURE IF EXISTS sp_updOrders $$
CREATE PROCEDURE sp_updOrders(custID int, prodId int)
BEGIN
UPDATE Orders
SET OrderDate=CURRENT_TIMESTAMP
WHERE ProductId=prodId AND CustomerID=custId;
END $$

DELIMITER ;*/
