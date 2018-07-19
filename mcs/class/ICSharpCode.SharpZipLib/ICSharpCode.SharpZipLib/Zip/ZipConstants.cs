// ZipConstants.cs
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
using System.Text;

namespace ICSharpCode.SharpZipLib.Zip 
{
	
	/// <summary>
	/// The kind of compression used for an entry in an archive
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public enum CompressionMethod
	{
		/// <summary>
		/// A direct copy of the file contents is held in the archive
		/// </summary>
		Stored     = 0,
		
		/// <summary>
		/// Common Zip compression method using a sliding dictionary 
		/// of up to 32KB and secondary compression from Huffman/Shannon-Fano trees
		/// </summary>
		Deflated   = 8,
		
		/// <summary>
		/// An extension to deflate with a 64KB window. Not supported by #Zip
		/// </summary>
		Deflate64  = 9,
		
		/// <summary>
		/// Not supported by #Zip
		/// </summary>
		BZip2      = 11,
		
		/// <summary>
		/// WinZip special for AES encryption, Not supported by #Zip
		/// </summary>
		WinZipAES  = 99,
		
	}
	
	/// <summary>
	/// Defines the contents of the general bit flags field for an archive entry.
	/// </summary>
	[Flags]
	enum GeneralBitFlags : int
	{
		/// <summary>
		/// If set indicates that the file is encrypted
		/// </summary>
		Encrypted         = 0x0001,
		/// <summary>
		/// Two bits defining the compression method (only for Method 6 Imploding and 8,9 Deflating)
		/// </summary>
		Method            = 0x0006,
		/// <summary>
		/// If set a trailing data desciptor is appended to the entry data
		/// </summary>
		Descriptor        = 0x0008,
		Reserved          = 0x0010,
		/// <summary>
		/// If set indicates the file contains Pkzip compressed patched data.
		/// </summary>
		Patched           = 0x0020,
		/// <summary>
		/// If set strong encryption has been used for this entry.
		/// </summary>
		StrongEncryption  = 0x0040,
		/// <summary>
		/// Reserved by PKWare for enhanced compression.
		/// </summary>
		EnhancedCompress  = 0x1000,
		/// <summary>
		/// If set indicates that values in the local header are masked to hide
		/// their actual values.
		/// </summary>
		/// <remarks>
		/// Used when encrypting ht ecentral directory contents.
		/// </remarks>
		HeaderMasked      = 0x2000
	}
	
	/// <summary>
	/// This class contains constants used for Zip format files
	/// </summary>
	[System.ObsoleteAttribute("This assembly has been deprecated. Please use https://www.nuget.org/packages/SharpZipLib/ instead.")]
	public sealed class ZipConstants
	{
		/// <summary>
		/// The version made by field for entries in the central header when created by this library
		/// </summary>
		/// <remarks>
		/// This is also the Zip version for the library when comparing against the version required to extract
		/// for an entry.  See <see cref="ZipInputStream.CanDecompressEntry">ZipInputStream.CanDecompressEntry</see>.
		/// </remarks>
		public const int VERSION_MADE_BY = 20;
		
		/// <summary>
		/// The minimum version required to support strong encryption
		/// </summary>
		public const int VERSION_STRONG_ENCRYPTION = 50;
		
		// The local entry header
		
		/// <summary>
		/// Size of local entry header (excluding variable length fields at end)
		/// </summary>
		public const int LOCHDR = 30;
		
		/// <summary>
		/// Signature for local entry header
		/// </summary>
		public const int LOCSIG = 'P' | ('K' << 8) | (3 << 16) | (4 << 24);

		/// <summary>
		/// Offset of version to extract in local entry header
		/// </summary>		
		public const int LOCVER =  4;
		
		/// <summary>
		/// Offset of general purpose flags in local entry header
		/// </summary>
		public const int LOCFLG =  6;
		
		/// <summary>
		/// Offset of compression method in local entry header
		/// </summary>
		public const int LOCHOW =  8;
		
		/// <summary>
		/// Offset of last mod file time + date in local entry header
		/// </summary>
		public const int LOCTIM = 10;
		
		/// <summary>
		/// Offset of crc-32 in local entry header
		/// </summary>
		public const int LOCCRC = 14;
		
		/// <summary>
		/// Offset of compressed size in local entry header
		/// </summary>
		public const int LOCSIZ = 18;
		
		/// <summary>
		/// Offset of uncompressed size in local entry header
		/// </summary>
		public const int LOCLEN = 22;
		
		/// <summary>
		/// Offset of file name length in local entry header
		/// </summary>
		public const int LOCNAM = 26;
		
		/// <summary>
		/// Offset of extra field length in local entry header
		/// </summary>
		public const int LOCEXT = 28;

		
		/// <summary>
		/// Signature for spanning entry
		/// </summary>
		public const int SPANNINGSIG = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);
		
		/// <summary>
		/// Signature for temporary spanning entry
		/// </summary>
		public const int SPANTEMPSIG = 'P' | ('K' << 8) | ('0' << 16) | ('0' << 24);
		
		/// <summary>
		/// Signature for data descriptor
		/// </summary>
		/// <remarks>
		/// This is only used where the length, Crc, or compressed size isnt known when the
		/// entry is created and the output stream doesnt support seeking.
		/// The local entry cannot be 'patched' with the correct values in this case
		/// so the values are recorded after the data prefixed by this header, as well as in the central directory.
		/// </remarks>
		public const int EXTSIG = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);
		
		/// <summary>
		/// Size of data descriptor
		/// </summary>
		public const int EXTHDR = 16;
		
		/// <summary>
		/// Offset of crc-32 in data descriptor
		/// </summary>
		public const int EXTCRC =  4;
		
		/// <summary>
		/// Offset of compressed size in data descriptor
		/// </summary>
		public const int EXTSIZ =  8;
		
		/// <summary>
		/// Offset of uncompressed length in data descriptor
		/// </summary>
		public const int EXTLEN = 12;
		
		
		/// <summary>
		/// Signature for central header
		/// </summary>
		public const int CENSIG = 'P' | ('K' << 8) | (1 << 16) | (2 << 24);
		
		/// <summary>
		/// Size of central header entry
		/// </summary>
		public const int CENHDR = 46;
		
		/// <summary>
		/// Offset of version made by in central file header
		/// </summary>
		public const int CENVEM =  4;
		
		/// <summary>
		/// Offset of version needed to extract in central file header
		/// </summary>
		public const int CENVER =  6;
		
		/// <summary>
		/// Offset of general purpose bit flag in central file header
		/// </summary>
		public const int CENFLG =  8;
		
		/// <summary>
		/// Offset of compression method in central file header
		/// </summary>
		public const int CENHOW = 10;
		
		/// <summary>
		/// Offset of time/date in central file header
		/// </summary>
		public const int CENTIM = 12;
		
		/// <summary>
		/// Offset of crc-32 in central file header
		/// </summary>
		public const int CENCRC = 16;
		
		/// <summary>
		/// Offset of compressed size in central file header
		/// </summary>
		public const int CENSIZ = 20;
		
		/// <summary>
		/// Offset of uncompressed size in central file header
		/// </summary>
		public const int CENLEN = 24;
		
		/// <summary>
		/// Offset of file name length in central file header
		/// </summary>
		public const int CENNAM = 28;
		
		/// <summary>
		/// Offset of extra field length in central file header
		/// </summary>
		public const int CENEXT = 30;
		
		/// <summary>
		/// Offset of file comment length in central file header
		/// </summary>
		public const int CENCOM = 32;
		
		/// <summary>
		/// Offset of disk start number in central file header
		/// </summary>
		public const int CENDSK = 34;
		
		/// <summary>
		/// Offset of internal file attributes in central file header
		/// </summary>
		public const int CENATT = 36;
		
		/// <summary>
		/// Offset of external file attributes in central file header
		/// </summary>
		public const int CENATX = 38;
		
		/// <summary>
		/// Offset of relative offset of local header in central file header
		/// </summary>
		public const int CENOFF = 42;
		
		
		/// <summary>
		/// Signature for Zip64 central file header
		/// </summary>
		public const int CENSIG64 = 'P' | ('K' << 8) | (6 << 16) | (6 << 24);
		
		
		
		/// <summary>
		/// Central header digitial signature
		/// </summary>
		public const int CENDIGITALSIG = 'P' | ('K' << 8) | (5 << 16) | (5 << 24);
		
		
		// The entries at the end of central directory
		
		/// <summary>
		/// End of central directory record signature
		/// </summary>
		public const int ENDSIG = 'P' | ('K' << 8) | (5 << 16) | (6 << 24);
		
		/// <summary>
		/// Size of end of central record (excluding variable fields)
		/// </summary>
		public const int ENDHDR = 22;
		
		// The following two fields are missing in SUN JDK
		
		/// <summary>
		/// Offset of number of this disk
		/// </summary>
		public const int ENDNRD =  4;
		
		/// <summary>
		/// Offset of number of disk with start of central directory
		/// </summary>
		public const int ENDDCD =  6;
		
		/// <summary>
		/// Offset of number of entries in the central directory of this disk
		/// </summary>
		public const int ENDSUB =  8;
		
		/// <summary>
		/// Offset of total number of entries in the central directory
		/// </summary>
		public const int ENDTOT = 10;
		
		/// <summary>
		/// Offset of size of central directory
		/// </summary>
		public const int ENDSIZ = 12;
		
		/// <summary>
		/// Offset of offset of start of central directory with respect to starting disk number
		/// </summary>
		public const int ENDOFF = 16;
		
		/// <summary>
		/// Offset of ZIP file comment length
		/// </summary>
		public const int ENDCOM = 20;
		
		/// <summary>
		/// Size of cryptographic header stored before entry data
		/// </summary>
		public const int CRYPTO_HEADER_SIZE = 12;

		
#if !COMPACT_FRAMEWORK

		static int defaultCodePage = 0;
		
		/// <summary>
		/// Default encoding used for string conversion.  0 gives the default system Ansi code page.
		/// Dont use unicode encodings if you want to be Zip compatible!
		/// Using the default code page isnt the full solution neccessarily
		/// there are many variable factors, codepage 850 is often a good choice for
		/// European users, however be careful about compatability.
		/// </summary>
		public static int DefaultCodePage {
			get {
				return defaultCodePage; 
			}
			set {
				defaultCodePage = value; 
			}
		}
#endif

		/// <summary>
		/// Convert a portion of a byte array to a string.
		/// </summary>		
		/// <param name="data">
		/// Data to convert to string
		/// </param>
		/// <param name="length">
		/// Number of bytes to convert starting from index 0
		/// </param>
		/// <returns>
		/// data[0]..data[length - 1] converted to a string
		/// </returns>
		public static string ConvertToString(byte[] data, int length)
		{
#if COMPACT_FRAMEWORK
			return Encoding.ASCII.GetString(data, 0, length);
#else
			return Encoding.GetEncoding(DefaultCodePage).GetString(data, 0, length);
#endif
		}
	
		/// <summary>
		/// Convert byte array to string
		/// </summary>
		/// <param name="data">
		/// Byte array to convert
		/// </param>
		/// <returns>
		/// <paramref name="data">data</paramref>converted to a string
		/// </returns>
		public static string ConvertToString(byte[] data)
		{
			return ConvertToString(data, data.Length);
		}

		/// <summary>
		/// Convert a string to a byte array
		/// </summary>
		/// <param name="str">
		/// String to convert to an array
		/// </param>
		/// <returns>Converted array</returns>
		public static byte[] ConvertToArray(string str)
		{
#if COMPACT_FRAMEWORK
			return Encoding.ASCII.GetBytes(str);
#else
			return Encoding.GetEncoding(DefaultCodePage).GetBytes(str);
#endif
		}
	}
}
