using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test_NUnit;
using Test_NUnit.Linq_101_Samples;

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
    public class Select_Distinct : TestBase
    {
        [Test(Description = "select - Simple. This sample uses select to return a sequence of just the Customers' contact names.")]
        public void LinqToSqlSelect01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    select c.ContactName;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "select - Anonymous Type 1. This sample uses select and anonymous types to return a sequence of just the Customers' contact names and phone numbers.")]
        public void LinqToSqlSelect02()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    select new { c.ContactName, c.Phone };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "select - Anonymous Type 2. This sample uses select and anonymous types to return a sequence of just the Employees' names and phone numbers, with the FirstName and LastName fields combined into a single field, 'Name', and the HomePhone field renamed to Phone in the resulting sequence.")]
        public void LinqToSqlSelect03()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select new { Name = e.FirstName + " " + e.LastName, Phone = e.HomePhone };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "select - Anonymous Type 3. This sample uses select and anonymous types to return a sequence of all Products' IDs and a calculated value called HalfPrice which is set to the Product's UnitPrice divided by 2.")]
        public void LinqToSqlSelect04()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    select new { p.ProductID, HalfPrice = p.UnitPrice / 2 };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Linq101SamplesModified("c# does not have that syntax capability. Symple projection instead")]
        [Test(Description = "select - Named Type. This sample uses SELECT and a known type to return a sequence of employees' names.")]
        public void LinqToSqlSelect06()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    select new { FirstName = e.FirstName, LastName = e.LastName };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "select - Filtered. This sample uses select and where to return a sequence of just the London Customers' contact names.")]
        public void LinqToSqlSelect07()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.City == "London"
                    select c.ContactName;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "select - Shaped. This sample uses select and anonymous types to return a shaped subset of the data about Customers.")]
        public void LinqToSqlSelect08()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    select new
                    {
                        c.CustomerID,
                        CompanyInfo = new { c.CompanyName, c.City, c.Country },
                        ContactInfo = new { c.ContactName, c.ContactTitle }
                    };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "select - Nested. This sample uses nested queries to return a sequence of all orders containing their OrderID, a subsequence of the items in the order where there is a discount, and the money saved if shipping is not included.")]
        public void LinqToSqlSelect09()
        {
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    select new
                    {
                        o.OrderID,
                        DiscountedProducts = (from od in o.OrderDetails
                                              where od.Discount == 0
                                              select od),
                        FreeShippingDiscount = o.Freight
                    };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "Distinct. This sample uses Distinct to select a sequence of the unique cities that have Customers.")]
        public void LinqToSqlSelect10()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     select c.City).Distinct();

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }
    }
}
