// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// internal System.Xml.XmlUtil
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
// Code ported from Open XML 2.3.17 (Delphi/Kylix)
//
// (C) 2001 Daniel Weber
//

using System;
using System.IO;

namespace System.Xml
{
	/// <summary>
	/// Helper class with static utility functions that are not Xml version specific
	/// Such as encoding changes
	/// </summary>
	internal class XmlUtil
	{
		public static char Iso8859_1ToUTF16Char(byte P)
		{
			return (char) P;
		}

		public static char Iso8859_2ToUTF16Char(byte P)
		{
			switch (P)
			{
				case 0xa1: return (char) 0x0104;  // LATIN CAPITAL LETTER A WITH OGONEK
				case 0xa2: return (char) 0x02d8;  // BREVE
				case 0xa3: return (char) 0x0141;  // LATIN CAPITAL LETTER L WITH STROKE
				case 0xa5: return (char) 0x0132;  // LATIN CAPITAL LETTER L WITH CARON
				case 0xa6: return (char) 0x015a;  // LATIN CAPITAL LETTER S WITH ACUTE
				case 0xa9: return (char) 0x0160;  // LATIN CAPITAL LETTER S WITH CARON
				case 0xaa: return (char) 0x015e;  // LATIN CAPITAL LETTER S WITH CEDILLA
				case 0xab: return (char) 0x0164;  // LATIN CAPITAL LETTER T WITH CARON
				case 0xac: return (char) 0x0179;  // LATIN CAPITAL LETTER Z WITH ACUTE
				case 0xae: return (char) 0x017d;  // LATIN CAPITAL LETTER Z WITH CARON
				case 0xaf: return (char) 0x017b;  // LATIN CAPITAL LETTER Z WITH DOT ABOVE
				case 0xb1: return (char) 0x0105;  // LATIN SMALL LETTER A WITH OGONEK
				case 0xb2: return (char) 0x02db;  // OGONEK
				case 0xb3: return (char) 0x0142;  // LATIN SMALL LETTER L WITH STROKE
				case 0xb5: return (char) 0x013e;  // LATIN SMALL LETTER L WITH CARON
				case 0xb6: return (char) 0x015b;  // LATIN SMALL LETTER S WITH ACUTE
				case 0xb7: return (char) 0x02c7;  // CARON
				case 0xb9: return (char) 0x0161;  // LATIN SMALL LETTER S WITH CARON
				case 0xba: return (char) 0x015f;  // LATIN SMALL LETTER S WITH CEDILLA
				case 0xbb: return (char) 0x0165;  // LATIN SMALL LETTER T WITH CARON
				case 0xbc: return (char) 0x017a;  // LATIN SMALL LETTER Z WITH ACUTE
				case 0xbd: return (char) 0x02dd;  // DOUBLE ACUTE ACCENT
				case 0xbe: return (char) 0x017e;  // LATIN SMALL LETTER Z WITH CARON
				case 0xbf: return (char) 0x017c;  // LATIN SMALL LETTER Z WITH DOT ABOVE
				case 0xc0: return (char) 0x0154;  // LATIN CAPITAL LETTER R WITH ACUTE
				case 0xc3: return (char) 0x0102;  // LATIN CAPITAL LETTER A WITH BREVE
				case 0xc5: return (char) 0x0139;  // LATIN CAPITAL LETTER L WITH ACUTE
				case 0xc6: return (char) 0x0106;  // LATIN CAPITAL LETTER C WITH ACUTE
				case 0xc8: return (char) 0x010c;  // LATIN CAPITAL LETTER C WITH CARON
				case 0xca: return (char) 0x0118;  // LATIN CAPITAL LETTER E WITH OGONEK
				case 0xcc: return (char) 0x011a;  // LATIN CAPITAL LETTER E WITH CARON
				case 0xcf: return (char) 0x010e;  // LATIN CAPITAL LETTER D WITH CARON
				case 0xd0: return (char) 0x0110;  // LATIN CAPITAL LETTER D WITH STROKE
				case 0xd1: return (char) 0x0143;  // LATIN CAPITAL LETTER N WITH ACUTE
				case 0xd2: return (char) 0x0147;  // LATIN CAPITAL LETTER N WITH CARON
				case 0xd5: return (char) 0x0150;  // LATIN CAPITAL LETTER O WITH DOUBLE ACUTE
				case 0xd8: return (char) 0x0158;  // LATIN CAPITAL LETTER R WITH CARON
				case 0xd9: return (char) 0x016e;  // LATIN CAPITAL LETTER U WITH RING ABOVE
				case 0xdb: return (char) 0x0170;  // LATIN CAPITAL LETTER U WITH WITH DOUBLE ACUTE
				case 0xde: return (char) 0x0162;  // LATIN CAPITAL LETTER T WITH CEDILLA
				case 0xe0: return (char) 0x0155;  // LATIN SMALL LETTER R WITH ACUTE
				case 0xe3: return (char) 0x0103;  // LATIN SMALL LETTER A WITH BREVE
				case 0xe5: return (char) 0x013a;  // LATIN SMALL LETTER L WITH ACUTE
				case 0xe6: return (char) 0x0107;  // LATIN SMALL LETTER C WITH ACUTE
				case 0xe8: return (char) 0x010d;  // LATIN SMALL LETTER C WITH CARON
				case 0xea: return (char) 0x0119;  // LATIN SMALL LETTER E WITH OGONEK
				case 0xec: return (char) 0x011b;  // LATIN SMALL LETTER E WITH CARON
				case 0xef: return (char) 0x010f;  // LATIN SMALL LETTER D WITH CARON
				case 0xf0: return (char) 0x0111;  // LATIN SMALL LETTER D WITH STROKE
				case 0xf1: return (char) 0x0144;  // LATIN SMALL LETTER N WITH ACUTE
				case 0xf2: return (char) 0x0148;  // LATIN SMALL LETTER N WITH CARON
				case 0xf5: return (char) 0x0151;  // LATIN SMALL LETTER O WITH DOUBLE ACUTE
				case 0xf8: return (char) 0x0159;  // LATIN SMALL LETTER R WITH CARON
				case 0xf9: return (char) 0x016f;  // LATIN SMALL LETTER U WITH RING ABOVE
				case 0xfb: return (char) 0x0171;  // LATIN SMALL LETTER U WITH WITH DOUBLE ACUTE
				case 0xfe: return (char) 0x0163;  // LATIN SMALL LETTER T WITH CEDILLA
				case 0xff: return (char) 0x02d9;  // DOT ABOVE
				default:
					return (char) P;
			}
		}

		public static char Iso8859_3ToUTF16Char( byte P)
		{
			switch (P)
			{
				case 0xa1: return (char) 0x0126;  // LATIN CAPITAL LETTER H WITH STROKE
				case 0xa2: return (char) 0x02d8;  // BREVE
				case 0xa5: throw new InvalidOperationException("Invalid ISO-8859-3 sequence [" + P.ToString() + "]");
				case 0xa6: return (char) 0x0124;  // LATIN CAPITAL LETTER H WITH CIRCUMFLEX
				case 0xa9: return (char) 0x0130;  // LATIN CAPITAL LETTER I WITH DOT ABOVE
				case 0xaa: return (char) 0x015e;  // LATIN CAPITAL LETTER S WITH CEDILLA
				case 0xab: return (char) 0x011e;  // LATIN CAPITAL LETTER G WITH BREVE
				case 0xac: return (char) 0x0134;  // LATIN CAPITAL LETTER J WITH CIRCUMFLEX
				case 0xae: throw new InvalidOperationException("Invalid ISO-8859-3 sequence [" + P.ToString() + "]");
				case 0xaf: return (char) 0x017b;  // LATIN CAPITAL LETTER Z WITH DOT
				case 0xb1: return (char) 0x0127;  // LATIN SMALL LETTER H WITH STROKE
				case 0xb6: return (char) 0x0125;  // LATIN SMALL LETTER H WITH CIRCUMFLEX
				case 0xb9: return (char) 0x0131;  // LATIN SMALL LETTER DOTLESS I
				case 0xba: return (char) 0x015f;  // LATIN SMALL LETTER S WITH CEDILLA
				case 0xbb: return (char) 0x011f;  // LATIN SMALL LETTER G WITH BREVE
				case 0xbc: return (char) 0x0135;  // LATIN SMALL LETTER J WITH CIRCUMFLEX
				case 0xbe: throw new InvalidOperationException("Invalid ISO-8859-3 sequence [" + P.ToString() + "]");
				case 0xbf: return (char) 0x017c;  // LATIN SMALL LETTER Z WITH DOT
				case 0xc3: throw new InvalidOperationException("Invalid ISO-8859-3 sequence [" + P.ToString() + "]");
				case 0xc5: return (char) 0x010a;  // LATIN CAPITAL LETTER C WITH DOT ABOVE
				case 0xc6: return (char) 0x0108;  // LATIN CAPITAL LETTER C WITH CIRCUMFLEX
				case 0xd0: throw new InvalidOperationException("Invalid ISO-8859-3 sequence [" + P.ToString() + "]");
				case 0xd5: return (char) 0x0120;  // LATIN CAPITAL LETTER G WITH DOT ABOVE
				case 0xd8: return (char) 0x011c;  // LATIN CAPITAL LETTER G WITH CIRCUMFLEX
				case 0xdd: return (char) 0x016c;  // LATIN CAPITAL LETTER U WITH BREVE
				case 0xde: return (char) 0x015c;  // LATIN CAPITAL LETTER S WITH CIRCUMFLEX
				case 0xe3: throw new InvalidOperationException("Invalid ISO-8859-3 sequence [" + P.ToString() + "]");
				case 0xe5: return (char) 0x010b;  // LATIN SMALL LETTER C WITH DOT ABOVE
				case 0xe6: return (char) 0x0109;  // LATIN SMALL LETTER C WITH CIRCUMFLEX
				case 0xf0: throw new InvalidOperationException("Invalid ISO-8859-3 sequence [" + P.ToString() + "]");
				case 0xf5: return (char) 0x0121;  // LATIN SMALL LETTER G WITH DOT ABOVE
				case 0xf8: return (char) 0x011d;  // LATIN SMALL LETTER G WITH CIRCUMFLEX
				case 0xfd: return (char) 0x016d;  // LATIN SMALL LETTER U WITH BREVE
				case 0xfe: return (char) 0x015d;  // LATIN SMALL LETTER S WITH CIRCUMFLEX
				case 0xff: return (char) 0x02d9;  // DOT ABOVE
				default:
					return (char) P;
			}
		}

		public static char Iso8859_4ToUTF16Char( byte P)
		{
			switch (P)
			{
				case 0xa1: return (char) 0x0104;  // LATIN CAPITAL LETTER A WITH OGONEK
				case 0xa2: return (char) 0x0138;  // LATIN SMALL LETTER KRA
				case 0xa3: return (char) 0x0156;  // LATIN CAPITAL LETTER R WITH CEDILLA
				case 0xa5: return (char) 0x0128;  // LATIN CAPITAL LETTER I WITH TILDE
				case 0xa6: return (char) 0x013b;  // LATIN CAPITAL LETTER L WITH CEDILLA
				case 0xa9: return (char) 0x0160;  // LATIN CAPITAL LETTER S WITH CARON
				case 0xaa: return (char) 0x0112;  // LATIN CAPITAL LETTER E WITH MACRON
				case 0xab: return (char) 0x0122;  // LATIN CAPITAL LETTER G WITH CEDILLA
				case 0xac: return (char) 0x0166;  // LATIN CAPITAL LETTER T WITH STROKE
				case 0xae: return (char) 0x017d;  // LATIN CAPITAL LETTER Z WITH CARON
				case 0xb1: return (char) 0x0105;  // LATIN SMALL LETTER A WITH OGONEK
				case 0xb2: return (char) 0x02db;  // OGONEK
				case 0xb3: return (char) 0x0157;  // LATIN SMALL LETTER R WITH CEDILLA
				case 0xb5: return (char) 0x0129;  // LATIN SMALL LETTER I WITH TILDE
				case 0xb6: return (char) 0x013c;  // LATIN SMALL LETTER L WITH CEDILLA
				case 0xb7: return (char) 0x02c7;  // CARON
				case 0xb9: return (char) 0x0161;  // LATIN SMALL LETTER S WITH CARON
				case 0xba: return (char) 0x0113;  // LATIN SMALL LETTER E WITH MACRON
				case 0xbb: return (char) 0x0123;  // LATIN SMALL LETTER G WITH CEDILLA
				case 0xbc: return (char) 0x0167;  // LATIN SMALL LETTER T WITH STROKE
				case 0xbd: return (char) 0x014a;  // LATIN CAPITAL LETTER ENG
				case 0xbe: return (char) 0x017e;  // LATIN SMALL LETTER Z WITH CARON
				case 0xbf: return (char) 0x014b;  // LATIN SMALL LETTER ENG
				case 0xc0: return (char) 0x0100;  // LATIN CAPITAL LETTER A WITH MACRON
				case 0xc7: return (char) 0x012e;  // LATIN CAPITAL LETTER I WITH OGONEK
				case 0xc8: return (char) 0x010c;  // LATIN CAPITAL LETTER C WITH CARON
				case 0xca: return (char) 0x0118;  // LATIN CAPITAL LETTER E WITH OGONEK
				case 0xcc: return (char) 0x0116;  // LATIN CAPITAL LETTER E WITH DOT ABOVE
				case 0xcf: return (char) 0x012a;  // LATIN CAPITAL LETTER I WITH MACRON
				case 0xd0: return (char) 0x0110;  // LATIN CAPITAL LETTER D WITH STROKE
				case 0xd1: return (char) 0x0145;  // LATIN CAPITAL LETTER N WITH CEDILLA
				case 0xd2: return (char) 0x014c;  // LATIN CAPITAL LETTER O WITH MACRON
				case 0xd3: return (char) 0x0136;  // LATIN CAPITAL LETTER K WITH CEDILLA
				case 0xd9: return (char) 0x0172;  // LATIN CAPITAL LETTER U WITH OGONEK
				case 0xdd: return (char) 0x0168;  // LATIN CAPITAL LETTER U WITH TILDE
				case 0xde: return (char) 0x016a;  // LATIN CAPITAL LETTER U WITH MACRON
				case 0xe0: return (char) 0x0101;  // LATIN SMALL LETTER A WITH MACRON
				case 0xe7: return (char) 0x012f;  // LATIN SMALL LETTER I WITH OGONEK
				case 0xe8: return (char) 0x010d;  // LATIN SMALL LETTER C WITH CARON
				case 0xea: return (char) 0x0119;  // LATIN SMALL LETTER E WITH OGONEK
				case 0xec: return (char) 0x0117;  // LATIN SMALL LETTER E WITH DOT ABOVE
				case 0xef: return (char) 0x012b;  // LATIN SMALL LETTER I WITH MACRON
				case 0xf0: return (char) 0x0111;  // LATIN SMALL LETTER D WITH STROKE
				case 0xf1: return (char) 0x0146;  // LATIN SMALL LETTER N WITH CEDILLA
				case 0xf2: return (char) 0x014d;  // LATIN SMALL LETTER O WITH MACRON
				case 0xf3: return (char) 0x0137;  // LATIN SMALL LETTER K WITH CEDILLA
				case 0xf9: return (char) 0x0173;  // LATIN SMALL LETTER U WITH OGONEK
				case 0xfd: return (char) 0x0169;  // LATIN SMALL LETTER U WITH TILDE
				case 0xfe: return (char) 0x016b;  // LATIN SMALL LETTER U WITH MACRON
				case 0xff: return (char) 0x02d9;  // DOT ABOVE
				default:
					return (char) P;
			}
		}

		public static char Iso8859_5ToUTF16Char(byte P)
		{
			if ( (P >= 0x00) & (P <= 0xa0) )
				return (char) P;
			else if ( P == 0xad )
				return (char) P;
			else if ( P == 0xf0 )
				return (char) 0x2116;	// NUMERO SIGN
			else if ( P == 0xfd )
				return (char) 0x00a7;	// SECTION SIGN
			else
				return System.Convert.ToChar( 0x0360 + P );
		}

		public static char Iso8859_6ToUTF16Char(byte P)
		{
			if ( (P >= 0x00) & ( P <= 0xa0) )
				return (char) P;
			else if ( P == 0xa4)
				return (char) P;
			else if ( ( P == 0xac )	| (P==0xbb) | (P==0xbf) )
				return System.Convert.ToChar(P + 0x0580);
			else if ( (P >= 0xc1) & ( P <= 0xda) )
				return System.Convert.ToChar(P + 0x0580);
			else if ( (P >= 0xe0) & ( P <= 0xf2) )
				return System.Convert.ToChar(P + 0x0580);
			else
				throw new InvalidOperationException("Invalid ISO-8859-6 sequence [" + P.ToString() + "]");
		}

		public static char Iso8859_7ToUTF16Char(byte P)
		{
			if ( (P >= 0x00) & ( P <= 0xa0) )
				return (char) P;
			else if ( (P >= 0xa6) & ( P <= 0xa9) )
				return (char) P;
			else if ( (P >= 0xab) & ( P <= 0xad) )
				return (char) P;
			else if ( (P >= 0xb0) & ( P <= 0xb3) )
				return (char) P;
			else if ( (P == 0xb7) | (P==0xbb) | (P==0xbd) )
				return (char) P;
			else if ( P ==0xa1 )	// LEFT SINGLE QUOTATION MARK
				return (char) 0x2018;
			else if ( P==0xa2 )		// RIGHT SINGLE QUOTATION MARK
				return (char) 0x2019;
			else if ( P==0xaf )		// HORIZONTAL BAR
				return (char) 0x2015;
			else if ( (P==0xd2) | (P==0xff) )
				throw new InvalidOperationException("Invalid ISO-8859-7 sequence [" + P.ToString() + "]");
			else
				return System.Convert.ToChar(P + 0x02d0);

		}

		public static char Iso8859_8ToUTF16Char(byte P)
		{
			if ( (P >= 0x00) & ( P <= 0xa0) )
				return (char) P;
			else if ( (P >= 0xa2) & ( P <= 0xa9) )
				return (char) P;
			else if ( (P >= 0xab) & ( P <= 0xae) )
				return (char) P;
			else if ( (P >= 0xb0) & ( P <= 0xb9) )
				return (char) P;
			else if ( (P >= 0xbb) & ( P <= 0xbe) )
				return (char) P;
			else if ( P==0xaa )		// MULTIPLICATION SIGN
				return (char) 0x00d7;
			else if ( P==0xaf )		// OVERLINE
				return (char) 0x203e;
			else if ( P==0xba )		// DIVISION SIGN
				return (char) 0x00f7;
			else if ( P==0xdf )		// DOUBLE LOW LINE
				return (char) 0x2017;
			else if ( (P >= 0xe0) & ( P <= 0xfa) )
				return System.Convert.ToChar(P + 0x04e0);
			else 
				throw new InvalidOperationException("Invalid ISO-8859-8 sequence [" + P.ToString() + "]");
		}

		public static char Iso8859_9ToUTF16Char(byte P)
		{
			switch (P)
			{
				case 0xd0:	return (char) 0x011e;  // LATIN CAPITAL LETTER G WITH BREVE
				case 0xdd:	return (char) 0x0130;  // LATIN CAPITAL LETTER I WITH DOT ABOVE
				case 0xde:	return (char) 0x015e;  // LATIN CAPITAL LETTER S WITH CEDILLA
				case 0xf0:  return (char) 0x011f;  // LATIN SMALL LETTER G WITH BREVE
				case 0xfd:	return (char) 0x0131;  // LATIN SMALL LETTER I WITH DOT ABOVE
				case 0xfe:	return (char) 0x015f;  // LATIN SMALL LETTER S WITH CEDILLA
				default:
					return (char) P;
			}
		}

		public static char Iso8859_10ToUTF16Char(byte P)
		{
			switch (P)
			{
				case 0xa1: return (char) 0x0104;  // LATIN CAPITAL LETTER A WITH OGONEK
				case 0xa2: return (char) 0x0112;  // LATIN CAPITAL LETTER E WITH MACRON
				case 0xa3: return (char) 0x0122;  // LATIN CAPITAL LETTER G WITH CEDILLA
				case 0xa4: return (char) 0x012a;  // LATIN CAPITAL LETTER I WITH MACRON
				case 0xa5: return (char) 0x0128;  // LATIN CAPITAL LETTER I WITH TILDE
				case 0xa6: return (char) 0x0136;  // LATIN CAPITAL LETTER K WITH CEDILLA
				case 0xa8: return (char) 0x013b;  // LATIN CAPITAL LETTER L WITH CEDILLA
				case 0xa9: return (char) 0x0110;  // LATIN CAPITAL LETTER D WITH STROKE
				case 0xaa: return (char) 0x0160;  // LATIN CAPITAL LETTER S WITH CARON
				case 0xab: return (char) 0x0166;  // LATIN CAPITAL LETTER T WITH STROKE
				case 0xac: return (char) 0x017d;  // LATIN CAPITAL LETTER Z WITH CARON
				case 0xae: return (char) 0x016a;  // LATIN CAPITAL LETTER U WITH MACRON
				case 0xaf: return (char) 0x014a;  // LATIN CAPITAL LETTER ENG
				case 0xb1: return (char) 0x0105;  // LATIN SMALL LETTER A WITH OGONEK
				case 0xb2: return (char) 0x0113;  // LATIN SMALL LETTER E WITH MACRON
				case 0xb3: return (char) 0x0123;  // LATIN SMALL LETTER G WITH CEDILLA
				case 0xb4: return (char) 0x012b;  // LATIN SMALL LETTER I WITH MACRON
				case 0xb5: return (char) 0x0129;  // LATIN SMALL LETTER I WITH TILDE
				case 0xb6: return (char) 0x0137;  // LATIN SMALL LETTER K WITH CEDILLA
				case 0xb8: return (char) 0x013c;  // LATIN SMALL LETTER L WITH CEDILLA
				case 0xb9: return (char) 0x0111;  // LATIN SMALL LETTER D WITH STROKE
				case 0xba: return (char) 0x0161;  // LATIN SMALL LETTER S WITH CARON
				case 0xbb: return (char) 0x0167;  // LATIN SMALL LETTER T WITH STROKE
				case 0xbc: return (char) 0x017e;  // LATIN SMALL LETTER Z WITH CARON
				case 0xbd: return (char) 0x2015;  // HORIZONTAL BAR
				case 0xbe: return (char) 0x016b;  // LATIN SMALL LETTER U WITH MACRON
				case 0xbf: return (char) 0x014b;  // LATIN SMALL LETTER ENG
				case 0xc0: return (char) 0x0100;  // LATIN CAPITAL LETTER A WITH MACRON
				case 0xc7: return (char) 0x012e;  // LATIN CAPITAL LETTER I WITH OGONEK
				case 0xc8: return (char) 0x010c;  // LATIN CAPITAL LETTER C WITH CARON
				case 0xca: return (char) 0x0118;  // LATIN CAPITAL LETTER E WITH OGONEK
				case 0xcc: return (char) 0x0116;  // LATIN CAPITAL LETTER E WITH DOT ABOVE
				case 0xd1: return (char) 0x0145;  // LATIN CAPITAL LETTER N WITH CEDILLA
				case 0xd2: return (char) 0x014c;  // LATIN CAPITAL LETTER O WITH MACRON
				case 0xd7: return (char) 0x0168;  // LATIN CAPITAL LETTER U WITH TILDE
				case 0xd9: return (char) 0x0172;  // LATIN CAPITAL LETTER U WITH OGONEK
				case 0xe0: return (char) 0x0101;  // LATIN SMALL LETTER A WITH MACRON
				case 0xe7: return (char) 0x012f;  // LATIN SMALL LETTER I WITH OGONEK
				case 0xe8: return (char) 0x010d;  // LATIN SMALL LETTER C WITH CARON
				case 0xea: return (char) 0x0119;  // LATIN SMALL LETTER E WITH OGONEK
				case 0xec: return (char) 0x0117;  // LATIN SMALL LETTER E WITH DOT ABOVE
				case 0xf1: return (char) 0x0146;  // LATIN SMALL LETTER N WITH CEDILLA
				case 0xf2: return (char) 0x014d;  // LATIN SMALL LETTER O WITH MACRON
				case 0xf7: return (char) 0x0169;  // LATIN SMALL LETTER U WITH TILDE
				case 0xf9: return (char) 0x0173;  // LATIN SMALL LETTER U WITH OGONEK
				case 0xff: return (char) 0x0138;  // LATIN SMALL LETTER KRA
				default:
					return (char) P;
			}
		}

		public static char Iso8859_13ToUTF16Char(byte P)
		{
			switch(P)
			{
				case 0xa1: return (char) 0x201d;  // RIGHT DOUBLE QUOTATION MARK
				case 0xa5: return (char) 0x201e;  // DOUBLE LOW-9 QUOTATION MARK
				case 0xa8: return (char) 0x00d8;  // LATIN CAPITAL LETTER O WITH STROKE
				case 0xaa: return (char) 0x0156;  // LATIN CAPITAL LETTER R WITH CEDILLA
				case 0xaf: return (char) 0x00c6;  // LATIN CAPITAL LETTER AE
				case 0xb4: return (char) 0x201c;  // LEFT DOUBLE QUOTATION MARK
				case 0xb8: return (char) 0x00f8;  // LATIN SMALL LETTER O WITH STROKE
				case 0xba: return (char) 0x0157;  // LATIN SMALL LETTER R WITH CEDILLA
				case 0xbf: return (char) 0x00e6;  // LATIN SMALL LETTER AE
				case 0xc0: return (char) 0x0104;  // LATIN CAPITAL LETTER A WITH OGONEK
				case 0xc1: return (char) 0x012e;  // LATIN CAPITAL LETTER I WITH OGONEK
				case 0xc2: return (char) 0x0100;  // LATIN CAPITAL LETTER A WITH MACRON
				case 0xc3: return (char) 0x0106;  // LATIN CAPITAL LETTER C WITH ACUTE
				case 0xc6: return (char) 0x0118;  // LATIN CAPITAL LETTER E WITH OGONEK
				case 0xc7: return (char) 0x0112;  // LATIN CAPITAL LETTER E WITH MACRON
				case 0xc8: return (char) 0x010c;  // LATIN CAPITAL LETTER C WITH CARON
				case 0xca: return (char) 0x0179;  // LATIN CAPITAL LETTER Z WITH ACUTE
				case 0xcb: return (char) 0x0116;  // LATIN CAPITAL LETTER E WITH DOT ABOVE
				case 0xcc: return (char) 0x0122;  // LATIN CAPITAL LETTER G WITH CEDILLA
				case 0xcd: return (char) 0x0136;  // LATIN CAPITAL LETTER K WITH CEDILLA
				case 0xce: return (char) 0x012a;  // LATIN CAPITAL LETTER I WITH MACRON
				case 0xcf: return (char) 0x013b;  // LATIN CAPITAL LETTER L WITH CEDILLA
				case 0xd0: return (char) 0x0160;  // LATIN CAPITAL LETTER S WITH CARON
				case 0xd1: return (char) 0x0143;  // LATIN CAPITAL LETTER N WITH ACUTE
				case 0xd2: return (char) 0x0145;  // LATIN CAPITAL LETTER N WITH CEDILLA
				case 0xd4: return (char) 0x014c;  // LATIN CAPITAL LETTER O WITH MACRON
				case 0xd8: return (char) 0x0172;  // LATIN CAPITAL LETTER U WITH OGONEK
				case 0xd9: return (char) 0x0141;  // LATIN CAPITAL LETTER L WITH STROKE
				case 0xda: return (char) 0x015a;  // LATIN CAPITAL LETTER S WITH ACUTE
				case 0xdb: return (char) 0x016a;  // LATIN CAPITAL LETTER U WITH MACRON
				case 0xdd: return (char) 0x017b;  // LATIN CAPITAL LETTER Z WITH DOT ABOVE
				case 0xde: return (char) 0x017d;  // LATIN CAPITAL LETTER Z WITH CARON
				case 0xe0: return (char) 0x0105;  // LATIN SMALL LETTER A WITH OGONEK
				case 0xe1: return (char) 0x012f;  // LATIN SMALL LETTER I WITH OGONEK
				case 0xe2: return (char) 0x0101;  // LATIN SMALL LETTER A WITH MACRON
				case 0xe3: return (char) 0x0107;  // LATIN SMALL LETTER C WITH ACUTE
				case 0xe6: return (char) 0x0119;  // LATIN SMALL LETTER E WITH OGONEK
				case 0xe7: return (char) 0x0113;  // LATIN SMALL LETTER E WITH MACRON
				case 0xe8: return (char) 0x010d;  // LATIN SMALL LETTER C WITH CARON
				case 0xea: return (char) 0x017a;  // LATIN SMALL LETTER Z WITH ACUTE
				case 0xeb: return (char) 0x0117;  // LATIN SMALL LETTER E WITH DOT ABOVE
				case 0xec: return (char) 0x0123;  // LATIN SMALL LETTER G WITH CEDILLA
				case 0xed: return (char) 0x0137;  // LATIN SMALL LETTER K WITH CEDILLA
				case 0xee: return (char) 0x012b;  // LATIN SMALL LETTER I WITH MACRON
				case 0xef: return (char) 0x013c;  // LATIN SMALL LETTER L WITH CEDILLA
				case 0xf0: return (char) 0x0161;  // LATIN SMALL LETTER S WITH CARON
				case 0xf1: return (char) 0x0144;  // LATIN SMALL LETTER N WITH ACUTE
				case 0xf2: return (char) 0x0146;  // LATIN SMALL LETTER N WITH CEDILLA
				case 0xf4: return (char) 0x014d;  // LATIN SMALL LETTER O WITH MACRON
				case 0xf8: return (char) 0x0173;  // LATIN SMALL LETTER U WITH OGONEK
				case 0xf9: return (char) 0x0142;  // LATIN SMALL LETTER L WITH STROKE
				case 0xfa: return (char) 0x015b;  // LATIN SMALL LETTER S WITH ACUTE
				case 0xfb: return (char) 0x016b;  // LATIN SMALL LETTER U WITH MACRON
				case 0xfd: return (char) 0x017c;  // LATIN SMALL LETTER Z WITH DOT ABOVE
				case 0xfe: return (char) 0x017e;  // LATIN SMALL LETTER Z WITH CARON
				case 0xff: return (char) 0x2019;  // RIGHT SINGLE QUOTATION MARK
				default:
					return (char) P;
			}
		}

		public static char Iso8859_14ToUTF16Char(byte P)
		{
			switch (P)
			{
				case 0xa1: return (char) 0x1e02;  // LATIN CAPITAL LETTER B WITH DOT ABOVE
				case 0xa2: return (char) 0x1e03;  // LATIN SMALL LETTER B WITH DOT ABOVE
				case 0xa4: return (char) 0x010a;  // LATIN CAPITAL LETTER C WITH DOT ABOVE
				case 0xa5: return (char) 0x010b;  // LATIN SMALL LETTER C WITH DOT ABOVE
				case 0xa6: return (char) 0x1e0a;  // LATIN CAPITAL LETTER D WITH DOT ABOVE
				case 0xa8: return (char) 0x1e80;  // LATIN CAPITAL LETTER W WITH GRAVE
				case 0xaa: return (char) 0x1e82;  // LATIN CAPITAL LETTER W WITH ACUTE
				case 0xab: return (char) 0x1e0b;  // LATIN SMALL LETTER D WITH DOT ABOVE
				case 0xac: return (char) 0x1ef2;  // LATIN CAPITAL LETTER Y WITH GRAVE
				case 0xaf: return (char) 0x0178;  // LATIN CAPITAL LETTER Y WITH DIAERESIS
				case 0xb0: return (char) 0x1e1e;  // LATIN CAPITAL LETTER F WITH DOT ABOVE
				case 0xb1: return (char) 0x1e1f;  // LATIN SMALL LETTER F WITH DOT ABOVE
				case 0xb2: return (char) 0x0120;  // LATIN CAPITAL LETTER G WITH DOT ABOVE
				case 0xb3: return (char) 0x0121;  // LATIN SMALL LETTER G WITH DOT ABOVE
				case 0xb4: return (char) 0x1e40;  // LATIN CAPITAL LETTER M WITH DOT ABOVE
				case 0xb5: return (char) 0x1e41;  // LATIN SMALL LETTER M WITH DOT ABOVE
				case 0xb7: return (char) 0x1e56;  // LATIN CAPITAL LETTER P WITH DOT ABOVE
				case 0xb8: return (char) 0x1e81;  // LATIN SMALL LETTER W WITH GRAVE
				case 0xb9: return (char) 0x1e57;  // LATIN SMALL LETTER P WITH DOT ABOVE
				case 0xba: return (char) 0x1e83;  // LATIN SMALL LETTER W WITH ACUTE
				case 0xbb: return (char) 0x1e60;  // LATIN CAPITAL LETTER S WITH DOT ABOVE
				case 0xbc: return (char) 0x1ef3;  // LATIN SMALL LETTER Y WITH GRAVE
				case 0xbd: return (char) 0x1e84;  // LATIN CAPITAL LETTER W WITH DIAERESIS
				case 0xbe: return (char) 0x1e85;  // LATIN SMALL LETTER W WITH DIAERESIS
				case 0xbf: return (char) 0x1e61;  // LATIN SMALL LETTER S WITH DOT ABOVE
				case 0xd0: return (char) 0x0174;  // LATIN CAPITAL LETTER W WITH CIRCUMFLEX
				case 0xd7: return (char) 0x1e6a;  // LATIN CAPITAL LETTER T WITH DOT ABOVE
				case 0xde: return (char) 0x0176;  // LATIN CAPITAL LETTER Y WITH CIRCUMFLEX
				case 0xf0: return (char) 0x0175;  // LATIN SMALL LETTER W WITH CIRCUMFLEX
				case 0xf7: return (char) 0x1e6b;  // LATIN SMALL LETTER T WITH DOT ABOVE
				case 0xfe: return (char) 0x0177;  // LATIN SMALL LETTER Y WITH CIRCUMFLEX
				default:
					return (char) P;
			}
		}

		public static char Iso8859_15ToUTF16Char(byte P)
		{
			switch (P)
			{
				case 0xa4: return (char) 0x20ac;  // EURO SIGN
				case 0xa6: return (char) 0x00a6;  // LATIN CAPITAL LETTER S WITH CARON
				case 0xa8: return (char) 0x0161;  // LATIN SMALL LETTER S WITH CARON
				case 0xb4: return (char) 0x017d;  // LATIN CAPITAL LETTER Z WITH CARON
				case 0xb8: return (char) 0x017e;  // LATIN SMALL LETTER Z WITH CARON
				case 0xbc: return (char) 0x0152;  // LATIN CAPITAL LIGATURE OE
				case 0xbd: return (char) 0x0153;  // LATIN SMALL LIGATURE OE
				case 0xbe: return (char) 0x0178;  // LATIN CAPITAL LETTER Y WITH DIAERESIS
				default:
					return (char) P;
			}
		}

		public static char KOI8_RToUTF16Char(byte P)
		{
			switch (P)
			{
				case 0x80: return (char) 0x2500;  // BOX DRAWINGS LIGHT HORIZONTAL
				case 0x81: return (char) 0x2502;  // BOX DRAWINGS LIGHT VERTICAL
				case 0x82: return (char) 0x250c;  // BOX DRAWINGS LIGHT DOWN AND RIGHT
				case 0x83: return (char) 0x2510;  // BOX DRAWINGS LIGHT DOWN AND LEFT
				case 0x84: return (char) 0x2514;  // BOX DRAWINGS LIGHT UP AND RIGHT
				case 0x85: return (char) 0x2518;  // BOX DRAWINGS LIGHT UP AND LEFT
				case 0x86: return (char) 0x251c;  // BOX DRAWINGS LIGHT VERTICAL AND RIGHT
				case 0x87: return (char) 0x2524;  // BOX DRAWINGS LIGHT VERTICAL AND LEFT
				case 0x88: return (char) 0x252c;  // BOX DRAWINGS LIGHT DOWN AND HORIZONTAL
				case 0x89: return (char) 0x2534;  // BOX DRAWINGS LIGHT UP AND HORIZONTAL
				case 0x8a: return (char) 0x253c;  // BOX DRAWINGS LIGHT VERTICAL AND HORIZONTAL
				case 0x8b: return (char) 0x2580;  // UPPER HALF BLOCK
				case 0x8c: return (char) 0x2584;  // LOWER HALF BLOCK
				case 0x8d: return (char) 0x2588;  // FULL BLOCK
				case 0x8e: return (char) 0x258c;  // LEFT HALF BLOCK
				case 0x8f: return (char) 0x2590;  // RIGHT HALF BLOCK
				case 0x90: return (char) 0x2591;  // LIGHT SHADE
				case 0x91: return (char) 0x2592;  // MEDIUM SHADE
				case 0x92: return (char) 0x2593;  // DARK SHADE
				case 0x93: return (char) 0x2320;  // TOP HALF INTEGRAL
				case 0x94: return (char) 0x25a0;  // BLACK SQUARE
				case 0x95: return (char) 0x2219;  // BULLET OPERATOR
				case 0x96: return (char) 0x221a;  // SQUARE ROOT
				case 0x97: return (char) 0x2248;  // ALMOST EQUAL TO
				case 0x98: return (char) 0x2264;  // LESS-THAN OR EQUAL TO
				case 0x99: return (char) 0x2265;  // GREATER-THAN OR EQUAL TO
				case 0x9a: return (char) 0x00a0;  // NO-BREAK SPACE
				case 0x9b: return (char) 0x2321;  // BOTTOM HALF INTEGRAL
				case 0x9c: return (char) 0x00b0;  // DEGREE SIGN
				case 0x9d: return (char) 0x00b2;  // SUPERSCRIPT TWO
				case 0x9e: return (char) 0x00b7;  // MIDDLE DOT
				case 0x9f: return (char) 0x00f7;  // DIVISION SIGN
				case 0xa0: return (char) 0x2550;  // BOX DRAWINGS DOUBLE HORIZONTAL
				case 0xa1: return (char) 0x2551;  // BOX DRAWINGS DOUBLE VERTICAL
				case 0xa2: return (char) 0x2552;  // BOX DRAWINGS DOWN SINGLE AND RIGHT DOUBLE
				case 0xa3: return (char) 0x0451;  // CYRILLIC SMALL LETTER IO
				case 0xa4: return (char) 0x2553;  // BOX DRAWINGS DOWN DOUBLE AND RIGHT SINGLE
				case 0xa5: return (char) 0x2554;  // BOX DRAWINGS DOUBLE DOWN AND RIGHT
				case 0xa6: return (char) 0x2555;  // BOX DRAWINGS DOWN SINGLE AND LEFT DOUBLE
				case 0xa7: return (char) 0x2556;  // BOX DRAWINGS DOWN DOUBLE AND LEFT SINGLE
				case 0xa8: return (char) 0x2557;  // BOX DRAWINGS DOUBLE DOWN AND LEFT
				case 0xa9: return (char) 0x2558;  // BOX DRAWINGS UP SINGLE AND RIGHT DOUBLE
				case 0xaa: return (char) 0x2559;  // BOX DRAWINGS UP DOUBLE AND RIGHT SINGLE
				case 0xab: return (char) 0x255a;  // BOX DRAWINGS DOUBLE UP AND RIGHT
				case 0xac: return (char) 0x255b;  // BOX DRAWINGS UP SINGLE AND LEFT DOUBLE
				case 0xad: return (char) 0x255c;  // BOX DRAWINGS UP DOUBLE AND LEFT SINGLE
				case 0xae: return (char) 0x255d;  // BOX DRAWINGS DOUBLE UP AND LEFT
				case 0xaf: return (char) 0x255e;  // BOX DRAWINGS VERTICAL SINGLE AND RIGHT DOUBLE
				case 0xb0: return (char) 0x255f;  // BOX DRAWINGS VERTICAL DOUBLE AND RIGHT SINGLE
				case 0xb1: return (char) 0x2560;  // BOX DRAWINGS DOUBLE VERTICAL AND RIGHT
				case 0xb2: return (char) 0x2561;  // BOX DRAWINGS VERTICAL SINGLE AND LEFT DOUBLE
				case 0xb3: return (char) 0x0401;  // CYRILLIC CAPITAL LETTER IO
				case 0xb4: return (char) 0x2562;  // BOX DRAWINGS VERTICAL DOUBLE AND LEFT SINGLE
				case 0xb5: return (char) 0x2563;  // BOX DRAWINGS DOUBLE VERTICAL AND LEFT
				case 0xb6: return (char) 0x2564;  // BOX DRAWINGS DOWN SINGLE AND HORIZONTAL DOUBLE
				case 0xb7: return (char) 0x2565;  // BOX DRAWINGS DOWN DOUBLE AND HORIZONTAL SINGLE
				case 0xb8: return (char) 0x2566;  // BOX DRAWINGS DOUBLE DOWN AND HORIZONTAL
				case 0xb9: return (char) 0x2567;  // BOX DRAWINGS UP SINGLE AND HORIZONTAL DOUBLE
				case 0xba: return (char) 0x2568;  // BOX DRAWINGS UP DOUBLE AND HORIZONTAL SINGLE
				case 0xbb: return (char) 0x2569;  // BOX DRAWINGS DOUBLE UP AND HORIZONTAL
				case 0xbc: return (char) 0x256a;  // BOX DRAWINGS VERTICAL SINGLE AND HORIZONTAL DOUBLE
				case 0xbd: return (char) 0x256b;  // BOX DRAWINGS VERTICAL DOUBLE AND HORIZONTAL SINGLE
				case 0xbe: return (char) 0x256c;  // BOX DRAWINGS DOUBLE VERTICAL AND HORIZONTAL
				case 0xbf: return (char) 0x00a9;  // COPYRIGHT SIGN
				case 0xc0: return (char) 0x044e;  // CYRILLIC SMALL LETTER YU
				case 0xc1: return (char) 0x0430;  // CYRILLIC SMALL LETTER A
				case 0xc2: return (char) 0x0431;  // CYRILLIC SMALL LETTER BE
				case 0xc3: return (char) 0x0446;  // CYRILLIC SMALL LETTER TSE
				case 0xc4: return (char) 0x0434;  // CYRILLIC SMALL LETTER DE
				case 0xc5: return (char) 0x0435;  // CYRILLIC SMALL LETTER IE
				case 0xc6: return (char) 0x0444;  // CYRILLIC SMALL LETTER EF
				case 0xc7: return (char) 0x0433;  // CYRILLIC SMALL LETTER GHE
				case 0xc8: return (char) 0x0445;  // CYRILLIC SMALL LETTER HA
				case 0xc9: return (char) 0x0438;  // CYRILLIC SMALL LETTER I
				case 0xca: return (char) 0x0439;  // CYRILLIC SMALL LETTER SHORT I
				case 0xcb: return (char) 0x043a;  // CYRILLIC SMALL LETTER KA
				case 0xcc: return (char) 0x043b;  // CYRILLIC SMALL LETTER EL
				case 0xcd: return (char) 0x043c;  // CYRILLIC SMALL LETTER EM
				case 0xce: return (char) 0x043d;  // CYRILLIC SMALL LETTER EN
				case 0xcf: return (char) 0x043e;  // CYRILLIC SMALL LETTER O
				case 0xd0: return (char) 0x043f;  // CYRILLIC SMALL LETTER PE
				case 0xd1: return (char) 0x044f;  // CYRILLIC SMALL LETTER YA
				case 0xd2: return (char) 0x0440;  // CYRILLIC SMALL LETTER ER
				case 0xd3: return (char) 0x0441;  // CYRILLIC SMALL LETTER ES
				case 0xd4: return (char) 0x0442;  // CYRILLIC SMALL LETTER TE
				case 0xd5: return (char) 0x0443;  // CYRILLIC SMALL LETTER U
				case 0xd6: return (char) 0x0436;  // CYRILLIC SMALL LETTER ZHE
				case 0xd7: return (char) 0x0432;  // CYRILLIC SMALL LETTER VE
				case 0xd8: return (char) 0x044c;  // CYRILLIC SMALL LETTER SOFT SIGN
				case 0xd9: return (char) 0x044b;  // CYRILLIC SMALL LETTER YERU
				case 0xda: return (char) 0x0437;  // CYRILLIC SMALL LETTER ZE
				case 0xdb: return (char) 0x0448;  // CYRILLIC SMALL LETTER SHA
				case 0xdc: return (char) 0x044d;  // CYRILLIC SMALL LETTER E
				case 0xdd: return (char) 0x0449;  // CYRILLIC SMALL LETTER SHCHA
				case 0xde: return (char) 0x0447;  // CYRILLIC SMALL LETTER CHE
				case 0xdf: return (char) 0x044a;  // CYRILLIC SMALL LETTER HARD SIGN
				case 0xe0: return (char) 0x042e;  // CYRILLIC CAPITAL LETTER YU
				case 0xe1: return (char) 0x0410;  // CYRILLIC CAPITAL LETTER A
				case 0xe2: return (char) 0x0411;  // CYRILLIC CAPITAL LETTER BE
				case 0xe3: return (char) 0x0426;  // CYRILLIC CAPITAL LETTER TSE
				case 0xe4: return (char) 0x0414;  // CYRILLIC CAPITAL LETTER DE
				case 0xe5: return (char) 0x0415;  // CYRILLIC CAPITAL LETTER IE
				case 0xe6: return (char) 0x0424;  // CYRILLIC CAPITAL LETTER EF
				case 0xe7: return (char) 0x0413;  // CYRILLIC CAPITAL LETTER GHE
				case 0xe8: return (char) 0x0425;  // CYRILLIC CAPITAL LETTER HA
				case 0xe9: return (char) 0x0418;  // CYRILLIC CAPITAL LETTER I
				case 0xea: return (char) 0x0419;  // CYRILLIC CAPITAL LETTER SHORT I
				case 0xeb: return (char) 0x041a;  // CYRILLIC CAPITAL LETTER KA
				case 0xec: return (char) 0x041b;  // CYRILLIC CAPITAL LETTER EL
				case 0xed: return (char) 0x041c;  // CYRILLIC CAPITAL LETTER EM
				case 0xee: return (char) 0x041d;  // CYRILLIC CAPITAL LETTER EN
				case 0xef: return (char) 0x041e;  // CYRILLIC CAPITAL LETTER O
				case 0xf0: return (char) 0x041f;  // CYRILLIC CAPITAL LETTER PE
				case 0xf1: return (char) 0x042f;  // CYRILLIC CAPITAL LETTER YA
				case 0xf2: return (char) 0x0420;  // CYRILLIC CAPITAL LETTER ER
				case 0xf3: return (char) 0x0421;  // CYRILLIC CAPITAL LETTER ES
				case 0xf4: return (char) 0x0422;  // CYRILLIC CAPITAL LETTER TE
				case 0xf5: return (char) 0x0423;  // CYRILLIC CAPITAL LETTER U
				case 0xf6: return (char) 0x0416;  // CYRILLIC CAPITAL LETTER ZHE
				case 0xf7: return (char) 0x0412;  // CYRILLIC CAPITAL LETTER VE
				case 0xf8: return (char) 0x042c;  // CYRILLIC CAPITAL LETTER SOFT SIGN
				case 0xf9: return (char) 0x042b;  // CYRILLIC CAPITAL LETTER YERU
				case 0xfa: return (char) 0x0417;  // CYRILLIC CAPITAL LETTER ZE
				case 0xfb: return (char) 0x0428;  // CYRILLIC CAPITAL LETTER SHA
				case 0xfc: return (char) 0x042d;  // CYRILLIC CAPITAL LETTER E
				case 0xfd: return (char) 0x0429;  // CYRILLIC CAPITAL LETTER SHCHA
				case 0xfe: return (char) 0x0427;  // CYRILLIC CAPITAL LETTER CHE
				case 0xff: return (char) 0x042a;  // CYRILLIC CAPITAL LETTER HARD SIGN
				default:
					return (char) P;
			}
		}

		public static char cp10000_MacRomanToUTF16Char(byte P)
		{
			switch (P)
			{
				case 0x80: return (char) 0x00c4;  // LATIN CAPITAL LETTER A WITH DIAERESIS
				case 0x81: return (char) 0x00c5;  // LATIN CAPITAL LETTER A WITH RING ABOVE
				case 0x82: return (char) 0x00c7;  // LATIN CAPITAL LETTER C WITH CEDILLA
				case 0x83: return (char) 0x00c9;  // LATIN CAPITAL LETTER E WITH ACUTE
				case 0x84: return (char) 0x00d1;  // LATIN CAPITAL LETTER N WITH TILDE
				case 0x85: return (char) 0x00d6;  // LATIN CAPITAL LETTER O WITH DIAERESIS
				case 0x86: return (char) 0x00dc;  // LATIN CAPITAL LETTER U WITH DIAERESIS
				case 0x87: return (char) 0x00e1;  // LATIN SMALL LETTER A WITH ACUTE
				case 0x88: return (char) 0x00e0;  // LATIN SMALL LETTER A WITH GRAVE
				case 0x89: return (char) 0x00e2;  // LATIN SMALL LETTER A WITH CIRCUMFLEX
				case 0x8a: return (char) 0x00e4;  // LATIN SMALL LETTER A WITH DIAERESIS
				case 0x8b: return (char) 0x00e3;  // LATIN SMALL LETTER A WITH TILDE
				case 0x8c: return (char) 0x00e5;  // LATIN SMALL LETTER A WITH RING ABOVE
				case 0x8d: return (char) 0x00e7;  // LATIN SMALL LETTER C WITH CEDILLA
				case 0x8e: return (char) 0x00e9;  // LATIN SMALL LETTER E WITH ACUTE
				case 0x8f: return (char) 0x00e8;  // LATIN SMALL LETTER E WITH GRAVE
				case 0x90: return (char) 0x00ea;  // LATIN SMALL LETTER E WITH CIRCUMFLEX
				case 0x91: return (char) 0x00eb;  // LATIN SMALL LETTER E WITH DIAERESIS
				case 0x92: return (char) 0x00ed;  // LATIN SMALL LETTER I WITH ACUTE
				case 0x93: return (char) 0x00ec;  // LATIN SMALL LETTER I WITH GRAVE
				case 0x94: return (char) 0x00ee;  // LATIN SMALL LETTER I WITH CIRCUMFLEX
				case 0x95: return (char) 0x00ef;  // LATIN SMALL LETTER I WITH DIAERESIS
				case 0x96: return (char) 0x00f1;  // LATIN SMALL LETTER N WITH TILDE
				case 0x97: return (char) 0x00f3;  // LATIN SMALL LETTER O WITH ACUTE
				case 0x98: return (char) 0x00f2;  // LATIN SMALL LETTER O WITH GRAVE
				case 0x99: return (char) 0x00f4;  // LATIN SMALL LETTER O WITH CIRCUMFLEX
				case 0x9a: return (char) 0x00f6;  // LATIN SMALL LETTER O WITH DIAERESIS
				case 0x9b: return (char) 0x00f5;  // LATIN SMALL LETTER O WITH TILDE
				case 0x9c: return (char) 0x00fa;  // LATIN SMALL LETTER U WITH ACUTE
				case 0x9d: return (char) 0x00f9;  // LATIN SMALL LETTER U WITH GRAVE
				case 0x9e: return (char) 0x00fb;  // LATIN SMALL LETTER U WITH CIRCUMFLEX
				case 0x9f: return (char) 0x00fc;  // LATIN SMALL LETTER U WITH DIAERESIS
				case 0xa0: return (char) 0x2020;  // DAGGER
				case 0xa1: return (char) 0x00b0;  // DEGREE SIGN
				case 0xa4: return (char) 0x00a7;  // SECTION SIGN
				case 0xa5: return (char) 0x2022;  // BULLET
				case 0xa6: return (char) 0x00b6;  // PILCROW SIGN
				case 0xa7: return (char) 0x00df;  // LATIN SMALL LETTER SHARP S
				case 0xa8: return (char) 0x00ae;  // REGISTERED SIGN
				case 0xaa: return (char) 0x2122;  // TRADE MARK SIGN
				case 0xab: return (char) 0x00b4;  // ACUTE ACCENT
				case 0xac: return (char) 0x00a8;  // DIAERESIS
				case 0xad: return (char) 0x2260;  // NOT EQUAL TO
				case 0xae: return (char) 0x00c6;  // LATIN CAPITAL LIGATURE AE
				case 0xaf: return (char) 0x00d8;  // LATIN CAPITAL LETTER O WITH STROKE
				case 0xb0: return (char) 0x221e;  // INFINITY
				case 0xb2: return (char) 0x2264;  // LESS-THAN OR EQUAL TO
				case 0xb3: return (char) 0x2265;  // GREATER-THAN OR EQUAL TO
				case 0xb4: return (char) 0x00a5;  // YEN SIGN
				case 0xb6: return (char) 0x2202;  // PARTIAL DIFFERENTIAL
				case 0xb7: return (char) 0x2211;  // N-ARY SUMMATION
				case 0xb8: return (char) 0x220f;  // N-ARY PRODUCT
				case 0xb9: return (char) 0x03c0;  // GREEK SMALL LETTER PI
				case 0xba: return (char) 0x222b;  // INTEGRAL
				case 0xbb: return (char) 0x00aa;  // FEMININE ORDINAL INDICATOR
				case 0xbc: return (char) 0x00ba;  // MASCULINE ORDINAL INDICATOR
				case 0xbd: return (char) 0x2126;  // OHM SIGN
				case 0xbe: return (char) 0x00e6;  // LATIN SMALL LIGATURE AE
				case 0xbf: return (char) 0x00f8;  // LATIN SMALL LETTER O WITH STROKE
				case 0xc0: return (char) 0x00bf;  // INVERTED QUESTION MARK
				case 0xc1: return (char) 0x00a1;  // INVERTED EXCLAMATION MARK
				case 0xc2: return (char) 0x00ac;  // NOT SIGN
				case 0xc3: return (char) 0x221a;  // SQUARE ROOT
				case 0xc4: return (char) 0x0192;  // LATIN SMALL LETTER F WITH HOOK
				case 0xc5: return (char) 0x2248;  // ALMOST EQUAL TO
				case 0xc6: return (char) 0x2206;  // INCREMENT
				case 0xc7: return (char) 0x00ab;  // LEFT-POINTING DOUBLE ANGLE QUOTATION MARK
				case 0xc8: return (char) 0x00bb;  // RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK
				case 0xc9: return (char) 0x2026;  // HORIZONTAL ELLIPSIS
				case 0xca: return (char) 0x00a0;  // NO-BREAK SPACE
				case 0xcb: return (char) 0x00c0;  // LATIN CAPITAL LETTER A WITH GRAVE
				case 0xcc: return (char) 0x00c3;  // LATIN CAPITAL LETTER A WITH TILDE
				case 0xcd: return (char) 0x00d5;  // LATIN CAPITAL LETTER O WITH TILDE
				case 0xce: return (char) 0x0152;  // LATIN CAPITAL LIGATURE OE
				case 0xcf: return (char) 0x0153;  // LATIN SMALL LIGATURE OE
				case 0xd0: return (char) 0x2013;  // EN DASH
				case 0xd1: return (char) 0x2014;  // EM DASH
				case 0xd2: return (char) 0x201c;  // LEFT DOUBLE QUOTATION MARK
				case 0xd3: return (char) 0x201d;  // RIGHT DOUBLE QUOTATION MARK
				case 0xd4: return (char) 0x2018;  // LEFT SINGLE QUOTATION MARK
				case 0xd5: return (char) 0x2019;  // RIGHT SINGLE QUOTATION MARK
				case 0xd6: return (char) 0x00f7;  // DIVISION SIGN
				case 0xd7: return (char) 0x25ca;  // LOZENGE
				case 0xd8: return (char) 0x00ff;  // LATIN SMALL LETTER Y WITH DIAERESIS
				case 0xd9: return (char) 0x0178;  // LATIN CAPITAL LETTER Y WITH DIAERESIS
				case 0xda: return (char) 0x2044;  // FRACTION SLASH
				case 0xdb: return (char) 0x00a4;  // CURRENCY SIGN
				case 0xdc: return (char) 0x2039;  // SINGLE LEFT-POINTING ANGLE QUOTATION MARK
				case 0xdd: return (char) 0x203a;  // SINGLE RIGHT-POINTING ANGLE QUOTATION MARK
				case 0xde: return (char) 0xfb01;  // LATIN SMALL LIGATURE FI
				case 0xdf: return (char) 0xfb02;  // LATIN SMALL LIGATURE FL
				case 0xe0: return (char) 0x2021;  // DOUBLE DAGGER
				case 0xe1: return (char) 0x00b7;  // MIDDLE DOT
				case 0xe2: return (char) 0x201a;  // SINGLE LOW-9 QUOTATION MARK
				case 0xe3: return (char) 0x201e;  // DOUBLE LOW-9 QUOTATION MARK
				case 0xe4: return (char) 0x2030;  // PER MILLE SIGN
				case 0xe5: return (char) 0x00c2;  // LATIN CAPITAL LETTER A WITH CIRCUMFLEX
				case 0xe6: return (char) 0x00ca;  // LATIN CAPITAL LETTER E WITH CIRCUMFLEX
				case 0xe7: return (char) 0x00c1;  // LATIN CAPITAL LETTER A WITH ACUTE
				case 0xe8: return (char) 0x00cb;  // LATIN CAPITAL LETTER E WITH DIAERESIS
				case 0xe9: return (char) 0x00c8;  // LATIN CAPITAL LETTER E WITH GRAVE
				case 0xea: return (char) 0x00cd;  // LATIN CAPITAL LETTER I WITH ACUTE
				case 0xeb: return (char) 0x00ce;  // LATIN CAPITAL LETTER I WITH CIRCUMFLEX
				case 0xec: return (char) 0x00cf;  // LATIN CAPITAL LETTER I WITH DIAERESIS
				case 0xed: return (char) 0x00cc;  // LATIN CAPITAL LETTER I WITH GRAVE
				case 0xee: return (char) 0x00d3;  // LATIN CAPITAL LETTER O WITH ACUTE
				case 0xef: return (char) 0x00d4;  // LATIN CAPITAL LETTER O WITH CIRCUMFLEX
				case 0xf0: throw new InvalidOperationException("Invalid cp10000_MacRoman sequence [" + P.ToString() + "]"); 
				case 0xf1: return (char) 0x00d2;  // LATIN CAPITAL LETTER O WITH GRAVE
				case 0xf2: return (char) 0x00da;  // LATIN CAPITAL LETTER U WITH ACUTE
				case 0xf3: return (char) 0x00db;  // LATIN CAPITAL LETTER U WITH CIRCUMFLEX
				case 0xf4: return (char) 0x00d9;  // LATIN CAPITAL LETTER U WITH GRAVE
				case 0xf5: return (char) 0x0131;  // LATIN SMALL LETTER DOTLESS I
				case 0xf6: return (char) 0x02c6;  // MODIFIER LETTER CIRCUMFLEX ACCENT
				case 0xf7: return (char) 0x02dc;  // SMALL TILDE
				case 0xf8: return (char) 0x00af;  // MACRON
				case 0xf9: return (char) 0x02d8;  // BREVE
				case 0xfa: return (char) 0x02d9;  // DOT ABOVE
				case 0xfb: return (char) 0x02da;  // RING ABOVE
				case 0xfc: return (char) 0x00b8;  // CEDILLA
				case 0xfd: return (char) 0x02dd;  // DOUBLE ACUTE ACCENT
				case 0xfe: return (char) 0x02db;  // OGONEK
				case 0xff: return (char) 0x02c7;  // CARON
				default:
					return (char) P;
			}
		}

		public static char cp1250ToUTF16Char(byte P)
		{
			// This function was provided by Miloslav Skácel (ported by DrW)
			switch (P)
			{
				case 0x80: throw new InvalidOperationException("Invalid Windows-1250 sequence [" + P.ToString() + "]"); 
				case 0x81: throw new InvalidOperationException("Invalid Windows-1250 sequence [" + P.ToString() + "]"); 
				case 0x83: throw new InvalidOperationException("Invalid Windows-1250 sequence [" + P.ToString() + "]"); 
				case 0x88: throw new InvalidOperationException("Invalid Windows-1250 sequence [" + P.ToString() + "]"); 
				case 0x90: throw new InvalidOperationException("Invalid Windows-1250 sequence [" + P.ToString() + "]"); 
				case 0x98: throw new InvalidOperationException("Invalid Windows-1250 sequence [" + P.ToString() + "]"); 
				case 0x82: return (char) 0x201a;  // SINGLE LOW-9 QUOTATION MARK
				case 0x84: return (char) 0x201e;  // DOUBLE LOW-9 QUOTATION MARK
				case 0x85: return (char) 0x2026;  // HORIZONTAL ELLIPSIS
				case 0x86: return (char) 0x2020;  // DAGGER
				case 0x87: return (char) 0x2021;  // DOUBLE DAGGER
				case 0x89: return (char) 0x2030;  // PER MILLE SIGN
				case 0x8a: return (char) 0x0160;  // LATIN CAPITAL LETTER S WITH CARON
				case 0x8b: return (char) 0x2039;  // SINGLE LEFT-POINTING ANGLE QUOTATION MARK
				case 0x8c: return (char) 0x015a;  // LATIN CAPITAL LETTER S WITH ACUTE
				case 0x8d: return (char) 0x0164;  // LATIN CAPITAL LETTER T WITH CARON
				case 0x8e: return (char) 0x017d;  // LATIN CAPITAL LETTER Z WITH CARON
				case 0x8f: return (char) 0x0179;  // LATIN CAPITAL LETTER Z WITH ACUTE
				case 0x91: return (char) 0x2018;  // LEFT SINGLE QUOTATION MARK
				case 0x92: return (char) 0x2019;  // RIGHT SINGLE QUOTATION MARK
				case 0x93: return (char) 0x201c;  // LEFT DOUBLE QUOTATION MARK
				case 0x94: return (char) 0x201d;  // RIGHT DOUBLE QUOTATION MARK
				case 0x95: return (char) 0x2022;  // BULLET
				case 0x96: return (char) 0x2013;  // EN-DASH
				case 0x97: return (char) 0x2014;  // EM-DASH
				case 0x99: return (char) 0x2122;  // TRADE MARK SIGN
				case 0x9a: return (char) 0x0161;  // LATIN SMALL LETTER S WITH CARON
				case 0x9b: return (char) 0x203a;  // SINGLE RIGHT-POINTING ANGLE QUOTATION MARK
				case 0x9c: return (char) 0x015b;  // LATIN SMALL LETTER S WITH ACUTE
				case 0x9d: return (char) 0x0165;  // LATIN SMALL LETTER T WITH CARON
				case 0x9e: return (char) 0x017e;  // LATIN SMALL LETTER Z WITH CARON
				case 0x9f: return (char) 0x017a;  // LATIN SMALL LETTER Z WITH ACUTE
				case 0xa0: return (char) 0x00a0;  // NO-BREAK SPACE
				case 0xa1: return (char) 0x02c7;  // CARON
				case 0xa2: return (char) 0x02d8;  // BREVE
				case 0xa3: return (char) 0x0141;  // LATIN CAPITAL LETTER L WITH STROKE
				case 0xa4: return (char) 0x00a4;  // CURRENCY SIGN
				case 0xa5: return (char) 0x0104;  // LATIN CAPITAL LETTER A WITH OGONEK
				case 0xa6: return (char) 0x00a6;  // BROKEN BAR
				case 0xa7: return (char) 0x00a7;  // SECTION SIGN
				case 0xa8: return (char) 0x00a8;  // DIAERESIS
				case 0xa9: return (char) 0x00a9;  // COPYRIGHT SIGN
				case 0xaa: return (char) 0x015e;  // LATIN CAPITAL LETTER S WITH CEDILLA
				case 0xab: return (char) 0x00ab;  // LEFT-POINTING DOUBLE ANGLE QUOTATION MARK
				case 0xac: return (char) 0x00ac;  // NOT SIGN
				case 0xad: return (char) 0x00ad;  // SOFT HYPHEN
				case 0xae: return (char) 0x00ae;  // REGISTERED SIGN
				case 0xaf: return (char) 0x017b;  // LATIN CAPITAL LETTER Z WITH DOT ABOVE
				case 0xb0: return (char) 0x00b0;  // DEGREE SIGN
				case 0xb1: return (char) 0x00b1;  // PLUS-MINUS SIGN
				case 0xb2: return (char) 0x02db;  // OGONEK
				case 0xb3: return (char) 0x0142;  // LATIN SMALL LETTER L WITH STROKE
				case 0xb4: return (char) 0x00b4;  // ACUTE ACCENT
				case 0xb5: return (char) 0x00b5;  // MIKRO SIGN
				case 0xb6: return (char) 0x00b6;  // PILCROW SIGN
				case 0xb7: return (char) 0x00b7;  // MIDDLE DOT
				case 0xb8: return (char) 0x00b8;  // CEDILLA
				case 0xb9: return (char) 0x0105;  // LATIN SMALL LETTER A WITH OGONEK
				case 0xba: return (char) 0x015f;  // LATIN SMALL LETTER S WITH CEDILLA
				case 0xbb: return (char) 0x00bb;  // RIGHT-POINTING DOUBLE ANGLE QUOTATION MARK
				case 0xbc: return (char) 0x013d;  // LATIN CAPITAL LETTER L WITH CARON
				case 0xbd: return (char) 0x02dd;  // DOUBLE ACUTE ACCENT
				case 0xbe: return (char) 0x013e;  // LATIN SMALL LETTER L WITH CARON
				case 0xbf: return (char) 0x017c;  // LATIN SMALL LETTER Z WITH DOT ABOVE
				case 0xc0: return (char) 0x0154;  // LATIN CAPITAL LETTER R WITH ACUTE
				case 0xc1: return (char) 0x00c1;  // LATIN CAPITAL LETTER A WITH ACUTE
				case 0xc2: return (char) 0x00c2;  // LATIN CAPITAL LETTER A WITH CIRCUMFLEX
				case 0xc3: return (char) 0x0102;  // LATIN CAPITAL LETTER A WITH BREVE
				case 0xc4: return (char) 0x00c4;  // LATIN CAPITAL LETTER A WITH DIAERESIS
				case 0xc5: return (char) 0x0139;  // LATIN CAPITAL LETTER L WITH ACUTE
				case 0xc6: return (char) 0x0106;  // LATIN CAPITAL LETTER C WITH ACUTE
				case 0xc7: return (char) 0x00c7;  // LATIN CAPITAL LETTER C WITH CEDILLA
				case 0xc8: return (char) 0x010c;  // LATIN CAPITAL LETTER C WITH CARON
				case 0xc9: return (char) 0x00c9;  // LATIN CAPITAL LETTER E WITH ACUTE
				case 0xca: return (char) 0x0118;  // LATIN CAPITAL LETTER E WITH OGONEK
				case 0xcb: return (char) 0x00cb;  // LATIN CAPITAL LETTER E WITH DIAERESIS
				case 0xcc: return (char) 0x011a;  // LATIN CAPITAL LETTER E WITH CARON
				case 0xcd: return (char) 0x00cd;  // LATIN CAPITAL LETTER I WITH ACUTE
				case 0xce: return (char) 0x00ce;  // LATIN CAPITAL LETTER I WITH CIRCUMFLEX
				case 0xcf: return (char) 0x010e;  // LATIN CAPITAL LETTER D WITH CARON
				case 0xd0: return (char) 0x0110;  // LATIN CAPITAL LETTER D WITH STROKE
				case 0xd1: return (char) 0x0143;  // LATIN CAPITAL LETTER N WITH ACUTE
				case 0xd2: return (char) 0x0147;  // LATIN CAPITAL LETTER N WITH CARON
				case 0xd3: return (char) 0x00d3;  // LATIN CAPITAL LETTER O WITH ACUTE
				case 0xd4: return (char) 0x00d4;  // LATIN CAPITAL LETTER O WITH CIRCUMFLEX
				case 0xd5: return (char) 0x0150;  // LATIN CAPITAL LETTER O WITH DOUBLE ACUTE
				case 0xd6: return (char) 0x00d6;  // LATIN CAPITAL LETTER O WITH DIAERESIS
				case 0xd7: return (char) 0x00d7;  // MULTIPLICATION SIGN
				case 0xd8: return (char) 0x0158;  // LATIN CAPITAL LETTER R WITH CARON
				case 0xd9: return (char) 0x016e;  // LATIN CAPITAL LETTER U WITH RING ABOVE
				case 0xda: return (char) 0x00da;  // LATIN CAPITAL LETTER U WITH ACUTE
				case 0xdb: return (char) 0x0170;  // LATIN CAPITAL LETTER U WITH WITH DOUBLE ACUTE
				case 0xdc: return (char) 0x00dc;  // LATIN CAPITAL LETTER U WITH DIAERESIS
				case 0xdd: return (char) 0x00dd;  // LATIN CAPITAL LETTER Y WITH ACUTE
				case 0xde: return (char) 0x0162;  // LATIN CAPITAL LETTER T WITH CEDILLA
				case 0xdf: return (char) 0x00df;  // LATIN SMALL LETTER SHARP S
				case 0xe0: return (char) 0x0155;  // LATIN SMALL LETTER R WITH ACUTE
				case 0xe1: return (char) 0x00e1;  // LATIN SMALL LETTER A WITH ACUTE
				case 0xe2: return (char) 0x00e2;  // LATIN SMALL LETTER A WITH CIRCUMFLEX
				case 0xe3: return (char) 0x0103;  // LATIN SMALL LETTER A WITH BREVE
				case 0xe4: return (char) 0x00e4;  // LATIN SMALL LETTER A WITH DIAERESIS
				case 0xe5: return (char) 0x013a;  // LATIN SMALL LETTER L WITH ACUTE
				case 0xe6: return (char) 0x0107;  // LATIN SMALL LETTER C WITH ACUTE
				case 0xe7: return (char) 0x00e7;  // LATIN SMALL LETTER C WITH CEDILLA
				case 0xe8: return (char) 0x010d;  // LATIN SMALL LETTER C WITH CARON 100D
				case 0xe9: return (char) 0x00e9;  // LATIN SMALL LETTER E WITH ACUTE
				case 0xea: return (char) 0x0119;  // LATIN SMALL LETTER E WITH OGONEK
				case 0xeb: return (char) 0x00eb;  // LATIN SMALL LETTER E WITH DIAERESIS
				case 0xec: return (char) 0x011b;  // LATIN SMALL LETTER E WITH CARON
				case 0xed: return (char) 0x00ed;  // LATIN SMALL LETTER I WITH ACUTE
				case 0xee: return (char) 0x00ee;  // LATIN SMALL LETTER I WITH CIRCUMFLEX
				case 0xef: return (char) 0x010f;  // LATIN SMALL LETTER D WITH CARON
				case 0xf0: return (char) 0x0111;  // LATIN SMALL LETTER D WITH STROKE
				case 0xf1: return (char) 0x0144;  // LATIN SMALL LETTER N WITH ACUTE
				case 0xf2: return (char) 0x0148;  // LATIN SMALL LETTER N WITH CARON
				case 0xf3: return (char) 0x00f3;  // LATIN SMALL LETTER O WITH ACUTE
				case 0xf4: return (char) 0x00f4;  // LATIN SMALL LETTER O WITH CIRCUMFLEX
				case 0xf5: return (char) 0x0151;  // LATIN SMALL LETTER O WITH DOUBLE ACUTE
				case 0xf6: return (char) 0x00f6;  // LATIN SMALL LETTER O WITH DIAERESIS
				case 0xf7: return (char) 0x00f7;  // DIVISION SIGN
				case 0xf8: return (char) 0x0159;  // LATIN SMALL LETTER R WITH CARON
				case 0xf9: return (char) 0x016f;  // LATIN SMALL LETTER U WITH RING ABOVE
				case 0xfa: return (char) 0x00fa;  // LATIN SMALL LETTER U WITH ACUTE
				case 0xfb: return (char) 0x0171;  // LATIN SMALL LETTER U WITH WITH DOUBLE ACUTE
				case 0xfc: return (char) 0x00fc;  // LATIN SMALL LETTER U WITH DIAERESIS
				case 0xfd: return (char) 0x00fd;  // LATIN SMALL LETTER Y WITH ACUTE
				case 0xfe: return (char) 0x0163;  // LATIN SMALL LETTER T WITH CEDILLA
				case 0xff: return (char) 0x02d9;  // DOT ABOVE
				default:
					return (char) P;
			}
		}

		public static char cp1251ToUTF16Char(byte P)
		{
			switch (P)
			{
				case 0x80: return (char) 0x0402;  // CYRILLIC CAPITAL LETTER DJE
				case 0x81: return (char) 0x0403;  // CYRILLIC CAPITAL LETTER GJE
				case 0x82: return (char) 0x201a;  // SINGLE LOW-9 QUOTATION MARK
				case 0x83: return (char) 0x0453;  // CYRILLIC SMALL LETTER GJE
				case 0x84: return (char) 0x201e;  // DOUBLE LOW-9 QUOTATION MARK
				case 0x85: return (char) 0x2026;  // HORIZONTAL ELLIPSIS
				case 0x86: return (char) 0x2020;  // DAGGER
				case 0x87: return (char) 0x2021;  // DOUBLE DAGGER
				case 0x88: return (char) 0x20ac;  // EURO SIGN
				case 0x89: return (char) 0x2030;  // PER MILLE SIGN
				case 0x8a: return (char) 0x0409;  // CYRILLIC CAPITAL LETTER LJE
				case 0x8b: return (char) 0x2039;  // SINGLE LEFT-POINTING ANGLE QUOTATION MARK
				case 0x8c: return (char) 0x040a;  // CYRILLIC CAPITAL LETTER NJE
				case 0x8d: return (char) 0x040c;  // CYRILLIC CAPITAL LETTER KJE
				case 0x8e: return (char) 0x040b;  // CYRILLIC CAPITAL LETTER TSHE
				case 0x8f: return (char) 0x040f;  // CYRILLIC CAPITAL LETTER DZHE
				case 0x90: return (char) 0x0452;  // CYRILLIC SMALL LETTER DJE
				case 0x91: return (char) 0x2018;  // LEFT SINGLE QUOTATION MARK
				case 0x92: return (char) 0x2019;  // RIGHT SINGLE QUOTATION MARK
				case 0x93: return (char) 0x201c;  // LEFT DOUBLE QUOTATION MARK
				case 0x94: return (char) 0x201d;  // RIGHT DOUBLE QUOTATION MARK
				case 0x95: return (char) 0x2022;  // BULLET
				case 0x96: return (char) 0x2013;  // EN DASH
				case 0x97: return (char) 0x2014;  // EM DASH
				case 0x98: throw new InvalidOperationException("Invalid cp1251 sequence [" + P.ToString() + "]"); 
				case 0x99: return (char) 0x2122;  // TRADE MARK SIGN
				case 0x9a: return (char) 0x0459;  // CYRILLIC SMALL LETTER LJE
				case 0x9b: return (char) 0x203a;  // SINGLE RIGHT-POINTING ANGLE QUOTATION MARK
				case 0x9c: return (char) 0x045a;  // CYRILLIC SMALL LETTER NJE
				case 0x9d: return (char) 0x045c;  // CYRILLIC SMALL LETTER KJE
				case 0x9e: return (char) 0x045b;  // CYRILLIC SMALL LETTER TSHE
				case 0x9f: return (char) 0x045f;  // CYRILLIC SMALL LETTER DZHE
				case 0xa0: return (char) 0x00a0;  // NO-BREAK SPACE
				case 0xa1: return (char) 0x040e;  // CYRILLIC CAPITAL LETTER SHORT U
				case 0xa2: return (char) 0x045e;  // CYRILLIC SMALL LETTER SHORT U
				case 0xa3: return (char) 0x0408;  // CYRILLIC CAPITAL LETTER JE
				case 0xa4: return (char) 0x00a4;  // CURRENCY SIGN
				case 0xa5: return (char) 0x0490;  // CYRILLIC CAPITAL LETTER GHE WITH UPTURN
				case 0xa8: return (char) 0x0401;  // CYRILLIC CAPITAL LETTER IO
				case 0xaa: return (char) 0x0404;  // CYRILLIC CAPITAL LETTER UKRAINIAN IE
				case 0xaf: return (char) 0x0407;  // CYRILLIC CAPITAL LETTER YI
				case 0xb2: return (char) 0x0406;  // CYRILLIC CAPITAL LETTER BYELORUSSIAN-UKRAINIAN I
				case 0xb3: return (char) 0x0456;  // CYRILLIC SMALL LETTER BYELORUSSIAN-UKRAINIAN I
				case 0xb4: return (char) 0x0491;  // CYRILLIC SMALL LETTER GHE WITH UPTURN
				case 0xb8: return (char) 0x0451;  // CYRILLIC SMALL LETTER IO
				case 0xb9: return (char) 0x2116;  // NUMERO SIGN
				case 0xba: return (char) 0x0454;  // CYRILLIC SMALL LETTER UKRAINIAN IE
				case 0xbc: return (char) 0x0458;  // CYRILLIC SMALL LETTER JE
				case 0xbd: return (char) 0x0405;  // CYRILLIC CAPITAL LETTER DZE
				case 0xbe: return (char) 0x0455;  // CYRILLIC SMALL LETTER DZE
				case 0xbf: return (char) 0x0457;  // CYRILLIC SMALL LETTER YI
			}

			if ( (P >= 0xc0) | (P <= 0xff) )
				return System.Convert.ToChar( P + 0x0350);
			return (char) P;
		}

		public static char cp1252ToUTF16Char(byte P)
		{
			// Provided by Olaf Lösken. (ported by DrW)
			// Info taken from
			// ftp://ftp.unicode.org/Public/MAPPINGS/VENDORS/MICSFT/WINDOWS/CP1252.TXT
			switch (P)
			{
				case 0x80 : return (char) 0x20AC; //EUROSIGN
				case 0x81 : throw new InvalidOperationException("Invalid Windows-1252 sequence [" + P.ToString() + "]"); 
				case 0x82 : return (char) 0x201A; //SINGLE LOW-9 QUOTATION MARK
				case 0x83 : return (char) 0x0192; //ATIN SMALL LETTER F WITH HOOK
				case 0x84 : return (char) 0x201E; //DOUBLE LOW-9 QUOTATION MARK
				case 0x85 : return (char) 0x2026; //HORIZONTAL ELLIPSIS
				case 0x86 : return (char) 0x2020; //DAGGER
				case 0x87 : return (char) 0x2021; //DOUBLE DAGGER
				case 0x88 : return (char) 0x02C6; //MODIFIER LETTER CIRCUMFLEX ACCENT
				case 0x89 : return (char) 0x2030; //PER MILLE SIGN
				case 0x8A : return (char) 0x0160; //LATIN CAPITAL LETTER S WITH CARON
				case 0x8B : return (char) 0x2039; //SINGLE LEFT-POINTING ANGLE QUOTATION MARK
				case 0x8C : return (char) 0x0152; //LATIN CAPITAL LIGATURE OE
				case 0x8D : throw new InvalidOperationException("Invalid Windows-1252 sequence [" + P.ToString() + "]"); 
				case 0x8E : return (char) 0x017D; //LATIN CAPITAL LETTER Z WITH CARON
				case 0x8F : throw new InvalidOperationException("Invalid Windows-1252 sequence [" + P.ToString() + "]"); 
				case 0x90 : throw new InvalidOperationException("Invalid Windows-1252 sequence [" + P.ToString() + "]"); 
				case 0x91 : return (char) 0x2018; //LEFT SINGLE QUOTATION MARK
				case 0x92 : return (char) 0x2019; //RIGHT SINGLE QUOTATION MARK
				case 0x93 : return (char) 0x201C; //LEFT DOUBLE QUOTATION MARK
				case 0x94 : return (char) 0x201D; //RIGHT DOUBLE QUOTATION MARK
				case 0x95 : return (char) 0x2022; //BULLET
				case 0x96 : return (char) 0x2013; //EN DASH
				case 0x97 : return (char) 0x2014; //EM DASH
				case 0x98 : return (char) 0x02DC; //SMALL TILDE
				case 0x99 : return (char) 0x2122; //TRADE MARK SIGN
				case 0x9A : return (char) 0x0161; //LATIN SMALL LETTER S WITH CARON
				case 0x9B : return (char) 0x203A; //SINGLE RIGHT-POINTING ANGLE QUOTATION MARK
				case 0x9C : return (char) 0x0153; //LATIN SMALL LIGATURE OE
				case 0x9D : throw new InvalidOperationException("Invalid Windows-1252 sequence [" + P.ToString() + "]"); 
				case 0x9E : return (char) 0x017E; //LATIN SMALL LETTER Z WITH CARON
				case 0x9F : return (char) 0x0178; //LATIN CAPITAL LETTER Y WITH D
				default:
					return (char) P;
			}
		}

		/// <summary>
		/// Read in a UTF-8 encoded character.  If no character is on the stream, throws
		/// an ArgumentException.<seealso cref="http://www.ietf.org/rfc/rfc2279.txt"/>
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrownn if 1) called at EOF, 
		/// 2) invalid UTF-8 encoding found.</exception>
		/// <param name="stream">Stream to read from</param>
		/// <returns>Encoded character (could be two characters, upper/lower Surragate pair)</returns>
		public static int ReadUTF8Char(Stream stream)
		{
			byte[] buf = new byte[1];

			if ( stream.Read(buf, 0, 1) != 1)
				throw new InvalidOperationException("Unexptected EOF reading stream");
				
			if (buf[0] >= 0x80)		// UTF-8 sequence
			{
				int numOctets = 1;
				byte first = buf[0];
				int mask = 0x40; 
				int ucs4 = buf[0];

				// first octed must be 110x xxxx to 1111 110x if high order bit set
				if ( (buf[0] & 0xc0) != 0xc0)
					throw new InvalidOperationException("Invalid UTF-8 sequence at position " + stream.Position.ToString());

				// we could mask off the first octet and get the number of octets,
				//	but it's easier to cycle through.  If the bit is set, we have another character to read
				while ( (mask & first) != 0 )
				{
					// read next character of stream
					if (stream.Length == stream.Position)
						throw new InvalidOperationException("Aborted UTF-8 (unexpected EOF) sequence at position " + stream.Position.ToString());
									
					if ( stream.Read(buf, 0, 1) != 1)
						throw new InvalidOperationException("Aborted UTF-8 sequence (missing characters) at position " + stream.Position.ToString());
								
					// all octet sequence bytes start with 10nn nnnn, or they are invalid
					if ( (buf[0] & 0xc0) != 0x80 )
						throw new InvalidOperationException("Invalid UTF-8 sequence at position " + stream.Position.ToString());

					// 6 bits are valid in this item (low order 6)
					//	mask them off and add them
					ucs4 = (ucs4 << 6) | (buf[0] & 0x3F);	// add bits to result
					numOctets++;		
					mask = mask >> 1;	// adjust mask
				}

				// Max 6 octets in sequence
				if ( numOctets > 6)		
					throw new InvalidOperationException("Invalid UTF-8 sequence (no 0-bit in hdr) at position " + stream.Position.ToString());
  
				// UTF-8 can encode up to the following values, per octet size
				int[] MaxCode = {0x7F, 0x7FF, 0xFFFF, 0x1FFFFF, 0x3FFFFFF, 0x7FFFFFFF};

				// mask off the original header bits
				ucs4 = ucs4 & MaxCode[numOctets - 1];	// array is zero-based

				// check for invalid sequence as suggested by RFC2279
				// (check that proper octet sequence size was used to encode character)
				//	(if 0x7F was mapped to a 2-octet sequence, this is an improper coding)
				if ( (numOctets > 1) && (ucs4 <= MaxCode[numOctets -2]))
					throw new InvalidOperationException("Invalid UTF-8 sequence (invalid sequence) at position " + stream.Position.ToString());

				return ucs4;
			}
			else
				// 1-byte value, return it
				return buf[0];
		}
		
		public static char Utf16LowSurrogate(int val)
		{
			int val2 = 0xDC00 ^ (val & 0x03FF);		// 0xdc00 xor (val and 0x03ff)
			return (char) val2;
		}

		public static char Utf16HighSurrogate(int val)
		{
			int value2 = 0xD7C0 + ( val >> 10 );
			return (char) value2;
		}

	}
}