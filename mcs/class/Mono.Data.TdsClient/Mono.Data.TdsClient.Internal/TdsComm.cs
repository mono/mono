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
			stream = new NetworkStream (socket);
		}
		
		#endregion // Constructors
		
		#region Properties
		
		public int PacketSize {
			get { return packetSize; }
			set { packetSize = value; }
		}
		
		#endregion // Properties
		
		#region Methods
		
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

			// Only one thread at a time can be building an outboudn packet.
			// This is primarily a concern with building cancel packets.
			//  XXX: as why should more than one thread work with the same tds-stream ??? would be fatal anyway

			Monitor.Enter (packetType);

			packetType = type;
			nextOutBufferIndex = headerLength;
		}

		public bool SomeThreadIsBuildingPacket ()
		{
			return packetType != TdsPacketType.None;
		}

		public void AppendByte (byte b)
		{
			if (nextOutBufferIndex == outBufferLength) {
				// If we have a full physical packet then ship it out to the
				// network.
				SendPhysicalPacket (false);
				nextOutBufferIndex = headerLength;
			}
	
			StoreByte( nextOutBufferIndex, b );
			nextOutBufferIndex++;
		}	
		
		public void AppendBytes (byte[] b)
		{
			AppendBytes (b, b.Length, (byte) 0);
		}		

		public void AppendBytes (byte[] b, int len, byte pad)
		{
			int i = 0;
			for ( ; i < b.Length && i < len; i++)
			    AppendByte( b[i] );

			for ( ; i < len; i++)
			    AppendByte( pad );
		}	


		public void AppendShort (short s)
		{
			AppendByte ((byte) ((s >> 8) & 0xff));
			AppendByte ((byte) ((s >> 0) & 0xff));
		}

		public void AppendTdsShort (short s)
		{
			AppendByte ((byte ) ((s >> 0) & 0xff));
			AppendByte ((byte ) ((s >> 8) & 0xff));
		}

		public void AppendFlt8 (double value)
		{
			long l = BitConverter.DoubleToInt64Bits (value);

			AppendByte ((byte) ((l >> 0) & 0xff));
			AppendByte ((byte) ((l >> 8) & 0xff));
			AppendByte ((byte) ((l >> 16) & 0xff));
			AppendByte ((byte) ((l >> 24) & 0xff));
			AppendByte ((byte) ((l >> 32) & 0xff));
			AppendByte ((byte) ((l >> 40) & 0xff));
			AppendByte ((byte) ((l >> 48) & 0xff));
			AppendByte ((byte) ((l >> 56) & 0xff));
		}

		public void AppendInt (int i)
		{
			AppendByte ((byte) ((i >> 24) & 0xff));
			AppendByte ((byte) ((i >> 16) & 0xff));
			AppendByte ((byte) ((i >> 8) & 0xff));
			AppendByte ((byte) ((i >> 0) & 0xff));
		}

		public void AppendTdsInt (int i)
		{
			AppendByte ((byte) ((i >> 0) & 0xff));
			AppendByte ((byte) ((i >> 8) & 0xff));
			AppendByte ((byte) ((i >> 16) & 0xff));
			AppendByte ((byte) ((i >> 24) & 0xff));
		}


		public void AppendInt64 (long i)
		{
			AppendByte ((byte) ((i >> 56) & 0xff));
			AppendByte ((byte) ((i >> 48) & 0xff));
			AppendByte ((byte) ((i >> 40) & 0xff));
			AppendByte ((byte) ((i >> 32) & 0xff));
			AppendByte ((byte) ((i >> 24) & 0xff));
			AppendByte ((byte) ((i >> 16) & 0xff));
			AppendByte ((byte) ((i >> 8) & 0xff));
			AppendByte ((byte) ((i >> 0) & 0xff));
		}

		public void AppendChars (string s)
		{
			foreach (char c in s)
			{
				byte b1 = (byte) (c & 0xFF);
				byte b2 = (byte) ((c >> 8) & 0xFF);
				AppendByte (b1);
				AppendByte (b2);
			}
		}	

		public void SendPacket ()
		{
			Monitor.Pulse (packetType);
			SendPhysicalPacket (true);
			nextOutBufferIndex = 0;
			packetType = TdsPacketType.None;
			Monitor.Exit (packetType);
		}
		
		private void StoreByte (int index, byte value)
		{
			outBuffer[index] = value;
		}		

		private void StoreShort (int index, short s)
		{
			outBuffer[index] = (byte) ((s >> 8) & 0xff);
			outBuffer[index + 1] = (byte) ((s >> 0) & 0xff);
		}

		private void SendPhysicalPacket (bool isLastSegment)
		{
			if (nextOutBufferIndex > headerLength || packetType == TdsPacketType.Cancel) {
				// packet type
				StoreByte (0, (byte) ((byte) packetType & 0xff));
				StoreByte (1, isLastSegment ? (byte) 1 : (byte) 0);
				StoreShort (2, (short) nextOutBufferIndex );
				StoreByte (4, (byte) 0);
				StoreByte (5, (byte) 0);
				StoreByte (6, (byte) (tdsVersion == TdsVersion.tds70 ? 1 : 0));
				StoreByte (7, (byte) 0);

				stream.Write (outBuffer, 0, nextOutBufferIndex);
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
					int lo = GetByte () & 0xFF;
					int hi = GetByte () & 0xFF;
					chars[i] = (char) (lo | ( hi << 8));
				}
				return new String (chars);
			}
			else {
				byte[] result = GetBytes (len, false);
				StringBuilder sb = new StringBuilder ();
				foreach (byte b in result)
					sb.Append (b);
				return sb.ToString ();
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

		public int GetTdsShort ()
		{
			int lo = ((int) GetByte () & 0xff);
			int hi = ((int) GetByte () & 0xff) << 8;
			return lo | hi;
		}


		public int GetTdsInt ()
		{
			int result;

			int b1 = ((int) GetByte () & 0xff);
			int b2 = ((int) GetByte () & 0xff) << 8;
			int b3 = ((int) GetByte () & 0xff) << 16;
			int b4 = ((int) GetByte () & 0xff) << 24;

			result = b4 | b3 | b2 | b1;

			return result;
		}

		public long GetTdsInt64 ()
		{
			long b1 = ((long) GetByte () & 0xff);
			long b2 = ((long) GetByte () & 0xff) << 8;
			long b3 = ((long) GetByte () & 0xff) << 16;
			long b4 = ((long) GetByte () & 0xff) << 24;
			long b5 = ((long) GetByte () & 0xff) << 32;
			long b6 = ((long) GetByte () & 0xff) << 40;
			long b7 = ((long) GetByte () & 0xff) << 48;
			long b8 = ((long) GetByte () & 0xff) << 56;
			return b1 | b2 | b3 | b4 | b5 | b6 | b7 | b8;
		}

		private void GetPhysicalPacket ()
		{
			// read the header
			for (int nread = 0; nread < 8; ) 
				nread += stream.Read (tmpBuf, nread, 8 - nread);

			TdsPacketType packetType = (TdsPacketType) tmpBuf[0];
			if (packetType != TdsPacketType.Logon && packetType != TdsPacketType.Query && packetType != TdsPacketType.Reply) {
				//throw new TdsUnknownPacketType (packetType, tmpBuf);
			}

			// figure out how many bytes are remaining in this packet.
			int len = Ntohs (tmpBuf, 2) - 8;

			if (len >= inBuffer.Length) 
				inBuffer = new byte[len];

			if (len < 0) {
				//throw new TdsException ("Confused by a length of " + len);
			}

			// now get the data
			for (int nread = 0; nread < len; )
				nread += stream.Read (inBuffer, nread, len - nread);

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
