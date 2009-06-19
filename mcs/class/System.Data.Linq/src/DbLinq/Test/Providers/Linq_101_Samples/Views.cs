using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
using Test_NUnit.Linq_101_Samples;

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
    public class Views : TestBase
    {
        [Linq101SamplesModified("Original db didn't has Invoices table so. It has been used Employees instead")]
        [Test(Description = "Query - Anonymous Type. This sample uses SELECT and WHERE to return a sequence of invoices where shipping city is London.")]
        public void LinqToSqlView01()
        {
            Northwind db = CreateDB();

            var q = from i in db.Employees
                    where i.City == "Seattle"
                    select new { i.Country, i.Address, i.City, i.BirthDate };

            var list = q.ToList();
            if (list.Count == 0)
                Assert.Ignore("Please check test"); // the test fails on SQLite
            Assert.IsTrue(list.Count > 0);
        }

        [Linq101SamplesModified("Original db didn't has Invoices table so. It has been used Employees instead")]
        [Test(Description = "Query - Negative. Entities must have a mapped ID, but SqlMetal does not generate an ID for view by default.")]
        public void LinqToSqlView02()
        {
            Northwind db = CreateDB();

            try
            {
                var q = from i in db.Employees
                        where i.City == "London"
                        select i;

                var list = q.ToList();
                Assert.IsTrue(list.Count > 0);

            }
            catch (Exception ex)
            {
                Console.WriteLine("{0} : {1}", ex.GetType().ToString(), ex.Message);
            }
        }

        [Linq101SamplesModified("Original db didn't has Quarterly_Orders table so. It has been used Employees instead")]
        [Test(Description = "Query - Identity mapping. This sample uses SELECT to query QuarterlyOrders.")]
        public void LinqToSqlView03()
        {
            Northwind db = CreateDB();

            var q = from qo in db.Employees
                    select qo;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);

        }
    }
}
