// TarInputStream.cs
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
	/// The TarInputStream reads a UNIX tar archive as an InputStream.
	/// methods are provided to position at each successive entry in
	/// the archive, and the read each entry as a normal input stream
	/// using read().
	/// </summary>
	public class TarInputStream : Stream
	{
		#region Constructors
		/// <summary>
		/// Construct a TarInputStream with default block factor
		/// </summary>
		/// <param name="inputStream">stream to source data from</param>
		public TarInputStream(Stream inputStream)
			: this(inputStream, TarBuffer.DefaultBlockFactor)
		{
		}

		/// <summary>
		/// Construct a TarInputStream with user specified block factor
		/// </summary>
		/// <param name="inputStream">stream to source data from</param>
		/// <param name="blockFactor">block factor to apply to archive</param>
		public TarInputStream(Stream inputStream, int blockFactor)
		{
			this.inputStream = inputStream;
			tarBuffer      = TarBuffer.CreateInputTarBuffer(inputStream, blockFactor);
		}

		#endregion

        /// <summary>
        /// Get/set flag indicating ownership of the underlying stream.
        /// When the flag is true <see cref="Close"></see> will close the underlying stream also.
        /// </summary>
        public bool IsStreamOwner
        {
            get { return tarBuffer.IsStreamOwner; }
            set { tarBuffer.IsStreamOwner = value; }
        }

		#region Stream Overrides
		/// <summary>
		/// Gets a value indicating whether the current stream supports reading
		/// </summary>
		public override bool CanRead
		{
			get {
				return inputStream.CanRead;
			}
		}
		
		/// <summary>
		/// Gets a value indicating whether the current stream supports seeking
		/// This property always returns false.
		/// </summary>
		public override bool CanSeek {
			get {
				return false;
			}
		}
		
		/// <summary>
		/// Gets a value indicating if the stream supports writing.
		/// This property always returns false.
		/// </summary>
		public override bool CanWrite {
			get {
				return false;
			}
		}
		
		/// <summary>
		/// The length in bytes of the stream
		/// </summary>
		public override long Length {
			get {
				return inputStream.Length;
			}
		}
		
		/// <summary>
		/// Gets or sets the position within the stream. 
		/// Setting the Position is not supported and throws a NotSupportedExceptionNotSupportedException
		/// </summary>
		/// <exception cref="NotSupportedException">Any attempt to set position</exception>
		public override long Position {
			get {
				return inputStream.Position;
			}
			set {
				throw new NotSupportedException("TarInputStream Seek not supported");
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
		/// Set the streams position.  This operation is not supported and will throw a NotSupportedException
		/// </summary>
		/// <param name="offset">The offset relative to the origin to seek to.</param>
		/// <param name="origin">The <see cref="SeekOrigin"/> to start seeking from.</param>
		/// <returns>The new position in the stream.</returns>
		/// <exception cref="NotSupportedException">Any access</exception>
		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("TarInputStream Seek not supported");
		}
		
		/// <summary>
		/// Sets the length of the stream
		/// This operation is not supported and will throw a NotSupportedException
		/// </summary>
		/// <param name="value">The new stream length.</param>
		/// <exception cref="NotSupportedException">Any access</exception>
		public override void SetLength(long value)
		{
			throw new NotSupportedException("TarInputStream SetLength not supported");
		}
		
		/// <summary>
		/// Writes a block of bytes to this stream using data from a buffer.
		/// This operation is not supported and will throw a NotSupportedException
		/// </summary>
		/// <param name="buffer">The buffer containing bytes to write.</param>
		/// <param name="offset">The offset in the buffer of the frist byte to write.</param>
		/// <param name="count">The number of bytes to write.</param>
		/// <exception cref="NotSupportedException">Any access</exception>
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("TarInputStream Write not supported");
		}
		
		/// <summary>
		/// Writes a byte to the current position in the file stream.
		/// This operation is not supported and will throw a NotSupportedException
		/// </summary>
		/// <param name="value">The byte value to write.</param>
		/// <exception cref="NotSupportedException">Any access</exception>
		public override void WriteByte(byte value)
		{
			throw new NotSupportedException("TarInputStream WriteByte not supported");
		}
		/// <summary>
		/// Reads a byte from the current tar archive entry.
		/// </summary>
		/// <returns>A byte cast to an int; -1 if the at the end of the stream.</returns>
		public override int ReadByte()
		{
			byte[] oneByteBuffer = new byte[1];
			int num = Read(oneByteBuffer, 0, 1);
			if (num <= 0)
			{
                // return -1 to indicate that no byte was read.
				return -1;
			}
			return oneByteBuffer[0];
		}

		/// <summary>
		/// Reads bytes from the current tar archive entry.
		///
		/// This method is aware of the boundaries of the current
		/// entry in the archive and will deal with them appropriately
		/// </summary>
		/// <param name="buffer">
		/// The buffer into which to place bytes read.
		/// </param>
		/// <param name="offset">
		/// The offset at which to place bytes read.
		/// </param>
		/// <param name="count">
		/// The number of bytes to read.
		/// </param>
		/// <returns>
		/// The number of bytes read, or 0 at end of stream/EOF.
		/// </returns>
		public override int Read(byte[] buffer, int offset, int count)
		{
			if ( buffer == null )
			{
				throw new ArgumentNullException("buffer");
			}

			int totalRead = 0;

			if (entryOffset >= entrySize)
			{
				return 0;
			}

			long numToRead = count;

			if ((numToRead + entryOffset) > entrySize)
			{
				numToRead = entrySize - entryOffset;
			}

			if (readBuffer != null)
			{
				int sz = (numToRead > readBuffer.Length) ? readBuffer.Length : (int)numToRead;

				Array.Copy(readBuffer, 0, buffer, offset, sz);

				if (sz >= readBuffer.Length)
				{
					readBuffer = null;
				}
				else
				{
					int newLen = readBuffer.Length - sz;
					byte[] newBuf = new byte[newLen];
					Array.Copy(readBuffer, sz, newBuf, 0, newLen);
					readBuffer = newBuf;
				}

				totalRead += sz;
				numToRead -= sz;
				offset += sz;
			}
			
			while (numToRead > 0)
			{
				byte[] rec = tarBuffer.ReadBlock();
				if (rec == null)
				{
					// Unexpected EOF!
					throw new TarException("unexpected EOF with " + numToRead + " bytes unread");
				}

				int sz     = (int)numToRead;
				int recLen = rec.Length;

				if (recLen > sz)
				{
					Array.Copy(rec, 0, buffer, offset, sz);
					readBuffer = new byte[recLen - sz];
					Array.Copy(rec, sz, readBuffer, 0, recLen - sz);
				}
				else
				{
					sz = recLen;
					Array.Copy(rec, 0, buffer, offset, recLen);
				}

				totalRead += sz;
				numToRead -= sz;
				offset += sz;
			}

			entryOffset += totalRead;

			return totalRead;
		}

		/// <summary>
		/// Closes this stream. Calls the TarBuffer's close() method.
		/// The underlying stream is closed by the TarBuffer.
		/// </summary>
		public override void Close()
		{
			tarBuffer.Close();
		}

		#endregion

		/// <summary>
		/// Set the entry factory for this instance.
		/// </summary>
		/// <param name="factory">The factory for creating new entries</param>
		public void SetEntryFactory(IEntryFactory factory)
		{
			entryFactory = factory;
		}
		
		/// <summary>
		/// Get the record size being used by this stream's TarBuffer.
		/// </summary>
		public int RecordSize
		{
			get { return tarBuffer.RecordSize; }
		}

		/// <summary>
		/// Get the record size being used by this stream's TarBuffer.
		/// </summary>
		/// <returns>
		/// TarBuffer record size.
		/// </returns>
		[Obsolete("Use RecordSize property instead")]
		public int GetRecordSize()
		{
			return tarBuffer.RecordSize;
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
		public long Available {
			get {
				return entrySize - entryOffset;
			}
		}
		
		/// <summary>
		/// Skip bytes in the input buffer. This skips bytes in the
		/// current entry's data, not the entire archive, and will
		/// stop at the end of the current entry's data if the number
		/// to skip extends beyond that point.
		/// </summary>
		/// <param name="skipCount">
		/// The number of bytes to skip.
		/// </param>
		public void Skip(long skipCount)
		{
			// TODO: REVIEW efficiency of TarInputStream.Skip
			// This is horribly inefficient, but it ensures that we
			// properly skip over bytes via the TarBuffer...
			//
			byte[] skipBuf = new byte[8 * 1024];
			
			for (long num = skipCount; num > 0;) {
				int toRead = num > skipBuf.Length ? skipBuf.Length : (int)num;
				int numRead = Read(skipBuf, 0, toRead);
				
				if (numRead == -1) {
					break;
				}
				
				num -= numRead;
			}
		}
		
		/// <summary>
		/// Return a value of true if marking is supported; false otherwise.
		/// </summary>
		/// <remarks>Currently marking is not supported, the return value is always false.</remarks>
		public bool IsMarkSupported {
			get {
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
			if (hasHitEOF) {
				return null;
			}
			
			if (currentEntry != null) {
				SkipToNextEntry();
			}
			
			byte[] headerBuf = tarBuffer.ReadBlock();
			
			if (headerBuf == null) {
				hasHitEOF = true;
			} else if (TarBuffer.IsEndOfArchiveBlock(headerBuf)) {
				hasHitEOF = true;
			}
			
			if (hasHitEOF) {
				currentEntry = null;
			} else {
				try {
					TarHeader header = new TarHeader();
					header.ParseBuffer(headerBuf);
					if ( !header.IsChecksumValid )
					{
						throw new TarException("Header checksum is invalid");
					}
					this.entryOffset = 0;
					this.entrySize = header.Size;
					
					StringBuilder longName = null;
					
					if (header.TypeFlag == TarHeader.LF_GNU_LONGNAME) {
						
						byte[] nameBuffer = new byte[TarBuffer.BlockSize];
						long numToRead = this.entrySize;
						
						longName = new StringBuilder();
						
						while (numToRead > 0) {
							int numRead = this.Read(nameBuffer, 0, (numToRead > nameBuffer.Length ? nameBuffer.Length : (int)numToRead));
							
							if (numRead == -1) {
								throw new InvalidHeaderException("Failed to read long name entry");
							}
							
							longName.Append(TarHeader.ParseName(nameBuffer, 0, numRead).ToString());
							numToRead -= numRead;
						}
						
						SkipToNextEntry();
						headerBuf = this.tarBuffer.ReadBlock();
					} else if (header.TypeFlag == TarHeader.LF_GHDR) {  // POSIX global extended header 
						// Ignore things we dont understand completely for now
						SkipToNextEntry();
						headerBuf = this.tarBuffer.ReadBlock();
					} else if (header.TypeFlag == TarHeader.LF_XHDR) {  // POSIX extended header
						// Ignore things we dont understand completely for now
						SkipToNextEntry();
						headerBuf = this.tarBuffer.ReadBlock();
					} else if (header.TypeFlag == TarHeader.LF_GNU_VOLHDR) {
						// TODO: could show volume name when verbose
						SkipToNextEntry();
						headerBuf = this.tarBuffer.ReadBlock();
					} else if (header.TypeFlag != TarHeader.LF_NORMAL && 
							   header.TypeFlag != TarHeader.LF_OLDNORM &&
							   header.TypeFlag != TarHeader.LF_DIR) {
						// Ignore things we dont understand completely for now
						SkipToNextEntry();
						headerBuf = tarBuffer.ReadBlock();
					}
					
					if (entryFactory == null) {
						currentEntry = new TarEntry(headerBuf);
						if (longName != null) {
							currentEntry.Name = longName.ToString();
						}
					} else {
						currentEntry = entryFactory.CreateEntry(headerBuf);
					}
					
					// Magic was checked here for 'ustar' but there are multiple valid possibilities
					// so this is not done anymore.
					
					entryOffset = 0;
					
					// TODO: Review How do we resolve this discrepancy?!
					entrySize = this.currentEntry.Size;
				} catch (InvalidHeaderException ex) {
					entrySize = 0;
					entryOffset = 0;
					currentEntry = null;
					string errorText = string.Format("Bad header in record {0} block {1} {2}",
						tarBuffer.CurrentRecord, tarBuffer.CurrentBlock, ex.Message);
					throw new InvalidHeaderException(errorText);
				}
			}
			return currentEntry;
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
			byte[] tempBuffer = new byte[32 * 1024];
			
			while (true) {
				int numRead = Read(tempBuffer, 0, tempBuffer.Length);
				if (numRead <= 0) {
					break;
				}
				outputStream.Write(tempBuffer, 0, numRead);
			}
		}

		void SkipToNextEntry()
		{
			long numToSkip = entrySize - entryOffset;

			if (numToSkip > 0)
			{
				Skip(numToSkip);
			}

			readBuffer = null;
		}
		
		/// <summary>
		/// This interface is provided, along with the method <see cref="SetEntryFactory"/>, to allow
		/// the programmer to have their own <see cref="TarEntry"/> subclass instantiated for the
		/// entries return from <see cref="GetNextEntry"/>.
		/// </summary>
		public interface IEntryFactory
		{
			/// <summary>
			/// Create an entry based on name alone
			/// </summary>
			/// <param name="name">
			/// Name of the new EntryPointNotFoundException to create
			/// </param>
			/// <returns>created TarEntry or descendant class</returns>
			TarEntry CreateEntry(string name);
			
			/// <summary>
			/// Create an instance based on an actual file
			/// </summary>
			/// <param name="fileName">
			/// Name of file to represent in the entry
			/// </param>
			/// <returns>
			/// Created TarEntry or descendant class
			/// </returns>
			TarEntry CreateEntryFromFile(string fileName);
			
			/// <summary>
			/// Create a tar entry based on the header information passed
			/// </summary>
			/// <param name="headerBuffer">
			/// Buffer containing header information to create an an entry from.
			/// </param>
			/// <returns>
			/// Created TarEntry or descendant class
			/// </returns>
			TarEntry CreateEntry(byte[] headerBuffer);
		}

		/// <summary>
		/// Standard entry factory class creating instances of the class TarEntry
		/// </summary>
		public class EntryFactoryAdapter : IEntryFactory
		{
			/// <summary>
			/// Create a <see cref="TarEntry"/> based on named
			/// </summary>
			/// <param name="name">The name to use for the entry</param>
			/// <returns>A new <see cref="TarEntry"/></returns>
			public TarEntry CreateEntry(string name)
			{
				return TarEntry.CreateTarEntry(name);
			}
			
			/// <summary>
			/// Create a tar entry with details obtained from <paramref name="fileName">file</paramref>
			/// </summary>
			/// <param name="fileName">The name of the file to retrieve details from.</param>
			/// <returns>A new <see cref="TarEntry"/></returns>
			public TarEntry CreateEntryFromFile(string fileName)
			{
				return TarEntry.CreateEntryFromFile(fileName);
			}

			/// <summary>
			/// Create an entry based on details in <paramref name="headerBuffer">header</paramref>
			/// </summary>			
			/// <param name="headerBuffer">The buffer containing entry details.</param>
			/// <returns>A new <see cref="TarEntry"/></returns>
			public TarEntry CreateEntry(byte[] headerBuffer)
			{
				return new TarEntry(headerBuffer);
			}
		}

		#region Instance Fields
		/// <summary>
		/// Flag set when last block has been read
		/// </summary>
		protected bool hasHitEOF;

		/// <summary>
		/// Size of this entry as recorded in header
		/// </summary>
		protected long entrySize;

		/// <summary>
		/// Number of bytes read for this entry so far
		/// </summary>
		protected long entryOffset;

		/// <summary>
		/// Buffer used with calls to <code>Read()</code>
		/// </summary>
		protected byte[] readBuffer;

		/// <summary>
		/// Working buffer
		/// </summary>
		protected TarBuffer tarBuffer;

		/// <summary>
		/// Current entry being read
		/// </summary>
		TarEntry  currentEntry;

		/// <summary>
		/// Factory used to create TarEntry or descendant class instance
		/// </summary>
		protected IEntryFactory entryFactory;

		/// <summary>
		/// Stream used as the source of input data.
		/// </summary>
		readonly Stream inputStream;
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
	
