/*
 * CP1256.cs - Arabic (Windows) code page.
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

// Generated from "ibm-5352.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.MidEast
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP1256 : ByteEncoding
{
	public CP1256()
		: base(1256, ToChars, "Arabic (Windows)",
		       "windows-1256", "windows-1256", "windows-1256",
		       true, true, true, true, 1256)
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
		'\u007E', '\u007F', '\u20AC', '\u067E', '\u201A', '\u0192', 
		'\u201E', '\u2026', '\u2020', '\u2021', '\u02C6', '\u2030', 
		'\u0679', '\u2039', '\u0152', '\u0686', '\u0698', '\u0688', 
		'\u06AF', '\u2018', '\u2019', '\u201C', '\u201D', '\u2022', 
		'\u2013', '\u2014', '\u06A9', '\u2122', '\u0691', '\u203A', 
		'\u0153', '\u200C', '\u200D', '\u06BA', '\u00A0', '\u060C', 
		'\u00A2', '\u00A3', '\u00A4', '\u00A5', '\u00A6', '\u00A7', 
		'\u00A8', '\u00A9', '\u06BE', '\u00AB', '\u00AC', '\u00AD', 
		'\u00AE', '\u00AF', '\u00B0', '\u00B1', '\u00B2', '\u00B3', 
		'\u00B4', '\u00B5', '\u00B6', '\u00B7', '\u00B8', '\u00B9', 
		'\u061B', '\u00BB', '\u00BC', '\u00BD', '\u00BE', '\u061F', 
		'\u06C1', '\u0621', '\u0622', '\u0623', '\u0624', '\u0625', 
		'\u0626', '\u0627', '\u0628', '\u0629', '\u062A', '\u062B', 
		'\u062C', '\u062D', '\u062E', '\u062F', '\u0630', '\u0631', 
		'\u0632', '\u0633', '\u0634', '\u0635', '\u0636', '\u00D7', 
		'\u0637', '\u0638', '\u0639', '\u063A', '\u0640', '\u0641', 
		'\u0642', '\u0643', '\u00E0', '\u0644', '\u00E2', '\u0645', 
		'\u0646', '\u0647', '\u0648', '\u00E7', '\u00E8', '\u00E9', 
		'\u00EA', '\u00EB', '\u0649', '\u064A', '\u00EE', '\u00EF', 
		'\u064B', '\u064C', '\u064D', '\u064E', '\u00F4', '\u064F', 
		'\u0650', '\u00F7', '\u0651', '\u00F9', '\u0652', '\u00FB', 
		'\u00FC', '\u200E', '\u200F', '\u06D2', 
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
				case 0x00A0:
				case 0x00A2:
				case 0x00A3:
				case 0x00A4:
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
				case 0x00D7:
				case 0x00E0:
				case 0x00E2:
				case 0x00E7:
				case 0x00E8:
				case 0x00E9:
				case 0x00EA:
				case 0x00EB:
				case 0x00EE:
				case 0x00EF:
				case 0x00F4:
				case 0x00F7:
				case 0x00F9:
				case 0x00FB:
				case 0x00FC:
					break;
				case 0x0152: ch = 0x8C; break;
				case 0x0153: ch = 0x9C; break;
				case 0x0192: ch = 0x83; break;
				case 0x02C6: ch = 0x88; break;
				case 0x060C: ch = 0xA1; break;
				case 0x061B: ch = 0xBA; break;
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
					ch -= 0x0560;
					break;
				case 0x0637:
				case 0x0638:
				case 0x0639:
				case 0x063A:
					ch -= 0x055F;
					break;
				case 0x0640:
				case 0x0641:
				case 0x0642:
				case 0x0643:
					ch -= 0x0564;
					break;
				case 0x0644: ch = 0xE1; break;
				case 0x0645:
				case 0x0646:
				case 0x0647:
				case 0x0648:
					ch -= 0x0562;
					break;
				case 0x0649: ch = 0xEC; break;
				case 0x064A: ch = 0xED; break;
				case 0x064B:
				case 0x064C:
				case 0x064D:
				case 0x064E:
					ch -= 0x055B;
					break;
				case 0x064F: ch = 0xF5; break;
				case 0x0650: ch = 0xF6; break;
				case 0x0651: ch = 0xF8; break;
				case 0x0652: ch = 0xFA; break;
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
				case 0x066B: ch = 0x2C; break;
				case 0x066C: ch = 0x2E; break;
				case 0x0679: ch = 0x8A; break;
				case 0x067E: ch = 0x81; break;
				case 0x0686: ch = 0x8D; break;
				case 0x0688: ch = 0x8F; break;
				case 0x0691: ch = 0x9A; break;
				case 0x0698: ch = 0x8E; break;
				case 0x06A9: ch = 0x98; break;
				case 0x06AF: ch = 0x90; break;
				case 0x06BA: ch = 0x9F; break;
				case 0x06BE: ch = 0xAA; break;
				case 0x06C1: ch = 0xC0; break;
				case 0x06D2: ch = 0xFF; break;
				case 0x06F0:
				case 0x06F1:
				case 0x06F2:
				case 0x06F3:
				case 0x06F4:
				case 0x06F5:
				case 0x06F6:
				case 0x06F7:
				case 0x06F8:
				case 0x06F9:
					ch -= 0x06C0;
					break;
				case 0x200C: ch = 0x9D; break;
				case 0x200D: ch = 0x9E; break;
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
				case 0x20AC: ch = 0x80; break;
				case 0x2122: ch = 0x99; break;
				case 0xFB56: ch = 0x81; break;
				case 0xFB58: ch = 0x81; break;
				case 0xFB66: ch = 0x8A; break;
				case 0xFB68: ch = 0x8A; break;
				case 0xFB7A: ch = 0x8D; break;
				case 0xFB7C: ch = 0x8D; break;
				case 0xFB88: ch = 0x8F; break;
				case 0xFB8A: ch = 0x8E; break;
				case 0xFB8C: ch = 0x9A; break;
				case 0xFB8E: ch = 0x98; break;
				case 0xFB90: ch = 0x98; break;
				case 0xFB92: ch = 0x90; break;
				case 0xFB94: ch = 0x90; break;
				case 0xFB9E: ch = 0x9F; break;
				case 0xFBA6: ch = 0xC0; break;
				case 0xFBA8: ch = 0xC0; break;
				case 0xFBAA: ch = 0xAA; break;
				case 0xFBAC: ch = 0xAA; break;
				case 0xFBAE: ch = 0xFF; break;
				case 0xFE70: ch = 0xF0; break;
				case 0xFE71: ch = 0xF0; break;
				case 0xFE72: ch = 0xF1; break;
				case 0xFE74: ch = 0xF2; break;
				case 0xFE76: ch = 0xF3; break;
				case 0xFE77: ch = 0xF3; break;
				case 0xFE78: ch = 0xF5; break;
				case 0xFE79: ch = 0xF5; break;
				case 0xFE7A: ch = 0xF6; break;
				case 0xFE7B: ch = 0xF6; break;
				case 0xFE7C: ch = 0xF8; break;
				case 0xFE7D: ch = 0xF8; break;
				case 0xFE7E: ch = 0xFA; break;
				case 0xFE7F: ch = 0xFA; break;
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
				case 0xFEC1: ch = 0xD8; break;
				case 0xFEC2: ch = 0xD8; break;
				case 0xFEC3: ch = 0xD8; break;
				case 0xFEC4: ch = 0xD8; break;
				case 0xFEC5: ch = 0xD9; break;
				case 0xFEC6: ch = 0xD9; break;
				case 0xFEC7: ch = 0xD9; break;
				case 0xFEC8: ch = 0xD9; break;
				case 0xFEC9: ch = 0xDA; break;
				case 0xFECA: ch = 0xDA; break;
				case 0xFECB: ch = 0xDA; break;
				case 0xFECC: ch = 0xDA; break;
				case 0xFECD: ch = 0xDB; break;
				case 0xFECE: ch = 0xDB; break;
				case 0xFECF: ch = 0xDB; break;
				case 0xFED0: ch = 0xDB; break;
				case 0xFED1: ch = 0xDD; break;
				case 0xFED2: ch = 0xDD; break;
				case 0xFED3: ch = 0xDD; break;
				case 0xFED4: ch = 0xDD; break;
				case 0xFED5: ch = 0xDE; break;
				case 0xFED6: ch = 0xDE; break;
				case 0xFED7: ch = 0xDE; break;
				case 0xFED8: ch = 0xDE; break;
				case 0xFED9: ch = 0xDF; break;
				case 0xFEDA: ch = 0xDF; break;
				case 0xFEDB: ch = 0xDF; break;
				case 0xFEDC: ch = 0xDF; break;
				case 0xFEDD: ch = 0xE1; break;
				case 0xFEDE: ch = 0xE1; break;
				case 0xFEDF: ch = 0xE1; break;
				case 0xFEE0: ch = 0xE1; break;
				case 0xFEE1: ch = 0xE3; break;
				case 0xFEE2: ch = 0xE3; break;
				case 0xFEE3: ch = 0xE3; break;
				case 0xFEE4: ch = 0xE3; break;
				case 0xFEE5: ch = 0xE4; break;
				case 0xFEE6: ch = 0xE4; break;
				case 0xFEE7: ch = 0xE4; break;
				case 0xFEE8: ch = 0xE4; break;
				case 0xFEE9: ch = 0xE5; break;
				case 0xFEEA: ch = 0xE5; break;
				case 0xFEEB: ch = 0xE5; break;
				case 0xFEEC: ch = 0xE5; break;
				case 0xFEED: ch = 0xE6; break;
				case 0xFEEE: ch = 0xE6; break;
				case 0xFEEF: ch = 0xEC; break;
				case 0xFEF0: ch = 0xEC; break;
				case 0xFEF1: ch = 0xED; break;
				case 0xFEF2: ch = 0xED; break;
				case 0xFEF3: ch = 0xED; break;
				case 0xFEF4: ch = 0xED; break;
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
				case 0x00A0:
				case 0x00A2:
				case 0x00A3:
				case 0x00A4:
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
				case 0x00D7:
				case 0x00E0:
				case 0x00E2:
				case 0x00E7:
				case 0x00E8:
				case 0x00E9:
				case 0x00EA:
				case 0x00EB:
				case 0x00EE:
				case 0x00EF:
				case 0x00F4:
				case 0x00F7:
				case 0x00F9:
				case 0x00FB:
				case 0x00FC:
					break;
				case 0x0152: ch = 0x8C; break;
				case 0x0153: ch = 0x9C; break;
				case 0x0192: ch = 0x83; break;
				case 0x02C6: ch = 0x88; break;
				case 0x060C: ch = 0xA1; break;
				case 0x061B: ch = 0xBA; break;
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
					ch -= 0x0560;
					break;
				case 0x0637:
				case 0x0638:
				case 0x0639:
				case 0x063A:
					ch -= 0x055F;
					break;
				case 0x0640:
				case 0x0641:
				case 0x0642:
				case 0x0643:
					ch -= 0x0564;
					break;
				case 0x0644: ch = 0xE1; break;
				case 0x0645:
				case 0x0646:
				case 0x0647:
				case 0x0648:
					ch -= 0x0562;
					break;
				case 0x0649: ch = 0xEC; break;
				case 0x064A: ch = 0xED; break;
				case 0x064B:
				case 0x064C:
				case 0x064D:
				case 0x064E:
					ch -= 0x055B;
					break;
				case 0x064F: ch = 0xF5; break;
				case 0x0650: ch = 0xF6; break;
				case 0x0651: ch = 0xF8; break;
				case 0x0652: ch = 0xFA; break;
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
				case 0x066B: ch = 0x2C; break;
				case 0x066C: ch = 0x2E; break;
				case 0x0679: ch = 0x8A; break;
				case 0x067E: ch = 0x81; break;
				case 0x0686: ch = 0x8D; break;
				case 0x0688: ch = 0x8F; break;
				case 0x0691: ch = 0x9A; break;
				case 0x0698: ch = 0x8E; break;
				case 0x06A9: ch = 0x98; break;
				case 0x06AF: ch = 0x90; break;
				case 0x06BA: ch = 0x9F; break;
				case 0x06BE: ch = 0xAA; break;
				case 0x06C1: ch = 0xC0; break;
				case 0x06D2: ch = 0xFF; break;
				case 0x06F0:
				case 0x06F1:
				case 0x06F2:
				case 0x06F3:
				case 0x06F4:
				case 0x06F5:
				case 0x06F6:
				case 0x06F7:
				case 0x06F8:
				case 0x06F9:
					ch -= 0x06C0;
					break;
				case 0x200C: ch = 0x9D; break;
				case 0x200D: ch = 0x9E; break;
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
				case 0x20AC: ch = 0x80; break;
				case 0x2122: ch = 0x99; break;
				case 0xFB56: ch = 0x81; break;
				case 0xFB58: ch = 0x81; break;
				case 0xFB66: ch = 0x8A; break;
				case 0xFB68: ch = 0x8A; break;
				case 0xFB7A: ch = 0x8D; break;
				case 0xFB7C: ch = 0x8D; break;
				case 0xFB88: ch = 0x8F; break;
				case 0xFB8A: ch = 0x8E; break;
				case 0xFB8C: ch = 0x9A; break;
				case 0xFB8E: ch = 0x98; break;
				case 0xFB90: ch = 0x98; break;
				case 0xFB92: ch = 0x90; break;
				case 0xFB94: ch = 0x90; break;
				case 0xFB9E: ch = 0x9F; break;
				case 0xFBA6: ch = 0xC0; break;
				case 0xFBA8: ch = 0xC0; break;
				case 0xFBAA: ch = 0xAA; break;
				case 0xFBAC: ch = 0xAA; break;
				case 0xFBAE: ch = 0xFF; break;
				case 0xFE70: ch = 0xF0; break;
				case 0xFE71: ch = 0xF0; break;
				case 0xFE72: ch = 0xF1; break;
				case 0xFE74: ch = 0xF2; break;
				case 0xFE76: ch = 0xF3; break;
				case 0xFE77: ch = 0xF3; break;
				case 0xFE78: ch = 0xF5; break;
				case 0xFE79: ch = 0xF5; break;
				case 0xFE7A: ch = 0xF6; break;
				case 0xFE7B: ch = 0xF6; break;
				case 0xFE7C: ch = 0xF8; break;
				case 0xFE7D: ch = 0xF8; break;
				case 0xFE7E: ch = 0xFA; break;
				case 0xFE7F: ch = 0xFA; break;
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
				case 0xFEC1: ch = 0xD8; break;
				case 0xFEC2: ch = 0xD8; break;
				case 0xFEC3: ch = 0xD8; break;
				case 0xFEC4: ch = 0xD8; break;
				case 0xFEC5: ch = 0xD9; break;
				case 0xFEC6: ch = 0xD9; break;
				case 0xFEC7: ch = 0xD9; break;
				case 0xFEC8: ch = 0xD9; break;
				case 0xFEC9: ch = 0xDA; break;
				case 0xFECA: ch = 0xDA; break;
				case 0xFECB: ch = 0xDA; break;
				case 0xFECC: ch = 0xDA; break;
				case 0xFECD: ch = 0xDB; break;
				case 0xFECE: ch = 0xDB; break;
				case 0xFECF: ch = 0xDB; break;
				case 0xFED0: ch = 0xDB; break;
				case 0xFED1: ch = 0xDD; break;
				case 0xFED2: ch = 0xDD; break;
				case 0xFED3: ch = 0xDD; break;
				case 0xFED4: ch = 0xDD; break;
				case 0xFED5: ch = 0xDE; break;
				case 0xFED6: ch = 0xDE; break;
				case 0xFED7: ch = 0xDE; break;
				case 0xFED8: ch = 0xDE; break;
				case 0xFED9: ch = 0xDF; break;
				case 0xFEDA: ch = 0xDF; break;
				case 0xFEDB: ch = 0xDF; break;
				case 0xFEDC: ch = 0xDF; break;
				case 0xFEDD: ch = 0xE1; break;
				case 0xFEDE: ch = 0xE1; break;
				case 0xFEDF: ch = 0xE1; break;
				case 0xFEE0: ch = 0xE1; break;
				case 0xFEE1: ch = 0xE3; break;
				case 0xFEE2: ch = 0xE3; break;
				case 0xFEE3: ch = 0xE3; break;
				case 0xFEE4: ch = 0xE3; break;
				case 0xFEE5: ch = 0xE4; break;
				case 0xFEE6: ch = 0xE4; break;
				case 0xFEE7: ch = 0xE4; break;
				case 0xFEE8: ch = 0xE4; break;
				case 0xFEE9: ch = 0xE5; break;
				case 0xFEEA: ch = 0xE5; break;
				case 0xFEEB: ch = 0xE5; break;
				case 0xFEEC: ch = 0xE5; break;
				case 0xFEED: ch = 0xE6; break;
				case 0xFEEE: ch = 0xE6; break;
				case 0xFEEF: ch = 0xEC; break;
				case 0xFEF0: ch = 0xEC; break;
				case 0xFEF1: ch = 0xED; break;
				case 0xFEF2: ch = 0xED; break;
				case 0xFEF3: ch = 0xED; break;
				case 0xFEF4: ch = 0xED; break;
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

}; // class CP1256

[Serializable]
public class ENCwindows_1256 : CP1256
{
	public ENCwindows_1256() : base() {}

}; // class ENCwindows_1256

}; // namespace I18N.MidEast
