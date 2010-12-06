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

namespace MonoTests.System.Xaml
{
	[TestFixture]
	public partial class XamlObjectReaderTest : XamlReaderTestBase
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
		[ExpectedException (typeof (XamlObjectReaderException))]
		public void ReadNonConstructible ()
		{
			// XamlType has no default constructor.
			new XamlObjectReader (XamlLanguage.String);
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

		// Based on Common tests

		[Test]
		public void Read_String ()
		{
			var r = new XamlObjectReader ("foo");
			Read_String (r);
		}

		[Test]
		public void WriteNullMemberAsObject ()
		{
			var r = new XamlObjectReader (new TestClass4 ());
			WriteNullMemberAsObject (r, delegate {
				Assert.IsNull (r.Instance, "#x"); }
				);
		}
		
		[Test]
		public void StaticMember ()
		{
			var r = new XamlObjectReader (new TestClass5 ());
			StaticMember (r);
		}

		[Test]
		public void Skip ()
		{
			var r = new XamlObjectReader ("Foo");
			Skip (r);
		}
		
		[Test]
		public void Skip2 ()
		{
			var r = new XamlObjectReader ("Foo");
			Skip2 (r);
		}

		[Test]
		public void Read_XmlDocument ()
		{
			var doc = new XmlDocument ();
			doc.LoadXml ("<root xmlns='urn:foo'><elem attr='val' /></root>");
			var r = new XamlObjectReader (doc);
			Read_XmlDocument (r);
		}

		[Test]
		public void Read_NonPrimitive ()
		{
			var r = new XamlObjectReader (new TestClass3 ());
			Read_NonPrimitive (r);
		}
		
		[Test]
		public void Read_Type ()
		{
			var r = new XamlObjectReader (typeof (int));
			Read_TypeOrTypeExtension (r);
		}
		
		[Test]
		public void Read_TypeExtension ()
		{
			var tx = new TypeExtension (typeof (int));
			var r = new XamlObjectReader (tx);
			Read_TypeOrTypeExtension (r);
		}

		void Read_TypeOrTypeExtension (XamlObjectReader r)
		{
			Read_TypeOrTypeExtension (r, delegate {
				Assert.IsTrue (r.Instance is TypeExtension, "#26");
				}, XamlLanguage.PositionalParameters);
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
			Read_TypeOrTypeExtension2 (r, delegate {
				Assert.IsTrue (r.Instance is TypeExtension, "#26");
			}, XamlLanguage.PositionalParameters);
		}
		
		[Test]
		public void Read_Reference ()
		{
			var r = new XamlObjectReader (new Reference ("FooBar"));
			Read_Reference (r);
		}
		
		[Test]
		public void Read_Null ()
		{
			var r = new XamlObjectReader (null);
			Read_NullOrNullExtension (r, (object) null);
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
			Read_NullOrNullExtension (r, delegate {
				Assert.AreEqual (instance, r.Instance, "#26"); // null and NullExtension are different here.
			});
		}
		
		[Test]
		public void Read_StaticExtension ()
		{
			var r = new XamlObjectReader (new StaticExtension ("FooBar"));
			Read_StaticExtension (r, XamlLanguage.PositionalParameters);
		}
		
		[Test]
		public void Read_ListInt32 ()
		{
			var obj = new List<int> (new int [] {5, -3, int.MaxValue, 0});
			Read_ListInt32 (obj);
		}
		
		[Test]
		public void Read_ListInt32_2 ()
		{
			var obj = new List<int> (new int [0]);
			Read_ListInt32 (obj);
		}
		
		void Read_ListInt32 (List<int> obj)
		{
			var r = new XamlObjectReader (obj);
			Read_ListInt32 (r, delegate {
				Assert.AreEqual (obj, r.Instance, "#26");
				}, obj);
		}
		
		[Test]
		public void Read_ListType ()
		{
			var obj = new List<Type> (new Type [] {typeof (int), typeof (Dictionary<Type, XamlType>)}) { Capacity = 2 };
			var r = new XamlObjectReader (obj);
			Read_ListType (r, true);
		}

		[Test]
		public void Read_ListArray ()
		{
			var obj = new List<Array> (new Array [] { new int [] { 1,2,3}, new string [] { "foo", "bar", "baz" }}) { Capacity = 2 };
			var r = new XamlObjectReader (obj);
			Read_ListArray (r);
		}

		[Test]
		public void Read_ArrayList ()
		{
			var obj = new ArrayList (new int [] {5, -3, 0});
			var r = new XamlObjectReader (obj);
			Read_ArrayList (r);
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
		
		[Test]
		public void Read_MyArrayExtension ()
		{
			var obj = new MyArrayExtension (new int [] {5, -3, 0});
			var r = new XamlObjectReader (obj);
			Read_ArrayOrArrayExtensionOrMyArrayExtension (r, obj, typeof (MyArrayExtension));
		}

		void Read_ArrayOrArrayExtension (XamlObjectReader r, object instance)
		{
			Read_ArrayOrArrayExtensionOrMyArrayExtension (r, instance, typeof (ArrayExtension));
		}

		void Read_ArrayOrArrayExtensionOrMyArrayExtension (XamlObjectReader r, object instance, Type extType)
		{
			Read_ArrayOrArrayExtensionOrMyArrayExtension (r, delegate {
				Assert.AreEqual (instance, r.Instance, "#26"); // different between Array and ArrayExtension. Also, different from Type and TypeExtension (Type returns TypeExtension, while Array remains to return Array)
				}, extType);
		}

		[Test]
		public void Read_ArrayExtension2 ()
		{
			var r = new XamlObjectReader (new ArrayExtension (typeof (int)));
			Read_ArrayExtension2 (r);
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
		public void Read_Guid ()
		{
			var obj = Guid.NewGuid ();
			var r = new XamlObjectReader (obj);
			Assert.IsNotNull (r.SchemaContext.GetXamlType (typeof (Guid)).TypeConverter, "premise#1");
			Read_CommonClrType (r, obj);
			Assert.AreEqual (obj.ToString (), Read_Initialization (r, null), "#1");
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
		[ExpectedException (typeof (XamlObjectReaderException))]
		[Category ("NotWorking")]
		public void Read_XDataWrapper ()
		{
			var obj = new XDataWrapper () { Markup = new XData () {Text = "<my_xdata/>" } };
			var r = new XamlObjectReader (obj);
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

		[Test]
		public void Read_CustomMarkupExtension ()
		{
			var r = new XamlObjectReader (new MyExtension () { Foo = typeof (int), Bar = "v2", Baz = "v7"});
			Read_CustomMarkupExtension (r);
		}
		
		[Test]
		public void Read_CustomMarkupExtension2 ()
		{
			var r = new XamlObjectReader (new MyExtension2 () { Foo = typeof (int), Bar = "v2"});
			Read_CustomMarkupExtension2 (r);
		}
		
		[Test]
		public void Read_CustomMarkupExtension3 ()
		{
			var r = new XamlObjectReader (new MyExtension3 () { Foo = typeof (int), Bar = "v2"});
			Read_CustomMarkupExtension3 (r);
		}
		
		[Test]
		public void Read_CustomMarkupExtension4 ()
		{
			var r = new XamlObjectReader (new MyExtension4 () { Foo = typeof (int), Bar = "v2"});
			Read_CustomMarkupExtension4 (r);
		}
		
		[Test]
		public void Read_CustomMarkupExtension5 ()
		{
			// This cannot be written to XamlXmlWriter though...

			var r = new XamlObjectReader (new MyExtension5 ("foo", "bar"));
			Read_CustomMarkupExtension5 (r);
		}
		
		[Test]
		public void Read_CustomMarkupExtension6 ()
		{
			var r = new XamlObjectReader (new MyExtension6 ("foo"));
			Read_CustomMarkupExtension6 (r);
		}

		[Test]
		public void Read_ArgumentAttributed ()
		{
			var obj = new ArgumentAttributed ("foo", "bar");
			var r = new XamlObjectReader (obj);
			Read_ArgumentAttributed (r, obj);
		}

		[Test]
		public void Read_Dictionary ()
		{
			var obj = new Dictionary<string,object> ();
			obj ["Foo"] = 5.0;
			obj ["Bar"] = -6.5;
			var r = new XamlObjectReader (obj);
			Read_Dictionary (r);
		}
		
		[Test]
		public void Read_Dictionary2 ()
		{
			var obj = new Dictionary<string,Type> ();
			obj ["Foo"] = typeof (int);
			obj ["Bar"] = typeof (Dictionary<Type,XamlType>);
			var r = new XamlObjectReader (obj);
			Read_Dictionary2 (r, XamlLanguage.PositionalParameters);
		}
		
		[Test]
		public void PositionalParameters1 ()
		{
			// Note: this can be read, but cannot be written to XML.
			var obj = new PositionalParametersClass1 ("foo", 5);
			var r = new XamlObjectReader (obj);
			PositionalParameters1 (r);
		}
		
		[Test]
		public void PositionalParameters2 ()
		{
			var obj = new PositionalParametersWrapper ("foo", 5);
			var r = new XamlObjectReader (obj);
			PositionalParameters2 (r);
		}

		[Test]
		public void ComplexPositionalParameters ()
		{
			var obj = new ComplexPositionalParameterWrapper () { Param = new ComplexPositionalParameterClass (new ComplexPositionalParameterValue () { Foo = "foo" })};
			var r = new XamlObjectReader (obj);
			ComplexPositionalParameters (r);
		}
		
		[Test]
		public void Read_ListWrapper ()
		{
			var obj = new ListWrapper (new List<int> (new int [] {5, -3, 0}));
			var r = new XamlObjectReader (obj);
			Read_ListWrapper (r);
		}
		
		[Test]
		public void Read_ListWrapper2 () // read-write list member.
		{
			var obj = new ListWrapper2 (new List<int> (new int [] {5, -3, 0}));
			var r = new XamlObjectReader (obj);
			Read_ListWrapper2 (r);
		}

		[Test]
		public void Read_ContentIncluded ()
		{
			var obj = new ContentIncludedClass () { Content = "foo" };
			var r = new XamlObjectReader (obj);
			Read_ContentIncluded (r);
		}

		[Test]
		public void Read_PropertyDefinition ()
		{
			var obj = new PropertyDefinition () { Modifier = "protected", Name = "foo", Type = XamlLanguage.String };
			var r = new XamlObjectReader (obj);
			Read_PropertyDefinition (r);
		}

		[Test]
		public void Read_StaticExtensionWrapper ()
		{
			var obj = new StaticExtensionWrapper () { Param = new StaticExtension ("StaticExtensionWrapper.Foo") };
			var r = new XamlObjectReader (obj);
			Read_StaticExtensionWrapper (r);
		}

		[Test]
		public void Read_TypeExtensionWrapper ()
		{
			var obj = new TypeExtensionWrapper () { Param = new TypeExtension ("Foo") };
			var r = new XamlObjectReader (obj);
			Read_TypeExtensionWrapper (r);
		}
		
		[Test]
		public void Read_EventContainer ()
		{
			var obj = new EventContainer ();
			obj.Run += delegate { Console.Error.WriteLine ("done"); };
			var xr = new XamlObjectReader (obj);
			Read_EventContainer (xr);
		}
		
		[Test]
		public void Read_NamedItems ()
		{
			// foo
			// - bar
			// -- foo
			// - baz
			var obj = new NamedItem ("foo");
			var obj2 = new NamedItem ("bar");
			obj.References.Add (obj2);
			obj.References.Add (new NamedItem ("baz"));
			obj2.References.Add (obj);

			var xr = new XamlObjectReader (obj);
			Read_NamedItems (xr, true);
		}

		[Test]
		public void Read_NamedItems2 ()
		{
			// i1
			// - i2
			// -- i3
			// - i4
			// -- i3
			var obj = new NamedItem2 ("i1");
			var obj2 = new NamedItem2 ("i2");
			var obj3 = new NamedItem2 ("i3");
			var obj4 = new NamedItem2 ("i4");
			obj.References.Add (obj2);
			obj.References.Add (obj4);
			obj2.References.Add (obj3);
			obj4.References.Add (obj3);

			var xr = new XamlObjectReader (obj);
			Read_NamedItems2 (xr, true);
		}

		[Test]
		[Category ("NotWorking")]
		public void Read_XmlSerializableWrapper ()
		{
			var obj = new XmlSerializableWrapper (new XmlSerializable ("<root/>"));
			var xr = new XamlObjectReader (obj);
			Read_XmlSerializableWrapper (xr, true);
		}
	}
}
