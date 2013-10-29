// ZipHelperStream.cs
//
// Copyright 2006, 2007 John Reilly
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

namespace ICSharpCode.SharpZipLib.Zip
{

	/// <summary>
	/// Holds data pertinent to a data descriptor.
	/// </summary>
	public class DescriptorData
	{
		/// <summary>
		/// Get /set the compressed size of data.
		/// </summary>
		public long CompressedSize
		{
			get { return compressedSize; }
			set { compressedSize = value; }
		}

		/// <summary>
		/// Get / set the uncompressed size of data
		/// </summary>
		public long Size
		{
			get { return size; }
			set { size = value; }
		}

		/// <summary>
		/// Get /set the crc value.
		/// </summary>
		public long Crc
		{
			get { return crc; }
			set { crc = (value & 0xffffffff); }
		}

		#region Instance Fields
		long size;
		long compressedSize;
		long crc;
		#endregion
	}

	class EntryPatchData
	{
		public long SizePatchOffset
		{
			get { return sizePatchOffset_; }
			set { sizePatchOffset_ = value; }
		}

		public long CrcPatchOffset
		{
			get { return crcPatchOffset_; }
			set { crcPatchOffset_ = value; }
		}

		#region Instance Fields
		long sizePatchOffset_;
		long crcPatchOffset_;
		#endregion
	}

	/// <summary>
	/// This class assists with writing/reading from Zip files.
	/// </summary>
	internal class ZipHelperStream : Stream
	{
		#region Constructors
		/// <summary>
		/// Initialise an instance of this class.
		/// </summary>
		/// <param name="name">The name of the file to open.</param>
		public ZipHelperStream(string name)
		{
			stream_ = new FileStream(name, FileMode.Open, FileAccess.ReadWrite);
			isOwner_ = true;
		}

		/// <summary>
		/// Initialise a new instance of <see cref="ZipHelperStream"/>.
		/// </summary>
		/// <param name="stream">The stream to use.</param>
		public ZipHelperStream(Stream stream)
		{
			stream_ = stream;
		}
		#endregion

		/// <summary>
		/// Get / set a value indicating wether the the underlying stream is owned or not.
		/// </summary>
		/// <remarks>If the stream is owned it is closed when this instance is closed.</remarks>
		public bool IsStreamOwner
		{
			get { return isOwner_; }
			set { isOwner_ = value; }
		}

		#region Base Stream Methods
		public override bool CanRead
		{
			get { return stream_.CanRead; }
		}

		public override bool CanSeek
		{
			get { return stream_.CanSeek; }
		}

#if !NET_1_0 && !NET_1_1 && !NETCF_1_0
		public override bool CanTimeout
		{
			get { return stream_.CanTimeout; }
		}
#endif

		public override long Length
		{
			get { return stream_.Length; }
		}

		public override long Position
		{
			get { return stream_.Position; }
			set { stream_.Position = value;	}
		}

		public override bool CanWrite
		{
			get { return stream_.CanWrite; }
		}

		public override void Flush()
		{
			stream_.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return stream_.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			stream_.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return stream_.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			stream_.Write(buffer, offset, count);
		}

		/// <summary>
		/// Close the stream.
		/// </summary>
		/// <remarks>
		/// The underlying stream is closed only if <see cref="IsStreamOwner"/> is true.
		/// </remarks>
		override public void Close()
		{
			Stream toClose = stream_;
			stream_ = null;
			if (isOwner_ && (toClose != null))
			{
				isOwner_ = false;
				toClose.Close();
			}
		}
		#endregion

		// Write the local file header
		// TODO: ZipHelperStream.WriteLocalHeader is not yet used and needs checking for ZipFile and ZipOuptutStream usage
		void WriteLocalHeader(ZipEntry entry, EntryPatchData patchData)
		{
			CompressionMethod method = entry.CompressionMethod;
			bool headerInfoAvailable = true; // How to get this?
			bool patchEntryHeader = false;

			WriteLEInt(ZipConstants.LocalHeaderSignature);

			WriteLEShort(entry.Version);
			WriteLEShort(entry.Flags);
			WriteLEShort((byte)method);
			WriteLEInt((int)entry.DosTime);

			if (headerInfoAvailable == true) {
				WriteLEInt((int)entry.Crc);
				if ( entry.LocalHeaderRequiresZip64 ) {
					WriteLEInt(-1);
					WriteLEInt(-1);
				}
				else {
					WriteLEInt(entry.IsCrypted ? (int)entry.CompressedSize + ZipConstants.CryptoHeaderSize : (int)entry.CompressedSize);
					WriteLEInt((int)entry.Size);
				}
			} else {
				if (patchData != null) {
					patchData.CrcPatchOffset = stream_.Position;
				}
				WriteLEInt(0);	// Crc

				if ( patchData != null ) {
					patchData.SizePatchOffset = stream_.Position;
				}

				// For local header both sizes appear in Zip64 Extended Information
				if ( entry.LocalHeaderRequiresZip64 && patchEntryHeader ) {
					WriteLEInt(-1);
					WriteLEInt(-1);
				}
				else {
					WriteLEInt(0);	// Compressed size
					WriteLEInt(0);	// Uncompressed size
				}
			}

			byte[] name = ZipConstants.ConvertToArray(entry.Flags, entry.Name);

			if (name.Length > 0xFFFF) {
				throw new ZipException("Entry name too long.");
			}

			ZipExtraData ed = new ZipExtraData(entry.ExtraData);

			if (entry.LocalHeaderRequiresZip64 && (headerInfoAvailable || patchEntryHeader)) {
				ed.StartNewEntry();
				if (headerInfoAvailable) {
					ed.AddLeLong(entry.Size);
					ed.AddLeLong(entry.CompressedSize);
				}
				else {
					ed.AddLeLong(-1);
					ed.AddLeLong(-1);
				}
				ed.AddNewEntry(1);

				if ( !ed.Find(1) ) {
					throw new ZipException("Internal error cant find extra data");
				}

				if ( patchData != null ) {
					patchData.SizePatchOffset = ed.CurrentReadIndex;
				}
			}
			else {
				ed.Delete(1);
			}

			byte[] extra = ed.GetEntryData();

			WriteLEShort(name.Length);
			WriteLEShort(extra.Length);

			if ( name.Length > 0 ) {
				stream_.Write(name, 0, name.Length);
			}

			if ( entry.LocalHeaderRequiresZip64 && patchEntryHeader ) {
				patchData.SizePatchOffset += stream_.Position;
			}

			if ( extra.Length > 0 ) {
				stream_.Write(extra, 0, extra.Length);
			}
		}

		/// <summary>
		/// Locates a block with the desired <paramref name="signature"/>.
		/// </summary>
		/// <param name="signature">The signature to find.</param>
		/// <param name="endLocation">Location, marking the end of block.</param>
		/// <param name="minimumBlockSize">Minimum size of the block.</param>
		/// <param name="maximumVariableData">The maximum variable data.</param>
		/// <returns>Eeturns the offset of the first byte after the signature; -1 if not found</returns>
		public long LocateBlockWithSignature(int signature, long endLocation, int minimumBlockSize, int maximumVariableData)
		{
			long pos = endLocation - minimumBlockSize;
			if ( pos < 0 ) {
				return -1;
			}

			long giveUpMarker = Math.Max(pos - maximumVariableData, 0);

			// TODO: This loop could be optimised for speed.
			do {
				if ( pos < giveUpMarker ) {
					return -1;
				}
				Seek(pos--, SeekOrigin.Begin);
			} while ( ReadLEInt() != signature );

			return Position;
		}

		/// <summary>
		/// Write Zip64 end of central directory records (File header and locator).
		/// </summary>
		/// <param name="noOfEntries">The number of entries in the central directory.</param>
		/// <param name="sizeEntries">The size of entries in the central directory.</param>
		/// <param name="centralDirOffset">The offset of the dentral directory.</param>
		public void WriteZip64EndOfCentralDirectory(long noOfEntries, long sizeEntries, long centralDirOffset)
		{
			long centralSignatureOffset = stream_.Position;
			WriteLEInt(ZipConstants.Zip64CentralFileHeaderSignature);
			WriteLELong(44);    // Size of this record (total size of remaining fields in header or full size - 12)
			WriteLEShort(ZipConstants.VersionMadeBy);   // Version made by
			WriteLEShort(ZipConstants.VersionZip64);   // Version to extract
			WriteLEInt(0);      // Number of this disk
			WriteLEInt(0);      // number of the disk with the start of the central directory
			WriteLELong(noOfEntries);       // No of entries on this disk
			WriteLELong(noOfEntries);       // Total No of entries in central directory
			WriteLELong(sizeEntries);       // Size of the central directory
			WriteLELong(centralDirOffset);  // offset of start of central directory
			// zip64 extensible data sector not catered for here (variable size)

			// Write the Zip64 end of central directory locator
			WriteLEInt(ZipConstants.Zip64CentralDirLocatorSignature);

			// no of the disk with the start of the zip64 end of central directory
			WriteLEInt(0);

			// relative offset of the zip64 end of central directory record
			WriteLELong(centralSignatureOffset);

			// total number of disks
			WriteLEInt(1);
		}

		/// <summary>
		/// Write the required records to end the central directory.
		/// </summary>
		/// <param name="noOfEntries">The number of entries in the directory.</param>
		/// <param name="sizeEntries">The size of the entries in the directory.</param>
		/// <param name="startOfCentralDirectory">The start of the central directory.</param>
		/// <param name="comment">The archive comment.  (This can be null).</param>
		public void WriteEndOfCentralDirectory(long noOfEntries, long sizeEntries,
			long startOfCentralDirectory, byte[] comment)
		{

			if ( (noOfEntries >= 0xffff) ||
				(startOfCentralDirectory >= 0xffffffff) ||
				(sizeEntries >= 0xffffffff) ) {
				WriteZip64EndOfCentralDirectory(noOfEntries, sizeEntries, startOfCentralDirectory);
			}

			WriteLEInt(ZipConstants.EndOfCentralDirectorySignature);

			// TODO: ZipFile Multi disk handling not done
			WriteLEShort(0);                    // number of this disk
			WriteLEShort(0);                    // no of disk with start of central dir


			// Number of entries
			if ( noOfEntries >= 0xffff ) {
				WriteLEUshort(0xffff);  // Zip64 marker
				WriteLEUshort(0xffff);
			}
			else {
				WriteLEShort(( short )noOfEntries);          // entries in central dir for this disk
				WriteLEShort(( short )noOfEntries);          // total entries in central directory
			}

			// Size of the central directory
			if ( sizeEntries >= 0xffffffff ) {
				WriteLEUint(0xffffffff);    // Zip64 marker
			}
			else {
				WriteLEInt(( int )sizeEntries);
			}


			// offset of start of central directory
			if ( startOfCentralDirectory >= 0xffffffff ) {
				WriteLEUint(0xffffffff);    // Zip64 marker
			}
			else {
				WriteLEInt(( int )startOfCentralDirectory);
			}

			int commentLength = (comment != null) ? comment.Length : 0;

			if ( commentLength > 0xffff ) {
				throw new ZipException(string.Format("Comment length({0}) is too long can only be 64K", commentLength));
			}

			WriteLEShort(commentLength);

			if ( commentLength > 0 ) {
				Write(comment, 0, comment.Length);
			}
		}

		#region LE value reading/writing
		/// <summary>
		/// Read an unsigned short in little endian byte order.
		/// </summary>
		/// <returns>Returns the value read.</returns>
		/// <exception cref="IOException">
		/// An i/o error occurs.
		/// </exception>
		/// <exception cref="EndOfStreamException">
		/// The file ends prematurely
		/// </exception>
		public int ReadLEShort()
		{
			int byteValue1 = stream_.ReadByte();

			if (byteValue1 < 0) {
				throw new EndOfStreamException();
			}

			int byteValue2 = stream_.ReadByte();
			if (byteValue2 < 0) {
				throw new EndOfStreamException();
			}

			return byteValue1 | (byteValue2 << 8);
		}

		/// <summary>
		/// Read an int in little endian byte order.
		/// </summary>
		/// <returns>Returns the value read.</returns>
		/// <exception cref="IOException">
		/// An i/o error occurs.
		/// </exception>
		/// <exception cref="System.IO.EndOfStreamException">
		/// The file ends prematurely
		/// </exception>
		public int ReadLEInt()
		{
			return ReadLEShort() | (ReadLEShort() << 16);
		}

		/// <summary>
		/// Read a long in little endian byte order.
		/// </summary>
		/// <returns>The value read.</returns>
		public long ReadLELong()
		{
			return (uint)ReadLEInt() | ((long)ReadLEInt() << 32);
		}

		/// <summary>
		/// Write an unsigned short in little endian byte order.
		/// </summary>
		/// <param name="value">The value to write.</param>
		public void WriteLEShort(int value)
		{
			stream_.WriteByte(( byte )(value & 0xff));
			stream_.WriteByte(( byte )((value >> 8) & 0xff));
		}

		/// <summary>
		/// Write a ushort in little endian byte order.
		/// </summary>
		/// <param name="value">The value to write.</param>
		public void WriteLEUshort(ushort value)
		{
			stream_.WriteByte(( byte )(value & 0xff));
			stream_.WriteByte(( byte )(value >> 8));
		}

		/// <summary>
		/// Write an int in little endian byte order.
		/// </summary>
		/// <param name="value">The value to write.</param>
		public void WriteLEInt(int value)
		{
			WriteLEShort(value);
			WriteLEShort(value >> 16);
		}

		/// <summary>
		/// Write a uint in little endian byte order.
		/// </summary>
		/// <param name="value">The value to write.</param>
		public void WriteLEUint(uint value)
		{
			WriteLEUshort(( ushort )(value & 0xffff));
			WriteLEUshort(( ushort )(value >> 16));
		}

		/// <summary>
		/// Write a long in little endian byte order.
		/// </summary>
		/// <param name="value">The value to write.</param>
		public void WriteLELong(long value)
		{
			WriteLEInt(( int )value);
			WriteLEInt(( int )(value >> 32));
		}

		/// <summary>
		/// Write a ulong in little endian byte order.
		/// </summary>
		/// <param name="value">The value to write.</param>
		public void WriteLEUlong(ulong value)
		{
			WriteLEUint(( uint )(value & 0xffffffff));
			WriteLEUint(( uint )(value >> 32));
		}

		#endregion

		/// <summary>
		/// Write a data descriptor.
		/// </summary>
		/// <param name="entry">The entry to write a descriptor for.</param>
		/// <returns>Returns the number of descriptor bytes written.</returns>
		public int WriteDataDescriptor(ZipEntry entry)
		{
			if (entry == null) {
				throw new ArgumentNullException("entry");
			}

			int result=0;

			// Add data descriptor if flagged as required
			if ((entry.Flags & (int)GeneralBitFlags.Descriptor) != 0)
			{
				// The signature is not PKZIP originally but is now described as optional
				// in the PKZIP Appnote documenting trhe format.
				WriteLEInt(ZipConstants.DataDescriptorSignature);
				WriteLEInt(unchecked((int)(entry.Crc)));

				result+=8;

				if (entry.LocalHeaderRequiresZip64)
				{
					WriteLELong(entry.CompressedSize);
					WriteLELong(entry.Size);
					result+=16;
				}
				else
				{
					WriteLEInt((int)entry.CompressedSize);
					WriteLEInt((int)entry.Size);
					result+=8;
				}
			}

			return result;
		}

		/// <summary>
		/// Read data descriptor at the end of compressed data.
		/// </summary>
		/// <param name="zip64">if set to <c>true</c> [zip64].</param>
		/// <param name="data">The data to fill in.</param>
		/// <returns>Returns the number of bytes read in the descriptor.</returns>
		public void ReadDataDescriptor(bool zip64, DescriptorData data)
		{
			int intValue = ReadLEInt();

			// In theory this may not be a descriptor according to PKZIP appnote.
			// In practise its always there.
			if (intValue != ZipConstants.DataDescriptorSignature) {
				throw new ZipException("Data descriptor signature not found");
			}

			data.Crc = ReadLEInt();

			if (zip64) {
				data.CompressedSize = ReadLELong();
				data.Size = ReadLELong();
			}
			else {
				data.CompressedSize = ReadLEInt();
				data.Size = ReadLEInt();
			}
		}

		#region Instance Fields
		bool isOwner_;
		Stream stream_;
		#endregion
	}
}
