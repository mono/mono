--------------------------------------------------------------------
-- script to create custom (...) SqlServer version of the Northwind test DB
--------------------------------------------------------------------

USE master;

GO

DROP DATABASE [Northwind];
CREATE DATABASE [Northwind];

USE [Northwind];

GO

--------------------------------------------------------------------
-- create tables
--------------------------------------------------------------------

CREATE TABLE [Region] (
  [RegionID] INTEGER PRIMARY KEY IDENTITY (1,1),
  [RegionDescription] VARCHAR(50) NOT NULL,
);

CREATE TABLE [Territories] (
  [TerritoryID] VARCHAR(20) PRIMARY KEY,
  [TerritoryDescription] VARCHAR(50) NOT NULL,
  [RegionID] INTEGER FOREIGN KEY REFERENCES [Region] ([RegionID]),
);

----------------------------------------------------------------------
CREATE TABLE [Categories] (
  [CategoryID] INTEGER PRIMARY KEY IDENTITY(1,1),
  [CategoryName] VARCHAR(15),
  [Description] TEXT NULL,
  [Picture] VARBINARY(MAX) NULL,
);

CREATE TABLE [Suppliers] (
  [SupplierID] INTEGER  NOT NULL IDENTITY(1,1) PRIMARY KEY,
  [CompanyName] VARCHAR(40) NOT NULL DEFAULT '',
  [ContactName] VARCHAR(30) NULL,
  [ContactTitle] VARCHAR(30) NULL,
  [Address] VARCHAR(60) NULL,
  [City] VARCHAR(15) NULL,
  [Region] VARCHAR(15) NULL,
  [PostalCode] VARCHAR(10) NULL,
  [Country] VARCHAR(15) NULL,
  [Phone] VARCHAR(24) NULL,
  [Fax] VARCHAR(24) NULL,
);

----------------------------------------------------------------------
CREATE TABLE [Products] (
  [ProductID] INTEGER NOT NULL IDENTITY(1,1) PRIMARY KEY,
  [ProductName] VARCHAR(40) NOT NULL DEFAULT '',
  [SupplierID] INTEGER NULL FOREIGN KEY REFERENCES [Suppliers] ([SupplierID]),
  [CategoryID] INTEGER NULL FOREIGN KEY REFERENCES [Categories] ([CategoryID]),
  [QuantityPerUnit] VARCHAR(20) NULL,
  [UnitPrice] DECIMAL NULL,
  [UnitsInStock] SMALLINT NULL,
  [UnitsOnOrder] SMALLINT NULL,
  [ReorderLevel] SMALLINT NULL,
  [Discontinued] BIT NOT NULL,
)


--------------------------------------------------------------------
CREATE TABLE [Customers] (
  [CustomerID] VARCHAR(5) NOT NULL PRIMARY KEY,
  [CompanyName] VARCHAR(40) NOT NULL DEFAULT '',
  [ContactName] VARCHAR(30) NULL,
  [ContactTitle] VARCHAR(30) NULL,
  [Address] VARCHAR(60) NULL,
  [City] VARCHAR(15) NULL,
  [Region] VARCHAR(15) NULL,
  [PostalCode] VARCHAR(10) NULL,
  [Country] VARCHAR(15) NULL,
  [Phone] VARCHAR(24) NULL,
  [Fax] VARCHAR(24) NULL,
);

--------------------------------------------------------------------
CREATE TABLE [Shippers] (
  [ShipperID] INTEGER NOT NULL IDENTITY(1,1) PRIMARY KEY,
  [CompanyName] VARCHAR(40) NOT NULL,
  [Phone] VARCHAR(24) NULL,
);

--------------------------------------------------------------------
CREATE TABLE [Employees] (
  [EmployeeID] INTEGER NOT NULL IDENTITY(1,1) PRIMARY KEY,
  [LastName] VARCHAR(20) NOT NULL,
  [FirstName] VARCHAR(10) NOT NULL,
  [Title] VARCHAR(30) NULL,
  [BirthDate] DATETIME NULL,
  [HireDate] DATETIME NULL,
  [Address] VARCHAR(60) NULL,
  [City] VARCHAR(15) NULL,
  [Region] VARCHAR(15) NULL,
  [PostalCode] VARCHAR(10) NULL,
  [Country] VARCHAR(15) NULL,
  [HomePhone] VARCHAR(24) NULL,
  [Photo] VARBINARY(MAX) NULL,
  [Notes] TEXT NULL,
  [TitleOfCourtesy] VARCHAR(25) NULL,
  [PhotoPath] VARCHAR (255) NULL,
  [Extension] VARCHAR(5) NULL,
  [ReportsTo] INTEGER NULL FOREIGN KEY REFERENCES [Employees] ([EmployeeID]),
);

--------------------------------------------------------------------
CREATE TABLE [EmployeeTerritories] (
  [EmployeeID] INTEGER NOT NULL FOREIGN KEY REFERENCES [Employees] ([EmployeeID]),
  [TerritoryID] VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES [Territories] ([TerritoryID]),
  CONSTRAINT [PK EmployeeTerritories] PRIMARY KEY (
	[EmployeeID],
	[TerritoryID]
  )
);

--------------------------------------------------------------------
CREATE TABLE [Orders] (
  [OrderID] INTEGER NOT NULL IDENTITY(1,1) PRIMARY KEY,
  [CustomerID] VARCHAR(5) NULL FOREIGN KEY REFERENCES [Customers] ([CustomerID]),
  [EmployeeID] INTEGER NULL FOREIGN KEY REFERENCES [Employees] ([EmployeeID]),
  [OrderDate] DATETIME NULL,
  [RequiredDate] DATETIME NULL,
  [ShippedDate] DATETIME NULL,
  [ShipVia] INT NULL FOREIGN KEY REFERENCES [Shippers] ([ShipperID]),
  [Freight] DECIMAL NULL,
  [ShipName] VARCHAR(40) NULL,
  [ShipAddress] VARCHAR(60) NULL,
  [ShipCity] VARCHAR(15) NULL,
  [ShipRegion] VARCHAR(15) NULL,
  [ShipPostalCode] VARCHAR(10) NULL,
  [ShipCountry] VARCHAR(15) NULL,
);

--------------------------------------------------------------------
CREATE TABLE [Order Details] (
  [OrderID] INTEGER NOT NULL FOREIGN KEY REFERENCES [Orders] ([OrderID]),
  [ProductID] INTEGER NOT NULL FOREIGN KEY REFERENCES [Products] ([ProductID]),
  [UnitPrice] DECIMAL NOT NULL,
  [Quantity] SMALLINT NOT NULL,
  [Discount] FLOAT NOT NULL,
  CONSTRAINT [PK Order Details] PRIMARY KEY (
	[OrderID],
	[ProductID]
  )
);

GO

--------------------------------------------------------------------
-- populate tables with seed data
--------------------------------------------------------------------

INSERT INTO [Categories] ([CategoryName], [Description])
VALUES ('Beverages',	'Soft drinks, coffees, teas, beers, and ales');

INSERT INTO [Categories] ([CategoryName], [Description])
VALUES ('Condiments','Sweet and savory sauces, relishes, spreads, and seasonings');

INSERT INTO [Categories] ([CategoryName], [Description])
VALUES ('Seafood','Seaweed and fish');


INSERT INTO [Region] ([RegionDescription]) VALUES ('North America');
INSERT INTO [Region] ([RegionDescription]) VALUES ('Europe');

INSERT INTO [Territories] ([TerritoryID], [TerritoryDescription], [RegionID]) VALUES ('US.Northwest', 'Northwest', 1);

INSERT INTO [Customers] ([CustomerID], [CompanyName], [ContactName], [Country], [PostalCode], [City])
VALUES ('AIRBU', 'airbus','jacques','France','10000','Paris');
INSERT INTO [Customers] ([CustomerID], [CompanyName], [ContactName], [Country], [PostalCode], [City])
VALUES ('BT___','BT','graeme','U.K.','E14','London');

INSERT INTO [Customers] ([CustomerID], [CompanyName], [ContactName], [Country], [PostalCode], [City])
VALUES ('ATT__','ATT','bob','USA','10021','New York');
INSERT INTO [Customers] ([CustomerID], [CompanyName], [ContactName], [Country], [PostalCode], [City])
VALUES ('UKMOD', 'MOD','(secret)','U.K.','E14','London');

INSERT INTO [Customers] ([CustomerID], [CompanyName], [ContactName], [ContactTitle], [Country], [PostalCode], [City], [Phone])
VALUES ('ALFKI', 'Alfreds Futterkiste','Maria Anders','Sales Representative','Germany','12209','Berlin','030-0074321');

INSERT INTO [Customers] ([CustomerID], [CompanyName], [ContactName], [ContactTitle], [Country], [PostalCode], [Address], [City], Phone, Fax)
VALUES ('BONAP', 'Bon app''','Laurence Lebihan','Owner','France','13008','12, rue des Bouchers','Marseille','91.24.45.40', '91.24.45.41');

INSERT INTO [Customers] ([CustomerID], [CompanyName], [ContactName], [ContactTitle], [Country], [PostalCode], [City], [Phone])
VALUES ('WARTH', 'Wartian Herkku','Pirkko Koskitalo','Accounting Manager','Finland','90110','Oulu','981-443655');

INSERT INTO [Suppliers] ([CompanyName], [ContactName], [ContactTitle], [Address], [City], [Region], [Country])
VALUES ('alles AG', 'Harald Reitmeyer', 'Prof', 'Fischergasse 8', 'Heidelberg', 'B-W', 'Germany');

INSERT INTO [Suppliers] ([CompanyName], [ContactName], [ContactTitle], [Address], [City], [Region], [Country])
VALUES ('Microsoft', 'Mr Allen', 'Monopolist', '1 MS', 'Redmond', 'WA', 'USA');

INSERT INTO [Suppliers] ([CompanyName], [ContactName], [ContactTitle], [Address], [City], [Region], [PostalCode], [Country], Phone, Fax)
VALUES ('Pavlova, Ltd.', 'Ian Devling', 'Marketing Manager', '74 Rose St. Moonie Ponds', 'Melbourne', 'Victoria', '3058', 'Australia', '(03) 444-2343', '(03) 444-6588');


-- (OLD WARNING: this actually inserts two 'Pen' rows into Products.)
-- could someone with knowledge of MySQL resolve this?
-- Answer: upgrade to newer version of MySql Query Browser - the problem will go away
INSERT INTO [Products] ([ProductName], [SupplierID], [QuantityPerUnit], [UnitsInStock], [UnitsOnOrder], [Discontinued])
VALUES ('Pen',1, 10,     12, 2,  0);
INSERT INTO [Products] ([ProductName], [SupplierID], [QuantityPerUnit], [UnitsInStock], [UnitsOnOrder], [Discontinued])
VALUES ('Bicycle',1, 1,  6, 0,  0);
INSERT INTO [Products] ([ProductName], [QuantityPerUnit], [UnitsInStock], [UnitsOnOrder], [Discontinued])
VALUES ('Phone',3,    7, 0,  0);
INSERT INTO [Products] ([ProductName], [QuantityPerUnit], [UnitsInStock], [UnitsOnOrder], [Discontinued])
VALUES ('SAM',1,      51, 11, 0);
INSERT INTO [Products] ([ProductName], [QuantityPerUnit], [UnitsInStock], [UnitsOnOrder], [Discontinued])
VALUES ('iPod',0,     11, 0, 0);
INSERT INTO [Products] ([ProductName], [QuantityPerUnit], [UnitsInStock], [UnitsOnOrder], [Discontinued])
VALUES ('Toilet Paper',2,  0, 3, 1);
INSERT INTO [Products] ([ProductName], [QuantityPerUnit], [UnitsInStock], [UnitsOnOrder], [Discontinued])
VALUES ('Fork',5,   111, 0, 0);
INSERT INTO [Products] ([ProductName], [SupplierID], [QuantityPerUnit], [UnitsInStock], [UnitsOnOrder], [Discontinued])
VALUES ('Linq Book',2, 1, 0, 26, 0);
INSERT INTO [Products] ([ProductName], [SupplierID], [QuantityPerUnit],UnitPrice,  [UnitsInStock], [UnitsOnOrder], [Discontinued])
VALUES ('Carnarvon Tigers', 3,'16 kg pkg.',62.50,  42, 0, 0);

INSERT INTO [Employees] ([LastName], [FirstName], [Title], [BirthDate], [HireDate], [Address], [City], [ReportsTo],[Country],[HomePhone])
VALUES ('Fuller','Andrew','Vice President, Sales','19540101','19890101', '908 W. Capital Way','Tacoma',NULL,'USA','(111)222333');

INSERT INTO [Employees] ([LastName], [FirstName], [Title], [BirthDate], [HireDate], [Address], [City], [ReportsTo],[Country],[HomePhone])
VALUES ('Davolio','Nancy','Sales Representative','19640101','19940101','507 - 20th Ave. E.  Apt. 2A','Seattle',1,'USA','(444)555666');

INSERT INTO [Employees] ([LastName], [FirstName], [Title], [BirthDate], [HireDate], [Address], [City], [ReportsTo],[Country],[HomePhone])
VALUES ('Builder','Bob','Handyman','19640101','19940101','666 dark street','Seattle',2,'USA','(777)888999');

INSERT into [EmployeeTerritories] ([EmployeeID], [TerritoryID])
VALUES (2,'US.Northwest');

--------------------------------------------------------------------
INSERT INTO [Orders] ([CustomerID], [EmployeeID], [OrderDate], [Freight])
VALUES ('AIRBU', 1, CURRENT_TIMESTAMP, 21.3);

INSERT INTO [Orders] ([CustomerID], [EmployeeID], [OrderDate], [Freight])
VALUES ('BT___', 1, CURRENT_TIMESTAMP, 11.1);

INSERT INTO [Orders] ([CustomerID], [EmployeeID], [OrderDate], [Freight])
VALUES ('BT___', 1, CURRENT_TIMESTAMP, 11.5);

INSERT INTO [Orders] ([CustomerID], [EmployeeID], [OrderDate], [Freight])
VALUES ('UKMOD', 1, CURRENT_TIMESTAMP, 32.5);

INSERT INTO [Orders] ([CustomerID], [EmployeeID], [OrderDate], [RequiredDate], [ShippedDate], [Freight], [ShipName], [ShipAddress], [ShipCity], [ShipCountry])
VALUES ('BONAP', 1, '1996-10-16', '1996-11-27', '1996-10-21', 10.21, 'Bon app''', '12, rue des Bouchers', 'Marseille', 'France' );

INSERT INTO [Order Details] ([OrderID], [ProductID], [UnitPrice], [Quantity], [Discount])
VALUES (1,2, 33, 5, 11);

INSERT INTO [Order Details] ([OrderID], [ProductID], [UnitPrice], [Quantity], [Discount])
VALUES (5,9, 50, 20,   0.05); -- CanarvonTigers

GO

--------------------------------------------------------------------
-- create stored procs
--------------------------------------------------------------------
/* we also need some functions to test the -sprocs option **/

CREATE FUNCTION hello0() RETURNS CHAR(20) 
AS
BEGIN
	RETURN 'hello0' 
END;

GO

CREATE FUNCTION hello1(@s CHAR(20)) RETURNS CHAR(30) 
AS
BEGIN
	RETURN 'Hello, ' + @s + '!'
END;

GO

CREATE FUNCTION hello2(@s CHAR(20),@s2 INT) RETURNS CHAR(30) 
AS
BEGIN
	RETURN 'Hello, ' + @s + '!'
END;

GO

CREATE FUNCTION [getOrderCount](@custId VARCHAR(5)) RETURNS INT
AS
BEGIN
	DECLARE @count1 INT
	SELECT @count1 = (SELECT COUNT(*) FROM Orders WHERE CustomerID = @custId)
	RETURN @count1
END;

GO

CREATE PROCEDURE [sp_selOrders](@s CHAR(20), @s2 INT OUT)
AS
BEGIN
	SELECT @s2 = 22;
	SELECT * from orders;
END

GO

--CREATE PROCEDURE [sp_updOrders](@custID INT, @prodId INT)
--AS
--BEGIN
--	UPDATE [Orders]
--	SET [OrderDate] = CURRENT_TIMESTAMP
--	WHERE [ProductID] = @prodId AND [CustomerID] = @custId;
--END

GO
