//
// ZipFileExtensions.cs
//
// Author:
//       João Matos <joao.matos@xamarin.com>
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

namespace System.IO.Compression
{
	public static class ZipFileExtensions
	{
		public static ZipArchiveEntry CreateEntryFromFile (
			this ZipArchive destination, string sourceFileName,
			string entryName)
		{
			return CreateEntryFromFile (destination, sourceFileName, entryName,
				CompressionLevel.Fastest);
		}

		public static ZipArchiveEntry CreateEntryFromFile (
			this ZipArchive destination, string sourceFileName,
			string entryName, CompressionLevel compressionLevel)
		{
			if (destination == null)
				throw new ArgumentNullException ("destination");

			if (sourceFileName == null)
				throw new ArgumentNullException ("sourceFileName");

			if (entryName == null)
				throw new ArgumentNullException ("entryName");

			ZipArchiveEntry entry;
			using (Stream stream = File.Open (sourceFileName, FileMode.Open,
				FileAccess.Read, FileShare.Read))
			{
				var zipArchiveEntry = destination.CreateEntry (entryName, compressionLevel);

				using (Stream entryStream = zipArchiveEntry.Open ())
					stream.CopyTo (entryStream);

				entry = zipArchiveEntry;
			}

			return entry;
		}

		public static void ExtractToDirectory (
			this ZipArchive source,
			string destinationDirectoryName)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			if (destinationDirectoryName == null)
				throw new ArgumentNullException ("destinationDirectoryName");

			var destDirInfo = Directory.CreateDirectory (destinationDirectoryName);
			string fullName = destDirInfo.FullName;

			foreach (var zipEntry in source.Entries)
			{
				var fullPath = Path.GetFullPath (Path.Combine (fullName, zipEntry.FullName));

				var isFileName = Path.GetFileName (fullPath).Length != 0;
				var dirPath = isFileName ? Path.GetDirectoryName (fullPath) : fullPath;
				Directory.CreateDirectory (dirPath);

				if (isFileName)
					zipEntry.ExtractToFile (fullPath, false);
			}
		}

		public static void ExtractToFile (
			this ZipArchiveEntry source,
			string destinationFileName)
		{
			ExtractToFile (source, destinationFileName, overwrite: false);
		}

		public static void ExtractToFile (
			this ZipArchiveEntry source, string destinationFileName,
			bool overwrite)
		{
			if (source == null)
				throw new ArgumentNullException ("source");

			if (destinationFileName == null)
				throw new ArgumentNullException ("destinationFileName");
				
			var mode = overwrite ? FileMode.Create : FileMode.CreateNew;
			using (var stream = File.Open (destinationFileName, mode, FileAccess.Write))
			{
				using (var stream2 = source.Open ())
					stream2.CopyTo(stream);
			}

			File.SetLastWriteTime(destinationFileName, source.LastWriteTime.DateTime);
		}
	}
}

