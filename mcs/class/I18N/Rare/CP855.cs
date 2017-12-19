/*
 * CP855.cs - Cyrillic (DOS) code page.
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

// Generated from "ibm-855.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.Rare
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP855 : ByteEncoding
{
	public CP855()
		: base(855, ToChars, "Cyrillic (DOS)",
		       "ibm855", "ibm855", "ibm855",
		       false, false, false, false, 1251)
	{}

	private static readonly char[] ToChars = {
		'\u0000', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', 
		'\u0006', '\u0007', '\u0008', '\u0009', '\u000A', '\u000B', 
		'\u000C', '\u000D', '\u000E', '\u000F', '\u0010', '\u0011', 
		'\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', 
		'\u0018', '\u0019', '\u001C', '\u001B', '\u007F', '\u001D', 
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
		'\u007E', '\u001A', '\u0452', '\u0402', '\u0453', '\u0403', 
		'\u0451', '\u0401', '\u0454', '\u0404', '\u0455', '\u0405', 
		'\u0456', '\u0406', '\u0457', '\u0407', '\u0458', '\u0408', 
		'\u0459', '\u0409', '\u045A', '\u040A', '\u045B', '\u040B', 
		'\u045C', '\u040C', '\u045E', '\u040E', '\u045F', '\u040F', 
		'\u044E', '\u042E', '\u044A', '\u042A', '\u0430', '\u0410', 
		'\u0431', '\u0411', '\u0446', '\u0426', '\u0434', '\u0414', 
		'\u0435', '\u0415', '\u0444', '\u0424', '\u0433', '\u0413', 
		'\u00AB', '\u00BB', '\u2591', '\u2592', '\u2593', '\u2502', 
		'\u2524', '\u0445', '\u0425', '\u0438', '\u0418', '\u2563', 
		'\u2551', '\u2557', '\u255D', '\u0439', '\u0419', '\u2510', 
		'\u2514', '\u2534', '\u252C', '\u251C', '\u2500', '\u253C', 
		'\u043A', '\u041A', '\u255A', '\u2554', '\u2569', '\u2566', 
		'\u2560', '\u2550', '\u256C', '\u00A4', '\u043B', '\u041B', 
		'\u043C', '\u041C', '\u043D', '\u041D', '\u043E', '\u041E', 
		'\u043F', '\u2518', '\u250C', '\u2588', '\u2584', '\u041F', 
		'\u044F', '\u2580', '\u042F', '\u0440', '\u0420', '\u0441', 
		'\u0421', '\u0442', '\u0422', '\u0443', '\u0423', '\u0436', 
		'\u0416', '\u0432', '\u0412', '\u044C', '\u042C', '\u2116', 
		'\u00AD', '\u044B', '\u042B', '\u0437', '\u0417', '\u0448', 
		'\u0428', '\u044D', '\u042D', '\u0449', '\u0429', '\u0447', 
		'\u0427', '\u00A7', '\u25A0', '\u00A0', 
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
		EncoderFallbackBuffer buffer = null;
		while (charCount > 0)
		{
			ch = (int)(chars[charIndex]);
			if(ch >= 26) switch(ch)
			{
				case 0x001B:
				case 0x001D:
				case 0x001E:
				case 0x001F:
				case 0x0020:
				case 0x0021:
				case 0x0022:
				case 0x0023:
				case 0x0024:
				case 0x0025:
				case 0x0026:
				case 0x0027:
				case 0x0028:
				case 0x0029:
				case 0x002A:
				case 0x002B:
				case 0x002C:
				case 0x002D:
				case 0x002E:
				case 0x002F:
				case 0x0030:
				case 0x0031:
				case 0x0032:
				case 0x0033:
				case 0x0034:
				case 0x0035:
				case 0x0036:
				case 0x0037:
				case 0x0038:
				case 0x0039:
				case 0x003A:
				case 0x003B:
				case 0x003C:
				case 0x003D:
				case 0x003E:
				case 0x003F:
				case 0x0040:
				case 0x0041:
				case 0x0042:
				case 0x0043:
				case 0x0044:
				case 0x0045:
				case 0x0046:
				case 0x0047:
				case 0x0048:
				case 0x0049:
				case 0x004A:
				case 0x004B:
				case 0x004C:
				case 0x004D:
				case 0x004E:
				case 0x004F:
				case 0x0050:
				case 0x0051:
				case 0x0052:
				case 0x0053:
				case 0x0054:
				case 0x0055:
				case 0x0056:
				case 0x0057:
				case 0x0058:
				case 0x0059:
				case 0x005A:
				case 0x005B:
				case 0x005C:
				case 0x005D:
				case 0x005E:
				case 0x005F:
				case 0x0060:
				case 0x0061:
				case 0x0062:
				case 0x0063:
				case 0x0064:
				case 0x0065:
				case 0x0066:
				case 0x0067:
				case 0x0068:
				case 0x0069:
				case 0x006A:
				case 0x006B:
				case 0x006C:
				case 0x006D:
				case 0x006E:
				case 0x006F:
				case 0x0070:
				case 0x0071:
				case 0x0072:
				case 0x0073:
				case 0x0074:
				case 0x0075:
				case 0x0076:
				case 0x0077:
				case 0x0078:
				case 0x0079:
				case 0x007A:
				case 0x007B:
				case 0x007C:
				case 0x007D:
				case 0x007E:
					break;
				case 0x001A: ch = 0x7F; break;
				case 0x001C: ch = 0x1A; break;
				case 0x007F: ch = 0x1C; break;
				case 0x00A0: ch = 0xFF; break;
				case 0x00A4: ch = 0xCF; break;
				case 0x00A7: ch = 0xFD; break;
				case 0x00AB: ch = 0xAE; break;
				case 0x00AD: ch = 0xF0; break;
				case 0x00B6: ch = 0x14; break;
				case 0x00BB: ch = 0xAF; break;
				case 0x0401: ch = 0x85; break;
				case 0x0402: ch = 0x81; break;
				case 0x0403: ch = 0x83; break;
				case 0x0404: ch = 0x87; break;
				case 0x0405: ch = 0x89; break;
				case 0x0406: ch = 0x8B; break;
				case 0x0407: ch = 0x8D; break;
				case 0x0408: ch = 0x8F; break;
				case 0x0409: ch = 0x91; break;
				case 0x040A: ch = 0x93; break;
				case 0x040B: ch = 0x95; break;
				case 0x040C: ch = 0x97; break;
				case 0x040E: ch = 0x99; break;
				case 0x040F: ch = 0x9B; break;
				case 0x0410: ch = 0xA1; break;
				case 0x0411: ch = 0xA3; break;
				case 0x0412: ch = 0xEC; break;
				case 0x0413: ch = 0xAD; break;
				case 0x0414: ch = 0xA7; break;
				case 0x0415: ch = 0xA9; break;
				case 0x0416: ch = 0xEA; break;
				case 0x0417: ch = 0xF4; break;
				case 0x0418: ch = 0xB8; break;
				case 0x0419: ch = 0xBE; break;
				case 0x041A: ch = 0xC7; break;
				case 0x041B: ch = 0xD1; break;
				case 0x041C: ch = 0xD3; break;
				case 0x041D: ch = 0xD5; break;
				case 0x041E: ch = 0xD7; break;
				case 0x041F: ch = 0xDD; break;
				case 0x0420: ch = 0xE2; break;
				case 0x0421: ch = 0xE4; break;
				case 0x0422: ch = 0xE6; break;
				case 0x0423: ch = 0xE8; break;
				case 0x0424: ch = 0xAB; break;
				case 0x0425: ch = 0xB6; break;
				case 0x0426: ch = 0xA5; break;
				case 0x0427: ch = 0xFC; break;
				case 0x0428: ch = 0xF6; break;
				case 0x0429: ch = 0xFA; break;
				case 0x042A: ch = 0x9F; break;
				case 0x042B: ch = 0xF2; break;
				case 0x042C: ch = 0xEE; break;
				case 0x042D: ch = 0xF8; break;
				case 0x042E: ch = 0x9D; break;
				case 0x042F: ch = 0xE0; break;
				case 0x0430: ch = 0xA0; break;
				case 0x0431: ch = 0xA2; break;
				case 0x0432: ch = 0xEB; break;
				case 0x0433: ch = 0xAC; break;
				case 0x0434: ch = 0xA6; break;
				case 0x0435: ch = 0xA8; break;
				case 0x0436: ch = 0xE9; break;
				case 0x0437: ch = 0xF3; break;
				case 0x0438: ch = 0xB7; break;
				case 0x0439: ch = 0xBD; break;
				case 0x043A: ch = 0xC6; break;
				case 0x043B: ch = 0xD0; break;
				case 0x043C: ch = 0xD2; break;
				case 0x043D: ch = 0xD4; break;
				case 0x043E: ch = 0xD6; break;
				case 0x043F: ch = 0xD8; break;
				case 0x0440: ch = 0xE1; break;
				case 0x0441: ch = 0xE3; break;
				case 0x0442: ch = 0xE5; break;
				case 0x0443: ch = 0xE7; break;
				case 0x0444: ch = 0xAA; break;
				case 0x0445: ch = 0xB5; break;
				case 0x0446: ch = 0xA4; break;
				case 0x0447: ch = 0xFB; break;
				case 0x0448: ch = 0xF5; break;
				case 0x0449: ch = 0xF9; break;
				case 0x044A: ch = 0x9E; break;
				case 0x044B: ch = 0xF1; break;
				case 0x044C: ch = 0xED; break;
				case 0x044D: ch = 0xF7; break;
				case 0x044E: ch = 0x9C; break;
				case 0x044F: ch = 0xDE; break;
				case 0x0451: ch = 0x84; break;
				case 0x0452: ch = 0x80; break;
				case 0x0453: ch = 0x82; break;
				case 0x0454: ch = 0x86; break;
				case 0x0455: ch = 0x88; break;
				case 0x0456: ch = 0x8A; break;
				case 0x0457: ch = 0x8C; break;
				case 0x0458: ch = 0x8E; break;
				case 0x0459: ch = 0x90; break;
				case 0x045A: ch = 0x92; break;
				case 0x045B: ch = 0x94; break;
				case 0x045C: ch = 0x96; break;
				case 0x045E: ch = 0x98; break;
				case 0x045F: ch = 0x9A; break;
				case 0x2022: ch = 0x07; break;
				case 0x203C: ch = 0x13; break;
				case 0x2116: ch = 0xEF; break;
				case 0x2190: ch = 0x1B; break;
				case 0x2191: ch = 0x18; break;
				case 0x2192: ch = 0x1A; break;
				case 0x2193: ch = 0x19; break;
				case 0x2194: ch = 0x1D; break;
				case 0x2195: ch = 0x12; break;
				case 0x21A8: ch = 0x17; break;
				case 0x221F: ch = 0x1C; break;
				case 0x2302: ch = 0x7F; break;
				case 0x2500: ch = 0xC4; break;
				case 0x2502: ch = 0xB3; break;
				case 0x250C: ch = 0xDA; break;
				case 0x2510: ch = 0xBF; break;
				case 0x2514: ch = 0xC0; break;
				case 0x2518: ch = 0xD9; break;
				case 0x251C: ch = 0xC3; break;
				case 0x2524: ch = 0xB4; break;
				case 0x252C: ch = 0xC2; break;
				case 0x2534: ch = 0xC1; break;
				case 0x253C: ch = 0xC5; break;
				case 0x2550: ch = 0xCD; break;
				case 0x2551: ch = 0xBA; break;
				case 0x2554: ch = 0xC9; break;
				case 0x2557: ch = 0xBB; break;
				case 0x255A: ch = 0xC8; break;
				case 0x255D: ch = 0xBC; break;
				case 0x2560: ch = 0xCC; break;
				case 0x2563: ch = 0xB9; break;
				case 0x2566: ch = 0xCB; break;
				case 0x2569: ch = 0xCA; break;
				case 0x256C: ch = 0xCE; break;
				case 0x2580: ch = 0xDF; break;
				case 0x2584: ch = 0xDC; break;
				case 0x2588: ch = 0xDB; break;
				case 0x2591: ch = 0xB0; break;
				case 0x2592: ch = 0xB1; break;
				case 0x2593: ch = 0xB2; break;
				case 0x25A0: ch = 0xFE; break;
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
				case 0x266B: ch = 0x0E; break;
				case 0xFFE8: ch = 0xB3; break;
				case 0xFFE9: ch = 0x1B; break;
				case 0xFFEA: ch = 0x18; break;
				case 0xFFEB: ch = 0x1A; break;
				case 0xFFEC: ch = 0x19; break;
				case 0xFFED: ch = 0xFE; break;
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
						charIndex++;
						charCount--;
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
			charIndex++;
			charCount--;
		}
		return byteIndex;
	}
}; // class CP855

[Serializable]
public class ENCibm855 : CP855
{
	public ENCibm855() : base() {}

}; // class ENCibm855

}; // namespace I18N.Rare
