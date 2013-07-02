//
// MonoTests.System.Resources.ResourceReaderTest.cs
//
// Author:
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
// Copyright (C) 2004 Novell (http://www.novell.com)
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
	public class ResourceReaderTest
	{
		internal static string m_ResourceFile;
		private static string m_BadResourceFile;
		private string _tempResourceFile;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			char ds = Path.DirectorySeparatorChar;
			if (ds == '/') {
				string base_path = Path.Combine (Directory.GetCurrentDirectory (), Path.Combine ("Test", "resources"));
				m_ResourceFile = Path.Combine (base_path, "MyResources.resources");
				m_BadResourceFile = Path.Combine (base_path, "Empty.resources");
			} else {
				m_ResourceFile = Path.Combine ("Test", Path.Combine ("resources", "MyResources.resources"));
				m_BadResourceFile = "resources" + ds + "Empty.resources";
			}
		}

		[SetUp]
		public void SetUp ()
		{
			_tempResourceFile = Path.GetTempFileName ();
		}

		[TearDown]
		public void TearDown ()
		{
			File.Delete (_tempResourceFile);
		}

		[Test]
		public void ConstructorString_Path_Null ()
		{
			try {
				new ResourceReader ((string) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("path", ex.ParamName, "#6");
			}
		}

		[Test]
		public void ConstructorString_Path_Empty ()
		{
			try {
				new ResourceReader (String.Empty);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Empty path name is not legal
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		[ExpectedException (typeof (FileNotFoundException))]
		public void ConstructorString_NotFound ()
		{
			// use a file name that is *very* unlikely to exsist
			new ResourceReader ("j38f8axvnn9h38hfa9nxn93f8hav4zvag87vvah32o");
		}

		[Test]
		[Ignore ("Not covered in the docs, not sure what the correct behavior should be for this")]
		[ExpectedException (typeof (DirectoryNotFoundException))]
		public void ConstructorString_Bad ()
		{
			Assert.IsTrue (File.Exists (m_BadResourceFile));
			new ResourceReader (m_BadResourceFile);
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void ConstructorString ()
		{
			if (!File.Exists (m_ResourceFile)) {
				Assert.Fail ("Resource file is not where it should be:" + Path.Combine (Directory.GetCurrentDirectory (), m_ResourceFile));
			}
			ResourceReader r = new ResourceReader (m_ResourceFile);
			Assert.IsNotNull (r, "ResourceReader");
			r.Close ();
		}

		[Test]
		public void ConstructorStream_Null ()
		{
			try {
				new ResourceReader ((Stream) null);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("stream", ex.ParamName, "#6");
			}
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void ConstructorStream_Closed ()
		{
			Stream stream = new FileStream (m_ResourceFile, FileMode.Open);
			stream.Close ();

			try {
				new ResourceReader (stream);
				Assert.Fail ("#1");
			} catch (ArgumentException ex) {
				// Stream was not readable
				Assert.AreEqual (typeof (ArgumentException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNull (ex.ParamName, "#5");
			}
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void Stream ()
		{
			Stream stream = new FileStream (m_ResourceFile, FileMode.Open);
			ResourceReader r = new ResourceReader (stream);
			Assert.IsNotNull (r, "ResourceReader");
			r.Close ();
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void Close ()
		{
			Stream stream = new FileStream (m_ResourceFile, FileMode.Open);
			ResourceReader r = new ResourceReader (stream);
			r.Close ();

			stream = new FileStream (m_ResourceFile, FileMode.Open);
			Assert.IsNotNull (stream, "FileStream");
			stream.Close ();
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void Enumerator ()
		{
			Stream stream = new FileStream (m_ResourceFile, FileMode.Open);
			ResourceReader reader = new ResourceReader (stream);

			IDictionaryEnumerator en = reader.GetEnumerator ();
			// Goes through the enumerator, printing out the key and value pairs.
			while (en.MoveNext ()) {
				DictionaryEntry de = (DictionaryEntry) en.Current;
				Assert.IsTrue (String.Empty != (string) de.Key, "Current.Key should not be empty");
				Assert.IsTrue (String.Empty != (string) de.Value, "Current.Value should not be empty");
				Assert.IsTrue (String.Empty != (string) en.Key, "Entry.Key should not be empty");
				Assert.IsTrue (String.Empty != (string) en.Value, "Entry.Value should not be empty");
			}
			reader.Close ();
		}

#if NET_2_0
		[Test] // bug #81757
		public void ReadNullResource ()
		{
			MemoryStream stream = new MemoryStream ();
			object value = null;
			ResourceWriter rw = new ResourceWriter (stream);
			rw.AddResource ("NullTest", value);
			rw.Generate ();
			stream.Position = 0;

			using (ResourceReader rr = new ResourceReader (stream)) {
				int entryCount = 0;
				foreach (DictionaryEntry de in rr) {
					Assert.AreEqual ("NullTest", de.Key, "#1");
					Assert.IsNull (de.Value, "#2");
					Assert.AreEqual (0, entryCount, "#3");
					entryCount++;
				}
				Assert.AreEqual (1, entryCount, "#4");
			}
		}
#endif

		[Test] // bug #79976
		public void ByteArray ()
		{
			byte [] content = new byte [] { 1, 2, 3, 4, 5, 6 };

			Stream stream = null;

#if NET_2_0
			// we currently do not support writing v2 resource files
			stream = new MemoryStream ();
			stream.Write (byte_resource_v2, 0, byte_resource_v2.Length);
			stream.Position = 0;
#else
			using (IResourceWriter rw = new ResourceWriter (_tempResourceFile)) {
				rw.AddResource ("byteArrayTest", content);
				rw.Generate ();
			}

			stream = File.OpenRead (_tempResourceFile);
#endif

			using (stream) {
				int entryCount = 0;
				using (IResourceReader rr = new ResourceReader (stream)) {
					foreach (DictionaryEntry de in rr) {
						Assert.AreEqual ("byteArrayTest", de.Key, "#1");
						Assert.AreEqual (content, de.Value, "#2");
						entryCount++;
					}
				}
				Assert.AreEqual (1, entryCount, "#3");
			}
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void GetResourceDataNullName ()
		{
			ResourceReader r = new ResourceReader ("Test/resources/StreamTest.resources");
			string type;
			byte [] bytes;

			try {
				r.GetResourceData (null, out type, out bytes);
				Assert.Fail ("#1");
			} catch (ArgumentNullException ex) {
				Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
				Assert.IsNull (ex.InnerException, "#3");
				Assert.IsNotNull (ex.Message, "#4");
				Assert.IsNotNull (ex.ParamName, "#5");
				Assert.AreEqual ("resourceName", ex.ParamName, "#6");
			} finally {
				r.Close ();
			}
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void GetResourceData ()
		{
			byte [] t1 = new byte [] {0x16, 0x00, 0x00, 0x00, 0x76, 0x65, 0x72, 0x69, 0x74, 0x61, 0x73, 0x20, 0x76, 0x6F, 0x73, 0x20, 0x6C, 0x69, 0x62, 0x65, 0x72, 0x61, 0x62, 0x69, 0x74, 0x0A};
			byte [] t2 = new byte [] {0x0A, 0x73, 0x6F, 0x6D, 0x65, 0x73, 0x74, 0x72, 0x69, 0x6E, 0x67};
			byte [] t3 = new byte [] {0x0E, 0x00, 0x00, 0x00, 0x73, 0x68, 0x61, 0x72, 0x64, 0x65, 0x6E, 0x66, 0x72, 0x65, 0x75, 0x64, 0x65, 0x0A};

			ResourceReader r = new ResourceReader ("Test/resources/StreamTest.resources");
			Hashtable items = new Hashtable ();
			foreach (DictionaryEntry de in r) {
				string type;
				byte [] bytes;
				r.GetResourceData ((string) de.Key, out type, out bytes);
				items [de.Key] = new DictionaryEntry (type, bytes);
			}

			DictionaryEntry p = (DictionaryEntry) items ["test"];
			Assert.AreEqual ("ResourceTypeCode.Stream", p.Key as string, "#1-1");
			Assert.AreEqual (t1, p.Value as byte [], "#1-2");

			p = (DictionaryEntry) items ["test2"];
			Assert.AreEqual ("ResourceTypeCode.String", p.Key as string, "#2-1");
			Assert.AreEqual (t2, p.Value as byte [], "#2-2");

			p = (DictionaryEntry) items ["test3"];
			Assert.AreEqual ("ResourceTypeCode.ByteArray", p.Key as string, "#3-1");
			Assert.AreEqual (t3, p.Value as byte [], "#3-2");

			r.Close ();
		}

		[Test]
		[Category ("MobileNotWorking")]
		public void GetResourceData2 ()
		{
			byte [] expected = new byte [] {
				0x00, 0x01, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF,
				0xFF, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
				0x00, 0x0C, 0x02, 0x00, 0x00, 0x00, 0x51, 0x53,
				0x79, 0x73, 0x74, 0x65, 0x6D, 0x2E, 0x44, 0x72,
				0x61, 0x77, 0x69, 0x6E, 0x67, 0x2C, 0x20, 0x56,
				0x65, 0x72, 0x73, 0x69, 0x6F, 0x6E, 0x3D, 0x32,
				0x2E, 0x30, 0x2E, 0x30, 0x2E, 0x30, 0x2C, 0x20,
				0x43, 0x75, 0x6C, 0x74, 0x75, 0x72, 0x65, 0x3D,
				0x6E, 0x65, 0x75, 0x74, 0x72, 0x61, 0x6C, 0x2C,
				0x20, 0x50, 0x75, 0x62, 0x6C, 0x69, 0x63, 0x4B,
				0x65, 0x79, 0x54, 0x6F, 0x6B, 0x65, 0x6E, 0x3D,
				0x62, 0x30, 0x33, 0x66, 0x35, 0x66, 0x37, 0x66,
				0x31, 0x31, 0x64, 0x35, 0x30, 0x61, 0x33, 0x61,
				0x05, 0x01, 0x00, 0x00, 0x00, 0x13, 0x53, 0x79,
				0x73, 0x74, 0x65, 0x6D, 0x2E, 0x44, 0x72, 0x61,
				0x77, 0x69, 0x6E, 0x67, 0x2E, 0x53, 0x69, 0x7A,
				0x65, 0x02, 0x00, 0x00, 0x00, 0x05, 0x77, 0x69,
				0x64, 0x74, 0x68, 0x06, 0x68, 0x65, 0x69, 0x67,
				0x68, 0x74, 0x00, 0x00, 0x08, 0x08, 0x02, 0x00,
				0x00, 0x00, 0x10, 0x00, 0x00, 0x00, 0x10, 0x00,
				0x00, 0x00, 0x0B};
			ResourceReader r = new ResourceReader ("Test/resources/bug81759.resources");
			string type;
			byte [] bytes;
			r.GetResourceData ("imageList.ImageSize", out type, out bytes);
			// Note that const should not be used here.
			Assert.AreEqual ("System.Drawing.Size, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", type, "#1");
			Assert.AreEqual (expected, bytes, "#2");
			r.Close ();
		}

		// we currently do not support writing v2 resource files
		private static readonly byte [] byte_resource_v2 = new byte [] {
			0xce, 0xca, 0xef, 0xbe, 0x01, 0x00, 0x00, 0x00, 0x91, 0x00, 0x00,
			0x00, 0x6c, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x52, 0x65,
			0x73, 0x6f, 0x75, 0x72, 0x63, 0x65, 0x73, 0x2e, 0x52, 0x65, 0x73,
			0x6f, 0x75, 0x72, 0x63, 0x65, 0x52, 0x65, 0x61, 0x64, 0x65, 0x72,
			0x2c, 0x20, 0x6d, 0x73, 0x63, 0x6f, 0x72, 0x6c, 0x69, 0x62, 0x2c,
			0x20, 0x56, 0x65, 0x72, 0x73, 0x69, 0x6f, 0x6e, 0x3d, 0x32, 0x2e,
			0x30, 0x2e, 0x30, 0x2e, 0x30, 0x2c, 0x20, 0x43, 0x75, 0x6c, 0x74,
			0x75, 0x72, 0x65, 0x3d, 0x6e, 0x65, 0x75, 0x74, 0x72, 0x61, 0x6c,
			0x2c, 0x20, 0x50, 0x75, 0x62, 0x6c, 0x69, 0x63, 0x4b, 0x65, 0x79,
			0x54, 0x6f, 0x6b, 0x65, 0x6e, 0x3d, 0x62, 0x37, 0x37, 0x61, 0x35,
			0x63, 0x35, 0x36, 0x31, 0x39, 0x33, 0x34, 0x65, 0x30, 0x38, 0x39,
			0x23, 0x53, 0x79, 0x73, 0x74, 0x65, 0x6d, 0x2e, 0x52, 0x65, 0x73,
			0x6f, 0x75, 0x72, 0x63, 0x65, 0x73, 0x2e, 0x52, 0x75, 0x6e, 0x74,
			0x69, 0x6d, 0x65, 0x52, 0x65, 0x73, 0x6f, 0x75, 0x72, 0x63, 0x65,
			0x53, 0x65, 0x74, 0x02, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00,
			0x00, 0x00, 0x00, 0x00, 0x50, 0x41, 0x44, 0x50, 0x41, 0x44, 0x50,
			0x80, 0x88, 0x5a, 0x0e, 0x00, 0x00, 0x00, 0x00, 0xdb, 0x00, 0x00,
			0x00, 0x1a, 0x62, 0x00, 0x79, 0x00, 0x74, 0x00, 0x65, 0x00, 0x41,
			0x00, 0x72, 0x00, 0x72, 0x00, 0x61, 0x00, 0x79, 0x00, 0x54, 0x00,
			0x65, 0x00, 0x73, 0x00, 0x74, 0x00, 0x00, 0x00, 0x00, 0x00, 0x20,
			0x06, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06 };
	}
}
