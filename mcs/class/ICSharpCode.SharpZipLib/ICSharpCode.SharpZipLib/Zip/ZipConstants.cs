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

// HISTORY
//	22-12-2009	DavidPierson	Added AES support

using System;
using System.Text;
using System.Threading;

#if NETCF_1_0 || NETCF_2_0
using System.Globalization;
#endif

namespace ICSharpCode.SharpZipLib.Zip 
{

	#region Enumerations

	/// <summary>
	/// Determines how entries are tested to see if they should use Zip64 extensions or not.
	/// </summary>
	public enum UseZip64
	{
		/// <summary>
		/// Zip64 will not be forced on entries during processing.
		/// </summary>
		/// <remarks>An entry can have this overridden if required <see cref="ZipEntry.ForceZip64"></see></remarks>
		Off,
		/// <summary>
		/// Zip64 should always be used.
		/// </summary>
		On,
		/// <summary>
		/// #ZipLib will determine use based on entry values when added to archive.
		/// </summary>
		Dynamic,
	}
	
	/// <summary>
	/// The kind of compression used for an entry in an archive
	/// </summary>
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
		/// An extension to deflate with a 64KB window. Not supported by #Zip currently
		/// </summary>
		Deflate64  = 9,
		
		/// <summary>
		/// BZip2 compression. Not supported by #Zip.
		/// </summary>
		BZip2      = 11,
		
		/// <summary>
		/// WinZip special for AES encryption, Now supported by #Zip.
		/// </summary>
		WinZipAES  = 99,
		
	}
	
	/// <summary>
	/// Identifies the encryption algorithm used for an entry
	/// </summary>
	public enum EncryptionAlgorithm
	{
		/// <summary>
		/// No encryption has been used.
		/// </summary>
		None           = 0,
		/// <summary>
		/// Encrypted using PKZIP 2.0 or 'classic' encryption.
		/// </summary>
		PkzipClassic   = 1,
		/// <summary>
		/// DES encryption has been used.
		/// </summary>
		Des            = 0x6601,
		/// <summary>
		/// RCS encryption has been used for encryption.
		/// </summary>
		RC2            = 0x6602,
		/// <summary>
		/// Triple DES encryption with 168 bit keys has been used for this entry.
		/// </summary>
		TripleDes168   = 0x6603,
		/// <summary>
		/// Triple DES with 112 bit keys has been used for this entry.
		/// </summary>
		TripleDes112   = 0x6609,
		/// <summary>
		/// AES 128 has been used for encryption.
		/// </summary>
		Aes128         = 0x660e,
		/// <summary>
		/// AES 192 has been used for encryption.
		/// </summary>
		Aes192         = 0x660f,
		/// <summary>
		/// AES 256 has been used for encryption.
		/// </summary>
		Aes256         = 0x6610,
		/// <summary>
		/// RC2 corrected has been used for encryption.
		/// </summary>
		RC2Corrected   = 0x6702,
		/// <summary>
		/// Blowfish has been used for encryption.
		/// </summary>
		Blowfish = 0x6720,
		/// <summary>
		/// Twofish has been used for encryption.
		/// </summary>
		Twofish = 0x6721,
		/// <summary>
		/// RC4 has been used for encryption.
		/// </summary>
		RC4            = 0x6801,
		/// <summary>
		/// An unknown algorithm has been used for encryption.
		/// </summary>
		Unknown        = 0xffff
	}

	/// <summary>
	/// Defines the contents of the general bit flags field for an archive entry.
	/// </summary>
	[Flags]
	public enum GeneralBitFlags : int
	{
		/// <summary>
		/// Bit 0 if set indicates that the file is encrypted
		/// </summary>
		Encrypted = 0x0001,
		/// <summary>
		/// Bits 1 and 2 - Two bits defining the compression method (only for Method 6 Imploding and 8,9 Deflating)
		/// </summary>
		Method = 0x0006,
		/// <summary>
		/// Bit 3 if set indicates a trailing data desciptor is appended to the entry data
		/// </summary>
		Descriptor = 0x0008,
		/// <summary>
		/// Bit 4 is reserved for use with method 8 for enhanced deflation
		/// </summary>
		ReservedPKware4 = 0x0010,
		/// <summary>
		/// Bit 5 if set indicates the file contains Pkzip compressed patched data.
		/// Requires version 2.7 or greater.
		/// </summary>
		Patched = 0x0020,
		/// <summary>
		/// Bit 6 if set indicates strong encryption has been used for this entry.
		/// </summary>
		StrongEncryption = 0x0040,
		/// <summary>
		/// Bit 7 is currently unused
		/// </summary>
		Unused7 = 0x0080,
		/// <summary>
		/// Bit 8 is currently unused
		/// </summary>
		Unused8 = 0x0100,
		/// <summary>
		/// Bit 9 is currently unused
		/// </summary>
		Unused9 = 0x0200,
		/// <summary>
		/// Bit 10 is currently unused
		/// </summary>
		Unused10 = 0x0400,
		/// <summary>
		/// Bit 11 if set indicates the filename and
		/// comment fields for this file must be encoded using UTF-8.
		/// </summary>
		UnicodeText = 0x0800,
		/// <summary>
		/// Bit 12 is documented as being reserved by PKware for enhanced compression.
		/// </summary>
		EnhancedCompress = 0x1000,
		/// <summary>
		/// Bit 13 if set indicates that values in the local header are masked to hide
		/// their actual values, and the central directory is encrypted.
		/// </summary>
		/// <remarks>
		/// Used when encrypting the central directory contents.
		/// </remarks>
		HeaderMasked = 0x2000,
		/// <summary>
		/// Bit 14 is documented as being reserved for use by PKware
		/// </summary>
		ReservedPkware14 = 0x4000,
		/// <summary>
		/// Bit 15 is documented as being reserved for use by PKware
		/// </summary>
		ReservedPkware15 = 0x8000
	}

	#endregion

	/// <summary>
	/// This class contains constants used for Zip format files
	/// </summary>
	public sealed class ZipConstants
	{
		#region Versions
		/// <summary>
		/// The version made by field for entries in the central header when created by this library
		/// </summary>
		/// <remarks>
		/// This is also the Zip version for the library when comparing against the version required to extract
		/// for an entry.  See <see cref="ZipEntry.CanDecompress"/>.
		/// </remarks>
		public const int VersionMadeBy = 51; // was 45 before AES
		
		/// <summary>
		/// The version made by field for entries in the central header when created by this library
		/// </summary>
		/// <remarks>
		/// This is also the Zip version for the library when comparing against the version required to extract
		/// for an entry.  See <see cref="ZipInputStream.CanDecompressEntry">ZipInputStream.CanDecompressEntry</see>.
		/// </remarks>
		[Obsolete("Use VersionMadeBy instead")]
		public const int VERSION_MADE_BY = 51;
		
		/// <summary>
		/// The minimum version required to support strong encryption
		/// </summary>
		public const int VersionStrongEncryption = 50;

		/// <summary>
		/// The minimum version required to support strong encryption
		/// </summary>
		[Obsolete("Use VersionStrongEncryption instead")]
		public const int VERSION_STRONG_ENCRYPTION = 50;

		/// <summary>
		/// Version indicating AES encryption
		/// </summary>
		public const int VERSION_AES = 51;

		/// <summary>
		/// The version required for Zip64 extensions (4.5 or higher)
		/// </summary>
		public const int VersionZip64 = 45;
		#endregion
		
		#region Header Sizes
		/// <summary>
		/// Size of local entry header (excluding variable length fields at end)
		/// </summary>
		public const int LocalHeaderBaseSize = 30;
		
		/// <summary>
		/// Size of local entry header (excluding variable length fields at end)
		/// </summary>
		[Obsolete("Use LocalHeaderBaseSize instead")]
		public const int LOCHDR = 30;
		
		/// <summary>
		/// Size of Zip64 data descriptor
		/// </summary>
		public const int Zip64DataDescriptorSize = 24;
		
		/// <summary>
		/// Size of data descriptor
		/// </summary>
		public const int DataDescriptorSize = 16;
		
		/// <summary>
		/// Size of data descriptor
		/// </summary>
		[Obsolete("Use DataDescriptorSize instead")]
		public const int EXTHDR = 16;
		
		/// <summary>
		/// Size of central header entry (excluding variable fields)
		/// </summary>
		public const int CentralHeaderBaseSize = 46;
		
		/// <summary>
		/// Size of central header entry
		/// </summary>
		[Obsolete("Use CentralHeaderBaseSize instead")]
		public const int CENHDR = 46;
		
		/// <summary>
		/// Size of end of central record (excluding variable fields)
		/// </summary>
		public const int EndOfCentralRecordBaseSize = 22;
		
		/// <summary>
		/// Size of end of central record (excluding variable fields)
		/// </summary>
		[Obsolete("Use EndOfCentralRecordBaseSize instead")]
		public const int ENDHDR = 22;
		
		/// <summary>
		/// Size of 'classic' cryptographic header stored before any entry data
		/// </summary>
		public const int CryptoHeaderSize = 12;
		
		/// <summary>
		/// Size of cryptographic header stored before entry data
		/// </summary>
		[Obsolete("Use CryptoHeaderSize instead")]
		public const int CRYPTO_HEADER_SIZE = 12;
		#endregion

		#region Header Signatures
		
		/// <summary>
		/// Signature for local entry header
		/// </summary>
		public const int LocalHeaderSignature = 'P' | ('K' << 8) | (3 << 16) | (4 << 24);

		/// <summary>
		/// Signature for local entry header
		/// </summary>
		[Obsolete("Use LocalHeaderSignature instead")]
		public const int LOCSIG = 'P' | ('K' << 8) | (3 << 16) | (4 << 24);

		/// <summary>
		/// Signature for spanning entry
		/// </summary>
		public const int SpanningSignature = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);
		
		/// <summary>
		/// Signature for spanning entry
		/// </summary>
		[Obsolete("Use SpanningSignature instead")]
		public const int SPANNINGSIG = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);
		
		/// <summary>
		/// Signature for temporary spanning entry
		/// </summary>
		public const int SpanningTempSignature = 'P' | ('K' << 8) | ('0' << 16) | ('0' << 24);
		
		/// <summary>
		/// Signature for temporary spanning entry
		/// </summary>
		[Obsolete("Use SpanningTempSignature instead")]
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
		public const int DataDescriptorSignature = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);
		
		/// <summary>
		/// Signature for data descriptor
		/// </summary>
		/// <remarks>
		/// This is only used where the length, Crc, or compressed size isnt known when the
		/// entry is created and the output stream doesnt support seeking.
		/// The local entry cannot be 'patched' with the correct values in this case
		/// so the values are recorded after the data prefixed by this header, as well as in the central directory.
		/// </remarks>
		[Obsolete("Use DataDescriptorSignature instead")]
		public const int EXTSIG = 'P' | ('K' << 8) | (7 << 16) | (8 << 24);
		
		/// <summary>
		/// Signature for central header
		/// </summary>
		[Obsolete("Use CentralHeaderSignature instead")]
		public const int CENSIG = 'P' | ('K' << 8) | (1 << 16) | (2 << 24);

		/// <summary>
		/// Signature for central header
		/// </summary>
		public const int CentralHeaderSignature = 'P' | ('K' << 8) | (1 << 16) | (2 << 24);

		/// <summary>
		/// Signature for Zip64 central file header
		/// </summary>
		public const int Zip64CentralFileHeaderSignature = 'P' | ('K' << 8) | (6 << 16) | (6 << 24);
		
		/// <summary>
		/// Signature for Zip64 central file header
		/// </summary>
		[Obsolete("Use Zip64CentralFileHeaderSignature instead")]
		public const int CENSIG64 = 'P' | ('K' << 8) | (6 << 16) | (6 << 24);
		
		/// <summary>
		/// Signature for Zip64 central directory locator
		/// </summary>
		public const int Zip64CentralDirLocatorSignature = 'P' | ('K' << 8) | (6 << 16) | (7 << 24);
		
		/// <summary>
		/// Signature for archive extra data signature (were headers are encrypted).
		/// </summary>
		public const int ArchiveExtraDataSignature = 'P' | ('K' << 8) | (6 << 16) | (7 << 24);
		
		/// <summary>
		/// Central header digitial signature
		/// </summary>
		public const int CentralHeaderDigitalSignature = 'P' | ('K' << 8) | (5 << 16) | (5 << 24);
		
		/// <summary>
		/// Central header digitial signature
		/// </summary>
		[Obsolete("Use CentralHeaderDigitalSignaure instead")]
		public const int CENDIGITALSIG = 'P' | ('K' << 8) | (5 << 16) | (5 << 24);

		/// <summary>
		/// End of central directory record signature
		/// </summary>
		public const int EndOfCentralDirectorySignature = 'P' | ('K' << 8) | (5 << 16) | (6 << 24);
		
		/// <summary>
		/// End of central directory record signature
		/// </summary>
		[Obsolete("Use EndOfCentralDirectorySignature instead")]
		public const int ENDSIG = 'P' | ('K' << 8) | (5 << 16) | (6 << 24);
		#endregion
		
#if NETCF_1_0 || NETCF_2_0
		// This isnt so great but is better than nothing.
        // Trying to work out an appropriate OEM code page would be good.
        // 850 is a good default for english speakers particularly in Europe.
		static int defaultCodePage = CultureInfo.CurrentCulture.TextInfo.ANSICodePage;
#else
		static int defaultCodePage = Thread.CurrentThread.CurrentCulture.TextInfo.OEMCodePage;
#endif
		
		/// <summary>
		/// Default encoding used for string conversion.  0 gives the default system OEM code page.
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

		/// <summary>
		/// Convert a portion of a byte array to a string.
		/// </summary>		
		/// <param name="data">
		/// Data to convert to string
		/// </param>
		/// <param name="count">
		/// Number of bytes to convert starting from index 0
		/// </param>
		/// <returns>
		/// data[0]..data[length - 1] converted to a string
		/// </returns>
		public static string ConvertToString(byte[] data, int count)
		{
			if ( data == null ) {
				return string.Empty;
			}

			return Encoding.GetEncoding(DefaultCodePage).GetString(data, 0, count);
		}
	
		/// <summary>
		/// Convert a byte array to string
		/// </summary>
		/// <param name="data">
		/// Byte array to convert
		/// </param>
		/// <returns>
		/// <paramref name="data">data</paramref>converted to a string
		/// </returns>
		public static string ConvertToString(byte[] data)
		{
			if ( data == null ) {
				return string.Empty;
			}
			return ConvertToString(data, data.Length);
		}

		/// <summary>
		/// Convert a byte array to string
		/// </summary>
		/// <param name="flags">The applicable general purpose bits flags</param>
		/// <param name="data">
		/// Byte array to convert
		/// </param>
		/// <param name="count">The number of bytes to convert.</param>
		/// <returns>
		/// <paramref name="data">data</paramref>converted to a string
		/// </returns>
		public static string ConvertToStringExt(int flags, byte[] data, int count)
		{
			if ( data == null ) {
				return string.Empty;
			}

			if ( (flags & (int)GeneralBitFlags.UnicodeText) != 0 ) {
				return Encoding.UTF8.GetString(data, 0, count);
			}
			else {
				return ConvertToString(data, count);
			}
		}

		/// <summary>
		/// Convert a byte array to string
		/// </summary>
		/// <param name="data">
		/// Byte array to convert
		/// </param>
		/// <param name="flags">The applicable general purpose bits flags</param>
		/// <returns>
		/// <paramref name="data">data</paramref>converted to a string
		/// </returns>
		public static string ConvertToStringExt(int flags, byte[] data)
		{
			if ( data == null ) {
				return string.Empty;
			}

			if ( (flags & (int)GeneralBitFlags.UnicodeText) != 0 ) {
				return Encoding.UTF8.GetString(data, 0, data.Length);
			}
			else {
				return ConvertToString(data, data.Length);
			}
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
			if ( str == null ) {
				return new byte[0];
			}

			return Encoding.GetEncoding(DefaultCodePage).GetBytes(str);
		}

		/// <summary>
		/// Convert a string to a byte array
		/// </summary>
        /// <param name="flags">The applicable <see cref="GeneralBitFlags">general purpose bits flags</see></param>
		/// <param name="str">
		/// String to convert to an array
		/// </param>
		/// <returns>Converted array</returns>
		public static byte[] ConvertToArray(int flags, string str)
		{
			if (str == null) {
				return new byte[0];
			}

			if ((flags & (int)GeneralBitFlags.UnicodeText) != 0) {
				return Encoding.UTF8.GetBytes(str);
			}
			else {
				return ConvertToArray(str);
			}
		}


		/// <summary>
		/// Initialise default instance of <see cref="ZipConstants">ZipConstants</see>
		/// </summary>
		/// <remarks>
		/// Private to prevent instances being created.
		/// </remarks>
		ZipConstants()
		{
			// Do nothing
		}
	}
}
