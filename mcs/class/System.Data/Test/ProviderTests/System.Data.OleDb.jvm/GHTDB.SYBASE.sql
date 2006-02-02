use master
go

set nocount on
go
set dateformat mdy
go

if exists (select * from master.dbo.sysdatabases
		where name = "GHTDB")
begin
	drop database GHTDB
end
go
print 'Creating the "GHTDB" database'
go

if (@@maxpagesize = 1024 * 2)
	create database GHTDB on default = 10
else
	create database GHTDB on default = 10
go

exec sp_dboption N'GHTDB', N'trunc log on chkpt', true, 1
print 'print1'
go
exec sp_dboption N'GHTDB', N'trunc. log', N'true', 1
print 'print2'
go
exec sp_dboption N'GHTDB', N'read only', N'false', 1
print 'print3'
go
exec sp_dboption N'GHTDB', N'dbo use', N'false', 1
print 'print4'
go
-- exec sp_dboption N'GHTDB', N'cursor close on commit', N'false', 1
-- exec sp_dboption N'GHTDB', N'abort tran on log full', N'true', 1
print 'print5'
go

use GHTDB
GO

checkpoint
go


SET QUOTED_IDENTIFIER OFF 
GO

print '------------------------------'
print 'create tables - start'
print '------------------------------'
go

CREATE TABLE [dbo].[TYPES_SIMPLE] (
-- ID
	[ID] char(10) NULL,
	[T_BIT] [bit] DEFAULT  0 not null,
-- integer
	[T_TINYINT] [tinyint] NULL ,
	[T_SMALLINT] [smallint] NULL ,
	[T_INT] [int] NULL ,
-- float
	[T_DECIMAL] [decimal](18, 0) NULL ,
	[T_NUMERIC] [numeric](18, 7) NULL ,
	[T_FLOAT] [float] NULL ,
	[T_REAL] [real] NULL ,
-- text
	[T_CHAR] [char] (10) NULL ,
	[T_NCHAR] [nchar] (10) NULL ,
	[T_VARCHAR] [varchar] (50) NULL ,
	[T_NVARCHAR] [nvarchar] (50) NULL
) ON [default]
GO

CREATE TABLE [dbo].[TYPES_EXTENDED] (
-- ID
	[ID] char(10) NULL,
-- Text
	[T_TEXT] [text] NULL ,
	[T_NTEXT] [nvarchar](1000) NULL ,
-- Binary
	[T_BINARY] [binary] (50) NULL ,
	[T_VARBINARY] [varbinary] (50) NULL ,
--Time
	[T_DATETIME] [datetime] NULL ,
	[T_SMALLDATETIME] [smalldatetime] NULL 
) ON [default] 

CREATE TABLE [dbo].[TYPES_SPECIFIC] (
-- ID
	[ID1] char(10) NULL
) ON [default] 
GO

CREATE TABLE [dbo].[Categories] (
	[CategoryID] numeric(5,0) IDENTITY NOT NULL ,
	[CategoryName] [nvarchar] (15) NOT NULL ,
	[Description] [nvarchar](1000) NULL ,
	[Picture] [image] NULL 
) ON [default] 
GO

CREATE TABLE [dbo].[CustomerCustomerDemo] (
	[CustomerID] [nchar] (5) NOT NULL ,
	[CustomerTypeID] [nchar] (10) NOT NULL 
) ON [default]
GO

CREATE TABLE [dbo].[CustomerDemographics] (
	[CustomerTypeID] [nchar] (10) NOT NULL ,
	[CustomerDesc] [nvarchar](1000) NULL 
) ON [default] 
GO

CREATE TABLE [dbo].[Customers] (
	[CustomerID] [nchar] (5) NOT NULL ,
	[CompanyName] [nvarchar] (40) NOT NULL ,
	[ContactName] [nvarchar] (30) NULL ,
	[ContactTitle] [nvarchar] (30) NULL ,
	[Address] [nvarchar] (60) NULL ,
	[City] [nvarchar] (15) NULL ,
	[Region] [nvarchar] (15) NULL ,
	[PostalCode] [nvarchar] (10) NULL ,
	[Country] [nvarchar] (15) NULL ,
	[Phone] [nvarchar] (24) NULL ,
	[Fax] [nvarchar] (24) NULL 
) ON [default]
GO

CREATE TABLE [dbo].[EmployeeTerritories] (
	[EmployeeID] [int] NOT NULL ,
	[TerritoryID] [nvarchar] (20) NOT NULL 
) ON [default]
GO

CREATE TABLE [dbo].[Employees] (
	[EmployeeID] [int] NOT NULL ,
	[LastName] [nvarchar] (20) NOT NULL ,
	[FirstName] [nvarchar] (10) NOT NULL ,
	[Title] [nvarchar] (30) NULL ,
	[TitleOfCourtesy] [nvarchar] (25) NULL ,
	[BirthDate] [datetime] NULL ,
	[HireDate] [datetime] NULL ,
	[Address] [nvarchar] (60) NULL ,
	[City] [nvarchar] (15) NULL ,
	[Region] [nvarchar] (15) NULL ,
	[PostalCode] [nvarchar] (10) NULL ,
	[Country] [nvarchar] (15) NULL ,
	[HomePhone] [nvarchar] (24) NULL ,
	[Extension] [nvarchar] (4) NULL ,
	[Photo] [image] NULL ,
	[Notes] [nvarchar](1000) NULL ,
	[ReportsTo] [int] NULL ,
	[PhotoPath] [nvarchar] (255) NULL 
) ON [default] 
GO

CREATE TABLE [dbo].[GH_EMPTYTABLE] (
	[Col1] [int] NULL ,
	[Col2] [varchar] (50) NULL 
) ON [default]
GO

CREATE TABLE [dbo].[Order Details] (
	[OrderID] numeric(5,0) NOT NULL ,
	[ProductID] numeric(5,0) NOT NULL ,
	[UnitPrice] [money] DEFAULT (0) NOT NULL ,
	[Quantity] [smallint] DEFAULT (1) NOT NULL ,
	[Discount] [real] DEFAULT (0) NOT NULL 
) ON [default]
GO

CREATE TABLE [dbo].[Orders] (
	[OrderID] numeric(5,0) IDENTITY NOT NULL ,
	[CustomerID] [nchar] (5) NULL ,
	[EmployeeID] [int] NULL ,
	[OrderDate] [datetime] NULL ,
	[RequiredDate] [datetime] NULL ,
	[ShippedDate] [datetime] NULL ,
	[ShipVia] numeric(5,0) NULL ,
	[Freight] [money] DEFAULT (0) NULL ,
	[ShipName] [nvarchar] (40) NULL ,
	[ShipAddress] [nvarchar] (60) NULL ,
	[ShipCity] [nvarchar] (15) NULL ,
	[ShipRegion] [nvarchar] (15) NULL ,
	[ShipPostalCode] [nvarchar] (10) NULL ,
	[ShipCountry] [nvarchar] (15) NULL 
) ON [default]
GO

CREATE TABLE [dbo].[Products] (
	[ProductID] numeric(5,0) IDENTITY NOT NULL ,
	[ProductName] [nvarchar] (40) NOT NULL ,
	[SupplierID] numeric(5,0) NULL ,
	[CategoryID] numeric(5,0) NULL ,
	[QuantityPerUnit] [nvarchar] (20) NULL ,
	[UnitPrice] [money] DEFAULT (0) NULL ,
	[UnitsInStock] [smallint] DEFAULT (0) NULL ,
	[UnitsOnOrder] [smallint] DEFAULT (0) NULL ,
	[ReorderLevel] [smallint] DEFAULT (0) NULL ,
	[Discontinued] [bit] NOT NULL 
) ON [default]
GO

CREATE TABLE [dbo].[Region] (
	[RegionID] [int] NOT NULL ,
	[RegionDescription] [nchar] (50) NOT NULL 
) ON [default]
GO

CREATE TABLE [dbo].[Shippers] (
	[ShipperID] numeric(5,0) IDENTITY NOT NULL ,
	[CompanyName] [nvarchar] (40) NOT NULL ,
	[Phone] [nvarchar] (24) NULL 
) ON [default]
GO

CREATE TABLE [dbo].[Suppliers] (
	[SupplierID] numeric(5,0) IDENTITY NOT NULL ,
	[CompanyName] [nvarchar] (40) NOT NULL ,
	[ContactName] [nvarchar] (30) NULL ,
	[ContactTitle] [nvarchar] (30) NULL ,
	[Address] [nvarchar] (60) NULL ,
	[City] [nvarchar] (15) NULL ,
	[Region] [nvarchar] (15) NULL ,
	[PostalCode] [nvarchar] (10) NULL ,
	[Country] [nvarchar] (15) NULL ,
	[Phone] [nvarchar] (24) NULL ,
	[Fax] [nvarchar] (24) NULL ,
	[HomePage] [nvarchar](1000) NULL 
) ON [default] 
GO

CREATE TABLE [dbo].[Territories] (
	[TerritoryID] [nvarchar] (20) NOT NULL ,
	[TerritoryDescription] [nchar] (50) NOT NULL ,
	[RegionID] [int] NOT NULL 
) ON [default]
GO

print '------------------------------'
print 'create tables - finish'
print '------------------------------'
go

ALTER TABLE [dbo].[Categories] ADD 
	CONSTRAINT [PK_Categories] PRIMARY KEY  CLUSTERED 
	(
		[CategoryID]
	)  ON [default] 
GO

ALTER TABLE [dbo].[Customers] ADD 
	CONSTRAINT [PK_Customers] PRIMARY KEY  CLUSTERED 
	(
		[CustomerID]
	)  ON [default] 
GO

ALTER TABLE [dbo].[Employees] ADD 
	CONSTRAINT [PK_Employees] PRIMARY KEY  CLUSTERED 
	(
		[EmployeeID]
	)  ON [default] 
GO

ALTER TABLE [dbo].[Order Details] ADD 
	CONSTRAINT [PK_Order_Details] PRIMARY KEY  CLUSTERED 
	(
		[OrderID],
		[ProductID]
	)  ON [default] 
GO

ALTER TABLE [dbo].[Orders] ADD 
	CONSTRAINT [PK_Orders] PRIMARY KEY  CLUSTERED 
	(
		[OrderID]
	)  ON [default] 
GO

ALTER TABLE [dbo].[Products] ADD 
	CONSTRAINT [PK_Products] PRIMARY KEY  CLUSTERED 
	(
		[ProductID]
	)  ON [default] 
GO

ALTER TABLE [dbo].[Shippers] ADD 
	CONSTRAINT [PK_Shippers] PRIMARY KEY  CLUSTERED 
	(
		[ShipperID]
	)  ON [default] 
GO

ALTER TABLE [dbo].[Suppliers] ADD 
	CONSTRAINT [PK_Suppliers] PRIMARY KEY  CLUSTERED 
	(
		[SupplierID]
	)  ON [default] 
GO

 CREATE  INDEX [CategoryName] ON [dbo].[Categories]([CategoryName]) ON [default]
GO

ALTER TABLE [dbo].[CustomerCustomerDemo] ADD 
	CONSTRAINT [PK_CustomerCustomerDemo] PRIMARY KEY  NONCLUSTERED 
	(
		[CustomerID],
		[CustomerTypeID]
	)  ON [default] 
GO

ALTER TABLE [dbo].[CustomerDemographics] ADD 
	CONSTRAINT [PK_CustomerDemographics] PRIMARY KEY  NONCLUSTERED 
	(
		[CustomerTypeID]
	)  ON [default] 
GO

 CREATE  INDEX [City] ON [dbo].[Customers]([City]) ON [default]
GO

 CREATE  INDEX [CompanyName] ON [dbo].[Customers]([CompanyName]) ON [default]
GO

 CREATE  INDEX [PostalCode] ON [dbo].[Customers]([PostalCode]) ON [default]
GO

 CREATE  INDEX [Region] ON [dbo].[Customers]([Region]) ON [default]
GO

ALTER TABLE [dbo].[EmployeeTerritories] ADD 
	CONSTRAINT [PK_EmployeeTerritories] PRIMARY KEY  NONCLUSTERED 
	(
		[EmployeeID],
		[TerritoryID]
	)  ON [default] 
GO

ALTER TABLE [dbo].[Employees] ADD 
	CONSTRAINT [CK_Birthdate] CHECK ([BirthDate] < getdate())
GO

 CREATE  INDEX [LastName] ON [dbo].[Employees]([LastName]) ON [default]
GO

 CREATE  INDEX [PostalCode] ON [dbo].[Employees]([PostalCode]) ON [default]
GO

ALTER TABLE [dbo].[Order Details] ADD 
	CONSTRAINT [CK_Discount] CHECK ([Discount] >= 0 and [Discount] <= 1),
	CONSTRAINT [CK_Quantity] CHECK ([Quantity] > 0),
	CONSTRAINT [CK_UnitPrice] CHECK ([UnitPrice] >= 0)
GO

print 'got here5'
go

 CREATE  INDEX [OrderID] ON [dbo].[Order Details]([OrderID]) ON [default]
GO

 CREATE  INDEX [OrdersOrder_Details] ON [dbo].[Order Details]([OrderID]) ON [default]
GO

 CREATE  INDEX [ProductID] ON [dbo].[Order Details]([ProductID]) ON [default]
GO

 CREATE  INDEX [ProductsOrder_Details] ON [dbo].[Order Details]([ProductID]) ON [default]
GO

 CREATE  INDEX [CustomerID] ON [dbo].[Orders]([CustomerID]) ON [default]
GO

 CREATE  INDEX [CustomersOrders] ON [dbo].[Orders]([CustomerID]) ON [default]
GO

 CREATE  INDEX [EmployeeID] ON [dbo].[Orders]([EmployeeID]) ON [default]
GO

 CREATE  INDEX [EmployeesOrders] ON [dbo].[Orders]([EmployeeID]) ON [default]
GO

 CREATE  INDEX [OrderDate] ON [dbo].[Orders]([OrderDate]) ON [default]
GO

 CREATE  INDEX [ShippedDate] ON [dbo].[Orders]([ShippedDate]) ON [default]
GO

 CREATE  INDEX [ShippersOrders] ON [dbo].[Orders]([ShipVia]) ON [default]
GO

 CREATE  INDEX [ShipPostalCode] ON [dbo].[Orders]([ShipPostalCode]) ON [default]
GO

print 'got here6'
go

ALTER TABLE [dbo].[Products] ADD 
	CONSTRAINT [CK_Products_UnitPrice] CHECK ([UnitPrice] >= 0),
	CONSTRAINT [CK_ReorderLevel] CHECK ([ReorderLevel] >= 0),
	CONSTRAINT [CK_UnitsInStock] CHECK ([UnitsInStock] >= 0),
	CONSTRAINT [CK_UnitsOnOrder] CHECK ([UnitsOnOrder] >= 0)
GO

print 'got here7'
go

 CREATE  INDEX [CategoriesProducts] ON [dbo].[Products]([CategoryID]) ON [default]
GO

 CREATE  INDEX [CategoryID] ON [dbo].[Products]([CategoryID]) ON [default]
GO

 CREATE  INDEX [ProductName] ON [dbo].[Products]([ProductName]) ON [default]
GO

 CREATE  INDEX [SupplierID] ON [dbo].[Products]([SupplierID]) ON [default]
GO

 CREATE  INDEX [SuppliersProducts] ON [dbo].[Products]([SupplierID]) ON [default]
GO

ALTER TABLE [dbo].[Region] ADD 
	CONSTRAINT [PK_Region] PRIMARY KEY  NONCLUSTERED 
	(
		[RegionID]
	)  ON [default] 
GO

 CREATE  INDEX [CompanyName] ON [dbo].[Suppliers]([CompanyName]) ON [default]
GO

 CREATE  INDEX [PostalCode] ON [dbo].[Suppliers]([PostalCode]) ON [default]
GO

ALTER TABLE [dbo].[Territories] ADD 
	CONSTRAINT [PK_Territories] PRIMARY KEY  NONCLUSTERED 
	(
		[TerritoryID]
	)  ON [default] 
GO

ALTER TABLE [dbo].[CustomerCustomerDemo] ADD 
	CONSTRAINT [FK_CustomerCustomerDemo] FOREIGN KEY 
	(
		[CustomerTypeID]
	) REFERENCES [dbo].[CustomerDemographics] (
		[CustomerTypeID]
	),
	CONSTRAINT [FK_CustCustDemo_Customers] FOREIGN KEY 
	(
		[CustomerID]
	) REFERENCES [dbo].[Customers] (
		[CustomerID]
	)
GO

ALTER TABLE [dbo].[EmployeeTerritories] ADD 
	CONSTRAINT [FK_EmpTer_Employees] FOREIGN KEY 
	(
		[EmployeeID]
	) REFERENCES [dbo].[Employees] (
		[EmployeeID]
	),
	CONSTRAINT [FK_EmpTer_Ter] FOREIGN KEY 
	(
		[TerritoryID]
	) REFERENCES [dbo].[Territories] (
		[TerritoryID]
	)
GO

ALTER TABLE [dbo].[Employees] ADD 
	CONSTRAINT [FK_Employees_Employees] FOREIGN KEY 
	(
		[ReportsTo]
	) REFERENCES [dbo].[Employees] (
		[EmployeeID]
	)
GO

ALTER TABLE [dbo].[Order Details] ADD 
	CONSTRAINT [FK_Order_Details_Orders] FOREIGN KEY 
	(
		[OrderID]
	) REFERENCES [dbo].[Orders] (
		[OrderID]
	),
	CONSTRAINT [FK_Order_Details_Products] FOREIGN KEY 
	(
		[ProductID]
	) REFERENCES [dbo].[Products] (
		[ProductID]
	)
GO

ALTER TABLE [dbo].[Orders] ADD 
	CONSTRAINT [FK_Orders_Customers] FOREIGN KEY 
	(
		[CustomerID]
	) REFERENCES [dbo].[Customers] (
		[CustomerID]
	),
	CONSTRAINT [FK_Orders_Employees] FOREIGN KEY 
	(
		[EmployeeID]
	) REFERENCES [dbo].[Employees] (
		[EmployeeID]
	),
	CONSTRAINT [FK_Orders_Shippers] FOREIGN KEY 
	(
		[ShipVia]
	) REFERENCES [dbo].[Shippers] (
		[ShipperID]
	)
GO

ALTER TABLE [dbo].[Products] ADD 
	CONSTRAINT [FK_Products_Categories] FOREIGN KEY 
	(
		[CategoryID]
	) REFERENCES [dbo].[Categories] (
		[CategoryID]
	),
	CONSTRAINT [FK_Products_Suppliers] FOREIGN KEY 
	(
		[SupplierID]
	) REFERENCES [dbo].[Suppliers] (
		[SupplierID]
	)
GO

ALTER TABLE [dbo].[Territories] ADD 
	CONSTRAINT [FK_Territories_Region] FOREIGN KEY 
	(
		[RegionID]
	) REFERENCES [dbo].[Region] (
		[RegionID]
	)
GO

SET QUOTED_IDENTIFIER ON 
GO

print '------------------------------'
print 'create views - start'
print '------------------------------'
go

SET QUOTED_IDENTIFIER ON 
GO

create view "Current Product List" AS
SELECT Product_List.ProductID, Product_List.ProductName
FROM Products AS Product_List
WHERE (((Product_List.Discontinued)=0))
--ORDER BY Product_List.ProductName

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

create view "Orders Qry" AS
SELECT Orders.OrderID, Orders.CustomerID, Orders.EmployeeID, Orders.OrderDate, Orders.RequiredDate, 
	Orders.ShippedDate, Orders.ShipVia, Orders.Freight, Orders.ShipName, Orders.ShipAddress, Orders.ShipCity, 
	Orders.ShipRegion, Orders.ShipPostalCode, Orders.ShipCountry, 
	Customers.CompanyName, Customers.Address, Customers.City, Customers.Region, Customers.PostalCode, Customers.Country
FROM Customers INNER JOIN Orders ON Customers.CustomerID = Orders.CustomerID

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

create view "Products Above Average Price" AS
SELECT Products.ProductName, Products.UnitPrice
FROM Products
WHERE Products.UnitPrice>(SELECT AVG(UnitPrice) From Products)
--ORDER BY Products.UnitPrice DESC

GO
SET QUOTED_IDENTIFIER OFF 
GO


SET QUOTED_IDENTIFIER ON 
GO

create view "Products by Category" AS
SELECT Categories.CategoryName, Products.ProductName, Products.QuantityPerUnit, Products.UnitsInStock, Products.Discontinued
FROM Categories INNER JOIN Products ON Categories.CategoryID = Products.CategoryID
WHERE Products.Discontinued <> 1
--ORDER BY Categories.CategoryName, Products.ProductName

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

create view "Quarterly Orders" AS
SELECT DISTINCT Customers.CustomerID, Customers.CompanyName, Customers.City, Customers.Country
FROM Customers RIGHT JOIN Orders ON Customers.CustomerID = Orders.CustomerID
WHERE Orders.OrderDate BETWEEN '19970101' And '19971231'

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

create view Invoices AS
SELECT Orders.ShipName, Orders.ShipAddress, Orders.ShipCity, Orders.ShipRegion, Orders.ShipPostalCode, 
	Orders.ShipCountry, Orders.CustomerID, Customers.CompanyName AS CustomerName, Customers.Address, Customers.City, 
	Customers.Region, Customers.PostalCode, Customers.Country, 
	(FirstName + ' ' + LastName) AS Salesperson, 
	Orders.OrderID, Orders.OrderDate, Orders.RequiredDate, Orders.ShippedDate, Shippers.CompanyName As ShipperName, 
	"Order Details".ProductID, Products.ProductName, "Order Details".UnitPrice, "Order Details".Quantity, 
	"Order Details".Discount, 
	(CONVERT(money,("Order Details".UnitPrice*Quantity*(1-Discount)/100))*100) AS ExtendedPrice, Orders.Freight
FROM 	Shippers INNER JOIN 
		(Products INNER JOIN 
			(
				(Employees INNER JOIN 
					(Customers INNER JOIN Orders ON Customers.CustomerID = Orders.CustomerID) 
				ON Employees.EmployeeID = Orders.EmployeeID) 
			INNER JOIN "Order Details" ON Orders.OrderID = "Order Details".OrderID) 
		ON Products.ProductID = "Order Details".ProductID) 
	ON Shippers.ShipperID = Orders.ShipVia

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

create view "Order Details Extended" AS
SELECT "Order Details".OrderID, "Order Details".ProductID, Products.ProductName, 
	"Order Details".UnitPrice, "Order Details".Quantity, "Order Details".Discount, 
	(CONVERT(money,("Order Details".UnitPrice*Quantity*(1-Discount)/100))*100) AS ExtendedPrice
FROM Products INNER JOIN "Order Details" ON Products.ProductID = "Order Details".ProductID
--ORDER BY "Order Details".OrderID

GO
SET QUOTED_IDENTIFIER OFF 

SET QUOTED_IDENTIFIER ON 
GO

create view "Order Subtotals" AS
SELECT "Order Details".OrderID, Sum(CONVERT(money,("Order Details".UnitPrice*Quantity*(1-Discount)/100))*100) AS Subtotal
FROM "Order Details"
GROUP BY "Order Details".OrderID

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

create view "Product Sales for 1997" AS
SELECT Categories.CategoryName, Products.ProductName, 
Sum(CONVERT(money,("Order Details".UnitPrice*Quantity*(1-Discount)/100))*100) AS ProductSales
FROM (Categories INNER JOIN Products ON Categories.CategoryID = Products.CategoryID) 
	INNER JOIN (Orders 
		INNER JOIN "Order Details" ON Orders.OrderID = "Order Details".OrderID) 
	ON Products.ProductID = "Order Details".ProductID
WHERE (((Orders.ShippedDate) Between '19970101' And '19971231'))
GROUP BY Categories.CategoryName, Products.ProductName

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

create view "Category Sales for 1997" AS
SELECT "Product Sales for 1997".CategoryName, Sum("Product Sales for 1997".ProductSales) AS CategorySales
FROM "Product Sales for 1997"
GROUP BY "Product Sales for 1997".CategoryName

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

create view "Sales Totals by Amount" AS
SELECT "Order Subtotals".Subtotal AS SaleAmount, Orders.OrderID, Customers.CompanyName, Orders.ShippedDate
FROM 	Customers INNER JOIN 
		(Orders INNER JOIN "Order Subtotals" ON Orders.OrderID = "Order Subtotals".OrderID) 
	ON Customers.CustomerID = Orders.CustomerID
WHERE ("Order Subtotals".Subtotal >2500) AND (Orders.ShippedDate BETWEEN '19970101' And '19971231')

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

create view "Sales by Category" AS
SELECT Categories.CategoryID, Categories.CategoryName, Products.ProductName, 
	Sum("Order Details Extended".ExtendedPrice) AS ProductSales
FROM 	Categories INNER JOIN 
		(Products INNER JOIN 
			(Orders INNER JOIN "Order Details Extended" ON Orders.OrderID = "Order Details Extended".OrderID) 
		ON Products.ProductID = "Order Details Extended".ProductID) 
	ON Categories.CategoryID = Products.CategoryID
WHERE Orders.OrderDate BETWEEN '19970101' And '19971231'
GROUP BY Categories.CategoryID, Categories.CategoryName, Products.ProductName
--ORDER BY Products.ProductName

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

create view "Summary of Sales by Quarter" AS
SELECT Orders.ShippedDate, Orders.OrderID, "Order Subtotals".Subtotal
FROM Orders INNER JOIN "Order Subtotals" ON Orders.OrderID = "Order Subtotals".OrderID
WHERE Orders.ShippedDate IS NOT NULL
--ORDER BY Orders.ShippedDate

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

create view "Summary of Sales by Year" AS
SELECT Orders.ShippedDate, Orders.OrderID, "Order Subtotals".Subtotal
FROM Orders INNER JOIN "Order Subtotals" ON Orders.OrderID = "Order Subtotals".OrderID
WHERE Orders.ShippedDate IS NOT NULL
--ORDER BY Orders.ShippedDate

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

print '------------------------------'
print 'create views - finish'
print '------------------------------'
go

print '------------------------------'
print 'create procedures - start'
print '------------------------------'
go

CREATE PROCEDURE CustOrderHist @CustomerID nchar(5)
AS
SELECT ProductName, Total=SUM(Quantity)
FROM Products P, [Order Details] OD, Orders O, Customers C
WHERE C.CustomerID = @CustomerID
AND C.CustomerID = O.CustomerID AND O.OrderID = OD.OrderID AND OD.ProductID = P.ProductID
GROUP BY ProductName

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

CREATE PROCEDURE CustOrdersDetail @OrderID int
AS
SELECT ProductName,
    UnitPrice=ROUND(Od.UnitPrice, 2),
    Quantity,
    Discount=CONVERT(int, Discount * 100), 
    ExtendedPrice=ROUND(CONVERT(money, Quantity * (1 - Discount) * Od.UnitPrice), 2)
FROM Products P, [Order Details] Od
WHERE Od.ProductID = P.ProductID and Od.OrderID = @OrderID

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

CREATE PROCEDURE CustOrdersOrders @CustomerID nchar(5)
AS
SELECT OrderID, 
	OrderDate,
	RequiredDate,
	ShippedDate
FROM Orders
WHERE CustomerID = @CustomerID
ORDER BY OrderID

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

create procedure "Employee Sales by Country" 
@Beginning_Date DateTime, @Ending_Date DateTime AS
SELECT Employees.Country, Employees.LastName, Employees.FirstName, Orders.ShippedDate, Orders.OrderID, "Order Subtotals".Subtotal AS SaleAmount
FROM Employees INNER JOIN 
	(Orders INNER JOIN "Order Subtotals" ON Orders.OrderID = "Order Subtotals".OrderID) 
	ON Employees.EmployeeID = Orders.EmployeeID
WHERE Orders.ShippedDate Between @Beginning_Date And @Ending_Date

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

CREATE PROCEDURE GH_CREATETABLE
AS
Begin
     Create Table #temp_tbl (
                            Col1 int,
                            Col2 int
                            )
          --insert values to the table
         insert into #temp_tbl values (11,12)
         insert into #temp_tbl values (21,22)
         insert into #temp_tbl values (31,32)
         --execute select on the created table
         select Col1 as Value1, Col2 as Value2 from #temp_tbl
         end
GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

CREATE PROCEDURE GH_MULTIRECORDSETS
as BEGIN
    -- Declare cursor
          SELECT EmployeeID, LastName FROM Employees where EmployeeID in (1,2) order by EmployeeID asc
          SELECT CustomerID, CompanyName,ContactName FROM Customers  where CustomerID in ('MORGK','NORTS') order by CustomerID asc
            -- return empty result set
          SELECT OrderID, ShipAddress,ShipVia, ShipCity FROM Orders where OrderID=-1
END
GO

CREATE procedure GH_INOUT1
@INPARAM varchar(20) ,
@OUTPARAM  int output
AS
declare @L_INPARAM varchar(30) 
select L_INPARAM = @INPARAM
select @OUTPARAM = 100
GO


CREATE procedure GH_REFCURSOR1
AS
SELECT EmployeeID, LastName FROM Employees
WHERE EmployeeID=1
GO

CREATE procedure GH_REFCURSOR2
@IN_EMPLOYEEID int
AS
SELECT EmployeeID, LastName FROM Employees 
where EmployeeID = @IN_EMPLOYEEID
GO


CREATE procedure GH_REFCURSOR3
@IN_LASTNAME varchar(20) AS
SELECT EmployeeID, LastName FROM Employees  
where LastName = @IN_LASTNAME
GO



SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

create procedure "Sales by Year" 
	@Beginning_Date DateTime, @Ending_Date DateTime AS
SELECT Orders.ShippedDate, Orders.OrderID, "Order Subtotals".Subtotal, DATENAME(yy,ShippedDate) AS Year
FROM Orders INNER JOIN "Order Subtotals" ON Orders.OrderID = "Order Subtotals".OrderID
WHERE Orders.ShippedDate Between @Beginning_Date And @Ending_Date

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

CREATE PROCEDURE SalesByCategory
    @CategoryName nvarchar(15), @OrdYear nvarchar(4) = '1998'
AS
IF @OrdYear != '1996' AND @OrdYear != '1997' AND @OrdYear != '1998' 
BEGIN
	SELECT @OrdYear = '1998'
END
SELECT ProductName,
	TotalPurchase=ROUND(SUM(CONVERT(decimal(14,2), OD.Quantity * (1-OD.Discount) * OD.UnitPrice)), 0)
FROM [Order Details] OD, Orders O, Products P, Categories C
WHERE OD.OrderID = O.OrderID 
	AND OD.ProductID = P.ProductID 
	AND P.CategoryID = C.CategoryID
	AND C.CategoryName = @CategoryName
	AND SUBSTRING(CONVERT(nvarchar(22), O.OrderDate, 111), 1, 4) = @OrdYear
GROUP BY ProductName
ORDER BY ProductName

GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER ON 
GO

create procedure "Ten Most Expensive Products" AS
SET ROWCOUNT 10
SELECT Products.ProductName AS TenMostExpensiveProducts, Products.UnitPrice
FROM Products
ORDER BY Products.UnitPrice DESC

GO
SET QUOTED_IDENTIFIER OFF 
GO

CREATE PROCEDURE GHSP_TYPES_SIMPLE_1
@T_TINYINT tinyint,
@T_SMALLINT smallint ,
@T_INT int,
@T_DECIMAL decimal(18, 0),
@T_NUMERIC numeric(18, 0) ,
@T_FLOAT float  ,
@T_REAL  real  ,
@T_CHAR char (10),
@T_NCHAR nchar (10),
@T_VARCHAR varchar (50) ,
@T_NVARCHAR nvarchar (50)
 AS
SELECT @T_TINYINT as 'T_TINYINT', @T_SMALLINT as 'T_SMALLINT' , @T_INT as 'T_INT', @T_DECIMAL as 'T_DECIMAL',
@T_NUMERIC as 'T_NUMERIC' , @T_FLOAT as 'T_FLOAT'  , @T_REAL  as 'T_REAL'  , @T_CHAR as 'T_CHAR', @T_NCHAR as 'T_NCHAR', @T_VARCHAR as 'T_VARCHAR' , @T_NVARCHAR as 'T_NVARCHAR'
GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER OFF 
GO

CREATE PROCEDURE GHSP_TYPES_SIMPLE_2
@T_TINYINT tinyint output,
@T_SMALLINT smallint output,
@T_INT int output,
@T_DECIMAL decimal(18, 0) output,
@T_NUMERIC numeric(18, 0)  output,
@T_FLOAT float output,
@T_REAL  real output,
@T_CHAR char (10) output,
@T_NCHAR nchar (10) output,
@T_VARCHAR varchar (50) output,
@T_NVARCHAR nvarchar (50) output
 AS
SELECT @T_TINYINT = @T_TINYINT*2
SELECT @T_SMALLINT = @T_SMALLINT*2 
SELECT @T_INT = @T_INT*2
SELECT @T_DECIMAL = @T_DECIMAL*2
SELECT @T_NUMERIC = @T_NUMERIC*2 
SELECT @T_FLOAT = @T_FLOAT*2  
SELECT @T_REAL = @T_REAL*2  
SELECT @T_CHAR = UPPER(@T_CHAR)
SELECT @T_NCHAR =UPPER(@T_NCHAR)
SELECT @T_VARCHAR = UPPER(@T_VARCHAR)
SELECT @T_NVARCHAR = UPPER(@T_NVARCHAR)
GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER OFF 
GO

CREATE PROCEDURE GHSP_TYPES_SIMPLE_3
@ID char,
@T_TINYINT tinyint output,
@T_SMALLINT smallint output,
@T_INT int output,
@T_DECIMAL decimal(18, 0) output,
@T_NUMERIC numeric(18, 0)  output,
@T_FLOAT float output,
@T_REAL  real output,
@T_CHAR char (10) output,
@T_NCHAR nchar (10) output,
@T_VARCHAR varchar (50) output,
@T_NVARCHAR nvarchar (50) output
AS
SELECT @T_TINYINT = T_TINYINT, @T_SMALLINT = T_SMALLINT , @T_INT = T_INT, @T_DECIMAL = T_DECIMAL ,
@T_NUMERIC = T_NUMERIC , @T_FLOAT = T_FLOAT  , @T_REAL = T_REAL  , @T_CHAR = T_CHAR, @T_NCHAR = T_NCHAR, 
@T_VARCHAR = T_VARCHAR, @T_NVARCHAR = T_NVARCHAR FROM TYPES_SIMPLE WHERE ID = @ID
GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER OFF 
GO

CREATE PROCEDURE GHSP_TYPES_SIMPLE_4
@ID char
AS
/*Insert*/
insert into TYPES_SIMPLE(ID,T_INT) values (@ID,50)
SELECT * FROM TYPES_SIMPLE WHERE ID = @ID
/*Update*/
update TYPES_SIMPLE set T_INT=60 where ID = @ID
SELECT * FROM TYPES_SIMPLE WHERE ID = @ID
/*Delete*/
delete from TYPES_SIMPLE WHERE ID = @ID
SELECT * FROM TYPES_SIMPLE WHERE ID = @ID
GO
SET QUOTED_IDENTIFIER OFF 
GO

SET QUOTED_IDENTIFIER OFF 
GO

CREATE PROCEDURE GHSP_TYPES_SIMPLE_5
AS
DECLARE @T_TINYINT tinyint
DECLARE @T_SMALLINT smallint
DECLARE @T_INT int
DECLARE @T_DECIMAL decimal(18,0)
DECLARE @T_NUMERIC numeric(18,0)
DECLARE @T_FLOAT float
DECLARE @T_REAL real
DECLARE @T_CHAR char(10)
DECLARE @T_NCHAR nchar(10)
DECLARE @T_VARCHAR varchar(50)
DECLARE @T_NVARCHAR nvarchar(50)

SELECT @T_TINYINT = 25
SELECT @T_SMALLINT = 77
SELECT @T_INT = 2525
SELECT @T_DECIMAL = 10
SELECT @T_NUMERIC = 123123
SELECT @T_FLOAT = 17.1414257
SELECT @T_REAL = 0.71425
SELECT @T_CHAR = 'abcdefghij'
SELECT @T_NCHAR = N'klmnopqrst'
SELECT @T_VARCHAR = 'qwertasdfg'
SELECT @T_NVARCHAR = N'qwertasdfg'
 
SELECT @T_TINYINT as 'T_TINYINT', @T_SMALLINT as 'T_SMALLINT' , @T_INT as 'T_INT', @T_DECIMAL as 'T_DECIMAL', @T_NUMERIC as 'T_NUMERIC' , @T_FLOAT as 'T_FLOAT'  , @T_REAL  as 'T_REAL'  , @T_CHAR as 'T_CHAR', @T_NCHAR as 'T_NCHAR', @T_VARCHAR as 'T_VARCHAR' , @T_NVARCHAR as 'T_NVARCHAR'

GO
SET QUOTED_IDENTIFIER OFF 
GO

if not exists (select * from master.dbo.syslogins where name = N'mainsoft')
BEGIN
	declare @logindb nvarchar(132), @loginlang nvarchar(132) select @logindb = N'GHTDB', @loginlang = N'us_english'
	if @logindb is null or not exists (select * from master.dbo.sysdatabases where name = @logindb)
		select @logindb = N'master'
	if @loginlang is null or (not exists (select * from master.dbo.syslanguages where name = @loginlang) and @loginlang <> N'us_english')
		select @loginlang = @@language
	exec sp_addlogin N'mainsoft', 'mainsoft', @logindb, @loginlang
END
GO

--exec sp_addsrvrolemember N'mainsoft', sysadmin
exec sp_adduser mainsoft
GO


if not exists (select * from dbo.sysusers where name = N'mainsoft' and uid < 16382)
	EXEC sp_grantdbaccess N'mainsoft', N'mainsoft', 1
GO

SET QUOTED_IDENTIFIER ON 
GO

CREATE TABLE [mainsoft].[CategoriesNew] (
	[CategoryID] numeric(5,0) IDENTITY NOT NULL ,
	[CategoryName] [nvarchar] (15) NOT NULL ,
	[Description] [nvarchar](1000) NULL ,
	[Picture] [image] NULL 
) ON [default] 
GO

CREATE TABLE [mainsoft].[Categories] (
	[CategoryID] [nvarchar] (15) NOT NULL ,
	[CategoryName] [nvarchar] (15) NOT NULL ,
	[Description] [nvarchar](1000) NULL ,
	[Picture] [int] NULL 
) ON [default] 
GO

CREATE  procedure [mainsoft].[GH_DUMMY]
@EmployeeIdPrm  char (10)
AS
SELECT * FROM Employees where EmployeeID > CONVERT(int,@EmployeeIdPrm)
GO
SET QUOTED_IDENTIFIER OFF 
GO

------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------
------------------------------------------------------------------------------------------------------------------
use master
go

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'GHTDB_EX')
	DROP DATABASE [GHTDB_EX]
GO

if (@@maxpagesize = 1024 * 2)
	create database GHTDB_EX on default = 10
else
	create database GHTDB_EX on default = 10
go

exec sp_dboption N'GHTDB_EX', N'trunc log on chkpt', true, 1
exec sp_dboption N'GHTDB_EX', N'trunc. log', N'true', 1
exec sp_dboption N'GHTDB_EX', N'read only', N'false', 1
exec sp_dboption N'GHTDB_EX', N'dbo use', N'false', 1
go


use [GHTDB_EX]
GO

CREATE TABLE [dbo].[Customers] (
	[CustomerID] [char] (10)  NOT NULL ,
	[CompanyName] [nvarchar] (40)  NULL ,
	[ContactName] [nvarchar] (30)  NULL ,
	[ContactTitle] [nvarchar] (30)  NULL ,
	[Address] [nvarchar] (60)  NULL ,
	[City] [nvarchar] (15)  NULL ,
	[Region] [nvarchar] (15)  NULL ,
	[PostalCode] [nvarchar] (10)  NULL ,
	[Country] [nvarchar] (15)  NULL ,
	[Phone] [nvarchar] (24)  NULL ,
	[Fax] [nvarchar] (24)  NULL 
) ON [default]
GO

SET QUOTED_IDENTIFIER OFF 
GO

print '------------------------------'
print 'create another GH_DUMMY which select from a different table'
print 'customers instead of employees'
print '------------------------------'
go
CREATE  procedure GH_DUMMY
@CustomerIdPrm char (10)
AS
SELECT * FROM Customers where CustomerID=CONVERT(char,@CustomerIdPrm)
GO
SET QUOTED_IDENTIFIER OFF 
GO

