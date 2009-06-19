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
using System.Text;
using System.Linq;
using NUnit.Framework;
using Test_NUnit;

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
    public class StoredProcTest : TestBase
    {

#if !SQLITE && !MSSQL && !L2SQL && !FIREBIRD
        [Test]
        public void SP1_CallHello0()
        {
            Northwind db = base.CreateDB();
            string result = db.Hello0();
            Assert.IsNotNull(result);
        }

        [Test]
        public void SP2_CallHello1()
        {
            Northwind db = base.CreateDB();
            string result = db.Hello1("xx");
            Assert.IsTrue(result!=null && result.Contains("xx"));
        }

        [Test]
        public void SP3_GetOrderCount_SelField()
        {
            Northwind db = base.CreateDB();
            var q = from c in db.Customers 
                    select new { c.CustomerID, OrderCount = db.GetOrderCount(c.CustomerID) };

            int count = 0;
            foreach (var c in q)
            {
                Assert.IsNotNull(c.CustomerID);
                Assert.Greater(c.OrderCount, -1);
                count++;
            }
            Assert.Greater(count, 0);
        }

        [Test]
        public void SP4_GetOrderCount_SelField_B()
        {
            Northwind db = base.CreateDB();
            var q = from c in db.Customers 
                    select new {c, OrderCount=db.GetOrderCount(c.CustomerID)};

            int count = 0;
            foreach (var v in q)
            {
                Assert.IsNotNull(v.c.CustomerID);
                Assert.Greater(v.OrderCount, -1);
                count++;
            }
            Assert.Greater(count, 0);
        }

        [Test]
        public void SPB_GetOrderCount_Having()
        {
            Northwind db = base.CreateDB();
            var q = from c in db.Customers where db.GetOrderCount(c.CustomerID) > 1 select c;

            int count = 0;
            foreach (var c in q)
            {
                Assert.IsTrue(c.CustomerID!=null, "Non-null customerID required");
                count++;
            }
            Assert.Greater(count, 0);
        }
#endif
    }

}
