//
// Stream Test Helper Classes
//
// Author: 
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.IO;

namespace MonoTests.System.IO {

	public class TestHelperStream : Stream {

		private bool _read;
		private bool _write;
		private bool _seek;
		private long _pos;
		private long _length;

		public TestHelperStream (bool read, bool write, bool seek) 
		{
			_read = read;
			_write = write;
			_seek = seek;
		}

		public override bool CanRead {
			get { return _read; }
		}

		public override bool CanSeek {
                        get { return _seek; }
                }

                public override bool CanWrite {
                        get { return _write; }
                }

		public override long Length {
			get { return _length; }
		}

		public override long Position
		{
			get {
				if (!_seek)
					throw new NotSupportedException ("Not seekable");
				return _pos;
			}
			set {
				if (!_seek)
					throw new NotSupportedException ("Not seekable");
				_pos = value;
			}
		}

		public override void Flush ()
		{
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			if (!_read)
				throw new NotSupportedException ("Not readable");
			return count;
		}

		public override int ReadByte ()
		{
			return -1;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			if (!_seek)
				throw new NotSupportedException ("Not seekable");
			return offset;
		}

		public override void SetLength (long value)
		{
			if (!_write)
				throw new NotSupportedException ("Not writeable");
			_length = value;
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			if (!_write)
				throw new NotSupportedException ("Not writeable");
		}

		public override void WriteByte (byte value)
		{
			if (!_write)
				throw new NotSupportedException ("Not writeable");
		}
	}

	public class ReadOnlyStream : TestHelperStream {

		public ReadOnlyStream () : base (true, false, true)
		{
		}
	}

	public class WriteOnlyStream : TestHelperStream {

		public WriteOnlyStream () : base (false, true, true)
		{
		}
	}

	public class NonSeekableStream : TestHelperStream {

		public NonSeekableStream () : base (true, true, false)
		{
		}
	}
}
