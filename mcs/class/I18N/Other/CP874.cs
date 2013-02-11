/*
 * CP874.cs - Thai (Windows) code page.
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

// Generated from "ibm-874.ucm".

// WARNING: Modifying this file directly might be a bad idea.
// You should edit the code generator tools/ucm2cp.c instead for your changes
// to appear in all relevant classes.
namespace I18N.Other
{

using System;
using System.Text;
using I18N.Common;

[Serializable]
public class CP874 : ByteEncoding
{
	public CP874()
		: base(874, ToChars, "Thai (Windows)",
		       "windows-874", "windows-874", "windows-874",
		       true, true, true, true, 874)
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
		'\u007E', '\u001A', '\u003F', '\u003F', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u003F', '\u003F', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u003F', '\u003F', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u003F', '\u003F', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u003F', '\u003F', '\u003F', '\u003F', 
		'\u003F', '\u003F', '\u003F', '\u003F', '\u0E48', '\u0E01', 
		'\u0E02', '\u0E03', '\u0E04', '\u0E05', '\u0E06', '\u0E07', 
		'\u0E08', '\u0E09', '\u0E0A', '\u0E0B', '\u0E0C', '\u0E0D', 
		'\u0E0E', '\u0E0F', '\u0E10', '\u0E11', '\u0E12', '\u0E13', 
		'\u0E14', '\u0E15', '\u0E16', '\u0E17', '\u0E18', '\u0E19', 
		'\u0E1A', '\u0E1B', '\u0E1C', '\u0E1D', '\u0E1E', '\u0E1F', 
		'\u0E20', '\u0E21', '\u0E22', '\u0E23', '\u0E24', '\u0E25', 
		'\u0E26', '\u0E27', '\u0E28', '\u0E29', '\u0E2A', '\u0E2B', 
		'\u0E2C', '\u0E2D', '\u0E2E', '\u0E2F', '\u0E30', '\u0E31', 
		'\u0E32', '\u0E33', '\u0E34', '\u0E35', '\u0E36', '\u0E37', 
		'\u0E38', '\u0E39', '\u0E3A', '\u0E49', '\u0E4A', '\u0E4B', 
		'\u0E4C', '\u0E3F', '\u0E40', '\u0E41', '\u0E42', '\u0E43', 
		'\u0E44', '\u0E45', '\u0E46', '\u0E47', '\u0E48', '\u0E49', 
		'\u0E4A', '\u0E4B', '\u0E4C', '\u0E4D', '\u0E4E', '\u0E4F', 
		'\u0E50', '\u0E51', '\u0E52', '\u0E53', '\u0E54', '\u0E55', 
		'\u0E56', '\u0E57', '\u0E58', '\u0E59', '\u0E5A', '\u0E5B', 
		'\u00A2', '\u00AC', '\u00A6', '\u00A0', 
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
					break;
				case 0x001A: ch = 0x7F; break;
				case 0x001C: ch = 0x1A; break;
				case 0x007F: ch = 0x1C; break;
				case 0x00A0: ch = 0xFF; break;
				case 0x00A2: ch = 0xFC; break;
				case 0x00A6: ch = 0xFE; break;
				case 0x00AC: ch = 0xFD; break;
				case 0x0E01:
				case 0x0E02:
				case 0x0E03:
				case 0x0E04:
				case 0x0E05:
				case 0x0E06:
				case 0x0E07:
				case 0x0E08:
				case 0x0E09:
				case 0x0E0A:
				case 0x0E0B:
				case 0x0E0C:
				case 0x0E0D:
				case 0x0E0E:
				case 0x0E0F:
				case 0x0E10:
				case 0x0E11:
				case 0x0E12:
				case 0x0E13:
				case 0x0E14:
				case 0x0E15:
				case 0x0E16:
				case 0x0E17:
				case 0x0E18:
				case 0x0E19:
				case 0x0E1A:
				case 0x0E1B:
				case 0x0E1C:
				case 0x0E1D:
				case 0x0E1E:
				case 0x0E1F:
				case 0x0E20:
				case 0x0E21:
				case 0x0E22:
				case 0x0E23:
				case 0x0E24:
				case 0x0E25:
				case 0x0E26:
				case 0x0E27:
				case 0x0E28:
				case 0x0E29:
				case 0x0E2A:
				case 0x0E2B:
				case 0x0E2C:
				case 0x0E2D:
				case 0x0E2E:
				case 0x0E2F:
				case 0x0E30:
				case 0x0E31:
				case 0x0E32:
				case 0x0E33:
				case 0x0E34:
				case 0x0E35:
				case 0x0E36:
				case 0x0E37:
				case 0x0E38:
				case 0x0E39:
				case 0x0E3A:
					ch -= 0x0D60;
					break;
				case 0x0E3F:
				case 0x0E40:
				case 0x0E41:
				case 0x0E42:
				case 0x0E43:
				case 0x0E44:
				case 0x0E45:
				case 0x0E46:
				case 0x0E47:
				case 0x0E48:
				case 0x0E49:
				case 0x0E4A:
				case 0x0E4B:
				case 0x0E4C:
				case 0x0E4D:
				case 0x0E4E:
				case 0x0E4F:
				case 0x0E50:
				case 0x0E51:
				case 0x0E52:
				case 0x0E53:
				case 0x0E54:
				case 0x0E55:
				case 0x0E56:
				case 0x0E57:
				case 0x0E58:
				case 0x0E59:
				case 0x0E5A:
				case 0x0E5B:
					ch -= 0x0D60;
					break;
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
					break;
				case 0x001A: ch = 0x7F; break;
				case 0x001C: ch = 0x1A; break;
				case 0x007F: ch = 0x1C; break;
				case 0x00A0: ch = 0xFF; break;
				case 0x00A2: ch = 0xFC; break;
				case 0x00A6: ch = 0xFE; break;
				case 0x00AC: ch = 0xFD; break;
				case 0x0E01:
				case 0x0E02:
				case 0x0E03:
				case 0x0E04:
				case 0x0E05:
				case 0x0E06:
				case 0x0E07:
				case 0x0E08:
				case 0x0E09:
				case 0x0E0A:
				case 0x0E0B:
				case 0x0E0C:
				case 0x0E0D:
				case 0x0E0E:
				case 0x0E0F:
				case 0x0E10:
				case 0x0E11:
				case 0x0E12:
				case 0x0E13:
				case 0x0E14:
				case 0x0E15:
				case 0x0E16:
				case 0x0E17:
				case 0x0E18:
				case 0x0E19:
				case 0x0E1A:
				case 0x0E1B:
				case 0x0E1C:
				case 0x0E1D:
				case 0x0E1E:
				case 0x0E1F:
				case 0x0E20:
				case 0x0E21:
				case 0x0E22:
				case 0x0E23:
				case 0x0E24:
				case 0x0E25:
				case 0x0E26:
				case 0x0E27:
				case 0x0E28:
				case 0x0E29:
				case 0x0E2A:
				case 0x0E2B:
				case 0x0E2C:
				case 0x0E2D:
				case 0x0E2E:
				case 0x0E2F:
				case 0x0E30:
				case 0x0E31:
				case 0x0E32:
				case 0x0E33:
				case 0x0E34:
				case 0x0E35:
				case 0x0E36:
				case 0x0E37:
				case 0x0E38:
				case 0x0E39:
				case 0x0E3A:
					ch -= 0x0D60;
					break;
				case 0x0E3F:
				case 0x0E40:
				case 0x0E41:
				case 0x0E42:
				case 0x0E43:
				case 0x0E44:
				case 0x0E45:
				case 0x0E46:
				case 0x0E47:
				case 0x0E48:
				case 0x0E49:
				case 0x0E4A:
				case 0x0E4B:
				case 0x0E4C:
				case 0x0E4D:
				case 0x0E4E:
				case 0x0E4F:
				case 0x0E50:
				case 0x0E51:
				case 0x0E52:
				case 0x0E53:
				case 0x0E54:
				case 0x0E55:
				case 0x0E56:
				case 0x0E57:
				case 0x0E58:
				case 0x0E59:
				case 0x0E5A:
				case 0x0E5B:
					ch -= 0x0D60;
					break;
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

}; // class CP874

[Serializable]
public class ENCwindows_874 : CP874
{
	public ENCwindows_874() : base() {}

}; // class ENCwindows_874

}; // namespace I18N.Other
