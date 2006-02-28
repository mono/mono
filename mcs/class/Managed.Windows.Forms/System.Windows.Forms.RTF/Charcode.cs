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

		#region Public Constructors
		public Charcode() : this(256) {
		}

		private Charcode(int size) {
			this.size = size;
			this.codes = new StandardCharCode[size];
			this.reverse = new Hashtable(size);

			for (int i = 0; i < size; i++) {
				codes[i] = StandardCharCode.nothing;
			}
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

			set {
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
				Charcode code = new Charcode(256);

				code[0x06] = StandardCharCode.formula;
				code[0x1e] =  StandardCharCode.nobrkhyphen;
				code[0x1f] = StandardCharCode.opthyphen;
				code[' '] = StandardCharCode.space;
				code['!'] = StandardCharCode.exclam;
				code['"'] = StandardCharCode.quotedbl;
				code['#'] = StandardCharCode.numbersign;
				code['$'] = StandardCharCode.dollar;
				code['%'] = StandardCharCode.percent;
				code['&'] = StandardCharCode.ampersand;
				code['\\'] = StandardCharCode.quoteright;
				code['('] = StandardCharCode.parenleft;
				code[')'] = StandardCharCode.parenright;
				code['*'] = StandardCharCode.asterisk;
				code['+'] = StandardCharCode.plus;
				code[','] = StandardCharCode.comma;
				code['-'] = StandardCharCode.hyphen;
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
				code['='] = StandardCharCode.equal;
				code['>'] = StandardCharCode.greater;
				code['?'] = StandardCharCode.question;
				code['@'] = StandardCharCode.at;
				code['A'] = StandardCharCode.A;
				code['B'] = StandardCharCode.B;
				code['C'] = StandardCharCode.C;
				code['D'] = StandardCharCode.D;
				code['E'] = StandardCharCode.E;
				code['F'] = StandardCharCode.F;
				code['G'] = StandardCharCode.G;
				code['H'] = StandardCharCode.H;
				code['I'] = StandardCharCode.I;
				code['J'] = StandardCharCode.J;
				code['K'] = StandardCharCode.K;
				code['L'] = StandardCharCode.L;
				code['M'] = StandardCharCode.M;
				code['N'] = StandardCharCode.N;
				code['O'] = StandardCharCode.O;
				code['P'] = StandardCharCode.P;
				code['Q'] = StandardCharCode.Q;
				code['R'] = StandardCharCode.R;
				code['S'] = StandardCharCode.S;
				code['T'] = StandardCharCode.T;
				code['U'] = StandardCharCode.U;
				code['V'] = StandardCharCode.V;
				code['W'] = StandardCharCode.W;
				code['X'] = StandardCharCode.X;
				code['Y'] = StandardCharCode.Y;
				code['Z'] = StandardCharCode.Z;
				code['['] = StandardCharCode.bracketleft;
				code['\\'] = StandardCharCode.backslash;
				code[']'] = StandardCharCode.bracketright;
				code['^'] = StandardCharCode.asciicircum;
				code['_'] = StandardCharCode.underscore;
				code['`'] = StandardCharCode.quoteleft;
				code['a'] = StandardCharCode.a;
				code['b'] = StandardCharCode.b;
				code['c'] = StandardCharCode.c;
				code['d'] = StandardCharCode.d;
				code['e'] = StandardCharCode.e;
				code['f'] = StandardCharCode.f;
				code['g'] = StandardCharCode.g;
				code['h'] = StandardCharCode.h;
				code['i'] = StandardCharCode.i;
				code['j'] = StandardCharCode.j;
				code['k'] = StandardCharCode.k;
				code['l'] = StandardCharCode.l;
				code['m'] = StandardCharCode.m;
				code['n'] = StandardCharCode.n;
				code['o'] = StandardCharCode.o;
				code['p'] = StandardCharCode.p;
				code['q'] = StandardCharCode.q;
				code['r'] = StandardCharCode.r;
				code['s'] = StandardCharCode.s;
				code['t'] = StandardCharCode.t;
				code['u'] = StandardCharCode.u;
				code['v'] = StandardCharCode.v;
				code['w'] = StandardCharCode.w;
				code['x'] = StandardCharCode.x;
				code['y'] = StandardCharCode.y;
				code['z'] = StandardCharCode.z;
				code['{'] = StandardCharCode.braceleft;
				code['|'] = StandardCharCode.bar;
				code['}'] = StandardCharCode.braceright;
				code['~'] = StandardCharCode.asciitilde;
				code[0xa0] = StandardCharCode.nobrkspace;
				code[0xa1] = StandardCharCode.exclamdown;
				code[0xa2] = StandardCharCode.cent;
				code[0xa3] = StandardCharCode.sterling;
				code[0xa4] = StandardCharCode.currency;
				code[0xa5] = StandardCharCode.yen;
				code[0xa6] = StandardCharCode.brokenbar;
				code[0xa7] = StandardCharCode.section;
				code[0xa8] = StandardCharCode.dieresis;
				code[0xa9] = StandardCharCode.copyright;
				code[0xaa] = StandardCharCode.ordfeminine;
				code[0xab] = StandardCharCode.guillemotleft;
				code[0xac] = StandardCharCode.logicalnot;
				code[0xad] = StandardCharCode.opthyphen;
				code[0xae] = StandardCharCode.registered;
				code[0xaf] = StandardCharCode.macron;
				code[0xb0] = StandardCharCode.degree;
				code[0xb1] = StandardCharCode.plusminus;
				code[0xb2] = StandardCharCode.twosuperior;
				code[0xb3] = StandardCharCode.threesuperior;
				code[0xb4] = StandardCharCode.acute;
				code[0xb5] = StandardCharCode.mu;
				code[0xb6] = StandardCharCode.paragraph;
				code[0xb7] = StandardCharCode.periodcentered;
				code[0xb8] = StandardCharCode.cedilla;
				code[0xb9] = StandardCharCode.onesuperior;
				code[0xba] = StandardCharCode.ordmasculine;
				code[0xbb] = StandardCharCode.guillemotright;
				code[0xbc] = StandardCharCode.onequarter;
				code[0xbd] = StandardCharCode.onehalf;
				code[0xbe] = StandardCharCode.threequarters;
				code[0xbf] = StandardCharCode.questiondown;
				code[0xc0] = StandardCharCode.Agrave;
				code[0xc1] = StandardCharCode.Aacute;
				code[0xc2] = StandardCharCode.Acircumflex;
				code[0xc3] = StandardCharCode.Atilde;
				code[0xc4] = StandardCharCode.Adieresis;
				code[0xc5] = StandardCharCode.Aring;
				code[0xc6] = StandardCharCode.AE;
				code[0xc7] = StandardCharCode.Ccedilla;
				code[0xc8] = StandardCharCode.Egrave;
				code[0xc9] = StandardCharCode.Eacute;
				code[0xca] = StandardCharCode.Ecircumflex;
				code[0xcb] = StandardCharCode.Edieresis;
				code[0xcc] = StandardCharCode.Igrave;
				code[0xcd] = StandardCharCode.Iacute;
				code[0xce] = StandardCharCode.Icircumflex;
				code[0xcf] = StandardCharCode.Idieresis;
				code[0xd0] = StandardCharCode.Eth;
				code[0xd1] = StandardCharCode.Ntilde;
				code[0xd2] = StandardCharCode.Ograve;
				code[0xd3] = StandardCharCode.Oacute;
				code[0xd4] = StandardCharCode.Ocircumflex;
				code[0xd5] = StandardCharCode.Otilde;
				code[0xd6] = StandardCharCode.Odieresis;
				code[0xd7] = StandardCharCode.multiply;
				code[0xd8] = StandardCharCode.Oslash;
				code[0xd9] = StandardCharCode.Ugrave;
				code[0xda] = StandardCharCode.Uacute;
				code[0xdb] = StandardCharCode.Ucircumflex;
				code[0xdc] = StandardCharCode.Udieresis;
				code[0xdd] = StandardCharCode.Yacute;
				code[0xde] = StandardCharCode.Thorn;
				code[0xdf] = StandardCharCode.germandbls;
				code[0xe0] = StandardCharCode.agrave;
				code[0xe1] = StandardCharCode.aacute;
				code[0xe2] = StandardCharCode.acircumflex;
				code[0xe3] = StandardCharCode.atilde;
				code[0xe4] = StandardCharCode.adieresis;
				code[0xe5] = StandardCharCode.aring;
				code[0xe6] = StandardCharCode.ae;
				code[0xe7] = StandardCharCode.ccedilla;
				code[0xe8] = StandardCharCode.egrave;
				code[0xe9] = StandardCharCode.eacute;
				code[0xea] = StandardCharCode.ecircumflex;
				code[0xeb] = StandardCharCode.edieresis;
				code[0xec] = StandardCharCode.igrave;
				code[0xed] = StandardCharCode.iacute;
				code[0xee] = StandardCharCode.icircumflex;
				code[0xef] = StandardCharCode.idieresis;
				code[0xf0] = StandardCharCode.eth;
				code[0xf1] = StandardCharCode.ntilde;
				code[0xf2] = StandardCharCode.ograve;
				code[0xf3] = StandardCharCode.oacute;
				code[0xf4] = StandardCharCode.ocircumflex;
				code[0xf5] = StandardCharCode.otilde;
				code[0xf6] = StandardCharCode.odieresis;
				code[0xf7] = StandardCharCode.divide;
				code[0xf8] = StandardCharCode.oslash;
				code[0xf9] = StandardCharCode.ugrave;
				code[0xfa] = StandardCharCode.uacute;
				code[0xfb] = StandardCharCode.ucircumflex;
				code[0xfc] = StandardCharCode.udieresis;
				code[0xfd] = StandardCharCode.yacute;
				code[0xfe] = StandardCharCode.thorn;
				code[0xff] = StandardCharCode.ydieresis;

				return code;
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
