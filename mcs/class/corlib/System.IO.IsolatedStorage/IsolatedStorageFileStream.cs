// System.IO.IsolatedStorage.IsolatedStorageFileStream
//
// Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.

using System;
using System.IO;
using System.Reflection;

namespace System.IO.IsolatedStorage {

	[MonoTODO]
	public class IsolatedStorageFileStream : FileStream
	{
		private static string CreateIsolatedPath (string path)
		{
			string dir = IsolatedStorageInfo.CreateAssemblyFilename (Assembly.GetEntryAssembly());

			string file = dir + "/" + path;

			// Ensure that the file can be created.
			FileInfo fi = new FileInfo (file);
			if (!fi.Directory.Exists)
				fi.Directory.Create ();

			return file;
		}

		public IsolatedStorageFileStream (string path, FileMode mode)
			: base(CreateIsolatedPath (path), mode)
		{
		}	

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access)
			: base (CreateIsolatedPath (path), mode, access)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, FileShare share)
			: base (CreateIsolatedPath (path), mode, access, share)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, FileShare share, int bufferSize)
			: base (CreateIsolatedPath (path), mode, access, share, bufferSize)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, IsolatedStorageFile isf)
			: base (CreateIsolatedPath (path), mode, access, share, bufferSize)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, FileShare share, IsolatedStorageFile isf)
			: base (CreateIsolatedPath (path), mode, access, share)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, FileAccess access, IsolatedStorageFile isf)
			: base (CreateIsolatedPath (path), mode, access)
		{
		}

		public IsolatedStorageFileStream (string path, FileMode mode, IsolatedStorageFile isf)
			: base (CreateIsolatedPath (path), mode)
		{
		}

		public override bool CanRead {
			get {return base.CanRead;}
		}

		public override bool CanSeek {
			get {return base.CanSeek;}
		}

		public override bool CanWrite {
			get {return base.CanWrite;}
		}

		public override IntPtr Handle {
			get {return base.Handle;}
		}

		public override bool IsAsync {
			get {return base.IsAsync;}
		}

		public override long Length {
			get {return base.Length;}
		}

		public override long Position {
			get {return base.Position;}
			set {base.Position = value;}
		}

		public override IAsyncResult BeginRead (byte[] buffer, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
		{
			return base.BeginRead (buffer, offset, numBytes, userCallback, stateObject);
		}

		public override IAsyncResult BeginWrite (byte[] buffer, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
		{
			return base.BeginWrite (buffer, offset, numBytes, userCallback, stateObject);
		}

		public override void Close ()
		{
			base.Close ();
		}

		public override int EndRead (IAsyncResult asyncResult)
		{
			return base.EndRead (asyncResult);
		}

		public override void EndWrite (IAsyncResult asyncResult)
		{
			base.EndWrite (asyncResult);
		}

		public override void Flush ()
		{
			base.Flush ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return base.Read (buffer, offset, count);
		}

		public override int ReadByte ()
		{
			return base.ReadByte ();
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			return base.Seek (offset, origin);
		}

		public override void SetLength (long value)
		{
			base.SetLength (value);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			base.Write (buffer, offset, count);
		}

		public override void WriteByte (byte value)
		{
			base.WriteByte (value);
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);
		}
	}
}

