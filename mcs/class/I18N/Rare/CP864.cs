/*
 * CP864.cs - Arabic (DOS) code page.
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

// Generated from "ibm-864.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.Rare
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP864 : ByteEncoding
{
	public CP864()
		: base(864, ToChars, "Arabic (DOS)",
		       "ibm864", "ibm864", "ibm864",
		       false, false, false, false, 1256)
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
		'\u007E', '\u001A', '\u00B0', '\u00B7', '\u2219', '\u221A', 
		'\u2592', '\u2500', '\u2502', '\u253C', '\u2524', '\u252C', 
		'\u251C', '\u2534', '\u2510', '\u250C', '\u2514', '\u2518', 
		'\u03B2', '\u221E', '\u03C6', '\u00B1', '\u00BD', '\u00BC', 
		'\u2248', '\u00AB', '\u00BB', '\uFEF7', '\uFEF8', '\u003F', 
		'\u003F', '\uFEFB', '\uFEFC', '\u200B', '\u00A0', '\u00AD', 
		'\uFE82', '\u00A3', '\u00A4', '\uFE84', '\u003F', '\u003F', 
		'\uFE8E', '\uFE8F', '\uFE95', '\uFE99', '\u060C', '\uFE9D', 
		'\uFEA1', '\uFEA5', '\u0660', '\u0661', '\u0662', '\u0663', 
		'\u0664', '\u0665', '\u0666', '\u0667', '\u0668', '\u0669', 
		'\uFED1', '\u061B', '\uFEB1', '\uFEB5', '\uFEB9', '\u061F', 
		'\u00A2', '\uFE80', '\uFE81', '\uFE83', '\uFE85', '\uFECA', 
		'\uFE8B', '\uFE8D', '\uFE91', '\uFE93', '\uFE97', '\uFE9B', 
		'\uFE9F', '\uFEA3', '\uFEA7', '\uFEA9', '\uFEAB', '\uFEAD', 
		'\uFEAF', '\uFEB3', '\uFEB7', '\uFEBB', '\uFEBF', '\uFEC3', 
		'\uFEC7', '\uFECB', '\uFECF', '\u00A6', '\u00AC', '\u00F7', 
		'\u00D7', '\uFEC9', '\u0640', '\uFED3', '\uFED7', '\uFEDB', 
		'\uFEDF', '\uFEE3', '\uFEE7', '\uFEEB', '\uFEED', '\uFEEF', 
		'\uFEF3', '\uFEBD', '\uFECC', '\uFECE', '\uFECD', '\uFEE1', 
		'\uFE7D', '\uFE7C', '\uFEE5', '\uFEE9', '\uFEEC', '\uFEF0', 
		'\uFEF2', '\uFED0', '\uFED5', '\uFEF5', '\uFEF6', '\uFEDD', 
		'\uFED9', '\uFEF1', '\u25A0', '\u003F', 
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
				case 0x00A0:
				case 0x00A3:
				case 0x00A4:
					break;
				case 0x001A: ch = 0x7F; break;
				case 0x001C: ch = 0x1A; break;
				case 0x007F: ch = 0x1C; break;
				case 0x00A2: ch = 0xC0; break;
				case 0x00A6: ch = 0xDB; break;
				case 0x00A7: ch = 0x15; break;
				case 0x00AB: ch = 0x97; break;
				case 0x00AC: ch = 0xDC; break;
				case 0x00AD: ch = 0xA1; break;
				case 0x00B0: ch = 0x80; break;
				case 0x00B1: ch = 0x93; break;
				case 0x00B6: ch = 0x14; break;
				case 0x00B7: ch = 0x81; break;
				case 0x00BB: ch = 0x98; break;
				case 0x00BC: ch = 0x95; break;
				case 0x00BD: ch = 0x94; break;
				case 0x00D7: ch = 0xDE; break;
				case 0x00F7: ch = 0xDD; break;
				case 0x03B2: ch = 0x90; break;
				case 0x03C6: ch = 0x92; break;
				case 0x060C: ch = 0xAC; break;
				case 0x061B: ch = 0xBB; break;
				case 0x061F: ch = 0xBF; break;
				case 0x0621:
				case 0x0622:
				case 0x0623:
				case 0x0624:
					ch -= 0x0560;
					break;
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
					ch -= 0x0560;
					break;
				case 0x064A: ch = 0xFD; break;
				case 0x0651: ch = 0xF1; break;
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
					ch -= 0x05B0;
					break;
				case 0x066A: ch = 0x25; break;
				case 0x066B: ch = 0x2C; break;
				case 0x066C: ch = 0x2E; break;
				case 0x066D: ch = 0x2A; break;
				case 0x200B: ch = 0x9F; break;
				case 0x203C: ch = 0x13; break;
				case 0x2190: ch = 0x1B; break;
				case 0x2191: ch = 0x18; break;
				case 0x2192: ch = 0x1A; break;
				case 0x2193: ch = 0x19; break;
				case 0x2194: ch = 0x1D; break;
				case 0x2195: ch = 0x12; break;
				case 0x21A8: ch = 0x17; break;
				case 0x2219: ch = 0x82; break;
				case 0x221A: ch = 0x83; break;
				case 0x221E: ch = 0x91; break;
				case 0x221F: ch = 0x1C; break;
				case 0x2248: ch = 0x96; break;
				case 0x2302: ch = 0x7F; break;
				case 0x2500: ch = 0x85; break;
				case 0x2502: ch = 0x86; break;
				case 0x250C: ch = 0x8D; break;
				case 0x2510: ch = 0x8C; break;
				case 0x2514: ch = 0x8E; break;
				case 0x2518: ch = 0x8F; break;
				case 0x251C: ch = 0x8A; break;
				case 0x2524: ch = 0x88; break;
				case 0x252C: ch = 0x89; break;
				case 0x2534: ch = 0x8B; break;
				case 0x253C: ch = 0x87; break;
				case 0x2550: ch = 0x05; break;
				case 0x2551: ch = 0x06; break;
				case 0x2554: ch = 0x0D; break;
				case 0x2557: ch = 0x0C; break;
				case 0x255A: ch = 0x0E; break;
				case 0x255D: ch = 0x0F; break;
				case 0x2560: ch = 0x0A; break;
				case 0x2563: ch = 0x08; break;
				case 0x2566: ch = 0x09; break;
				case 0x2569: ch = 0x0B; break;
				case 0x256C: ch = 0x07; break;
				case 0x2592: ch = 0x84; break;
				case 0x25A0: ch = 0xFE; break;
				case 0x25AC: ch = 0x16; break;
				case 0x25B2: ch = 0x1E; break;
				case 0x25BA: ch = 0x10; break;
				case 0x25BC: ch = 0x1F; break;
				case 0x25C4: ch = 0x11; break;
				case 0x263A: ch = 0x01; break;
				case 0x263C: ch = 0x04; break;
				case 0x266A: ch = 0x02; break;
				case 0x266C: ch = 0x03; break;
				case 0xFE7C: ch = 0xF1; break;
				case 0xFE7D: ch = 0xF0; break;
				case 0xFE80: ch = 0xC1; break;
				case 0xFE81: ch = 0xC2; break;
				case 0xFE82: ch = 0xA2; break;
				case 0xFE83: ch = 0xC3; break;
				case 0xFE84: ch = 0xA5; break;
				case 0xFE85: ch = 0xC4; break;
				case 0xFE86: ch = 0xC4; break;
				case 0xFE8B: ch = 0xC6; break;
				case 0xFE8C: ch = 0xC6; break;
				case 0xFE8D: ch = 0xC7; break;
				case 0xFE8E: ch = 0xA8; break;
				case 0xFE8F: ch = 0xA9; break;
				case 0xFE90: ch = 0xA9; break;
				case 0xFE91: ch = 0xC8; break;
				case 0xFE92: ch = 0xC8; break;
				case 0xFE93: ch = 0xC9; break;
				case 0xFE94: ch = 0xC9; break;
				case 0xFE95: ch = 0xAA; break;
				case 0xFE96: ch = 0xAA; break;
				case 0xFE97: ch = 0xCA; break;
				case 0xFE98: ch = 0xCA; break;
				case 0xFE99: ch = 0xAB; break;
				case 0xFE9A: ch = 0xAB; break;
				case 0xFE9B: ch = 0xCB; break;
				case 0xFE9C: ch = 0xCB; break;
				case 0xFE9D: ch = 0xAD; break;
				case 0xFE9E: ch = 0xAD; break;
				case 0xFE9F: ch = 0xCC; break;
				case 0xFEA0: ch = 0xCC; break;
				case 0xFEA1: ch = 0xAE; break;
				case 0xFEA2: ch = 0xAE; break;
				case 0xFEA3: ch = 0xCD; break;
				case 0xFEA4: ch = 0xCD; break;
				case 0xFEA5: ch = 0xAF; break;
				case 0xFEA6: ch = 0xAF; break;
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
				case 0xFEB1: ch = 0xBC; break;
				case 0xFEB2: ch = 0xBC; break;
				case 0xFEB3: ch = 0xD3; break;
				case 0xFEB4: ch = 0xD3; break;
				case 0xFEB5: ch = 0xBD; break;
				case 0xFEB6: ch = 0xBD; break;
				case 0xFEB7: ch = 0xD4; break;
				case 0xFEB8: ch = 0xD4; break;
				case 0xFEB9: ch = 0xBE; break;
				case 0xFEBA: ch = 0xBE; break;
				case 0xFEBB: ch = 0xD5; break;
				case 0xFEBC: ch = 0xD5; break;
				case 0xFEBD: ch = 0xEB; break;
				case 0xFEBE: ch = 0xEB; break;
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
				case 0xFEC9: ch = 0xDF; break;
				case 0xFECA: ch = 0xC5; break;
				case 0xFECB: ch = 0xD9; break;
				case 0xFECC: ch = 0xEC; break;
				case 0xFECD: ch = 0xEE; break;
				case 0xFECE: ch = 0xED; break;
				case 0xFECF: ch = 0xDA; break;
				case 0xFED0: ch = 0xF7; break;
				case 0xFED1: ch = 0xBA; break;
				case 0xFED2: ch = 0xBA; break;
				case 0xFED3: ch = 0xE1; break;
				case 0xFED4: ch = 0xE1; break;
				case 0xFED5: ch = 0xF8; break;
				case 0xFED6: ch = 0xF8; break;
				case 0xFED7: ch = 0xE2; break;
				case 0xFED8: ch = 0xE2; break;
				case 0xFED9: ch = 0xFC; break;
				case 0xFEDA: ch = 0xFC; break;
				case 0xFEDB: ch = 0xE3; break;
				case 0xFEDC: ch = 0xE3; break;
				case 0xFEDD: ch = 0xFB; break;
				case 0xFEDE: ch = 0xFB; break;
				case 0xFEDF: ch = 0xE4; break;
				case 0xFEE0: ch = 0xE4; break;
				case 0xFEE1: ch = 0xEF; break;
				case 0xFEE2: ch = 0xEF; break;
				case 0xFEE3: ch = 0xE5; break;
				case 0xFEE4: ch = 0xE5; break;
				case 0xFEE5: ch = 0xF2; break;
				case 0xFEE6: ch = 0xF2; break;
				case 0xFEE7: ch = 0xE6; break;
				case 0xFEE8: ch = 0xE6; break;
				case 0xFEE9: ch = 0xF3; break;
				case 0xFEEA: ch = 0xF3; break;
				case 0xFEEB: ch = 0xE7; break;
				case 0xFEEC: ch = 0xF4; break;
				case 0xFEED: ch = 0xE8; break;
				case 0xFEEE: ch = 0xE8; break;
				case 0xFEEF: ch = 0xE9; break;
				case 0xFEF0: ch = 0xF5; break;
				case 0xFEF1: ch = 0xFD; break;
				case 0xFEF2: ch = 0xF6; break;
				case 0xFEF3: ch = 0xEA; break;
				case 0xFEF4: ch = 0xEA; break;
				case 0xFEF5: ch = 0xF9; break;
				case 0xFEF6: ch = 0xFA; break;
				case 0xFEF7: ch = 0x99; break;
				case 0xFEF8: ch = 0x9A; break;
				case 0xFEFB: ch = 0x9D; break;
				case 0xFEFC: ch = 0x9E; break;
				case 0xFFE8: ch = 0x86; break;
				case 0xFFE9: ch = 0x1B; break;
				case 0xFFEA: ch = 0x18; break;
				case 0xFFEB: ch = 0x1A; break;
				case 0xFFEC: ch = 0x19; break;
				case 0xFFED: ch = 0xFE; break;
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
				case 0x00A0:
				case 0x00A3:
				case 0x00A4:
					break;
				case 0x001A: ch = 0x7F; break;
				case 0x001C: ch = 0x1A; break;
				case 0x007F: ch = 0x1C; break;
				case 0x00A2: ch = 0xC0; break;
				case 0x00A6: ch = 0xDB; break;
				case 0x00A7: ch = 0x15; break;
				case 0x00AB: ch = 0x97; break;
				case 0x00AC: ch = 0xDC; break;
				case 0x00AD: ch = 0xA1; break;
				case 0x00B0: ch = 0x80; break;
				case 0x00B1: ch = 0x93; break;
				case 0x00B6: ch = 0x14; break;
				case 0x00B7: ch = 0x81; break;
				case 0x00BB: ch = 0x98; break;
				case 0x00BC: ch = 0x95; break;
				case 0x00BD: ch = 0x94; break;
				case 0x00D7: ch = 0xDE; break;
				case 0x00F7: ch = 0xDD; break;
				case 0x03B2: ch = 0x90; break;
				case 0x03C6: ch = 0x92; break;
				case 0x060C: ch = 0xAC; break;
				case 0x061B: ch = 0xBB; break;
				case 0x061F: ch = 0xBF; break;
				case 0x0621:
				case 0x0622:
				case 0x0623:
				case 0x0624:
					ch -= 0x0560;
					break;
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
					ch -= 0x0560;
					break;
				case 0x064A: ch = 0xFD; break;
				case 0x0651: ch = 0xF1; break;
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
					ch -= 0x05B0;
					break;
				case 0x066A: ch = 0x25; break;
				case 0x066B: ch = 0x2C; break;
				case 0x066C: ch = 0x2E; break;
				case 0x066D: ch = 0x2A; break;
				case 0x200B: ch = 0x9F; break;
				case 0x203C: ch = 0x13; break;
				case 0x2190: ch = 0x1B; break;
				case 0x2191: ch = 0x18; break;
				case 0x2192: ch = 0x1A; break;
				case 0x2193: ch = 0x19; break;
				case 0x2194: ch = 0x1D; break;
				case 0x2195: ch = 0x12; break;
				case 0x21A8: ch = 0x17; break;
				case 0x2219: ch = 0x82; break;
				case 0x221A: ch = 0x83; break;
				case 0x221E: ch = 0x91; break;
				case 0x221F: ch = 0x1C; break;
				case 0x2248: ch = 0x96; break;
				case 0x2302: ch = 0x7F; break;
				case 0x2500: ch = 0x85; break;
				case 0x2502: ch = 0x86; break;
				case 0x250C: ch = 0x8D; break;
				case 0x2510: ch = 0x8C; break;
				case 0x2514: ch = 0x8E; break;
				case 0x2518: ch = 0x8F; break;
				case 0x251C: ch = 0x8A; break;
				case 0x2524: ch = 0x88; break;
				case 0x252C: ch = 0x89; break;
				case 0x2534: ch = 0x8B; break;
				case 0x253C: ch = 0x87; break;
				case 0x2550: ch = 0x05; break;
				case 0x2551: ch = 0x06; break;
				case 0x2554: ch = 0x0D; break;
				case 0x2557: ch = 0x0C; break;
				case 0x255A: ch = 0x0E; break;
				case 0x255D: ch = 0x0F; break;
				case 0x2560: ch = 0x0A; break;
				case 0x2563: ch = 0x08; break;
				case 0x2566: ch = 0x09; break;
				case 0x2569: ch = 0x0B; break;
				case 0x256C: ch = 0x07; break;
				case 0x2592: ch = 0x84; break;
				case 0x25A0: ch = 0xFE; break;
				case 0x25AC: ch = 0x16; break;
				case 0x25B2: ch = 0x1E; break;
				case 0x25BA: ch = 0x10; break;
				case 0x25BC: ch = 0x1F; break;
				case 0x25C4: ch = 0x11; break;
				case 0x263A: ch = 0x01; break;
				case 0x263C: ch = 0x04; break;
				case 0x266A: ch = 0x02; break;
				case 0x266C: ch = 0x03; break;
				case 0xFE7C: ch = 0xF1; break;
				case 0xFE7D: ch = 0xF0; break;
				case 0xFE80: ch = 0xC1; break;
				case 0xFE81: ch = 0xC2; break;
				case 0xFE82: ch = 0xA2; break;
				case 0xFE83: ch = 0xC3; break;
				case 0xFE84: ch = 0xA5; break;
				case 0xFE85: ch = 0xC4; break;
				case 0xFE86: ch = 0xC4; break;
				case 0xFE8B: ch = 0xC6; break;
				case 0xFE8C: ch = 0xC6; break;
				case 0xFE8D: ch = 0xC7; break;
				case 0xFE8E: ch = 0xA8; break;
				case 0xFE8F: ch = 0xA9; break;
				case 0xFE90: ch = 0xA9; break;
				case 0xFE91: ch = 0xC8; break;
				case 0xFE92: ch = 0xC8; break;
				case 0xFE93: ch = 0xC9; break;
				case 0xFE94: ch = 0xC9; break;
				case 0xFE95: ch = 0xAA; break;
				case 0xFE96: ch = 0xAA; break;
				case 0xFE97: ch = 0xCA; break;
				case 0xFE98: ch = 0xCA; break;
				case 0xFE99: ch = 0xAB; break;
				case 0xFE9A: ch = 0xAB; break;
				case 0xFE9B: ch = 0xCB; break;
				case 0xFE9C: ch = 0xCB; break;
				case 0xFE9D: ch = 0xAD; break;
				case 0xFE9E: ch = 0xAD; break;
				case 0xFE9F: ch = 0xCC; break;
				case 0xFEA0: ch = 0xCC; break;
				case 0xFEA1: ch = 0xAE; break;
				case 0xFEA2: ch = 0xAE; break;
				case 0xFEA3: ch = 0xCD; break;
				case 0xFEA4: ch = 0xCD; break;
				case 0xFEA5: ch = 0xAF; break;
				case 0xFEA6: ch = 0xAF; break;
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
				case 0xFEB1: ch = 0xBC; break;
				case 0xFEB2: ch = 0xBC; break;
				case 0xFEB3: ch = 0xD3; break;
				case 0xFEB4: ch = 0xD3; break;
				case 0xFEB5: ch = 0xBD; break;
				case 0xFEB6: ch = 0xBD; break;
				case 0xFEB7: ch = 0xD4; break;
				case 0xFEB8: ch = 0xD4; break;
				case 0xFEB9: ch = 0xBE; break;
				case 0xFEBA: ch = 0xBE; break;
				case 0xFEBB: ch = 0xD5; break;
				case 0xFEBC: ch = 0xD5; break;
				case 0xFEBD: ch = 0xEB; break;
				case 0xFEBE: ch = 0xEB; break;
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
				case 0xFEC9: ch = 0xDF; break;
				case 0xFECA: ch = 0xC5; break;
				case 0xFECB: ch = 0xD9; break;
				case 0xFECC: ch = 0xEC; break;
				case 0xFECD: ch = 0xEE; break;
				case 0xFECE: ch = 0xED; break;
				case 0xFECF: ch = 0xDA; break;
				case 0xFED0: ch = 0xF7; break;
				case 0xFED1: ch = 0xBA; break;
				case 0xFED2: ch = 0xBA; break;
				case 0xFED3: ch = 0xE1; break;
				case 0xFED4: ch = 0xE1; break;
				case 0xFED5: ch = 0xF8; break;
				case 0xFED6: ch = 0xF8; break;
				case 0xFED7: ch = 0xE2; break;
				case 0xFED8: ch = 0xE2; break;
				case 0xFED9: ch = 0xFC; break;
				case 0xFEDA: ch = 0xFC; break;
				case 0xFEDB: ch = 0xE3; break;
				case 0xFEDC: ch = 0xE3; break;
				case 0xFEDD: ch = 0xFB; break;
				case 0xFEDE: ch = 0xFB; break;
				case 0xFEDF: ch = 0xE4; break;
				case 0xFEE0: ch = 0xE4; break;
				case 0xFEE1: ch = 0xEF; break;
				case 0xFEE2: ch = 0xEF; break;
				case 0xFEE3: ch = 0xE5; break;
				case 0xFEE4: ch = 0xE5; break;
				case 0xFEE5: ch = 0xF2; break;
				case 0xFEE6: ch = 0xF2; break;
				case 0xFEE7: ch = 0xE6; break;
				case 0xFEE8: ch = 0xE6; break;
				case 0xFEE9: ch = 0xF3; break;
				case 0xFEEA: ch = 0xF3; break;
				case 0xFEEB: ch = 0xE7; break;
				case 0xFEEC: ch = 0xF4; break;
				case 0xFEED: ch = 0xE8; break;
				case 0xFEEE: ch = 0xE8; break;
				case 0xFEEF: ch = 0xE9; break;
				case 0xFEF0: ch = 0xF5; break;
				case 0xFEF1: ch = 0xFD; break;
				case 0xFEF2: ch = 0xF6; break;
				case 0xFEF3: ch = 0xEA; break;
				case 0xFEF4: ch = 0xEA; break;
				case 0xFEF5: ch = 0xF9; break;
				case 0xFEF6: ch = 0xFA; break;
				case 0xFEF7: ch = 0x99; break;
				case 0xFEF8: ch = 0x9A; break;
				case 0xFEFB: ch = 0x9D; break;
				case 0xFEFC: ch = 0x9E; break;
				case 0xFFE8: ch = 0x86; break;
				case 0xFFE9: ch = 0x1B; break;
				case 0xFFEA: ch = 0x18; break;
				case 0xFFEB: ch = 0x1A; break;
				case 0xFFEC: ch = 0x19; break;
				case 0xFFED: ch = 0xFE; break;
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

}; // class CP864

[Serializable]
public class ENCibm864 : CP864
{
	public ENCibm864() : base() {}

}; // class ENCibm864

}; // namespace I18N.Rare
