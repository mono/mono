//
// ZipTest.cs
//
// Author:
//       João Matos <joao.matos@xamarin.com>
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

using MonoTests.Helpers;

namespace MonoTests.System.IO.Compression.FileSystem
{
	[TestFixture]
	public class ZipArchiveTests
	{
		string tmpFile;
		[SetUp]
		public void SetUp ()
		{
			tmpFile = Path.GetTempFileName ();
		}

		[TearDown]
		public void Dispose()
		{
			File.Delete (tmpFile);
		}

		[Test]
		public void ZipCreateFromDirectory()
		{
			if (File.Exists (tmpFile))
				File.Delete (tmpFile);

			ZipFile.CreateFromDirectory (TestResourceHelper.GetFullPathOfResource ("Test/resources/foo"), tmpFile);
			Assert.IsTrue(File.Exists(tmpFile));

			using (var archive = new ZipArchive (File.Open (tmpFile, FileMode.Open),
				ZipArchiveMode.Read))
			{
				Assert.IsNotNull (archive.GetEntry ("foo.txt"));
				Assert.IsNotNull (archive.GetEntry ("bar.txt"));

				Assert.IsNotNull (archive.GetEntry ("foobar/foo.txt"));
				Assert.IsNotNull (archive.GetEntry ("foobar/bar.txt"));				
			}
		}

		[Test]
		public void ZipCreateFromDirectoryIncludeBase()
		{
			if (File.Exists (tmpFile))
				File.Delete (tmpFile);

			ZipFile.CreateFromDirectory (TestResourceHelper.GetFullPathOfResource ("Test/resources/foo"), tmpFile, CompressionLevel.Fastest,
				includeBaseDirectory: true);
			Assert.IsTrue (File.Exists (tmpFile));

			using (var archive = new ZipArchive (File.Open (tmpFile, FileMode.Open),
				ZipArchiveMode.Read))
			{
				Assert.IsNotNull (archive.GetEntry ("foo/foo.txt"));
				Assert.IsNotNull (archive.GetEntry ("foo/bar.txt"));

				Assert.IsNotNull (archive.GetEntry ("foo/foobar/foo.txt"));
				Assert.IsNotNull (archive.GetEntry ("foo/foobar/bar.txt"));				
			}
		}		

		[Test]
		public void ZipExtractToDirectory()
		{
			var extractDir = Path.Combine (Path.GetTempPath (), "extract");
			if (Directory.Exists (extractDir))
				Directory.Delete (extractDir, true);

			if (File.Exists (tmpFile))
				File.Delete (tmpFile);

			ZipFile.CreateFromDirectory (TestResourceHelper.GetFullPathOfResource ("Test/resources/foo"), tmpFile);

			ZipFile.ExtractToDirectory (tmpFile, extractDir);
			Assert.IsTrue(Directory.Exists (extractDir));

			Assert.IsTrue (File.Exists (Path.Combine (extractDir, "foo.txt")), Path.Combine (extractDir, "foo.txt"));
			Assert.IsTrue (File.Exists (Path.Combine (extractDir, "bar.txt")), Path.Combine (extractDir, "bar.txt"));
			Assert.IsTrue (Directory.Exists (Path.Combine (extractDir, "foobar")), Path.Combine (extractDir, "foobar"));
			Assert.IsTrue (File.Exists (Path.Combine (extractDir, "foobar", "foo.txt")), Path.Combine (extractDir, "foobar", "foo.txt"));
			Assert.IsTrue (File.Exists (Path.Combine (extractDir, "foobar", "bar.txt")), Path.Combine (extractDir, "foobar", "bar.txt"));

			Directory.Delete (extractDir, true);
		}

		[Test]
		public void ZipCreateFromEntryChangeTimestamp()
		{
			if (File.Exists (tmpFile))
				File.Delete (tmpFile);

			var file = TestResourceHelper.GetFullPathOfResource ("Test/resources/foo/foo.txt");
			using (var archive = new ZipArchive(File.Open(tmpFile, FileMode.Create),
				ZipArchiveMode.Update))
			{
				archive.CreateEntryFromFile(file, file);
			}

			var date = File.GetLastWriteTimeUtc(file);

			using (var archive = new ZipArchive (File.Open (tmpFile, FileMode.Open),
				ZipArchiveMode.Read))
			{
				var entry = archive.GetEntry (file);
				Assert.IsNotNull (entry);
				var lastWriteTimeUtc = entry.LastWriteTime.ToUniversalTime ();
				Assert.AreEqual (date.Year, lastWriteTimeUtc.Year);
				Assert.AreEqual (date.Month, lastWriteTimeUtc.Month);
				Assert.AreEqual (date.Day, lastWriteTimeUtc.Day);
			}
		}
	}
}
