using System;
using System.IO;
using System.Text;

namespace ByteFX.Data.MySqlClient
{
/*	internal enum PacketType 
	{
		None,
		UpdateOrOk,
		ResultSchema,
		Last,
		Auth,
		Error,
		LoadDataLocal,
		Other
	}*/

	/// <summary>
	/// Summary description for Packet.
	/// </summary>
	internal class Packet : MemoryStream
	{
		Encoding		encoding;
		private static int	HEADER_LEN = 7;

		public Packet() : base(256+HEADER_LEN)
		{
			Position = HEADER_LEN;
		}

		public Packet(int len) : base(len+HEADER_LEN)
		{
			Position = HEADER_LEN;
		}

		public Packet(byte[] bytes) : base(bytes, 0, bytes.Length, true, true)
		{
		}

		public Encoding Encoding 
		{
			set { encoding = value; }
			get { return encoding; }
		}

		public byte this[int index] 
		{
			get { return GetBuffer()[index]; }
		}

		public new int Length 
		{
			get { return (int)base.Length; }
		}

		public bool IsLastPacket()
		{
			if (Length == 1 && this[0] == 0xfe) return true;
			return false;
		}

		public byte[] GetBytes( byte packetSeq ) 
		{
			long oldPos = Position;
			Position = 3;
			WriteInteger( Length-HEADER_LEN, 3 );
			WriteByte( packetSeq );
			Position = oldPos;
			return GetBuffer();
		}

		public int ReadNBytes()
		{
			byte c = (byte)ReadByte();
			if (c < 1 || c > 4) throw new MySqlException("Unexpected byte count received");
			return ReadInteger((int)c);
		}

		public string ReadLenString()
		{
			int len = ReadLenInteger();

			byte[] buffer = new Byte[len];
			Read(buffer, 0, len);
			return encoding.GetString( buffer, 0, len);
		}


		/// <summary>
		/// WriteInteger
		/// </summary>
		/// <param name="v"></param>
		/// <param name="numbytes"></param>
		public void WriteInteger( int v, int numbytes )
		{
			int val = v;

			if (numbytes < 1 || numbytes > 4) 
				throw new ArgumentOutOfRangeException("Wrong byte count for WriteInteger");

			for (int x=0; x < numbytes; x++)
			{
				WriteByte( (byte)(val&0xff) );
				val >>= 8;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="numbytes"></param>
		/// <returns></returns>
		public int ReadInteger(int numbytes)
		{
			int val = 0;
			int raise = 1;
			for (int x=0; x < numbytes; x++)
			{
				int b = ReadByte();
				val += (b*raise);
				raise *= 256;
			}
			return val;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public int ReadLenInteger()
		{
			byte c  = (byte)ReadByte();

			switch(c) 
			{
				case 251 : return -1; 
				case 252 : return ReadInteger(2);
				case 253 : return ReadInteger(3);
				case 254 : return ReadInteger(4);
				default  : return c;
			}
		}

		public bool HasMoreData
		{
			get { return Position < Length; }
		}

		#region String Functions
		public string ReadString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			while ( HasMoreData )
			{
				byte b = (byte)ReadByte();
				if (b == 0) break;
				sb.Append( Convert.ToChar( b ));
			}

			return sb.ToString();
		}

		public void WriteString(string v, Encoding encoding)
		{
			WriteStringNoNull(v, encoding);
			WriteByte(0);
		}

		public void WriteStringNoNull(string v, Encoding encoding)
		{
			byte[] bytes = encoding.GetBytes(v);
			Write(bytes, 0, bytes.Length);
		}

		#endregion


	}
}
