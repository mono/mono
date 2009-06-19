using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test_NUnit;
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
    public class Attach : TestBase
    {
        [Test]
        public void Attach01()
        {
            var db1 = CreateDB();
            var employee = new Employee();

            db1.Employees.Attach(employee);
        }

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void Attach02()
        {
            var db1 = CreateDB();
            var db2 = CreateDB();
            var employee = new Employee();

            db1.Employees.Attach(employee);
            employee.Address = "new address";

            db2.Employees.Attach(employee);
        }

        [Test]
        public void AttachAll01()
        {
            var db1 = CreateDB();
            var employees = new Employee[] { new Employee { EmployeeID = 20 }, new Employee { EmployeeID = 21 } };
            db1.Employees.AttachAll(employees);
        }

        [Test]
        [ExpectedException(typeof(System.Data.Linq.DuplicateKeyException))]
        public void AttachAll02()
        {
            var db1 = CreateDB();
            var employees = new Employee[] { new Employee { EmployeeID = 20 }, new Employee { EmployeeID = 20 } };
            db1.Employees.AttachAll(employees);
        }


        [Test]
        [ExpectedException(typeof(System.Data.Linq.DuplicateKeyException))]
        public void AttachAll03()
        {
            var db1 = CreateDB();
            var employee1 = db1.Employees.First();
            var employees = new Employee[] { new Employee { EmployeeID = employee1.EmployeeID } };
            db1.Employees.AttachAll(employees);
        }

#if !DEBUG && (SQLITE || (MSSQL && !L2SQL))
        [Explicit]
#endif
        [Test]
        [ExpectedException(typeof(ChangeConflictException))]
        public void NotExistingAttatch()
        {
            Random rand = new Random();

            Northwind db = CreateDB();
            var orderDetail = new OrderDetail { OrderID = 0, ProductID = 0 };
            db.OrderDetails.Attach(orderDetail);

            float newDiscount = 15 + (float)rand.NextDouble();
            orderDetail.Discount = newDiscount;
            db.SubmitChanges();
        }
    }
}
