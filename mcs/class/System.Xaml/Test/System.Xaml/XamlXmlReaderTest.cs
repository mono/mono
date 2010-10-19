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
	public class XamlXmlReaderTest
	{

		// read test

		XamlReader GetReader (string filename)
		{
			return new XamlXmlReader (XmlReader.Create (Path.Combine ("Test/XmlFiles", filename), new XmlReaderSettings () { CloseInput =true }));
		}

		void ReadTest (string filename)
		{
			var r = GetReader (filename);
			while (!r.IsEof)
				r.Read ();
		}

		T LoadTest<T> (string filename)
		{
			Type type = typeof (T);
			var obj = XamlServices.Load (GetReader (filename));
			Assert.AreEqual (type, obj.GetType (), "type");
			return (T) obj;
		}

		[Test]
		public void Read_String ()
		{
			ReadTest ("String.xml");
			var ret = LoadTest<string> ("String.xml");
			Assert.AreEqual ("foo", ret, "ret");
		}

		[Test]
		public void Read_Int32 ()
		{
			ReadTest ("Int32.xml");
			var ret = LoadTest<int> ("Int32.xml");
			Assert.AreEqual (5, ret, "ret");
		}

		[Test]
		public void Read_DateTime ()
		{
			ReadTest ("DateTime.xml");
			var ret = LoadTest<DateTime> ("DateTime.xml");
			Assert.AreEqual (new DateTime (2010, 4, 14), ret, "ret");
		}

		[Test]
		public void Read_TimeSpan ()
		{
			ReadTest ("TimeSpan.xml");
			var ret = LoadTest<TimeSpan> ("TimeSpan.xml");
			Assert.AreEqual (TimeSpan.FromMinutes (7), ret, "ret");
		}

		[Test]
		public void Read_Null ()
		{
			ReadTest ("NullExtension.xml");
			Assert.IsNull (XamlServices.Load (GetReader ("NullExtension.xml")));
		}

		[Test]
		public void Read_Reference ()
		{
			ReadTest ("Reference.xml");
			var ret = XamlServices.Load (GetReader ("Reference.xml"));
			Assert.IsNotNull (ret, "#1"); // the returned value is however not a Reference (in .NET 4.0 it is MS.Internal.Xaml.Context.NameFixupToken).
		}

		[Test]
		public void Read_ArrayInt32 ()
		{
			ReadTest ("Array_Int32.xml");
			var ret = LoadTest<int[]> ("Array_Int32.xml");
			Assert.AreEqual (5, ret.Length, "#1");
			Assert.AreEqual (2147483647, ret [4], "#2");
		}

		[Test]
		public void Read_ListInt32 ()
		{
			ReadTest ("List_Int32.xml");
			var ret = LoadTest<List<int>> ("List_Int32.xml");
			Assert.AreEqual (5, ret.Count, "#1");
			Assert.AreEqual (2147483647, ret [4], "#2");
		}

		[Test]
		[Category ("NotWorking")]
		public void Read_DictionaryInt32String ()
		{
			ReadTest ("Dictionary_Int32_String.xml");
			//LoadTest<Dictionary<int,string>> ("Dictionary_Int32_String.xml");
		}

		[Test]
		[Category ("NotWorking")]
		public void Read_DictionaryStringType ()
		{
			ReadTest ("Dictionary_String_Type.xml");
			//LoadTest<Dictionary<string,Type>> ("Dictionary_String_Type.xml");
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
			var ret = LoadTest<Guid> ("Guid.xml");
			Assert.AreEqual (Guid.Parse ("9c3345ec-8922-4662-8e8d-a4e41f47cf09"), ret, "ret");
		}

		[Test]
		public void Read_GuidFactoryMethod ()
		{
			ReadTest ("GuidFactoryMethod.xml");
			//var ret = LoadTest<Guid> ("GuidFactoryMethod.xml");
			//Assert.AreEqual (Guid.Parse ("9c3345ec-8922-4662-8e8d-a4e41f47cf09"), ret, "ret");
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

			Assert.IsTrue (r.Read (), "sbase#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sbase#2");
			Assert.AreEqual (XamlLanguage.Base, r.Member, "sbase#3");

			Assert.IsTrue (r.Read (), "vbase#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vbase#2");
			Assert.IsTrue (r.Value is string, "vbase#3");

			Assert.IsTrue (r.Read (), "ebase#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "ebase#2");

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

			Assert.IsTrue (r.Read (), "sbase#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sbase#2");
			Assert.AreEqual (XamlLanguage.Base, r.Member, "sbase#3");

			Assert.IsTrue (r.Read (), "vbase#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vbase#2");
			Assert.IsTrue (r.Value is string, "vbase#3");

			Assert.IsTrue (r.Read (), "ebase#21");

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
		public void ReadTypeDetails ()
		{
			var r = GetReader ("Type.xml");

			Assert.IsTrue (r.Read (), "ns#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns#2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "ns#3");

			Assert.IsTrue (r.Read (), "so#1");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "so#2");
			Assert.AreEqual (XamlLanguage.Type, r.Type, "so#3");

			Assert.IsTrue (r.Read (), "sbase#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sbase#2");
			Assert.AreEqual (XamlLanguage.Base, r.Member, "sbase#3");

			Assert.IsTrue (r.Read (), "vbase#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vbase#2");
			Assert.IsTrue (r.Value is string, "vbase#3");

			Assert.IsTrue (r.Read (), "ebase#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "ebase#2");

			Assert.IsTrue (r.Read (), "sinit#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sinit#2");
			// FIXME: They are identical on .NET.
			// They aren't on mono, every GetMember() returns different object
			// (it is the same as .NET; see XamlMemberTest.EqualsTest.) 
			// and XamlMember is almost non-comparable unless they
			// are identical, so we fail here.
			Assert.AreEqual (XamlLanguage.Type.GetMember ("Type").ToString (), r.Member.ToString (), "sinit#3");
			//Assert.AreEqual (XamlLanguage.Type.GetMember ("Type"), r.Member, "sinit#3-2");
			//Assert.IsTrue (Object.ReferenceEquals (XamlLanguage.Type.GetMember ("Type"), r.Member), "sinit#3-3");

			Assert.IsTrue (r.Read (), "vinit#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vinit#2");
			Assert.AreEqual ("x:Int32", r.Value, "vinit#3"); // string

			Assert.IsTrue (r.Read (), "einit#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "einit#2");

			Assert.IsTrue (r.Read (), "eo#1");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "eo#2");

			Assert.IsFalse (r.Read (), "end");
		}

		[Test]
		public void ReadListInt32Details ()
		{
			var r = GetReader ("List_Int32.xml");

			Assert.IsTrue (r.Read (), "ns#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns#2");
			Assert.AreEqual ("clr-namespace:System.Collections.Generic;assembly=mscorlib", r.Namespace.Namespace, "ns#3");
			Assert.AreEqual (String.Empty, r.Namespace.Prefix, "ns#4");

			Assert.IsTrue (r.Read (), "ns2#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns2#2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "ns2#3");
			Assert.AreEqual ("x", r.Namespace.Prefix, "ns2#4");

			Assert.IsTrue (r.Read (), "so#1");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "so#2");
			var xt = new XamlType (typeof (List<int>), r.SchemaContext);
			Assert.AreEqual (xt, r.Type, "so#3");

			Assert.IsTrue (r.Read (), "sbase#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sbase#2");
			Assert.AreEqual (XamlLanguage.Base, r.Member, "sbase#3");

			Assert.IsTrue (r.Read (), "vbase#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vbase#2");
			Assert.IsTrue (r.Value is string, "vbase#3");

			Assert.IsTrue (r.Read (), "ebase#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "ebase#2");

			Assert.IsTrue (r.Read (), "scap#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "scap#2");
			Assert.AreEqual (xt.GetMember ("Capacity"), r.Member, "scap#3");

			Assert.IsTrue (r.Read (), "vcap#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vcap#2");
			Assert.AreEqual ("5", r.Value, "vcap#3"); // string

			Assert.IsTrue (r.Read (), "ecap#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "ecap#2");

			Assert.IsTrue (r.Read (), "sItems#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sItems#2");
			Assert.AreEqual (XamlLanguage.Items, r.Member, "sItems#3");

			int [] values = {4, -5, 0, 255, int.MaxValue};
			var ci = new CultureInfo ("en-US");

			for (int i = 0; i < 5; i++) {
				Assert.IsTrue (r.Read (), "soItem#1." + i);
				Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "soItem#2." + i);
				Assert.AreEqual (XamlLanguage.Int32, r.Type, "soItem#3." + i);

				Assert.IsTrue (r.Read (), "sItem#1." + i);
				Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sItem#2." + i);
				Assert.AreEqual (XamlLanguage.Initialization, r.Member, "sItem#3." + i);

				Assert.IsTrue (r.Read (), "vItem#1." + i);
				Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vItem#2." + i);
				Assert.AreEqual (values [i].ToString (ci), r.Value, "vItem#3." + i);

				Assert.IsTrue (r.Read (), "eItem#1." + i);
				Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "eItem#2." + i);

				Assert.IsTrue (r.Read (), "eoItem#1");
				Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "eoItem#2");
			}

			Assert.IsTrue (r.Read (), "eItems#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "eItems#2");

			Assert.IsTrue (r.Read (), "eo#1");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "eo#2");

			Assert.IsFalse (r.Read (), "end");
		}

		[Test]
		public void ReadStringDetails ()
		{
			var r = GetReader ("String.xml");

			Assert.IsTrue (r.Read (), "ns#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns#2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "ns#3");

			Assert.IsTrue (r.Read (), "so#1");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "so#2");
			Assert.AreEqual (XamlLanguage.String, r.Type, "so#3");

			Assert.IsTrue (r.Read (), "sbase#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sbase#2");
			Assert.AreEqual (XamlLanguage.Base, r.Member, "sbase#3");

			Assert.IsTrue (r.Read (), "vbase#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vbase#2");
			Assert.IsTrue (r.Value is string, "vbase#3");

			Assert.IsTrue (r.Read (), "ebase#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "ebase#2");

			Assert.IsTrue (r.Read (), "sinit#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sinit#2");
			Assert.AreEqual (XamlLanguage.Initialization, r.Member, "sinit#3");

			Assert.IsTrue (r.Read (), "vinit#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vinit#2");
			Assert.AreEqual ("foo", r.Value, "vinit#3"); // string

			Assert.IsTrue (r.Read (), "einit#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "einit#2");

			Assert.IsTrue (r.Read (), "eo#1");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "eo#2");

			Assert.IsFalse (r.Read (), "end");
		}

		[Test]
		public void ReadArrayInt32Details ()
		{
			var r = GetReader ("Array_Int32.xml");

			Assert.IsTrue (r.Read (), "ns#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns#2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "ns#3");
			Assert.AreEqual ("x", r.Namespace.Prefix, "ns#4");

			Assert.IsTrue (r.Read (), "soa#1");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "soa#2");
			var xt = new XamlType (typeof (ArrayExtension), r.SchemaContext);
			Assert.AreEqual (xt, r.Type, "soa#3");

			// base
			Assert.IsTrue (r.Read (), "sbase#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sbase#2");
			Assert.AreEqual (XamlLanguage.Base, r.Member, "sbase#3");

			Assert.IsTrue (r.Read (), "vbase#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vbase#2");
			Assert.IsTrue (r.Value is string, "vbase#3");

			Assert.IsTrue (r.Read (), "ebase#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "ebase#2");

			// Type
			Assert.IsTrue (r.Read (), "stype#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "stype#2");
			Assert.AreEqual (xt.GetMember ("Type"), r.Member, "stype#3");

			// Type
			Assert.IsTrue (r.Read (), "vtype#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "type#2");
			Assert.AreEqual ("x:Int32", r.Value, "vtype#3"); // string value

			Assert.IsTrue (r.Read (), "etype#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "ecap#2");

			// Items
			Assert.IsTrue (r.Read (), "sItemsA#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sItemsA#2");
			Assert.AreEqual (xt.GetMember ("Items"), r.Member, "sItemsA#3");

			// (GetObject)
			Assert.IsTrue (r.Read (), "go#1");
			Assert.AreEqual (XamlNodeType.GetObject, r.NodeType, "go#2");
			Assert.IsNull (r.Type, "go#3");
			Assert.IsNull (r.Member, "go#4");
			Assert.IsNull (r.Value, "go#5");

			Assert.IsTrue (r.Read (), "sItems#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sItems#2");
			Assert.AreEqual (XamlLanguage.Items, r.Member, "sItems#3");

			int [] values = {4, -5, 0, 255, int.MaxValue};
			var ci = new CultureInfo ("en-US");

			for (int i = 0; i < 5; i++) {
				Assert.IsTrue (r.Read (), "soItem#1." + i);
				Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "soItem#2." + i);
				Assert.AreEqual (XamlLanguage.Int32, r.Type, "soItem#3." + i);

				Assert.IsTrue (r.Read (), "sItem#1." + i);
				Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sItem#2." + i);
				Assert.AreEqual (XamlLanguage.Initialization, r.Member, "sItem#3." + i);

				Assert.IsTrue (r.Read (), "vItem#1." + i);
				Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vItem#2." + i);
				Assert.AreEqual (values [i].ToString (ci), r.Value, "vItem#3." + i);

				Assert.IsTrue (r.Read (), "eItem#1." + i);
				Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "eItem#2." + i);

				Assert.IsTrue (r.Read (), "eoItem#1");
				Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "eoItem#2");
			}

			Assert.IsTrue (r.Read (), "eItems#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "eItems#2");

			// end of GetObject
			Assert.IsTrue (r.Read (), "eog#1");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "eo#2");

			// end of ArrayExtension.Items
			Assert.IsTrue (r.Read (), "eItemsA#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "eItemsA#2");

			// end of ArrayExtension
			Assert.IsTrue (r.Read (), "eoa#1");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "eoa#2");

			Assert.IsFalse (r.Read (), "end");
		}

		[Test]
		public void ReadNullDetails ()
		{
			var r = GetReader ("NullExtension.xml");

			Assert.IsTrue (r.Read (), "ns#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns#2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "ns#3");
			Assert.AreEqual ("x", r.Namespace.Prefix, "ns#4");

			Assert.IsTrue (r.Read (), "so#1");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "so#2");
			var xt = new XamlType (typeof (NullExtension), r.SchemaContext);
			Assert.AreEqual (xt, r.Type, "so#3");

			// base
			Assert.IsTrue (r.Read (), "sbase#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sbase#2");
			Assert.AreEqual (XamlLanguage.Base, r.Member, "sbase#3");

			Assert.IsTrue (r.Read (), "vbase#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vbase#2");
			Assert.IsTrue (r.Value is string, "vbase#3");

			Assert.IsTrue (r.Read (), "ebase#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "ebase#2");

			// end of NullExtension
			Assert.IsTrue (r.Read (), "eo#1");
			Assert.AreEqual (XamlNodeType.EndObject, r.NodeType, "eo#2");

			Assert.IsFalse (r.Read (), "end");
		}

		[Test]
		public void ReadReferenceDetails ()
		{
			var r = GetReader ("Reference.xml");

			Assert.IsTrue (r.Read (), "ns#1");
			Assert.AreEqual (XamlNodeType.NamespaceDeclaration, r.NodeType, "ns#2");
			Assert.AreEqual (XamlLanguage.Xaml2006Namespace, r.Namespace.Namespace, "ns#3");
			Assert.AreEqual ("x", r.Namespace.Prefix, "ns#4");

			Assert.IsTrue (r.Read (), "so#1");
			Assert.AreEqual (XamlNodeType.StartObject, r.NodeType, "so#2");
			var xt = new XamlType (typeof (Reference), r.SchemaContext);
			Assert.AreEqual (xt, r.Type, "so#3");

			// base
			Assert.IsTrue (r.Read (), "sbase#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sbase#2");
			Assert.AreEqual (XamlLanguage.Base, r.Member, "sbase#3");

			Assert.IsTrue (r.Read (), "vbase#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vbase#2");
			Assert.IsTrue (r.Value is string, "vbase#3");

			Assert.IsTrue (r.Read (), "ebase#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "ebase#2");

			// value
			Assert.IsTrue (r.Read (), "sinit#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sinit#2");
			Assert.AreEqual (xt.GetMember ("Name"), r.Member, "sinit#3");

			Assert.IsTrue (r.Read (), "vinit#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vinit#2");
			Assert.AreEqual ("FooBar", r.Value, "vinit#3"); // string

			Assert.IsTrue (r.Read (), "einit#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "einit#2");

			// end of Reference
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

			Assert.IsTrue (r.Read (), "sbase#1");
			Assert.AreEqual (XamlNodeType.StartMember, r.NodeType, "sbase#2");
			Assert.AreEqual (XamlLanguage.Base, r.Member, "sbase#3");

			Assert.IsTrue (r.Read (), "vbase#1");
			Assert.AreEqual (XamlNodeType.Value, r.NodeType, "vbase#2");
			Assert.IsTrue (r.Value is string, "vbase#3");

			Assert.IsTrue (r.Read (), "ebase#1");
			Assert.AreEqual (XamlNodeType.EndMember, r.NodeType, "ebase#2");

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
	}
}
