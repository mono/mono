//
// ExpressionTest_MemberBind.cs
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
	[Category("SRE")]
	public class ExpressionTest_MemberBind {

		public class Foo {
			public string Bar;
			public string Baz;

			public Gazonk Gaz;

			public Gazonk Gazoo { get; set; }

			public string Gruik { get; set; }

			public Foo ()
			{
				Gazoo = new Gazonk ();
				Gaz = new Gazonk ();
			}
		}

		public class Gazonk {
			public string Tzap;

			public int Klang;

			public string Couic { get; set; }

			public string Bang () { return ""; }
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullMethod ()
		{
			Expression.MemberBind (null as MethodInfo, new MemberBinding [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullMember ()
		{
			Expression.MemberBind (null as MemberInfo, new MemberBinding [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void NullBindings ()
		{
			Expression.MemberBind (typeof (Foo).GetField ("Bar"), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MemberNotFieldOrProp ()
		{
			Expression.MemberBind (typeof (Gazonk).GetMethod ("Bang") as MemberInfo, new MemberBinding [0]);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MemberTypeMismatch ()
		{
			Expression.MemberBind (typeof (Gazonk).GetField ("Klang"), Expression.Bind (typeof (Foo).GetField ("Bar"), "bar".ToConstant ()));
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodNotPropertyAccessor ()
		{
			Expression.MemberBind (typeof (Gazonk).GetMethod ("Bang"), new MemberBinding [0]);
		}

		[Test]
		public void MemberBindToField ()
		{
			var mb = Expression.MemberBind (typeof (Foo).GetField ("Gaz"),
				Expression.Bind (typeof (Gazonk).GetField ("Tzap"), "tzap".ToConstant ()));

			Assert.AreEqual (MemberBindingType.MemberBinding, mb.BindingType);
			Assert.AreEqual ("Gaz = {Tzap = \"tzap\"}", mb.ToString ());
		}

		[Test]
		public void MemberBindToProperty ()
		{
			var mb = Expression.MemberBind (typeof (Foo).GetProperty ("Gazoo"),
				Expression.Bind (typeof (Gazonk).GetField ("Tzap"), "tzap".ToConstant ()));

			Assert.AreEqual (MemberBindingType.MemberBinding, mb.BindingType);
			Assert.AreEqual ("Gazoo = {Tzap = \"tzap\"}", mb.ToString ());
		}

		[Test]
		public void MemberBindToPropertyAccessor ()
		{
			var mb = Expression.MemberBind (typeof (Foo).GetProperty ("Gazoo").GetSetMethod (true),
				Expression.Bind (typeof (Gazonk).GetField ("Tzap"), "tzap".ToConstant ()));

			Assert.AreEqual (MemberBindingType.MemberBinding, mb.BindingType);
			Assert.AreEqual ("Gazoo = {Tzap = \"tzap\"}", mb.ToString ());
		}

		[Test]
		public void CompiledMemberBinding ()
		{
			var getfoo = Expression.Lambda<Func<Foo>> (
				Expression.MemberInit (
					Expression.New (typeof (Foo)),
					Expression.MemberBind (
						typeof (Foo).GetProperty ("Gazoo"),
						Expression.Bind (typeof (Gazonk).GetField ("Tzap"),
							"tzap".ToConstant ()),
						Expression.Bind (typeof (Gazonk).GetField ("Klang"),
							42.ToConstant ())))).Compile ();

			var foo = getfoo ();

			Assert.IsNotNull (foo);
			Assert.AreEqual ("tzap", foo.Gazoo.Tzap);
			Assert.AreEqual (42, foo.Gazoo.Klang);
		}
	}
}
