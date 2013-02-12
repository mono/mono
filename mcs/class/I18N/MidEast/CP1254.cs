/*
 * CP1254.cs - Turkish (Windows) code page.
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

// Generated from "ibm-5350.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.MidEast
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP1254 : ByteEncoding
{
	public CP1254()
		: base(1254, ToChars, "Turkish (Windows)",
		       "iso-8859-9", "windows-1254", "windows-1254",
		       true, true, true, true, 1254)
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
		'\u007E', '\u007F', '\u20AC', '\u0081', '\u201A', '\u0192', 
		'\u201E', '\u2026', '\u2020', '\u2021', '\u02C6', '\u2030', 
		'\u0160', '\u2039', '\u0152', '\u008D', '\u008E', '\u008F', 
		'\u0090', '\u2018', '\u2019', '\u201C', '\u201D', '\u2022', 
		'\u2013', '\u2014', '\u02DC', '\u2122', '\u0161', '\u203A', 
		'\u0153', '\u009D', '\u009E', '\u0178', '\u00A0', '\u00A1', 
		'\u00A2', '\u00A3', '\u00A4', '\u00A5', '\u00A6', '\u00A7', 
		'\u00A8', '\u00A9', '\u00AA', '\u00AB', '\u00AC', '\u00AD', 
		'\u00AE', '\u00AF', '\u00B0', '\u00B1', '\u00B2', '\u00B3', 
		'\u00B4', '\u00B5', '\u00B6', '\u00B7', '\u00B8', '\u00B9', 
		'\u00BA', '\u00BB', '\u00BC', '\u00BD', '\u00BE', '\u00BF', 
		'\u00C0', '\u00C1', '\u00C2', '\u00C3', '\u00C4', '\u00C5', 
		'\u00C6', '\u00C7', '\u00C8', '\u00C9', '\u00CA', '\u00CB', 
		'\u00CC', '\u00CD', '\u00CE', '\u00CF', '\u011E', '\u00D1', 
		'\u00D2', '\u00D3', '\u00D4', '\u00D5', '\u00D6', '\u00D7', 
		'\u00D8', '\u00D9', '\u00DA', '\u00DB', '\u00DC', '\u0130', 
		'\u015E', '\u00DF', '\u00E0', '\u00E1', '\u00E2', '\u00E3', 
		'\u00E4', '\u00E5', '\u00E6', '\u00E7', '\u00E8', '\u00E9', 
		'\u00EA', '\u00EB', '\u00EC', '\u00ED', '\u00EE', '\u00EF', 
		'\u011F', '\u00F1', '\u00F2', '\u00F3', '\u00F4', '\u00F5', 
		'\u00F6', '\u00F7', '\u00F8', '\u00F9', '\u00FA', '\u00FB', 
		'\u00FC', '\u0131', '\u015F', '\u00FF', 
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
				case 0x008D:
				case 0x008E:
				case 0x008F:
				case 0x0090:
				case 0x009D:
				case 0x009E:
				case 0x00A0:
				case 0x00A1:
				case 0x00A2:
				case 0x00A3:
				case 0x00A4:
				case 0x00A5:
				case 0x00A6:
				case 0x00A7:
				case 0x00A8:
				case 0x00A9:
				case 0x00AA:
				case 0x00AB:
				case 0x00AC:
				case 0x00AD:
				case 0x00AE:
				case 0x00AF:
				case 0x00B0:
				case 0x00B1:
				case 0x00B2:
				case 0x00B3:
				case 0x00B4:
				case 0x00B5:
				case 0x00B6:
				case 0x00B7:
				case 0x00B8:
				case 0x00B9:
				case 0x00BA:
				case 0x00BB:
				case 0x00BC:
				case 0x00BD:
				case 0x00BE:
				case 0x00BF:
				case 0x00C0:
				case 0x00C1:
				case 0x00C2:
				case 0x00C3:
				case 0x00C4:
				case 0x00C5:
				case 0x00C6:
				case 0x00C7:
				case 0x00C8:
				case 0x00C9:
				case 0x00CA:
				case 0x00CB:
				case 0x00CC:
				case 0x00CD:
				case 0x00CE:
				case 0x00CF:
				case 0x00D1:
				case 0x00D2:
				case 0x00D3:
				case 0x00D4:
				case 0x00D5:
				case 0x00D6:
				case 0x00D7:
				case 0x00D8:
				case 0x00D9:
				case 0x00DA:
				case 0x00DB:
				case 0x00DC:
				case 0x00DF:
				case 0x00E0:
				case 0x00E1:
				case 0x00E2:
				case 0x00E3:
				case 0x00E4:
				case 0x00E5:
				case 0x00E6:
				case 0x00E7:
				case 0x00E8:
				case 0x00E9:
				case 0x00EA:
				case 0x00EB:
				case 0x00EC:
				case 0x00ED:
				case 0x00EE:
				case 0x00EF:
				case 0x00F1:
				case 0x00F2:
				case 0x00F3:
				case 0x00F4:
				case 0x00F5:
				case 0x00F6:
				case 0x00F7:
				case 0x00F8:
				case 0x00F9:
				case 0x00FA:
				case 0x00FB:
				case 0x00FC:
				case 0x00FF:
					break;
				case 0x011E: ch = 0xD0; break;
				case 0x011F: ch = 0xF0; break;
				case 0x0130: ch = 0xDD; break;
				case 0x0131: ch = 0xFD; break;
				case 0x0152: ch = 0x8C; break;
				case 0x0153: ch = 0x9C; break;
				case 0x015E: ch = 0xDE; break;
				case 0x015F: ch = 0xFE; break;
				case 0x0160: ch = 0x8A; break;
				case 0x0161: ch = 0x9A; break;
				case 0x0178: ch = 0x9F; break;
				case 0x0192: ch = 0x83; break;
				case 0x02C6: ch = 0x88; break;
				case 0x02DC: ch = 0x98; break;
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
				case 0x008D:
				case 0x008E:
				case 0x008F:
				case 0x0090:
				case 0x009D:
				case 0x009E:
				case 0x00A0:
				case 0x00A1:
				case 0x00A2:
				case 0x00A3:
				case 0x00A4:
				case 0x00A5:
				case 0x00A6:
				case 0x00A7:
				case 0x00A8:
				case 0x00A9:
				case 0x00AA:
				case 0x00AB:
				case 0x00AC:
				case 0x00AD:
				case 0x00AE:
				case 0x00AF:
				case 0x00B0:
				case 0x00B1:
				case 0x00B2:
				case 0x00B3:
				case 0x00B4:
				case 0x00B5:
				case 0x00B6:
				case 0x00B7:
				case 0x00B8:
				case 0x00B9:
				case 0x00BA:
				case 0x00BB:
				case 0x00BC:
				case 0x00BD:
				case 0x00BE:
				case 0x00BF:
				case 0x00C0:
				case 0x00C1:
				case 0x00C2:
				case 0x00C3:
				case 0x00C4:
				case 0x00C5:
				case 0x00C6:
				case 0x00C7:
				case 0x00C8:
				case 0x00C9:
				case 0x00CA:
				case 0x00CB:
				case 0x00CC:
				case 0x00CD:
				case 0x00CE:
				case 0x00CF:
				case 0x00D1:
				case 0x00D2:
				case 0x00D3:
				case 0x00D4:
				case 0x00D5:
				case 0x00D6:
				case 0x00D7:
				case 0x00D8:
				case 0x00D9:
				case 0x00DA:
				case 0x00DB:
				case 0x00DC:
				case 0x00DF:
				case 0x00E0:
				case 0x00E1:
				case 0x00E2:
				case 0x00E3:
				case 0x00E4:
				case 0x00E5:
				case 0x00E6:
				case 0x00E7:
				case 0x00E8:
				case 0x00E9:
				case 0x00EA:
				case 0x00EB:
				case 0x00EC:
				case 0x00ED:
				case 0x00EE:
				case 0x00EF:
				case 0x00F1:
				case 0x00F2:
				case 0x00F3:
				case 0x00F4:
				case 0x00F5:
				case 0x00F6:
				case 0x00F7:
				case 0x00F8:
				case 0x00F9:
				case 0x00FA:
				case 0x00FB:
				case 0x00FC:
				case 0x00FF:
					break;
				case 0x011E: ch = 0xD0; break;
				case 0x011F: ch = 0xF0; break;
				case 0x0130: ch = 0xDD; break;
				case 0x0131: ch = 0xFD; break;
				case 0x0152: ch = 0x8C; break;
				case 0x0153: ch = 0x9C; break;
				case 0x015E: ch = 0xDE; break;
				case 0x015F: ch = 0xFE; break;
				case 0x0160: ch = 0x8A; break;
				case 0x0161: ch = 0x9A; break;
				case 0x0178: ch = 0x9F; break;
				case 0x0192: ch = 0x83; break;
				case 0x02C6: ch = 0x88; break;
				case 0x02DC: ch = 0x98; break;
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

}; // class CP1254

[Serializable]
public class ENCwindows_1254 : CP1254
{
	public ENCwindows_1254() : base() {}

}; // class ENCwindows_1254

}; // namespace I18N.MidEast
