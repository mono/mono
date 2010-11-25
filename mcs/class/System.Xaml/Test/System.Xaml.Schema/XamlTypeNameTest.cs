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
using System.Text;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.Xaml.Schema
{
	[TestFixture]
	public class XamlTypeNameTest
	{
		[Test]
		public void ConstructorDefault ()
		{
			var xtn = new XamlTypeName ();
			Assert.IsNotNull (xtn.TypeArguments, "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorXamlTypeNull ()
		{
			new XamlTypeName (null);
		}

		[Test]
		public void ConstructorNameNull ()
		{
			// allowed.
			var xtn = new XamlTypeName ("urn:foo", null);
			Assert.IsNotNull (xtn.TypeArguments, "#1");
		}

		[Test]
		public void ConstructorNamespaceNull ()
		{
			// allowed.
			var xtn = new XamlTypeName (null, "FooBar");
			Assert.IsNotNull (xtn.TypeArguments, "#1");
		}

		[Test]
		public void ConstructorName ()
		{
			var n = new XamlTypeName ("urn:foo", "FooBar");
			Assert.IsNotNull (n.TypeArguments, "#1");
			Assert.AreEqual (0, n.TypeArguments.Count, "#2");
		}

		[Test]
		public void ConstructorTypeArgumentsNull ()
		{
			var n = new XamlTypeName ("urn:foo", "FooBar", (XamlTypeName []) null);
			Assert.IsNotNull (n.TypeArguments, "#1");
			Assert.AreEqual (0, n.TypeArguments.Count, "#2");
		}

		[Test]
		[Ignore (".NET causes NRE on ToString(). It is not really intended and should raise an error")]
		public void ConstructorTypeArgumentsNullEntry ()
		{
			new XamlTypeName ("urn:foo", "FooBar", new XamlTypeName [] {null});
		}

		[Test]
		public void ConstructorTypeArguments ()
		{
			new XamlTypeName ("urn:foo", "FooBar", new XamlTypeName [] {new XamlTypeName ("urn:bar", "FooBarBaz")});
		}

		[Test]
		public void ConstructorTypeArgumentsEmpty ()
		{
			var n = new XamlTypeName ("urn:foo", "FooBar", new XamlTypeName [0]);
			Assert.IsNotNull (n.TypeArguments, "#1");
			Assert.AreEqual (0, n.TypeArguments.Count, "#2");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ToStringDefault ()
		{
			var n = new XamlTypeName ();
			n.ToString ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ToStringNameNull ()
		{
			var n = new XamlTypeName ("urn:foo", null);
			n.ToString ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ToStringNamespaceNull ()
		{
			// allowed.
			var n = new XamlTypeName (null, "FooBar");
			n.ToString ();
		}

		[Test]
		public void ToStringTypeArgumentsNull ()
		{
			var n = new XamlTypeName ("urn:foo", "FooBar", (XamlTypeName []) null);
			Assert.AreEqual ("{urn:foo}FooBar", n.ToString (), "#1");
		}

		[Test]
		[Ignore (".NET raises NRE")]
		public void ToStringTypeArgumentsNullEntry ()
		{
			var n = new XamlTypeName ("urn:foo", "FooBar", new XamlTypeName [] {null, new XamlTypeName ("urn:bar", "FooBarBaz")});
			Assert.AreEqual ("{urn:foo}FooBar()", n.ToString (), "#1");
		}

		[Test]
		public void ToStringTypeArguments ()
		{
			var n = new XamlTypeName ("urn:foo", "FooBar", new XamlTypeName [] {new XamlTypeName ("urn:bar", "FooBarBaz")});
			Assert.AreEqual ("{urn:foo}FooBar({urn:bar}FooBarBaz)", n.ToString (), "#1");
		}

		[Test]
		public void ToStringTypeArguments2 ()
		{
			var n = new XamlTypeName ("urn:foo", "Foo", new XamlTypeName [] {new XamlTypeName ("urn:bar", "Bar"), new XamlTypeName ("urn:baz", "Baz")});
			Assert.AreEqual ("{urn:foo}Foo({urn:bar}Bar, {urn:baz}Baz)", n.ToString (), "#1");
		}

		[Test]
		public void ToStringEmptyNamespace ()
		{
			var n = new XamlTypeName (string.Empty, "Foo");
			Assert.AreEqual ("{}Foo", n.ToString (), "#1");
		}

		[Test]
		public void ToStringXamlTypePredefined ()
		{
			var n = new XamlTypeName (XamlLanguage.Int32);
			Assert.AreEqual ("{http://schemas.microsoft.com/winfx/2006/xaml}Int32", n.ToString (), "#1");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ToStringNamespaceLookupInsufficient ()
		{
			var n = new XamlTypeName ("urn:foo", "Foo", new XamlTypeName [] {new XamlTypeName ("urn:bar", "Bar"), new XamlTypeName ("urn:baz", "Baz")});
			var lookup = new MyNamespaceLookup ();
			lookup.Add ("a", "urn:foo");
			lookup.Add ("c", "urn:baz");
			// it fails because there is missing mapping for urn:bar.
			Assert.AreEqual ("a:Foo({urn:bar}Bar, c:Baz)", n.ToString (lookup), "#1");
		}

		[Test]
		public void ToStringNullLookup ()
		{
			var n = new XamlTypeName ("urn:foo", "Foo", new XamlTypeName [] {new XamlTypeName ("urn:bar", "Bar"), new XamlTypeName ("urn:baz", "Baz")});
			Assert.AreEqual ("{urn:foo}Foo({urn:bar}Bar, {urn:baz}Baz)", n.ToString (null), "#1");
		}

		[Test]
		public void ToStringNamespaceLookup ()
		{
			var n = new XamlTypeName ("urn:foo", "Foo", new XamlTypeName [] {new XamlTypeName ("urn:bar", "Bar"), new XamlTypeName ("urn:baz", "Baz")});
			var lookup = new MyNamespaceLookup ();
			lookup.Add ("a", "urn:foo");
			lookup.Add ("b", "urn:bar");
			lookup.Add ("c", "urn:baz");
			Assert.AreEqual ("a:Foo(b:Bar, c:Baz)", n.ToString (lookup), "#1");
			Assert.AreEqual ("b:Bar, c:Baz", XamlTypeName.ToString (n.TypeArguments, lookup), "#2");
		}

		// This test shows that MarkupExtension names are not replaced at XamlTypeName.ToString(), while XamlXmlWriter writes like "x:Null".
		[Test]
		public void ToStringNamespaceLookup2 ()
		{
			var lookup = new MyNamespaceLookup ();
			lookup.Add ("x", XamlLanguage.Xaml2006Namespace);
			Assert.AreEqual ("x:NullExtension", new XamlTypeName (XamlLanguage.Null).ToString (lookup), "#1");
			// WHY is TypeExtension not the case?
			//Assert.AreEqual ("x:TypeExtension", new XamlTypeName (XamlLanguage.Type).ToString (lookup), "#2");
			Assert.AreEqual ("x:ArrayExtension", new XamlTypeName (XamlLanguage.Array).ToString (lookup), "#3");
			Assert.AreEqual ("x:StaticExtension", new XamlTypeName (XamlLanguage.Static).ToString (lookup), "#4");
			Assert.AreEqual ("x:Reference", new XamlTypeName (XamlLanguage.Reference).ToString (lookup), "#5");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void StaticToStringNullLookup ()
		{
			XamlTypeName.ToString (new XamlTypeName [] {new XamlTypeName ("urn:foo", "bar")}, null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void StaticToStringNullTypeNameList ()
		{
			XamlTypeName.ToString (null, new MyNamespaceLookup ());
		}

		[Test]
		public void StaticToStringEmptyArray ()
		{
			Assert.AreEqual ("", XamlTypeName.ToString (new XamlTypeName [0], new MyNamespaceLookup ()), "#1");
		}

		class MyNamespaceLookup : INamespacePrefixLookup
		{
			Dictionary<string,string> dic = new Dictionary<string,string> ();

			public void Add (string prefix, string ns)
			{
				dic [ns] = prefix;
			}

			public string LookupPrefix (string ns)
			{
				string p;
				return dic.TryGetValue (ns, out p) ? p : null;
			}
		}

		XamlTypeName dummy;

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TryParseNullName ()
		{
			XamlTypeName.TryParse (null, new MyNSResolver (), out dummy);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void TryParseNullResolver ()
		{
			XamlTypeName.TryParse ("Foo", null, out dummy);
		}

		[Test]
		public void TryParseEmptyName ()
		{
			Assert.IsFalse (XamlTypeName.TryParse (String.Empty, new MyNSResolver (), out dummy), "#1");
		}

		[Test]
		public void TryParseColon ()
		{
			var r = new MyNSResolver ();
			r.Add ("a", "urn:foo");
			Assert.IsFalse (XamlTypeName.TryParse (":", r, out dummy), "#1");
			Assert.IsFalse (XamlTypeName.TryParse ("a:", r, out dummy), "#2");
			Assert.IsFalse (XamlTypeName.TryParse (":b", r, out dummy), "#3");
		}

		[Test]
		public void TryParseInvalidName ()
		{
			var r = new MyNSResolver ();
			r.Add ("a", "urn:foo");
			r.Add ("#", "urn:bar");
			Assert.IsFalse (XamlTypeName.TryParse ("$%#___!", r, out dummy), "#1");
			Assert.IsFalse (XamlTypeName.TryParse ("a:#$#", r, out dummy), "#2");
			Assert.IsFalse (XamlTypeName.TryParse ("#:foo", r, out dummy), "#3");
		}

		[Test]
		public void TryParseNoFillEmpty ()
		{
			Assert.IsFalse (XamlTypeName.TryParse ("Foo", new MyNSResolver (true), out dummy), "#1");
		}

		[Test]
		public void TryParseFillEmpty ()
		{
			var r = new MyNSResolver ();
			Assert.IsTrue (XamlTypeName.TryParse ("Foo", r, out dummy), "#1");
			Assert.IsNotNull (dummy, "#2");
			Assert.AreEqual (String.Empty, dummy.Namespace, "#2-2");
			Assert.AreEqual ("Foo", dummy.Name, "#2-3");
		}

		[Test]
		public void TryParseAlreadyQualified ()
		{
			Assert.IsFalse (XamlTypeName.TryParse ("{urn:foo}Foo", new MyNSResolver (), out dummy), "#1");
		}

		[Test]
		public void TryParseResolveFailure ()
		{
			Assert.IsFalse (XamlTypeName.TryParse ("x:Foo", new MyNSResolver (), out dummy), "#1");
		}

		[Test]
		public void TryParseResolveSuccess ()
		{
			var r = new MyNSResolver ();
			r.Add ("x", "urn:foo");
			Assert.IsTrue (XamlTypeName.TryParse ("x:Foo", r, out dummy), "#1");
			Assert.IsNotNull (dummy, "#2");
			Assert.AreEqual ("urn:foo", dummy.Namespace, "#2-2");
			Assert.AreEqual ("Foo", dummy.Name, "#2-3");
		}

		[Test]
		public void TryParseInvalidGenericName ()
		{
			var r = new MyNSResolver ();
			r.Add ("x", "urn:foo");
			Assert.IsFalse (XamlTypeName.TryParse ("x:Foo()", r, out dummy), "#1");
		}

		[Test]
		public void TryParseGenericName ()
		{
			var r = new MyNSResolver ();
			r.Add ("x", "urn:foo");
			Assert.IsTrue (XamlTypeName.TryParse ("x:Foo(x:Foo,x:Bar)", r, out dummy), "#1");
			Assert.AreEqual (2, dummy.TypeArguments.Count, "#2");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ParseListNullNames ()
		{
			XamlTypeName.ParseList (null, new MyNSResolver ());
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ParseListNullResolver ()
		{
			XamlTypeName.ParseList ("foo", null);
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ParseListInvalid ()
		{
			XamlTypeName.ParseList ("foo bar", new MyNSResolver ());
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ParseListInvalid2 ()
		{
			XamlTypeName.ParseList ("foo,", new MyNSResolver ());
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ParseListInvalid3 ()
		{
			XamlTypeName.ParseList ("", new MyNSResolver ());
		}

		[Test]
		public void ParseListValid ()
		{
			var l = XamlTypeName.ParseList ("foo,  bar", new MyNSResolver ());
			Assert.AreEqual (2, l.Count, "#1");
			Assert.AreEqual ("{}foo", l [0].ToString (), "#2");
			Assert.AreEqual ("{}bar", l [1].ToString (), "#3");
			l = XamlTypeName.ParseList ("foo,bar", new MyNSResolver ());
			Assert.AreEqual ("{}foo", l [0].ToString (), "#4");
			Assert.AreEqual ("{}bar", l [1].ToString (), "#5");
		}
		
		[Test]
		public void GenericArrayName ()
		{
			var ns = new MyNSResolver ();
			ns.Add ("s", "urn:foo");
			var xn = XamlTypeName.Parse ("s:Nullable(s:Int32)[,,]", ns);
			Assert.AreEqual ("urn:foo", xn.Namespace, "#1");
			// note that array suffix comes here.
			Assert.AreEqual ("Nullable[,,]", xn.Name, "#2");
			// note that array suffix is detached from Name and appended after generic type arguments.
			Assert.AreEqual ("{urn:foo}Nullable({urn:foo}Int32)[,,]", xn.ToString (), "#3");
		}

		[Test]
		public void GenericGenericName ()
		{
			var ns = new MyNSResolver ();
			ns.Add ("s", "urn:foo");
			ns.Add ("", "urn:bar");
			ns.Add ("x", XamlLanguage.Xaml2006Namespace);
			var xn = XamlTypeName.Parse ("List(KeyValuePair(x:Int32, s:DateTime))", ns);
			Assert.AreEqual ("urn:bar", xn.Namespace, "#1");
			Assert.AreEqual ("List", xn.Name, "#2");
			Assert.AreEqual ("{urn:bar}List({urn:bar}KeyValuePair({http://schemas.microsoft.com/winfx/2006/xaml}Int32, {urn:foo}DateTime))", xn.ToString (), "#3");
		}

		class MyNSResolver : IXamlNamespaceResolver
		{
			public MyNSResolver ()
				: this (false)
			{
			}

			public MyNSResolver (bool returnNullForEmpty)
			{
				if (!returnNullForEmpty)
					dic.Add (String.Empty, String.Empty);
			}

			Dictionary<string,string> dic = new Dictionary<string,string> ();

			public void Add (string prefix, string ns)
			{
				dic [prefix] = ns;
			}

			public string GetNamespace (string prefix)
			{
				string ns;
				return dic.TryGetValue (prefix, out ns) ? ns : null;
			}
			
			public IEnumerable<NamespaceDeclaration> GetNamespacePrefixes ()
			{
				foreach (var p in dic)
					yield return new NamespaceDeclaration (p.Value, p.Key);
			}
		}
	}
}
