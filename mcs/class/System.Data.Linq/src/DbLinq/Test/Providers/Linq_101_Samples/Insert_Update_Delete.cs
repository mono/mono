using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Test_NUnit;
using Test_NUnit.Linq_101_Samples;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

using nwind;

#if MYSQL
namespace Test_NUnit_MySql.Linq_101_Samples
#elif ORACLE
#if ODP
        namespace Test_NUnit_OracleODP.Linq_101_Samples
#else
        namespace Test_NUnit_Oracle.Linq_101_Samples
#endif
#elif POSTGRES
namespace Test_NUnit_PostgreSql.Linq_101_Samples
#elif SQLITE
    namespace Test_NUnit_Sqlite.Linq_101_Samples
#elif INGRES
    namespace Test_NUnit_Ingres.Linq_101_Samples
#elif MSSQL
#if MONO_STRICT
    namespace Test_NUnit_MsSql_Strict.Linq_101_Samples
#else
    namespace Test_NUnit_MsSql.Linq_101_Samples
#endif
#elif FIREBIRD
    namespace Test_NUnit_Firebird
#else
    #error unknown target
#endif
{
    [TestFixture]
    public class Insert_Update_Delete : TestBase
    {
        [Linq101SamplesModified("Console and ObjectDummper references deleted")]
        [Test(Description = "Insert - Simple. This sample uses the Add method to add a new Customer to the Customers Table object. The call to SubmitChanges persists this new Customer to the database.")]
        public void LinqToSqlInsert01()
        {
            Northwind db = CreateDB();

            Customer cust = db.Customers.FirstOrDefault(c => c.CustomerID == "MCSFT");
            if (cust != null)
            {
                try
                {
                    db.Customers.DeleteOnSubmit(cust);
                    db.SubmitChanges();
                }
                catch
                {
                    Assert.Ignore("Inconclusive: the object already exist. And the couldn't be removed");
                }
            }

            var q = from c in db.Customers
                    where c.Region == "WA"
                    select c;

            var newCustomer = new Customer
            {
                CustomerID = "MCSFT",
                CompanyName = "Microsoft",
                ContactName = "John Doe",
                ContactTitle = "Sales Manager",
                Address = "1 Microsoft Way",
                City = "Redmond",
                Region = "WA",
                PostalCode = "98052",
                Country = "USA",
                Phone = "(425) 555-1234",
                Fax = null
            };

            db.Customers.InsertOnSubmit(newCustomer);
            db.SubmitChanges();

            var reloadedCustomer = db.Customers.First(c => c.CustomerID == newCustomer.CustomerID);

            Assert.AreEqual(reloadedCustomer.CompanyName, newCustomer.CompanyName);
            Assert.AreEqual(reloadedCustomer.ContactName, newCustomer.ContactName);
            Assert.AreEqual(reloadedCustomer.ContactTitle, newCustomer.ContactTitle);
            Assert.AreEqual(reloadedCustomer.Address, newCustomer.Address);
            Assert.AreEqual(reloadedCustomer.City, newCustomer.City);
            Assert.AreEqual(reloadedCustomer.Region, newCustomer.Region);
            Assert.AreEqual(reloadedCustomer.PostalCode, newCustomer.PostalCode);
            Assert.AreEqual(reloadedCustomer.Country, newCustomer.Country);
            Assert.AreEqual(reloadedCustomer.Phone, newCustomer.Phone);
            Assert.AreEqual(reloadedCustomer.Fax, newCustomer.Fax);
        }

#if !SQLITE
        [Linq101SamplesModified("Console and ObjectDummper references deleted")]
        [Linq101SamplesModified("The original sample didn't compile, db2 Northwind context was used for nothing")]
        [Test(Description = "Insert - 1-to-Many. This sample uses the Add method to add a new Category to the Categories table object, and a new Product to the Products Table object with a foreign key relationship to the new Category. The call to SubmitChanges persists these new objects and their relationships to the database.")]
        public void LinqToSqlInsert02()
        {
            Northwind db = CreateDB();

            var ds = new DataLoadOptions();

            ds.LoadWith<Category>(c => c.Products);
            db.LoadOptions = ds;

            var q = from c in db.Categories
                    where c.CategoryName == "Widgets"
                    select c;

            var newCategory = new Category
                                {
                                    CategoryName = "Widgets",
                                    Description = "Widgets are the customer-facing analogues to sprockets and cogs."
                                };

            var newProduct = new Product
            {
                ProductName = "Blue Widget",
                UnitPrice = 34.56m,
                Category = newCategory
            };
            db.Categories.InsertOnSubmit(newCategory);
            db.SubmitChanges();

            var reloadedProduct = db.Products.First(p => p.ProductID == newProduct.ProductID);

            Assert.AreEqual(reloadedProduct.ProductName, newProduct.ProductName);
            Assert.AreEqual(reloadedProduct.UnitPrice, newProduct.UnitPrice);
            Assert.AreEqual(reloadedProduct.Category.CategoryID, newProduct.CategoryID);

            var reloadedCategory = reloadedProduct.Category;

            Assert.AreEqual(reloadedCategory.CategoryName, newCategory.CategoryName);
            Assert.AreEqual(reloadedCategory.Description, reloadedCategory.Description);
        }
#endif

        [Linq101SamplesModified("Console and ObjectDummper references deleted")]
        [Linq101SamplesModified("The original sample didn't compile, db2 Northwind context was used for nothing")]
        [Test(Description = "Insert - Many-to-Many. This sample uses the Add method to add a new Employee to the Employees table object, a new Territory to the Territories table object, and a new EmployeeTerritory to the EmployeeTerritories table object with foreign key relationships to the new Employee and Territory. The call to SubmitChanges persists these new objects and their relationships to the database.")]
        public void LinqToSqlInsert03()
        {
            Northwind db = CreateDB();

            var ds = new DataLoadOptions();
            ds.LoadWith<Employee>(p => p.EmployeeTerritories);
            ds.LoadWith<EmployeeTerritory>(p => p.Territory);

            db.LoadOptions = ds;
            var q = from e in db.Employees where e.FirstName == "Nancy" select e;


            if (db.Employees.Any(e => e.FirstName == "Kira" && e.LastName == "Smith"))
                Assert.Ignore();


            var newEmployee = new Employee { FirstName = "Kira", LastName = "Smith" };
            var newTerritory = new Territory
            {
                TerritoryID = "12345",
                TerritoryDescription = "Anytown",
                Region = db.Regions.First()
            };

            var newEmployeeTerritory = new EmployeeTerritory { Employee = newEmployee, Territory = newTerritory };
            db.Employees.InsertOnSubmit(newEmployee);
            db.Territories.InsertOnSubmit(newTerritory);
            db.EmployeeTerritories.InsertOnSubmit(newEmployeeTerritory);
            db.SubmitChanges();
        }

        [Linq101SamplesModified("Console and ObjectDummper references deleted")]
        [Test(Description = "Update - Simple. This sample uses SubmitChanges to persist an update made to a retrieved Customer object back to the database.")]
        public void LinqToSqlInsert04()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.CustomerID == "ALFKI"
                    select c;

            Customer cust = (from c in db.Customers
                             where c.CustomerID == "ALFKI"
                             select c).First();

            cust.ContactTitle = "Vice President";
            db.SubmitChanges();

            Customer reloadedCustomer = db.Customers.First(c => c.CustomerID == cust.CustomerID);
            Assert.AreEqual(reloadedCustomer.ContactTitle, cust.ContactTitle);
        }

        [Linq101SamplesModified("Console and ObjectDummper references deleted")]
        [Test(Description = "Update - Multiple. This sample uses SubmitChanges to persist updates made to multiple retrieved Product objects back to the database.")]
        public void LinqToSqlInsert05()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    where p.CategoryID.Value == 1
                    select p;

            foreach (var p in q)
                p.UnitPrice += 1.0m;

            db.SubmitChanges();


            var reloadedProducts = db.Products.Where(p => p.CategoryID.Value == 1);
            IEnumerator<Product> original = q.GetEnumerator();
            IEnumerator<Product> reloaded = reloadedProducts.GetEnumerator();

            while (original.MoveNext() && reloaded.MoveNext())
                Assert.AreEqual(original.Current.UnitPrice, reloaded.Current.UnitPrice);

            Assert.AreEqual(original.MoveNext(), reloaded.MoveNext());
        }



        [Linq101SamplesModified("Console and ObjectDummper references deleted")]
        [Test(Description = "Delete - Simple. This sample uses the Remove method to delete an OrderDetail from the OrderDetails Table object. The call to SubmitChanges persists this deletion to the database.")]
        public void LinqToSqlInsert06()
        {
            Northwind db = CreateDB();

            OrderDetail ode = db.OrderDetails.First();
            decimal orderID = ode.OrderID;
            decimal productID = ode.ProductID;


            OrderDetail order = (from c in db.OrderDetails
                                 where c.OrderID == orderID && c.ProductID == productID
                                 select c).First();

            //what happened to Table.Remove()?
            //The Add and AddAll methods are now InsertOnSubmit and InsertAllOnSubmit. The Remove and RemoveAll are now DeleteOnSubmit and DeleteAllOnSubmit.
            //http://blogs.vertigo.com/personal/petar/Blog/Lists/Posts/Post.aspx?List=9441ab3e%2Df290%2D4a5b%2Da591%2D49a8226de525&ID=3

            db.OrderDetails.DeleteOnSubmit(order); //formerly Remove(order);
            db.SubmitChanges();

            Assert.IsFalse(db.OrderDetails.Any(od => od.OrderID == orderID && od.ProductID == productID));

            try
            {
                db.OrderDetails.InsertOnSubmit(order);
                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                Assert.Ignore("the orderDetail deleted hasn't be restored, so next run over this text will fail: " + ex.Message);
            }
        }

        [SetUp]
        public void Setup_LinqToSqlInsert07()
        {
            //Northwind db = CreateDB();
            //var o = new Order { CustomerID = "WARTH", Employee = db.Employees.First() };
            //o.OrderDetails.Add(new OrderDetail { Discount = 0.1f, Quantity = 1, Product = db.Products.First(p => p.Discontinued) });
            //o.OrderDetails.Add(new OrderDetail { Discount = 0.2f, Quantity = 1, Product = db.Products.First(p => !p.Discontinued) });
            //db.Orders.InsertOnSubmit(o);
            //db.SubmitChanges();
        }

        [Linq101SamplesModified("Console and ObjectDummper references deleted")]
        [Test(Description = "Delete - One-to-Many. This sample uses the Remove method to delete an Order and Order Detail from the Order Details and Orders tables. First deleting Order Details and then deleting from Orders. The call to SubmitChanges persists this deletion to the database.")]
        public void LinqToSqlInsert07()
        {
            Northwind db = CreateDB();
           
                var orderDetails =
                    from o in db.OrderDetails
                    where o.Order.CustomerID == "WARTH" 
                    select o;

                var order =
                    (from o in db.Orders
                     where o.CustomerID == "WARTH" 
                     select o).FirstOrDefault();

                if (!orderDetails.Any() || order == null)
                    Assert.Ignore("Preconditions");
                

                foreach (var od in orderDetails)
                {
                    db.OrderDetails.DeleteOnSubmit(od); //formerly Remove(od);
                }

                db.Orders.DeleteOnSubmit(order); //formerly Remove(order);
                db.SubmitChanges();

                Assert.IsFalse(
                    db.OrderDetails.Any(od => od.Order.Customer.CustomerID == "WARTH" && od.Order.EmployeeID == 3));
                Assert.IsFalse(db.Orders.Any(ord => ord.OrderID == order.OrderID));
       
        }
    }
}
