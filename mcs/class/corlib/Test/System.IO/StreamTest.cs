//
// StreamTest.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright 2011 Xamarin, Inc (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.IO;

using NUnit.Framework;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class StreamTest
	{
		class MockStream : Stream
		{
			bool canRead, canSeek, canWrite;
			public event Action OnFlush;
			public event Func<byte[], int, int, int> OnRead;
			public Action<byte[], int, int> OnWrite;

			public MockStream (bool canRead, bool canSeek, bool canWrite)
			{
				this.canRead = canRead;
				this.canSeek = canSeek;
				this.canWrite = canWrite;
			}

			public override bool CanRead {
				get {
					return canRead;
				}
			}

			public override bool CanSeek {
				get {
					return canSeek;
				}
			}

			public override bool CanWrite {
				get {
					return canWrite;
				}
			}

			public override void Flush ()
			{
				if (OnFlush != null)
					OnFlush ();
			}

			public override long Length {
				get { throw new NotImplementedException (); }
			}

			public override long Position {
				get {
					throw new NotImplementedException ();
				}
				set {
					throw new NotImplementedException ();
				}
			}

			public override int Read (byte[] buffer, int offset, int count)
			{
				if (OnRead != null)
					return OnRead (buffer, offset, count);

				return -1;
			}

			public override long Seek (long offset, SeekOrigin origin)
			{
				throw new NotImplementedException ();
			}

			public override void SetLength (long value)
			{
				throw new NotImplementedException ();
			}

			public override void Write (byte[] buffer, int offset, int count)
			{
				if (OnWrite != null)
					OnWrite (buffer, offset, count);
			}
		}

#if NET_4_5
		[Test]
		public void FlushAsync ()
		{
			bool called = false;
			var ms = new MockStream (false, false, false);
			ms.OnFlush += () => { called = true; };
			var t = ms.FlushAsync ();
			Assert.IsTrue (t.Wait (1000), "#1");
			Assert.IsTrue (called, "#2");
		}

		[Test]
		public void FlushAsync_Exception ()
		{
			var ms = new MockStream (false, false, false);
			ms.OnFlush += () => { throw new ApplicationException (); };
			var t = ms.FlushAsync ();
			try {
				t.Wait (1000);
				Assert.Fail ();
			} catch (AggregateException) {
			}
		}

		[Test]
		public void ReadAsync ()
		{
			bool called = false;
			var buffer = new byte[4];
			var ms = new MockStream (true, false, false);
			ms.OnRead += (b, p, c) => { called = true; return 2; };
			var t = ms.ReadAsync (buffer, 0, 4);

			Assert.IsTrue (t.Wait (1000), "#1");
			Assert.IsTrue (called, "#2");
			Assert.AreEqual (2, t.Result, "#3");
		}

		[Test]
		public void ReadAsync_Exception ()
		{
			var buffer = new byte[4];
			var ms = new MockStream (true, false, false);
			ms.OnRead += (b, p, c) => { throw new ApplicationException (); };
			var t = ms.ReadAsync (buffer, 0, 4);

			try {
				t.Wait (1000);
				Assert.Fail ();
			} catch (AggregateException) {
			}
		}

		[Test]
		public void WriteAsync ()
		{
			bool called = false;
			var buffer = new byte[4];
			var ms = new MockStream (false, false, true);
			ms.OnWrite += (b, p, c) => { called = true; };
			var t = ms.WriteAsync (buffer, 0, 4);

			Assert.IsTrue (t.Wait (1000), "#1");
			Assert.IsTrue (called, "#2");
		}

		[Test]
		public void WriteAsync_Exception ()
		{
			var buffer = new byte[4];
			var ms = new MockStream (false, false, true);
			ms.OnWrite += (b, p, c) => { throw new ApplicationException (); };
			var t = ms.WriteAsync (buffer, 0, 4);

			try {
				t.Wait (1000);
				Assert.Fail ();
			} catch (AggregateException) {
			}
		}
#endif
	}
}
