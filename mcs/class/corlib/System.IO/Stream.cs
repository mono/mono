//
// System.IO/Stream.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System.Threading;

namespace System.IO
{

	public abstract class Stream : MarshalByRefObject, IDisposable
	{
		// public static readonly Stream Null;

		static Stream ()
		{
			//Null = new NullStream ();
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

		public virtual void Dispose ()
		{
		}
		
		protected virtual WaitHandle CreateWaitHandle()
		{
			return(null);
		}
		
		protected virtual void Dispose (bool disposing)
		{
		}

		~Stream ()
		{
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
	}

	class NullStream : Stream
	{
                private long position = 0;
                private long length = System.Int64.MaxValue;

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
				return length;
			}
		}

		public override long Position
		{
			get {
				return position;
			}
			set {
				position = value;
			}
		}

		public override void Flush ()
		{
		}

		public override int Read (byte[] buffer,
					  int offset,
					  int count)
		{
			int max = offset + count;
			
			for (int i = offset; i < max; i++)
				buffer [i] = 0;

			return count;
		}

		public override int ReadByte ()
		{
			return 0;
		}

		public override long Seek (long offset,
					   SeekOrigin origin)
		{
			switch (origin) {
			case SeekOrigin.Begin:
				position = offset;
				break;
			case SeekOrigin.Current:
				position = position + offset;
				break;
			case SeekOrigin.End:
				position = Length - offset;
				break;
			}

			return position;
		}

		public override void SetLength (long value)
		{
			length = value;
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
