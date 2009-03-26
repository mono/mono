#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne, Pascal Craponne
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

#if MYSQL
namespace Test_NUnit_MySql
#elif ORACLE
#if ODP
        namespace Test_NUnit_OracleODP
#else
        namespace Test_NUnit_Oracle
#endif
#elif POSTGRES
namespace Test_NUnit_PostgreSql
#elif SQLITE
namespace Test_NUnit_Sqlite
#elif INGRES
    namespace Test_NUnit_Ingres
#elif MSSQL
#if MONO_STRICT
namespace Test_NUnit_MsSql_Strict
#else
namespace Test_NUnit_MsSql
#endif
#elif FIREBIRD
    namespace Test_NUnit_Firebird
#else
#error unknown target
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

        [Description("Subquery with nested select")]
        [Test]
        public void CQ3_SubquerySelect()
        {
            var db = CreateDB();
            var q = db.Employees.Select(e => e.Orders.Select(o => o));
            var count = q.ToList().Count;
            Assert.IsTrue(count > 0);
        }

        [Description("Subquery with nested entityset")]
        [Test]
        public void CQ4_SubqueryNested()
        {
            var db = CreateDB();
            var q = db.Employees.Select(e => e.Orders.Select(o => o.OrderDetails));
            var count = q.ToList().Count;
            Assert.IsTrue(count > 0);
        }

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
            Assert.AreEqual(count,1 );
        }

    }
}
