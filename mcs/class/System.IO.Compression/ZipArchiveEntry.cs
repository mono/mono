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

using SharpCompress.Archive;

namespace System.IO.Compression
{
	internal class ZipArchiveEntryStream : Stream, IDisposable
	{
		private readonly ZipArchiveEntry entry;
		private readonly Stream stream;

		public override bool CanRead {
			get { 
				return stream.CanRead;
			}
		}

		public override bool CanSeek {
			get {
				return stream.CanSeek;
			}
		}

		public override bool CanWrite {
			get {
				return stream.CanWrite;
			}
		}

		public override long Length {
			get {
				return stream.Length;
			}
		}

		public override long Position {
			get {
				return stream.Position;
			}
			set {
				stream.Position = value;
			}
		}

		public ZipArchiveEntryStream(ZipArchiveEntry entry, Stream stream)
		{
			this.entry = entry;
			this.stream = stream;
		}

		public override void Flush ()
		{
			stream.Flush();
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			return stream.Seek(offset, origin);
		}

		public override void SetLength (long value)
		{
			stream.SetLength(value);
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return stream.Read(buffer, offset, count);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			stream.Write(buffer, offset, count);
		}

		public new void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
			base.Dispose();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) 
			{
				entry.openStream = null;
				stream.Dispose();
			}
		}
	}

	public class ZipArchiveEntry
	{
		readonly SharpCompress.Archive.Zip.ZipArchiveEntry entry;
		internal ZipArchiveEntryStream openStream;
		private bool wasDeleted;

		internal ZipArchiveEntry(ZipArchive	archive, SharpCompress.Archive.Zip.ZipArchiveEntry entry)
		{
			if (archive == null)
				throw new ArgumentNullException("archive");

			if (entry == null)
				throw new ArgumentNullException("entry");

			Archive = archive;
			this.entry = entry;
		}

		public ZipArchive Archive {
			get;
			private set;
		}

		public long CompressedLength {
			get {
				if (Archive.Mode == ZipArchiveMode.Create)
					throw new InvalidOperationException("Property cannot be retrieved when the mode is set to Create");

				return entry.CompressedSize;
			}
		}

		public string FullName {
			get { return entry.Key; }
		}

		public DateTimeOffset LastWriteTime {
			get { return entry.LastModifiedTime.GetValueOrDefault(); }
			set { entry.LastModifiedTime = value.DateTime; }
		}

		public long Length {
			get {
				if (Archive.Mode == ZipArchiveMode.Create)
					throw new InvalidOperationException("Property cannot be retrieved when the mode is set to Create");

				return entry.Size;
			}
		}

		public string Name {
			get { return Path.GetFileName(entry.Key); }
		}

		public void Delete()
		{
			if (Archive.disposed)
				throw new ObjectDisposedException("The zip archive for this entry has been disposed.");

			if (Archive.Mode !=	ZipArchiveMode.Update)
				throw new NotSupportedException("The zip archive for this entry was opened in a mode other than Update.");

			if (openStream != null)
				throw new IOException("The entry is already open for reading or writing.");

			wasDeleted = true;
			Archive.zipFile.RemoveEntry(entry);
		}

		public Stream Open()
		{
			if (Archive.disposed)
				throw new ObjectDisposedException("The zip archive for this entry has been disposed.");

			if (openStream != null && Archive.Mode == ZipArchiveMode.Update)
				throw new IOException("The entry is already currently open for writing.");

			if (wasDeleted)
				throw new IOException("The entry has been deleted from the archive.");

			if (Archive.Mode == ZipArchiveMode.Create && openStream != null)
				throw new IOException("The archive for this entry was opened with the Create mode, and this entry has already been written to.");

			openStream = new ZipArchiveEntryStream(this, entry.OpenEntryStream());

			return openStream;
		}
	}
}
