/*
 * CP20420.cs - IBM EBCDIC (Arabic) code page.
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

// Generated from "ibm-420.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.Rare
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP20420 : ByteEncoding
{
	public CP20420()
		: base(20420, ToChars, "IBM EBCDIC (Arabic)",
		       "IBM420", "IBM420", "IBM420",
		       false, false, false, false, 1256)
	{}

	private static readonly char[] ToChars = {
		'\u0000', '\u0001', '\u0002', '\u0003', '\u009C', '\u0009', 
		'\u0086', '\u007F', '\u0097', '\u008D', '\u008E', '\u000B', 
		'\u000C', '\u000D', '\u000E', '\u000F', '\u0010', '\u0011', 
		'\u0012', '\u0013', '\u009D', '\u0085', '\u0008', '\u0087', 
		'\u0018', '\u0019', '\u0092', '\u008F', '\u001C', '\u001D', 
		'\u001E', '\u001F', '\u0080', '\u0081', '\u0082', '\u0083', 
		'\u0084', '\u000A', '\u0017', '\u001B', '\u0088', '\u0089', 
		'\u008A', '\u008B', '\u008C', '\u0005', '\u0006', '\u0007', 
		'\u0090', '\u0091', '\u0016', '\u0093', '\u0094', '\u0095', 
		'\u0096', '\u0004', '\u0098', '\u0099', '\u009A', '\u009B', 
		'\u0014', '\u0015', '\u009E', '\u001A', '\u0020', '\u00A0', 
		'\u0651', '\uFE7D', '\u0640', '\u200B', '\u0621', '\u0622', 
		'\uFE82', '\u0623', '\u00A2', '\u002E', '\u003C', '\u0028', 
		'\u002B', '\u007C', '\u0026', '\uFE84', '\u0624', '\u003F', 
		'\u003F', '\u0626', '\u0627', '\uFE8E', '\u0628', '\uFE91', 
		'\u0021', '\u0024', '\u002A', '\u0029', '\u003B', '\u00AC', 
		'\u002D', '\u002F', '\u0629', '\u062A', '\uFE97', '\u062B', 
		'\uFE9B', '\u062C', '\uFE9F', '\u062D', '\u00A6', '\u002C', 
		'\u0025', '\u005F', '\u003E', '\u003F', '\uFEA3', '\u062E', 
		'\uFEA7', '\u062F', '\u0630', '\u0631', '\u0632', '\u0633', 
		'\uFEB3', '\u060C', '\u003A', '\u0023', '\u0040', '\u0027', 
		'\u003D', '\u0022', '\u0634', '\u0061', '\u0062', '\u0063', 
		'\u0064', '\u0065', '\u0066', '\u0067', '\u0068', '\u0069', 
		'\uFEB7', '\u0635', '\uFEBB', '\u0636', '\uFEBF', '\u0637', 
		'\u0638', '\u006A', '\u006B', '\u006C', '\u006D', '\u006E', 
		'\u006F', '\u0070', '\u0071', '\u0072', '\u0639', '\uFECA', 
		'\uFECB', '\uFECC', '\u063A', '\uFECE', '\uFECF', '\u00F7', 
		'\u0073', '\u0074', '\u0075', '\u0076', '\u0077', '\u0078', 
		'\u0079', '\u007A', '\uFED0', '\u0641', '\uFED3', '\u0642', 
		'\uFED7', '\u0643', '\uFEDB', '\u0644', '\uFEF5', '\uFEF6', 
		'\uFEF7', '\uFEF8', '\u003F', '\u003F', '\uFEFB', '\uFEFC', 
		'\uFEDF', '\u0645', '\uFEE3', '\u0646', '\uFEE7', '\u0647', 
		'\u061B', '\u0041', '\u0042', '\u0043', '\u0044', '\u0045', 
		'\u0046', '\u0047', '\u0048', '\u0049', '\u00AD', '\uFEEB', 
		'\u003F', '\uFEEC', '\u003F', '\u0648', '\u061F', '\u004A', 
		'\u004B', '\u004C', '\u004D', '\u004E', '\u004F', '\u0050', 
		'\u0051', '\u0052', '\u0649', '\uFEF0', '\u064A', '\uFEF2', 
		'\uFEF3', '\u0660', '\u00D7', '\u003F', '\u0053', '\u0054', 
		'\u0055', '\u0056', '\u0057', '\u0058', '\u0059', '\u005A', 
		'\u0661', '\u0662', '\u003F', '\u0663', '\u0664', '\u0665', 
		'\u0030', '\u0031', '\u0032', '\u0033', '\u0034', '\u0035', 
		'\u0036', '\u0037', '\u0038', '\u0039', '\u003F', '\u0666', 
		'\u0667', '\u0668', '\u0669', '\u009F', 
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
			if(ch >= 4) switch(ch)
			{
				case 0x000B:
				case 0x000C:
				case 0x000D:
				case 0x000E:
				case 0x000F:
				case 0x0010:
				case 0x0011:
				case 0x0012:
				case 0x0013:
				case 0x0018:
				case 0x0019:
				case 0x001C:
				case 0x001D:
				case 0x001E:
				case 0x001F:
					break;
				case 0x0004: ch = 0x37; break;
				case 0x0005: ch = 0x2D; break;
				case 0x0006: ch = 0x2E; break;
				case 0x0007: ch = 0x2F; break;
				case 0x0008: ch = 0x16; break;
				case 0x0009: ch = 0x05; break;
				case 0x000A: ch = 0x25; break;
				case 0x0014: ch = 0x3C; break;
				case 0x0015: ch = 0x3D; break;
				case 0x0016: ch = 0x32; break;
				case 0x0017: ch = 0x26; break;
				case 0x001A: ch = 0x3F; break;
				case 0x001B: ch = 0x27; break;
				case 0x0020: ch = 0x40; break;
				case 0x0021: ch = 0x5A; break;
				case 0x0022: ch = 0x7F; break;
				case 0x0023: ch = 0x7B; break;
				case 0x0024: ch = 0x5B; break;
				case 0x0025: ch = 0x6C; break;
				case 0x0026: ch = 0x50; break;
				case 0x0027: ch = 0x7D; break;
				case 0x0028: ch = 0x4D; break;
				case 0x0029: ch = 0x5D; break;
				case 0x002A: ch = 0x5C; break;
				case 0x002B: ch = 0x4E; break;
				case 0x002C: ch = 0x6B; break;
				case 0x002D: ch = 0x60; break;
				case 0x002E: ch = 0x4B; break;
				case 0x002F: ch = 0x61; break;
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
					ch += 0x00C0;
					break;
				case 0x003A: ch = 0x7A; break;
				case 0x003B: ch = 0x5E; break;
				case 0x003C: ch = 0x4C; break;
				case 0x003D: ch = 0x7E; break;
				case 0x003E: ch = 0x6E; break;
				case 0x003F: ch = 0x6F; break;
				case 0x0040: ch = 0x7C; break;
				case 0x0041:
				case 0x0042:
				case 0x0043:
				case 0x0044:
				case 0x0045:
				case 0x0046:
				case 0x0047:
				case 0x0048:
				case 0x0049:
					ch += 0x0080;
					break;
				case 0x004A:
				case 0x004B:
				case 0x004C:
				case 0x004D:
				case 0x004E:
				case 0x004F:
				case 0x0050:
				case 0x0051:
				case 0x0052:
					ch += 0x0087;
					break;
				case 0x0053:
				case 0x0054:
				case 0x0055:
				case 0x0056:
				case 0x0057:
				case 0x0058:
				case 0x0059:
				case 0x005A:
					ch += 0x008F;
					break;
				case 0x005F: ch = 0x6D; break;
				case 0x0061:
				case 0x0062:
				case 0x0063:
				case 0x0064:
				case 0x0065:
				case 0x0066:
				case 0x0067:
				case 0x0068:
				case 0x0069:
					ch += 0x0020;
					break;
				case 0x006A:
				case 0x006B:
				case 0x006C:
				case 0x006D:
				case 0x006E:
				case 0x006F:
				case 0x0070:
				case 0x0071:
				case 0x0072:
					ch += 0x0027;
					break;
				case 0x0073:
				case 0x0074:
				case 0x0075:
				case 0x0076:
				case 0x0077:
				case 0x0078:
				case 0x0079:
				case 0x007A:
					ch += 0x002F;
					break;
				case 0x007C: ch = 0x4F; break;
				case 0x007F: ch = 0x07; break;
				case 0x0080:
				case 0x0081:
				case 0x0082:
				case 0x0083:
				case 0x0084:
					ch -= 0x0060;
					break;
				case 0x0085: ch = 0x15; break;
				case 0x0086: ch = 0x06; break;
				case 0x0087: ch = 0x17; break;
				case 0x0088:
				case 0x0089:
				case 0x008A:
				case 0x008B:
				case 0x008C:
					ch -= 0x0060;
					break;
				case 0x008D: ch = 0x09; break;
				case 0x008E: ch = 0x0A; break;
				case 0x008F: ch = 0x1B; break;
				case 0x0090: ch = 0x30; break;
				case 0x0091: ch = 0x31; break;
				case 0x0092: ch = 0x1A; break;
				case 0x0093:
				case 0x0094:
				case 0x0095:
				case 0x0096:
					ch -= 0x0060;
					break;
				case 0x0097: ch = 0x08; break;
				case 0x0098:
				case 0x0099:
				case 0x009A:
				case 0x009B:
					ch -= 0x0060;
					break;
				case 0x009C: ch = 0x04; break;
				case 0x009D: ch = 0x14; break;
				case 0x009E: ch = 0x3E; break;
				case 0x009F: ch = 0xFF; break;
				case 0x00A0: ch = 0x41; break;
				case 0x00A2: ch = 0x4A; break;
				case 0x00A6: ch = 0x6A; break;
				case 0x00AC: ch = 0x5F; break;
				case 0x00AD: ch = 0xCA; break;
				case 0x00D7: ch = 0xE0; break;
				case 0x00F7: ch = 0xA1; break;
				case 0x060C: ch = 0x79; break;
				case 0x061B: ch = 0xC0; break;
				case 0x061F: ch = 0xD0; break;
				case 0x0621: ch = 0x46; break;
				case 0x0622: ch = 0x47; break;
				case 0x0623: ch = 0x49; break;
				case 0x0624: ch = 0x52; break;
				case 0x0625: ch = 0x56; break;
				case 0x0626: ch = 0x55; break;
				case 0x0627: ch = 0x56; break;
				case 0x0628: ch = 0x58; break;
				case 0x0629: ch = 0x62; break;
				case 0x062A: ch = 0x63; break;
				case 0x062B: ch = 0x65; break;
				case 0x062C: ch = 0x67; break;
				case 0x062D: ch = 0x69; break;
				case 0x062E: ch = 0x71; break;
				case 0x062F:
				case 0x0630:
				case 0x0631:
				case 0x0632:
				case 0x0633:
					ch -= 0x05BC;
					break;
				case 0x0634: ch = 0x80; break;
				case 0x0635: ch = 0x8B; break;
				case 0x0636: ch = 0x8D; break;
				case 0x0637: ch = 0x8F; break;
				case 0x0638: ch = 0x90; break;
				case 0x0639: ch = 0x9A; break;
				case 0x063A: ch = 0x9E; break;
				case 0x0640: ch = 0x44; break;
				case 0x0641: ch = 0xAB; break;
				case 0x0642: ch = 0xAD; break;
				case 0x0643: ch = 0xAF; break;
				case 0x0644: ch = 0xB1; break;
				case 0x0645: ch = 0xBB; break;
				case 0x0646: ch = 0xBD; break;
				case 0x0647: ch = 0xBF; break;
				case 0x0648: ch = 0xCF; break;
				case 0x0649: ch = 0xDA; break;
				case 0x064A: ch = 0xDC; break;
				case 0x0651: ch = 0x42; break;
				case 0x0660: ch = 0xDF; break;
				case 0x0661: ch = 0xEA; break;
				case 0x0662: ch = 0xEB; break;
				case 0x0663: ch = 0xED; break;
				case 0x0664: ch = 0xEE; break;
				case 0x0665: ch = 0xEF; break;
				case 0x0666:
				case 0x0667:
				case 0x0668:
				case 0x0669:
					ch -= 0x056B;
					break;
				case 0x066A: ch = 0x6C; break;
				case 0x066B: ch = 0x6B; break;
				case 0x066C: ch = 0x4B; break;
				case 0x066D: ch = 0x5C; break;
				case 0x200B: ch = 0x45; break;
				case 0xFE7C: ch = 0x42; break;
				case 0xFE7D: ch = 0x43; break;
				case 0xFE80:
				case 0xFE81:
				case 0xFE82:
				case 0xFE83:
					ch -= 0xFE3A;
					break;
				case 0xFE84: ch = 0x51; break;
				case 0xFE85: ch = 0x52; break;
				case 0xFE86: ch = 0x52; break;
				case 0xFE87: ch = 0x56; break;
				case 0xFE88: ch = 0x57; break;
				case 0xFE8B: ch = 0x55; break;
				case 0xFE8C:
				case 0xFE8D:
				case 0xFE8E:
				case 0xFE8F:
					ch -= 0xFE37;
					break;
				case 0xFE90: ch = 0x58; break;
				case 0xFE91: ch = 0x59; break;
				case 0xFE92: ch = 0x59; break;
				case 0xFE93: ch = 0x62; break;
				case 0xFE94: ch = 0x62; break;
				case 0xFE95: ch = 0x63; break;
				case 0xFE96: ch = 0x63; break;
				case 0xFE97: ch = 0x64; break;
				case 0xFE98: ch = 0x64; break;
				case 0xFE99: ch = 0x65; break;
				case 0xFE9A: ch = 0x65; break;
				case 0xFE9B: ch = 0x66; break;
				case 0xFE9C: ch = 0x66; break;
				case 0xFE9D: ch = 0x67; break;
				case 0xFE9E: ch = 0x67; break;
				case 0xFE9F: ch = 0x68; break;
				case 0xFEA0: ch = 0x68; break;
				case 0xFEA1: ch = 0x69; break;
				case 0xFEA2: ch = 0x69; break;
				case 0xFEA3: ch = 0x70; break;
				case 0xFEA4: ch = 0x70; break;
				case 0xFEA5: ch = 0x71; break;
				case 0xFEA6: ch = 0x71; break;
				case 0xFEA7: ch = 0x72; break;
				case 0xFEA8: ch = 0x72; break;
				case 0xFEA9: ch = 0x73; break;
				case 0xFEAA: ch = 0x73; break;
				case 0xFEAB: ch = 0x74; break;
				case 0xFEAC: ch = 0x74; break;
				case 0xFEAD: ch = 0x75; break;
				case 0xFEAE: ch = 0x75; break;
				case 0xFEAF: ch = 0x76; break;
				case 0xFEB0: ch = 0x76; break;
				case 0xFEB1: ch = 0x77; break;
				case 0xFEB2: ch = 0x77; break;
				case 0xFEB3: ch = 0x78; break;
				case 0xFEB4: ch = 0x78; break;
				case 0xFEB5: ch = 0x80; break;
				case 0xFEB6: ch = 0x80; break;
				case 0xFEB7: ch = 0x8A; break;
				case 0xFEB8: ch = 0x8A; break;
				case 0xFEB9: ch = 0x8B; break;
				case 0xFEBA: ch = 0x8B; break;
				case 0xFEBB: ch = 0x8C; break;
				case 0xFEBC: ch = 0x8C; break;
				case 0xFEBD: ch = 0x8D; break;
				case 0xFEBE: ch = 0x8D; break;
				case 0xFEBF: ch = 0x8E; break;
				case 0xFEC0: ch = 0x8E; break;
				case 0xFEC1: ch = 0x8F; break;
				case 0xFEC2: ch = 0x8F; break;
				case 0xFEC3: ch = 0x8F; break;
				case 0xFEC4: ch = 0x8F; break;
				case 0xFEC5: ch = 0x90; break;
				case 0xFEC6: ch = 0x90; break;
				case 0xFEC7: ch = 0x90; break;
				case 0xFEC8: ch = 0x90; break;
				case 0xFEC9:
				case 0xFECA:
				case 0xFECB:
				case 0xFECC:
				case 0xFECD:
				case 0xFECE:
				case 0xFECF:
					ch -= 0xFE2F;
					break;
				case 0xFED0: ch = 0xAA; break;
				case 0xFED1: ch = 0xAB; break;
				case 0xFED2: ch = 0xAB; break;
				case 0xFED3: ch = 0xAC; break;
				case 0xFED4: ch = 0xAC; break;
				case 0xFED5: ch = 0xAD; break;
				case 0xFED6: ch = 0xAD; break;
				case 0xFED7: ch = 0xAE; break;
				case 0xFED8: ch = 0xAE; break;
				case 0xFED9: ch = 0xAF; break;
				case 0xFEDA: ch = 0xAF; break;
				case 0xFEDB: ch = 0xB0; break;
				case 0xFEDC: ch = 0xB0; break;
				case 0xFEDD: ch = 0xB1; break;
				case 0xFEDE: ch = 0xB1; break;
				case 0xFEDF: ch = 0xBA; break;
				case 0xFEE0: ch = 0xBA; break;
				case 0xFEE1: ch = 0xBB; break;
				case 0xFEE2: ch = 0xBB; break;
				case 0xFEE3: ch = 0xBC; break;
				case 0xFEE4: ch = 0xBC; break;
				case 0xFEE5: ch = 0xBD; break;
				case 0xFEE6: ch = 0xBD; break;
				case 0xFEE7: ch = 0xBE; break;
				case 0xFEE8: ch = 0xBE; break;
				case 0xFEE9: ch = 0xBF; break;
				case 0xFEEA: ch = 0xBF; break;
				case 0xFEEB: ch = 0xCB; break;
				case 0xFEEC: ch = 0xCD; break;
				case 0xFEED: ch = 0xCF; break;
				case 0xFEEE: ch = 0xCF; break;
				case 0xFEEF:
				case 0xFEF0:
				case 0xFEF1:
				case 0xFEF2:
				case 0xFEF3:
					ch -= 0xFE15;
					break;
				case 0xFEF4: ch = 0xDE; break;
				case 0xFEF5:
				case 0xFEF6:
				case 0xFEF7:
				case 0xFEF8:
					ch -= 0xFE43;
					break;
				case 0xFEF9: ch = 0xB8; break;
				case 0xFEFA: ch = 0xB9; break;
				case 0xFEFB: ch = 0xB8; break;
				case 0xFEFC: ch = 0xB9; break;
				case 0xFF01: ch = 0x5A; break;
				case 0xFF02: ch = 0x7F; break;
				case 0xFF03: ch = 0x7B; break;
				case 0xFF04: ch = 0x5B; break;
				case 0xFF05: ch = 0x6C; break;
				case 0xFF06: ch = 0x50; break;
				case 0xFF07: ch = 0x7D; break;
				case 0xFF08: ch = 0x4D; break;
				case 0xFF09: ch = 0x5D; break;
				case 0xFF0A: ch = 0x5C; break;
				case 0xFF0B: ch = 0x4E; break;
				case 0xFF0C: ch = 0x6B; break;
				case 0xFF0D: ch = 0x60; break;
				case 0xFF0E: ch = 0x4B; break;
				case 0xFF0F: ch = 0x61; break;
				case 0xFF10:
				case 0xFF11:
				case 0xFF12:
				case 0xFF13:
				case 0xFF14:
				case 0xFF15:
				case 0xFF16:
				case 0xFF17:
				case 0xFF18:
				case 0xFF19:
					ch -= 0xFE20;
					break;
				case 0xFF1A: ch = 0x7A; break;
				case 0xFF1B: ch = 0x5E; break;
				case 0xFF1C: ch = 0x4C; break;
				case 0xFF1D: ch = 0x7E; break;
				case 0xFF1E: ch = 0x6E; break;
				case 0xFF1F: ch = 0x6F; break;
				case 0xFF20: ch = 0x7C; break;
				case 0xFF21:
				case 0xFF22:
				case 0xFF23:
				case 0xFF24:
				case 0xFF25:
				case 0xFF26:
				case 0xFF27:
				case 0xFF28:
				case 0xFF29:
					ch -= 0xFE60;
					break;
				case 0xFF2A:
				case 0xFF2B:
				case 0xFF2C:
				case 0xFF2D:
				case 0xFF2E:
				case 0xFF2F:
				case 0xFF30:
				case 0xFF31:
				case 0xFF32:
					ch -= 0xFE59;
					break;
				case 0xFF33:
				case 0xFF34:
				case 0xFF35:
				case 0xFF36:
				case 0xFF37:
				case 0xFF38:
				case 0xFF39:
				case 0xFF3A:
					ch -= 0xFE51;
					break;
				case 0xFF3F: ch = 0x6D; break;
				case 0xFF41:
				case 0xFF42:
				case 0xFF43:
				case 0xFF44:
				case 0xFF45:
				case 0xFF46:
				case 0xFF47:
				case 0xFF48:
				case 0xFF49:
					ch -= 0xFEC0;
					break;
				case 0xFF4A:
				case 0xFF4B:
				case 0xFF4C:
				case 0xFF4D:
				case 0xFF4E:
				case 0xFF4F:
				case 0xFF50:
				case 0xFF51:
				case 0xFF52:
					ch -= 0xFEB9;
					break;
				case 0xFF53:
				case 0xFF54:
				case 0xFF55:
				case 0xFF56:
				case 0xFF57:
				case 0xFF58:
				case 0xFF59:
				case 0xFF5A:
					ch -= 0xFEB1;
					break;
				case 0xFF5C: ch = 0x4F; break;
				default:
					HandleFallback (ref buffer, chars, ref charIndex, ref charCount, bytes, ref byteIndex, ref byteCount);
					charIndex++;
					charCount--;
					continue;
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
}; // class CP20420

[Serializable]
public class ENCibm420 : CP20420
{
	public ENCibm420() : base() {}

}; // class ENCibm420

}; // namespace I18N.Rare
