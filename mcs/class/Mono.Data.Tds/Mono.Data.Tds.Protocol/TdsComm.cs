//
// Mono.Data.Tds.Protocol.TdsComm.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (C) 2002 Tim Coleman
// Copyright (c) 2009 Novell, Inc.
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
using System.IO;
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
		bool connReset;
		Encoding encoder;

		string dataSource;
		int commandTimeout;

		byte[] outBuffer;
		int outBufferLength;
		int nextOutBufferIndex = 0;
		bool lsb;

		byte[] inBuffer;
		int inBufferLength;
		int inBufferIndex = 0;

		static int headerLength = 8;

		byte[] tmpBuf = new byte[8];
		byte[] resBuffer = new byte[256];

		int packetsSent;
		int packetsReceived = 0;

		Socket socket;
		TdsVersion tdsVersion;

		#endregion // Fields
		
		#region Constructors

		public TdsComm (string dataSource, int port, int packetSize, int timeout, TdsVersion tdsVersion)
		{
			this.packetSize = packetSize;
			this.tdsVersion = tdsVersion;
			this.dataSource = dataSource;

			outBuffer = new byte[packetSize];
			inBuffer = new byte[packetSize];

			outBufferLength = packetSize;
			inBufferLength = packetSize;

			lsb = true;
			
			IPEndPoint endPoint;
			bool have_exception = false;
			
			try {
#if NET_2_0
				IPAddress ip;
				if(IPAddress.TryParse(this.dataSource, out ip)) {
					endPoint = new IPEndPoint(ip, port);
				} else {
					IPHostEntry hostEntry = Dns.GetHostEntry (this.dataSource);
					endPoint = new IPEndPoint(hostEntry.AddressList [0], port);
				}
#else
				IPHostEntry hostEntry = Dns.Resolve (this.dataSource);
				endPoint = new IPEndPoint (hostEntry.AddressList [0], port);
#endif
			} catch (SocketException e) {
				throw new TdsInternalException ("Server does not exist or connection refused.", e);
			}

			try {
				socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				IAsyncResult ares = socket.BeginConnect (endPoint, null, null);
				int timeout_ms = timeout * 1000;
				if (timeout > 0 && !ares.IsCompleted && !ares.AsyncWaitHandle.WaitOne (timeout_ms, false))
					throw Tds.CreateTimeoutException (dataSource, "Open()");
				socket.EndConnect (ares);
				try {
					// MS sets these socket option
					socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.KeepAlive, 1);
				} catch (SocketException) {
					// Some platform may throw an exception, so
					// eat all socket exception, yeaowww! 
				}

				try {
#if NET_2_0
					socket.NoDelay = true;
#endif
					socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.SendTimeout, timeout_ms);
					socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, timeout_ms);
				} catch {
					// Ignore exceptions here for systems that do not support these options.
				}
				// Let the stream own the socket and take the pleasure of closing it
				stream = new NetworkStream (socket, true);
			} catch (SocketException e) {
				have_exception = true;
				throw new TdsInternalException ("Server does not exist or connection refused.", e);
			} catch (Exception) {
				have_exception = true;
				throw;
			} finally {
				if (have_exception && socket != null) {
					try {
						Socket s = socket;
						socket = null;
						s.Close ();
					} catch {}
				}
			}
			if (!socket.Connected)
				throw new TdsInternalException ("Server does not exist or connection refused.", null);
			packetsSent = 1;
		}
		
		#endregion // Constructors
		
		#region Properties

		public int CommandTimeout {
			get { return commandTimeout; }
			set { commandTimeout = value; }
		}

		internal Encoding Encoder {
			get { return encoder; }
			set { encoder = value; }
		}
		
		public int PacketSize {
			get { return packetSize; }
			set { packetSize = value; }
		}
		
		public bool TdsByteOrder {
			get { return !lsb; }
			set { lsb = !value; }
		}
		#endregion // Properties
		
		#region Methods

		public byte[] Swap(byte[] toswap) {
			byte[] ret = new byte[toswap.Length];
			for(int i = 0; i < toswap.Length; i++)
				ret [toswap.Length - i - 1] = toswap[i];

			return ret;
		}

		public void SendIfFull ()
		{
			if (nextOutBufferIndex == outBufferLength) {
				SendPhysicalPacket (false);
				nextOutBufferIndex = headerLength;
			}
		}

		public void SendIfFull (int reserve) 
		{
			if (nextOutBufferIndex+reserve > outBufferLength) {
				SendPhysicalPacket (false);
				nextOutBufferIndex = headerLength;
			}
		}

		public void Append (object o)
		{
			if (o == null || o == DBNull.Value) {
				Append ((byte)0);
				return ;
			}

			switch (Type.GetTypeCode (o.GetType ())) {
			case TypeCode.Byte :
				Append ((byte) o);
				return;
			case TypeCode.Boolean:
				if ((bool)o == true)
					Append ((byte)1);
				else
					Append ((byte)0);
				return;
			case TypeCode.Object :
				if (o is byte[])
					Append ((byte[]) o);
				return;
			case TypeCode.Int16 :
				Append ((short) o);
				return;
			case TypeCode.Int32 :
				Append ((int) o);
				return;
			case TypeCode.String :
				Append ((string) o);
				return;
			case TypeCode.Double :
				Append ((double) o);
				return;
			case TypeCode.Single :
				Append ((float) o);
				return;
			case TypeCode.Int64 :
				Append ((long) o);
				return;
			case TypeCode.Decimal:
				Append ((decimal) o, 17);
				return;
			case TypeCode.DateTime:
				Append ((DateTime) o, 8);
				return;
			}
			throw new InvalidOperationException (String.Format ("Object Type :{0} , not being appended", o.GetType ()));
		}

		public void Append (byte b)
		{
			SendIfFull ();
			Store (nextOutBufferIndex, b);
			nextOutBufferIndex++;
		}	

		public void Append (DateTime t, int bytes)
		{
			DateTime epoch = new DateTime (1900,1,1);
			
			TimeSpan span = t - epoch; //new TimeSpan (t.Ticks - epoch.Ticks);
			int days, hours, minutes, secs;
			long msecs;
			int val = 0;	

			days = span.Days;
			hours = span.Hours;
			minutes = span.Minutes;
			secs = span.Seconds;
			msecs = span.Milliseconds;
			
			if (epoch > t) {
				// If t.Hour/Min/Sec/MSec is > 0, days points to the next day and hence, 
				// we move it back by a day - otherwise, no change
				days = (t.Hour > 0 || t.Minute > 0 || t.Second > 0 || t.Millisecond > 0) ? days-1: days;
				hours = t.Hour;
				minutes = t.Minute;
				secs = t.Second;
				msecs = t.Millisecond;
			}

			SendIfFull (bytes);
			if (bytes == 8) {
				long ms = (hours * 3600 + minutes * 60 + secs)*1000L + (long)msecs;
				val = (int) ((ms*300)/1000);
				AppendInternal ((int) days);
				AppendInternal ((int) val);
			} else if (bytes ==4) {
				val = span.Hours * 60 + span.Minutes;
				AppendInternal ((short) days);
				AppendInternal ((short) val);
			} else {
				throw new Exception ("Invalid No of bytes");
			}
		}

		public void Append (byte[] b)
		{
			Append (b, b.Length, (byte) 0);
		}

		
		public void Append (byte[] b, int len, byte pad)
		{
			int bufBytesToCopy = System.Math.Min (b.Length, len);
			int padBytesToCopy = len - bufBytesToCopy;
			int bufPos = 0;

			/* copy out of our input buffer in the largest chunks possible *
			 * at a time. limited only by the buffer size for our outgoing *
			 * packets.                                                    */

			while (bufBytesToCopy > 0)
			{
				SendIfFull ();

				int availBytes = outBufferLength - nextOutBufferIndex;
				int bufSize = System.Math.Min (availBytes, bufBytesToCopy);

				Buffer.BlockCopy (b, bufPos, outBuffer, nextOutBufferIndex, bufSize);

				nextOutBufferIndex += bufSize;
				bufBytesToCopy -= bufSize;
				bufPos += bufSize;
			}

			while (padBytesToCopy > 0)
			{
				SendIfFull ();

				int availBytes = outBufferLength - nextOutBufferIndex;
				int bufSize = System.Math.Min (availBytes, padBytesToCopy);

				for (int i = 0; i < bufSize; i++)
					outBuffer [nextOutBufferIndex++] = pad;

				padBytesToCopy -= bufSize;
			}
		}

		private void AppendInternal (short s)
		{
			if (!lsb) {
				outBuffer[nextOutBufferIndex++] = (byte) (((byte) (s >> 8)) & 0xff);
				outBuffer[nextOutBufferIndex++] = (byte) ((byte) (s & 0xff));
			} else {
				outBuffer[nextOutBufferIndex++] = (byte) ((byte) (s & 0xff));
				outBuffer[nextOutBufferIndex++] = (byte) (((byte) (s >> 8)) & 0xff);
			}
		}

		public void Append (short s)
		{
			SendIfFull (sizeof (short));
			AppendInternal (s);
		}

		public void Append (ushort s)
		{
			SendIfFull (sizeof (short));
			AppendInternal ((short) s);
		}

		private void AppendInternal (int i)
		{
			if (!lsb) {
				AppendInternal ((short) (((short) (i >> 16)) & 0xffff));
				AppendInternal ((short) ((short) (i & 0xffff)));
			} else {
				AppendInternal ((short) ((short) (i & 0xffff)));
				AppendInternal ((short) (((short) (i >> 16)) & 0xffff));
			}				
		}

		public void Append (int i)
		{
			SendIfFull (sizeof (int));
			AppendInternal (i);
		}

		public void Append (string s)
		{
			if (tdsVersion < TdsVersion.tds70) { 
				Append (encoder.GetBytes (s));
			} else {
				for (int i = 0; i < s.Length; i++) {
					SendIfFull (sizeof(short));
					AppendInternal ((short)s[i]);
				}
			}
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
			if (!lsb)
				Append (Swap (BitConverter.GetBytes (value)), sizeof(double), (byte)0);
			else
				Append (BitConverter.GetBytes (value), sizeof(double), (byte)0);
		}

		public void Append (float value)
		{
			if (!lsb)
				Append (Swap (BitConverter.GetBytes (value)), sizeof(float), (byte)0);
			else
				Append (BitConverter.GetBytes (value), sizeof(float), (byte)0);
		}

		public void Append (long l)
		{
			SendIfFull (sizeof (long));
			if (!lsb) {
				AppendInternal ((int) (((int) (l >> 32)) & 0xffffffff));
				AppendInternal ((int) ((int) (l & 0xffffffff)));
			} else {
				AppendInternal ((int) ((int) (l & 0xffffffff)));
				AppendInternal ((int) (((int) (l >> 32)) & 0xffffffff));
			}				
		}

		public void Append (decimal d, int bytes)
		{
			int[] arr = Decimal.GetBits (d);
			byte sign =  (d > 0 ? (byte)1 : (byte)0);
			SendIfFull (bytes);
			Append (sign) ;
			AppendInternal (arr[0]);
			AppendInternal (arr[1]);
			AppendInternal (arr[2]);
			AppendInternal ((int)0);
		}

		public void Close ()
		{
			if (stream == null)
				return;

			connReset = false;
			socket = null;
			try {
				stream.Close ();
			} catch {
			}
			stream = null;
		}

		public bool IsConnected () 
		{
			return socket != null && socket.Connected && !(socket.Poll (0, SelectMode.SelectRead) && socket.Available == 0);
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

				Buffer.BlockCopy (inBuffer, inBufferIndex, result, i, avail);
				i += avail;
				inBufferIndex += avail;
			}

			return result;
		}

		public string GetString (int len, Encoding enc)
		{
			if (tdsVersion >= TdsVersion.tds70) 
				return GetString (len, true, null);
			else
				return GetString (len, false, null);
		}
		
		public string GetString (int len)
		{
			if (tdsVersion >= TdsVersion.tds70) 
				return GetString (len, true);
			else
				return GetString (len, false);
		}

		public string GetString (int len, bool wide, Encoding enc)
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
				// Use the passed encoder, if available
				if (enc != null)
					return (enc.GetString (result));
				else
					return (encoder.GetString (result));
			}
		}
		
		public string GetString (int len, bool wide)
		{
			return GetString (len, wide, null);
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
			for (int i = 0; i < 4; i += 1) {
				input[i] = GetByte ();
			}
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
                        int dataLength = GetPhysicalPacketHeader ();
                        GetPhysicalPacketData (dataLength);
		}

		int Read (byte [] buffer, int offset, int count)
		{
			try {
				return stream.Read (buffer, offset, count);
			} catch {
				socket = null;
				stream.Close ();
				throw;
			}
		}

                private int GetPhysicalPacketHeader ()
                {
                        int nread = 0;

			int n;
			// read the header
			while (nread < 8) {
				n = Read (tmpBuf, nread, 8 - nread);
				if (n <= 0) {
					socket = null;
					stream.Close ();
					throw new IOException (n == 0 ? "Connection lost" : "Connection error");
				}
				nread += n;
			}

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
                        
                        return len;

                }
                
                private void GetPhysicalPacketData (int length)
                {
                        // now get the data
			int nread = 0;
			int n;

			while (nread < length) {
				n = Read (inBuffer, nread, length - nread);
				if (n <= 0) {
					socket = null;
					stream.Close ();
					throw new IOException (n == 0 ? "Connection lost" : "Connection error");
				}
				nread += n;
			}

			packetsReceived++;

			// adjust the bookkeeping info about the incoming buffer
			inBufferLength = length;
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
			if (newSize != outBufferLength) {
				byte[] newBuf = new byte [newSize];
				Buffer.BlockCopy (outBuffer, 0, newBuf, 0, newSize);
				outBufferLength = newSize;
				outBuffer = newBuf;
			}
		}

		public bool ResetConnection {
			get { return connReset; }
			set { connReset = value; }
		}

		public void SendPacket ()
		{
			// Reset connection flag is only valid for SQLBatch/RPC/DTC messages
			if (packetType != TdsPacketType.Query && packetType != TdsPacketType.RPC)
				connReset = false;
			
			SendPhysicalPacket (true);
			nextOutBufferIndex = 0;
			packetType = TdsPacketType.None;
			// Reset connection-reset flag to false - as any exception would anyway close 
			// the whole connection
			connReset = false;
			packetsSent = 1;
		}
		
		private void SendPhysicalPacket (bool isLastSegment)
		{
			if (nextOutBufferIndex > headerLength || packetType == TdsPacketType.Cancel) {
				byte status =  (byte) ((isLastSegment ? 0x01 : 0x00) | (connReset ? 0x08 : 0x00)); 
				// packet type
				Store (0, (byte) packetType);
				Store (1, status);
				Store (2, (short) nextOutBufferIndex );
				Store (4, (byte) 0);
				Store (5, (byte) 0);
				if (tdsVersion >= TdsVersion.tds70)
					Store (6, (byte) packetsSent);
				else	
					Store (6, (byte) 0);
				Store (7, (byte) 0);

				stream.Write (outBuffer, 0, nextOutBufferIndex);
				stream.Flush ();
				packetsSent++;
			}
		}
		
		public void Skip (long i)
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
#if NET_2_0
                #region Async Methods

                public IAsyncResult BeginReadPacket (AsyncCallback callback, object stateObject)
		{
                        TdsAsyncResult ar = new TdsAsyncResult (callback, stateObject);

                        stream.BeginRead (tmpBuf, 0, 8, new AsyncCallback(OnReadPacketCallback), ar);
                        return ar;
		}
                
                /// <returns>Packet size in bytes</returns>
                public int EndReadPacket (IAsyncResult ar)
                {
                        if (!ar.IsCompleted)
                                ar.AsyncWaitHandle.WaitOne ();
                        return (int) ((TdsAsyncResult) ar).ReturnValue;
                }
                

                public void OnReadPacketCallback (IAsyncResult socketAsyncResult)
                {
                        TdsAsyncResult ar = (TdsAsyncResult) socketAsyncResult.AsyncState;
                        int nread = stream.EndRead (socketAsyncResult);
			int n;
                        
			while (nread < 8) {
				n = Read (tmpBuf, nread, 8 - nread);
				if (n <= 0) {
					socket = null;
					stream.Close ();
					throw new IOException (n == 0 ? "Connection lost" : "Connection error");
				}
				nread += n;
			}

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

                        GetPhysicalPacketData (len);
                        int value = len + 8;
                        ar.ReturnValue = ((object)value); // packet size
                        ar.MarkComplete ();
                }
                
                #endregion // Async Methods
#endif // NET_2_0

	}

}
