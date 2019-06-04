/*
 * CP28594.cs - Baltic (ISO) code page.
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

// Generated from "ibm-914.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.Other
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP28594 : ByteEncoding
{
	public CP28594()
		: base(28594, ToChars, "Baltic (ISO)",
		       "iso-8859-4", "iso-8859-4", "iso-8859-4",
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
		'\u007E', '\u007F', '\u0080', '\u0081', '\u0082', '\u0083', 
		'\u0084', '\u0085', '\u0086', '\u0087', '\u0088', '\u0089', 
		'\u008A', '\u008B', '\u008C', '\u008D', '\u008E', '\u008F', 
		'\u0090', '\u0091', '\u0092', '\u0093', '\u0094', '\u0095', 
		'\u0096', '\u0097', '\u0098', '\u0099', '\u009A', '\u009B', 
		'\u009C', '\u009D', '\u009E', '\u009F', '\u00A0', '\u0104', 
		'\u0138', '\u0156', '\u00A4', '\u0128', '\u013B', '\u00A7', 
		'\u00A8', '\u0160', '\u0112', '\u0122', '\u0166', '\u00AD', 
		'\u017D', '\u00AF', '\u00B0', '\u0105', '\u02DB', '\u0157', 
		'\u00B4', '\u0129', '\u013C', '\u02C7', '\u00B8', '\u0161', 
		'\u0113', '\u0123', '\u0167', '\u014A', '\u017E', '\u014B', 
		'\u0100', '\u00C1', '\u00C2', '\u00C3', '\u00C4', '\u00C5', 
		'\u00C6', '\u012E', '\u010C', '\u00C9', '\u0118', '\u00CB', 
		'\u0116', '\u00CD', '\u00CE', '\u012A', '\u0110', '\u0145', 
		'\u014C', '\u0136', '\u00D4', '\u00D5', '\u00D6', '\u00D7', 
		'\u00D8', '\u0172', '\u00DA', '\u00DB', '\u00DC', '\u0168', 
		'\u016A', '\u00DF', '\u0101', '\u00E1', '\u00E2', '\u00E3', 
		'\u00E4', '\u00E5', '\u00E6', '\u012F', '\u010D', '\u00E9', 
		'\u0119', '\u00EB', '\u0117', '\u00ED', '\u00EE', '\u012B', 
		'\u0111', '\u0146', '\u014D', '\u0137', '\u00F4', '\u00F5', 
		'\u00F6', '\u00F7', '\u00F8', '\u0173', '\u00FA', '\u00FB', 
		'\u00FC', '\u0169', '\u016B', '\u02D9', 
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
			if(ch >= 161) switch(ch)
			{
				case 0x00A4:
				case 0x00A7:
				case 0x00A8:
				case 0x00AD:
				case 0x00AF:
				case 0x00B0:
				case 0x00B4:
				case 0x00B8:
				case 0x00C1:
				case 0x00C2:
				case 0x00C3:
				case 0x00C4:
				case 0x00C5:
				case 0x00C6:
				case 0x00C9:
				case 0x00CB:
				case 0x00CD:
				case 0x00CE:
				case 0x00D4:
				case 0x00D5:
				case 0x00D6:
				case 0x00D7:
				case 0x00D8:
				case 0x00DA:
				case 0x00DB:
				case 0x00DC:
				case 0x00DF:
				case 0x00E1:
				case 0x00E2:
				case 0x00E3:
				case 0x00E4:
				case 0x00E5:
				case 0x00E6:
				case 0x00E9:
				case 0x00EB:
				case 0x00ED:
				case 0x00EE:
				case 0x00F4:
				case 0x00F5:
				case 0x00F6:
				case 0x00F7:
				case 0x00F8:
				case 0x00FA:
				case 0x00FB:
				case 0x00FC:
					break;
				case 0x0100: ch = 0xC0; break;
				case 0x0101: ch = 0xE0; break;
				case 0x0104: ch = 0xA1; break;
				case 0x0105: ch = 0xB1; break;
				case 0x010C: ch = 0xC8; break;
				case 0x010D: ch = 0xE8; break;
				case 0x0110: ch = 0xD0; break;
				case 0x0111: ch = 0xF0; break;
				case 0x0112: ch = 0xAA; break;
				case 0x0113: ch = 0xBA; break;
				case 0x0116: ch = 0xCC; break;
				case 0x0117: ch = 0xEC; break;
				case 0x0118: ch = 0xCA; break;
				case 0x0119: ch = 0xEA; break;
				case 0x0122: ch = 0xAB; break;
				case 0x0123: ch = 0xBB; break;
				case 0x0128: ch = 0xA5; break;
				case 0x0129: ch = 0xB5; break;
				case 0x012A: ch = 0xCF; break;
				case 0x012B: ch = 0xEF; break;
				case 0x012E: ch = 0xC7; break;
				case 0x012F: ch = 0xE7; break;
				case 0x0136: ch = 0xD3; break;
				case 0x0137: ch = 0xF3; break;
				case 0x0138: ch = 0xA2; break;
				case 0x013B: ch = 0xA6; break;
				case 0x013C: ch = 0xB6; break;
				case 0x0145: ch = 0xD1; break;
				case 0x0146: ch = 0xF1; break;
				case 0x014A: ch = 0xBD; break;
				case 0x014B: ch = 0xBF; break;
				case 0x014C: ch = 0xD2; break;
				case 0x014D: ch = 0xF2; break;
				case 0x0156: ch = 0xA3; break;
				case 0x0157: ch = 0xB3; break;
				case 0x0160: ch = 0xA9; break;
				case 0x0161: ch = 0xB9; break;
				case 0x0166: ch = 0xAC; break;
				case 0x0167: ch = 0xBC; break;
				case 0x0168: ch = 0xDD; break;
				case 0x0169: ch = 0xFD; break;
				case 0x016A: ch = 0xDE; break;
				case 0x016B: ch = 0xFE; break;
				case 0x0172: ch = 0xD9; break;
				case 0x0173: ch = 0xF9; break;
				case 0x017D: ch = 0xAE; break;
				case 0x017E: ch = 0xBE; break;
				case 0x02C7: ch = 0xB7; break;
				case 0x02D9: ch = 0xFF; break;
				case 0x02DB: ch = 0xB2; break;
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
}; // class CP28594

[Serializable]
public class ENCiso_8859_4 : CP28594
{
	public ENCiso_8859_4() : base() {}

}; // class ENCiso_8859_4

}; // namespace I18N.Other
