//
// Mono.Data.TdsClient.Internal.TdsComm.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) 2002 Tim Coleman
//

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Mono.Data.TdsClient.Internal {
        internal sealed class TdsComm
	{
		#region Fields

		NetworkStream stream;
		int packetSize;
		TdsPacketType packetType = TdsPacketType.None;
		Encoding encoder;

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

		TdsVersion tdsVersion;
		
		#endregion // Fields
		
		#region Constructors
		
		public TdsComm (Socket socket, int packetSize, TdsVersion tdsVersion)
		{
			this.packetSize = packetSize;
			this.tdsVersion = tdsVersion;

			outBuffer = new byte[packetSize];
			inBuffer = new byte[packetSize];

			outBufferLength = packetSize;
			inBufferLength = packetSize;
			stream = new NetworkStream (socket);
		}
		
		#endregion // Constructors
		
		#region Properties

		internal Encoding Encoder {
			set { encoder = value; }
		}
		
		public int PacketSize {
			get { return packetSize; }
			set { packetSize = value; }
		}
		
		#endregion // Properties
		
		#region Methods

		internal void ResizeOutBuf (int newSize)
		{
			if (newSize > outBufferLength) {
				byte[] newBuf = new byte[newSize];
				Array.Copy (outBuffer, 0, newBuf, 0, outBufferLength);
				outBufferLength = newSize;
				outBuffer = newBuf;
			}
		}
		
		public void StartPacket (TdsPacketType type)
		{
			if (type != TdsPacketType.Cancel && inBufferIndex != inBufferLength)
			{
				// SAfe It's ok to throw this exception so that we will know there
				//      is a design flaw somewhere, but we should empty the buffer
				//      however. Otherwise the connection will never close (e.g. if
				//      SHOWPLAN_ALL is ON, a resultset will be returned by commit
				//      or rollback and we will never get rid of it). It's true
				//      that we should find a way to actually process these packets
				//      but for now, just dump them (we have thrown an exception).
				inBufferIndex = inBufferLength;
			}

			packetType = type;
			nextOutBufferIndex = headerLength;
		}

		public bool SomeThreadIsBuildingPacket ()
		{
			return packetType != TdsPacketType.None;
		}

		public void Append (byte b)
		{
			if (nextOutBufferIndex == outBufferLength) {
				SendPhysicalPacket (false);
				nextOutBufferIndex = headerLength;
			}
			StoreByte (nextOutBufferIndex, b);
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
			if (tdsVersion < TdsVersion.tds70) {
				Append ((byte) (((byte) (s >> 8)) & 0xff));
				Append ((byte) (((byte) (s >> 0)) & 0xff));
			}
			else
				Append (BitConverter.GetBytes (s));
		}

		public void Append (int i)
		{
			if (tdsVersion < TdsVersion.tds70) {
				Append ((byte) (((byte) (i >> 24)) & 0xff));
				Append ((byte) (((byte) (i >> 16)) & 0xff));
				Append ((byte) (((byte) (i >> 8)) & 0xff));
				Append ((byte) (((byte) (i >> 0)) & 0xff));
			} 
			else
				Append (BitConverter.GetBytes (i));
		}

		public void Append (string s)
		{
			if (tdsVersion < TdsVersion.tds70) 
				Append (encoder.GetBytes (s));
			else 
				foreach (char c in s)
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
			else {
				Append (BitConverter.GetBytes (l));
			}
		}

		public void SendPacket ()
		{
			SendPhysicalPacket (true);
			nextOutBufferIndex = 0;
			packetType = TdsPacketType.None;
		}
		
		private void StoreByte (int index, byte value)
		{
			outBuffer[index] = value;
		}		

		private void StoreShort (int index, short s)
		{
			outBuffer[index] = (byte) (((byte) (s >> 8)) & 0xff);
			outBuffer[index + 1] = (byte) (((byte) (s >> 0)) & 0xff);
		}

		private void SendPhysicalPacket (bool isLastSegment)
		{
			if (nextOutBufferIndex > headerLength || packetType == TdsPacketType.Cancel) {
				// packet type
				StoreByte (0, (byte) packetType);
				StoreByte (1, (byte) (isLastSegment ? 1 : 0));
				StoreShort (2, (short) nextOutBufferIndex );
				StoreByte (4, (byte) 0);
				StoreByte (5, (byte) 0);
				StoreByte (6, (byte) (tdsVersion == TdsVersion.tds70 ? 0x1 : 0x0));
				StoreByte (7, (byte) 0);

				stream.Write (outBuffer, 0, nextOutBufferIndex);
				stream.Flush ();
				packetsSent++;
			}
		}
		
		public byte Peek ()
		{
			// If out of data, read another physical packet.
			if (inBufferIndex >= inBufferLength)
				GetPhysicalPacket ();

			return inBuffer[inBufferIndex];
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
			if (tdsVersion == TdsVersion.tds70) {
				char[] chars = new char[len];
				for (int i = 0; i < len; ++i) {
					int lo = ((byte) GetByte ()) & 0xFF;
					int hi = ((byte) GetByte ()) & 0xFF;
					chars[i] = (char) (lo | ( hi << 8));
				}
				return new String (chars);
			}
			else {
				byte[] result = new byte[len + 1];
				Array.Copy (GetBytes (len, false), result, len);
				result[len] = (byte) 0;
				return (encoder.GetString (result));
			}
		}

		public void Skip (int i)
		{
			for ( ; i > 0; i--)
				GetByte ();
		}
		// skip()


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

			return (BitConverter.ToInt16 (input, 0));
		}


		public int GetTdsInt ()
		{
			byte[] input = new byte[4];
			for (int i = 0; i < 4; i += 1)
				input[i] = GetByte ();
			return (BitConverter.ToInt32 (input, 0));
		}

		public long GetTdsInt64 ()
		{
			byte[] input = new byte[8];
			for (int i = 0; i < 8; i += 1)
				input[i] = GetByte ();
			return (BitConverter.ToInt64 (input, 0));
		}

		private void GetPhysicalPacket ()
		{
			int nread = 0;

			// read the header
			while (nread < 8)
				nread += stream.Read (tmpBuf, nread, 8 - nread);

			TdsPacketType packetType = (TdsPacketType) tmpBuf[0];
			if (packetType != TdsPacketType.Logon && packetType != TdsPacketType.Query && packetType != TdsPacketType.Reply) {
				throw new TdsException (String.Format ("Unknown packet type {0}", tmpBuf[0]));
			}

			// figure out how many bytes are remaining in this packet.
			int len = Ntohs (tmpBuf, 2) - 8;

			if (len >= inBuffer.Length) 
				inBuffer = new byte[len];

			if (len < 0) {
				throw new TdsException (String.Format ("Confused by a length of {0}", len));
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
		#endregion // Methods
	}

}
