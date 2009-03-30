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
#elif MSSQL && MONO_STRICT
    namespace Test_NUnit_MsSql_Strict.Linq_101_Samples
#elif MSSQL
    namespace Test_NUnit_MsSql.Linq_101_Samples
#elif FIREBIRD
    namespace Test_NUnit_Firebird.Linq_101_Samples
#endif
{
    [TestFixture]
    public class Where : TestBase
    {
        [Test(Description = "where - 1. This sample uses where to filter for Customers in London.")]
        public void LinqToSqlWhere01()
        {
            Northwind db = CreateDB();

            var q = from c in db.Customers
                    where c.City == "London"
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "where - 2. This sample uses where to filter for Employees hired during or after 1994.")]
        public void LinqToSqlWhere02()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where e.HireDate >= new DateTime(1994,1,1)
                    select e;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Linq101SamplesModified("Strange casting, It seems like original northwind discontinued were boolean")]
        [Test(Description = "where - 3. This sample uses where to filter for Products that have stock below their reorder level and are not discontinued.")]
        public void LinqToSqlWhere03()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    where p.UnitsInStock <= p.ReorderLevel || !Convert.ToBoolean(p.Discontinued)==true
                    select p;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Linq101SamplesModified("Strange casting, It seems like original northwind discontinued were boolean")]
        [Test(Description = "where - 4. This sample uses WHERE to filter out Products that are either UnitPrice is greater than 10 or is discontinued.")]
        public void LinqToSqlWhere04()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    where p.UnitPrice > 10.0m || Convert.ToBoolean(p.Discontinued)==true
                    select p;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description="Where - 5. This sample calls WHERE twice to filter out Products that UnitPrice is greater than 10 and is discontinued.")]
        public void LinqToSqlWhere05()
        {
            Northwind db = CreateDB();

            var q = db.Products.Where(p => p.UnitPrice > 5.0m)
                               .Where(p => !Convert.ToBoolean(p.Discontinued)==true);

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Linq101SamplesModified("Changed shipper table by Employee since some of our databases haven't got such table")]
        [Test(Description = "First - Simple. This sample uses First to select the first Shipper in the table.")]
        public void LinqToSqlWhere06()
        {
            Northwind db = CreateDB();
            Employee employee = db.Employees.First();
        }

        [Test(Description = "First - Element. This sample uses Take to select the first Customer with CustomerID 'BONAP'.")]
        public void LinqToSqlWhere07()
        {
            Northwind db = CreateDB();

            var cust = (from c in db.Customers
                        where c.CustomerID == "BONAP"
                        select c).Take(1);

            var list = cust.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "First - Condition. This sample uses First to select an Order with freight greater than 10.00.")]
        public void LinqToSqlWhere08()
        {
            Northwind db = CreateDB();
            var ord = (from o in db.Orders
                       where o.Freight > 10m
                       select o).First();

        }
    }
}
