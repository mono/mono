//
// System.Globalization.CharUnicodeInfo.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// (C)2005 Novell Inc,
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Globalization
{
	public static class CharUnicodeInfo
	{
		public static int GetDecimalDigitValue (char ch)
		{
			int i = (int) ch;
			switch (i) {
			// They are not decimal digits but are regarded as they were.
			case 178:
				return 2;
			case 179:
				return 3;
			case 185:
				return 1;
			case 8304:
				return 0;
			}

			// They are not decimal digits but are regarded as they were.
			if (8308 <= i && i < 8314)
				return i - 8304;
			if (8320 <= i && i < 8330)
				return i - 8320;

			if (!Char.IsDigit (ch))
				return -1;

			if (i < 58)
				return i - 48;

			if (i < 1642)
				return i - 1632;
			if (i < 1786)
				return i - 1776;
			if (i < 2416)
				return i - 2406;
			if (i < 2544)
				return i - 2534;
			if (i < 2672)
				return i - 2662;
			if (i < 2800)
				return i - 2790;
			if (i < 2928)
				return i - 2918;
			if (i < 3056)
				return i - 3046;
			if (i < 3184)
				return i - 3174;
			if (i < 3312)
				return i - 3302;
			if (i < 3440)
				return i - 3430;
			if (i < 3674)
				return i - 3664;
			if (i < 3802)
				return i - 3792;
			if (i < 3882)
				return i - 3872;
			if (i < 4170)
				return i - 4160;
			if (i < 4978)
				return i - 4968;
			if (i < 6122)
				return i - 6112;
			if (i < 6170)
				return i - 6160;
			if (i < 8314)
				return i - 8304;
			if (i < 8330)
				return i - 8320;
			if (i < 65296)
				return -1;
			if (i < 65306)
				return i - 65296;
			return -1;
		}

		public static int GetDecimalDigitValue (string s, int index)
		{
			if (s == null)
				throw new ArgumentNullException ("s");
			return GetDecimalDigitValue (s [index]);
		}

		public static int GetDigitValue (char ch)
		{
			int i = GetDecimalDigitValue (ch);

			if (i >= 0)
				return i;
			i = (int) ch;

			if (i == 9450)
				return 0;

			// They are False in Char.IsDigit(), but returns a digit
			if (i >= 9312 && i < 9321)
				return i - 9311;
			if (i >= 9332 && i < 9341)
				return i - 9331;
			if (i >= 9352 && i < 9361)
				return i - 9351;
			if (i >= 9461 && i < 9470)
				return i - 9460;
			if (i >= 10102 && i < 10111)
				return i - 10101;
			if (i >= 10112 && i < 10121)
				return i - 10111;
			if (i >= 10122 && i < 10131)
				return i - 10121;

			return -1;
		}

		public static int GetDigitValue (string s, int index)
		{
			if (s == null)
				throw new ArgumentNullException ("s");
			return GetDigitValue (s [index]);
		}

		public static double GetNumericValue (char ch)
		{
			int i = GetDigitValue (ch);
			if (i >= 0)
				return i;

			i = (int) ch;

			switch (i) {
			case 188:
				return 0.25;
			case 189:
				return 0.5;
			case 190:
				return 0.75;
			case 2548:
				return 1;
			case 2549:
				return 2;
			case 2550:
				return 3;
			case 2551:
				return 4;
			case 2553:
				return 16;
			case 3056:
				return 10;
			case 3057:
				return 100;
			case 3058:
				return 1000;
			case 4988:
				return 10000;
			case 5870:
				return 17;
			case 5871:
				return 18;
			case 5872:
				return 19;
			case 8531:
				return 1.0 / 3;
			case 8532:
				return 2.0 / 3;
			case 8537:
				return 1.0 / 6;
			case 8538:
				return 5.0 / 6;
			case 8539:
				return 1.0 / 8;
			case 8540:
				return 3.0 / 8;
			case 8541:
				return 5.0 / 8;
			case 8542:
				return 7.0 / 8;
			case 8543:
				return 1;
			case 8556:
				return 50;
			case 8557:
				return 100;
			case 8558:
				return 500;
			case 8559:
				return 1000;
			case 8572:
				return 50;
			case 8573:
				return 100;
			case 8574:
				return 500;
			case 8575:
				return 1000;
			case 8576:
				return 1000;
			case 8577:
				return 5000;
			case 8578:
				return 10000;
			case 9470: // IsNumber(c) is False BTW.
			case 10111:
			case 10121:
			case 10131:
				return 10;
			case 12295:
				return 0;
			case 12344:
				return 10;
			case 12345:
				return 20;
			case 12346:
				return 30;
			}

			// They are not True by IsNumber() but regarded as they were.
			if (9451 <= i && i < 9461)
				return i - 9440;
			if (12321 <= i && i < 12330)
				return i - 12320;
			if (12881 <= i && i < 12896)
				return i - 12860;
			if (12977 <= i && i < 12992)
				return i - 12941;

			if (!char.IsNumber (ch))
				return -1;

			if (i < 3891)
				return 0.5 + i - 3882;
			if (i < 4988)
				return (i - 4977) * 10;
			if (i < 8537)
				return 0.2 * (i - 8532);
			if (i < 8556)
				return i - 8543;
			if (i < 8572)
				return i - 8559;
			if (i < 9332)
				return i - 9311;
			if (i < 9352)
				return i - 9331;
			if (i < 9372)
				return i - 9351;
			if (i < 12694)
				return i - 12689;
			if (i < 12842)
				return i - 12831;
			if (i < 12938)
				return i - 12927;

			return -1;
		}

		public static double GetNumericValue (string s, int index)
		{
			if (s == null)
				throw new ArgumentNullException ("s");
			return GetNumericValue (s [index]);
		}

		public static UnicodeCategory GetUnicodeCategory (char ch)
		{
			return Char.GetUnicodeCategory (ch);
		}

		public static UnicodeCategory GetUnicodeCategory (string s, int index)
		{
			if (s == null)
				throw new ArgumentNullException ("s");
			return Char.GetUnicodeCategory (s, index);
		}
	}
}
