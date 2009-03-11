using System.Collections.Generic;
using System.Reflection;
using DbLinq.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq.Expressions;
using NUnit.Framework;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace DbLinqTest
{
    /// <summary>
    ///This is a test class for ExpressionEqualityComparerTest and is intended
    ///to contain all ExpressionEqualityComparerTest Unit Tests
    ///</summary>
    [TestClass, TestFixture]
    public class ExpressionEqualityComparerTest
    {
        private readonly IEqualityComparer<Expression> equalityComparer = new ExpressionEqualityComparer();

        private void CheckEquality(Expression a, Expression b)
        {
            Assert.AreEqual(equalityComparer.GetHashCode(a), equalityComparer.GetHashCode(b));
            Assert.IsTrue(equalityComparer.Equals(a, b));
        }

        private void CheckInequality(Expression a, Expression b)
        {
            Assert.IsFalse(equalityComparer.Equals(a, b));
        }

        [TestMethod, Test]
        public void Equality1Test()
        {
            CheckEquality(Expression.Add(Expression.Constant(1), Expression.Constant(2)),
                          Expression.Add(Expression.Constant(1), Expression.Constant(2)));
        }

        [TestMethod, Test]
        public void Inequality1Test()
        {
            CheckInequality(Expression.Add(Expression.Constant(1), Expression.Constant(2)),
                            Expression.Add(Expression.Constant(1), Expression.Constant(3)));
        }

        [TestMethod, Test]
        public void Equality2Test()
        {
            CheckEquality(Expression.Condition(Expression.Constant(true), Expression.Constant(1), Expression.Constant(2)),
                          Expression.Condition(Expression.Constant(true), Expression.Constant(1), Expression.Constant(2)));
        }

        [TestMethod, Test]
        public void Equality3Test()
        {
            CheckEquality(Expression.Constant(1), Expression.Constant(1));
        }

        [TestMethod, Test]
        public void Equality4Test()
        {
            CheckEquality(Expression.Constant("1"), Expression.Constant("1"));
        }

        [TestMethod, Test]
        public void Inequality4Test()
        {
            CheckInequality(Expression.Constant(1), Expression.Constant("1"));
        }
        [TestMethod, Test]
        public void Inequality5Test()
        {
            CheckInequality(Expression.Constant(1), null);
        }
        [TestMethod, Test]
        public void Inequality6Test()
        {
            CheckInequality(null, Expression.Constant("1"));
        }
        [TestMethod, Test]
        public void Inequality7Test()
        {
            CheckInequality(Expression.Constant(1), Expression.Negate(Expression.Constant(1)));
        }
        static int F()
        {
            return 1;
        }

        static int G()
        {
            return 1;
        }

        //[TestMethod, Test]
        //public void Equality8Test()
        //{
        //    CheckEquality(
        //        Expression.Invoke(Expression.Call(GetType().GetMethod("F", BindingFlags.NonPublic | BindingFlags.Static))),
        //        Expression.Invoke(Expression.Call(GetType().GetMethod("F", BindingFlags.NonPublic | BindingFlags.Static)))
        //        );
        //}

        //[TestMethod, Test]
        //public void Inequality8Test()
        //{
        //    CheckInequality(
        //        Expression.Invoke(Expression.Call(GetType().GetMethod("F", BindingFlags.NonPublic | BindingFlags.Static))),
        //        Expression.Invoke(Expression.Call(GetType().GetMethod("G", BindingFlags.NonPublic | BindingFlags.Static)))
        //        );
        //}
        [TestMethod, Test]
        public void Equality9Test()
        {
            CheckEquality(
                Expression.Call(GetType().GetMethod("F", BindingFlags.NonPublic | BindingFlags.Static)),
                Expression.Call(GetType().GetMethod("F", BindingFlags.NonPublic | BindingFlags.Static))
                );
        }

        [TestMethod, Test]
        public void Inequality9Test()
        {
            CheckInequality(
                Expression.Call(GetType().GetMethod("F", BindingFlags.NonPublic | BindingFlags.Static)),
                Expression.Call(GetType().GetMethod("G", BindingFlags.NonPublic | BindingFlags.Static))
                );
        }

        // TODO: finish tests, lazy boy
    }
}
