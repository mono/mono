//
// Mono.Data.Tds.Protocol.TdsComm.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Mono.Data.Tds.Protocol {
        internal sealed class TdsComm
	{
		#region Fields

		NetworkStream stream;
		int packetSize;
		TdsPacketType packetType = TdsPacketType.None;
		Encoding encoder;

		string dataSource;
		int commandTimeout;
		int connectionTimeout;

		byte[] outBuffer;
		int outBufferLength;
		int nextOutBufferIndex = 0;

		byte[] inBuffer;
		int inBufferLength;
		int inBufferIndex = 0;

		static int headerLength = 8;

		byte[] tmpBuf = new byte[8];
		byte[] resBuffer = new byte[256];

		int packetsSent = 0;
		int packetsReceived = 0;

		Socket socket;
		TdsVersion tdsVersion;

		ManualResetEvent connected = new ManualResetEvent (false);
		
		#endregion // Fields
		
		#region Constructors

		[MonoTODO ("Fix when asynchronous socket connect works on Linux.")]		
		public TdsComm (string dataSource, int port, int packetSize, int timeout, TdsVersion tdsVersion)
		{
			this.packetSize = packetSize;
			this.tdsVersion = tdsVersion;
			this.dataSource = dataSource;
			this.connectionTimeout = timeout;

			outBuffer = new byte[packetSize];
			inBuffer = new byte[packetSize];

			outBufferLength = packetSize;
			inBufferLength = packetSize;

			socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPHostEntry hostEntry = Dns.Resolve (dataSource);
			IPEndPoint endPoint;
			endPoint = new IPEndPoint (hostEntry.AddressList [0], port);

			// This replaces the code below for now
			socket.Connect (endPoint);

			/*
			FIXME: Asynchronous socket connection doesn't work right on linux, so comment 
			       this out for now.  This *does* do the right thing on windows

			connected.Reset ();
			IAsyncResult asyncResult = socket.BeginConnect (endPoint, new AsyncCallback (ConnectCallback), socket);

			if (timeout > 0 && !connected.WaitOne (new TimeSpan (0, 0, timeout), true))
				throw Tds.CreateTimeoutException (dataSource, "Open()");
			else if (timeout > 0 && !connected.WaitOne ())
				throw Tds.CreateTimeoutException (dataSource, "Open()");
			*/

			stream = new NetworkStream (socket);
		}
		
		#endregion // Constructors
		
		#region Properties

		public int CommandTimeout {
			get { return commandTimeout; }
			set { commandTimeout = value; }
		}

		internal Encoding Encoder {
			set { encoder = value; }
		}
		
		public int PacketSize {
			get { return packetSize; }
			set { packetSize = value; }
		}
		
		#endregion // Properties
		
		#region Methods

		public byte[] Swap(byte[] toswap) {
			byte[] ret = new byte[toswap.Length];
			for(int i = 0; i < toswap.Length; i++)
				ret [toswap.Length - i - 1] = toswap[i];

			return ret;
		}
		public void Append (object o)
		{
			switch (o.GetType ().ToString ()) {
			case "System.Byte":
				Append ((byte) o);
				return;
			case "System.Byte[]":
				Append ((byte[]) o);
				return;
			case "System.Int16":
				Append ((short) o);
				return;
			case "System.Int32":
				Append ((int) o);
				return;
			case "System.String":
				Append ((string) o);
				return;
			case "System.Double":
				Append ((double) o);
				return;
			case "System.Int64":
				Append ((long) o);
				return;
			}
		}

		public void Append (byte b)
		{
			if (nextOutBufferIndex == outBufferLength) {
				SendPhysicalPacket (false);
				nextOutBufferIndex = headerLength;
			}
			Store (nextOutBufferIndex, b);
			nextOutBufferIndex++;
		}	
		
		public void Append (byte[] b)
		{
			Append (b, b.Length, (byte) 0);
		}		

		public void Append (byte[] b, int len, byte pad)
		{
			int i = 0;
			for ( ; i < b.Length && i < len; i++)
			    Append (b[i]);

			for ( ; i < len; i++)
			    Append (pad);
		}	

		public void Append (short s)
		{
			if(!BitConverter.IsLittleEndian)
				Append (Swap (BitConverter.GetBytes(s)));
			else 
				Append (BitConverter.GetBytes (s));
		}

		public void Append (int i)
		{
			if(!BitConverter.IsLittleEndian)
				Append (Swap (BitConverter.GetBytes(i)));
			else
				Append (BitConverter.GetBytes (i));
		}

		public void Append (string s)
		{
			if (tdsVersion < TdsVersion.tds70) 
				Append (encoder.GetBytes (s));
			else 
				foreach (char c in s)
					if(!BitConverter.IsLittleEndian)
						Append (Swap (BitConverter.GetBytes (c)));
					else
						Append (BitConverter.GetBytes (c));
		}	

		// Appends with padding
		public byte[] Append (string s, int len, byte pad)
		{
			if (s == null)
				return new byte[0];

			byte[] result = encoder.GetBytes (s);
			Append (result, len, pad);
			return result;
		}

		public void Append (double value)
		{
			Append (BitConverter.DoubleToInt64Bits (value));
		}

		public void Append (long l)
		{
			if (tdsVersion < TdsVersion.tds70) {
				Append ((byte) (((byte) (l >> 56)) & 0xff));
				Append ((byte) (((byte) (l >> 48)) & 0xff));
				Append ((byte) (((byte) (l >> 40)) & 0xff));
				Append ((byte) (((byte) (l >> 32)) & 0xff));
				Append ((byte) (((byte) (l >> 24)) & 0xff));
				Append ((byte) (((byte) (l >> 16)) & 0xff));
				Append ((byte) (((byte) (l >> 8)) & 0xff));
				Append ((byte) (((byte) (l >> 0)) & 0xff));
			}
			else 
				if (!BitConverter.IsLittleEndian)
					Append (Swap (BitConverter.GetBytes (l)));
				else
					Append (BitConverter.GetBytes (l));
		}

		public void Close ()
		{
			stream.Close ();
		}

		private void ConnectCallback (IAsyncResult ar)
		{
			Socket s = (Socket) ar.AsyncState;
			if (Poll (s, connectionTimeout, SelectMode.SelectWrite)) {
				socket.EndConnect (ar);
				connected.Set ();
			}
                }

		public byte GetByte ()
		{
			byte result;

			if (inBufferIndex >= inBufferLength) {
				// out of data, read another physical packet.
				GetPhysicalPacket ();
			}

			result = inBuffer[inBufferIndex++];
			return result;
		}

		public byte[] GetBytes (int len, bool exclusiveBuffer)
		{
			byte[] result = null;
			int i;

			// Do not keep an internal result buffer larger than 16k.
			// This would unnecessarily use up memory.
			if (exclusiveBuffer || len > 16384)
				result = new byte[len];
			else
			{
				if (resBuffer.Length < len)
					resBuffer = new byte[len];
				result = resBuffer;
			}

			for (i = 0; i<len; )
			{
				if (inBufferIndex >= inBufferLength)
					GetPhysicalPacket ();

				int avail = inBufferLength - inBufferIndex;
				avail = avail>len-i ? len-i : avail;

				System.Array.Copy (inBuffer, inBufferIndex, result, i, avail);
				i += avail;
				inBufferIndex += avail;
			}

			return result;
		}

		public string GetString (int len)
		{
			if (tdsVersion == TdsVersion.tds70) 
				return GetString (len, true);
			else
				return GetString (len, false);
		}

		public string GetString (int len, bool wide)
		{
			if (wide) {
				char[] chars = new char[len];
				for (int i = 0; i < len; ++i) {
					int lo = ((byte) GetByte ()) & 0xFF;
					int hi = ((byte) GetByte ()) & 0xFF;
					chars[i] = (char) (lo | ( hi << 8));
				}
				return new String (chars);
			}
			else {
				byte[] result = new byte[len];
				Array.Copy (GetBytes (len, false), result, len);
				return (encoder.GetString (result));
			}
		}

		public int GetNetShort ()
		{
			byte[] tmp = new byte[2];
			tmp[0] = GetByte ();
			tmp[1] = GetByte ();
			return Ntohs (tmp, 0);
		}

		public short GetTdsShort ()
		{
			byte[] input = new byte[2];

			for (int i = 0; i < 2; i += 1)
				input[i] = GetByte ();
			if(!BitConverter.IsLittleEndian)
				return (BitConverter.ToInt16 (Swap (input), 0));
			else
				return (BitConverter.ToInt16 (input, 0));
		}


		public int GetTdsInt ()
		{
			byte[] input = new byte[4];
			for (int i = 0; i < 4; i += 1)
				input[i] = GetByte ();
			if(!BitConverter.IsLittleEndian)
				return (BitConverter.ToInt32 (Swap (input), 0));
			else
				return (BitConverter.ToInt32 (input, 0));
		}

		public long GetTdsInt64 ()
		{
			byte[] input = new byte[8];
			for (int i = 0; i < 8; i += 1)
				input[i] = GetByte ();
			if(!BitConverter.IsLittleEndian)
				return (BitConverter.ToInt64 (Swap (input), 0));
			else
				return (BitConverter.ToInt64 (input, 0));
		}

		private void GetPhysicalPacket ()
		{
			int nread = 0;

			// read the header
			while (nread < 8)
				nread += stream.Read (tmpBuf, nread, 8 - nread);

			TdsPacketType packetType = (TdsPacketType) tmpBuf[0];
			if (packetType != TdsPacketType.Logon && packetType != TdsPacketType.Query && packetType != TdsPacketType.Reply) 
			{
				throw new Exception (String.Format ("Unknown packet type {0}", tmpBuf[0]));
			}

			// figure out how many bytes are remaining in this packet.
			int len = Ntohs (tmpBuf, 2) - 8;

			if (len >= inBuffer.Length) 
				inBuffer = new byte[len];

			if (len < 0) {
				throw new Exception (String.Format ("Confused by a length of {0}", len));
			}

			// now get the data
			nread = 0;
			while (nread < len) {
				nread += stream.Read (inBuffer, nread, len - nread);
			}

			packetsReceived++;

			// adjust the bookkeeping info about the incoming buffer
			inBufferLength = len;
			inBufferIndex = 0;
		}

		private static int Ntohs (byte[] buf, int offset)
		{
			int lo = ((int) buf[offset + 1] & 0xff);
			int hi = (((int) buf[offset] & 0xff ) << 8);

			return hi | lo;
			// return an int since we really want an _unsigned_
		}		

		public byte Peek ()
		{
			// If out of data, read another physical packet.
			if (inBufferIndex >= inBufferLength)
				GetPhysicalPacket ();

			return inBuffer[inBufferIndex];
		}

		public bool Poll (int seconds, SelectMode selectMode)
		{
			return Poll (socket, seconds, selectMode);
		}

		private bool Poll (Socket s, int seconds, SelectMode selectMode)
		{
			long uSeconds = seconds * 1000000;
			bool bState = false;

			while (uSeconds > (long) Int32.MaxValue) {
				bState = s.Poll (Int32.MaxValue, selectMode);
				if (bState) 
					return true;
				uSeconds -= Int32.MaxValue;
			}
			return s.Poll ((int) uSeconds, selectMode);
		}

		internal void ResizeOutBuf (int newSize)
		{
			if (newSize > outBufferLength) {
				byte[] newBuf = new byte [newSize];
				Array.Copy (outBuffer, 0, newBuf, 0, outBufferLength);
				outBufferLength = newSize;
				outBuffer = newBuf;
			}
		}

		public void SendPacket ()
		{
			SendPhysicalPacket (true);
			nextOutBufferIndex = 0;
			packetType = TdsPacketType.None;
		}
		
		private void SendPhysicalPacket (bool isLastSegment)
		{
			if (nextOutBufferIndex > headerLength || packetType == TdsPacketType.Cancel) {
				// packet type
				Store (0, (byte) packetType);
				Store (1, (byte) (isLastSegment ? 1 : 0));
				Store (2, (short) nextOutBufferIndex );
				Store (4, (byte) 0);
				Store (5, (byte) 0);
				Store (6, (byte) (tdsVersion == TdsVersion.tds70 ? 0x1 : 0x0));
				Store (7, (byte) 0);

				stream.Write (outBuffer, 0, nextOutBufferIndex);
				stream.Flush ();
				packetsSent++;
			}
		}
		
		public void Skip (int i)
		{
			for ( ; i > 0; i--)
				GetByte ();
		}

		public void StartPacket (TdsPacketType type)
		{
			if (type != TdsPacketType.Cancel && inBufferIndex != inBufferLength)
				inBufferIndex = inBufferLength;

			packetType = type;
			nextOutBufferIndex = headerLength;
		}

		private void Store (int index, byte value)
		{
			outBuffer[index] = value;
		}		

		private void Store (int index, short value)
		{
			outBuffer[index] = (byte) (((byte) (value >> 8)) & 0xff);
			outBuffer[index + 1] = (byte) (((byte) (value >> 0)) & 0xff);
		}

		#endregion // Methods
	}

}
