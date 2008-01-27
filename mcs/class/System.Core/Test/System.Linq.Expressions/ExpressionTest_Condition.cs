//
// ExpressionTest.cs
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

namespace MonoTests.System.Linq.Expressions {

	[TestFixture]
	public class ExpressionTest_Condition {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg1Null ()
		{
			Expression.Condition (null, Expression.Constant (1), Expression.Constant (0));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg2Null ()
		{
			Expression.Condition (Expression.Equal (Expression.Constant (42), Expression.Constant (42)), null, Expression.Constant (0));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void Arg3Null ()
		{
			Expression.Condition (Expression.Equal (Expression.Constant (42), Expression.Constant (42)), Expression.Constant (1), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TestNotBool ()
		{
			Expression.Condition (Expression.Constant (42), Expression.Constant (1), Expression.Constant (0));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void TrueBlockTypeNotFalseBlockType ()
		{
			Expression.Condition (Expression.Constant (42), Expression.Constant (1.1), Expression.Constant (0));
		}

		[Test]
		public void TestSimpleConditional ()
		{
			var cond = Expression.Condition (Expression.GreaterThan (Expression.Constant (2), Expression.Constant (1)), Expression.Constant (1), Expression.Constant (0));
			Assert.AreEqual (typeof (bool), cond.Test.Type);
			Assert.AreEqual ("IIF((2 > 1), 1, 0)", cond.ToString ());
		}

		[Test]
		public void CompileConditional ()
		{
			var parameters = new [] { Expression.Parameter (typeof (int), "number") };

			var l = Expression.Lambda<Func<int, string>> (
						Expression.Condition (
							Expression.GreaterThanOrEqual (
								parameters [0],
								Expression.Constant (0)),
							Expression.Constant ("+"),
							Expression.Constant ("-")),
						parameters);

			var gt = l.Compile ();

			Assert.AreEqual ("+", gt (1));
			Assert.AreEqual ("+", gt (0));
			Assert.AreEqual ("-", gt (-1));
		}
	}
}
