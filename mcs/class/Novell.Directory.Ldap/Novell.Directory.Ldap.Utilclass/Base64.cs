/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Utilclass.Base64.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;

namespace Novell.Directory.Ldap.Utilclass
{
	
	/// <summary> The Base64 utility class performs base64 encoding and decoding.
	/// 
	/// The Base64 Content-Transfer-Encoding is designed to represent
	/// arbitrary sequences of octets in a form that need not be humanly
	/// readable.  The encoding and decoding algorithms are simple, but the
	/// encoded data are consistently only about 33 percent larger than the
	/// unencoded data.  The base64 encoding algorithm is defined by
	/// RFC 2045.
	/// </summary>
	public class Base64
	{
		/// <summary>Conversion table for encoding to base64.
		/// 
		/// emap is a six-bit value to base64 (8-bit) converstion table.
		/// For example, the value of the 6-bit value 15
		/// is mapped to 0x50 which is the ASCII letter 'P', i.e. the letter P
		/// is the base64 encoded character that represents the 6-bit value 15.
		/// </summary>
		/*
		* 8-bit base64 encoded character                 base64       6-bit
		*                                                encoded      original
		*                                                character    binary value
		*/
		private static readonly char[] emap = new char[]{'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/'}; // 4-9, + /;  56-63
		
		/// <summary>conversion table for decoding from base64.
		/// 
		/// dmap is a base64 (8-bit) to six-bit value converstion table.
		/// For example the ASCII character 'P' has a value of 80.
		/// The value in the 80th position of the table is 0x0f or 15.
		/// 15 is the original 6-bit value that the letter 'P' represents.
		/// </summary>
		/*
		* 6-bit decoded value                            base64    base64
		*                                                encoded   character
		*                                                value
		*
		* Note: about half of the values in the table are only place holders
		*/
		private static readonly sbyte[] dmap = new sbyte[]{(sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x3e), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x3f), (sbyte) (0x34), (sbyte) (0x35), (sbyte) (0x36), (sbyte) (0x37), (sbyte) (0x38), (sbyte) (0x39), (sbyte) (0x3a), (sbyte) (0x3b), (sbyte) (0x3c), (sbyte) (0x3d), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x01), (sbyte) (0x02), (sbyte) (0x03), (sbyte) (0x04), (sbyte) (0x05), (sbyte) (0x06), (sbyte) (0x07), (sbyte) (0x08), (sbyte) (0x09), (sbyte) (0x0a), (sbyte) (0x0b), (sbyte) (0x0c), (sbyte) (0x0d), (sbyte) (0x0e), (sbyte) (0x0f), (sbyte) (0x10), (sbyte) (0x11), (sbyte) (0x12), (sbyte) (0x13), (sbyte) (0x14), (sbyte) (0x15), (sbyte) (0x16), (sbyte) (0x17), (sbyte) (0x18), (sbyte) (0x19), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x1a), (sbyte) (0x1b), (sbyte) (0x1c), (sbyte) (0x1d), (sbyte) (0x1e), (sbyte) (0x1f), (sbyte) (0x20), (sbyte) (0x21), (sbyte) (0x22), (sbyte) (0x23), (sbyte) (0x24), (sbyte) (0x25), (sbyte) (0x26), (sbyte) (0x27), (sbyte) (0x28), (sbyte) (0x29), (sbyte) (0x2a), (sbyte) (0x2b), (sbyte) (0x2c), (sbyte) (0x2d), (sbyte) (0x2e), (sbyte) (0x2f), (sbyte) (0x30), (sbyte) (0x31), (sbyte) (0x32), (sbyte) (0x33), (sbyte) (0x00), (
			sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00), (sbyte) (0x00)}; // 120-127 'xyz     '
		
		/// <summary> Default constructor, don't allow instances of the
		/// utility class to be created.
		/// </summary>
		private Base64()
		{
			return ;
		}
		
		
		/// <summary> Encodes the specified String into a base64 encoded String object.
		/// 
		/// </summary>
		/// <param name="inputString"> The String object to be encoded.
		/// 
		/// </param>
		/// <returns> a String containing the encoded value of the input.
		/// </returns>
		public static System.String encode(System.String inputString)
		{
			try
			{
				System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
				byte[] ibytes = encoder.GetBytes(inputString);
				sbyte[] sbytes=SupportClass.ToSByteArray(ibytes);
				return encode(sbytes);
			}
			catch (System.IO.IOException ue)
			{
				throw new System.SystemException("US-ASCII String encoding not supported by JVM");
			}
		}
		
		/// <summary> Encodes the specified bytes into a base64 array of bytes.
		/// Each byte in the return array represents a base64 character.
		/// 
		/// </summary>
		/// <param name="inputBytes"> the byte array to be encoded.
		/// 
		/// </param>
		/// <returns>            a String containing the base64 encoded data
		/// </returns>
		[CLSCompliantAttribute(false)]
		public static System.String encode(sbyte[] inputBytes)
		{
			int i, j, k;
			int t, t1, t2;
			int ntb; // number of three-bytes in inputBytes
			bool onePadding = false, twoPaddings = false;
			char[] encodedChars; // base64 encoded chars
			int len = inputBytes.Length;
			
			if (len == 0)
			{
				// No data, return no data.
				return new System.Text.StringBuilder("").ToString();
			}
			
			// every three bytes will be encoded into four bytes
			if (len % 3 == 0)
			{
				ntb = len / 3;
			}
			// the last one or two bytes will be encoded into
			// four bytes with one or two paddings
			else
			{
				ntb = len / 3 + 1;
			}
			
			// need two paddings
			if ((len % 3) == 1)
			{
				twoPaddings = true;
			}
			// need one padding
			else if ((len % 3) == 2)
			{
				onePadding = true;
			}
			
			encodedChars = new char[ntb * 4];
			
			// map of decoded and encoded bits
			//     bits in 3 decoded bytes:   765432  107654  321076  543210
			//     bits in 4 encoded bytes: 76543210765432107654321076543210
			//       plain           "AAA":   010000  010100  000101  000001
			//       base64 encoded "QUFB": 00010000000101000000010100000001
			// one padding:
			//     bits in 2 decoded bytes:   765432  10 7654  3210
			//     bits in 4 encoded bytes: 765432107654 321076543210 '='
			//       plain            "AA":   010000  010100  0001
			//       base64 encoded "QUE=": 00010000000101000000010000111101
			// two paddings:
			//     bits in 1 decoded bytes:   765432  10
			//     bits in 4 encoded bytes: 7654321076543210 '=' '='
			//       plain             "A":   010000  01
			//       base64 encoded "QQ==": 00010000000100000011110100111101
			//
			// note: the encoded bits which have no corresponding decoded bits
			// are filled with zeros; '=' = 00111101.
			for (i = 0, j = 0, k = 1; i < len; i += 3, j += 4, k++)
			{
				
				// build encodedChars[j]
				t = 0x00ff & inputBytes[i];
				encodedChars[j] = emap[t >> 2];
				
				// build encodedChars[j+1]
				if ((k == ntb) && twoPaddings)
				{
					encodedChars[j + 1] = emap[(t & 0x03) << 4];
					encodedChars[j + 2] = '=';
					encodedChars[j + 3] = '=';
					break;
				}
				else
				{
					t1 = 0x00ff & inputBytes[i + 1];
					encodedChars[j + 1] = emap[((t & 0x03) << 4) + ((t1 & 0xf0) >> 4)];
				}
				
				// build encodedChars[j+2]
				if ((k == ntb) && onePadding)
				{
					encodedChars[j + 2] = emap[(t1 & 0x0f) << 2];
					encodedChars[j + 3] = '=';
					break;
				}
				else
				{
					t2 = 0x00ff & inputBytes[i + 2];
					encodedChars[j + 2] = (emap[(t1 & 0x0f) << 2 | (t2 & 0xc0) >> 6]);
				}
				
				// build encodedChars[j+3]
				encodedChars[j + 3] = (emap[(t2 & 0x3f)]);
			}
			return new System.String(encodedChars);
		}
		
		
		/// <summary> Decodes the input base64 encoded String.
		/// The resulting binary data is returned as an array of bytes.
		/// 
		/// </summary>
		/// <param name="encodedString">The base64 encoded String object.
		/// 
		/// </param>
		/// <returns> The decoded byte array.
		/// </returns>
		[CLSCompliantAttribute(false)]
		public static sbyte[] decode(System.String encodedString)
		{
			char[] c = new char[encodedString.Length];
			SupportClass.GetCharsFromString(encodedString, 0, encodedString.Length, ref c, 0);
			return decode(c);
		}
		
		/// <summary> Decodes the input base64 encoded array of characters.
		/// The resulting binary data is returned as an array of bytes.
		/// 
		/// </summary>
		/// <param name="encodedChars">The character array containing the base64 encoded data.
		/// 
		/// </param>
		/// <returns> A byte array object containing decoded bytes.
		/// </returns>
		[CLSCompliantAttribute(false)]
		public static sbyte[] decode(char[] encodedChars)
		{
			int i, j, k;
			int ecLen = encodedChars.Length; // length of encodedChars
			int gn = ecLen / 4; // number of four-byte groups in encodedChars
			int dByteLen; // length of decoded bytes, default is '0'
			bool onePad = false, twoPads = false;
			sbyte[] decodedBytes; // decoded bytes
			
			if (encodedChars.Length == 0)
			{
				return new sbyte[0];
			}
			// the number of encoded bytes should be multiple of 4
			if ((ecLen % 4) != 0)
			{
				throw new System.SystemException("Novell.Directory.Ldap.ldif_dsml." + "Base64Decoder: decode: mal-formatted encode value");
			}
			
			// every four-bytes in encodedString, except the last one if it in the
			// form of '**==' or '***=' ( can't be '*===' or '===='), will be
			// decoded into three bytes.
			if ((encodedChars[ecLen - 1] == (int) '=') && (encodedChars[ecLen - 2] == (int) '='))
			{
				// the last four bytes of encodedChars is in the form of '**=='
				twoPads = true;
				// the first two bytes of the last four-bytes of encodedChars will
				// be decoded into one byte.
				dByteLen = gn * 3 - 2;
				decodedBytes = new sbyte[dByteLen];
			}
			else if (encodedChars[ecLen - 1] == '=')
			{
				// the last four bytes of encodedChars is in the form of '***='
				onePad = true;
				// the first two bytes of the last four-bytes of encodedChars will
				// be decoded into two bytes.
				dByteLen = gn * 3 - 1;
				decodedBytes = new sbyte[dByteLen];
			}
			else
			{
				// the last four bytes of encodedChars is in the form of '****',
				// e.g. no pad.
				dByteLen = gn * 3;
				decodedBytes = new sbyte[dByteLen];
			}
			
			// map of encoded and decoded bits
			// no padding:
			//     bits in 4 encoded bytes: 76543210 76543210 76543210 76543210
			//     bits in 3 decoded bytes:   765432   107654   321076   543210
			//        base64  string "QUFB":00010000 00010100 000001010 0000001
			//          plain string  "AAA":   010000  010100  000101  000001
			// one padding:
			//     bits in 4 encoded bytes: 76543210 76543210 76543210 76543210
			//     bits in 2 decoded bytes:   765432   107654   3210
			//       base64  string "QUE=": 00010000 000101000 0000100 00111101
			//         plain string   "AA":   010000  010100  0001
			// two paddings:
			//     bits in 4 encoded bytes: 76543210 76543210 76543210 76543210
			//     bits in 1 decoded bytes:   765432   10
			//       base64  string "QQ==": 00010000 00010000 00111101 00111101
			//         plain string    "A":   010000  01
			for (i = 0, j = 0, k = 1; i < ecLen; i += 4, j += 3, k++)
			{
				// build decodedBytes[j].
				decodedBytes[j] = (sbyte) (dmap[encodedChars[i]] << 2 | (dmap[encodedChars[i + 1]] & 0x30) >> 4);
				
				// build decodedBytes[j+1]
				if ((k == gn) && twoPads)
				{
					break;
				}
				else
				{
					decodedBytes[j + 1] = (sbyte) ((dmap[encodedChars[i + 1]] & 0x0f) << 4 | (dmap[encodedChars[i + 2]] & 0x3c) >> 2);
				}
				
				// build decodedBytes[j+2]
				if ((k == gn) && onePad)
				{
					break;
				}
				else
				{
					decodedBytes[j + 2] = (sbyte) ((dmap[encodedChars[i + 2]] & 0x03) << 6 | dmap[encodedChars[i + 3]] & 0x3f);
				}
			}
			return decodedBytes;
		}
		
		/// <summary> Decodes a base64 encoded StringBuffer.
		/// Decodes all or part of the input base64 encoded StringBuffer, each
		/// Character value representing a base64 character. The resulting
		/// binary data is returned as an array of bytes.
		/// 
		/// </summary>
		/// <param name="encodedSBuf">The StringBuffer object that contains base64
		/// encoded data.
		/// </param>
		/// <param name="start"> The start index of the base64 encoded data.
		/// </param>
		/// <param name="end"> The end index + 1 of the base64 encoded data.
		/// 
		/// </param>
		/// <returns> The decoded byte array
		/// </returns>
		[CLSCompliantAttribute(false)]
		public static sbyte[] decode(System.Text.StringBuilder encodedSBuf, int start, int end)
		{
			int i, j, k;
			int esbLen = end - start; // length of the encoded part
			int gn = esbLen / 4; // number of four-bytes group in ebs
			int dByteLen; // length of dbs, default is '0'
			bool onePad = false, twoPads = false;
			sbyte[] decodedBytes; // decoded bytes
			
			if (encodedSBuf.Length == 0)
			{
				return new sbyte[0];
			}
			// the number of encoded bytes should be multiple of number 4
			if ((esbLen % 4) != 0)
			{
				throw new System.SystemException("Novell.Directory.Ldap.ldif_dsml." + "Base64Decoder: decode error: mal-formatted encode value");
			}
			
			// every four-bytes in ebs, except the last one if it in the form of
			// '**==' or '***=' ( can't be '*===' or '===='), will be decoded into
			// three bytes.
			if ((encodedSBuf[end - 1] == (int) '=') && (encodedSBuf[end - 2] == (int) '='))
			{
				// the last four bytes of ebs is in the form of '**=='
				twoPads = true;
				// the first two bytes of the last four-bytes of ebs will be
				// decoded into one byte.
				dByteLen = gn * 3 - 2;
				decodedBytes = new sbyte[dByteLen];
			}
			else if (encodedSBuf[end - 1] == (int) '=')
			{
				// the last four bytes of ebs is in the form of '***='
				onePad = true;
				// the first two bytes of the last four-bytes of ebs will be
				// decoded into two bytes.
				dByteLen = gn * 3 - 1;
				decodedBytes = new sbyte[dByteLen];
			}
			else
			{
				// the last four bytes of ebs is in the form of '****', eg. no pad.
				dByteLen = gn * 3;
				decodedBytes = new sbyte[dByteLen];
			}
			
			// map of encoded and decoded bits
			// no padding:
			//     bits in 4 encoded bytes: 76543210 76543210 76543210 76543210
			//     bits in 3 decoded bytes:   765432   107654   321076   543210
			//        base64  string "QUFB":00010000 00010100 000001010 0000001
			//          plain string  "AAA":   010000  010100  000101  000001
			// one padding:
			//     bits in 4 encoded bytes: 76543210 76543210 76543210 76543210
			//     bits in 2 decoded bytes:   765432   107654   3210
			//       base64  string "QUE=": 00010000 000101000 0000100 00111101
			//         plain string   "AA":   010000  010100  0001
			// two paddings:
			//     bits in 4 encoded bytes: 76543210 76543210 76543210 76543210
			//     bits in 1 decoded bytes:   765432   10
			//       base64  string "QQ==": 00010000 00010000 00111101 00111101
			//         plain string    "A":   010000  01
			for (i = 0, j = 0, k = 1; i < esbLen; i += 4, j += 3, k++)
			{
				// build decodedBytes[j].
				decodedBytes[j] = (sbyte) (dmap[encodedSBuf[start + i]] << 2 | (dmap[encodedSBuf[start + i + 1]] & 0x30) >> 4);
				
				// build decodedBytes[j+1]
				if ((k == gn) && twoPads)
				{
					break;
				}
				else
				{
					decodedBytes[j + 1] = (sbyte) ((dmap[encodedSBuf[start + i + 1]] & 0x0f) << 4 | (dmap[encodedSBuf[start + i + 2]] & 0x3c) >> 2);
				}
				
				// build decodedBytes[j+2]
				if ((k == gn) && onePad)
				{
					break;
				}
				else
				{
					decodedBytes[j + 2] = (sbyte) ((dmap[encodedSBuf[start + i + 2]] & 0x03) << 6 | dmap[encodedSBuf[start + i + 3]] & 0x3f);
				}
			}
			return decodedBytes;
		}
		
		/// <summary> Checks if the input byte array contains only safe values, that is,
		/// the data does not need to be encoded for use with LDIF.
		/// The rules for checking safety are based on the rules for LDIF
		/// (Ldap Data Interchange Format) per RFC 2849.  The data does
		/// not need to be encoded if all the following are true:
		/// 
		/// The data cannot start with the following byte values:
		/// <pre>
		/// 00 (NUL)
		/// 10 (LF)
		/// 13 (CR)
		/// 32 (SPACE)
		/// 58 (:)
		/// 60 (LESSTHAN)
		/// Any character with value greater than 127
		/// (Negative for a byte value)
		/// </pre>
		/// The data cannot contain any of the following byte values:
		/// <pre>
		/// 00 (NUL)
		/// 10 (LF)
		/// 13 (CR)
		/// Any character with value greater than 127
		/// (Negative for a byte value)
		/// </pre>
		/// The data cannot end with a space.
		/// 
		/// </summary>
		/// <param name="bytes">the bytes to be checked.
		/// 
		/// </param>
		/// <returns> true if encoding not required for LDIF
		/// </returns>
		[CLSCompliantAttribute(false)]
		public static bool isLDIFSafe(sbyte[] bytes)
		{
			int len = bytes.Length;
			if (len > 0)
			{
				int testChar = bytes[0];
				// unsafe if first character is a NON-SAFE-INIT-CHAR
				if ((testChar == 0x00) || (testChar == 0x0A) || (testChar == 0x0D) || (testChar == 0x20) || (testChar == 0x3A) || (testChar == 0x3C) || (testChar < 0))
				{
					// non ascii (>127 is negative)
					return false;
				}
				// unsafe if last character is a space
				if (bytes[len - 1] == ' ')
				{
					return false;
				}
				// unsafe if contains any non safe character
				if (len > 1)
				{
					for (int i = 1; i < bytes.Length; i++)
					{
						testChar = bytes[i];
						if ((testChar == 0x00) || (testChar == 0x0A) || (testChar == 0x0D) || (testChar < 0))
						{
							// non ascii (>127 is negative)
							return false;
						}
					}
				}
			}
			return true;
		}
		/// <summary> Checks if the input String contains only safe values, that is,
		/// the data does not need to be encoded for use with LDIF.
		/// The rules for checking safety are based on the rules for LDIF
		/// (Ldap Data Interchange Format) per RFC 2849.  The data does
		/// not need to be encoded if all the following are true:
		/// 
		/// The data cannot start with the following char values:
		/// <pre>
		/// 00 (NUL)
		/// 10 (LF)
		/// 13 (CR)
		/// 32 (SPACE)
		/// 58 (:)
		/// 60 (LESSTHAN)
		/// Any character with value greater than 127
		/// </pre>
		/// The data cannot contain any of the following char values:
		/// <pre>
		/// 00 (NUL)
		/// 10 (LF)
		/// 13 (CR)
		/// Any character with value greater than 127
		/// </pre>
		/// The data cannot end with a space.
		/// 
		/// </summary>
		/// <param name="str">the String to be checked.
		/// 
		/// </param>
		/// <returns> true if encoding not required for LDIF
		/// </returns>
		public static bool isLDIFSafe(System.String str)
		{
			try
			{
				System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
				byte[] ibytes = encoder.GetBytes(str);
				sbyte[] sbytes=SupportClass.ToSByteArray(ibytes);				return (isLDIFSafe(sbytes));
			}
			catch (System.IO.IOException ue)
			{
				throw new System.SystemException("UTF-8 String encoding not supported by JVM");
			}
		}
		
		/* **************UTF-8 Validation methods and members*******************
		* The following text is taken from draft-yergeau-rfc2279bis-02 and explains
		* UTF-8 encoding:
		*
		*In UTF-8, characters are encoded using sequences of 1 to 6 octets.
		* If the range of character numbers is restricted to U+0000..U+10FFFF
		* (the UTF-16 accessible range), then only sequences of one to four
		* octets will occur.  The only octet of a "sequence" of one has the
		* higher-order bit set to 0, the remaining 7 bits being used to encode
		* the character number.  In a sequence of n octets, n>1, the initial
		* octet has the n higher-order bits set to 1, followed by a bit set to
		* 0.  The remaining bit(s) of that octet contain bits from the number
		* of the character to be encoded.  The following octet(s) all have the
		* higher-order bit set to 1 and the following bit set to 0, leaving 6
		* bits in each to contain bits from the character to be encoded.
		*
		* The table below summarizes the format of these different octet types.
		* The letter x indicates bits available for encoding bits of the
		* character number.
		*
		* <pre>
		* Char. number range  |        UTF-8 octet sequence
		*    (hexadecimal)    |              (binary)
		* --------------------+---------------------------------------------
		* 0000 0000-0000 007F | 0xxxxxxx
		* 0000 0080-0000 07FF | 110xxxxx 10xxxxxx
		* 0000 0800-0000 FFFF | 1110xxxx 10xxxxxx 10xxxxxx
		* 0001 0000-001F FFFF | 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
		* 0020 0000-03FF FFFF | 111110xx 10xxxxxx 10xxxxxx 10xxxxxx 10xxxxxx
		* 0400 0000-7FFF FFFF | 1111110x 10xxxxxx ... 10xxxxxx
		* </pre>
		*/
		
		/// <summary> Given the first byte in a sequence, getByteCount returns the number of
		/// additional bytes in a UTF-8 character sequence (not including the first
		/// byte).
		/// 
		/// </summary>
		/// <param name="b"> The first byte in a UTF-8 character sequence.
		/// 
		/// </param>
		/// <returns> the number of additional bytes in a UTF-8 character sequence.
		/// </returns>
		private static int getByteCount(sbyte b)
		{
			if (b > 0)
				return 0;
			if ((b & 0xE0) == 0xC0)
			{
				return 1; //one additional byte (2 bytes total)
			}
			if ((b & 0xF0) == 0xE0)
			{
				return 2; //two additional bytes (3 bytes total)
			}
			if ((b & 0xF8) == 0xF0)
			{
				return 3; //three additional bytes (4 bytes total)
			}
			if ((b & 0xFC) == 0xF8)
			{
				return 4; //four additional bytes (5 bytes total)
			}
			if ((b & 0xFF) == 0xFC)
			{
				return 5; //five additional bytes (6 bytes total)
			}
			return - 1;
		}
		
		/// <summary> Bit masks used to determine if a the value of UTF-8 byte sequence
		/// is less than the minimum value.
		/// 
		/// If the value of a byte sequence is less than the minimum value then
		/// the number should be encoded in fewer bytes and is invalid.  For example
		/// If the first byte indicates that a sequence has three bytes in a
		/// sequence. Then the top five bits cannot be zero.  Notice the index into
		/// the array is one less than the number of bytes in a sequence.
		/// A validity test for this could be:
		/// </summary>
		private static readonly sbyte[][] lowerBoundMask = {new sbyte[]{0, 0}, new sbyte[]{(sbyte) (0x1E), (sbyte) (0x00)}, new sbyte[]{(sbyte) (0x0F), (sbyte) (0x20)}, new sbyte[]{(sbyte) (0x07), (sbyte) (0x30)}, new sbyte[]{(sbyte) (0x02), (sbyte) (0x38)}, new sbyte[]{(sbyte) (0x01), (sbyte) (0x3C)}};
		
		/// <summary>mask to AND with a continuation byte: should equal continuationResult </summary>
		private static sbyte continuationMask = (sbyte) SupportClass.Identity(0xC0);
		
		/// <summary>expected result of ANDing a continuation byte with continuationMask </summary>
		private static sbyte continuationResult = (sbyte) SupportClass.Identity(0x80);
		
		/// <summary> Determines if an array of bytes contains only valid UTF-8 characters.
		/// 
		/// UTF-8 is the standard encoding for Ldap strings.  If a value contains
		/// data that is not valid UTF-8 then data is lost converting the
		/// value to a Java String.
		/// 
		/// 
		/// In addition, Java Strings currently use UCS2 (Unicode Code Standard
		/// 2-byte characters). UTF-8 can be encoded as USC2 and UCS4 (4-byte
		/// characters).  Some valid UTF-8 characters cannot be represented as UCS2
		/// characters. To determine if all UTF-8 sequences can be encoded into
		/// UCS2 characters (a Java String), specify the <code>isUCS2Only</code>
		/// parameter as <code>true</code>.
		/// 
		/// </summary>
		/// <param name="array"> An array of bytes that are to be tested for valid UTF-8
		/// encoding.
		/// 
		/// </param>
		/// <param name="isUCS2Only">true if the UTF-8 values must be restricted to fit
		/// within UCS2 encoding (2 bytes)
		/// </param>
		/// <returns> true if all values in the byte array are valid UTF-8
		/// sequences.  If <code>isUCS2Only</code> is
		/// <code>true</code>, the method returns false if a UTF-8
		/// sequence generates any character that cannot be
		/// represented as a UCS2 character (Java String)
		/// </returns>
		[CLSCompliantAttribute(false)]
		public static bool isValidUTF8(sbyte[] array, bool isUCS2Only)
		{
			int index = 0;
			while (index < array.Length)
			{
				int count = getByteCount(array[index]);
				if (count == 0)
				{
					//anything that qualifies as count=0 is valid UTF-8
					index++;
					continue;
				}
				
				if (count == - 1 || index + count >= array.Length || (isUCS2Only && count >= 3))
				{
					/* Any count that puts us out of bounds for the index is
					* invalid.  Valid UCS2 characters can only have 2 additional
					* bytes. (three total) */
					return false;
				}
				
				/* Tests if the first and second byte are below the minimum bound */
				if ((lowerBoundMask[count][0] & array[index]) == 0 && (lowerBoundMask[count][1] & array[index + 1]) == 0)
				{
					return false;
				}
				
				/* testing continuation on the second and following bytes */
				for (int i = 1; i <= count; i++)
				{
					if ((array[index + i] & continuationMask) != continuationResult)
					{
						return false;
					}
				}
				index += count + 1;
			}
			return true;
		}
	}
}
