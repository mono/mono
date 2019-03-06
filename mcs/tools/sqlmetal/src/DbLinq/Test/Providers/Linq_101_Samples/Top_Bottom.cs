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
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737940.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class Top_Bottom : TestBase
    {

        [Test(Description="This sample uses Take to select the first 5 Employees hired.")]
        public void LinqToSqlTop01()
        {
            Northwind db = CreateDB();

            var q = (from e in db.Employees 
                orderby e.HireDate select e). Take(5);

            List<Employee> list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        // The .Skip() method won't work on Ingres as it does not support the OFFSET clause
        // but it's on the roadmap...
#if !INGRES
        [Test(Description = "This sample uses Skip to select all but the 10 most expensive Products.")]
        public void LinqToSqlTop02()
        {
            Northwind db = CreateDB();

            var q = (from p in db.Products
                     orderby p.UnitPrice descending
                     select p).Skip(4);

            List<Product> list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "This bug was submitted by Andrus")]
        public void LinqToSqlTop03_Ex_Andrus()
        {
            Northwind db = CreateDB();

            var q = db.Customers.Skip(3).Take(5);

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);

        }
#endif
    }
}
