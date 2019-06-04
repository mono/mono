using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Test_NUnit;

using nwind;

#if MONO_STRICT
using System.Data.Linq;
#else
using DbLinq.Data.Linq;
#endif

namespace nwind
{
    public partial class Customer
    {
        public object ExtraneousMethod()
        {
            return null;
        }
    }
}

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
    public class DataLoadOptions_Test : TestBase
    {
        static object ThrowException()
        {
            throw new ApplicationException();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoadWith_BadExpression1()
        {
            new DataLoadOptions().LoadWith<Customer>(cc => cc.ExtraneousMethod());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoadWith_BadExpression2()
        {
            new DataLoadOptions().LoadWith<Customer>(cc => 1);
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoadWith_BadExpression3()
        {
            new DataLoadOptions().LoadWith<Customer>(cc => ThrowException());
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoadWith_BadExpression4()
        {
            new DataLoadOptions().LoadWith<Customer>(cc => cc.Orders.Select(o => o));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoadWith_BadExpression5()
        {
            new DataLoadOptions().LoadWith<Order> (o => o.Customer.Orders);
        }

#if !DEBUG && (MSSQL && !L2SQL)
        [Explicit]
#endif
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoadWith_BadCycles1()
        {
            var lo = new DataLoadOptions();
            lo.LoadWith<Customer>(c => c.Orders);
            lo.LoadWith<Order>(o => o.Customer);
        }

#if !DEBUG && (MSSQL && !L2SQL)
        [Explicit]
#endif
        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void LoadWith_BadCycles2()
        {
            var lo = new DataLoadOptions();
            lo.LoadWith<Order>(o => o.Customer);
            lo.LoadWith<Customer>(c => c.Orders);
        }

        [Test]
        public void LoadWith_Good1()
        {
            var lo = new DataLoadOptions();
            lo.LoadWith<Customer>(c => c.Orders);
            lo.LoadWith<Order>(o => o.Employee);
        }

        [Test]
        public void LoadWith_Good2()
        {
            var lo = new DataLoadOptions();
            lo.LoadWith<Order>(o => o.Employee);
            lo.LoadWith<Customer>(c => c.Orders);
        }
    }
}
