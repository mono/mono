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
	}
}

#endif

