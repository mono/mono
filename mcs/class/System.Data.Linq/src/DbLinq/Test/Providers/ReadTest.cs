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
using System.Text;
using System.Linq;
using NUnit.Framework;
using Test_NUnit;
using System.Data.Linq.Mapping;

using nwind;

#if MONO_STRICT
using DataLinq = System.Data.Linq;
#else
using DataLinq = DbLinq.Data.Linq;
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
    public class ReadTest : TestBase
    {
        #region Tests 'A' check for DB being ready


        /// <summary>
        /// in NUnit, tests are executed in alpha order.
        /// We want to start by checking access to DB.
        /// </summary>
        [Test]
        public void A1_PingDatabase()
        {
            Northwind db = CreateDB();
            bool pingOK = db.DatabaseExists();
            //bool pingOK = Conn.Ping(); //Schildkroete - Ping throws NullRef if conn is not open
            Assert.IsTrue(pingOK, "Pinging database");
        }



        [Test]
        public void A3_ProductsTableHasPen()
        {
            Northwind db = CreateDB();
            //string sql = @"SELECT count(*) FROM linqtestdb.Products WHERE ProductName='Pen'";
            string sql = @"SELECT count(*) FROM [Products] WHERE [ProductName]='Pen'";
            long iResult = db.ExecuteCommand(sql);
            //long iResult = base.ExecuteScalar(sql);
            Assert.AreEqual(iResult, 1L, "Expecting one Pen in Products table, got:" + iResult + " (SQL:" + sql + ")");
        }

        [Test]
        public void A4_SelectSingleCustomer()
        {
            Northwind db = CreateDB();

            // Query for a specific customer
            var cust = db.Customers.Single(c => c.CompanyName == "airbus");
            Assert.IsNotNull(cust, "Expected one customer 'airbus'");
        }

        [Test]
        public void A5_SelectSingleOrDefault()
        {
            Northwind db = CreateDB();

            // Query for a specific customer
            var cust = db.Customers.SingleOrDefault(c => c.CompanyName == "airbus");
            Assert.IsNotNull(cust, "Expected one customer 'airbus'");
        }


        [Test]
        public void A6_ConnectionOpenTest()
        {
            Northwind db = CreateDB(System.Data.ConnectionState.Open);
            Product p1 = db.Products.Single(p => p.ProductID == 1);
            Assert.IsTrue(p1.ProductID == 1);
        }

        [Test]
        public void A7_ConnectionClosedTest()
        {
            Northwind db = CreateDB(System.Data.ConnectionState.Closed);
            Product p1 = db.Products.Single(p => p.ProductID == 1);
            Assert.IsTrue(p1.ProductID == 1);
        }

        #endregion

        //TODO: group B, which checks AllTypes

        #region Tests 'C' do plain select - no aggregation
        [Test]
        public void C1_SelectProducts()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products select p;
            List<Product> products = q.ToList();
            int productCount = products.Count;
            Assert.Greater(productCount, 0, "Expected some products, got none");
        }

        [Test]
        public void C2_SelectPenId()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products where p.ProductName == "Pen" select p.ProductID;
            var productIDs = q.ToList();
            int productCount = productIDs.Count;
            Assert.AreEqual(productCount, 1, "Expected one pen, got count=" + productCount);
        }

        [Test]
        public void C2b_SelectPenId()
        {
            Northwind db = CreateDB();

            var pen = "Pen";
            var q = from p in db.Products where p.ProductName == pen select p.ProductID;
            var productIDs = q.ToList();
            int productCount = productIDs.Count;
            Assert.AreEqual(productCount, 1, "Expected one pen, got count=" + productCount);
        }

        [Test]
        public void C3_SelectPenIdName()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products
                    where p.ProductName == "Pen"
                    select new { ProductId = p.ProductID, Name = p.ProductName };
            int count = 0;
            //string penName;
            foreach (var v in q)
            {
                Assert.AreEqual(v.Name, "Pen", "Expected ProductName='Pen'");
                count++;
            }
            Assert.AreEqual(count, 1, "Expected one pen, got count=" + count);
        }

        [Test]
        public void C4_CountWithOrderBy()
        {
            Northwind db = CreateDB();
            var q = (from p in db.Products
                     orderby p.ProductID
                     select p).Count();
            Assert.IsTrue(q > 0);
        }

        [Test]
        public void C5_ConstantProperty()
        {
            Northwind db = CreateDB();
            var res = from o in db.Orders
                      select new { test = 1 };
            var list = res.ToList();
            Assert.IsTrue(list.Count > 0);
        }


        [Test]
        public void C6_NullParentEmplyee()
        {
            //this should generate a LEFT JOIN statement, but currently does not.
            Northwind db = CreateDB();

            var query = from e in db.Employees
                        select new
                        {
                            Name = e.FirstName,
                            ReportsTo = e.ReportsToEmployee.FirstName
                        };

            var list = query.ToList();
            // PC patch: I get 4 results...
            Assert.IsTrue(list.Count >= 3);
        }



        [Test]
        public void C7_CaseInsensitiveSubstringSearch()
        {
            Northwind db = CreateDB();

            string search = "HERKKU";
            var query = db.Customers.Where(d => d.CompanyName.ToUpper()
              .Contains(search));

            var list = query.ToList();
            Assert.AreEqual(1, list.Count);
        }


        /// <summary>
        /// from http://www.agilior.pt/blogs/pedro.rainho/archive/2008/04/11/4271.aspx
        /// </summary>
        [Test(Description = "Using LIKE operator from linq query")]
        public void C7B_LikeOperator()
        {
            Northwind db = CreateDB();

            //this used to read "Like(HU%F)" but I don't think we have that company.

            var query = (from c in db.Customers
                         where System.Data.Linq.SqlClient.SqlMethods.Like(c.CompanyName, "Alfre%")
                         select c).ToList();
            var list = query.ToList();
            Assert.AreEqual(1, list.Count);
        }

        [Test]
        public void C8_SelectPenByLocalVariable()
        {
            Northwind db = CreateDB();
            string pen = "Pen";

            var q = from p in db.Products
                    where (p.ProductName == pen)
                    select p;
            var productIDs = q.ToList();
            int productCount = productIDs.Count;
            Assert.AreEqual(productCount, 1, "Expected one pen, got count=" + productCount);
        }

        [Test]
        public void C9_OrderByLeftJoin()
        {
            Northwind db = CreateDB();
            var q = from p in db.Orders
                    orderby p.Customer.City
                    select p;
            
            int count = q.ToList().Count();
            int ordcount = db.Orders.Count();
            Assert.AreEqual(ordcount, count);
        }

        [Test]
        public void C10_ConstantPredicate()
        {
            Northwind db = CreateDB();
            var q = from p in db.Customers
                    where true
                    select p;

            int count = q.ToList().Count();
            Assert.Greater(count,0);
        }

        [Test]
        public void C11_SelectProductsDiscontinued()
        {
            Northwind db = CreateDB();
            var q = from p in db.Products 
#if INGRES
                    where p.Discontinued != 0
#else
                    where p.Discontinued == true 
#endif
                    select p.ProductID;

            var productIDs = q.ToList();
            int productCount = productIDs.Count;
            Assert.AreEqual(productCount, 8, "Expected eight products discontinued, got count=" + productCount);
        }

        [Test]
        public void C12_SelectEmployee_MultiJoinWithWhere()
        {
            Northwind db = CreateDB();
            var q = from t in db.Territories
                      join l in db.EmployeeTerritories on t.TerritoryID equals l.TerritoryID
                      join e in db.Employees on l.EmployeeID equals e.EmployeeID
                      where t.RegionID > 3
                      select e; 
            /* Note that written this way it work, but it's not always possible.
            var q = from t in db.Territories.Where(t => t.RegionID > 3)
                    join l in db.EmployeeTerritories on t.TerritoryID equals l.TerritoryID
                    join e in db.Employees on l.EmployeeID equals e.EmployeeID
                    select e; 
             */
            var employeeCount = q.Count();
            Assert.AreEqual(4, employeeCount, "Expected for employees, got count=" + employeeCount);
        }

        #endregion

        #region region D - select first or last - calls IQueryable.Execute instead of GetEnumerator
        [Test]
        public void D01_SelectFirstPenID()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products where p.ProductName == "Pen" select p.ProductID;
            var productID = q.First();
            Assert.Greater(productID, 0, "Expected penID>0, got " + productID);
        }


        /// <summary>
        /// Reported by pwy.mail in http://code.google.com/p/dblinq2007/issues/detail?id=67
        /// </summary>
        [Test]
        public void D01b_SelectFirstOrDefaultCustomer()
        {
            Northwind db = CreateDB();
            var q =
              from c in db.Customers
              select c;

            Customer customer = q.FirstOrDefault();
            Assert.IsNotNull(customer.CustomerID);
        }


        [Test]
        public void D02_SelectFirstPen()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products where p.ProductName == "Pen" select p;
            Product pen = q.First();
            Assert.IsNotNull(pen, "Expected non-null Product");
        }

        [Test]
        public void D03_SelectLastPenID()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products where p.ProductName == "Pen" select p.ProductID;
            var productID = q.Last();
            Assert.Greater(productID, 0, "Expected penID>0, got " + productID);
        }

        [Test]
        public void D04_SelectProducts_OrderByName()
        {
            Northwind db = CreateDB();

            var q = from p in db.Products orderby p.ProductName select p;
            string prevProductName = null;
            foreach (Product p in q)
            {
                if (prevProductName == p.ProductName && p.ProductName.StartsWith("temp_"))
                    continue; //skip temp rows

                if (prevProductName != null)
                {
                    //int compareNames = prevProductName.CompareTo(p.ProductName);
                    int compareNames = string.Compare(prevProductName, p.ProductName, stringComparisonType);
                    Assert.Less(compareNames, 0, "When ordering by names, expected " + prevProductName + " to come after " + p.ProductName);
                }
                prevProductName = p.ProductName;
            }
            //Assert.Greater(productID,0,"Expected penID>0, got "+productID);
        }

        [Test]
        public void D05_SelectOrdersForProduct()
        {
            Northwind db = CreateDB();
            //var q = from p in db.Products where "Pen"==p.ProductName select p.Order;
            //List<Order> penOrders = q.ToList();
            //Assert.Greater(penOrders.Count,0,"Expected some orders for product 'Pen'");

            var q =
                from o in db.Orders
                where o.Customer.City == "London"
                select new { c = o.Customer, o };

            var list1 = q.ToList();
            foreach (var co in list1)
            {
                Assert.IsNotNull(co.c, "Expected non-null customer");
                Assert.IsNotNull(co.c.City, "Expected non-null customer city");
                Assert.IsNotNull(co.o, "Expected non-null order");
            }
            Assert.Greater(list1.Count, 0, "Expected some orders for London customers");
        }

        [Test]
        public void D06_OrdersFromLondon()
        {
            Northwind db = CreateDB();
            var q =
                from o in db.Orders
                where o.Customer.City == "London"
                select new { c = o.Customer, o };

            var list1 = q.ToList();
            foreach (var co in list1)
            {
                Assert.IsNotNull(co.c, "Expected non-null customer");
                Assert.IsNotNull(co.o, "Expected non-null order");
            }
            Assert.Greater(list1.Count, 0, "Expected some orders for London customers");
        }
        [Test]
        public void D07_OrdersFromLondon_Alt()
        {
            //this is a "SelectMany" query:
            Northwind db = CreateDB();

            var q =
                from c in db.Customers
                from o in c.Orders
                where c.City == "London"
                select new { c, o };

            Assert.Greater(q.ToList().Count, 0, "Expected some orders for London customers");
        }

        [Test]
        public void D08_Products_Take5()
        {
            Northwind db = CreateDB();
            var q = (from p in db.Products select p).Take(5);
            List<Product> prods = q.ToList();
            Assert.AreEqual(5, prods.Count, "Expected five products");
        }

        [Test]
        public void D09_Products_LetterP_Take5()
        {
            Northwind db = CreateDB();

            //var q = (from p in db.Products where p.ProductName.Contains("p") select p).Take(5);
            var q = db.Products.Where(p => p.ProductName.Contains("p")).Take(5);
            List<Product> prods = q.ToList();
#if POSTGRES || INGRES
            int expectedCount = 0; //Only 'Toilet Paper'
#else
            int expectedCount = 2; //Oracle, Mysql: 'Toilet Paper' and 'iPod'
#endif
            Assert.Greater(prods.Count, expectedCount, "Expected couple of products with letter 'p'");
        }

        [Test]
        public void D10_Products_LetterP_Desc()
        {
            Northwind db = CreateDB();

            var q = (from p in db.Products
                     where p.ProductName.Contains("P")
                     orderby p.ProductID descending
                     select p
            ).Take(5);
            //var q = db.Products.Where( p=>p.ProductName.Contains("p")).Take(5);
            List<Product> prods = q.ToList();
            Assert.Greater(prods.Count, 2, "Expected couple of products with letter 'p'");

            var prodID0 = prods[0].ProductID;
            var prodID1 = prods[1].ProductID;
            Assert.Greater(prodID0, prodID1, "Sorting is broken");
        }

        [Test]
        public void D11_Products_DoubleWhere()
        {
            Northwind db = CreateDB();
            var q1 = db.Products.Where(p => p.ProductID > 1).Where(q => q.ProductID < 10);
            int count1 = q1.Count();
        }


        [Test]
        public void D12_SelectDerivedClass()
        {
            Northwind dbo = CreateDB();
            Northwind1 db = new Northwind1(dbo.Connection);

            var derivedCustomer = (from c in db.ChildCustomers
                                   where c.City == "London"
                                   select c).First();
            Assert.IsTrue(derivedCustomer.City == "London");
        }

        public class Northwind1 : Northwind
        {
            public Northwind1(System.Data.IDbConnection connection)
                : base(connection)
            { }

            public class CustomerDerivedClass : Customer { }
            public class CustomerDerivedClass2 : CustomerDerivedClass { }

            public DataLinq.Table<CustomerDerivedClass> ChildCustomers
            {
                get { return base.GetTable<CustomerDerivedClass>(); }
            }
        }


        [Test(Description = "Calls ExecuteQuery<> to store result into object type property")]
        // note: for PostgreSQL requires database with lowercase names, NorthwindReqular.SQL
        public void D13_ExecuteQueryObjectProperty()
        {
            Northwind db = CreateDB();

            var res = db.ExecuteQuery<Pen>(@"SELECT [ProductID] AS PenId FROM [Products] WHERE
              [ProductName] ='Pen'").Single();
            Assert.AreEqual(1, res.PenId);
        }

        class Pen
        {
            internal int PenId;
        }

        [Test]
        public void D14_ProjectedProductList()
        {
            Northwind db = CreateDB();

            var query = from pr in db.Products
                        select new
                        {
                            pr.ProductID,
                            pr.ProductName,
                            pr.Supplier,         // exception!
                            pr.UnitPrice,        // exception!
                            pr.UnitsInStock,
                            pr.UnitsOnOrder
                        };
            //WARNING - as of 2008Apr, we return Suppliers without blowing up, but they need to be live
            var list = query.ToList();
            Assert.IsTrue(list.Count > 0);
            foreach (var item in list)
            {
                Assert.IsTrue(item.Supplier != null);
            }
        }

        [Test]
        public void D15_DuplicateProperty()
        {
            Northwind dbo = CreateDB();
            NorthwindDupl db = new NorthwindDupl(dbo.Connection);
            var derivedCustomer = (from c in db.ChildCustomers
                                   where c.City == "London"
                                   select c).First();
            Assert.IsTrue(derivedCustomer.City == "London");
        }

        public class NorthwindDupl : Northwind
        {
            public NorthwindDupl(System.Data.IDbConnection connection)
                : base(connection)
            { }

            public class CustomerDerivedClass : Customer
            {
                private string city;
                [Column(Storage = "city", Name = "city")]
                public new string City
                {
                    get
                    {
                        return city;
                    }
                    set
                    {
                        if (value != city)
                        {
                            city = value;
                        }
                    }
                }
            }

            public DataLinq.Table<CustomerDerivedClass> ChildCustomers
            {
                get { return base.GetTable<CustomerDerivedClass>(); }
            }
        }

        /// <summary>
        /// DbLinq must use field and should not look to setter.
        /// </summary>
        // PC: is this specified somewhere?
        [Test]
        public void D16_CustomerWithoutSetter()
        {
            Assert.Ignore("See if this is specified");
            Northwind dbo = CreateDB();
            NorthwindAbstractBaseClass db = new NorthwindAbstractBaseClass(dbo.Connection);
            var Customer = (from c in db.ChildCustomers
                            where c.City == "London"
                            select c).First();
            Assert.IsTrue(Customer.City == "London");
        }


        abstract class AbstractCustomer
        {
            public abstract string City { get; }
        }

        class NorthwindAbstractBaseClass : Northwind
        {
            public NorthwindAbstractBaseClass(System.Data.IDbConnection connection)
                : base(connection) { }

            [Table(Name = "customers")]
            public class Customer : AbstractCustomer
            {
                string city;
                [Column(Storage = "city", Name = "city")]
                public override string City
                {
                    get
                    {
                        return city;
                    }
                }
            }

            [Table(Name = "customers")]
            public class Customer2 : Customer { }

            public DataLinq.Table<Customer2> ChildCustomers
            {
                get { return base.GetTable<Customer2>(); }
            }
        }


        #endregion

    }
}
