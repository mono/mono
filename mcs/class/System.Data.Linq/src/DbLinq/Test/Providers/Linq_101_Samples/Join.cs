using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test_NUnit;

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
    /// <summary>
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737929.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class Join : TestBase
    {

        [Test(Description = "This sample uses foreign key navigation in the from clause to select all orders for customers in London")]
        public void LinqToSqlJoin01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    from o in c.Orders
                    where c.City == "London"
                    select o;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0, "No rows returned");
            Assert.IsTrue(list[0].CustomerID != null, "Missing CustomerID");
        }

        [Test(Description = "This sample uses foreign key navigation in the from clause to select all orders for customers in London")]
        public void LinqToSqlJoin01_b()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    from o in c.Orders
                    where c.City == "London"
                    select new { o.CustomerID, o.OrderID };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "This sample uses foreign key navigation in the where clause to filter for Products whose Supplier is in the USA that are out of stock")]
        public void LinqToSqlJoin02()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    where p.Supplier.Country == "USA" && p.UnitsInStock == 0
                    select p;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "This sample uses foreign key navigation in the from clause to filter for employees in Seattle, and also list their territories")]
        public void LinqToSqlJoin03()
        {
            //Logger.Write(Level.Information, "\nLinq.Join03()");
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    from et in e.EmployeeTerritories
                    where e.City == "Seattle"
                    select new { e.FirstName, e.LastName, et.Territory.TerritoryDescription };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "SelectMany - Self-Join.  filter for pairs of employees where one employee reports to the other and where both employees are from the same City")]
        public void LinqToSqlJoin04()
        {
            //Logger.Write(Level.Information, "\nLinq.Join04()");
            Northwind db = CreateDB();

            var q = from e1 in db.Employees
                    from e2 in e1.Employees
                    where e1.City == e2.City
                    select new
                    {
                        FirstName1 = e1.FirstName,
                        LastName1 = e1.LastName,
                        FirstName2 = e2.FirstName,
                        LastName2 = e2.LastName,
                        e1.City
                    };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
            foreach (var v in list)
            {
                Assert.IsTrue(v.LastName1 != v.LastName2, "Last names must be different");
            }
        }

        [Test(Description = "GroupJoin - Two-way join. This sample explictly joins two tables and projects results from both tables.")]
        public void LinqToSqlJoin05()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    join o in db.Orders on c.CustomerID equals o.CustomerID into orders
                    select new { c.ContactName, OrderCount = orders.Count() };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);

        }

        [Test(Description = "GroupJoin - Three-way join. This sample explictly joins three tables and projects results from each of them.")]
        public void LinqToSqlJoin06()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    join o in db.Orders on c.CustomerID equals o.CustomerID into ords
                    join e in db.Employees on c.City equals e.City into emps
                    select new { c.ContactName, ords = ords.Count(), emps = emps.Count() };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test(Description = "GroupJoin - LEFT OUTER JOIN. This sample shows how to get LEFT OUTER JOIN by using DefaultIfEmpty(). The DefaultIfEmpty() method returns null when there is no Order for the Employee.")]
        public void LinqToSqlJoin07()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    join o in db.Orders on e equals o.Employee into ords
                    from o in ords.DefaultIfEmpty()
                    select new { e.FirstName, e.LastName, Order = o };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test(Description = "GroupJoin - Projected let assignment. This sample projects a 'let' expression resulting from a join.")]
        public void LinqToSqlJoin08()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    join o in db.Orders on c.CustomerID equals o.CustomerID into ords
                    let z = c.City + c.Country
                    from o in ords
                    select new { c.ContactName, o.OrderID, z };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test(Description = "GroupJoin - Composite Key.This sample shows a join with a composite key.")]
        public void LinqToSqlJoin09()
        {
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    from p in db.Products
                    join d in db.OrderDetails
                        on new { o.OrderID, p.ProductID }
                        equals new { d.OrderID, d.ProductID }
                    into details
                    from d in details
                    select new { o.OrderID, p.ProductID, d.UnitPrice };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        /// <summary>
        /// This sample shows how to construct a join where one side is nullable and the other isn't.
        /// </summary>
        [Test(Description = "GroupJoin - Nullable\\Nonnullable Key Relationship")]
        public void LinqToSqlJoin10()
        {
            //Microsoft Linq-to-SQL generated statement that we want to match:
            //SELECT [t0].[OrderID], [t1].[FirstName]
            //FROM [dbo].[Orders] AS [t0], [dbo].[Employees] AS [t1]
            //WHERE [t0].[EmployeeID] = ([t1].[EmployeeID])

            Northwind db = CreateDB();

            var q = from o in db.Orders
                    join e in db.Employees on o.EmployeeID equals e.EmployeeID into emps
                    from e in emps
                    select new { o.OrderID, e.FirstName };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }
    }
}
