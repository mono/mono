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

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
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
    public class ReadTests_DateTimeFunctions : TestBase
    {
#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void GetYear()
        {
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    where o.OrderDate.Value.Year == 1996
                    select o;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void GetMonth()
        {
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    where o.OrderDate.Value.Month == 10
                    select o;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void GetDay()
        {
            Northwind db = CreateDB();

            var q = from o in db.Orders
                    where o.OrderDate.Value.Day == 16
                    select o;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void GetHours()
        {
            Northwind db = CreateDB();

            var q = (from o in db.Orders
                     where o.OrderDate.Value.Hour == 0
                     select o).ToList();


        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void GetMinutes()
        {
            Northwind db = CreateDB();

            var q = (from o in db.Orders
                     where o.OrderDate.Value.Minute == 0
                     select o).ToList();


        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void GetSeconds()
        {
            Northwind db = CreateDB();

            var q = (from o in db.Orders
                     where o.OrderDate.Value.Second == 16
                     select o).ToList();

        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void GetMilliSeconds()
        {
            Northwind db = CreateDB();

            var q = (from o in db.Orders
                     where o.OrderDate.Value.Millisecond == 0
                     select o).ToList();

        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void GetCurrentDateTime()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where e.BirthDate.HasValue && e.BirthDate.Value == DateTime.Now
                        select e;

            var list = query.ToList();
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void Parse01()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where e.BirthDate.Value == DateTime.Parse("1984/05/02")
                        select e;

            var list = query.ToList();
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Parse02()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where e.BirthDate.Value == DateTime.Parse(e.BirthDate.ToString())
                        select e;

            var list = query.ToList();
        }

        [Test]
        public void Parse03()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where e.BirthDate.HasValue
                        select e.BirthDate.Value == DateTime.Parse("1984/05/02");


            var list = query.ToList();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Parse04()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        select e.BirthDate.Value == DateTime.Parse(e.BirthDate.ToString());


            var list = query.ToList();
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void DateTimeDiffTotalHours()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value - DateTime.Parse("1984/05/02")).TotalHours > 0
                        select e;


            var list = query.ToList();
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void DateTimeDiffHours()
        {
            Northwind db = CreateDB();

            DateTime parameterDateTime = db.Employees.First().BirthDate.Value.AddHours(2);

            var query = from e in db.Employees
                        where (e.BirthDate.Value - parameterDateTime).Hours > -2
                        select e;


            var list = query.ToList();
            Assert.Greater(list.Count, 0);
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void DateTimeDiffTotalMinutes()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value - DateTime.Parse("1984/05/02")).TotalMinutes > 0
                        select e;


            var list = query.ToList();
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void DateTimeDiffMinutes()
        {
            Northwind db = CreateDB();

            DateTime parameterDateTime = db.Employees.First().BirthDate.Value.AddMinutes(2);

            var query = from e in db.Employees
                        where (e.BirthDate.Value - parameterDateTime).Minutes == -2
                        select e;


            var list = query.ToList();
            Assert.Greater(list.Count, 0);
        }


#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void DateTimeDiffTotalSeconds()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value - DateTime.Parse("1984/05/02")).TotalSeconds > 0
                        select e;


            var list = query.ToList();
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void DateTimeDiffSeconds()
        {
            Northwind db = CreateDB();

            DateTime parameterDateTime = db.Employees.First().BirthDate.Value.AddSeconds(2);

            var query = from e in db.Employees
                        where (e.BirthDate.Value - parameterDateTime).Seconds == -2
                        select e;


            var list = query.ToList();
            Assert.Greater(list.Count, 0);
        }

#if !DEBUG && (SQLITE || MSSQL)
        // L2SQL: SQL Server doesnt' seem to support millisecond precision.
        [Explicit]
#endif
        [Test]
        public void DateTimeDiffMilliseconds()
        {
            Northwind db = CreateDB();

            DateTime parameterDateTime = db.Employees.First().BirthDate.Value.AddMilliseconds(2);

            var query = from e in db.Employees
                        where (e.BirthDate.Value - parameterDateTime).Milliseconds == -2
                        select e;
            

            var list = query.ToList();
            Assert.Greater(list.Count, 0);
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void DateTimeDiffTotalMilliseconds()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value - DateTime.Parse("1984/05/02")).TotalMinutes > 0
                        select e;


            var list = query.ToList();
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void DateTimeDiffDays()
        {
            Northwind db = CreateDB();

            DateTime parameterDateTime = db.Employees.First().BirthDate.Value.AddDays(2);

            var query = from e in db.Employees
                        where (e.BirthDate.Value - parameterDateTime).Days == -2
                        select e;


            var list = query.ToList();
            Assert.Greater(list.Count, 0);
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void DateTimeDiffTotalDays()
        {
            Northwind db = CreateDB();
            DateTime firstDate = db.Employees.First().BirthDate.Value;

            DateTime parameterDate = firstDate.Date.AddDays(2);
            parameterDate = parameterDate.Date.AddHours(12);


            var query = from e in db.Employees
                        where (e.BirthDate.Value -parameterDate).TotalDays == -2.5
                        select e;

            var list = query.ToList();

            Assert.Greater(list.Count, 0);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DateTimeDiffTotalDaysSelectWithNulls01()
        {
            
            Northwind db = CreateDB();

            var employee = new Employee
            {
                FirstName = "Test First",
                LastName  = "Test Last",
            };
            db.Employees.InsertOnSubmit(employee);
            db.SubmitChanges();

            DateTime firstDate = db.Employees.First().BirthDate.Value;
            firstDate.Date.AddDays(2);
            DateTime parameterDate = firstDate.Date.AddHours(12);

            try
            {
                //this test should throw an invalid operation exception since one BirthDate is null so select clausle should crash
                var query = from e in db.Employees
                            select (e.BirthDate.Value - parameterDate).TotalDays;

                var list = query.ToList();

                Assert.Greater(list.Count, 0);
            }
            finally
            {
                db.Employees.DeleteOnSubmit(employee);
                db.SubmitChanges();
            }
        }

        [Test]
        public void DateTimeDiffTotalDaysSelectWithNulls02()
        {
            Northwind db = CreateDB();

            var employee = new Employee
            {
                FirstName = "Test First",
                LastName = "Test Last",
            };
            db.Employees.InsertOnSubmit(employee);
            db.SubmitChanges();

            DateTime firstDate = db.Employees.First().BirthDate.Value;

            DateTime parameterDate = firstDate.Date.AddDays(2);
            parameterDate = parameterDate.Date.AddHours(12);

            try
            {
                var query = from e in db.Employees
                            where e.BirthDate.HasValue
                            select (e.BirthDate.Value - parameterDate).TotalDays;

                var list = query.ToList();

                Assert.Greater(list.Count, 0);
            }
            finally
            {
                db.Employees.DeleteOnSubmit(employee);
                db.SubmitChanges();
            }
        }


#if !DEBUG && (SQLITE || (MSSQL && L2SQL))
        // L2SQL: System.Data.SqlClient.SqlException : The datepart minute is not supported by date function datepart for data type date.
        [Explicit]
#endif
        [Test]
        public void DateGetDate()
        {
            Northwind db = CreateDB();
            var query = from e in db.Employees
                        where (e.BirthDate.Value.Date).Minute == 0
                        select e;


            var list = query.ToList();
        }
    }
}
