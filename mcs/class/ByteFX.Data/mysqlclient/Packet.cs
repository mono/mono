using System;
using System.IO;
using System.Text;

namespace ByteFX.Data.MySqlClient
{
	internal enum PacketType 
	{
		None,
		UpdateOrOk,
		ResultSchema,
		Last,
		Auth,
		Error,
		LoadDataLocal,
		Other
	}

	/// <summary>
	/// Summary description for Packet.
	/// </summary>
	internal class Packet
	{
		MemoryStream	data;
		PacketType		type = PacketType.None;
		Encoding		encoding;

		public Packet()
		{
			data = new MemoryStream();
		}

		public Packet(int len)
		{
			data = new MemoryStream(len);
		}

		public Packet(byte[] bytes)
		{
			data = new MemoryStream( bytes.Length );
			data.Write( bytes, 0, bytes.Length );
			data.Position = 0;
		}

		public Encoding Encoding 
		{
			set { encoding = value; }
			get { return encoding; }
		}

		public int Length 
		{
			get { return (int)data.Length; }
		}

		public PacketType Type
		{
			get { if (type == PacketType.None) ParseType(); return type; }
			set { type = value; }
		}

		public long Position
		{
			get { return data.Position; }
			set { data.Position = value; }
		}

		public void AppendPacket( Packet newPacket )
		{
			data.Position = data.Length;
			byte[] bytes = newPacket.GetBytes();
			data.Write( bytes, 0, bytes.Length );
		}

		private PacketType ParseType()
		{
			byte b = ReadByte();

			// a 1 byte packet with byte 0xfe means last packet
			if ( data.Length == 1 && b == 0xfe)
				type = PacketType.Last;
			
			// a first byte of 0xff means the packet is an error message
			else if ( b == 0xff )
				type = PacketType.Error;

			// the first byte == 0 means an update packet or column count
			else if ( b == 0 ) 
				type = PacketType.UpdateOrOk;
			else
				type = PacketType.Other;
			return type;
		}

		public byte[] GetBytes()
		{
			return data.ToArray();
		}

		public void WriteByte( byte b )
		{
			data.WriteByte( b );
		}

		public byte ReadByte()
		{
			return (byte)data.ReadByte();
		}

		public void ReadBytes( byte[] buffer, int offset, int len )
		{
			data.Read( buffer, offset, len );
		}

		public void WriteBytes( byte[] bytes, int offset, int len )
		{
			data.Write( bytes, offset, len );
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
			ReadBytes(buffer, 0, len);
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
				data.WriteByte( (byte)(val&0xff) );
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
				int b = data.ReadByte();
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

		public bool CanRead
		{
			get { return data.Position < data.Length; }
		}

		#region String Functions
		public string ReadString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			while ( CanRead )
			{
				byte b = ReadByte();
				if (b == 0) break;
				sb.Append( Convert.ToChar( b ));
			}

			return sb.ToString();
		}

		public void WriteString(string v, Encoding encoding)
		{
			WriteStringNoNull(v, encoding);
			data.WriteByte(0);
		}

		public void WriteStringNoNull(string v, Encoding encoding)
		{
			byte[] bytes = encoding.GetBytes(v);
			data.Write(bytes, 0, bytes.Length);
		}

		#endregion


	}
}
