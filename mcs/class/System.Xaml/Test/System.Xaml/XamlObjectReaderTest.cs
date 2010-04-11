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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using System.Xaml;
using System.Xaml.Schema;
using System.Xml;
using NUnit.Framework;

using Category = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Xaml
{
	[TestFixture]
	public class XamlObjectReaderTest
	{
		[Test]
		public void ConstructorNullObject ()
		{
			// allowed.
			new XamlObjectReader (null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullSchemaContext ()
		{
			new XamlObjectReader ("foo", (XamlSchemaContext) null);
		}

		[Test]
		public void ConstructorNullSettings ()
		{
			new XamlObjectReader ("foo", (XamlObjectReaderSettings) null);
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorNullSchemaContext2 ()
		{
			new XamlObjectReader ("foo", null, new XamlObjectReaderSettings ());
		}

		[Test]
		public void ConstructorNullSettings2 ()
		{
			new XamlObjectReader ("foo", new XamlSchemaContext (null, null), null);
		}

		[Test]
		public void ReadNull ()
		{
			var r = new XamlObjectReader (null);
			Assert.IsTrue (r.Read (), "#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#1-2");
			Assert.IsTrue (r.Read (), "#2");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#2-2");
			Assert.AreEqual (XamlLanguage.Null, r.Type, "#2-3");
			Assert.IsTrue (r.Read (), "#3");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#3-2");
			Assert.IsFalse (r.Read (), "#4");
			Assert.AreEqual (XamlNodeType.None, r.NodeType, "#4-2");
		}

		[Test]
		public void Read1 ()
		{
			var r = new XamlObjectReader ("Foo");
			Assert.AreEqual (XamlNodeType.None, r.NodeType, "#1");
			Assert.IsNull (r.Member, "#2");
			Assert.IsNull (r.Namespace, "#3");
			Assert.IsNull (r.Member, "#4");
			Assert.IsNull (r.Type, "#5");
			Assert.IsNull (r.Value, "#6");

			Assert.IsTrue (r.Read (), "#11");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#12");
			Assert.IsNotNull (r.Namespace, "#13");
			Assert.AreEqual ("x", r.Namespace.Prefix, "#13-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "#13-3");

			Assert.IsTrue (r.Read (), "#21");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#22");
			Assert.IsNotNull (r.Type, "#23");
			Assert.AreEqual (new XamlType (typeof (string), r.SchemaContext), r.Type, "#23-2");
			Assert.IsNull (r.Namespace, "#25");

			Assert.IsTrue (r.Read (), "#31");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#32");
			Assert.IsNotNull (r.Member, "#33");
			Assert.AreEqual (XamlLanguage.Initialization, r.Member, "#33-2");
			Assert.IsNull (r.Type, "#34");

			Assert.IsTrue (r.Read (), "#41");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "#42");
			Assert.AreEqual ("Foo", r.Value, "#43");
			Assert.IsNull (r.Member, "#44");

			Assert.IsTrue (r.Read (), "#51");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#52");
			Assert.IsNull (r.Type, "#53");
			Assert.IsNull (r.Member, "#54");

			Assert.IsTrue (r.Read (), "#61");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#62");
			Assert.IsNull (r.Type, "#63");

			Assert.IsFalse (r.Read (), "#71");
			Assert.IsTrue (r.IsEof, "#72");
		}

		[Test]
		[ExpectedException (typeof (XamlObjectReaderException))]
		[Category ("NotWorking")]
		public void ReadNonConstructible ()
		{
			var r = new XamlObjectReader (XamlLanguage.String);
			// XamlType has no default constructor.
			r.Read ();
		}

		[Test]
		[ExpectedException (typeof (XamlObjectReaderException))]
		public void NonPublicType ()
		{
			new XamlObjectReader (new TestClass1 ());
		}

		[Test]
		[ExpectedException (typeof (XamlObjectReaderException))]
		public void NestedType ()
		{
			new XamlObjectReader (new TestClass2 ());
		}
		
		public class TestClass2
		{
		}

		[Test]
		public void ConstructibleType ()
		{
			new XamlObjectReader (new TestClass3 ());
		}

		[Test]
		public void Skip ()
		{
			var r = new XamlObjectReader ("Foo");
			r.Skip ();
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#1");
			r.Skip ();
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#2");
			r.Skip ();
			Assert.IsTrue (r.IsEof, "#3");
		}

		[Test]
		public void Skip2 ()
		{
			var r = new XamlObjectReader ("Foo");
			r.Read (); // NamespaceDeclaration
			r.Read (); // Type
			r.Read (); // Member (Initialization)
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#1");
			r.Skip ();
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#2");
			r.Skip ();
			Assert.IsTrue (r.IsEof, "#3");
		}

		[Test]
		[Category ("NotWorking")]
		public void Read2 ()
		{
			var doc = new XmlDocument ();
			doc.LoadXml ("<root xmlns='urn:foo'><elem attr='val' /></root>");
			var r = new XamlObjectReader (doc);

			for (int i = 0; i < 3; i++) {
				r.Read ();
				Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#1-" + i);
			}
			r.Read ();

			Assert.AreEqual (new XamlType (typeof (XmlDocument), r.SchemaContext), r.Type, "#2");
			r.Read ();
			var l = new List<XamlMember> ();
			for (int i = 0; i < 5; i++) {
				Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#3-" + i);
				l.Add (r.Member);
				r.Skip ();
			}
			l.First (m => m.Name == "Value");
			l.First (m => m.Name == "InnerXml");
			l.First (m => m.Name == "Prefix");
			l.First (m => m.Name == "PreserveWhitespace");
			l.First (m => m.Name == "Schemas");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#4");
			Assert.IsFalse (r.Read (), "#5");
		}
	}

	class TestClass1
	{
	}

	public class TestClass3
	{
	}
}
