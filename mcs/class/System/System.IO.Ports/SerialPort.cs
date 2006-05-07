/* -*- Mode: Csharp; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*- */

#if NET_2_0

using System;
using System.ComponentModel;
using System.Text;
using System.Runtime.InteropServices;

namespace System.IO.Ports
{
	public class SerialPort : Component
	{
		public const int InfiniteTimeout = -1;
		const int DefaultReadBufferSize = 4096;
		const int DefaultWriteBufferSize = 2048;
		const int DefaultBaudRate = 9600;
		const int DefaultDataBits = 8;
		const Parity DefaultParity = Parity.None;
		const StopBits DefaultStopBits = StopBits.One;

		bool is_open;
		int baud_rate;
		Parity parity;
		StopBits stop_bits;
		Handshake handshake;
		int data_bits;
		bool break_state = false;
		bool dtr_enable = false;
		bool rts_enable = false;
		ISerialStream stream;
		Encoding encoding = Encoding.ASCII;
		string new_line = Environment.NewLine;
		string port_name;
		int read_timeout = InfiniteTimeout;
		int write_timeout = InfiniteTimeout;
		int readBufferSize = DefaultReadBufferSize;
		int writeBufferSize = DefaultWriteBufferSize;
		object error_received = new object ();
		object data_received = new object ();
		object pin_changed = new object ();
		
		static string default_port_name = "ttyS0";

		public SerialPort () : 
			this (GetDefaultPortName (), DefaultBaudRate, DefaultParity, DefaultDataBits, DefaultStopBits)
		{
		}

		/*
		  IContainer is in 2.0?
		  public SerialPort (IContainer container) {
		  }
		*/

		public SerialPort (string portName) :
			this (portName, DefaultBaudRate, DefaultParity, DefaultDataBits, DefaultStopBits)
		{
		}

		public SerialPort (string portName, int baudRate) :
			this (portName, baudRate, DefaultParity, DefaultDataBits, DefaultStopBits)
		{
		}

		public SerialPort (string portName, int baudRate, Parity parity) :
			this (portName, baudRate, parity, DefaultDataBits, DefaultStopBits)
		{
		}

		public SerialPort (string portName, int baudRate, Parity parity, int dataBits) :
			this (portName, baudRate, parity, dataBits, DefaultStopBits)
		{
		}

		public SerialPort (string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) 
		{
			port_name = portName;
			baud_rate = baudRate;
			data_bits = dataBits;
			stop_bits = stopBits;
			this.parity = parity;
		}

		static string GetDefaultPortName ()
		{
			return default_port_name;
		}

		public Stream BaseStream {
			get {
				if (!is_open)
					throw new InvalidOperationException ();

				return (Stream) stream;
			}
		}

		public int BaudRate {
			get {
				return baud_rate;
			}
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");
				
				if (is_open)
					stream.SetAttributes (value, parity, data_bits, stop_bits, handshake);
				
				baud_rate = value;
			}
		}

		public bool BreakState {
			get {
				return break_state;
			}
			set {
				CheckOpen ();
				if (value == break_state)
					return; // Do nothing.

				break_state = value;
				// Update the state
			}
		}

		public int BytesToRead {
			get {
				CheckOpen ();
				return stream.BytesToRead;
			}
		}

		public int BytesToWrite {
			get {
				CheckOpen ();
				return stream.BytesToWrite;
			}
		}

		public bool CDHolding {
			get {
				CheckOpen ();
				return (stream.GetSignals () & SerialSignal.Cd) != 0;
			}
		}

		public bool CtsHolding {
			get {
				CheckOpen ();
				return (stream.GetSignals () & SerialSignal.Cts) != 0;
			}
		}

		public int DataBits {
			get {
				return data_bits;
			}
			set {
				if (value < 5 || value > 8)
					throw new ArgumentOutOfRangeException ("value");

				if (is_open)
					stream.SetAttributes (baud_rate, parity, value, stop_bits, handshake);
				
				data_bits = value;
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
				return (stream.GetSignals () & SerialSignal.Dsr) != 0;
			}
		}

		public bool DtrEnable {
			get {
				return dtr_enable;
			}
			set {
				if (value == dtr_enable)
					return;
				if (is_open)
					stream.SetSignal (SerialSignal.Dtr, value);
				
				dtr_enable = value;
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

				if (is_open)
					stream.SetAttributes (baud_rate, parity, data_bits, stop_bits, value);
				
				handshake = value;
			}
		}

		public bool IsOpen {
			get {
				return is_open;
			}
		}

		public string NewLine {
			get {
				return new_line;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				
				new_line = value;
			}
		}

		public Parity Parity {
			get {
				return parity;
			}
			set {
				if (value < Parity.None || value > Parity.Space)
					throw new ArgumentOutOfRangeException ("value");

				if (is_open)
					stream.SetAttributes (baud_rate, value, data_bits, stop_bits, handshake);
				
				parity = value;
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
				return port_name;
			}
			set {
				if (is_open)
					throw new InvalidOperationException ("Port name cannot be set while port is open.");
				if (value == null)
					throw new ArgumentNullException ("value");
				if (value.Length == 0 || value.StartsWith ("\\\\"))
					throw new ArgumentException ("value");

				port_name = value;
			}
		}

		public int ReadBufferSize {
			get {
				return readBufferSize;
			}
			set {
				if (is_open)
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
				return read_timeout;
			}
			set {
				if (value <= 0 && value != InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				if (is_open)
					stream.ReadTimeout = value;
				
				read_timeout = value;
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
				return rts_enable;
			}
			set {
				if (value == rts_enable)
					return;
				if (is_open)
					stream.SetSignal (SerialSignal.Rts, value);
				
				rts_enable = value;
			}
		}

		public StopBits StopBits {
			get {
				return stop_bits;
			}
			set {
				if (value < StopBits.One || value > StopBits.OnePointFive)
					throw new ArgumentOutOfRangeException ("value");
				
				if (is_open)
					stream.SetAttributes (baud_rate, parity, data_bits, value, handshake);
				
				stop_bits = value;
			}
		}

		public int WriteBufferSize {
			get {
				return writeBufferSize;
			}
			set {
				if (is_open)
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
				return write_timeout;
			}
			set {
				if (value <= 0 && value != InfiniteTimeout)
					throw new ArgumentOutOfRangeException ("value");

				if (is_open)
					stream.WriteTimeout = value;
				
				write_timeout = value;
			}
		}

		// methods

		public void Close ()
		{
			Dispose (false);
		}

		protected override void Dispose (bool disposing)
		{
			if (!is_open)
				return;
			
			is_open = false;
			stream.Close ();
			stream = null;
		}

		public void DiscardInBuffer ()
		{
			CheckOpen ();
			stream.DiscardInBuffer ();
		}

		public void DiscardOutBuffer ()
		{
			CheckOpen ();
			stream.DiscardOutBuffer ();
		}

		public static string [] GetPortNames ()
		{
			int p = (int) Environment.OSVersion.Platform;
			if (p == 4 || p == 128) // Are we on Unix?
				return Directory.GetFiles ("/dev/", "ttyS*");

			throw new NotImplementedException ("Detection of ports is not implemented for this platform yet.");
		}

		public void Open ()
		{
			if (is_open)
				throw new InvalidOperationException ("Port is already open");
			
			stream = new SerialPortStream (port_name, baud_rate, data_bits, parity, stop_bits, dtr_enable,
					rts_enable, handshake, read_timeout, write_timeout, readBufferSize, writeBufferSize);
			is_open = true;
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
			
			return stream.Read (buffer, offset, count);
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
			return stream.Read (bytes, 0, bytes.Length);
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
			return ReadTo (new_line);
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
			Write (str + new_line);
		}

		void CheckOpen ()
		{
			if (!is_open)
				throw new InvalidOperationException ("Specified port is not open.");
		}

		internal void OnErrorReceived (SerialErrorReceivedEventArgs args)
		{
			SerialErrorReceivedEventHandler handler =
				(SerialErrorReceivedEventHandler) Events [error_received];

			if (handler != null)
				handler (this, args);
		}

		internal void OnDataReceived (SerialDataReceivedEventArgs args)
		{
			SerialDataReceivedEventHandler handler =
				(SerialDataReceivedEventHandler) Events [data_received];

			if (handler != null)
				handler (this, args);
		}
		
		internal void OnDataReceived (SerialPinChangedEventArgs args)
		{
			SerialPinChangedEventHandler handler =
				(SerialPinChangedEventHandler) Events [pin_changed];

			if (handler != null)
				handler (this, args);
		}

		// events
		public event SerialErrorReceivedEventHandler ErrorReceived {
			add { Events.AddHandler (error_received, value); }
			remove { Events.RemoveHandler (error_received, value); }
		}
		
		public event SerialPinChangedEventHandler PinChanged {
			add { Events.AddHandler (pin_changed, value); }
			remove { Events.RemoveHandler (pin_changed, value); }
		}
		
		public event SerialDataReceivedEventHandler DataReceived {
			add { Events.AddHandler (data_received, value); }
			remove { Events.RemoveHandler (data_received, value); }
		}
	}

	public delegate void SerialDataReceivedEventHandler (object sender, SerialDataReceivedEventArgs e);
	public delegate void SerialPinChangedEventHandler (object sender, SerialPinChangedEventArgs e);
	public delegate void SerialErrorReceivedEventHandler (object sender, SerialErrorReceivedEventArgs e);

}

#endif
