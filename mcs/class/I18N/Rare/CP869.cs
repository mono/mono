/*
 * CP869.cs - Greek (DOS) code page.
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

// Generated from "ibm-869.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.Rare
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP869 : ByteEncoding
{
	public CP869()
		: base(869, ToChars, "Greek (DOS)",
		       "ibm869", "ibm869", "ibm869",
		       false, false, false, false, 1253)
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
		'\u007E', '\u001A', '\u003F', '\u003F', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u0386', '\u003F', '\u0387', '\u00AC', 
		'\u00A6', '\u2018', '\u2019', '\u0388', '\u2015', '\u0389', 
		'\u038A', '\u03AA', '\u038C', '\u003F', '\u003F', '\u038E', 
		'\u03AB', '\u00A9', '\u038F', '\u00B2', '\u00B3', '\u03AC', 
		'\u00A3', '\u03AD', '\u03AE', '\u03AF', '\u03CA', '\u0390', 
		'\u03CC', '\u03CD', '\u0391', '\u0392', '\u0393', '\u0394', 
		'\u0395', '\u0396', '\u0397', '\u00BD', '\u0398', '\u0399', 
		'\u00AB', '\u00BB', '\u2591', '\u2592', '\u2593', '\u2502', 
		'\u2524', '\u039A', '\u039B', '\u039C', '\u039D', '\u2563', 
		'\u2551', '\u2557', '\u255D', '\u039E', '\u039F', '\u2510', 
		'\u2514', '\u2534', '\u252C', '\u251C', '\u2500', '\u253C', 
		'\u03A0', '\u03A1', '\u255A', '\u2554', '\u2569', '\u2566', 
		'\u2560', '\u2550', '\u256C', '\u03A3', '\u03A4', '\u03A5', 
		'\u03A6', '\u03A7', '\u03A8', '\u03A9', '\u03B1', '\u03B2', 
		'\u03B3', '\u2518', '\u250C', '\u2588', '\u2584', '\u03B4', 
		'\u03B5', '\u2580', '\u03B6', '\u03B7', '\u03B8', '\u03B9', 
		'\u03BA', '\u03BB', '\u03BC', '\u03BD', '\u03BE', '\u03BF', 
		'\u03C0', '\u03C1', '\u03C3', '\u03C2', '\u03C4', '\u00B4', 
		'\u00AD', '\u00B1', '\u03C5', '\u03C6', '\u03C7', '\u00A7', 
		'\u03C8', '\u0385', '\u00B0', '\u00A8', '\u03C9', '\u03CB', 
		'\u03B0', '\u03CE', '\u25A0', '\u00A0', 
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
				case 0x00A3: ch = 0x9C; break;
				case 0x00A6: ch = 0x8A; break;
				case 0x00A7: ch = 0xF5; break;
				case 0x00A8: ch = 0xF9; break;
				case 0x00A9: ch = 0x97; break;
				case 0x00AB: ch = 0xAE; break;
				case 0x00AC: ch = 0x89; break;
				case 0x00AD: ch = 0xF0; break;
				case 0x00B0: ch = 0xF8; break;
				case 0x00B1: ch = 0xF1; break;
				case 0x00B2: ch = 0x99; break;
				case 0x00B3: ch = 0x9A; break;
				case 0x00B4: ch = 0xEF; break;
				case 0x00B6: ch = 0x14; break;
				case 0x00B7: ch = 0x88; break;
				case 0x00BB: ch = 0xAF; break;
				case 0x00BD: ch = 0xAB; break;
				case 0x0385: ch = 0xF7; break;
				case 0x0386: ch = 0x86; break;
				case 0x0387: ch = 0x88; break;
				case 0x0388: ch = 0x8D; break;
				case 0x0389: ch = 0x8F; break;
				case 0x038A: ch = 0x90; break;
				case 0x038C: ch = 0x92; break;
				case 0x038E: ch = 0x95; break;
				case 0x038F: ch = 0x98; break;
				case 0x0390: ch = 0xA1; break;
				case 0x0391:
				case 0x0392:
				case 0x0393:
				case 0x0394:
				case 0x0395:
				case 0x0396:
				case 0x0397:
					ch -= 0x02ED;
					break;
				case 0x0398: ch = 0xAC; break;
				case 0x0399: ch = 0xAD; break;
				case 0x039A:
				case 0x039B:
				case 0x039C:
				case 0x039D:
					ch -= 0x02E5;
					break;
				case 0x039E: ch = 0xBD; break;
				case 0x039F: ch = 0xBE; break;
				case 0x03A0: ch = 0xC6; break;
				case 0x03A1: ch = 0xC7; break;
				case 0x03A3:
				case 0x03A4:
				case 0x03A5:
				case 0x03A6:
				case 0x03A7:
				case 0x03A8:
				case 0x03A9:
					ch -= 0x02D4;
					break;
				case 0x03AA: ch = 0x91; break;
				case 0x03AB: ch = 0x96; break;
				case 0x03AC: ch = 0x9B; break;
				case 0x03AD: ch = 0x9D; break;
				case 0x03AE: ch = 0x9E; break;
				case 0x03AF: ch = 0x9F; break;
				case 0x03B0: ch = 0xFC; break;
				case 0x03B1: ch = 0xD6; break;
				case 0x03B2: ch = 0xD7; break;
				case 0x03B3: ch = 0xD8; break;
				case 0x03B4: ch = 0xDD; break;
				case 0x03B5: ch = 0xDE; break;
				case 0x03B6:
				case 0x03B7:
				case 0x03B8:
				case 0x03B9:
				case 0x03BA:
				case 0x03BB:
				case 0x03BC:
				case 0x03BD:
				case 0x03BE:
				case 0x03BF:
				case 0x03C0:
				case 0x03C1:
					ch -= 0x02D6;
					break;
				case 0x03C2: ch = 0xED; break;
				case 0x03C3: ch = 0xEC; break;
				case 0x03C4: ch = 0xEE; break;
				case 0x03C5: ch = 0xF2; break;
				case 0x03C6: ch = 0xF3; break;
				case 0x03C7: ch = 0xF4; break;
				case 0x03C8: ch = 0xF6; break;
				case 0x03C9: ch = 0xFA; break;
				case 0x03CA: ch = 0xA0; break;
				case 0x03CB: ch = 0xFB; break;
				case 0x03CC: ch = 0xA2; break;
				case 0x03CD: ch = 0xA3; break;
				case 0x03CE: ch = 0xFD; break;
				case 0x03D5: ch = 0xF3; break;
				case 0x2015: ch = 0x8E; break;
				case 0x2018: ch = 0x8B; break;
				case 0x2019: ch = 0x8C; break;
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
}; // class CP869

[Serializable]
public class ENCibm869 : CP869
{
	public ENCibm869() : base() {}

}; // class ENCibm869

}; // namespace I18N.Rare
