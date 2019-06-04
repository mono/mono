/*
 * CP708.cs - Arabic (ASMO 708) code page.
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

// Generated from "ibm-1089.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.Rare
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP708 : ByteEncoding
{
	public CP708()
		: base(708, ToChars, "Arabic (ASMO 708)",
		       "iso-8859-6", "asmo-708", "asmo-708",
		       false, false, false, false, 1256)
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
		'\u009C', '\u009D', '\u009E', '\u009F', '\u00A0', '\u003F', 
		'\u003F', '\u003F', '\u00A4', '\u003F', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u003F', '\u003F', '\u060C', '\u00AD', 
		'\u003F', '\u003F', '\u003F', '\u003F', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u003F', '\u003F', '\u003F', '\u003F', 
		'\u003F', '\u061B', '\u003F', '\u003F', '\u003F', '\u061F', 
		'\u003F', '\u0621', '\u0622', '\u0623', '\u0624', '\u0625', 
		'\u0626', '\u0627', '\u0628', '\u0629', '\u062A', '\u062B', 
		'\u062C', '\u062D', '\u062E', '\u062F', '\u0630', '\u0631', 
		'\u0632', '\u0633', '\u0634', '\u0635', '\u0636', '\u0637', 
		'\u0638', '\u0639', '\u063A', '\u003F', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u0640', '\u0641', '\u0642', '\u0643', 
		'\u0644', '\u0645', '\u0646', '\u0647', '\u0648', '\u0649', 
		'\u064A', '\u064B', '\u064C', '\u064D', '\u064E', '\u064F', 
		'\u0650', '\u0651', '\u0652', '\u003F', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u003F', '\u003F', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u003F', '\u003F', 
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
				case 0x00AD:
					break;
				case 0x060C: ch = 0xAC; break;
				case 0x061B: ch = 0xBB; break;
				case 0x061F: ch = 0xBF; break;
				case 0x0621:
				case 0x0622:
				case 0x0623:
				case 0x0624:
				case 0x0625:
				case 0x0626:
				case 0x0627:
				case 0x0628:
				case 0x0629:
				case 0x062A:
				case 0x062B:
				case 0x062C:
				case 0x062D:
				case 0x062E:
				case 0x062F:
				case 0x0630:
				case 0x0631:
				case 0x0632:
				case 0x0633:
				case 0x0634:
				case 0x0635:
				case 0x0636:
				case 0x0637:
				case 0x0638:
				case 0x0639:
				case 0x063A:
					ch -= 0x0560;
					break;
				case 0x0640:
				case 0x0641:
				case 0x0642:
				case 0x0643:
				case 0x0644:
				case 0x0645:
				case 0x0646:
				case 0x0647:
				case 0x0648:
				case 0x0649:
				case 0x064A:
				case 0x064B:
				case 0x064C:
				case 0x064D:
				case 0x064E:
				case 0x064F:
				case 0x0650:
				case 0x0651:
				case 0x0652:
					ch -= 0x0560;
					break;
				case 0x0660:
				case 0x0661:
				case 0x0662:
				case 0x0663:
				case 0x0664:
				case 0x0665:
				case 0x0666:
				case 0x0667:
				case 0x0668:
				case 0x0669:
					ch -= 0x0630;
					break;
				case 0x066A: ch = 0x25; break;
				case 0x066B: ch = 0x2C; break;
				case 0x066C: ch = 0x2E; break;
				case 0x066D: ch = 0x2A; break;
				case 0xFE70: ch = 0xEB; break;
				case 0xFE71: ch = 0xEB; break;
				case 0xFE72: ch = 0xEC; break;
				case 0xFE74: ch = 0xED; break;
				case 0xFE76: ch = 0xEE; break;
				case 0xFE77: ch = 0xEE; break;
				case 0xFE78: ch = 0xEF; break;
				case 0xFE79: ch = 0xEF; break;
				case 0xFE7A: ch = 0xF0; break;
				case 0xFE7B: ch = 0xF0; break;
				case 0xFE7C: ch = 0xF1; break;
				case 0xFE7D: ch = 0xF1; break;
				case 0xFE7E: ch = 0xF2; break;
				case 0xFE7F: ch = 0xF2; break;
				case 0xFE80: ch = 0xC1; break;
				case 0xFE81: ch = 0xC2; break;
				case 0xFE82: ch = 0xC2; break;
				case 0xFE83: ch = 0xC3; break;
				case 0xFE84: ch = 0xC3; break;
				case 0xFE85: ch = 0xC4; break;
				case 0xFE86: ch = 0xC4; break;
				case 0xFE87: ch = 0xC5; break;
				case 0xFE88: ch = 0xC5; break;
				case 0xFE89: ch = 0xC6; break;
				case 0xFE8A: ch = 0xC6; break;
				case 0xFE8B: ch = 0xC6; break;
				case 0xFE8C: ch = 0xC6; break;
				case 0xFE8D: ch = 0xC7; break;
				case 0xFE8E: ch = 0xC7; break;
				case 0xFE8F: ch = 0xC8; break;
				case 0xFE90: ch = 0xC8; break;
				case 0xFE91: ch = 0xC8; break;
				case 0xFE92: ch = 0xC8; break;
				case 0xFE93: ch = 0xC9; break;
				case 0xFE94: ch = 0xC9; break;
				case 0xFE95: ch = 0xCA; break;
				case 0xFE96: ch = 0xCA; break;
				case 0xFE97: ch = 0xCA; break;
				case 0xFE98: ch = 0xCA; break;
				case 0xFE99: ch = 0xCB; break;
				case 0xFE9A: ch = 0xCB; break;
				case 0xFE9B: ch = 0xCB; break;
				case 0xFE9C: ch = 0xCB; break;
				case 0xFE9D: ch = 0xCC; break;
				case 0xFE9E: ch = 0xCC; break;
				case 0xFE9F: ch = 0xCC; break;
				case 0xFEA0: ch = 0xCC; break;
				case 0xFEA1: ch = 0xCD; break;
				case 0xFEA2: ch = 0xCD; break;
				case 0xFEA3: ch = 0xCD; break;
				case 0xFEA4: ch = 0xCD; break;
				case 0xFEA5: ch = 0xCE; break;
				case 0xFEA6: ch = 0xCE; break;
				case 0xFEA7: ch = 0xCE; break;
				case 0xFEA8: ch = 0xCE; break;
				case 0xFEA9: ch = 0xCF; break;
				case 0xFEAA: ch = 0xCF; break;
				case 0xFEAB: ch = 0xD0; break;
				case 0xFEAC: ch = 0xD0; break;
				case 0xFEAD: ch = 0xD1; break;
				case 0xFEAE: ch = 0xD1; break;
				case 0xFEAF: ch = 0xD2; break;
				case 0xFEB0: ch = 0xD2; break;
				case 0xFEB1: ch = 0xD3; break;
				case 0xFEB2: ch = 0xD3; break;
				case 0xFEB3: ch = 0xD3; break;
				case 0xFEB4: ch = 0xD3; break;
				case 0xFEB5: ch = 0xD4; break;
				case 0xFEB6: ch = 0xD4; break;
				case 0xFEB7: ch = 0xD4; break;
				case 0xFEB8: ch = 0xD4; break;
				case 0xFEB9: ch = 0xD5; break;
				case 0xFEBA: ch = 0xD5; break;
				case 0xFEBB: ch = 0xD5; break;
				case 0xFEBC: ch = 0xD5; break;
				case 0xFEBD: ch = 0xD6; break;
				case 0xFEBE: ch = 0xD6; break;
				case 0xFEBF: ch = 0xD6; break;
				case 0xFEC0: ch = 0xD6; break;
				case 0xFEC1: ch = 0xD7; break;
				case 0xFEC2: ch = 0xD7; break;
				case 0xFEC3: ch = 0xD7; break;
				case 0xFEC4: ch = 0xD7; break;
				case 0xFEC5: ch = 0xD8; break;
				case 0xFEC6: ch = 0xD8; break;
				case 0xFEC7: ch = 0xD8; break;
				case 0xFEC8: ch = 0xD8; break;
				case 0xFEC9: ch = 0xD9; break;
				case 0xFECA: ch = 0xD9; break;
				case 0xFECB: ch = 0xD9; break;
				case 0xFECC: ch = 0xD9; break;
				case 0xFECD: ch = 0xDA; break;
				case 0xFECE: ch = 0xDA; break;
				case 0xFECF: ch = 0xDA; break;
				case 0xFED0: ch = 0xDA; break;
				case 0xFED1: ch = 0xE1; break;
				case 0xFED2: ch = 0xE1; break;
				case 0xFED3: ch = 0xE1; break;
				case 0xFED4: ch = 0xE1; break;
				case 0xFED5: ch = 0xE2; break;
				case 0xFED6: ch = 0xE2; break;
				case 0xFED7: ch = 0xE2; break;
				case 0xFED8: ch = 0xE2; break;
				case 0xFED9: ch = 0xE3; break;
				case 0xFEDA: ch = 0xE3; break;
				case 0xFEDB: ch = 0xE3; break;
				case 0xFEDC: ch = 0xE3; break;
				case 0xFEDD: ch = 0xE4; break;
				case 0xFEDE: ch = 0xE4; break;
				case 0xFEDF: ch = 0xE4; break;
				case 0xFEE0: ch = 0xE4; break;
				case 0xFEE1: ch = 0xE5; break;
				case 0xFEE2: ch = 0xE5; break;
				case 0xFEE3: ch = 0xE5; break;
				case 0xFEE4: ch = 0xE5; break;
				case 0xFEE5: ch = 0xE6; break;
				case 0xFEE6: ch = 0xE6; break;
				case 0xFEE7: ch = 0xE6; break;
				case 0xFEE8: ch = 0xE6; break;
				case 0xFEE9: ch = 0xE7; break;
				case 0xFEEA: ch = 0xE7; break;
				case 0xFEEB: ch = 0xE7; break;
				case 0xFEEC: ch = 0xE7; break;
				case 0xFEED: ch = 0xE8; break;
				case 0xFEEE: ch = 0xE8; break;
				case 0xFEEF: ch = 0xE9; break;
				case 0xFEF0: ch = 0xE9; break;
				case 0xFEF1: ch = 0xEA; break;
				case 0xFEF2: ch = 0xEA; break;
				case 0xFEF3: ch = 0xEA; break;
				case 0xFEF4: ch = 0xEA; break;
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
}; // class CP708

[Serializable]
public class ENCasmo_708 : CP708
{
	public ENCasmo_708() : base() {}

}; // class ENCasmo_708

}; // namespace I18N.Rare
