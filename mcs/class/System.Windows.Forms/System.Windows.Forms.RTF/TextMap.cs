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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// This map is for convencience only, any app can create/use it's own
// StdCharCode -> <text> table

using System.Collections;

namespace System.Windows.Forms.RTF {

#if RTF_LIB
	public
#else
	internal
#endif
	class TextMap {
		#region Local Variables
		private string[]		table;
		#endregion	// Local Variables

		#region Public Constructors
		public TextMap() {
			table = new string[(int)StandardCharCode.MaxChar];

			for (int i = 0; i < (int)StandardCharCode.MaxChar; i++) {
				table[i] = string.Empty;
			}
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		internal string this[StandardCharCode c] {	// FIXME - this should be public, if the whole namespace was public (ie standalone RTF parser)
			get {
				return table[(int)c];
			}

			set {
				table[(int)c] = value;
			}
		}

		public string[] Table {
			get {
				return table;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Static Methods
		public static void SetupStandardTable(string[] table)
		{
			/*
			table[(int)StandardCharCode.space] = " ";
			table[(int)StandardCharCode.exclam] = "!";
			table[(int)StandardCharCode.quotedbl] = "\"";
			table[(int)StandardCharCode.numbersign] = "#";
			table[(int)StandardCharCode.dollar] = "$";
			table[(int)StandardCharCode.percent] = "%";
			table[(int)StandardCharCode.ampersand] = "&";
			table[(int)StandardCharCode.quoteright] = "'";
			table[(int)StandardCharCode.parenleft] = "(";
			table[(int)StandardCharCode.parenright] = ")";
			table[(int)StandardCharCode.asterisk] = "*";
			table[(int)StandardCharCode.plus] = "+";
			table[(int)StandardCharCode.comma] = ",";
			table[(int)StandardCharCode.hyphen] = "-";
			table[(int)StandardCharCode.period] = ".";
			table[(int)StandardCharCode.slash] = "/";
			table[(int)StandardCharCode.zero] = "0";
			table[(int)StandardCharCode.one] = "1";
			table[(int)StandardCharCode.two] = "2";
			table[(int)StandardCharCode.three] = "3";
			table[(int)StandardCharCode.four] = "4";
			table[(int)StandardCharCode.five] = "5";
			table[(int)StandardCharCode.six] = "6";
			table[(int)StandardCharCode.seven] = "7";
			table[(int)StandardCharCode.eight] = "8";
			table[(int)StandardCharCode.nine] = "9";
			table[(int)StandardCharCode.colon] = ":";
			table[(int)StandardCharCode.semicolon] = ";";
			table[(int)StandardCharCode.less] = "<";
			table[(int)StandardCharCode.equal] = "=";
			table[(int)StandardCharCode.greater] = ">";
			table[(int)StandardCharCode.question] = "?";
			table[(int)StandardCharCode.at] = "@";
			table[(int)StandardCharCode.A] = "A";
			table[(int)StandardCharCode.B] = "B";
			table[(int)StandardCharCode.C] = "C";
			table[(int)StandardCharCode.D] = "D";
			table[(int)StandardCharCode.E] = "E";
			table[(int)StandardCharCode.F] = "F";
			table[(int)StandardCharCode.G] = "G";
			table[(int)StandardCharCode.H] = "H";
			table[(int)StandardCharCode.I] = "I";
			table[(int)StandardCharCode.J] = "J";
			table[(int)StandardCharCode.K] = "K";
			table[(int)StandardCharCode.L] = "L";
			table[(int)StandardCharCode.M] = "M";
			table[(int)StandardCharCode.N] = "N";
			table[(int)StandardCharCode.O] = "O";
			table[(int)StandardCharCode.P] = "P";
			table[(int)StandardCharCode.Q] = "Q";
			table[(int)StandardCharCode.R] = "R";
			table[(int)StandardCharCode.S] = "S";
			table[(int)StandardCharCode.T] = "T";
			table[(int)StandardCharCode.U] = "U";
			table[(int)StandardCharCode.V] = "V";
			table[(int)StandardCharCode.W] = "W";
			table[(int)StandardCharCode.X] = "X";
			table[(int)StandardCharCode.Y] = "Y";
			table[(int)StandardCharCode.Z] = "Z";
			table[(int)StandardCharCode.bracketleft] = "[";
			table[(int)StandardCharCode.backslash] = "\\";
			table[(int)StandardCharCode.bracketright] = "]";
			table[(int)StandardCharCode.asciicircum] = "^";
			table[(int)StandardCharCode.underscore] = "_";
			table[(int)StandardCharCode.quoteleft] = "`";
			table[(int)StandardCharCode.a] = "a";
			table[(int)StandardCharCode.b] = "b";
			table[(int)StandardCharCode.c] = "c";
			table[(int)StandardCharCode.d] = "d";
			table[(int)StandardCharCode.e] = "e";
			table[(int)StandardCharCode.f] = "f";
			table[(int)StandardCharCode.g] = "g";
			table[(int)StandardCharCode.h] = "h";
			table[(int)StandardCharCode.i] = "i";
			table[(int)StandardCharCode.j] = "j";
			table[(int)StandardCharCode.k] = "k";
			table[(int)StandardCharCode.l] = "l";
			table[(int)StandardCharCode.m] = "m";
			table[(int)StandardCharCode.n] = "n";
			table[(int)StandardCharCode.o] = "o";
			table[(int)StandardCharCode.p] = "p";
			table[(int)StandardCharCode.q] = "q";
			table[(int)StandardCharCode.r] = "r";
			table[(int)StandardCharCode.s] = "s";
			table[(int)StandardCharCode.t] = "t";
			table[(int)StandardCharCode.u] = "u";
			table[(int)StandardCharCode.v] = "v";
			table[(int)StandardCharCode.w] = "w";
			table[(int)StandardCharCode.x] = "x";
			table[(int)StandardCharCode.y] = "y";
			table[(int)StandardCharCode.z] = "z";
			table[(int)StandardCharCode.braceleft] = "{";
			table[(int)StandardCharCode.bar] = "|";
			table[(int)StandardCharCode.braceright] = "}";
			table[(int)StandardCharCode.asciitilde] = "~";
			table[(int)StandardCharCode.AE] = "AE";
			table[(int)StandardCharCode.OE] = "OE";
			table[(int)StandardCharCode.acute] = "'";
			table[(int)StandardCharCode.ae] = "ae";
			table[(int)StandardCharCode.angleleft] = "<";
			table[(int)StandardCharCode.angleright] = ">";
			table[(int)StandardCharCode.arrowboth] = "<->";
			table[(int)StandardCharCode.arrowdblboth] = "<=>";
			table[(int)StandardCharCode.arrowdblleft] = "<=";
			table[(int)StandardCharCode.arrowdblright] = "=>";
			table[(int)StandardCharCode.arrowleft] = "<-";
			table[(int)StandardCharCode.arrowright] = "->";
			table[(int)StandardCharCode.bullet] = "o";
			table[(int)StandardCharCode.cent] = "cent";
			table[(int)StandardCharCode.circumflex] = "^";
			table[(int)StandardCharCode.copyright] = "(c)";
			table[(int)StandardCharCode.copyrightsans] = "(c)";
			table[(int)StandardCharCode.degree] = "deg.";
			table[(int)StandardCharCode.divide] = "/";
			table[(int)StandardCharCode.dotlessi] = "i";
			table[(int)StandardCharCode.ellipsis] = "...";
			table[(int)StandardCharCode.emdash] = "--";
			table[(int)StandardCharCode.endash] = "-";
			table[(int)StandardCharCode.fi] = "fi";
			table[(int)StandardCharCode.fl] = "fl";
			table[(int)StandardCharCode.fraction] = "/";
			table[(int)StandardCharCode.germandbls] = "ss";
			table[(int)StandardCharCode.grave] = "`";
			table[(int)StandardCharCode.greaterequal] = ">=";
			table[(int)StandardCharCode.guillemotleft] = "<<";
			table[(int)StandardCharCode.guillemotright] = ">>";
			table[(int)StandardCharCode.guilsinglleft] = "<";
			table[(int)StandardCharCode.guilsinglright] = ">";
			table[(int)StandardCharCode.lessequal] = "<=";
			table[(int)StandardCharCode.logicalnot] = "~";
			table[(int)StandardCharCode.mathasterisk] = "*";
			table[(int)StandardCharCode.mathequal] = "=";
			table[(int)StandardCharCode.mathminus] = "-";
			table[(int)StandardCharCode.mathnumbersign] = "#";
			table[(int)StandardCharCode.mathplus] = "+";
			table[(int)StandardCharCode.mathtilde] = "~";
			table[(int)StandardCharCode.minus] = "-";
			table[(int)StandardCharCode.mu] = "u";
			table[(int)StandardCharCode.multiply] = "x";
			table[(int)StandardCharCode.nobrkhyphen] = "-";
			table[(int)StandardCharCode.nobrkspace] = "";
			table[(int)StandardCharCode.notequal] = "!=";
			table[(int)StandardCharCode.oe] = "oe";
			table[(int)StandardCharCode.onehalf] = "1/2";
			table[(int)StandardCharCode.onequarter] = "1/4";
			table[(int)StandardCharCode.periodcentered] = ".";
			table[(int)StandardCharCode.plusminus] = "+/-";
			table[(int)StandardCharCode.quotedblbase] = ",,";
			table[(int)StandardCharCode.quotedblleft] = "\"";
			table[(int)StandardCharCode.quotedblright] = "\"";
			table[(int)StandardCharCode.quotesinglbase] = ",";
			table[(int)StandardCharCode.registered] = "reg.";
			table[(int)StandardCharCode.registersans] = "reg.";
			table[(int)StandardCharCode.threequarters] = "3/4";
			table[(int)StandardCharCode.tilde] = "~";
			table[(int)StandardCharCode.trademark] = "(TM)";
			table[(int)StandardCharCode.trademarksans] = "(TM)";

			table[(int)StandardCharCode.aacute] = "\xE0";
			table[(int)StandardCharCode.questiondown] = "\xBF";

			table[(int)StandardCharCode.udieresis] = "\xFC";
			table[(int)StandardCharCode.Udieresis] = "\xDC";
			table[(int)StandardCharCode.odieresis] = "\xF6";
			table[(int)StandardCharCode.Odieresis] = "\xD6";
			*/
		
			table [(int) StandardCharCode.formula] = "\x6";
			table [(int) StandardCharCode.nobrkhyphen] = "\x1e";
			table [(int) StandardCharCode.opthyphen] = "\x1f";
			table [(int) StandardCharCode.space] = " ";
			table [(int) StandardCharCode.exclam] = "!";
			table [(int) StandardCharCode.quotedbl] = "\"";
			table [(int) StandardCharCode.numbersign] = "#";
			table [(int) StandardCharCode.dollar] = "$";
			table [(int) StandardCharCode.percent] = "%";
			table [(int) StandardCharCode.ampersand] = "&";
			table [(int) StandardCharCode.parenleft] = "(";
			table [(int) StandardCharCode.parenright] = ")";
			table [(int) StandardCharCode.asterisk] = "*";
			table [(int) StandardCharCode.plus] = "+";
			table [(int) StandardCharCode.comma] = ",";
			table [(int) StandardCharCode.hyphen] = "-";
			table [(int) StandardCharCode.period] = ".";
			table [(int) StandardCharCode.slash] = "/";
			table [(int) StandardCharCode.zero] = "0";
			table [(int) StandardCharCode.one] = "1";
			table [(int) StandardCharCode.two] = "2";
			table [(int) StandardCharCode.three] = "3";
			table [(int) StandardCharCode.four] = "4";
			table [(int) StandardCharCode.five] = "5";
			table [(int) StandardCharCode.six] = "6";
			table [(int) StandardCharCode.seven] = "7";
			table [(int) StandardCharCode.eight] = "8";
			table [(int) StandardCharCode.nine] = "9";
			table [(int) StandardCharCode.colon] = ":";
			table [(int) StandardCharCode.semicolon] = ";";
			table [(int) StandardCharCode.less] = "<";
			table [(int) StandardCharCode.equal] = "=";
			table [(int) StandardCharCode.greater] = ">";
			table [(int) StandardCharCode.question] = "?";
			table [(int) StandardCharCode.at] = "@";
			table [(int) StandardCharCode.A] = "A";
			table [(int) StandardCharCode.B] = "B";
			table [(int) StandardCharCode.C] = "C";
			table [(int) StandardCharCode.D] = "D";
			table [(int) StandardCharCode.E] = "E";
			table [(int) StandardCharCode.F] = "F";
			table [(int) StandardCharCode.G] = "G";
			table [(int) StandardCharCode.H] = "H";
			table [(int) StandardCharCode.I] = "I";
			table [(int) StandardCharCode.J] = "J";
			table [(int) StandardCharCode.K] = "K";
			table [(int) StandardCharCode.L] = "L";
			table [(int) StandardCharCode.M] = "M";
			table [(int) StandardCharCode.N] = "N";
			table [(int) StandardCharCode.O] = "O";
			table [(int) StandardCharCode.P] = "P";
			table [(int) StandardCharCode.Q] = "Q";
			table [(int) StandardCharCode.R] = "R";
			table [(int) StandardCharCode.S] = "S";
			table [(int) StandardCharCode.T] = "T";
			table [(int) StandardCharCode.U] = "U";
			table [(int) StandardCharCode.V] = "V";
			table [(int) StandardCharCode.W] = "W";
			table [(int) StandardCharCode.X] = "X";
			table [(int) StandardCharCode.Y] = "Y";
			table [(int) StandardCharCode.Z] = "Z";
			table [(int) StandardCharCode.bracketleft] = "[";
			table [(int) StandardCharCode.backslash] = "\\";
			table [(int) StandardCharCode.bracketright] = "]";
			table [(int) StandardCharCode.asciicircum] = "^";
			table [(int) StandardCharCode.underscore] = "_";
			table [(int) StandardCharCode.quoteleft] = "`";
			table [(int) StandardCharCode.a] = "a";
			table [(int) StandardCharCode.b] = "b";
			table [(int) StandardCharCode.c] = "c";
			table [(int) StandardCharCode.d] = "d";
			table [(int) StandardCharCode.e] = "e";
			table [(int) StandardCharCode.f] = "f";
			table [(int) StandardCharCode.g] = "g";
			table [(int) StandardCharCode.h] = "h";
			table [(int) StandardCharCode.i] = "i";
			table [(int) StandardCharCode.j] = "j";
			table [(int) StandardCharCode.k] = "k";
			table [(int) StandardCharCode.l] = "l";
			table [(int) StandardCharCode.m] = "m";
			table [(int) StandardCharCode.n] = "n";
			table [(int) StandardCharCode.o] = "o";
			table [(int) StandardCharCode.p] = "p";
			table [(int) StandardCharCode.q] = "q";
			table [(int) StandardCharCode.r] = "r";
			table [(int) StandardCharCode.s] = "s";
			table [(int) StandardCharCode.t] = "t";
			table [(int) StandardCharCode.u] = "u";
			table [(int) StandardCharCode.v] = "v";
			table [(int) StandardCharCode.w] = "w";
			table [(int) StandardCharCode.x] = "x";
			table [(int) StandardCharCode.y] = "y";
			table [(int) StandardCharCode.z] = "z";
			table [(int) StandardCharCode.braceleft] = "{";
			table [(int) StandardCharCode.bar] = "|";
			table [(int) StandardCharCode.braceright] = "}";
			table [(int) StandardCharCode.asciitilde] = "~";
			table [(int) StandardCharCode.nobrkspace] = "\xa0";
			table [(int) StandardCharCode.exclamdown] = "\xa1";
			table [(int) StandardCharCode.cent] = "\xa2";
			table [(int) StandardCharCode.sterling] = "\xa3";
			table [(int) StandardCharCode.currency] = "\xa4";
			table [(int) StandardCharCode.yen] = "\xa5";
			table [(int) StandardCharCode.brokenbar] = "\xa6";
			table [(int) StandardCharCode.section] = "\xa7";
			table [(int) StandardCharCode.dieresis] = "\xa8";
			table [(int) StandardCharCode.copyright] = "\xa9";
			table [(int) StandardCharCode.ordfeminine] = "\xaa";
			table [(int) StandardCharCode.guillemotleft] = "\xab";
			table [(int) StandardCharCode.logicalnot] = "\xac";
			table [(int) StandardCharCode.opthyphen] = "\xad";
			table [(int) StandardCharCode.registered] = "\xae";
			table [(int) StandardCharCode.macron] = "\xaf";
			table [(int) StandardCharCode.degree] = "\xb0";
			table [(int) StandardCharCode.plusminus] = "\xb1";
			table [(int) StandardCharCode.twosuperior] = "\xb2";
			table [(int) StandardCharCode.threesuperior] = "\xb3";
			table [(int) StandardCharCode.acute] = "\xb4";
			table [(int) StandardCharCode.mu] = "\xb5";
			table [(int) StandardCharCode.paragraph] = "\xb6";
			table [(int) StandardCharCode.periodcentered] = "\xb7";
			table [(int) StandardCharCode.cedilla] = "\xb8";
			table [(int) StandardCharCode.onesuperior] = "\xb9";
			table [(int) StandardCharCode.ordmasculine] = "\xba";
			table [(int) StandardCharCode.guillemotright] = "\xbb";
			table [(int) StandardCharCode.onequarter] = "\xbc";
			table [(int) StandardCharCode.onehalf] = "\xbd";
			table [(int) StandardCharCode.threequarters] = "\xbe";
			table [(int) StandardCharCode.questiondown] = "\xbf";
			table [(int) StandardCharCode.Agrave] = "\xc0";
			table [(int) StandardCharCode.Aacute] = "\xc1";
			table [(int) StandardCharCode.Acircumflex] = "\xc2";
			table [(int) StandardCharCode.Atilde] = "\xc3";
			table [(int) StandardCharCode.Adieresis] = "\xc4";
			table [(int) StandardCharCode.Aring] = "\xc5";
			table [(int) StandardCharCode.AE] = "\xc6";
			table [(int) StandardCharCode.Ccedilla] = "\xc7";
			table [(int) StandardCharCode.Egrave] = "\xc8";
			table [(int) StandardCharCode.Eacute] = "\xc9";
			table [(int) StandardCharCode.Ecircumflex] = "\xca";
			table [(int) StandardCharCode.Edieresis] = "\xcb";
			table [(int) StandardCharCode.Igrave] = "\xcc";
			table [(int) StandardCharCode.Iacute] = "\xcd";
			table [(int) StandardCharCode.Icircumflex] = "\xce";
			table [(int) StandardCharCode.Idieresis] = "\xcf";
			table [(int) StandardCharCode.Eth] = "\xd0";
			table [(int) StandardCharCode.Ntilde] = "\xd1";
			table [(int) StandardCharCode.Ograve] = "\xd2";
			table [(int) StandardCharCode.Oacute] = "\xd3";
			table [(int) StandardCharCode.Ocircumflex] = "\xd4";
			table [(int) StandardCharCode.Otilde] = "\xd5";
			table [(int) StandardCharCode.Odieresis] = "\xd6";
			table [(int) StandardCharCode.multiply] = "\xd7";
			table [(int) StandardCharCode.Oslash] = "\xd8";
			table [(int) StandardCharCode.Ugrave] = "\xd9";
			table [(int) StandardCharCode.Uacute] = "\xda";
			table [(int) StandardCharCode.Ucircumflex] = "\xdb";
			table [(int) StandardCharCode.Udieresis] = "\xdc";
			table [(int) StandardCharCode.Yacute] = "\xdd";
			table [(int) StandardCharCode.Thorn] = "\xde";
			table [(int) StandardCharCode.germandbls] = "\xdf";
			table [(int) StandardCharCode.agrave] = "\xe0";
			table [(int) StandardCharCode.aacute] = "\xe1";
			table [(int) StandardCharCode.acircumflex] = "\xe2";
			table [(int) StandardCharCode.atilde] = "\xe3";
			table [(int) StandardCharCode.adieresis] = "\xe4";
			table [(int) StandardCharCode.aring] = "\xe5";
			table [(int) StandardCharCode.ae] = "\xe6";
			table [(int) StandardCharCode.ccedilla] = "\xe7";
			table [(int) StandardCharCode.egrave] = "\xe8";
			table [(int) StandardCharCode.eacute] = "\xe9";
			table [(int) StandardCharCode.ecircumflex] = "\xea";
			table [(int) StandardCharCode.edieresis] = "\xeb";
			table [(int) StandardCharCode.igrave] = "\xec";
			table [(int) StandardCharCode.iacute] = "\xed";
			table [(int) StandardCharCode.icircumflex] = "\xee";
			table [(int) StandardCharCode.idieresis] = "\xef";
			table [(int) StandardCharCode.eth] = "\xf0";
			table [(int) StandardCharCode.ntilde] = "\xf1";
			table [(int) StandardCharCode.ograve] = "\xf2";
			table [(int) StandardCharCode.oacute] = "\xf3";
			table [(int) StandardCharCode.ocircumflex] = "\xf4";
			table [(int) StandardCharCode.otilde] = "\xf5";
			table [(int) StandardCharCode.odieresis] = "\xf6";
			table [(int) StandardCharCode.divide] = "\xf7";
			table [(int) StandardCharCode.oslash] = "\xf8";
			table [(int) StandardCharCode.ugrave] = "\xf9";
			table [(int) StandardCharCode.uacute] = "\xfa";
			table [(int) StandardCharCode.ucircumflex] = "\xfb";
			table [(int) StandardCharCode.udieresis] = "\xfc";
			table [(int) StandardCharCode.yacute] = "\xfd";
			table [(int) StandardCharCode.thorn] = "\xfe";
			table [(int) StandardCharCode.ydieresis] = "\xff";

		}
		#endregion	// Public Static Methods
	}
}
