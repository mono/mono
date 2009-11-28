//
// System.IO.Ports.WinSerialStream.cs
//
// Authors:
//	Carlos Alberto Cortez (calberto.cortez@gmail.com)
//
// (c) Copyright 2006 Novell, Inc. (http://www.novell.com)
//

using System;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.ComponentModel;

#if NET_2_0

namespace System.IO.Ports
{
	class WinSerialStream : Stream, ISerialStream, IDisposable
	{
		// Windows API Constants
		const uint GenericRead = 0x80000000;
		const uint GenericWrite = 0x40000000;
		const uint OpenExisting = 3;
		const uint FileFlagOverlapped = 0x40000000;
		const uint PurgeRxClear = 0x0004;
		const uint PurgeTxClear = 0x0008;
		const uint WinInfiniteTimeout = 0xFFFFFFFF;
		const uint FileIOPending = 997;

		// Signal constants
		const uint SetRts = 3;
		const uint ClearRts = 4;
		const uint SetDtr = 5;
		const uint ClearDtr = 6;
		const uint SetBreak = 8;
		const uint ClearBreak = 9;
		const uint CtsOn = 0x0010;
		const uint DsrOn = 0x0020;
		const uint RsldOn = 0x0080;

		// Event constants
		const uint EvRxChar = 0x0001;
		const uint EvCts = 0x0008;
		const uint EvDsr = 0x0010;
		const uint EvRlsd = 0x0020;
		const uint EvBreak = 0x0040;
		const uint EvErr = 0x0080;
		const uint EvRing = 0x0100;

		int handle;
		int read_timeout;
		int write_timeout;
		bool disposed;
		IntPtr write_overlapped;
		IntPtr read_overlapped;
		ManualResetEvent read_event;
		ManualResetEvent write_event;
		Timeouts timeouts;

		[DllImport("kernel32", SetLastError = true)]
		static extern int CreateFile(string port_name, uint desired_access,
				uint share_mode, uint security_attrs, uint creation, uint flags,
				uint template);

		[DllImport("kernel32", SetLastError = true)]
		static extern bool SetupComm(int handle, int read_buffer_size, int write_buffer_size);

		[DllImport("kernel32", SetLastError = true)]
		static extern bool PurgeComm(int handle, uint flags);

		[DllImport("kernel32", SetLastError = true)]
		static extern bool SetCommTimeouts(int handle, Timeouts timeouts);

		public WinSerialStream (string port_name, int baud_rate, int data_bits, Parity parity, StopBits sb,
				bool dtr_enable, bool rts_enable, Handshake hs, int read_timeout, int write_timeout,
				int read_buffer_size, int write_buffer_size)
		{
			handle = CreateFile (port_name, GenericRead | GenericWrite, 0, 0, OpenExisting,
					FileFlagOverlapped, 0);

			if (handle == -1)
				ReportIOError (port_name);

			// Set port low level attributes
			SetAttributes (baud_rate, parity, data_bits, sb, hs);

			// Clean buffers and set sizes
			if (!PurgeComm (handle, PurgeRxClear | PurgeTxClear) ||
					!SetupComm (handle, read_buffer_size, write_buffer_size))
				ReportIOError (null);

			// Set timeouts
			this.read_timeout = read_timeout;
			this.write_timeout = write_timeout;
			timeouts = new Timeouts (read_timeout, write_timeout);
			if (!SetCommTimeouts(handle, timeouts))
				ReportIOError (null);

			/// Set DTR and RTS
			SetSignal(SerialSignal.Dtr, dtr_enable);

			if (hs != Handshake.RequestToSend &&
					hs != Handshake.RequestToSendXOnXOff)
				SetSignal(SerialSignal.Rts, rts_enable);

			// Init overlapped structures
			NativeOverlapped wo = new NativeOverlapped ();
			write_event = new ManualResetEvent (false);
#if NET_2_0
			wo.EventHandle = write_event.Handle;
#else
			wo.EventHandle = (int) write_event.Handle;
#endif
			write_overlapped = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (NativeOverlapped)));
			Marshal.StructureToPtr (wo, write_overlapped, true);

			NativeOverlapped ro = new NativeOverlapped ();
			read_event = new ManualResetEvent (false);
#if NET_2_0
			ro.EventHandle = read_event.Handle;
#else
			ro.EventHandle = (int) read_event.Handle;
#endif
			read_overlapped = Marshal.AllocHGlobal (Marshal.SizeOf (typeof (NativeOverlapped)));
			Marshal.StructureToPtr (ro, read_overlapped, true);
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

		public override bool CanTimeout {
			get {
				return true;
			}
		}

		public override bool CanWrite {
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

				timeouts.SetValues (value, write_timeout);
				if (!SetCommTimeouts (handle, timeouts))
					ReportIOError (null);

				read_timeout = value;
			}
		}

		public override int WriteTimeout {
			get {
				return write_timeout;
			}
			set
			{
				if (value < 0 && value != SerialPort.InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				timeouts.SetValues (read_timeout, value);
				if (!SetCommTimeouts (handle, timeouts))
					ReportIOError (null);

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

		[DllImport("kernel32", SetLastError = true)]
		static extern bool CloseHandle (int handle);

		protected override void Dispose (bool disposing)
		{
			if (disposed)
				return;

			disposed = true;
			CloseHandle (handle);
			Marshal.FreeHGlobal (write_overlapped);
			Marshal.FreeHGlobal (read_overlapped);
		}

		void IDisposable.Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public override void Close ()
		{
			((IDisposable)this).Dispose ();
		}

		~WinSerialStream ()
		{
			Dispose (false);
		}

		public override void Flush ()
		{
			CheckDisposed ();
			// No dothing by now
		}

		public override long Seek (long offset, SeekOrigin origin)
		{
			throw new NotSupportedException();
		}

		public override void SetLength (long value)
		{
			throw new NotSupportedException();
		}

#if !TARGET_JVM
		[DllImport("kernel32", SetLastError = true)]
			static extern unsafe bool ReadFile (int handle, byte* buffer, int bytes_to_read,
					out int bytes_read, IntPtr overlapped);

		[DllImport("kernel32", SetLastError = true)]
			static extern unsafe bool GetOverlappedResult (int handle, IntPtr overlapped,
					ref int bytes_transfered, bool wait);
#endif

		public override int Read ([In, Out] byte [] buffer, int offset, int count)
		{
			CheckDisposed ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || count < 0)
				throw new ArgumentOutOfRangeException ("offset or count less than zero.");

			if (buffer.Length - offset < count )
				throw new ArgumentException ("offset+count",
							      "The size of the buffer is less than offset + count.");

			int bytes_read;

			unsafe {
				fixed (byte* ptr = buffer) {
					if (ReadFile (handle, ptr + offset, count, out bytes_read, read_overlapped))
						return bytes_read;
				
					// Test for overlapped behavior
					if (Marshal.GetLastWin32Error () != FileIOPending)
						ReportIOError (null);
				
					if (!GetOverlappedResult (handle, read_overlapped, ref bytes_read, true))
						ReportIOError (null);
			
				}
			}

			if (bytes_read == 0)
				throw new TimeoutException (); // We didn't get any byte

			return bytes_read;
		}

#if !TARGET_JVM
		[DllImport("kernel32", SetLastError = true)]
		static extern unsafe bool WriteFile (int handle, byte* buffer, int bytes_to_write,
				out int bytes_written, IntPtr overlapped);
#endif

		public override void Write (byte [] buffer, int offset, int count)
		{
			CheckDisposed ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			if (offset < 0 || count < 0)
				throw new ArgumentOutOfRangeException ();

			if (buffer.Length - offset < count)
				throw new ArgumentException ("offset+count",
							     "The size of the buffer is less than offset + count.");

			int bytes_written = 0;

			unsafe {
				fixed (byte* ptr = buffer) {
					if (WriteFile (handle, ptr + offset, count, out bytes_written, write_overlapped))
						return;
					if (Marshal.GetLastWin32Error() != FileIOPending)
						ReportIOError (null);
					
					if (!GetOverlappedResult(handle, write_overlapped, ref bytes_written, true))
						ReportIOError (null);
				}
			}

			// If the operation timed out, then
			// we transfered less bytes than the requested ones
			if (bytes_written < count)
				throw new TimeoutException ();
		}

		[DllImport("kernel32", SetLastError = true)]
		static extern bool GetCommState (int handle, [Out] DCB dcb);

		[DllImport ("kernel32", SetLastError=true)]
		static extern bool SetCommState (int handle, DCB dcb);

		public void SetAttributes (int baud_rate, Parity parity, int data_bits, StopBits bits, Handshake hs)
		{
			DCB dcb = new DCB ();
			if (!GetCommState (handle, dcb))
				ReportIOError (null);

			dcb.SetValues (baud_rate, parity, data_bits, bits, hs);
			if (!SetCommState (handle, dcb))
				ReportIOError (null);
		}

		void ReportIOError(string optional_arg)
		{
			int error = Marshal.GetLastWin32Error ();
			string message;
			switch (error) {
				case 2:
				case 3:
					message = "The port `" + optional_arg + "' does not exist.";
					break;
				case 87:
					message = "Parameter is incorrect.";
					break;
				default:
					// As fallback, we show the win32 error
					message = new Win32Exception ().Message;
					break;
			}

			throw new IOException (message);
		}

		void CheckDisposed ()
		{
			if (disposed)
				throw new ObjectDisposedException (GetType ().FullName);
		}

		// ISerialStream members
		public void DiscardInBuffer ()
		{
			if (!PurgeComm (handle, PurgeRxClear))
				ReportIOError (null);
		}

		public void DiscardOutBuffer ()
		{
			if (!PurgeComm (handle, PurgeRxClear))
				ReportIOError (null);
		}

		[DllImport ("kernel32", SetLastError=true)]
		static extern bool ClearCommError (int handle, out uint errors, out CommStat stat);

		public int BytesToRead {
			get {
				uint errors;
				CommStat stat;
				if (!ClearCommError (handle, out errors, out stat))
					ReportIOError (null);

				return (int)stat.BytesIn;
			}
		}

		public int BytesToWrite {
			get {
				uint errors;
				CommStat stat;
				if (!ClearCommError (handle, out errors, out stat))
					ReportIOError (null);

				return (int)stat.BytesOut;
			}
		}

		[DllImport ("kernel32", SetLastError=true)]
		static extern bool GetCommModemStatus (int handle, out uint flags);

		public SerialSignal GetSignals ()
		{
			uint flags;
			if (!GetCommModemStatus (handle, out flags))
				ReportIOError (null);

			SerialSignal signals = SerialSignal.None;
			if ((flags & RsldOn) != 0)
				signals |= SerialSignal.Cd;
			if ((flags & CtsOn) != 0)
				signals |= SerialSignal.Cts;
			if ((flags & DsrOn) != 0)
				signals |= SerialSignal.Dsr;

			return signals;
		}
		
		[DllImport ("kernel32", SetLastError=true)]
		static extern bool EscapeCommFunction (int handle, uint flags);

		public void SetSignal (SerialSignal signal, bool value)
		{
			if (signal != SerialSignal.Rts && signal != SerialSignal.Dtr)
				throw new Exception ("Wrong internal value");

			uint flag;
			if (signal == SerialSignal.Rts)
				if (value)
					flag = SetRts;
				else
					flag = ClearRts;
			else
				if (value)
					flag = SetDtr;
				else
					flag = ClearDtr;

			if (!EscapeCommFunction (handle, flag))
				ReportIOError (null);
		}

		public void SetBreakState (bool value)
		{
			if (!EscapeCommFunction (handle, value ? SetBreak : ClearBreak))
				ReportIOError (null);
		}

	}
	
	[StructLayout (LayoutKind.Sequential)]
	class DCB
	{
		public int dcb_length;
		public int baud_rate;
		public int flags;
		public short w_reserved;
		public short xon_lim;
		public short xoff_lim;
		public byte byte_size;
		public byte parity;
		public byte stop_bits;
		public byte xon_char;
		public byte xoff_char;
		public byte error_char;
		public byte eof_char;
		public byte evt_char;
		public short w_reserved1;

		// flags:
		//const int fBinary = 0x0001;
		//const int fParity = 0x0002;
		const int fOutxCtsFlow = 0x0004;
		//const int fOutxDsrFlow1 = 0x0008;
		//const int fOutxDsrFlow2 = 0x0010;
		//const int fDtrControl = 0x00020;
		//const int fDsrSensitivity = 0x0040;
		//const int fTXContinueOnXoff = 0x0080;
		const int fOutX = 0x0100;
		const int fInX = 0x0200;
		//const int fErrorChar = 0x0400;
		//const int fNull = 0x0800;
		//const int fRtsControl1 = 0x1000;
		const int fRtsControl2 = 0x2000;
		//const int fAbortOnError = 0x4000;

		public void SetValues (int baud_rate, Parity parity, int byte_size, StopBits sb, Handshake hs)
		{
			switch (sb) {
				case StopBits.One:
					stop_bits = 0;
					break;
				case StopBits.OnePointFive:
					stop_bits = 1;
					break;
				case StopBits.Two:
					stop_bits = 2;
					break;
				default: // Shouldn't happen
					break;
			}

			this.baud_rate = baud_rate;
			this.parity = (byte)parity;
			this.byte_size = (byte)byte_size;

			// Clear Handshake flags
			flags &= ~(fOutxCtsFlow | fOutX | fInX | fRtsControl2);

			// Set Handshake flags
			switch (hs)
			{
				case Handshake.None:
					break;
				case Handshake.XOnXOff:
					flags |= fOutX | fInX;
					break;
				case Handshake.RequestToSend:
					flags |= fOutxCtsFlow | fRtsControl2;
					break;
				case Handshake.RequestToSendXOnXOff:
					flags |= fOutxCtsFlow | fOutX | fInX | fRtsControl2;
					break;
				default: // Shouldn't happen
					break;
			}
		}
	}
	
	[StructLayout (LayoutKind.Sequential)]
	class Timeouts
	{
		public uint ReadIntervalTimeout;
		public uint ReadTotalTimeoutMultiplier;
		public uint ReadTotalTimeoutConstant;
		public uint WriteTotalTimeoutMultiplier;
		public uint WriteTotalTimeoutConstant;

		public const uint MaxDWord = 0xFFFFFFFF;

		public Timeouts (int read_timeout, int write_timeout)
		{
			SetValues (read_timeout, write_timeout);
		}

		public void SetValues (int read_timeout, int write_timeout)
		{
			// FIXME: The windows api docs are not very clear about read timeouts,
			// and we have to simulate infinite with a big value (uint.MaxValue - 1)
			ReadIntervalTimeout = MaxDWord;
			ReadTotalTimeoutMultiplier = MaxDWord;
			ReadTotalTimeoutConstant = (read_timeout == -1 ? MaxDWord - 1 : (uint) read_timeout);

			WriteTotalTimeoutMultiplier = 0;
			WriteTotalTimeoutConstant = (write_timeout == -1 ? MaxDWord : (uint) write_timeout);
		}

	}

	[StructLayout (LayoutKind.Sequential)]
	struct CommStat
	{
		public uint flags;
		public uint BytesIn;
		public uint BytesOut;
	}
}

#endif

