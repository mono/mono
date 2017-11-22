/*
 * CP28593.cs - Latin 3 (ISO) code page.
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

// Generated from "ibm-913.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.West
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP28593 : ByteEncoding
{
	public CP28593()
		: base(28593, ToChars, "Latin 3 (ISO)",
		       "iso-8859-3", "iso-8859-3", "iso-8859-3",
		       true, true, true, true, 28593)
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
		'\u009C', '\u009D', '\u009E', '\u009F', '\u00A0', '\u0126', 
		'\u02D8', '\u00A3', '\u00A4', '\u003F', '\u0124', '\u00A7', 
		'\u00A8', '\u0130', '\u015E', '\u011E', '\u0134', '\u00AD', 
		'\u003F', '\u017B', '\u00B0', '\u0127', '\u00B2', '\u00B3', 
		'\u00B4', '\u00B5', '\u0125', '\u00B7', '\u00B8', '\u0131', 
		'\u015F', '\u011F', '\u0135', '\u00BD', '\u003F', '\u017C', 
		'\u00C0', '\u00C1', '\u00C2', '\u003F', '\u00C4', '\u010A', 
		'\u0108', '\u00C7', '\u00C8', '\u00C9', '\u00CA', '\u00CB', 
		'\u00CC', '\u00CD', '\u00CE', '\u00CF', '\u003F', '\u00D1', 
		'\u00D2', '\u00D3', '\u00D4', '\u0120', '\u00D6', '\u00D7', 
		'\u011C', '\u00D9', '\u00DA', '\u00DB', '\u00DC', '\u016C', 
		'\u015C', '\u00DF', '\u00E0', '\u00E1', '\u00E2', '\u003F', 
		'\u00E4', '\u010B', '\u0109', '\u00E7', '\u00E8', '\u00E9', 
		'\u00EA', '\u00EB', '\u00EC', '\u00ED', '\u00EE', '\u00EF', 
		'\u003F', '\u00F1', '\u00F2', '\u00F3', '\u00F4', '\u0121', 
		'\u00F6', '\u00F7', '\u011D', '\u00F9', '\u00FA', '\u00FB', 
		'\u00FC', '\u016D', '\u015D', '\u02D9', 
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
				case 0x00A3:
				case 0x00A4:
				case 0x00A7:
				case 0x00A8:
				case 0x00AD:
				case 0x00B0:
				case 0x00B2:
				case 0x00B3:
				case 0x00B4:
				case 0x00B5:
				case 0x00B7:
				case 0x00B8:
				case 0x00BD:
				case 0x00C0:
				case 0x00C1:
				case 0x00C2:
				case 0x00C4:
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
				case 0x00D6:
				case 0x00D7:
				case 0x00D9:
				case 0x00DA:
				case 0x00DB:
				case 0x00DC:
				case 0x00DF:
				case 0x00E0:
				case 0x00E1:
				case 0x00E2:
				case 0x00E4:
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
				case 0x00F6:
				case 0x00F7:
				case 0x00F9:
				case 0x00FA:
				case 0x00FB:
				case 0x00FC:
					break;
				case 0x0108: ch = 0xC6; break;
				case 0x0109: ch = 0xE6; break;
				case 0x010A: ch = 0xC5; break;
				case 0x010B: ch = 0xE5; break;
				case 0x011C: ch = 0xD8; break;
				case 0x011D: ch = 0xF8; break;
				case 0x011E: ch = 0xAB; break;
				case 0x011F: ch = 0xBB; break;
				case 0x0120: ch = 0xD5; break;
				case 0x0121: ch = 0xF5; break;
				case 0x0124: ch = 0xA6; break;
				case 0x0125: ch = 0xB6; break;
				case 0x0126: ch = 0xA1; break;
				case 0x0127: ch = 0xB1; break;
				case 0x0130: ch = 0xA9; break;
				case 0x0131: ch = 0xB9; break;
				case 0x0134: ch = 0xAC; break;
				case 0x0135: ch = 0xBC; break;
				case 0x015C: ch = 0xDE; break;
				case 0x015D: ch = 0xFE; break;
				case 0x015E: ch = 0xAA; break;
				case 0x015F: ch = 0xBA; break;
				case 0x016C: ch = 0xDD; break;
				case 0x016D: ch = 0xFD; break;
				case 0x017B: ch = 0xAF; break;
				case 0x017C: ch = 0xBF; break;
				case 0x02D8: ch = 0xA2; break;
				case 0x02D9: ch = 0xFF; break;
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
}; // class CP28593

[Serializable]
public class ENCiso_8859_3 : CP28593
{
	public ENCiso_8859_3() : base() {}

}; // class ENCiso_8859_3

}; // namespace I18N.West
