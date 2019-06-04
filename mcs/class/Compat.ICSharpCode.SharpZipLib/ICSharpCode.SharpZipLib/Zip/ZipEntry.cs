// ZipEntry.cs
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

namespace ICSharpCode.SharpZipLib.Zip 
{
	
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public enum CompressionMethod
	{
		Stored   = 0,
		Deflated = 8,
	}
	
	/// <summary>
	/// This class represents a member of a zip archive.  ZipFile and
	/// ZipInputStream will give you instances of this class as information
	/// about the members in an archive.  On the other hand ZipOutputStream
	/// needs an instance of this class to create a new member.
	///
	/// author of the original java version : Jochen Hoenicke
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public class ZipEntry : ICloneable
	{
		static int KNOWN_SIZE   = 1;
		static int KNOWN_CSIZE  = 2;
		static int KNOWN_CRC    = 4;
		static int KNOWN_TIME   = 8;
		
		string name;
		uint   size;
		ushort version;
		uint   compressedSize;
		uint   crc;
		uint   dosTime;
		
		ushort known = 0;
		CompressionMethod  method = CompressionMethod.Deflated;
		byte[] extra = null;
		string comment = null;
		bool   isCrypted;
		
		int zipFileIndex = -1;  /* used by ZipFile */
		int flags;              /* used by ZipOutputStream */
		int offset;             /* used by ZipFile and ZipOutputStream */
		
		public bool IsEncrypted {
			get {
				return (flags & 1) != 0; 
			}
			set {
				if (value) {
					flags |= 1;
				} else {
					flags &= ~1;
				}
			}
		}
		
		public int ZipFileIndex {
			get {
				return zipFileIndex;
			}
			set {
				zipFileIndex = value;
			}
		}
		
		public int Offset {
			get {
				return offset;
			}
			set {
				offset = value;
			}
		}
		
		public int Flags {                                // Stops having two things represent same concept in class (flag isCrypted removed)
			get { 
				return flags; 
			}
			set {
				flags = value; 
			}
		}
		
		
		/// <summary>
		/// Creates a zip entry with the given name.
		/// </summary>
		/// <param name="name">
		/// the name. May include directory components separated by '/'.
		/// </param>
		public ZipEntry(string name)
		{
			if (name == null)  {
				throw new System.ArgumentNullException("name");
			}
			this.DateTime  = System.DateTime.Now;
			this.name = name;
		}
		
		/// <summary>
		/// Creates a copy of the given zip entry.
		/// </summary>
		/// <param name="e">
		/// the entry to copy.
		/// </param>
		public ZipEntry(ZipEntry e)
		{
			name           = e.name;
			known          = e.known;
			size           = e.size;
			compressedSize = e.compressedSize;
			crc            = e.crc;
			dosTime        = e.dosTime;
			method         = e.method;
			extra          = e.extra;
			comment        = e.comment;
		}
		
		public int Version {
			get {
				return version;
			}
			set {
				version = (ushort)value;
			}
		}
		
		public long DosTime {
			get {
				if ((known & KNOWN_TIME) == 0) {
					return 0;
				} else {
					return dosTime;
				}
			}
			set {
				this.dosTime = (uint)value;
				known |= (ushort)KNOWN_TIME;
			}
		}
		
		
		/// <summary>
		/// Gets/Sets the time of last modification of the entry.
		/// </summary>
		public DateTime DateTime {
			get {
				uint sec  = 2 * (dosTime & 0x1f);
				uint min  = (dosTime >> 5) & 0x3f;
				uint hrs  = (dosTime >> 11) & 0x1f;
				uint day  = (dosTime >> 16) & 0x1f;
				uint mon  = ((dosTime >> 21) & 0xf);
				uint year = ((dosTime >> 25) & 0x7f) + 1980; /* since 1900 */
				return new System.DateTime((int)year, (int)mon, (int)day, (int)hrs, (int)min, (int)sec);
			}
			set {
				DosTime = ((uint)value.Year - 1980 & 0x7f) << 25 | 
				          ((uint)value.Month) << 21 |
				          ((uint)value.Day) << 16 |
				          ((uint)value.Hour) << 11 |
				          ((uint)value.Minute) << 5 |
				          ((uint)value.Second) >> 1;
			}
		}
		
		/// <summary>
		/// Returns the entry name.  The path components in the entry are
		/// always separated by slashes ('/').
		/// </summary>
		public string Name {
			get {
				return name;
			}
		}
		
		//		/// <summary>
		//		/// Gets/Sets the time of last modification of the entry.
		//		/// </summary>
		//		/// <returns>
		//		/// the time of last modification of the entry, or -1 if unknown.
		//		/// </returns>
		//		public long Time {
		//			get {
		//				return (known & KNOWN_TIME) != 0 ? time * 1000L : -1;
		//			}
		//			set {
		//				this.time = (int) (value / 1000L);
		//				this.known |= (ushort)KNOWN_TIME;
		//			}
		//		}
		
		/// <summary>
		/// Gets/Sets the size of the uncompressed data.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// if size is not in 0..0xffffffffL
		/// </exception>
		/// <returns>
		/// the size or -1 if unknown.
		/// </returns>
		public long Size {
			get {
				return (known & KNOWN_SIZE) != 0 ? (long)size : -1L;
			}
			set  {
				if (((ulong)value & 0xFFFFFFFF00000000L) != 0) {
					throw new ArgumentOutOfRangeException("size");
				}
				this.size  = (uint)value;
				this.known |= (ushort)KNOWN_SIZE;
			}
		}
		
		/// <summary>
		/// Gets/Sets the size of the compressed data.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// if csize is not in 0..0xffffffffL
		/// </exception>
		/// <returns>
		/// the size or -1 if unknown.
		/// </returns>
		public long CompressedSize {
			get {
				return (known & KNOWN_CSIZE) != 0 ? (long)compressedSize : -1L;
			}
			set {
				if (((ulong)value & 0xffffffff00000000L) != 0) {
					throw new ArgumentOutOfRangeException();
				}
				this.compressedSize = (uint)value;
				this.known |= (ushort)KNOWN_CSIZE;
			}
		}
		
		/// <summary>
		/// Gets/Sets the crc of the uncompressed data.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// if crc is not in 0..0xffffffffL
		/// </exception>
		/// <returns>
		/// the crc or -1 if unknown.
		/// </returns>
		public long Crc {
			get {
				return (known & KNOWN_CRC) != 0 ? crc & 0xffffffffL : -1L;
			}
			set {
				if (((ulong)crc & 0xffffffff00000000L) != 0) 
				{
					throw new Exception();
				}
				this.crc = (uint)value;
				this.known |= (ushort)KNOWN_CRC;
			}
		}
		
		/// <summary>
		/// Gets/Sets the compression method. Only DEFLATED and STORED are supported.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// if method is not supported.
		/// </exception>
		/// <returns>
		/// the compression method or -1 if unknown.
		/// </returns>
		/// <see cref="ZipOutputStream.DEFLATED"/>
		/// <see cref="ZipOutputStream.STORED"/>
		public CompressionMethod CompressionMethod {
			get {
				return method;
			}
			set {
				this.method = value;
			}
		}
		
		/// <summary>
		/// Gets/Sets the extra data.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// if extra is longer than 0xffff bytes.
		/// </exception>
		/// <returns>
		/// the extra data or null if not set.
		/// </returns>
		public byte[] ExtraData {
			get {
				return extra;
			}
			set {
				if (value == null) {
					this.extra = null;
					return;
				}
				
				if (value.Length > 0xffff) {
					throw new System.ArgumentOutOfRangeException();
				}
				this.extra = value;
				try {
					int pos = 0;
					while (pos < extra.Length) {
						int sig = (extra[pos++] & 0xff) | (extra[pos++] & 0xff) << 8;
						int len = (extra[pos++] & 0xff) | (extra[pos++] & 0xff) << 8;
						if (sig == 0x5455) {
							/* extended time stamp, unix format by Rainer Prem <Rainer@Prem.de> */
							int flags = extra[pos];
							if ((flags & 1) != 0) {
								int iTime = ((extra[pos+1] & 0xff) |
									(extra[pos+2] & 0xff) << 8 |
									(extra[pos+3] & 0xff) << 16 |
									(extra[pos+4] & 0xff) << 24);
								
								DateTime = (new DateTime ( 1970, 1, 1, 0, 0, 0 ) + new TimeSpan ( 0, 0, 0, iTime, 0 )).ToLocalTime ();
								known |= (ushort)KNOWN_TIME;
							}
						}
						pos += len;
					}
				} catch (Exception) {
					/* be lenient */
					return;
				}
			}
		}
		
		/// <summary>
		/// Gets/Sets the entry comment.
		/// </summary>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// if comment is longer than 0xffff.
		/// </exception>
		/// <returns>
		/// the comment or null if not set.
		/// </returns>
		public string Comment {
			get {
				return comment;
			}
			set {
				if (value.Length > 0xffff) 
				{
					throw new ArgumentOutOfRangeException();
				}
				this.comment = value;
			}
		}
		
		/// <summary>
		/// Gets true, if the entry is a directory.  This is solely
		/// determined by the name, a trailing slash '/' marks a directory.
		/// </summary>
		public bool IsDirectory {
			get {
				int nlen = name.Length;
				return nlen > 0 && name[nlen - 1] == '/';
			}
		}
		
		/// <value>
		/// True, if the entry is encrypted.
		/// </value>
		public bool IsCrypted {
			get {
				return isCrypted;
			}
			set {
				isCrypted = value;
			}
		}
		
		/// <summary>
		/// Creates a copy of this zip entry.
		/// </summary>
		public object Clone()
		{
			return this.MemberwiseClone();
		}
		
		/// <summary>
		/// Gets the string representation of this ZipEntry.  This is just
		/// the name as returned by getName().
		/// </summary>
		public override string ToString()
		{
			return name;
		}
	}
}
