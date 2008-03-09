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
	internal class CapabilitiesChecksum
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

		private static string Hex(byte b)
		{
			char[] list = new char[2];
			BitArray myBA1 = new BitArray(new bool[4] { false, false, false, false });
			BitArray myBA2 = new BitArray(new bool[4] { false, false, false, false });
			BitArray myBA3 = new BitArray(new byte[] { b });

			//1 byte = 8 bits
			//4 bits = 1 Hex Value.
			myBA1.Set(0, myBA3.Get(0));
			myBA1.Set(1, myBA3.Get(1));
			myBA1.Set(2, myBA3.Get(2));
			myBA1.Set(3, myBA3.Get(3));

			myBA2.Set(0, myBA3.Get(4));
			myBA2.Set(1, myBA3.Get(5));
			myBA2.Set(2, myBA3.Get(6));
			myBA2.Set(3, myBA3.Get(7));

			list[0] = CapabilitiesChecksum.MapToHex(myBA1);
			list[1] = CapabilitiesChecksum.MapToHex(myBA2);
			return new string(list, 0, 2);
		}
		private static char MapToHex(BitArray a)
		{
			//-----------------------------------------------------------------------------------
			//I could of done bit wise comparisons much more efficiant, but that would of given
			//me a bigger headach then this. Which I can reason though even half asleep.
			//-----------------------------------------------------------------------------------
			if (a.Get(0) == false && a.Get(1) == false && a.Get(2) == false && a.Get(3) == false)
			{
				return '0';
			}
			else if (a.Get(0) == true && a.Get(1) == false && a.Get(2) == false && a.Get(3) == false)
			{
				return '1';
			}
			else if (a.Get(0) == false && a.Get(1) == true && a.Get(2) == false && a.Get(3) == false)
			{
				return '2';
			}
			else if (a.Get(0) == true && a.Get(1) == true && a.Get(2) == false && a.Get(3) == false)
			{
				return '3';
			}
			else if (a.Get(0) == false && a.Get(1) == false && a.Get(2) == true && a.Get(3) == false)
			{
				return '4';
			}
			else if (a.Get(0) == true && a.Get(1) == false && a.Get(2) == true && a.Get(3) == false)
			{
				return '5';
			}
			else if (a.Get(0) == false && a.Get(1) == true && a.Get(2) == true && a.Get(3) == false)
			{
				return '6';
			}
			else if (a.Get(0) == true && a.Get(1) == true && a.Get(2) == true && a.Get(3) == false)
			{
				return '7';
			}
			else if (a.Get(0) == false && a.Get(1) == false && a.Get(2) == false && a.Get(3) == true)
			{
				return '8';
			}
			else if (a.Get(0) == true && a.Get(1) == false && a.Get(2) == false && a.Get(3) == true)
			{
				return '9';
			}
			else if (a.Get(0) == false && a.Get(1) == true && a.Get(2) == false && a.Get(3) == true)
			{
				return 'A';
			}
			else if (a.Get(0) == true && a.Get(1) == true && a.Get(2) == false && a.Get(3) == true)
			{
				return 'B';
			}
			else if (a.Get(0) == false && a.Get(1) == false && a.Get(2) == true && a.Get(3) == true)
			{
				return 'C';
			}
			else if (a.Get(0) == true && a.Get(1) == false && a.Get(2) == true && a.Get(3) == true)
			{
				return 'D';
			}
			else if (a.Get(0) == false && a.Get(1) == true && a.Get(2) == true && a.Get(3) == true)
			{
				return 'E';
			}
			else if (a.Get(0) == true && a.Get(1) == true && a.Get(2) == true && a.Get(3) == true)
			{
				return 'F';
			}
			//this should never ever happen, unless a bit
			//gets switched mid way though the checks.
			throw new System.Exception("shit fell threw");
		}
	}
}
#endif
