//
// ExpressionTest_GreaterThan.cs
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// (C) 2008 Novell, Inc. (http://www.novell.com)
//
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
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions
{
	[TestFixture]
	public class ExpressionTest_GreaterThan
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.GreaterThan (null, Expression.Constant (1));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.GreaterThan (Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void NoOperatorClass ()
		{
			Expression.GreaterThan (Expression.Constant (new NoOpClass ()), Expression.Constant (new NoOpClass ()));
		}

		[Test]
		public void Double ()
		{
			var expr = Expression.GreaterThan (Expression.Constant (2.0), Expression.Constant (1.0));
			Assert.AreEqual (ExpressionType.GreaterThan, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.IsNull (expr.Method);
			Assert.AreEqual ("(2 > 1)", expr.ToString ());
		}

		[Test]
		public void Integer ()
		{
			var expr = Expression.GreaterThan (Expression.Constant (2), Expression.Constant (1));
			Assert.AreEqual (ExpressionType.GreaterThan, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.IsNull (expr.Method);
			Assert.AreEqual ("(2 > 1)", expr.ToString ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void MismatchedTypes ()
		{
			Expression.GreaterThan (Expression.Constant (new OpClass ()), Expression.Constant (true));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void Boolean ()
		{
			Expression.GreaterThan (Expression.Constant (true), Expression.Constant (false));
		}

		[Test]
		public void UserDefinedClass ()
		{
			MethodInfo mi = typeof (OpClass).GetMethod ("op_GreaterThan");

			Assert.IsNotNull (mi);

			BinaryExpression expr = Expression.GreaterThan (Expression.Constant (new OpClass ()), Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.GreaterThan, expr.NodeType);
			Assert.AreEqual (typeof (bool), expr.Type);
			Assert.AreEqual (mi, expr.Method);
			Assert.AreEqual ("op_GreaterThan", expr.Method.Name);
			Assert.AreEqual ("(value(MonoTests.System.Linq.Expressions.OpClass) > value(MonoTests.System.Linq.Expressions.OpClass))", expr.ToString ());
		}
	}
}
