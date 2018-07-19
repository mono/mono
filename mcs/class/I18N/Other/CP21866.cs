/*
 * CP21866.cs - Ukrainian (KOI8-U) code page.
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

// Generated from "koi8-u.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.Other
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP21866 : ByteEncoding
{
	public CP21866()
		: base(21866, ToChars, "Ukrainian (KOI8-U)",
		       "koi8-u", "koi8-u", "koi8-u",
		       true, true, true, true, 1251)
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
		'\u007E', '\u007F', '\u2500', '\u2502', '\u250C', '\u2510', 
		'\u2514', '\u2518', '\u251C', '\u2524', '\u252C', '\u2534', 
		'\u253C', '\u2580', '\u2584', '\u2588', '\u258C', '\u2590', 
		'\u2591', '\u2592', '\u2593', '\u2320', '\u25A0', '\u2219', 
		'\u221A', '\u2248', '\u2264', '\u2265', '\u00A0', '\u2321', 
		'\u00B0', '\u00B2', '\u00B7', '\u00F7', '\u2550', '\u2551', 
		'\u2552', '\u0451', '\u0454', '\u2554', '\u0456', '\u0457', 
		'\u2557', '\u2558', '\u2559', '\u255A', '\u255B', '\u0491', 
		'\u255D', '\u255E', '\u255F', '\u2560', '\u2561', '\u0401', 
		'\u0404', '\u2563', '\u0406', '\u0407', '\u2566', '\u2567', 
		'\u2568', '\u2569', '\u256A', '\u0490', '\u256C', '\u00A9', 
		'\u044E', '\u0430', '\u0431', '\u0446', '\u0434', '\u0435', 
		'\u0444', '\u0433', '\u0445', '\u0438', '\u0439', '\u043A', 
		'\u043B', '\u043C', '\u043D', '\u043E', '\u043F', '\u044F', 
		'\u0440', '\u0441', '\u0442', '\u0443', '\u0436', '\u0432', 
		'\u044C', '\u044B', '\u0437', '\u0448', '\u044D', '\u0449', 
		'\u0447', '\u044A', '\u042E', '\u0410', '\u0411', '\u0426', 
		'\u0414', '\u0415', '\u0424', '\u0413', '\u0425', '\u0418', 
		'\u0419', '\u041A', '\u041B', '\u041C', '\u041D', '\u041E', 
		'\u041F', '\u042F', '\u0420', '\u0421', '\u0422', '\u0423', 
		'\u0416', '\u0412', '\u042C', '\u042B', '\u0417', '\u0428', 
		'\u042D', '\u0429', '\u0427', '\u042A', 
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
			if(ch >= 128) switch(ch)
			{
				case 0x00A0: ch = 0x9A; break;
				case 0x00A9: ch = 0xBF; break;
				case 0x00B0: ch = 0x9C; break;
				case 0x00B2: ch = 0x9D; break;
				case 0x00B7: ch = 0x9E; break;
				case 0x00F7: ch = 0x9F; break;
				case 0x0401: ch = 0xB3; break;
				case 0x0404: ch = 0xB4; break;
				case 0x0406: ch = 0xB6; break;
				case 0x0407: ch = 0xB7; break;
				case 0x0410: ch = 0xE1; break;
				case 0x0411: ch = 0xE2; break;
				case 0x0412: ch = 0xF7; break;
				case 0x0413: ch = 0xE7; break;
				case 0x0414: ch = 0xE4; break;
				case 0x0415: ch = 0xE5; break;
				case 0x0416: ch = 0xF6; break;
				case 0x0417: ch = 0xFA; break;
				case 0x0418:
				case 0x0419:
				case 0x041A:
				case 0x041B:
				case 0x041C:
				case 0x041D:
				case 0x041E:
				case 0x041F:
					ch -= 0x032F;
					break;
				case 0x0420:
				case 0x0421:
				case 0x0422:
				case 0x0423:
					ch -= 0x032E;
					break;
				case 0x0424: ch = 0xE6; break;
				case 0x0425: ch = 0xE8; break;
				case 0x0426: ch = 0xE3; break;
				case 0x0427: ch = 0xFE; break;
				case 0x0428: ch = 0xFB; break;
				case 0x0429: ch = 0xFD; break;
				case 0x042A: ch = 0xFF; break;
				case 0x042B: ch = 0xF9; break;
				case 0x042C: ch = 0xF8; break;
				case 0x042D: ch = 0xFC; break;
				case 0x042E: ch = 0xE0; break;
				case 0x042F: ch = 0xF1; break;
				case 0x0430: ch = 0xC1; break;
				case 0x0431: ch = 0xC2; break;
				case 0x0432: ch = 0xD7; break;
				case 0x0433: ch = 0xC7; break;
				case 0x0434: ch = 0xC4; break;
				case 0x0435: ch = 0xC5; break;
				case 0x0436: ch = 0xD6; break;
				case 0x0437: ch = 0xDA; break;
				case 0x0438:
				case 0x0439:
				case 0x043A:
				case 0x043B:
				case 0x043C:
				case 0x043D:
				case 0x043E:
				case 0x043F:
					ch -= 0x036F;
					break;
				case 0x0440:
				case 0x0441:
				case 0x0442:
				case 0x0443:
					ch -= 0x036E;
					break;
				case 0x0444: ch = 0xC6; break;
				case 0x0445: ch = 0xC8; break;
				case 0x0446: ch = 0xC3; break;
				case 0x0447: ch = 0xDE; break;
				case 0x0448: ch = 0xDB; break;
				case 0x0449: ch = 0xDD; break;
				case 0x044A: ch = 0xDF; break;
				case 0x044B: ch = 0xD9; break;
				case 0x044C: ch = 0xD8; break;
				case 0x044D: ch = 0xDC; break;
				case 0x044E: ch = 0xC0; break;
				case 0x044F: ch = 0xD1; break;
				case 0x0451: ch = 0xA3; break;
				case 0x0454: ch = 0xA4; break;
				case 0x0456: ch = 0xA6; break;
				case 0x0457: ch = 0xA7; break;
				case 0x0490: ch = 0xBD; break;
				case 0x0491: ch = 0xAD; break;
				case 0x2219: ch = 0x95; break;
				case 0x221A: ch = 0x96; break;
				case 0x2248: ch = 0x97; break;
				case 0x2264: ch = 0x98; break;
				case 0x2265: ch = 0x99; break;
				case 0x2320: ch = 0x93; break;
				case 0x2321: ch = 0x9B; break;
				case 0x2500: ch = 0x80; break;
				case 0x2502: ch = 0x81; break;
				case 0x250C: ch = 0x82; break;
				case 0x2510: ch = 0x83; break;
				case 0x2514: ch = 0x84; break;
				case 0x2518: ch = 0x85; break;
				case 0x251C: ch = 0x86; break;
				case 0x2524: ch = 0x87; break;
				case 0x252C: ch = 0x88; break;
				case 0x2534: ch = 0x89; break;
				case 0x253C: ch = 0x8A; break;
				case 0x2550: ch = 0xA0; break;
				case 0x2551: ch = 0xA1; break;
				case 0x2552: ch = 0xA2; break;
				case 0x2554: ch = 0xA5; break;
				case 0x2557:
				case 0x2558:
				case 0x2559:
				case 0x255A:
				case 0x255B:
					ch -= 0x24AF;
					break;
				case 0x255D:
				case 0x255E:
				case 0x255F:
				case 0x2560:
				case 0x2561:
					ch -= 0x24AF;
					break;
				case 0x2563: ch = 0xB5; break;
				case 0x2566:
				case 0x2567:
				case 0x2568:
				case 0x2569:
				case 0x256A:
					ch -= 0x24AE;
					break;
				case 0x256C: ch = 0xBE; break;
				case 0x2580: ch = 0x8B; break;
				case 0x2584: ch = 0x8C; break;
				case 0x2588: ch = 0x8D; break;
				case 0x258C: ch = 0x8E; break;
				case 0x2590:
				case 0x2591:
				case 0x2592:
				case 0x2593:
					ch -= 0x2501;
					break;
				case 0x25A0: ch = 0x94; break;
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
}; // class CP21866

[Serializable]
public class ENCkoi8_u : CP21866
{
	public ENCkoi8_u() : base() {}

}; // class ENCkoi8_u

}; // namespace I18N.Other
