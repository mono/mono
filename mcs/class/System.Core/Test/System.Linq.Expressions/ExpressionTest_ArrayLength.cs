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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{    
    [TestFixture]
    public class ExpressionTest_ArrayLength
    {
        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void Arg1Null ()
        {
            Expression.ArrayLength (null);
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void Arg1NotArray ()
        {
            Expression.ArrayLength (Expression.Constant ("This is not an array!"));
        }

        [Test]
        public void Rank1String ()
        {
            string[] array = { "a", "b", "c" };
            
            UnaryExpression expr = Expression.ArrayLength (Expression.Constant (array));
            Assert.AreEqual (ExpressionType.ArrayLength, expr.NodeType, "ArrayLength#01");
            Assert.AreEqual (typeof (int), expr.Type, "ArrayLength#02");
            Assert.IsNull (expr.Method, "ArrayLength#03");
            Assert.AreEqual ("ArrayLength(value(System.String[]))", expr.ToString(), "ArrayLength#04");
        }

        [Test]
        public void Rank2String ()
        {
            string[,] array = {{ "a", "b", "c" }, { "a", "b", "c" }};
            
            UnaryExpression expr = Expression.ArrayLength (Expression.Constant (array));
            Assert.AreEqual (ExpressionType.ArrayLength, expr.NodeType, "ArrayLength#05");
            Assert.AreEqual (typeof (int), expr.Type, "ArrayLength#06");
            Assert.IsNull (expr.Method, "ArrayLength#07");
            Assert.AreEqual ("ArrayLength(value(System.String[,]))", expr.ToString(), "ArrayLength#08");
        }
    }
}
