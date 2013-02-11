/*
 * CP1141.cs - IBM EBCDIC (Germany with Euro) code page.
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

// Generated from "ibm-1141.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.Rare
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP1141 : ByteEncoding
{
	public CP1141()
		: base(1141, ToChars, "IBM EBCDIC (Germany with Euro)",
		       "IBM01141", "IBM01141", "IBM01141",
		       false, false, false, false, 1252)
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
		'\u00E2', '\u007B', '\u00E0', '\u00E1', '\u00E3', '\u00E5', 
		'\u00E7', '\u00F1', '\u00C4', '\u002E', '\u003C', '\u0028', 
		'\u002B', '\u0021', '\u0026', '\u00E9', '\u00EA', '\u00EB', 
		'\u00E8', '\u00ED', '\u00EE', '\u00EF', '\u00EC', '\u007E', 
		'\u00DC', '\u0024', '\u002A', '\u0029', '\u003B', '\u005E', 
		'\u002D', '\u002F', '\u00C2', '\u005B', '\u00C0', '\u00C1', 
		'\u00C3', '\u00C5', '\u00C7', '\u00D1', '\u00F6', '\u002C', 
		'\u0025', '\u005F', '\u003E', '\u003F', '\u00F8', '\u00C9', 
		'\u00CA', '\u00CB', '\u00C8', '\u00CD', '\u00CE', '\u00CF', 
		'\u00CC', '\u0060', '\u003A', '\u0023', '\u00A7', '\u0027', 
		'\u003D', '\u0022', '\u00D8', '\u0061', '\u0062', '\u0063', 
		'\u0064', '\u0065', '\u0066', '\u0067', '\u0068', '\u0069', 
		'\u00AB', '\u00BB', '\u00F0', '\u00FD', '\u00FE', '\u00B1', 
		'\u00B0', '\u006A', '\u006B', '\u006C', '\u006D', '\u006E', 
		'\u006F', '\u0070', '\u0071', '\u0072', '\u00AA', '\u00BA', 
		'\u00E6', '\u00B8', '\u00C6', '\u20AC', '\u00B5', '\u00DF', 
		'\u0073', '\u0074', '\u0075', '\u0076', '\u0077', '\u0078', 
		'\u0079', '\u007A', '\u00A1', '\u00BF', '\u00D0', '\u00DD', 
		'\u00DE', '\u00AE', '\u00A2', '\u00A3', '\u00A5', '\u00B7', 
		'\u00A9', '\u0040', '\u00B6', '\u00BC', '\u00BD', '\u00BE', 
		'\u00AC', '\u007C', '\u00AF', '\u00A8', '\u00B4', '\u00D7', 
		'\u00E4', '\u0041', '\u0042', '\u0043', '\u0044', '\u0045', 
		'\u0046', '\u0047', '\u0048', '\u0049', '\u00AD', '\u00F4', 
		'\u00A6', '\u00F2', '\u00F3', '\u00F5', '\u00FC', '\u004A', 
		'\u004B', '\u004C', '\u004D', '\u004E', '\u004F', '\u0050', 
		'\u0051', '\u0052', '\u00B9', '\u00FB', '\u007D', '\u00F9', 
		'\u00FA', '\u00FF', '\u00D6', '\u00F7', '\u0053', '\u0054', 
		'\u0055', '\u0056', '\u0057', '\u0058', '\u0059', '\u005A', 
		'\u00B2', '\u00D4', '\u005C', '\u00D2', '\u00D3', '\u00D5', 
		'\u0030', '\u0031', '\u0032', '\u0033', '\u0034', '\u0035', 
		'\u0036', '\u0037', '\u0038', '\u0039', '\u00B3', '\u00DB', 
		'\u005D', '\u00D9', '\u00DA', '\u009F', 
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
				case 0x00B6:
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
				case 0x0021: ch = 0x4F; break;
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
				case 0x0040: ch = 0xB5; break;
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
				case 0x005B: ch = 0x63; break;
				case 0x005C: ch = 0xEC; break;
				case 0x005D: ch = 0xFC; break;
				case 0x005E: ch = 0x5F; break;
				case 0x005F: ch = 0x6D; break;
				case 0x0060: ch = 0x79; break;
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
				case 0x007B: ch = 0x43; break;
				case 0x007C: ch = 0xBB; break;
				case 0x007D: ch = 0xDC; break;
				case 0x007E: ch = 0x59; break;
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
				case 0x00A1: ch = 0xAA; break;
				case 0x00A2: ch = 0xB0; break;
				case 0x00A3: ch = 0xB1; break;
				case 0x00A5: ch = 0xB2; break;
				case 0x00A6: ch = 0xCC; break;
				case 0x00A7: ch = 0x7C; break;
				case 0x00A8: ch = 0xBD; break;
				case 0x00A9: ch = 0xB4; break;
				case 0x00AA: ch = 0x9A; break;
				case 0x00AB: ch = 0x8A; break;
				case 0x00AC: ch = 0xBA; break;
				case 0x00AD: ch = 0xCA; break;
				case 0x00AE: ch = 0xAF; break;
				case 0x00AF: ch = 0xBC; break;
				case 0x00B0: ch = 0x90; break;
				case 0x00B1: ch = 0x8F; break;
				case 0x00B2: ch = 0xEA; break;
				case 0x00B3: ch = 0xFA; break;
				case 0x00B4: ch = 0xBE; break;
				case 0x00B5: ch = 0xA0; break;
				case 0x00B7: ch = 0xB3; break;
				case 0x00B8: ch = 0x9D; break;
				case 0x00B9: ch = 0xDA; break;
				case 0x00BA: ch = 0x9B; break;
				case 0x00BB: ch = 0x8B; break;
				case 0x00BC: ch = 0xB7; break;
				case 0x00BD: ch = 0xB8; break;
				case 0x00BE: ch = 0xB9; break;
				case 0x00BF: ch = 0xAB; break;
				case 0x00C0: ch = 0x64; break;
				case 0x00C1: ch = 0x65; break;
				case 0x00C2: ch = 0x62; break;
				case 0x00C3: ch = 0x66; break;
				case 0x00C4: ch = 0x4A; break;
				case 0x00C5: ch = 0x67; break;
				case 0x00C6: ch = 0x9E; break;
				case 0x00C7: ch = 0x68; break;
				case 0x00C8: ch = 0x74; break;
				case 0x00C9: ch = 0x71; break;
				case 0x00CA: ch = 0x72; break;
				case 0x00CB: ch = 0x73; break;
				case 0x00CC: ch = 0x78; break;
				case 0x00CD: ch = 0x75; break;
				case 0x00CE: ch = 0x76; break;
				case 0x00CF: ch = 0x77; break;
				case 0x00D0: ch = 0xAC; break;
				case 0x00D1: ch = 0x69; break;
				case 0x00D2: ch = 0xED; break;
				case 0x00D3: ch = 0xEE; break;
				case 0x00D4: ch = 0xEB; break;
				case 0x00D5: ch = 0xEF; break;
				case 0x00D6: ch = 0xE0; break;
				case 0x00D7: ch = 0xBF; break;
				case 0x00D8: ch = 0x80; break;
				case 0x00D9: ch = 0xFD; break;
				case 0x00DA: ch = 0xFE; break;
				case 0x00DB: ch = 0xFB; break;
				case 0x00DC: ch = 0x5A; break;
				case 0x00DD: ch = 0xAD; break;
				case 0x00DE: ch = 0xAE; break;
				case 0x00DF: ch = 0xA1; break;
				case 0x00E0: ch = 0x44; break;
				case 0x00E1: ch = 0x45; break;
				case 0x00E2: ch = 0x42; break;
				case 0x00E3: ch = 0x46; break;
				case 0x00E4: ch = 0xC0; break;
				case 0x00E5: ch = 0x47; break;
				case 0x00E6: ch = 0x9C; break;
				case 0x00E7: ch = 0x48; break;
				case 0x00E8: ch = 0x54; break;
				case 0x00E9: ch = 0x51; break;
				case 0x00EA: ch = 0x52; break;
				case 0x00EB: ch = 0x53; break;
				case 0x00EC: ch = 0x58; break;
				case 0x00ED: ch = 0x55; break;
				case 0x00EE: ch = 0x56; break;
				case 0x00EF: ch = 0x57; break;
				case 0x00F0: ch = 0x8C; break;
				case 0x00F1: ch = 0x49; break;
				case 0x00F2: ch = 0xCD; break;
				case 0x00F3: ch = 0xCE; break;
				case 0x00F4: ch = 0xCB; break;
				case 0x00F5: ch = 0xCF; break;
				case 0x00F6: ch = 0x6A; break;
				case 0x00F7: ch = 0xE1; break;
				case 0x00F8: ch = 0x70; break;
				case 0x00F9: ch = 0xDD; break;
				case 0x00FA: ch = 0xDE; break;
				case 0x00FB: ch = 0xDB; break;
				case 0x00FC: ch = 0xD0; break;
				case 0x00FD: ch = 0x8D; break;
				case 0x00FE: ch = 0x8E; break;
				case 0x00FF: ch = 0xDF; break;
				case 0x203E: ch = 0xBC; break;
				case 0x20AC: ch = 0x9F; break;
				case 0xFF01: ch = 0x4F; break;
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
				case 0xFF20: ch = 0xB5; break;
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
				case 0xFF3B: ch = 0x63; break;
				case 0xFF3C: ch = 0xEC; break;
				case 0xFF3D: ch = 0xFC; break;
				case 0xFF3E: ch = 0x5F; break;
				case 0xFF3F: ch = 0x6D; break;
				case 0xFF40: ch = 0x79; break;
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
				case 0xFF5B: ch = 0x43; break;
				case 0xFF5C: ch = 0xBB; break;
				case 0xFF5D: ch = 0xDC; break;
				case 0xFF5E: ch = 0x59; break;
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
				case 0x00B6:
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
				case 0x0021: ch = 0x4F; break;
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
				case 0x0040: ch = 0xB5; break;
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
				case 0x005B: ch = 0x63; break;
				case 0x005C: ch = 0xEC; break;
				case 0x005D: ch = 0xFC; break;
				case 0x005E: ch = 0x5F; break;
				case 0x005F: ch = 0x6D; break;
				case 0x0060: ch = 0x79; break;
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
				case 0x007B: ch = 0x43; break;
				case 0x007C: ch = 0xBB; break;
				case 0x007D: ch = 0xDC; break;
				case 0x007E: ch = 0x59; break;
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
				case 0x00A1: ch = 0xAA; break;
				case 0x00A2: ch = 0xB0; break;
				case 0x00A3: ch = 0xB1; break;
				case 0x00A5: ch = 0xB2; break;
				case 0x00A6: ch = 0xCC; break;
				case 0x00A7: ch = 0x7C; break;
				case 0x00A8: ch = 0xBD; break;
				case 0x00A9: ch = 0xB4; break;
				case 0x00AA: ch = 0x9A; break;
				case 0x00AB: ch = 0x8A; break;
				case 0x00AC: ch = 0xBA; break;
				case 0x00AD: ch = 0xCA; break;
				case 0x00AE: ch = 0xAF; break;
				case 0x00AF: ch = 0xBC; break;
				case 0x00B0: ch = 0x90; break;
				case 0x00B1: ch = 0x8F; break;
				case 0x00B2: ch = 0xEA; break;
				case 0x00B3: ch = 0xFA; break;
				case 0x00B4: ch = 0xBE; break;
				case 0x00B5: ch = 0xA0; break;
				case 0x00B7: ch = 0xB3; break;
				case 0x00B8: ch = 0x9D; break;
				case 0x00B9: ch = 0xDA; break;
				case 0x00BA: ch = 0x9B; break;
				case 0x00BB: ch = 0x8B; break;
				case 0x00BC: ch = 0xB7; break;
				case 0x00BD: ch = 0xB8; break;
				case 0x00BE: ch = 0xB9; break;
				case 0x00BF: ch = 0xAB; break;
				case 0x00C0: ch = 0x64; break;
				case 0x00C1: ch = 0x65; break;
				case 0x00C2: ch = 0x62; break;
				case 0x00C3: ch = 0x66; break;
				case 0x00C4: ch = 0x4A; break;
				case 0x00C5: ch = 0x67; break;
				case 0x00C6: ch = 0x9E; break;
				case 0x00C7: ch = 0x68; break;
				case 0x00C8: ch = 0x74; break;
				case 0x00C9: ch = 0x71; break;
				case 0x00CA: ch = 0x72; break;
				case 0x00CB: ch = 0x73; break;
				case 0x00CC: ch = 0x78; break;
				case 0x00CD: ch = 0x75; break;
				case 0x00CE: ch = 0x76; break;
				case 0x00CF: ch = 0x77; break;
				case 0x00D0: ch = 0xAC; break;
				case 0x00D1: ch = 0x69; break;
				case 0x00D2: ch = 0xED; break;
				case 0x00D3: ch = 0xEE; break;
				case 0x00D4: ch = 0xEB; break;
				case 0x00D5: ch = 0xEF; break;
				case 0x00D6: ch = 0xE0; break;
				case 0x00D7: ch = 0xBF; break;
				case 0x00D8: ch = 0x80; break;
				case 0x00D9: ch = 0xFD; break;
				case 0x00DA: ch = 0xFE; break;
				case 0x00DB: ch = 0xFB; break;
				case 0x00DC: ch = 0x5A; break;
				case 0x00DD: ch = 0xAD; break;
				case 0x00DE: ch = 0xAE; break;
				case 0x00DF: ch = 0xA1; break;
				case 0x00E0: ch = 0x44; break;
				case 0x00E1: ch = 0x45; break;
				case 0x00E2: ch = 0x42; break;
				case 0x00E3: ch = 0x46; break;
				case 0x00E4: ch = 0xC0; break;
				case 0x00E5: ch = 0x47; break;
				case 0x00E6: ch = 0x9C; break;
				case 0x00E7: ch = 0x48; break;
				case 0x00E8: ch = 0x54; break;
				case 0x00E9: ch = 0x51; break;
				case 0x00EA: ch = 0x52; break;
				case 0x00EB: ch = 0x53; break;
				case 0x00EC: ch = 0x58; break;
				case 0x00ED: ch = 0x55; break;
				case 0x00EE: ch = 0x56; break;
				case 0x00EF: ch = 0x57; break;
				case 0x00F0: ch = 0x8C; break;
				case 0x00F1: ch = 0x49; break;
				case 0x00F2: ch = 0xCD; break;
				case 0x00F3: ch = 0xCE; break;
				case 0x00F4: ch = 0xCB; break;
				case 0x00F5: ch = 0xCF; break;
				case 0x00F6: ch = 0x6A; break;
				case 0x00F7: ch = 0xE1; break;
				case 0x00F8: ch = 0x70; break;
				case 0x00F9: ch = 0xDD; break;
				case 0x00FA: ch = 0xDE; break;
				case 0x00FB: ch = 0xDB; break;
				case 0x00FC: ch = 0xD0; break;
				case 0x00FD: ch = 0x8D; break;
				case 0x00FE: ch = 0x8E; break;
				case 0x00FF: ch = 0xDF; break;
				case 0x203E: ch = 0xBC; break;
				case 0x20AC: ch = 0x9F; break;
				case 0xFF01: ch = 0x4F; break;
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
				case 0xFF20: ch = 0xB5; break;
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
				case 0xFF3B: ch = 0x63; break;
				case 0xFF3C: ch = 0xEC; break;
				case 0xFF3D: ch = 0xFC; break;
				case 0xFF3E: ch = 0x5F; break;
				case 0xFF3F: ch = 0x6D; break;
				case 0xFF40: ch = 0x79; break;
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
				case 0xFF5B: ch = 0x43; break;
				case 0xFF5C: ch = 0xBB; break;
				case 0xFF5D: ch = 0xDC; break;
				case 0xFF5E: ch = 0x59; break;
				default: ch = 0x3F; break;
			}
			bytes[byteIndex++] = (byte)ch;
			--charCount;
		}
	}
	*/

}; // class CP1141

[Serializable]
public class ENCibm01141 : CP1141
{
	public ENCibm01141() : base() {}

}; // class ENCibm01141

}; // namespace I18N.Rare
