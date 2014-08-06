//
// ZipFile.cs
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
using System.Text;

namespace System.IO.Compression
{
	public static class ZipFile
	{
		public static void CreateFromDirectory (
			string sourceDirectoryName, string destinationArchiveFileName)
		{
			CreateFromDirectory (sourceDirectoryName, destinationArchiveFileName,
				CompressionLevel.Fastest, includeBaseDirectory: false);
		}

		public static void CreateFromDirectory (
			string sourceDirectoryName, string destinationArchiveFileName,
			CompressionLevel compressionLevel, bool includeBaseDirectory)
		{
			CreateFromDirectory (sourceDirectoryName, destinationArchiveFileName,
				CompressionLevel.Fastest, includeBaseDirectory, Encoding.UTF8);
		}

		public static void CreateFromDirectory (
			string sourceDirectoryName,
			string destinationArchiveFileName,
			CompressionLevel compressionLevel,
			bool includeBaseDirectory,
			Encoding entryNameEncoding)
		{
			if (sourceDirectoryName == null)
				throw new ArgumentNullException ("sourceDirectoryName");

			if (destinationArchiveFileName == null)
				throw new ArgumentNullException ("destinationArchiveFileName");

			if (string.IsNullOrWhiteSpace (sourceDirectoryName))
				throw new ArgumentException ("sourceDirectoryName");
				
			if (string.IsNullOrWhiteSpace (destinationArchiveFileName))
				throw new ArgumentException ("destinationArchiveFileName");

			if (entryNameEncoding == Encoding.Unicode ||
			    entryNameEncoding == Encoding.UTF32 ||
			    entryNameEncoding == Encoding.UTF7)
				throw new ArgumentException ("entryNameEncoding");

			if (entryNameEncoding == null)
				entryNameEncoding = Encoding.UTF8;

			if (!Directory.Exists (sourceDirectoryName))
				throw new DirectoryNotFoundException ("sourceDirectoryName is invalid or does not exist");

			var sourceDir = new DirectoryInfo (Path.GetFullPath (sourceDirectoryName));

			string fullBaseName = sourceDir.FullName;
			if (includeBaseDirectory && sourceDir.Parent != null)
				fullBaseName = sourceDir.Parent.FullName;

			bool hasEntries = false;
			char[] separators = new char[] {
				Path.DirectorySeparatorChar,
				Path.AltDirectorySeparatorChar
			};

			using (var zipFile = ZipFile.Open (destinationArchiveFileName, ZipArchiveMode.Create,
				entryNameEncoding)) {
				var entries = sourceDir.EnumerateFileSystemInfos ("*", SearchOption.AllDirectories);
				foreach (var entry in entries) {
					hasEntries = true;

					int length = entry.FullName.Length - fullBaseName.Length;
					string entryName = entry.FullName.Substring(fullBaseName.Length, length);

					entryName = entryName.TrimStart(separators);

					if (entry is FileInfo)
						zipFile.CreateEntryFromFile (entry.FullName, entryName, compressionLevel);
					else
						zipFile.CreateEntry (entryName + Path.DirectorySeparatorChar);
				}

				// Create the base directory even if we had no entries
				if (includeBaseDirectory && !hasEntries)
					zipFile.CreateEntry(sourceDir.Name + Path.DirectorySeparatorChar);
			}
		}

		public static void ExtractToDirectory (
			string sourceArchiveFileName, string destinationDirectoryName)
		{
			ExtractToDirectory (sourceArchiveFileName, destinationDirectoryName,
				Encoding.UTF8);
		}

		public static void ExtractToDirectory (
			string sourceArchiveFileName, string destinationDirectoryName,
			Encoding entryNameEncoding)
		{
			if (sourceArchiveFileName == null)
				throw new ArgumentNullException ("sourceArchiveFileName");

			using (ZipArchive zipArchive = ZipFile.Open(sourceArchiveFileName,
				ZipArchiveMode.Read, entryNameEncoding))
			{
				zipArchive.ExtractToDirectory(destinationDirectoryName);
			}
		}

		public static ZipArchive Open (
			string archiveFileName, ZipArchiveMode mode)
		{
			return Open (archiveFileName, mode);
		}

		public static ZipArchive Open (
			string archiveFileName, ZipArchiveMode mode,
			Encoding entryNameEncoding)
		{
			if (archiveFileName == null)
				throw new ArgumentNullException ("archiveFileName");

			if (string.IsNullOrWhiteSpace (archiveFileName))
				throw new ArgumentException ("archiveFileName");

			FileStream stream;

			switch (mode) {
			case ZipArchiveMode.Read:
				if (!File.Exists (archiveFileName))
					throw new FileNotFoundException ();
				stream = new FileStream (archiveFileName, FileMode.Open, FileAccess.Read,
					FileShare.Read);
				break;
			case ZipArchiveMode.Create:
				if (File.Exists (archiveFileName))
					throw new IOException ("mode is set to Create but the file already exists");
				stream = new FileStream (archiveFileName, FileMode.CreateNew, FileAccess.Write);
				break;
			case ZipArchiveMode.Update:
				stream = new FileStream (archiveFileName, FileMode.OpenOrCreate,
					FileAccess.ReadWrite);
				break;
			default:
				throw new ArgumentOutOfRangeException ();
			}

			return new ZipArchive (stream, mode, false, entryNameEncoding);
		}

		public static ZipArchive OpenRead (string archiveFileName)
		{
			return ZipFile.Open (archiveFileName, ZipArchiveMode.Read);
		}
	}
}

