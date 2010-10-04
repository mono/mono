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
			//
			// Basically, assume that no content could be written 
			// for an object member within XamlObjectWriter.
			xw.WriteEndMember ();
		}

		[Test]
		public void ValueAfterObject2 ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm3);
			xw.WriteStartObject (xt3);
			xw.WriteEndObject ();
			// passes here, but should be rejected later.
			xw.WriteValue ("foo");
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

		[Test]
		[ExpectedException (typeof (XamlObjectWriterException))]
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
		public void ObjectContainsObjectAndValue ()
		{
			var xw = new XamlObjectWriter (sctx, null);
			xw.WriteStartObject (xt3);
			xw.WriteStartMember (xm3);
			xw.WriteStartObject (xt3);
			xw.WriteEndObject ();
			xw.WriteValue ("foo"); // but this is allowed ...
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
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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
		[Category ("NotWorking")]
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
	}
}
