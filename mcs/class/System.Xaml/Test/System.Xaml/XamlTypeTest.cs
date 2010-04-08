//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xaml;
using System.Xaml.Schema;
using NUnit.Framework;

namespace MonoTests.System.Xaml
{
	[TestFixture]
	public class XamlTypeTest
	{
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorTypeNullType ()
		{
			new XamlType (null, new XamlSchemaContext (null, null));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorTypeNullSchemaContext ()
		{
			new XamlType (typeof (int), null);
		}

		[Test]
		public void ConstructorSimpleType ()
		{
			var t = new XamlType (typeof (int), new XamlSchemaContext (null, null));
			Assert.AreEqual ("Int32", t.Name, "#1");
			Assert.AreEqual (typeof (int), t.UnderlyingType, "#2");
			Assert.IsNotNull (t.BaseType, "#3-1");
			// So, it is type aware. It's weird that t.Name still returns full name just as it is passed to the .ctor.
			Assert.AreEqual ("ValueType", t.BaseType.Name, "#3-2");
			Assert.AreEqual ("clr-namespace:System;assembly=mscorlib", t.BaseType.PreferredXamlNamespace, "#3-3");
			// It is likely only for primitive types such as int.
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, t.PreferredXamlNamespace, "#4");

			t = new XamlType (typeof (XamlXmlReader), new XamlSchemaContext (null, null));
			Assert.AreEqual ("XamlXmlReader", t.Name, "#11");
			Assert.AreEqual (typeof (XamlXmlReader), t.UnderlyingType, "#12");
			Assert.IsNotNull (t.BaseType, "#13");
			Assert.AreEqual (typeof (XamlReader), t.BaseType.UnderlyingType, "#13-2");
			Assert.AreEqual ("clr-namespace:System.Xaml;assembly=System.Xaml", t.BaseType.PreferredXamlNamespace, "#13-3");
			Assert.AreEqual ("clr-namespace:System.Xaml;assembly=System.Xaml", t.PreferredXamlNamespace, "#14");
		}

		[Test]
		public void ConstructorNullTypeInvoker ()
		{
			// allowed.
			new XamlType (typeof (int), new XamlSchemaContext (null, null), null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNamesNullName ()
		{
			new XamlType (String.Empty, null, null, new XamlSchemaContext (null, null));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNamesNullSchemaContext ()
		{
			new XamlType ("System", "Int32", null, null);
		}

		[Test]
		public void ConstructorNames ()
		{
			// null typeArguments is allowed.
			new XamlType ("System", "Int32", null, new XamlSchemaContext (null, null));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNameNullName ()
		{
			new MyXamlType (null, null, new XamlSchemaContext (null, null));
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNameNullSchemaContext ()
		{
			new MyXamlType ("System.Int32", null, null);
		}

		[Test]
		public void ConstructorNameInvalid ()
		{
			// ... all allowed.
			new XamlType (String.Empty, ".", null, new XamlSchemaContext (null, null));
			new XamlType (String.Empty, "<>", null, new XamlSchemaContext (null, null));
			new XamlType (String.Empty, "", null, new XamlSchemaContext (null, null));
		}

		[Test]
		public void ConstructorNameWithFullName ()
		{
			// null typeArguments is allowed.
			var t = new MyXamlType ("System.Int32", null, new XamlSchemaContext (null, null));
			Assert.AreEqual ("System.Int32", t.Name, "#1");
			Assert.IsNull (t.UnderlyingType, "#2");
			Assert.IsNotNull (t.BaseType, "#3-1");
			// So, it is type aware. It's weird that t.Name still returns full name just as it is passed to the .ctor.
			Assert.AreEqual ("Object", t.BaseType.Name, "#3-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, t.BaseType.PreferredXamlNamespace, "#3-3");
			Assert.IsNull (t.BaseType.BaseType, "#3-4");
			Assert.AreEqual (String.Empty, t.PreferredXamlNamespace, "#4");
			Assert.IsFalse (t.IsArray, "#5");
			Assert.IsFalse (t.IsGeneric, "#6");
			Assert.IsTrue (t.IsPublic, "#7");
		}

		[Test]
		public void NoSuchTypeByName ()
		{
			var t = new MyXamlType ("System.NoSuchType", null, new XamlSchemaContext (null, null));
			Assert.AreEqual ("System.NoSuchType", t.Name, "#1");
			Assert.IsNull (t.UnderlyingType, "#2");
			Assert.IsNotNull (t.BaseType, "#3-1");
			// So, it is type aware. It's weird that t.Name still returns full name just as it is passed to the .ctor.
			Assert.AreEqual ("Object", t.BaseType.Name, "#3-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, t.BaseType.PreferredXamlNamespace, "#3-3");
			Assert.AreEqual (String.Empty, t.PreferredXamlNamespace, "#4");
		}

		[Test]
		public void NoSuchTypeByNames ()
		{
			var t = new XamlType ("urn:foo", "System.NoSuchType", null, new XamlSchemaContext (null, null));
			Assert.AreEqual ("System.NoSuchType", t.Name, "#1");
			Assert.IsNull (t.UnderlyingType, "#2");
			Assert.IsNotNull (t.BaseType, "#3-1");
			// So, it is type aware. It's weird that t.Name still returns full name just as it is passed to the .ctor.
			Assert.AreEqual ("Object", t.BaseType.Name, "#3-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, t.BaseType.PreferredXamlNamespace, "#3-3");
			Assert.AreEqual ("urn:foo", t.PreferredXamlNamespace, "#4");
		}

		[Test]
		[Ignore ("It results in NRE on .NET 4.0 RC")]
		public void EmptyTypeArguments ()
		{
			var sctx = new XamlSchemaContext (null, null);
			var t1 = new MyXamlType ("System.Int32", null, sctx);
			var t2 = new MyXamlType ("System.Int32", new XamlType [0], sctx);
			Assert.IsTrue (t1 == t2, "#1");
			Assert.IsTrue (t1.Equals (t2), "#2");
		}

		[Test]
		public void EmptyTypeArguments2 ()
		{
			var sctx = new XamlSchemaContext (null, null);
			var t1 = new XamlType ("System", "Int32", null, sctx);
			var t2 = new XamlType ("System", "Int32", new XamlType [0], sctx);
			Assert.IsNull (t1.TypeArguments, "#1");
			Assert.IsNull (t2.TypeArguments, "#2");
			Assert.IsTrue (t1 == t2, "#3");
			Assert.IsTrue (t1.Equals (t2), "#4");
		}

		[Test]
		public void EqualityAcrossConstructors ()
		{
			var sctx = new XamlSchemaContext (null, null);
			var t1 = new XamlType (typeof (int), sctx);
			var t2 = new XamlType (t1.PreferredXamlNamespace, t1.Name, null, sctx);
			// not sure if it always returns false for different .ctor comparisons...
			Assert.IsFalse (t1 == t2, "#3");
		}

		[Test]
		public void ArrayAndCollection ()
		{
			var sctx = new XamlSchemaContext (null, null);
			var t = new XamlType (typeof (int), sctx);
			Assert.IsFalse (t.IsArray, "#1.1");
			Assert.IsFalse (t.IsCollection, "#1.2");
			Assert.IsNull (t.ItemType, "#1.3");
			t = new XamlType (typeof (ArrayList), sctx);
			Assert.IsFalse (t.IsArray, "#2.1");
			Assert.IsTrue (t.IsCollection, "#2.2");
			Assert.IsNotNull (t.ItemType, "#2.3");
			Assert.AreEqual ("Object", t.ItemType.Name, "#2.4");
			t = new XamlType (typeof (int []), sctx);
			Assert.IsTrue (t.IsArray, "#3.1");
			// why?
			Assert.IsFalse (t.IsCollection, "#3.2");
			Assert.IsNotNull (t.ItemType, "#3.3");
			Assert.AreEqual (typeof (int), t.ItemType.UnderlyingType, "#3.4");
		}

		[Test]
		public void Dictionary ()
		{
			var sctx = new XamlSchemaContext (null, null);
			var t = new XamlType (typeof (int), sctx);
			Assert.IsFalse (t.IsDictionary, "#1.1");
			Assert.IsNull (t.KeyType, "#1.2");
			t = new XamlType (typeof (Hashtable), sctx);
			Assert.IsTrue (t.IsDictionary, "#2.1");
			Assert.IsNotNull (t.KeyType, "#2.2");
			Assert.AreEqual ("Object", t.KeyType.Name, "#2.3");
			t = new XamlType (typeof (Dictionary<int,string>), sctx);
			Assert.IsTrue (t.IsDictionary, "#3.1");
			Assert.IsNotNull (t.KeyType, "#3.2");
			Assert.AreEqual ("Int32", t.KeyType.Name, "#3.3");
		}

		[Test]
		[Category ("NotWorking")]
		public void IsConstructible ()
		{
			var sctx = new XamlSchemaContext (null, null);
			// ... is it?
			Assert.IsTrue (new XamlType (typeof (int), sctx).IsConstructible, "#1");
			// ... is it?
			Assert.IsFalse (new XamlType (typeof (Foo), sctx).IsConstructible, "#2");
			Assert.IsFalse (new XamlType (typeof (Bar), sctx).IsConstructible, "#3");
			Assert.IsTrue (new XamlType (typeof (object), sctx).IsConstructible, "#4");
		}

		public class Foo
		{
		}
	
		class Bar
		{
			internal Bar () {}
		}
	}

	class MyXamlType : XamlType
	{
		public MyXamlType (string fullName, IList<XamlType> typeArguments, XamlSchemaContext context)
			: base (fullName, typeArguments, context)
		{
		}
	}
}
