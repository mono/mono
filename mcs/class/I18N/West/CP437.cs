/*
 * CP437.cs - OEM United States code page.
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

// Generated from "windows-437-2000.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.West
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP437 : ByteEncoding
{
	public CP437()
		: base(437, ToChars, "OEM United States",
		       "IBM437", "IBM437", "IBM437",
		       false, false, false, false, 1252)
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
		'\u007E', '\u007F', '\u00C7', '\u00FC', '\u00E9', '\u00E2', 
		'\u00E4', '\u00E0', '\u00E5', '\u00E7', '\u00EA', '\u00EB', 
		'\u00E8', '\u00EF', '\u00EE', '\u00EC', '\u00C4', '\u00C5', 
		'\u00C9', '\u00E6', '\u00C6', '\u00F4', '\u00F6', '\u00F2', 
		'\u00FB', '\u00F9', '\u00FF', '\u00D6', '\u00DC', '\u00A2', 
		'\u00A3', '\u00A5', '\u20A7', '\u0192', '\u00E1', '\u00ED', 
		'\u00F3', '\u00FA', '\u00F1', '\u00D1', '\u00AA', '\u00BA', 
		'\u00BF', '\u2310', '\u00AC', '\u00BD', '\u00BC', '\u00A1', 
		'\u00AB', '\u00BB', '\u2591', '\u2592', '\u2593', '\u2502', 
		'\u2524', '\u2561', '\u2562', '\u2556', '\u2555', '\u2563', 
		'\u2551', '\u2557', '\u255D', '\u255C', '\u255B', '\u2510', 
		'\u2514', '\u2534', '\u252C', '\u251C', '\u2500', '\u253C', 
		'\u255E', '\u255F', '\u255A', '\u2554', '\u2569', '\u2566', 
		'\u2560', '\u2550', '\u256C', '\u2567', '\u2568', '\u2564', 
		'\u2565', '\u2559', '\u2558', '\u2552', '\u2553', '\u256B', 
		'\u256A', '\u2518', '\u250C', '\u2588', '\u2584', '\u258C', 
		'\u2590', '\u2580', '\u03B1', '\u00DF', '\u0393', '\u03C0', 
		'\u03A3', '\u03C3', '\u00B5', '\u03C4', '\u03A6', '\u0398', 
		'\u03A9', '\u03B4', '\u221E', '\u03C6', '\u03B5', '\u2229', 
		'\u2261', '\u00B1', '\u2265', '\u2264', '\u2320', '\u2321', 
		'\u00F7', '\u2248', '\u00B0', '\u2219', '\u00B7', '\u221A', 
		'\u207F', '\u00B2', '\u25A0', '\u00A0', 
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
				case 0x00A0: ch = 0xFF; break;
				case 0x00A1: ch = 0xAD; break;
				case 0x00A2: ch = 0x9B; break;
				case 0x00A3: ch = 0x9C; break;
				case 0x00A4: ch = 0x0F; break;
				case 0x00A5: ch = 0x9D; break;
				case 0x00A6: ch = 0xDD; break;
				case 0x00A7: ch = 0x15; break;
				case 0x00A8: ch = 0x22; break;
				case 0x00A9: ch = 0x63; break;
				case 0x00AA: ch = 0xA6; break;
				case 0x00AB: ch = 0xAE; break;
				case 0x00AC: ch = 0xAA; break;
				case 0x00AD: ch = 0x2D; break;
				case 0x00AE: ch = 0x72; break;
				case 0x00AF: ch = 0x5F; break;
				case 0x00B0: ch = 0xF8; break;
				case 0x00B1: ch = 0xF1; break;
				case 0x00B2: ch = 0xFD; break;
				case 0x00B3: ch = 0x33; break;
				case 0x00B4: ch = 0x27; break;
				case 0x00B5: ch = 0xE6; break;
				case 0x00B6: ch = 0x14; break;
				case 0x00B7: ch = 0xFA; break;
				case 0x00B8: ch = 0x2C; break;
				case 0x00B9: ch = 0x31; break;
				case 0x00BA: ch = 0xA7; break;
				case 0x00BB: ch = 0xAF; break;
				case 0x00BC: ch = 0xAC; break;
				case 0x00BD: ch = 0xAB; break;
				case 0x00BE: ch = 0x5F; break;
				case 0x00BF: ch = 0xA8; break;
				case 0x00C0: ch = 0x41; break;
				case 0x00C1: ch = 0x41; break;
				case 0x00C2: ch = 0x41; break;
				case 0x00C3: ch = 0x41; break;
				case 0x00C4: ch = 0x8E; break;
				case 0x00C5: ch = 0x8F; break;
				case 0x00C6: ch = 0x92; break;
				case 0x00C7: ch = 0x80; break;
				case 0x00C8: ch = 0x45; break;
				case 0x00C9: ch = 0x90; break;
				case 0x00CA: ch = 0x45; break;
				case 0x00CB: ch = 0x45; break;
				case 0x00CC: ch = 0x49; break;
				case 0x00CD: ch = 0x49; break;
				case 0x00CE: ch = 0x49; break;
				case 0x00CF: ch = 0x49; break;
				case 0x00D0: ch = 0x44; break;
				case 0x00D1: ch = 0xA5; break;
				case 0x00D2: ch = 0x4F; break;
				case 0x00D3: ch = 0x4F; break;
				case 0x00D4: ch = 0x4F; break;
				case 0x00D5: ch = 0x4F; break;
				case 0x00D6: ch = 0x99; break;
				case 0x00D7: ch = 0x78; break;
				case 0x00D8: ch = 0x4F; break;
				case 0x00D9: ch = 0x55; break;
				case 0x00DA: ch = 0x55; break;
				case 0x00DB: ch = 0x55; break;
				case 0x00DC: ch = 0x9A; break;
				case 0x00DD: ch = 0x59; break;
				case 0x00DE: ch = 0x5F; break;
				case 0x00DF: ch = 0xE1; break;
				case 0x00E0: ch = 0x85; break;
				case 0x00E1: ch = 0xA0; break;
				case 0x00E2: ch = 0x83; break;
				case 0x00E3: ch = 0x61; break;
				case 0x00E4: ch = 0x84; break;
				case 0x00E5: ch = 0x86; break;
				case 0x00E6: ch = 0x91; break;
				case 0x00E7: ch = 0x87; break;
				case 0x00E8: ch = 0x8A; break;
				case 0x00E9: ch = 0x82; break;
				case 0x00EA: ch = 0x88; break;
				case 0x00EB: ch = 0x89; break;
				case 0x00EC: ch = 0x8D; break;
				case 0x00ED: ch = 0xA1; break;
				case 0x00EE: ch = 0x8C; break;
				case 0x00EF: ch = 0x8B; break;
				case 0x00F0: ch = 0x64; break;
				case 0x00F1: ch = 0xA4; break;
				case 0x00F2: ch = 0x95; break;
				case 0x00F3: ch = 0xA2; break;
				case 0x00F4: ch = 0x93; break;
				case 0x00F5: ch = 0x6F; break;
				case 0x00F6: ch = 0x94; break;
				case 0x00F7: ch = 0xF6; break;
				case 0x00F8: ch = 0x6F; break;
				case 0x00F9: ch = 0x97; break;
				case 0x00FA: ch = 0xA3; break;
				case 0x00FB: ch = 0x96; break;
				case 0x00FC: ch = 0x81; break;
				case 0x00FD: ch = 0x79; break;
				case 0x00FE: ch = 0x5F; break;
				case 0x00FF: ch = 0x98; break;
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
				case 0x0191: ch = 0x9F; break;
				case 0x0192: ch = 0x9F; break;
				case 0x0197: ch = 0x49; break;
				case 0x019A: ch = 0x6C; break;
				case 0x019F: ch = 0x4F; break;
				case 0x01A0: ch = 0x4F; break;
				case 0x01A1: ch = 0x6F; break;
				case 0x01A9: ch = 0xE4; break;
				case 0x01AB: ch = 0x74; break;
				case 0x01AE: ch = 0x54; break;
				case 0x01AF: ch = 0x55; break;
				case 0x01B0: ch = 0x75; break;
				case 0x01B6: ch = 0x7A; break;
				case 0x01C0: ch = 0x7C; break;
				case 0x01C3: ch = 0x21; break;
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
				case 0x0278: ch = 0xED; break;
				case 0x02B9: ch = 0x27; break;
				case 0x02BA: ch = 0x22; break;
				case 0x02BC: ch = 0x27; break;
				case 0x02C4: ch = 0x5E; break;
				case 0x02C6: ch = 0x5E; break;
				case 0x02C8: ch = 0x27; break;
				case 0x02C9: ch = 0xC4; break;
				case 0x02CA: ch = 0x27; break;
				case 0x02CB: ch = 0x60; break;
				case 0x02CD: ch = 0x5F; break;
				case 0x02DA: ch = 0xF8; break;
				case 0x02DC: ch = 0x7E; break;
				case 0x0300: ch = 0x60; break;
				case 0x0301: ch = 0x27; break;
				case 0x0302: ch = 0x5E; break;
				case 0x0303: ch = 0x7E; break;
				case 0x0304: ch = 0xC4; break;
				case 0x0308: ch = 0x22; break;
				case 0x030A: ch = 0xF8; break;
				case 0x030E: ch = 0x22; break;
				case 0x0327: ch = 0x2C; break;
				case 0x0331: ch = 0x5F; break;
				case 0x0332: ch = 0x5F; break;
				case 0x037E: ch = 0x3B; break;
				case 0x0391: ch = 0xE0; break;
				case 0x0393: ch = 0xE2; break;
				case 0x0394: ch = 0xEB; break;
				case 0x0395: ch = 0xEE; break;
				case 0x0398: ch = 0xE9; break;
				case 0x03A0: ch = 0xE3; break;
				case 0x03A3: ch = 0xE4; break;
				case 0x03A4: ch = 0xE7; break;
				case 0x03A6: ch = 0xE8; break;
				case 0x03A9: ch = 0xEA; break;
				case 0x03B1: ch = 0xE0; break;
				case 0x03B2: ch = 0xE1; break;
				case 0x03B4: ch = 0xEB; break;
				case 0x03B5: ch = 0xEE; break;
				case 0x03BC: ch = 0xE6; break;
				case 0x03C0: ch = 0xE3; break;
				case 0x03C3: ch = 0xE5; break;
				case 0x03C4: ch = 0xE7; break;
				case 0x03C6: ch = 0xED; break;
				case 0x04BB: ch = 0x68; break;
				case 0x0589: ch = 0x3A; break;
				case 0x066A: ch = 0x25; break;
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
				case 0x2017: ch = 0x5F; break;
				case 0x2018: ch = 0x60; break;
				case 0x2019: ch = 0x27; break;
				case 0x201A: ch = 0x2C; break;
				case 0x201C: ch = 0x22; break;
				case 0x201D: ch = 0x22; break;
				case 0x201E: ch = 0x2C; break;
				case 0x2020: ch = 0x2B; break;
				case 0x2021: ch = 0xD8; break;
				case 0x2022: ch = 0x07; break;
				case 0x2024: ch = 0xFA; break;
				case 0x2026: ch = 0x2E; break;
				case 0x2030: ch = 0x25; break;
				case 0x2032: ch = 0x27; break;
				case 0x2035: ch = 0x60; break;
				case 0x2039: ch = 0x3C; break;
				case 0x203A: ch = 0x3E; break;
				case 0x203C: ch = 0x13; break;
				case 0x2044: ch = 0x2F; break;
				case 0x2070: ch = 0xF8; break;
				case 0x2074:
				case 0x2075:
				case 0x2076:
				case 0x2077:
				case 0x2078:
					ch -= 0x2040;
					break;
				case 0x207F: ch = 0xFC; break;
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
				case 0x20A4: ch = 0x9C; break;
				case 0x20A7: ch = 0x9E; break;
				case 0x20DD: ch = 0x09; break;
				case 0x2102: ch = 0x43; break;
				case 0x2107: ch = 0x45; break;
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
				case 0x2118: ch = 0x50; break;
				case 0x2119: ch = 0x50; break;
				case 0x211A: ch = 0x51; break;
				case 0x211B: ch = 0x52; break;
				case 0x211C: ch = 0x52; break;
				case 0x211D: ch = 0x52; break;
				case 0x2122: ch = 0x54; break;
				case 0x2124: ch = 0x5A; break;
				case 0x2126: ch = 0xEA; break;
				case 0x2128: ch = 0x5A; break;
				case 0x212A: ch = 0x4B; break;
				case 0x212B: ch = 0x8F; break;
				case 0x212C: ch = 0x42; break;
				case 0x212D: ch = 0x43; break;
				case 0x212E: ch = 0x65; break;
				case 0x212F: ch = 0x65; break;
				case 0x2130: ch = 0x45; break;
				case 0x2131: ch = 0x46; break;
				case 0x2133: ch = 0x4D; break;
				case 0x2134: ch = 0x6F; break;
				case 0x2190: ch = 0x1B; break;
				case 0x2191: ch = 0x18; break;
				case 0x2192: ch = 0x1A; break;
				case 0x2193: ch = 0x19; break;
				case 0x2194: ch = 0x1D; break;
				case 0x2195: ch = 0x12; break;
				case 0x21A8: ch = 0x17; break;
				case 0x2205: ch = 0xED; break;
				case 0x2211: ch = 0xE4; break;
				case 0x2212: ch = 0x2D; break;
				case 0x2213: ch = 0xF1; break;
				case 0x2215: ch = 0x2F; break;
				case 0x2216: ch = 0x5C; break;
				case 0x2217: ch = 0x2A; break;
				case 0x2218: ch = 0xF8; break;
				case 0x2219: ch = 0xF9; break;
				case 0x221A: ch = 0xFB; break;
				case 0x221E: ch = 0xEC; break;
				case 0x221F: ch = 0x1C; break;
				case 0x2223: ch = 0x7C; break;
				case 0x2229: ch = 0xEF; break;
				case 0x2236: ch = 0x3A; break;
				case 0x223C: ch = 0x7E; break;
				case 0x2248: ch = 0xF7; break;
				case 0x2261: ch = 0xF0; break;
				case 0x2264: ch = 0xF3; break;
				case 0x2265: ch = 0xF2; break;
				case 0x226A: ch = 0xAE; break;
				case 0x226B: ch = 0xAF; break;
				case 0x22C5: ch = 0xFA; break;
				case 0x2302: ch = 0x7F; break;
				case 0x2303: ch = 0x5E; break;
				case 0x2310: ch = 0xA9; break;
				case 0x2320: ch = 0xF4; break;
				case 0x2321: ch = 0xF5; break;
				case 0x2329: ch = 0x3C; break;
				case 0x232A: ch = 0x3E; break;
				case 0x2500: ch = 0xC4; break;
				case 0x2502: ch = 0xB3; break;
				case 0x250C: ch = 0xDA; break;
				case 0x2510: ch = 0xBF; break;
				case 0x2514: ch = 0xC0; break;
				case 0x2518: ch = 0xD9; break;
				case 0x251C: ch = 0xC3; break;
				case 0x2524: ch = 0xB4; break;
				case 0x252C: ch = 0xC2; break;
				case 0x2534: ch = 0xC1; break;
				case 0x253C: ch = 0xC5; break;
				case 0x2550: ch = 0xCD; break;
				case 0x2551: ch = 0xBA; break;
				case 0x2552: ch = 0xD5; break;
				case 0x2553: ch = 0xD6; break;
				case 0x2554: ch = 0xC9; break;
				case 0x2555: ch = 0xB8; break;
				case 0x2556: ch = 0xB7; break;
				case 0x2557: ch = 0xBB; break;
				case 0x2558: ch = 0xD4; break;
				case 0x2559: ch = 0xD3; break;
				case 0x255A: ch = 0xC8; break;
				case 0x255B: ch = 0xBE; break;
				case 0x255C: ch = 0xBD; break;
				case 0x255D: ch = 0xBC; break;
				case 0x255E: ch = 0xC6; break;
				case 0x255F: ch = 0xC7; break;
				case 0x2560: ch = 0xCC; break;
				case 0x2561: ch = 0xB5; break;
				case 0x2562: ch = 0xB6; break;
				case 0x2563: ch = 0xB9; break;
				case 0x2564: ch = 0xD1; break;
				case 0x2565: ch = 0xD2; break;
				case 0x2566: ch = 0xCB; break;
				case 0x2567: ch = 0xCF; break;
				case 0x2568: ch = 0xD0; break;
				case 0x2569: ch = 0xCA; break;
				case 0x256A: ch = 0xD8; break;
				case 0x256B: ch = 0xD7; break;
				case 0x256C: ch = 0xCE; break;
				case 0x2580: ch = 0xDF; break;
				case 0x2584: ch = 0xDC; break;
				case 0x2588: ch = 0xDB; break;
				case 0x258C: ch = 0xDD; break;
				case 0x2590: ch = 0xDE; break;
				case 0x2591: ch = 0xB0; break;
				case 0x2592: ch = 0xB1; break;
				case 0x2593: ch = 0xB2; break;
				case 0x25A0: ch = 0xFE; break;
				case 0x25AC: ch = 0x16; break;
				case 0x25B2: ch = 0x1E; break;
				case 0x25BA: ch = 0x10; break;
				case 0x25BC: ch = 0x1F; break;
				case 0x25C4: ch = 0x11; break;
				case 0x25CB: ch = 0x09; break;
				case 0x25D8: ch = 0x08; break;
				case 0x25D9: ch = 0x0A; break;
				case 0x263A: ch = 0x01; break;
				case 0x263B: ch = 0x02; break;
				case 0x263C: ch = 0x0F; break;
				case 0x2640: ch = 0x0C; break;
				case 0x2642: ch = 0x0B; break;
				case 0x2660: ch = 0x06; break;
				case 0x2663: ch = 0x05; break;
				case 0x2665: ch = 0x03; break;
				case 0x2666: ch = 0x04; break;
				case 0x266A: ch = 0x0D; break;
				case 0x266B: ch = 0x0E; break;
				case 0x2713: ch = 0xFB; break;
				case 0x2758: ch = 0x7C; break;
				case 0x3000: ch = 0x20; break;
				case 0x3007: ch = 0x09; break;
				case 0x3008: ch = 0x3C; break;
				case 0x3009: ch = 0x3E; break;
				case 0x300A: ch = 0xAE; break;
				case 0x300B: ch = 0xAF; break;
				case 0x301A: ch = 0x5B; break;
				case 0x301B: ch = 0x5D; break;
				case 0x30FB: ch = 0xFA; break;
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
					continue;
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
				case 0x00A0: ch = 0xFF; break;
				case 0x00A1: ch = 0xAD; break;
				case 0x00A2: ch = 0x9B; break;
				case 0x00A3: ch = 0x9C; break;
				case 0x00A4: ch = 0x0F; break;
				case 0x00A5: ch = 0x9D; break;
				case 0x00A6: ch = 0xDD; break;
				case 0x00A7: ch = 0x15; break;
				case 0x00A8: ch = 0x22; break;
				case 0x00A9: ch = 0x63; break;
				case 0x00AA: ch = 0xA6; break;
				case 0x00AB: ch = 0xAE; break;
				case 0x00AC: ch = 0xAA; break;
				case 0x00AD: ch = 0x2D; break;
				case 0x00AE: ch = 0x72; break;
				case 0x00AF: ch = 0x5F; break;
				case 0x00B0: ch = 0xF8; break;
				case 0x00B1: ch = 0xF1; break;
				case 0x00B2: ch = 0xFD; break;
				case 0x00B3: ch = 0x33; break;
				case 0x00B4: ch = 0x27; break;
				case 0x00B5: ch = 0xE6; break;
				case 0x00B6: ch = 0x14; break;
				case 0x00B7: ch = 0xFA; break;
				case 0x00B8: ch = 0x2C; break;
				case 0x00B9: ch = 0x31; break;
				case 0x00BA: ch = 0xA7; break;
				case 0x00BB: ch = 0xAF; break;
				case 0x00BC: ch = 0xAC; break;
				case 0x00BD: ch = 0xAB; break;
				case 0x00BE: ch = 0x5F; break;
				case 0x00BF: ch = 0xA8; break;
				case 0x00C0: ch = 0x41; break;
				case 0x00C1: ch = 0x41; break;
				case 0x00C2: ch = 0x41; break;
				case 0x00C3: ch = 0x41; break;
				case 0x00C4: ch = 0x8E; break;
				case 0x00C5: ch = 0x8F; break;
				case 0x00C6: ch = 0x92; break;
				case 0x00C7: ch = 0x80; break;
				case 0x00C8: ch = 0x45; break;
				case 0x00C9: ch = 0x90; break;
				case 0x00CA: ch = 0x45; break;
				case 0x00CB: ch = 0x45; break;
				case 0x00CC: ch = 0x49; break;
				case 0x00CD: ch = 0x49; break;
				case 0x00CE: ch = 0x49; break;
				case 0x00CF: ch = 0x49; break;
				case 0x00D0: ch = 0x44; break;
				case 0x00D1: ch = 0xA5; break;
				case 0x00D2: ch = 0x4F; break;
				case 0x00D3: ch = 0x4F; break;
				case 0x00D4: ch = 0x4F; break;
				case 0x00D5: ch = 0x4F; break;
				case 0x00D6: ch = 0x99; break;
				case 0x00D7: ch = 0x78; break;
				case 0x00D8: ch = 0x4F; break;
				case 0x00D9: ch = 0x55; break;
				case 0x00DA: ch = 0x55; break;
				case 0x00DB: ch = 0x55; break;
				case 0x00DC: ch = 0x9A; break;
				case 0x00DD: ch = 0x59; break;
				case 0x00DE: ch = 0x5F; break;
				case 0x00DF: ch = 0xE1; break;
				case 0x00E0: ch = 0x85; break;
				case 0x00E1: ch = 0xA0; break;
				case 0x00E2: ch = 0x83; break;
				case 0x00E3: ch = 0x61; break;
				case 0x00E4: ch = 0x84; break;
				case 0x00E5: ch = 0x86; break;
				case 0x00E6: ch = 0x91; break;
				case 0x00E7: ch = 0x87; break;
				case 0x00E8: ch = 0x8A; break;
				case 0x00E9: ch = 0x82; break;
				case 0x00EA: ch = 0x88; break;
				case 0x00EB: ch = 0x89; break;
				case 0x00EC: ch = 0x8D; break;
				case 0x00ED: ch = 0xA1; break;
				case 0x00EE: ch = 0x8C; break;
				case 0x00EF: ch = 0x8B; break;
				case 0x00F0: ch = 0x64; break;
				case 0x00F1: ch = 0xA4; break;
				case 0x00F2: ch = 0x95; break;
				case 0x00F3: ch = 0xA2; break;
				case 0x00F4: ch = 0x93; break;
				case 0x00F5: ch = 0x6F; break;
				case 0x00F6: ch = 0x94; break;
				case 0x00F7: ch = 0xF6; break;
				case 0x00F8: ch = 0x6F; break;
				case 0x00F9: ch = 0x97; break;
				case 0x00FA: ch = 0xA3; break;
				case 0x00FB: ch = 0x96; break;
				case 0x00FC: ch = 0x81; break;
				case 0x00FD: ch = 0x79; break;
				case 0x00FE: ch = 0x5F; break;
				case 0x00FF: ch = 0x98; break;
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
				case 0x0191: ch = 0x9F; break;
				case 0x0192: ch = 0x9F; break;
				case 0x0197: ch = 0x49; break;
				case 0x019A: ch = 0x6C; break;
				case 0x019F: ch = 0x4F; break;
				case 0x01A0: ch = 0x4F; break;
				case 0x01A1: ch = 0x6F; break;
				case 0x01A9: ch = 0xE4; break;
				case 0x01AB: ch = 0x74; break;
				case 0x01AE: ch = 0x54; break;
				case 0x01AF: ch = 0x55; break;
				case 0x01B0: ch = 0x75; break;
				case 0x01B6: ch = 0x7A; break;
				case 0x01C0: ch = 0x7C; break;
				case 0x01C3: ch = 0x21; break;
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
				case 0x0278: ch = 0xED; break;
				case 0x02B9: ch = 0x27; break;
				case 0x02BA: ch = 0x22; break;
				case 0x02BC: ch = 0x27; break;
				case 0x02C4: ch = 0x5E; break;
				case 0x02C6: ch = 0x5E; break;
				case 0x02C8: ch = 0x27; break;
				case 0x02C9: ch = 0xC4; break;
				case 0x02CA: ch = 0x27; break;
				case 0x02CB: ch = 0x60; break;
				case 0x02CD: ch = 0x5F; break;
				case 0x02DA: ch = 0xF8; break;
				case 0x02DC: ch = 0x7E; break;
				case 0x0300: ch = 0x60; break;
				case 0x0301: ch = 0x27; break;
				case 0x0302: ch = 0x5E; break;
				case 0x0303: ch = 0x7E; break;
				case 0x0304: ch = 0xC4; break;
				case 0x0308: ch = 0x22; break;
				case 0x030A: ch = 0xF8; break;
				case 0x030E: ch = 0x22; break;
				case 0x0327: ch = 0x2C; break;
				case 0x0331: ch = 0x5F; break;
				case 0x0332: ch = 0x5F; break;
				case 0x037E: ch = 0x3B; break;
				case 0x0391: ch = 0xE0; break;
				case 0x0393: ch = 0xE2; break;
				case 0x0394: ch = 0xEB; break;
				case 0x0395: ch = 0xEE; break;
				case 0x0398: ch = 0xE9; break;
				case 0x03A0: ch = 0xE3; break;
				case 0x03A3: ch = 0xE4; break;
				case 0x03A4: ch = 0xE7; break;
				case 0x03A6: ch = 0xE8; break;
				case 0x03A9: ch = 0xEA; break;
				case 0x03B1: ch = 0xE0; break;
				case 0x03B2: ch = 0xE1; break;
				case 0x03B4: ch = 0xEB; break;
				case 0x03B5: ch = 0xEE; break;
				case 0x03BC: ch = 0xE6; break;
				case 0x03C0: ch = 0xE3; break;
				case 0x03C3: ch = 0xE5; break;
				case 0x03C4: ch = 0xE7; break;
				case 0x03C6: ch = 0xED; break;
				case 0x04BB: ch = 0x68; break;
				case 0x0589: ch = 0x3A; break;
				case 0x066A: ch = 0x25; break;
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
				case 0x2017: ch = 0x5F; break;
				case 0x2018: ch = 0x60; break;
				case 0x2019: ch = 0x27; break;
				case 0x201A: ch = 0x2C; break;
				case 0x201C: ch = 0x22; break;
				case 0x201D: ch = 0x22; break;
				case 0x201E: ch = 0x2C; break;
				case 0x2020: ch = 0x2B; break;
				case 0x2021: ch = 0xD8; break;
				case 0x2022: ch = 0x07; break;
				case 0x2024: ch = 0xFA; break;
				case 0x2026: ch = 0x2E; break;
				case 0x2030: ch = 0x25; break;
				case 0x2032: ch = 0x27; break;
				case 0x2035: ch = 0x60; break;
				case 0x2039: ch = 0x3C; break;
				case 0x203A: ch = 0x3E; break;
				case 0x203C: ch = 0x13; break;
				case 0x2044: ch = 0x2F; break;
				case 0x2070: ch = 0xF8; break;
				case 0x2074:
				case 0x2075:
				case 0x2076:
				case 0x2077:
				case 0x2078:
					ch -= 0x2040;
					break;
				case 0x207F: ch = 0xFC; break;
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
				case 0x20A4: ch = 0x9C; break;
				case 0x20A7: ch = 0x9E; break;
				case 0x20DD: ch = 0x09; break;
				case 0x2102: ch = 0x43; break;
				case 0x2107: ch = 0x45; break;
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
				case 0x2118: ch = 0x50; break;
				case 0x2119: ch = 0x50; break;
				case 0x211A: ch = 0x51; break;
				case 0x211B: ch = 0x52; break;
				case 0x211C: ch = 0x52; break;
				case 0x211D: ch = 0x52; break;
				case 0x2122: ch = 0x54; break;
				case 0x2124: ch = 0x5A; break;
				case 0x2126: ch = 0xEA; break;
				case 0x2128: ch = 0x5A; break;
				case 0x212A: ch = 0x4B; break;
				case 0x212B: ch = 0x8F; break;
				case 0x212C: ch = 0x42; break;
				case 0x212D: ch = 0x43; break;
				case 0x212E: ch = 0x65; break;
				case 0x212F: ch = 0x65; break;
				case 0x2130: ch = 0x45; break;
				case 0x2131: ch = 0x46; break;
				case 0x2133: ch = 0x4D; break;
				case 0x2134: ch = 0x6F; break;
				case 0x2190: ch = 0x1B; break;
				case 0x2191: ch = 0x18; break;
				case 0x2192: ch = 0x1A; break;
				case 0x2193: ch = 0x19; break;
				case 0x2194: ch = 0x1D; break;
				case 0x2195: ch = 0x12; break;
				case 0x21A8: ch = 0x17; break;
				case 0x2205: ch = 0xED; break;
				case 0x2211: ch = 0xE4; break;
				case 0x2212: ch = 0x2D; break;
				case 0x2213: ch = 0xF1; break;
				case 0x2215: ch = 0x2F; break;
				case 0x2216: ch = 0x5C; break;
				case 0x2217: ch = 0x2A; break;
				case 0x2218: ch = 0xF8; break;
				case 0x2219: ch = 0xF9; break;
				case 0x221A: ch = 0xFB; break;
				case 0x221E: ch = 0xEC; break;
				case 0x221F: ch = 0x1C; break;
				case 0x2223: ch = 0x7C; break;
				case 0x2229: ch = 0xEF; break;
				case 0x2236: ch = 0x3A; break;
				case 0x223C: ch = 0x7E; break;
				case 0x2248: ch = 0xF7; break;
				case 0x2261: ch = 0xF0; break;
				case 0x2264: ch = 0xF3; break;
				case 0x2265: ch = 0xF2; break;
				case 0x226A: ch = 0xAE; break;
				case 0x226B: ch = 0xAF; break;
				case 0x22C5: ch = 0xFA; break;
				case 0x2302: ch = 0x7F; break;
				case 0x2303: ch = 0x5E; break;
				case 0x2310: ch = 0xA9; break;
				case 0x2320: ch = 0xF4; break;
				case 0x2321: ch = 0xF5; break;
				case 0x2329: ch = 0x3C; break;
				case 0x232A: ch = 0x3E; break;
				case 0x2500: ch = 0xC4; break;
				case 0x2502: ch = 0xB3; break;
				case 0x250C: ch = 0xDA; break;
				case 0x2510: ch = 0xBF; break;
				case 0x2514: ch = 0xC0; break;
				case 0x2518: ch = 0xD9; break;
				case 0x251C: ch = 0xC3; break;
				case 0x2524: ch = 0xB4; break;
				case 0x252C: ch = 0xC2; break;
				case 0x2534: ch = 0xC1; break;
				case 0x253C: ch = 0xC5; break;
				case 0x2550: ch = 0xCD; break;
				case 0x2551: ch = 0xBA; break;
				case 0x2552: ch = 0xD5; break;
				case 0x2553: ch = 0xD6; break;
				case 0x2554: ch = 0xC9; break;
				case 0x2555: ch = 0xB8; break;
				case 0x2556: ch = 0xB7; break;
				case 0x2557: ch = 0xBB; break;
				case 0x2558: ch = 0xD4; break;
				case 0x2559: ch = 0xD3; break;
				case 0x255A: ch = 0xC8; break;
				case 0x255B: ch = 0xBE; break;
				case 0x255C: ch = 0xBD; break;
				case 0x255D: ch = 0xBC; break;
				case 0x255E: ch = 0xC6; break;
				case 0x255F: ch = 0xC7; break;
				case 0x2560: ch = 0xCC; break;
				case 0x2561: ch = 0xB5; break;
				case 0x2562: ch = 0xB6; break;
				case 0x2563: ch = 0xB9; break;
				case 0x2564: ch = 0xD1; break;
				case 0x2565: ch = 0xD2; break;
				case 0x2566: ch = 0xCB; break;
				case 0x2567: ch = 0xCF; break;
				case 0x2568: ch = 0xD0; break;
				case 0x2569: ch = 0xCA; break;
				case 0x256A: ch = 0xD8; break;
				case 0x256B: ch = 0xD7; break;
				case 0x256C: ch = 0xCE; break;
				case 0x2580: ch = 0xDF; break;
				case 0x2584: ch = 0xDC; break;
				case 0x2588: ch = 0xDB; break;
				case 0x258C: ch = 0xDD; break;
				case 0x2590: ch = 0xDE; break;
				case 0x2591: ch = 0xB0; break;
				case 0x2592: ch = 0xB1; break;
				case 0x2593: ch = 0xB2; break;
				case 0x25A0: ch = 0xFE; break;
				case 0x25AC: ch = 0x16; break;
				case 0x25B2: ch = 0x1E; break;
				case 0x25BA: ch = 0x10; break;
				case 0x25BC: ch = 0x1F; break;
				case 0x25C4: ch = 0x11; break;
				case 0x25CB: ch = 0x09; break;
				case 0x25D8: ch = 0x08; break;
				case 0x25D9: ch = 0x0A; break;
				case 0x263A: ch = 0x01; break;
				case 0x263B: ch = 0x02; break;
				case 0x263C: ch = 0x0F; break;
				case 0x2640: ch = 0x0C; break;
				case 0x2642: ch = 0x0B; break;
				case 0x2660: ch = 0x06; break;
				case 0x2663: ch = 0x05; break;
				case 0x2665: ch = 0x03; break;
				case 0x2666: ch = 0x04; break;
				case 0x266A: ch = 0x0D; break;
				case 0x266B: ch = 0x0E; break;
				case 0x2713: ch = 0xFB; break;
				case 0x2758: ch = 0x7C; break;
				case 0x3000: ch = 0x20; break;
				case 0x3007: ch = 0x09; break;
				case 0x3008: ch = 0x3C; break;
				case 0x3009: ch = 0x3E; break;
				case 0x300A: ch = 0xAE; break;
				case 0x300B: ch = 0xAF; break;
				case 0x301A: ch = 0x5B; break;
				case 0x301B: ch = 0x5D; break;
				case 0x30FB: ch = 0xFA; break;
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
				default: ch = 0x3F; break;
			}
			bytes[byteIndex++] = (byte)ch;
			--charCount;
		}
	}
	*/

}; // class CP437

[Serializable]
public class ENCibm437 : CP437
{
	public ENCibm437() : base() {}

}; // class ENCibm437

}; // namespace I18N.West
