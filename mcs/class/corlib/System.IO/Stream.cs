//
// System.IO/Stream.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001, 2002 Ximian, Inc.  http://www.ximian.com
//

using System.Threading;
using System.Runtime.Remoting.Messaging;

namespace System.IO
{
	[Serializable]
	public abstract class Stream : MarshalByRefObject, IDisposable
	{
		public static readonly Stream Null;

		static Stream ()
		{
			Null = new NullStream ();
		}

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
			Flush ();
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

		public abstract int Read (byte[] buffer,
					  int offset,
					  int count);

		public virtual int ReadByte ()
		{
			byte[] buffer = new byte [1];

			if (Read (buffer, 0, 1) == 1)
				return buffer [0];
			
			return -1;
		}

		public abstract long Seek (long offset,
					   SeekOrigin origin);

		public abstract void SetLength (long value);

		public abstract void Write (byte[] buffer,
					    int offset,
					    int count);

		public virtual void WriteByte (byte value)
		{
			byte[] buffer = new byte [1];

			buffer [0] = value;

			Write (buffer, 0, 1);
		}

		delegate int ReadDelegate (byte [] buffer, int offset, int count);

		public virtual IAsyncResult
		BeginRead (byte [] buffer, int offset, int count, AsyncCallback cback, object state)
		{
			if (!CanRead)
				throw new NotSupportedException ("This stream does not support reading");

			ReadDelegate read_delegate = new ReadDelegate (Read);

			return read_delegate.BeginInvoke (buffer, offset, count, cback, state);			
		}

		delegate void WriteDelegate (byte [] buffer, int offset, int count);

		public virtual IAsyncResult
		BeginWrite (byte [] buffer, int offset, int count, AsyncCallback cback, object state)
		{
			if (!CanWrite)
				throw new NotSupportedException ("This stream does not support writing");
	
			WriteDelegate write_delegate = new WriteDelegate (Write);

			return write_delegate.BeginInvoke (buffer, offset, count, cback, state);
		}
		
		public virtual int EndRead (IAsyncResult async_result)
		{
			if (async_result == null)
				throw new ArgumentNullException ("async_result");

			AsyncResult ar = (AsyncResult)async_result;
			ReadDelegate read_delegate = (ReadDelegate)ar.AsyncDelegate;
			return read_delegate.EndInvoke (async_result);
		}

		public virtual void EndWrite (IAsyncResult async_result)
		{
			if (async_result == null)
				throw new ArgumentNullException ("async_result");

			AsyncResult ar = (AsyncResult)async_result;
			WriteDelegate write_delegate = (WriteDelegate)ar.AsyncDelegate;
			write_delegate.EndInvoke (async_result);
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







