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
    public class ReadTest_Complex : TestBase
    {
        Northwind db;

        public ReadTest_Complex()
        {
            db = CreateDB();

        }

        #region 'D' tests exercise 'local object constants'
        [Test]
        public void D0_SelectPensByLocalProperty()
        {
            //reported by Andrus.
            //http://groups.google.com/group/dblinq/browse_thread/thread/c25527cbed93d265

            Northwind db = CreateDB();

            Product localProduct = new Product { ProductName = "Pen" };
            var q = from p in db.Products where p.ProductName == localProduct.ProductName select p;

            List<Product> products = q.ToList();
            int productCount = products.Count;
            Assert.AreEqual(productCount, 1, "Expected one pen, got count=" + productCount);
        }

        [Test]
        public void D1_SelectPensByLocalProperty()
        {
            Northwind db = CreateDB();
            var pen = new { Name = "Pen" };
            var q = from p in db.Products where p.ProductName == pen.Name select p;

            List<Product> products = q.ToList();
            int productCount = products.Count;
            Assert.AreEqual(productCount, 1, "Expected one pen, got count=" + productCount);
        }

        [Test]
        public void D2_SelectPensByLocalPropertyAndConstant()
        {

            Northwind db = CreateDB();
            string pen = "Pen";
            var q = from p in db.Products
                    where p.ProductName == pen &&
                        p.QuantityPerUnit == "10"
                    select p;
            List<Product> products = q.ToList();
            int productCount = products.Count;
            Assert.AreEqual(1, productCount, "Expected one pen, got count=" + productCount);
        }

        [Test]
        public void D3_ArrayContains()
        {
            Northwind db = CreateDB();

            var data = from p in db.Customers
                       where new string[] { "ALFKI", "WARTH" }.Contains(p.CustomerID)
                       select new { p.CustomerID, p.Country };

            var dataList = data.ToList();
            //Assert.AreEqual(productCount, 1, "Expected one pen, got count=" + productCount);
        }
        #endregion

        #region Tests 'F' work on aggregation
        [Test]
        public void F1_ProductCount()
        {
            var q = from p in db.Products select p;
            int productCount = q.Count();
            Assert.Greater(productCount, 0, "Expected non-zero product count");
        }

        [Test]
        public void F2_ProductCount_Projected()
        {
            var q = from p in db.Products select p.ProductID;
            int productCount = q.Count();
            Assert.Greater(productCount, 0, "Expected non-zero product count");
            Console.WriteLine();
        }
        [Test]
        public void F2_ProductCount_Clause()
        {
            var q = from p in db.Products select p.ProductID;
            int productCount = q.Count(i => i < 3);
            Assert.Greater(productCount, 0, "Expected non-zero product count");
            Assert.IsTrue(productCount < 4, "Expected product count < 3");
        }

        [Test]
        public void F3_MaxProductId()
        {
            var q = from p in db.Products select p.ProductID;
            var maxID = q.Max();
            Assert.Greater(maxID, 0, "Expected non-zero product count");
        }

        [Test]
        public void F4_MinProductId()
        {
            var q = from p in db.Products select p.ProductID;
            var minID = q.Min();
            Assert.Greater(minID, 0, "Expected non-zero product count");
        }

#if !ORACLE // picrap: this test causes an internal buffer overflow when marshaling with oracle win32 driver

        [Test]
        public void F5_AvgProductId()
        {
            var q = from p in db.Products select p.ProductID;
            double avg = q.Average();
            Assert.Greater(avg, 0, "Expected non-zero productID average");
        }

#endif

        [Test]
        public void F7_ExplicitJoin()
        {
            //a nice and light nonsense join:
            //bring in rows such as {Pen,AIRBU}
            var q =
                from p in db.Products
                join o in db.Orders on p.ProductID equals o.OrderID
                select new { p.ProductName, o.CustomerID };

            int rowCount = 0;
            foreach (var v in q)
            {
                rowCount++;
                Assert.IsTrue(v.ProductName != null);
                Assert.IsTrue(v.CustomerID != null);
            }
            Assert.IsTrue(rowCount > 2);
        }

        [Test]
        public void F7b_ExplicitJoin()
        {
            var q =
                from c in db.Customers
                join o in db.Orders on c.CustomerID equals o.CustomerID
                where c.City == "London"
                select o;
        }

#if INCLUDING_CLAUSE
        //Including() clause discontinued in Studio Orcas?
        [Test]
        public void F8_IncludingClause()
        {
            var q = (
	            from c in db.Customers
	            where c.City == "London"
	            select c)
	            .Including(c => c.Orders);
        }

        [Test]
        public void F8_Including_Nested()
        {
            var q = (
	            from c in db.Customers
	            where c.City == "London"
	            select c)
	            .Including(c => c.Orders.Including(o => o.OrderDetails));
        }
#endif

        [Test]
        public void F9_Project_AndContinue()
        {
            var q =
                from c in db.Customers
                where c.City == "London"
                select new { Name = c.ContactName, c.Phone } into x
                orderby x.Name
                select x;
        }

        [Test]
        public void F10_DistinctCity()
        {
            var q1 = from c in db.Customers select c.City;
            var q2 = q1.Distinct();

            int numLondon = 0;
            foreach (string city in q2)
            {
                if (city == "London") { numLondon++; }
            }
            Assert.AreEqual(1, numLondon, "Expected to see London once");
        }

        [Test]
        public void F11_ConcatString()
        {
            var q4 = from p in db.Products select p.ProductName + p.ProductID;
            //var q4 = from p in db.Products select p.ProductID;
            var q5 = q4.ToList();
            Assert.Greater(q5.Count, 2, "Expected to see some concat strings");
            foreach (string s0 in q5)
            {
                bool startWithLetter = Char.IsLetter(s0[0]);
                bool endsWithDigit = Char.IsDigit(s0[s0.Length - 1]);
                Assert.IsTrue(startWithLetter && endsWithDigit, "String must start with letter and end with digit");
            }
        }

        [Test]
        public void F12_ConcatString_2()
        {
            var q4 = from p in db.Products
                     where (p.ProductName + p.ProductID).Contains("e")
                     select p.ProductName;
            //select p.ProductName+p.ProductID;
            //var q4 = from p in db.Products select p.ProductID;
            var q5 = q4.ToList();
            //Assert.Greater( q5.Count, 2, "Expected to see some concat strings");
            //foreach(string s0 in q5)
            //{
            //    bool startWithLetter = Char.IsLetter(s0[0]);
            //    bool endsWithDigit = Char.IsDigit(s0[s0.Length-1]);
            //    Assert.IsTrue(startWithLetter && endsWithDigit, "String must start with letter and end with digit");
            //}
        }
        #endregion

        const string obsoleteError = @"Since beta2 in Linq2Sql to project a new entity (ie: select new Order(3)) is forbidden for coherence reasons, so this tests doesn't mimic the Linq2Sql behavior and it is obsolete and should be modified. If you apply such test cases to Linq2Sql you'll get Test_NUnit_MsSql_Strict.DynamicLinqTest.DL5_NestedObjectSelect:
        System.NotSupportedException : Explicit construction of entity type 'MsNorthwind.XX' in query is not allowed.\n\nMore Info in: http://linqinaction.net/blogs/roller/archive/2007/11/27/explicit-construction-of-entity-type-in-query-is-not-allowed.aspx";
        [Test]
        public void F13_NewCustomer()
        {
            Assert.Ignore(obsoleteError);
            Northwind db = CreateDB();
            IQueryable<Customer> q = (from c in db.Customers
                                      select
                                      new Customer
                                      {
                                          CustomerID = c.CustomerID
                                      });
            var list = q.ToList();
            Assert.Greater(list.Count(), 0, "Expected list");
            //Assert.Greater(list.Count(), 0, "Expected list");
            Assert.Ignore("test passed but: theoretically constructions of entity types are not allowed");
        }

        [Test]
        public void F14_NewCustomer_Order()
        {
            Assert.Ignore(obsoleteError);
            Northwind db = CreateDB();
            IQueryable<Customer> q = (from c in db.Customers
                                      select
                                      new Customer
                                      {
                                          CustomerID = c.CustomerID
                                      });
            //this OrderBy clause messes up the SQL statement
            var q2 = q.OrderBy(c => c.CustomerID);
            var list = q2.ToList();
            Assert.Greater(list.Count(), 0, "Expected list");
            //Assert.Greater(list.Count(), 0, "Expected list");
        }


        [Test]
        public void F15_OrderByCoalesce()
        {
            Northwind db = CreateDB();
            var q = from c in db.Customers
                    orderby c.ContactName ?? ""
                    select c;
            var list = q.ToList();
            Assert.Greater(list.Count(), 0, "Expected list");
        }

        [Test(Description = "Non-dynamic version of DL5_NestedObjectSelect")]
        public void F16_NestedObjectSelect()
        {
            Assert.Ignore(obsoleteError);
            Northwind db = CreateDB();
            var q = from o in db.Orders
                    select new Order() { OrderID = o.OrderID, Customer = new Customer() { ContactName = o.Customer.ContactName } };
            var list = q.ToList();
        }

        [Test(Description = "Non-dynamic version of DL5_NestedObjectSelect")]
        public void F17_NestedObjectSelect_Ver2()
        {
            Assert.Ignore(obsoleteError);
            Northwind db = CreateDB();
            var query = from order in db.Orders
                        select new Order
                        {
                            OrderID = order.OrderID,
                            Customer = new Customer
                            {
                                ContactName = order.Customer.ContactName,
                                ContactTitle = order.Customer.ContactTitle
                            }
                        };
            var list = query.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "byte[] test")]
        public void F18_ByteArrayAssignmentTest()
        {
            var db = CreateDB();

            var nc = new Category { CategoryName = "test", Picture = new byte[] { 1, 2, 3, 4 } };
            db.Categories.InsertOnSubmit(nc);
            db.SubmitChanges();

            var q = from c in db.Categories select new { c.Picture };
            var l = q.ToList();
            Assert.IsTrue(l.Count > 0);
        }

        /// <summary>
        /// the following three tests are from Jahmani's page
        /// LinqToSQL: Comprehensive Support for SQLite, MS Access, SQServer2000/2005
        /// http://www.codeproject.com/KB/linq/linqToSql_7.aspx?msg=2428251
        /// </summary>
        [Test(Description = "list of customers who have place orders that have all been shipped to the customers city.")]
        public void O1_OperatorAll()
        {
            var q = from c in db.Customers
                    where (from o in c.Orders
                           select o).All(o => o.ShipCity == c.City)
                    select new { c.CustomerID, c.ContactName };
            var list = q.ToList();
        }

        [Test(Description = "list of customers who have placed no orders")]
        public void O2_OperatorAny()
        {
            //SELECT  t0.CustomerID, t0.ContactName
            //FROM Customers AS t0
            //WHERE  NOT  (
            //(    SELECT  COUNT(*) 
            //    FROM Orders AS t1
            //    WHERE (t1.CustomerID = t0.CustomerID)
            //) > 0
            //)
            var q = from customer in db.Customers
                    where !customer.Orders.Any()
                    select new { customer.CustomerID, customer.ContactName };
            //var q = from customer in db.Customers
            //        where customer.Orders.Count() == 0
            //        select new { customer.CustomerID, customer.ContactName };
            var list = q.ToList();
        }

        [Test(Description = "provide a list of customers and employees who live in London.")]
        public void O3_OperatorUnion()
        {
            var q = (from c in db.Customers.Where(d => d.City == "London")
                     select new { ContactName = c.ContactName })
              .Union(from e in db.Employees.Where(f => f.City == "London")
                     select new { ContactName = e.LastName });
            var list = q.ToList();
            Assert.IsTrue(list.Count > 0, "Expected some customers and employees from London");

            int countOfGraeme = list.Count(l => l.ContactName == "graeme");
            Assert.IsTrue(countOfGraeme == 1, "Expected London contacts to include graeme");
        }

        [Test]
        public void O4_OperatorContains()
        {
            var ids = new Id[] { 1, 2, 3 };
            Northwind db = CreateDB();

            //var q = from p in db.Products select p.ProductID;
            //int productCount = q.Count();

            var products = from p in db.Products
                           where ids.Contains(p.ProductID)
                           select p;

            Assert.AreEqual(3, products.Count());

        }


    }
}
