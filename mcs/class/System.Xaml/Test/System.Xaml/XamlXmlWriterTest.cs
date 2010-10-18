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
	}

	public class TestXmlWriterClass1
	{
		public int Foo { get; set; }
	}
}
