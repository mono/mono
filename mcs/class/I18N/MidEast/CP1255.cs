/*
 * CP1255.cs - Hebrew (Windows) code page.
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

// Generated from "ibm-5351.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.MidEast
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP1255 : ByteEncoding
{
	public CP1255()
		: base(1255, ToChars, "Hebrew (Windows)",
		       "windows-1255", "windows-1255", "windows-1255",
		       true, true, true, true, 1255)
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
		'\u008A', '\u2039', '\u008C', '\u008D', '\u008E', '\u008F', 
		'\u0090', '\u2018', '\u2019', '\u201C', '\u201D', '\u2022', 
		'\u2013', '\u2014', '\u02DC', '\u2122', '\u009A', '\u203A', 
		'\u009C', '\u009D', '\u009E', '\u009F', '\u00A0', '\u00A1', 
		'\u00A2', '\u00A3', '\u20AA', '\u00A5', '\u00A6', '\u00A7', 
		'\u00A8', '\u00A9', '\u00D7', '\u00AB', '\u00AC', '\u00AD', 
		'\u00AE', '\u00AF', '\u00B0', '\u00B1', '\u00B2', '\u00B3', 
		'\u00B4', '\u00B5', '\u00B6', '\u00B7', '\u00B8', '\u00B9', 
		'\u00F7', '\u00BB', '\u00BC', '\u00BD', '\u00BE', '\u00BF', 
		'\u05B0', '\u05B1', '\u05B2', '\u05B3', '\u05B4', '\u05B5', 
		'\u05B6', '\u05B7', '\u05B8', '\u05B9', '\u003F', '\u05BB', 
		'\u05BC', '\u05BD', '\u05BE', '\u05BF', '\u05C0', '\u05C1', 
		'\u05C2', '\u05C3', '\u05F0', '\u05F1', '\u05F2', '\u05F3', 
		'\u05F4', '\u003F', '\u003F', '\u003F', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u05D0', '\u05D1', '\u05D2', '\u05D3', 
		'\u05D4', '\u05D5', '\u05D6', '\u05D7', '\u05D8', '\u05D9', 
		'\u05DA', '\u05DB', '\u05DC', '\u05DD', '\u05DE', '\u05DF', 
		'\u05E0', '\u05E1', '\u05E2', '\u05E3', '\u05E4', '\u05E5', 
		'\u05E6', '\u05E7', '\u05E8', '\u05E9', '\u05EA', '\u003F', 
		'\u003F', '\u200E', '\u200F', '\u003F', 
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
				case 0x008A:
				case 0x008C:
				case 0x008D:
				case 0x008E:
				case 0x008F:
				case 0x0090:
				case 0x009A:
				case 0x009C:
				case 0x009D:
				case 0x009E:
				case 0x009F:
				case 0x00A0:
				case 0x00A1:
				case 0x00A2:
				case 0x00A3:
				case 0x00A5:
				case 0x00A6:
				case 0x00A7:
				case 0x00A8:
				case 0x00A9:
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
				case 0x00BB:
				case 0x00BC:
				case 0x00BD:
				case 0x00BE:
				case 0x00BF:
					break;
				case 0x00D7: ch = 0xAA; break;
				case 0x00F7: ch = 0xBA; break;
				case 0x0192: ch = 0x83; break;
				case 0x02C6: ch = 0x88; break;
				case 0x02DC: ch = 0x98; break;
				case 0x05B0:
				case 0x05B1:
				case 0x05B2:
				case 0x05B3:
				case 0x05B4:
				case 0x05B5:
				case 0x05B6:
				case 0x05B7:
				case 0x05B8:
				case 0x05B9:
					ch -= 0x04F0;
					break;
				case 0x05BB:
				case 0x05BC:
				case 0x05BD:
				case 0x05BE:
				case 0x05BF:
				case 0x05C0:
				case 0x05C1:
				case 0x05C2:
				case 0x05C3:
					ch -= 0x04F0;
					break;
				case 0x05D0:
				case 0x05D1:
				case 0x05D2:
				case 0x05D3:
				case 0x05D4:
				case 0x05D5:
				case 0x05D6:
				case 0x05D7:
				case 0x05D8:
				case 0x05D9:
				case 0x05DA:
				case 0x05DB:
				case 0x05DC:
				case 0x05DD:
				case 0x05DE:
				case 0x05DF:
				case 0x05E0:
				case 0x05E1:
				case 0x05E2:
				case 0x05E3:
				case 0x05E4:
				case 0x05E5:
				case 0x05E6:
				case 0x05E7:
				case 0x05E8:
				case 0x05E9:
				case 0x05EA:
					ch -= 0x04F0;
					break;
				case 0x05F0:
				case 0x05F1:
				case 0x05F2:
				case 0x05F3:
				case 0x05F4:
					ch -= 0x051C;
					break;
				case 0x200E: ch = 0xFD; break;
				case 0x200F: ch = 0xFE; break;
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
				case 0x20AA: ch = 0xA4; break;
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
				case 0x008A:
				case 0x008C:
				case 0x008D:
				case 0x008E:
				case 0x008F:
				case 0x0090:
				case 0x009A:
				case 0x009C:
				case 0x009D:
				case 0x009E:
				case 0x009F:
				case 0x00A0:
				case 0x00A1:
				case 0x00A2:
				case 0x00A3:
				case 0x00A5:
				case 0x00A6:
				case 0x00A7:
				case 0x00A8:
				case 0x00A9:
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
				case 0x00BB:
				case 0x00BC:
				case 0x00BD:
				case 0x00BE:
				case 0x00BF:
					break;
				case 0x00D7: ch = 0xAA; break;
				case 0x00F7: ch = 0xBA; break;
				case 0x0192: ch = 0x83; break;
				case 0x02C6: ch = 0x88; break;
				case 0x02DC: ch = 0x98; break;
				case 0x05B0:
				case 0x05B1:
				case 0x05B2:
				case 0x05B3:
				case 0x05B4:
				case 0x05B5:
				case 0x05B6:
				case 0x05B7:
				case 0x05B8:
				case 0x05B9:
					ch -= 0x04F0;
					break;
				case 0x05BB:
				case 0x05BC:
				case 0x05BD:
				case 0x05BE:
				case 0x05BF:
				case 0x05C0:
				case 0x05C1:
				case 0x05C2:
				case 0x05C3:
					ch -= 0x04F0;
					break;
				case 0x05D0:
				case 0x05D1:
				case 0x05D2:
				case 0x05D3:
				case 0x05D4:
				case 0x05D5:
				case 0x05D6:
				case 0x05D7:
				case 0x05D8:
				case 0x05D9:
				case 0x05DA:
				case 0x05DB:
				case 0x05DC:
				case 0x05DD:
				case 0x05DE:
				case 0x05DF:
				case 0x05E0:
				case 0x05E1:
				case 0x05E2:
				case 0x05E3:
				case 0x05E4:
				case 0x05E5:
				case 0x05E6:
				case 0x05E7:
				case 0x05E8:
				case 0x05E9:
				case 0x05EA:
					ch -= 0x04F0;
					break;
				case 0x05F0:
				case 0x05F1:
				case 0x05F2:
				case 0x05F3:
				case 0x05F4:
					ch -= 0x051C;
					break;
				case 0x200E: ch = 0xFD; break;
				case 0x200F: ch = 0xFE; break;
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
				case 0x20AA: ch = 0xA4; break;
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

}; // class CP1255

[Serializable]
public class ENCwindows_1255 : CP1255
{
	public ENCwindows_1255() : base() {}

}; // class ENCwindows_1255

}; // namespace I18N.MidEast
