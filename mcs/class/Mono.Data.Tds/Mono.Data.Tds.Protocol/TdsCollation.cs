//
// Mono.Data.Tds.Protocol.TdsCollation.cs
//
// Authors:
//   Veerapuram Varadhan  <vvaradhan@novell.com>
//	 Dmitry S. Kataev  <dmitryskey@hotmail.com>
//
// Charset parts - Copyright (C) 2006,2007 Dmitry S. Kataev
// Copyright (C) Novell Inc, 2009
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Text;

namespace Mono.Data.Tds {
	internal static class TdsCollation 
	{
		public static int LCID (byte[] collation)
		{
			if (collation == null)
				return -1;
			
			return (collation[0] | (collation[1] << 8) | ((collation[2] & 0x0F) << 16));
		}
		
		public static int CollationFlags (byte[] collation)
		{
			if (collation == null)
				return -1;
			
			return ((collation[2] & 0xF0) | ((collation[3] & 0x0F) << 4));
		}

		public static int Version (byte[] collation)
		{
			if (collation == null)
				return -1;
			
			return (collation[3] & 0xF0);
		}
		
		public static int SortId (byte[] collation)
		{
			if (collation == null)
				return -1;
			
			return (collation[4]);
		}
	}
	
	internal static class TdsCharset
	{
		private static Hashtable lcidCodes = new Hashtable();
		private static Hashtable sortCodes = new Hashtable();
		
		static TdsCharset ()
		{
			lcidCodes[0x436] = 1252;
			lcidCodes[0x41C] = 1250;
			lcidCodes[0x401] = 1256;
			lcidCodes[0x801] = 1256;
			lcidCodes[0xC01] = 1256;
			lcidCodes[0x1001] = 1256;
			lcidCodes[0x1401] = 1256;
			lcidCodes[0x1801] = 1256;
			lcidCodes[0x1C01] = 1256;
			lcidCodes[0x2001] = 1256;
			lcidCodes[0x2401] = 1256;
			lcidCodes[0x2801] = 1256;
			lcidCodes[0x2C01] = 1256;
			lcidCodes[0x3001] = 1256;
			lcidCodes[0x3401] = 1256;
			lcidCodes[0x3801] = 1256;
			lcidCodes[0x3C01] = 1256;
			lcidCodes[0x4001] = 1256;
			lcidCodes[0x42D] = 1252;
			lcidCodes[0x423] = 1251;
			lcidCodes[0x402] = 1251;
			lcidCodes[0x403] = 1252;
			lcidCodes[0x30404] = 950;
			lcidCodes[0x404] = 950;
			lcidCodes[0x804] = 936;
			lcidCodes[0x20804] = 936;
			lcidCodes[0x1004] = 936;
			lcidCodes[0x41a] = 1250;
			lcidCodes[0x405] = 1250;
			lcidCodes[0x406] = 1252;
			lcidCodes[0x413] = 1252;
			lcidCodes[0x813] = 1252;
			lcidCodes[0x409] = 1252;
			lcidCodes[0x809] = 1252;
			lcidCodes[0x1009] = 1252;
			lcidCodes[0x1409] = 1252;
			lcidCodes[0xC09] = 1252;
			lcidCodes[0x1809] = 1252;
			lcidCodes[0x1C09] = 1252;
			lcidCodes[0x2409] = 1252;
			lcidCodes[0x2009] = 1252;
			lcidCodes[0x425] = 1257;
			lcidCodes[0x0438] = 1252;
			lcidCodes[0x429] = 1256;
			lcidCodes[0x40B] = 1252;
			lcidCodes[0x40C] = 1252;
			lcidCodes[0x80C] = 1252;
			lcidCodes[0x100C] = 1252;
			lcidCodes[0xC0C] = 1252;
			lcidCodes[0x140C] = 1252;
			lcidCodes[0x10437] = 1252;
			lcidCodes[0x10407] = 1252;
			lcidCodes[0x407] = 1252;
			lcidCodes[0x807] = 1252;
			lcidCodes[0xC07] = 1252;
			lcidCodes[0x1007] = 1252;
			lcidCodes[0x1407] = 1252;
			lcidCodes[0x408] = 1253;
			lcidCodes[0x40D] = 1255;
			lcidCodes[0x439] = 65001;
			lcidCodes[0x40E] = 1250;
			lcidCodes[0x104E] = 1250;
			lcidCodes[0x40F] = 1252;
			lcidCodes[0x421] = 1252;
			lcidCodes[0x410] = 1252;
			lcidCodes[0x810] = 1252;
			lcidCodes[0x411] = 932;
			lcidCodes[0x10411] = 932;
			lcidCodes[0x412] = 949;
			lcidCodes[0x412] = 949;
			lcidCodes[0x426] = 1257;
			lcidCodes[0x427] = 1257;
			lcidCodes[0x827] = 1257;
			lcidCodes[0x41C] = 1251;
			lcidCodes[0x414] = 1252;
			lcidCodes[0x814] = 1252;
			lcidCodes[0x415] = 1250;
			lcidCodes[0x816] = 1252;
			lcidCodes[0x416] = 1252;
			lcidCodes[0x418] = 1250;
			lcidCodes[0x419] = 1251;
			lcidCodes[0x81A] = 1251;
			lcidCodes[0xC1A] = 1251;
			lcidCodes[0x41B] = 1250;
			lcidCodes[0x424] = 1250;
			lcidCodes[0x80A] = 1252;
			lcidCodes[0x40A] = 1252;
			lcidCodes[0xC0A] = 1252;
			lcidCodes[0x100A] = 1252;
			lcidCodes[0x140A] = 1252;
			lcidCodes[0x180A] = 1252;
			lcidCodes[0x1C0A] = 1252;
			lcidCodes[0x200A] = 1252;
			lcidCodes[0x240A] = 1252;
			lcidCodes[0x280A] = 1252;
			lcidCodes[0x2C0A] = 1252;
			lcidCodes[0x300A] = 1252;
			lcidCodes[0x340A] = 1252;
			lcidCodes[0x380A] = 1252;
			lcidCodes[0x3C0A] = 1252;
			lcidCodes[0x400A] = 1252;
			lcidCodes[0x41D] = 1252;
			lcidCodes[0x41E] = 874;
			lcidCodes[0x41F] = 1254;
			lcidCodes[0x422] = 1251;
			lcidCodes[0x420] = 1256;
			lcidCodes[0x42A] = 1258;
							
			sortCodes[30] = 437;
			sortCodes[31] = 437;
			sortCodes[32] = 437;
			sortCodes[33] = 437;
			sortCodes[34] = 437;
			sortCodes[40] = 850;
			sortCodes[41] = 850;
			sortCodes[42] = 850;
			sortCodes[43] = 850;
			sortCodes[44] = 850;
			sortCodes[49] = 850;
			sortCodes[50] = 1252;
			sortCodes[51] = 1252;
			sortCodes[52] = 1252;
			sortCodes[53] = 1252;
			sortCodes[54] = 1252;
			sortCodes[55] = 850;
			sortCodes[56] = 850;
			sortCodes[57] = 850;
			sortCodes[58] = 850;
			sortCodes[59] = 850;
			sortCodes[60] = 850;
			sortCodes[61] = 850;
			sortCodes[71] = 1252;
			sortCodes[72] = 1252;
			sortCodes[73] = 1252;
			sortCodes[74] = 1252;
			sortCodes[75] = 1252;
			sortCodes[80] = 1250;
			sortCodes[81] = 1250;
			sortCodes[82] = 1250;
			sortCodes[83] = 1250;
			sortCodes[84] = 1250;
			sortCodes[85] = 1250;
			sortCodes[86] = 1250;
			sortCodes[87] = 1250;
			sortCodes[88] = 1250;
			sortCodes[89] = 1250;
			sortCodes[90] = 1250;
			sortCodes[91] = 1250;
			sortCodes[92] = 1250;
			sortCodes[93] = 1250;
			sortCodes[94] = 1250;
			sortCodes[95] = 1250;
			sortCodes[96] = 1250;
			sortCodes[97] = 1250;
			sortCodes[98] = 1250;
			sortCodes[104] = 1251;
			sortCodes[105] = 1251;
			sortCodes[106] = 1251;
			sortCodes[107] = 1251;
			sortCodes[108] = 1251;
			sortCodes[112] = 1253;
			sortCodes[113] = 1253;
			sortCodes[114] = 1253;
			sortCodes[120] = 1253;
			sortCodes[121] = 1253;
			sortCodes[124] = 1253;
			sortCodes[128] = 1254;
			sortCodes[129] = 1254;
			sortCodes[130] = 1254;
			sortCodes[136] = 1255;
			sortCodes[137] = 1255;
			sortCodes[138] = 1255;
			sortCodes[144] = 1256;
			sortCodes[145] = 1256;
			sortCodes[146] = 1256;
			sortCodes[152] = 1257;
			sortCodes[153] = 1257;
			sortCodes[154] = 1257;
			sortCodes[155] = 1257;
			sortCodes[156] = 1257;
			sortCodes[157] = 1257;
			sortCodes[158] = 1257;
			sortCodes[159] = 1257;
			sortCodes[160] = 1257;
			sortCodes[183] = 1252;
			sortCodes[184] = 1252;
			sortCodes[185] = 1252;
			sortCodes[186] = 1252;
			sortCodes[192] = 932;
			sortCodes[193] = 932;
			sortCodes[194] = 949;
			sortCodes[195] = 949;
			sortCodes[196] = 950;
			sortCodes[197] = 950;
			sortCodes[198] = 936;
			sortCodes[199] = 936;
			sortCodes[200] = 932;
			sortCodes[201] = 949;
			sortCodes[202] = 950;
			sortCodes[203] = 936;
			sortCodes[204] = 874;
			sortCodes[205] = 874;
			sortCodes[206] = 874;
		}
		
		public static Encoding GetEncoding (byte[] collation) 
		{
			if (TdsCollation.SortId (collation) != 0)
				return GetEncodingFromSortOrder (collation);
			else
				return GetEncodingFromLCID (collation);
		}
		
		public static Encoding GetEncodingFromLCID (byte[] collation)
		{
			int lcid = TdsCollation.LCID (collation);
			return GetEncodingFromLCID (lcid);
		}
		
		public static Encoding GetEncodingFromLCID (int lcid)
		{
			if (lcidCodes[lcid] != null)
				return Encoding.GetEncoding ((int)lcidCodes[lcid]);
			else
				return null;			
		}
		
		public static Encoding GetEncodingFromSortOrder(byte[] collation)
		{
			int sortId = TdsCollation.SortId (collation);
			return GetEncodingFromSortOrder (sortId);
		}

		public static Encoding GetEncodingFromSortOrder (int sortId)
		{
			if (sortCodes[sortId] != null)
				return Encoding.GetEncoding ((int)sortCodes[sortId]);
			else
				return null;			
		}
	} 	
}
