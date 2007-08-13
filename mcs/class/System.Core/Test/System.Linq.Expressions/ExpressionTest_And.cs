// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//        Federico Di Gregorio <fog@initd.org>

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{    
    [TestFixture]
    public class ExpressionTest_And
    {
        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void Arg1Null ()
        {
            Expression.And (null, Expression.Constant (1));
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void Arg2Null ()
        {
            Expression.And (Expression.Constant (1), null);
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void NoOperatorClass ()
        {
            Expression.And (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void ArgTypesDifferent ()
        {
            Expression.AndAlso (Expression.Constant (1), Expression.Constant (true));
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void Double ()
        {
            Expression.And (Expression.Constant (1.0), Expression.Constant (2.0));
        }

        [Test]
        public void Integer ()
        {
            BinaryExpression expr = Expression.And (Expression.Constant (1), Expression.Constant (2));
            Assert.AreEqual (ExpressionType.And, expr.NodeType, "And#01");
            Assert.AreEqual (typeof (int), expr.Type, "And#02");
            Assert.IsNull (expr.Method, "And#03");
            Assert.AreEqual ("(1 & 2)", expr.ToString(), "And#04");
        }

        [Test]
        public void Boolean ()
        {
            BinaryExpression expr = Expression.And (Expression.Constant (true), Expression.Constant (false));
            Assert.AreEqual (ExpressionType.And, expr.NodeType, "And#05");
            Assert.AreEqual (typeof (bool), expr.Type, "And#06");
            Assert.IsNull (expr.Method, "And#07");
            Assert.AreEqual ("(True And False)", expr.ToString(), "And#08");
        }

        [Test]
        public void UserDefinedClass ()
        {
            // We can use the simplest version of GetMethod because we already know only one
            // exists in the very simple class we're using for the tests.
            MethodInfo mi = typeof (OpClass).GetMethod ("op_BitwiseAnd");
            
            BinaryExpression expr = Expression.And (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
            Assert.AreEqual (ExpressionType.And, expr.NodeType, "And#09");
            Assert.AreEqual (typeof (OpClass), expr.Type, "And#10");
            Assert.AreEqual (mi, expr.Method, "And#11");
            Assert.AreEqual ("op_BitwiseAnd", expr.Method.Name, "And#12");
            Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) & value(MonoTests.System.Linq.Expressions.OpClass))",
                expr.ToString(), "And#13");
        }
    }
}
