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

		private List<ElementInit> list;
		
		[SetUp]
		public void init()
		{
			list= new List<ElementInit> ();
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void MemberNull ()
		{
			Expression.ListBind (null as MemberInfo, list);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void PropertyAccessorNull ()
		{
			Expression.ListBind (null as MethodInfo, list);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgNull ()
		{
			list.Add(null);
			Expression.ListBind (typeof (Foo).GetProperty ("Bar").GetSetMethod (), list);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ArgNull2 ()
		{
			list.Add(null);
			Expression.ListBind (typeof (Foo).GetMember ("foo")[0], list);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MemberTypeImplementIEnumerable ()
		{
			Expression.ListBind (typeof (Foo).GetMember ("str")[0], list);
		}
		//TODO test for other than fielf and property...
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodeGetImplementIEnumerable2 ()
		{
			Expression.ListBind (typeof (Foo).GetProperty ("BarBar").GetGetMethod (), list);
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MethodMustBeAnAccessor ()
		{
			Expression.ListBind (typeof (Foo).GetMethod ("test"), list);
		}		
		
		/*[Test]
		public void ListBindToString ()
		{
			//TODO list add or do with a lambda expression
			var ListBind = Expression.ListBind (typeof (Foo).GetMember ("Bar"), list);

			Assert.AreEqual ("Bar.Add(\"\")", ListBind.ToString ());
		}*/

		public class Foo {
	
			public string [] Bar
			{
				get {return foo;}
				set {foo = value;}
			}
			public string [] foo;
			public string str;
			public string [] test ()
			{
				return null;
			}
			public int BarBar
			{
				get {return 0;}
			}
		}
	}
}
