/*
 * CP1257.cs - Baltic (Windows) code page.
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

// Generated from "ibm-5353.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.Other
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP1257 : ByteEncoding
{
	public CP1257()
		: base(1257, ToChars, "Baltic (Windows)",
		       "iso-8859-4", "windows-1257", "windows-1257",
		       true, true, true, true, 1257)
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
		'\u007E', '\u007F', '\u20AC', '\u0081', '\u201A', '\u0083', 
		'\u201E', '\u2026', '\u2020', '\u2021', '\u0088', '\u2030', 
		'\u008A', '\u2039', '\u008C', '\u00A8', '\u02C7', '\u00B8', 
		'\u0090', '\u2018', '\u2019', '\u201C', '\u201D', '\u2022', 
		'\u2013', '\u2014', '\u0098', '\u2122', '\u009A', '\u203A', 
		'\u009C', '\u00AF', '\u02DB', '\u009F', '\u00A0', '\u003F', 
		'\u00A2', '\u00A3', '\u00A4', '\u003F', '\u00A6', '\u00A7', 
		'\u00D8', '\u00A9', '\u0156', '\u00AB', '\u00AC', '\u00AD', 
		'\u00AE', '\u00C6', '\u00B0', '\u00B1', '\u00B2', '\u00B3', 
		'\u00B4', '\u00B5', '\u00B6', '\u00B7', '\u00F8', '\u00B9', 
		'\u0157', '\u00BB', '\u00BC', '\u00BD', '\u00BE', '\u00E6', 
		'\u0104', '\u012E', '\u0100', '\u0106', '\u00C4', '\u00C5', 
		'\u0118', '\u0112', '\u010C', '\u00C9', '\u0179', '\u0116', 
		'\u0122', '\u0136', '\u012A', '\u013B', '\u0160', '\u0143', 
		'\u0145', '\u00D3', '\u014C', '\u00D5', '\u00D6', '\u00D7', 
		'\u0172', '\u0141', '\u015A', '\u016A', '\u00DC', '\u017B', 
		'\u017D', '\u00DF', '\u0105', '\u012F', '\u0101', '\u0107', 
		'\u00E4', '\u00E5', '\u0119', '\u0113', '\u010D', '\u00E9', 
		'\u017A', '\u0117', '\u0123', '\u0137', '\u012B', '\u013C', 
		'\u0161', '\u0144', '\u0146', '\u00F3', '\u014D', '\u00F5', 
		'\u00F6', '\u00F7', '\u0173', '\u0142', '\u015B', '\u016B', 
		'\u00FC', '\u017C', '\u017E', '\u02D9', 
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
			if(ch >= 128) switch(ch)
			{
				case 0x0081:
				case 0x0083:
				case 0x0088:
				case 0x008A:
				case 0x008C:
				case 0x0090:
				case 0x0098:
				case 0x009A:
				case 0x009C:
				case 0x009F:
				case 0x00A0:
				case 0x00A2:
				case 0x00A3:
				case 0x00A4:
				case 0x00A6:
				case 0x00A7:
				case 0x00A9:
				case 0x00AB:
				case 0x00AC:
				case 0x00AD:
				case 0x00AE:
				case 0x00B0:
				case 0x00B1:
				case 0x00B2:
				case 0x00B3:
				case 0x00B4:
				case 0x00B5:
				case 0x00B6:
				case 0x00B7:
				case 0x00B9:
				case 0x00BB:
				case 0x00BC:
				case 0x00BD:
				case 0x00BE:
				case 0x00C4:
				case 0x00C5:
				case 0x00C9:
				case 0x00D3:
				case 0x00D5:
				case 0x00D6:
				case 0x00D7:
				case 0x00DC:
				case 0x00DF:
				case 0x00E4:
				case 0x00E5:
				case 0x00E9:
				case 0x00F3:
				case 0x00F5:
				case 0x00F6:
				case 0x00F7:
				case 0x00FC:
					break;
				case 0x00A8: ch = 0x8D; break;
				case 0x00AF: ch = 0x9D; break;
				case 0x00B8: ch = 0x8F; break;
				case 0x00C6: ch = 0xAF; break;
				case 0x00D8: ch = 0xA8; break;
				case 0x00E6: ch = 0xBF; break;
				case 0x00F8: ch = 0xB8; break;
				case 0x0100: ch = 0xC2; break;
				case 0x0101: ch = 0xE2; break;
				case 0x0104: ch = 0xC0; break;
				case 0x0105: ch = 0xE0; break;
				case 0x0106: ch = 0xC3; break;
				case 0x0107: ch = 0xE3; break;
				case 0x010C: ch = 0xC8; break;
				case 0x010D: ch = 0xE8; break;
				case 0x0112: ch = 0xC7; break;
				case 0x0113: ch = 0xE7; break;
				case 0x0116: ch = 0xCB; break;
				case 0x0117: ch = 0xEB; break;
				case 0x0118: ch = 0xC6; break;
				case 0x0119: ch = 0xE6; break;
				case 0x0122: ch = 0xCC; break;
				case 0x0123: ch = 0xEC; break;
				case 0x012A: ch = 0xCE; break;
				case 0x012B: ch = 0xEE; break;
				case 0x012E: ch = 0xC1; break;
				case 0x012F: ch = 0xE1; break;
				case 0x0136: ch = 0xCD; break;
				case 0x0137: ch = 0xED; break;
				case 0x013B: ch = 0xCF; break;
				case 0x013C: ch = 0xEF; break;
				case 0x0141: ch = 0xD9; break;
				case 0x0142: ch = 0xF9; break;
				case 0x0143: ch = 0xD1; break;
				case 0x0144: ch = 0xF1; break;
				case 0x0145: ch = 0xD2; break;
				case 0x0146: ch = 0xF2; break;
				case 0x014C: ch = 0xD4; break;
				case 0x014D: ch = 0xF4; break;
				case 0x0156: ch = 0xAA; break;
				case 0x0157: ch = 0xBA; break;
				case 0x015A: ch = 0xDA; break;
				case 0x015B: ch = 0xFA; break;
				case 0x0160: ch = 0xD0; break;
				case 0x0161: ch = 0xF0; break;
				case 0x016A: ch = 0xDB; break;
				case 0x016B: ch = 0xFB; break;
				case 0x0172: ch = 0xD8; break;
				case 0x0173: ch = 0xF8; break;
				case 0x0179: ch = 0xCA; break;
				case 0x017A: ch = 0xEA; break;
				case 0x017B: ch = 0xDD; break;
				case 0x017C: ch = 0xFD; break;
				case 0x017D: ch = 0xDE; break;
				case 0x017E: ch = 0xFE; break;
				case 0x02C7: ch = 0x8E; break;
				case 0x02D9: ch = 0xFF; break;
				case 0x02DB: ch = 0x9E; break;
				case 0x2013: ch = 0x96; break;
				case 0x2014: ch = 0x97; break;
				case 0x2018: ch = 0x91; break;
				case 0x2019: ch = 0x92; break;
				case 0x201A: ch = 0x82; break;
				case 0x201C: ch = 0x93; break;
				case 0x201D: ch = 0x94; break;
				case 0x201E: ch = 0x84; break;
				case 0x2020: ch = 0x86; break;
				case 0x2021: ch = 0x87; break;
				case 0x2022: ch = 0x95; break;
				case 0x2026: ch = 0x85; break;
				case 0x2030: ch = 0x89; break;
				case 0x2039: ch = 0x8B; break;
				case 0x203A: ch = 0x9B; break;
				case 0x20AC: ch = 0x80; break;
				case 0x2122: ch = 0x99; break;
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
			if(ch >= 128) switch(ch)
			{
				case 0x0081:
				case 0x0083:
				case 0x0088:
				case 0x008A:
				case 0x008C:
				case 0x0090:
				case 0x0098:
				case 0x009A:
				case 0x009C:
				case 0x009F:
				case 0x00A0:
				case 0x00A2:
				case 0x00A3:
				case 0x00A4:
				case 0x00A6:
				case 0x00A7:
				case 0x00A9:
				case 0x00AB:
				case 0x00AC:
				case 0x00AD:
				case 0x00AE:
				case 0x00B0:
				case 0x00B1:
				case 0x00B2:
				case 0x00B3:
				case 0x00B4:
				case 0x00B5:
				case 0x00B6:
				case 0x00B7:
				case 0x00B9:
				case 0x00BB:
				case 0x00BC:
				case 0x00BD:
				case 0x00BE:
				case 0x00C4:
				case 0x00C5:
				case 0x00C9:
				case 0x00D3:
				case 0x00D5:
				case 0x00D6:
				case 0x00D7:
				case 0x00DC:
				case 0x00DF:
				case 0x00E4:
				case 0x00E5:
				case 0x00E9:
				case 0x00F3:
				case 0x00F5:
				case 0x00F6:
				case 0x00F7:
				case 0x00FC:
					break;
				case 0x00A8: ch = 0x8D; break;
				case 0x00AF: ch = 0x9D; break;
				case 0x00B8: ch = 0x8F; break;
				case 0x00C6: ch = 0xAF; break;
				case 0x00D8: ch = 0xA8; break;
				case 0x00E6: ch = 0xBF; break;
				case 0x00F8: ch = 0xB8; break;
				case 0x0100: ch = 0xC2; break;
				case 0x0101: ch = 0xE2; break;
				case 0x0104: ch = 0xC0; break;
				case 0x0105: ch = 0xE0; break;
				case 0x0106: ch = 0xC3; break;
				case 0x0107: ch = 0xE3; break;
				case 0x010C: ch = 0xC8; break;
				case 0x010D: ch = 0xE8; break;
				case 0x0112: ch = 0xC7; break;
				case 0x0113: ch = 0xE7; break;
				case 0x0116: ch = 0xCB; break;
				case 0x0117: ch = 0xEB; break;
				case 0x0118: ch = 0xC6; break;
				case 0x0119: ch = 0xE6; break;
				case 0x0122: ch = 0xCC; break;
				case 0x0123: ch = 0xEC; break;
				case 0x012A: ch = 0xCE; break;
				case 0x012B: ch = 0xEE; break;
				case 0x012E: ch = 0xC1; break;
				case 0x012F: ch = 0xE1; break;
				case 0x0136: ch = 0xCD; break;
				case 0x0137: ch = 0xED; break;
				case 0x013B: ch = 0xCF; break;
				case 0x013C: ch = 0xEF; break;
				case 0x0141: ch = 0xD9; break;
				case 0x0142: ch = 0xF9; break;
				case 0x0143: ch = 0xD1; break;
				case 0x0144: ch = 0xF1; break;
				case 0x0145: ch = 0xD2; break;
				case 0x0146: ch = 0xF2; break;
				case 0x014C: ch = 0xD4; break;
				case 0x014D: ch = 0xF4; break;
				case 0x0156: ch = 0xAA; break;
				case 0x0157: ch = 0xBA; break;
				case 0x015A: ch = 0xDA; break;
				case 0x015B: ch = 0xFA; break;
				case 0x0160: ch = 0xD0; break;
				case 0x0161: ch = 0xF0; break;
				case 0x016A: ch = 0xDB; break;
				case 0x016B: ch = 0xFB; break;
				case 0x0172: ch = 0xD8; break;
				case 0x0173: ch = 0xF8; break;
				case 0x0179: ch = 0xCA; break;
				case 0x017A: ch = 0xEA; break;
				case 0x017B: ch = 0xDD; break;
				case 0x017C: ch = 0xFD; break;
				case 0x017D: ch = 0xDE; break;
				case 0x017E: ch = 0xFE; break;
				case 0x02C7: ch = 0x8E; break;
				case 0x02D9: ch = 0xFF; break;
				case 0x02DB: ch = 0x9E; break;
				case 0x2013: ch = 0x96; break;
				case 0x2014: ch = 0x97; break;
				case 0x2018: ch = 0x91; break;
				case 0x2019: ch = 0x92; break;
				case 0x201A: ch = 0x82; break;
				case 0x201C: ch = 0x93; break;
				case 0x201D: ch = 0x94; break;
				case 0x201E: ch = 0x84; break;
				case 0x2020: ch = 0x86; break;
				case 0x2021: ch = 0x87; break;
				case 0x2022: ch = 0x95; break;
				case 0x2026: ch = 0x85; break;
				case 0x2030: ch = 0x89; break;
				case 0x2039: ch = 0x8B; break;
				case 0x203A: ch = 0x9B; break;
				case 0x20AC: ch = 0x80; break;
				case 0x2122: ch = 0x99; break;
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

}; // class CP1257

[Serializable]
public class ENCwindows_1257 : CP1257
{
	public ENCwindows_1257() : base() {}

}; // class ENCwindows_1257

}; // namespace I18N.Other
