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
    public class ExpressionTest_AndAlso
    {
        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void Arg1Null ()
        {
            Expression.AndAlso (null, Expression.Constant (1));
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void Arg2Null ()
        {
            Expression.AndAlso (Expression.Constant (1), null);
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void NoOperatorClass ()
        {
            Expression.AndAlso (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void Double ()
        {
            Expression.AndAlso (Expression.Constant (1.0), Expression.Constant (2.0));
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void Integer ()
        {
            Expression.AndAlso (Expression.Constant (1), Expression.Constant (2));
        }

        [Test]
        [ExpectedException (typeof (InvalidOperationException))]
        public void MismatchedTypes ()
        {
            Expression.AndAlso (Expression.Constant (new OpClass ()), Expression.Constant (true));
        }

        [Test]
        public void Boolean ()
        {
            BinaryExpression expr = Expression.AndAlso (Expression.Constant (true), Expression.Constant (false));
            Assert.AreEqual (ExpressionType.AndAlso, expr.NodeType, "AndAlso#01");
            Assert.AreEqual (typeof (bool), expr.Type, "AndAlso#02");
            Assert.IsNull (expr.Method, "AndAlso#03");
            Assert.AreEqual ("(True && False)", expr.ToString(), "AndAlso#04");
        }

        [Test]
        public void UserDefinedClass ()
        {
            // We can use the simplest version of GetMethod because we already know only one
            // exists in the very simple class we're using for the tests.
            MethodInfo mi = typeof (OpClass).GetMethod ("op_BitwiseAnd");

            BinaryExpression expr = Expression.AndAlso (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
            Assert.AreEqual (ExpressionType.AndAlso, expr.NodeType, "AndAlso#05");
            Assert.AreEqual (typeof (OpClass), expr.Type, "AndAlso#06");
            Assert.AreEqual (mi, expr.Method, "AndAlso#07");
            Assert.AreEqual ("op_BitwiseAnd", expr.Method.Name, "AndAlso#08");
            Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) && value(MonoTests.System.Linq.Expressions.OpClass))",
                expr.ToString(), "AndAlso#09");
        }
    }
}
