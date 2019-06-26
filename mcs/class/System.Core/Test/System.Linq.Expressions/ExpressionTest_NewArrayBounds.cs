//
// ExpressionTest_NewArrayBounds.cs
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
	[Category("SRE")]
	public class ExpressionTest_NewArrayBounds {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgTypeNull ()
		{
			Expression.NewArrayBounds (null, new Expression [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgBoundsNull ()
		{
			Expression.NewArrayBounds (typeof (int), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ArgBoundsContainsExpressionTypeNotInteger ()
		{
			Expression.NewArrayBounds (typeof (int), 1.ToConstant (), "2".ToConstant ());
		}

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (ArgumentException))]
		public void NewVoid ()
		{
			Expression.NewArrayBounds (typeof (void), 1.ToConstant ());
		}

		[Test]
		public void TestArrayBounds ()
		{
			var ab = Expression.NewArrayBounds (typeof (int), 1.ToConstant (), 2.ToConstant ());
			Assert.AreEqual (typeof (int [,]), ab.Type);
			Assert.AreEqual (2, ab.Expressions.Count);
			Assert.AreEqual ("new System.Int32[,](1, 2)", ab.ToString ());
		}

		static Func<object> CreateNewArrayFactory<T> (params int [] bounds)
		{
			return Expression.Lambda<Func<object>> (
				Expression.NewArrayBounds (
					typeof (T),
					(from bound in bounds select bound.ToConstant ()).ToArray ())).Compile ();
		}

		[Test]
		public void TestArrayAssignability ()
		{
			Expression.Lambda<Func<int []>> (
				Expression.NewArrayBounds (
					typeof (int),
					4.ToConstant ()));
		}

		[Test]
		public void CompileNewArraySingleDimensional ()
		{
			var factory = CreateNewArrayFactory<int> (3);

			var array = (int []) factory ();
			var type = array.GetType ();

			Assert.IsNotNull (array);
			Assert.AreEqual (3, array.Length);
			Assert.IsTrue (type.IsArray);
			Assert.AreEqual (1, type.GetArrayRank ());
		}

		[Test]
		public void CompileNewArrayMultiDimensional ()
		{
			var factory = CreateNewArrayFactory<int> (3, 3);

			var array = (int [,]) factory ();
			var type = array.GetType ();

			Assert.IsNotNull (array);
			Assert.IsTrue (type.IsArray);
			Assert.AreEqual (2, type.GetArrayRank ());
			Assert.AreEqual (9, array.Length);
		}
	}
}
