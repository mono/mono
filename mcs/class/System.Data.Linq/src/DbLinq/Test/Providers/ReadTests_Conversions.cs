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
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
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
#elif MSSQL && MONO_STRICT
    namespace Test_NUnit_MsSql_Strict
#elif MSSQL
    namespace Test_NUnit_MsSql
#elif FIREBIRD
    namespace Test_NUnit_Firebird
#endif
{
    [TestFixture]
    public class ReadTests_Conversions:TestBase
    {

        [Test]
        public void TestToString01()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where e.LastName.ToString() == "F"
                        select e;

            var list = query.ToList();
        }

        [Test]
        [ExpectedException(typeof(System.NotSupportedException))]
        public void TestToString02()
        {
            Northwind db = CreateDB();
            var query = from e in db.OrderDetails
                        where e.ToString() == "1"
                        select e;

            var list = query.ToList();
        }

        [Test]
        public void TestToString03()
        {
            Northwind db = CreateDB();
            var query = from e in db.OrderDetails
                        where e.Discount.ToString() == "1"
                        select e;

            var list = query.ToList();
        }

        [Test]
        public void TestToString04()
        {
            Northwind db = CreateDB();
            object strangeObject4Sql = new HttpStyleUriParser();
            var query = from e in db.OrderDetails
                        where e.Discount.ToString() == strangeObject4Sql.ToString()
                        select e;

            var list = query.ToList();
        }

        [Test]
        public void ParseInt()
        {
            Northwind db = CreateDB();
            string year = "1997";
            var query = from e in db.Employees
                        where e.BirthDate.Value.Year == int.Parse(year)
                        select e;

            var list = query.ToList();
        }

        [Test]
        public void ParseFloat()
        {
            Northwind db = CreateDB();
            string realNumber = "0,1";
            var query = from e in db.Employees
                        where e.BirthDate.Value.Year == float.Parse(realNumber)
                        select e;

            var list = query.ToList();
        }
    }
}
