--####################################################################
--Peter Magnusson provided the script to create SqlLite version of the Northwind test DB - thanks!
--####################################################################

CREATE TABLE IF NOT EXISTS [Regions] (
  [RegionID] INTEGER PRIMARY KEY  NOT NULL,
  [RegionDescription] VARCHAR(50) NOT NULL
  
);


CREATE TABLE IF NOT EXISTS [Territories] (
  [TerritoryID] VARCHAR(20)  NOT NULL,
  [TerritoryDescription] VARCHAR(50) NOT NULL,
  [RegionID] INTEGER NOT NULL REFERENCES Regions(RegionID),
  PRIMARY KEY([TerritoryID])
);



--####################################################################
CREATE TABLE IF NOT EXISTS [Categories] (
  [CategoryID] INTEGER  NOT NULL ,
  [CategoryName] VARCHAR(15) NOT NULL,
  [Description] TEXT NULL,
  [Picture] BLOB NULL,
  PRIMARY KEY([CategoryID])
);


CREATE TABLE IF NOT EXISTS [Suppliers] (
  [SupplierID] INTEGER  NOT NULL ,
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
  PRIMARY KEY([SupplierID])
);


--####################################################################
CREATE TABLE IF NOT EXISTS [Products] (
  [ProductID] INTEGER NOT NULL ,
  [ProductName] VARCHAR(40) NOT NULL DEFAULT '' COLLATE NOCASE,
  [SupplierID] INTEGER NULL REFERENCES Suppliers(SupplierID),
  [CategoryID] INTEGER NULL,
  [QuantityPerUnit] VARCHAR(20) NULL,
  [UnitPrice] DECIMAL NULL,
  [UnitsInStock] SMALLINT NULL,
  [UnitsOnOrder] SMALLINT NULL,
  [ReorderLevel] SMALLINT NULL,
  [Discontinued] BIT NOT NULL,
  PRIMARY KEY([ProductID])
);



--####################################################################
CREATE TABLE IF NOT EXISTS [Customers] (
  [CustomerID] VARCHAR(5) NOT NULL,
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
  PRIMARY KEY([CustomerID])
);


--####################################################################
CREATE TABLE IF NOT EXISTS [Shippers] (
  [ShipperID] INTEGER NOT NULL ,
  [CompanyName] VARCHAR(40) NOT NULL,
  [Phone] VARCHAR(24) NULL,
  PRIMARY KEY([ShipperID])
);


--####################################################################
CREATE TABLE IF NOT EXISTS [Employees] (
  [EmployeeID] INTEGER NOT NULL ,
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
  [Photo] BLOB NULL,
  [Notes] TEXT NULL,
  [TitleOfCourtesy] VARCHAR(25) NULL,
  [PhotoPath] VARCHAR (255) NULL,
  [Extension] VARCHAR(5) NULL,
  [ReportsTo] INTEGER NULL REFERENCES Employees(EmployeeID),
  PRIMARY KEY([EmployeeID])
);


--####################################################################
CREATE TABLE IF NOT EXISTS [EmployeeTerritories] (
  [EmployeeID] INTEGER NOT NULL REFERENCES Employees(EmployeeID),
  [TerritoryID] VARCHAR(20) NOT NULL REFERENCES Territories(TerritoryID),
  PRIMARY KEY([EmployeeID],[TerritoryID])
);



--####################################################################
CREATE TABLE IF NOT EXISTS [Orders] (
  [OrderID] INTEGER NOT NULL ,
  [CustomerID] VARCHAR(5) NULL CONSTRAINT fk_customer_id REFERENCES Customers(CustomerID),
  [EmployeeID] INTEGER NULL CONSTRAINT fk_employee_id REFERENCES Employees(EmployeeID),
  [OrderDate] DATETIME NULL,
  [RequiredDate] DATETIME NULL,
  [ShippedDate] DATETIME NULL,
  [ShipVia] INT NULL,
  [Freight] DECIMAL NULL,
  [ShipName] VARCHAR(40) NULL,
  [ShipAddress] VARCHAR(60) NULL,
  [ShipCity] VARCHAR(15) NULL,
  [ShipRegion] VARCHAR(15) NULL,
  [ShipPostalCode] VARCHAR(10) NULL,
  [ShipCountry] VARCHAR(15) NULL,
  PRIMARY KEY([OrderID])
);




-- Foreign Key Preventing insert
CREATE TRIGGER IF NOT EXISTS fki_Orders_CustomerID_Customers_CustomerID
BEFORE INSERT ON [Orders]
FOR EACH ROW BEGIN
  SELECT RAISE(ROLLBACK, 'insert on table "Orders" violates foreign key constraint "fki_Orders_CustomerID_Customers_CustomerID"')
  WHERE NEW.CustomerID IS NOT NULL AND (SELECT [CustomerID] FROM [Customers] WHERE [CustomerID] = NEW.CustomerID) IS NULL;
END;

-- Foreign key preventing update
CREATE TRIGGER IF NOT EXISTS fku_Orders_CustomerID_Customers_CustomerID
BEFORE UPDATE ON [Orders]
FOR EACH ROW BEGIN
    SELECT RAISE(ROLLBACK, 'update on table "Orders" violates foreign key constraint "fku_Orders_CustomerID_Customers_CustomerID"')
      WHERE NEW.CustomerID IS NOT NULL AND (SELECT [CustomerID] FROM [Customers] WHERE [CustomerID] = NEW.CustomerID) IS NULL;
END;

-- Foreign key preventing delete
CREATE TRIGGER IF NOT EXISTS fkd_Orders_CustomerID_Customers_CustomerID
BEFORE DELETE ON [Customers]
FOR EACH ROW BEGIN
  SELECT RAISE(ROLLBACK, 'delete on table "[Customers]" violates foreign key constraint "fkd_Orders_CustomerID_Customers_CustomerID"')
  WHERE (SELECT CustomerID FROM Orders WHERE CustomerID = OLD.[CustomerID]) IS NOT NULL;
END;
-- Foreign Key Preventing insert
CREATE TRIGGER IF NOT EXISTS fki_Orders_EmployeeID_Employees_EmployeeID
BEFORE INSERT ON [Orders]
FOR EACH ROW BEGIN
  SELECT RAISE(ROLLBACK, 'insert on table "Orders" violates foreign key constraint "fki_Orders_EmployeeID_Employees_EmployeeID"')
  WHERE NEW.EmployeeID IS NOT NULL AND (SELECT [EmployeeID] FROM [Employees] WHERE [EmployeeID] = NEW.EmployeeID) IS NULL;
END;

-- Foreign key preventing update
CREATE TRIGGER IF NOT EXISTS fku_Orders_EmployeeID_Employees_EmployeeID
BEFORE UPDATE ON [Orders]
FOR EACH ROW BEGIN
    SELECT RAISE(ROLLBACK, 'update on table "Orders" violates foreign key constraint "fku_Orders_EmployeeID_Employees_EmployeeID"')
      WHERE NEW.EmployeeID IS NOT NULL AND (SELECT [EmployeeID] FROM [Employees] WHERE [EmployeeID] = NEW.EmployeeID) IS NULL;
END;

-- Foreign key preventing delete
CREATE TRIGGER IF NOT EXISTS fkd_Orders_EmployeeID_Employees_EmployeeID
BEFORE DELETE ON [Employees]
FOR EACH ROW BEGIN
  SELECT RAISE(ROLLBACK, 'delete on table "[Employees]" violates foreign key constraint "fkd_Orders_EmployeeID_Employees_EmployeeID"')
  WHERE (SELECT EmployeeID FROM Orders WHERE EmployeeID = OLD.[EmployeeID]) IS NOT NULL;
END;

--####################################################################
CREATE TABLE IF NOT EXISTS [Order Details] (
  [OrderID] INTEGER NOT NULL                 REFERENCES Orders (OrderID),
  [ProductID] INTEGER NOT NULL               REFERENCES Products (ProductID),
  [UnitPrice] DECIMAL NOT NULL,
  [Quantity] SMALLINT NOT NULL,
  [Discount] FLOAT NOT NULL,
  PRIMARY KEY([OrderID],[ProductID])
);





--####################################################################
--## populate tables with seed data
--####################################################################
DELETE FROM [Categories];
Insert INTO [Categories] (CategoryName,Description)
values ('Beverages',	'Soft drinks, coffees, teas, beers, and ales');
Insert INTO [Categories] (CategoryName,Description)
values     ('Condiments','Sweet and savory sauces, relishes, spreads, and seasonings');
Insert INTO [Categories] (CategoryName,Description)
values ('Seafood','Seaweed and fish');


INSERT INTO Regions (RegionDescription) VALUES ('North America');
INSERT INTO Regions (RegionDescription) VALUES ('Europe');

DELETE FROM EmployeeTerritories; -- must be truncated before Territories
DELETE FROM Territories;
INSERT INTO Territories (TerritoryID,TerritoryDescription, RegionID) VALUES ('US.Northwest', 'Northwest', 1);

DELETE FROM [Orders]; -- must be truncated before Customer
DELETE FROM [Customers];

insert INTO [Customers] (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('AIRBU', 'airbus','jacques','France','10000','Paris');
insert INTO [Customers] (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('BT___','BT','graeme','U.K.','E14','London');

insert INTO [Customers] (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('ATT__','ATT','bob','USA','10021','New York');
insert INTO [Customers] (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
values ('UKMOD', 'MOD','(secret)','U.K.','E14','London');

insert INTO [Customers] (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,City, Phone)
values ('ALFKI', 'Alfreds Futterkiste','Maria Anders','Sales Representative','Germany','12209','Berlin','030-0074321');

insert INTO [Customers] (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,Address,City, Phone, Fax)
values ('BONAP', 'Bon app''','Laurence Lebihan','Owner','France','13008','12, rue des Bouchers','Marseille','91.24.45.40', '91.24.45.41');

insert INTO [Customers] (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,City, Phone)
values ('WARTH', 'Wartian Herkku','Pirkko Koskitalo','Accounting Manager','Finland','90110','Oulu','981-443655');

DELETE FROM [Orders]; -- must be truncated before Products
DELETE FROM [Products];
DELETE FROM [Suppliers];

insert INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region, Country)
VALUES ('alles AG', 'Harald Reitmeyer', 'Prof', 'Fischergasse 8', 'Heidelberg', 'B-W', 'Germany');

insert INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region, Country)
VALUES ('Microsoft', 'Mr Allen', 'Monopolist', '1 MS', 'Redmond', 'WA', 'USA');

INSERT INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region, PostalCode, Country, Phone, Fax)
VALUES ('Pavlova, Ltd.', 'Ian Devling', 'Marketing Manager', '74 Rose St. Moonie Ponds', 'Melbourne', 'Victoria', '3058', 'Australia', '(03) 444-2343', '(03) 444-6588');

--## (OLD WARNING: this actually inserts two 'Pen' rows into Products.)
--## could someone with knowledge of MySQL resolve this?
--## Answer: upgrade to newer version of MySql Query Browser - the problem will go away
insert INTO [Products] (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Pen',1, 10,     12, 2,  0);
insert INTO [Products] (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Bicycle',1, 1,  6, 0,  0);
insert INTO [Products] (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Phone',3,    7, 0,  0);
insert INTO [Products] (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('SAM',1,      51, 11, 0);
insert INTO [Products] (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('iPod',0,     11, 0, 0);
insert INTO [Products] (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Toilet Paper',2,  0, 3, 1);
insert INTO [Products] (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Fork',5,   111, 0, 0);
insert INTO [Products] (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Linq Book',2, 1, 0, 26, 0);
INSERT INTO [Products] (ProductName,SupplierID, QuantityPerUnit,UnitPrice,  UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Carnarvon Tigers', 3,'16 kg pkg.',62.50,  42, 0, 0);

DELETE FROM [Employees];

insert INTO [Employees] (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo,Country,HomePhone)
VALUES ('Fuller','Andrew','Vice President, Sales','19540101','19890101', '908 W. Capital Way','Tacoma',NULL,'USA','(111)222333');

insert INTO [Employees] (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo,Country,HomePhone)
VALUES ('Davolio','Nancy','Sales Representative','19640101','19940101','507 - 20th Ave. E.  Apt. 2A','Seattle',1,'USA','(444)555666');

insert INTO [Employees] (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo,Country,HomePhone)
VALUES ('Builder','Bob','Handyman','19640101','19940101','666 dark street','Seattle',2,'USA','(777)888999');

insert into employeeTerritories (EmployeeID,TerritoryID)
values (2,'US.Northwest');

--####################################################################
DELETE FROM [Orders];
insert INTO [Orders] (CustomerID, EmployeeID, OrderDate, Freight)
Values ('AIRBU', 1, '2007-12-14', 21.3);

insert INTO [Orders] (CustomerID, EmployeeID, OrderDate, Freight)
Values ('BT___', 1, '2007-12-15', 11.1);

insert INTO [Orders] (CustomerID, EmployeeID, OrderDate, Freight)
Values ('BT___', 1, '2007-12-16', 11.5);

insert INTO [Orders] (CustomerID, EmployeeID, OrderDate, Freight)
Values ('UKMOD', 1, '2007-12-17', 32.5);

insert INTO [Orders] (CustomerID, EmployeeID, OrderDate, RequiredDate, ShippedDate, Freight, ShipName, ShipAddress, ShipCity, ShipCountry)
Values ('BONAP', 1, '1996-10-16', '1996-11-27', '1996-10-21', 10.21, 'Bon app''', '12, rue des Bouchers', 'Marseille', 'France' );

INSERT INTO [Order Details] (OrderID, ProductID, UnitPrice, Quantity, Discount)
VALUES (1,2, 33, 5, 11);

INSERT INTO [Order Details] (OrderID, ProductID, UnitPrice, Quantity,   Discount)
VALUES (5,9, 50, 20,   0.05); --## CanarvonTigers for customer BONAP



