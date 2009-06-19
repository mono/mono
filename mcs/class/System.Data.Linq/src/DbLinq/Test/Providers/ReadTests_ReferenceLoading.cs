using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
using Test_NUnit.Linq_101_Samples;
using System.Data.Linq;

using nwind;

// test ns 
#if MYSQL
    namespace Test_NUnit_MySql
#elif ORACLE && ODP
    namespace Test_NUnit_OracleODP
#elif ORACLE
    namespace Test_NUnit_Oracle
#elif POSTGRES
    namespace Test_NUnit_PostgreSql
#elif SQLITE
    namespace Test_NUnit_Sqlite
#elif INGRES
    namespace Test_NUnit_Ingres
#elif MSSQL && L2SQL
    namespace Test_NUnit_MsSql_Strict
#elif MSSQL
    namespace Test_NUnit_MsSql
#elif FIREBIRD
    namespace Test_NUnit_Firebird
#endif
{
    [TestFixture]
    public class ReadTests_ReferenceLoading : TestBase
    {

        [Test]
        public void ReferenceLoading01()
        {
            var db = CreateDB();
            var order = db.Orders.First();
            Assert.IsNotNull(order.Employee);
        }

        [Test]
        public void ReferenceLoading02()
        {
            var db = CreateDB();
            var c = db.Customers.First();
            Assert.IsNotNull(c.Orders.First().Employee);
        }

        [Test]
        public void ReferenceLoading03()
        {
            var db = CreateDB();
            var employeeTerritory = db.EmployeeTerritories.First();
            Assert.IsNotNull(employeeTerritory.Territory.Region.RegionID);
        }

        [Test]
        public void ReferenceLoading04()
        {
            var db = CreateDB();
            var q = db.Employees.Select(e => new { e.Region });

            var list = q.ToList();
            Assert.AreEqual(db.Employees.Count(), list.Count);
        }

#if !DEBUG && (SQLITE || (MSSQL && !MONO_STRICT))
        [Explicit]
#endif
        [Test]
        public void ComplexProjection01()
        {
            var db = CreateDB();
            var q = db.Employees.Select(e => e.Orders);

            var list = q.ToList();
            Assert.AreEqual(db.Employees.Count(), list.Count);
        }

#if !DEBUG && (SQLITE || (MSSQL && !MONO_STRICT))
        [Explicit]
#endif
        [Test]
        public void ComplexProjection02()
        {
            var db = CreateDB();
            var q = db.Employees.Select(e => new { e.Orders });

            var list = q.ToList();
            Assert.AreEqual(db.Employees.Count(), list.Count);
        }


#if !DEBUG && (SQLITE || (MSSQL && !MONO_STRICT))
        [Explicit]
#endif
        [Test]
        public void ComplexProjection03()
        {
            var db = CreateDB();
            var q = db.Employees.Select(e => e.Orders.Select(o => o.OrderID));

            var list = q.ToList();
            Assert.AreEqual(db.Employees.Count(), list.Count);
        }


#if !DEBUG && (SQLITE || (MSSQL && !MONO_STRICT))
        [Explicit]
#endif
        [Test]
        public void ComplexProjection04()
        {
            var db = CreateDB();
            var q = db.Employees.Select(e => e.Orders.Select(o => o.OrderID));

            var list = q.ToList();
            Assert.AreEqual(db.Employees.Count(), list.Count);
        }

#if !DEBUG && (SQLITE || (MSSQL && !MONO_STRICT))
        [Explicit]
#endif
        [Test]
        public void ComplexProjection05()
        {
            var db = CreateDB();
            var q = db.Orders.Select(o => o.Employee.EmployeeTerritories);

            var list = q.ToList();
            Assert.AreEqual(db.Orders.Count(), list.Count);
        }

#if !DEBUG && (SQLITE || (MSSQL && !MONO_STRICT))
        [Explicit]
#endif
        [Test]
        public void ComplexProjection06()
        {
            var db = CreateDB();
            var q = db.Orders.Select(o => new { o.Employee, X = o.OrderDetails.Select(od => od.Product) });

            var list = q.ToList();
            Assert.AreEqual(db.Orders.Count(), list.Count);
        }

#if !DEBUG && (SQLITE || (MSSQL && !MONO_STRICT))
        [Explicit]
#endif
        [Test]
        public void ComplexProjection07()
        {
            var db = CreateDB();
            var q = db.Employees.Select(e => e.Orders.Select(o=>o));

            var list = q.ToList();
        }
    }
}
