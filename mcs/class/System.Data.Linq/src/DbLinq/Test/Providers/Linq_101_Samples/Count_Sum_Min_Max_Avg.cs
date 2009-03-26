using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test_NUnit;

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
    namespace Test_NUnit_Firebird.Linq_101_Samples
#else
    #error unknown target
#endif
{
    /// <summary>
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737922.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class Count_Sum_Min_Max_Avg : TestBase
    {
        [Test]
        public void LinqToSqlCount01()
        {
            Northwind db = CreateDB();
            var q = db.Customers.Count();

            Assert.IsTrue(q > 0, "Expect non-zero count");
        }

        [Test]
        public void LinqToSqlCount02()
        {
            Northwind db = CreateDB();
#if INGRES && !MONO_STRICT
            var q = (from p in db.Products where p.Discontinued == 0 select p)
                .Count();
#else
            var q = (from p in db.Products where !p.Discontinued select p)
                .Count();
#endif

            Assert.IsTrue(q > 0, "Expect non-zero count");
        }

        [Test(Description = "This sample uses Sum to find the total freight over all Orders.")]
        public void LinqToSqlCount03()
        {
            Northwind db = CreateDB();
            var q = (from o in db.Orders select o.Freight).Sum();
            Assert.IsTrue(q > 0, "Freight sum must be > 0");
        }

        [Test(Description = "This sample uses Sum to find the total number of units on order over all Products.")]
        public void LinqToSqlCount04()
        {
            Northwind db = CreateDB();
            var q = (from p in db.Products select (int)p.UnitsOnOrder.Value).Sum();
            Assert.IsTrue(q > 0, "Freight sum must be > 0");
        }

        [Test(Description = "This sample uses Min to find the lowest unit price of any Product")]
        public void LinqToSqlCount05()
        {
            Northwind db = CreateDB();
            var q = (from p in db.Products select p.UnitsOnOrder).Min();
            Assert.IsTrue(q == 0, "Min UnitsOnOrder must be 0");
        }

        [Test(Description = "This sample uses Min to find the lowest freight of any Order.")]
        public void LinqToSqlCount06()
        {
            Northwind db = CreateDB();
            var q = (from o in db.Orders select o.Freight).Min();
            Assert.IsTrue(q > 0, "Freight sum must be > 0");
        }

        [Test(Description = "This sample uses Min to find the Products that have the lowest unit price in each category")]
        public void LinqToSqlCount07()
        {
            #region SHOW_MICROSOFT_GENERATED_SQL
            /*
            //the one Linq statement below gets translated into 9 SQL statements
SELECT MIN([t0].[UnitPrice]) AS [value], [t0].[CategoryID]
FROM [dbo].[Products] AS [t0]
GROUP BY [t0].[CategoryID]

SELECT [t0].[ProductID], [t0].[ProductName], [t0].[SupplierID], [t0].[CategoryID], [t0].[QuantityPerUnit], [t0].[UnitPrice], [t0].[UnitsInStock], [t0].[UnitsOnOrder], [t0].[ReorderLevel], [t0].[Discontinued]
FROM [dbo].[Products] AS [t0]
WHERE ([t0].[UnitPrice] = @x2) AND (((@x1 IS NULL) AND ([t0].[CategoryID] IS NULL)) OR ((@x1 IS NOT NULL) AND ([t0].[CategoryID] IS NOT NULL) AND (@x1 = [t0].[CategoryID])))
-- @x1: Input Int (Size = 0; Prec = 0; Scale = 0) [1]
-- @x2: Input Money (Size = 0; Prec = 19; Scale = 4) [4.5000]

SELECT [t0].[ProductID], [t0].[ProductName], [t0].[SupplierID], [t0].[CategoryID], [t0].[QuantityPerUnit], [t0].[UnitPrice], [t0].[UnitsInStock], [t0].[UnitsOnOrder], [t0].[ReorderLevel], [t0].[Discontinued]
FROM [dbo].[Products] AS [t0]
WHERE ([t0].[UnitPrice] = @x2) AND (((@x1 IS NULL) AND ([t0].[CategoryID] IS NULL)) OR ((@x1 IS NOT NULL) AND ([t0].[CategoryID] IS NOT NULL) AND (@x1 = [t0].[CategoryID])))
-- @x1: Input Int (Size = 0; Prec = 0; Scale = 0) [2]
-- @x2: Input Money (Size = 0; Prec = 19; Scale = 4) [10.0000]

SELECT [t0].[ProductID], [t0].[ProductName], [t0].[SupplierID], [t0].[CategoryID], [t0].[QuantityPerUnit], [t0].[UnitPrice], [t0].[UnitsInStock], [t0].[UnitsOnOrder], [t0].[ReorderLevel], [t0].[Discontinued]
FROM [dbo].[Products] AS [t0]
WHERE ([t0].[UnitPrice] = @x2) AND (((@x1 IS NULL) AND ([t0].[CategoryID] IS NULL)) OR ((@x1 IS NOT NULL) AND ([t0].[CategoryID] IS NOT NULL) AND (@x1 = [t0].[CategoryID])))
-- @x1: Input Int (Size = 0; Prec = 0; Scale = 0) [3]
-- @x2: Input Money (Size = 0; Prec = 19; Scale = 4) [9.2000]

SELECT [t0].[ProductID], [t0].[ProductName], [t0].[SupplierID], [t0].[CategoryID], [t0].[QuantityPerUnit], [t0].[UnitPrice], [t0].[UnitsInStock], [t0].[UnitsOnOrder], [t0].[ReorderLevel], [t0].[Discontinued]
FROM [dbo].[Products] AS [t0]
WHERE ([t0].[UnitPrice] = @x2) AND (((@x1 IS NULL) AND ([t0].[CategoryID] IS NULL)) OR ((@x1 IS NOT NULL) AND ([t0].[CategoryID] IS NOT NULL) AND (@x1 = [t0].[CategoryID])))
-- @x1: Input Int (Size = 0; Prec = 0; Scale = 0) [4]
-- @x2: Input Money (Size = 0; Prec = 19; Scale = 4) [2.5000]

SELECT [t0].[ProductID], [t0].[ProductName], [t0].[SupplierID], [t0].[CategoryID], [t0].[QuantityPerUnit], [t0].[UnitPrice], [t0].[UnitsInStock], [t0].[UnitsOnOrder], [t0].[ReorderLevel], [t0].[Discontinued]
FROM [dbo].[Products] AS [t0]
WHERE ([t0].[UnitPrice] = @x2) AND (((@x1 IS NULL) AND ([t0].[CategoryID] IS NULL)) OR ((@x1 IS NOT NULL) AND ([t0].[CategoryID] IS NOT NULL) AND (@x1 = [t0].[CategoryID])))
-- @x1: Input Int (Size = 0; Prec = 0; Scale = 0) [5]
-- @x2: Input Money (Size = 0; Prec = 19; Scale = 4) [7.0000]

SELECT [t0].[ProductID], [t0].[ProductName], [t0].[SupplierID], [t0].[CategoryID], [t0].[QuantityPerUnit], [t0].[UnitPrice], [t0].[UnitsInStock], [t0].[UnitsOnOrder], [t0].[ReorderLevel], [t0].[Discontinued]
FROM [dbo].[Products] AS [t0]
WHERE ([t0].[UnitPrice] = @x2) AND (((@x1 IS NULL) AND ([t0].[CategoryID] IS NULL)) OR ((@x1 IS NOT NULL) AND ([t0].[CategoryID] IS NOT NULL) AND (@x1 = [t0].[CategoryID])))
-- @x1: Input Int (Size = 0; Prec = 0; Scale = 0) [6]
-- @x2: Input Money (Size = 0; Prec = 19; Scale = 4) [7.4500]

SELECT [t0].[ProductID], [t0].[ProductName], [t0].[SupplierID], [t0].[CategoryID], [t0].[QuantityPerUnit], [t0].[UnitPrice], [t0].[UnitsInStock], [t0].[UnitsOnOrder], [t0].[ReorderLevel], [t0].[Discontinued]
FROM [dbo].[Products] AS [t0]
WHERE ([t0].[UnitPrice] = @x2) AND (((@x1 IS NULL) AND ([t0].[CategoryID] IS NULL)) OR ((@x1 IS NOT NULL) AND ([t0].[CategoryID] IS NOT NULL) AND (@x1 = [t0].[CategoryID])))
-- @x1: Input Int (Size = 0; Prec = 0; Scale = 0) [7]
-- @x2: Input Money (Size = 0; Prec = 19; Scale = 4) [10.0000]

SELECT [t0].[ProductID], [t0].[ProductName], [t0].[SupplierID], [t0].[CategoryID], [t0].[QuantityPerUnit], [t0].[UnitPrice], [t0].[UnitsInStock], [t0].[UnitsOnOrder], [t0].[ReorderLevel], [t0].[Discontinued]
FROM [dbo].[Products] AS [t0]
WHERE ([t0].[UnitPrice] = @x2) AND (((@x1 IS NULL) AND ([t0].[CategoryID] IS NULL)) OR ((@x1 IS NOT NULL) AND ([t0].[CategoryID] IS NOT NULL) AND (@x1 = [t0].[CategoryID])))
-- @x1: Input Int (Size = 0; Prec = 0; Scale = 0) [8]
-- @x2: Input Money (Size = 0; Prec = 19; Scale = 4) [6.0000]
    */
            #endregion

            Northwind db = CreateDB();
            var categories = (from p in db.Products
                              group p by p.CategoryID into g
                              select new
                              {
                                  CategoryID = g.Key,
                                  CheapestProducts = from p2 in g
                                                     where p2.UnitPrice == g.Min(p3 => p3.UnitPrice)
                                                     select p2
                              });

            var list = categories.ToList();
            Assert.IsTrue(list.Count > 0, "Expected count > 0");
        }

        [Test(Description = "This sample uses Max to find the latest hire date of any Employee")]
        public void LinqToSqlCount08()
        {
            Northwind db = CreateDB();
            var q = (from e in db.Employees select e.HireDate).Max();
            Assert.IsTrue(q > new DateTime(1990, 1, 1), "Hire date must be > 2000");
        }

        [Test(Description = "This sample uses Max to find the most units in stock of any Product")]
        public void LinqToSqlCount09()
        {
            Northwind db = CreateDB();
            var q = (from p in db.Products select p.UnitsInStock).Max();
            Assert.IsTrue(q > 0, "Max UnitsInStock must be > 0");
        }

        [Test(Description = "This sample uses Max to find the Products that have the highest unit price in each category")]
        public void LinqToSqlCount10()
        {
            //Miscrosoft translates this query into multiple SQL statements
            Northwind db = CreateDB();
            var q = from p in db.Products
                    group p by p.CategoryID into g
                    select new
                    {
                        g,
                        MostExpensiveProducts = from p2 in g
                                                where p2.UnitPrice == g.Max(p3 => p3.UnitPrice)
                                                select p2
                    };
            var list = q.ToList();
            Assert.IsTrue(list.Count > 0, "Got most expensive items > 0");
        }



        [Test(Description = "This sample uses Average to find the average freight of all Orders.")]
        public void LinqToSqlCount11()
        {
            Northwind db = CreateDB();
            var q = (from o in db.Orders
                     select o.Freight).Average();

            Console.WriteLine(q);
            Assert.IsTrue(q > 0, "Avg orders'freight must be > 0");
        }

        [Test(Description = "This sample uses Average to find the average unit price of all Products.")]
        public void LinqToSqlCount12()
        {
            Northwind db = CreateDB();
            var q = (from p in db.Products
                     select p.UnitPrice).Average();

            Console.WriteLine(q);

            Console.WriteLine(q);
            Assert.IsTrue(q > 0, "Avg products'unitPrice must be > 0");
        }


        [Test(Description = "This sample uses Average to find the Products that have unit price higher than the average unit price of the category for each category.")]
        public void LinqToSqlCount13()
        {
            Northwind db = CreateDB();
            var categories = from p in db.Products
                             group p by p.CategoryID into g
                             select new
                                {
                                    g,
                                    ExpensiveProducts = from p2 in g
                                                        where (p2.UnitPrice > g.Average(p3 => p3.UnitPrice))
                                                        select p2
                                };


            var list = categories.ToList();
            Assert.IsTrue(list.Count > 0, "Got categorized products > 0");
        }



    }
}
