using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ByteFX.Data.MySqlClient
{
	/// <summary>
	/// Summary description for CompressedStream.
	/// </summary>
	internal class CompressedStream : Stream
	{
		private	Stream	srcStream;
		private byte[]	buffer;
		private	int		index;

		public CompressedStream( Stream src )
		{
			srcStream = src;
			index = 0;
			buffer = new byte[0];
		}

		#region Properties
		public override bool CanRead
		{
			get	{ return srcStream.CanRead; }
		}

		public override bool CanWrite
		{
			get	{ return srcStream.CanWrite; }
		}

		public override bool CanSeek
		{
			get	{ return srcStream.CanSeek; }
		}

		public override long Length
		{
			get { return srcStream.Length; }
		}

		public override long Position
		{
			get	{ return srcStream.Position; }
			set	{ srcStream.Position = value; }
		}
		#endregion

		public override void Close()
		{
			srcStream.Close();
			base.Close ();
		}

		public override void Flush()
		{
			srcStream.Flush();
		}

		public override void SetLength(long value)
		{
			srcStream.SetLength( value );
		}

		public override int ReadByte()
		{
			EnsureData(1);

			return (int)buffer[index++];
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
				throw new ArgumentNullException( "buffer", "Buffer must not be null" );
			if (offset < 0 || offset >= buffer.Length)
				throw new ArgumentOutOfRangeException( "Offset must be a valid position in buffer" );
			if ((offset + count) > buffer.Length)
				throw new ArgumentException( "Buffer is not large enough to complete operation" );

			EnsureData( count );

			for (int i=0; i < count; i++)
				buffer[offset+i] = this.buffer[index++];

			return count;
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			srcStream.Write( buffer, offset, count );
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return srcStream.Seek( offset, origin );
		}

		private void ReadBuffer( Stream s, byte[] buf, int offset, int length )
		{
			while (length > 0)
			{
				int amountRead = s.Read( buf, offset, length );
				if (amountRead == 0)
				throw new MySqlException("Unexpected end of data encountered");
				length -= amountRead;
				offset += amountRead;
			}
		}

		private void ReadNextPacket()
		{
			// read off the uncompressed and compressed lengths
			int compressedLen = srcStream.ReadByte() + (srcStream.ReadByte() << 8) + 
				(srcStream.ReadByte() << 16);
			byte seq = (byte)srcStream.ReadByte();
			int unCompressedLen = srcStream.ReadByte() + (srcStream.ReadByte() << 8) + 
				(srcStream.ReadByte() << 16);

			// if the data is in fact compressed, then uncompress it
			byte[] unCompressedBuffer = null;
			if (unCompressedLen > 0) 
			{
				unCompressedBuffer = new byte[ unCompressedLen ];
				InflaterInputStream iis = new InflaterInputStream( srcStream );
				ReadBuffer( iis, unCompressedBuffer, 0, unCompressedLen );
			}
			else 
			{
				unCompressedBuffer = new byte[ compressedLen ];
				ReadBuffer( srcStream, unCompressedBuffer, 0, compressedLen );
			}

			// now join this buffer to our existing one
			int left = buffer.Length - index;
			byte[] newBuffer = new byte[ left + unCompressedBuffer.Length ];

			int newIndex = 0;
			// first copy in the rest of the original
			for (int i=index; i < buffer.Length; i++)
				newBuffer[newIndex++] = buffer[i];
			unCompressedBuffer.CopyTo( newBuffer, newIndex );
			buffer = newBuffer;
			index = 0;
		}

		private void EnsureData( int size )
		{
			while ((buffer.Length - index) < size)
				ReadNextPacket();
		}
	}
}
