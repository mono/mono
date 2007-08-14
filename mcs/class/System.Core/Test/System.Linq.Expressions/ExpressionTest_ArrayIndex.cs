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
    public class ExpressionTest_ArrayIndex
    {
        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void Arg1Null ()
        {
            Expression.ArrayIndex (null, Expression.Constant (1));
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void Arg2Null1 ()
        {
            Expression.ArrayIndex (Expression.Constant (new int[1]), (Expression)null);
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void Arg2Null2 ()
        {
            Expression.ArrayIndex (Expression.Constant (new int[1]), (IEnumerable<Expression>)null);
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void Arg2Null3 ()
        {
            Expression.ArrayIndex (Expression.Constant (new int[1]), (Expression[])null);
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void Arg2WrongType1 ()
        {
            Expression.ArrayIndex (Expression.Constant (new int[1]), Expression.Constant (true));
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void Arg1NotArray ()
        {
            Expression.ArrayIndex (Expression.Constant ("This is not an array!"), Expression.Constant (1));
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void Arg2WrongType2 ()
        {
            Expression[] indexes = { Expression.Constant (1), Expression.Constant (1L) };

            Expression.ArrayIndex (Expression.Constant (new int[1,1]), indexes);
        }

        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void Arg2WrongNumber1 ()
        {
            Expression[] indexes = { Expression.Constant (1), Expression.Constant (0) };

            Expression.ArrayIndex (Expression.Constant (new int[1]), indexes);
        }

        [Test]
        public void Rank1Struct ()
        {
            int[] array = { 42 };
            
            BinaryExpression expr = Expression.ArrayIndex (Expression.Constant (array), Expression.Constant(0));
            Assert.AreEqual (ExpressionType.ArrayIndex, expr.NodeType, "ArrayIndex#01");
            Assert.AreEqual (typeof (int), expr.Type, "ArrayIndex#02");
            Assert.IsNull (expr.Method, "ArrayIndex#03");
            Assert.AreEqual ("value(System.Int32[])[0]", expr.ToString(), "ArrayIndex#04");
        }

        [Test]
        public void Rank1UserDefinedClass ()
        {
            NoOpClass[] array = { new NoOpClass() };
            
            BinaryExpression expr = Expression.ArrayIndex (Expression.Constant (array), Expression.Constant(0));
            Assert.AreEqual (ExpressionType.ArrayIndex, expr.NodeType, "ArrayIndex#05");
            Assert.AreEqual (typeof (NoOpClass), expr.Type, "ArrayIndex#06");
            Assert.IsNull (expr.Method, "ArrayIndex#07");
            Assert.AreEqual ("value(MonoTests.System.Linq.Expressions.NoOpClass[])[0]", expr.ToString(), "ArrayIndex#08");
        }

        [Test]
        public void Rank2Struct ()
        {
            int[,] array = { {42}, {42} };
            Expression[] indexes = { Expression.Constant (1), Expression.Constant (0) };
            
            MethodCallExpression expr = Expression.ArrayIndex (Expression.Constant (array), indexes);
            Assert.AreEqual (ExpressionType.Call, expr.NodeType, "ArrayIndex#09");
            Assert.AreEqual (typeof (int), expr.Type, "ArrayIndex#10");
            Assert.AreEqual ("value(System.Int32[,]).Get(1, 0)", expr.ToString(), "ArrayIndex#12");
        }

        [Test]
        public void Rank2UserDefinedClass ()
        {
            NoOpClass[,] array = { {new NoOpClass()}, {new NoOpClass()} };
            Expression[] indexes = { Expression.Constant (1), Expression.Constant (0) };
            
            MethodCallExpression expr = Expression.ArrayIndex (Expression.Constant (array), indexes);
            Assert.AreEqual (ExpressionType.Call, expr.NodeType, "ArrayIndex#13");
            Assert.AreEqual (typeof (NoOpClass), expr.Type, "ArrayIndex#14");
            Assert.AreEqual ("value(MonoTests.System.Linq.Expressions.NoOpClass[,]).Get(1, 0)", expr.ToString(), "ArrayIndex#16");
        }
    }
}
