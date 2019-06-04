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
    public class ReadTests_Join : TestBase
    {

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test(Description = "example by Frans Brouma: select all customers that have no orders")]
        public void LeftJoin_DefaultIfEmpty()
        {
            //example by Frans Brouma on Matt Warren's site
            //select all customers that have no orders
            //http://blogs.msdn.com/mattwar/archive/2007/09/04/linq-building-an-iqueryable-provider-part-vii.aspx
            //http://weblogs.asp.net/fbouma/archive/2007/11/23/developing-linq-to-llblgen-pro-part-9.aspx

            Northwind db = CreateDB();

            var q = from c in db.Customers
                    join o in db.Orders on c.CustomerID equals o.CustomerID into oc
                    from x in oc.DefaultIfEmpty()
                    where x.OrderID == null
                    select c;

            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
            int countPARIS = list.Count(item => item.CustomerID == "PARIS");
            Assert.IsTrue(countPARIS == 1);
        }

        [Test]
        public void LeftOuterJoin_Suppliers()
        {
            //http://blogs.class-a.nl/blogs/anko/archive/2008/03/14/linq-to-sql-outer-joins.aspx
            //example by Anko Duizer (NL)
            Northwind db = CreateDB();
            var query = from s in db.Suppliers
                        join c in db.Customers on s.City equals c.City into temp
                        from t in temp.DefaultIfEmpty()
                        select new
                        {
                            SupplierName = s.CompanyName,
                            CustomerName = t.CompanyName,
                            City = s.City
                        };

            var list = query.ToList();

            bool foundMelb = false, foundNull = false;
            foreach (var item in list)
            {
                foundMelb = foundMelb || item.City == "Melbourne";
                foundNull = foundNull || item.City == null;
            }
            Assert.IsTrue(foundMelb, "Expected rows with City=Melbourne");
            Assert.IsFalse(foundNull, "Expected no rows with City=null");
        }

        // picrap: commented out, it doesn't build because of db.Orderdetails (again, a shared source file...)

        [Test(Description = "Problem discovered by Laurent")]
        public void Join_Laurent()
        {
            Northwind db = CreateDB();

            var q1 = (from p in db.Products
                      join o in db.OrderDetails on p.ProductID equals o.ProductID
                      where p.ProductID > 1
                      select new
                      {
                          p.ProductName,
                          o.OrderID,
                          o.ProductID,
                      }
                      ).ToList();

            Assert.IsTrue(q1.Count > 0);
        }

#if !DEBUG && (SQLITE || POSTGRES || MSSQL)
        // L2SQL: System.InvalidOperationException : The type 'Test_NUnit_MsSql_Strict.ReadTests_Join+Northwind1+ExtendedOrder' is not mapped as a Table.
        [Explicit]
#endif
        [Test]
        public void RetrieveParentAssociationProperty()
        {
            Northwind dbo = CreateDB();
            Northwind1 db = new Northwind1(dbo.Connection);
            var t = db.GetTable<Northwind1.ExtendedOrder>();
            var q = from order in t
                    select new
                    {
                        order.OrderID,
                        order.CustomerShipCity.ContactName
                    };
            var list = q.ToList();
            Assert.IsTrue(list.Count > 0);
        }



#if !DEBUG && (SQLITE || POSTGRES || MSSQL)
        // L2SQL: System.InvalidOperationException : The type 'Test_NUnit_MsSql_Strict.ReadTests_Join+Northwind1+ExtendedOrder' is not mapped as a Table.
        [Explicit]
#endif
        [Test]
        public void DifferentParentAndAssociationPropertyNames()
        {
            Northwind dbo = CreateDB();
            Northwind1 db = new Northwind1(dbo.Connection);
            var query = db.GetTable<Northwind1.ExtendedOrder>() as IQueryable<Northwind1.ExtendedOrder>;

            var q2 = query.Select(e => new Northwind1.ExtendedOrder
            {
                OrderID = e.OrderID,
                ShipAddress = e.CustomerShipCity.ContactName
            });
            var list = q2.ToList();
            Assert.IsTrue(list.Count > 0);
        }

#if !DEBUG && (SQLITE || POSTGRES || MSSQL)
        // L2SQL: System.InvalidOperationException : The type 'Test_NUnit_MsSql_Strict.ReadTests_Join+Northwind1+ExtendedOrder' is not mapped as a Table.
        [Explicit]
#endif
        [Test]
        public void SelectCustomerContactNameFromOrder()
        {
            Northwind dbo = CreateDB();
            Northwind1 db = new Northwind1(dbo.Connection);
            var t = db.GetTable<Northwind1.ExtendedOrder>();

            var q = from order in t
                    select new
                    {
                        order.CustomerContactName
                    };
            var list = q.ToList();
            Assert.AreEqual(db.Orders.Count(), list.Count());
            foreach (var s in list)
                Assert.AreEqual("Test", s);
        }

        public class Northwind1 : Northwind
        {
            public Northwind1(System.Data.IDbConnection connection)
                : base(connection) { }

            // Linq-SQL requires this: [System.Data.Linq.Mapping.Table(Name = "orders")]
            public class ExtendedOrder : Order
            {
#if MONO_STRICT
                System.Data.Linq
#else
                DbLinq.Data.Linq
#endif
.EntityRef<Customer> _x_Customer;

                [System.Data.Linq.Mapping.Association(Storage = "_x_Customer",
                    ThisKey = "ShipCity", Name =
#if MYSQL
 "orders_ibfk_1"
#elif ORACLE
 "SYS_C004742"
#elif POSTGRES
 "fk_order_customer"
#elif SQLITE
 "fk_Orders_1"
#elif INGRES
 "fk_order_customer"
#elif MSSQL
 "fk_order_customer"
#elif FIREBIRD
 "??" // TODO: correct FK name
#else
#error unknown target
#endif
)]
                public Customer CustomerShipCity
                {
                    get { return _x_Customer.Entity; }
                    set { _x_Customer.Entity = value; }
                }

                public string CustomerContactName
                {
                    get
                    {
                        return "Test";
                    }
                }
            }

            public Table<ExtendedOrder> ExtendedOrders
            {
                get { return base.GetTable<ExtendedOrder>(); }
            }
        }

        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void WhereBeforeSelect()
        {
            Northwind db = CreateDB();
            var t = db.GetTable<Order>();

            var query = t.Where(o => o.OrderID != 0);

            query = query.Select(dok => new Order
            {
                OrderID = dok.OrderID,
                OrderDate = dok.OrderDate,
                ShipCity = dok.Customer.ContactName,
                Freight = dok.Freight
            });
            var list = query.ToList();
        }

        /// <summary>
        /// Reported by  pwy.mail in http://code.google.com/p/dblinq2007/issues/detail?id=66
        /// </summary>
        [Test]
        public void OrdersLazyLoad()
        {
            Northwind db = CreateDB();

            var q =
              from c in db.Customers
              select c;

            foreach (var c in q)
            {
                Console.WriteLine(c.Address);
                foreach (var o in c.Orders)
                    Console.WriteLine(o.OrderID);
            }

        }

        [Test]
        public void JoinWhere()
        {
            Northwind db = CreateDB();

            var custID = "BT___";

            var custOderInfos = from o in db.Orders
                                join em in db.Employees on o.EmployeeID equals em.EmployeeID
                                where o.CustomerID == custID
                                select new { o, em };

            var l = custOderInfos.ToList();
        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        // submitted by bryan costanich
        public void ImplicitLeftOuterJoin()
        {
            var db = CreateDB();

            var dbItems =
                    (from a in db.Products
                     from b in db.Suppliers
                     where a.SupplierID == b.SupplierID
                     select a);

            var list = dbItems.ToList();
        }
    }

}
