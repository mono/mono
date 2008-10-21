#if NET_2_0
/*
Used to determine Browser Capabilities by the Browsers UserAgent String and related
Browser supplied Headers.
Copyright (C) 2002-Present  Owen Brady (Ocean at xvision.com)

Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Collections;
using System.Text;

namespace System.Web.Configuration
{
	internal sealed class CapabilitiesChecksum
	{
		public CapabilitiesChecksum()
		{
		}
		
		public static string BuildChecksum(string raw)
		{
			byte[] array = System.Text.Encoding.Default.GetBytes(raw.ToCharArray());
			array = System.Security.Cryptography.MD5.Create().ComputeHash(array);
			string[] c = new string[array.Length];
			for (int i = 0;i <= array.Length - 1;i++)
			{
				c[i] = CapabilitiesChecksum.Hex(array[i]);
			}
			array = null;
			return string.Join("", c);
		}
		
		public static string BuildChecksum(byte[] array)
		{
			array = System.Security.Cryptography.MD5.Create().ComputeHash(array);
			string[] c = new string[array.Length];
			for (int i = 0;i <= array.Length - 1;i++)
			{
				c[i] = CapabilitiesChecksum.Hex(array[i]);
			}
			array = null;
			return string.Join("", c);
		}

		//
		// The original version of the code returned the hex half-octets reversed (e.g. if
		// b == 0xF2, 0x2F would be returned) for some reason. This version keeps this
		// convention.
		//
		static string Hex (byte b)
		{
			char[] list = new char[] {MapToHex ((byte)(b & 0x0F)), MapToHex ((byte)((b & 0xF0) >> 4))};
			return new String (list, 0, 2);
		}
  
		static char MapToHex (byte b)
		{
			if (b >= 0 && b <= 9)
				return (char)(b + 0x30);
			
			if (b >= 10 && b <= 16)
				return (char)(b + 0x37);
			
			throw new System.ArgumentException ("Unexpected error.");
		}
	}
}
#endif
