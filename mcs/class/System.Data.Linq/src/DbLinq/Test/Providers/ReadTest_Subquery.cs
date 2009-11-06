#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Test_NUnit;

using nwind;

#if ORACLE
using Id = System.Decimal;
#else
using Id = System.Int32;
#endif

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
    public class ReadTest_Subquery : TestBase
    {
        [Description("Simple projection")]
        [Test]
        public void CQ1_SimpleProjection()
        {
            var db = CreateDB();
            var q = db.Employees.Select(e => e.LastName);
            var count = q.ToList().Count;
            Assert.IsTrue(count > 0);
        }

        /*
         Generated SQL should look like
SELECT o$.*
FROM Employees AS e$
LEFT OUTER JOIN Orders AS o$ ON o$.[EmployeeID] = e$.[EmployeeID]
         */
#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Description("Subquery")]
        [Test]
        public void CQ2_Subquery()
        {
            var db = CreateDB();
            var q = db.Employees.Select(e => e.Orders);
            var l = q.ToList();
            var count = l.Count;
            Assert.IsTrue(count > 0);
        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Description("Subquery with nested select")]
        [Test]
        public void CQ3_SubquerySelect()
        {
            var db = CreateDB();
            var q = db.Employees.Select(e => e.Orders.Select(o => o));
            var count = q.ToList().Count;
            Assert.IsTrue(count > 0);
        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Description("Subquery with nested entityset")]
        [Test]
        public void CQ4_SubqueryNested()
        {
            var db = CreateDB();
            var q = db.Employees.Select(e => e.Orders.Select(o => o.OrderDetails));
            var count = q.ToList().Count;
            Assert.IsTrue(count > 0);
        }

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Description("Subquery with nested query")]
        [Test]
        public void CQ5_SubqueryNestedQuery()
        {
            var db = CreateDB();
            var q = from d in db.Orders
                    where (from r in db.OrderDetails
                           where r.ProductID == 1
                           select
                               r.OrderID).Contains(d.OrderID)
                    select d;
            var count = q.ToList().Count;
            Assert.AreEqual(38, count);
        }


        [Test]
        public void QueryableContains01()
        {
            var db = CreateDB();
            var q1 = db.OrderDetails.Where(o => o.Discount > 0).Select(o => o.OrderID);
            var q = db.OrderDetails.Where(o => !q1.Contains(o.OrderID));
            Assert.AreEqual(1110, q.Count());
        }

        [Test]
        public void QueryableContains02()
        {
            var db = CreateDB();
            DateTime t = DateTime.Parse("01/01/1950");
            var q1 = db.Employees.Where(e => e.BirthDate.HasValue && e.BirthDate.Value > t).Select(e => e.EmployeeID);
            var q = db.Orders.Where(o => o.EmployeeID.HasValue && !q1.Contains(o.EmployeeID.Value));
            Assert.AreEqual(279, q.Count());
        }
    }
}
