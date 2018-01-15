// ZipOutputStream.cs
//
// Copyright (C) 2001 Mike Krueger
// Copyright (C) 2004 John Reilly
//
// This file was translated from java, it was part of the GNU Classpath
// Copyright (C) 2001 Free Software Foundation, Inc.
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
using System.Collections;
using System.Text;

using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace ICSharpCode.SharpZipLib.Zip
{
	/// <summary>
	/// This is a DeflaterOutputStream that writes the files into a zip
	/// archive one after another.  It has a special method to start a new
	/// zip entry.  The zip entries contains information about the file name
	/// size, compressed size, CRC, etc.
	/// 
	/// It includes support for Stored and Deflated entries.
	/// This class is not thread safe.
	/// <br/>
	/// <br/>Author of the original java version : Jochen Hoenicke
	/// </summary>
	/// <example> This sample shows how to create a zip file
	/// <code>
	/// using System;
	/// using System.IO;
	/// 
	/// using ICSharpCode.SharpZipLib.Zip;
	/// 
	/// class MainClass
	/// {
	/// 	public static void Main(string[] args)
	/// 	{
	/// 		string[] filenames = Directory.GetFiles(args[0]);
	/// 		
	/// 		ZipOutputStream s = new ZipOutputStream(File.Create(args[1]));
	/// 		
	/// 		s.SetLevel(5); // 0 - store only to 9 - means best compression
	/// 		
	/// 		foreach (string file in filenames) {
	/// 			FileStream fs = File.OpenRead(file);
	/// 			
	/// 			byte[] buffer = new byte[fs.Length];
	/// 			fs.Read(buffer, 0, buffer.Length);
	/// 			
	/// 			ZipEntry entry = new ZipEntry(file);
	/// 			
	/// 			s.PutNextEntry(entry);
	/// 			
	/// 			s.Write(buffer, 0, buffer.Length);
	/// 			
	/// 		}
	/// 		
	/// 		s.Finish();
	/// 		s.Close();
	/// 	}
	/// }	
	/// </code>
	/// </example>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public class ZipOutputStream : DeflaterOutputStream
	{
		private ArrayList entries  = new ArrayList();
		private Crc32     crc      = new Crc32();
		private ZipEntry  curEntry = null;
		
		int defaultCompressionLevel = Deflater.DEFAULT_COMPRESSION;
		CompressionMethod curMethod = CompressionMethod.Deflated;

		
		private long size;
		private long offset = 0;
		
		private byte[] zipComment = new byte[0];
		
		/// <summary>
		/// Gets boolean indicating central header has been added for this archive...
		/// No further entries can be added once this has been done.
		/// </summary>
		public bool IsFinished {
			get {
				return entries == null;
			}
		}

		/// <summary>
		/// Creates a new Zip output stream, writing a zip archive.
		/// </summary>
		/// <param name="baseOutputStream">
		/// The output stream to which the archive contents are written.
		/// </param>
		public ZipOutputStream(Stream baseOutputStream) : base(baseOutputStream, new Deflater(Deflater.DEFAULT_COMPRESSION, true))
		{
		}
		
		/// <summary>
		/// Set the zip file comment.
		/// </summary>
		/// <param name="comment">
		/// The comment string
		/// </param>
		/// <exception name ="ArgumentOutOfRangeException">
		/// Encoding of comment is longer than 0xffff bytes.
		/// </exception>
		public void SetComment(string comment)
		{
			byte[] commentBytes = ZipConstants.ConvertToArray(comment);
			if (commentBytes.Length > 0xffff) {
				throw new ArgumentOutOfRangeException("comment");
			}
			zipComment = commentBytes;
		}
		
		/// <summary>
		/// Sets default compression level.  The new level will be activated
		/// immediately.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Level specified is not supported.
		/// </exception>
		/// <see cref="Deflater"/>
		public void SetLevel(int level)
		{
			defaultCompressionLevel = level;
			def.SetLevel(level);
		}
		
		/// <summary>
		/// Get the current deflate compression level
		/// </summary>
		/// <returns>The current compression level</returns>
		public int GetLevel()
		{
			return def.GetLevel();
		}
		
		/// <summary>
		/// Write an unsigned short in little endian byte order.
		/// </summary>
		private  void WriteLeShort(int value)
		{
			baseOutputStream.WriteByte((byte)(value & 0xff));
			baseOutputStream.WriteByte((byte)((value >> 8) & 0xff));
		}
		
		/// <summary>
		/// Write an int in little endian byte order.
		/// </summary>
		private void WriteLeInt(int value)
		{
			WriteLeShort(value);
			WriteLeShort(value >> 16);
		}
		
		/// <summary>
		/// Write an int in little endian byte order.
		/// </summary>
		private void WriteLeLong(long value)
		{
			WriteLeInt((int)value);
			WriteLeInt((int)(value >> 32));
		}
		
		
		bool patchEntryHeader = false;
		
		long headerPatchPos   = -1;

		/// <summary>
		/// Starts a new Zip entry. It automatically closes the previous
		/// entry if present.
		/// All entry elements bar name are optional, but must be correct if present.
		/// If the compression method is stored and the output is not patchable
		/// the compression for that entry is automatically changed to deflate level 0
		/// </summary>
		/// <param name="entry">
		/// the entry.
		/// </param>
		/// <exception cref="System.IO.IOException">
		/// if an I/O error occured.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// if stream was finished
		/// </exception>
		/// <exception cref="ZipException">
		/// Too many entries in the Zip file<br/>
		/// Entry name is too long<br/>
		/// Finish has already been called<br/>
		/// </exception>
		public void PutNextEntry(ZipEntry entry)
		{
			if (entries == null) {
				throw new InvalidOperationException("ZipOutputStream was finished");
			}
			
			if (curEntry != null) {
				CloseEntry();
			}

			if (entries.Count >= 0xffff) {
				throw new ZipException("Too many entries for Zip file");
			}
			
			CompressionMethod method = entry.CompressionMethod;
			int compressionLevel = defaultCompressionLevel;
			
			entry.Flags = 0;
			patchEntryHeader = false;
			bool headerInfoAvailable = true;
			
			if (method == CompressionMethod.Stored) {
				if (entry.CompressedSize >= 0) {
					if (entry.Size < 0) {
						entry.Size = entry.CompressedSize;
					} else if (entry.Size != entry.CompressedSize) {
						throw new ZipException("Method STORED, but compressed size != size");
					}
				} else {
					if (entry.Size >= 0) {
						entry.CompressedSize = entry.Size;
					}
				}
					
				if (entry.Size < 0 || entry.Crc < 0) {
					if (CanPatchEntries == true) {
						headerInfoAvailable = false;
					}
					else {
                  // Cant patch entries so storing is not possible.
						method = CompressionMethod.Deflated;
						compressionLevel = 0;
					}
				}
			}
				
			if (method == CompressionMethod.Deflated) {
				if (entry.Size == 0) {
               // No need to compress - no data.
					entry.CompressedSize = entry.Size;
					entry.Crc = 0;
					method = CompressionMethod.Stored;
				} else if (entry.CompressedSize < 0 || entry.Size < 0 || entry.Crc < 0) {
					headerInfoAvailable = false;
				}
			}
			
			if (headerInfoAvailable == false) {
				if (CanPatchEntries == false) {
					entry.Flags |= 8;
				} else {
					patchEntryHeader = true;
				}
			}
			
			if (Password != null) {
				entry.IsCrypted = true;
				if (entry.Crc < 0) {
               // Need to append data descriptor as crc is used for encryption and its not known.
					entry.Flags |= 8;
				}
			}
			entry.Offset = (int)offset;
			entry.CompressionMethod = (CompressionMethod)method;
			
			curMethod    = method;
			
			// Write the local file header
			WriteLeInt(ZipConstants.LOCSIG);
			
			WriteLeShort(entry.Version);
			WriteLeShort(entry.Flags);
			WriteLeShort((byte)method);
			WriteLeInt((int)entry.DosTime);
			if (headerInfoAvailable == true) {
				WriteLeInt((int)entry.Crc);
				WriteLeInt(entry.IsCrypted ? (int)entry.CompressedSize + ZipConstants.CRYPTO_HEADER_SIZE : (int)entry.CompressedSize);
				WriteLeInt((int)entry.Size);
			} else {
				if (patchEntryHeader == true) {
					headerPatchPos = baseOutputStream.Position;
				}
				WriteLeInt(0);	// Crc
				WriteLeInt(0);	// Compressed size
				WriteLeInt(0);	// Uncompressed size
			}
			
			byte[] name = ZipConstants.ConvertToArray(entry.Name);
			
			if (name.Length > 0xFFFF) {
				throw new ZipException("Entry name too long.");
			}

			byte[] extra = entry.ExtraData;
			if (extra == null) {
				extra = new byte[0];
			}

			if (extra.Length > 0xFFFF) {
				throw new ZipException("Extra data too long.");
			}
			
			WriteLeShort(name.Length);
			WriteLeShort(extra.Length);
			baseOutputStream.Write(name, 0, name.Length);
			baseOutputStream.Write(extra, 0, extra.Length);
			
			offset += ZipConstants.LOCHDR + name.Length + extra.Length;
			
			// Activate the entry.
			curEntry = entry;
			crc.Reset();
			if (method == CompressionMethod.Deflated) {
				def.Reset();
				def.SetLevel(compressionLevel);
			}
			size = 0;
			
			if (entry.IsCrypted == true) {
				if (entry.Crc < 0) {			// so testing Zip will says its ok
					WriteEncryptionHeader(entry.DosTime << 16);
				} else {
					WriteEncryptionHeader(entry.Crc);
				}
			}
		}
		
		/// <summary>
		/// Closes the current entry, updating header and footer information as required
		/// </summary>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// No entry is active.
		/// </exception>
		public void CloseEntry()
		{
			if (curEntry == null) {
				throw new InvalidOperationException("No open entry");
			}
			
			// First finish the deflater, if appropriate
			if (curMethod == CompressionMethod.Deflated) {
				base.Finish();
			}
			
			long csize = curMethod == CompressionMethod.Deflated ? def.TotalOut : size;
			
			if (curEntry.Size < 0) {
				curEntry.Size = size;
			} else if (curEntry.Size != size) {
				throw new ZipException("size was " + size + ", but I expected " + curEntry.Size);
			}
			
			if (curEntry.CompressedSize < 0) {
				curEntry.CompressedSize = csize;
			} else if (curEntry.CompressedSize != csize) {
				throw new ZipException("compressed size was " + csize + ", but I expected " + curEntry.CompressedSize);
			}
			
			if (curEntry.Crc < 0) {
				curEntry.Crc = crc.Value;
			} else if (curEntry.Crc != crc.Value) {
				throw new ZipException("crc was " + crc.Value +	", but I expected " + curEntry.Crc);
			}
			
			offset += csize;

			if (offset > 0xffffffff) {
				throw new ZipException("Maximum Zip file size exceeded");
			}
				
			if (curEntry.IsCrypted == true) {
				curEntry.CompressedSize += ZipConstants.CRYPTO_HEADER_SIZE;
			}
				
			// Patch the header if possible
			if (patchEntryHeader == true) {
				long curPos = baseOutputStream.Position;
				baseOutputStream.Seek(headerPatchPos, SeekOrigin.Begin);
				WriteLeInt((int)curEntry.Crc);
				WriteLeInt((int)curEntry.CompressedSize);
				WriteLeInt((int)curEntry.Size);
				baseOutputStream.Seek(curPos, SeekOrigin.Begin);
				patchEntryHeader = false;
			}

			// Add data descriptor if flagged as required
			if ((curEntry.Flags & 8) != 0) {
				WriteLeInt(ZipConstants.EXTSIG);
				WriteLeInt((int)curEntry.Crc);
				WriteLeInt((int)curEntry.CompressedSize);
				WriteLeInt((int)curEntry.Size);
				offset += ZipConstants.EXTHDR;
			}
			
			entries.Add(curEntry);
			curEntry = null;
		}
		
		void WriteEncryptionHeader(long crcValue)
		{
			offset += ZipConstants.CRYPTO_HEADER_SIZE;
			
			InitializePassword(Password);
			
			byte[] cryptBuffer = new byte[ZipConstants.CRYPTO_HEADER_SIZE];
			Random rnd = new Random();
			rnd.NextBytes(cryptBuffer);
			cryptBuffer[11] = (byte)(crcValue >> 24);
			
			EncryptBlock(cryptBuffer, 0, cryptBuffer.Length);
			baseOutputStream.Write(cryptBuffer, 0, cryptBuffer.Length);
		}
		
		/// <summary>
		/// Writes the given buffer to the current entry.
		/// </summary>
		/// <exception cref="ZipException">
		/// Archive size is invalid
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// No entry is active.
		/// </exception>
		public override void Write(byte[] b, int off, int len)
		{
			if (curEntry == null) {
				throw new InvalidOperationException("No open entry.");
			}
			
			if (len <= 0)
				return;
			
			crc.Update(b, off, len);
			size += len;
			
			if (size > 0xffffffff || size < 0) {
				throw new ZipException("Maximum entry size exceeded");
			}
				

			switch (curMethod) {
				case CompressionMethod.Deflated:
					base.Write(b, off, len);
					break;
				
				case CompressionMethod.Stored:
					if (Password != null) {
						byte[] buf = new byte[len];
						Array.Copy(b, off, buf, 0, len);
						EncryptBlock(buf, 0, len);
						baseOutputStream.Write(buf, off, len);
					} else {
						baseOutputStream.Write(b, off, len);
					}
					break;
			}
		}
		
		/// <summary>
		/// Finishes the stream.  This will write the central directory at the
		/// end of the zip file and flush the stream.
		/// </summary>
		/// <remarks>
		/// This is automatically called when the stream is closed.
		/// </remarks>
		/// <exception cref="System.IO.IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="ZipException">
		/// Comment exceeds the maximum length<br/>
		/// Entry name exceeds the maximum length
		/// </exception>
		public override void Finish()
		{
			if (entries == null)  {
				return;
			}
			
			if (curEntry != null) {
				CloseEntry();
			}
			
			int numEntries = 0;
			int sizeEntries = 0;
			
			foreach (ZipEntry entry in entries) {
				CompressionMethod method = entry.CompressionMethod;
				WriteLeInt(ZipConstants.CENSIG); 
				WriteLeShort(ZipConstants.VERSION_MADE_BY);
				WriteLeShort(entry.Version);
				WriteLeShort(entry.Flags);
				WriteLeShort((short)method);
				WriteLeInt((int)entry.DosTime);
				WriteLeInt((int)entry.Crc);
				WriteLeInt((int)entry.CompressedSize);
				WriteLeInt((int)entry.Size);
				
				byte[] name = ZipConstants.ConvertToArray(entry.Name);
				
				if (name.Length > 0xffff) {
					throw new ZipException("Name too long.");
				}
				
				byte[] extra = entry.ExtraData;
				if (extra == null) {
					extra = new byte[0];
				}
				
				byte[] entryComment = entry.Comment != null ? ZipConstants.ConvertToArray(entry.Comment) : new byte[0];
				if (entryComment.Length > 0xffff) {
					throw new ZipException("Comment too long.");
				}
				
				WriteLeShort(name.Length);
				WriteLeShort(extra.Length);
				WriteLeShort(entryComment.Length);
				WriteLeShort(0);	// disk number
				WriteLeShort(0);	// internal file attr
									// external file attribute

				if (entry.ExternalFileAttributes != -1) {
					WriteLeInt(entry.ExternalFileAttributes);
				} else {
					if (entry.IsDirectory) {                         // mark entry as directory (from nikolam.AT.perfectinfo.com)
						WriteLeInt(16);
					} else {
						WriteLeInt(0);
					}
				}

				WriteLeInt(entry.Offset);
				
				baseOutputStream.Write(name,    0, name.Length);
				baseOutputStream.Write(extra,   0, extra.Length);
				baseOutputStream.Write(entryComment, 0, entryComment.Length);
				++numEntries;
				sizeEntries += ZipConstants.CENHDR + name.Length + extra.Length + entryComment.Length;
			}
			
			WriteLeInt(ZipConstants.ENDSIG);
			WriteLeShort(0);                    // number of this disk
			WriteLeShort(0);                    // no of disk with start of central dir
			WriteLeShort(numEntries);           // entries in central dir for this disk
			WriteLeShort(numEntries);           // total entries in central directory
			WriteLeInt(sizeEntries);            // size of the central directory
			WriteLeInt((int)offset);            // offset of start of central dir
			WriteLeShort(zipComment.Length);
			baseOutputStream.Write(zipComment, 0, zipComment.Length);
			baseOutputStream.Flush();
			entries = null;
		}
	}
}
