//
// ZipFile.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
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
using System.Text;

namespace System.IO.Compression
{
	[MonoTODO]
	public static class ZipFile
	{
		public static void CreateFromDirectory (
			string sourceDirectoryName, string destinationArchiveFileName)
		{
			throw new NotImplementedException ();
		}

		public static void CreateFromDirectory (
			string sourceDirectoryName, string destinationArchiveFileName,
			CompressionLevel compressionLevel, bool includeBaseDirectory)
		{
			throw new NotImplementedException ();
		}

		public static void CreateFromDirectory (
			string sourceDirectoryName,
			string destinationArchiveFileName,
			CompressionLevel compressionLevel,
			bool includeBaseDirectory,
			Encoding entryNameEncoding)
		{
			throw new NotImplementedException ();
		}

		public static void ExtractToDirectory (
			string sourceArchiveFileName, string destinationDirectoryName)
		{
			throw new NotImplementedException ();
		}

		public static void ExtractToDirectory (
			string sourceArchiveFileName, string destinationDirectoryName,
			Encoding entryNameEncoding)
		{
			throw new NotImplementedException ();
		}

		public static ZipArchive Open (
			string archiveFileName, ZipArchiveMode mode)
		{
			throw new NotImplementedException ();
		}

		public static ZipArchive Open (
			string archiveFileName, ZipArchiveMode mode,
			Encoding entryNameEncoding)
		{
			throw new NotImplementedException ();
		}

		public static ZipArchive OpenRead (string archiveFileName)
		{
			throw new NotImplementedException ();
		}
	}
}

