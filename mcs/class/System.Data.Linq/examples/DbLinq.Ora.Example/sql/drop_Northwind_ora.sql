DROP TRIGGER Region_Trigger;
DROP TRIGGER Categories_Trigger;
DROP TRIGGER Suppliers_Trigger;
DROP TRIGGER Products_Trigger;
DROP TRIGGER Orders_Trigger;
DROP TRIGGER Employees_Trigger;

-- Old case insensitive versions... Just in case
DROP TABLE OrderDetails;
DROP TABLE Orders;
DROP TABLE Products;
DROP TABLE Customers;
DROP TABLE EmployeeTerritories;
DROP TABLE Employees;
DROP TABLE Suppliers;
DROP TABLE Categories;
DROP TABLE Territories;
DROP TABLE Region;
-- New version
DROP TABLE "OrderDetails";
DROP TABLE "Orders";
DROP TABLE "Products";
DROP TABLE "Customers";
DROP TABLE "EmployeeTerritories";
DROP TABLE "Employees";
DROP TABLE "Suppliers";
DROP TABLE "Categories";
DROP TABLE "Territories";
DROP TABLE "Region";

DROP SEQUENCE Suppliers_seq;
DROP SEQUENCE Products_seq;
DROP SEQUENCE Orders_seq;
DROP SEQUENCE Employees_seq;
DROP SEQUENCE Categories_Seq;
DROP SEQUENCE Region_seq;
DROP SEQUENCE Territories_seq;

COMMIT;

EXIT
