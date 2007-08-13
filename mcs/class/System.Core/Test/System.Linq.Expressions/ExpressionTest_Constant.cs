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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{
    [TestFixture]
    public class ExpressionTest_Constant
    {
        [Test]
        [ExpectedException (typeof (ArgumentException))]
        public void Arg2NotNullable ()
        {
            Expression.Constant(null, typeof(int));
        }

        [Test]
        [ExpectedException (typeof (ArgumentNullException))]
        public void Arg2Null ()
        {
            Expression.Constant (1, null);
        }

        [Test]
        public void NullValue ()
        {
            ConstantExpression expr = Expression.Constant (null);
            Assert.AreEqual (ExpressionType.Constant, expr.NodeType, "Constant#01");
            Assert.IsNull (expr.Value, "Constant#02");
            Assert.AreEqual (typeof (object), expr.Type, "Constant#03");
            Assert.AreEqual ("null", expr.ToString(), "Constant#04");
        }

        [Test]
        public void NullableValue1 ()
        {
            ConstantExpression expr = Expression.Constant (null, typeof(int?));
            Assert.AreEqual (ExpressionType.Constant, expr.NodeType, "Constant#05");
            Assert.IsNull (expr.Value, "Constant#06");
            Assert.AreEqual (typeof (int?), expr.Type, "Constant#07");
            Assert.AreEqual ("null", expr.ToString(), "Constant#08");
        }

        [Test]
        public void NullableValue2 ()
        {
            ConstantExpression expr = Expression.Constant (1, typeof (int?));
            Assert.AreEqual (ExpressionType.Constant, expr.NodeType, "Constant#09");
            Assert.AreEqual (1, expr.Value, "Constant#10");
            Assert.AreEqual (typeof (int?), expr.Type, "Constant#11");
            Assert.AreEqual ("1", expr.ToString(), "Constant#12");
        }

        [Test]
        public void NullableValue3 ()
        {
            ConstantExpression expr = Expression.Constant ((int?)1);
            Assert.AreEqual (ExpressionType.Constant, expr.NodeType, "Constant#13");
            Assert.AreEqual (1, expr.Value, "Constant#14");
            Assert.AreEqual (typeof (int), expr.Type, "Constant#15");
            Assert.AreEqual ("1", expr.ToString(), "Constant#16");
        }

        [Test]
        public void IntegerValue ()
        {
            ConstantExpression expr = Expression.Constant (0);
            Assert.AreEqual (ExpressionType.Constant, expr.NodeType, "Constant#17");
            Assert.AreEqual (0, expr.Value, "Constant#18");
            Assert.AreEqual (typeof (int), expr.Type, "Constant#19");
            Assert.AreEqual ("0", expr.ToString(), "Constant#20");
        }

        [Test]
        public void StringValue ()
        {
            ConstantExpression expr = Expression.Constant ("a string");
            Assert.AreEqual (ExpressionType.Constant, expr.NodeType, "Constant#21");
            Assert.AreEqual ("a string", expr.Value, "Constant#22");
            Assert.AreEqual (typeof (string), expr.Type, "Constant#23");
            Assert.AreEqual ("\"a string\"", expr.ToString(), "Constant#24");
        }

        [Test]
        public void DateTimeValue ()
        {
            ConstantExpression expr = Expression.Constant (new DateTime(1971, 10, 19));
            Assert.AreEqual (ExpressionType.Constant, expr.NodeType, "Constant#25");
            Assert.AreEqual (new DateTime(1971, 10, 19), expr.Value, "Constant#26");
            Assert.AreEqual (typeof (DateTime), expr.Type, "Constant#27");
            Assert.AreEqual (new DateTime(1971, 10, 19).ToString(), expr.ToString(), "Constant#28");
        }

        [Test]
        public void UserClassValue ()
        {
            OpClass oc = new OpClass ();
            ConstantExpression expr = Expression.Constant (oc);
            Assert.AreEqual (ExpressionType.Constant, expr.NodeType, "Constant#29");
            Assert.AreEqual (oc, expr.Value, "Constant#30");
            Assert.AreEqual (typeof (OpClass), expr.Type, "Constant#31");
            Assert.AreEqual ("value(MonoTests.System.Linq.Expressions.OpClass)", expr.ToString(), "Constant#32");
        }
    }
}
