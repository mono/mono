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
#elif MSSQL && MONO_STRICT
    namespace Test_NUnit_MsSql_Strict.Linq_101_Samples
#elif MSSQL
    namespace Test_NUnit_MsSql.Linq_101_Samples
#elif FIREBIRD
    namespace Test_NUnit_Firebird.Linq_101_Samples
#endif
{
    [TestFixture]
    public class Paging : TestBase
    {
        [Test(Description = "Paging - Index. This sample uses the Skip and Take operators to do paging by skipping the first 50 records and then returning the next 10, thereby providing the data for page 6 of the Products table.")]
        public void LinqToSqlPaging01()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     orderby c.ContactName
                     select c)
                    .Skip(1)
                    .Take(2);

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "Paging - Ordered Unique Key. This sample uses a where clause and the take operator to do paging by, first filtering to get only the ProductIDs above 50 (the last ProductID from page 5), then ordering by ProductID, and finally taking the first 10 results, thereby providing the data for page 6 of the Products table. Note that this method only works when ordering by a unique key.")]
        public void LinqToSqlPaging02()
        {
            Northwind db = CreateDB();

            var q = (from p in db.Products
                     where p.ProductID > 3
                     orderby p.ProductID
                     select p)
                    .Take(10);

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }
    }
}
