/* -*- Mode: Csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */

#if NET_2_0

using System.Text;
using System.Runtime.InteropServices;

namespace System.IO.Ports
{
	public enum Handshake
	{
		None,                 /* No control is used for the handshake */
		RequestToSend,        /* Request-to-Send (RTS) hardware flow
				       * control is used. RTS is used to signal
				       * that data is available for
				       * transmission. */
		RequestToSendXOnXOff, /* Both the Request-to-Send (RTS) hardware
				       * control and the XON/XOFF software
				       * controls are used. */
		XOnXOff               /* The XON/XOFF software control protocol is
				       * used.  XOFF is a software control sent to
				       * stop the transmission of data and the XON
				       * control is sent to resume the
				       * transmission.  These controls are used
				       * instead of the Request to Send (RTS) and
				       * Clear to Send (CTS) hardware controls. */
	}

	public enum Parity
	{
		Even, /* Sets the parity bit so that the count of bits set is an even number */
		Mark, /* Leaves the parity bit set to 1. */
		None, /* No parity check occurs */
		Odd,  /* Sets the parity bit so that the count of bits set is an odd number */
		Space /* Leaves the parity bit set to 0 */
	}

	public enum StopBits
	{
		One,          /* One stop bit is used */
		OnePointFive, /* Three stop bits are used. */
		Two           /* Two stop bits are used. */
	}

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
		string newLine    = "\n";
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
	  
		public int BaudRate
		{
			get {
				return baudRate;
			}
			set {
				this.baudRate = value;
				set_attributes (unixFd, baudRate, parity, dataBits, stopBits, handshake);
			}
		}

		public bool BreakState
		{
			get {
				return breakState;
			}
			set {
				if (!isOpen)
					throw new InvalidOperationException ();

				throw new NotImplementedException ();
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

		public int DataBits
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				if (value < 5 || value > 8)
					throw new ArgumentOutOfRangeException ();

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

		public Encoding Encoding
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public Handshake Handshake
		{
			get {
				if (!isOpen)
					throw new InvalidOperationException ();

				return handshake;
			}
			set {
				if (!isOpen)
					throw new InvalidOperationException ();

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

		public string NewLine
		{
			get {
				return newLine;
			}
			set {
				newLine = value;
			}
		}

		public Parity Parity
		{
			get {
				if (!isOpen)
					throw new InvalidOperationException ();

				return parity;
			}
			set {
				if (!isOpen)
					throw new InvalidOperationException ();

				this.parity = value;

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

		public string PortName
		{
			get {
				return portName;
			}
			set {
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
					throw new ArgumentOutOfRangeException ();

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
					throw new ArgumentOutOfRangeException ();

				readTimeout = value;
			}
		}

		public int ReceivedBytesThreshold
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ();

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

		public StopBits StopBits
		{
			get {
				return stopBits;
			}
			set {
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
					throw new ArgumentOutOfRangeException ();

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
					throw new ArgumentOutOfRangeException ();

				writeTimeout = value;
			}
		}

		// methods

		[DllImport("MonoPosixHelper")]
		private static extern void close_serial (int unixFd);
		public void Close ()
		{
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
			return read_serial (unixFd, buffer, offset, count, readTimeout);
		}

		public int Read (char[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}

		public void Write (byte[] buffer, int offset, int count)
		{
			write_serial (unixFd, buffer, offset, count, writeTimeout);
		}

		public void Write (char[] buffer, int offset, int count)
		{
			throw new NotImplementedException ();
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
};

#endif /* NET_2_0 */
