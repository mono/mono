using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_NUnit;
using NUnit.Framework;
using Test_NUnit.Linq_101_Samples;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

using nwind;

// test ns Linq_101_Samples
#if MYSQL
    namespace Test_NUnit_MySql.Linq_101_Samples
#elif ORACLE && ODP
    namespace Test_NUnit_OracleODP.Linq_101_Samples
#elif ORACLE
    namespace Test_NUnit_Oracle.Linq_101_Samples
#elif POSTGRES
    namespace Test_NUnit_PostgreSql.Linq_101_Samples
#elif SQLITE
    namespace Test_NUnit_Sqlite.Linq_101_Samples
#elif INGRES
    namespace Test_NUnit_Ingres.Linq_101_Samples
#elif MSSQL && MONO_STRICT
    namespace Test_NUnit_MsSql_Strict.Linq_101_Samples
#elif MSSQL
    namespace Test_NUnit_MsSql.Linq_101_Samples
#elif FIREBIRD
    namespace Test_NUnit_Firebird.Linq_101_Samples
#endif
{
    [TestFixture]
    public class Object_Loading : TestBase
    {
        [Test(Description = "This sample demonstrates how to use Including to request related data during the original query so that additional roundtrips to the database are not required later when navigating through the retrieved objects.")]
        public void LinqToSqlObject01()
        {
            Northwind db = CreateDB();

            var custs = from c in db.Customers
                        where c.City == "Marseille"
                        select c;

            foreach (var cust in custs)
                foreach (var ord in cust.Orders)
                {
                    Console.WriteLine("CustomerID {0} has an OrderID {1}.", cust.CustomerID, ord.OrderID);
                }

            var list = custs.ToList();
            Assert.IsTrue(list.Count > 0);

        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !MONO_STRICT))
        [Explicit]
#endif
        [Linq101SamplesModified("The original sample didn't compile, db2 Northwind context was used for nothing")]
        [Test(Description = "This sample demonstrates how to use Including to request related data during the original query so that additional roundtrips to the database are not required later when navigating through the retrieved objects.")]
        public void LinqToSqlObject02()
        {
            Northwind db = CreateDB();


            var ds = new DataLoadOptions();
            ds.LoadWith<Customer>(p => p.Orders);

            db.LoadOptions = ds;

            var custs = from c in db.Customers
                        where c.City == "Marseille"
                        select c;

            foreach (var cust in custs)
                foreach (var ord in cust.Orders)
                    Console.WriteLine("CustomerID {0} has an OrderID {1}.", cust.CustomerID, ord.OrderID);

            var list = custs.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test(Description = "This sample demonstrates how navigating through relationships in retrieved objects can end up triggering new queries to the database if the data was not requested by the original query.")]
        public void LinqToSqlObject03()
        {
            Northwind db = CreateDB();

            var custs = from c in db.Customers
                        where c.City == "London"
                        select c;

            foreach (var cust in custs)
                foreach (var ord in cust.Orders)
                    foreach (var orderDetail in ord.OrderDetails)
                    {
                        Console.WriteLine("CustomerID {0} has an OrderID {1} with ProductID {2} that has name {3}.",
                            cust.CustomerID, ord.OrderID, orderDetail.ProductID, orderDetail.Product.ProductName);
                    }

            var list = custs.ToList();
            Assert.IsTrue(list.Count > 0);

        }

#if !DEBUG && (SQLITE || POSTGRES || (MSSQL && !MONO_STRICT))
        [Explicit]
#endif
        [Linq101SamplesModified("The original sample didn't compile, db2 Northwind context was used for nothing")]
        [Test(Description = "This sample demonstrates how to use Including to request related data during the original query so that additional roundtrips to the database are not required later when navigating through the retrieved objects.")]
        public void LinqToSqlObject04()
        {
            var db = CreateDB();

            var ds = new DataLoadOptions();
            ds.LoadWith<Customer>(p => p.Orders);
            ds.LoadWith<Order>(p => p.OrderDetails);

            db.LoadOptions = ds;

            var custs = from c in db.Customers
                        where c.City == "London"
                        select c;

            foreach (var cust in custs)
                foreach (var ord in cust.Orders)
                    foreach (var orderDetail in ord.OrderDetails)
                    {
                        Console.WriteLine("CustomerID {0} has an OrderID {1} with ProductID {2} that has name {3}.",
                            cust.CustomerID, ord.OrderID, orderDetail.ProductID, orderDetail.Product.ProductName);
                    }

            var list = custs.ToList();
            Assert.IsTrue(list.Count > 0);


        }

        [Test(Description = "This sample demonstrates how navigating through relationships in retrieved objects can result in triggering new queries to the database if the data was not requested by the original query.")]
        public void LinqToSqlObject05()
        {
            var db = CreateDB();

            var emps = from e in db.Employees select e;

            foreach (var emp in emps)
                foreach (var man in emp.Employees)
                    Console.WriteLine("Employee {0} reported to Manager {1}.", emp.FirstName, man.FirstName);

            var list = emps.ToList();
            Assert.IsTrue(list.Count > 0);
        }



        [Test(Description = "This sample demonstrates how navigating through Link in retrieved objects can end up triggering new queries to the database if the data type is Link.")]
        public void LinqToSqlObject06()
        {
            var db = CreateDB();

            var emps = from c in db.Employees select c;

            foreach (var emp in emps)
                Console.WriteLine("{0}", emp.Notes);

            var list = emps.ToList();
            Assert.IsTrue(list.Count > 0);
        }
    }
}
