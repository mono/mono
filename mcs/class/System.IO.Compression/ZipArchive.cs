//
// ZipArchiveEntry.cs
//
// Author:
//       Joao Matos <joao.matos@xamarin.com>
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SharpCompress.Common;

namespace System.IO.Compression
{
	public class ZipArchive : IDisposable
	{
		internal Stream stream;
		internal readonly bool leaveStreamOpen;
		internal readonly ZipArchiveMode mode;
		internal Encoding entryNameEncoding;
		internal bool disposed;
		internal Dictionary<string, ZipArchiveEntry> entries; 
		internal SharpCompress.Archive.Zip.ZipArchive zipFile;

		public ZipArchive (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			this.stream = stream;
			mode = ZipArchiveMode.Read;
			CreateZip(stream, mode);
		}

		public ZipArchive (Stream stream, ZipArchiveMode mode)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			this.stream = stream;
			this.mode = mode;
			CreateZip(stream, mode);
		}

		public ZipArchive (Stream stream, ZipArchiveMode mode, bool leaveOpen)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			this.stream = stream;
			this.mode = mode;
			leaveStreamOpen = leaveOpen;
			CreateZip(stream, mode);
		}

		public ZipArchive (Stream stream, ZipArchiveMode mode, bool leaveOpen, Encoding entryNameEncoding)
		{
			if (stream == null)
				throw new ArgumentNullException("stream");

			this.stream = stream;
			this.mode = mode;
			leaveStreamOpen = leaveOpen;
			this.entryNameEncoding = entryNameEncoding;
			CreateZip(stream, mode);
		}

		private void CreateZip(Stream stream, ZipArchiveMode mode)
		{
			if (mode != ZipArchiveMode.Read && mode != ZipArchiveMode.Create && mode != ZipArchiveMode.Update)
				throw new ArgumentOutOfRangeException("mode");

			// If the mode parameter is set to Read, the stream must support reading.
			if (mode == ZipArchiveMode.Read && !stream.CanRead)
				throw new ArgumentException("Stream must support reading for Read archive mode");

			// If the mode parameter is set to Create, the stream must support writing.
			if (mode == ZipArchiveMode.Create && !stream.CanWrite)
				throw new ArgumentException("Stream must support writing for Create archive mode");

			// If the mode parameter is set to Update, the stream must support reading, writing, and seeking.
			if (mode == ZipArchiveMode.Update && (!stream.CanRead || !stream.CanWrite || !stream.CanSeek))
				throw new ArgumentException("Stream must support reading, writing and seeking for Update archive mode");

			try {
				zipFile = mode != ZipArchiveMode.Create && stream.Length != 0
					? SharpCompress.Archive.Zip.ZipArchive.Open(stream)
					: SharpCompress.Archive.Zip.ZipArchive.Create();
			} catch (Exception e) {
				throw new InvalidDataException("The contents of the stream are not in the zip archive format.", e);
			}

			entries = new Dictionary<string, ZipArchiveEntry>();
			if (Mode != ZipArchiveMode.Create) {
				foreach (var entry in zipFile.Entries) {
					var zipEntry = new ZipArchiveEntry(this, entry);
					entries[entry.Key] = zipEntry;
				}
			}
		}

		public ReadOnlyCollection<ZipArchiveEntry> Entries {
			get {
				if (disposed)
					throw new ObjectDisposedException("The zip archive has been disposed.");

				if (Mode == ZipArchiveMode.Create)
					throw new NotSupportedException("Cannot access entries in Create mode.");

				if (zipFile == null)
					throw new InvalidDataException("The zip archive is corrupt, and its entries cannot be retrieved.");

				if (entries == null)
					return new ReadOnlyCollection<ZipArchiveEntry>(new List<ZipArchiveEntry>());

				return new ReadOnlyCollection<ZipArchiveEntry>(entries.Values.ToList());
			}
		}

		public ZipArchiveMode Mode {
			get {
				if (disposed)
					throw new ObjectDisposedException("The zip archive has been disposed.");

				return mode;
			}
		}

		public ZipArchiveEntry CreateEntry (string entryName)
		{
			if (disposed)
				throw new ObjectDisposedException("The zip archive has been disposed.");

			return CreateEntry(entryName, CompressionLevel.Optimal);
		}

		public ZipArchiveEntry CreateEntry (string entryName, CompressionLevel compressionLevel)
		{
			if (disposed)
				throw new ObjectDisposedException("The zip archive has been disposed.");

			if (entryName == string.Empty)
				throw new ArgumentException("Entry name cannot be empty.");

			if (entryName == null)
				throw new ArgumentNullException("entryName");

			if (mode != ZipArchiveMode.Create && mode != ZipArchiveMode.Update)
				throw new NotSupportedException("The zip archive does not support writing.");

			if (zipFile == null)
				throw new InvalidDataException("The zip archive is corrupt, and its entries cannot be retrieved.");

			var memoryStream = new MemoryStream();
			var entry = zipFile.AddEntry(entryName, memoryStream);
			var archiveEntry = new ZipArchiveEntry(this, entry);
			entries[entryName] = archiveEntry;

			return archiveEntry;
		}

		public ZipArchiveEntry GetEntry (string entryName)
		{
			if (disposed)
				throw new ObjectDisposedException("The zip archive has been disposed.");

			if (entryName == string.Empty)
				throw new ArgumentException("Entry name cannot be empty.");

			if (entryName == null)
				throw new ArgumentNullException("entryName");

			if (mode != ZipArchiveMode.Read && mode != ZipArchiveMode.Update)
				throw new NotSupportedException("The zip archive does not support reading.");

			if (zipFile == null)
				throw new InvalidDataException("The zip archive is corrupt, and its entries cannot be retrieved.");

			return entries.ContainsKey(entryName) ? entries[entryName] : null;
		}

		private void Save()
		{
			using (var newZip = new MemoryStream()) {
				zipFile.SaveTo(newZip, CompressionType.Deflate, entryNameEncoding ?? Encoding.UTF8);

				stream.SetLength(0);
				stream.Position = 0;
				newZip.Position = 0;
				newZip.CopyTo(stream);
			}
		}

		protected virtual void Dispose (bool disposing)
		{
			if (disposed)
				return;

			if (mode != ZipArchiveMode.Read)
				Save();

			disposed = true;

			if (leaveStreamOpen)
				return;

			if (stream != null) {
				stream.Dispose();
				stream = null;
			}
		}

		public void Dispose ()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}

