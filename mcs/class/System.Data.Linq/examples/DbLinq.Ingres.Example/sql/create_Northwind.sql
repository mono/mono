/********************************************************************/
/* Script to create Ingres version of the Northwind test DB         */
/********************************************************************/

/********************************************************************/

CREATE SEQUENCE Region_seq     START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Categories_seq START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Suppliers_seq  START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Products_seq   START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Orders_seq     START WITH 1    INCREMENT BY 1;
CREATE SEQUENCE Employees_seq  START WITH 1    INCREMENT BY 1;
COMMIT;\p\g

/********************************************************************/

CREATE TABLE Region (
  RegionID INTEGER NOT NULL DEFAULT Region_seq.nextval,
  RegionDescription VARCHAR(50) NOT NULL,
  PRIMARY KEY(RegionID)
);
COMMIT;\p\g

CREATE TABLE Territories (
  TerritoryID VARCHAR(20) NOT NULL,
  TerritoryDescription VARCHAR(50) NOT NULL,
  RegionID INTEGER NOT NULL,
  PRIMARY KEY(TerritoryID),
  FOREIGN KEY (RegionID) REFERENCES Region (RegionID)
);
COMMIT;\p\g

/********************************************************************/

CREATE TABLE Categories (
  CategoryID INTEGER  NOT NULL DEFAULT Categories_seq.nextval,
  CategoryName VARCHAR(15) NOT NULL,
  Description VARCHAR(500),
  Picture LONG BYTE,
  PRIMARY KEY(CategoryID)
);
COMMIT;\p\g

CREATE TABLE Suppliers (
  SupplierID INTEGER NOT NULL DEFAULT Suppliers_seq.nextval,
  CompanyName VARCHAR(40) NOT NULL,
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
);
COMMIT;\p\g

/********************************************************************/

CREATE TABLE Products (
  ProductID INTEGER NOT NULL DEFAULT Products_seq.nextval,
  ProductName VARCHAR(40) NOT NULL,
  SupplierID INTEGER,
  CategoryID INTEGER,
  QuantityPerUnit VARCHAR(20),
  UnitPrice DECIMAL NULL,
  UnitsInStock SMALLINT NULL,
  UnitsOnOrder SMALLINT NULL,
  ReorderLevel SMALLINT NULL,
  Discontinued SMALLINT NOT NULL,
  PRIMARY KEY(ProductID),
  FOREIGN KEY (CategoryID) REFERENCES Categories (CategoryID),
  FOREIGN KEY (SupplierID) REFERENCES Suppliers (SupplierID)
);
COMMIT;\p\g
  
CREATE TABLE Customers (
  CustomerID VARCHAR(5) NOT NULL,
  CompanyName VARCHAR(40) NOT NULL,
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
);
COMMIT;\p\g

/********************************************************************/

CREATE TABLE Employees (
  EmployeeID INTEGER NOT NULL DEFAULT Employees_seq.nextval,
  LastName VARCHAR(20) NOT NULL,
  FirstName VARCHAR(10) NOT NULL,
  Title VARCHAR(30),
  BirthDate INGRESDATE,
  HireDate INGRESDATE,
  Address VARCHAR(60),
  City VARCHAR(15),
  Region VARCHAR(15),
  PostalCode VARCHAR(10),
  Country VARCHAR(15),
  HomePhone VARCHAR(24),
  Photo LONG BYTE,
  Notes VARCHAR(100),
  TitleOfCourtesy VARCHAR(25),
  PhotoPath VARCHAR (255),
  Extension VARCHAR(5),
  ReportsTo INTEGER,
  PRIMARY KEY(EmployeeID),
  FOREIGN KEY (ReportsTo)  REFERENCES Employees (EmployeeID)
);
COMMIT;\p\g

CREATE TABLE EmployeeTerritories (
  EmployeeID INTEGER NOT NULL,
  TerritoryID VARCHAR(20) NOT NULL,
  PRIMARY KEY(EmployeeID,TerritoryID),
  FOREIGN KEY (EmployeeID) REFERENCES Employees (EmployeeID),
  FOREIGN KEY (TerritoryID) REFERENCES Territories (TerritoryID)
);
COMMIT;\p\g

/********************************************************************/

CREATE TABLE Orders (
  OrderID INTEGER NOT NULL DEFAULT Orders_seq.nextval,
  CustomerID VARCHAR(5),
  EmployeeID INTEGER,
  OrderDate INGRESDATE,
  RequiredDate INGRESDATE,
  ShippedDate INGRESDATE,
  ShipVia INT NULL,
  Freight DECIMAL,
  ShipName VARCHAR(40),
  ShipAddress VARCHAR(60),
  ShipCity VARCHAR(15),
  ShipRegion VARCHAR(15),
  ShipPostalCode VARCHAR(10),
  ShipCountry VARCHAR(15),
  PRIMARY KEY(OrderID),
  FOREIGN KEY (CustomerID) REFERENCES Customers (CustomerID),
  FOREIGN KEY (EmployeeID) REFERENCES Employees (EmployeeID)
);
COMMIT;\p\g

/********************************************************************/

CREATE TABLE OrderDetails (
  OrderID INTEGER NOT NULL,
  ProductID INTEGER NOT NULL,
  UnitPrice DECIMAL NOT NULL,
  Quantity SMALLINT NOT NULL,
  Discount FLOAT NOT NULL,
  PRIMARY KEY(OrderID,ProductID),
  FOREIGN KEY (OrderID) REFERENCES Orders (OrderID),
  FOREIGN KEY (ProductID) REFERENCES Products (ProductID)
);
COMMIT;\p\g 

/********************************************************************/

INSERT INTO Categories (CategoryName, Description)
VALUES ('Beverages',	'Soft drinks, coffees, teas, beers, and ales');
INSERT INTO Categories (CategoryName,Description)
VALUES ('Condiments','Sweet and savory sauces, relishes, spreads, and seasonings');
INSERT INTO Categories (CategoryName,Description)
VALUES ('Seafood','Seaweed and fish');

COMMIT;\p\g

/********************************************************************/

INSERT INTO Region (RegionDescription) VALUES ('North America');
INSERT INTO Region (RegionDescription) VALUES ('Europe');
COMMIT;\p\g

/********************************************************************/

INSERT INTO Customers (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
VALUES ('AIRBU', 'airbus','jacques','France','10000','Paris');
INSERT INTO Customers (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
VALUES ('BT___','BT','graeme','U.K.','E14','London');

INSERT INTO Customers (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
VALUES ('ATT__','ATT','bob','USA','10021','New York');
INSERT INTO Customers (CustomerID, CompanyName,ContactName,Country,PostalCode,City)
VALUES ('UKMOD', 'MOD','(secret)','U.K.','E14','London');

INSERT INTO Customers (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,City, Phone)
VALUES ('ALFKI', 'Alfreds Futterkiste','Maria Anders','Sales Representative','Germany','12209','Berlin','030-0074321');

INSERT INTO Customers (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,Address,City, Phone, Fax)
values ('BONAP', 'Bon app''','Laurence Lebihan','Owner','France','13008','12, rue des Bouchers','Marseille','91.24.45.40', '91.24.45.41');

INSERT INTO Customers (CustomerID, CompanyName,ContactName, ContactTitle, Country,PostalCode,City, Phone)
VALUES ('WARTH', 'Wartian Herkku','Pirkko Koskitalo','Accounting Manager','Finland','90110','Oulu','981-443655');
COMMIT;\p\g

/********************************************************************/

INSERT INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region, Country)
VALUES ('alles AG', 'Harald Reitmeyer', 'Prof', 'Fischergasse 8', 'Heidelberg', 'B-W', 'Germany');

INSERT INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region, Country)
VALUES ('Microsoft', 'Mr Allen', 'Monopolist', '1 MS', 'Redmond', 'WA', 'USA');

INSERT INTO Suppliers (CompanyName, ContactName, ContactTitle, Address, City, Region, PostalCode, Country, Phone, Fax)
VALUES ('Pavlova, Ltd.', 'Ian Devling', 'Marketing Manager', '74 Rose St. Moonie Ponds', 'Melbourne', 'Victoria', '3058', 'Australia', '(03) 444-2343', '(03) 444-6588');


INSERT INTO Products (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Pen',1, 10,     12, 2,  0);
INSERT INTO Products (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Bicycle',1, 1,  6, 0,  0);
INSERT INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Phone',3,    7, 0,  0);
INSERT INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('SAM',1,      51, 11, 0);
INSERT INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('iPod',0,     11, 0, 0);
INSERT INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Toilet Paper',2,  0, 3, 1);
INSERT INTO Products (ProductName,QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Fork',5,   111, 0, 0);
INSERT INTO Products (ProductName,SupplierID, QuantityPerUnit,UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Linq Book',2, 1, 0, 26, 0);
INSERT INTO Products (ProductName,SupplierID, QuantityPerUnit,UnitPrice,  UnitsInStock,UnitsOnOrder,Discontinued)
VALUES ('Carnarvon Tigers', 3,'16 kg pkg.',62.50,  42, 0, 0);

COMMIT;\p\g

/********************************************************************/

INSERT INTO Employees (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo,Country,HomePhone)
VALUES ('Fuller','Andrew','Vice President, Sales',date('01-jan-1964'), date('01-jan-1989'), '908 W. Capital Way','Tacoma',NULL,'USA','(111)222333');

INSERT INTO Employees (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo,Country,HomePhone)
VALUES ('Davolio','Nancy','Sales Representative',date('01-jan-1964'), date('01-jan-1994'),'507 - 20th Ave. E.  Apt. 2A','Seattle',1,'USA','(444)555666');

INSERT INTO Employees (LastName,FirstName,Title,BirthDate,HireDate,Address,City,ReportsTo,Country,HomePhone)
VALUES ('Builder', 'Bob', 'Handyman',date('01-jan-1964'), date('01-jan-1964'),'666 dark street','Seattle',2,'USA','(777)888999');
COMMIT;\p\g

INSERT INTO Territories (TerritoryID, TerritoryDescription, RegionID) VALUES ('US.Northwest', 'Northwest', 1);
COMMIT;\p\g

INSERT INTO EmployeeTerritories (EmployeeID,TerritoryID)
VALUES (2,'US.Northwest');
COMMIT;\p\g

/********************************************************************/
/*truncate table Orders;*/
/********************************************************************/
INSERT INTO Orders (CustomerID, EmployeeID, OrderDate, Freight)
VALUES ('AIRBU', 1, date('now'), 21.3);

INSERT INTO Orders (CustomerID, EmployeeID, OrderDate, Freight)
VALUES ('BT___', 1, date('now'), 11.1);

INSERT INTO Orders (CustomerID, EmployeeID, OrderDate, Freight)
VALUES ('BT___', 1, date('now'), 11.5);

INSERT INTO Orders (CustomerID, EmployeeID, OrderDate, Freight)
VALUES ('UKMOD', 1, date('now'), 32.5);

INSERT INTO Orders (CustomerID, EmployeeID, OrderDate, RequiredDate, ShippedDate, Freight, ShipName, ShipAddress, ShipCity, ShipCountry)
VALUES ('BONAP', 1, '16-oct-1996', '27-nov-1996', '21-oct-1996', 10.21, 'Bon app''', '12, rue des Bouchers', 'Marseille', 'France' );


INSERT INTO OrderDetails (OrderID, ProductID, UnitPrice, Quantity, Discount)
VALUES (1, 2, 33, 5, 11);

INSERT INTO OrderDetails (OrderID, ProductID, UnitPrice, Quantity,   Discount)
VALUES (5,9, 50, 20,   0.05); /* CanarvonTigers for customer BONAP */

COMMIT;\p\g
\q


