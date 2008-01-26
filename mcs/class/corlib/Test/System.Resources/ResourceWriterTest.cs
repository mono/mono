//
// ResourceWriterTest.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Resources;

using NUnit.Framework;

namespace MonoTests.System.Resources
{
	[TestFixture]
	public class ResourceWriterTest
	{
		private string tempFolder = null;

		[SetUp]
		public void SetUp ()
		{
			tempFolder = Path.Combine (Path.GetTempPath (),
				"MonoTests.System.Resources.ResourceWriterTest");
			if (!Directory.Exists (tempFolder))
				Directory.CreateDirectory (tempFolder);
		}

		[TearDown]
		public void TearDown ()
		{
			if (Directory.Exists (tempFolder))
				Directory.Delete (tempFolder, true);
		}

		[Test] // ctor (Stream)
		public void Constructor0_Stream_NotWritable ()
		{
			MemoryStream ms = new MemoryStream (new byte [0], false);

			try {
				new ResourceWriter (ms);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Stream was not writable
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test] // ctor (Stream)
		public void Constructor0_Stream_Null ()
		{
			try {
				new ResourceWriter ((Stream) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("stream", ex.ParamName, "#6");
			}
		}

		[Test] // ctor (string)
		public void Constructor1_FileName_Null ()
		{
			try {
				new ResourceWriter ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("fileName", ex.ParamName, "#6");
			}
		}

		[Test] // AddResource (string, byte [])
		public void AddResource0 ()
		{
			byte [] value = new byte [] { 5, 7 };

			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", value);
			writer.Generate ();

			try {
				writer.AddResource ("Address", new byte [] { 8, 12 });
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// The resource writer has already been closed
				// and cannot be edited
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			ms.Position = 0;
			ResourceReader rr = new ResourceReader (ms);
			IDictionaryEnumerator enumerator = rr.GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext (), "#B1");
			Assert.AreEqual ("Name", enumerator.Key, "#B3");
			Assert.AreEqual (value, enumerator.Value, "#B4");
			Assert.IsFalse (enumerator.MoveNext (), "#B5");

			writer.Close ();
		}

		[Test] // AddResource (string, byte [])
		public void AddResource0_Name_Null ()
		{
			byte [] value = new byte [] { 5, 7 };

			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);

			try {
				writer.AddResource ((string) null, value);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}

			writer.Close ();
		}

		[Test] // AddResource (string, byte [])
		public void AddResource0_Value_Null ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", (byte []) null);
			writer.Generate ();

			ms.Position = 0;
			ResourceReader rr = new ResourceReader (ms);
			IDictionaryEnumerator enumerator = rr.GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext (), "#1");
			Assert.AreEqual ("Name", enumerator.Key, "#2");
			Assert.IsNull (enumerator.Value, "#3");
			Assert.IsFalse (enumerator.MoveNext (), "#4");

			writer.Close ();
		}

		[Test] // AddResource (string, object)
		public void AddResource1 ()
		{
			TimeSpan value = new TimeSpan (2, 5, 8);

			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Interval", value);
			writer.Generate ();

			try {
				writer.AddResource ("Start", value);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// The resource writer has already been closed
				// and cannot be edited
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			ms.Position = 0;
			ResourceReader rr = new ResourceReader (ms);
			IDictionaryEnumerator enumerator = rr.GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext (), "#B1");
			Assert.AreEqual ("Interval", enumerator.Key, "#B3");
			Assert.AreEqual (value, enumerator.Value, "#B4");
			Assert.IsFalse (enumerator.MoveNext (), "#B5");

			writer.Close ();
		}

		[Test] // AddResource (string, object)
		public void AddResource1_Name_Null ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);

			try {
				writer.AddResource ((string) null, new object ());
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}

			writer.Close ();
		}

		[Test] // AddResource (string, object)
		public void AddResource1_Value_Null ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", (object) null);
			writer.Generate ();

			ms.Position = 0;
			ResourceReader rr = new ResourceReader (ms);
			IDictionaryEnumerator enumerator = rr.GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext (), "#1");
			Assert.AreEqual ("Name", enumerator.Key, "#2");
			Assert.IsNull (enumerator.Value, "#3");
			Assert.IsFalse (enumerator.MoveNext (), "#4");

			writer.Close ();
		}

		[Test] // AddResource (string, string)
		public void AddResource2 ()
		{
			String value = "Some\0Value\tOr\rAnother";

			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Text", value);
			writer.Generate ();

			try {
				writer.AddResource ("Description", value);
				Assert.Fail ("#A1");
			} catch (InvalidOperationException ex) {
				// The resource writer has already been closed
				// and cannot be edited
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
			}

			ms.Position = 0;
			ResourceReader rr = new ResourceReader (ms);
			IDictionaryEnumerator enumerator = rr.GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext (), "#B1");
			Assert.AreEqual ("Text", enumerator.Key, "#B3");
			Assert.AreEqual (value, enumerator.Value, "#B4");
			Assert.IsFalse (enumerator.MoveNext (), "#B5");

			writer.Close ();
		}

		[Test] // AddResource (string, string)
		public void AddResource2_Name_Null ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);

			try {
				writer.AddResource ((string) null, "abc");
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("name", ex.ParamName, "#6");
			}

			writer.Close ();
		}

		[Test] // AddResource (string, string)
		public void AddResource2_Value_Null ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", (string) null);
			writer.Generate ();

			ms.Position = 0;
			ResourceReader rr = new ResourceReader (ms);
			IDictionaryEnumerator enumerator = rr.GetEnumerator ();
			Assert.IsTrue (enumerator.MoveNext (), "#1");
			Assert.AreEqual ("Name", enumerator.Key, "#2");
			Assert.IsNull (enumerator.Value, "#3");
			Assert.IsFalse (enumerator.MoveNext (), "#4");

			writer.Close ();
		}

		[Test]
		public void AddResource_Closed ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", "Miguel");
			writer.Close ();

			try {
				writer.AddResource ("Address", "US");
				Assert.Fail ("#1");
			} catch (InvalidOperationException ex) {
				// The resource writer has already been closed
				// and cannot be edited
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
			}
		}

		[Test]
		public void AddResource_Name_Duplicate ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("FirstName", "Miguel");

			try {
				writer.AddResource ("FirstNaMe", "Chris");
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Item has already been added. Key is dictionary:
				// 'FirstName'  Key being added: 'FirstNaMe'
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}

			writer.AddResource ("Name", "Miguel");
			writer.Close ();
		}

#if NET_2_0
		[Test]
		public void Bug81759 ()
		{
			MemoryStream ms = new MemoryStream ();
			using (ResourceReader xr = new ResourceReader (
				"Test/resources/bug81759.resources")) {
				ResourceWriter rw = new ResourceWriter (ms);
				foreach (DictionaryEntry de in xr)
					rw.AddResource ((string) de.Key, de.Value);
				rw.Close ();
			}
			ResourceReader rr = new ResourceReader (new MemoryStream (ms.ToArray ()));
			foreach (DictionaryEntry de in rr) {
				Assert.AreEqual ("imageList.ImageSize", de.Key as string, "#1");
				Assert.AreEqual ("Size", de.Value.GetType ().Name, "#2");
			}
		}
#endif

		[Test]
		public void Close ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", "Miguel");
			Assert.IsTrue (ms.CanWrite, "#A1");
			Assert.IsTrue (ms.GetBuffer ().Length == 0, "#A2");
			writer.Close ();
			Assert.IsFalse (ms.CanWrite, "#B1");
			Assert.IsFalse (ms.GetBuffer ().Length == 0, "#B2");
			writer.Close ();
		}

		[Test] // bug #339074
		public void Close_NoResources ()
		{
			string tempFile = Path.Combine (AppDomain.CurrentDomain.BaseDirectory,
				"test.resources");

			ResourceWriter writer = new ResourceWriter (tempFile);
			writer.Close ();

			using (FileStream fs = File.OpenRead (tempFile)) {
				Assert.IsFalse (fs.Length == 0, "#1");

				using (ResourceReader reader = new ResourceReader (fs)) {
					Assert.IsFalse (reader.GetEnumerator ().MoveNext (), "#2");
				}
			}
		}

		[Test]
		public void Generate ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", "Miguel");
			Assert.IsTrue (ms.Length == 0, "#A1");
			Assert.IsTrue (ms.CanWrite, "#A2");
			writer.Generate ();
			Assert.IsFalse (ms.Length == 0, "#B2");
			Assert.IsTrue (ms.CanWrite, "#B2");

			try {
				writer.Generate ();
				Assert.Fail ("#C1");
			} catch (InvalidOperationException ex) {
				// The resource writer has already been closed
				// and cannot be edited
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
			}

			writer.Close ();
		}

		[Test]
		public void Generate_Closed ()
		{
			MemoryStream ms = new MemoryStream ();
			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Name", "Miguel");
			writer.Close ();

			try {
				writer.Generate ();
				Assert.Fail ("#B1");
			} catch (InvalidOperationException ex) {
				// The resource writer has already been closed
				// and cannot be edited
				Assert.AreEqual (typeof (InvalidOperationException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
			}
		}

		[Test] // bug #82566
		public void WriteEnum ()
		{
			MemoryStream ms = new MemoryStream ();

			ResourceWriter writer = new ResourceWriter (ms);
			writer.AddResource ("Targets", AttributeTargets.Assembly);
			writer.Generate ();

			ms.Position = 0;

			bool found = false;

			ResourceReader reader = new ResourceReader (ms);
			foreach (DictionaryEntry de in reader) {
				string name = de.Key as string;
				Assert.IsNotNull (name, "#1");
				Assert.AreEqual ("Targets", name, "#2");
				Assert.IsNotNull (de.Value, "#3");
				Assert.AreEqual (AttributeTargets.Assembly, de.Value, "#4");
				found = true;
			}

			Assert.IsTrue (found, "#5");

			writer.Dispose ();
		}

	}
}
