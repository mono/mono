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

		void LoadTest (string filename, Type type)
		{
			var obj = XamlServices.Load (GetReader (filename));
			Assert.AreEqual (type, obj.GetType (), "type");
		}

		[Test]
		public void Read_String ()
		{
			ReadTest ("String.xml");
			//LoadTest ("String.xml", typeof (string));
		}

		[Test]
		public void Read_Int32 ()
		{
			ReadTest ("Int32.xml");
			//LoadTest ("Int32.xml", typeof (int));
		}

		[Test]
		public void Read_DateTime ()
		{
			ReadTest ("DateTime.xml");
			//LoadTest ("DateTime.xml", typeof (DateTime));
		}

		[Test]
		public void Read_TimeSpan ()
		{
			ReadTest ("TimeSpan.xml");
			//LoadTest ("TimeSpan.xml", typeof (TimeSpan));
		}

		[Test]
		public void Read_ArrayInt32 ()
		{
			ReadTest ("Array_Int32.xml");
			//LoadTest ("Array_Int32.xml", typeof (int []));
		}

		[Test]
		public void Read_ListInt32 ()
		{
			ReadTest ("List_Int32.xml");
			//LoadTest ("List_Int32.xml", typeof (List<int>));
		}

		[Test]
		[Category ("NotWorking")]
		public void Read_DictionaryInt32String ()
		{
			ReadTest ("Dictionary_Int32_String.xml");
			//LoadTest ("Dictionary_Int32_String.xml", typeof (Dictionary<int,string>));
		}

		[Test]
		[Category ("NotWorking")]
		public void Read_DictionaryStringType ()
		{
			ReadTest ("Dictionary_String_Type.xml");
			//LoadTest ("Dictionary_String_Type.xml", typeof (Dictionary<string,Type>));
		}

		[Test]
		public void Read_SilverlightApp1 ()
		{
			ReadTest ("SilverlightApp1.xaml");
		}

		[Test]
		public void Read1 ()
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
		public void Read2 ()
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
		public void Read3 ()
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
		[Category ("NotWorking")]
		public void Read4 ()
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
	}
}
