/*
 * CP28603.cs - Estonian (ISO) code page.
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

// Generated from "windows-28603-vista.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.West
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP28603 : ByteEncoding
{
	public CP28603()
		: base(28603, ToChars, "Estonian (ISO)",
		       "iso-8859-13", "iso-8859-13", "iso-8859-13",
		       false, false, true, true, 1257)
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
		'\u009C', '\u009D', '\u009E', '\u009F', '\u00A0', '\u201D', 
		'\u00A2', '\u00A3', '\u00A4', '\u201E', '\u00A6', '\u00A7', 
		'\u00D8', '\u00A9', '\u0156', '\u00AB', '\u00AC', '\u00AD', 
		'\u00AE', '\u00C6', '\u00B0', '\u00B1', '\u00B2', '\u00B3', 
		'\u201C', '\u00B5', '\u00B6', '\u00B7', '\u00F8', '\u00B9', 
		'\u0157', '\u00BB', '\u00BC', '\u00BD', '\u00BE', '\u00E6', 
		'\u0104', '\u012E', '\u0100', '\u0106', '\u00C4', '\u00C5', 
		'\u0118', '\u0112', '\u010C', '\u00C9', '\u0179', '\u0116', 
		'\u0122', '\u0136', '\u012A', '\u013B', '\u0160', '\u0143', 
		'\u0145', '\u00D3', '\u014C', '\u00D5', '\u00D6', '\u00D7', 
		'\u0172', '\u0141', '\u015A', '\u016A', '\u00DC', '\u017B', 
		'\u017D', '\u00DF', '\u0105', '\u012F', '\u0101', '\u0107', 
		'\u00E4', '\u00E5', '\u0119', '\u0113', '\u010D', '\u00E9', 
		'\u017A', '\u0117', '\u0123', '\u0137', '\u012B', '\u013C', 
		'\u0161', '\u0144', '\u0146', '\u00F3', '\u014D', '\u00F5', 
		'\u00F6', '\u00F7', '\u0173', '\u0142', '\u015B', '\u016B', 
		'\u00FC', '\u017C', '\u017E', '\u2019', 
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
				case 0x00A2:
				case 0x00A3:
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
				case 0x00B2:
				case 0x00B3:
				case 0x00B5:
				case 0x00B6:
				case 0x00B7:
				case 0x00B9:
				case 0x00BB:
				case 0x00BC:
				case 0x00BD:
				case 0x00BE:
				case 0x00C4:
				case 0x00C5:
				case 0x00C9:
				case 0x00D3:
				case 0x00D5:
				case 0x00D6:
				case 0x00D7:
				case 0x00DC:
				case 0x00DF:
				case 0x00E4:
				case 0x00E5:
				case 0x00E9:
				case 0x00F3:
				case 0x00F5:
				case 0x00F6:
				case 0x00F7:
				case 0x00FC:
					break;
				case 0x00AA: ch = 0x61; break;
				case 0x00BA: ch = 0x6F; break;
				case 0x00C0: ch = 0x41; break;
				case 0x00C1: ch = 0x41; break;
				case 0x00C2: ch = 0x41; break;
				case 0x00C3: ch = 0x41; break;
				case 0x00C6: ch = 0xAF; break;
				case 0x00C7: ch = 0x43; break;
				case 0x00C8: ch = 0x45; break;
				case 0x00CA: ch = 0x45; break;
				case 0x00CB: ch = 0x45; break;
				case 0x00CC: ch = 0x49; break;
				case 0x00CD: ch = 0x49; break;
				case 0x00CE: ch = 0x49; break;
				case 0x00CF: ch = 0x49; break;
				case 0x00D1: ch = 0x4E; break;
				case 0x00D2: ch = 0x4F; break;
				case 0x00D4: ch = 0x4F; break;
				case 0x00D8: ch = 0xA8; break;
				case 0x00D9: ch = 0x55; break;
				case 0x00DA: ch = 0x55; break;
				case 0x00DB: ch = 0x55; break;
				case 0x00DD: ch = 0x59; break;
				case 0x00E0: ch = 0x61; break;
				case 0x00E1: ch = 0x61; break;
				case 0x00E2: ch = 0x61; break;
				case 0x00E3: ch = 0x61; break;
				case 0x00E6: ch = 0xBF; break;
				case 0x00E7: ch = 0x63; break;
				case 0x00E8: ch = 0x65; break;
				case 0x00EA: ch = 0x65; break;
				case 0x00EB: ch = 0x65; break;
				case 0x00EC: ch = 0x69; break;
				case 0x00ED: ch = 0x69; break;
				case 0x00EE: ch = 0x69; break;
				case 0x00EF: ch = 0x69; break;
				case 0x00F1: ch = 0x6E; break;
				case 0x00F2: ch = 0x6F; break;
				case 0x00F4: ch = 0x6F; break;
				case 0x00F8: ch = 0xB8; break;
				case 0x00F9: ch = 0x75; break;
				case 0x00FA: ch = 0x75; break;
				case 0x00FB: ch = 0x75; break;
				case 0x00FD: ch = 0x79; break;
				case 0x00FF: ch = 0x79; break;
				case 0x0100: ch = 0xC2; break;
				case 0x0101: ch = 0xE2; break;
				case 0x0102: ch = 0x41; break;
				case 0x0103: ch = 0x61; break;
				case 0x0104: ch = 0xC0; break;
				case 0x0105: ch = 0xE0; break;
				case 0x0106: ch = 0xC3; break;
				case 0x0107: ch = 0xE3; break;
				case 0x0108: ch = 0x43; break;
				case 0x0109: ch = 0x63; break;
				case 0x010A: ch = 0x43; break;
				case 0x010B: ch = 0x63; break;
				case 0x010C: ch = 0xC8; break;
				case 0x010D: ch = 0xE8; break;
				case 0x010E: ch = 0x44; break;
				case 0x010F: ch = 0x64; break;
				case 0x0112: ch = 0xC7; break;
				case 0x0113: ch = 0xE7; break;
				case 0x0114: ch = 0x45; break;
				case 0x0115: ch = 0x65; break;
				case 0x0116: ch = 0xCB; break;
				case 0x0117: ch = 0xEB; break;
				case 0x0118: ch = 0xC6; break;
				case 0x0119: ch = 0xE6; break;
				case 0x011A: ch = 0x45; break;
				case 0x011B: ch = 0x65; break;
				case 0x011C: ch = 0x47; break;
				case 0x011D: ch = 0x67; break;
				case 0x011E: ch = 0x47; break;
				case 0x011F: ch = 0x67; break;
				case 0x0120: ch = 0x47; break;
				case 0x0121: ch = 0x67; break;
				case 0x0122: ch = 0xCC; break;
				case 0x0123: ch = 0xEC; break;
				case 0x0124: ch = 0x48; break;
				case 0x0125: ch = 0x68; break;
				case 0x0128: ch = 0x49; break;
				case 0x0129: ch = 0x69; break;
				case 0x012A: ch = 0xCE; break;
				case 0x012B: ch = 0xEE; break;
				case 0x012C: ch = 0x49; break;
				case 0x012D: ch = 0x69; break;
				case 0x012E: ch = 0xC1; break;
				case 0x012F: ch = 0xE1; break;
				case 0x0130: ch = 0x49; break;
				case 0x0134: ch = 0x4A; break;
				case 0x0135: ch = 0x6A; break;
				case 0x0136: ch = 0xCD; break;
				case 0x0137: ch = 0xED; break;
				case 0x0139: ch = 0x4C; break;
				case 0x013A: ch = 0x6C; break;
				case 0x013B: ch = 0xCF; break;
				case 0x013C: ch = 0xEF; break;
				case 0x013D: ch = 0x4C; break;
				case 0x013E: ch = 0x6C; break;
				case 0x0141: ch = 0xD9; break;
				case 0x0142: ch = 0xF9; break;
				case 0x0143: ch = 0xD1; break;
				case 0x0144: ch = 0xF1; break;
				case 0x0145: ch = 0xD2; break;
				case 0x0146: ch = 0xF2; break;
				case 0x0147: ch = 0x4E; break;
				case 0x0148: ch = 0x6E; break;
				case 0x014C: ch = 0xD4; break;
				case 0x014D: ch = 0xF4; break;
				case 0x014E: ch = 0x4F; break;
				case 0x014F: ch = 0x6F; break;
				case 0x0150: ch = 0x4F; break;
				case 0x0151: ch = 0x6F; break;
				case 0x0154: ch = 0x52; break;
				case 0x0155: ch = 0x72; break;
				case 0x0156: ch = 0xAA; break;
				case 0x0157: ch = 0xBA; break;
				case 0x0158: ch = 0x52; break;
				case 0x0159: ch = 0x72; break;
				case 0x015A: ch = 0xDA; break;
				case 0x015B: ch = 0xFA; break;
				case 0x015C: ch = 0x53; break;
				case 0x015D: ch = 0x73; break;
				case 0x015E: ch = 0x53; break;
				case 0x015F: ch = 0x73; break;
				case 0x0160: ch = 0xD0; break;
				case 0x0161: ch = 0xF0; break;
				case 0x0162: ch = 0x54; break;
				case 0x0163: ch = 0x74; break;
				case 0x0164: ch = 0x54; break;
				case 0x0165: ch = 0x74; break;
				case 0x0168: ch = 0x55; break;
				case 0x0169: ch = 0x75; break;
				case 0x016A: ch = 0xDB; break;
				case 0x016B: ch = 0xFB; break;
				case 0x016C: ch = 0x55; break;
				case 0x016D: ch = 0x75; break;
				case 0x016E: ch = 0x55; break;
				case 0x016F: ch = 0x75; break;
				case 0x0170: ch = 0x55; break;
				case 0x0171: ch = 0x75; break;
				case 0x0172: ch = 0xD8; break;
				case 0x0173: ch = 0xF8; break;
				case 0x0174: ch = 0x57; break;
				case 0x0175: ch = 0x77; break;
				case 0x0176: ch = 0x59; break;
				case 0x0177: ch = 0x79; break;
				case 0x0178: ch = 0x59; break;
				case 0x0179: ch = 0xCA; break;
				case 0x017A: ch = 0xEA; break;
				case 0x017B: ch = 0xDD; break;
				case 0x017C: ch = 0xFD; break;
				case 0x017D: ch = 0xDE; break;
				case 0x017E: ch = 0xFE; break;
				case 0x017F: ch = 0x73; break;
				case 0x01A0: ch = 0x4F; break;
				case 0x01A1: ch = 0x6F; break;
				case 0x01AF: ch = 0x55; break;
				case 0x01B0: ch = 0x75; break;
				case 0x01CD: ch = 0x41; break;
				case 0x01CE: ch = 0x61; break;
				case 0x01CF: ch = 0x49; break;
				case 0x01D0: ch = 0x69; break;
				case 0x01D1: ch = 0x4F; break;
				case 0x01D2: ch = 0x6F; break;
				case 0x01D3: ch = 0x55; break;
				case 0x01D4: ch = 0x75; break;
				case 0x01D5: ch = 0xDC; break;
				case 0x01D6: ch = 0xFC; break;
				case 0x01D7: ch = 0xDC; break;
				case 0x01D8: ch = 0xFC; break;
				case 0x01D9: ch = 0xDC; break;
				case 0x01DA: ch = 0xFC; break;
				case 0x01DB: ch = 0xDC; break;
				case 0x01DC: ch = 0xFC; break;
				case 0x01DE: ch = 0xC4; break;
				case 0x01DF: ch = 0xE4; break;
				case 0x01E0: ch = 0x41; break;
				case 0x01E1: ch = 0x61; break;
				case 0x01E2: ch = 0xAF; break;
				case 0x01E3: ch = 0xBF; break;
				case 0x01E6: ch = 0x47; break;
				case 0x01E7: ch = 0x67; break;
				case 0x01E8: ch = 0x4B; break;
				case 0x01E9: ch = 0x6B; break;
				case 0x01EA: ch = 0x4F; break;
				case 0x01EB: ch = 0x6F; break;
				case 0x01EC: ch = 0x4F; break;
				case 0x01ED: ch = 0x6F; break;
				case 0x01F0: ch = 0x6A; break;
				case 0x01F4: ch = 0x47; break;
				case 0x01F5: ch = 0x67; break;
				case 0x01F8: ch = 0x4E; break;
				case 0x01F9: ch = 0x6E; break;
				case 0x01FA: ch = 0xC5; break;
				case 0x01FB: ch = 0xE5; break;
				case 0x01FC: ch = 0xAF; break;
				case 0x01FD: ch = 0xBF; break;
				case 0x01FE: ch = 0xA8; break;
				case 0x01FF: ch = 0xB8; break;
				case 0x0200: ch = 0x41; break;
				case 0x0201: ch = 0x61; break;
				case 0x0202: ch = 0x41; break;
				case 0x0203: ch = 0x61; break;
				case 0x0204: ch = 0x45; break;
				case 0x0205: ch = 0x65; break;
				case 0x0206: ch = 0x45; break;
				case 0x0207: ch = 0x65; break;
				case 0x0208: ch = 0x49; break;
				case 0x0209: ch = 0x69; break;
				case 0x020A: ch = 0x49; break;
				case 0x020B: ch = 0x69; break;
				case 0x020C: ch = 0x4F; break;
				case 0x020D: ch = 0x6F; break;
				case 0x020E: ch = 0x4F; break;
				case 0x020F: ch = 0x6F; break;
				case 0x0210: ch = 0x52; break;
				case 0x0211: ch = 0x72; break;
				case 0x0212: ch = 0x52; break;
				case 0x0213: ch = 0x72; break;
				case 0x0214: ch = 0x55; break;
				case 0x0215: ch = 0x75; break;
				case 0x0216: ch = 0x55; break;
				case 0x0217: ch = 0x75; break;
				case 0x0218: ch = 0x53; break;
				case 0x0219: ch = 0x73; break;
				case 0x021A: ch = 0x54; break;
				case 0x021B: ch = 0x74; break;
				case 0x021E: ch = 0x48; break;
				case 0x021F: ch = 0x68; break;
				case 0x0226: ch = 0x41; break;
				case 0x0227: ch = 0x61; break;
				case 0x0228: ch = 0x45; break;
				case 0x0229: ch = 0x65; break;
				case 0x022A: ch = 0xD6; break;
				case 0x022B: ch = 0xF6; break;
				case 0x022C: ch = 0xD5; break;
				case 0x022D: ch = 0xF5; break;
				case 0x022E: ch = 0x4F; break;
				case 0x022F: ch = 0x6F; break;
				case 0x0230: ch = 0x4F; break;
				case 0x0231: ch = 0x6F; break;
				case 0x0232: ch = 0x59; break;
				case 0x0233: ch = 0x79; break;
				case 0x02B0: ch = 0x68; break;
				case 0x02B2: ch = 0x6A; break;
				case 0x02B3: ch = 0x72; break;
				case 0x02B7: ch = 0x77; break;
				case 0x02B8: ch = 0x79; break;
				case 0x02E1: ch = 0x6C; break;
				case 0x02E2: ch = 0x73; break;
				case 0x02E3: ch = 0x78; break;
				case 0x037E: ch = 0x3B; break;
				case 0x0387: ch = 0xB7; break;
				case 0x1E00: ch = 0x41; break;
				case 0x1E01: ch = 0x61; break;
				case 0x1E02: ch = 0x42; break;
				case 0x1E03: ch = 0x62; break;
				case 0x1E04: ch = 0x42; break;
				case 0x1E05: ch = 0x62; break;
				case 0x1E06: ch = 0x42; break;
				case 0x1E07: ch = 0x62; break;
				case 0x1E08: ch = 0x43; break;
				case 0x1E09: ch = 0x63; break;
				case 0x1E0A: ch = 0x44; break;
				case 0x1E0B: ch = 0x64; break;
				case 0x1E0C: ch = 0x44; break;
				case 0x1E0D: ch = 0x64; break;
				case 0x1E0E: ch = 0x44; break;
				case 0x1E0F: ch = 0x64; break;
				case 0x1E10: ch = 0x44; break;
				case 0x1E11: ch = 0x64; break;
				case 0x1E12: ch = 0x44; break;
				case 0x1E13: ch = 0x64; break;
				case 0x1E14: ch = 0xC7; break;
				case 0x1E15: ch = 0xE7; break;
				case 0x1E16: ch = 0xC7; break;
				case 0x1E17: ch = 0xE7; break;
				case 0x1E18: ch = 0x45; break;
				case 0x1E19: ch = 0x65; break;
				case 0x1E1A: ch = 0x45; break;
				case 0x1E1B: ch = 0x65; break;
				case 0x1E1C: ch = 0x45; break;
				case 0x1E1D: ch = 0x65; break;
				case 0x1E1E: ch = 0x46; break;
				case 0x1E1F: ch = 0x66; break;
				case 0x1E20: ch = 0x47; break;
				case 0x1E21: ch = 0x67; break;
				case 0x1E22: ch = 0x48; break;
				case 0x1E23: ch = 0x68; break;
				case 0x1E24: ch = 0x48; break;
				case 0x1E25: ch = 0x68; break;
				case 0x1E26: ch = 0x48; break;
				case 0x1E27: ch = 0x68; break;
				case 0x1E28: ch = 0x48; break;
				case 0x1E29: ch = 0x68; break;
				case 0x1E2A: ch = 0x48; break;
				case 0x1E2B: ch = 0x68; break;
				case 0x1E2C: ch = 0x49; break;
				case 0x1E2D: ch = 0x69; break;
				case 0x1E2E: ch = 0x49; break;
				case 0x1E2F: ch = 0x69; break;
				case 0x1E30: ch = 0x4B; break;
				case 0x1E31: ch = 0x6B; break;
				case 0x1E32: ch = 0x4B; break;
				case 0x1E33: ch = 0x6B; break;
				case 0x1E34: ch = 0x4B; break;
				case 0x1E35: ch = 0x6B; break;
				case 0x1E36: ch = 0x4C; break;
				case 0x1E37: ch = 0x6C; break;
				case 0x1E38: ch = 0x4C; break;
				case 0x1E39: ch = 0x6C; break;
				case 0x1E3A: ch = 0x4C; break;
				case 0x1E3B: ch = 0x6C; break;
				case 0x1E3C: ch = 0x4C; break;
				case 0x1E3D: ch = 0x6C; break;
				case 0x1E3E: ch = 0x4D; break;
				case 0x1E3F: ch = 0x6D; break;
				case 0x1E40: ch = 0x4D; break;
				case 0x1E41: ch = 0x6D; break;
				case 0x1E42: ch = 0x4D; break;
				case 0x1E43: ch = 0x6D; break;
				case 0x1E44: ch = 0x4E; break;
				case 0x1E45: ch = 0x6E; break;
				case 0x1E46: ch = 0x4E; break;
				case 0x1E47: ch = 0x6E; break;
				case 0x1E48: ch = 0x4E; break;
				case 0x1E49: ch = 0x6E; break;
				case 0x1E4A: ch = 0x4E; break;
				case 0x1E4B: ch = 0x6E; break;
				case 0x1E4C: ch = 0xD5; break;
				case 0x1E4D: ch = 0xF5; break;
				case 0x1E4E: ch = 0xD5; break;
				case 0x1E4F: ch = 0xF5; break;
				case 0x1E50: ch = 0xD4; break;
				case 0x1E51: ch = 0xF4; break;
				case 0x1E52: ch = 0xD4; break;
				case 0x1E53: ch = 0xF4; break;
				case 0x1E54: ch = 0x50; break;
				case 0x1E55: ch = 0x70; break;
				case 0x1E56: ch = 0x50; break;
				case 0x1E57: ch = 0x70; break;
				case 0x1E58: ch = 0x52; break;
				case 0x1E59: ch = 0x72; break;
				case 0x1E5A: ch = 0x52; break;
				case 0x1E5B: ch = 0x72; break;
				case 0x1E5C: ch = 0x52; break;
				case 0x1E5D: ch = 0x72; break;
				case 0x1E5E: ch = 0x52; break;
				case 0x1E5F: ch = 0x72; break;
				case 0x1E60: ch = 0x53; break;
				case 0x1E61: ch = 0x73; break;
				case 0x1E62: ch = 0x53; break;
				case 0x1E63: ch = 0x73; break;
				case 0x1E64: ch = 0xDA; break;
				case 0x1E65: ch = 0xFA; break;
				case 0x1E66: ch = 0xD0; break;
				case 0x1E67: ch = 0xF0; break;
				case 0x1E68: ch = 0x53; break;
				case 0x1E69: ch = 0x73; break;
				case 0x1E6A: ch = 0x54; break;
				case 0x1E6B: ch = 0x74; break;
				case 0x1E6C: ch = 0x54; break;
				case 0x1E6D: ch = 0x74; break;
				case 0x1E6E: ch = 0x54; break;
				case 0x1E6F: ch = 0x74; break;
				case 0x1E70: ch = 0x54; break;
				case 0x1E71: ch = 0x74; break;
				case 0x1E72: ch = 0x55; break;
				case 0x1E73: ch = 0x75; break;
				case 0x1E74: ch = 0x55; break;
				case 0x1E75: ch = 0x75; break;
				case 0x1E76: ch = 0x55; break;
				case 0x1E77: ch = 0x75; break;
				case 0x1E78: ch = 0x55; break;
				case 0x1E79: ch = 0x75; break;
				case 0x1E7A: ch = 0xDB; break;
				case 0x1E7B: ch = 0xFB; break;
				case 0x1E7C: ch = 0x56; break;
				case 0x1E7D: ch = 0x76; break;
				case 0x1E7E: ch = 0x56; break;
				case 0x1E7F: ch = 0x76; break;
				case 0x1E80: ch = 0x57; break;
				case 0x1E81: ch = 0x77; break;
				case 0x1E82: ch = 0x57; break;
				case 0x1E83: ch = 0x77; break;
				case 0x1E84: ch = 0x57; break;
				case 0x1E85: ch = 0x77; break;
				case 0x1E86: ch = 0x57; break;
				case 0x1E87: ch = 0x77; break;
				case 0x1E88: ch = 0x57; break;
				case 0x1E89: ch = 0x77; break;
				case 0x1E8A: ch = 0x58; break;
				case 0x1E8B: ch = 0x78; break;
				case 0x1E8C: ch = 0x58; break;
				case 0x1E8D: ch = 0x78; break;
				case 0x1E8E: ch = 0x59; break;
				case 0x1E8F: ch = 0x79; break;
				case 0x1E90: ch = 0x5A; break;
				case 0x1E91: ch = 0x7A; break;
				case 0x1E92: ch = 0x5A; break;
				case 0x1E93: ch = 0x7A; break;
				case 0x1E94: ch = 0x5A; break;
				case 0x1E95: ch = 0x7A; break;
				case 0x1E96: ch = 0x68; break;
				case 0x1E97: ch = 0x74; break;
				case 0x1E98: ch = 0x77; break;
				case 0x1E99: ch = 0x79; break;
				case 0x1E9B: ch = 0x73; break;
				case 0x1EA0: ch = 0x41; break;
				case 0x1EA1: ch = 0x61; break;
				case 0x1EA2: ch = 0x41; break;
				case 0x1EA3: ch = 0x61; break;
				case 0x1EA4: ch = 0x41; break;
				case 0x1EA5: ch = 0x61; break;
				case 0x1EA6: ch = 0x41; break;
				case 0x1EA7: ch = 0x61; break;
				case 0x1EA8: ch = 0x41; break;
				case 0x1EA9: ch = 0x61; break;
				case 0x1EAA: ch = 0x41; break;
				case 0x1EAB: ch = 0x61; break;
				case 0x1EAC: ch = 0x41; break;
				case 0x1EAD: ch = 0x61; break;
				case 0x1EAE: ch = 0x41; break;
				case 0x1EAF: ch = 0x61; break;
				case 0x1EB0: ch = 0x41; break;
				case 0x1EB1: ch = 0x61; break;
				case 0x1EB2: ch = 0x41; break;
				case 0x1EB3: ch = 0x61; break;
				case 0x1EB4: ch = 0x41; break;
				case 0x1EB5: ch = 0x61; break;
				case 0x1EB6: ch = 0x41; break;
				case 0x1EB7: ch = 0x61; break;
				case 0x1EB8: ch = 0x45; break;
				case 0x1EB9: ch = 0x65; break;
				case 0x1EBA: ch = 0x45; break;
				case 0x1EBB: ch = 0x65; break;
				case 0x1EBC: ch = 0x45; break;
				case 0x1EBD: ch = 0x65; break;
				case 0x1EBE: ch = 0x45; break;
				case 0x1EBF: ch = 0x65; break;
				case 0x1EC0: ch = 0x45; break;
				case 0x1EC1: ch = 0x65; break;
				case 0x1EC2: ch = 0x45; break;
				case 0x1EC3: ch = 0x65; break;
				case 0x1EC4: ch = 0x45; break;
				case 0x1EC5: ch = 0x65; break;
				case 0x1EC6: ch = 0x45; break;
				case 0x1EC7: ch = 0x65; break;
				case 0x1EC8: ch = 0x49; break;
				case 0x1EC9: ch = 0x69; break;
				case 0x1ECA: ch = 0x49; break;
				case 0x1ECB: ch = 0x69; break;
				case 0x1ECC: ch = 0x4F; break;
				case 0x1ECD: ch = 0x6F; break;
				case 0x1ECE: ch = 0x4F; break;
				case 0x1ECF: ch = 0x6F; break;
				case 0x1ED0: ch = 0x4F; break;
				case 0x1ED1: ch = 0x6F; break;
				case 0x1ED2: ch = 0x4F; break;
				case 0x1ED3: ch = 0x6F; break;
				case 0x1ED4: ch = 0x4F; break;
				case 0x1ED5: ch = 0x6F; break;
				case 0x1ED6: ch = 0x4F; break;
				case 0x1ED7: ch = 0x6F; break;
				case 0x1ED8: ch = 0x4F; break;
				case 0x1ED9: ch = 0x6F; break;
				case 0x1EDA: ch = 0x4F; break;
				case 0x1EDB: ch = 0x6F; break;
				case 0x1EDC: ch = 0x4F; break;
				case 0x1EDD: ch = 0x6F; break;
				case 0x1EDE: ch = 0x4F; break;
				case 0x1EDF: ch = 0x6F; break;
				case 0x1EE0: ch = 0x4F; break;
				case 0x1EE1: ch = 0x6F; break;
				case 0x1EE2: ch = 0x4F; break;
				case 0x1EE3: ch = 0x6F; break;
				case 0x1EE4: ch = 0x55; break;
				case 0x1EE5: ch = 0x75; break;
				case 0x1EE6: ch = 0x55; break;
				case 0x1EE7: ch = 0x75; break;
				case 0x1EE8: ch = 0x55; break;
				case 0x1EE9: ch = 0x75; break;
				case 0x1EEA: ch = 0x55; break;
				case 0x1EEB: ch = 0x75; break;
				case 0x1EEC: ch = 0x55; break;
				case 0x1EED: ch = 0x75; break;
				case 0x1EEE: ch = 0x55; break;
				case 0x1EEF: ch = 0x75; break;
				case 0x1EF0: ch = 0x55; break;
				case 0x1EF1: ch = 0x75; break;
				case 0x1EF2: ch = 0x59; break;
				case 0x1EF3: ch = 0x79; break;
				case 0x1EF4: ch = 0x59; break;
				case 0x1EF5: ch = 0x79; break;
				case 0x1EF6: ch = 0x59; break;
				case 0x1EF7: ch = 0x79; break;
				case 0x1EF8: ch = 0x59; break;
				case 0x1EF9: ch = 0x79; break;
				case 0x1FEF: ch = 0x60; break;
				case 0x2000: ch = 0x20; break;
				case 0x2001: ch = 0x20; break;
				case 0x2002: ch = 0x20; break;
				case 0x2003: ch = 0x20; break;
				case 0x2004: ch = 0x20; break;
				case 0x2005: ch = 0x20; break;
				case 0x2006: ch = 0x20; break;
				case 0x2007: ch = 0x20; break;
				case 0x2008: ch = 0x20; break;
				case 0x2009: ch = 0x20; break;
				case 0x200A: ch = 0x20; break;
				case 0x2019: ch = 0xFF; break;
				case 0x201C: ch = 0xB4; break;
				case 0x201D: ch = 0xA1; break;
				case 0x201E: ch = 0xA5; break;
				case 0x2024: ch = 0x2E; break;
				case 0x202F: ch = 0x20; break;
				case 0x205F: ch = 0x20; break;
				case 0x2070: ch = 0x30; break;
				case 0x2071: ch = 0x69; break;
				case 0x2074:
				case 0x2075:
				case 0x2076:
				case 0x2077:
				case 0x2078:
				case 0x2079:
					ch -= 0x2040;
					break;
				case 0x207A: ch = 0x2B; break;
				case 0x207C: ch = 0x3D; break;
				case 0x207D: ch = 0x28; break;
				case 0x207E: ch = 0x29; break;
				case 0x207F: ch = 0x6E; break;
				case 0x2080:
				case 0x2081:
				case 0x2082:
				case 0x2083:
				case 0x2084:
				case 0x2085:
				case 0x2086:
				case 0x2087:
				case 0x2088:
				case 0x2089:
					ch -= 0x2050;
					break;
				case 0x208A: ch = 0x2B; break;
				case 0x208C: ch = 0x3D; break;
				case 0x208D: ch = 0x28; break;
				case 0x208E: ch = 0x29; break;
				case 0x2102: ch = 0x43; break;
				case 0x210A: ch = 0x67; break;
				case 0x210B: ch = 0x48; break;
				case 0x210C: ch = 0x48; break;
				case 0x210D: ch = 0x48; break;
				case 0x210E: ch = 0x68; break;
				case 0x2110: ch = 0x49; break;
				case 0x2111: ch = 0x49; break;
				case 0x2112: ch = 0x4C; break;
				case 0x2113: ch = 0x6C; break;
				case 0x2115: ch = 0x4E; break;
				case 0x2119: ch = 0x50; break;
				case 0x211A: ch = 0x51; break;
				case 0x211B: ch = 0x52; break;
				case 0x211C: ch = 0x52; break;
				case 0x211D: ch = 0x52; break;
				case 0x2124: ch = 0x5A; break;
				case 0x2128: ch = 0x5A; break;
				case 0x212A: ch = 0x4B; break;
				case 0x212B: ch = 0xC5; break;
				case 0x212C: ch = 0x42; break;
				case 0x212D: ch = 0x43; break;
				case 0x212F: ch = 0x65; break;
				case 0x2130: ch = 0x45; break;
				case 0x2131: ch = 0x46; break;
				case 0x2133: ch = 0x4D; break;
				case 0x2134: ch = 0x6F; break;
				case 0x2139: ch = 0x69; break;
				case 0x2145: ch = 0x44; break;
				case 0x2146: ch = 0x64; break;
				case 0x2147: ch = 0x65; break;
				case 0x2148: ch = 0x69; break;
				case 0x2149: ch = 0x6A; break;
				case 0x2160: ch = 0x49; break;
				case 0x2164: ch = 0x56; break;
				case 0x2169: ch = 0x58; break;
				case 0x216C: ch = 0x4C; break;
				case 0x216D: ch = 0x43; break;
				case 0x216E: ch = 0x44; break;
				case 0x216F: ch = 0x4D; break;
				case 0x2170: ch = 0x69; break;
				case 0x2174: ch = 0x76; break;
				case 0x2179: ch = 0x78; break;
				case 0x217C: ch = 0x6C; break;
				case 0x217D: ch = 0x63; break;
				case 0x217E: ch = 0x64; break;
				case 0x217F: ch = 0x6D; break;
				case 0x2260: ch = 0x3D; break;
				case 0x226E: ch = 0x3C; break;
				case 0x226F: ch = 0x3E; break;
				case 0x2460:
				case 0x2461:
				case 0x2462:
				case 0x2463:
				case 0x2464:
				case 0x2465:
				case 0x2466:
				case 0x2467:
				case 0x2468:
					ch -= 0x242F;
					break;
				case 0x24B6:
				case 0x24B7:
				case 0x24B8:
				case 0x24B9:
				case 0x24BA:
				case 0x24BB:
				case 0x24BC:
				case 0x24BD:
				case 0x24BE:
				case 0x24BF:
				case 0x24C0:
				case 0x24C1:
				case 0x24C2:
				case 0x24C3:
				case 0x24C4:
				case 0x24C5:
				case 0x24C6:
				case 0x24C7:
				case 0x24C8:
				case 0x24C9:
				case 0x24CA:
				case 0x24CB:
				case 0x24CC:
				case 0x24CD:
				case 0x24CE:
				case 0x24CF:
					ch -= 0x2475;
					break;
				case 0x24D0:
				case 0x24D1:
				case 0x24D2:
				case 0x24D3:
				case 0x24D4:
				case 0x24D5:
				case 0x24D6:
				case 0x24D7:
				case 0x24D8:
				case 0x24D9:
				case 0x24DA:
				case 0x24DB:
				case 0x24DC:
				case 0x24DD:
				case 0x24DE:
				case 0x24DF:
				case 0x24E0:
				case 0x24E1:
				case 0x24E2:
				case 0x24E3:
				case 0x24E4:
				case 0x24E5:
				case 0x24E6:
				case 0x24E7:
				case 0x24E8:
				case 0x24E9:
					ch -= 0x246F;
					break;
				case 0x24EA: ch = 0x30; break;
				case 0x3000: ch = 0x20; break;
				case 0xFB29: ch = 0x2B; break;
				case 0xFE33: ch = 0x5F; break;
				case 0xFE34: ch = 0x5F; break;
				case 0xFE35: ch = 0x28; break;
				case 0xFE36: ch = 0x29; break;
				case 0xFE37: ch = 0x7B; break;
				case 0xFE38: ch = 0x7D; break;
				case 0xFE4D: ch = 0x5F; break;
				case 0xFE4E: ch = 0x5F; break;
				case 0xFE4F: ch = 0x5F; break;
				case 0xFE50: ch = 0x2C; break;
				case 0xFE52: ch = 0x2E; break;
				case 0xFE54: ch = 0x3B; break;
				case 0xFE55: ch = 0x3A; break;
				case 0xFE57: ch = 0x21; break;
				case 0xFE59: ch = 0x28; break;
				case 0xFE5A: ch = 0x29; break;
				case 0xFE5B: ch = 0x7B; break;
				case 0xFE5C: ch = 0x7D; break;
				case 0xFE5F: ch = 0x23; break;
				case 0xFE60: ch = 0x26; break;
				case 0xFE61: ch = 0x2A; break;
				case 0xFE62: ch = 0x2B; break;
				case 0xFE63: ch = 0x2D; break;
				case 0xFE64: ch = 0x3C; break;
				case 0xFE65: ch = 0x3E; break;
				case 0xFE66: ch = 0x3D; break;
				case 0xFE68: ch = 0x5C; break;
				case 0xFE69: ch = 0x24; break;
				case 0xFE6A: ch = 0x25; break;
				case 0xFE6B: ch = 0x40; break;
				case 0xFF01:
				case 0xFF02:
				case 0xFF03:
				case 0xFF04:
				case 0xFF05:
				case 0xFF06:
				case 0xFF07:
				case 0xFF08:
				case 0xFF09:
				case 0xFF0A:
				case 0xFF0B:
				case 0xFF0C:
				case 0xFF0D:
				case 0xFF0E:
				case 0xFF0F:
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
				case 0xFF1A:
				case 0xFF1B:
				case 0xFF1C:
				case 0xFF1D:
				case 0xFF1E:
					ch -= 0xFEE0;
					break;
				case 0xFF20:
				case 0xFF21:
				case 0xFF22:
				case 0xFF23:
				case 0xFF24:
				case 0xFF25:
				case 0xFF26:
				case 0xFF27:
				case 0xFF28:
				case 0xFF29:
				case 0xFF2A:
				case 0xFF2B:
				case 0xFF2C:
				case 0xFF2D:
				case 0xFF2E:
				case 0xFF2F:
				case 0xFF30:
				case 0xFF31:
				case 0xFF32:
				case 0xFF33:
				case 0xFF34:
				case 0xFF35:
				case 0xFF36:
				case 0xFF37:
				case 0xFF38:
				case 0xFF39:
				case 0xFF3A:
				case 0xFF3B:
				case 0xFF3C:
				case 0xFF3D:
				case 0xFF3E:
				case 0xFF3F:
				case 0xFF40:
				case 0xFF41:
				case 0xFF42:
				case 0xFF43:
				case 0xFF44:
				case 0xFF45:
				case 0xFF46:
				case 0xFF47:
				case 0xFF48:
				case 0xFF49:
				case 0xFF4A:
				case 0xFF4B:
				case 0xFF4C:
				case 0xFF4D:
				case 0xFF4E:
				case 0xFF4F:
				case 0xFF50:
				case 0xFF51:
				case 0xFF52:
				case 0xFF53:
				case 0xFF54:
				case 0xFF55:
				case 0xFF56:
				case 0xFF57:
				case 0xFF58:
				case 0xFF59:
				case 0xFF5A:
				case 0xFF5B:
				case 0xFF5C:
				case 0xFF5D:
				case 0xFF5E:
					ch -= 0xFEE0;
					break;
				case 0xFFE0: ch = 0xA2; break;
				case 0xFFE1: ch = 0xA3; break;
				case 0xFFE2: ch = 0xAC; break;
				case 0xFFE4: ch = 0xA6; break;
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
}; // class CP28603

[Serializable]
public class ENCiso_8859_13 : CP28603
{
	public ENCiso_8859_13() : base() {}

}; // class ENCiso_8859_13

}; // namespace I18N.West
