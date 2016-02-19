//
// ZipTests.cs
//
// Author:
//	   Joao Matos <joao.matos@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;

namespace MonoTests.System.IO.Compression
{
	[TestFixture]
	public class ZipArchiveTests
	{
		static string GetSHA1HashFromFile(Stream stream)
		{
			using (var sha1 = SHA1.Create())
			{
				return BitConverter.ToString(sha1.ComputeHash(stream))
					.Replace("-", string.Empty);
			}
		}

		[Test]
		public void ZipGetEntryReadMode()
		{
			File.Copy("archive.zip", "test.zip", overwrite: true);
			using (var archive = new ZipArchive(File.Open("test.zip", FileMode.Open),
				ZipArchiveMode.Read))
			{
				var entry = archive.GetEntry("foo.txt");
				Assert.IsNotNull(entry);

				var nullEntry = archive.GetEntry("nonexisting");
				Assert.IsNull(nullEntry);
			}

			File.Delete ("test.zip");
		}

		[Test]
		public void ZipGetEntryCreateMode()
		{
			File.Copy("archive.zip", "test.zip", overwrite: true);
			using (var archive = new ZipArchive(File.Open("test.zip", FileMode.Open),
				ZipArchiveMode.Create))
			{
				try {
					archive.GetEntry("foo");
				} catch(NotSupportedException ex) {
					return;
				}

				Assert.Fail();
			}

			File.Delete ("test.zip");
		}

		[Test]
		public void ZipGetEntryUpdateMode()
		{
			File.Copy("archive.zip", "test.zip", overwrite: true);
			using (var archive = new ZipArchive(File.Open("test.zip", FileMode.Open),
				ZipArchiveMode.Read))
			{
				var entry = archive.GetEntry("foo.txt");
				Assert.IsNotNull(entry);

				var nullEntry = archive.GetEntry("nonexisting");
				Assert.IsNull(nullEntry);
			}

			File.Delete ("test.zip");
		}

		[Test]
		public void ZipGetEntryOpen()
		{
			File.Copy("archive.zip", "test.zip", overwrite: true);
			using (var archive = new ZipArchive(File.Open("test.zip", FileMode.Open),
				ZipArchiveMode.Read))
			{
				var entry = archive.GetEntry("foo.txt");
				Assert.IsNotNull(entry);

				var foo = entry.Open();
			}

			File.Delete ("test.zip");
		}

		[Test]
		public void ZipOpenAndReopenEntry()
		{
			try {
				File.Copy("archive.zip", "test.zip", overwrite: true);
				using (var archive = new ZipArchive(File.Open("test.zip", FileMode.Open),
					ZipArchiveMode.Update))
				{
					var entry = archive.GetEntry("foo.txt");
					Assert.IsNotNull(entry);

					var stream = entry.Open();

					try {
						stream = entry.Open();
					} catch (global::System.IO.IOException ex) {
						return;
					}

					Assert.Fail();
				}
			} finally {
				File.Delete ("test.zip");
			}
		}


		[Test]
		public void ZipOpenCloseAndReopenEntry()
		{
			File.Copy("archive.zip", "test.zip", overwrite: true);
			using (var archive = new ZipArchive(File.Open("test.zip", FileMode.Open),
				ZipArchiveMode.Update))
			{
				var entry = archive.GetEntry("foo.txt");
				Assert.IsNotNull(entry);

				var stream = entry.Open();
				stream.Dispose();
				stream = entry.Open();
			}

			File.Delete ("test.zip");
		}

		[Test]
		public void ZipGetEntryDeleteReadMode()
		{
			File.Copy("archive.zip", "delete.zip", overwrite: true);
			using (var archive = new ZipArchive(File.Open("delete.zip", FileMode.Open),
				ZipArchiveMode.Update))
			{
				var entry = archive.GetEntry("foo.txt");
				Assert.IsNotNull(entry);

				entry.Delete();
			}

			using (var archive = new ZipArchive(File.Open("delete.zip", FileMode.Open),
				ZipArchiveMode.Read))
			{
				var entry = archive.GetEntry("foo.txt");
				Assert.IsNull(entry);
			}

			File.Delete ("delete.zip");
		}

		[Test]
		public void ZipGetEntryDeleteUpdateMode()
		{
			File.Copy("archive.zip", "delete.zip", overwrite: true);
			using (var archive = new ZipArchive(File.Open("delete.zip", FileMode.Open),
				ZipArchiveMode.Update))
			{
				var entry = archive.GetEntry("foo.txt");
				Assert.IsNotNull(entry);

				entry.Delete();
			}

			using (var archive = new ZipArchive(File.Open("delete.zip", FileMode.Open),
				ZipArchiveMode.Read))
			{
				var entry = archive.GetEntry("foo.txt");
				Assert.IsNull(entry);
			}

			File.Delete ("delete.zip");
		}

		[Test]
		public void ZipCreateArchive()
		{
			using (var archive = new ZipArchive(File.Open("create.zip", FileMode.Create),
				ZipArchiveMode.Create))
			{
				var dir = archive.CreateEntry("foobar/");

				var entry = archive.CreateEntry("foo.txt");
				using (var stream = entry.Open())
				{
					using (var streamWriter = new StreamWriter(stream))
						streamWriter.Write("foo");
				}
			}

			using (var archive = new ZipArchive(File.Open("create.zip", FileMode.Open),
				ZipArchiveMode.Read))
			{
				Assert.IsNotNull(archive.GetEntry("foobar/"));

				var entry = archive.GetEntry("foo.txt");
				Assert.IsNotNull(entry);

				var streamReader = new StreamReader(entry.Open());
				var text = streamReader.ReadToEnd();

				Assert.AreEqual("foo", text);
			}

			File.Delete ("create.zip");
		}

		[Test]
		public void ZipEnumerateEntriesReadMode()
		{
			File.Copy("archive.zip", "test.zip", overwrite: true);
			using (var archive = new ZipArchive(File.Open("test.zip", FileMode.Open),
				ZipArchiveMode.Read))
			{
				var entries = archive.Entries;
				Assert.AreEqual(5, entries.Count);

				Assert.AreEqual("bar.txt", entries[0].FullName);
				Assert.AreEqual("foo.txt", entries[1].FullName);
				Assert.AreEqual("foobar/", entries[2].FullName);
				Assert.AreEqual("foobar/bar.txt", entries[3].FullName);
				Assert.AreEqual("foobar/foo.txt", entries[4].FullName);
			}

			File.Delete ("test.zip");
		}

		[Test]
		public void ZipEnumerateEntriesUpdateMode()
		{
			File.Copy("archive.zip", "test.zip", overwrite: true);
			using (var archive = new ZipArchive(File.Open("test.zip", FileMode.Open),
				ZipArchiveMode.Read))
			{
				var entries = archive.Entries;
				Assert.AreEqual(5, entries.Count);

				Assert.AreEqual("bar.txt", entries[0].FullName);
				Assert.AreEqual("foo.txt", entries[1].FullName);
				Assert.AreEqual("foobar/", entries[2].FullName);
				Assert.AreEqual("foobar/bar.txt", entries[3].FullName);
				Assert.AreEqual("foobar/foo.txt", entries[4].FullName);
			}

			File.Delete ("test.zip");
		}

		[Test]
		public void ZipEnumerateEntriesCreateMode()
		{
			File.Copy("archive.zip", "test.zip", overwrite: true);
			using (var archive = new ZipArchive(File.Open("test.zip", FileMode.Open),
				ZipArchiveMode.Create))
			{
				try {
					archive.Entries.ToList();
				} catch(NotSupportedException ex) {
					return;
				}
				
				Assert.Fail();				
			}

			File.Delete ("test.zip");
		}

		[Test]
		public void ZipUpdateEmptyArchive()
		{
			File.WriteAllText("empty.zip", string.Empty);
			using (var archive = new ZipArchive(File.Open("empty.zip", FileMode.Open),
				ZipArchiveMode.Update))
			{
			}
			File.Delete ("empty.zip");
		}
	}
}
