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

			SyncReadResult srr = new SyncReadResult (state);
			try
			{
				srr.Complete (Read (buffer, offset, count));
			}
			catch (IOException e)
			{
				srr._exception = e;
			}

			if (cback != null)
				cback (srr);

			return srr;
		}

		public virtual IAsyncResult
		BeginWrite (byte [] buffer, int offset, int count, AsyncCallback cback, object state)
		{
			if (!CanWrite)
				throw new NotSupportedException ("This stream does not support reading");

			SyncWriteResult swr = new SyncWriteResult (state);
			try
			{
				Write (buffer, offset, count);
				swr.Complete ();
			}
			catch (IOException e)
			{
				swr._exception = e;
			}

			if (cback != null)
				cback (swr);

			return swr;
		}
		
		public virtual int EndRead (IAsyncResult async_result)
		{
			if (async_result == null)
				throw new ArgumentNullException ("async_result");
			SyncReadResult srr = async_result as SyncReadResult;
			if (srr == null)
				throw new ArgumentException ("async_result is invalid");
			if (srr._fEndCalled)
				throw new InvalidOperationException ("EndRead called twice");
			srr._fEndCalled = true;
			if (srr._exception != null)
				throw srr._exception;
			return srr._cbRead;
		}

		public virtual void EndWrite (IAsyncResult async_result)
		{
			if (async_result == null)
				throw new ArgumentNullException ("async_result");
			SyncWriteResult swr = async_result as SyncWriteResult;
			if (swr == null)
				throw new ArgumentException ("async_result is invalid");
			if (swr._fEndCalled)
				throw new InvalidOperationException ("EndRead called twice");
			swr._fEndCalled = true;
			if (swr._exception != null)
				throw swr._exception;
		}

		// this class implements the synchronous IASyncResult for the obove methods
		private class SyncResult : IAsyncResult
		{
			object _objState;		// client-supplied state
			bool _fComplete;		// if the IO operation completed successfully
			ManualResetEvent _hWait;		// the wait event
			public bool _fEndCalled;		// true iff the End method was called already
			public Exception _exception;	// holds any exception throw during IO operation

			public SyncResult (object objState)
			{
				_objState = objState;
				_hWait = new ManualResetEvent (false);
			}

			public void Complete ()
			{
				_fComplete = true;
				_hWait.Set ();
			}

			// IAsyncResult members
			object IAsyncResult.AsyncState
			{
				get { return _objState; }
			}

			WaitHandle IAsyncResult.AsyncWaitHandle
			{
				get { return _hWait; }
			}

			bool IAsyncResult.CompletedSynchronously
			{
				get { return true; }
			}

			bool IAsyncResult.IsCompleted
			{
				get { return _fComplete; }
			}
		}
		private class SyncReadResult : SyncResult
		{
			public int _cbRead;		// the number of bytes read

			public SyncReadResult (object objState) : base (objState) {}

			public void Complete (int cbRead)
			{
				_cbRead = cbRead;
				Complete ();
			}
		}
		private class SyncWriteResult : SyncResult
		{
			public SyncWriteResult (object objState) : base (objState) {}
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
			return 0;
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







