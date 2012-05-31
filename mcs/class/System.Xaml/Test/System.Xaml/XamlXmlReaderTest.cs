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
using System.Globalization;
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
	public class XamlXmlReaderTest : XamlReaderTestBase
	{
		// read test

		XamlReader GetReader (string filename)
		{
#if NET_4_5
			string ver = "net_4_5";
#else
			string ver = "net_4_0";
#endif
			string xml = File.ReadAllText (Path.Combine ("Test/XmlFiles", filename)).Replace ("net_4_0", ver);
			return new XamlXmlReader (XmlReader.Create (new StringReader (xml)));
		}

		void ReadTest (string filename)
		{
			var r = GetReader (filename);
			while (!r.IsEof)
				r.Read ();
		}

		[Test]
		public void SchemaContext ()
		{
			Assert.AreNotEqual (XamlLanguage.Type.SchemaContext, new XamlXmlReader (XmlReader.Create (new StringReader ("<root/>"))).SchemaContext, "#1");
		}

		[Test]
		public void Read_Int32 ()
		{
			ReadTest ("Int32.xml");
		}

		[Test]
		public void Read_DateTime ()
		{
			ReadTest ("DateTime.xml");
		}

		[Test]
		public void Read_TimeSpan ()
		{
			ReadTest ("TimeSpan.xml");
		}

		[Test]
		public void Read_ArrayInt32 ()
		{
			ReadTest ("Array_Int32.xml");
		}

		[Test]
		public void Read_DictionaryInt32String ()
		{
			ReadTest ("Dictionary_Int32_String.xml");
		}

		[Test]
		public void Read_DictionaryStringType ()
		{
			ReadTest ("Dictionary_String_Type.xml");
		}

		[Test]
		public void Read_SilverlightApp1 ()
		{
			ReadTest ("SilverlightApp1.xaml");
		}

		[Test]
		public void Read_Guid ()
		{
			ReadTest ("Guid.xml");
		}

		[Test]
		public void Read_GuidFactoryMethod ()
		{
			ReadTest ("GuidFactoryMethod.xml");
		}

		[Test]
		public void ReadInt32Details ()
		{
			var r = GetReader ("Int32.xml");

			Assert.IsTrue (r.Read (), "ns#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns#2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "ns#3");

			Assert.IsTrue (r.Read (), "so#1");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "so#2");
			Assert.AreEqual (XamlLanguage.Int32, r.Type, "so#3");

			ReadBase (r);

			Assert.IsTrue (r.Read (), "sinit#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sinit#2");
			Assert.AreEqual (XamlLanguage.Initialization, r.Member, "sinit#3");

			Assert.IsTrue (r.Read (), "vinit#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vinit#2");
			Assert.AreEqual ("5", r.Value, "vinit#3"); // string

			Assert.IsTrue (r.Read (), "einit#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "einit#2");

			Assert.IsTrue (r.Read (), "eo#1");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "eo#2");

			Assert.IsFalse (r.Read (), "end");
		}

		[Test]
		public void ReadDateTimeDetails ()
		{
			var r = GetReader ("DateTime.xml");

			Assert.IsTrue (r.Read (), "ns#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns#2");
			Assert.AreEqual ("clr-namespace:System;assembly=mscorlib", r.Namespace.Namespace, "ns#3");

			Assert.IsTrue (r.Read (), "so#1");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "so#2");
			Assert.AreEqual (r.SchemaContext.GetXamlType (typeof (DateTime)), r.Type, "so#3");

			ReadBase (r);

			Assert.IsTrue (r.Read (), "sinit#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sinit#2");
			Assert.AreEqual (XamlLanguage.Initialization, r.Member, "sinit#3");

			Assert.IsTrue (r.Read (), "vinit#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vinit#2");
			Assert.AreEqual ("2010-04-14", r.Value, "vinit#3"); // string

			Assert.IsTrue (r.Read (), "einit#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "einit#2");

			Assert.IsTrue (r.Read (), "eo#1");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "eo#2");
			Assert.IsFalse (r.Read (), "end");
		}

		[Test]
		public void ReadGuidFactoryMethodDetails ()
		{
			var r = GetReader ("GuidFactoryMethod.xml");

			Assert.IsTrue (r.Read (), "ns#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns#2");
			Assert.AreEqual ("clr-namespace:System;assembly=mscorlib", r.Namespace.Namespace, "ns#3");
			Assert.AreEqual (String.Empty, r.Namespace.Prefix, "ns#4");

			Assert.IsTrue (r.Read (), "ns2#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns2#2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "ns2#3");
			Assert.AreEqual ("x", r.Namespace.Prefix, "ns2#4");

			Assert.IsTrue (r.Read (), "so#1");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "so#2");
			var xt = r.SchemaContext.GetXamlType (typeof (Guid));
			Assert.AreEqual (xt, r.Type, "so#3");

			ReadBase (r);

			Assert.IsTrue (r.Read (), "sfactory#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sfactory#2");
			Assert.AreEqual (XamlLanguage.FactoryMethod, r.Member, "sfactory#3");

			Assert.IsTrue (r.Read (), "vfactory#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vfactory#2");
			Assert.AreEqual ("Parse", r.Value, "vfactory#3"); // string

			Assert.IsTrue (r.Read (), "efactory#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "efactory#2");

			Assert.IsTrue (r.Read (), "sarg#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sarg#2");
			Assert.AreEqual (XamlLanguage.Arguments, r.Member, "sarg#3");

			Assert.IsTrue (r.Read (), "sarg1#1");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "sarg1#2");
			Assert.AreEqual (XamlLanguage.String, r.Type, "sarg1#3");

			Assert.IsTrue (r.Read (), "sInit#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sInit#2");
			Assert.AreEqual (XamlLanguage.Initialization, r.Member, "sInit#3");

			Assert.IsTrue (r.Read (), "varg1#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "varg1#2");
			Assert.AreEqual ("9c3345ec-8922-4662-8e8d-a4e41f47cf09", r.Value, "varg1#3");

			Assert.IsTrue (r.Read (), "eInit#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "eInit#2");

			Assert.IsTrue (r.Read (), "earg1#1");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "earg1#2");

			Assert.IsTrue (r.Read (), "earg#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "earg#2");


			Assert.IsTrue (r.Read (), "eo#1");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "eo#2");

			Assert.IsFalse (r.Read (), "end");
		}

		[Test]
		public void ReadEventStore ()
		{
			var r = GetReader ("EventStore2.xml");

			var xt = r.SchemaContext.GetXamlType (typeof (EventStore));
			var xm = xt.GetMember ("Event1");
			Assert.IsNotNull (xt, "premise#1");
			Assert.IsNotNull (xm, "premise#2");
			Assert.IsTrue (xm.IsEvent, "premise#3");
			while (true) {
				r.Read ();
				if (r.Member != null && r.Member.IsEvent)
					break;
				if (r.IsEof)
					Assert.Fail ("Items did not appear");
			}

			Assert.AreEqual (xm, r.Member, "#x1");
			Assert.AreEqual ("Event1", r.Member.Name, "#x2");

			Assert.IsTrue (r.Read (), "#x11");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "#x12");
			Assert.AreEqual ("Method1", r.Value, "#x13");

			Assert.IsTrue (r.Read (), "#x21");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#x22");

			xm = xt.GetMember ("Event2");
			Assert.IsTrue (r.Read (), "#x31");
			Assert.AreEqual (xm, r.Member, "#x32");
			Assert.AreEqual ("Event2", r.Member.Name, "#x33");

			Assert.IsTrue (r.Read (), "#x41");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "#x42");
			Assert.AreEqual ("Method2", r.Value, "#x43");

			Assert.IsTrue (r.Read (), "#x51");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#x52");

			Assert.IsTrue (r.Read (), "#x61");
			Assert.AreEqual ("Event1", r.Member.Name, "#x62");

			Assert.IsTrue (r.Read (), "#x71");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "#x72");
			Assert.AreEqual ("Method3", r.Value, "#x73"); // nonexistent, but no need to raise an error.

			Assert.IsTrue (r.Read (), "#x81");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "#x82");

			while (!r.IsEof)
				r.Read ();

			r.Close ();
		}

		// common XamlReader tests.

		[Test]
		public void Read_String ()
		{
			var r = GetReader ("String.xml");
			Read_String (r);
		}

		[Test]
		public void WriteNullMemberAsObject ()
		{
			var r = GetReader ("TestClass4.xml");
			WriteNullMemberAsObject (r, null);
		}
		
		[Test]
		public void StaticMember ()
		{
			var r = GetReader ("TestClass5.xml");
			StaticMember (r);
		}

		[Test]
		public void Skip ()
		{
			var r = GetReader ("String.xml");
			Skip (r);
		}
		
		[Test]
		public void Skip2 ()
		{
			var r = GetReader ("String.xml");
			Skip2 (r);
		}

		[Test]
		public void Read_XmlDocument ()
		{
			var doc = new XmlDocument ();
			doc.LoadXml ("<root xmlns='urn:foo'><elem attr='val' /></root>");
			// note that corresponding XamlXmlWriter is untested yet.
			var r = GetReader ("XmlDocument.xml");
			Read_XmlDocument (r);
		}

		[Test]
		public void Read_NonPrimitive ()
		{
			var r = GetReader ("NonPrimitive.xml");
			Read_NonPrimitive (r);
		}
		
		[Test]
		public void Read_TypeExtension ()
		{
			var r = GetReader ("Type.xml");
			Read_TypeOrTypeExtension (r, null, XamlLanguage.Type.GetMember ("Type"));
		}
		
		[Test]
		public void Read_Type2 ()
		{
			var r = GetReader ("Type2.xml");
			Read_TypeOrTypeExtension2 (r, null, XamlLanguage.Type.GetMember ("Type"));
		}
		
		[Test]
		public void Read_Reference ()
		{
			var r = GetReader ("Reference.xml");
			Read_Reference (r);
		}
		
		[Test]
		public void Read_Null ()
		{
			var r = GetReader ("NullExtension.xml");
			Read_NullOrNullExtension (r, null);
		}
		
		[Test]
		public void Read_StaticExtension ()
		{
			var r = GetReader ("StaticExtension.xml");
			Read_StaticExtension (r, XamlLanguage.Static.GetMember ("Member"));
		}
		
		[Test]
		public void Read_ListInt32 ()
		{
			var r = GetReader ("List_Int32.xml");
			Read_ListInt32 (r, null, new int [] {5, -3, int.MaxValue, 0}.ToList ());
		}
		
		[Test]
		public void Read_ListInt32_2 ()
		{
			var r = GetReader ("List_Int32_2.xml");
			Read_ListInt32 (r, null, new int [0].ToList ());
		}
		
		[Test]
		public void Read_ListType ()
		{
			var r = GetReader ("List_Type.xml");
			Read_ListType (r, false);
		}

		[Test]
		public void Read_ListArray ()
		{
			var r = GetReader ("List_Array.xml");
			Read_ListArray (r);
		}

		[Test]
		public void Read_ArrayList ()
		{
			var r = GetReader ("ArrayList.xml");
			Read_ArrayList (r);
		}
		
		[Test]
		public void Read_Array ()
		{
			var r = GetReader ("ArrayExtension.xml");
			Read_ArrayOrArrayExtensionOrMyArrayExtension (r, null, typeof (ArrayExtension));
		}
		
		[Test]
		public void Read_MyArrayExtension ()
		{
			var r = GetReader ("MyArrayExtension.xml");
			Read_ArrayOrArrayExtensionOrMyArrayExtension (r, null, typeof (MyArrayExtension));
		}

		[Test]
		public void Read_ArrayExtension2 ()
		{
			var r = GetReader ("ArrayExtension2.xml");
			Read_ArrayExtension2 (r);
		}

		[Test]
		public void Read_CustomMarkupExtension ()
		{
			var r = GetReader ("MyExtension.xml");
			Read_CustomMarkupExtension (r);
		}
		
		[Test]
		public void Read_CustomMarkupExtension2 ()
		{
			var r = GetReader ("MyExtension2.xml");
			Read_CustomMarkupExtension2 (r);
		}
		
		[Test]
		public void Read_CustomMarkupExtension3 ()
		{
			var r = GetReader ("MyExtension3.xml");
			Read_CustomMarkupExtension3 (r);
		}
		
		[Test]
		public void Read_CustomMarkupExtension4 ()
		{
			var r = GetReader ("MyExtension4.xml");
			Read_CustomMarkupExtension4 (r);
		}
		
		[Test]
		public void Read_CustomMarkupExtension6 ()
		{
			var r = GetReader ("MyExtension6.xml");
			Read_CustomMarkupExtension6 (r);
		}

		[Test]
		public void Read_ArgumentAttributed ()
		{
			var obj = new ArgumentAttributed ("foo", "bar");
			var r = GetReader ("ArgumentAttributed.xml");
			Read_ArgumentAttributed (r, obj);
		}

		[Test]
		public void Read_Dictionary ()
		{
			var obj = new Dictionary<string,object> ();
			obj ["Foo"] = 5.0;
			obj ["Bar"] = -6.5;
			var r = GetReader ("Dictionary_String_Double.xml");
			Read_Dictionary (r);
		}
		
		[Test]
		public void Read_Dictionary2 ()
		{
			var obj = new Dictionary<string,Type> ();
			obj ["Foo"] = typeof (int);
			obj ["Bar"] = typeof (Dictionary<Type,XamlType>);
			var r = GetReader ("Dictionary_String_Type_2.xml");
			Read_Dictionary2 (r, XamlLanguage.Type.GetMember ("Type"));
		}
		
		[Test]
		public void PositionalParameters2 ()
		{
			var r = GetReader ("PositionalParametersWrapper.xml");
			PositionalParameters2 (r);
		}

		[Test]
		public void ComplexPositionalParameters ()
		{
			var r = GetReader ("ComplexPositionalParameterWrapper.xml");
			ComplexPositionalParameters (r);
		}
		
		[Test]
		public void Read_ListWrapper ()
		{
			var r = GetReader ("ListWrapper.xml");
			Read_ListWrapper (r);
		}
		
		[Test]
		public void Read_ListWrapper2 () // read-write list member.
		{
			var r = GetReader ("ListWrapper2.xml");
			Read_ListWrapper2 (r);
		}

		[Test]
		public void Read_ContentIncluded ()
		{
			var r = GetReader ("ContentIncluded.xml");
			Read_ContentIncluded (r);
		}

		[Test]
		public void Read_PropertyDefinition ()
		{
			var r = GetReader ("PropertyDefinition.xml");
			Read_PropertyDefinition (r);
		}

		[Test]
		public void Read_StaticExtensionWrapper ()
		{
			var r = GetReader ("StaticExtensionWrapper.xml");
			Read_StaticExtensionWrapper (r);
		}

		[Test]
		public void Read_TypeExtensionWrapper ()
		{
			var r = GetReader ("TypeExtensionWrapper.xml");
			Read_TypeExtensionWrapper (r);
		}

		[Test]
		public void Read_NamedItems ()
		{
			var r = GetReader ("NamedItems.xml");
			Read_NamedItems (r, false);
		}

		[Test]
		public void Read_NamedItems2 ()
		{
			var r = GetReader ("NamedItems2.xml");
			Read_NamedItems2 (r, false);
		}

		[Test]
		public void Read_XmlSerializableWrapper ()
		{
			var r = GetReader ("XmlSerializableWrapper.xml");
			Read_XmlSerializableWrapper (r, false);
		}

		[Test]
		public void Read_XmlSerializable ()
		{
			var r = GetReader ("XmlSerializable.xml");
			Read_XmlSerializable (r);
		}

		[Test]
		public void Read_ListXmlSerializable ()
		{
			var r = GetReader ("List_XmlSerializable.xml");
			Read_ListXmlSerializable (r);
		}

		[Test]
		public void Read_AttachedProperty ()
		{
			var r = GetReader ("AttachedProperty.xml");
			Read_AttachedProperty (r);
		}

		[Test]
		public void Read_AbstractWrapper ()
		{
			var r = GetReader ("AbstractContainer.xml");
			while (!r.IsEof)
				r.Read ();
		}

		[Test]
		public void Read_ReadOnlyPropertyContainer ()
		{
			var r = GetReader ("ReadOnlyPropertyContainer.xml");
			while (!r.IsEof)
				r.Read ();
		}

		[Test]
		public void Read_TypeConverterOnListMember ()
		{
			var r = GetReader ("TypeConverterOnListMember.xml");
			Read_TypeConverterOnListMember (r);
		}

		[Test]
		public void Read_EnumContainer ()
		{
			var r = GetReader ("EnumContainer.xml");
			Read_EnumContainer (r);
		}

		[Test]
		public void Read_CollectionContentProperty ()
		{
			var r = GetReader ("CollectionContentProperty.xml");
			Read_CollectionContentProperty (r, false);
		}

		[Test]
		public void Read_CollectionContentProperty2 ()
		{
			// bug #681835
			var r = GetReader ("CollectionContentProperty2.xml");
			Read_CollectionContentProperty (r, true);
		}

		[Test]
		public void Read_CollectionContentPropertyX ()
		{
			var r = GetReader ("CollectionContentPropertyX.xml");
			Read_CollectionContentPropertyX (r, false);
		}

		[Test]
		public void Read_CollectionContentPropertyX2 ()
		{
			var r = GetReader ("CollectionContentPropertyX2.xml");
			Read_CollectionContentPropertyX (r, true);
		}

		[Test]
		public void Read_AmbientPropertyContainer ()
		{
			var r = GetReader ("AmbientPropertyContainer.xml");
			Read_AmbientPropertyContainer (r, false);
		}

		[Test]
		public void Read_AmbientPropertyContainer2 ()
		{
			var r = GetReader ("AmbientPropertyContainer2.xml");
			Read_AmbientPropertyContainer (r, true);
		}

		[Test]
		public void Read_NullableContainer ()
		{
			var r = GetReader ("NullableContainer.xml");
			Read_NullableContainer (r);
		}

		// It is not really a common test; it just makes use of base helper methods.
		[Test]
		public void Read_DirectListContainer ()
		{
			var r = GetReader ("DirectListContainer.xml");
			Read_DirectListContainer (r);
		}

		// It is not really a common test; it just makes use of base helper methods.
		[Test]
		public void Read_DirectDictionaryContainer ()
		{
			var r = GetReader ("DirectDictionaryContainer.xml");
			Read_DirectDictionaryContainer (r);
		}

		// It is not really a common test; it just makes use of base helper methods.
		[Test]
		public void Read_DirectDictionaryContainer2 ()
		{
			var r = GetReader ("DirectDictionaryContainer2.xml");
			Read_DirectDictionaryContainer2 (r);
		}
		
		[Test]
		public void Read_ContentPropertyContainer ()
		{
			var r = GetReader ("ContentPropertyContainer.xml");
			Read_ContentPropertyContainer (r);
		}

		#region non-common tests
		[Test]
		public void Bug680385 ()
		{
			XamlServices.Load ("Test/XmlFiles/CurrentVersion.xaml");
		}
		#endregion
	}
}
