//
// ExpressionTest_UnaryPlus.cs
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
	public class ExpressionTest_UnaryPlus
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.UnaryPlus (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgNotStatic ()
		{
			Expression.UnaryPlus (Expression.Constant (new object ()), typeof (OpClass).GetMethod ("WrongUnaryNotStatic"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgParameterCount ()
		{
			Expression.UnaryPlus (Expression.Constant (new object ()), typeof (OpClass).GetMethod ("WrongUnaryParameterCount"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgReturnsVoid ()
		{
			Expression.UnaryPlus (Expression.Constant (new object ()), typeof (OpClass).GetMethod ("WrongUnaryReturnVoid"));
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void PlusBool ()
		{
			Expression.UnaryPlus (true.ToConstant ());
		}

		[Test]
		public void Number ()
		{
			var up = Expression.UnaryPlus (1.ToConstant ());
			Assert.AreEqual ("+1", up.ToString ());
		}

		[Test]
		public void UserDefinedClass ()
		{
			var mi = typeof (OpClass).GetMethod ("op_UnaryPlus");

			var expr = Expression.UnaryPlus (Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.UnaryPlus, expr.NodeType);
			Assert.AreEqual (typeof (OpClass), expr.Type);
			Assert.AreEqual (mi, expr.Method);
			Assert.AreEqual ("op_UnaryPlus", expr.Method.Name);
			Assert.AreEqual ("+value(MonoTests.System.Linq.Expressions.OpClass)",	expr.ToString ());
		}

		[Test]
		public void PlusNullableInt32 ()
		{
			var n = Expression.UnaryPlus (Expression.Parameter (typeof (int?), ""));
			Assert.AreEqual (typeof (int?), n.Type);
			Assert.IsTrue (n.IsLifted);
			Assert.IsTrue (n.IsLiftedToNull);
			Assert.IsNull (n.Method);
		}

		[Test]
		public void CompilePlusInt32 ()
		{
			var p = Expression.Parameter (typeof (int), "i");
			var plus = Expression.Lambda<Func<int, int>> (Expression.UnaryPlus (p), p).Compile ();

			Assert.AreEqual (-2, plus (-2));
			Assert.AreEqual (0, plus (0));
			Assert.AreEqual (3, plus (3));
		}
	}
}
