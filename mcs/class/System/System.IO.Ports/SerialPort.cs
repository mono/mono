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
		const int DefaultReadBufferSize = 4096;
		const int DefaultWriteBufferSize = 2048;

		bool   isOpen     = false;
		int    baudRate   = 9600;
		Parity parity     = Parity.None;
		StopBits stopBits = StopBits.One;
		Handshake handshake = Handshake.None;
		int    dataBits   = 8;
		bool   breakState = false;
		bool dtr_enable = false;
		bool rts_enable = false;
		SerialPortStream stream;
		Encoding encoding = Encoding.ASCII;
		string newLine    = Environment.NewLine;
		string portName;
		int    readTimeout = InfiniteTimeout;
		int    writeTimeout = InfiniteTimeout;
		int readBufferSize = DefaultReadBufferSize;
		int writeBufferSize = DefaultWriteBufferSize;
		int readBufferOffset;
		int readBufferLength;
		int writeBufferLength;
		byte [] readBuffer;
		//byte [] writeBuffer;

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

		public SerialPort (string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) 
		{
			this.portName = portName;
			this.baudRate = baudRate;
			this.parity = parity;
			this.dataBits = dataBits;
			this.stopBits = stopBits;
		}

		public Stream BaseStream {
			get {
				if (!isOpen)
					throw new InvalidOperationException ();

				return stream;
			}
		}

		public int BaudRate {
			get {
				return baudRate;
			}
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");
				
				baudRate = value;
				if (isOpen)
					stream.BaudRate = value;
			}
		}

		public bool BreakState {
			get {
				return breakState;
			}
			set {
				CheckOpen ();
				if (value == breakState)
					return; // Do nothing.

				breakState = value;
				// Update the state
			}
		}

		public int BytesToRead {
			get {
				CheckOpen ();
				return readBufferLength + stream.BytesToRead;
			}
		}

		public int BytesToWrite {
			get {
				CheckOpen ();
				return writeBufferLength + stream.BytesToWrite;
			}
		}

		public bool CDHolding {
			get {
				CheckOpen ();
				throw new NotImplementedException ();
			}
		}

		public bool CtsHolding {
			get {
				CheckOpen ();
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
				if (isOpen)
					stream.DataBits = value;
			}
		}

		public bool DiscardNull {
			get {
				CheckOpen ();
				throw new NotImplementedException ();
			}
			set {
				CheckOpen ();
				throw new NotImplementedException ();
			}
		}

		public bool DsrHolding {
			get {
				CheckOpen ();
				throw new NotImplementedException ();
			}
		}

		public bool DtrEnable {
			get {
				throw new NotImplementedException ();
			}
			set {
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
				return handshake;
			}
			set {
				if (value < Handshake.None || value > Handshake.RequestToSendXOnXOff)
					throw new ArgumentOutOfRangeException ("value");

				handshake = value;
				if (isOpen)
					stream.Handshake = value;
			}
		}

		public bool IsOpen {
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
				return parity;
			}
			set {
				if (value < Parity.None || value > Parity.Space)
					throw new ArgumentOutOfRangeException ("value");

				parity = value;
				if (isOpen)
					stream.Parity = value;
			}
		}

		public byte ParityReplace {
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
					throw new InvalidOperationException ("Port name cannot be set while port is open.");
				if (value == null)
					throw new ArgumentNullException ("value");
				if (value.Length == 0 || value.StartsWith ("\\\\"))
					throw new ArgumentException ("value");

				portName = value;
			}
		}

		public int ReadBufferSize {
			get {
				return readBufferSize;
			}
			set {
				if (isOpen)
					throw new InvalidOperationException ();
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");
				if (value <= DefaultReadBufferSize)
					return;

				readBufferSize = value;
			}
		}

		public int ReadTimeout {
			get {
				return readTimeout;
			}
			set {
				if (value <= 0 && value != InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				readTimeout = value;
				if (isOpen)
					stream.ReadTimeout = value;
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

		public bool RtsEnable {
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
				
				stopBits = value;
				if (isOpen)
					stream.StopBits = value;
			}
		}

		public int WriteBufferSize {
			get {
				return writeBufferSize;
			}
			set {
				if (isOpen)
					throw new InvalidOperationException ();
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");
				if (value <= DefaultWriteBufferSize)
					return;

				writeBufferSize = value;
			}
		}

		public int WriteTimeout {
			get {
				return writeTimeout;
			}
			set {
				if (value <= 0 && value != InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				writeTimeout = value;
				if (isOpen)
					stream.WriteTimeout = value;
			}
		}

		// methods

		public void Close ()
		{
			isOpen = false;

			if (stream != null)
				stream.Close ();
			
			stream = null;
			readBuffer = null;
			//writeBuffer = null;
		}

		public void DiscardInBuffer ()
		{
			CheckOpen ();
			stream.DiscardInputBuffer ();
		}

		public void DiscardOutBuffer ()
		{
			CheckOpen ();
			stream.DiscardOutputBuffer ();
		}

		public static string [] GetPortNames ()
		{
			return new string [0]; // Return empty by now
		}

		public void Open ()
		{
			if (isOpen)
				throw new InvalidOperationException ("Port is already open");
			
			stream = new SerialPortStream (portName, baudRate, dataBits, parity, stopBits, dtr_enable,
					rts_enable, handshake, readTimeout, writeTimeout);
			isOpen = true;
			
			readBuffer = new byte [readBufferSize];
			//writeBuffer = new byte [writeBufferSize];
		}

		public int Read (byte[] buffer, int offset, int count)
		{
			CheckOpen ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
			if (count > buffer.Length - offset)
				throw new ArgumentException ("count > buffer.Length - offset");
			
			if (readBufferLength <= 0) {
				readBufferOffset = 0;
				readBufferLength = stream.Read (readBuffer, 0, readBuffer.Length);
			}
			
			if (readBufferLength == 0)
				return 0; // No bytes left
			
			if (count > readBufferLength)
				count = readBufferLength; // Update count if needed

			Buffer.BlockCopy (readBuffer, readBufferOffset, buffer, offset, count);
			readBufferOffset += count;
			readBufferLength -= count;

			return count;
		}

		public int Read (char[] buffer, int offset, int count)
		{
			CheckOpen ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
			if (count > buffer.Length - offset)
				throw new ArgumentException ("count > buffer.Length - offset");

			byte [] bytes = encoding.GetBytes (buffer, offset, count);
			return Read (bytes, 0, bytes.Length);
		}

		public int ReadByte ()
		{
			byte [] buff = new byte [1];
			if (Read (buff, 0, 1) > 0)
				return buff [0];

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
			CheckOpen ();
			if (value == null)
				throw new ArgumentNullException ("value");
			if (value.Length == 0)
				throw new ArgumentException ("value");

			throw new NotImplementedException ();
		}

		public void Write (string str)
		{
			CheckOpen ();
			if (str == null)
				throw new ArgumentNullException ("str");
			
			byte [] buffer = encoding.GetBytes (str);
			Write (buffer, 0, buffer.Length);
		}

		public void Write (byte [] buffer, int offset, int count)
		{
			CheckOpen ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
			if (count > buffer.Length - offset)
				throw new ArgumentException ("count > buffer.Length - offset");

			stream.Write (buffer, offset, count);
		}

		public void Write (char [] buffer, int offset, int count)
		{
			CheckOpen ();
			if (buffer == null)
				throw new ArgumentNullException ("buffer");
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException ("offset");
			if (count < 0 || count > buffer.Length)
				throw new ArgumentOutOfRangeException ("count");
			if (count > buffer.Length - offset)
				throw new ArgumentException ("count > buffer.Length - offset");

			byte [] bytes = encoding.GetBytes (buffer, offset, count);
			stream.Write (bytes, 0, bytes.Length);
		}

		public void WriteLine (string str)
		{
			Write (str + newLine);
		}

		void CheckOpen ()
		{
			if (!isOpen)
				throw new InvalidOperationException ("Specified port is not open.");
		}

		// events
#pragma warning disable 67
		public event SerialErrorReceivedEventHandler ErrorReceived;
		public event SerialPinChangedEventHandler PinChanged;
		public event SerialDataReceivedEventHandler DataReceived;
#pragma warning restore
	}

	public delegate void SerialDataReceivedEventHandler (object sender, SerialDataReceivedEventArgs e);
	public delegate void SerialPinChangedEventHandler (object sender, SerialPinChangedEventArgs e);
	public delegate void SerialErrorReceivedEventHandler (object sender, SerialErrorReceivedEventArgs e);

}

#endif
