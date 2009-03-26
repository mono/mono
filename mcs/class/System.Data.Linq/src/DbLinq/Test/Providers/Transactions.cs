using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test_NUnit;

using System.Data;
using System.Data.Common;

using nwind;

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
    public class Transactions : TestBase
    {
        [SetUp]
        public void Setup_LinqToSqlInsert07()
        {
            Northwind db = CreateDB();
            var orderDetails =
                 from o in db.OrderDetails
                 where o.Order.CustomerID == "WARTH"
                 select o;

            var order =
                (from o in db.Orders
                 where o.CustomerID == "WARTH"
                 select o).FirstOrDefault();

            if (!orderDetails.Any() || order == null)
            {
                var o = new Order { CustomerID = "WARTH", Employee = db.Employees.First() };
                o.OrderDetails.Add(new OrderDetail { Discount = 0.1f, Quantity = 1, Product = db.Products.First(p => p.ProductID % 2 == 0) });
                o.OrderDetails.Add(new OrderDetail { Discount = 0.2f, Quantity = 1, Product = db.Products.First(p => p.ProductID % 2 != 0) });
                db.Orders.InsertOnSubmit(o);
                db.SubmitChanges();
            }
        }

        [Test]
        public void TransactionRollbackDelete()
        {
            Northwind db = CreateDB();
            DbTransaction t = BeginTransaction(db);

            try
            {
                var orderDetails =
                    from o in db.OrderDetails
                    where o.Order.CustomerID == "WARTH"
                    select o;

                var order =
                    (from o in db.Orders
                     where o.CustomerID == "WARTH"
                     select o).FirstOrDefault();

                if (!orderDetails.Any() || order == null)
                    Assert.Ignore("Preconditions");

                db.OrderDetails.DeleteAllOnSubmit(orderDetails); //formerly Remove(od);

                db.Orders.DeleteOnSubmit(order); //formerly Remove(order);
                db.SubmitChanges();

                Assert.IsFalse(
                    db.OrderDetails.Any(od => od.Order.Customer.CustomerID == "WARTH" && od.Order.EmployeeID == 3));
                Assert.IsFalse(db.Orders.Any(ord => ord.OrderID == order.OrderID));
            }
            finally
            {
                t.Rollback();
            }
        }

        private DbTransaction BeginTransaction(Northwind db)
        {
            db.Connection.Open();
            DbTransaction t = db.Connection.BeginTransaction();
            db.Transaction = t;

            return t;
        }

        [Test]
        public void TransactionCheckAndRollbackInsert()
        {
            Northwind db = CreateDB();
            DbTransaction t = BeginTransaction(db);

            var cust = new Customer();
            int beforeCustomersCount = db.Customers.Count();

            string id = new object().GetHashCode().ToString().Substring(0, 5);
            cust.CustomerID = id;
            cust.Country = "Spain";
            cust.CompanyName = "Coco";

            db.Customers.InsertOnSubmit(cust);
            db.SubmitChanges();

            int afterCustomercount = db.Customers.Count();
            Assert.IsTrue(beforeCustomersCount + 1 == afterCustomercount);

            t.Rollback();

            afterCustomercount = db.Customers.Count();
            Assert.IsTrue(beforeCustomersCount == afterCustomercount);

            var customer = db.Customers.FirstOrDefault(c => c.CustomerID == id);
            Assert.IsNull(customer);
        }
    }
}
