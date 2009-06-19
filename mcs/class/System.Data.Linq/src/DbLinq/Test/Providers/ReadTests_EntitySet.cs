using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
using Test_NUnit.Linq_101_Samples;
using System.Data.Linq;

#if MONO_STRICT
using DataLoadOptions = System.Data.Linq.DataLoadOptions;
#else
using DataLoadOptions = DbLinq.Data.Linq.DataLoadOptions;
#endif

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
    public class EntitySet : TestBase
    {

        [Test]
        public void SimpleMemberAccess01()
        {
            var customer = new Customer();
            var orders = customer.Orders;
        }

        [Test]
        public void SimpleMemberAccess02()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            Assert.Greater(customer.Orders.Count, 0);
        }

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void EntitySetEnumerationProjection()
        {
            var db = CreateDB();
            var results = (from c in db.Customers select c.Orders).ToList();

            Assert.Greater(results.Count, 0);
        }

        [Test]
        public void HasLoadedOrAsignedValues01()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            Assert.IsFalse(customer.Orders.HasLoadedOrAssignedValues);

            customer.Orders.Add(new Order());
            Assert.IsTrue(customer.Orders.HasLoadedOrAssignedValues);
        }

        [Test]
        public void HasLoadedOrAsignedValues02()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            Assert.IsFalse(customer.Orders.HasLoadedOrAssignedValues);

            customer.Orders.Assign(System.Linq.Enumerable.Empty<Order>());
            Assert.IsTrue(customer.Orders.HasLoadedOrAssignedValues);
        }


        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidSourceChange()
        {
            var db = CreateDB();
            var customer = db.Customers.First();

            Assert.Greater(customer.Orders.Count, 0, "#1");
            Assert.IsTrue(customer.Orders.HasLoadedOrAssignedValues, "#2");
            customer.Orders.SetSource(System.Linq.Enumerable.Empty<Order>());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidSourceChange2()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            Assert.IsFalse(customer.Orders.HasLoadedOrAssignedValues, "#1");
            customer.Orders.Assign(new List<Order>());
            Assert.IsTrue(customer.Orders.HasLoadedOrAssignedValues, "#2");
            customer.Orders.SetSource(System.Linq.Enumerable.Empty<Order>());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void InvalidSourceChange3()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            customer.Orders.SetSource(new List<Order>());
            Assert.IsFalse(customer.Orders.HasLoadedOrAssignedValues, "#1");
            customer.Orders.Load();
            Assert.IsTrue(customer.Orders.HasLoadedOrAssignedValues, "#2");
            customer.Orders.SetSource(System.Linq.Enumerable.Empty<Order>());
        }

        [Test]
        public void SourceChange()
        {
            var db = CreateDB();

            int ordersCount = (from cust in db.Customers
                               select cust.Orders.Count).First();

            Assert.Greater(ordersCount, 0);

            var customer2 = db.Customers.First();
            customer2.Orders.SetSource(System.Linq.Enumerable.Empty<Order>());
            Assert.AreEqual(customer2.Orders.Count, 0);
        }


#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void Refresh01()
        {
            var db = CreateDB();
            var c = db.Customers.First();

            int beforeCount = c.Orders.Count;
            Assert.Greater(beforeCount, 0);
            c.Orders.Clear();
            Assert.AreEqual(c.Orders.Count, 0);
            c.Orders.AddRange(db.Orders);
            Assert.Greater(c.Orders.Count, beforeCount);
            db.Refresh(RefreshMode.OverwriteCurrentValues, c.Orders);

            Assert.AreEqual(c.Orders.Count, beforeCount);
        }

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void Refresh02()
        {
            var db = CreateDB();
            var c = db.Customers.First();

            int beforeCount = c.Orders.Count;
            Assert.Greater(beforeCount, 0);
            c.Orders.Clear();
            Assert.AreEqual(c.Orders.Count, 0);
            c.Orders.AddRange(db.Orders);

            int middleCount = c.Orders.Count;
            Assert.Greater(c.Orders.Count, beforeCount);

            db.Refresh(RefreshMode.KeepCurrentValues, c.Orders);
            Assert.AreEqual(c.Orders.Count, middleCount);

            db.Refresh(RefreshMode.KeepChanges, c.Orders);
            Assert.AreEqual(c.Orders.Count, middleCount);
        }


#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void Refresh03()
        {
            var db = CreateDB();
            var c = db.Customers.First();

            var order = c.Orders.First();
            string newcustomerId = "NEWCUSTOMERID";
            order.CustomerID = newcustomerId;

            db.Refresh(RefreshMode.OverwriteCurrentValues, c.Orders);
            Assert.AreNotEqual(order.CustomerID, newcustomerId);
        }

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void Refresh04()
        {
            var db = CreateDB();
            var c = db.Customers.First();

            var order = c.Orders.First();
            string newcustomerId = "NEWCUSTOMERID";
            order.CustomerID = newcustomerId;

            db.Refresh(RefreshMode.KeepCurrentValues, c.Orders);
            Assert.AreEqual(order.CustomerID, newcustomerId);

            db.Refresh(RefreshMode.KeepChanges, c.Orders);
            Assert.AreEqual(order.CustomerID, newcustomerId);
        }


        [Test]
        public void ListChangedEvent()
        {
            var db = CreateDB();
            var customer = db.Customers.Where(c => c.Orders.Count > 0).First();
            Assert.Greater(customer.Orders.Count, 0);
            bool ok;
            System.ComponentModel.ListChangedEventArgs args = null;
            customer.Orders.ListChanged += delegate(object sender, System.ComponentModel.ListChangedEventArgs a) 
                { 
                    ok = true; 
                    args = a; 
                };

            ok = false;
            args = null;
            customer.Orders.Remove(customer.Orders.First());
            Assert.IsTrue(ok);
            Assert.AreEqual(System.ComponentModel.ListChangedType.ItemDeleted, args.ListChangedType);
            Assert.AreEqual(0, args.NewIndex);
            Assert.AreEqual(-1, args.OldIndex);

            ok = false;
            args = null;
            customer.Orders.Assign(Enumerable.Empty<Order>());
            Assert.IsTrue(ok);
            Assert.AreEqual(System.ComponentModel.ListChangedType.Reset, args.ListChangedType);
            Assert.AreEqual(0, args.NewIndex);
            Assert.AreEqual(-1, args.OldIndex);

            ok = false;
            args = null;
            customer.Orders.Add(db.Orders.First());
            Assert.IsTrue(ok);
            Assert.AreEqual(System.ComponentModel.ListChangedType.ItemAdded, args.ListChangedType);
            Assert.AreEqual(0, args.NewIndex);
            Assert.AreEqual(-1, args.OldIndex);

            ok = false;
            args = null;
            customer.Orders.Clear();
            Assert.IsTrue(ok);
            Assert.AreEqual(System.ComponentModel.ListChangedType.Reset, args.ListChangedType);
            Assert.AreEqual(0, args.NewIndex);
            Assert.AreEqual(-1, args.OldIndex);

            ok = false;
            args = null;
            customer.Orders.Insert(0, new Order());
            Assert.IsTrue(ok);
            Assert.AreEqual(System.ComponentModel.ListChangedType.ItemAdded, args.ListChangedType);
            Assert.AreEqual(0, args.NewIndex);
            Assert.AreEqual(-1, args.OldIndex);

            ok = false;
            args = null;
            customer.Orders.RemoveAt(0);
            Assert.IsTrue(ok);
            Assert.AreEqual(System.ComponentModel.ListChangedType.ItemDeleted, args.ListChangedType);
            Assert.AreEqual(args.NewIndex, 0);
            Assert.AreEqual(args.OldIndex, -1);
        }

        [Test]
        public void Load()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            var orders = customer.Orders;

            Assert.IsFalse(orders.HasLoadedOrAssignedValues);
            orders.Load();
            Assert.IsTrue(orders.HasLoadedOrAssignedValues);
        }

        [Test]
        public void DeferedExecution()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            Assert.IsTrue(customer.Orders.IsDeferred);

            customer.Orders.Load();
            Assert.IsFalse(customer.Orders.IsDeferred);
        }

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void DeferedExecutionAndLoadWith()
        {
            var db = CreateDB();
            DataLoadOptions loadoptions = new DataLoadOptions();
            loadoptions.LoadWith<Customer>(c => c.Orders);
            db.LoadOptions = loadoptions;

            var customer = db.Customers.First();
            Assert.IsFalse(customer.Orders.IsDeferred, "#1");
            Assert.IsTrue(customer.Orders.HasLoadedOrAssignedValues, "#2");
        }

        [Test]
        public void Add()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            int beforeCount = customer.Orders.Count;
            var order = new Order();
            customer.Orders.Add(order);
            Assert.AreEqual(beforeCount + 1, customer.Orders.Count, "#3");
            customer.Orders.Add(order); // do not actually add
            Assert.AreEqual(beforeCount + 1, customer.Orders.Count, "#4");
        }

        [Test]
        [ExpectedException (typeof (ArgumentOutOfRangeException))]
        public void IList_Add()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            var order = new Order();
            ((IList)customer.Orders).Add(order);
            ((IList)customer.Orders).Add(order); // raises ArgumentOutOfRangeException for duplicate
        }

        [Test]
        public void Clear()
        {
            var db = CreateDB();
            var customer = db.Customers.First();

            if (customer.Orders.Count == 0)
                Assert.Ignore();

            customer.Orders.Clear();
            Assert.AreEqual(customer.Orders.Count, 0);
        }

        [Test]
        public void AddRange()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            int beforeCount = customer.Orders.Count;
            var order = new Order();
            customer.Orders.AddRange(new Order[] { order, new Order() });
            Assert.AreEqual(beforeCount + 2, customer.Orders.Count);
            customer.Orders.AddRange(new Order[] { new Order(), order }); // one is existing -> not added
            Assert.AreEqual(beforeCount + 3, customer.Orders.Count);
        }

        [Test]
        public void Remove()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            Assert.IsTrue(customer.Orders.IsDeferred);
            int beforeCount = customer.Orders.Count;
            Assert.IsFalse(customer.Orders.IsDeferred);

            if (beforeCount == 0)
                Assert.Ignore();

            Assert.IsFalse(customer.Orders.Remove(null));
            Assert.AreEqual(beforeCount, customer.Orders.Count);

            Assert.IsTrue(customer.Orders.Remove(customer.Orders.First()));
            Assert.AreEqual(customer.Orders.Count, beforeCount - 1);
        }

        [Test]
        public void RemoveAt()
        {
            var db = CreateDB();
            var customer = db.Customers.First();
            int beforeCount = customer.Orders.Count;

            if (beforeCount == 0)
                Assert.Ignore();

            customer.Orders.RemoveAt(0);
            Assert.AreEqual(customer.Orders.Count, beforeCount - 1);
        }

        [Test]
        public void RemoveAll()
        {
            Clear();
        }
    }
}
