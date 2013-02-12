/*
 * CP28597.cs - Greek (ISO) code page.
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

// Generated from "ibm-4909.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.West
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP28597 : ByteEncoding
{
	public CP28597()
		: base(28597, ToChars, "Greek (ISO)",
		       "iso-8859-7", "iso-8859-7", "iso-8859-7",
		       true, true, true, true, 1253)
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
		'\u009C', '\u009D', '\u009E', '\u009F', '\u00A0', '\u2018', 
		'\u2019', '\u00A3', '\u20AC', '\u003F', '\u00A6', '\u00A7', 
		'\u00A8', '\u00A9', '\u003F', '\u00AB', '\u00AC', '\u00AD', 
		'\u003F', '\u2015', '\u00B0', '\u00B1', '\u00B2', '\u00B3', 
		'\u00B4', '\u0385', '\u0386', '\u0387', '\u0388', '\u0389', 
		'\u038A', '\u00BB', '\u038C', '\u00BD', '\u038E', '\u038F', 
		'\u0390', '\u0391', '\u0392', '\u0393', '\u0394', '\u0395', 
		'\u0396', '\u0397', '\u0398', '\u0399', '\u039A', '\u039B', 
		'\u039C', '\u039D', '\u039E', '\u039F', '\u03A0', '\u03A1', 
		'\u003F', '\u03A3', '\u03A4', '\u03A5', '\u03A6', '\u03A7', 
		'\u03A8', '\u03A9', '\u03AA', '\u03AB', '\u03AC', '\u03AD', 
		'\u03AE', '\u03AF', '\u03B0', '\u03B1', '\u03B2', '\u03B3', 
		'\u03B4', '\u03B5', '\u03B6', '\u03B7', '\u03B8', '\u03B9', 
		'\u03BA', '\u03BB', '\u03BC', '\u03BD', '\u03BE', '\u03BF', 
		'\u03C0', '\u03C1', '\u03C2', '\u03C3', '\u03C4', '\u03C5', 
		'\u03C6', '\u03C7', '\u03C8', '\u03C9', '\u03CA', '\u03CB', 
		'\u03CC', '\u03CD', '\u03CE', '\u003F', 
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
				case 0x00A3:
				case 0x00A6:
				case 0x00A7:
				case 0x00A8:
				case 0x00A9:
				case 0x00AB:
				case 0x00AC:
				case 0x00AD:
				case 0x00B0:
				case 0x00B1:
				case 0x00B2:
				case 0x00B3:
				case 0x00B4:
				case 0x00B7:
				case 0x00BB:
				case 0x00BD:
					break;
				case 0x0385:
				case 0x0386:
				case 0x0387:
				case 0x0388:
				case 0x0389:
				case 0x038A:
					ch -= 0x02D0;
					break;
				case 0x038C: ch = 0xBC; break;
				case 0x038E:
				case 0x038F:
				case 0x0390:
				case 0x0391:
				case 0x0392:
				case 0x0393:
				case 0x0394:
				case 0x0395:
				case 0x0396:
				case 0x0397:
				case 0x0398:
				case 0x0399:
				case 0x039A:
				case 0x039B:
				case 0x039C:
				case 0x039D:
				case 0x039E:
				case 0x039F:
				case 0x03A0:
				case 0x03A1:
					ch -= 0x02D0;
					break;
				case 0x03A3:
				case 0x03A4:
				case 0x03A5:
				case 0x03A6:
				case 0x03A7:
				case 0x03A8:
				case 0x03A9:
				case 0x03AA:
				case 0x03AB:
				case 0x03AC:
				case 0x03AD:
				case 0x03AE:
				case 0x03AF:
				case 0x03B0:
				case 0x03B1:
				case 0x03B2:
				case 0x03B3:
				case 0x03B4:
				case 0x03B5:
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
				case 0x03C2:
				case 0x03C3:
				case 0x03C4:
				case 0x03C5:
				case 0x03C6:
				case 0x03C7:
				case 0x03C8:
				case 0x03C9:
				case 0x03CA:
				case 0x03CB:
				case 0x03CC:
				case 0x03CD:
				case 0x03CE:
					ch -= 0x02D0;
					break;
				case 0x03D5: ch = 0xF6; break;
				case 0x2015: ch = 0xAF; break;
				case 0x2018: ch = 0xA1; break;
				case 0x2019: ch = 0xA2; break;
				case 0x20AC: ch = 0xA4; break;
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
				case 0x00A3:
				case 0x00A6:
				case 0x00A7:
				case 0x00A8:
				case 0x00A9:
				case 0x00AB:
				case 0x00AC:
				case 0x00AD:
				case 0x00B0:
				case 0x00B1:
				case 0x00B2:
				case 0x00B3:
				case 0x00B4:
				case 0x00B7:
				case 0x00BB:
				case 0x00BD:
					break;
				case 0x0385:
				case 0x0386:
				case 0x0387:
				case 0x0388:
				case 0x0389:
				case 0x038A:
					ch -= 0x02D0;
					break;
				case 0x038C: ch = 0xBC; break;
				case 0x038E:
				case 0x038F:
				case 0x0390:
				case 0x0391:
				case 0x0392:
				case 0x0393:
				case 0x0394:
				case 0x0395:
				case 0x0396:
				case 0x0397:
				case 0x0398:
				case 0x0399:
				case 0x039A:
				case 0x039B:
				case 0x039C:
				case 0x039D:
				case 0x039E:
				case 0x039F:
				case 0x03A0:
				case 0x03A1:
					ch -= 0x02D0;
					break;
				case 0x03A3:
				case 0x03A4:
				case 0x03A5:
				case 0x03A6:
				case 0x03A7:
				case 0x03A8:
				case 0x03A9:
				case 0x03AA:
				case 0x03AB:
				case 0x03AC:
				case 0x03AD:
				case 0x03AE:
				case 0x03AF:
				case 0x03B0:
				case 0x03B1:
				case 0x03B2:
				case 0x03B3:
				case 0x03B4:
				case 0x03B5:
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
				case 0x03C2:
				case 0x03C3:
				case 0x03C4:
				case 0x03C5:
				case 0x03C6:
				case 0x03C7:
				case 0x03C8:
				case 0x03C9:
				case 0x03CA:
				case 0x03CB:
				case 0x03CC:
				case 0x03CD:
				case 0x03CE:
					ch -= 0x02D0;
					break;
				case 0x03D5: ch = 0xF6; break;
				case 0x2015: ch = 0xAF; break;
				case 0x2018: ch = 0xA1; break;
				case 0x2019: ch = 0xA2; break;
				case 0x20AC: ch = 0xA4; break;
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

}; // class CP28597

[Serializable]
public class ENCiso_8859_7 : CP28597
{
	public ENCiso_8859_7() : base() {}

}; // class ENCiso_8859_7

}; // namespace I18N.West
