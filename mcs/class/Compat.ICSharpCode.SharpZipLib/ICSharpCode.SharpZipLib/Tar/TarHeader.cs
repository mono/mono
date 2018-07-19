// TarHeader.cs
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


/* The tar format and its POSIX successor PAX have a long history which makes for compatability
   issues when creating and reading files...
  
This is the ustar (Posix 1003.1) header.

struct header 
{
   char t_name[100];          //   0 Filename               
   char t_mode[8];            // 100 Permissions            
   char t_uid[8];             // 108 Numerical User ID      
   char t_gid[8];             // 116 Numerical Group ID     
   char t_size[12];           // 124 Filesize               
   char t_mtime[12];          // 136 st_mtime               
   char t_chksum[8];          // 148 Checksum               
   char t_typeflag;           // 156 Type of File            
   char t_linkname[100];      // 157 Target of Links        
   char t_magic[6];           // 257 "ustar"                
   char t_version[2];         // 263 Version fixed to 00   
   char t_uname[32];          // 265 User Name              
   char t_gname[32];          // 297 Group Name             
   char t_devmajor[8];        // 329 Major for devices      
   char t_devminor[8];        // 337 Minor for devices      
   char t_prefix[155];        // 345 Prefix for t_name      
                              // 500 End                    
   char t_mfill[12];          // 500 Filler up to 512       
};

*/

using System;
using System.Text;

namespace ICSharpCode.SharpZipLib.Tar 
{
	
	
	/// <summary>
	/// This class encapsulates the Tar Entry Header used in Tar Archives.
	/// The class also holds a number of tar constants, used mostly in headers.
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public class TarHeader : ICloneable
	{
		/// <summary>
		/// The length of the name field in a header buffer.
		/// </summary>
		public readonly static int NAMELEN = 100;
		
		/// <summary>
		/// The length of the mode field in a header buffer.
		/// </summary>
		public readonly static int MODELEN = 8;
		
		/// <summary>
		/// The length of the user id field in a header buffer.
		/// </summary>
		public readonly static int UIDLEN = 8;
		
		/// <summary>
		/// The length of the group id field in a header buffer.
		/// </summary>
		public readonly static int GIDLEN = 8;
		
		/// <summary>
		/// The length of the checksum field in a header buffer.
		/// </summary>
		public readonly static int CHKSUMLEN = 8;
		
		/// <summary>
		/// The length of the size field in a header buffer.
		/// </summary>
		public readonly static int SIZELEN = 12;
		
		/// <summary>
		/// The length of the magic field in a header buffer.
		/// </summary>
		public readonly static int MAGICLEN = 6;
		
      /// <summary>
      /// The length of the version field in a header buffer.
      /// </summary>
      public readonly static int VERSIONLEN = 2;

		/// <summary>
		/// The length of the modification time field in a header buffer.
		/// </summary>
		public readonly static int MODTIMELEN = 12;
		
		/// <summary>
		/// The length of the user name field in a header buffer.
		/// </summary>
		public readonly static int UNAMELEN = 32;
		
		/// <summary>
		/// The length of the group name field in a header buffer.
		/// </summary>
		public readonly static int GNAMELEN = 32;
		
		/// <summary>
		/// The length of the devices field in a header buffer.
		/// </summary>
		public readonly static int DEVLEN = 8;
		
		/// <summary>
		/// LF_ constants represents the "type" of an entry
		/// </summary>
		/// 

      /// <summary>
      ///  This is the "old way" of indicating a normal file.
      /// </summary>
      public const byte	LF_OLDNORM	= 0;
		
		/// <summary>
		/// Normal file type.
		/// </summary>
		public const byte	LF_NORMAL	= (byte) '0';
		
		/// <summary>
		/// Link file type.
		/// </summary>
		public const byte	LF_LINK		= (byte) '1';
		
		/// <summary>
		/// Symbolic link file type.
		/// </summary>
		public const byte	LF_SYMLINK	= (byte) '2';
		
		/// <summary>
		/// Character device file type.
		/// </summary>
		public const byte	LF_CHR		= (byte) '3';
		
		/// <summary>
		/// Block device file type.
		/// </summary>
		public const byte	LF_BLK		= (byte) '4';
		
		/// <summary>
		/// Directory file type.
		/// </summary>
		public const byte	LF_DIR		= (byte) '5';
		
		/// <summary>
		/// FIFO (pipe) file type.
		/// </summary>
		public const byte	LF_FIFO		= (byte) '6';
		
		/// <summary>
		/// Contiguous file type.
		/// </summary>
		public const byte	LF_CONTIG	= (byte) '7';
		
		/// <summary>
		/// Posix.1 2001 global extended header
		/// </summary>
		///
		public const byte   LF_GHDR    = (byte) 'g';
		
		/// <summary>
		/// Posix.1 2001 extended header
		/// </summary>
		public readonly static byte   LF_XHDR    = (byte) 'x';
		
		
		
		
		// POSIX allows for upper case ascii type as extensions
		
		// Solaris access control list
		public const byte   LF_ACL            = (byte) 'A';
		
		// This is a dir entry that contains the names of files that were in the
		// dir at the time the dump was made
		public const byte   LF_GNU_DUMPDIR    = (byte) 'D';
		
		// Solaris Extended Attribute File
		public const byte   LF_EXTATTR        = (byte) 'E' ;
		
		// Inode (metadata only) no file content
		public const byte   LF_META           = (byte) 'I';
		
		// Identifies the next file on the tape as having a long link name
		public const byte   LF_GNU_LONGLINK   = (byte) 'K';
		
		// Identifies the next file on the tape as having a long name
		public const byte   LF_GNU_LONGNAME   = (byte) 'L';
		
		// Continuation of a file that began on another volume
		public const byte   LF_GNU_MULTIVOL   = (byte) 'M';
		
		// For storing filenames that dont fit in the main header (old GNU)
		public const byte   LF_GNU_NAMES      = (byte) 'N';
		
		// Sparse file
		public const byte   LF_GNU_SPARSE     = (byte) 'S';
		
		// Tape/volume header ignore on extraction
		public const byte   LF_GNU_VOLHDR     = (byte) 'V';
		
		/// <summary>
		/// The magic tag representing a POSIX tar archive.  (includes trailing NULL)
		/// </summary>
		public readonly static string	TMAGIC		= "ustar ";
		
		/// <summary>
		/// The magic tag representing an old GNU tar archive where version is included in magic and overwrites it
		/// </summary>
		public readonly static string	GNU_TMAGIC	= "ustar  ";
		
		/// <summary>
		/// The entry's name.
		/// </summary>
		public StringBuilder name;
		
		/// <summary>
		/// The entry's permission mode.
		/// </summary>
		public int mode;
		
		/// <summary>
		/// The entry's user id.
		/// </summary>
		public int userId;
		
		/// <summary>
		/// The entry's group id.
		/// </summary>
		public int groupId;
		
		/// <summary>
		/// The entry's size.
		/// </summary>
		public long size;
		
		/// <summary>
		/// The entry's modification time.
		/// </summary>
		public DateTime modTime;
		
		/// <summary>
		/// The entry's checksum.
		/// </summary>
		public int checkSum;
		
		/// <summary>
		/// The entry's type flag.
		/// </summary>
		public byte typeFlag;
		
		/// <summary>
		/// The entry's link name.
		/// </summary>
		public StringBuilder linkName;
		
		/// <summary>
		/// The entry's magic tag.
		/// </summary>
		public StringBuilder magic;
		
		/// <summary>
		/// The entry's version.
		/// </summary>
		public StringBuilder version;
		
		/// <summary>
		/// The entry's user name.
		/// </summary>
		public StringBuilder userName;
		
		/// <summary>
		/// The entry's group name.
		/// </summary>
		public StringBuilder groupName;
		
		/// <summary>
		/// The entry's major device number.
		/// </summary>
		public int devMajor;
		
		/// <summary>
		/// The entry's minor device number.
		/// </summary>
		public int devMinor;
		
		public TarHeader()
		{
			this.magic = new StringBuilder(TarHeader.TMAGIC);
			this.version = new StringBuilder(" ");
			
			this.name     = new StringBuilder();
			this.linkName = new StringBuilder();
			
			string user = Environment.UserName;
			//         string user = "PocketPC";
			//         string user = "Everyone";
			
			if (user.Length > 31) {
				user = user.Substring(0, 31);
			}
			
			this.userId    = 1003;  // -jr- was 0
			this.groupId   = 513;   // -jr- was 0
			this.userName  = new StringBuilder(user);
// -jr-
//			this.groupName = new StringBuilder(String.Empty);
//         this.groupName = new StringBuilder("Everyone");  Attempt2
         this.groupName = new StringBuilder("None"); // Gnu compatible
         this.size      = 0;
		}
		
		/// <summary>
		/// TarHeaders can be cloned.
		/// </summary>
		public object Clone()
		{
			TarHeader hdr = new TarHeader();
			
			hdr.name      = (this.name == null) ? null : new StringBuilder(this.name.ToString());
			hdr.mode      = this.mode;
			hdr.userId    = this.userId;
			hdr.groupId   = this.groupId;
			hdr.size      = this.size;
			hdr.modTime   = this.modTime;
			hdr.checkSum  = this.checkSum;
			hdr.typeFlag  = this.typeFlag;
			hdr.linkName  = (this.linkName == null)  ? null : new StringBuilder(this.linkName.ToString());
			hdr.magic     = (this.magic == null)     ? null : new StringBuilder(this.magic.ToString());
         hdr.version   = (this.version == null)   ? null : new StringBuilder(this.version.ToString());
			hdr.userName  = (this.userName == null)  ? null : new StringBuilder(this.userName.ToString());
			hdr.groupName = (this.groupName == null) ? null : new StringBuilder(this.groupName.ToString());
			hdr.devMajor  = this.devMajor;
			hdr.devMinor  = this.devMinor;
			
			return hdr;
		}
		
		/// <summary>
		/// Get the name of this entry.
		/// </summary>
		/// <returns>
		/// The entry's name.
		/// </returns>
		public string GetName()
		{
			return this.name.ToString();
		}
		
		/// <summary>
		/// Parse an octal string from a header buffer. This is used for the
		/// file permission mode value.
		/// </summary>
		/// <param name = "header">
		/// The header buffer from which to parse.
		/// </param>
		/// <param name = "offset">
		/// The offset into the buffer from which to parse.
		/// </param>
		/// <param name = "length">
		/// The number of header bytes to parse.
		/// </param>
		/// <returns>
		/// The long value of the octal string.
		/// </returns>
		public static long ParseOctal(byte[] header, int offset, int length)
		{
			long result = 0;
			bool stillPadding = true;
			
			int end = offset + length;
			for (int i = offset; i < end ; ++i) 
			{
				if (header[i] == 0) 
				{
					break;
				}
				
				if (header[i] == (byte)' ' || header[i] == '0') 
				{
					if (stillPadding) 
					{
						continue;
					}
					
					if (header[i] == (byte)' ') 
					{
						break;
					}
				}
				
				stillPadding = false;
				
				result = (result << 3) + (header[i] - '0');
			}
			
			return result;
		}
		
		/// <summary>
		/// Parse an entry name from a header buffer.
		/// </summary>
		/// <param name="header">
		/// The header buffer from which to parse.
		/// </param>
		/// <param name="offset">
		/// The offset into the buffer from which to parse.
		/// </param>
		/// <param name="length">
		/// The number of header bytes to parse.
		/// </param>
		/// <returns>
		/// The header's entry name.
		/// </returns>
		public static StringBuilder ParseName(byte[] header, int offset, int length)
		{
			StringBuilder result = new StringBuilder(length);
			
			for (int i = offset; i < offset + length; ++i) 
			{
				if (header[i] == 0) 
				{
					break;
				}
				result.Append((char)header[i]);
			}
			
			return result;
		}
		
      public static int GetNameBytes(StringBuilder name, int nameOffset, byte[] buf, int bufferOffset, int length)
      {
         int i;
			
         for (i = 0 ; i < length && nameOffset + i < name.Length; ++i) 
         {
            buf[bufferOffset + i] = (byte)name[nameOffset + i];
         }
			
         for (; i < length ; ++i) 
         {
            buf[bufferOffset + i] = 0;
         }
			
         return bufferOffset + length;
      }

      /// <summary>
		/// Determine the number of bytes in an entry name.
		/// </summary>
		/// <param name="name">
		/// </param>
		/// <param name="buf">
		/// The header buffer from which to parse.
		/// </param>
		/// <param name="offset">
		/// The offset into the buffer from which to parse.
		/// </param>
		/// <param name="length">
		/// The number of header bytes to parse.
		/// </param>
		/// <returns>
		/// The number of bytes in a header's entry name.
		/// </returns>
		public static int GetNameBytes(StringBuilder name, byte[] buf, int offset, int length)
		{
         return GetNameBytes(name, 0, buf, offset, length);
		}
		
		/// <summary>
		/// Parse an octal integer from a header buffer.
		/// </summary>
		/// <param name = "val">
		/// </param>
		/// <param name = "buf">
		/// The header buffer from which to parse.
		/// </param>
		/// <param name = "offset">
		/// The offset into the buffer from which to parse.
		/// </param>
		/// <param name = "length">
		/// The number of header bytes to parse.
		/// </param>
		/// <returns>
		/// The integer value of the octal bytes.
		/// </returns>
		public static int GetOctalBytes(long val, byte[] buf, int offset, int length)
		{
         // TODO check for values too large...

			int idx = length - 1;

			// Either a space or null is valid here.  We use NULL as per GNUTar
			buf[offset + idx] = 0;
			--idx;

         if (val > 0)
			{
				for (long v = val; idx >= 0 && v > 0; --idx) 
				{
					buf[offset + idx] = (byte)((byte)'0' + (byte)(v & 7));
					v >>= 3;
				}
			}
				
			for (; idx >= 0; --idx) 
			{
            buf[offset + idx] = (byte)'0';
			}
			
			return offset + length;
		}
		
		/// <summary>
		/// Parse an octal long integer from a header buffer.
		/// </summary>
		/// <param name = "val">
		/// </param>
		/// <param name = "buf">
		/// The header buffer from which to parse.
		/// </param>
		/// <param name = "offset">
		/// The offset into the buffer from which to parse.
		/// </param>
		/// <param name = "length">
		/// The number of header bytes to parse.
		/// </param>
		/// <returns>
		/// The long value of the octal bytes.
		/// </returns>
		public static int GetLongOctalBytes(long val, byte[] buf, int offset, int length)
		{
         return GetOctalBytes(val, buf, offset, length);
		}
		
		/// <summary>
		/// Add the checksum octal integer to header buffer.
		/// </summary>
		/// <param name = "val">
		/// </param>
		/// <param name = "buf">
		/// The header buffer to set the checksum for
		/// </param>
		/// <param name = "offset">
		/// The offset into the buffer for the checksum
		/// </param>
		/// <param name = "length">
		/// The number of header bytes to update.
      /// It's formatted differently from the other fields: it has 6 digits, a
      /// null, then a space -- rather than digits, a space, then a null.
      /// The final space is already there, from checksumming

		/// </param>
		/// <returns>
		/// The modified buffer offset
		/// </returns>
		private static int GetCheckSumOctalBytes(long val, byte[] buf, int offset, int length)
		{
			TarHeader.GetOctalBytes(val, buf, offset, length - 1);
//			buf[offset + length - 1] = (byte)' ';  -jr- 23-Jan-2004 this causes failure!!!
//			buf[offset + length - 2] = 0;
			return offset + length;
		}
		
      /// <summary>
      /// Compute the checksum for a tar entry header.  
      /// The checksum field must be all spaces prior to this happening
      /// </summary>
      /// <param name = "buf">
      /// The tar entry's header buffer.
      /// </param>
      /// <returns>
      /// The computed checksum.
      /// </returns>
      private static long ComputeCheckSum(byte[] buf)
      {
         long sum = 0;
         for (int i = 0; i < buf.Length; ++i) 
         {
            sum += buf[i];
         }
         return sum;
      }

      readonly static long     timeConversionFactor = 10000000L;                                    // -jr- 1 tick == 100 nanoseconds
      readonly static DateTime datetTime1970        = new DateTime(1970, 1, 1, 0, 0, 0, 0); 
//      readonly static DateTime datetTime1970        = new DateTime(1970, 1, 1, 0, 0, 0, 0).ToUniversalTime(); // -jr- Should be UTC?  doesnt match Gnutar if this is so though, why?
		
      static int GetCTime(System.DateTime dateTime)
      {
         return (int)((dateTime.Ticks - datetTime1970.Ticks) / timeConversionFactor);
      }
		
      static DateTime GetDateTimeFromCTime(long ticks)
      {
         return new DateTime(datetTime1970.Ticks + ticks * timeConversionFactor);
      }

      /// <summary>
      /// Parse TarHeader information from a header buffer.
      /// </summary>
      /// <param name = "header">
      /// The tar entry header buffer to get information from.
      /// </param>
      public void ParseBuffer(byte[] header)
      {
         int offset = 0;
			
         name = TarHeader.ParseName(header, offset, TarHeader.NAMELEN);
         offset += TarHeader.NAMELEN;
			
         mode = (int)TarHeader.ParseOctal(header, offset, TarHeader.MODELEN);
         offset += TarHeader.MODELEN;
			
         userId = (int)TarHeader.ParseOctal(header, offset, TarHeader.UIDLEN);
         offset += TarHeader.UIDLEN;
			
         groupId = (int)TarHeader.ParseOctal(header, offset, TarHeader.GIDLEN);
         offset += TarHeader.GIDLEN;
			
         size = TarHeader.ParseOctal(header, offset, TarHeader.SIZELEN);
         offset += TarHeader.SIZELEN;
			
         modTime = GetDateTimeFromCTime(TarHeader.ParseOctal(header, offset, TarHeader.MODTIMELEN));
         offset += TarHeader.MODTIMELEN;
			
         checkSum = (int)TarHeader.ParseOctal(header, offset, TarHeader.CHKSUMLEN);
         offset += TarHeader.CHKSUMLEN;
			
         typeFlag = header[ offset++ ];

         linkName = TarHeader.ParseName(header, offset, TarHeader.NAMELEN);
         offset += TarHeader.NAMELEN;
			
         magic = TarHeader.ParseName(header, offset, TarHeader.MAGICLEN);
         offset += TarHeader.MAGICLEN;

         version = TarHeader.ParseName(header, offset, TarHeader.VERSIONLEN);
         offset += TarHeader.VERSIONLEN;
			
         userName = TarHeader.ParseName(header, offset, TarHeader.UNAMELEN);
         offset += TarHeader.UNAMELEN;
			
         groupName = TarHeader.ParseName(header, offset, TarHeader.GNAMELEN);
         offset += TarHeader.GNAMELEN;
			
         devMajor = (int)TarHeader.ParseOctal(header, offset, TarHeader.DEVLEN);
         offset += TarHeader.DEVLEN;
			
         devMinor = (int)TarHeader.ParseOctal(header, offset, TarHeader.DEVLEN);

         // Fields past this point not currently parsed or used...
      }

      /// <summary>
      /// 'Write' header information to buffer provided
      /// </summary>
      /// <param name="outbuf">output buffer for header information</param>
      public void WriteHeader(byte[] outbuf)
      {
         int offset = 0;
			
         offset = GetNameBytes(this.name, outbuf, offset, TarHeader.NAMELEN);
         offset = GetOctalBytes(this.mode, outbuf, offset, TarHeader.MODELEN);
         offset = GetOctalBytes(this.userId, outbuf, offset, TarHeader.UIDLEN);
         offset = GetOctalBytes(this.groupId, outbuf, offset, TarHeader.GIDLEN);
			
         long size = this.size;
			
         offset = GetLongOctalBytes(size, outbuf, offset, TarHeader.SIZELEN);
         offset = GetLongOctalBytes(GetCTime(this.modTime), outbuf, offset, TarHeader.MODTIMELEN);
			
         int csOffset = offset;
         for (int c = 0; c < TarHeader.CHKSUMLEN; ++c) 
         {
            outbuf[offset++] = (byte)' ';
         }
			
         outbuf[offset++] = this.typeFlag;
			
         offset = GetNameBytes(this.linkName, outbuf, offset, NAMELEN);
         offset = GetNameBytes(this.magic, outbuf, offset, MAGICLEN);
         offset = GetNameBytes(this.version, outbuf, offset, VERSIONLEN);
         offset = GetNameBytes(this.userName, outbuf, offset, UNAMELEN);
         offset = GetNameBytes(this.groupName, outbuf, offset, GNAMELEN);

         if (this.typeFlag == LF_CHR || this.typeFlag == LF_BLK)
         {
            offset = GetOctalBytes(this.devMajor, outbuf, offset, DEVLEN);
            offset = GetOctalBytes(this.devMinor, outbuf, offset, DEVLEN);
         }
			
         for ( ; offset < outbuf.Length; ) 
         {
            outbuf[offset++] = 0;
         }
			
         long checkSum = ComputeCheckSum(outbuf);
			
         GetCheckSumOctalBytes(checkSum, outbuf, csOffset, CHKSUMLEN);
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
