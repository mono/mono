// TarHeader.cs
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
using System.Text;

namespace ICSharpCode.SharpZipLib.Tar {
	
	
	/// <summary>
	/// This class encapsulates the Tar Entry Header used in Tar Archives.
	/// The class also holds a number of tar constants, used mostly in headers.
	/// </summary>
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
		public readonly static int MAGICLEN = 8;
		
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
		/// LF_ constants represent the "link flag" of an entry, or more commonly,
		/// the "entry type". This is the "old way" of indicating a normal file.
		/// </summary>
		public readonly static byte	LF_OLDNORM	= 0;
		
		/// <summary>
		/// Normal file type.
		/// </summary>
		public readonly static byte	LF_NORMAL	= (byte) '0';
		
		/// <summary>
		/// Link file type.
		/// </summary>
		public readonly static byte	LF_LINK		= (byte) '1';
		
		/// <summary>
		/// Symbolic link file type.
		/// </summary>
		public readonly static byte	LF_SYMLINK	= (byte) '2';
		
		/// <summary>
		/// Character device file type.
		/// </summary>
		public readonly static byte	LF_CHR		= (byte) '3';
		
		/// <summary>
		/// Block device file type.
		/// </summary>
		public readonly static byte	LF_BLK		= (byte) '4';
		
		/// <summary>
		/// Directory file type.
		/// </summary>
		public readonly static byte	LF_DIR		= (byte) '5';
		
		/// <summary>
		/// FIFO (pipe) file type.
		/// </summary>
		public readonly static byte	LF_FIFO		= (byte) '6';
		
		/// <summary>
		/// Contiguous file type.
		/// </summary>
		public readonly static byte	LF_CONTIG	= (byte) '7';
		
		/// <summary>
		/// The magic tag representing a POSIX tar archive.
		/// </summary>
		public readonly static string	TMAGIC		= "ustar";
		
		/// <summary>
		/// The magic tag representing a GNU tar archive.
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
		/// The entry's link flag.
		/// </summary>
		public byte linkFlag;
		
		/// <summary>
		/// The entry's link name.
		/// </summary>
		public StringBuilder linkName;
		
		/// <summary>
		/// The entry's magic tag.
		/// </summary>
		public StringBuilder magic;
		
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
			
			this.name     = new StringBuilder();
			this.linkName = new StringBuilder();
			
			string user = Environment.UserName;
			
			if (user.Length > 31) {
				user = user.Substring(0, 31);
			}
			
			this.userId    = 0;
			this.groupId   = 0;
			this.userName  = new StringBuilder(user);
			this.groupName = new StringBuilder(String.Empty);
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
			hdr.linkFlag  = this.linkFlag;
			hdr.linkName  = (this.linkName == null)  ? null : new StringBuilder(this.linkName.ToString());
			hdr.magic     = (this.magic == null)     ? null : new StringBuilder(this.magic.ToString());
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
			for (int i = offset; i < end ; ++i) {
				if (header[i] == 0) {
					break;
				}
				
				if (header[i] == (byte)' ' || header[i] == '0') {
					if (stillPadding) {
						continue;
					}
					
					if (header[i] == (byte)' ') {
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
			
			for (int i = offset; i < offset + length; ++i) {
				if (header[i] == 0) {
					break;
				}
				result.Append((char)header[i]);
			}
			
			return result;
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
			int i;
			
			for (i = 0 ; i < length && i < name.Length; ++i) {
				buf[offset + i] = (byte)name[i];
			}
			
			for (; i < length ; ++i) {
				buf[offset + i] = 0;
			}
			
			return offset + length;
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
			byte[] result = new byte[length];
			
			int idx = length - 1;
			
			buf[offset + idx] = 0;
			--idx;
			buf[offset + idx] = (byte)' ';
			--idx;
			
			if (val == 0) {
				buf[offset + idx] = (byte)'0';
				--idx;
			} else {
				for (long v = val; idx >= 0 && v > 0; --idx) {
					buf[offset + idx] = (byte)((byte)'0' + (byte)(v & 7));
					v >>= 3;
				}
			}
				
			for (; idx >= 0; --idx) {
				buf[offset + idx] = (byte)' ';
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
			byte[] temp = new byte[length + 1];
			TarHeader.GetOctalBytes(val, temp, 0, length + 1);
			Array.Copy(temp, 0, buf, offset, length);
			return offset + length;
		}
		
		/// <summary>
		/// Parse the checksum octal integer from a header buffer.
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
		/// The integer value of the entry's checksum.
		/// </returns>
		public static int GetCheckSumOctalBytes(long val, byte[] buf, int offset, int length)
		{
			TarHeader.GetOctalBytes(val, buf, offset, length);
			buf[offset + length - 1] = (byte)' ';
			buf[offset + length - 2] = 0;
			return offset + length;
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
