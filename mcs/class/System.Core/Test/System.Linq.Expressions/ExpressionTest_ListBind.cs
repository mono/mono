//
// ExpressionTest_ListBind.cs
//
// Author:
//   olivier Dufour (olivier.duff@gmail.com)
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;

namespace MonoTests.System.Linq.Expressions {

	[TestFixture]
	public class ExpressionTest_ListBind {

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MemberNull ()
		{
			Expression.ListBind (null as MemberInfo, new List<ElementInit> ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PropertyAccessorNull ()
		{
			Expression.ListBind (null as MethodInfo, new List<ElementInit> ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgNull ()
		{
			var list = new List<ElementInit> ();
			list.Add (null);
			Expression.ListBind (typeof (Foo).GetProperty ("Bar").GetSetMethod (), list);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MemberTypeImplementIEnumerable ()
		{
			Expression.ListBind (typeof (Foo).GetMember ("baz") [0], new List<ElementInit> ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodeGetImplementIEnumerable2 ()
		{
			Expression.ListBind (typeof (Foo).GetProperty ("BarBar").GetGetMethod (), new List<ElementInit> ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodMustBeAnAccessor ()
		{
			Expression.ListBind (typeof (Foo).GetMethod ("test"), new List<ElementInit> ());
		}

		[Test]
		public void ListBindToString ()
		{
			var add = typeof (List<string>).GetMethod ("Add");

			var list = new List<ElementInit> () {
				Expression.ElementInit (add, Expression.Constant ("foo")),
				Expression.ElementInit (add, Expression.Constant ("bar")),
				Expression.ElementInit (add, Expression.Constant ("baz")),
			};

			var binding = Expression.ListBind (typeof (Foo).GetProperty ("List"), list);

			Assert.AreEqual ("List = {Void Add(System.String)(\"foo\"), Void Add(System.String)(\"bar\"), Void Add(System.String)(\"baz\")}", binding.ToString ());
		}

		[Test]
		public void CompiledListBinding ()
		{
			var add = typeof (List<string>).GetMethod ("Add");

			var lb = Expression.Lambda<Func<Foo>> (
				Expression.MemberInit (
					Expression.New (typeof (Foo)),
					Expression.ListBind (
						typeof (Foo).GetProperty ("List"),
						Expression.ElementInit (add, Expression.Constant ("foo")),
						Expression.ElementInit (add, Expression.Constant ("bar")),
						Expression.ElementInit (add, Expression.Constant ("baz"))))).Compile ();

			var foo = lb ();

			Assert.IsNotNull (foo);
			Assert.AreEqual (3, foo.List.Count);
			Assert.AreEqual ("foo", foo.List [0]);
			Assert.AreEqual ("bar", foo.List [1]);
			Assert.AreEqual ("baz", foo.List [2]);
		}

		public class Foo {

			public string [] foo;
			public string str;

			public int baz;

			private List<string> list = new List<string> ();

			public List<string> List {
				get { return list; }
			}

			public string [] Bar
			{
				get { return foo; }
				set { foo = value; }
			}

			public int BarBar
			{
				get { return 0; }
			}

			public string [] test ()
			{
				return null;
			}
		}
	}
}
