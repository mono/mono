/*
 * CP20290.cs - IBM EBCDIC (Japanese Katakana Extended) code page.
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

// Generated from "ibm-290.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.Rare
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP20290 : ByteEncoding
{
	public CP20290()
		: base(20290, ToChars, "IBM EBCDIC (Japanese Katakana Extended)",
		       "IBM290", "IBM290", "IBM290",
		       false, false, false, false, 932)
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
		'\u0014', '\u0015', '\u009E', '\u001A', '\u0020', '\uFF61', 
		'\uFF62', '\uFF63', '\uFF64', '\uFF65', '\uFF66', '\uFF67', 
		'\uFF68', '\uFF69', '\u00A3', '\u002E', '\u003C', '\u0028', 
		'\u002B', '\u007C', '\u0026', '\uFF6A', '\uFF6B', '\uFF6C', 
		'\uFF6D', '\uFF6E', '\uFF6F', '\u003F', '\uFF70', '\u003F', 
		'\u0021', '\u00A5', '\u002A', '\u0029', '\u003B', '\u00AC', 
		'\u002D', '\u002F', '\u0061', '\u0062', '\u0063', '\u0064', 
		'\u0065', '\u0066', '\u0067', '\u0068', '\u003F', '\u002C', 
		'\u0025', '\u005F', '\u003E', '\u003F', '\u005B', '\u0069', 
		'\u006A', '\u006B', '\u006C', '\u006D', '\u006E', '\u006F', 
		'\u0070', '\u0060', '\u003A', '\u0023', '\u0040', '\u0027', 
		'\u003D', '\u0022', '\u005D', '\uFF71', '\uFF72', '\uFF73', 
		'\uFF74', '\uFF75', '\uFF76', '\uFF77', '\uFF78', '\uFF79', 
		'\uFF7A', '\u0071', '\uFF7B', '\uFF7C', '\uFF7D', '\uFF7E', 
		'\uFF7F', '\uFF80', '\uFF81', '\uFF82', '\uFF83', '\uFF84', 
		'\uFF85', '\uFF86', '\uFF87', '\uFF88', '\uFF89', '\u0072', 
		'\u003F', '\uFF8A', '\uFF8B', '\uFF8C', '\u007E', '\u203E', 
		'\uFF8D', '\uFF8E', '\uFF8F', '\uFF90', '\uFF91', '\uFF92', 
		'\uFF93', '\uFF94', '\uFF95', '\u0073', '\uFF96', '\uFF97', 
		'\uFF98', '\uFF99', '\u005E', '\u00A2', '\u005C', '\u0074', 
		'\u0075', '\u0076', '\u0077', '\u0078', '\u0079', '\u007A', 
		'\uFF9A', '\uFF9B', '\uFF9C', '\uFF9D', '\uFF9E', '\uFF9F', 
		'\u007B', '\u0041', '\u0042', '\u0043', '\u0044', '\u0045', 
		'\u0046', '\u0047', '\u0048', '\u0049', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u003F', '\u003F', '\u007D', '\u004A', 
		'\u004B', '\u004C', '\u004D', '\u004E', '\u004F', '\u0050', 
		'\u0051', '\u0052', '\u003F', '\u003F', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u0024', '\u003F', '\u0053', '\u0054', 
		'\u0055', '\u0056', '\u0057', '\u0058', '\u0059', '\u005A', 
		'\u003F', '\u003F', '\u003F', '\u003F', '\u003F', '\u003F', 
		'\u0030', '\u0031', '\u0032', '\u0033', '\u0034', '\u0035', 
		'\u0036', '\u0037', '\u0038', '\u0039', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u003F', '\u009F', 
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
				case 0x0024: ch = 0xE0; break;
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
				case 0x005B: ch = 0x70; break;
				case 0x005C: ch = 0xB2; break;
				case 0x005D: ch = 0x80; break;
				case 0x005E: ch = 0xB0; break;
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
					ch += 0x0001;
					break;
				case 0x0069:
				case 0x006A:
				case 0x006B:
				case 0x006C:
				case 0x006D:
				case 0x006E:
				case 0x006F:
				case 0x0070:
					ch += 0x0008;
					break;
				case 0x0071: ch = 0x8B; break;
				case 0x0072: ch = 0x9B; break;
				case 0x0073: ch = 0xAB; break;
				case 0x0074:
				case 0x0075:
				case 0x0076:
				case 0x0077:
				case 0x0078:
				case 0x0079:
				case 0x007A:
					ch += 0x003F;
					break;
				case 0x007B: ch = 0xC0; break;
				case 0x007C: ch = 0x4F; break;
				case 0x007D: ch = 0xD0; break;
				case 0x007E: ch = 0xA0; break;
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
				case 0x00A2: ch = 0xB1; break;
				case 0x00A3: ch = 0x4A; break;
				case 0x00A5: ch = 0x5B; break;
				case 0x00AC: ch = 0x5F; break;
				case 0x203E: ch = 0xA1; break;
				case 0xFF01: ch = 0x5A; break;
				case 0xFF02: ch = 0x7F; break;
				case 0xFF03: ch = 0x7B; break;
				case 0xFF04: ch = 0xE0; break;
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
				case 0xFF3B: ch = 0x70; break;
				case 0xFF3C: ch = 0xB2; break;
				case 0xFF3D: ch = 0x80; break;
				case 0xFF3E: ch = 0xB0; break;
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
					ch -= 0xFEDF;
					break;
				case 0xFF49:
				case 0xFF4A:
				case 0xFF4B:
				case 0xFF4C:
				case 0xFF4D:
				case 0xFF4E:
				case 0xFF4F:
				case 0xFF50:
					ch -= 0xFED8;
					break;
				case 0xFF51: ch = 0x8B; break;
				case 0xFF52: ch = 0x9B; break;
				case 0xFF53: ch = 0xAB; break;
				case 0xFF54:
				case 0xFF55:
				case 0xFF56:
				case 0xFF57:
				case 0xFF58:
				case 0xFF59:
				case 0xFF5A:
					ch -= 0xFEA1;
					break;
				case 0xFF5B: ch = 0xC0; break;
				case 0xFF5C: ch = 0x4F; break;
				case 0xFF5D: ch = 0xD0; break;
				case 0xFF5E: ch = 0xA0; break;
				case 0xFF61:
				case 0xFF62:
				case 0xFF63:
				case 0xFF64:
				case 0xFF65:
				case 0xFF66:
				case 0xFF67:
				case 0xFF68:
				case 0xFF69:
					ch -= 0xFF20;
					break;
				case 0xFF6A:
				case 0xFF6B:
				case 0xFF6C:
				case 0xFF6D:
				case 0xFF6E:
				case 0xFF6F:
					ch -= 0xFF19;
					break;
				case 0xFF70: ch = 0x58; break;
				case 0xFF71:
				case 0xFF72:
				case 0xFF73:
				case 0xFF74:
				case 0xFF75:
				case 0xFF76:
				case 0xFF77:
				case 0xFF78:
				case 0xFF79:
				case 0xFF7A:
					ch -= 0xFEF0;
					break;
				case 0xFF7B:
				case 0xFF7C:
				case 0xFF7D:
				case 0xFF7E:
				case 0xFF7F:
				case 0xFF80:
				case 0xFF81:
				case 0xFF82:
				case 0xFF83:
				case 0xFF84:
				case 0xFF85:
				case 0xFF86:
				case 0xFF87:
				case 0xFF88:
				case 0xFF89:
					ch -= 0xFEEF;
					break;
				case 0xFF8A: ch = 0x9D; break;
				case 0xFF8B: ch = 0x9E; break;
				case 0xFF8C: ch = 0x9F; break;
				case 0xFF8D:
				case 0xFF8E:
				case 0xFF8F:
				case 0xFF90:
				case 0xFF91:
				case 0xFF92:
				case 0xFF93:
				case 0xFF94:
				case 0xFF95:
					ch -= 0xFEEB;
					break;
				case 0xFF96:
				case 0xFF97:
				case 0xFF98:
				case 0xFF99:
					ch -= 0xFEEA;
					break;
				case 0xFF9A:
				case 0xFF9B:
				case 0xFF9C:
				case 0xFF9D:
				case 0xFF9E:
				case 0xFF9F:
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
}; // class CP20290

[Serializable]
public class ENCibm290 : CP20290
{
	public ENCibm290() : base() {}

}; // class ENCibm290

}; // namespace I18N.Rare
