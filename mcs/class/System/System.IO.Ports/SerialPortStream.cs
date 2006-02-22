//
// System.IO.Ports.Handshake.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//


#if NET_2_0

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace System.IO.Ports
{
	class SerialPortStream : Stream
	{
		SerialPort port;

		public SerialPortStream (SerialPort port)
		{
			this.port = port;
		}

		public override bool CanRead {
			get {
				return true;
			}
		}

		public override bool CanSeek {
			get {
				return false;
			}
		}

		public override bool CanWrite {
			get {
				return true;
			}
		}

		public override long Length {
			get {
				return -1;
			}
		}

		public override long Position {
			get {
				return -1;
			}
			set {
				throw new InvalidOperationException ();
			}
		}

		public override void Flush ()
		{
			throw new NotImplementedException ();
		}

		public override int Read ([In,Out] byte[] buffer, int offset, int count)
		{
			return port.Read (buffer, offset, count);
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new InvalidOperationException ();
		}

		public override void SetLength (long value)
		{
			throw new InvalidOperationException ();
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			port.Write (buffer, offset, count);
		}
	}
}

#endif


