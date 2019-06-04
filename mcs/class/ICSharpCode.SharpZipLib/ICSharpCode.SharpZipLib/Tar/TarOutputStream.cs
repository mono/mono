// TarOutputStream.cs
//
// Copyright (C) 2001 Mike Krueger
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

using System;
using System.IO;
using System.Text;

namespace ICSharpCode.SharpZipLib.Tar 
{
	
	/// <summary>
	/// The TarOutputStream writes a UNIX tar archive as an OutputStream.
	/// Methods are provided to put entries, and then write their contents
	/// by writing to this stream using write().
	/// </summary>
	/// public
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public class TarOutputStream : Stream
	{
		/// <summary>
		/// flag indicating debugging code should be activated or not
		/// </summary>
		protected bool      debug;
		
		/// <summary>
		/// Size for the current entry
		/// </summary>
		protected long      currSize;

		/// <summary>
		/// bytes written for this entry so far
		/// </summary>
		protected long       currBytes;
		
		/// <summary>
		/// single block working buffer 
		/// </summary>
		protected byte[]    blockBuf;

		/// <summary>
		/// current 'Assembly' buffer length
		/// </summary>		
		protected int       assemLen;
		
		/// <summary>
		/// 'Assembly' buffer used to assmble data before writing
		/// </summary>
		protected byte[]    assemBuf;
		
		/// <summary>
		/// TarBuffer used to provide correct blocking factor
		/// </summary>
		protected TarBuffer buffer;
		
		/// <summary>
		/// the destination stream for the archive contents
		/// </summary>
		protected Stream    outputStream;
		
		/// <summary>
		/// true if the stream supports reading; otherwise, false.
		/// </summary>
		public override bool CanRead {
			get {
				return outputStream.CanRead;
			}
		}
		
		/// <summary>
		/// true if the stream supports seeking; otherwise, false.
		/// </summary>
		public override bool CanSeek {
			get {
				return outputStream.CanSeek;
			}
		}
		
		/// <summary>
		/// true if stream supports writing; otherwise, false.
		/// </summary>
		public override bool CanWrite {
			get {
				return outputStream.CanWrite;
			}
		}
		
		/// <summary>
		/// length of stream in bytes
		/// </summary>
		public override long Length {
			get {
				return outputStream.Length;
			}
		}
		
		/// <summary>
		/// gets or sets the position within the current stream.
		/// </summary>
		public override long Position {
			get {
				return outputStream.Position;
			}
			set {
				outputStream.Position = value;
			}
		}
		
		/// <summary>
		/// set the position within the current stream
		/// </summary>
		public override long Seek(long offset, SeekOrigin origin)
		{
			return outputStream.Seek(offset, origin);
		}
		
		/// <summary>
		/// set the length of the current stream
		/// </summary>
		public override void SetLength(long val)
		{
			outputStream.SetLength(val);
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
		/// <returns>The total number of bytes read, or zero if at the end of the stream</returns>
		public override int Read(byte[] b, int off, int len)
		{
			return outputStream.Read(b, off, len);
		}

		/// <summary>
		/// All buffered data is written to destination
		/// </summary>		
		public override void Flush()
		{
			outputStream.Flush();
		}
				
		/// <summary>
		/// Construct TarOutputStream using default block factor
		/// </summary>
		/// <param name="outputStream">stream to write to</param>
		public TarOutputStream(Stream outputStream) : this(outputStream, TarBuffer.DefaultBlockFactor)
		{
		}
		
		/// <summary>
		/// Construct TarOutputStream with user specified block factor
		/// </summary>
		/// <param name="outputStream">stream to write to</param>
		/// <param name="blockFactor">blocking factor</param>
		public TarOutputStream(Stream outputStream, int blockFactor)
		{
			this.outputStream = outputStream;
			this.buffer       = TarBuffer.CreateOutputTarBuffer(outputStream, blockFactor);
			
			this.debug        = false;
			this.assemLen     = 0;
			this.assemBuf     = new byte[TarBuffer.BlockSize];
			this.blockBuf     = new byte[TarBuffer.BlockSize];
		}
		
		/// <summary>
		/// Ends the TAR archive without closing the underlying OutputStream.
		/// The result is that the EOF record of nulls is written.
		/// </summary>
		public void Finish()
		{
			this.WriteEOFRecord();
		}
		
		/// <summary>
		/// Ends the TAR archive and closes the underlying OutputStream.
		/// This means that finish() is called followed by calling the
		/// TarBuffer's close().
		/// </summary>
		public override void Close()
		{
			this.Finish();
			this.buffer.Close();
		}
		
		/// <summary>
		/// Get the record size being used by this stream's TarBuffer.
		/// </summary>
		/// <returns>
		/// The TarBuffer record size.
		/// </returns>
		public int GetRecordSize()
		{
			return this.buffer.GetRecordSize();
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
			if (entry.TarHeader.Name.Length >= TarHeader.NAMELEN) {
				TarHeader longHeader = new TarHeader();
				longHeader.TypeFlag = TarHeader.LF_GNU_LONGNAME;
				longHeader.Name = longHeader.Name + "././@LongLink";
				longHeader.UserId = 0;
				longHeader.GroupId = 0;
				longHeader.GroupName = "";
				longHeader.UserName = "";
				longHeader.LinkName = "";

				longHeader.Size = entry.TarHeader.Name.Length;

				longHeader.WriteHeader(this.blockBuf);
				this.buffer.WriteBlock(this.blockBuf);  // Add special long filename header block

				int nameCharIndex = 0;

				while (nameCharIndex < entry.TarHeader.Name.Length) {
					Array.Clear(blockBuf, 0, blockBuf.Length);
					TarHeader.GetAsciiBytes(entry.TarHeader.Name, nameCharIndex, this.blockBuf, 0, TarBuffer.BlockSize);
					nameCharIndex += TarBuffer.BlockSize;
					this.buffer.WriteBlock(this.blockBuf);
				}
			}
			
			entry.WriteEntryHeader(this.blockBuf);
			this.buffer.WriteBlock(this.blockBuf);
			
			this.currBytes = 0;
			
			this.currSize = entry.IsDirectory ? 0 : entry.Size;
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
			if (this.assemLen > 0) {
				for (int i = this.assemLen; i < this.assemBuf.Length; ++i) {
					this.assemBuf[i] = 0;
				}
				
				this.buffer.WriteBlock(this.assemBuf);
				
				this.currBytes += this.assemLen;
				this.assemLen = 0;
			}
			
			if (this.currBytes < this.currSize) {
				throw new TarException("entry closed at '" + this.currBytes + "' before the '" + this.currSize + "' bytes specified in the header were written");
			}
		}
		
		/// <summary>
		/// Writes a byte to the current tar archive entry.
		/// This method simply calls Write(byte[], int, int).
		/// </summary>
		/// <param name="b">
		/// The byte to be written.
		/// </param>
		public override void WriteByte(byte b)
		{
			this.Write(new byte[] { b }, 0, 1);
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
		/// <param name = "wBuf">
		/// The buffer to write to the archive.
		/// </param>
		/// <param name = "wOffset">
		/// The offset in the buffer from which to get bytes.
		/// </param>
		/// <param name = "numToWrite">
		/// The number of bytes to write.
		/// </param>
		public override void Write(byte[] wBuf, int wOffset, int numToWrite)
		{
			if (wBuf == null) {
				throw new ArgumentNullException("TarOutputStream.Write buffer null");
			}
			
			if ((this.currBytes + numToWrite) > this.currSize) {
				throw new ArgumentOutOfRangeException("request to write '" + numToWrite + "' bytes exceeds size in header of '" + this.currSize + "' bytes");
			}
			
			//
			// We have to deal with assembly!!!
			// The programmer can be writing little 32 byte chunks for all
			// we know, and we must assemble complete blocks for writing.
			// TODO  REVIEW Maybe this should be in TarBuffer? Could that help to
			//        eliminate some of the buffer copying.
			//
			if (this.assemLen > 0) {
				if ((this.assemLen + numToWrite ) >= this.blockBuf.Length) {
					int aLen = this.blockBuf.Length - this.assemLen;
					
					Array.Copy(this.assemBuf, 0, this.blockBuf, 0, this.assemLen);
					Array.Copy(wBuf, wOffset, this.blockBuf, this.assemLen, aLen);
					
					this.buffer.WriteBlock(this.blockBuf);
					
					this.currBytes += this.blockBuf.Length;
					
					wOffset    += aLen;
					numToWrite -= aLen;
					
					this.assemLen = 0;
				} else {					// ( (this.assemLen + numToWrite ) < this.blockBuf.length )
					Array.Copy(wBuf, wOffset, this.assemBuf, this.assemLen, numToWrite);
					wOffset       += numToWrite;
					this.assemLen += numToWrite;
					numToWrite -= numToWrite;
				}
			}
			
			//
			// When we get here we have EITHER:
			//   o An empty "assemble" buffer.
			//   o No bytes to write (numToWrite == 0)
			//
			while (numToWrite > 0) {
				if (numToWrite < this.blockBuf.Length) {
					Array.Copy(wBuf, wOffset, this.assemBuf, this.assemLen, numToWrite);
					this.assemLen += numToWrite;
					break;
				}
				
				this.buffer.WriteBlock(wBuf, wOffset);
				
				int num = this.blockBuf.Length;
				this.currBytes += num;
				numToWrite     -= num;
				wOffset        += num;
			}
		}
		
		/// <summary>
		/// Write an EOF (end of archive) record to the tar archive.
		/// An EOF record consists of a record of all zeros.
		/// </summary>
		void WriteEOFRecord()
		{
			Array.Clear(blockBuf, 0, blockBuf.Length);
			this.buffer.WriteBlock(this.blockBuf);
		}
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
