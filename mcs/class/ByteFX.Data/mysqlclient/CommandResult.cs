using System;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Summary description for CommandResult.
	/// </summary>
	internal class CommandResult
	{
		private Driver	driver;
		private Packet	packet;
		private int		affectedRows = -1;
		private int		fieldCount = -1;
		private int		fieldsRead = 0;
		private long	fieldLength = 0;
		private bool	readSchema = false;
		private bool	readRows = false;

		public CommandResult(Packet p, Driver d)
		{
			driver = d;
			packet = p;
			fieldCount = (int)p.ReadLenInteger();
			if (fieldCount == 0)
				affectedRows =(int)p.ReadLenInteger();
		}

		#region Properties
		public bool IsResultSet 
		{
			get { return fieldCount > 0; }
		}

		public int ColumnCount 
		{
			get { return fieldCount; }
		}

		public int RowsAffected
		{
			get { return affectedRows; }
		}

		#endregion

		public MySqlField GetField()
		{
			MySqlField f = new MySqlField( driver.Encoding );
			packet = driver.ReadPacket();

			f.TableName = packet.ReadLenString();
			f.ColumnName = packet.ReadLenString();
			f.ColumnLength = (int)packet.ReadNBytes();
			f.Type = (MySqlDbType)packet.ReadNBytes();
			packet.ReadByte();									// this is apparently 2 -- not sure what it is for
			f.Flags = (ColumnFlags)packet.ReadInteger(2);		//(short)(d.ReadByte() & 0xff);
			f.NumericScale = packet.ReadByte();
			fieldsRead++;
			return f;
		}

		public byte[] GetFieldBuffer()
		{
			return packet.GetBuffer();
		}

		public int GetFieldIndex()
		{
			return (int)packet.Position;
		}

		public long GetFieldLength()
		{
			return fieldLength;
		}

		public bool NextField()
		{
			if (fieldLength >= 0)
				packet.Position += fieldLength;
			if (! packet.HasMoreData) return false;
			fieldLength = packet.ReadLenInteger();
			fieldsRead++;
			return true;
		}


		/// <summary>
		/// Checks to see if there are any row packets coming
		/// </summary>
		/// <returns>True if there are row packets available, false if not</returns>
		public bool CheckForRows()
		{
			// first read off any unread field defs
			while (fieldsRead < fieldCount)
				GetField();

			// read off the end of schema packet
			packet = driver.ReadPacket();
			if ( ! packet.IsLastPacket())
				throw new MySqlException("Expected end of schema packet");
			readSchema = true;

			packet = driver.PeekPacket();
			return ! packet.IsLastPacket();
		}

		public bool ReadDataRow()
		{
			packet = driver.ReadPacket();
			if (packet.IsLastPacket())
			{
				readRows = true;
				return false;
			}
			fieldsRead = 0;
			fieldLength = 0;
			NextField();
			return true;
		}

		public void Clear()
		{
			Packet p;

			if (! readSchema)
			{
				do 
				{
					p = driver.ReadPacket();
				} while (! p.IsLastPacket());
			}

			if (! readRows)
				do 
				{
					p = driver.ReadPacket();
				} while (! p.IsLastPacket());
		}
	}
}
