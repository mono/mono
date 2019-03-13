﻿using System;
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
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737930.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class NullTest : TestBase
    {
        [Test]
        public void Null()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees 
                    where e.ReportsTo==null select e;

            List<Employee> list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void NullableT_HasValue()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees 
                    where !e.ReportsTo.HasValue select e;

            List<Employee> list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void NullableT_Value()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees 
                    where e.ReportsTo.HasValue 
                    select new { e.FirstName, e.LastName, ReportsTo = e.ReportsTo.Value };

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void Null_EX1()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where null == e.ReportsTo
                    select e;

            List<Employee> list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void Null_EX2()
        {
            Northwind db = CreateDB();

            var q = from e in db.Employees
                    where null != e.ReportsTo
                    select e;

            List<Employee> list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

    }
}
