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
	public class ExpressionTest {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetFuncTypeArgNull ()
		{
			Expression.GetFuncType (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetFuncTypeArgEmpty ()
		{
			Expression.GetFuncType (new Type [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetFuncTypeArgTooBig ()
		{
			Expression.GetFuncType (new Type [6]);
		}

		[Test]
		public void GetFuncTypeTest ()
		{
			var func = Expression.GetFuncType (new [] {typeof (int)});
			Assert.AreEqual (typeof (Func<int>), func);

			func = Expression.GetFuncType (new [] {typeof (int), typeof (int)});
			Assert.AreEqual (typeof (Func<int, int>), func);

			func = Expression.GetFuncType (new [] {typeof (int), typeof (int), typeof (int)});
			Assert.AreEqual (typeof (Func<int, int, int>), func);

			func = Expression.GetFuncType (new [] {typeof (int), typeof (int), typeof (int), typeof (int)});
			Assert.AreEqual (typeof (Func<int, int, int, int>), func);

			func = Expression.GetFuncType (new [] {typeof (int), typeof (int), typeof (int), typeof (int), typeof (int)});
			Assert.AreEqual (typeof (Func<int, int, int, int, int>), func);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void GetActionTypeArgNull ()
		{
			Expression.GetActionType (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetActionTypeArgEmpty ()
		{
			Expression.GetActionType (new Type [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void GetActionTypeArgTooBig ()
		{
			Expression.GetActionType (new Type [5]);
		}

		[Test]
		public void GetActionTypeTest ()
		{
			var action = Expression.GetActionType (new [] {typeof (int)});
			Assert.AreEqual (typeof (Action<int>), action);

			action = Expression.GetActionType (new [] {typeof (int), typeof (int)});
			Assert.AreEqual (typeof (Action<int, int>), action);

			action = Expression.GetActionType (new [] {typeof (int), typeof (int), typeof (int)});
			Assert.AreEqual (typeof (Action<int, int, int>), action);

			action = Expression.GetActionType (new [] {typeof (int), typeof (int), typeof (int), typeof (int)});
			Assert.AreEqual (typeof (Action<int, int, int, int>), action);
		}
	}
}
