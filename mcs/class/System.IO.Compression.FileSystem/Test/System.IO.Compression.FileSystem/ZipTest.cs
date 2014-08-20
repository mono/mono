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

namespace MonoTests.System.IO.Compression.FileSystem
{
	[TestFixture]
	public class ZipArchiveTests
	{
		[TearDown]
		public void Dispose()
		{
			File.Delete ("foo.zip");
		}

		[Test]
		public void ZipCreateFromDirectory()
		{
			if (File.Exists ("foo.zip"))
				File.Delete ("foo.zip");

			ZipFile.CreateFromDirectory ("foo", "foo.zip");
			Assert.IsTrue(File.Exists("foo.zip"));

			using (var archive = new ZipArchive (File.Open ("foo.zip", FileMode.Open),
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
			if (File.Exists ("foo.zip"))
				File.Delete ("foo.zip");

			ZipFile.CreateFromDirectory ("foo", "foo.zip", CompressionLevel.Fastest,
				includeBaseDirectory: true);
			Assert.IsTrue (File.Exists ("foo.zip"));

			using (var archive = new ZipArchive (File.Open ("foo.zip", FileMode.Open),
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
			if (Directory.Exists ("extract"))
				Directory.Delete ("extract", true);

			if (File.Exists ("foo.zip"))
				File.Delete ("foo.zip");

			ZipFile.CreateFromDirectory ("foo", "foo.zip");

			ZipFile.ExtractToDirectory ("foo.zip", "extract");
			Assert.IsTrue(Directory.Exists ("extract"));

			Assert.IsTrue (File.Exists ("extract/foo.txt"));
			Assert.IsTrue (File.Exists ("extract/bar.txt"));
			Assert.IsTrue (Directory.Exists ("extract/foobar"));
			Assert.IsTrue (File.Exists ("extract/foobar/foo.txt"));
			Assert.IsTrue (File.Exists ("extract/foobar/bar.txt"));

			Directory.Delete ("extract", true);
		}
	}
}
