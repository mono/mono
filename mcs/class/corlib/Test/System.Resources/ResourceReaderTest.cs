//
// MonoTests.System.Resources.ResourceReaderTest.cs
//
// Author:
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
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
		private static string m_ResourceFile = Path.GetDirectoryName (Assembly.GetAssembly (typeof (ResourceReaderTest)).Location) + Path.DirectorySeparatorChar + "resources" + Path.DirectorySeparatorChar + "MyResources.resources";
		private static string m_BadResourceFile = Path.GetDirectoryName (Assembly.GetAssembly (typeof (ResourceReaderTest)).Location) + Path.DirectorySeparatorChar + "resources" + Path.DirectorySeparatorChar + "Empty.resources";
		

		[Test]
		public void TestConstructorStringExceptions() {
			ResourceReader r;
			try {
				r = new ResourceReader((String)null);
				Fail("Should throw exception on null");
			} catch{}
			try {
				r = new ResourceReader("");
				Fail("Should throw exception on empty path");
			} catch{}
			try {
				// use a file name that is *very* unlikely to exsist
				r = new ResourceReader("j38f8axvnn9h38hfa9nxn93f8hav4zvag87vvah32o");
				Fail("Should throw exception on file not found");
			} catch{}
			try {
				r = new ResourceReader(m_BadResourceFile);
				Fail("Should throw exception on bad resource file");
			}
			catch {}
		}

		[Test]
		public void TestConstructorString() {
			if (!File.Exists(m_ResourceFile)) {
				Fail("Resource file is not where it should be:" + Directory.GetCurrentDirectory()+ "\\" + m_ResourceFile);
			}
			ResourceReader r = null;
			try {
				r = new ResourceReader(m_ResourceFile);
			}
			catch {
				Fail("Should have been able to open resource file.");
			}
			finally {
				if (null != r)
					r.Close();
			}
			Assert("String constructor should not be null", null != r);
		}

		[Test]
		public void TestConstructorStreamException1() {
			ResourceReader r;
			try {
				r = new ResourceReader((Stream)null);
				Fail("Should throw exception on null");
			} catch{}
		}

		[Ignore("makes mono throw an NullReferenceException")]
		[Test]
		public void TestConstructorStreamException2() {
			ResourceReader r;
			try {
				Stream stream = new FileStream (m_ResourceFile, FileMode.Open);
				stream.Close();
				r = new ResourceReader(stream);
				Fail("Should throw exception on cannot read");
			} catch{}
		}

		[Test]
		public void TestStream(){
			ResourceReader r = null;
			try {
				Stream stream = new FileStream (m_ResourceFile, FileMode.Open);
				r = new ResourceReader(stream);
			} 
			catch{
				Fail("Should not throw exception constructing from stream");
			}
			finally {
				if (null != r) {
					r.Close();
				}
			}
		}

		[Test]
		public void TestClose() {
			ResourceReader r = null;
			Stream stream = new FileStream (m_ResourceFile, FileMode.Open);
			r = new ResourceReader(stream);
			r.Close();
			try {
				stream = new FileStream (m_ResourceFile, FileMode.Open);
			} 
			catch{
				Fail("Should be able to open the stream again after close");
			}
			finally {
				if (null != stream) {
					stream.Close();
				}
			}
		}

		[Test]
		public void TestEnumerator(){
			ResourceReader reader = null;
			Stream stream = new FileStream (m_ResourceFile, FileMode.Open);
			reader = new ResourceReader(stream);
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
