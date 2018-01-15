// ZipOutputStream.cs
// Copyright (C) 2001 Mike Krueger
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
	/// This is a FilterOutputStream that writes the files into a zip
	/// archive one after another.  It has a special method to start a new
	/// zip entry.  The zip entries contains information about the file name
	/// size, compressed size, CRC, etc.
	/// 
	/// It includes support for STORED and DEFLATED entries.
	/// This class is not thread safe.
	/// 
	/// author of the original java version : Jochen Hoenicke
	/// </summary>
	/// <example> This sample shows how to create a zip file
	/// <code>
	/// using System;
	/// using System.IO;
	/// 
	/// using NZlib.Zip;
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
		
		private CompressionMethod curMethod;
		private int size;
		private int offset = 0;
		
		private byte[] zipComment = new byte[0];
		private int defaultMethod = DEFLATED;
		
		/// <summary>
		/// Our Zip version is hard coded to 1.0 resp. 2.0
		/// </summary>
		private const int ZIP_STORED_VERSION   = 10;
		private const int ZIP_DEFLATED_VERSION = 20;
		
		/// <summary>
		/// Compression method.  This method doesn't compress at all.
		/// </summary>
		public const int STORED      =  0;
		
		/// <summary>
		/// Compression method.  This method uses the Deflater.
		/// </summary>
		public const int DEFLATED    =  8;
		
		/// <summary>
		/// Creates a new Zip output stream, writing a zip archive.
		/// </summary>
		/// <param name="baseOutputStream">
		/// the output stream to which the zip archive is written.
		/// </param>
		public ZipOutputStream(Stream baseOutputStream) : base(baseOutputStream, new Deflater(Deflater.DEFAULT_COMPRESSION, true))
		{ 
		}
		
		/// <summary>
		/// Set the zip file comment.
		/// </summary>
		/// <param name="comment">
		/// the comment.
		/// </param>
		/// <exception name ="ArgumentException">
		/// if UTF8 encoding of comment is longer than 0xffff bytes.
		/// </exception>
		public void SetComment(string comment)
		{
			byte[] commentBytes = ZipConstants.ConvertToArray(comment);
			if (commentBytes.Length > 0xffff) {
				throw new ArgumentException("Comment too long.");
			}
			zipComment = commentBytes;
		}
		
		/// <summary>
		/// Sets default compression method.  If the Zip entry specifies
		/// another method its method takes precedence.
		/// </summary>
		/// <param name = "method">
		/// the method.
		/// </param>
		/// <exception name = "ArgumentException">
		/// if method is not supported.
		/// </exception>
		public void SetMethod(int method)
		{
			if (method != STORED && method != DEFLATED) {
				throw new ArgumentException("Method not supported.");
			}
			defaultMethod = method;
		}
		
		/// <summary>
		/// Sets default compression level.  The new level will be activated
		/// immediately.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// if level is not supported.
		/// </exception>
		/// <see cref="Deflater"/>
		public void SetLevel(int level)
		{
			def.SetLevel(level);
		}
		
		/// <summary>
		/// Write an unsigned short in little endian byte order.
		/// </summary>
		private  void WriteLeShort(int value)
		{
			baseOutputStream.WriteByte((byte)value);
			baseOutputStream.WriteByte((byte)(value >> 8));
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
		
		
		bool shouldWriteBack = false;
		// -jr- Having a hard coded flag for seek capability requires this so users can determine
		// if the entries will be patched or not.
		public bool CanPatchEntries {
			get { 
				return baseOutputStream.CanSeek; 
			}
		}
		
		long seekPos         = -1;
		/// <summary>
		/// Starts a new Zip entry. It automatically closes the previous
		/// entry if present.  If the compression method is stored, the entry
		/// must have a valid size and crc, otherwise all elements (except
		/// name) are optional, but must be correct if present.  If the time
		/// is not set in the entry, the current time is used.
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
		public void PutNextEntry(ZipEntry entry)
		{
			if (entries == null) {
				throw new InvalidOperationException("ZipOutputStream was finished");
			}
			
			CompressionMethod method = entry.CompressionMethod;
			int flags = 0;
			entry.IsCrypted = Password != null;
			switch (method) {
				case CompressionMethod.Stored:
					if (entry.CompressedSize >= 0) {
						if (entry.Size < 0) {
							entry.Size = entry.CompressedSize;
						} else if (entry.Size != entry.CompressedSize) {
							throw new ZipException("Method STORED, but compressed size != size");
						}
					} else {
						entry.CompressedSize = entry.Size;
					}
					
					if (entry.IsCrypted) {
						entry.CompressedSize += 12;
					}
					
					if (entry.Size < 0) {
						throw new ZipException("Method STORED, but size not set");
					} else if (entry.Crc < 0) {
						throw new ZipException("Method STORED, but crc not set");
					}
					break;
				case CompressionMethod.Deflated:
					if (entry.CompressedSize < 0 || entry.Size < 0 || entry.Crc < 0) {
						flags |= 8;
					}
					break;
			}
			
			if (curEntry != null) {
				CloseEntry();
			}
			
			//			if (entry.DosTime < 0) {
			//				entry.Time = System.Environment.TickCount;
			//			}
			if (entry.IsCrypted) {
				flags |= 1;
			}
			entry.Flags  = flags;
			entry.Offset = offset;
			entry.CompressionMethod = (CompressionMethod)method;
			
			curMethod    = method;
			// Write the local file header
			WriteLeInt(ZipConstants.LOCSIG);
			
			// write ZIP version
			WriteLeShort(method == CompressionMethod.Stored ? ZIP_STORED_VERSION : ZIP_DEFLATED_VERSION);
			if ((flags & 8) == 0) {
				WriteLeShort(flags);
				WriteLeShort((byte)method);
				WriteLeInt((int)entry.DosTime);
				WriteLeInt((int)entry.Crc);
				WriteLeInt((int)entry.CompressedSize);
				WriteLeInt((int)entry.Size);
			} else {
				if (baseOutputStream.CanSeek) {
					shouldWriteBack = true;
					WriteLeShort((short)(flags & ~8));
				} else {
					shouldWriteBack = false;
					WriteLeShort(flags);
				}
				WriteLeShort((byte)method);
				WriteLeInt((int)entry.DosTime);
				if (baseOutputStream.CanSeek) {
					seekPos = baseOutputStream.Position;
				}
				WriteLeInt(0);
				WriteLeInt(0);
				WriteLeInt(0);
			}
			byte[] name = ZipConstants.ConvertToArray(entry.Name);
			
			if (name.Length > 0xFFFF) {
				throw new ZipException("Name too long.");
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
			
			if (Password != null) {
				InitializePassword(Password);
				byte[] cryptbuffer = new byte[12];
				Random rnd = new Random();
				for (int i = 0; i < cryptbuffer.Length; ++i) {
					cryptbuffer[i] = (byte)rnd.Next();
				}
				EncryptBlock(cryptbuffer, 0, cryptbuffer.Length);
				baseOutputStream.Write(cryptbuffer, 0, cryptbuffer.Length);
			}
			offset += ZipConstants.LOCHDR + name.Length + extra.Length;
			
			/* Activate the entry. */
			curEntry = entry;
			crc.Reset();
			if (method == CompressionMethod.Deflated) {
				def.Reset();
			}
			size = 0;
		}
		
		/// <summary>
		/// Closes the current entry.
		/// </summary>
		/// <exception cref="System.IO.IOException">
		/// if an I/O error occured.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// if no entry is active.
		/// </exception>
		public void CloseEntry()
		{
			if (curEntry == null) {
				throw new InvalidOperationException("No open entry");
			}
			
			/* First finish the deflater, if appropriate */
			if (curMethod == CompressionMethod.Deflated) {
				base.Finish();
			}
			
			int csize = curMethod == CompressionMethod.Deflated ? def.TotalOut : size;
			
			if (curEntry.Size < 0) {
				curEntry.Size = size;
			} else if (curEntry.Size != size) {
				throw new ZipException("size was " + size + ", but I expected " + curEntry.Size);
			}
			
			if (curEntry.IsCrypted) {
				csize += 12;
			}
			
			if (curEntry.CompressedSize < 0) {
				curEntry.CompressedSize = csize;
			} else if (curEntry.CompressedSize != csize) {
				throw new ZipException("compressed size was " + csize + ", but I expected " + curEntry.CompressedSize);
			}
			
			if (curEntry.Crc < 0) {
				curEntry.Crc = crc.Value;
			} else if (curEntry.Crc != crc.Value) {
				throw new ZipException("crc was " + crc.Value +
					", but I expected " + 
					curEntry.Crc);
			}
			
			offset += csize;
			
			/* Now write the data descriptor entry if needed. */
			if (curMethod == CompressionMethod.Deflated && (curEntry.Flags & 8) != 0) {
				if (shouldWriteBack) {
					curEntry.Flags &= ~8;
					long curPos = baseOutputStream.Position;
					baseOutputStream.Seek(seekPos, SeekOrigin.Begin);
					WriteLeInt((int)curEntry.Crc);
					WriteLeInt((int)curEntry.CompressedSize);
					WriteLeInt((int)curEntry.Size);
					baseOutputStream.Seek(curPos, SeekOrigin.Begin);
					shouldWriteBack = false;
				} else {
					WriteLeInt(ZipConstants.EXTSIG);
					WriteLeInt((int)curEntry.Crc);
					WriteLeInt((int)curEntry.CompressedSize);
					WriteLeInt((int)curEntry.Size);
					offset += ZipConstants.EXTHDR;
				}
			}
			
			entries.Add(curEntry);
			curEntry = null;
		}
		
		/// <summary>
		/// Writes the given buffer to the current entry.
		/// </summary>
		/// <exception cref="System.IO.IOException">
		/// if an I/O error occured.
		/// </exception>
		/// <exception cref="System.InvalidOperationException">
		/// if no entry is active.
		/// </exception>
		public override void Write(byte[] b, int off, int len)
		{
			if (curEntry == null) {
				throw new InvalidOperationException("No open entry.");
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
			crc.Update(b, off, len);
			
			size += len;
		}
		
		/// <summary>
		/// Finishes the stream.  This will write the central directory at the
		/// end of the zip file and flush the stream.
		/// </summary>
		/// <exception cref="System.IO.IOException">
		/// if an I/O error occured.
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
				// TODO : check the appnote file for compilance with the central directory standard
				CompressionMethod method = entry.CompressionMethod;
				WriteLeInt(ZipConstants.CENSIG); 
				WriteLeShort(method == CompressionMethod.Stored ? ZIP_STORED_VERSION : ZIP_DEFLATED_VERSION);
				WriteLeShort(method == CompressionMethod.Stored ? ZIP_STORED_VERSION : ZIP_DEFLATED_VERSION);
				
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
				
				string strComment = entry.Comment;
				byte[] comment = strComment != null ? ZipConstants.ConvertToArray(strComment) : new byte[0];
				if (comment.Length > 0xffff) {
					throw new ZipException("Comment too long.");
				}
				
				WriteLeShort(name.Length);
				WriteLeShort(extra.Length);
				WriteLeShort(comment.Length);
				WriteLeShort(0); // disk number
				WriteLeShort(0); // internal file attr
				if (entry.IsDirectory) {                         // -jr- 17-12-2003 mark entry as directory (from nikolam.AT.perfectinfo.com)
					WriteLeInt(16);
				} else {
					WriteLeInt(0);   // external file attr
				}
				WriteLeInt(entry.Offset);
				
				baseOutputStream.Write(name,    0, name.Length);
				baseOutputStream.Write(extra,   0, extra.Length);
				baseOutputStream.Write(comment, 0, comment.Length);
				++numEntries;
				sizeEntries += ZipConstants.CENHDR + name.Length + extra.Length + comment.Length;
			}
			
			WriteLeInt(ZipConstants.ENDSIG);
			WriteLeShort(0); // disk number 
			WriteLeShort(0); // disk with start of central dir
			WriteLeShort(numEntries);
			WriteLeShort(numEntries);
			WriteLeInt(sizeEntries);
			WriteLeInt(offset);
			WriteLeShort(zipComment.Length);
			baseOutputStream.Write(zipComment, 0, zipComment.Length);
			baseOutputStream.Flush();
			entries = null;
		}
	}
}
