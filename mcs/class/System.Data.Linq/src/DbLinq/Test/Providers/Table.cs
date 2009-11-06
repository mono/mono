using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
using Test_NUnit.Linq_101_Samples;
using System.Data.Linq;

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
    public class Table : TestBase
    {
        [Test]
        public void BasicAccess()
        {
            var db = CreateDB();
            var customers = db.Customers.ToArray();
        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void GetModifiedMembers()
        {
            var db = CreateDB();
            var customer = db.Customers.First();

            string beforeCountry = customer.Country;
            string afterCountry = "Spain";
            customer.Country = afterCountry;

            string beforeFax = customer.Fax;
            string afterFax = "4823-435-6456";
            customer.Fax = afterFax;

            ModifiedMemberInfo[] modifiedInfoList = db.Customers.GetModifiedMembers(customer);
            Assert.AreEqual(modifiedInfoList.Count(), 2);

            ModifiedMemberInfo modInfo = modifiedInfoList.First();
            Assert.AreEqual(modInfo.Member, typeof(Customer).GetProperty("Country"));
            Assert.AreEqual(modInfo.CurrentValue, afterCountry);
            Assert.AreEqual(modInfo.OriginalValue, beforeCountry);

            modInfo = modifiedInfoList.ElementAt(1);
            Assert.AreEqual(modInfo.Member, typeof(Customer).GetProperty("Fax"));
            Assert.AreEqual(modInfo.CurrentValue, afterFax);
            Assert.AreEqual(modInfo.OriginalValue, beforeFax);
        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void GetOriginalEntityState()
        {
            var db = CreateDB();
            var customer = db.Customers.First();

            string beforeCountry = customer.Country;
            string afterCountry = "Spain";
            customer.Country = afterCountry;

            string beforeFax = customer.Fax;
            string afterFax = "4823-435-6456";
            customer.Fax = afterFax;

            var originalCustomer = db.Customers.GetOriginalEntityState(customer);
            Assert.AreEqual(originalCustomer.Fax, beforeFax);
            Assert.AreEqual(originalCustomer.Country, beforeCountry);

        }

        //[Test]
        //public void IsReadOnly()
        //{
        //    var db = CreateDB();
        //    db.ObjectTrackingEnabled=false;
        //    db.Customers.ToArray();
        //    Assert.IsFalse(db.Customers.IsReadOnly);

        //    var db2 = CreateDB();
        //    db2.ObjectTrackingEnabled = true;
        //    db2.Customers.ToArray();
        //    Assert.IsTrue(db2.Customers.IsReadOnly);
        //}

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void Attach01()
        {
            var db = CreateDB();
            var customer = new Customer();
            db.Customers.Attach(customer);

            Assert.IsFalse(db.Customers.Contains(customer));
        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Attach02()
        {
            var db = CreateDB();
            var customer = new Customer();
            db.Customers.Attach(customer);

            Assert.IsFalse(db.Customers.Contains(customer));
            var db2 = CreateDB();
            db2.Customers.Attach(customer);
        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Attach03()
        {
            var db = CreateDB();
            db.ObjectTrackingEnabled = false;
            var customer = new Customer();
            db.Customers.Attach(customer);
        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void Attach04()
        {
            var db = CreateDB();
            var originalCustomer = db.Customers.First();
            var customer = new Customer();
            db.Customers.Attach(customer, originalCustomer);

            Assert.Greater(db.Customers.GetModifiedMembers(customer).Count(), 0);
        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Attach05()
        {
            var db = CreateDB();
            var customer = new Customer();
            db.Customers.Attach(customer, true);
        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void Attach06()
        {
            var db = CreateDB();
            var customer = new Customer();
            //http://geekswithblogs.net/michelotti/archive/2007/12/17/117791.aspx
            //we have to do a test related with that stuff, but we need to change all of datacontexts

            Assert.Ignore();
        }


#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void AttachAll()
        {
            var db = CreateDB();
            var customers = new Customer[] { new Customer { CustomerID = "ID1" }, new Customer { CustomerID = "ID2" } };
            db.Customers.AttachAll(customers);

            Assert.IsFalse(customers.Any(c => db.Customers.Contains(c)));

        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        public void GetBindingList()
        {
            var db = CreateDB();
            var customers = db.Customers.GetNewBindingList();

            Assert.AreEqual(customers.Count, db.Customers.Count());
        }

        [Description("Check direct use of DataContext instead of typed DataContext")]
        [Test]
        public void T1_DirectDataContext()
        {
            var db = CreateDB();

            var dc = new 
#if MONO_STRICT
            System.Data.Linq.DataContext(db.Connection);
#else
            DbLinq.Data.Linq.DataContext(db.Connection, CreateVendor());
#endif

            var dbq = from p in db.Products where p.ProductName == "Chai" select p.ProductID;
            var dbc = dbq.ToList().Count;
            Assert.AreEqual(dbc, 1);

            var dcq = from p in dc.GetTable<Product>() where p.ProductName == "Chai" select p.ProductID;
            var dcc = dcq.ToList().Count;
            Assert.AreEqual(dbc, 1);
        }
    }
}
