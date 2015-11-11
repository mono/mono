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
	public class XamlObjectWriterTest
	{
		PropertyInfo str_len = typeof (string).GetProperty ("Length");
		XamlSchemaContext sctx = new XamlSchemaContext (null, null);
		XamlType xt, xt2, xt3, xt4;
		XamlMember xm, xm2, xm3;

		public XamlObjectWriterTest ()
		{
			xt = new XamlType (typeof (string), sctx);
			xt2 = new XamlType (typeof (List<int>), sctx);
			xt3 = new XamlType (typeof (TestClass1), sctx);
			xt4 = new XamlType (typeof (Foo), sctx);
			xm = new XamlMember (str_len, sctx);
			xm2 = new XamlMember (typeof (TestClass1).GetProperty ("TestProp1"), sctx);
			xm3 = new XamlMember (typeof (TestClass1).GetProperty ("TestProp2"), sctx);
		}
		
		public class TestClass1
		{
			public TestClass1 ()
			{
				TestProp3 = "foobar";
			}
			public string TestProp1 { get; set; }
			// nested.
			public TestClass1 TestProp2 { get; set; }
			public string TestProp3 { get; set; }
			public int TestProp4 { get; set; }
		}

		public class Foo : List<int>
		{
			public Foo ()
			{
				Bar = new List<string> ();
			}
			public List<string> Bar { get; private set; }
			public List<string> Baz { get; set; }
			public string Ext { get; set; }
		}
		
		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void SchemaContextNull ()
		{
			new XamlObjectWriter (null);
		}

		[Test]
		public void SettingsNull ()
		{
			// allowed.
			var w = new XamlObjectWriter (sctx, null);
			Assert.AreEqual (sctx, w.SchemaContext, "#1");
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void InitWriteEndMember ()
		{
			new XamlObjectWriter (sctx, null).WriteEndMember ();
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void InitWriteEndObject ()
		{
			new XamlObjectWriter (sctx, null).WriteEndObject ();
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void InitWriteGetObject ()
		{
			new XamlObjectWriter (sctx, null).WriteGetObject ();
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void InitWriteValue ()
		{
			new XamlObjectWriter (sctx, null).WriteValue ("foo");
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void InitWriteStartMember ()
		{
			new XamlObjectWriter (sctx, null).WriteStartMember (new XamlMember (str_len, sctx));
		}

		[Test]
		public void InitWriteNamespace ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteNamespace (new NamespaceDeclaration ("urn:foo", "x")); // ignored.
			xw.Close ();
			Assert.IsNull (xw.Result, "#1");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void WriteNamespaceNull ()
		{
			new XamlObjectWriter (sctx, null).WriteNamespace (null);
		}

		[Test]
		public void InitWriteStartObject ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (new XamlType (typeof (int), sctx));
			xw.Close ();
			Assert.AreEqual (0, xw.Result, "#1");
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void GetObjectAfterStartObject ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteGetObject ();
		}

		[Test]
		//[ExpectedException (typeof (XamlObjectWriterException))]
		public void WriteStartObjectAfterTopLevel ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteEndObject ();
			// writing another root is <del>not</del> allowed.
			xw.WriteStartObject (xt3);
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void WriteEndObjectExcess ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteEndObject ();
			xw.WriteEndObject ();
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void StartObjectWriteEndMember ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteEndMember ();
		}

		[Test]
		public void WriteObjectAndMember ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm2);
			xw.WriteValue ("foo");
			xw.WriteEndMember ();
			xw.Close ();
		}

		[Test]
		public void StartMemberWriteEndMember ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm3);
			xw.WriteEndMember (); // unlike XamlXmlWriter, it is not treated as an error...
			xw.Close ();
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void StartMemberWriteStartMember ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm3);
			xw.WriteStartMember (xm3);
		}

		[Test]
		public void WriteObjectInsideMember ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm3);
			xw.WriteStartObject (xt3);
			xw.WriteEndObject ();
			xw.WriteEndMember ();
			xw.Close ();
		}

		[Test]
		[ExpectedException (typeof (XamlDuplicateMemberException))]
		public void ValueAfterObject ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm3);
			xw.WriteStartObject (xt3);
			xw.WriteEndObject ();
			// passes here, but ...
			xw.WriteValue ("foo");
			// rejected here, unlike XamlXmlWriter.
			xw.WriteEndMember ();
		}

		[Test]
		[ExpectedException (typeof (XamlDuplicateMemberException))]
		public void ValueAfterObject2 ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm3);
			xw.WriteStartObject (xt3);
			xw.WriteEndObject ();
			// passes here, but should be rejected later.
			xw.WriteValue ("foo");

			xw.WriteEndMember (); // Though this raises an error.
		}

		[Test]
		[ExpectedException (typeof (XamlDuplicateMemberException))]
		public void DuplicateAssignment ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm3);
			xw.WriteStartObject (xt3);
			xw.WriteEndObject ();
			xw.WriteValue ("foo"); // causes duplicate assignment.
			xw.WriteEndMember ();
		}

		[Test]
		[ExpectedException (typeof (XamlDuplicateMemberException))]
		public void DuplicateAssignment2 ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm3);
			xw.WriteStartObject (xt3);
			xw.WriteEndObject ();
			xw.WriteEndMember ();
			xw.WriteStartMember (xm3);
		}

		[Test]
		//[ExpectedException (typeof (ArgumentException))] // oh? XamlXmlWriter raises this.
		[Category ("NotWorking")] // so, it's not worthy of passing.
		public void WriteValueTypeMismatch ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (XamlLanguage.Initialization);
			xw.WriteValue (new TestClass1 ());
			xw.WriteEndMember ();
			xw.Close ();
			Assert.IsNotNull (xw.Result, "#1");
			Assert.AreEqual (typeof (TestClass1), xw.Result.GetType (), "#2");
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))] // it fails to convert type and set property value.
		public void WriteValueTypeMismatch2 ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm3);
			xw.WriteValue ("foo");
			xw.WriteEndMember ();
		}

		[Test]
		public void WriteValueTypeOK ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (XamlLanguage.Initialization);
			xw.WriteValue ("foo");
			xw.WriteEndMember ();
			xw.Close ();
			Assert.AreEqual ("foo", xw.Result, "#1");
		}

		[Test]
		// This behavior is different from XamlXmlWriter. Compare to XamlXmlWriterTest.WriteValueList().
		[Category ("NotWorking")] // not worthy of passing
		public void WriteValueList ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (new XamlType (typeof (List<string>), sctx));
			xw.WriteStartMember (XamlLanguage.Items);
			xw.WriteValue ("foo");
			xw.WriteValue ("bar");
			xw.WriteEndMember ();
			xw.Close ();
			var l = xw.Result as List<string>;
			Assert.IsNotNull (l, "#1");
			Assert.AreEqual ("foo", l [0], "#2");
			Assert.AreEqual ("bar", l [1], "#3");
		}

		// I believe .NET XamlObjectWriter.Dispose() is hack and should
		// be fixed to exactly determine which of End (member or object)
		// to call that results in this ExpectedException.
		// Surprisingly, PositionalParameters is allowed to be closed
		// without EndMember. So it smells that .NET is hacky.
		// We should disable this test and introduce better code (which
		// is already in XamlWriterInternalBase).
		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		[Ignore ("See the comment in XamlObjectWriterTest.cs")]
		public void CloseWithoutEndMember ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (XamlLanguage.Initialization);
			xw.WriteValue ("foo");
			xw.Close ();
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void WriteValueAfterValue ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteValue ("foo");
			xw.WriteValue ("bar");
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void WriteValueAfterNullValue ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteValue (null);
			xw.WriteValue ("bar");
		}

		[ExpectedException (typeof (XamlObjectWriterException))]
		public void StartMemberWriteEndObject ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm3);
			xw.WriteEndObject ();
		}

		[Test]
		public void WriteNamespace ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteNamespace (new NamespaceDeclaration (XamlLanguage.Xaml2006Namespace, "x"));
			xw.WriteNamespace (new NamespaceDeclaration ("urn:foo", "y"));
			xw.WriteStartObject (xt3);
			xw.WriteEndObject ();
			xw.Close ();
			var ret = xw.Result;
			Assert.IsTrue (ret is TestClass1, "#1");
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void StartObjectStartObject ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartObject (xt3);
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void StartObjectValue ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteValue ("foo");
		}

		[Test]
		[ExpectedException (typeof (XamlDuplicateMemberException))] // duplicate member assignment
		public void ObjectContainsObjectAndObject ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm3);
			xw.WriteStartObject (xt3);
			xw.WriteEndObject ();
			xw.WriteStartObject (xt3);
			xw.WriteEndObject (); // the exception happens *here*
			// FIXME: so, WriteEndMember() should not be required, but we fail here. Practically this difference should not matter.
			xw.WriteEndMember (); // of xm3
		}

		[Test]
		[ExpectedException (typeof (XamlDuplicateMemberException))] // duplicate member assignment
		public void ObjectContainsObjectAndValue ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm3);
			xw.WriteStartObject (xt3);
			xw.WriteEndObject ();
			xw.WriteValue ("foo"); // but this is allowed ...

			xw.WriteEndMember (); // Though this raises an error.
		}

		[Test]
		[ExpectedException (typeof (XamlDuplicateMemberException))] // duplicate member assignment
		public void ObjectContainsObjectAndValue2 ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm3);
			xw.WriteStartObject (xt3);
			xw.WriteEndObject ();
			xw.WriteValue ("foo");
			xw.WriteEndMember (); // ... until here.
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))] // unlike XamlXmlWriter (IOE)
		public void EndObjectAfterNamespace ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteNamespace (new NamespaceDeclaration ("urn:foo", "y"));
			xw.WriteEndObject ();
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))] // ... unlike XamlXmlWriter (throws IOE)
		public void WriteValueAfterNamespace ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt);
			xw.WriteStartMember (XamlLanguage.Initialization);
			xw.WriteNamespace (new NamespaceDeclaration ("urn:foo", "y"));
			xw.WriteValue ("foo");
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))] // ... unlike XamlXmlWriter (allowed)
		public void ValueThenStartObject ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm2);
			xw.WriteValue ("foo");
			xw.WriteStartObject (xt3);
			xw.Close ();
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))] // ... unlike XamlXmlWriter (allowed, as it allows StartObject after Value)
		public void ValueThenNamespace ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm2);
			xw.WriteValue ("foo");
			xw.WriteNamespace (new NamespaceDeclaration ("y", "urn:foo")); // this does not raise an error (since it might start another object)
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))] // strange, this does *not* result in IOE...
		public void ValueThenNamespaceThenEndMember ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm2);
			xw.WriteValue ("foo");
			xw.WriteNamespace (new NamespaceDeclaration ("y", "urn:foo"));
			xw.WriteEndMember ();
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))] // This is also very different, requires exactly opposite namespace output manner to XamlXmlWriter (namespace first, object follows).
		public void StartMemberAfterNamespace ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteNamespace (new NamespaceDeclaration ("urn:foo", "y"));
		}

		[Test]
		[Category ("NotWorking")] // not worthy of passing
		public void StartMemberBeforeNamespace ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm2); // note that it should be done *after* WriteNamespace in XamlXmlWriter. SO inconsistent.
			xw.WriteNamespace (new NamespaceDeclaration ("urn:foo", "y"));
			xw.WriteEndMember ();
			xw.Close ();
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void StartMemberBeforeNamespace2 ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm2);
			xw.WriteNamespace (new NamespaceDeclaration ("urn:foo", "y"));
			// and here, NamespaceDeclaration is written as if it 
			// were another value object( unlike XamlXmlWriter)
			// and rejects further value.
			xw.WriteValue ("foo");
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void EndMemberThenStartObject ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm2);
			xw.WriteValue ("foo");
			xw.WriteEndMember ();
			xw.WriteStartObject (xt3);
		}

		// The semantics on WriteGetObject() is VERY different from XamlXmlWriter.

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void GetObjectOnNullValue ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm2);
			xw.WriteGetObject ();
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void GetObjectOnNullValue2 ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt4);
			xw.WriteStartMember (new XamlMember (typeof (Foo).GetProperty ("Baz"), sctx)); // unlike Bar, Baz is not initialized.
			xw.WriteGetObject (); // fails, because it is null.
		}

		[Test]
		public void GetObjectOnIntValue ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xt3.GetMember ("TestProp4")); // int
			xw.WriteGetObject (); // passes!!! WTF
			xw.WriteEndObject ();
		}

		[Test]
		// String is not treated as a collection on XamlXmlWriter, while this XamlObjectWriter does.
		public void GetObjectOnNonNullString ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			Assert.IsNull (xw.Result, "#1");
			xw.WriteStartMember (xt3.GetMember ("TestProp3"));
			xw.WriteGetObject ();
			Assert.IsNull (xw.Result, "#2");
		}

		[Test]
		public void GetObjectOnCollection ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt4);
			xw.WriteStartMember (new XamlMember (typeof (Foo).GetProperty ("Bar"), sctx));
			xw.WriteGetObject ();
			xw.Close ();
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void ValueAfterGetObject ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt4);
			xw.WriteStartMember (new XamlMember (typeof (Foo).GetProperty ("Bar"), sctx));
			xw.WriteGetObject ();
			xw.WriteValue ("foo");
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void StartObjectAfterGetObject ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt4);
			xw.WriteStartMember (new XamlMember (typeof (Foo).GetProperty ("Bar"), sctx));
			xw.WriteGetObject ();
			xw.WriteStartObject (xt);
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void EndMemberAfterGetObject ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt4);
			xw.WriteStartMember (new XamlMember (typeof (Foo).GetProperty ("Bar"), sctx));
			xw.WriteGetObject ();
			xw.WriteEndMember (); // ...!?
		}

		[Test]
		public void StartMemberAfterGetObject ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt4);
			var xmm = xt4.GetMember ("Bar");
			xw.WriteStartMember (xmm); // <List.Bar>
			xw.WriteGetObject (); // shifts current member to List<T>.
			xw.WriteStartMember (xmm.Type.GetMember ("Capacity"));
			xw.WriteValue (5);
			xw.WriteEndMember ();
			/*
			xw.WriteEndObject (); // got object
			xw.WriteEndMember (); // Bar
			xw.WriteEndObject (); // started object
			*/
			xw.Close ();
		}

		[Test]
		public void EndObjectAfterGetObject ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt4);
			xw.WriteStartMember (new XamlMember (typeof (Foo).GetProperty ("Bar"), sctx));
			xw.WriteGetObject ();
			xw.WriteEndObject ();
		}

		[Test]
		public void WriteAttachableProperty ()
		{
			Attached2 result = null;
			
			var rsettings = new XamlXmlReaderSettings ();
			using (var reader = new XamlXmlReader (new StringReader (String.Format (@"<Attached2 AttachedWrapper3.Property=""Test"" xmlns=""clr-namespace:MonoTests.System.Xaml;assembly={0}""></Attached2>", typeof (AttachedWrapper3).Assembly.GetName ().Name)), rsettings)) {
				var wsettings = new XamlObjectWriterSettings ();
				using (var writer = new XamlObjectWriter (reader.SchemaContext, wsettings)) {
					XamlServices.Transform (reader, writer, false);
					result = (Attached2) writer.Result;
				}
			}

			Assert.AreEqual ("Test", result.Property, "#1");
		}
		
		[Test]
		public void OnSetValueAndHandledFalse () // part of bug #3003
		{
			const string ver = "net_4_x";

			/*
			var obj = new TestClass3 ();
			obj.Nested = new TestClass3 ();
			var sw = new StringWriter ();
			var xxw = new XamlXmlWriter (XmlWriter.Create (sw), new XamlSchemaContext ());
			XamlServices.Transform (new XamlObjectReader (obj), xxw);
			Console.Error.WriteLine (sw);
			*/
			var xml = "<TestClass3 xmlns='clr-namespace:MonoTests.System.Xaml;assembly=System.Xaml_test_net_4_0' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'><TestClass3.Nested><TestClass3 Nested='{x:Null}' /></TestClass3.Nested></TestClass3>".Replace ("net_4_0", ver);
			var settings = new XamlObjectWriterSettings ();
			bool invoked = false;
			settings.XamlSetValueHandler = (sender, e) => {
				invoked = true;
				Assert.IsNotNull (sender, "#1");
				Assert.AreEqual (typeof (TestClass3), sender.GetType (), "#2");
				Assert.AreEqual ("Nested", e.Member.Name, "#3");
				Assert.IsTrue (sender != e.Member.Invoker.GetValue (sender), "#4");
				Assert.IsFalse (e.Handled, "#5");
				// ... and leave Handled as false, to invoke the actual setter
			};
			var xow = new XamlObjectWriter (new XamlSchemaContext (), settings);
			var xxr = new XamlXmlReader (XmlReader.Create (new StringReader (xml)));
			XamlServices.Transform (xxr, xow);
			Assert.IsTrue (invoked, "#6");
			Assert.IsNotNull (xow.Result, "#7");
			var ret = xow.Result as TestClass3;
			Assert.IsNotNull (ret.Nested, "#8");
		}
		
		[Test] // bug #3003 repro
		public void EventsAndProcessingOrder ()
		{
			var asm = Assembly.GetExecutingAssembly ();
			var context = new XamlSchemaContext (new Assembly [] { asm });
			var output = XamarinBug3003.TestContext.Writer;
			output.WriteLine ();

			var reader = new XamlXmlReader (XmlReader.Create (new StringReader (XamarinBug3003.TestContext.XmlInput)), context);

			var writerSettings = new XamlObjectWriterSettings ();
			writerSettings.AfterBeginInitHandler = (sender, e) => {
				output.WriteLine ("XamlObjectWriterSettings.AfterBeginInit: {0}", e.Instance);
			};
			writerSettings.AfterEndInitHandler = (sender, e) => {
				output.WriteLine ("XamlObjectWriterSettings.AfterEndInit: {0}", e.Instance);
			};

			writerSettings.BeforePropertiesHandler = (sender, e) => {
				output.WriteLine ("XamlObjectWriterSettings.BeforeProperties: {0}", e.Instance);
			};
			writerSettings.AfterPropertiesHandler = (sender, e) => {
				output.WriteLine ("XamlObjectWriterSettings.AfterProperties: {0}", e.Instance);
			};
			writerSettings.XamlSetValueHandler = (sender, e) => {
				output.WriteLine ("XamlObjectWriterSettings.XamlSetValue: {0}, Member: {1}", e.Value, e.Member.Name);
			};

			var writer = new XamlObjectWriter (context, writerSettings);
			XamlServices.Transform (reader, writer);
			var obj = writer.Result as XamarinBug3003.Parent;

			output.WriteLine ("Loaded {0}", obj);

			Assert.AreEqual (XamarinBug3003.TestContext.ExpectedResult.Replace ("\r\n", "\n"), output.ToString ().Replace ("\r\n", "\n"), "#1");

			Assert.AreEqual (2, obj.Children.Count, "#2");
		}

		// extra use case based tests.

		[Test]
		public void WriteEx_Type_WriteString ()
		{
			var ow = new XamlObjectWriter (sctx);
			ow.WriteNamespace (new NamespaceDeclaration (XamlLanguage.Xaml2006Namespace, "x"
			));
			ow.WriteStartObject (XamlLanguage.Type);
			ow.WriteStartMember (XamlLanguage.PositionalParameters);
			ow.WriteValue ("x:Int32");
			ow.Close ();
			Assert.AreEqual (typeof (int), ow.Result, "#1");
		}

		[Test]
		public void WriteEx_Type_WriteType ()
		{
			var ow = new XamlObjectWriter (sctx);
			ow.WriteNamespace (new NamespaceDeclaration (XamlLanguage.Xaml2006Namespace, "x"
			));
			ow.WriteStartObject (XamlLanguage.Type);
			ow.WriteStartMember (XamlLanguage.PositionalParameters);
			ow.WriteValue (typeof (int));
			ow.Close ();
			Assert.AreEqual (typeof (int), ow.Result, "#1");
		}

		[Test]
		public void LookupCorrectEventBoundMethod ()
		{
			var o = (XamarinBug2927.MyRootClass) XamlServices.Load (GetReader ("LookupCorrectEvent.xml"));
			o.Child.Descendant.Work ();
			Assert.IsTrue (o.Invoked, "#1");
			Assert.IsFalse (o.Child.Invoked, "#2");
			Assert.IsFalse (o.Child.Descendant.Invoked, "#3");
		}
		
		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
		public void LookupCorrectEventBoundMethod2 ()
		{
			XamlServices.Load (GetReader ("LookupCorrectEvent2.xml"));
		}
		
		[Test]
		public void LookupCorrectEventBoundMethod3 ()
		{
			XamlServices.Load (GetReader ("LookupCorrectEvent3.xml"));
		}
		
		// common use case based tests (to other readers/writers).

		XamlReader GetReader (string filename)
		{
			const string ver = "net_4_x";
			string xml = File.ReadAllText (Path.Combine ("Test/XmlFiles", filename)).Replace ("net_4_0", ver);
			return new XamlXmlReader (XmlReader.Create (new StringReader (xml)));
		}

		[Test]
		public void Write_String ()
		{
			using (var xr = GetReader ("String.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual ("foo", des, "#1");
			}
		}

		[Test]
		public void Write_Int32 ()
		{
			using (var xr = GetReader ("Int32.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (5, des, "#1");
			}
		}

		[Test]
		public void Write_DateTime ()
		{
			using (var xr = GetReader ("DateTime.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (new DateTime (2010, 4, 14), des, "#1");
			}
		}

		[Test]
		public void Write_TimeSpan ()
		{
			using (var xr = GetReader ("TimeSpan.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (TimeSpan.FromMinutes (7), des, "#1");
			}
		}

		[Test]
		public void Write_Uri ()
		{
			using (var xr = GetReader ("Uri.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (new Uri ("urn:foo"), des, "#1");
			}
		}

		[Test]
		public void Write_Null ()
		{
			using (var xr = GetReader ("NullExtension.xml")) {
				var des = XamlServices.Load (xr);
				Assert.IsNull (des, "#1");
			}
		}

		[Test]
		public void Write_Type ()
		{
			using (var xr = GetReader ("Type.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (typeof (int), des, "#1");
			}
		}

		[Test]
		public void Write_Type2 ()
		{
			var obj = typeof (MonoTests.System.Xaml.TestClass1);
			using (var xr = GetReader ("Type2.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (obj, des, "#1");
			}
		}

		[Test]
		public void Write_Guid ()
		{
			var obj = Guid.Parse ("9c3345ec-8922-4662-8e8d-a4e41f47cf09");
			using (var xr = GetReader ("Guid.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (obj, des, "#1");
			}
		}

		[Test]
		public void Write_GuidFactoryMethod ()
		{
			var obj = Guid.Parse ("9c3345ec-8922-4662-8e8d-a4e41f47cf09");
			using (var xr = GetReader ("GuidFactoryMethod.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (obj, des, "#1");
			}
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))] // cannot resolve the StaticExtension value.
		public void Write_StaticExtension ()
		{
			var obj = new StaticExtension ("FooBar");
			using (var xr = GetReader ("StaticExtension.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (obj, des, "#1");
			}
		}

		[Test]
		[Ignore ("Not sure why MemberType is NOT serialized. Needs investigation")]
		public void Write_StaticExtension2 ()
		{
			var obj = new StaticExtension ("FooBar"); //incorrect
			using (var xr = GetReader ("StaticExtension2.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (obj, des, "#1");
			}
		}

		[Test]
		public void Write_Reference ()
		{
			using (var xr = GetReader ("Reference.xml")) {
				var des = XamlServices.Load (xr);
				// .NET does not return Reference.
				// Its ProvideValue() returns MS.Internal.Xaml.Context.NameFixupToken,
				// which is assumed (by name) to resolve to the referenced object.
				Assert.IsNotNull (des, "#1");
				//Assert.AreEqual (new Reference ("FooBar"), des, "#1");
			}
		}

		[Test]
		public void Write_ArrayInt32 ()
		{
			var obj = new int [] {4, -5, 0, 255, int.MaxValue};
			using (var xr = GetReader ("Array_Int32.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (obj, des, "#1");
			}
		}

		[Test]
		public void Write_ListInt32 ()
		{
			var obj = new int [] {5, -3, int.MaxValue, 0}.ToList ();
			using (var xr = GetReader ("List_Int32.xml")) {
				var des = (List<int>) XamlServices.Load (xr);
				Assert.AreEqual (obj.ToArray (), des.ToArray (), "#1");
			}
		}

		[Test]
		public void Write_ListInt32_2 ()
		{
			var obj = new List<int> (new int [0]) { Capacity = 0 }; // set explicit capacity for trivial implementation difference
			using (var xr = GetReader ("List_Int32_2.xml")) {
				var des = (List<int>) XamlServices.Load (xr);
				Assert.AreEqual (obj.ToArray (), des.ToArray (), "#1");
			}
		}

		[Test]
		public void Write_ListType ()
		{
			var obj = new List<Type> (new Type [] {typeof (int), typeof (Dictionary<Type, XamlType>)}) { Capacity = 2 };
			using (var xr = GetReader ("List_Type.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (obj, des, "#1");
			}
		}

		[Test]
		public void Write_ListArray ()
		{
			var obj = new List<Array> (new Array [] { new int [] { 1,2,3}, new string [] { "foo", "bar", "baz" }}) { Capacity = 2 };
			using (var xr = GetReader ("List_Array.xml")) {
				var des = (List<Array>) XamlServices.Load (xr);
				Assert.AreEqual (obj, des, "#1");
			}
		}

		[Test]
		public void Write_DictionaryInt32String ()
		{
			var dic = new Dictionary<int,string> ();
			dic.Add (0, "foo");
			dic.Add (5, "bar");
			dic.Add (-2, "baz");
			using (var xr = GetReader ("Dictionary_Int32_String.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (dic, des, "#1");
			}
		}

		[Test]
		public void Write_DictionaryStringType ()
		{
			var dic = new Dictionary<string,Type> ();
			dic.Add ("t1", typeof (int));
			dic.Add ("t2", typeof (int []));
			dic.Add ("t3", typeof (int?));
			dic.Add ("t4", typeof (List<int>));
			dic.Add ("t5", typeof (Dictionary<int,DateTime>));
			dic.Add ("t6", typeof (List<KeyValuePair<int,DateTime>>));
			using (var xr = GetReader ("Dictionary_String_Type.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (dic, des, "#1");
			}
		}

		[Test]
		[Ignore ("Needs to get successfully deserialized. Currently we can't")]
		public void Write_PositionalParameters1Wrapper ()
		{
			// Unlike the above case, this has the wrapper object and hence PositionalParametersClass1 can be written as an attribute (markup extension)
			var obj = new PositionalParametersWrapper ("foo", 5);
			using (var xr = GetReader ("PositionalParametersWrapper.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (obj, des, "#1");
			}
		}
		
		[Test]
		public void Write_ArgumentAttributed ()
		{
			//var obj = new ArgumentAttributed ("foo", "bar");
			using (var xr = GetReader ("ArgumentAttributed.xml")) {
				var des = (ArgumentAttributed) XamlServices.Load (xr);
				Assert.AreEqual ("foo", des.Arg1, "#1");
				Assert.AreEqual ("bar", des.Arg2, "#2");
			}
		}

		[Test]
		public void Write_ArrayExtension2 ()
		{
			//var obj = new ArrayExtension (typeof (int));
			using (var xr = GetReader ("ArrayExtension2.xml")) {
				var des = XamlServices.Load (xr);
				// The resulting object is not ArrayExtension.
				Assert.AreEqual (new int [0], des, "#1");
			}
		}

		[Test]
		public void Write_ArrayList ()
		{
			var obj = new ArrayList (new int [] {5, -3, 0});
			using (var xr = GetReader ("ArrayList.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (obj, des, "#1");
			}
		}

		[Test]
		[Ignore ("Needs to get successfully deserialized. Currently we can't")]
		public void ComplexPositionalParameterWrapper ()
		{
			//var obj = new ComplexPositionalParameterWrapper () { Param = new ComplexPositionalParameterClass (new ComplexPositionalParameterValue () { Foo = "foo" })};
			using (var xr = GetReader ("ComplexPositionalParameterWrapper.xml")) {
				var des = (ComplexPositionalParameterWrapper) XamlServices.Load (xr);
				Assert.IsNotNull (des.Param, "#1");
				Assert.AreEqual ("foo", des.Param.Value, "#2");
			}
		}

		[Test]
		public void Write_ListWrapper ()
		{
			var obj = new ListWrapper (new List<int> (new int [] {5, -3, 0}) { Capacity = 3}); // set explicit capacity for trivial implementation difference
			using (var xr = GetReader ("ListWrapper.xml")) {
				var des = (ListWrapper) XamlServices.Load (xr);
				Assert.IsNotNull (des, "#1");
				Assert.IsNotNull (des.Items, "#2");
				Assert.AreEqual (obj.Items.ToArray (), des.Items.ToArray (), "#3");
			}
		}

		[Test]
		public void Write_ListWrapper2 ()
		{
			var obj = new ListWrapper2 (new List<int> (new int [] {5, -3, 0}) { Capacity = 3}); // set explicit capacity for trivial implementation difference
			using (var xr = GetReader ("ListWrapper2.xml")) {
				var des = (ListWrapper2) XamlServices.Load (xr);
				Assert.IsNotNull (des, "#1");
				Assert.IsNotNull (des.Items, "#2");
				Assert.AreEqual (obj.Items.ToArray (), des.Items.ToArray (), "#3");
			}
		}

		[Test]
		public void Write_MyArrayExtension ()
		{
			//var obj = new MyArrayExtension (new int [] {5, -3, 0});
			using (var xr = GetReader ("MyArrayExtension.xml")) {
				var des = XamlServices.Load (xr);
				// ProvideValue() returns an array
				Assert.AreEqual (new int [] {5, -3, 0}, des, "#1");
			}
		}

		[Test]
		public void Write_MyArrayExtensionA ()
		{
			//var obj = new MyArrayExtensionA (new int [] {5, -3, 0});
			using (var xr = GetReader ("MyArrayExtensionA.xml")) {
				var des = XamlServices.Load (xr);
				// ProvideValue() returns an array
				Assert.AreEqual (new int [] {5, -3, 0}, des, "#1");
			}
		}

		[Test]
		public void Write_MyExtension ()
		{
			//var obj = new MyExtension () { Foo = typeof (int), Bar = "v2", Baz = "v7"};
			using (var xr = GetReader ("MyExtension.xml")) {
				var des = XamlServices.Load (xr);
				// ProvideValue() returns this.
				Assert.AreEqual ("provided_value", des, "#1");
			}
		}

		[Test]
		[ExpectedException (typeof (InvalidCastException))] // unable to cast string to MarkupExtension
		[Category ("NotWorking")]
		public void Write_MyExtension2 ()
		{
			//var obj = new MyExtension2 () { Foo = typeof (int), Bar = "v2"};
			using (var xr = GetReader ("MyExtension2.xml")) {
				XamlServices.Load (xr);
			}
		}

		[Test]
		public void Write_MyExtension3 ()
		{
			//var obj = new MyExtension3 () { Foo = typeof (int), Bar = "v2"};
			using (var xr = GetReader ("MyExtension3.xml")) {
				var des = XamlServices.Load (xr);
				// StringConverter is used and the resulting value comes from ToString().
				Assert.AreEqual ("MonoTests.System.Xaml.MyExtension3", des, "#1");
			}
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))] // wrong TypeConverter input (input string for DateTimeConverter invalid)
		public void Write_MyExtension4 ()
		{
			var obj = new MyExtension4 () { Foo = typeof (int), Bar = "v2"};
			using (var xr = GetReader ("MyExtension4.xml")) {
				var des = XamlServices.Load (xr);
				Assert.AreEqual (obj, des, "#1");
			}
		}

		[Test]
		public void Write_MyExtension6 ()
		{
			//var obj = new MyExtension6 ("foo");
			using (var xr = GetReader ("MyExtension6.xml")) {
				var des = XamlServices.Load (xr);
				// ProvideValue() returns this.
				Assert.AreEqual ("foo", des, "#1");
			}
		}
		
		[Test]
		public void Write_PropertyDefinition ()
		{
			//var obj = new PropertyDefinition () { Modifier = "protected", Name = "foo", Type = XamlLanguage.String };
			using (var xr = GetReader ("PropertyDefinition.xml")) {
				var des = (PropertyDefinition) XamlServices.Load (xr);
				Assert.AreEqual ("protected", des.Modifier, "#1");
				Assert.AreEqual ("foo", des.Name, "#2");
				Assert.AreEqual (XamlLanguage.String, des.Type, "#3");
			}
		}
		
		[Test]
		[Ignore ("this still does not give successful deserialization result - should there be any way?")]
		public void Write_StaticExtensionWrapper ()
		{
			//var obj = new StaticExtensionWrapper () { Param = new StaticExtension ("Foo") };
			using (var xr = GetReader ("StaticExtensionWrapper.xml")) {
				var des = (StaticExtensionWrapper) XamlServices.Load (xr);
				Assert.IsNotNull (des.Param, "#1");
				Assert.AreEqual ("Foo", des.Param.Member, "#2");
			}
		}
		
		[Test]
		[Ignore ("this still does not give successful deserialization result - should there be any way?")]
		public void Write_TypeExtensionWrapper ()
		{
			//var obj = new TypeExtensionWrapper () { Param = new TypeExtension ("Foo") };
			using (var xr = GetReader ("TypeExtensionWrapper.xml")) {
				var des = (TypeExtensionWrapper) XamlServices.Load (xr);
				Assert.IsNotNull (des.Param, "#1");
				// TypeName was not serialized into xml, hence deserialized as empty.
				Assert.AreEqual (String.Empty, des.Param.TypeName, "#2");
			}
		}
		
		[Test]
		public void Write_NamedItems ()
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

			using (var xr = GetReader ("NamedItems.xml")) {
				var des = (NamedItem) XamlServices.Load (xr);
				Assert.IsNotNull (des, "#1");
				Assert.AreEqual (2, des.References.Count, "#2");
				Assert.AreEqual (typeof (NamedItem), des.References [0].GetType (), "#3");
				Assert.AreEqual (typeof (NamedItem), des.References [1].GetType (), "#4");
				Assert.AreEqual (des, des.References [0].References [0], "#5");
			}
		}
		
		[Test]
		public void Write_NamedItems2 ()
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

			using (var xr = GetReader ("NamedItems2.xml")) {
				var des = (NamedItem2) XamlServices.Load (xr);
				Assert.IsNotNull (des, "#1");
				Assert.AreEqual (2, des.References.Count, "#2");
				Assert.AreEqual (typeof (NamedItem2), des.References [0].GetType (), "#3");
				Assert.AreEqual (typeof (NamedItem2), des.References [1].GetType (), "#4");
				Assert.AreEqual (1, des.References [0].References.Count, "#5");
				Assert.AreEqual (1, des.References [1].References.Count, "#6");
				Assert.AreEqual (des.References [0].References [0], des.References [1].References [0], "#7");
			}
		}

		[Test]
		public void Write_XmlSerializableWrapper ()
		{
			var assns = "clr-namespace:MonoTests.System.Xaml;assembly=" + GetType ().Assembly.GetName ().Name;
			using (var xr = GetReader ("XmlSerializableWrapper.xml")) {
				var des = (XmlSerializableWrapper) XamlServices.Load (xr);
				Assert.IsNotNull (des, "#1");
				Assert.IsNotNull (des.Value, "#2");
				Assert.AreEqual ("<root xmlns=\"" + assns + "\" />", des.Value.GetRaw (), "#3");
			}
		}

		[Test]
		public void Write_XmlSerializable ()
		{
			using (var xr = GetReader ("XmlSerializable.xml")) {
				var des = (XmlSerializable) XamlServices.Load (xr);
				Assert.IsNotNull (des, "#1");
			}
		}

		[Test]
		public void Write_ListXmlSerializable ()
		{
			using (var xr = GetReader ("List_XmlSerializable.xml")) {
				var des = (List<XmlSerializable>) XamlServices.Load (xr);
				Assert.AreEqual (1, des.Count, "#1");
			}
		}

		[Test]
		public void Write_AttachedProperty ()
		{
			using (var xr = GetReader ("AttachedProperty.xml")) {
				AttachedWrapper des = null;
				try {
					des = (AttachedWrapper) XamlServices.Load (xr);
					Assert.IsNotNull (des.Value, "#1");
					Assert.AreEqual ("x", Attachable.GetFoo (des), "#2");
					Assert.AreEqual ("y", Attachable.GetFoo (des.Value), "#3");
				} finally {
					if (des != null) {
						Attachable.SetFoo (des, null);
						Attachable.SetFoo (des.Value, null);
					}
				}
			}
		}

		[Test]
		public void Write_EventStore ()
		{
			using (var xr = GetReader ("EventStore.xml")) {
				var res = (EventStore) XamlServices.Load (xr);
				Assert.AreEqual ("foo", res.Examine (), "#1");
				Assert.IsTrue (res.Method1Invoked, "#2");
			}
		}

		[Test]
		[ExpectedException (typeof (XamlDuplicateMemberException))] // for two occurence of Event1 ...
		public void Write_EventStore2 ()
		{
			using (var xr = GetReader ("EventStore2.xml")) {
				XamlServices.Load (xr);
			}
		}

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))] // attaching nonexistent method
		public void Write_EventStore3 ()
		{
			using (var xr = GetReader ("EventStore3.xml")) {
				XamlServices.Load (xr);
			}
		}

		[Test]
		[Category ("NotWorking")] // type resolution failure.
		public void Write_EventStore4 ()
		{
			using (var xr = GetReader ("EventStore4.xml")) {
				var res = (EventStore2<EventArgs>) XamlServices.Load (xr);
				Assert.AreEqual ("foo", res.Examine (), "#1");
				Assert.IsTrue (res.Method1Invoked, "#2");
			}
		}

		[Test]
		public void Write_AbstractWrapper ()
		{
			using (var xr = GetReader ("AbstractContainer.xml")) {
				var res = (AbstractContainer) XamlServices.Load (xr);
				Assert.IsNull (res.Value1, "#1");
				Assert.IsNotNull (res.Value2, "#2");
				Assert.AreEqual ("x", res.Value2.Foo, "#3");
			}
		}

		[Test]
		public void Write_ReadOnlyPropertyContainer ()
		{
			using (var xr = GetReader ("ReadOnlyPropertyContainer.xml")) {
				var res = (ReadOnlyPropertyContainer) XamlServices.Load (xr);
				Assert.AreEqual ("x", res.Foo, "#1");
				Assert.AreEqual ("x", res.Bar, "#2");
			}
		}

		[Test]
		public void Write_TypeConverterOnListMember ()
		{
			using (var xr = GetReader ("TypeConverterOnListMember.xml")) {
				var res = (SecondTest.TypeOtherAssembly) XamlServices.Load (xr);
				Assert.AreEqual (3, res.Values.Count, "#1");
				Assert.AreEqual (3, res.Values [2], "#2");
			}
		}

		[Test]
		public void Write_EnumContainer ()
		{
			using (var xr = GetReader ("EnumContainer.xml")) {
				var res = (EnumContainer) XamlServices.Load (xr);
				Assert.AreEqual (EnumValueType.Two, res.EnumProperty, "#1");
			}
		}

		[Test]
		public void Write_CollectionContentProperty ()
		{
			using (var xr = GetReader ("CollectionContentProperty.xml")) {
				var res = (CollectionContentProperty) XamlServices.Load (xr);
				Assert.AreEqual (4, res.ListOfItems.Count, "#1");
			}
		}

		[Test]
		public void Write_CollectionContentProperty2 ()
		{
			using (var xr = GetReader ("CollectionContentProperty2.xml")) {
				var res = (CollectionContentProperty) XamlServices.Load (xr);
				Assert.AreEqual (4, res.ListOfItems.Count, "#1");
			}
		}

		[Test]
		public void Write_AmbientPropertyContainer ()
		{
			using (var xr = GetReader ("AmbientPropertyContainer.xml")) {
				var res = (SecondTest.ResourcesDict) XamlServices.Load (xr);
				Assert.AreEqual (2, res.Count, "#1");
				Assert.IsTrue (res.ContainsKey ("TestDictItem"), "#2");
				Assert.IsTrue (res.ContainsKey ("okay"), "#3");
				var i1 = res ["TestDictItem"] as SecondTest.TestObject;
				Assert.IsNull (i1.TestProperty, "#4");
				var i2 = res ["okay"] as SecondTest.TestObject;
				Assert.AreEqual (i1, i2.TestProperty, "#5");
			}
		}

		[Test] // bug #682102
		public void Write_AmbientPropertyContainer2 ()
		{
			using (var xr = GetReader ("AmbientPropertyContainer2.xml")) {
				var res = (SecondTest.ResourcesDict) XamlServices.Load (xr);
				Assert.AreEqual (2, res.Count, "#1");
				Assert.IsTrue (res.ContainsKey ("TestDictItem"), "#2");
				Assert.IsTrue (res.ContainsKey ("okay"), "#3");
				var i1 = res ["TestDictItem"] as SecondTest.TestObject;
				Assert.IsNull (i1.TestProperty, "#4");
				var i2 = res ["okay"] as SecondTest.TestObject;
				Assert.AreEqual (i1, i2.TestProperty, "#5");
			}
		}

		[Test]
		public void Write_NullableContainer ()
		{
			using (var xr = GetReader ("NullableContainer.xml")) {
				var res = (NullableContainer) XamlServices.Load (xr);
				Assert.AreEqual (5, res.TestProp, "#1");
			}
		}

		[Test]
		public void Write_DirectListContainer ()
		{
			using (var xr = GetReader ("DirectListContainer.xml")) {
				var res = (DirectListContainer) XamlServices.Load (xr);
				Assert.AreEqual (3, res.Items.Count, "#1");
				Assert.AreEqual ("Hello3", res.Items [2].Value, "#2");
			}
		}

		[Test]
		public void Write_DirectDictionaryContainer ()
		{
			using (var xr = GetReader ("DirectDictionaryContainer.xml")) {
				var res = (DirectDictionaryContainer) XamlServices.Load (xr);
				Assert.AreEqual (3, res.Items.Count, "#1");
				Assert.AreEqual (40, res.Items [EnumValueType.Three], "#2");
			}
		}

		[Test]
		public void Write_DirectDictionaryContainer2 ()
		{
			using (var xr = GetReader ("DirectDictionaryContainer2.xml")) {
				var res = (SecondTest.ResourcesDict2) XamlServices.Load (xr);
				Assert.AreEqual (2, res.Count, "#1");
				Assert.AreEqual ("1", ((SecondTest.TestObject2) res ["1"]).TestProperty, "#2");
				Assert.AreEqual ("two", ((SecondTest.TestObject2) res ["two"]).TestProperty, "#3");
			}
		}
	}
}
