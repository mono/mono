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

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

// Some test result remarks:
// - TypeExtension: [ConstructorArgument] -> PositionalParameters
// - StaticExtension: almost identical to TypeExtension
// - Reference: [ConstructorArgument], [ContentProperty] -> only ordinal member.
// - ArrayExtension: [ConstrutorArgument], [ContentProperty] -> no PositionalParameters, Items.
// - NullExtension: no member.
// - MyExtension: [ConstructorArgument] -> only ordinal members...hmm?

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
		public void WriteNullMemberAsObject ()
		{
			var r = new XamlObjectReader (new TestClass4 ());

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
			var xt = new XamlType (typeof (TestClass4), r.SchemaContext);
			Assert.AreEqual (xt, r.Type, "#17-2");
			Assert.IsTrue (r.Instance is TestClass4, "#17-3");
			Assert.AreEqual (2, xt.GetAllMembers ().Count, "#17-4");

			Assert.IsTrue (r.Read (), "#21");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#22");
			Assert.AreEqual (xt.GetMember ("Bar"), r.Member, "#22-2");

			Assert.IsTrue (r.Read (), "#26");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#27");
			Assert.AreEqual (XamlLanguage.Null, r.Type, "#27-2");
			Assert.IsNull (r.Instance, "#27-3");

			Assert.IsTrue (r.Read (), "#31");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#32");

			Assert.IsTrue (r.Read (), "#36");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#37");

			Assert.IsTrue (r.Read (), "#41");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#42");
			Assert.AreEqual (xt.GetMember ("Foo"), r.Member, "#42-2");

			Assert.IsTrue (r.Read (), "#43");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#43-2");
			Assert.AreEqual (XamlLanguage.Null, r.Type, "#43-3");
			Assert.IsNull (r.Instance, "#43-4");

			Assert.IsTrue (r.Read (), "#44");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#44-2");

			Assert.IsTrue (r.Read (), "#46");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#47");

			Assert.IsTrue (r.Read (), "#51");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#52");

			Assert.IsFalse (r.Read (), "#56");
			Assert.IsTrue (r.IsEof, "#57");
		}

		[Test]
		public void StaticMember ()
		{
			var r = new XamlObjectReader (new TestClass5 ());

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
			var xt = new XamlType (typeof (TestClass5), r.SchemaContext);
			Assert.AreEqual (xt, r.Type, "#17-2");
			Assert.IsTrue (r.Instance is TestClass5, "#17-3");
			Assert.AreEqual (2, xt.GetAllMembers ().Count, "#17-4");
			Assert.IsTrue (xt.GetAllMembers ().Any (xm => xm.Name == "Bar"), "#17-5");
			Assert.IsTrue (xt.GetAllMembers ().Any (xm => xm.Name == "Baz"), "#17-6");

			Assert.IsTrue (r.Read (), "#21");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#22");
			Assert.AreEqual (xt.GetMember ("Bar"), r.Member, "#22-2");

			Assert.IsTrue (r.Read (), "#26");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#27");
			Assert.AreEqual (XamlLanguage.Null, r.Type, "#27-2");
			Assert.IsNull (r.Instance, "#27-3");

			Assert.IsTrue (r.Read (), "#31");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#32");

			Assert.IsTrue (r.Read (), "#36");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#37");
			// static Foo is not included in GetAllXembers() return value.
			// nonpublic Baz does not appear either.

			Assert.IsTrue (r.Read (), "#51");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#52");

			Assert.IsFalse (r.Read (), "#56");
			Assert.IsTrue (r.IsEof, "#57");
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
		public void Read_Type ()
		{
			var r = new XamlObjectReader (typeof (int));
			Read_TypeOrTypeExtension (r, typeof (int));
		}
		
		[Test]
		public void Read_TypeExtension ()
		{
			var tx = new TypeExtension (typeof (int));
			var r = new XamlObjectReader (tx);
			Read_TypeOrTypeExtension (r, tx);
		}

		void Read_TypeOrTypeExtension (XamlObjectReader r, object obj)
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
		public void Read_Type2 ()
		{
			var r = new XamlObjectReader (typeof (TestClass1));
			Read_TypeOrTypeExtension2 (r);
		}
		
		[Test]
		public void Read_TypeExtension2 ()
		{
			var r = new XamlObjectReader (new TypeExtension (typeof (TestClass1)));
			Read_TypeOrTypeExtension2 (r);
		}

		void Read_TypeOrTypeExtension2 (XamlObjectReader r)
		{
			Assert.IsTrue (r.Read (), "#11");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#12");

			var defns = "clr-namespace:MonoTests.System.Xaml;assembly=" + GetType ().Assembly.GetName ().Name;

			Assert.AreEqual (String.Empty, r.Namespace.Prefix, "#13-2");
			Assert.AreEqual (defns, r.Namespace.Namespace, "#13-3:" + r.Namespace.Prefix);

			Assert.IsTrue (r.Read (), "#16");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#17");
			Assert.IsNotNull (r.Namespace, "#18");
			Assert.AreEqual ("x", r.Namespace.Prefix, "#18-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "#18-3:" + r.Namespace.Prefix);

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
		public void Read_Reference ()
		{
			var r = new XamlObjectReader (new Reference ("TestName"));
			Assert.IsTrue (r.Read (), "#11");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#12");
			Assert.AreEqual ("x", r.Namespace.Prefix, "#13-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "#13-3");

			Assert.IsTrue (r.Read (), "#21");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#22");
			var xt = new XamlType (typeof (Reference), r.SchemaContext);
			Assert.AreEqual (xt, r.Type, "#23-2");
			Assert.IsTrue (r.Instance is Reference, "#26");

			Assert.IsTrue (r.Read (), "#31");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#32");
			// unlike TypeExtension there is no PositionalParameters.
			Assert.AreEqual (xt.GetMember ("Name"), r.Member, "#33-2");

			// It is a ContentProperty (besides [ConstructorArgument])
			Assert.IsTrue (r.Read (), "#41");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "#42");
			Assert.AreEqual ("TestName", r.Value, "#43-2");

			Assert.IsTrue (r.Read (), "#51");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#52");

			Assert.IsTrue (r.Read (), "#61");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#62");

			Assert.IsFalse (r.Read (), "#71");
			Assert.IsTrue (r.IsEof, "#72");
		}


		[Test]
		public void Read_Null ()
		{
			var r = new XamlObjectReader (null);
			Read_NullOrNullExtension (r, null);
		}

		[Test]
		public void Read_NullExtension ()
		{
			var o = new NullExtension ();
			var r = new XamlObjectReader (o);
			Read_NullOrNullExtension (r, o);
		}
		
		void Read_NullOrNullExtension (XamlObjectReader r, object instance)
		{
			Assert.IsTrue (r.Read (), "#11");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#12");
			Assert.IsNotNull (r.Namespace, "#13");
			Assert.AreEqual ("x", r.Namespace.Prefix, "#13-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "#13-3");
			Assert.IsNull (r.Instance, "#14");

			Assert.IsTrue (r.Read (), "#21");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#22");
			Assert.AreEqual (new XamlType (typeof (NullExtension), r.SchemaContext), r.Type, "#23-2");
			Assert.AreEqual (instance, r.Instance, "#26"); // null and NullExtension are different here.

			Assert.IsTrue (r.Read (), "#61");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#62");

			Assert.IsFalse (r.Read (), "#71");
			Assert.IsTrue (r.IsEof, "#72");
		}

		[Test] // almost identical to TypeExtension (only type/instance difference)
		[Category ("NotWorking")]
		public void Read_StaticExtension ()
		{
			var r = new XamlObjectReader (new StaticExtension ("MyMember"));
			Assert.IsTrue (r.Read (), "#11");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#12");
			Assert.IsNotNull (r.Namespace, "#13");
			Assert.AreEqual ("x", r.Namespace.Prefix, "#13-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "#13-3");
			Assert.IsNull (r.Instance, "#14");

			Assert.IsTrue (r.Read (), "#21");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#22");
			Assert.AreEqual (new XamlType (typeof (StaticExtension), r.SchemaContext), r.Type, "#23-2");
			Assert.IsTrue (r.Instance is StaticExtension, "#26");

			Assert.IsTrue (r.Read (), "#31");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#32");
			Assert.AreEqual (XamlLanguage.PositionalParameters, r.Member, "#33-2");

			Assert.IsTrue (r.Read (), "#41");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "#42");
			Assert.AreEqual ("MyMember", r.Value, "#43-2");

			Assert.IsTrue (r.Read (), "#51");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#52");

			Assert.IsTrue (r.Read (), "#61");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#62");

			Assert.IsFalse (r.Read (), "#71");
			Assert.IsTrue (r.IsEof, "#72");
		}

		[Test]
		public void Read_Array ()
		{
			var obj = new int [] {5, -3, 0};
			var r = new XamlObjectReader (obj);
			Read_ArrayOrArrayExtension (r, obj);
		}
		
		[Test]
		public void Read_ArrayExtension ()
		{
			var obj = new ArrayExtension (new int [] {5, -3, 0});
			var r = new XamlObjectReader (obj);
			Read_ArrayOrArrayExtension (r, obj);
		}

		void Read_ArrayOrArrayExtension (XamlObjectReader r, object instance)
		{
			Assert.IsTrue (r.Read (), "#11");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#12");
			Assert.IsNotNull (r.Namespace, "#13");
			Assert.AreEqual ("x", r.Namespace.Prefix, "#13-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "#13-3");

			Assert.IsTrue (r.Read (), "#21");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#22");
			var xt = new XamlType (typeof (ArrayExtension), r.SchemaContext);
			Assert.AreEqual (xt, r.Type, "#23");
			Assert.AreEqual (instance, r.Instance, "#26"); // different between Array and ArrayExtension. Also, different from Type and TypeExtension (Type returns TypeExtension, while Array remains to return Array)

			// This assumption on member ordering ("Type" then "Items") is somewhat wrong, and we might have to adjust it in the future.

			Assert.IsTrue (r.Read (), "#31");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#32");
			Assert.AreEqual (xt.GetMember ("Type"), r.Member, "#33");

			Assert.IsTrue (r.Read (), "#41");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "#42");
			Assert.AreEqual ("x:Int32", r.Value, "#43");

			Assert.IsTrue (r.Read (), "#51");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#52");

			Assert.IsTrue (r.Read (), "#61");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#62");
			Assert.AreEqual (xt.GetMember ("Items"), r.Member, "#63");

			Assert.IsTrue (r.Read (), "#71");
			Assert.AreEqual (XamlNodeType.GetObject, r.NodeType, "#71-2");
			Assert.IsNull (r.Type, "#71-3");
			Assert.IsNull (r.Member, "#71-4");
			Assert.IsNull (r.Value, "#71-5");

			Assert.IsTrue (r.Read (), "#72");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#72-2");
			Assert.AreEqual (XamlLanguage.Items, r.Member, "#72-3");

			string [] values = {"5", "-3", "0"};
			for (int i = 0; i < 3; i++) {
				Assert.IsTrue (r.Read (), i + "#73");
				Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, i + "#73-2");
				Assert.IsTrue (r.Read (), i + "#74");
				Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, i + "#74-2");
				Assert.AreEqual (XamlLanguage.Initialization, r.Member, i + "#74-3");
				Assert.IsTrue (r.Read (), i + "#75");
				Assert.IsNotNull (r.Value, i + "#75-2");
				Assert.AreEqual (values [i], r.Value, i + "#73-3");
				Assert.IsTrue (r.Read (), i + "#74");
				Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, i + "#74-2");
				Assert.IsTrue (r.Read (), i + "#75");
				Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, i + "#75-2");
			}

			Assert.IsTrue (r.Read (), "#81");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#82"); // XamlLanguage.Items

			Assert.IsTrue (r.Read (), "#83");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#84"); // GetObject

			Assert.IsTrue (r.Read (), "#85");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#86"); // ArrayExtension.Items

			Assert.IsTrue (r.Read (), "#87");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#88"); // ArrayExtension

			Assert.IsFalse (r.Read (), "#89");
		}

		[Test] // It gives Type member, not PositionalParameters... and no Items member here.
		public void Read_ArrayExtension2 ()
		{
			var r = new XamlObjectReader (new ArrayExtension (typeof (int)));
			Assert.IsTrue (r.Read (), "#11");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#12");
			Assert.IsNotNull (r.Namespace, "#13");
			Assert.AreEqual ("x", r.Namespace.Prefix, "#13-2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "#13-3");
			Assert.IsNull (r.Instance, "#14");

			Assert.IsTrue (r.Read (), "#21");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#22");
			var xt = new XamlType (typeof (ArrayExtension), r.SchemaContext);
			Assert.AreEqual (xt, r.Type, "#23-2");
			Assert.IsTrue (r.Instance is ArrayExtension, "#26");

			Assert.IsTrue (r.Read (), "#31");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#32");
			Assert.AreEqual (xt.GetMember ("Type"), r.Member, "#33-2");

			Assert.IsTrue (r.Read (), "#41");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "#42");
			Assert.AreEqual ("x:Int32", r.Value, "#43-2");

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

		[Test]
		public void Read_CustomMarkupExtension ()
		{
			var r = new XamlObjectReader (new MyExtension () { Foo = typeof (int), Bar = "v2", Baz = "v7"});
			r.Read (); // ns
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#1");
			r.Read (); // ns
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#1-2");
			r.Read ();
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#2-0");
			Assert.IsFalse (r.IsEof, "#1");
			var xt = r.Type;

			r.Read ();
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#2-1");
			Assert.IsFalse (r.IsEof, "#2-2");
			Assert.AreEqual (xt.GetMember ("Bar"), r.Member, "#2-3");

			Assert.IsTrue (r.Read (), "#2-4");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "#2-5");
			Assert.AreEqual ("v2", r.Value, "#2-6");

			Assert.IsTrue (r.Read (), "#2-7");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#2-8");

			r.Read ();
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#3-1");
			Assert.IsFalse (r.IsEof, "#3-2");
			Assert.AreEqual (xt.GetMember ("Baz"), r.Member, "#3-3");

			Assert.IsTrue (r.Read (), "#3-4");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "#3-5");
			Assert.AreEqual ("v7", r.Value, "#3-6");

			Assert.IsTrue (r.Read (), "#3-7");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#3-8");
			
			r.Read ();
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#4-1");
			Assert.IsFalse (r.IsEof, "#4-2");
			Assert.AreEqual (xt.GetMember ("Foo"), r.Member, "#4-3");
			Assert.IsTrue (r.Read (), "#4-4");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "#4-5");
			Assert.AreEqual ("x:Int32", r.Value, "#4-6");

			Assert.IsTrue (r.Read (), "#4-7");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#4-8");

			Assert.IsTrue (r.Read (), "#5");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "#5-2");

			Assert.IsFalse (r.Read (), "#6");
		}

		[Test]
		public void Read_CustomMarkupExtension2 ()
		{
			var r = new XamlObjectReader (new MyExtension2 () { Foo = typeof (int), Bar = "v2"});
			r.Read (); // ns
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#1");
			r.Read (); // note that there wasn't another NamespaceDeclaration.
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#2-0");
			var xt = r.Type;
			Assert.AreEqual (r.SchemaContext.GetXamlType (typeof (MyExtension2)), xt, "#2");
			Assert.IsTrue (r.Read (), "#3");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#3-2");
			Assert.AreEqual (XamlLanguage.Initialization, r.Member, "#4");
			Assert.IsTrue (r.Read (), "#5");
			Assert.AreEqual ("MonoTests.System.Xaml.MyExtension2", r.Value, "#6");
			Assert.IsTrue (r.Read (), "#7"); // EndMember
			Assert.IsTrue (r.Read (), "#8"); // EndObject
			Assert.IsFalse (r.Read (), "#9");
		}

		[Test]
		public void Read_CustomMarkupExtension3 ()
		{
			var r = new XamlObjectReader (new MyExtension3 () { Foo = typeof (int), Bar = "v2"});
			r.Read (); // ns
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#1");
			r.Read (); // note that there wasn't another NamespaceDeclaration.
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#2-0");
			var xt = r.Type;
			Assert.AreEqual (r.SchemaContext.GetXamlType (typeof (MyExtension3)), xt, "#2");
			Assert.IsTrue (r.Read (), "#3");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#3-2");
			Assert.AreEqual (XamlLanguage.Initialization, r.Member, "#4");
			Assert.IsTrue (r.Read (), "#5");
			Assert.AreEqual ("MonoTests.System.Xaml.MyExtension3", r.Value, "#6");
			Assert.IsTrue (r.Read (), "#7"); // EndMember
			Assert.IsTrue (r.Read (), "#8"); // EndObject
			Assert.IsFalse (r.Read (), "#9");
		}

		[Test]
		public void Read_CustomMarkupExtension4 ()
		{
			var r = new XamlObjectReader (new MyExtension4 () { Foo = typeof (int), Bar = "v2"});
			r.Read (); // ns
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "#1");
			r.Read (); // note that there wasn't another NamespaceDeclaration.
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "#2-0");
			var xt = r.Type;
			Assert.AreEqual (r.SchemaContext.GetXamlType (typeof (MyExtension4)), xt, "#2");
			Assert.IsTrue (r.Read (), "#3");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "#3-2");
			Assert.AreEqual (XamlLanguage.Initialization, r.Member, "#4");
			Assert.IsTrue (r.Read (), "#5");
			Assert.AreEqual ("MonoTests.System.Xaml.MyExtension4", r.Value, "#6");
			Assert.IsTrue (r.Read (), "#7"); // EndMember
			Assert.IsTrue (r.Read (), "#8"); // EndObject
			Assert.IsFalse (r.Read (), "#9");
		}

		[Test]
		public void Read_ArgumentAttributed ()
		{
			var obj = new ArgumentAttributed ("foo", "bar");
			var r = new XamlObjectReader (obj);
			Read_CommonClrType (r, obj, new KeyValuePair<string,string> ("x", XamlLanguage.Xaml2006Namespace));
			var args = Read_AttributedArguments_String (r, new string [] {"arg1", "arg2"});
			Assert.AreEqual ("foo", args [0], "#1");
			Assert.AreEqual ("bar", args [1], "#2");
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

		object [] Read_AttributedArguments_String (XamlObjectReader r, string [] argNames) // valid only for string arguments.
		{
			object [] ret = new object [argNames.Length];

			Assert.IsTrue (r.Read (), "attarg.Arguments.Start1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "attarg.Arguments.Start2");
			Assert.IsNotNull (r.Member, "attarg.Arguments.Start3");
			Assert.AreEqual (XamlLanguage.Arguments, r.Member, "attarg.Arguments.Start4");
			for (int i = 0; i < argNames.Length; i++) {
				string arg = argNames [i];
				Assert.IsTrue (r.Read (), "attarg.ArgStartObject1." + arg);
				Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "attarg.ArgStartObject2." + arg);
				Assert.AreEqual (typeof (string), r.Type.UnderlyingType, "attarg.ArgStartObject3." + arg);
				Assert.IsTrue (r.Read (), "attarg.ArgStartMember1." + arg);
				Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "attarg.ArgStartMember2." + arg);
				Assert.AreEqual (XamlLanguage.Initialization, r.Member, "attarg.ArgStartMember3." + arg); // (as the argument is string here by definition)
				Assert.IsTrue (r.Read (), "attarg.ArgValue1." + arg);
				Assert.AreEqual (XamlNodeType.Value, r.NodeType, "attarg.ArgValue2." + arg);
				Assert.AreEqual (typeof (string), r.Value.GetType (), "attarg.ArgValue3." + arg);
				ret [i] = r.Value;
				Assert.IsTrue (r.Read (), "attarg.ArgEndMember1." + arg);
				Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "attarg.ArgEndMember2." + arg);
				Assert.IsTrue (r.Read (), "attarg.ArgEndObject1." + arg);
				Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "attarg.ArgEndObject2." + arg);
			}
			Assert.IsTrue (r.Read (), "attarg.Arguments.End1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "attarg.Arguments.End2");
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
		void Read_CommonClrType (XamlObjectReader r, object obj, params KeyValuePair<string,string> [] additionalNamespaces)
		{
			Assert.IsTrue (r.Read (), "ct#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ct#2");
			Assert.IsNotNull (r.Namespace, "ct#3");
			Assert.AreEqual (String.Empty, r.Namespace.Prefix, "ct#3-2");
			Assert.AreEqual ("clr-namespace:" + obj.GetType ().Namespace + ";assembly=" + obj.GetType ().Assembly.GetName ().Name, r.Namespace.Namespace, "ct#3-3");

			foreach (var kvp in additionalNamespaces) {
				Assert.IsTrue (r.Read (), "ct#4." + kvp.Key);
				Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ct#5." + kvp.Key);
				Assert.IsNotNull (r.Namespace, "ct#6." + kvp.Key);
				Assert.AreEqual (kvp.Key, r.Namespace.Prefix, "ct#6-2." + kvp.Key);
				Assert.AreEqual (kvp.Value, r.Namespace.Namespace, "ct#6-3." + kvp.Key);
			}

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

	public class TestClass4
	{
		public string Foo { get; set; }
		public string Bar { get; set; }
	}
	
	public class TestClass5
	{
		public static string Foo { get; set; }
		public string Bar { get; set; }
		public string Baz { internal get; set; }
	}

	public class MyExtension : MarkupExtension
	{
		public MyExtension ()
		{
		}

		public MyExtension (Type arg1, string arg2, string arg3)
		{
			Foo = arg1;
			Bar = arg2;
			Baz = arg3;
		}

		[ConstructorArgument ("arg1")]
		public Type Foo { get; set; }
		
		[ConstructorArgument ("arg2")]
		public string Bar { get; set; }
		
		[ConstructorArgument ("arg3")]
		public string Baz { get; set; }

		public override object ProvideValue (IServiceProvider provider)
		{
			return "provided_value";
		}
	}

	[TypeConverter (typeof (StringConverter))] // This attribute is the markable difference between MyExtension and this type.
	public class MyExtension2 : MarkupExtension
	{
		public MyExtension2 ()
		{
		}

		public MyExtension2 (Type arg1, string arg2)
		{
			Foo = arg1;
			Bar = arg2;
		}

		[ConstructorArgument ("arg1")]
		public Type Foo { get; set; }
		
		[ConstructorArgument ("arg2")]
		public string Bar { get; set; }

		public override object ProvideValue (IServiceProvider provider)
		{
			return "provided_value";
		}
	}

	[TypeConverter (typeof (StringConverter))] // same as MyExtension2 except that it is *not* MarkupExtension.
	public class MyExtension3
	{
		public MyExtension3 ()
		{
		}

		// cf. According to [MS-XAML-2009] 3.2.1.11, constructors are invalid unless the type is derived from TypeExtension. So, it is likely *ignored*.
		public MyExtension3 (Type arg1, string arg2)
		{
			Foo = arg1;
			Bar = arg2;
		}

		[ConstructorArgument ("arg1")]
		public Type Foo { get; set; }
		
		[ConstructorArgument ("arg2")]
		public string Bar { get; set; }
	}

	[TypeConverter (typeof (DateTimeConverter))] // same as MyExtension3 except for the type converter.
	public class MyExtension4
	{
		public MyExtension4 ()
		{
		}

		// cf. According to [MS-XAML-2009] 3.2.1.11, constructors are invalid unless the type is derived from TypeExtension. So, it is likely *ignored*.
		public MyExtension4 (Type arg1, string arg2)
		{
			Foo = arg1;
			Bar = arg2;
		}

		[ConstructorArgument ("arg1")]
		public Type Foo { get; set; }
		
		[ConstructorArgument ("arg2")]
		public string Bar { get; set; }
	}
}
