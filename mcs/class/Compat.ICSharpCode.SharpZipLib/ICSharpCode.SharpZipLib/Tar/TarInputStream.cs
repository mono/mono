// TarInputStream.cs
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
	/// The TarInputStream reads a UNIX tar archive as an InputStream.
	/// methods are provided to position at each successive entry in
	/// the archive, and the read each entry as a normal input stream
	/// using read().
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public class TarInputStream : Stream
	{
		protected bool debug;
		protected bool hasHitEOF;
		
		protected int entrySize;
		protected int entryOffset;
		
		protected byte[] readBuf;
		
		protected TarBuffer buffer;
		protected TarEntry  currEntry;
		protected IEntryFactory eFactory;
		
		Stream inputStream;

		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override bool CanRead 
		{
			get 
			{
				return inputStream.CanRead;
			}
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override bool CanSeek 
		{
         // TODO is this valid?  should it return false?
			get 
			{
				return inputStream.CanSeek;
			}
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override bool CanWrite 
		{
			get 
			{
				return inputStream.CanWrite;
			}
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override long Length 
		{
			get 
			{
				return inputStream.Length;
			}
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override long Position 
		{
			get 
			{
				return inputStream.Position;
			}
			set 
			{
				inputStream.Position = value;
			}
		}
		
		/// <summary>
		/// Flushes the baseInputStream
		/// </summary>
		public override void Flush()
		{
			inputStream.Flush();
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override long Seek(long offset, SeekOrigin origin)
		{
         // TODO allow this?
			return inputStream.Seek(offset, origin);
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override void SetLength(long val)
		{
			inputStream.SetLength(val);
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override void Write(byte[] array, int offset, int count)
		{
			inputStream.Write(array, offset, count);
		}
		
		/// <summary>
		/// I needed to implement the abstract member.
		/// </summary>
		public override void WriteByte(byte val)
		{
			inputStream.WriteByte(val);
		}
			
		
		public TarInputStream(Stream inputStream) : this(inputStream, TarBuffer.DefaultBlockFactor)
		{
		}
		
		public TarInputStream(Stream inputStream, int blockFactor)
		{
			this.inputStream = inputStream;
			this.buffer      = TarBuffer.CreateInputTarBuffer(inputStream, blockFactor);
			
			this.readBuf   = null;
			this.debug     = false;
			this.hasHitEOF = false;
			this.eFactory  = null;
		}
		
		public void SetDebug(bool debugFlag)
		{
			this.debug = debugFlag;
			SetBufferDebug(debugFlag);
		}

		public void SetBufferDebug(bool debug)
		{
			this.buffer.SetDebug(debug);
		}
		
		
		
		public void SetEntryFactory(IEntryFactory factory)
		{
			this.eFactory = factory;
		}
		
		/// <summary>
		/// Closes this stream. Calls the TarBuffer's close() method.
		/// The underlying stream is closed by the TarBuffer.
		/// </summary>
		public override void Close()
		{
			this.buffer.Close();
		}
		
		/// <summary>
		/// Get the record size being used by this stream's TarBuffer.
		/// </summary>
		/// <returns>
		/// TarBuffer record size.
		/// </returns>
		public int GetRecordSize()
		{
			return this.buffer.GetRecordSize();
		}
		
		/// <summary>
		/// Get the available data that can be read from the current
		/// entry in the archive. This does not indicate how much data
		/// is left in the entire archive, only in the current entry.
		/// This value is determined from the entry's size header field
		/// and the amount of data already read from the current entry.
		/// </summary>
		/// <returns>
		/// The number of available bytes for the current entry.
		/// </returns>
		public int Available 
		{
			get 
			{
				return this.entrySize - this.entryOffset;
			}
		}
		
		/// <summary>
		/// Skip bytes in the input buffer. This skips bytes in the
		/// current entry's data, not the entire archive, and will
		/// stop at the end of the current entry's data if the number
		/// to skip extends beyond that point.
		/// </summary>
		/// <param name="numToSkip">
		/// The number of bytes to skip.
		/// </param>
		public void Skip(int numToSkip)
		{
			// TODO REVIEW
			// This is horribly inefficient, but it ensures that we
			// properly skip over bytes via the TarBuffer...
			//
			byte[] skipBuf = new byte[8 * 1024];
			
			for (int num = numToSkip; num > 0;)
			{
				int numRead = this.Read(skipBuf, 0, (num > skipBuf.Length ? skipBuf.Length : num));
				
				if (numRead == -1) 
				{
					break;
				}
				
				num -= numRead;
			}
		}
		
		/// <summary>
		/// Since we do not support marking just yet, we return false.
		/// </summary>
		public bool IsMarkSupported 
		{
			get 
			{
				return false;
			}
		}
		
		/// <summary>
		/// Since we do not support marking just yet, we do nothing.
		/// </summary>
		/// <param name ="markLimit">
		/// The limit to mark.
		/// </param>
		public void Mark(int markLimit)
		{
		}
		
		/// <summary>
		/// Since we do not support marking just yet, we do nothing.
		/// </summary>
		public void Reset()
		{
		}

      void SkipToNextEntry()
      {
         int numToSkip = this.entrySize - this.entryOffset;
				
         if (this.debug) 
         {
            //Console.WriteLine.WriteLine("TarInputStream: SKIP currENTRY '" + this.currEntry.Name + "' SZ " + this.entrySize + " OFF " + this.entryOffset + "  skipping " + numToSkip + " bytes");
         }
				
         if (numToSkip > 0) 
         {
            this.Skip(numToSkip);
         }
				
         this.readBuf = null;
      }

		/// <summary>
		/// Get the next entry in this tar archive. This will skip
		/// over any remaining data in the current entry, if there
		/// is one, and place the input stream at the header of the
		/// next entry, and read the header and instantiate a new
		/// TarEntry from the header bytes and return that entry.
		/// If there are no more entries in the archive, null will
		/// be returned to indicate that the end of the archive has
		/// been reached.
		/// </summary>
		/// <returns>
		/// The next TarEntry in the archive, or null.
		/// </returns>
		public TarEntry GetNextEntry()
		{
			if (this.hasHitEOF) 
			{
				return null;
			}
			
			if (this.currEntry != null) 
			{
            SkipToNextEntry();
			}
			
			byte[] headerBuf = this.buffer.ReadBlock();
			
			if (headerBuf == null) 
			{
				if (this.debug) 
				{
					//Console.WriteLine.WriteLine("READ NULL BLOCK");
				}
				
				this.hasHitEOF = true;
			} 
			else if (this.buffer.IsEOFBlock(headerBuf)) 
			{
				if (this.debug) 
				{
					//Console.WriteLine.WriteLine( "READ EOF BLOCK" );
				}
				
				this.hasHitEOF = true;
			}
			
			if (this.hasHitEOF) 
			{
				this.currEntry = null;
			} 
			else 
			{
				try 
				{
               TarHeader header = new TarHeader();
               header.ParseBuffer(headerBuf);
               this.entryOffset = 0;
               this.entrySize = (int)header.size;

               StringBuilder longName = null;

               if (header.typeFlag == TarHeader.LF_GNU_LONGNAME)
               {
                  Console.WriteLine("TarInputStream: Long name found '" + header.name + "' size = " + header.size); // DEBUG

                  byte[] nameBuffer = new byte[TarBuffer.BlockSize];

                  int numToRead = this.entrySize;

                  longName = new StringBuilder();

                  while (numToRead > 0)
                  {
                     int numRead = this.Read(nameBuffer, 0, (numToRead > nameBuffer.Length ? nameBuffer.Length : numToRead));
				
                     if (numRead == -1) 
                     {
                        throw new InvalidHeaderException("Failed to read long name entry");
                     }
				
                     longName.Append(TarHeader.ParseName(nameBuffer, 0, numRead).ToString());
                     numToRead -= numRead;
                  }

                  Console.WriteLine("TarInputStream: Long name is '" + longName.ToString()); // DEBUG

                  SkipToNextEntry();
                  headerBuf = this.buffer.ReadBlock();
               }
               else if (header.typeFlag == TarHeader.LF_GHDR) // POSIX global extended header
               {
                  // Ignore things we dont understand completely for now
                  SkipToNextEntry();
                  headerBuf = this.buffer.ReadBlock();
               }
               else if (header.typeFlag == TarHeader.LF_XHDR)     // POSIX extended header
               {
                  // Ignore things we dont understand completely for now
                  SkipToNextEntry();
                  headerBuf = this.buffer.ReadBlock();
               }
               else if (header.typeFlag == TarHeader.LF_GNU_VOLHDR)
               {
                  // TODO could show volume name when verbose?
                  SkipToNextEntry();
                  headerBuf = this.buffer.ReadBlock();
               }
               else if (header.typeFlag != TarHeader.LF_NORMAL
                  && header.typeFlag != TarHeader.LF_OLDNORM)
               {
                  // Ignore things we dont understand completely for now
                  SkipToNextEntry();
                  headerBuf = this.buffer.ReadBlock();
               }

               if (this.eFactory == null) 
					{
                  this.currEntry = new TarEntry(headerBuf);
                  if (longName != null)
                  {
                     this.currEntry.TarHeader.name.Length = 0;
                     this.currEntry.TarHeader.name.Append(longName.ToString());
                  }
					} 
					else 
					{
                  this.currEntry = this.eFactory.CreateEntry(headerBuf);
					}

					// TODO -jr- ustar is not the only magic possible by any means
               // tar, xtar, ... 
					if (!(headerBuf[257] == 'u' && headerBuf[258] == 's' && headerBuf[259] == 't' && headerBuf[260] == 'a' && headerBuf[261] == 'r')) 
					{
						throw new InvalidHeaderException("header magic is not 'ustar', but '" + headerBuf[257] + headerBuf[258] + headerBuf[259] + headerBuf[260] + headerBuf[261] + 
							"', or (dec) " + ((int)headerBuf[257]) + ", " + ((int)headerBuf[258]) + ", " + ((int)headerBuf[259]) + ", " + ((int)headerBuf[260]) + ", " + ((int)headerBuf[261]));
					}
					        
					if (this.debug) 
					{
						//Console.WriteLine.WriteLine("TarInputStream: SET CURRENTRY '" + this.currEntry.Name + "' size = " + this.currEntry.Size);
					}
			
					this.entryOffset = 0;
					
					// TODO REVIEW How do we resolve this discrepancy?!
					this.entrySize = (int) this.currEntry.Size;
				} 
				catch (InvalidHeaderException ex) 
				{
					this.entrySize = 0;
					this.entryOffset = 0;
					this.currEntry = null;
					throw new InvalidHeaderException("bad header in record " + this.buffer.GetCurrentBlockNum() + " block " + this.buffer.GetCurrentBlockNum() + ", " + ex.Message);
				}
			}
			return this.currEntry;
		}
		
		/// <summary>
		/// Reads a byte from the current tar archive entry.
		/// This method simply calls read(byte[], int, int).
		/// </summary>
		public override int ReadByte()
		{
			byte[] oneByteBuffer = new byte[1];
			int num = this.Read(oneByteBuffer, 0, 1);
			if (num <= 0)               // return -1 to indicate that no byte was read.
			{
				return -1;
			}
			return (int)oneByteBuffer[0];
		}
		
		/// <summary>
		/// Reads bytes from the current tar archive entry.
		/// 
		/// This method is aware of the boundaries of the current
		/// entry in the archive and will deal with them appropriately
		/// </summary>
		/// <param name="outputBuffer">
		/// The buffer into which to place bytes read.
		/// </param>
		/// <param name="offset">
		/// The offset at which to place bytes read.
		/// </param>
		/// <param name="numToRead">
		/// The number of bytes to read.
		/// </param>
		/// <returns>
		/// The number of bytes read, or 0 at end of stream/EOF.
		/// </returns>
		public override int Read(byte[] outputBuffer, int offset, int numToRead)
		{
			int totalRead = 0;
			
			if (this.entryOffset >= this.entrySize) 
			{
				return 0;
			}
			
			if ((numToRead + this.entryOffset) > this.entrySize) 
			{
				numToRead = this.entrySize - this.entryOffset;
			}
			
			if (this.readBuf != null) 
			{
				int sz = (numToRead > this.readBuf.Length) ? this.readBuf.Length : numToRead;
				
				Array.Copy(this.readBuf, 0, outputBuffer, offset, sz);
				
				if (sz >= this.readBuf.Length) 
				{
					this.readBuf = null;
				} 
				else 
				{
					int newLen = this.readBuf.Length - sz;
					byte[] newBuf = new byte[newLen];
					Array.Copy(this.readBuf, sz, newBuf, 0, newLen);
					this.readBuf = newBuf;
				}
				
				totalRead += sz;
				numToRead -= sz;
				offset += sz;
			}
			
			while (numToRead > 0) 
			{
				byte[] rec = this.buffer.ReadBlock();
				if (rec == null) 
				{
					// Unexpected EOF!
					throw new IOException("unexpected EOF with " + numToRead + " bytes unread");
				}
				
				int sz     = numToRead;
				int recLen = rec.Length;
				
				if (recLen > sz) 
				{
					Array.Copy(rec, 0, outputBuffer, offset, sz);
					this.readBuf = new byte[recLen - sz];
					Array.Copy(rec, sz, this.readBuf, 0, recLen - sz);
				} 
				else 
				{
					sz = recLen;
					Array.Copy(rec, 0, outputBuffer, offset, recLen);
				}
				
				totalRead += sz;
				numToRead -= sz;
				offset += sz;
			}
			
			this.entryOffset += totalRead;
			
			return totalRead;
		}
		
		/// <summary>
		/// Copies the contents of the current tar archive entry directly into
		/// an output stream.
		/// </summary>
		/// <param name="outputStream">
		/// The OutputStream into which to write the entry's data.
		/// </param>
		public void CopyEntryContents(Stream outputStream)
		{
			byte[] buf = new byte[32 * 1024];
			
			while (true) 
			{
				int numRead = this.Read(buf, 0, buf.Length);
				if (numRead <= 0) 
				{
					break;
				}
				outputStream.Write(buf, 0, numRead);
			}
		}
		
		/// <summary>
		/// This interface is provided, with the method setEntryFactory(), to allow
		/// the programmer to have their own TarEntry subclass instantiated for the
		/// entries return from getNextEntry().
		/// </summary>
		[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
		public interface IEntryFactory
		{
			TarEntry CreateEntry(string name);
			
			TarEntry CreateEntryFromFile(string fileName);
			
			TarEntry CreateEntry(byte[] headerBuf);
		}
		
		[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
		public class EntryFactoryAdapter : IEntryFactory
		{
			public TarEntry CreateEntry(string name)
			{
				return TarEntry.CreateTarEntry(name);
			}
			
			public TarEntry CreateEntryFromFile(string fileName)
			{
				return TarEntry.CreateEntryFromFile(fileName);
			}
			
			public TarEntry CreateEntry(byte[] headerBuf)
			{
				return new TarEntry(headerBuf);
			}
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
	
