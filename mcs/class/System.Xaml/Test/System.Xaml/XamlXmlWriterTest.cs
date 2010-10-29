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
using NUnit.Framework;

using CategoryAttribute = NUnit.Framework.CategoryAttribute;

namespace MonoTests.System.Xaml
{
	[TestFixture]
	public class XamlXmlWriterTest
	{
		PropertyInfo str_len = typeof (string).GetProperty ("Length");
		XamlSchemaContext sctx = new XamlSchemaContext (null, null);
		XamlType xt, xt2;
		XamlMember xm;

		public XamlXmlWriterTest ()
		{
			xt = new XamlType (typeof (string), sctx);
			xt2 = new XamlType (typeof (List<int>), sctx);
			xm = new XamlMember (str_len, sctx);
		}

		public class Foo : List<int>
		{
			public List<string> Bar { get; set; }
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SchemaContextNull ()
		{
			new XamlXmlWriter (new MemoryStream (), null);
		}

		[Test]
		public void SettingsNull ()
		{
			// allowed.
			var w = new XamlXmlWriter (new MemoryStream (), sctx, null);
			Assert.AreEqual (sctx, w.SchemaContext, "#1");
			Assert.IsNotNull (w.Settings, "#2");
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void InitWriteEndMember ()
		{
			new XamlXmlWriter (new MemoryStream (), sctx, null).WriteEndMember ();
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void InitWriteEndObject ()
		{
			new XamlXmlWriter (new MemoryStream (), sctx, null).WriteEndObject ();
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void InitWriteGetObject ()
		{
			new XamlXmlWriter (new MemoryStream (), sctx, null).WriteGetObject ();
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void InitWriteValue ()
		{
			new XamlXmlWriter (new StringWriter (), sctx, null).WriteValue ("foo");
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void InitWriteStartMember ()
		{
			new XamlXmlWriter (new StringWriter (), sctx, null).WriteStartMember (new XamlMember (str_len, sctx));
		}

		[Test]
		public void InitWriteNamespace ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteNamespace (new NamespaceDeclaration ("urn:foo", "x")); // ignored.
			xw.Close ();
			Assert.AreEqual ("", sw.ToString (), "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WriteNamespaceNull ()
		{
			new XamlXmlWriter (new StringWriter (), sctx, null).WriteNamespace (null);
		}

		[Test]
		public void InitWriteStartObject ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><Int32 xmlns='http://schemas.microsoft.com/winfx/2006/xaml' />";
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (new XamlType (typeof (int), sctx));
			xw.Close ();
			Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void GetObjectAfterStartObject ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteGetObject ();
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void WriteStartObjectAfterTopLevel ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteEndObject ();
			// writing another root is not allowed.
			xw.WriteStartObject (xt);
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void WriteEndObjectExcess ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteEndObject ();
			xw.WriteEndObject ();
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void StartObjectWriteEndMember ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteEndMember ();
		}

		[Test]
		public void WriteObjectAndMember ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><String Length='foo' xmlns='http://schemas.microsoft.com/winfx/2006/xaml' />";
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xm);
			xw.WriteValue ("foo");
			xw.WriteEndMember ();
			xw.Close ();
			Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void StartMemberWriteEndMember ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xm);
			xw.WriteEndMember (); // wow, really?
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void StartMemberWriteStartMember ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xm);
			xw.WriteStartMember (xm);
		}

		[Test]
		public void WriteObjectInsideMember ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><String xmlns='http://schemas.microsoft.com/winfx/2006/xaml'><String.Length><String /></String.Length></String>";
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xm);
			xw.WriteStartObject (xt);
			xw.WriteEndObject ();
			xw.WriteEndMember ();
			xw.Close ();
			Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		[Test]
		public void ValueAfterObject ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><String xmlns='http://schemas.microsoft.com/winfx/2006/xaml'><String.Length><String />foo</String.Length></String>";
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xm);
			xw.WriteStartObject (xt);
			xw.WriteEndObject ();
			// allowed.
			xw.WriteValue ("foo");
			xw.WriteEndMember ();
			xw.Close ();
			Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		[Test]
		public void ValueAfterObject2 ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><String xmlns='http://schemas.microsoft.com/winfx/2006/xaml'><String.Length>foo<String />foo</String.Length></String>";
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xm);
			xw.WriteValue ("foo");
			xw.WriteStartObject (xt);
			xw.WriteEndObject ();
			// allowed.
			xw.WriteValue ("foo");
			xw.WriteEndMember ();
			xw.Close ();
			Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		[Test]
		public void ValueAfterObject3 ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><String xmlns='http://schemas.microsoft.com/winfx/2006/xaml'><String.Length><String />foo<String />foo</String.Length></String>";
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xm);
			xw.WriteStartObject (xt);
			xw.WriteEndObject ();
			xw.WriteValue ("foo");
			xw.WriteStartObject (xt);
			xw.WriteEndObject ();
			xw.WriteValue ("foo");
			xw.WriteEndMember ();
			xw.Close ();
			Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void WriteValueTypeNonString ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xm);
			xw.WriteValue (5); // even the type matches the member type, writing non-string value is rejected.
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void WriteValueAfterValue ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteValue ("foo");
			xw.WriteValue ("bar");
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void WriteValueAfterNullValue ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteValue (null);
			xw.WriteValue ("bar");
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		[Category ("NotWorking")] // it raises ArgumentException earlier, which should not matter.
		public void WriteValueList ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (new XamlType (typeof (List<string>), sctx));
			xw.WriteStartMember (XamlLanguage.Items);
			xw.WriteValue ("foo");
			xw.WriteValue ("bar");
		}

		[ExpectedException (typeof (XamlXmlWriterException))]
		public void StartMemberWriteEndObject ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xm);
			xw.WriteEndObject ();
		}

		[Test]
		public void WriteNamespace ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><x:String xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' xmlns:y='urn:foo' />";
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteNamespace (new NamespaceDeclaration (XamlLanguage.Xaml2006Namespace, "x"));
			xw.WriteNamespace (new NamespaceDeclaration ("urn:foo", "y"));
			xw.WriteStartObject (xt);
			xw.WriteEndObject ();
			xw.Close ();
			Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void StartObjectStartObject ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartObject (xt);
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void StartObjectValue ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteValue ("foo");
		}

		[Test]
		public void ObjectThenNamespaceThenObjectThenObject ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><String xmlns='http://schemas.microsoft.com/winfx/2006/xaml'><String.Length><String /><String /></String.Length></String>";
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt); // <String>
			xw.WriteStartMember (xm); // <String.Length>
			xw.WriteStartObject (xt); // <String />
			xw.WriteEndObject ();
			xw.WriteStartObject (xt); // <String />
			xw.WriteEndObject ();
			xw.Close ();
			Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		// This doesn't result in XamlXmlWriterException. Instead,
		// IOE is thrown. WriteValueAfterNamespace() too.
		// It is probably because namespaces are verified independently
		// from state transition (and borks when the next write is not
		// appropriate).
		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void EndObjectAfterNamespace ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteNamespace (new NamespaceDeclaration ("urn:foo", "y"));
			xw.WriteEndObject ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))] // ... shouldn't it be XamlXmlWriterException?
		public void WriteValueAfterNamespace ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (XamlLanguage.Initialization);
			xw.WriteNamespace (new NamespaceDeclaration ("urn:foo", "y"));
			xw.WriteValue ("foo");
		}

		[Test]
		public void ValueThenStartObject ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><String xmlns='http://schemas.microsoft.com/winfx/2006/xaml'><String.Length>foo<String /></String.Length></String>";
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xm);
			xw.WriteValue ("foo");
			xw.WriteStartObject (xt);
			xw.Close ();
			Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		[Test]
		public void ValueThenNamespace ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xm);
			xw.WriteValue ("foo");
			xw.WriteNamespace (new NamespaceDeclaration ("y", "urn:foo")); // this does not raise an error (since it might start another object)
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))] // strange, this does *not* result in IOE...
		public void ValueThenNamespaceThenEndMember ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xm);
			xw.WriteValue ("foo");
			xw.WriteNamespace (new NamespaceDeclaration ("y", "urn:foo"));
			xw.WriteEndMember ();
		}

		[Test]
		public void StartMemberAfterNamespace ()
		{
			// This test shows:
			// 1) StartMember after NamespaceDeclaration is valid
			// 2) Member is written as an element (not attribute)
			//    if there is a NamespaceDeclaration in the middle.
			string xml = @"<?xml version='1.0' encoding='utf-16'?><String xmlns='http://schemas.microsoft.com/winfx/2006/xaml'><String.Length xmlns:y='urn:foo'>foo</String.Length></String>";
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteNamespace (new NamespaceDeclaration ("urn:foo", "y"));
			xw.WriteStartMember (xm);
			xw.WriteValue ("foo");
			xw.Close ();
			Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void EndMemberThenStartObject ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xm);
			xw.WriteValue ("foo");
			xw.WriteEndMember ();
			xw.WriteStartObject (xt);
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetObjectOnNonCollection ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xm);
			xw.WriteGetObject ();
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void GetObjectOnNonCollection2 ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (new XamlMember (typeof (string).GetProperty ("Length"), sctx)); // Length is of type int, which is not a collection
			xw.WriteGetObject ();
		}

		[Test]
		public void GetObjectOnCollection ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><List xmlns='clr-namespace:System.Collections.Generic;assembly=mscorlib'><x:TypeArguments xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>x:Int32</x:TypeArguments><List.Bar /></List>";
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt2);
			xw.WriteStartMember (new XamlMember (typeof (Foo).GetProperty ("Bar"), sctx));
			xw.WriteGetObject ();
			xw.Close ();
			// FIXME: enable it once we got generic type output fixed.
			//Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void ValueAfterGetObject ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt2);
			xw.WriteStartMember (new XamlMember (typeof (Foo).GetProperty ("Bar"), sctx));
			xw.WriteGetObject ();
			xw.WriteValue ("foo");
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void StartObjectAfterGetObject ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt2);
			xw.WriteStartMember (new XamlMember (typeof (Foo).GetProperty ("Bar"), sctx));
			xw.WriteGetObject ();
			xw.WriteStartObject (xt);
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		public void EndMemberAfterGetObject ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt2);
			xw.WriteStartMember (new XamlMember (typeof (Foo).GetProperty ("Bar"), sctx));
			xw.WriteGetObject ();
			xw.WriteEndMember (); // ...!?
		}

		[Test]
		public void StartMemberAfterGetObject ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><List xmlns='clr-namespace:System.Collections.Generic;assembly=mscorlib'><x:TypeArguments xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>x:Int32</x:TypeArguments><List.Bar><List.Length /></List.Bar></List>";
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt2); // <List
			xw.WriteStartMember (new XamlMember (typeof (Foo).GetProperty ("Bar"), sctx)); // <List.Bar>
			xw.WriteGetObject ();
			xw.WriteStartMember (xm); // <List.Length /> . Note that the corresponding member is String.Length(!)
			xw.Close ();
			// FIXME: enable it once we got generic type output fixed.
			//Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		[Test]
		public void EndObjectAfterGetObject ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			xw.WriteStartObject (xt2);
			xw.WriteStartMember (new XamlMember (typeof (Foo).GetProperty ("Bar"), sctx));
			xw.WriteGetObject ();
			xw.WriteEndObject ();
		}

		[Test]
		public void WriteNode ()
		{
			string xml = @"<?xml version='1.0' encoding='utf-16'?><x:String xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>foo</x:String>";
			var r = new XamlObjectReader ("foo", sctx);
			var sw = new StringWriter ();
			var w = new XamlXmlWriter (sw, sctx, null);
			while (r.Read ())
				w.WriteNode (r);
			w.Close ();
			Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		[Test]
		public void WriteNode2 ()
		{
			var r = new XamlObjectReader ("foo", sctx);
			var w = new XamlObjectWriter (sctx, null);
			while (r.Read ())
				w.WriteNode (r);
			w.Close ();
			Assert.AreEqual ("foo", w.Result, "#1");
		}

		[Test]
		public void ConstructorArguments ()
		{
			string xml = String.Format (@"<?xml version='1.0' encoding='utf-16'?><ArgumentAttributed xmlns='clr-namespace:MonoTests.System.Xaml;assembly={0}' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'><x:Arguments><x:String>xxx</x:String><x:String>yyy</x:String></x:Arguments></ArgumentAttributed>",  GetType ().Assembly.GetName ().Name);
			Assert.IsFalse (sctx.FullyQualifyAssemblyNamesInClrNamespaces, "premise0");
			var r = new XamlObjectReader (new ArgumentAttributed ("xxx", "yyy"), sctx);
			var sw = new StringWriter ();
			var w = new XamlXmlWriter (sw, sctx, null);
			XamlServices.Transform (r, w);
			Assert.AreEqual (xml, sw.ToString ().Replace ('"', '\''), "#1");
		}

		[Test]
		public void WriteValueAsString ()
		{
			var sw = new StringWriter ();
			var xw = new XamlXmlWriter (sw, sctx, null);
			var xt = sctx.GetXamlType (typeof (TestXmlWriterClass1));
			xw.WriteStartObject (xt);
			xw.WriteStartMember (xt.GetMember ("Foo"));
			xw.WriteValue ("50");
			xw.Close ();
			string xml = String.Format (@"<?xml version='1.0' encoding='utf-16'?><TestXmlWriterClass1 xmlns='clr-namespace:MonoTests.System.Xaml;assembly={0}' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'></TestXmlWriterClass1>",  GetType ().Assembly.GetName ().Name);
		}

		string ReadXml (string name)
		{
			return File.ReadAllText ("Test/XmlFiles/" + name).Trim ().Replace ("\r\n", "\n").Replace ("\n", Environment.NewLine);
		}

		[Test]
		public void Write_String ()
		{
			Assert.AreEqual (ReadXml ("String.xml"), XamlServices.Save ("foo"), "#1");
		}

		[Test]
		public void Write_Int32 ()
		{
			Assert.AreEqual (ReadXml ("Int32.xml"), XamlServices.Save (5), "#1");
		}

		[Test]
		public void Write_DateTime ()
		{
			Assert.AreEqual (ReadXml ("DateTime.xml"), XamlServices.Save (new DateTime (2010, 4, 14)), "#1");
		}

		[Test]
		public void Write_TimeSpan ()
		{
			Assert.AreEqual (ReadXml ("TimeSpan.xml"), XamlServices.Save (TimeSpan.FromMinutes (7)), "#1");
		}

		[Test]
		public void Write_Uri ()
		{
			Assert.AreEqual (ReadXml ("Uri.xml"), XamlServices.Save (new Uri ("urn:foo")), "#1");
		}

		[Test]
		public void Write_Null ()
		{
			Assert.AreEqual (ReadXml ("NullExtension.xml"), XamlServices.Save (null), "#1");
		}

		[Test]
		public void Write_NullExtension ()
		{
			Assert.AreEqual (ReadXml ("NullExtension.xml"), XamlServices.Save (new NullExtension ()), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_Type ()
		{
			Assert.AreEqual (ReadXml ("Type.xml").Trim (), XamlServices.Save (typeof (int)), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_Type2 ()
		{
			Assert.AreEqual (ReadXml ("Type2.xml").Trim (), XamlServices.Save (typeof (TestClass1)), "#1");
		}

		[Test]
		public void Write_Guid ()
		{
			Assert.AreEqual (ReadXml ("Guid.xml").Trim (), XamlServices.Save (Guid.Parse ("9c3345ec-8922-4662-8e8d-a4e41f47cf09")), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_StaticExtension ()
		{
			Assert.AreEqual (ReadXml ("StaticExtension.xml").Trim (), XamlServices.Save (new StaticExtension ("FooBar")), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_StaticExtension2 ()
		{
			Assert.AreEqual (ReadXml ("StaticExtension.xml").Trim (), XamlServices.Save (new StaticExtension () { Member = "FooBar"}), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_Reference ()
		{
			Assert.AreEqual (ReadXml ("Reference.xml").Trim (), XamlServices.Save (new Reference ("FooBar")), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_ArrayInt32 ()
		{
			Assert.AreEqual (ReadXml ("Array_Int32.xml").Trim (), XamlServices.Save (new int [] {4, -5, 0, 255, int.MaxValue}), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_ListInt32 ()
		{
			Assert.AreEqual (ReadXml ("List_Int32.xml").Trim (), XamlServices.Save (new int [] {4, -5, 0, 255, int.MaxValue}.ToList ()), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_ListInt32_2 ()
		{
			var obj = new List<int> (new int [0]);
			Assert.AreEqual (ReadXml ("List_Int32_2.xml").Trim (), XamlServices.Save (obj), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_DictionaryInt32String ()
		{
			var dic = new Dictionary<int,string> ();
			dic.Add (0, "foo");
			dic.Add (5, "bar");
			dic.Add (-2, "baz");
			Assert.AreEqual (ReadXml ("Dictionary_Int32_String.xml").Trim (), XamlServices.Save (dic), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_DictionaryStringType ()
		{
			var dic = new Dictionary<string,Type> ();
			dic.Add ("t1", typeof (int));
			dic.Add ("t2", typeof (int []));
			dic.Add ("t3", typeof (int?));
			dic.Add ("t4", typeof (List<int>));
			dic.Add ("t5", typeof (Dictionary<int,DateTime>));
			dic.Add ("t6", typeof (List<KeyValuePair<int,DateTime>>));
			Assert.AreEqual (ReadXml ("Dictionary_String_Type.xml").Trim (), XamlServices.Save (dic), "#1");
		}

		[Test]
		[ExpectedException (typeof (XamlXmlWriterException))]
		[Category ("NotWorking")]
		public void Write_PositionalParameters1 ()
		{
			// PositionalParameters can only be written when the 
			// instance is NOT the root object.
			//
			// A single positional parameter can be written as an 
			// attribute, but there are two in PositionalParameters1.
			//
			// A default constructor could be used to not use
			// PositionalParameters, but there isn't in this type.
			var obj = new PositionalParametersClass1 ("foo", 5);
			XamlServices.Save (obj);
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_PositionalParameters1Wrapper ()
		{
			// Unlike the above case, this has the wrapper object and hence PositionalParametersClass1 can be written as an attribute (markup extension)
			var obj = new PositionalParametersWrapper ("foo", 5);
			Assert.AreEqual (ReadXml ("PositionalParametersWrapper.xml").Trim (), XamlServices.Save (obj), "#1");
		}
		
		[Test]
		[Category ("NotWorking")]
		public void Write_ArgumentAttributed ()
		{
			var obj = new ArgumentAttributed ("foo", "bar");
			Assert.AreEqual (ReadXml ("ArgumentAttributed.xml").Trim (), XamlServices.Save (obj), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_ArrayExtension2 ()
		{
			var obj = new ArrayExtension (typeof (int));
			Assert.AreEqual (ReadXml ("ArrayExtension2.xml").Trim (), XamlServices.Save (obj), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_ArrayList ()
		{
			var obj = new ArrayList (new int [] {5, -3, 0});
			Assert.AreEqual (ReadXml ("ArrayList.xml").Trim (), XamlServices.Save (obj), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void ComplexPositionalParameterWrapper ()
		{
			var obj = new ComplexPositionalParameterWrapper () { Param = new ComplexPositionalParameterClass (new ComplexPositionalParameterValue () { Foo = "foo" })};
			Assert.AreEqual (ReadXml ("ComplexPositionalParameterWrapper.xml").Trim (), XamlServices.Save (obj), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_ListWrapper ()
		{
			var obj = new ListWrapper (new List<int> (new int [] {5, -3, 0}));
			Assert.AreEqual (ReadXml ("ListWrapper.xml").Trim (), XamlServices.Save (obj), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_ListWrapper2 ()
		{
			var obj = new ListWrapper2 (new List<int> (new int [] {5, -3, 0}));
			Assert.AreEqual (ReadXml ("ListWrapper2.xml").Trim (), XamlServices.Save (obj), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_MyArrayExtension ()
		{
			var obj = new MyArrayExtension (new int [] {5, -3, 0});
			Assert.AreEqual (ReadXml ("MyArrayExtension.xml").Trim (), XamlServices.Save (obj), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_MyExtension ()
		{
			var obj = new MyExtension () { Foo = typeof (int), Bar = "v2", Baz = "v7"};
			Assert.AreEqual (ReadXml ("MyExtension.xml").Trim (), XamlServices.Save (obj), "#1");
		}

		[Test]
		public void Write_MyExtension2 ()
		{
			var obj = new MyExtension2 () { Foo = typeof (int), Bar = "v2"};
			Assert.AreEqual (ReadXml ("MyExtension2.xml").Trim (), XamlServices.Save (obj), "#1");
		}

		[Test]
		public void Write_MyExtension3 ()
		{
			var obj = new MyExtension3 () { Foo = typeof (int), Bar = "v2"};
			Assert.AreEqual (ReadXml ("MyExtension3.xml").Trim (), XamlServices.Save (obj), "#1");
		}

		[Test]
		public void Write_MyExtension4 ()
		{
			var obj = new MyExtension4 () { Foo = typeof (int), Bar = "v2"};
			Assert.AreEqual (ReadXml ("MyExtension4.xml").Trim (), XamlServices.Save (obj), "#1");
		}

		[Test]
		[Category ("NotWorking")]
		public void Write_MyExtension6 ()
		{
			var obj = new MyExtension6 ("foo");
			Assert.AreEqual (ReadXml ("MyExtension6.xml").Trim (), XamlServices.Save (obj), "#1");
		}
	}

	public class TestXmlWriterClass1
	{
		public int Foo { get; set; }
	}
}
