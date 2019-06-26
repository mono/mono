using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;

using nwind;

// test ns Linq_101_Samples
#if MYSQL
    namespace Test_NUnit_MySql.Linq_101_Samples
#elif ORACLE && ODP
    namespace Test_NUnit_OracleODP.Linq_101_Samples
#elif ORACLE
    namespace Test_NUnit_Oracle.Linq_101_Samples
#elif POSTGRES
    namespace Test_NUnit_PostgreSql.Linq_101_Samples
#elif SQLITE
    namespace Test_NUnit_Sqlite.Linq_101_Samples
#elif INGRES
    namespace Test_NUnit_Ingres.Linq_101_Samples
#elif MSSQL && L2SQL
    namespace Test_NUnit_MsSql_Strict.Linq_101_Samples
#elif MSSQL
    namespace Test_NUnit_MsSql.Linq_101_Samples
#elif FIREBIRD
    namespace Test_NUnit_Firebird.Linq_101_Samples
#endif
{
    [TestFixture]
    public class OrderBy : TestBase
    {
        [Test(Description = "OrderBy - Simple. This sample uses orderby to sort Employees by hire date.")]
        public void LinqToSqlOrderBy01()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    orderby e.HireDate
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "OrderBy - With where. This sample uses where and orderby to sort Orders shipped to London by freight.")]
        public void LinqToSqlOrderBy02()
        {
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    where o.ShipCity == "Marseille"
                    orderby o.Freight
                    select o;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "OrderByDescending. This sample uses orderby to sort Products by unit price from highest to lowest.")]
        public void LinqToSqlOrderBy03()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    orderby p.UnitPrice descending
                    select p;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "ThenBy. This sample uses a compound orderby to sort Customers by city and then contact name.")]
        public void LinqToSqlOrderBy04()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    orderby c.City, c.ContactName
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "ThenByDescending. This sample uses orderby to sort Orders from EmployeeID 1 by ship-to country, and then by freight from highest to lowest.")]
        public void LinqToSqlOrderBy05()
        {
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    where o.EmployeeID == 1
                    orderby o.ShipCountry, o.Freight descending
                    select o;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test(Description = "OrderBy - Group by. This sample uses Orderby, Max and Group by to find the Products that have the highest unit price in each category, and sorts the group by category id.")]
        public void LinqToSqlOrderBy06()
        {
            Northwind db = CreateDB();

            var categories = from p in db.Products
                             orderby p.CategoryID
                             group p by p.CategoryID into Group
                             select new
                             {
                                 Group,
                                 MostExpensiveProducts =
                                     from p2 in Group
                                     where p2.UnitPrice == Group.Max(p3 => p3.UnitPrice)
                                     select p2
                             };

            var list = categories.ToList();
            Assert.IsTrue(list.Count > 0);
        }
    }
}
