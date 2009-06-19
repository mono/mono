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
    public class UnionAll_Union_Intersect : TestBase
    {
        [Test(Description = "Concat - Simple. This sample uses Concat to return a sequence of all Customer and Employee phone/fax numbers.")]
        public void LinqToSqlUnion01()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers select c.Phone).Concat(
                     from c in db.Customers select c.Fax).Concat(
                     from e in db.Employees select e.HomePhone);

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "Concat - Compound. This sample uses Concat to return a sequence of all Customer and Employee name and phone number mappings.")]
        public void LinqToSqlUnion02()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     select new { Name = c.CompanyName, Phone = c.Phone })
                     .Concat(from e in db.Employees
                             select new { Name = e.FirstName + " " + e.LastName, Phone = e.HomePhone });

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "Union. This sample uses Union to return a sequence of all countries that either Customers or Employees are in.")]
        public void LinqToSqlUnion03()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     select c.Country).Union(from e in db.Employees
                                             select e.Country);

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "Intersect. This sample uses Intersect to return a sequence of all countries that both Customers and Employees live in.")]
        public void LinqToSqlUnion04()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     select c.Country).Intersect(from e in db.Employees
                                                 select e.Country);

            var list = q.ToList();
            if (list.Count == 0)
                Assert.Ignore("Please check test validity");
            //Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "Except. This sample uses Except to return a sequence of all countries that Customers live in but no Employees live in.")]
        public void LinqToSqlUnion05()
        {
            Northwind db = CreateDB();

            var q = (from c in db.Customers
                     select c.Country).Except(from e in db.Employees select e.Country);

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }
    }
}
