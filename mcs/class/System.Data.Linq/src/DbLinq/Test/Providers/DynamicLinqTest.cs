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
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Linq.Dynamic;
using Test_NUnit;
using System.Linq.Expressions;
using System.Reflection;

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
    public class DynamicLinqTest : TestBase
    {
        [Test]
        public void DL1_Products()
        {
            Northwind db = CreateDB();

            var q = db.Products.Where("SupplierID=1 And UnitsInStock>2")
                .OrderBy("ProductID");
            var list = q.ToList();
            Assert.IsTrue(list.Count > 0, "Expected results from dynamic query");
        }

        [Test]
        public void DL2_ProductCount()
        {
            Northwind db = CreateDB();

            int numProducts = db.Products.Where("SupplierID=1").Count();
            Assert.IsTrue(numProducts > 0, "Expected results from dynamic query");
        }

        //note:
        //user Sqlite reports problems with DynamicLinq Count() -
        //but neither DL2 nor DL3 tests seem to hit the problem.

        [Test]
        public void DL3_ProductCount()
        {
            Northwind db = CreateDB();

            int numProducts = db.Products.Count();
            Assert.IsTrue(numProducts > 0, "Expected results from dynamic query");
        }

        [Test]
        public void DL4_DynamicAssociationProperty()
        {

            Northwind db = CreateDB();
            var orders = db.GetTable<Order>();
            var res = orders.Select(@"new (OrderID,Customer.ContactName)");

            List<object> list = new List<object>();
            foreach (var u in res)
                list.Add(u);
            Assert.IsTrue(list.Count > 0);
        }

        #region NestedPropertiesDynamicSelect

        const string obsoleteError=@"Since beta2 in Linq2Sql to project a new entity (ie: select new Order(3)) is forbidden for coherence reasons, so this tests doesn't mimic the Linq2Sql behavior and it is obsolete and should be modified. If you apply such test cases to Linq2Sql you'll get Test_NUnit_MsSql_Strict.DynamicLinqTest.DL5_NestedObjectSelect:
        System.NotSupportedException : Explicit construction of entity type 'MsNorthwind.XX' in query is not allowed.\n\nMore Info in: http://linqinaction.net/blogs/roller/archive/2007/11/27/explicit-construction-of-entity-type-in-query-is-not-allowed.aspx";
        [Test(Description = "dynamic version of F16_NestedObjectSelect")]
        public void DL5_NestedObjectSelect()
        {
            Assert.Ignore(obsoleteError);
            Northwind db = CreateDB();
            var orders = db.GetTable<Order>();
            var res = orders.SelectNested(new string[] { "OrderID", "Customer.ContactName" });

            List<Order> list = res.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void DL6_StaticVersionOfDynamicAssociatonWithExtensionMethodTest(bool bug_in_dynamic_linq)
        {
            Assert.Ignore(obsoleteError);

            //is this maybe a bug in DynamicLinq?
            //from DynamicLinq, we receive this query which has ContactName but misses ContactTitle:
            //MTable.CreateQuery: value(Table`1[Order]).Select(o => new Order() {OrderID = o.OrderID, Customer = new Customer() {ContactName = o.Customer.ContactName}})

            //Also - the non-dynamic version F17_NestedObjectSelect_Ver2 succeeds.

            Northwind db = CreateDB();
            var orders = db.GetTable<Order>().ToArray().AsQueryable();

            var query = from order in orders
                        //where order.Customer != null
                        select new Order
                        {
                            OrderID = order.OrderID,
                            Customer = new Customer
                            {
                                ContactName = order.Customer.ContactName,
                                ContactTitle = order.Customer.ContactTitle
                            }
                        };
            var list = query.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [Test]
        public void DL7_DynamicAssociatonUsingDoubleProjection(bool bug_in_dynamic_linq)
        {
            Assert.Ignore(obsoleteError);

            //this fails - but not in our code:
            //A first chance exception of type 'System.NullReferenceException' occurred in Unknown Module.
            //System.Transactions Critical: 0 : <TraceRecord xmlns="http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord" Severity="Critical"><TraceIdentifier>http://msdn.microsoft.com/TraceCodes/System/ActivityTracing/2004/07/Reliability/Exception/Unhandled</TraceIdentifier><Description>Unhandled exception</Description><AppDomain>Test_NUnit_Mysql.vshost.exe</AppDomain><Exception><ExceptionType>System.NullReferenceException, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</ExceptionType><Message>Object reference not set to an instance of an object.</Message><StackTrace>   at lambda_method(ExecutionScope , Order )
            //   at System.Linq.Enumerable.&amp;lt;SelectIterator&amp;gt;d__d`2.MoveNext()
            //   at System.Collections.Generic.List`1..ctor(IEnumerable`1 collection)
            //   at System.Linq.Enumerable.ToList[TSource](IEnumerable`1 source)
            //   at Test_NUnit_MySql.DynamicLinqTest.DL7_DynamicAssociatonUsingDoubleProjection() in E:\ggprj\dbLinq\dblinq2007\Tests\Test_NUnit\DynamicLinqTest.cs:line 150

            Northwind db = CreateDB();

            // Double projection works in Linq-SQL:
            var orders = db.GetTable<Order>().ToArray().AsQueryable();
            var query = orders.SelectNested(new string[] { "OrderID", "Customer.ContactName" });
            var list = query.ToList();
            Assert.IsTrue(list.Count > 0);
        }

        #endregion

        #region Predicates

        /// <summary>
        /// Reported by pwy.mail in issue http://code.google.com/p/dblinq2007/issues/detail?id=68
        /// </summary>
        [Test]
        public void DL8_CountTest2()
        {
            Northwind db = CreateDB();
            Expression<Func<Customer, bool>> predicate = c => c.City == "Paris";
            int count = db.Customers.Count(predicate);
            Assert.Greater(count, 0); // Some databases have more than 1 customer in Paris
        }

        /// <summary>
        /// Reported by pwy.mail in issue http://code.google.com/p/dblinq2007/issues/detail?id=69
        /// </summary>
        [Test]
        public void DL9_PredicateBuilderCount()
        {
            //2008.May.17: breaks because we are not handling an 'InvocationExpression' in ExpressionTreeParser.
            //possibily a tree rewrite is needed.
            Northwind db = CreateDB();
            var predicate = PredicateBuilder.True<Customer>();
            predicate = predicate.And(m => m.City == "Paris");
            int count = db.Customers.Count(predicate);
            Assert.AreEqual(1, count);
        }


        /// <summary>
        /// Reported by pwy.mail in issue http://code.google.com/p/dblinq2007/issues/detail?id=69
        /// </summary>
        [Test]
        public void DL10_PredicateBuilderWhere()
        {
            Northwind db = CreateDB();
            var predicate = PredicateBuilder.True<Customer>();

            predicate = predicate.And(m => m.City == "Paris");
            predicate = predicate.And(n => n.CompanyName == "Around the Horn");
            IList<Customer> list = db.Customers.AsQueryable().Where(predicate).ToList();
        }

        /// <summary>
        /// Reported by pwy.mail in issue http://code.google.com/p/dblinq2007/issues/detail?id=73
        /// </summary>
        [Test]
        public void DL11_ThenByDescending()
        {
            Northwind db = CreateDB();
            var q = db.Products.Where("SupplierID=1 And UnitsInStock>2")
                .OrderBy(" ProductName asc,ProductID desc");
            var list = q.ToList();
            Assert.IsTrue(list.Count > 0, "Expected results from dynamic query");
        }

        /// <summary>
        /// Build predicate expressions dynamically.
        /// </summary>
        static class PredicateBuilder
        {
            public static Expression<Func<T, bool>> True<T>() { return f => true; }
            public static Expression<Func<T, bool>> False<T>() { return f => false; }
        }

    }
        #endregion

    #region ExtensionMethods

    /// <summary>
    /// Extension written by Marc Gravell.
    /// Traverses nested properties
    /// </summary>
    static class SelectUsingSingleProjection
    {
        internal static IQueryable<T> SelectNested<T>(this IQueryable<T> source, params string[] propertyNames)
            where T : new()
        {
            Type type = typeof(T);
            var sourceItem = Expression.Parameter(type, "t");
            Expression exp = CreateAndInit(type, sourceItem, propertyNames);
            return source.Select(Expression.Lambda<Func<T, T>>(exp, sourceItem));
        }

        static Expression CreateAndInit(Type type, Expression source, string[] propertyNames)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (source == null) throw new ArgumentNullException("source");
            if (propertyNames == null) throw new ArgumentNullException("propertyNames");

            var newExpr = Expression.New(type.GetConstructor(Type.EmptyTypes));
            // take "Foo.A", "Bar", "Foo.B" to "Foo" ["A","B"], "Bar" []
            var groupedNames = from name in propertyNames
                               let dotIndex = name.IndexOf('.')
                               let primary = dotIndex < 0 ? name : name.Substring(0, dotIndex)
                               let aux = dotIndex < 0 ? null : name.Substring(dotIndex + 1)
                               group aux by primary into grouped
                               select new
                               {
                                   Primary = grouped.Key,
                                   Aux = grouped.Where(x => x != null).ToArray()
                               };
            List<MemberBinding> bindings = new List<MemberBinding>();
            foreach (var grp in groupedNames)
            {
                PropertyInfo dest = type.GetProperty(grp.Primary);
                Expression value, readFrom = Expression.Property(source, grp.Primary);
                if (grp.Aux.Length == 0)
                {
                    value = readFrom;
                }
                else
                {
                    value = CreateAndInit(dest.PropertyType, readFrom, grp.Aux);
                }
                bindings.Add(Expression.Bind(dest, value));
            }
            return Expression.MemberInit(newExpr, bindings);
        }


        /// <summary>
        /// Extension method provided by pwy.mail in issue http://code.google.com/p/dblinq2007/issues/detail?id=69
        /// </summary>
        internal static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                        Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }

        internal static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
    }
    #endregion

}
