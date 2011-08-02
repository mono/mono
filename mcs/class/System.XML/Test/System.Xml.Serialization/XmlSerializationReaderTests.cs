//
// MonoTests.System.Xml.Serialization.XmlSerializationReaderTests	
//
// Author:
//	Gert Driesen (drieseng@users.sourceforge.net)
//	Atsushi Enomoto (atsushi@ximian.com)
//
//  (C) 2006 Gert Driesen
//  Copyright (C) 2007 Novell, Inc.
// 

using System;
using System.Collections;
using System.IO;
using System.Xml.Serialization;

using NUnit.Framework;

using MonoTests.System.Xml.TestClasses;

namespace MonoTests.System.XmlSerialization
{
	[TestFixture]
	public class XmlSerializationReaderTests : XmlSerializarionReaderTester
	{
		[Test]
		public void TestToEnum ()
		{
			Hashtable values = new Hashtable ();
			values.Add ("One", 1L);
			values.Add ("Two", 2L);
			values.Add ("Four", 4L);

			Assert.AreEqual (1, ToEnum ("One", values, "Some.Type.Name"), "#A1");
			Assert.AreEqual (2, ToEnum (" Two ", values, "Some.Type.Name"), "#A2");
			Assert.AreEqual (4, ToEnum ("Four", values, "Some.Type.Name"), "#A3");
			Assert.AreEqual (5, ToEnum ("One Four", values, "Some.Type.Name"), "#A4");
			Assert.AreEqual (7, ToEnum ("One Two Four", values, "Some.Type.Name"), "#A5");
			Assert.AreEqual (0, ToEnum ("", values, "Some.Type.Name"), "#A6");
			Assert.AreEqual (0, ToEnum ("     ", values, "Some.Type.Name"), "#A7");
			Assert.AreEqual (2, ToEnum ("Two Two", values, "Some.Type.Name"), "#A8");

			values.Add ("", 24L);
			Assert.AreEqual (24, ToEnum ("", values, "Some.Type.Name"), "#B1");
			Assert.AreEqual (24, ToEnum ("  ", values, "Some.Type.Name"), "#B2");
		}

		[Test]
		public void TestToEnum_InvalidValue ()
		{
			try {
				ToEnum ("SomeValue", new Hashtable (), "Some.Type.Name");
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNotNull (ex.Message, "#A3");
				Assert.IsTrue (ex.Message.IndexOf ("'SomeValue'") != -1, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("Some.Type.Name") != -1, "#A5");
				Assert.IsNull (ex.InnerException, "#A6");
			}

			Hashtable values = new Hashtable ();
			values.Add ("One", 1L);
			values.Add ("Two", 2L);
			values.Add ("Four", 4L);

			try {
				ToEnum ("one", values, "Some.Type.Name");
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNotNull (ex.Message, "#B3");
				Assert.IsTrue (ex.Message.IndexOf ("'one'") != -1, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("Some.Type.Name") != -1, "#B5");
				Assert.IsNull (ex.InnerException, "#B6");
			}

			values.Clear ();
			values.Add ("One", FlagEnum.e1);

			try {
				ToEnum ("One", values, "Some.Type.Name");
				Assert.Fail ("#C1");
			} catch (InvalidCastException ex) {
			}

			values.Clear ();
			values.Add ("One", 1);

			try {
				ToEnum ("One", values, "Some.Type.Name");
				Assert.Fail ("#D1");
			} catch (InvalidCastException ex) {
			}

			values.Clear ();
			values.Add ("One", null);

			try {
				ToEnum ("One", values, "Some.Type.Name");
				Assert.Fail ("#E1");
			} catch (InvalidOperationException ex) {
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#E2");
				Assert.IsNotNull (ex.Message, "#E3");
				Assert.IsTrue (ex.Message.IndexOf ("'One'") != -1, "#E4");
				Assert.IsTrue (ex.Message.IndexOf ("Some.Type.Name") != -1, "#E5");
				Assert.IsNull (ex.InnerException, "#E6");
			}
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestToEnum_Null_Value ()
		{
			ToEnum ((string) null, new Hashtable (), "DoesNotMatter");
		}

		[Test]
		[ExpectedException (typeof (NullReferenceException))]
		public void TestToEnum_Null_Values ()
		{
			ToEnum ("", (Hashtable) null, "DoesNotMatter");
		}

		[Test]
#if ONLY_1_1
		[Category ("NotDotNet")] // hangs on .NET 1.1
#endif
		public void HandleOutAttributeParameters ()
		{
			XmlReflectionMember m = new XmlReflectionMember ();
			m.MemberName = "hooray";
			m.MemberType = typeof (string);
			m.XmlAttributes = new XmlAttributes ();
			m.XmlAttributes.XmlAttribute = new XmlAttributeAttribute ();
			XmlReflectionImporter imp = new XmlReflectionImporter ();
			XmlMembersMapping map = imp.ImportMembersMapping (
				"elem", "urn:x", new XmlReflectionMember [] {m}, true);
			XmlSerializer ser = XmlSerializer.FromMappings (
				new XmlMapping [] {map}) [0];
			string xml = "<elem xmlns='urn:x' hooray='doh' />";
			object [] arr = ser.Deserialize (new StringReader (xml))
				as object [];
			Assert.IsNotNull (arr, "#1");
			Assert.AreEqual (1, arr.Length, "#2");
			Assert.AreEqual ("doh", arr [0], "#3");

			xml = "<elem xmlns='urn:x' hooray='doh'></elem>";
			arr = ser.Deserialize (new StringReader (xml)) as object [];
			Assert.IsNotNull (arr, "#4");
			Assert.AreEqual (1, arr.Length, "#5");
			Assert.AreEqual ("doh", arr [0], "#6");
		}

		[Test]
		public void ExplicitlyOrderedMembers1 ()
		{
			var xs = new XmlSerializer (typeof (ExplicitlyOrderedMembersType1));
			var result = (ExplicitlyOrderedMembersType1) xs.Deserialize(new StringReader (@"
<root>
	<child>Hello</child>
	<child>World</child>
	<child0>test</child0>
</root>"));
			Assert.AreEqual ("Hello", result.Child1, "#1");
			Assert.AreEqual ("World", result.Child2, "#2");
			Assert.AreEqual ("test", result.Child0, "#3");
		}

		[Test]
		[ExpectedException (typeof (InvalidOperationException))]
		public void ExplicitlyOrderedMembers2 ()
		{
			new XmlSerializer (typeof (ExplicitlyOrderedMembersType2));
		}

		[Test]
		public void ExplicitlyOrderedMembers3 ()
		{
			var xs = new XmlSerializer (typeof (ExplicitlyOrderedMembersType3));
			var result = (ExplicitlyOrderedMembersType3) xs.Deserialize(new StringReader (@"
<root>
	<child>Hello</child>
	<child>World</child>
	<child0>test</child0>
</root>"));
			Assert.AreEqual ("Hello", result.Child1, "#1");
			Assert.AreEqual ("World", result.Child2, "#2");
			Assert.IsNull (result.Child0, "#3"); // not "test"
		}
	}

	public class XmlSerializarionReaderTester : XmlSerializationReader
	{
		// appease the compiler
		protected override void InitCallbacks ()
		{
		}

		protected override void InitIDs ()
		{
		}
	}
}
