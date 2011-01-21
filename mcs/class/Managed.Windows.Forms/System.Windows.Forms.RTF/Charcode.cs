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

// COMPLETE

using System.Collections;

namespace System.Windows.Forms.RTF {
	internal class Charcode {
		#region Local Variables
		private StandardCharCode[]	codes;
		private Hashtable		reverse;
		private int			size;
		#endregion	// Local Variables
		
		#region Cached Values
		static Charcode ansi_generic;
		#endregion

		#region Public Constructors
		public Charcode() : this(256) {
		}

		private Charcode(int size) {
			this.size = size;
			this.codes = new StandardCharCode[size];
			this.reverse = new Hashtable(size);

			// No need to reinitialize array to its default value
			//for (int i = 0; i < size; i++) {
			//	codes[i] = StandardCharCode.nothing;
			//}
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public int this[StandardCharCode c] {
			get {
				object obj;

				obj = reverse[c];
				if (obj != null) {
					return (int)obj;
				}
				for (int i = 0; i < size; i++) {
					if (codes[i] == c) {
						return i;
					}
				}

				return -1;
			}
		}

		public StandardCharCode this[int c] {
			get {
				if (c < 0 || c >= size) {
					return StandardCharCode.nothing;
				}

				return codes[c];
			}

			private set {
				if (c < 0 || c >= size) {
					return;
				}

				codes[c] = value;
				reverse[value] = c;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		#endregion	// Public Instance Methods

		#region Public Static Methods
		public static Charcode AnsiGeneric {
			get {
				if (ansi_generic != null)
					return ansi_generic;
					
				ansi_generic = new Charcode(256);

				ansi_generic[0x06] = StandardCharCode.formula;
				ansi_generic[0x1e] =  StandardCharCode.nobrkhyphen;
				ansi_generic[0x1f] = StandardCharCode.opthyphen;
				ansi_generic[' '] = StandardCharCode.space;
				ansi_generic['!'] = StandardCharCode.exclam;
				ansi_generic['"'] = StandardCharCode.quotedbl;
				ansi_generic['#'] = StandardCharCode.numbersign;
				ansi_generic['$'] = StandardCharCode.dollar;
				ansi_generic['%'] = StandardCharCode.percent;
				ansi_generic['&'] = StandardCharCode.ampersand;
				ansi_generic['\\'] = StandardCharCode.quoteright;
				ansi_generic['('] = StandardCharCode.parenleft;
				ansi_generic[')'] = StandardCharCode.parenright;
				ansi_generic['*'] = StandardCharCode.asterisk;
				ansi_generic['+'] = StandardCharCode.plus;
				ansi_generic[','] = StandardCharCode.comma;
				ansi_generic['-'] = StandardCharCode.hyphen;
				ansi_generic['.'] = StandardCharCode.period;
				ansi_generic['/'] = StandardCharCode.slash;
				ansi_generic['0'] = StandardCharCode.zero;
				ansi_generic['1'] = StandardCharCode.one;
				ansi_generic['2'] = StandardCharCode.two;
				ansi_generic['3'] = StandardCharCode.three;
				ansi_generic['4'] = StandardCharCode.four;
				ansi_generic['5'] = StandardCharCode.five;
				ansi_generic['6'] = StandardCharCode.six;
				ansi_generic['7'] = StandardCharCode.seven;
				ansi_generic['8'] = StandardCharCode.eight;
				ansi_generic['9'] = StandardCharCode.nine;
				ansi_generic[':'] = StandardCharCode.colon;
				ansi_generic[';'] = StandardCharCode.semicolon;
				ansi_generic['<'] = StandardCharCode.less;
				ansi_generic['='] = StandardCharCode.equal;
				ansi_generic['>'] = StandardCharCode.greater;
				ansi_generic['?'] = StandardCharCode.question;
				ansi_generic['@'] = StandardCharCode.at;
				ansi_generic['A'] = StandardCharCode.A;
				ansi_generic['B'] = StandardCharCode.B;
				ansi_generic['C'] = StandardCharCode.C;
				ansi_generic['D'] = StandardCharCode.D;
				ansi_generic['E'] = StandardCharCode.E;
				ansi_generic['F'] = StandardCharCode.F;
				ansi_generic['G'] = StandardCharCode.G;
				ansi_generic['H'] = StandardCharCode.H;
				ansi_generic['I'] = StandardCharCode.I;
				ansi_generic['J'] = StandardCharCode.J;
				ansi_generic['K'] = StandardCharCode.K;
				ansi_generic['L'] = StandardCharCode.L;
				ansi_generic['M'] = StandardCharCode.M;
				ansi_generic['N'] = StandardCharCode.N;
				ansi_generic['O'] = StandardCharCode.O;
				ansi_generic['P'] = StandardCharCode.P;
				ansi_generic['Q'] = StandardCharCode.Q;
				ansi_generic['R'] = StandardCharCode.R;
				ansi_generic['S'] = StandardCharCode.S;
				ansi_generic['T'] = StandardCharCode.T;
				ansi_generic['U'] = StandardCharCode.U;
				ansi_generic['V'] = StandardCharCode.V;
				ansi_generic['W'] = StandardCharCode.W;
				ansi_generic['X'] = StandardCharCode.X;
				ansi_generic['Y'] = StandardCharCode.Y;
				ansi_generic['Z'] = StandardCharCode.Z;
				ansi_generic['['] = StandardCharCode.bracketleft;
				ansi_generic['\\'] = StandardCharCode.backslash;
				ansi_generic[']'] = StandardCharCode.bracketright;
				ansi_generic['^'] = StandardCharCode.asciicircum;
				ansi_generic['_'] = StandardCharCode.underscore;
				ansi_generic['`'] = StandardCharCode.quoteleft;
				ansi_generic['a'] = StandardCharCode.a;
				ansi_generic['b'] = StandardCharCode.b;
				ansi_generic['c'] = StandardCharCode.c;
				ansi_generic['d'] = StandardCharCode.d;
				ansi_generic['e'] = StandardCharCode.e;
				ansi_generic['f'] = StandardCharCode.f;
				ansi_generic['g'] = StandardCharCode.g;
				ansi_generic['h'] = StandardCharCode.h;
				ansi_generic['i'] = StandardCharCode.i;
				ansi_generic['j'] = StandardCharCode.j;
				ansi_generic['k'] = StandardCharCode.k;
				ansi_generic['l'] = StandardCharCode.l;
				ansi_generic['m'] = StandardCharCode.m;
				ansi_generic['n'] = StandardCharCode.n;
				ansi_generic['o'] = StandardCharCode.o;
				ansi_generic['p'] = StandardCharCode.p;
				ansi_generic['q'] = StandardCharCode.q;
				ansi_generic['r'] = StandardCharCode.r;
				ansi_generic['s'] = StandardCharCode.s;
				ansi_generic['t'] = StandardCharCode.t;
				ansi_generic['u'] = StandardCharCode.u;
				ansi_generic['v'] = StandardCharCode.v;
				ansi_generic['w'] = StandardCharCode.w;
				ansi_generic['x'] = StandardCharCode.x;
				ansi_generic['y'] = StandardCharCode.y;
				ansi_generic['z'] = StandardCharCode.z;
				ansi_generic['{'] = StandardCharCode.braceleft;
				ansi_generic['|'] = StandardCharCode.bar;
				ansi_generic['}'] = StandardCharCode.braceright;
				ansi_generic['~'] = StandardCharCode.asciitilde;
				ansi_generic[0xa0] = StandardCharCode.nobrkspace;
				ansi_generic[0xa1] = StandardCharCode.exclamdown;
				ansi_generic[0xa2] = StandardCharCode.cent;
				ansi_generic[0xa3] = StandardCharCode.sterling;
				ansi_generic[0xa4] = StandardCharCode.currency;
				ansi_generic[0xa5] = StandardCharCode.yen;
				ansi_generic[0xa6] = StandardCharCode.brokenbar;
				ansi_generic[0xa7] = StandardCharCode.section;
				ansi_generic[0xa8] = StandardCharCode.dieresis;
				ansi_generic[0xa9] = StandardCharCode.copyright;
				ansi_generic[0xaa] = StandardCharCode.ordfeminine;
				ansi_generic[0xab] = StandardCharCode.guillemotleft;
				ansi_generic[0xac] = StandardCharCode.logicalnot;
				ansi_generic[0xad] = StandardCharCode.opthyphen;
				ansi_generic[0xae] = StandardCharCode.registered;
				ansi_generic[0xaf] = StandardCharCode.macron;
				ansi_generic[0xb0] = StandardCharCode.degree;
				ansi_generic[0xb1] = StandardCharCode.plusminus;
				ansi_generic[0xb2] = StandardCharCode.twosuperior;
				ansi_generic[0xb3] = StandardCharCode.threesuperior;
				ansi_generic[0xb4] = StandardCharCode.acute;
				ansi_generic[0xb5] = StandardCharCode.mu;
				ansi_generic[0xb6] = StandardCharCode.paragraph;
				ansi_generic[0xb7] = StandardCharCode.periodcentered;
				ansi_generic[0xb8] = StandardCharCode.cedilla;
				ansi_generic[0xb9] = StandardCharCode.onesuperior;
				ansi_generic[0xba] = StandardCharCode.ordmasculine;
				ansi_generic[0xbb] = StandardCharCode.guillemotright;
				ansi_generic[0xbc] = StandardCharCode.onequarter;
				ansi_generic[0xbd] = StandardCharCode.onehalf;
				ansi_generic[0xbe] = StandardCharCode.threequarters;
				ansi_generic[0xbf] = StandardCharCode.questiondown;
				ansi_generic[0xc0] = StandardCharCode.Agrave;
				ansi_generic[0xc1] = StandardCharCode.Aacute;
				ansi_generic[0xc2] = StandardCharCode.Acircumflex;
				ansi_generic[0xc3] = StandardCharCode.Atilde;
				ansi_generic[0xc4] = StandardCharCode.Adieresis;
				ansi_generic[0xc5] = StandardCharCode.Aring;
				ansi_generic[0xc6] = StandardCharCode.AE;
				ansi_generic[0xc7] = StandardCharCode.Ccedilla;
				ansi_generic[0xc8] = StandardCharCode.Egrave;
				ansi_generic[0xc9] = StandardCharCode.Eacute;
				ansi_generic[0xca] = StandardCharCode.Ecircumflex;
				ansi_generic[0xcb] = StandardCharCode.Edieresis;
				ansi_generic[0xcc] = StandardCharCode.Igrave;
				ansi_generic[0xcd] = StandardCharCode.Iacute;
				ansi_generic[0xce] = StandardCharCode.Icircumflex;
				ansi_generic[0xcf] = StandardCharCode.Idieresis;
				ansi_generic[0xd0] = StandardCharCode.Eth;
				ansi_generic[0xd1] = StandardCharCode.Ntilde;
				ansi_generic[0xd2] = StandardCharCode.Ograve;
				ansi_generic[0xd3] = StandardCharCode.Oacute;
				ansi_generic[0xd4] = StandardCharCode.Ocircumflex;
				ansi_generic[0xd5] = StandardCharCode.Otilde;
				ansi_generic[0xd6] = StandardCharCode.Odieresis;
				ansi_generic[0xd7] = StandardCharCode.multiply;
				ansi_generic[0xd8] = StandardCharCode.Oslash;
				ansi_generic[0xd9] = StandardCharCode.Ugrave;
				ansi_generic[0xda] = StandardCharCode.Uacute;
				ansi_generic[0xdb] = StandardCharCode.Ucircumflex;
				ansi_generic[0xdc] = StandardCharCode.Udieresis;
				ansi_generic[0xdd] = StandardCharCode.Yacute;
				ansi_generic[0xde] = StandardCharCode.Thorn;
				ansi_generic[0xdf] = StandardCharCode.germandbls;
				ansi_generic[0xe0] = StandardCharCode.agrave;
				ansi_generic[0xe1] = StandardCharCode.aacute;
				ansi_generic[0xe2] = StandardCharCode.acircumflex;
				ansi_generic[0xe3] = StandardCharCode.atilde;
				ansi_generic[0xe4] = StandardCharCode.adieresis;
				ansi_generic[0xe5] = StandardCharCode.aring;
				ansi_generic[0xe6] = StandardCharCode.ae;
				ansi_generic[0xe7] = StandardCharCode.ccedilla;
				ansi_generic[0xe8] = StandardCharCode.egrave;
				ansi_generic[0xe9] = StandardCharCode.eacute;
				ansi_generic[0xea] = StandardCharCode.ecircumflex;
				ansi_generic[0xeb] = StandardCharCode.edieresis;
				ansi_generic[0xec] = StandardCharCode.igrave;
				ansi_generic[0xed] = StandardCharCode.iacute;
				ansi_generic[0xee] = StandardCharCode.icircumflex;
				ansi_generic[0xef] = StandardCharCode.idieresis;
				ansi_generic[0xf0] = StandardCharCode.eth;
				ansi_generic[0xf1] = StandardCharCode.ntilde;
				ansi_generic[0xf2] = StandardCharCode.ograve;
				ansi_generic[0xf3] = StandardCharCode.oacute;
				ansi_generic[0xf4] = StandardCharCode.ocircumflex;
				ansi_generic[0xf5] = StandardCharCode.otilde;
				ansi_generic[0xf6] = StandardCharCode.odieresis;
				ansi_generic[0xf7] = StandardCharCode.divide;
				ansi_generic[0xf8] = StandardCharCode.oslash;
				ansi_generic[0xf9] = StandardCharCode.ugrave;
				ansi_generic[0xfa] = StandardCharCode.uacute;
				ansi_generic[0xfb] = StandardCharCode.ucircumflex;
				ansi_generic[0xfc] = StandardCharCode.udieresis;
				ansi_generic[0xfd] = StandardCharCode.yacute;
				ansi_generic[0xfe] = StandardCharCode.thorn;
				ansi_generic[0xff] = StandardCharCode.ydieresis;

				return ansi_generic;
			}
		}

		public static Charcode AnsiSymbol {
			get {
				Charcode code = new Charcode(256);

				code[0x06] = StandardCharCode.formula;
				code[0x1e] = StandardCharCode.nobrkhyphen;
				code[0x1f] = StandardCharCode.opthyphen;
				code[' '] = StandardCharCode.space;
				code['!'] = StandardCharCode.exclam;
				code['"'] = StandardCharCode.universal;
				code['#'] = StandardCharCode.mathnumbersign;
				code['$'] = StandardCharCode.existential;
				code['%'] = StandardCharCode.percent;
				code['&'] = StandardCharCode.ampersand;
				code['\\'] = StandardCharCode.suchthat;
				code['('] = StandardCharCode.parenleft;
				code[')'] = StandardCharCode.parenright;
				code['*'] = StandardCharCode.mathasterisk;
				code['+'] = StandardCharCode.mathplus;
				code[','] = StandardCharCode.comma;
				code['-'] = StandardCharCode.mathminus;
				code['.'] = StandardCharCode.period;
				code['/'] = StandardCharCode.slash;
				code['0'] = StandardCharCode.zero;
				code['1'] = StandardCharCode.one;
				code['2'] = StandardCharCode.two;
				code['3'] = StandardCharCode.three;
				code['4'] = StandardCharCode.four;
				code['5'] = StandardCharCode.five;
				code['6'] = StandardCharCode.six;
				code['7'] = StandardCharCode.seven;
				code['8'] = StandardCharCode.eight;
				code['9'] = StandardCharCode.nine;
				code[':'] = StandardCharCode.colon;
				code[';'] = StandardCharCode.semicolon;
				code['<'] = StandardCharCode.less;
				code['='] = StandardCharCode.mathequal;
				code['>'] = StandardCharCode.greater;
				code['?'] = StandardCharCode.question;
				code['@'] = StandardCharCode.congruent;
				code['A'] = StandardCharCode.Alpha;
				code['B'] = StandardCharCode.Beta;
				code['C'] = StandardCharCode.Chi;
				code['D'] = StandardCharCode.Delta;
				code['E'] = StandardCharCode.Epsilon;
				code['F'] = StandardCharCode.Phi;
				code['G'] = StandardCharCode.Gamma;
				code['H'] = StandardCharCode.Eta;
				code['I'] = StandardCharCode.Iota;
				code['K'] = StandardCharCode.Kappa;
				code['L'] = StandardCharCode.Lambda;
				code['M'] = StandardCharCode.Mu;
				code['N'] = StandardCharCode.Nu;
				code['O'] = StandardCharCode.Omicron;
				code['P'] = StandardCharCode.Pi;
				code['Q'] = StandardCharCode.Theta;
				code['R'] = StandardCharCode.Rho;
				code['S'] = StandardCharCode.Sigma;
				code['T'] = StandardCharCode.Tau;
				code['U'] = StandardCharCode.Upsilon;
				code['V'] = StandardCharCode.varsigma;
				code['W'] = StandardCharCode.Omega;
				code['X'] = StandardCharCode.Xi;
				code['Y'] = StandardCharCode.Psi;
				code['Z'] = StandardCharCode.Zeta;
				code['['] = StandardCharCode.bracketleft;
				code['\\'] = StandardCharCode.backslash;
				code[']'] = StandardCharCode.bracketright;
				code['^'] = StandardCharCode.asciicircum;
				code['_'] = StandardCharCode.underscore;
				code['`'] = StandardCharCode.quoteleft;
				code['a'] = StandardCharCode.alpha;
				code['b'] = StandardCharCode.beta;
				code['c'] = StandardCharCode.chi;
				code['d'] = StandardCharCode.delta;
				code['e'] = StandardCharCode.epsilon;
				code['f'] = StandardCharCode.phi;
				code['g'] = StandardCharCode.gamma;
				code['h'] = StandardCharCode.eta;
				code['i'] = StandardCharCode.iota;
				code['k'] = StandardCharCode.kappa;
				code['l'] = StandardCharCode.lambda;
				code['m'] = StandardCharCode.mu;
				code['n'] = StandardCharCode.nu;
				code['o'] = StandardCharCode.omicron;
				code['p'] = StandardCharCode.pi;
				code['q'] = StandardCharCode.theta;
				code['r'] = StandardCharCode.rho;
				code['s'] = StandardCharCode.sigma;
				code['t'] = StandardCharCode.tau;
				code['u'] = StandardCharCode.upsilon;
				code['w'] = StandardCharCode.omega;
				code['x'] = StandardCharCode.xi;
				code['y'] = StandardCharCode.psi;
				code['z'] = StandardCharCode.zeta;
				code['{'] = StandardCharCode.braceleft;
				code['|'] = StandardCharCode.bar;
				code['}'] = StandardCharCode.braceright;
				code['~'] = StandardCharCode.mathtilde;

				return code;
			}
		}
		#endregion	// Public Static Methods
	}
}
