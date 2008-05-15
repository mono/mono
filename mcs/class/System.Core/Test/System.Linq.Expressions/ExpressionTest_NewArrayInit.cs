//
// ExpressionTest_NewArrayInit.cs
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
	public class ExpressionTest_NewArrayInit {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullType ()
		{
			Expression.NewArrayInit (null, new Expression [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullInitializers ()
		{
			Expression.NewArrayInit (typeof (int), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void InitializersContainNull ()
		{
			Expression.NewArrayInit (typeof (int), 1.ToConstant (), null, 3.ToConstant ());
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void WrongInitializer ()
		{
			Expression.NewArrayInit (typeof (int), 1.ToConstant (), "2".ToConstant (), 3.ToConstant ());
		}

		[Test]
		[Category ("NotDotNet")]
		[ExpectedException (typeof (ArgumentException))]
		public void NewVoid ()
		{
			Expression.NewArrayInit (typeof (void), new Expression [0]);
		}

		[Test]
		public void TestArrayInit ()
		{
			var a = Expression.NewArrayInit (typeof (int), 1.ToConstant (), 2.ToConstant (), 3.ToConstant ());
			Assert.AreEqual (typeof (int []), a.Type);
			Assert.AreEqual (3, a.Expressions.Count);
			Assert.AreEqual ("new [] {1, 2, 3}", a.ToString ());
		}

		static Func<T []> CreateArrayInit<T> (T [] ts)
		{
			return Expression.Lambda<Func<T []>> (
				Expression.NewArrayInit (
					typeof (T),
					(from t in ts select t.ToConstant ()).ToArray ())).Compile ();
		}

		static void AssertCreatedArrayIsEqual<T> (params T [] ts)
		{
			var creator = CreateArrayInit (ts);
			var array = creator ();

			Assert.IsTrue (ts.SequenceEqual (array));
		}

		[Test]
		public void CompileInitArrayOfInt ()
		{
			AssertCreatedArrayIsEqual (new int [] { 1, 2, 3, 4 });
		}

		enum Months { Jan, Feb, Mar, Apr };

		[Test]
		public void CompileInitArrayOfEnums ()
		{
			AssertCreatedArrayIsEqual (new Months [] { Months.Jan, Months.Feb, Months.Mar, Months.Apr });
		}

		class Foo {
		}

		[Test]
		public void CompileInitArrayOfClasses ()
		{
			AssertCreatedArrayIsEqual (new Foo [] { new Foo (), new Foo (), new Foo (), new Foo () });
		}

		struct Bar {
			public int bar;
			public Bar (int b) { bar = b; }
		}

		[Test]
		public void CompileInitArrayOfStructs ()
		{
			AssertCreatedArrayIsEqual (new Bar [] { new Bar (1), new Bar (2), new Bar (3), new Bar (4) });
		}
	}
}
