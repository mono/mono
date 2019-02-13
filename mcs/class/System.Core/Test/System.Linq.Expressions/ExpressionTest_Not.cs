//
// ExpressionTest_Not.cs
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
	[Category("SRE")]
	public class ExpressionTest_Not
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.Not (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgNotStatic ()
		{
			Expression.Not (Expression.Constant (new object ()), typeof (OpClass).GetMethod ("WrongUnaryNotStatic"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgParameterCount ()
		{
			Expression.Not (Expression.Constant (new object ()), typeof (OpClass).GetMethod ("WrongUnaryParameterCount"));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodArgReturnsVoid ()
		{
			Expression.Not (Expression.Constant (new object ()), typeof (OpClass).GetMethod ("WrongUnaryReturnVoid"));
		}

		[Test]
		public void Number ()
		{
			var up = Expression.Not (1.ToConstant ());
			Assert.AreEqual ("Not(1)", up.ToString ());
		}

		[Test]
		public void UserDefinedClass ()
		{
			var mi = typeof (OpClass).GetMethod ("op_LogicalNot");

			var expr = Expression.Not (Expression.Constant (new OpClass ()));
			Assert.AreEqual (ExpressionType.Not, expr.NodeType);
			Assert.AreEqual (typeof (OpClass), expr.Type);
			Assert.AreEqual (mi, expr.Method);
			Assert.AreEqual ("op_LogicalNot", expr.Method.Name);
			Assert.AreEqual ("Not(value(MonoTests.System.Linq.Expressions.OpClass))",	expr.ToString ());
		}

		[Test]
		public void NotNullableInt32 ()
		{
			var n = Expression.Not (Expression.Parameter (typeof (int?), ""));
			Assert.AreEqual (typeof (int?), n.Type);
			Assert.IsTrue (n.IsLifted);
			Assert.IsTrue (n.IsLiftedToNull);
			Assert.IsNull (n.Method);
		}

		[Test]
		public void NotNullableBool ()
		{
			var n = Expression.Not (Expression.Parameter (typeof (bool?), ""));
			Assert.AreEqual (typeof (bool?), n.Type);
			Assert.IsTrue (n.IsLifted);
			Assert.IsTrue (n.IsLiftedToNull);
			Assert.IsNull (n.Method);
		}

		[Test]
		public void CompileNotInt32 ()
		{
			var p = Expression.Parameter (typeof (int), "i");
			var not = Expression.Lambda<Func<int, int>> (Expression.Not (p), p).Compile ();

			Assert.AreEqual (-2, not (1));
			Assert.AreEqual (-4, not (3));
			Assert.AreEqual (2, not (-3));
		}

		[Test]
		public void CompiledNotNullableInt32 ()
		{
			var p = Expression.Parameter (typeof (int?), "i");
			var not = Expression.Lambda<Func<int?, int?>> (Expression.Not (p), p).Compile ();

			Assert.AreEqual (null, not (null));
			Assert.AreEqual ((int?) -4, not (3));
			Assert.AreEqual ((int?) 2, not (-3));
		}

		[Test]
		public void CompileNotBool ()
		{
			var p = Expression.Parameter (typeof (bool), "i");
			var not = Expression.Lambda<Func<bool, bool>> (Expression.Not (p), p).Compile ();

			Assert.AreEqual (false, not (true));
			Assert.AreEqual (true, not (false));
		}

		[Test]
		public void CompiledNotNullableBool ()
		{
			var p = Expression.Parameter (typeof (bool?), "i");
			var not = Expression.Lambda<Func<bool?, bool?>> (Expression.Not (p), p).Compile ();

			Assert.AreEqual ((bool?) null, not (null));
			Assert.AreEqual ((bool?) false, not (true));
			Assert.AreEqual ((bool?) true, not (false));
		}

		struct Slot {
			public int Value;

			public Slot (int value)
			{
				this.Value = value;
			}

			public static bool operator ! (Slot s)
			{
				return s.Value > 0;
			}
		}

		[Test]
		public void UserDefinedNotNullable ()
		{
			var s = Expression.Parameter (typeof (Slot?), "s");
			var node = Expression.Not (s);
			Assert.IsTrue (node.IsLifted);
			Assert.IsTrue (node.IsLiftedToNull);
			Assert.AreEqual (typeof (bool?), node.Type);
			Assert.AreEqual (typeof (Slot).GetMethod ("op_LogicalNot"), node.Method);

			var not = Expression.Lambda<Func<Slot?, bool?>> (node, s).Compile ();

			Assert.AreEqual (null, not (null));
			Assert.AreEqual (true, not (new Slot (1)));
			Assert.AreEqual (false, not (new Slot (0)));
		}
	}
}
