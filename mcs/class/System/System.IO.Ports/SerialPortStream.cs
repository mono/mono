//
// System.IO.Ports.SerialPortStream.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//


#if NET_2_0

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace System.IO.Ports
{
	class SerialPortStream : Stream, IDisposable
	{
		int fd;
		int baud_rate;
		int data_bits;
		Parity parity;
		StopBits stop_bits;
		Handshake handshake;
		int read_timeout;
		int write_timeout;
		bool disposed;

		[DllImport ("MonoPosixHelper")]
		static extern int open_serial (string portName);

		public SerialPortStream (string portName, int baudRate, int dataBits, Parity par, StopBits stopBits,
				bool dtrEnable, bool rtsEnable, Handshake handsh, int readTimeout, int writeTimeout,
				int readBufferSize, int writeBufferSize)
		{
			fd = open_serial (portName);
			if (fd == -1)
				throw new IOException ();
			
			baud_rate = baudRate;
			data_bits = dataBits;
			parity = par;
			stop_bits = stopBits;
			handshake = handsh;
			read_timeout = readTimeout;
			write_timeout = writeTimeout;
			
			if (!set_attributes (fd, baud_rate, parity, data_bits, stop_bits, handshake))
				throw new IOException ();
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

		public override bool CanTimeout {
			get {
				return true;
			}
		}

		public override int ReadTimeout {
			get {
				return read_timeout;
			}
			set {
				if (value < 0 && value != SerialPort.InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				read_timeout = value;
			}
		}

		public override int WriteTimeout {
			get {
				return write_timeout;
			}
			set {
				if (value < 0 && value != SerialPort.InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				write_timeout = value;
			}
		}

		public override long Length {
			get {
				throw new NotSupportedException ();
			}
		}

		public override long Position {
			get {
				throw new NotSupportedException ();
			}
			set {
				throw new NotSupportedException ();
			}
		}

		public override void Flush ()
		{
			// If used, this _could_ flush the serial port
			// buffer (not the SerialPort class buffer)
		}

		[DllImport ("MonoPosixHelper")]
		static extern int read_serial (int fd, byte [] buffer, int offset, int count, int timeout);

		public override int Read ([In,Out] byte[] buffer, int offset, int count)
		{
			CheckDisposed ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
			if (count > buffer.Length - offset)
				throw new ArgumentException ("count > buffer.Length - offset");
			
			return read_serial (fd, buffer, offset, count, read_timeout);
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		[DllImport ("MonoPosixHelper")]
		static extern void write_serial (int fd, byte [] buffer, int offset, int count, int timeout);

		public override void Write (byte[] buffer, int offset, int count)
		{
			CheckDisposed ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
			if (count > buffer.Length - offset)
				throw new ArgumentException ("offset+count > buffer.Length");

			write_serial (fd, buffer, offset, count, write_timeout);
		}

		protected override void Dispose (bool disposing)
		{
			if (disposed)
				return;
			
			disposed = true;
			close_serial (fd);
		}

		[DllImport ("MonoPosixHelper")]
		static extern void close_serial (int fd);

		public override void Close ()
		{
			((IDisposable) this).Dispose ();
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		~SerialPortStream ()
		{
			Dispose (false);
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
		}

		[DllImport ("MonoPosixHelper")]
		static extern bool set_attributes (int fd, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake);

		// FIXME - Separate baud rate from the other values,
		// since it can be set individually
		internal int BaudRate {
			get {
				return baud_rate;
			}
			set {
				baud_rate = value;
				set_attributes (fd, baud_rate, parity, data_bits, stop_bits, handshake);
			}
		}

		internal Parity Parity {
			get {
				return parity;
			}
			set {
				parity = value;
				set_attributes (fd, baud_rate, parity, data_bits, stop_bits, handshake);
			}
		}

		internal int DataBits {
			get {
				return data_bits;
			}
			set {
				data_bits = value;
				set_attributes (fd, baud_rate, parity, data_bits, stop_bits, handshake);
			}
		}

		internal StopBits StopBits {
			get {
				return stop_bits;
			}
			set {
				stop_bits = value;
				set_attributes (fd, baud_rate, parity, data_bits, stop_bits, handshake);
			}
		}

		internal Handshake Handshake {
			get {
				return handshake;
			}
			set {
				handshake = value;
				set_attributes (fd, baud_rate, parity, data_bits, stop_bits, handshake);
			}
		}

		internal int BytesToRead {
			get {
				return 0; // Not implemented yet
			}
		}

		internal int BytesToWrite {
			get {
				return 0; // Not implemented yet
			}
		}

		[DllImport ("MonoPosixHelper")]
		static extern void discard_buffer (int fd, bool inputBuffer);
		
		internal void DiscardInputBuffer ()
		{
			discard_buffer (fd, true);
		}

		internal void DiscardOutputBuffer ()
		{
			discard_buffer (fd, false);
		}

	}
}

#endif


