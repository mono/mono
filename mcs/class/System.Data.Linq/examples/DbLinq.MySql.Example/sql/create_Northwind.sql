####################################################################
## script to create MySql version of the Northwind test DB
####################################################################

DROP DATABASE IF EXISTS `Northwind`;

CREATE DATABASE `Northwind`;

USE `Northwind`;

/*DROP USER IF EXISTS 'LinqUser'@'%'; */
/*DELETE FROM `mysql`.`user` WHERE `User`='LinqUser';*/
/*DROP USER 'LinqUser'@'%';*/


## create user LinqUser, password: 'linq2'
##CREATE USER 'LinqUser'@'%'
##  IDENTIFIED BY PASSWORD '*247E8BFCE2F07F00D7FD773390A282540001077B';

## give our new user full permissions on new database:
GRANT ALL ON Northwind.*  TO 'LinqUser'@'%';
FLUSH PRIVILEGES;


####################################################################
## create tables
####################################################################

CREATE TABLE `Region` (
  `RegionID` INTEGER NOT NULL AUTO_INCREMENT,
  `RegionDescription` VARCHAR(50) NOT NULL,
  PRIMARY KEY(`RegionID`)
)
ENGINE = InnoDB;

CREATE TABLE `Territories` (
  `TerritoryID` VARCHAR(20) NOT NULL,
  `TerritoryDescription` VARCHAR(50) NOT NULL,
  `RegionID` INTEGER NOT NULL,
  PRIMARY KEY(`TerritoryID`),
  FOREIGN KEY `FK_Terr_Region` (`RegionID`) REFERENCES `Region` (`RegionID`)
)
ENGINE = InnoDB;


####################################################################
CREATE TABLE `Categories` (
  `CategoryID` INTEGER  NOT NULL AUTO_INCREMENT,
  `CategoryName` VARCHAR(15) NOT NULL,
  `Description` TEXT NULL,
  `Picture` BLOB NULL,
  PRIMARY KEY(`CategoryID`)
)
ENGINE = InnoDB;

CREATE TABLE `Suppliers` (
  `SupplierID` INTEGER  NOT NULL AUTO_INCREMENT,
  `CompanyName` VARCHAR(40) NOT NULL DEFAULT '',
  `ContactName` VARCHAR(30) NULL,
  `ContactTitle` VARCHAR(30) NULL,
  `Address` VARCHAR(60) NULL,
  `City` VARCHAR(15) NULL,
  `Region` VARCHAR(15) NULL,
  `PostalCode` VARCHAR(10) NULL,
  `Country` VARCHAR(15) NULL,
  `Phone` VARCHAR(24) NULL,
  `Fax` VARCHAR(24) NULL,
  PRIMARY KEY(`SupplierID`)
)
ENGINE = InnoDB;

####################################################################
CREATE TABLE `Products` (
  `ProductID` INTEGER NOT NULL AUTO_INCREMENT,
  `ProductName` VARCHAR(40) NOT NULL DEFAULT '',
  `SupplierID` INTEGER NULL,
  `CategoryID` INTEGER NULL,
  `QuantityPerUnit` VARCHAR(20) NULL,
  `UnitPrice` DECIMAL NULL,
  `UnitsInStock` SMALLINT NULL,
  `UnitsOnOrder` SMALLINT NULL,
  `ReorderLevel` SMALLINT NULL,
  `Discontinued` BIT NOT NULL,
  PRIMARY KEY(`ProductID`),
  FOREIGN KEY `FK_prod_catg` (`CategoryID`) REFERENCES `Categories` (`CategoryID`),
  FOREIGN KEY `FK_prod_supp` (`SupplierID`) REFERENCES `Suppliers` (`SupplierID`)
)
ENGINE = InnoDB
COMMENT = 'Holds Products';


####################################################################
CREATE TABLE Customers (
  `CustomerID` VARCHAR(5) NOT NULL,
  `CompanyName` VARCHAR(40) NOT NULL DEFAULT '',
  `ContactName` VARCHAR(30) NULL,
  `ContactTitle` VARCHAR(30) NULL,
  `Address` VARCHAR(60) NULL,
  `City` VARCHAR(15) NULL,
  `Region` VARCHAR(15) NULL,
  `PostalCode` VARCHAR(10) NULL,
  `Country` VARCHAR(15) NULL,
  `Phone` VARCHAR(24) NULL,
  `Fax` VARCHAR(24) NULL,
  PRIMARY KEY(`CustomerID`)
)
ENGINE = InnoDB;

####################################################################
CREATE TABLE `Shippers` (
  `ShipperID` INTEGER NOT NULL AUTO_INCREMENT,
  `CompanyName` VARCHAR(40) NOT NULL,
  `Phone` VARCHAR(24) NULL,
  PRIMARY KEY(`ShipperID`)
)
ENGINE = InnoDB;

####################################################################
CREATE TABLE `Employees` (
  `EmployeeID` INTEGER NOT NULL AUTO_INCREMENT,
  `LastName` VARCHAR(20) NOT NULL,
  `FirstName` VARCHAR(10) NOT NULL,
  `Title` VARCHAR(30) NULL,
  `BirthDate` DATETIME NULL,
  `HireDate` DATETIME NULL,
  `Address` VARCHAR(60) NULL,
  `City` VARCHAR(15) NULL,
  `Region` VARCHAR(15) NULL,
  `PostalCode` VARCHAR(10) NULL,
  `Country` VARCHAR(15) NULL,
  `HomePhone` VARCHAR(24) NULL,
  `Photo` BLOB NULL,
  `Notes` TEXT NULL,
  `TitleOfCourtesy` VARCHAR(25) NULL,
  `PhotoPath` VARCHAR (255) NULL,
  `Extension` VARCHAR(5) NULL,
  `ReportsTo` INTEGER NULL,
  PRIMARY KEY(`EmployeeID`),
  FOREIGN KEY `FK_Emp_ReportsToEmp` (`ReportsTo`)  REFERENCES `Employees` (`EmployeeID`)
)
ENGINE = InnoDB;

####################################################################
CREATE TABLE `EmployeeTerritories` (
  `EmployeeID` INTEGER NOT NULL,
  `TerritoryID` VARCHAR(20) NOT NULL,
  PRIMARY KEY(`EmployeeID`,`TerritoryID`),
  FOREIGN KEY `FK_empTerr_emp` (`EmployeeID`) REFERENCES `Employees` (`EmployeeID`),
  FOREIGN KEY `FK_empTerr_terr` (`TerritoryID`) REFERENCES `Territories` (`TerritoryID`)
)
ENGINE = InnoDB;


####################################################################
CREATE TABLE `Orders` (
  `OrderID` INTEGER NOT NULL AUTO_INCREMENT,
  `CustomerID` VARCHAR(5) NULL,
  `EmployeeID` INTEGER NULL,
  `OrderDate` DATETIME NULL,
  `RequiredDate` DATETIME NULL,
  `ShippedDate` DATETIME NULL,
  `ShipVia` INT NULL,
  `Freight` DECIMAL NULL,
  `ShipName` VARCHAR(40) NULL,
  `ShipAddress` VARCHAR(60) NULL,
  `ShipCity` VARCHAR(15) NULL,
  `ShipRegion` VARCHAR(15) NULL,
  `ShipPostalCode` VARCHAR(10) NULL,
  `ShipCountry` VARCHAR(15) NULL,
  PRIMARY KEY(`OrderID`),
  FOREIGN KEY `FK_orders_cust` (`CustomerID`) REFERENCES Customers (`CustomerID`),
  FOREIGN KEY `FK_orders_emp` (`EmployeeID`) REFERENCES `Employees` (`EmployeeID`),
  FOREIGN KEY `FK_orders_ship` (`ShipVia`) REFERENCES `Shippers` (`ShipperID`)
)
ENGINE = InnoDB;

####################################################################
CREATE TABLE `Order Details` (
  `OrderID` INTEGER NOT NULL,
  `ProductID` INTEGER NOT NULL,
  `UnitPrice` DECIMAL NOT NULL,
  `Quantity` SMALLINT NOT NULL,
  `Discount` FLOAT NOT NULL,
  PRIMARY KEY(`OrderID`,`ProductID`),
  FOREIGN KEY `FK_orderDet_ord` (`OrderID`) REFERENCES `Orders` (`OrderID`),
  FOREIGN KEY `FK_orderDet_prod` (`ProductID`) REFERENCES `Products` (`ProductID`)
)
ENGINE = InnoDB;




####################################################################
## populate tables with seed data
####################################################################
truncate table `Categories`;
Insert INTO Categories (CategoryName,Description)
values ('Beverages',	'Soft drinks, coffees, teas, beers, and ales');

Insert INTO Categories (CategoryName,Description)
values ('Condiments','Sweet and savory sauces, relishes, spreads, and seasonings');

Insert INTO Categories (CategoryName,Description)
values ('Seafood','Seaweed and fish');


INSERT INTO Region (RegionDescription) VALUES ('North America');
INSERT INTO Region (RegionDescription) VALUES ('Europe');

INSERT INTO Territories (TerritoryID,TerritoryDescription, RegionID) VALUES ('US.Northwest', 'Northwest', 1);

truncate table `Orders`; -- must be truncated before Customer
truncate table Customers;

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

truncate table `Orders`; -- must be truncated before Products
truncate table `Products`;
truncate table `Suppliers`;

insert INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region, Country)
VALUES ('alles AG', 'Harald Reitmeyer', 'Prof', 'Fischergasse 8', 'Heidelberg', 'B-W', 'Germany');

insert INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region, Country)
VALUES ('Microsoft', 'Mr Allen', 'Monopolist', '1 MS', 'Redmond', 'WA', 'USA');

INSERT INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region, PostalCode, Country, Phone, Fax)
VALUES ('Pavlova, Ltd.', 'Ian Devling', 'Marketing Manager', '74 Rose St. Moonie Ponds', 'Melbourne', 'Victoria', '3058', 'Australia', '(03) 444-2343', '(03) 444-6588');


## (OLD WARNING: this actually inserts two 'Pen' rows into Products.)
## could someone with knowledge of MySQL resolve this?
## Answer: upgrade to newer version of MySql Query Browser - the problem will go away
insert INTO `Products` (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Pen',1, 10,     12, 2,  0);
insert INTO `Products` (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Bicycle',1, 1,  6, 0,  0);
insert INTO `Products` (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Phone',3,    7, 0,  0);
insert INTO `Products` (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('SAM',1,      51, 11, 0);
insert INTO `Products` (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('iPod',0,     11, 0, 0);
insert INTO `Products` (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Toilet Paper',2,  0, 3, 1);
insert INTO `Products` (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Fork',5,   111, 0, 0);
insert INTO `Products` (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Linq Book',2, 1, 0, 26, 0);
INSERT INTO `Products` (ProductName,SupplierID, QuantityPerUnit,UnitPrice,  UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Carnarvon Tigers', 3,'16 kg pkg.',62.50,  42, 0, 0);

truncate table `Employees`;

insert INTO `Employees` (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo,Country,HomePhone)
VALUES ('Fuller','Andrew','Vice President, Sales','19540101','19890101', '908 W. Capital Way','Tacoma',NULL,'USA','(111)222333');

insert INTO `Employees` (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo,Country,HomePhone)
VALUES ('Davolio','Nancy','Sales Representative','19640101','19940101','507 - 20th Ave. E.  Apt. 2A','Seattle',1,'USA','(444)555666');

insert INTO `Employees` (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo,Country,HomePhone)
VALUES ('Builder','Bob','Handyman','19640101','19940101','666 dark street','Seattle',2,'USA','(777)888999');

insert into employeeTerritories (EmployeeID,TerritoryID)
values (2,'US.Northwest');

####################################################################
truncate table `Orders`;
insert INTO `Orders` (CustomerID, EmployeeID, OrderDate, Freight)
Values ('AIRBU', 1, now(), 21.3);

insert INTO `Orders` (CustomerID, EmployeeID, OrderDate, Freight)
Values ('BT___', 1, now(), 11.1);

insert INTO `Orders` (CustomerID, EmployeeID, OrderDate, Freight)
Values ('BT___', 1, now(), 11.5);

insert INTO `Orders` (CustomerID, EmployeeID, OrderDate, Freight)
Values ('UKMOD', 1, now(), 32.5);

insert INTO `Orders` (CustomerID, EmployeeID, OrderDate, RequiredDate, ShippedDate, Freight, ShipName, ShipAddress, ShipCity, ShipCountry)
Values ('BONAP', 1, '1996-10-16', '1996-11-27', '1996-10-21', 10.21, 'Bon app''', '12, rue des Bouchers', 'Marseille', 'France' );

INSERT INTO `Order Details` (OrderID, ProductID, UnitPrice, Quantity, Discount)
VALUES (1,2, 33, 5, 11);

INSERT INTO `Order Details` (OrderID, ProductID, UnitPrice, Quantity,   Discount)
VALUES (5,9, 50, 20,   0.05); ## CanarvonTigers

####################################################################
## create stored procs
####################################################################
/* we also need some functions to test the -sprocs option **/
CREATE FUNCTION hello0() RETURNS char(20) RETURN 'hello0';
CREATE FUNCTION hello1(s CHAR(20)) RETURNS char(30) RETURN CONCAT('Hello, ',s,'!');
CREATE FUNCTION `hello2`(s CHAR(20),s2 int) RETURNS char(30) RETURN CONCAT('Hello, ',s,'!');

DELIMITER $$

DROP FUNCTION IF EXISTS `getOrderCount` $$
CREATE FUNCTION `getOrderCount`(custId VARCHAR(5)) RETURNS INT
BEGIN
DECLARE count1 int;
SELECT COUNT(*) INTO count1 FROM Orders WHERE CustomerID=custId;
RETURN count1;
END $$

DROP PROCEDURE IF EXISTS `sp_selOrders` $$
CREATE PROCEDURE `sp_selOrders`(s CHAR(20), OUT s2 int)
BEGIN
set s2 = 22;
select * from orders;
END $$

DROP PROCEDURE IF EXISTS `sp_updOrders` $$
CREATE PROCEDURE `sp_updOrders`(custID int, prodId int)
BEGIN
UPDATE Orders
SET OrderDate=Now()
WHERE ProductId=prodId AND CustomerID=custId;
END $$

DELIMITER ;