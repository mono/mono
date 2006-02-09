//
// MonoTests.System.Xml.Serialization.XmlSerializationReaderTests	
//
// Author:
//	Gert Driesen (drieseng@users.sourceforge.net)
//
//  (C) 2006 Novell
// 

using System;
using System.Collections;
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
