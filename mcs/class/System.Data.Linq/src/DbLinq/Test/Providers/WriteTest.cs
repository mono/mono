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
using System.ComponentModel;
using System.Data.Linq.Mapping;

using nwind;

#if MONO_STRICT
using System.Data.Linq;
#if MONO
using DbLinq.Util;
#endif
#else
using DbLinq.Data.Linq;
using DbLinq.Util;
#endif

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
    public class WriteTest : TestBase
    {
        [SetUp]
        public void TestSetup()
        {
            base.BaseSetUp();

            Profiler.At("START: WriteTest.TestSetup()");

            Northwind db = CreateDB();
            // "[Products]" gets converted to "Products".
            //This is a DbLinq-defined escape sequence, by Pascal.
            //db.ExecuteCommand("DELETE FROM [Products] WHERE [ProductName] like 'temp%'");

            var deleteProducts = db.Products.Where(p => p.ProductName.StartsWith("temp")).ToList();
            db.Products.DeleteAllOnSubmit(deleteProducts);

            var deleteCategories = db.Categories.Where(c => c.CategoryName.StartsWith("temp")).ToList();
            db.Categories.DeleteAllOnSubmit(deleteCategories);

            db.SubmitChanges();

            Profiler.At("END: WriteTest.TestSetup()");
        }

        #region Tests 'E' test live object cache
        [Test]
        public void E1_LiveObjectsAreUnique()
        {
            //grab an object twice, make sure we get the same object each time
            Northwind db = CreateDB();
            var q = from p in db.Products select p;
            Product product1 = q.First();
            Product product2 = q.First();
            Assert.AreSame(product1, product2); 

            string uniqueStr = "Unique" + Environment.TickCount;
            product1.QuantityPerUnit = uniqueStr;
            bool isSameObject1 = product2.QuantityPerUnit == uniqueStr;
            Assert.IsTrue(isSameObject1, "Expected product1 and product2 to be the same live object, but their fields are different");
            object oProduct1 = product1;
            object oProduct2 = product2;
            bool isSameObject2 = oProduct1 == oProduct2;
            Assert.IsTrue(isSameObject2, "Expected product1 and product2 to be the same live object, but their fields are different");
        }

        [Test]
        public void E2_LiveObjectsAreUnique_Scalar()
        {
            //grab an object twice, make sure we get the same object each time
            Northwind db = CreateDB();
            var q = from p in db.Products select p;
            Product product1 = q.First(p => p.ProductName == "Chai");
            Product product2 = q.Single(p => p.ProductName == "Chai");
            bool isSame = object.ReferenceEquals(product1, product2);
            Assert.IsTrue(isSame, "Expected product2 and product2 to be the same live object");
        }

#if MYSQL && USE_ALLTYPES
        [Test]
        public void E3_UpdateEnum()
        {
            Northwind db = CreateDB();

            var q = from at in db.Alltypes where at.int_ == 1 select at;

            Alltype row = q.First();
            DbLinq_EnumTest newValue = row.DbLinq_EnumTest == DbLinq_EnumTest.BB
                ? DbLinq_EnumTest.CC
                : DbLinq_EnumTest.BB;

            row.DbLinq_EnumTest = newValue;

            db.SubmitChanges();
        }
#endif
        #endregion


        #region Tests 'G' do insertion
        private int insertProduct_priv()
        {
            Northwind db = CreateDB();

            Product newProd = new Product();
            newProd.CategoryID = db.Categories.First().CategoryID;
            newProd.ProductName = "Temp." + Environment.TickCount;
            newProd.QuantityPerUnit = "33 1/2";
            db.Products.InsertOnSubmit(newProd);
            db.SubmitChanges();
            Assert.Greater(newProd.ProductID, 0, "After insertion, ProductID should be non-zero");
            //Assert.IsFalse(newProd.IsModified, "After insertion, Product.IsModified should be false");
            return (int)newProd.ProductID; //this test cab be used from delete tests
        }

        [Test]
        public void G1_InsertProduct()
        {
            insertProduct_priv();
        }

        [Test]
        public void G2_DeleteTest()
        {
            int insertedID = insertProduct_priv();
            Assert.Greater(insertedID, 0, "DeleteTest cannot operate if row was not inserted");

            Northwind db = CreateDB();

            var q = from p in db.Products where p.ProductID == insertedID select p;
            List<Product> insertedProducts = q.ToList();
            foreach (Product insertedProd in insertedProducts)
            {
                db.Products.DeleteOnSubmit(insertedProd);
            }
            db.SubmitChanges();

            int numLeft = (from p in db.Products where p.ProductID == insertedID select p).Count();
            Assert.AreEqual(numLeft, 0, "After deletion, expected count of Products with ID=" + insertedID + " to be zero, instead got " + numLeft);
        }

        [Test]
        public void G3_DeleteTest()
        {
            int insertedID = insertProduct_priv();
            Assert.Greater(insertedID, 0, "DeleteTest cannot operate if row was not inserted");

            Northwind db = CreateDB();

            var q = from p in db.Products where p.ProductID == insertedID select p;
            List<Product> insertedProducts = q.ToList();
            foreach (Product insertedProd in insertedProducts)
            {
                db.Products.DeleteOnSubmit(insertedProd);
            }
            db.SubmitChanges();

            int numLeft = (from p in db.Products where p.ProductID == insertedID select p).Count();
            Assert.AreEqual(numLeft, 0, "After deletion, expected count of Products with ID=" + insertedID + " to be zero, instead got " + numLeft);
        }

        [Test]
        public void G4_DuplicateSubmitTest()
        {
            Northwind db = CreateDB();
            int productCount1 = db.Products.Count();
#if INGRES && !MONO_STRICT
            Product p_temp = new Product { ProductName = "temp_g4", Discontinued = "N" };
#else
            Product p_temp = new Product { ProductName = "temp_g4", Discontinued = false };
#endif
            db.Products.InsertOnSubmit(p_temp);
            db.SubmitChanges();
            db.SubmitChanges();
            int productCount2 = db.Products.Count();
            Assert.IsTrue(productCount2 == productCount1 + 1, "Expected product count to grow by one");
        }

        /// <summary>
        /// there is a bug in v0.14 where fields cannot be updated to be null.
        /// </summary>
        [Test]
        public void G5_SetFieldToNull()
        {
            string productName = "temp_G5_" + Environment.TickCount;
            Northwind db = CreateDB();
#if ORACLE
            //todo fix Oracle
            Product p1 = new Product { ProductName = productName, Discontinued = false, UnitPrice = 11 };
#elif INGRES && !MONO_STRICT
            Product p1 = new Product { ProductName = productName, Discontinued = "N", UnitPrice = 11m };
#else
            Product p1 = new Product { ProductName = productName, Discontinued = false, UnitPrice = 11m };
#endif
            db.Products.InsertOnSubmit(p1);
            db.SubmitChanges();

            p1.UnitPrice = null;
            db.SubmitChanges();

            Northwind db3 = CreateDB();
            Product p3 = db3.Products.Single(p => p.ProductName == productName);
            Assert.IsNull(p3.UnitPrice);
        }

        /// <summary>
        /// there is a bug in v0.14 where table Customers cannot be updated,
        /// because quotes where missing around the primaryKey in the UPDATE statement.
        /// </summary>
        [Test]
        public void G6_UpdateTableWithStringPK()
        {
            Northwind db = CreateDB();
            var customer = new Customer
            {
                CompanyName = "Test Company",
                ContactName = "Test Customer",
                CustomerID  = "BT___",
            };
            db.Customers.InsertOnSubmit(customer);
            db.SubmitChanges();
            Customer BT = db.Customers.Single(c => c.CustomerID == "BT___");
            BT.Country = "U.K.";
            db.SubmitChanges();

            db.Customers.DeleteOnSubmit(customer);
            db.SubmitChanges();
        }

        [Test]
        public void G7_InsertTableWithStringPK()
        {
            Northwind db = CreateDB();
            db.ExecuteCommand("DELETE FROM [Customers] WHERE [CustomerID]='TEMP_'");

            Customer custTemp = new Customer
            {
                CustomerID = "TEMP_",
                CompanyName = "Magellan",
                ContactName = "Antonio Pigafetta",
                City = "Lisboa",
            };
            db.Customers.InsertOnSubmit(custTemp);
            db.SubmitChanges();
        }

        [Test]
        public void G8_DeleteTableWithStringPK()
        {
            Northwind db = CreateDB();
            Customer cust = (from c in db.Customers
                             where c.CustomerID == "TEMP_"
                             select c).Single();
            db.Customers.DeleteOnSubmit(cust);
            db.SubmitChanges();
        }

        [Test]
        public void G9_UpdateOnlyChangedProperty()
        {
            Northwind db = CreateDB();
            var cust = (from c in db.Customers
                        select c).First();

            var old = cust.City;
            cust.City = "Tallinn";
            db.SubmitChanges();
            db.SubmitChanges(); // A second call does not update anything

            //exposes bug:
            //Npgsql.NpgsqlException was unhandled
            //Message="ERROR: 23502: null value in column \"companyname\" violates not-null constraint" 
            cust.City = old;
            db.SubmitChanges();

        }

#if POSTGRES

        public class Northwind1 : Northwind
        {
            public Northwind1(System.Data.IDbConnection connection)
                : base(connection) { }

            [System.Data.Linq.Mapping.Table(Name = "cust1")]
            public class Cust1
            {
                
                string _customerid;

                [System.Data.Linq.Mapping.Column(Storage = "_customerid",
                Name = "customerid", IsPrimaryKey = true,
                DbType = "char(10)",
                IsDbGenerated = true,
                Expression = "nextval('seq8')")]
                public string CustomerId
                {
                    get { return _customerid; }
                    set { _customerid = value; }
                }

                // Dummy property is required only as workaround over empty insert list bug
                // If this bug is fixed this may be removed
                string _dummy;
                [System.Data.Linq.Mapping.Column(Storage = "_dummy",
                DbType = "text", Name = "dummy")]
                public string Dummy
                {
                    get;
                    set;
                }

            }

            public Table<Cust1> Cust1s
            {

                get
                {
                    return base.GetTable<Cust1>();
                }
            }
        }

        [Test]
        public void G10_InsertCharSerialPrimaryKey()
        {
            Northwind dbo = CreateDB();
            Northwind1 db = new Northwind1(dbo.Connection);
            try
            {
                db.ExecuteCommand(
                    @"create sequence seq8;
create temp table cust1 ( CustomerID char(10) DEFAULT nextval('seq8'),
dummy text
);
");

                Table<Northwind1.Cust1> cust1s =
                    db.GetTable<Northwind1.Cust1>();

                var cust1 = new Northwind1.Cust1();
                cust1.Dummy = "";
                db.Cust1s.InsertOnSubmit(cust1);
                db.SubmitChanges();
                Assert.IsNotNull(cust1.CustomerId);
            }
            finally
            {
                try { db.ExecuteCommand("drop table cust1;"); }
                catch { }
                try { db.ExecuteCommand("drop sequence seq8;"); }
                catch { }
            }
        }
#endif

        public class NorthwindG11 : Northwind
        {
            public NorthwindG11(System.Data.IDbConnection connection)
                : base(connection) { }

            [Table(Name = "rid")]
            public class Rid : INotifyPropertyChanged
            {

                protected int _id;

                protected int _reanr;


#if INGRES
          [System.Data.Linq.Mapping.Column(Storage = "_id", Name = "id", DbType = "integer", IsPrimaryKey = true, IsDbGenerated = true, Expression = "next value for rid_id1_seq")]
#else
                [System.Data.Linq.Mapping.Column(Storage = "_id", Name = "id", DbType = "integer", IsPrimaryKey = true, IsDbGenerated = true, Expression = "nextval('rid_id1_seq')")]
#endif
                public int Id
                {
                    get { return _id; }
                    set
                    {
                        _id = value;
                        OnPropertyChanged("Id");
                    }
                }

#if INGRES
          [System.Data.Linq.Mapping.Column(Storage = "_reanr", Name = "reanr", DbType = "integer", IsDbGenerated = true, CanBeNull = false, Expression = "next value for rid_reanr_seq")]
#else
                [System.Data.Linq.Mapping.Column(Storage = "_reanr", Name = "reanr", DbType = "integer", IsDbGenerated = true, CanBeNull = false, Expression = "nextval('rid_reanr_seq')")]
#endif
                public int Reanr
                {
                    get { return _reanr; }
                    set
                    {
                        _reanr = value;
                        OnPropertyChanged("Reanr");
                    }
                }


                #region INotifyPropertyChanged handling
                public event PropertyChangedEventHandler PropertyChanged;
                protected virtual void OnPropertyChanged(string propertyName)
                {
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    }
                }
                #endregion

            }


            public Table<Rid> Rids
            {
                get
                {
                    return base.GetTable<Rid>();
                }
            }
        }

#if (POSTGRES || INGRES) && !MONO_STRICT
#if !DEBUG && POSTGRES
        [Explicit]
#endif
        [Test]
        public void G11_TwoSequencesInTable()
        {
            Northwind dbo = CreateDB();
            NorthwindG11 db = new NorthwindG11(dbo.Connection);

            db.ExecuteCommand(@"create sequence rid_id1_seq");
            db.ExecuteCommand(@"create sequence rid_reanr_seq");
#if INGRES
            db.ExecuteCommand(@"create table Rid ( id int primary key DEFAULT rid_id1_seq.nextval, reanr int DEFAULT rid_reanr_seq.nextval)");
#else
            db.ExecuteCommand(@"create temp table Rid ( id int primary key DEFAULT nextval('rid_id1_seq'), reanr int DEFAULT nextval('rid_reanr_seq'))");
#endif
            DbLinq.Data.Linq.Table<NorthwindG11.Rid> Rids = db.GetTable<NorthwindG11.Rid>();

            var Rid = new NorthwindG11.Rid();
            Rid.Reanr = 22;
            Exception e = null;
            db.Rids.InsertOnSubmit(Rid);

            Rid = new NorthwindG11.Rid();
            Rid.Reanr = 23;
            db.Rids.InsertOnSubmit(Rid);
            try
            {
                db.SubmitChanges();
            }
            catch (Exception ex)
            {
                e = ex;
            }
            db.ExecuteCommand("drop table rid");
            db.ExecuteCommand("drop sequence rid_reanr_seq");
            db.ExecuteCommand("drop sequence rid_id1_seq");
            if (e != null)
            {
                throw e;
            }
            Assert.AreEqual(2, Rid.Id);
            Assert.AreEqual(23, Rid.Reanr);
        }

#endif

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void G12_EmptyInsertList()
        {
            Northwind db = CreateDB();
            Region newRegion = new Region() { RegionDescription = "" }; // RegionDescription must be non-null
            db.Regions.InsertOnSubmit(newRegion);
            db.SubmitChanges();
            Assert.IsNotNull(newRegion.RegionID);
            db.Regions.DeleteOnSubmit(newRegion);
            db.SubmitChanges();
        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void G13_ProvidedAutoGeneratedColumn()
        {
            Northwind db = CreateDB();
            Category newCat = new Category();
            newCat.CategoryID = 999;
            newCat.CategoryName = "test";
            db.Categories.InsertOnSubmit(newCat);
            db.SubmitChanges();
            // CategoryID is [Column(AutoSync=AutoSync.OnInsert)], so it's 
            // value is ignored on insert and will be updated
            Assert.AreNotEqual(999, newCat.CategoryID);
            // then, load our object
            var checkCat = (from c in db.Categories where c.CategoryID == newCat.CategoryID select c).Single();
            Assert.AreEqual(newCat.CategoryID, checkCat.CategoryID);
            // remove the whole thing
            db.Categories.DeleteOnSubmit(newCat);
            db.SubmitChanges();
        }


        [Test]
        public void G14_AutoGeneratedSupplierIdAndCompanyName()
        {
            Northwind db = CreateDB();
            Supplier supplier = new Supplier()
            {
                CompanyName = "Test Company",
            };
            db.Suppliers.InsertOnSubmit(supplier);
            db.SubmitChanges();
            Assert.IsNotNull(supplier.SupplierID);
            Assert.AreEqual("Test Company", supplier.CompanyName);
            db.Suppliers.DeleteOnSubmit(supplier);
            db.SubmitChanges();
        }


        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void G15_CustomerIdUpdate()
        {
            //if you run this against Microsoft Linq-to-Sql, it throws an InvalidOperationEx:
            //{"Value of member 'CustomerID' of an object of type 'Customers' changed. 
            //A member defining the identity of the object cannot be changed.
            //Consider adding a new object with new identity and deleting the existing one instead."}

            Northwind db = CreateDB();
            Customer c1 = (from c in db.Customers
                           where c.CustomerID == "AIRBU"
                           select c).Single();
            c1.CustomerID = "TEMP";
            db.SubmitChanges();
            Customer c2 = (from c in db.Customers
                           where c.CustomerID == "TEMP"
                           select c).Single();

            c2.CustomerID = "AIRBU";
            db.SubmitChanges();
        }

        /// <summary>
        /// Quote from MSDN:
        /// If the object requested by the query is easily identifiable as one
        /// already retrieved, no query is executed. The identity table acts as a cache
        /// of all previously retrieved objects

        /// From Matt Warren: http://forums.microsoft.com/MSDN/ShowPost.aspx?PostID=345635&SiteID=1
        /// The cache is checked when the query is a simple table.Where(pred) or table.First(pred) where the 
        /// predicate refers only to the primary key.  Otherwise the query is always sent and the cache only checked 
        /// after the results are retrieved. 
        /// The DLINQ cache is not distributed or shared, it is local and contained within the context.  It is only a 
        /// referential identity cache used to guarantee that two reads of the same entity return the same instance. 
        /// You are not expected to hold the cache for an extended duration (except possibly for a client scenario), 
        /// or share it across threads, processes, or machines in a cluster. 
        /// </summary>
#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void G16_CustomerCacheHit()
        {
            Northwind db = CreateDB();
            Customer c1 = new Customer() { CustomerID = "temp", CompanyName = "Test", ContactName = "Test" };
            db.Customers.InsertOnSubmit(c1);
            db.SubmitChanges();
            db.ExecuteCommand("delete from \"Customers\" WHERE \"CustomerID\"='temp'");

            var res = db.Customers.First(c => c.CustomerID == "temp");
            Assert.IsNotNull(res);
        }



#if !DEBUG && (SQLITE || POSTGRES || MSSQL)
        // L2SQL: System.InvalidOperationException : The type 'Test_NUnit_MsSql_Strict.WriteTest+OrderDetailWithSum' is not mapped as a Table.
        [Explicit]
#endif
        [Test]
        public void G17_LocalPropertyUpdate()
        {
            Northwind dbo = CreateDB();
            NorthwindLocalProperty db = new NorthwindLocalProperty(dbo.Connection);
            var det = db.OrderDetailWithSums.First();
            det.ChangeQuantity();
            Assert.AreEqual(0, db.GetChangeSet().Updates.Count);
            db.SubmitChanges();
        }


        class NorthwindLocalProperty : Northwind
        {
            internal NorthwindLocalProperty(System.Data.IDbConnection connection)
                : base(connection) { }

            internal Table<OrderDetailWithSum> OrderDetailWithSums
            {
                get
                {
                    return GetTable<OrderDetailWithSum>();
                }
            }

        }

        class OrderDetailWithSum : OrderDetail, INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            internal decimal? Sum
            {
                get
                {
                    return Quantity * UnitPrice;
                }
            }

            internal void ChangeQuantity()
            {
                OnPropertyChanged("Sum");
            }
        }

#if !DEBUG && (!(MSSQL && L2SQL))
        [Explicit]
#endif
        // L2SQL: System.NotSupportedException : An attempt has been made to Attach or Add an entity that is not new, perhaps having been loaded from another DataContext.  This is not supported.
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void G18_UpdateWithAttach()
        {
            List<Order> list;
            using (Northwind db = CreateDB())
                list = db.Orders.ToList();

            using (Northwind db = CreateDB())
            {
                var tbl = db.GetTable<Order>();
                foreach (var order in list)
                {
                    if (order.Freight == null)
                        continue;
                    tbl.Attach(order);
                }
                db.SubmitChanges();
            }
        }


#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void G19_ExistingCustomerCacheHit()
        {
            Northwind db = CreateDB();
            string id = "ALFKI";
            Customer c1 = (from c in db.Customers
                           where id == c.CustomerID
                           select c).Single();

            db.Connection.ConnectionString = null;

            var x = db.Customers.First(c => id == c.CustomerID);
        }


        [Test]
        public void G20_CustomerCacheHitComparingToLocalVariable()
        {
             Northwind db = CreateDB();
             try
             {
                Customer c1 = new Customer() { CustomerID = "temp", CompanyName = "Test", ContactName = "Test" };
                db.Customers.InsertOnSubmit(c1);
                db.SubmitChanges();

                string id = "temp";
                var res = from c in db.Customers
                          where c.CustomerID == id
                          select c;

                Assert.AreEqual(1, res.Count(), "#1");

                db.ExecuteCommand("DELETE FROM \"Customers\" WHERE \"CustomerID\"='temp'");

                res = from c in db.Customers
                      where c.CustomerID == id
                      select c;
                Assert.AreEqual(0, res.Count(), "#2");
            }
            finally
            {
                db.ExecuteCommand("DELETE FROM \"Customers\" WHERE \"CustomerID\"='temp'");
            }
        }

        #endregion

        [Test]
        public void Update01()
        {
            var db = CreateDB();
            Employee p = db.Employees.First(r => r.LastName == "Fuller");

            DateTime beforeDateTime = p.BirthDate.Value;
            DateTime now = beforeDateTime.AddMinutes(10);

            p.BirthDate = now;
            db.SubmitChanges();

            Employee p2 = db.Employees.First(r => r.LastName == "Fuller");
            Assert.AreEqual(p2.BirthDate, now);

            //undo changes
            p.BirthDate = beforeDateTime;
            db.SubmitChanges();
        }

#if !DEBUG && SQLITE
        [Explicit]
#endif
        [Test]
        public void InsertAndDeleteWithDependencies()
        {
            const string newCategoryName  = "temp Category";
            const string newProduct1 = "temp First Test Product";
            const string newProduct2 = "temp Second Test Product";

            var db = CreateDB();

            var product = new Product
            {
#if INGRES
                Discontinued = "Y",
#else
                Discontinued = true,
#endif
                ProductName = newProduct1,
            };

            var category = new Category
            {
                CategoryName = newCategoryName,
                Description  = "Insert Description Here",
            };
            category.Products.Add(product);

            Assert.AreEqual(0, category.CategoryID);
            Assert.AreEqual(0, product.CategoryID.Value);

            db.Categories.InsertOnSubmit(category);
            db.SubmitChanges();

            Assert.AreEqual(1, db.Categories.Where(c => c.CategoryName == newCategoryName).Count());
            Assert.AreNotEqual(0, category.CategoryID);
            Assert.AreEqual(1, db.Products.Where(p => p.ProductName == newProduct1).Count());
            Assert.AreEqual(category.CategoryID, product.CategoryID.Value);

            var p2 = new Product
            {
#if INGRES
                Discontinued = "Y",
#else
                Discontinued = true,
#endif
                ProductName = newProduct2
            };
            category.Products.Add(p2);
            db.SubmitChanges();

            Assert.AreEqual(1, db.Products.Where(p => p.ProductName == newProduct2).Count());

            db.Products.DeleteOnSubmit(product);
            db.Products.DeleteOnSubmit(p2);
            db.Categories.DeleteOnSubmit(category);
            db.SubmitChanges();

            Assert.AreEqual(0, db.Categories.Where(c => c.CategoryName == newCategoryName).Count());
            Assert.AreEqual(0, db.Products.Where(p => p.ProductName == newProduct1).Count());
            Assert.AreEqual(0, db.Products.Where(p => p.ProductName == newProduct2).Count());
        }
    }
}
