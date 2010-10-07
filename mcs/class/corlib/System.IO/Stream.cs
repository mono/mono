//
// System.IO.Stream.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Miguel de Icaza (miguel@ximian.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2001, 2002 Ximian, Inc.  http://www.ximian.com
// (c) 2004 Novell, Inc. (http://www.novell.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Threading;
using System.Runtime.Remoting.Messaging;
using System.Runtime.InteropServices;

namespace System.IO
{
	[Serializable]
	[ComVisible (true)]
#if NET_2_1
	public abstract class Stream : IDisposable
#else
	public abstract class Stream : MarshalByRefObject, IDisposable
#endif
	{
		public static readonly Stream Null = new NullStream ();

		protected Stream ()
		{
		}

		public abstract bool CanRead
		{
			get;
		}

		public abstract bool CanSeek
		{
			get;
		}

		public abstract bool CanWrite
		{
			get;
		}

		[ComVisible (false)]
		public virtual bool CanTimeout {
			get {
				return false;
			}
		}

		public abstract long Length
		{
			get;
		}

		public abstract long Position
		{
			get;
			set;
		}


		// 2.0 version of Dispose.
		public void Dispose ()
		{
			Close ();
		}

		// 2.0 version of Dispose.
		protected virtual void Dispose (bool disposing)
		{
			// nothing.
		}

		//
		// 2.0 version of Close (): calls Dispose (true)
		//
		public virtual void Close ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		[ComVisible (false)]
		public virtual int ReadTimeout {
			get {
				throw new InvalidOperationException ("Timeouts are not supported on this stream.");
			}
			set {
				throw new InvalidOperationException ("Timeouts are not supported on this stream.");
			}
		}

		[ComVisible (false)]
		public virtual int WriteTimeout {
			get {
				throw new InvalidOperationException ("Timeouts are not supported on this stream.");
			}
			set {
				throw new InvalidOperationException ("Timeouts are not supported on this stream.");
			}
		}

		public static Stream Synchronized (Stream stream)
		{
			return new SynchronizedStream (stream);
		}

		[Obsolete ("CreateWaitHandle is due for removal.  Use \"new ManualResetEvent(false)\" instead.")]
		protected virtual WaitHandle CreateWaitHandle()
		{
			return new ManualResetEvent (false);
		}
		
		public abstract void Flush ();

		public abstract int Read ([In,Out] byte[] buffer, int offset, int count);

		public virtual int ReadByte ()
		{
			byte[] buffer = new byte [1];

			if (Read (buffer, 0, 1) == 1)
				return buffer [0];
			
			return -1;
		}

		public abstract long Seek (long offset, SeekOrigin origin);

		public abstract void SetLength (long value);

		public abstract void Write (byte[] buffer, int offset, int count);

		public virtual void WriteByte (byte value)
		{
			byte[] buffer = new byte [1];

			buffer [0] = value;

			Write (buffer, 0, 1);
		}

		public virtual IAsyncResult
		BeginRead (byte [] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (!CanRead)
				throw new NotSupportedException ("This stream does not support reading");

			// Creating a class derived from Stream that doesn't override BeginRead
			// shows that it actually calls Read and does everything synchronously.
			// Just put this in the Read override:
			//	Console.WriteLine ("Read");
			// 	Console.WriteLine (Environment.StackTrace);
			//	Thread.Sleep (10000);
			//	return 10;

			StreamAsyncResult result = new StreamAsyncResult (state);
			try {
				int nbytes = Read (buffer, offset, count);
				result.SetComplete (null, nbytes);
			} catch (Exception e) {
				result.SetComplete (e, 0);
			}

			if (callback != null)
				callback (result);

			return result;
		}

//		delegate void WriteDelegate (byte [] buffer, int offset, int count);

		public virtual IAsyncResult
		BeginWrite (byte [] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			if (!CanWrite)
				throw new NotSupportedException ("This stream does not support writing");
	
			// Creating a class derived from Stream that doesn't override BeginWrite
			// shows that it actually calls Write and does everything synchronously except
			// when invoking the callback, which is done from the ThreadPool.
			// Just put this in the Write override:
			// 	Console.WriteLine ("Write");
			// 	Console.WriteLine (Environment.StackTrace);
			//	Thread.Sleep (10000);

			StreamAsyncResult result = new StreamAsyncResult (state);
			try {
				Write (buffer, offset, count);
				result.SetComplete (null);
			} catch (Exception e) {
				result.SetComplete (e);
			}

			if (callback != null)
				callback.BeginInvoke (result, null, null);

			return result;
		}
		
		public virtual int EndRead (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			StreamAsyncResult result = asyncResult as StreamAsyncResult;
			if (result == null || result.NBytes == -1)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			if (result.Done)
				throw new InvalidOperationException ("EndRead already called.");

			result.Done = true;
			if (result.Exception != null)
				throw result.Exception;

			return result.NBytes;
		}

		public virtual void EndWrite (IAsyncResult asyncResult)
		{
			if (asyncResult == null)
				throw new ArgumentNullException ("asyncResult");

			StreamAsyncResult result = asyncResult as StreamAsyncResult;
			if (result == null || result.NBytes != -1)
				throw new ArgumentException ("Invalid IAsyncResult", "asyncResult");

			if (result.Done)
				throw new InvalidOperationException ("EndWrite already called.");

			result.Done = true;
			if (result.Exception != null)
				throw result.Exception;
		}

#if MOONLIGHT || NET_4_0
		public void CopyTo (Stream destination)
		{
			CopyTo (destination, 16*1024);
		}

		public void CopyTo (Stream destination, int bufferSize)
		{
			if (destination == null)
				throw new ArgumentNullException ("destination");
			if (!CanRead)
				throw new NotSupportedException ("This stream does not support reading");
			if (!destination.CanWrite)
				throw new NotSupportedException ("This destination stream does not support writing");
			if (bufferSize <= 0)
				throw new ArgumentOutOfRangeException ("bufferSize");

			var buffer = new byte [bufferSize];
			int nread;
			while ((nread = Read (buffer, 0, bufferSize)) != 0)
				destination.Write (buffer, 0, nread);
		}

		protected virtual void ObjectInvariant ()
		{
		}
#endif
	}

	class NullStream : Stream
	{
		public override bool CanRead
		{
			get {
				return true;
			}
		}

		public override bool CanSeek
		{
                        get {
                                return true;
                        }
                }

                public override bool CanWrite
		{
                        get {
                                return true;
                        }
                }

		public override long Length
		{
			get {
				return 0;
			}
		}

		public override long Position
		{
			get {
				return 0;
			}
			set {
			}
		}

		public override void Flush ()
		{
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return 0;
		}

		public override int ReadByte ()
		{
			return -1;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			return 0;
		}

		public override void SetLength (long value)
		{
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
		}

		public override void WriteByte (byte value)
		{
		}
	}

	class SynchronizedStream : Stream {
		Stream source;
		object slock;
			
		internal SynchronizedStream (Stream source)
		{
			this.source = source;
			slock = new object ();
		}
		
		public override bool CanRead
		{
			get {
				lock (slock)
					return source.CanRead;
			}
		}

		public override bool CanSeek
		{
                        get {
				lock (slock)
					return source.CanSeek;
                        }
                }

                public override bool CanWrite
		{
                        get {
				lock (slock)
					return source.CanWrite;
                        }
                }

		public override long Length
		{
			get {
				lock (slock)
					return source.Length;
			}
		}

		public override long Position
		{
			get {
				lock (slock)
					return source.Position;
			}
			set {
				lock (slock)
					source.Position = value;
			}
		}

		public override void Flush ()
		{
			lock (slock)
				source.Flush ();
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			lock (slock)
				return source.Read (buffer, offset, count);
		}

		public override int ReadByte ()
		{
			lock (slock)
				return source.ReadByte ();
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			lock (slock)
				return source.Seek (offset, origin);
		}

		public override void SetLength (long value)
		{
			lock (slock)
				source.SetLength (value);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			lock (slock)
				source.Write (buffer, offset, count);
		}

		public override void WriteByte (byte value)
		{
			lock (slock)
				source.WriteByte (value);
		}
	}
}
