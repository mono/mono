using System;
using System.IO;
using System.Text;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Summary description for Packet.
	/// </summary>
	internal class Packet : MemoryStream
	{
		Encoding		encoding;
		byte			sequence;
		int				completeLen;
		public static int   NULL_LEN=-1;
		private int		shortLen = 2;
		private int		intLen = 3;
		private int		longLen = 4;
		private bool	longInts = false;

		public Packet(bool longInts) : base()
		{
			LongInts = longInts;
		}

		public Packet(byte[] bytes, bool longInts) : base(bytes.Length)
		{
			this.Write( bytes, 0, bytes.Length );
			Position = 0;
			LongInts = longInts;
		}

		public bool LongInts 
		{
			get { return longInts; }
			set 
			{ 
				longInts = value; 
				if (longInts) 
				{
					intLen = 4;
					longLen = 8;
				}
			}
		}

		public int CompleteLength 
		{
			get { return completeLen; }
			set { completeLen = value; }
		}

		public byte Sequence 
		{
			get { return sequence; }
			set { sequence = value; }
		}

		public Encoding Encoding 
		{
			set { encoding = value; }
			get { return encoding; }
		}

		public void Clear()
		{
			Position = 0;
			this.SetLength(0);
		}

		public byte this[int index] 
		{
			get 
			{ 
				long pos = Position;
				Position = index;
				byte b = (byte)ReadByte();
				Position = pos;
				return b;
			}
		}

		public new int Length 
		{
			get { return (int)base.Length; }
		}

		public bool IsLastPacket()
		{
			return (Length == 1 && this[0] == 0xfe);
		}

		public void Append( Packet p )
		{
			long oldPos = Position;
			Position = Length;
			this.Write( p.GetBuffer(), 0, p.Length );
			Position = oldPos;
		}

		public Packet ReadPacket()
		{
			if (! HasMoreData) return null;

			int len = this.ReadInteger(3);
			byte seq = (byte)this.ReadByte();
			byte[] buf = new byte[ len ];
			this.Read( buf, 0, len );
			Packet p = new Packet( buf, LongInts );
			p.Sequence = seq;
			p.Encoding = this.Encoding;
			return p;
		}

		public int ReadNBytes()
		{
			byte c = (byte)ReadByte();
			if (c < 1 || c > 4) throw new MySqlException("Unexpected byte count received");
			return ReadInteger((int)c);
		}

		public string ReadLenString()
		{
			long len = ReadLenInteger();

			byte[] buffer = new Byte[len];
			Read(buffer, 0, (int)len);
			return encoding.GetString( buffer, 0, (int)len);
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
		public long ReadLenInteger()
		{
			byte c  = (byte)ReadByte();

			switch(c) 
			{
				case 251 : return NULL_LEN; 
				case 252 : return ReadInteger(shortLen);
				case 253 : return ReadInteger(intLen);
				case 254 : return ReadInteger(longLen);
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
