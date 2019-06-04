/*
 * CP28596.cs - Arabic (ISO) code page.
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

// Generated from "windows-28596-2000.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.MidEast
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP28596 : ByteEncoding
{
	public CP28596()
		: base(28596, ToChars, "Arabic (ISO)",
		       "iso-8859-6", "iso-8859-6", "iso-8859-6",
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
		'\u007E', '\u007F', '\u0080', '\u0081', '\u0082', '\u0083', 
		'\u0084', '\u0085', '\u0086', '\u0087', '\u0088', '\u0089', 
		'\u008A', '\u008B', '\u008C', '\u008D', '\u008E', '\u008F', 
		'\u0090', '\u0091', '\u0092', '\u0093', '\u0094', '\u0095', 
		'\u0096', '\u0097', '\u0098', '\u0099', '\u009A', '\u009B', 
		'\u009C', '\u009D', '\u009E', '\u009F', '\u00A0', '\uF7C8', 
		'\uF7C9', '\uF7CA', '\u00A4', '\uF7CB', '\uF7CC', '\uF7CD', 
		'\uF7CE', '\uF7CF', '\uF7D0', '\uF7D1', '\u060C', '\u00AD', 
		'\uF7D2', '\uF7D3', '\uF7D4', '\uF7D5', '\uF7D6', '\uF7D7', 
		'\uF7D8', '\uF7D9', '\uF7DA', '\uF7DB', '\uF7DC', '\uF7DD', 
		'\uF7DE', '\u061B', '\uF7DF', '\uF7E0', '\uF7E1', '\u061F', 
		'\uF7E2', '\u0621', '\u0622', '\u0623', '\u0624', '\u0625', 
		'\u0626', '\u0627', '\u0628', '\u0629', '\u062A', '\u062B', 
		'\u062C', '\u062D', '\u062E', '\u062F', '\u0630', '\u0631', 
		'\u0632', '\u0633', '\u0634', '\u0635', '\u0636', '\u0637', 
		'\u0638', '\u0639', '\u063A', '\uF7E3', '\uF7E4', '\uF7E5', 
		'\uF7E6', '\uF7E7', '\u0640', '\u0641', '\u0642', '\u0643', 
		'\u0644', '\u0645', '\u0646', '\u0647', '\u0648', '\u0649', 
		'\u064A', '\u064B', '\u064C', '\u064D', '\u064E', '\u064F', 
		'\u0650', '\u0651', '\u0652', '\uF7E8', '\uF7E9', '\uF7EA', 
		'\uF7EB', '\uF7EC', '\uF7ED', '\uF7EE', '\uF7EF', '\uF7F0', 
		'\uF7F1', '\uF7F2', '\uF7F3', '\uF7F4', 
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
				case 0x00A1: ch = 0x21; break;
				case 0x00A2: ch = 0x63; break;
				case 0x00A5: ch = 0x59; break;
				case 0x00A6: ch = 0x7C; break;
				case 0x00A9: ch = 0x43; break;
				case 0x00AA: ch = 0x61; break;
				case 0x00AB: ch = 0x3C; break;
				case 0x00AE: ch = 0x52; break;
				case 0x00B2: ch = 0x32; break;
				case 0x00B3: ch = 0x33; break;
				case 0x00B7: ch = 0x2E; break;
				case 0x00B8: ch = 0x2C; break;
				case 0x00B9: ch = 0x31; break;
				case 0x00BA: ch = 0x6F; break;
				case 0x00BB: ch = 0x3E; break;
				case 0x00C0: ch = 0x41; break;
				case 0x00C1: ch = 0x41; break;
				case 0x00C2: ch = 0x41; break;
				case 0x00C3: ch = 0x41; break;
				case 0x00C4: ch = 0x41; break;
				case 0x00C5: ch = 0x41; break;
				case 0x00C6: ch = 0x41; break;
				case 0x00C7: ch = 0x43; break;
				case 0x00C8: ch = 0x45; break;
				case 0x00C9: ch = 0x45; break;
				case 0x00CA: ch = 0x45; break;
				case 0x00CB: ch = 0x45; break;
				case 0x00CC: ch = 0x49; break;
				case 0x00CD: ch = 0x49; break;
				case 0x00CE: ch = 0x49; break;
				case 0x00CF: ch = 0x49; break;
				case 0x00D0: ch = 0x44; break;
				case 0x00D1: ch = 0x4E; break;
				case 0x00D2: ch = 0x4F; break;
				case 0x00D3: ch = 0x4F; break;
				case 0x00D4: ch = 0x4F; break;
				case 0x00D5: ch = 0x4F; break;
				case 0x00D6: ch = 0x4F; break;
				case 0x00D8: ch = 0x4F; break;
				case 0x00D9: ch = 0x55; break;
				case 0x00DA: ch = 0x55; break;
				case 0x00DB: ch = 0x55; break;
				case 0x00DC: ch = 0x55; break;
				case 0x00DD: ch = 0x59; break;
				case 0x00E0: ch = 0x61; break;
				case 0x00E1: ch = 0x61; break;
				case 0x00E2: ch = 0x61; break;
				case 0x00E3: ch = 0x61; break;
				case 0x00E4: ch = 0x61; break;
				case 0x00E5: ch = 0x61; break;
				case 0x00E6: ch = 0x61; break;
				case 0x00E7: ch = 0x63; break;
				case 0x00E8: ch = 0x65; break;
				case 0x00E9: ch = 0x65; break;
				case 0x00EA: ch = 0x65; break;
				case 0x00EB: ch = 0x65; break;
				case 0x00EC: ch = 0x69; break;
				case 0x00ED: ch = 0x69; break;
				case 0x00EE: ch = 0x69; break;
				case 0x00EF: ch = 0x69; break;
				case 0x00F1: ch = 0x6E; break;
				case 0x00F2: ch = 0x6F; break;
				case 0x00F3: ch = 0x6F; break;
				case 0x00F4: ch = 0x6F; break;
				case 0x00F5: ch = 0x6F; break;
				case 0x00F6: ch = 0x6F; break;
				case 0x00F8: ch = 0x6F; break;
				case 0x00F9: ch = 0x75; break;
				case 0x00FA: ch = 0x75; break;
				case 0x00FB: ch = 0x75; break;
				case 0x00FC: ch = 0x75; break;
				case 0x00FD: ch = 0x79; break;
				case 0x00FF: ch = 0x79; break;
				case 0x0100: ch = 0x41; break;
				case 0x0101: ch = 0x61; break;
				case 0x0102: ch = 0x41; break;
				case 0x0103: ch = 0x61; break;
				case 0x0104: ch = 0x41; break;
				case 0x0105: ch = 0x61; break;
				case 0x0106: ch = 0x43; break;
				case 0x0107: ch = 0x63; break;
				case 0x0108: ch = 0x43; break;
				case 0x0109: ch = 0x63; break;
				case 0x010A: ch = 0x43; break;
				case 0x010B: ch = 0x63; break;
				case 0x010C: ch = 0x43; break;
				case 0x010D: ch = 0x63; break;
				case 0x010E: ch = 0x44; break;
				case 0x010F: ch = 0x64; break;
				case 0x0110: ch = 0x44; break;
				case 0x0111: ch = 0x64; break;
				case 0x0112: ch = 0x45; break;
				case 0x0113: ch = 0x65; break;
				case 0x0114: ch = 0x45; break;
				case 0x0115: ch = 0x65; break;
				case 0x0116: ch = 0x45; break;
				case 0x0117: ch = 0x65; break;
				case 0x0118: ch = 0x45; break;
				case 0x0119: ch = 0x65; break;
				case 0x011A: ch = 0x45; break;
				case 0x011B: ch = 0x65; break;
				case 0x011C: ch = 0x47; break;
				case 0x011D: ch = 0x67; break;
				case 0x011E: ch = 0x47; break;
				case 0x011F: ch = 0x67; break;
				case 0x0120: ch = 0x47; break;
				case 0x0121: ch = 0x67; break;
				case 0x0122: ch = 0x47; break;
				case 0x0123: ch = 0x67; break;
				case 0x0124: ch = 0x48; break;
				case 0x0125: ch = 0x68; break;
				case 0x0126: ch = 0x48; break;
				case 0x0127: ch = 0x68; break;
				case 0x0128: ch = 0x49; break;
				case 0x0129: ch = 0x69; break;
				case 0x012A: ch = 0x49; break;
				case 0x012B: ch = 0x69; break;
				case 0x012C: ch = 0x49; break;
				case 0x012D: ch = 0x69; break;
				case 0x012E: ch = 0x49; break;
				case 0x012F: ch = 0x69; break;
				case 0x0130: ch = 0x49; break;
				case 0x0131: ch = 0x69; break;
				case 0x0134: ch = 0x4A; break;
				case 0x0135: ch = 0x6A; break;
				case 0x0136: ch = 0x4B; break;
				case 0x0137: ch = 0x6B; break;
				case 0x0139: ch = 0x4C; break;
				case 0x013A: ch = 0x6C; break;
				case 0x013B: ch = 0x4C; break;
				case 0x013C: ch = 0x6C; break;
				case 0x013D: ch = 0x4C; break;
				case 0x013E: ch = 0x6C; break;
				case 0x0141: ch = 0x4C; break;
				case 0x0142: ch = 0x6C; break;
				case 0x0143: ch = 0x4E; break;
				case 0x0144: ch = 0x6E; break;
				case 0x0145: ch = 0x4E; break;
				case 0x0146: ch = 0x6E; break;
				case 0x0147: ch = 0x4E; break;
				case 0x0148: ch = 0x6E; break;
				case 0x014C: ch = 0x4F; break;
				case 0x014D: ch = 0x6F; break;
				case 0x014E: ch = 0x4F; break;
				case 0x014F: ch = 0x6F; break;
				case 0x0150: ch = 0x4F; break;
				case 0x0151: ch = 0x6F; break;
				case 0x0152: ch = 0x4F; break;
				case 0x0153: ch = 0x6F; break;
				case 0x0154: ch = 0x52; break;
				case 0x0155: ch = 0x72; break;
				case 0x0156: ch = 0x52; break;
				case 0x0157: ch = 0x72; break;
				case 0x0158: ch = 0x52; break;
				case 0x0159: ch = 0x72; break;
				case 0x015A: ch = 0x53; break;
				case 0x015B: ch = 0x73; break;
				case 0x015C: ch = 0x53; break;
				case 0x015D: ch = 0x73; break;
				case 0x015E: ch = 0x53; break;
				case 0x015F: ch = 0x73; break;
				case 0x0160: ch = 0x53; break;
				case 0x0161: ch = 0x73; break;
				case 0x0162: ch = 0x54; break;
				case 0x0163: ch = 0x74; break;
				case 0x0164: ch = 0x54; break;
				case 0x0165: ch = 0x74; break;
				case 0x0166: ch = 0x54; break;
				case 0x0167: ch = 0x74; break;
				case 0x0168: ch = 0x55; break;
				case 0x0169: ch = 0x75; break;
				case 0x016A: ch = 0x55; break;
				case 0x016B: ch = 0x75; break;
				case 0x016C: ch = 0x55; break;
				case 0x016D: ch = 0x75; break;
				case 0x016E: ch = 0x55; break;
				case 0x016F: ch = 0x75; break;
				case 0x0170: ch = 0x55; break;
				case 0x0171: ch = 0x75; break;
				case 0x0172: ch = 0x55; break;
				case 0x0173: ch = 0x75; break;
				case 0x0174: ch = 0x57; break;
				case 0x0175: ch = 0x77; break;
				case 0x0176: ch = 0x59; break;
				case 0x0177: ch = 0x79; break;
				case 0x0178: ch = 0x59; break;
				case 0x0179: ch = 0x5A; break;
				case 0x017A: ch = 0x7A; break;
				case 0x017B: ch = 0x5A; break;
				case 0x017C: ch = 0x7A; break;
				case 0x017D: ch = 0x5A; break;
				case 0x017E: ch = 0x7A; break;
				case 0x0180: ch = 0x62; break;
				case 0x0189: ch = 0x44; break;
				case 0x0191: ch = 0x46; break;
				case 0x0192: ch = 0x66; break;
				case 0x0197: ch = 0x49; break;
				case 0x019A: ch = 0x6C; break;
				case 0x019F: ch = 0x4F; break;
				case 0x01A0: ch = 0x4F; break;
				case 0x01A1: ch = 0x6F; break;
				case 0x01AB: ch = 0x74; break;
				case 0x01AE: ch = 0x54; break;
				case 0x01AF: ch = 0x55; break;
				case 0x01B0: ch = 0x75; break;
				case 0x01B6: ch = 0x7A; break;
				case 0x01CD: ch = 0x41; break;
				case 0x01CE: ch = 0x61; break;
				case 0x01CF: ch = 0x49; break;
				case 0x01D0: ch = 0x69; break;
				case 0x01D1: ch = 0x4F; break;
				case 0x01D2: ch = 0x6F; break;
				case 0x01D3: ch = 0x55; break;
				case 0x01D4: ch = 0x75; break;
				case 0x01D5: ch = 0x55; break;
				case 0x01D6: ch = 0x75; break;
				case 0x01D7: ch = 0x55; break;
				case 0x01D8: ch = 0x75; break;
				case 0x01D9: ch = 0x55; break;
				case 0x01DA: ch = 0x75; break;
				case 0x01DB: ch = 0x55; break;
				case 0x01DC: ch = 0x75; break;
				case 0x01DE: ch = 0x41; break;
				case 0x01DF: ch = 0x61; break;
				case 0x01E4: ch = 0x47; break;
				case 0x01E5: ch = 0x67; break;
				case 0x01E6: ch = 0x47; break;
				case 0x01E7: ch = 0x67; break;
				case 0x01E8: ch = 0x4B; break;
				case 0x01E9: ch = 0x6B; break;
				case 0x01EA: ch = 0x4F; break;
				case 0x01EB: ch = 0x6F; break;
				case 0x01EC: ch = 0x4F; break;
				case 0x01ED: ch = 0x6F; break;
				case 0x01F0: ch = 0x6A; break;
				case 0x0261: ch = 0x67; break;
				case 0x02B9: ch = 0x27; break;
				case 0x02BA: ch = 0x22; break;
				case 0x02BC: ch = 0x27; break;
				case 0x02C4: ch = 0x5E; break;
				case 0x02C6: ch = 0x5E; break;
				case 0x02C8: ch = 0x27; break;
				case 0x02CB: ch = 0x60; break;
				case 0x02CD: ch = 0x5F; break;
				case 0x02DC: ch = 0x7E; break;
				case 0x0300: ch = 0x60; break;
				case 0x0302: ch = 0x5E; break;
				case 0x0303: ch = 0x7E; break;
				case 0x030E: ch = 0x22; break;
				case 0x0331: ch = 0x5F; break;
				case 0x0332: ch = 0x5F; break;
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
				case 0x2000: ch = 0x20; break;
				case 0x2001: ch = 0x20; break;
				case 0x2002: ch = 0x20; break;
				case 0x2003: ch = 0x20; break;
				case 0x2004: ch = 0x20; break;
				case 0x2005: ch = 0x20; break;
				case 0x2006: ch = 0x20; break;
				case 0x2010: ch = 0x2D; break;
				case 0x2011: ch = 0x2D; break;
				case 0x2013: ch = 0x2D; break;
				case 0x2014: ch = 0x2D; break;
				case 0x2018: ch = 0x27; break;
				case 0x2019: ch = 0x27; break;
				case 0x201A: ch = 0x2C; break;
				case 0x201C: ch = 0x22; break;
				case 0x201D: ch = 0x22; break;
				case 0x201E: ch = 0x22; break;
				case 0x2022: ch = 0x2E; break;
				case 0x2026: ch = 0x2E; break;
				case 0x2032: ch = 0x27; break;
				case 0x2035: ch = 0x60; break;
				case 0x2039: ch = 0x3C; break;
				case 0x203A: ch = 0x3E; break;
				case 0x2122: ch = 0x54; break;
				case 0xF7C8: ch = 0xA1; break;
				case 0xF7C9: ch = 0xA2; break;
				case 0xF7CA: ch = 0xA3; break;
				case 0xF7CB:
				case 0xF7CC:
				case 0xF7CD:
				case 0xF7CE:
				case 0xF7CF:
				case 0xF7D0:
				case 0xF7D1:
					ch -= 0xF726;
					break;
				case 0xF7D2:
				case 0xF7D3:
				case 0xF7D4:
				case 0xF7D5:
				case 0xF7D6:
				case 0xF7D7:
				case 0xF7D8:
				case 0xF7D9:
				case 0xF7DA:
				case 0xF7DB:
				case 0xF7DC:
				case 0xF7DD:
				case 0xF7DE:
					ch -= 0xF724;
					break;
				case 0xF7DF: ch = 0xBC; break;
				case 0xF7E0: ch = 0xBD; break;
				case 0xF7E1: ch = 0xBE; break;
				case 0xF7E2: ch = 0xC0; break;
				case 0xF7E3:
				case 0xF7E4:
				case 0xF7E5:
				case 0xF7E6:
				case 0xF7E7:
					ch -= 0xF708;
					break;
				case 0xF7E8:
				case 0xF7E9:
				case 0xF7EA:
				case 0xF7EB:
				case 0xF7EC:
				case 0xF7ED:
				case 0xF7EE:
				case 0xF7EF:
				case 0xF7F0:
				case 0xF7F1:
				case 0xF7F2:
				case 0xF7F3:
				case 0xF7F4:
					ch -= 0xF6F5;
					break;
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
}; // class CP28596

[Serializable]
public class ENCiso_8859_6 : CP28596
{
	public ENCiso_8859_6() : base() {}

}; // class ENCiso_8859_6

}; // namespace I18N.MidEast
