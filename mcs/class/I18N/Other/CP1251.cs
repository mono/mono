/*
 * CP1251.cs - Cyrillic (Windows) code page.
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

// Generated from "ibm-5347.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.Other
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP1251 : ByteEncoding
{
	public CP1251()
		: base(1251, ToChars, "Cyrillic (Windows)",
		       "koi8-r", "windows-1251", "windows-1251",
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
		'\u007E', '\u007F', '\u0402', '\u0403', '\u201A', '\u0453', 
		'\u201E', '\u2026', '\u2020', '\u2021', '\u20AC', '\u2030', 
		'\u0409', '\u2039', '\u040A', '\u040C', '\u040B', '\u040F', 
		'\u0452', '\u2018', '\u2019', '\u201C', '\u201D', '\u2022', 
		'\u2013', '\u2014', '\u0098', '\u2122', '\u0459', '\u203A', 
		'\u045A', '\u045C', '\u045B', '\u045F', '\u00A0', '\u040E', 
		'\u045E', '\u0408', '\u00A4', '\u0490', '\u00A6', '\u00A7', 
		'\u0401', '\u00A9', '\u0404', '\u00AB', '\u00AC', '\u00AD', 
		'\u00AE', '\u0407', '\u00B0', '\u00B1', '\u0406', '\u0456', 
		'\u0491', '\u00B5', '\u00B6', '\u00B7', '\u0451', '\u2116', 
		'\u0454', '\u00BB', '\u0458', '\u0405', '\u0455', '\u0457', 
		'\u0410', '\u0411', '\u0412', '\u0413', '\u0414', '\u0415', 
		'\u0416', '\u0417', '\u0418', '\u0419', '\u041A', '\u041B', 
		'\u041C', '\u041D', '\u041E', '\u041F', '\u0420', '\u0421', 
		'\u0422', '\u0423', '\u0424', '\u0425', '\u0426', '\u0427', 
		'\u0428', '\u0429', '\u042A', '\u042B', '\u042C', '\u042D', 
		'\u042E', '\u042F', '\u0430', '\u0431', '\u0432', '\u0433', 
		'\u0434', '\u0435', '\u0436', '\u0437', '\u0438', '\u0439', 
		'\u043A', '\u043B', '\u043C', '\u043D', '\u043E', '\u043F', 
		'\u0440', '\u0441', '\u0442', '\u0443', '\u0444', '\u0445', 
		'\u0446', '\u0447', '\u0448', '\u0449', '\u044A', '\u044B', 
		'\u044C', '\u044D', '\u044E', '\u044F', 
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
				case 0x0098:
				case 0x00A0:
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
				case 0x00B5:
				case 0x00B6:
				case 0x00B7:
				case 0x00BB:
					break;
				case 0x0401: ch = 0xA8; break;
				case 0x0402: ch = 0x80; break;
				case 0x0403: ch = 0x81; break;
				case 0x0404: ch = 0xAA; break;
				case 0x0405: ch = 0xBD; break;
				case 0x0406: ch = 0xB2; break;
				case 0x0407: ch = 0xAF; break;
				case 0x0408: ch = 0xA3; break;
				case 0x0409: ch = 0x8A; break;
				case 0x040A: ch = 0x8C; break;
				case 0x040B: ch = 0x8E; break;
				case 0x040C: ch = 0x8D; break;
				case 0x040E: ch = 0xA1; break;
				case 0x040F: ch = 0x8F; break;
				case 0x0410:
				case 0x0411:
				case 0x0412:
				case 0x0413:
				case 0x0414:
				case 0x0415:
				case 0x0416:
				case 0x0417:
				case 0x0418:
				case 0x0419:
				case 0x041A:
				case 0x041B:
				case 0x041C:
				case 0x041D:
				case 0x041E:
				case 0x041F:
				case 0x0420:
				case 0x0421:
				case 0x0422:
				case 0x0423:
				case 0x0424:
				case 0x0425:
				case 0x0426:
				case 0x0427:
				case 0x0428:
				case 0x0429:
				case 0x042A:
				case 0x042B:
				case 0x042C:
				case 0x042D:
				case 0x042E:
				case 0x042F:
				case 0x0430:
				case 0x0431:
				case 0x0432:
				case 0x0433:
				case 0x0434:
				case 0x0435:
				case 0x0436:
				case 0x0437:
				case 0x0438:
				case 0x0439:
				case 0x043A:
				case 0x043B:
				case 0x043C:
				case 0x043D:
				case 0x043E:
				case 0x043F:
				case 0x0440:
				case 0x0441:
				case 0x0442:
				case 0x0443:
				case 0x0444:
				case 0x0445:
				case 0x0446:
				case 0x0447:
				case 0x0448:
				case 0x0449:
				case 0x044A:
				case 0x044B:
				case 0x044C:
				case 0x044D:
				case 0x044E:
				case 0x044F:
					ch -= 0x0350;
					break;
				case 0x0451: ch = 0xB8; break;
				case 0x0452: ch = 0x90; break;
				case 0x0453: ch = 0x83; break;
				case 0x0454: ch = 0xBA; break;
				case 0x0455: ch = 0xBE; break;
				case 0x0456: ch = 0xB3; break;
				case 0x0457: ch = 0xBF; break;
				case 0x0458: ch = 0xBC; break;
				case 0x0459: ch = 0x9A; break;
				case 0x045A: ch = 0x9C; break;
				case 0x045B: ch = 0x9E; break;
				case 0x045C: ch = 0x9D; break;
				case 0x045E: ch = 0xA2; break;
				case 0x045F: ch = 0x9F; break;
				case 0x0490: ch = 0xA5; break;
				case 0x0491: ch = 0xB4; break;
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
				case 0x20AC: ch = 0x88; break;
				case 0x2116: ch = 0xB9; break;
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
}; // class CP1251

[Serializable]
public class ENCwindows_1251 : CP1251
{
	public ENCwindows_1251() : base() {}

}; // class ENCwindows_1251

}; // namespace I18N.Other
