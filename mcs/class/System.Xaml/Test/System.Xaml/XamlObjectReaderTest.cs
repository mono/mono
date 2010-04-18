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
		public void ReadNonConstructible ()
		{
			// XamlType has no default constructor.
			var r = new XamlObjectReader (XamlLanguage.String);
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

		[Test]
		public void Read_NonPrimitive ()
		{
			var r = new XamlObjectReader (new TestClass3 ());
			Assert.AreEqual (XamlNodeType.None, r.NodeType, "#1");
			Assert.IsTrue (r.Read (), "#6");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#7");
			Assert.AreEqual (String.Empty, r.Namespace.Prefix, "#7-2");
			Assert.AreEqual ("clr-namespace:MonoTests.System.Xaml;assembly=" + GetType ().Assembly.GetName ().Name, r.Namespace.Namespace, "#7-3");

			Assert.IsTrue (r.Read (), "#11");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#12");
			Assert.AreEqual ("x", r.Namespace.Prefix, "#12-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "#12-3");

			Assert.IsTrue (r.Read (), "#16");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#17");
			var xt = new XamlType (typeof (TestClass3), r.SchemaContext);
			Assert.AreEqual (xt, r.Type, "#17-2");
			Assert.IsTrue (r.Instance is TestClass3, "#17-3");

			Assert.IsTrue (r.Read (), "#21");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#22");
			Assert.AreEqual (xt.GetMember ("Nested"), r.Member, "#22-2");

			Assert.IsTrue (r.Read (), "#26");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#27");
			Assert.AreEqual (XamlLanguage.Null, r.Type, "#27-2");
			Assert.IsNull (r.Instance, "#27-3");

			Assert.IsTrue (r.Read (), "#31");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#32");

			Assert.IsTrue (r.Read (), "#36");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#37");

			Assert.IsTrue (r.Read (), "#41");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#42");

			Assert.IsFalse (r.Read (), "#46");
			Assert.IsTrue (r.IsEof, "#47");
		}

		[Test]
		[Category ("NotWorking")]
		public void Read_Type ()
		{
			var r = new XamlObjectReader (typeof (int));
			Read_TypeOrTypeExtension (r);
		}
		
		[Test]
		[Category ("NotWorking")]
		public void Read_TypeExtension ()
		{
			var r = new XamlObjectReader (new TypeExtension (typeof (int)));
			Read_TypeOrTypeExtension (r);
		}

		void Read_TypeOrTypeExtension (XamlObjectReader r)
		{
			Assert.IsTrue (r.Read (), "#11");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#12");
			Assert.IsNotNull (r.Namespace, "#13");
			Assert.AreEqual ("x", r.Namespace.Prefix, "#13-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "#13-3");
			Assert.IsNull (r.Instance, "#14");

			Assert.IsTrue (r.Read (), "#21");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#22");
			Assert.IsNotNull (r.Type, "#23");
			Assert.AreEqual (new XamlType (typeof (TypeExtension), r.SchemaContext), r.Type, "#23-2");
			Assert.IsNull (r.Namespace, "#25");
			Assert.IsTrue (r.Instance is TypeExtension, "#26");

			Assert.IsTrue (r.Read (), "#31");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#32");
			Assert.IsNotNull (r.Member, "#33");
			Assert.AreEqual (XamlLanguage.PositionalParameters, r.Member, "#33-2");
			Assert.IsNull (r.Type, "#34");
			Assert.IsNull (r.Instance, "#35");

			Assert.IsTrue (r.Read (), "#41");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "#42");
			Assert.IsNotNull (r.Value, "#43");
			Assert.AreEqual ("x:Int32", r.Value, "#43-2");
			Assert.IsNull (r.Member, "#44");
			Assert.IsNull (r.Instance, "#45");

			Assert.IsTrue (r.Read (), "#51");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#52");
			Assert.IsNull (r.Type, "#53");
			Assert.IsNull (r.Member, "#54");
			Assert.IsNull (r.Instance, "#55");

			Assert.IsTrue (r.Read (), "#61");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#62");
			Assert.IsNull (r.Type, "#63");

			Assert.IsFalse (r.Read (), "#71");
			Assert.IsTrue (r.IsEof, "#72");
		}

		[Test]
		[Category ("NotWorking")] // namespace node differences
		public void Read_Type2 ()
		{
			var r = new XamlObjectReader (typeof (TestClass1));
			Read_TypeOrTypeExtension2 (r);
		}
		
		[Test]
		[Category ("NotWorking")] // namespace node differences
		public void Read_TypeExtension2 ()
		{
			var r = new XamlObjectReader (new TypeExtension (typeof (TestClass1)));
			Read_TypeOrTypeExtension2 (r);
		}

		void Read_TypeOrTypeExtension2 (XamlObjectReader r)
		{
			Assert.IsTrue (r.Read (), "#11");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#12");
			Assert.AreEqual (String.Empty, r.Namespace.Prefix, "#13-2");
			Assert.AreEqual ("clr-namespace:MonoTests.System.Xaml;assembly=" + GetType ().Assembly.GetName ().Name, r.Namespace.Namespace, "#13-3");

			Assert.IsTrue (r.Read (), "#16");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#17");
			Assert.IsNotNull (r.Namespace, "#18");
			Assert.AreEqual ("x", r.Namespace.Prefix, "#18-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "#18-3");

			Assert.IsTrue (r.Read (), "#21");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#22");
			Assert.AreEqual (new XamlType (typeof (TypeExtension), r.SchemaContext), r.Type, "#23-2");
			Assert.IsTrue (r.Instance is TypeExtension, "#26");

			Assert.IsTrue (r.Read (), "#31");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#32");
			Assert.AreEqual (XamlLanguage.PositionalParameters, r.Member, "#33-2");

			Assert.IsTrue (r.Read (), "#41");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "#42");
			Assert.AreEqual ("TestClass1", r.Value, "#43-2");

			Assert.IsTrue (r.Read (), "#51");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#52");

			Assert.IsTrue (r.Read (), "#61");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#62");

			Assert.IsFalse (r.Read (), "#71");
			Assert.IsTrue (r.IsEof, "#72");
		}

		[Test]
		public void Read_DateTime ()
		{
			var obj = new DateTime (2010, 4, 15);
			var r = new XamlObjectReader (obj);
			Read_CommonClrType (r, obj);
			Assert.AreEqual ("2010-04-15", Read_Initialization (r, null), "#1");
		}

		[Test]
		public void Read_TimeSpan ()
		{
			Read_CommonXamlPrimitive (TimeSpan.FromMinutes (4));
		}

		[Test]
		public void Read_Uri ()
		{
			Read_CommonXamlPrimitive (new Uri ("urn:foo"));
		}

		[Test]
		[ExpectedException (typeof (XamlObjectReaderException))]
		[Category ("NotWorking")]
		public void Read_XData ()
		{
			var r = new XamlObjectReader (new XData () {Text = "xdata text"}); // XmlReader implementation is not visible.
			while (!r.IsEof)
				r.Read ();
		}

		[Test]
		public void ReadStandardTypes ()
		{
			SimpleReadStandardType (new ArrayExtension ());
			SimpleReadStandardType (new NullExtension ());
			SimpleReadStandardType (new PropertyDefinition ());
			SimpleReadStandardType (new Reference ());
			SimpleReadStandardType (new StaticExtension ());
			SimpleReadStandardType (new TypeExtension ());
		}

		void SimpleReadStandardType (object instance)
		{
			var r = new XamlObjectReader (instance);
			while (!r.IsEof)
				r.Read ();
		}

		void Read_CommonXamlPrimitive (object obj)
		{
			var r = new XamlObjectReader (obj);
			Read_CommonXamlType (r);
			Read_Initialization (r, obj);
		}

		// from StartMember of Initialization to EndMember
		string Read_Initialization (XamlObjectReader r, object comparableValue)
		{
			Assert.IsTrue (r.Read (), "init#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "init#2");
			Assert.IsNotNull (r.Member, "init#3");
			Assert.AreEqual (XamlLanguage.Initialization, r.Member, "init#3-2");
			Assert.IsTrue (r.Read (), "init#4");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "init#5");
			Assert.AreEqual (typeof (string), r.Value.GetType (), "init#6");
			string ret = (string) r.Value;
			if (comparableValue != null)
				Assert.AreEqual (comparableValue.ToString (), r.Value, "init#6-2");
			Assert.IsTrue (r.Read (), "init#7");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "init#8");
			return ret;
		}

		// from initial to StartObject
		void Read_CommonXamlType (XamlObjectReader r)
		{
			Assert.IsTrue (r.Read (), "ct#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ct#2");
			Assert.IsNotNull (r.Namespace, "ct#3");
			Assert.AreEqual ("x", r.Namespace.Prefix, "ct#3-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "ct#3-3");
			Assert.IsNull (r.Instance, "ct#4");

			Assert.IsTrue (r.Read (), "ct#5");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "ct#6");
		}

		// from initial to StartObject
		void Read_CommonClrType (XamlObjectReader r, object obj)
		{
			Assert.IsTrue (r.Read (), "ct#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ct#2");
			Assert.IsNotNull (r.Namespace, "ct#3");
			Assert.AreEqual (String.Empty, r.Namespace.Prefix, "ct#3-2");
			Assert.AreEqual ("clr-namespace:" + obj.GetType ().Namespace + ";assembly=" + obj.GetType ().Assembly.GetName ().Name, r.Namespace.Namespace, "ct#3-3");

/*
			Assert.IsTrue (r.Read (), "ct#4");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ct#5");
			Assert.IsNotNull (r.Namespace, "ct#6");
			Assert.AreEqual ("x", r.Namespace.Prefix, "ct#6-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "ct#6-3");
*/

			Assert.IsTrue (r.Read (), "ct#7");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "ct#8");
		}
	}

	class TestClass1
	{
	}

	public class TestClass3
	{
		public TestClass3 Nested { get; set; }
	}
}
