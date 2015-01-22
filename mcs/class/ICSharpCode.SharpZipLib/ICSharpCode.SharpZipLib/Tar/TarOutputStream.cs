// TarOutputStream.cs
//
// Copyright (C) 2001 Mike Krueger
// Copyright 2005 John Reilly
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
// 
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

// HISTORY
//	2012-06-04	Z-1419	Last char of file name was dropped if path length > 100

using System;
using System.IO;

namespace ICSharpCode.SharpZipLib.Tar 
{
	
	/// <summary>
	/// The TarOutputStream writes a UNIX tar archive as an OutputStream.
	/// Methods are provided to put entries, and then write their contents
	/// by writing to this stream using write().
	/// </summary>
	/// public
	public class TarOutputStream : Stream
	{
		#region Constructors
		/// <summary>
		/// Construct TarOutputStream using default block factor
		/// </summary>
		/// <param name="outputStream">stream to write to</param>
		public TarOutputStream(Stream outputStream)
			: this(outputStream, TarBuffer.DefaultBlockFactor)
		{
		}
		
		/// <summary>
		/// Construct TarOutputStream with user specified block factor
		/// </summary>
		/// <param name="outputStream">stream to write to</param>
		/// <param name="blockFactor">blocking factor</param>
		public TarOutputStream(Stream outputStream, int blockFactor)
		{
			if ( outputStream == null )
			{
				throw new ArgumentNullException("outputStream");
			}

			this.outputStream = outputStream;
			buffer = TarBuffer.CreateOutputTarBuffer(outputStream, blockFactor);

			assemblyBuffer = new byte[TarBuffer.BlockSize];
			blockBuffer  = new byte[TarBuffer.BlockSize];
		}
		#endregion

        /// <summary>
        /// Get/set flag indicating ownership of the underlying stream.
        /// When the flag is true <see cref="Close"></see> will close the underlying stream also.
        /// </summary>
        public bool IsStreamOwner
        {
            get { return buffer.IsStreamOwner; }
            set { buffer.IsStreamOwner = value; }
        }

		/// <summary>
		/// true if the stream supports reading; otherwise, false.
		/// </summary>
		public override bool CanRead
		{
			get
			{
				return outputStream.CanRead;
			}
		}
		
		/// <summary>
		/// true if the stream supports seeking; otherwise, false.
		/// </summary>
		public override bool CanSeek
		{
			get
			{
				return outputStream.CanSeek;
			}
		}
		
		/// <summary>
		/// true if stream supports writing; otherwise, false.
		/// </summary>
		public override bool CanWrite
		{
			get
			{
				return outputStream.CanWrite;
			}
		}
		
		/// <summary>
		/// length of stream in bytes
		/// </summary>
		public override long Length
		{
			get
			{
				return outputStream.Length;
			}
		}
		
		/// <summary>
		/// gets or sets the position within the current stream.
		/// </summary>
		public override long Position
		{
			get
			{
				return outputStream.Position;
			}
			set
			{
				outputStream.Position = value;
			}
		}
		
		/// <summary>
		/// set the position within the current stream
		/// </summary>
		/// <param name="offset">The offset relative to the <paramref name="origin"/> to seek to</param>
		/// <param name="origin">The <see cref="SeekOrigin"/> to seek from.</param>
		/// <returns>The new position in the stream.</returns>
		public override long Seek(long offset, SeekOrigin origin)
		{
			return outputStream.Seek(offset, origin);
		}
		
		/// <summary>
		/// Set the length of the current stream
		/// </summary>
		/// <param name="value">The new stream length.</param>
		public override void SetLength(long value)
		{
			outputStream.SetLength(value);
		}
		
		/// <summary>
		/// Read a byte from the stream and advance the position within the stream 
		/// by one byte or returns -1 if at the end of the stream.
		/// </summary>
		/// <returns>The byte value or -1 if at end of stream</returns>
		public override int ReadByte()
		{
			return outputStream.ReadByte();
		}
		
		/// <summary>
		/// read bytes from the current stream and advance the position within the 
		/// stream by the number of bytes read.
		/// </summary>
		/// <param name="buffer">The buffer to store read bytes in.</param>
		/// <param name="offset">The index into the buffer to being storing bytes at.</param>
		/// <param name="count">The desired number of bytes to read.</param>
		/// <returns>The total number of bytes read, or zero if at the end of the stream.
		/// The number of bytes may be less than the <paramref name="count">count</paramref>
		/// requested if data is not avialable.</returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			return outputStream.Read(buffer, offset, count);
		}

		/// <summary>
		/// All buffered data is written to destination
		/// </summary>		
		public override void Flush()
		{
			outputStream.Flush();
		}
				
		/// <summary>
		/// Ends the TAR archive without closing the underlying OutputStream.
		/// The result is that the EOF block of nulls is written.
		/// </summary>
		public void Finish()
		{
			if ( IsEntryOpen )
			{
				CloseEntry();
			}
			WriteEofBlock();
		}
		
		/// <summary>
		/// Ends the TAR archive and closes the underlying OutputStream.
		/// </summary>
		/// <remarks>This means that Finish() is called followed by calling the
		/// TarBuffer's Close().</remarks>
		public override void Close()
		{
			if ( !isClosed )
			{
				isClosed = true;
				Finish();
				buffer.Close();
			}
		}
		
		/// <summary>
		/// Get the record size being used by this stream's TarBuffer.
		/// </summary>
		public int RecordSize
		{
			get { return buffer.RecordSize; }
		}

		/// <summary>
		/// Get the record size being used by this stream's TarBuffer.
		/// </summary>
		/// <returns>
		/// The TarBuffer record size.
		/// </returns>
		[Obsolete("Use RecordSize property instead")]
		public int GetRecordSize()
		{
			return buffer.RecordSize;
		}
		
		/// <summary>
		/// Get a value indicating wether an entry is open, requiring more data to be written.
		/// </summary>
		bool IsEntryOpen
		{
			get { return (currBytes < currSize); }

		}

		/// <summary>
		/// Put an entry on the output stream. This writes the entry's
		/// header and positions the output stream for writing
		/// the contents of the entry. Once this method is called, the
		/// stream is ready for calls to write() to write the entry's
		/// contents. Once the contents are written, closeEntry()
		/// <B>MUST</B> be called to ensure that all buffered data
		/// is completely written to the output stream.
		/// </summary>
		/// <param name="entry">
		/// The TarEntry to be written to the archive.
		/// </param>
		public void PutNextEntry(TarEntry entry)
		{
			if ( entry == null ) {
				throw new ArgumentNullException("entry");
			}

			if (entry.TarHeader.Name.Length >= TarHeader.NAMELEN) {
				TarHeader longHeader = new TarHeader();
				longHeader.TypeFlag = TarHeader.LF_GNU_LONGNAME;
				longHeader.Name = longHeader.Name + "././@LongLink";
				longHeader.UserId = 0;
				longHeader.GroupId = 0;
				longHeader.GroupName = "";
				longHeader.UserName = "";
				longHeader.LinkName = "";
                longHeader.Size = entry.TarHeader.Name.Length + 1;	// Plus one to avoid dropping last char

				longHeader.WriteHeader(blockBuffer);
				buffer.WriteBlock(blockBuffer);  // Add special long filename header block

				int nameCharIndex = 0;

				while (nameCharIndex < entry.TarHeader.Name.Length) {
					Array.Clear(blockBuffer, 0, blockBuffer.Length);
					TarHeader.GetAsciiBytes(entry.TarHeader.Name, nameCharIndex, this.blockBuffer, 0, TarBuffer.BlockSize);
					nameCharIndex += TarBuffer.BlockSize;
					buffer.WriteBlock(blockBuffer);
				}
			}
			
			entry.WriteEntryHeader(blockBuffer);
			buffer.WriteBlock(blockBuffer);
			
			currBytes = 0;
			
			currSize = entry.IsDirectory ? 0 : entry.Size;
		}
		
		/// <summary>
		/// Close an entry. This method MUST be called for all file
		/// entries that contain data. The reason is that we must
		/// buffer data written to the stream in order to satisfy
		/// the buffer's block based writes. Thus, there may be
		/// data fragments still being assembled that must be written
		/// to the output stream before this entry is closed and the
		/// next entry written.
		/// </summary>
		public void CloseEntry()
		{
			if (assemblyBufferLength > 0) {
				Array.Clear(assemblyBuffer, assemblyBufferLength, assemblyBuffer.Length - assemblyBufferLength);
				
				buffer.WriteBlock(assemblyBuffer);
				
				currBytes += assemblyBufferLength;
				assemblyBufferLength = 0;
			}
			
			if (currBytes < currSize) {
				string errorText = string.Format(
					"Entry closed at '{0}' before the '{1}' bytes specified in the header were written",
					currBytes, currSize);
				throw new TarException(errorText);
			}
		}
		
		/// <summary>
		/// Writes a byte to the current tar archive entry.
		/// This method simply calls Write(byte[], int, int).
		/// </summary>
		/// <param name="value">
		/// The byte to be written.
		/// </param>
		public override void WriteByte(byte value)
		{
			Write(new byte[] { value }, 0, 1);
		}
		
		/// <summary>
		/// Writes bytes to the current tar archive entry. This method
		/// is aware of the current entry and will throw an exception if
		/// you attempt to write bytes past the length specified for the
		/// current entry. The method is also (painfully) aware of the
		/// record buffering required by TarBuffer, and manages buffers
		/// that are not a multiple of recordsize in length, including
		/// assembling records from small buffers.
		/// </summary>
		/// <param name = "buffer">
		/// The buffer to write to the archive.
		/// </param>
		/// <param name = "offset">
		/// The offset in the buffer from which to get bytes.
		/// </param>
		/// <param name = "count">
		/// The number of bytes to write.
		/// </param>
		public override void Write(byte[] buffer, int offset, int count)
		{
			if ( buffer == null ) {
				throw new ArgumentNullException("buffer");
			}
			
			if ( offset < 0 )
			{
#if NETCF_1_0
				throw new ArgumentOutOfRangeException("offset");
#else
				throw new ArgumentOutOfRangeException("offset", "Cannot be negative");
#endif
			}

			if ( buffer.Length - offset < count )
			{
				throw new ArgumentException("offset and count combination is invalid");
			}

			if ( count < 0 )
			{
#if NETCF_1_0
				throw new ArgumentOutOfRangeException("count");
#else
				throw new ArgumentOutOfRangeException("count", "Cannot be negative");
#endif
			}

			if ( (currBytes + count) > currSize ) {
				string errorText = string.Format("request to write '{0}' bytes exceeds size in header of '{1}' bytes",
					count, this.currSize);
#if NETCF_1_0
				throw new ArgumentOutOfRangeException("count");
#else
				throw new ArgumentOutOfRangeException("count", errorText);
#endif
			}
			
			//
			// We have to deal with assembly!!!
			// The programmer can be writing little 32 byte chunks for all
			// we know, and we must assemble complete blocks for writing.
			// TODO  REVIEW Maybe this should be in TarBuffer? Could that help to
			//        eliminate some of the buffer copying.
			//
			if (assemblyBufferLength > 0) {
				if ((assemblyBufferLength + count ) >= blockBuffer.Length) {
					int aLen = blockBuffer.Length - assemblyBufferLength;
					
					Array.Copy(assemblyBuffer, 0, blockBuffer, 0, assemblyBufferLength);
					Array.Copy(buffer, offset, blockBuffer, assemblyBufferLength, aLen);
					
					this.buffer.WriteBlock(blockBuffer);
					
					currBytes += blockBuffer.Length;
					
					offset    += aLen;
					count -= aLen;
					
					assemblyBufferLength = 0;
				} else {
					Array.Copy(buffer, offset, assemblyBuffer, assemblyBufferLength, count);
					offset += count;
					assemblyBufferLength += count;
					count -= count;
				}
			}
			
			//
			// When we get here we have EITHER:
			//   o An empty "assembly" buffer.
			//   o No bytes to write (count == 0)
			//
			while (count > 0) {
				if (count < blockBuffer.Length) {
					Array.Copy(buffer, offset, assemblyBuffer, assemblyBufferLength, count);
					assemblyBufferLength += count;
					break;
				}
				
				this.buffer.WriteBlock(buffer, offset);
				
				int bufferLength = blockBuffer.Length;
				currBytes += bufferLength;
				count -= bufferLength;
				offset += bufferLength;
			}
		}
		
		/// <summary>
		/// Write an EOF (end of archive) block to the tar archive.
		/// An EOF block consists of all zeros.
		/// </summary>
		void WriteEofBlock()
		{
			Array.Clear(blockBuffer, 0, blockBuffer.Length);
			buffer.WriteBlock(blockBuffer);
		}

		#region Instance Fields
		/// <summary>
		/// bytes written for this entry so far
		/// </summary>
		long currBytes;

		/// <summary>
		/// current 'Assembly' buffer length
		/// </summary>
		int assemblyBufferLength;

		/// <summary>
		/// Flag indicating wether this instance has been closed or not.
		/// </summary>
		bool isClosed;

		/// <summary>
		/// Size for the current entry
		/// </summary>
		protected long currSize;

		/// <summary>
		/// single block working buffer
		/// </summary>
		protected byte[] blockBuffer;

		/// <summary>
		/// 'Assembly' buffer used to assemble data before writing
		/// </summary>
		protected byte[] assemblyBuffer;

		/// <summary>
		/// TarBuffer used to provide correct blocking factor
		/// </summary>
		protected TarBuffer buffer;

		/// <summary>
		/// the destination stream for the archive contents
		/// </summary>
		protected Stream outputStream;
		#endregion
	}
}

/* The original Java file had this header:
	** Authored by Timothy Gerard Endres
	** <mailto:time@gjt.org>  <http://www.trustice.com>
	**
	** This work has been placed into the public domain.
	** You may use this work in any way and for any purpose you wish.
	**
	** THIS SOFTWARE IS PROVIDED AS-IS WITHOUT WARRANTY OF ANY KIND,
	** NOT EVEN THE IMPLIED WARRANTY OF MERCHANTABILITY. THE AUTHOR
	** OF THIS SOFTWARE, ASSUMES _NO_ RESPONSIBILITY FOR ANY
	** CONSEQUENCE RESULTING FROM THE USE, MODIFICATION, OR
	** REDISTRIBUTION OF THIS SOFTWARE.
	**
	*/
