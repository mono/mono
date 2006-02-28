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
		int baudRate;
		int dataBits;
		Parity parity;
		StopBits stopBits;
		Handshake handshake;
		int readTimeout;
		int writeTimeout;
		bool disposed;

		[DllImport ("MonoPosixHelper")]
		static extern int open_serial (string portName);

		public SerialPortStream (SerialPort port)
		{
			fd = open_serial (port.PortName);
			if (fd == -1)
				throw new IOException ();

			readTimeout = port.ReadTimeout;
			writeTimeout = port.WriteTimeout;
			baudRate = port.BaudRate;
			parity = port.Parity;
			dataBits = port.DataBits;
			stopBits = port.StopBits;
			handshake = port.Handshake;
			
			if (!set_attributes (fd, port.BaudRate, port.Parity, port.DataBits, port.StopBits, port.Handshake))
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

		// Remove this comments as soon as these properties
		// are added to System.IO.Stream
		/*
		public override bool CanTimeout {
			get {
				return true;
			}
		}

		public override int ReadTimeout {
			get {
				return readTimeout;
			}
			set {
				if (value < 0 && value != SerialPort.InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				readTimeout = value;
			}
		}

		public override int WriteTimeout {
			get {
				return writeTimeout;
			}
			set {
				if (value < 0 && value != SerialPort.InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				writeTimeout = value;
			}
		}*/

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
			
			return read_serial (fd, buffer, offset, count, readTimeout);
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

			write_serial (fd, buffer, offset, count, writeTimeout);
		}

		protected void Dispose (bool disposing)
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
				return baudRate;
			}
			set {
				baudRate = value;
				set_attributes (fd, baudRate, parity, dataBits, stopBits, handshake);
			}
		}

		internal Parity Parity {
			get {
				return parity;
			}
			set {
				parity = value;
				set_attributes (fd, baudRate, parity, dataBits, stopBits, handshake);
			}
		}

		internal int DataBits {
			get {
				return dataBits;
			}
			set {
				dataBits = value;
				set_attributes (fd, baudRate, parity, dataBits, stopBits, handshake);
			}
		}

		internal StopBits StopBits {
			get {
				return stopBits;
			}
			set {
				stopBits = stopBits;
				set_attributes (fd, baudRate, parity, dataBits, stopBits, handshake);
			}
		}

		internal Handshake Handshake {
			get {
				return handshake;
			}
			set {
				handshake = value;
				set_attributes (fd, baudRate, parity, dataBits, stopBits, handshake);
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


