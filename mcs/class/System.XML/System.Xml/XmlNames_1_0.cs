// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// internal System.Xml.XmlNames
//
// Author:
//   Daniel Weber (daniel-weber@austin.rr.com)
// Code ported from Open XML 2.3.17
//
// (C) 2001 Daniel Weber
//

using System;

namespace System.Xml
{
	/// <summary>
	/// Helper class to determine validity of Xml Names
	/// </summary>
	internal class XmlNames_1_0
	{
		// Private data members

		// public properties

		// public methods

		// Static methods
		/// <summary>
		/// Return true if char is a valid Xml character
		/// </summary>
		/// <param name="S">unicode character to check</param>
		/// <returns>true if valid XML character, false otherwise.</returns>
		public static bool IsXmlChar(char S)
		{
			// TODO - verify XmlNames.IsXmlChar(char) ranges correct
			if (S == 0x0009) return true;
			if (S == 0x000A) return true;
			if (S == 0x000D) return true;
			if (S == 0x0020) return true;
			if ( (S >= 0xE000) & (S <= 0xFFFD) ) return true;
			if ( (S >= 0xD800) & (S <=0xDBFF) ) return true;
			if ( (S >= 0xDC00) & (S <= 0xDFFF) ) return true;
			return false;
		}

		/// <summary>
		/// Returns true if char is a valid Xml whitespace character.
		/// S is in unicode.
		/// </summary>
		/// <param name="S"></param>
		/// <returns></returns>
		public static bool IsXmlWhiteSpace( char S)
		{
			// TODO - validate XmlNames.IsXmlWhiteSpace
			if (S == 0x0009) return true;
			if (S == 0x000A) return true;
			if (S == 0x000D) return true;
			if (S == 0x0020) return true;
			return false;
		}

		/// <summary>
		/// Return true if character is a valid Xml base character
		/// </summary>
		/// <param name="S">unicode character to check.</param>
		/// <returns></returns>
		public static bool IsXmlBaseChar(char S)
		{
			long c = System.Convert.ToInt64(S);
			c = c >> 16;

			// Taken directly from Appendix B of the XML 1.0 spec
			// Do a switch on the  high byte to minimize if's on each character.
			switch (c)
			{
				case 0:
					if ( (S >= 0x0041) & (S <= 0x005a) ) return true;	
					if ( (S >= 0x0061) & (S <= 0x007a) ) return true;	
					if ( (S >= 0x00c0) & (S <= 0x00d6) ) return true;	
					if ( (S >= 0x00d8) & (S <= 0x00f6) ) return true;	
					if ( (S >= 0x00f8) & (S <= 0x00ff) ) return true;	
					break;
				case 1:
					if ( (S >= 0x0100) & (S <= 0x0131) ) return true;	
					if ( (S >= 0x0134) & (S <= 0x013E) ) return true;

					if ( (S >= 0x0141) & (S <= 0x0148) ) return true;
					if ( (S >= 0x014a) & (S <= 0x017e) ) return true;
					if ( (S >= 0x0180) & (S <= 0x01c3) ) return true;

					if ( (S >= 0x01cd) & (S <= 0x01f0) ) return true;
					if ( (S >= 0x01f4) & (S <= 0x01f5) ) return true;
					if (S >= 0x01fa)  return true;
					break;
				case 2:
					if ( (S >= 0x01fa) & (S <= 0x0217) ) return true;
					if ( (S >= 0x0250) & (S <= 0x02a8) ) return true;
					if ( (S >= 0x02bb) & (S <= 0x02c1) ) return true;
					break;
				case 3:
					if ( S == 0x0386) return true;
					if ( (S >= 0x0388) & (S <= 0x038a) ) return true;
					if ( S == 0x038c) return true;
					if ( (S >= 0x038e) & (S <= 0x03a1) ) return true;
					if ( (S >= 0x03a3) & (S <= 0x03ce) ) return true;
					if ( (S >= 0x03D0) & (S <= 0x03D6) ) return true;

					if ( S == 0x03DA) return true;
					if ( S == 0x03DC) return true;
					if ( S == 0x03DE) return true;
					if ( S == 0x03E0) return true;
					if ( (S >= 0x03E2) & (S <= 0x03F3) ) return true;
					break;
				case 4:
					if ( (S >= 0x0401) & (S <= 0x040C) ) return true;
					if ( (S >= 0x040E) & (S <= 0x044F) ) return true;

					if ( (S >= 0x0451) & (S <= 0x045C) ) return true;
					if ( (S >= 0x045E) & (S <= 0x0481) ) return true;
					if ( (S >= 0x0490) & (S <= 0x04C4) ) return true;
					if ( (S >= 0x04C7) & (S <= 0x04C8) ) return true;
					if ( (S >= 0x04CB) & (S <= 0x04CC) ) return true;

					if ( (S >= 0x04D0) & (S <= 0x04EB) ) return true;
					if ( (S >= 0x04EE) & (S <= 0x04F5) ) return true;
					if ( (S >= 0x04F8) & (S <= 0x04F9) ) return true;
					break;
				case 5:
					if ( (S >= 0x0531) & (S <= 0x0556) ) return true;
					if ( S == 0x0559) return true;

					if ( (S >= 0x0561) & (S <= 0x0586) ) return true;
					if ( (S >= 0x05D0) & (S <= 0x05EA) ) return true;
					if ( (S >= 0x05F0) & (S <= 0x05F2) ) return true;
					break;
				case 6:
					if ( (S >= 0x0621) & (S <= 0x063A) ) return true;
					if ( (S >= 0x0641) & (S <= 0x064A) ) return true;
					if ( (S >= 0x0671) & (S <= 0x06B7) ) return true;
					if ( (S >= 0x06BA) & (S <= 0x06BE) ) return true;
					if ( (S >= 0x06C0) & (S <= 0x06CE) ) return true;
					if ( (S >= 0x06D0) & (S <= 0x06D3) ) return true;
					if ( S == 0x06D5) return true;
					if ( (S >= 0x06E5) & (S <= 0x06E6) ) return true;
					break;
				case 9:
					if ( (S >= 0x0905) & (S <= 0x0939) ) return true;
					if ( S == 0x093D) return true;
					if ( (S >= 0x0958) & (S <= 0x0961) ) return true;
					if ( (S >= 0x0985) & (S <= 0x098C) ) return true;
					if ( (S >= 0x098F) & (S <= 0x0990) ) return true;
					if ( (S >= 0x0993) & (S <= 0x09A8) ) return true;
					if ( (S >= 0x09AA) & (S <= 0x09B0) ) return true;
					if ( S == 0x09B2) return true;
					if ( (S >= 0x09B6) & (S <= 0x09B9) ) return true;
					if ( (S >= 0x09DC) & (S <= 0x09DD) ) return true;
					if ( (S >= 0x09DF) & (S <= 0x09E1) ) return true;
					if ( (S >= 0x09F0) & (S <= 0x09F1) ) return true;
					break;
				case 10:		// 0x0Ann
					if ( (S >= 0x0A05) & (S <= 0x0A0A) ) return true;
					if ( (S >= 0x0A0F) & (S <= 0x0A10) ) return true;
					if ( (S >= 0x0A13) & (S <= 0x0A28) ) return true;
					if ( (S >= 0x0A2A) & (S <= 0x0A30) ) return true;
					if ( (S >= 0x0A32) & (S <= 0x0A33) ) return true;
					if ( (S >= 0x0A35) & (S <= 0x0A36) ) return true;
					if ( (S >= 0x0A38) & (S <= 0x0A39) ) return true;
					if ( (S >= 0x0A59) & (S <= 0x0A5C) ) return true;
					if ( S == 0x0A5E) return true;
					if ( (S >= 0x0A72) & (S <= 0x0A74) ) return true;
					if ( (S >= 0x0A85) & (S <= 0x0A8B) ) return true;
					if ( S == 0x0A8D) return true;
					if ( (S >= 0x0A8F) & (S <= 0x0A91) ) return true;
					if ( (S >= 0x0A93) & (S <= 0x0AA8) ) return true;
					if ( (S >= 0x0AAA) & (S <= 0x0AB0) ) return true;
					if ( (S >= 0x0AB2) & (S <= 0x0AB3) ) return true;
					if ( (S >= 0x0AB5) & (S <= 0x0AB9) ) return true;
					if ( S == 0x0ABD) return true;
					if ( S == 0x0AE0) return true;
					break;
				case 11:		// 0x0Bnn
					if ( (S >= 0x0B05) & (S <= 0x0B0C) ) return true;
					if ( (S >= 0x0B0F) & (S <= 0x0B10) ) return true;
					if ( (S >= 0x0B13) & (S <= 0x0B28) ) return true;
					if ( (S >= 0x0B2A) & (S <= 0x0B30) ) return true;
					if ( (S >= 0x0B32) & (S <= 0x0B33) ) return true;
					if ( (S >= 0x0B36) & (S <= 0x0B39) ) return true;
					if ( S == 0x0B3D) return true;
					if ( (S >= 0x0B5C) & (S <= 0x0B5D) ) return true;
					if ( (S >= 0x0B5F) & (S <= 0x0B61) ) return true;
					if ( (S >= 0x0B85) & (S <= 0x0B8A) ) return true;
					if ( (S >= 0x0B8E) & (S <= 0x0B90) ) return true;
					if ( (S >= 0x0B92) & (S <= 0x0B95) ) return true;
					if ( (S >= 0x0B99) & (S <= 0x0B9A) ) return true;
					if ( S == 0x0B9C) return true;
					if ( (S >= 0x0B9E) & (S <= 0x0B9F) ) return true;
					if ( (S >= 0x0BA3) & (S <= 0x0BA4) ) return true;
					if ( (S >= 0x0BA8) & (S <= 0x0BAA) ) return true;
					if ( (S >= 0x0BAE) & (S <= 0x0BB5) ) return true;
					if ( (S >= 0x0BB7) & (S <= 0x0BB9) ) return true;
					break;
				case 12:		// 0x0Cnn
					if ( (S >= 0x0C05) & (S <= 0x0C0C) ) return true;
					if ( (S >= 0x0C0E) & (S <= 0x0C10) ) return true;
					if ( (S >= 0x0C12) & (S <= 0x0C28) ) return true;
					if ( (S >= 0x0C2A) & (S <= 0x0C33) ) return true;
					if ( (S >= 0x0C35) & (S <= 0x0C39) ) return true;
					if ( (S >= 0x0C60) & (S <= 0x0C61) ) return true;
					if ( (S >= 0x0C85) & (S <= 0x0C8C) ) return true;
					if ( (S >= 0x0C8E) & (S <= 0x0C90) ) return true;
					if ( (S >= 0x0C92) & (S <= 0x0CA8) ) return true;
					if ( (S >= 0x0CAA) & (S <= 0x0CB3) ) return true;
					if ( (S >= 0x0CB5) & (S <= 0x0CB9) ) return true;
					if ( S == 0x0CDE) return true;
					if ( (S >= 0x0CE0) & (S <= 0x0CE1) ) return true;
					break;
				case 13:		// 0x0Dnn
					if ( (S >= 0x0D05) & (S <= 0x0D0C) ) return true;
					if ( (S >= 0x0D0E) & (S <= 0x0D10) ) return true;
					if ( (S >= 0x0D12) & (S <= 0x0D28) ) return true;
					if ( (S >= 0x0D2A) & (S <= 0x0D39) ) return true;
					if ( (S >= 0x0D60) & (S <= 0x0D61) ) return true;
					break;
				case 14:		// 0x0Enn
					if ( (S >= 0x0E01) & (S <= 0x0E2E) ) return true;
					if ( S == 0x0E30) return true;
					if ( (S >= 0x0E32) & (S <= 0x0E33) ) return true;
					if ( (S >= 0x0E40) & (S <= 0x0E45) ) return true;
					if ( (S >= 0x0E81) & (S <= 0x0E82) ) return true;
					if ( S == 0x0E84) return true;
					if ( (S >= 0x0E87) & (S <= 0x0E88) ) return true;
					if ( S == 0x0E8A) return true;
					if ( S == 0x0E8D) return true;
					if ( (S >= 0x0E94) & (S <= 0x0E97) ) return true;
					if ( (S >= 0x0E99) & (S <= 0x0E9F) ) return true;
					if ( (S >= 0x0EA1) & (S <= 0x0EA3) ) return true;
					if ( S == 0x0EA5) return true;
					if ( S == 0x0EA7) return true;
					if ( (S >= 0x0EAA) & (S <= 0x0EAB) ) return true;
					if ( (S >= 0x0EAD) & (S <= 0x0EAE) ) return true;
					if ( S == 0x0EB0) return true;
					if ( (S >= 0x0EB2) & (S <= 0x0EB3) ) return true;
					if ( S == 0x0EBD) return true;
					if ( (S >= 0x0EC0) & (S <= 0x0EC4) ) return true;
					break;
				case 0x0F:
					if ( (S >= 0x0F40) & (S <= 0x0F47) ) return true;
					if ( (S >= 0x0F49) & (S <= 0x0F69) ) return true;
					break;
				case 0x10:
					if ( (S >= 0x10A0) & (S <= 0x10C5) ) return true;
					if ( (S >= 0x10D0) & (S <= 0x10F6) ) return true;
					break;
				case 0x11:
					if ( S == 0x1100) return true;
					if ( (S >= 0x1102) & (S <= 0x1103) ) return true;
					if ( (S >= 0x1105) & (S <= 0x1107) ) return true;
					if ( S == 0x1109) return true;
					if ( (S >= 0x110B) & (S <= 0x110C) ) return true;
					if ( (S >= 0x110E) & (S <= 0x1112) ) return true;
					if ( S == 0x113C) return true;
					if ( S == 0x113E) return true;
					if ( S == 0x1140) return true;
					if ( S == 0x114C) return true;
					if ( S == 0x114E) return true;
					if ( S == 0x1150) return true;
					if ( (S >= 0x1154) & (S <= 0x1155) ) return true;
					if ( S == 0x1159) return true;
					if ( (S >= 0x115F) & (S <= 0x1161) ) return true;
					if ( S == 0x1163) return true;
					if ( S == 0x1165) return true;
					if ( S == 0x1167) return true;
					if ( S == 0x1169) return true;
					if ( (S >= 0x116D) & (S <= 0x116E) ) return true;
					if ( (S >= 0x1172) & (S <= 0x1173) ) return true;
					if ( S == 0x1175) return true;
					if ( S == 0x119E) return true;
					if ( S == 0x11A8) return true;
					if ( S == 0x11AB) return true;
					if ( (S >= 0x11AE) & (S <= 0x11AF) ) return true;
					if ( (S >= 0x11B7) & (S <= 0x11B8) ) return true;
					if ( S == 0x11BA) return true;
					if ( (S >= 0x11BC) & (S <= 0x11C2) ) return true;
					if ( S == 0x11EB) return true;
					if ( S == 0x11F0) return true;
					if ( S == 0x11F9) return true;
					break;
				case 0x1E:
					if ( (S >= 0x1E00) & (S <= 0x1E9B) ) return true;
					if ( (S >= 0x1EA0) & (S <= 0x1EF9) ) return true;
					break;
				case 0x1F:
					if ( (S >= 0x1F00) & (S <= 0x1F15) ) return true;
					if ( (S >= 0x1F18) & (S <= 0x1F1D) ) return true;
					if ( (S >= 0x1F20) & (S <= 0x1F45) ) return true;
					if ( (S >= 0x1F48) & (S <= 0x1F4D) ) return true;
					if ( (S >= 0x1F50) & (S <= 0x1F57) ) return true;
					if ( S == 0x1F59) return true;
					if ( S == 0x1F5B) return true;
					if ( S == 0x1F5D) return true;
					if ( (S >= 0x1F5F) & (S <= 0x1F7D) ) return true;
					if ( (S >= 0x1F80) & (S <= 0x1FB4) ) return true;
					if ( (S >= 0x1FB6) & (S <= 0x1FBC) ) return true;
					if ( S == 0x1FBE) return true;
					if ( (S >= 0x1FC2) & (S <= 0x1FC4) ) return true;
					if ( (S >= 0x1FC6) & (S <= 0x1FCC) ) return true;
					if ( (S >= 0x1FD0) & (S <= 0x1FD3) ) return true;
					if ( (S >= 0x1FD6) & (S <= 0x1FDB) ) return true;
					if ( (S >= 0x1FE0) & (S <= 0x1FEC) ) return true;
					if ( (S >= 0x1FF2) & (S <= 0x1FF4) ) return true;
					if ( (S >= 0x1FF6) & (S <= 0x1FFC) ) return true;
					break;
				case 33:		// 0x21nn
					if ( S == 0x2126) return true;
					if ( (S >= 0x212A) & (S <= 0x212B) ) return true;
					if ( S == 0x212E) return true;
					if ( (S >= 0x2180) & (S <= 0x2182) ) return true;
					break;
				case 48:		// 0x30nn
					if ( (S >= 0x3041) & (S <= 0x3094) ) return true;
					if ( (S >= 0x30A1) & (S <= 0x30FA) ) return true;
					break;
				case 49:		// 0x31nn
					if ( (S >= 0x3105) & (S <= 0x312C) ) return true;
					break;
				default:
					if ( (S >= 0xAC00) & (S <= 0xd7a3) ) return true;
					break;
			}

			return false;
		}

		/// <summary>
		/// Return true if S is a valid Xml Ideographic
		/// </summary>
		/// <param name="S">unicode character to check.</param>
		/// <returns></returns>
		public static bool IsXmlIdeographic( char S )
		{
			if ( (S >= 0x4E00) & (S <= 0x9FA5) ) return true;
			if ( S == 0x3007) return true;
			if ( (S >= 0x3021) & (S <= 0x3029) ) return true;
			return false;

		}

		/// <summary>
		/// Return true if S is a valid Xml combining character.
		/// </summary>
		/// <param name="S">Unicode character to check</param>
		/// <returns></returns>
		public static bool IsXmlCombiningChar( char S )
		{
			if ( (S >= 0x0300) & (S <= 0x0345) ) return true;
			if ( (S >= 0x0360) & (S <= 0x0361) ) return true;
			if ( (S >= 0x0483) & (S <= 0x0486) ) return true;
			if ( (S >= 0x0591) & (S <= 0x05A1) ) return true;
			if ( (S >= 0x05A3) & (S <= 0x05B9) ) return true;
			if ( (S >= 0x05BB) & (S <= 0x05BD) ) return true;
			if ( S == 0x05BF) return true;
			if ( (S >= 0x05C1) & (S <= 0x05C2) ) return true;
			if ( S == 0x05C4) return true;
			if ( (S >= 0x064B) & (S <= 0x0652) ) return true;
			if ( S == 0x0670) return true;
			if ( (S >= 0x06D6) & (S <= 0x06DC) ) return true;
			if ( (S >= 0x06DD) & (S <= 0x06DF) ) return true;
			if ( (S >= 0x06E0) & (S <= 0x06E4) ) return true;
			if ( (S >= 0x06E7) & (S <= 0x06E8) ) return true;
			if ( (S >= 0x06EA) & (S <= 0x06ED) ) return true;
			if ( (S >= 0x0901) & (S <= 0x0903) ) return true;
			if ( S == 0x093C) return true;
			if ( (S >= 0x093E) & (S <= 0x094C) ) return true;
			if ( S == 0x094D) return true;
			if ( (S >= 0x0951) & (S <= 0x0954) ) return true;
			if ( (S >= 0x0962) & (S <= 0x0963) ) return true;
			if ( (S >= 0x0981) & (S <= 0x0983) ) return true;
			if ( S == 0x09BC) return true;
			if ( S == 0x09BE) return true;
			if ( S == 0x09BF) return true;
			if ( (S >= 0x09C0) & (S <= 0x09C4) ) return true;
			if ( (S >= 0x09C7) & (S <= 0x09C8) ) return true;
			if ( (S >= 0x09CB) & (S <= 0x09CD) ) return true;
			if ( S == 0x09D7) return true;
			if ( (S >= 0x09E2) & (S <= 0x09E3) ) return true;
			if ( S == 0x0A02) return true;
			if ( S == 0x0A3C) return true;
			if ( S == 0x0A3E) return true;
			if ( S == 0x0A3F) return true;
			if ( (S >= 0x0A40) & (S <= 0x0A42) ) return true;
			if ( (S >= 0x0A47) & (S <= 0x0A48) ) return true;
			if ( (S >= 0x0A4B) & (S <= 0x0A4D) ) return true;
			if ( (S >= 0x0A70) & (S <= 0x0A71) ) return true;
			if ( (S >= 0x0A81) & (S <= 0x0A83) ) return true;
			if ( S == 0x0ABC) return true;
			if ( (S >= 0x0ABE) & (S <= 0x0AC5) ) return true;
			if ( (S >= 0x0AC7) & (S <= 0x0AC9) ) return true;
			if ( (S >= 0x0ACB) & (S <= 0x0ACD) ) return true;
			if ( (S >= 0x0B01) & (S <= 0x0B03) ) return true;
			if ( S == 0x0B3C) return true;
			if ( (S >= 0x0B3E) & (S <= 0x0B43) ) return true;
			if ( (S >= 0x0B47) & (S <= 0x0B48) ) return true;
			if ( (S >= 0x0B4B) & (S <= 0x0B4D) ) return true;
			if ( (S >= 0x0B56) & (S <= 0x0B57) ) return true;
			if ( (S >= 0x0B82) & (S <= 0x0B83) ) return true;
			if ( (S >= 0x0BBE) & (S <= 0x0BC2) ) return true;
			if ( (S >= 0x0BC6) & (S <= 0x0BC8) ) return true;
			if ( (S >= 0x0BCA) & (S <= 0x0BCD) ) return true;
			if ( S == 0x0BD7) return true;
			if ( (S >= 0x0C01) & (S <= 0x0C03) ) return true;
			if ( (S >= 0x0C3E) & (S <= 0x0C44) ) return true;
			if ( (S >= 0x0C46) & (S <= 0x0C48) ) return true;
			if ( (S >= 0x0C4A) & (S <= 0x0C4D) ) return true;
			if ( (S >= 0x0C55) & (S <= 0x0C56) ) return true;
			if ( (S >= 0x0C82) & (S <= 0x0C83) ) return true;
			if ( (S >= 0x0CBE) & (S <= 0x0CC4) ) return true;
			if ( (S >= 0x0CC6) & (S <= 0x0CC8) ) return true;
			if ( (S >= 0x0CCA) & (S <= 0x0CCD) ) return true;
			if ( (S >= 0x0CD5) & (S <= 0x0CD6) ) return true;
			if ( (S >= 0x0D02) & (S <= 0x0D03) ) return true;
			if ( (S >= 0x0D3E) & (S <= 0x0D43) ) return true;
			if ( (S >= 0x0D46) & (S <= 0x0D48) ) return true;
			if ( (S >= 0x0D4A) & (S <= 0x0D4D) ) return true;
			if ( S == 0x0D57) return true;
			if ( S == 0x0E31) return true;
			if ( (S >= 0x0E34) & (S <= 0x0E3A) ) return true;
			if ( (S >= 0x0E47) & (S <= 0x0E4E) ) return true;
			if ( S == 0x0EB1) return true;
			if ( (S >= 0x0EB4) & (S <= 0x0EB9) ) return true;
			if ( (S >= 0x0EBB) & (S <= 0x0EBC) ) return true;
			if ( (S >= 0x0EC8) & (S <= 0x0ECD) ) return true;
			if ( (S >= 0x0F18) & (S <= 0x0F19) ) return true;
			if ( S == 0x0F35) return true;
			if ( S == 0x0F37) return true;
			if ( S == 0x0F39) return true;
			if ( S == 0x0F3E) return true;
			if ( S == 0x0F3F) return true;
			if ( (S >= 0x0F71) & (S <= 0x0F84) ) return true;
			if ( (S >= 0x0F86) & (S <= 0x0F8B) ) return true;
			if ( (S >= 0x0F90) & (S <= 0x0F95) ) return true;
			if ( S == 0x0F97) return true;
			if ( (S >= 0x0F99) & (S <= 0x0FAD) ) return true;
			if ( (S >= 0x0FB1) & (S <= 0x0FB7) ) return true;
			if ( S == 0x0FB9) return true;
			if ( (S >= 0x20D0) & (S <= 0x20DC) ) return true;
			if ( S == 0x20E1) return true;
			if ( (S >= 0x302A) & (S <= 0x302F) ) return true;
			if ( S == 0x3099) return true;
			if ( S == 0x309A) return true;

			return false;
		}

		/// <summary>
		/// Return true if S is a valid Xml digit.
		/// </summary>
		/// <param name="S">Unicode character to check.</param>
		/// <returns>true if S is a valid unicode digit.</returns>
		public static bool IsXmlDigit( char S )
		{
			// TODO - validiate IsXmlDigit
			if ( (S >= 0x0030) & (S <= 0x0039) ) return true;
			if ( (S >= 0x0660) & (S <= 0x0669) ) return true;
			if ( (S >= 0x06F0) & (S <= 0x06F9) ) return true;
			if ( (S >= 0x0966) & (S <= 0x096F) ) return true;
			if ( (S >= 0x09E6) & (S <= 0x09EF) ) return true;
			if ( (S >= 0x0A66) & (S <= 0x0A6F) ) return true;
			if ( (S >= 0x0AE6) & (S <= 0x0AEF) ) return true;
			if ( (S >= 0x0B66) & (S <= 0x0B6F) ) return true;
			if ( (S >= 0x0BE7) & (S <= 0x0BEF) ) return true;
			if ( (S >= 0x0C66) & (S <= 0x0C6F) ) return true;
			if ( (S >= 0x0CE6) & (S <= 0x0CEF) ) return true;
			if ( (S >= 0x0D66) & (S <= 0x0D6F) ) return true;
			if ( (S >= 0x0E50) & (S <= 0x0E59) ) return true;
			if ( (S >= 0x0ED0) & (S <= 0x0ED9) ) return true;
			if ( (S >= 0x0F20) & (S <= 0x0F29) ) return true;

			return false;
		}

		/// <summary>
		/// Return true if S is a valid Xml Extender.
		/// </summary>
		/// <param name="S">unicode character to check.</param>
		/// <returns>true if S is a valid Xml Extender.</returns>
		public static bool IsXmlExtender ( char S )
		{
			if ( S == 0x00B7) return true;
			if ( S == 0x02D0) return true;
			if ( S == 0x02D1) return true;
			if ( S == 0x0387) return true;
			if ( S == 0x0640) return true;
			if ( S == 0x0E46) return true;
			if ( S == 0x0EC6) return true;
			if ( S == 0x3005) return true;
			if ( (S >= 0x3031) & ( S <= 0x3035) ) return true;
			if ( (S >= 0x309D) & ( S <= 0x309E) ) return true;
			if ( (S >= 0x30FC) & ( S <= 0x30FE) ) return true;

			return false;
		}

		public static bool IsXmlLetter(char S)
		{
			return IsXmlIdeographic(S) | IsXmlBaseChar(S);
		}

		/// <summary>
		/// Return true if S is a valid Xml name char.
		/// </summary>
		/// <param name="S">Unicode character to check</param>
		/// <returns>true if S is valid in an xml name.</returns>
		public static bool IsXmlNameChar ( char S )
		{
			if ( IsXmlLetter(S) | IsXmlDigit(S) | IsXmlCombiningChar(S) | IsXmlExtender(S) )
				return true;
			if ( (S == '.') | (S == '-') | (S == '_') | (S == ':') )
				return true;
			return false;
		}

		/// <summary>
		/// Return true if the passed string is a valid Xml 1.0 name
		/// </summary>
		/// <param name="s">String to check for validity</param>
		/// <returns>true if string is valid Xml name</returns>
		public static bool isXmlName( string s )
		{
			if ( s.Length == 0 ) return false;

			if ( !IsXmlLetter(s[0]) | (s[0] != '_') | (s[0] != ':') )
				return false;

			for (int i = 1; i < s.Length; i++)
			{
				if (! IsXmlNameChar( s[i] ) )
					return false;
			}

			return true;
		}


		// Constructors

	}
}
