//
// MemoryMappedFileTest.cs
//
// Author:
//   Zoltan Varga (vargaz@gmail.com)
//
// (C) 2009 Novell, Inc. (http://www.novell.com)
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

#if NET_4_0

using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;

using NUnit.Framework;

namespace MonoTests.System.IO.MemoryMappedFiles {

	[TestFixture]
	public class MemoryMappedFileTest {

		void AssertThrows<ExType> (Action del) where ExType : Exception {
			bool thrown = false;

			try {
				del ();
			} catch (ExType) {
				thrown = true;
			}
			Assert.IsTrue (thrown);
		}

		static int named_index;
		static String MkNamedMapping ()
		{
			return "test-" + named_index++;
		}


		static string tempDir = Path.Combine (Path.GetTempPath (), typeof (MemoryMappedFileTest).FullName);

		string fname;

		[SetUp]
		protected void SetUp () {
			if (Directory.Exists (tempDir))
				Directory.Delete (tempDir, true);

			Directory.CreateDirectory (tempDir);

			fname = Path.Combine (tempDir, "basic.txt");

			using (StreamWriter sw = new StreamWriter (fname)) {
				sw.WriteLine ("Hello!");
				sw.WriteLine ("World!");
			}
		}

		[TearDown]
		protected void TearDown () {
			if (Directory.Exists (tempDir))
				Directory.Delete (tempDir, true);
		}

		[Test]
		public void Basic () {
			var file = MemoryMappedFile.CreateFromFile (fname, FileMode.Open);

			using (var stream = file.CreateViewStream ()) {
				TextReader r = new StreamReader (stream);

				string s;

				s = r.ReadLine ();
				Assert.AreEqual ("Hello!", s);
				s = r.ReadLine ();
				Assert.AreEqual ("World!", s);
			}
		}

		[Test]
		public void CreateNew ()
		{
			// This must succeed
			MemoryMappedFile.CreateNew (Path.Combine (tempDir, "createNew.test"), 8192);
		}

		[Test]
		[ExpectedException (typeof (IOException))]
		public void CreateNew_OnExistingFile ()
		{
			// This must succeed
			MemoryMappedFile.CreateNew (Path.Combine (tempDir, "createNew.test"), 8192);
			
			// This should fail, the file exists
			MemoryMappedFile.CreateNew (Path.Combine (tempDir, "createNew.test"), 8192);
		}

		// Call this twice, it should always work
		[Test]
		public void CreateOrOpen_Multiple ()
		{
			MemoryMappedFile.CreateOrOpen (Path.Combine (tempDir, "createOrOpen.test"), 8192);
			MemoryMappedFile.CreateOrOpen (Path.Combine (tempDir, "createOrOpen.test"), 8192);
		}

		[Test]
		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void CreateFromFileWithSmallerCapacityThanFile ()
		{
			var f = Path.Combine (tempDir, "8192-file");
			File.WriteAllBytes (f, new byte [8192]);

			// We are requesting fewer bytes to map.
			MemoryMappedFile.CreateFromFile (f, FileMode.Open, "myMap", 4192);
		}
	
		[Test]
		public void CreateFromFile_Null () {
			AssertThrows<ArgumentNullException> (delegate () {
					MemoryMappedFile.CreateFromFile (null);
				});
		}

		[Test]
		public void CreateViewStream_Offsets () {
			var file = MemoryMappedFile.CreateFromFile (fname, FileMode.Open);

			using (var stream = file.CreateViewStream (2, 3)) {
				byte[] arr = new byte [128];

				int len = stream.Read (arr, 0, 128);

				Assert.AreEqual (3, len);

				Assert.AreEqual ('l', (char)arr [0]);
				Assert.AreEqual ('l', (char)arr [1]);
				Assert.AreEqual ('o', (char)arr [2]);
			}
		}

		[Test]
		public void CreateViewStream_Rights () {
			var file = MemoryMappedFile.CreateFromFile (fname, FileMode.Open);

			using (var stream = file.CreateViewStream (0, 0, MemoryMappedFileAccess.Read)) {
				AssertThrows<NotSupportedException> (delegate () {
						stream.WriteByte (0);
					});
			}

			using (var stream = file.CreateViewStream (0, 0, MemoryMappedFileAccess.Write)) {
				AssertThrows<NotSupportedException> (delegate () {
						stream.ReadByte ();
					});
			}
		}

		[Test]
		public unsafe void CreateViewBasic () {
			var file = MemoryMappedFile.CreateFromFile (fname, FileMode.Open);

			using (var v = file.CreateViewAccessor ()) {
				string s = "";

				// FIXME: Use using
				var handle = v.SafeMemoryMappedViewHandle;
				byte *b = null;

				try {
					handle.AcquirePointer (ref b);

					for (int i = 0; i < 5; ++i)
						s += (char)b [i];
				} finally {
					handle.ReleasePointer ();
				}

				Assert.AreEqual ("Hello", s);
			}
		}

		[Test]
		public unsafe void ViewReadArray () {
			var file = MemoryMappedFile.CreateFromFile (fname, FileMode.Open);

			using (var v = file.CreateViewAccessor ()) {
				var a = new byte [5];
				var n = v.ReadArray (0, a, 0, 5);
				Assert.AreEqual (5, n);
				var s = new string (Array.ConvertAll (a, b => (char)b));
				Assert.AreEqual ("Hello", s);
			}
		}


		[Test]
		public void NamedMappingToInvalidFile ()
		{
			var fileName = Path.Combine (tempDir, "temp_file_123");
	        if (File.Exists (fileName))
	            File.Delete (fileName);
	        var memoryMappedFile90 = MemoryMappedFile.CreateNew (fileName, 4194304, MemoryMappedFileAccess.ReadWrite);
	        memoryMappedFile90.CreateViewStream (4186112, 3222, MemoryMappedFileAccess.Write);
		}

		[Test]
		public void CreateTheSameAreaTwiceShouldFail ()
		{
			var name = MkNamedMapping ();
			using (var m0 = MemoryMappedFile.CreateNew(name, 4096, MemoryMappedFileAccess.ReadWrite)) {
				try {
					using (var m1 = MemoryMappedFile.CreateNew (name, 4096, MemoryMappedFileAccess.ReadWrite)) {
						Assert.Fail ("Must fail");
					}
				} catch (IOException) {}
			}
		}

		[Test]
		public void MapAFileToAMemoryAreaShouldFail ()
		{
			var name = MkNamedMapping ();
			using (var m0 = MemoryMappedFile.CreateNew(name, 4096, MemoryMappedFileAccess.ReadWrite)) {
				try {
					using (var m1 = MemoryMappedFile.CreateFromFile (fname, FileMode.OpenOrCreate, name)) {
						Assert.Fail ("Must fail");
					}
				} catch (IOException) {}
			}
		}

		[Test]
		public void NamedMappingsShareMemoryArea ()
		{
			var name = MkNamedMapping ();
			using (var m0 = MemoryMappedFile.CreateNew(name, 4096, MemoryMappedFileAccess.ReadWrite)) {
				using (var m1 = MemoryMappedFile.CreateOrOpen (name, 4096, MemoryMappedFileAccess.ReadWrite)) {
					using (MemoryMappedViewAccessor v0 = m0.CreateViewAccessor (), v1 = m1.CreateViewAccessor ()) {
						v0.Write (10, 0x12345);
						Assert.AreEqual (0x12345, v1.ReadInt32 (10));
					}
				}
			}
		}

		[Test]
		public void NamedFileCanBeOpen ()
		{
			var name = MkNamedMapping ();
			using (var sw = new FileStream (fname, FileMode.Open)) {
				byte[] b = new byte[20];
				for (int i = 0; i < 20; ++i)
					b[i] = 0xFF;
				sw.Write (b, 0, 20);
			}

			using (var m0 = MemoryMappedFile.CreateFromFile (fname, FileMode.Open, name)) {
				using (var m1 = MemoryMappedFile.CreateOrOpen (name, 4096)) {
					using (MemoryMappedViewAccessor v0 = m0.CreateViewAccessor (), v1 = m1.CreateViewAccessor ()) {
						v0.Write (10, 0x11223344);
						Assert.AreEqual (0x11223344, v1.ReadInt32 (10));
					}
				}
			}
		}

		[Test]
		public void MapAtEdgeOfPage ()
		{
			using (var f = new FileStream (fname, FileMode.Open)) {
				var b = new byte [4096];
				for (int i = 0; i < 4096; ++i)
					b[i] = 0xAA;
				for (int i = 0; i < 2; ++i)
					f.Write (b, 0, 4096);
			}
			var m0 = MemoryMappedFile.CreateFromFile (fname, FileMode.Open);
			var v0 = m0.CreateViewAccessor (500, 4096);
			var v1 = m0.CreateViewAccessor (0, 4096 * 2);
			for (int i = 0; i < 4096; ++i) {
				Assert.AreEqual (0xAA, v1.ReadByte (i + 500));
				v0.Write (i, (byte)0xFF);
				Assert.AreEqual (0xFF, v1.ReadByte (i + 500));
			}
		}

		[Test]
		public void DoubleAccountingInOffsetCalculation ()
		{
			var memoryMappedFile90 = MemoryMappedFile.CreateNew (MkNamedMapping (), 4194304, MemoryMappedFileAccess.ReadWrite);
			var stream = memoryMappedFile90.CreateViewStream (4186112, 3222, MemoryMappedFileAccess.Write);
			using (var tw = new StreamWriter(stream))
			{
				tw.WriteLine ("Hello World!");
			}
		}
	}
}

#endif

