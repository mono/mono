using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
using Test_NUnit.Linq_101_Samples;

using nwind;

#if MYSQL
namespace Test_NUnit_MySql
#elif ORACLE
#if ODP
namespace Test_NUnit_OracleODP
#else
        namespace Test_NUnit_Oracle
#endif
#elif POSTGRES
    namespace Test_NUnit_PostgreSql
#elif SQLITE
    namespace Test_NUnit_Sqlite
#elif INGRES
    namespace Test_NUnit_Ingres
#elif MSSQL
#if MONO_STRICT
namespace Test_NUnit_MsSql_Strict
#else
namespace Test_NUnit_MsSql
#endif
#elif FIREBIRD
    namespace Test_NUnit_Firebird
#else
    #error unknown target
#endif
{
    [TestFixture]
    public class AnyCount : TestBase
    {
        [Test]
        public void AnyInternal01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where !c.Orders.Any()
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void AnyInternal02()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where !c.Orders.Any(o => o.Customer.ContactName == "WARTH")
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }
        [Test]
        public void AnyInternal03()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where !c.Orders.Where(o => o.Customer.ContactName == "WARTH")
                                     .Any(o => o.Customer.Country == "USA")
                     select c).ToList();
        }

        [Test]
        public void AnyInternal04()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where !c.Orders.Select(o => o.Customer.Country)
                                     .Any(ct => ct == "USA")
                     select c).ToList();
        }

        [Test]
        public void AnyInternal05()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     select new { c.CustomerID, HasUSAOrders = c.Orders.Any(o => o.ShipCountry == "USA") }).ToList();
        }


        [Test]
        public void AnyExternal01()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where c.Country == "USA"
                     select c).Any();

        }

        [Test]
        public void AnyExternal02()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where c.Country == "USA"
                     select c).Any(cust => cust.City == "Seatle");
        }

        [Test]
        public void AnyExternal03()
        {
            Northwind db = CreateDB();
            var q = db.Customers.Any();
        }

        [Test]
        public void AnyExternal04()
        {
            Northwind db = CreateDB();
            var q = db.Customers.Any(cust => cust.City == "Seatle");
        }


        [Test]
        public void CountInternal01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.Orders.Count() % 2 == 0
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void CountInternal02()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.Orders.Count(o => o.Customer.ContactName == "WARTH") % 2 == 0
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }


        [Test]
        public void CountInternal03()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where c.Orders.Where(o => o.Customer.ContactName == "WARTH")
                                     .Count(o => o.Customer.Country == "USA") % 2 == 0
                     select c).ToList();
        }

        [Test]
        public void CountInternal04()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where c.Orders.Select(o => o.Customer.Country)
                                     .Count(ct => ct == "USA") % 2 == 0
                     select c).ToList();
        }


        [Test]
        public void CountExternal01()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where c.Country == "USA"
                     select c).Count();
        }

        [Test]
        public void CountExternal02()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where c.Country == "USA"
                     select c).Count(cust => cust.City == "Seatle");
        }

        [Test]
        public void CountExternal03()
        {
            Northwind db = CreateDB();
            var q = db.Customers.Count();
        }

        [Test]
        public void CountExternal04()
        {
            Northwind db = CreateDB();
            var q = db.Customers.Count(cust => cust.City == "Seatle");
        }

        [Test]
        public void CountInternal05()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     select new { c.CustomerID, HasUSAOrders = c.Orders.Count(o => o.ShipCountry == "USA") }).ToList();
        }

        [Test]
        public void FirstInternal01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.Orders.FirstOrDefault() != null
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void FirstInternal02()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.Orders.FirstOrDefault(o => o.Customer.ContactName == "WARTH") != null
                    select c;

            var list = q.ToList();
        }

        [Test]
        public void FirstInternal03()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where c.Orders.Where(o => o.Customer.ContactName == "WARTH")
                                     .FirstOrDefault(o => o.Customer.Country == "USA") != null
                     select c).ToList();
        }

        [Test]
        public void FirstInternal04()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where c.Orders.Select(o => o.Customer.Country)
                                   .FirstOrDefault(ct => ct == "USA") != null
                     select c).ToList();
        }

        [Test]
        public void FirstExternal01()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where c.Country == "USA"
                     select c).First();
        }

        [Test]
        public void FirstExternal02()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     where c.Country == "France"
                     select c).First(cust => cust.City == "Marseille");

        }

        [Test]
        public void FirstExternal03()
        {
            Northwind db = CreateDB();
            var q = db.Customers.First();
        }

        [Test]
        public void FirstExternal04()
        {
            Northwind db = CreateDB();
            var q = db.Customers.First(cust => cust.City == "Marseille");
        }

        [Test]
        public void FirstInternal05()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     select new { c.CustomerID, FirstUSAOrder = c.Orders.First(o => o.ShipCountry == "France") }).ToList();
        }

        [Test]
        public void ArrayContains()
        {
            var db = CreateDB();
            decimal[] d = new decimal[] { 1, 4, 5, 6 };
            var q = db.OrderDetails.Where(o => d.Contains(o.OrderID));

            Assert.Greater(q.Count(), 0);
        }

    }
}
