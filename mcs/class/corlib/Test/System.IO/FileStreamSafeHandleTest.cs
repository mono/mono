// FileStreamSafeHandleTests.cs - Test Cases for System.IO.FileStream class
//
// Authors:
// 	Marcos Henrich (marcos.henrich@xamarin.com)
//
// Copyright 2015 Xamarin Inc (http://www.xamarin.com).
// 

using System;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
using NUnit.Framework;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class FileStreamWithClosedSafeHandleTests
	{
#if !MOBILE
		private FileStream GetFileStreamWithClosedHandle ()
		{
			var fs1 = new FileStream ("test2", FileMode.OpenOrCreate);
			var fs2 = new FileStream (fs1.SafeFileHandle, FileAccess.Read);
			fs1.Close ();

			return fs2;
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void GetLength ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			var l = fs.Length;
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void GetPosition ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			var p = fs.Position;
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void SetPosition ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.Position = 3;
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void GetAccessControl ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.GetAccessControl ();
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void SetAccessControl ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.SetAccessControl (new FileSecurity ());
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Flush ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.Flush (false);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void SetLength ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.SetLength (20);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Read ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.Read (new byte [2], 0, 1);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Seek ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.Seek (0, SeekOrigin.Begin);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Write ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.Write (new byte [2], 0, 1);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void ReadByte ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.ReadByte ();
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void WriteByte ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.WriteByte (0);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void Lock ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.Lock (0, 1);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void UnLock ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.Unlock (0, 1);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void ReadAsync ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.ReadAsync (new byte [2], 0, 1);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void WriteAsync ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.WriteAsync (new byte [2], 0, 1);
		}

		[Test]
		[ExpectedException (typeof (ObjectDisposedException))]
		public void FlushAsync ()
		{
			var fs = GetFileStreamWithClosedHandle ();

			fs.FlushAsync (new CancellationToken ());
		}
#endif
	}
}
