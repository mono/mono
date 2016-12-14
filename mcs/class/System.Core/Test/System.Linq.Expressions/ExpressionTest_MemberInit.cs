//
// ExpressionTest_MemberInit.cs
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
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;

using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions {

	[TestFixture]
	public class ExpressionTest_MemberInit {

		public class Foo {
			public string Bar;
			public string Baz;
		}

		public class Gazonk {
			public string Tzap;
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullExpression ()
		{
			Expression.MemberInit (null, new MemberBinding [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullBindings ()
		{
			Expression.MemberInit (
				Expression.New (typeof (Foo)),
				null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MemberNotAssignableToNewType ()
		{
			Expression.MemberInit (
				Expression.New (typeof (Foo)),
				new MemberBinding [] { Expression.Bind (typeof (Gazonk).GetField ("Tzap"), "tzap".ToConstant ()) });
		}

		[Test]
		public void InitFields ()
		{
			var m = Expression.MemberInit (
				Expression.New (typeof (Foo)),
				new MemberBinding [] {
					Expression.Bind (typeof (Foo).GetField ("Bar"), "bar".ToConstant ()),
					Expression.Bind (typeof (Foo).GetField ("Baz"), "baz".ToConstant ()) });

			Assert.AreEqual (typeof (Foo), m.Type);
			Assert.AreEqual (ExpressionType.MemberInit, m.NodeType);
			Assert.AreEqual ("new Foo() {Bar = \"bar\", Baz = \"baz\"}", m.ToString ());
		}

		public class Thing {
			public string Foo;
			public string Bar { get; set; }
		}

		[Test]
		public void CompiledInit ()
		{
			var i = Expression.Lambda<Func<Thing>> (
				Expression.MemberInit (
					Expression.New (typeof (Thing)),
					Expression.Bind (typeof (Thing).GetField ("Foo"), "foo".ToConstant ()),
					Expression.Bind (typeof (Thing).GetProperty ("Bar"), "bar".ToConstant ()))).Compile ();

			var thing = i ();
			Assert.IsNotNull (thing);
			Assert.AreEqual ("foo", thing.Foo);
			Assert.AreEqual ("bar", thing.Bar);
		}
	}
}
