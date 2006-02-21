/* -*- Mode: Csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */

#if NET_2_0

using System;
using System.Text;
using System.Runtime.InteropServices;

namespace System.IO.Ports
{
	public class SerialPort /* : Component */
	{

		public const int InfiniteTimeout = -1;

		bool   isOpen     = false;
		int    baudRate   = 9600;
		Parity parity     = Parity.None;
		StopBits stopBits = StopBits.One;
		Handshake handshake = Handshake.None;
		int    dataBits   = 8;
		bool   breakState = false;
		Stream baseStream;
		Encoding encoding = Encoding.ASCII;
		string newLine    = Environment.NewLine;
		string portName;
		int    unixFd;
		int    readTimeout = InfiniteTimeout;
		int    writeTimeout = InfiniteTimeout;

		private class SerialPortStream : Stream
		{
			SerialPort port;

			public SerialPortStream (SerialPort port)
			{
				this.port = port;
			}

			public override bool CanRead
			{
				get {
					return true;
				}
			}

			public override bool CanSeek
			{
				get {
					return false;
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
					return -1;
				}
			}

			public override long Position
			{
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

		public SerialPort ()
		{
			throw new NotImplementedException ();
		}

		/*
		  IContainer is in 2.0?
		  public SerialPort (IContainer container) {
		  }
		*/

		public SerialPort (string portName)
		{
			this.portName = portName;
		}

		public SerialPort (string portName, int baudRate)
		{
			this.portName = portName;
			this.baudRate = baudRate;
		}

		public SerialPort (string portName, int baudRate, Parity parity)
		{
			this.portName = portName;
			this.baudRate = baudRate;
			this.parity = parity;
		}

		public SerialPort (string portName, int baudRate, Parity parity, int dataBits)
		{
			this.portName = portName;
			this.baudRate = baudRate;
			this.parity = parity;
			this.dataBits = dataBits;
		}

		public SerialPort (string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) {
			this.portName = portName;
			this.baudRate = baudRate;
			this.parity = parity;
			this.dataBits = dataBits;
			this.stopBits = stopBits;
		}

		public Stream BaseStream
		{
			get {
				if (!isOpen)
					throw new InvalidOperationException ();

				if (baseStream == null)
					baseStream = new SerialPortStream (this);

				return baseStream;
			}
		}

		[DllImport("MonoPosixHelper")]
		private static extern bool set_attributes (int unix_fd, int baud_rate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake);
	  
		public int BaudRate {
			get {
				return baudRate;
			}
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");
				
				baudRate = value;
				set_attributes (unixFd, baudRate, parity, dataBits, stopBits, handshake);
			}
		}

		public bool BreakState {
			get {
				return breakState;
			}
			set {
				if (!isOpen)
					throw new InvalidOperationException ();
				if (value == breakState)
					return; // Do nothing.

				breakState = value;
				// Update the state
			}
		}

		public int BytesToRead
		{
			get {
				if (!isOpen)
					throw new InvalidOperationException ();

				throw new NotImplementedException ();
			}
		}

		public int BytesToWrite
		{
			get {
				if (!isOpen)
					throw new InvalidOperationException ();

				throw new NotImplementedException ();
			}
		}

		public bool CDHolding
		{
			get {
				if (!isOpen)
					throw new InvalidOperationException ();

				throw new NotImplementedException ();
			}
		}

		public bool CtsHolding
		{
			get {
				if (!isOpen)
					throw new InvalidOperationException ();

				throw new NotImplementedException ();
			}
		}

		public int DataBits {
			get {
				return dataBits;
			}
			set {
				if (value < 5 || value > 8)
					throw new ArgumentOutOfRangeException ("value");

				dataBits = value;
				set_attributes (unixFd, baudRate, parity, dataBits, stopBits, handshake);
			}
		}

		public bool DiscardNull
		{
			get {
				if (!isOpen)
					throw new InvalidOperationException ();

				throw new NotImplementedException ();
			}
			set {
				if (!isOpen)
					throw new InvalidOperationException ();

				throw new NotImplementedException ();
			}
		}

		public bool DsrHolding
		{
			get {
				if (!isOpen)
					throw new InvalidOperationException ();

				throw new NotImplementedException ();
			}
		}

		public bool DtrEnable
		{
			get {
				throw new NotImplementedException ();
			}
		}

		public Encoding Encoding {
			get {
				return encoding;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");

				encoding = value;
			}
		}

		public Handshake Handshake {
			get {
				if (!isOpen)
					throw new InvalidOperationException ();

				return handshake;
			}
			set {
				if (!isOpen)
					throw new InvalidOperationException ();
				if (value < Handshake.None || value > Handshake.RequestToSendXOnXOff)
					throw new ArgumentOutOfRangeException ("value");

				handshake = value;

				set_attributes (unixFd, baudRate, parity, dataBits, stopBits, handshake);
			}
		}

		public bool IsOpen
		{
			get {
				return isOpen;
			}
		}

		public string NewLine {
			get {
				return newLine;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				
				newLine = value;
			}
		}

		public Parity Parity {
			get {
				if (!isOpen)
					throw new InvalidOperationException ();

				return parity;
			}
			set {
				if (!isOpen)
					throw new InvalidOperationException ();
				if (value < Parity.None || value > Parity.Space)
					throw new ArgumentOutOfRangeException ("value");

				parity = value;
				set_attributes (unixFd, baudRate, parity, dataBits, stopBits, handshake);
			}
		}

		public byte ParityReplace
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public string PortName {
			get {
				return portName;
			}
			set {
				if (isOpen)
					throw new InvalidOperationException ();
				if (value == null)
					throw new ArgumentNullException ("value");
				if (value.Length == 0 || value.StartsWith ("\\"))
					throw new ArgumentException ("value");
				
				throw new NotImplementedException ();
			}
		}

		public int ReadBufferSize
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");

				throw new NotImplementedException ();
			}
		}

		public int ReadTimeout
		{
			get {
				return readTimeout;
			}
			set {
				if (value <= 0 && value != InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				readTimeout = value;
			}
		}

		public int ReceivedBytesThreshold {
			get {
				throw new NotImplementedException ();
			}
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");

				throw new NotImplementedException ();
			}
		}

		public bool RtsEnable
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public StopBits StopBits {
			get {
				return stopBits;
			}
			set {
				if (value < StopBits.One || value > StopBits.OnePointFive)
					throw new ArgumentOutOfRangeException ("value");
				
				this.stopBits = value;
				set_attributes (unixFd, baudRate, parity, dataBits, stopBits, handshake);
			}
		}

		public int WriteBufferSize
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");

				throw new NotImplementedException ();
			}
		}

		public int WriteTimeout
		{
			get {
				return writeTimeout;
			}
			set {
				if (value <= 0 && value != InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				writeTimeout = value;
			}
		}

		// methods

		[DllImport("MonoPosixHelper")]
		private static extern void close_serial (int unixFd);
		public void Close ()
		{
			if (!isOpen)
				return;
			
			isOpen = false;

			close_serial (unixFd);
		}

		[DllImport("MonoPosixHelper")]
		private static extern void discard_buffer (int unixFd, bool input_buffer);
		public void DiscardInBuffer ()
		{
			if (!isOpen)
				throw new InvalidOperationException ();
			discard_buffer (unixFd, true);
		}

		public void DiscardOutBuffer ()
		{
			if (!isOpen)
				throw new InvalidOperationException ();
			discard_buffer (unixFd, false);
		}

		[DllImport("MonoPosixHelper")]
		private static extern string[] list_serial_devices ();
		public static string[] GetPortNames()
		{
			return list_serial_devices ();
		}

		[DllImport("MonoPosixHelper")]
		private static extern int open_serial (string portName);

		public void Open ()
		{
			if (portName == null || portName.StartsWith ("\\\\"))
				throw new ArgumentException ();

			unixFd = open_serial (portName);
			if (unixFd == -1)
				throw new IOException();

			set_attributes (unixFd, baudRate, parity, dataBits, stopBits, handshake);

			isOpen = true;
		}

		[DllImport("MonoPosixHelper")]
		private static extern int read_serial (int unixFd, byte[] buffer, int offset, int count, int timeout);

		public int Read (byte[] buffer, int offset, int count)
		{
			if (!isOpen)
				throw new InvalidOperationException ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
			if (count > buffer.Length - offset)
				throw new ArgumentException ("count > buffer.Length - offset");
			
			return read_serial (unixFd, buffer, offset, count, readTimeout);
		}

		public int Read (char[] buffer, int offset, int count)
		{
			if (!isOpen)
				throw new InvalidOperationException ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
			if (count > buffer.Length - offset)
				throw new ArgumentException ("count > buffer.Length - offset");

			byte [] bytes = encoding.GetBytes (buffer, offset, count);
			return read_serial (unixFd, bytes, 0, bytes.Length, readTimeout);
		}

		byte[] read_buffer = new byte[4096];

		public int ReadByte ()
		{
			if (Read (read_buffer, 0, 1) == 1)
				return read_buffer [0];
			
			return -1;
		}

		public int ReadChar ()
		{
			throw new NotImplementedException ();
		}

		public string ReadExisting ()
		{
			throw new NotImplementedException ();
		}

		public string ReadLine ()
		{
			return ReadTo (newLine);
		}

		public string ReadTo (string value)
		{
			throw new NotImplementedException ();
		}

		[DllImport("MonoPosixHelper")]
		private static extern void write_serial (int unixFd, byte[] buffer, int offset, int count, int timeout);

		public void Write (string str)
		{
			if (str == null)
				throw new ArgumentNullException ("str");
			if (!isOpen)
				throw new InvalidOperationException ("Specified port is not open");
			
			byte [] buffer = encoding.GetBytes (str);
			Write (buffer, 0, buffer.Length);
		}

		public void Write (byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
			if (offset + count > buffer.Length)
				throw new ArgumentException ("offset+count > buffer.Length");
			
			if (!isOpen)
				throw new InvalidOperationException ("Specified port is not open");
			
			write_serial (unixFd, buffer, offset, count, writeTimeout);
		}

		public void Write (char[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
			if (offset + count > buffer.Length)
				throw new ArgumentException ("offset+count > buffer.Length");

			if (!isOpen)
				throw new InvalidOperationException ("Specified port is not open");
			
			byte [] bytes = encoding.GetBytes (buffer, offset, count);
			write_serial (unixFd, bytes, offset, count, writeTimeout);
		}

		public void WriteLine (string str)
		{
			Write (str + newLine);
		}

		// events

		public delegate void SerialReceivedEventHandler (object sender, SerialReceivedEventArgs e);
		public delegate void SerialPinChangedEventHandler (object sender, SerialPinChangedEventArgs e);
		public delegate void SerialErrorEventHandler (object sender, SerialErrorEventArgs e);

		public event SerialErrorEventHandler ErrorEvent;
		public event SerialPinChangedEventHandler PinChangedEvent;
		public event SerialReceivedEventHandler ReceivedEvent;
	}
}

#endif
