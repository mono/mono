// TarBuffer.cs
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

namespace ICSharpCode.SharpZipLib.Tar {
	
	/// <summary>
	/// The TarBuffer class implements the tar archive concept
	/// of a buffered input stream. This concept goes back to the
	/// days of blocked tape drives and special io devices. In the
	/// C# universe, the only real function that this class
	/// performs is to ensure that files have the correct "block"
	/// size, or other tars will complain.
	/// <p>
	/// You should never have a need to access this class directly.
	/// TarBuffers are created by Tar IO Streams.
	/// </p>
	/// </summary>
	public class TarBuffer
	{
		public static readonly int DEFAULT_RCDSIZE = 512;
		public static readonly int DEFAULT_BLKSIZE = DEFAULT_RCDSIZE * 20;
		
		Stream inputStream;
		Stream outputStream;
		
		byte[] blockBuffer;
		int    currBlkIdx;
		int    currRecIdx;
		int    blockSize;
		int    recordSize;
		int    recsPerBlock;
		
		bool   debug;
		
		protected TarBuffer()
		{
		}
		
		public static TarBuffer CreateInputTarBuffer(Stream inputStream)
		{
			return CreateInputTarBuffer(inputStream, TarBuffer.DEFAULT_BLKSIZE);
		}
		public static TarBuffer CreateInputTarBuffer(Stream inputStream, int blockSize )
		{
			return CreateInputTarBuffer(inputStream, blockSize, TarBuffer.DEFAULT_RCDSIZE);
		}
		public static TarBuffer CreateInputTarBuffer(Stream inputStream, int blockSize, int recordSize)
		{
			TarBuffer tarBuffer = new TarBuffer();
			tarBuffer.inputStream  = inputStream;
			tarBuffer.outputStream = null;
			tarBuffer.Initialize(blockSize, recordSize);
			
			return tarBuffer;
		}

		public static TarBuffer CreateOutputTarBuffer(Stream outputStream)
		{
			return CreateOutputTarBuffer(outputStream, TarBuffer.DEFAULT_BLKSIZE);
		}
		public static TarBuffer CreateOutputTarBuffer(Stream outputStream, int blockSize )
		{
			return CreateOutputTarBuffer(outputStream, blockSize, TarBuffer.DEFAULT_RCDSIZE);
		}
		public static TarBuffer CreateOutputTarBuffer(Stream outputStream, int blockSize, int recordSize)
		{
			TarBuffer tarBuffer = new TarBuffer();
			tarBuffer.inputStream  = null;
			tarBuffer.outputStream = outputStream;
			tarBuffer.Initialize(blockSize, recordSize);
			
			return tarBuffer;
		}
		
		/// <summary>
		/// Initialization common to all constructors.
		/// </summary>
		void Initialize(int blockSize, int recordSize)
		{
			this.debug        = false;
			this.blockSize    = blockSize;
			this.recordSize   = recordSize;
			this.recsPerBlock = this.blockSize / this.recordSize;
			this.blockBuffer  = new byte[this.blockSize];
			
			if (inputStream != null) {
				this.currBlkIdx = -1;
				this.currRecIdx = this.recsPerBlock;
			} else {
				this.currBlkIdx = 0;
				this.currRecIdx = 0;
			}
		}
		
		/// <summary>
		/// Get the TAR Buffer's block size. Blocks consist of multiple records.
		/// </summary>
		public int GetBlockSize()
		{
			return this.blockSize;
		}
		
		/// <summary>
		/// Get the TAR Buffer's record size.
		/// </summary>
		public int GetRecordSize()
		{
			return this.recordSize;
		}
		
		/// <summary>
		/// Set the debugging flag for the buffer.
		/// </summary>
		public void SetDebug(bool debug)
		{
			this.debug = debug;
		}
		
		/// <summary>
		/// Determine if an archive record indicate End of Archive. End of
		/// archive is indicated by a record that consists entirely of null bytes.
		/// </summary>
		/// <param name = "record">
		/// The record data to check.
		/// </param>
		public bool IsEOFRecord(byte[] record)
		{
			for (int i = 0, sz = this.GetRecordSize(); i < sz; ++i) {
				if (record[i] != 0) {
					return false;
				}
			}
			
			return true;
		}
		
		/// <summary>
		/// Skip over a record on the input stream.
		/// </summary>
		public void SkipRecord()
		{
			if (this.debug) {
				Console.Error.WriteLine("SkipRecord: recIdx = " + this.currRecIdx + " blkIdx = " + this.currBlkIdx);
			}
			
			if (this.inputStream == null) {
				throw new System.IO.IOException("no input stream defined");
			}
			
			if (this.currRecIdx >= this.recsPerBlock) {
				if (!this.ReadBlock()) {
					return; // UNDONE
				}
			}
			
			this.currRecIdx++;
		}
		
		/// <summary>
		/// Read a record from the input stream and return the data.
		/// </summary>
		/// <returns>
		/// The record data.
		/// </returns>
		public byte[] ReadRecord()
		{
			if (this.debug) {
				Console.Error.WriteLine( "ReadRecord: recIdx = " + this.currRecIdx + " blkIdx = " + this.currBlkIdx );
			}
			
			if (this.inputStream == null) {
				throw new System.IO.IOException("no input stream defined");
			}
			
			if (this.currRecIdx >= this.recsPerBlock) {
				if (!this.ReadBlock()) {
					return null;
				}
			}
			
			byte[] result = new byte[this.recordSize];
			
			Array.Copy(this.blockBuffer, (this.currRecIdx * this.recordSize), result, 0, this.recordSize );
			this.currRecIdx++;
			return result;
		}
		
		/// <returns>
		/// false if End-Of-File, else true
		/// </returns>
		bool ReadBlock()
		{
			Console.WriteLine(this.debug);
			if (this.debug) {
				Console.Error.WriteLine("ReadBlock: blkIdx = " + this.currBlkIdx);
			}
			
			if (this.inputStream == null) {
				throw new System.IO.IOException("no input stream stream defined");
			}
						
			this.currRecIdx = 0;
			
			int offset = 0;
			int bytesNeeded = this.blockSize;
			for (; bytesNeeded > 0 ;) {
				long numBytes = this.inputStream.Read(this.blockBuffer, offset, bytesNeeded);
				
				//
				// NOTE
				// We have fit EOF, and the block is not full!
				//
				// This is a broken archive. It does not follow the standard
				// blocking algorithm. However, because we are generous, and
				// it requires little effort, we will simply ignore the error
				// and continue as if the entire block were read. This does
				// not appear to break anything upstream. We used to return
				// false in this case.
				//
				// Thanks to 'Yohann.Roussel@alcatel.fr' for this fix.
				//
				if (numBytes <= 0) {
					break;
				}
				
				offset      += (int)numBytes;
				bytesNeeded -= (int)numBytes;
				if (numBytes != this.blockSize) {
					if (this.debug) {
						Console.Error.WriteLine("ReadBlock: INCOMPLETE READ " + numBytes + " of " + this.blockSize + " bytes read.");
					}
				}
			}
			
			this.currBlkIdx++;
			return true;
		}
		
		/// <summary>
		/// Get the current block number, zero based.
		/// </summary>
		/// <returns>
		/// The current zero based block number.
		/// </returns>
		public int GetCurrentBlockNum()
		{
			return this.currBlkIdx;
		}
		
		/// <summary>
		/// Get the current record number, within the current block, zero based.
		/// Thus, current offset = (currentBlockNum * recsPerBlk) + currentRecNum.
		/// </summary>
		/// <returns>
		/// The current zero based record number.
		/// </returns>
		public int GetCurrentRecordNum()
		{
			return this.currRecIdx - 1;
		}
		
		/// <summary>
		/// Write an archive record to the archive.
		/// </summary>
		/// <param name="record">
		/// The record data to write to the archive.
		/// </param>
		/// 
		public void WriteRecord(byte[] record)
		{
			if (this.debug) {
				Console.Error.WriteLine("WriteRecord: recIdx = " + this.currRecIdx + " blkIdx = " + this.currBlkIdx );
			}
			
			if (this.outputStream == null) {
				throw new System.IO.IOException("no output stream defined");
			}
						
			if (record.Length != this.recordSize) {
				throw new IOException("record to write has length '" + record.Length + "' which is not the record size of '" + this.recordSize + "'" );
			}
			
			if (this.currRecIdx >= this.recsPerBlock) {
				this.WriteBlock();
			}
			Array.Copy(record, 0, this.blockBuffer, (this.currRecIdx * this.recordSize), this.recordSize );
			this.currRecIdx++;
		}
		
		/// <summary>
		/// Write an archive record to the archive, where the record may be
		/// inside of a larger array buffer. The buffer must be "offset plus
		/// record size" long.
		/// </summary>
		/// <param name="buf">
		/// The buffer containing the record data to write.
		/// </param>
		/// <param name="offset">
		/// The offset of the record data within buf.
		/// </param>
		public void WriteRecord(byte[] buf, int offset)
		{
			if (this.debug) {
				Console.Error.WriteLine("WriteRecord: recIdx = " + this.currRecIdx + " blkIdx = " + this.currBlkIdx );
			}
			
			if (this.outputStream == null) {
				throw new System.IO.IOException("no output stream stream defined");
			}
						
			if ((offset + this.recordSize) > buf.Length) {
				throw new IOException("record has length '" + buf.Length + "' with offset '" + offset + "' which is less than the record size of '" + this.recordSize + "'" );
			}
			
			if (this.currRecIdx >= this.recsPerBlock) {
				this.WriteBlock();
			}
			
			Array.Copy(buf, offset, this.blockBuffer, (this.currRecIdx * this.recordSize), this.recordSize );
			
			this.currRecIdx++;
		}
		
		/// <summary>
		/// Write a TarBuffer block to the archive.
		/// </summary>
		void WriteBlock()
		{
			if (this.debug) {
				Console.Error.WriteLine("WriteBlock: blkIdx = " + this.currBlkIdx);
			}
			
			if (this.outputStream == null) {
				throw new System.IO.IOException("no output stream defined");
			}
			
			this.outputStream.Write(this.blockBuffer, 0, this.blockSize);
			this.outputStream.Flush();
			
			this.currRecIdx = 0;
			this.currBlkIdx++;
		}
		
		/// <summary>
		/// Flush the current data block if it has any data in it.
		/// </summary>
		void Flush()
		{
			if (this.debug) {
				Console.Error.WriteLine("TarBuffer.FlushBlock() called.");
			}
			
			if (this.outputStream == null) {
				throw new System.IO.IOException("no output base stream defined");
			}
			
			if (this.currRecIdx > 0) {
				this.WriteBlock();
			}
			outputStream.Flush();
		}
		
		/// <summary>
		/// Close the TarBuffer. If this is an output buffer, also flush the
		/// current block before closing.
		/// </summary>
		public void Close()
		{
			if (this.debug) {
				Console.Error.WriteLine("TarBuffer.Close().");
			}
			
			if (outputStream != null ) {
				Flush();
	
//				if ( this.outStream != System.out
//						&& this.outStream != System.err ) {
				outputStream.Close();
				outputStream = null;
			} else if (inputStream != null) {
//				if (this.inStream != System.in ) {
				inputStream.Close();
				inputStream = null;
			}
		}
	}
}
/* The original Java file had this header:
	*
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
