using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
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
    namespace Test_NUnit_Firebird.Linq_101_Samples
#else
    #error unknown target
#endif
{
    /// <summary>
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737930.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class GroupBy_Having : TestBase
    {
        [Test(Description = "GroupBy - Simple. This sample uses group by to partition Products by CategoryID.")]
        public void LinqToSqlGroupBy01()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    group p by p.CategoryID into g
                    select g;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }



        [Test(Description = "GroupBy - Max. This sample uses group by and Max to find the maximum unit price for each CategoryID.")]
        public void LinqToSqlGroupBy02()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    group p by p.CategoryID into g
                    select new { g, MaxPrice = g.Max(p => p.UnitPrice) };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "GroupBy - Min. This sample uses group by and Min to find the minimum unit price for each CategoryID.")]
        public void LinqToSqlGroupBy03()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    group p by p.CategoryID into g
                    select new { g, MinPrice = g.Min(p => p.UnitPrice) };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "GroupBy - Average. This sample uses group by and Average to find the average UnitPrice for each CategoryID.")]
        public void LinqToSqlGroupBy04()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    group p by p.CategoryID into g
                    select new { g, AveragePrice = g.Average(p => p.UnitPrice) };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }



        [Test(Description = "GroupBy - Sum. This sample uses group by and Sum to find the total UnitPrice for each CategoryID.")]
        public void LinqToSqlGroupBy05()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    group p by p.CategoryID into g
                    select new { g, TotalPrice = g.Sum(p => p.UnitPrice) };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "GroupBy - Count. This sample uses group by and Count to find the number of Products in each CategoryID.")]
        public void LinqToSqlGroupBy06()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    group p by p.CategoryID into g
                    select new { g, NumProducts = g.Count() };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Linq101SamplesModified("Strange short to boolean casting, perhaps in the original Northwind Product.Discontinued was a boolean property")]
        [Test(Description = "GroupBy - Count - Conditional. This sample uses group by and Count to find the number of Products in each CategoryID that are discontinued.")]
        public void LinqToSqlGroupBy07()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    group p by p.CategoryID into g
                    select new { g, NumProducts = g.Count(p => Convert.ToBoolean(p.Discontinued)) };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }




        [Test(Description = "GroupBy - followed by where. This sample uses a where clause after a group by clause to find all categories that have at least 10 products.")]
        public void LinqToSqlGroupBy08()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    group p by p.CategoryID into g
                    where g.Count() >= 10
                    select new { g, ProductCount = g.Count() };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }




        [Linq101SamplesModified("Strange syntactical strategy. Everybody aggree with this traduction?")]
        [Test(Description = "GroupBy - Multiple Columns. This sample uses group by to group products by CategoryID and SupplierID.")]
        public void LinqToSqlGroupBy09()
        {
            Northwind db = CreateDB();

            var categories = from p in db.Products
                             let Key = new { p.CategoryID, p.SupplierID }
                             group p by Key into g
                             select new { g.Key, g };

            var list = categories.ToList();
            Assert.IsTrue(list.Count > 0);
        }


        [Linq101SamplesModified("Strange syntactical strategy. Everybody aggree with this traduction?")]
        [Test(Description = "GroupBy - Expression. This sample uses group by to return two sequences of products. The first sequence contains products with unit price greater than 10. The second sequence contains products with unit price less than or equal to 10.")]
        public void LinqToSqlGroupBy10()
        {
            Northwind db = CreateDB();

            var categories = from p in db.Products
                             let Key = new { Criterion = p.UnitPrice > 10 || p.UnitPrice == null }
                             group p by Key into g
                             select g;

            var list = categories.ToList();
            Assert.IsTrue(list.Count > 0);

        }
    }
}
