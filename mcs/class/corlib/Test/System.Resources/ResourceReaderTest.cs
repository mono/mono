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

namespace MonoTests.System.Resources {

	[TestFixture]
	public class ResourceReaderTest : Assertion {
		private static string m_ResourceFile;
		private static string m_BadResourceFile;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			char ds = Path.DirectorySeparatorChar;
			if (ds == '/') {
				FileInfo code_base = new FileInfo (Assembly.GetExecutingAssembly ().Location);
				string base_path = code_base.Directory.FullName + ds + "Test" + ds + "resources" + ds;
				m_ResourceFile = base_path + "MyResources.resources";
				m_BadResourceFile = base_path + "Empty.resources";
			} else {
				m_ResourceFile = Path.Combine ("Test", Path.Combine ("resources","MyResources.resources"));
				m_BadResourceFile = "resources" + ds + "Empty.resources";
			}
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
			ResourceReader r = new ResourceReader("j38f8axvnn9h38hfa9nxn93f8hav4zvag87vvah32o");
		}

		[Test]
		[ExpectedException (typeof (DirectoryNotFoundException))]
		public void ConstructorString_Bad () 
		{
			ResourceReader r = new ResourceReader(m_BadResourceFile);
		}

		[Test]
		public void ConstructorString () 
		{
			if (!File.Exists(m_ResourceFile)) {
				Fail ("Resource file is not where it should be:" + Path.Combine (Directory.GetCurrentDirectory(), m_ResourceFile));
			}
			ResourceReader r = new ResourceReader(m_ResourceFile);
			AssertNotNull ("ResourceReader", r);
			r.Close();
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void ConstructorStream_Null ()
		{
			Stream s = null;
			ResourceReader r = new ResourceReader (s);
			Fail("Should throw exception on null");
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
			AssertNotNull ("ResourceReader", r);
			r.Close();
		}

		[Test]
		public void Close () 
		{
			Stream stream = new FileStream (m_ResourceFile, FileMode.Open);
			ResourceReader r = new ResourceReader (stream);
			r.Close ();

			stream = new FileStream (m_ResourceFile, FileMode.Open);
			AssertNotNull ("FileStream", stream);
			stream.Close ();
		}

		[Test]
		public void Enumerator ()
		{
			Stream stream = new FileStream (m_ResourceFile, FileMode.Open);
			ResourceReader reader = new ResourceReader (stream);

			IDictionaryEnumerator en = reader.GetEnumerator();
			// Goes through the enumerator, printing out the key and value pairs.
			while (en.MoveNext()) {
				DictionaryEntry de = (DictionaryEntry)en.Current;
				Assert("Current.Key should not be empty",String.Empty != (string)de.Key);
				Assert("Current.Value should not be empty",String.Empty != (string)de.Value);
				Assert("Current.Value should not be empty",String.Empty != (string)de.Value);
				Assert("Entry.Key should not be empty",String.Empty != (string)en.Key);
				Assert("Entry.Value should not be empty",String.Empty != (string)en.Value);
			}
			reader.Close();
		}
	}  // class ResourceReaderTest
}  // namespace MonoTests.System.Resources
