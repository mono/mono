using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test_NUnit;

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
    /// <summary>
    /// Source:  http://msdn2.microsoft.com/en-us/vbasic/bb737931.aspx
    /// manually translated from VB into C#.
    /// </summary>
    [TestFixture]
    public class Object_Identity : TestBase
    {

        /// <summary>
        /// This sample demonstrates how, upon executing the same query twice, you will receive a reference to the same object in memory each time.
        /// </summary>
        [Test(Description = "Object Caching - 1.")]
        public void LinqToSqlObjectIdentity01()
        {
            Northwind db = CreateDB();

            Customer cust1 = db.Customers.First(c => c.CustomerID == "BONAP");

            Customer cust2 = (from c in db.Customers
                              where c.CustomerID == "BONAP"
                              select c).First();

            bool isSameObject = Object.ReferenceEquals(cust1, cust2);
            Assert.IsTrue(isSameObject);
            Assert.IsTrue(cust1.CustomerID == "BONAP", "CustomerID must be BONAP - was: " + cust1.CustomerID);
        }

        [Test(Description="Example 2 from msdn")]
        public void MSDN_ObjectIdentity2()
        {
            //source: http://msdn2.microsoft.com/en-us/library/bb399376.aspx
            Northwind db = CreateDB();

            Customer cust1 =
                (from cust in db.Customers
                 where cust.CustomerID == "BONAP"
                 select cust).First();

            Customer cust2 =
                (from ord in db.Orders
                 where ord.Customer.CustomerID == "BONAP"
                 select ord).First().Customer;

            bool isSameObject = Object.ReferenceEquals(cust1, cust2);
            Assert.IsTrue(isSameObject);
        }

    }
}
