//
// MonoTests.System.Resources.ResourceReaderTest.cs
//
// Author:
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak II
//


using System;
using System.Resources;
using System.IO;
using System.Collections;
using MonoTests.System.Resources;
using NUnit.Framework;

namespace MonoTests.System.Resources {

	public class ResourceReaderTest : TestCase {
		private static string m_ResourceFile = "MyResources.resources";
		private static string m_BadResourceFile = "Bad.resources";
		
		public ResourceReaderTest(String name) : base(name) {
		}
		
		public static ITest Suite {
			get {
				return new TestSuite(typeof(ResourceReaderTest));
			}
		}

		protected override void SetUp() {
		}

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

		public void TestConstructorString() {
			ResourceReader r = null;
			try {
				r = new ResourceReader(m_ResourceFile);
			}
			catch {
				Fail("Should have been able to open resource file: ");
			}
			finally {
				if (null != r)
					r.Close();
			}
			Assert("String constructor should not be null", null != r);
		}

		public void TestConstructorStreamExceptions() {
			ResourceReader r;
			try {
				r = new ResourceReader((Stream)null);
				Fail("Should throw exception on null");
			} catch{}

			try {
				Stream stream = new FileStream (m_ResourceFile, FileMode.Open);
				stream.Close();
				r = new ResourceReader(stream);
				Fail("Should throw exception on cannot read");
			} catch{}
		}

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

		public void TestEnumerator(){
			ResourceReader reader = null;
			Stream stream = new FileStream (m_ResourceFile, FileMode.Open);
			reader = new ResourceReader(stream);
			IDictionaryEnumerator en = reader.GetEnumerator();
      
			// Goes through the enumerator, printing out the key and value pairs.
			while (en.MoveNext()) {
				Console.WriteLine();
				Console.WriteLine("Name: {0}", en.Key);
				Console.WriteLine("Value: {0}", en.Value);
			}
			reader.Close();
		}
	}  // class ResourceReaderTest
}  // namespace MonoTests.System.Resources