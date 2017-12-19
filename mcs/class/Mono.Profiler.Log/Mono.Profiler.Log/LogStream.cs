// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;

namespace Mono.Profiler.Log {

	public class LogStream : Stream {

		public Stream BaseStream { get; }

		public virtual bool EndOfStream => BaseStream.Position == BaseStream.Length;

		public override bool CanRead => true;

		public override bool CanSeek => false;

		public override bool CanWrite => false;

		public override long Length => throw new NotSupportedException ();

		public override long Position {
			get => throw new NotSupportedException ();
			set => throw new NotSupportedException ();
		}

		readonly byte[] _byteBuffer = new byte [1];

		public LogStream (Stream baseStream)
		{
			if (baseStream == null)
				throw new ArgumentNullException (nameof (baseStream));

			if (!baseStream.CanRead)
				throw new ArgumentException ("Stream does not support reading.", nameof (baseStream));

			BaseStream = baseStream;
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing)
				BaseStream.Dispose ();
		}

		public override void Flush ()
		{
			throw new NotSupportedException ();
		}

		public override int ReadByte ()
		{
			// The base method on Stream is extremely inefficient in that it
			// allocates a 1-byte array for every call. Simply use a private
			// buffer instead.
			return Read (_byteBuffer, 0, sizeof (byte)) == 0 ? -1 : _byteBuffer [0];
		}

		public override int Read (byte[] buffer, int offset, int count)
		{
			return BaseStream.Read (buffer, offset, count);
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException ();
		}
	}
}
