/*
 * CP28592.cs - Central European (ISO) code page.
 *
 * Copyright (c) 2002  Southern Storm Software, Pty Ltd
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included
 * in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
 * OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR
 * OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 */

// Generated from "ibm-912.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.West
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP28592 : ByteEncoding
{
	public CP28592()
		: base(28592, ToChars, "Central European (ISO)",
		       "iso-8859-2", "iso-8859-2", "iso-8859-2",
		       true, true, true, true, 1250)
	{}

	private static readonly char[] ToChars = {
		'\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', 
		'\u0006', '\u0007', '\u0008', '\u0009', '\u000A', '\u000B', 
		'\u000C', '\u000D', '\u000E', '\u000F', '\u0010', '\u0011', 
		'\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', 
		'\u0018', '\u0019', '\u001A', '\u001B', '\u001C', '\u001D', 
		'\u001E', '\u001F', '\u0020', '\u0021', '\u0022', '\u0023', 
		'\u0024', '\u0025', '\u0026', '\u0027', '\u0028', '\u0029', 
		'\u002A', '\u002B', '\u002C', '\u002D', '\u002E', '\u002F', 
		'\u0030', '\u0031', '\u0032', '\u0033', '\u0034', '\u0035', 
		'\u0036', '\u0037', '\u0038', '\u0039', '\u003A', '\u003B', 
		'\u003C', '\u003D', '\u003E', '\u003F', '\u0040', '\u0041', 
		'\u0042', '\u0043', '\u0044', '\u0045', '\u0046', '\u0047', 
		'\u0048', '\u0049', '\u004A', '\u004B', '\u004C', '\u004D', 
		'\u004E', '\u004F', '\u0050', '\u0051', '\u0052', '\u0053', 
		'\u0054', '\u0055', '\u0056', '\u0057', '\u0058', '\u0059', 
		'\u005A', '\u005B', '\u005C', '\u005D', '\u005E', '\u005F', 
		'\u0060', '\u0061', '\u0062', '\u0063', '\u0064', '\u0065', 
		'\u0066', '\u0067', '\u0068', '\u0069', '\u006A', '\u006B', 
		'\u006C', '\u006D', '\u006E', '\u006F', '\u0070', '\u0071', 
		'\u0072', '\u0073', '\u0074', '\u0075', '\u0076', '\u0077', 
		'\u0078', '\u0079', '\u007A', '\u007B', '\u007C', '\u007D', 
		'\u007E', '\u007F', '\u0080', '\u0081', '\u0082', '\u0083', 
		'\u0084', '\u0085', '\u0086', '\u0087', '\u0088', '\u0089', 
		'\u008A', '\u008B', '\u008C', '\u008D', '\u008E', '\u008F', 
		'\u0090', '\u0091', '\u0092', '\u0093', '\u0094', '\u0095', 
		'\u0096', '\u0097', '\u0098', '\u0099', '\u009A', '\u009B', 
		'\u009C', '\u009D', '\u009E', '\u009F', '\u00A0', '\u0104', 
		'\u02D8', '\u0141', '\u00A4', '\u013D', '\u015A', '\u00A7', 
		'\u00A8', '\u0160', '\u015E', '\u0164', '\u0179', '\u00AD', 
		'\u017D', '\u017B', '\u00B0', '\u0105', '\u02DB', '\u0142', 
		'\u00B4', '\u013E', '\u015B', '\u02C7', '\u00B8', '\u0161', 
		'\u015F', '\u0165', '\u017A', '\u02DD', '\u017E', '\u017C', 
		'\u0154', '\u00C1', '\u00C2', '\u0102', '\u00C4', '\u0139', 
		'\u0106', '\u00C7', '\u010C', '\u00C9', '\u0118', '\u00CB', 
		'\u011A', '\u00CD', '\u00CE', '\u010E', '\u0110', '\u0143', 
		'\u0147', '\u00D3', '\u00D4', '\u0150', '\u00D6', '\u00D7', 
		'\u0158', '\u016E', '\u00DA', '\u0170', '\u00DC', '\u00DD', 
		'\u0162', '\u00DF', '\u0155', '\u00E1', '\u00E2', '\u0103', 
		'\u00E4', '\u013A', '\u0107', '\u00E7', '\u010D', '\u00E9', 
		'\u0119', '\u00EB', '\u011B', '\u00ED', '\u00EE', '\u010F', 
		'\u0111', '\u0144', '\u0148', '\u00F3', '\u00F4', '\u0151', 
		'\u00F6', '\u00F7', '\u0159', '\u016F', '\u00FA', '\u0171', 
		'\u00FC', '\u00FD', '\u0163', '\u02D9', 
	};

	// Get the number of bytes needed to encode a character buffer.
	public unsafe override int GetByteCountImpl (char* chars, int count)
	{
		if (this.EncoderFallback != null)		{
			//Calculate byte count by actually doing encoding and discarding the data.
			return GetBytesImpl(chars, count, null, 0);
		}
		else
		
		{
			return count;
		}
	}
	
	// Get the number of bytes needed to encode a character buffer.
	public override int GetByteCount (String s)
	{
		if (this.EncoderFallback != null)
		{
			//Calculate byte count by actually doing encoding and discarding the data.
			unsafe
			{
				fixed (char *s_ptr = s)
				{
					return GetBytesImpl(s_ptr, s.Length, null, 0);
				}
			}
		}
		else
		{
			//byte count equals character count because no EncoderFallback set
			return s.Length;
		}
	}
	
	//ToBytes is just an alias for GetBytesImpl, but doesn't return byte count
	protected unsafe override void ToBytes(char* chars, int charCount,
	                                byte* bytes, int byteCount)
	{
		//Calling ToBytes with null destination buffer doesn't make any sense
		if (bytes == null)
			throw new ArgumentNullException("bytes");
		GetBytesImpl(chars, charCount, bytes, byteCount);
	}
	
	public unsafe override int GetBytesImpl (char* chars, int charCount,
	                                         byte* bytes, int byteCount)
	{
		int ch;
		int charIndex = 0;
		int byteIndex = 0;
#if NET_2_0
		EncoderFallbackBuffer buffer = null;
#endif
		while (charCount > 0)
		{
			ch = (int)(chars[charIndex]);
			charIndex++;
			charCount--;
			if(ch >= 161) switch(ch)
			{
				case 0x00A4:
				case 0x00A7:
				case 0x00A8:
				case 0x00AD:
				case 0x00B0:
				case 0x00B4:
				case 0x00B8:
				case 0x00C1:
				case 0x00C2:
				case 0x00C4:
				case 0x00C7:
				case 0x00C9:
				case 0x00CB:
				case 0x00CD:
				case 0x00CE:
				case 0x00D0:
				case 0x00D3:
				case 0x00D4:
				case 0x00D6:
				case 0x00D7:
				case 0x00DA:
				case 0x00DC:
				case 0x00DD:
				case 0x00DF:
				case 0x00E1:
				case 0x00E2:
				case 0x00E4:
				case 0x00E7:
				case 0x00E9:
				case 0x00EB:
				case 0x00ED:
				case 0x00EE:
				case 0x00F3:
				case 0x00F4:
				case 0x00F6:
				case 0x00F7:
				case 0x00FA:
				case 0x00FC:
				case 0x00FD:
					break;
				case 0x00A2: ch = 0x8D; break;
				case 0x00A5: ch = 0x8E; break;
				case 0x00A9: ch = 0x88; break;
				case 0x00AE: ch = 0x9F; break;
				case 0x00B6: ch = 0x14; break;
				case 0x0102: ch = 0xC3; break;
				case 0x0103: ch = 0xE3; break;
				case 0x0104: ch = 0xA1; break;
				case 0x0105: ch = 0xB1; break;
				case 0x0106: ch = 0xC6; break;
				case 0x0107: ch = 0xE6; break;
				case 0x010C: ch = 0xC8; break;
				case 0x010D: ch = 0xE8; break;
				case 0x010E: ch = 0xCF; break;
				case 0x010F: ch = 0xEF; break;
				case 0x0110: ch = 0xD0; break;
				case 0x0111: ch = 0xF0; break;
				case 0x0118: ch = 0xCA; break;
				case 0x0119: ch = 0xEA; break;
				case 0x011A: ch = 0xCC; break;
				case 0x011B: ch = 0xEC; break;
				case 0x0139: ch = 0xC5; break;
				case 0x013A: ch = 0xE5; break;
				case 0x013D: ch = 0xA5; break;
				case 0x013E: ch = 0xB5; break;
				case 0x0141: ch = 0xA3; break;
				case 0x0142: ch = 0xB3; break;
				case 0x0143: ch = 0xD1; break;
				case 0x0144: ch = 0xF1; break;
				case 0x0147: ch = 0xD2; break;
				case 0x0148: ch = 0xF2; break;
				case 0x0150: ch = 0xD5; break;
				case 0x0151: ch = 0xF5; break;
				case 0x0154: ch = 0xC0; break;
				case 0x0155: ch = 0xE0; break;
				case 0x0158: ch = 0xD8; break;
				case 0x0159: ch = 0xF8; break;
				case 0x015A: ch = 0xA6; break;
				case 0x015B: ch = 0xB6; break;
				case 0x015E: ch = 0xAA; break;
				case 0x015F: ch = 0xBA; break;
				case 0x0160: ch = 0xA9; break;
				case 0x0161: ch = 0xB9; break;
				case 0x0162: ch = 0xDE; break;
				case 0x0163: ch = 0xFE; break;
				case 0x0164: ch = 0xAB; break;
				case 0x0165: ch = 0xBB; break;
				case 0x016E: ch = 0xD9; break;
				case 0x016F: ch = 0xF9; break;
				case 0x0170: ch = 0xDB; break;
				case 0x0171: ch = 0xFB; break;
				case 0x0179: ch = 0xAC; break;
				case 0x017A: ch = 0xBC; break;
				case 0x017B: ch = 0xAF; break;
				case 0x017C: ch = 0xBF; break;
				case 0x017D: ch = 0xAE; break;
				case 0x017E: ch = 0xBE; break;
				case 0x02C7: ch = 0xB7; break;
				case 0x02D8: ch = 0xA2; break;
				case 0x02D9: ch = 0xFF; break;
				case 0x02DB: ch = 0xB2; break;
				case 0x02DD: ch = 0xBD; break;
				case 0x2022: ch = 0x07; break;
				case 0x203C: ch = 0x13; break;
				case 0x2190: ch = 0x1B; break;
				case 0x2191: ch = 0x18; break;
				case 0x2192: ch = 0x1A; break;
				case 0x2193: ch = 0x19; break;
				case 0x2194: ch = 0x1D; break;
				case 0x2195: ch = 0x12; break;
				case 0x21A8: ch = 0x17; break;
				case 0x221F: ch = 0x1C; break;
				case 0x2500: ch = 0x94; break;
				case 0x2502: ch = 0x83; break;
				case 0x250C: ch = 0x86; break;
				case 0x2510: ch = 0x8F; break;
				case 0x2514: ch = 0x90; break;
				case 0x2518: ch = 0x85; break;
				case 0x251C: ch = 0x93; break;
				case 0x2524: ch = 0x84; break;
				case 0x252C: ch = 0x92; break;
				case 0x2534: ch = 0x91; break;
				case 0x253C: ch = 0x95; break;
				case 0x2550: ch = 0x9D; break;
				case 0x2551: ch = 0x8A; break;
				case 0x2554: ch = 0x99; break;
				case 0x2557: ch = 0x8B; break;
				case 0x255A: ch = 0x98; break;
				case 0x255D: ch = 0x8C; break;
				case 0x2560: ch = 0x9C; break;
				case 0x2563: ch = 0x89; break;
				case 0x2566: ch = 0x9B; break;
				case 0x2569: ch = 0x9A; break;
				case 0x256C: ch = 0x9E; break;
				case 0x2580: ch = 0x97; break;
				case 0x2584: ch = 0x96; break;
				case 0x2588: ch = 0x87; break;
				case 0x2591: ch = 0x80; break;
				case 0x2592: ch = 0x81; break;
				case 0x2593: ch = 0x82; break;
				case 0x25AC: ch = 0x16; break;
				case 0x25B2: ch = 0x1E; break;
				case 0x25BA: ch = 0x10; break;
				case 0x25BC: ch = 0x1F; break;
				case 0x25C4: ch = 0x11; break;
				case 0x25CB: ch = 0x09; break;
				case 0x25D8: ch = 0x08; break;
				case 0x25D9: ch = 0x0A; break;
				case 0x263A: ch = 0x01; break;
				case 0x263B: ch = 0x02; break;
				case 0x263C: ch = 0x0F; break;
				case 0x2640: ch = 0x0C; break;
				case 0x2642: ch = 0x0B; break;
				case 0x2660: ch = 0x06; break;
				case 0x2663: ch = 0x05; break;
				case 0x2665: ch = 0x03; break;
				case 0x2666: ch = 0x04; break;
				case 0x266A: ch = 0x0D; break;
				case 0x266C: ch = 0x0E; break;
				case 0xFFE8: ch = 0x83; break;
				case 0xFFE9: ch = 0x1B; break;
				case 0xFFEA: ch = 0x18; break;
				case 0xFFEB: ch = 0x1A; break;
				case 0xFFEC: ch = 0x19; break;
				case 0xFFEE: ch = 0x09; break;
				default:
				{
					if(ch >= 0xFF01 && ch <= 0xFF5E)
					{
						ch -= 0xFEE0;
					}
					else
					{
						HandleFallback (ref buffer, chars, ref charIndex, ref charCount, bytes, ref byteIndex, ref byteCount);
						continue;
					}
				}
				break;
			}
			//Write encoded byte to buffer, if buffer is defined and fallback was not used
			if (bytes != null)
				bytes[byteIndex] = (byte)ch;
			byteIndex++;
			byteCount--;
		}
		return byteIndex;
	}

	/*
	protected override void ToBytes(String s, int charIndex, int charCount,
	                                byte[] bytes, int byteIndex)
	{
		int ch;
		while(charCount > 0)
		{
			ch = (int)(s[charIndex++]);
			if(ch >= 161) switch(ch)
			{
				case 0x00A4:
				case 0x00A7:
				case 0x00A8:
				case 0x00AD:
				case 0x00B0:
				case 0x00B4:
				case 0x00B8:
				case 0x00C1:
				case 0x00C2:
				case 0x00C4:
				case 0x00C7:
				case 0x00C9:
				case 0x00CB:
				case 0x00CD:
				case 0x00CE:
				case 0x00D0:
				case 0x00D3:
				case 0x00D4:
				case 0x00D6:
				case 0x00D7:
				case 0x00DA:
				case 0x00DC:
				case 0x00DD:
				case 0x00DF:
				case 0x00E1:
				case 0x00E2:
				case 0x00E4:
				case 0x00E7:
				case 0x00E9:
				case 0x00EB:
				case 0x00ED:
				case 0x00EE:
				case 0x00F3:
				case 0x00F4:
				case 0x00F6:
				case 0x00F7:
				case 0x00FA:
				case 0x00FC:
				case 0x00FD:
					break;
				case 0x00A2: ch = 0x8D; break;
				case 0x00A5: ch = 0x8E; break;
				case 0x00A9: ch = 0x88; break;
				case 0x00AE: ch = 0x9F; break;
				case 0x00B6: ch = 0x14; break;
				case 0x0102: ch = 0xC3; break;
				case 0x0103: ch = 0xE3; break;
				case 0x0104: ch = 0xA1; break;
				case 0x0105: ch = 0xB1; break;
				case 0x0106: ch = 0xC6; break;
				case 0x0107: ch = 0xE6; break;
				case 0x010C: ch = 0xC8; break;
				case 0x010D: ch = 0xE8; break;
				case 0x010E: ch = 0xCF; break;
				case 0x010F: ch = 0xEF; break;
				case 0x0110: ch = 0xD0; break;
				case 0x0111: ch = 0xF0; break;
				case 0x0118: ch = 0xCA; break;
				case 0x0119: ch = 0xEA; break;
				case 0x011A: ch = 0xCC; break;
				case 0x011B: ch = 0xEC; break;
				case 0x0139: ch = 0xC5; break;
				case 0x013A: ch = 0xE5; break;
				case 0x013D: ch = 0xA5; break;
				case 0x013E: ch = 0xB5; break;
				case 0x0141: ch = 0xA3; break;
				case 0x0142: ch = 0xB3; break;
				case 0x0143: ch = 0xD1; break;
				case 0x0144: ch = 0xF1; break;
				case 0x0147: ch = 0xD2; break;
				case 0x0148: ch = 0xF2; break;
				case 0x0150: ch = 0xD5; break;
				case 0x0151: ch = 0xF5; break;
				case 0x0154: ch = 0xC0; break;
				case 0x0155: ch = 0xE0; break;
				case 0x0158: ch = 0xD8; break;
				case 0x0159: ch = 0xF8; break;
				case 0x015A: ch = 0xA6; break;
				case 0x015B: ch = 0xB6; break;
				case 0x015E: ch = 0xAA; break;
				case 0x015F: ch = 0xBA; break;
				case 0x0160: ch = 0xA9; break;
				case 0x0161: ch = 0xB9; break;
				case 0x0162: ch = 0xDE; break;
				case 0x0163: ch = 0xFE; break;
				case 0x0164: ch = 0xAB; break;
				case 0x0165: ch = 0xBB; break;
				case 0x016E: ch = 0xD9; break;
				case 0x016F: ch = 0xF9; break;
				case 0x0170: ch = 0xDB; break;
				case 0x0171: ch = 0xFB; break;
				case 0x0179: ch = 0xAC; break;
				case 0x017A: ch = 0xBC; break;
				case 0x017B: ch = 0xAF; break;
				case 0x017C: ch = 0xBF; break;
				case 0x017D: ch = 0xAE; break;
				case 0x017E: ch = 0xBE; break;
				case 0x02C7: ch = 0xB7; break;
				case 0x02D8: ch = 0xA2; break;
				case 0x02D9: ch = 0xFF; break;
				case 0x02DB: ch = 0xB2; break;
				case 0x02DD: ch = 0xBD; break;
				case 0x2022: ch = 0x07; break;
				case 0x203C: ch = 0x13; break;
				case 0x2190: ch = 0x1B; break;
				case 0x2191: ch = 0x18; break;
				case 0x2192: ch = 0x1A; break;
				case 0x2193: ch = 0x19; break;
				case 0x2194: ch = 0x1D; break;
				case 0x2195: ch = 0x12; break;
				case 0x21A8: ch = 0x17; break;
				case 0x221F: ch = 0x1C; break;
				case 0x2500: ch = 0x94; break;
				case 0x2502: ch = 0x83; break;
				case 0x250C: ch = 0x86; break;
				case 0x2510: ch = 0x8F; break;
				case 0x2514: ch = 0x90; break;
				case 0x2518: ch = 0x85; break;
				case 0x251C: ch = 0x93; break;
				case 0x2524: ch = 0x84; break;
				case 0x252C: ch = 0x92; break;
				case 0x2534: ch = 0x91; break;
				case 0x253C: ch = 0x95; break;
				case 0x2550: ch = 0x9D; break;
				case 0x2551: ch = 0x8A; break;
				case 0x2554: ch = 0x99; break;
				case 0x2557: ch = 0x8B; break;
				case 0x255A: ch = 0x98; break;
				case 0x255D: ch = 0x8C; break;
				case 0x2560: ch = 0x9C; break;
				case 0x2563: ch = 0x89; break;
				case 0x2566: ch = 0x9B; break;
				case 0x2569: ch = 0x9A; break;
				case 0x256C: ch = 0x9E; break;
				case 0x2580: ch = 0x97; break;
				case 0x2584: ch = 0x96; break;
				case 0x2588: ch = 0x87; break;
				case 0x2591: ch = 0x80; break;
				case 0x2592: ch = 0x81; break;
				case 0x2593: ch = 0x82; break;
				case 0x25AC: ch = 0x16; break;
				case 0x25B2: ch = 0x1E; break;
				case 0x25BA: ch = 0x10; break;
				case 0x25BC: ch = 0x1F; break;
				case 0x25C4: ch = 0x11; break;
				case 0x25CB: ch = 0x09; break;
				case 0x25D8: ch = 0x08; break;
				case 0x25D9: ch = 0x0A; break;
				case 0x263A: ch = 0x01; break;
				case 0x263B: ch = 0x02; break;
				case 0x263C: ch = 0x0F; break;
				case 0x2640: ch = 0x0C; break;
				case 0x2642: ch = 0x0B; break;
				case 0x2660: ch = 0x06; break;
				case 0x2663: ch = 0x05; break;
				case 0x2665: ch = 0x03; break;
				case 0x2666: ch = 0x04; break;
				case 0x266A: ch = 0x0D; break;
				case 0x266C: ch = 0x0E; break;
				case 0xFFE8: ch = 0x83; break;
				case 0xFFE9: ch = 0x1B; break;
				case 0xFFEA: ch = 0x18; break;
				case 0xFFEB: ch = 0x1A; break;
				case 0xFFEC: ch = 0x19; break;
				case 0xFFEE: ch = 0x09; break;
				default:
				{
					if(ch >= 0xFF01 && ch <= 0xFF5E)
					{
						ch -= 0xFEE0;
					}
					else
					{
						ch = 0x3F;
					}
				}
				break;
			}
			bytes[byteIndex++] = (byte)ch;
			--charCount;
		}
	}
	*/

}; // class CP28592

[Serializable]
public class ENCiso_8859_2 : CP28592
{
	public ENCiso_8859_2() : base() {}

}; // class ENCiso_8859_2

}; // namespace I18N.West
