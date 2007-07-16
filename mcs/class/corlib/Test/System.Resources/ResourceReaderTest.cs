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
				FileInfo code_base = new FileInfo (Assembly.GetExecutingAssembly ().Location);
				string base_path = Path.Combine (code_base.Directory.FullName, Path.Combine ("Test", "resources"));
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
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorString_Null ()
		{
			string s = null;
			ResourceReader r = new ResourceReader (s);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorString_Empty ()
		{
			ResourceReader r = new ResourceReader (String.Empty);
		}


		[Test]
		[ExpectedException (typeof (FileNotFoundException))]
		public void ConstructorString_NotFound ()
		{
			// use a file name that is *very* unlikely to exsist
			ResourceReader r = new ResourceReader ("j38f8axvnn9h38hfa9nxn93f8hav4zvag87vvah32o");
		}

		[Test]
		[Ignore ("Not covered in the docs, not sure what the correct behavior should be for this")]
		[ExpectedException (typeof (DirectoryNotFoundException))]
		public void ConstructorString_Bad ()
		{
			Assert.IsTrue (File.Exists (m_BadResourceFile));
			ResourceReader r = new ResourceReader (m_BadResourceFile);
		}

		[Test]
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
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorStream_Null ()
		{
			Stream s = null;
			ResourceReader r = new ResourceReader (s);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void ConstructorStream_Closed ()
		{
			Stream stream = new FileStream (m_ResourceFile, FileMode.Open);
			stream.Close ();
			ResourceReader r = new ResourceReader (stream);
		}

		[Test]
		public void Stream ()
		{
			Stream stream = new FileStream (m_ResourceFile, FileMode.Open);
			ResourceReader r = new ResourceReader (stream);
			Assert.IsNotNull (r, "ResourceReader");
			r.Close ();
		}

		[Test]
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

#if NET_2_0
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
#endif
	}
}
