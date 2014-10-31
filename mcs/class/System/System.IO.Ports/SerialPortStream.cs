//
// System.IO.Ports.SerialPortStream.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//
// Slightly modified by Konrad M. Kruczynski (added baud rate value checking)


using System;
using System.IO;
using System.Runtime.InteropServices;

namespace System.IO.Ports
{
	class SerialPortStream : Stream, ISerialStream, IDisposable
	{
		int fd;
		int read_timeout;
		int write_timeout;
		bool disposed;

		[DllImport ("MonoPosixHelper", SetLastError = true)]
		static extern int open_serial (string portName);

		public SerialPortStream (string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits,
				bool dtrEnable, bool rtsEnable, Handshake handshake, int readTimeout, int writeTimeout,
				int readBufferSize, int writeBufferSize)
		{
			fd = open_serial (portName);
			if (fd == -1)
				ThrowIOException ();
				
			TryBaudRate (baudRate);
			
			if (!set_attributes (fd, baudRate, parity, dataBits, stopBits, handshake))
				ThrowIOException (); // Probably Win32Exc for compatibility

			read_timeout = readTimeout;
			write_timeout = writeTimeout;
			
			SetSignal (SerialSignal.Dtr, dtrEnable);
			
			if (handshake != Handshake.RequestToSend && 
					handshake != Handshake.RequestToSendXOnXOff)
				SetSignal (SerialSignal.Rts, rtsEnable);
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

		[DllImport ("MonoPosixHelper", SetLastError = true)]
		static extern int read_serial (int fd, byte [] buffer, int offset, int count);
		

		[DllImport ("MonoPosixHelper", SetLastError = true)]
		static extern bool poll_serial (int fd, out int error, int timeout);

		public override int Read ([In,Out] byte[] buffer, int offset, int count)
		{
			CheckDisposed ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || count < 0)
				throw new ArgumentOutOfRangeException ("offset or count less than zero.");

			if (buffer.Length - offset < count )
				throw new ArgumentException ("offset+count",
							      "The size of the buffer is less than offset + count.");
			
			int error;
			bool poll_result = poll_serial (fd, out error, read_timeout);
			if (error == -1)
				ThrowIOException ();

			if (!poll_result) {
				// see bug 79735   http://bugzilla.ximian.com/show_bug.cgi?id=79735
				// should the next line read: return -1; 
				throw new TimeoutException();
			}

			int result = read_serial (fd, buffer, offset, count);
			if (result == -1)
				ThrowIOException ();
			return result;
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException ();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException ();
		}

		[DllImport ("MonoPosixHelper", SetLastError = true)]
		static extern int write_serial (int fd, byte [] buffer, int offset, int count, int timeout);

		public override void Write (byte[] buffer, int offset, int count)
		{
			CheckDisposed ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || count < 0)
				throw new ArgumentOutOfRangeException ();

			if (buffer.Length - offset < count)
				throw new ArgumentException ("offset+count",
							     "The size of the buffer is less than offset + count.");

			// FIXME: this reports every write error as timeout
			if (write_serial (fd, buffer, offset, count, write_timeout) < 0)
				throw new TimeoutException("The operation has timed-out");
		}

		protected override void Dispose (bool disposing)
		{
			if (disposed)
				return;
			
			disposed = true;
			if (close_serial (fd) != 0)
				ThrowIOException();
		}

		[DllImport ("MonoPosixHelper", SetLastError = true)]
		static extern int close_serial (int fd);

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
			try {
				Dispose (false);
			} catch (IOException) {
			}
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
		}

		[DllImport ("MonoPosixHelper", SetLastError = true)]
		static extern bool set_attributes (int fd, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake);

		public void SetAttributes (int baud_rate, Parity parity, int data_bits, StopBits sb, Handshake hs)
		{
			if (!set_attributes (fd, baud_rate, parity, data_bits, sb, hs))
				ThrowIOException ();
		}

		[DllImport("MonoPosixHelper", SetLastError = true)]
		static extern int get_bytes_in_buffer (int fd, int input);
		
		public int BytesToRead {
			get {
				int result = get_bytes_in_buffer (fd, 1);
				if (result == -1)
					ThrowIOException ();
				return result;
			}
		}

		public int BytesToWrite {
			get {
				int result = get_bytes_in_buffer (fd, 0);
				if (result == -1)
					ThrowIOException ();
				return result;
			}
		}

		[DllImport ("MonoPosixHelper", SetLastError = true)]
		static extern int discard_buffer (int fd, bool inputBuffer);

		public void DiscardInBuffer ()
		{
			if (discard_buffer (fd, true) != 0)
				ThrowIOException();
		}

		public void DiscardOutBuffer ()
		{
			if (discard_buffer (fd, false) != 0)
				ThrowIOException();
		}
		
		[DllImport ("MonoPosixHelper", SetLastError = true)]
		static extern SerialSignal get_signals (int fd, out int error);

		public SerialSignal GetSignals ()
		{
			int error;
			SerialSignal signals = get_signals (fd, out error);
			if (error == -1)
				ThrowIOException ();

			return signals;
		}

		[DllImport ("MonoPosixHelper", SetLastError = true)]
		static extern int set_signal (int fd, SerialSignal signal, bool value);

		public void SetSignal (SerialSignal signal, bool value)
		{
			if (signal < SerialSignal.Cd || signal > SerialSignal.Rts ||
					signal == SerialSignal.Cd ||
					signal == SerialSignal.Cts ||
					signal == SerialSignal.Dsr)
				throw new Exception ("Invalid internal value");

			if (set_signal (fd, signal, value) == -1)
				ThrowIOException ();
		}

		[DllImport ("MonoPosixHelper", SetLastError = true)]
		static extern int breakprop (int fd);

		public void SetBreakState (bool value)
		{
			if (value)
				if (breakprop (fd) == -1)
					ThrowIOException ();
		}

		[DllImport ("libc")]
		static extern IntPtr strerror (int errnum);

		static void ThrowIOException ()
		{
			int errnum = Marshal.GetLastWin32Error ();
			string error_message = Marshal.PtrToStringAnsi (strerror (errnum));

			throw new IOException (error_message);
		}
		
		[DllImport ("MonoPosixHelper")]
		static extern bool is_baud_rate_legal (int baud_rate);
		
		private void TryBaudRate (int baudRate)
		{
			if (!is_baud_rate_legal (baudRate))
			{
				// this kind of exception to be compatible with MSDN API
				throw new ArgumentOutOfRangeException ("baudRate",
					"Given baud rate is not supported on this platform.");
			}			
		}
	}
}


