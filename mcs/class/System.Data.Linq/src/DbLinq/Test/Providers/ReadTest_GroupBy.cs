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
#elif MSSQL && L2SQL
    namespace Test_NUnit_MsSql_Strict
#elif MSSQL
    namespace Test_NUnit_MsSql
#elif FIREBIRD
    namespace Test_NUnit_Firebird
#endif
{
    [TestFixture]
    public class ReadTest_GroupBy : TestBase
    {


        [Test]
        public void G01_SimpleGroup_Count()
        {
            Northwind db = base.CreateDB();

            var q2 = db.Customers.GroupBy(c => c.City)
                .Select(g => new { g.Key, Count = g.Count() });

            int rowCount = 0;
            foreach (var g in q2)
            {
                rowCount++;
                Assert.IsTrue(g.Count > 0, "Must have Count");
                Assert.IsTrue(g.Key != null, "Must have City");
            }
            Assert.IsTrue(rowCount > 0, "Must have some rows");
        }

#if !DEBUG && (MSSQL && !L2SQL)
        [Explicit]
#endif
        [Test]
        public void G02_SimpleGroup_First()
        {
            try
            {
                //Note: this SQL is allowed in Mysql but illegal on Postgres 
                //(PostgreSql ERROR: column "c$.customerid" must appear in the GROUP BY clause or be used in an aggregate function - SQL state: 42803)
                //"SELECT City, customerid FROM customer GROUP BY City"
                //that's why DbLinq disallows it
                Northwind db = base.CreateDB();
                var q2 = db.Customers.GroupBy(c => c.City);
                var q3 = q2.First();

                Assert.IsTrue(q3 != null && q3.Key != null, "Must have result with Key");
                foreach (var c in q3)
                {
                    Assert.IsTrue(c.City != null, "City must be non-null");
                }
            }
            catch(InvalidOperationException)
            {
                Assert.Ignore("Some vendors don't support this request (which doesn't make sense anyway)");
            }
        }

#if !DEBUG && (MSSQL && !L2SQL)
        [Explicit]
#endif
        [Test]
        public void G03_SimpleGroup_WithSelector_Invalid()
        {
            try
            {
                //Note: this SQL is allowed in Mysql but illegal on Postgres 
                //(PostgreSql ERROR: column "c$.customerid" must appear in the GROUP BY clause or be used in an aggregate function - SQL state: 42803)
                //"SELECT City, customerid FROM customer GROUP BY City"
                Northwind db = base.CreateDB();

                var q2 = db.Customers.GroupBy(c => c.City, c => new {c.City, c.CustomerID});

                foreach (var g in q2)
                {
                    int entryCount = 0;
                    foreach (var c in g)
                    {
                        Assert.IsTrue(c.City != null, "City must be non-null");
                        entryCount++;
                    }
                    Assert.IsTrue(entryCount > 0, "Must have some entries in group");
                }
            }
            catch (InvalidOperationException)
            {
                Assert.Ignore("Some vendors don't support this request (which doesn't make sense anyway)");
            }
        }

        [Test]
        public void G03_DoubleKey()
        {
            //Note: this SQL is allowed in Mysql but illegal on Postgres 
            //(PostgreSql ERROR: column "c$.customerid" must appear in the GROUP BY clause or be used in an aggregate function - SQL state: 42803)
            //"SELECT City, customerid FROM customer GROUP BY City"
            Northwind db = base.CreateDB();

            var q2 = from o in db.Orders
                     group o by new { o.CustomerID, o.EmployeeID } into g
                     select new { g.Key.CustomerID, g.Key.EmployeeID, Count = g.Count() };

            int entryCount = 0;
            foreach (var g in q2)
            {
                entryCount++;
                Assert.IsTrue(g.CustomerID != null, "Must have non-null customerID");
                Assert.IsTrue(g.EmployeeID > 0, "Must have >0 employeeID");
                Assert.IsTrue(g.Count >= 0, "Must have non-neg Count");
            }
            Assert.IsTrue(entryCount > 0, "Must have some entries in group");
        }


#if !DEBUG && (MSSQL && !L2SQL)
        [Explicit]
#endif
        [Test]
        public void G04_SimpleGroup_WithSelector()
        {
            try
            {
                //Note: this SQL is allowed in Mysql but illegal on Postgres 
                //(PostgreSql ERROR: column "c$.customerid" must appear in the GROUP BY clause or be used in an aggregate function - SQL state: 42803)
                //"SELECT City, customerid FROM customer GROUP BY City"
                Northwind db = base.CreateDB();
                var q2 = db.Customers.GroupBy(c => c.City, c => c.CustomerID);

                foreach (var g in q2)
                {
                    int entryCount = 0;
                    foreach (var c in g)
                    {
                        Assert.IsTrue(c != null, "CustomerID must be non-null");
                        entryCount++;
                    }
                    Assert.IsTrue(entryCount > 0, "Must have some entries in group");
                }
            }
            catch (InvalidOperationException)
            {
                Assert.Ignore("Some vendors don't support this request (which doesn't make sense anyway)");
            }
        }

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void G05_Group_Into()
        {
            Northwind db = base.CreateDB();
            var q2 =
                from c in db.Customers
                //where c.Country == "France"
                group new { c.PostalCode, c.ContactName } by c.City into g
                select g;
            var q3 = from g in q2 select new { FortyTwo = 42, g.Key, Count = g.Count() };
            //select new {g.Key.Length, g};
            //select new {42,g};

            int entryCount = 0;
            foreach (var g in q3)
            {
                Assert.IsTrue(g.FortyTwo == 42, "Forty42 must be there");
                Assert.IsTrue(g.Count > 0, "Positive count");
                entryCount++;
            }
            Assert.IsTrue(entryCount > 0, "Must have some entries in group");
        }


        [Test]
        public void G06_OrderCountByCustomerID()
        {
            Northwind db = base.CreateDB();

            var q2 = from o in db.Orders
                     group o by o.CustomerID into g
                     //where g.Count()>1
                     select new { g.Key, OrderCount = g.Count() };

            var lst = q2.ToList();
            Assert.Greater(lst.Count, 0, "Expected some grouped order results");
            var result0 = lst[0];
            Assert.IsTrue(result0.Key != null, "Key must be non-null");
            Assert.Greater(result0.OrderCount, 0, "Count must be > 0");
            //select new { g.Key , SumPerCustomer = g.Sum(o2=>o2.OrderID) };
        }

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void G07_OrderCountByCustomerID_Where()
        {
            Northwind db = base.CreateDB();

            var q2 = from o in db.Orders
                     group o by o.CustomerID into g
                     where g.Count() > 1
                     select new { g.Key, OrderCount = g.Count() };

            var lst = q2.ToList();
            Assert.Greater(lst.Count, 0, "Expected some grouped order results");
            var result0 = lst[0];
            Assert.IsTrue(result0.Key != null, "Key must be non-null");
            Assert.Greater(result0.OrderCount, 0, "Count must be > 0");
            //select new { g.Key , SumPerCustomer = g.Sum(o2=>o2.OrderID) };
        }

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void G08_OrderSumByCustomerID()
        {
            Northwind db = base.CreateDB();

            var q2 = from o in db.Orders
                     group o by o.CustomerID into g
                     //where g.Count()>1
                     select new { g.Key, OrderSum = g.Sum(o => o.OrderID) };
            var lst = q2.ToList();
            Assert.Greater(lst.Count, 0, "Expected some grouped order results");
            foreach (var result in lst)
            {
                Console.WriteLine("  Result: custID=" + result.Key + " sum=" + result.OrderSum);
                Assert.IsTrue(result.Key != null, "Key must be non-null");
                Assert.Greater(result.OrderSum, 0, "OrderSum must be > 0");
            }
            //select new { g.Key , SumPerCustomer = g.Sum(o2=>o2.OrderID) };
        }

        /// <summary>
        /// Reported by  pwy.mail in http://code.google.com/p/dblinq2007/issues/detail?id=64
        /// </summary>
#if !DEBUG && (MSSQL && !L2SQL)
        [Explicit]
#endif
        [Test]
        public void G09_UnitPriceGreaterThan10()
        {
            Northwind db = base.CreateDB();

            var priceQuery =
                from prod in db.Products
                group prod by new
                {
                    Criterion = prod.UnitPrice > 10
                }
                    into grouping
                    select grouping;

            foreach (var prodObj in priceQuery)
            {
                if (prodObj.Key.Criterion == false)
                    Console.WriteLine("Prices 10 or less:");
                else
                    Console.WriteLine("\nPrices greater than 10");
                foreach (var listing in prodObj)
                {
                    Console.WriteLine("{0}, {1}", listing.ProductName,
                        listing.UnitPrice);
                }
            }

        }


    }
}
