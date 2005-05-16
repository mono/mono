//
// System.IO/Stream.cs
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
	public abstract class Stream : MarshalByRefObject, IDisposable
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

		public abstract long Length
		{
			get;
		}

		public abstract long Position
		{
			get;
			set;
		}


		public virtual void Close ()
		{
		}

		void IDisposable.Dispose ()
		{
			Close ();
		}

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
		BeginRead (byte [] buffer, int offset, int count, AsyncCallback cback, object state)
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

			if (cback != null)
				cback (result);

			return result;
		}

		delegate void WriteDelegate (byte [] buffer, int offset, int count);

		public virtual IAsyncResult
		BeginWrite (byte [] buffer, int offset, int count, AsyncCallback cback, object state)
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

			if (cback != null)
				cback.BeginInvoke (result, null, null);

			return result;
		}
		
		public virtual int EndRead (IAsyncResult async_result)
		{
			if (async_result == null)
				throw new ArgumentNullException ("async_result");

			StreamAsyncResult result = async_result as StreamAsyncResult;
			if (result == null || result.NBytes == -1)
				throw new ArgumentException ("Invalid IAsyncResult", "async_result");

			if (result.Done)
				throw new InvalidOperationException ("EndRead already called.");

			result.Done = true;
			if (result.Exception != null)
				throw result.Exception;

			return result.NBytes;
		}

		public virtual void EndWrite (IAsyncResult async_result)
		{
			if (async_result == null)
				throw new ArgumentNullException ("async_result");

			StreamAsyncResult result = async_result as StreamAsyncResult;
			if (result == null || result.NBytes != -1)
				throw new ArgumentException ("Invalid IAsyncResult", "async_result");

			if (result.Done)
				throw new InvalidOperationException ("EndWrite already called.");

			result.Done = true;
			if (result.Exception != null)
				throw result.Exception;
		}
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

		public override int Read (byte[] buffer,
					  int offset,
					  int count)
		{
			return 0;
		}

		public override int ReadByte ()
		{
			return -1;
		}

		public override long Seek (long offset,
					   SeekOrigin origin)
		{
			return 0;
		}

		public override void SetLength (long value)
		{
		}

		public override void Write (byte[] buffer,
					    int offset,
					    int count)
		{
		}

		public override void WriteByte (byte value)
		{
		}
	}
}
