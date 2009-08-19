using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Text.RegularExpressions {

	/* This behaves like a growing list of tuples (base, offsetpos) */
	class RxLinkRef: LinkRef {
		public int[] offsets;
		public int current = 0;

		public RxLinkRef ()
		{
			offsets = new int [8];
		}

		// the start of the branch instruction
		// in the program stream
		public void PushInstructionBase (int offset)
		{
			if ((current & 1) != 0)
				throw new Exception ();
			if (current == offsets.Length) {
				int[] newarray = new int [offsets.Length * 2];
				Array.Copy (offsets, newarray, offsets.Length);
				offsets = newarray;
			}
			offsets [current++] = offset;
		}

		// the position in the program stream where the jump offset is stored
		public void PushOffsetPosition (int offset)
		{
			if ((current & 1) == 0)
				throw new Exception ();
			offsets [current++] = offset;
		}

	}

	class RxCompiler : ICompiler {
		protected byte[] program = new byte [32];
		protected int curpos = 0;

		public RxCompiler () {
		}

		void MakeRoom (int bytes)
		{
			while (curpos + bytes > program.Length) {
				int newsize = program.Length * 2;
				byte[] newp = new byte [newsize];
				Buffer.BlockCopy (program, 0, newp, 0, program.Length);
				program = newp;
			}
		}

		void Emit (byte val)
		{
			MakeRoom (1);
			program [curpos] = val;
			++curpos;
		}

		void Emit (RxOp opcode)
		{
			Emit ((byte)opcode);
		}

		void Emit (ushort val)
		{
			MakeRoom (2);
			program [curpos] = (byte)val;
			program [curpos + 1] = (byte)(val >> 8);
			curpos += 2;
		}

		void Emit (int val)
		{
			MakeRoom (4);
			program [curpos] = (byte)val;
			program [curpos + 1] = (byte)(val >> 8);
			program [curpos + 2] = (byte)(val >> 16);
			program [curpos + 3] = (byte)(val >> 24);
			curpos += 4;
		}

		void BeginLink (LinkRef lref) {
			RxLinkRef link = lref as RxLinkRef;
			link.PushInstructionBase (curpos);
		}

		void EmitLink (LinkRef lref)
		{
			RxLinkRef link = lref as RxLinkRef;
			link.PushOffsetPosition (curpos);
			Emit ((ushort)0);
		}

		// ICompiler implementation
		public void Reset ()
		{
			curpos = 0;
		}

		public IMachineFactory GetMachineFactory ()
		{
			byte[] code = new byte [curpos];
			Buffer.BlockCopy (program, 0, code, 0, curpos);
			//Console.WriteLine ("Program size: {0}", curpos);

			return new RxInterpreterFactory (code, null);
		}

		public void EmitFalse ()
		{
			Emit (RxOp.False);
		}

		public void EmitTrue ()
		{
			Emit (RxOp.True);
		}

		/* Overriden by CILCompiler */
		public virtual void EmitOp (RxOp op, bool negate, bool ignore, bool reverse) {
			int offset = 0;
			if (negate)
				offset += 1;
			if (ignore)
				offset += 2;
			if (reverse)
				offset += 4;

			Emit ((RxOp)((int)op + offset));
		}

		public virtual void EmitOpIgnoreReverse (RxOp op, bool ignore, bool reverse) {
			int offset = 0;
			if (ignore)
				offset += 1;
			if (reverse)
				offset += 2;

			Emit ((RxOp)((int)op + offset));
		}

		public virtual void EmitOpNegateReverse (RxOp op, bool negate, bool reverse) {
			int offset = 0;
			if (negate)
				offset += 1;
			if (reverse)
				offset += 2;

			Emit ((RxOp)((int)op + offset));
		}

		public void EmitCharacter (char c, bool negate, bool ignore, bool reverse)
		{
			if (ignore)
				c = Char.ToLower (c);
			if (c < 256) {
				EmitOp (RxOp.Char, negate, ignore, reverse);
				Emit ((byte)c);
			} else {
				EmitOp (RxOp.UnicodeChar, negate, ignore, reverse);
				Emit ((ushort)c);
			}
		}

		void EmitUniCat (UnicodeCategory cat, bool negate, bool reverse)
		{
			EmitOpNegateReverse (RxOp.CategoryUnicode, negate, reverse);
			Emit ((byte)cat);
		}

		void EmitCatGeneral (Category cat, bool negate, bool reverse)
		{
			EmitOpNegateReverse (RxOp.CategoryGeneral, negate, reverse);
			Emit ((byte)cat);
		}

		public void EmitCategory (Category cat, bool negate, bool reverse)
		{
			switch (cat) {
			case Category.Any:
			case Category.EcmaAny:
				EmitOpNegateReverse (RxOp.CategoryAny, negate, reverse);
				break;
			case Category.AnySingleline:
				EmitOpNegateReverse (RxOp.CategoryAnySingleline, negate, reverse);
				break;
			case Category.Word:
				EmitOpNegateReverse (RxOp.CategoryWord, negate, reverse);
				break;
			case Category.Digit:
				EmitOpNegateReverse (RxOp.CategoryDigit, negate, reverse);
				break;
			case Category.WhiteSpace:
				EmitOpNegateReverse (RxOp.CategoryWhiteSpace, negate, reverse);
				break;
			/* FIXME: translate EcmaWord, EcmaWhiteSpace into Bitmaps? EcmaWhiteSpace will fit very well with the IL engine */
			case Category.EcmaWord:
				EmitOpNegateReverse (RxOp.CategoryEcmaWord, negate, reverse);
				break;
			case Category.EcmaDigit:
				EmitRange ('0', '9', negate, false, reverse);
				break;
			case Category.EcmaWhiteSpace:
				EmitOpNegateReverse (RxOp.CategoryEcmaWhiteSpace, negate, reverse);
				break;
			case Category.UnicodeSpecials:
				EmitOpNegateReverse (RxOp.CategoryUnicodeSpecials, negate, reverse);
				break;
			// Unicode categories...
			// letter
			case Category.UnicodeLu: EmitUniCat (UnicodeCategory.UppercaseLetter, negate, reverse); break;
			case Category.UnicodeLl: EmitUniCat (UnicodeCategory.LowercaseLetter, negate, reverse); break;
			case Category.UnicodeLt: EmitUniCat (UnicodeCategory.TitlecaseLetter, negate, reverse); break;
			case Category.UnicodeLm: EmitUniCat (UnicodeCategory.ModifierLetter, negate, reverse); break;
			case Category.UnicodeLo: EmitUniCat (UnicodeCategory.OtherLetter, negate, reverse); break;
			// mark
			case Category.UnicodeMn: EmitUniCat (UnicodeCategory.NonSpacingMark, negate, reverse); break;
			case Category.UnicodeMe: EmitUniCat (UnicodeCategory.EnclosingMark, negate, reverse); break;
			case Category.UnicodeMc: EmitUniCat (UnicodeCategory.SpacingCombiningMark, negate, reverse); break;
			case Category.UnicodeNd: EmitUniCat (UnicodeCategory.DecimalDigitNumber, negate, reverse); break;
			// number
			case Category.UnicodeNl: EmitUniCat (UnicodeCategory.LetterNumber, negate, reverse); break;
			case Category.UnicodeNo: EmitUniCat (UnicodeCategory.OtherNumber, negate, reverse); break;
			// separator
			case Category.UnicodeZs: EmitUniCat (UnicodeCategory.SpaceSeparator, negate, reverse); break;
			case Category.UnicodeZl: EmitUniCat (UnicodeCategory.LineSeparator, negate, reverse); break;
			case Category.UnicodeZp: EmitUniCat (UnicodeCategory.ParagraphSeparator, negate, reverse); break;
			// punctuation
			case Category.UnicodePd: EmitUniCat (UnicodeCategory.DashPunctuation, negate, reverse); break;
			case Category.UnicodePs: EmitUniCat (UnicodeCategory.OpenPunctuation, negate, reverse); break;
			case Category.UnicodePi: EmitUniCat (UnicodeCategory.InitialQuotePunctuation, negate, reverse); break;
			case Category.UnicodePe: EmitUniCat (UnicodeCategory.ClosePunctuation, negate, reverse); break;
			case Category.UnicodePf: EmitUniCat (UnicodeCategory.FinalQuotePunctuation, negate, reverse); break;
			case Category.UnicodePc: EmitUniCat (UnicodeCategory.ConnectorPunctuation, negate, reverse); break;
			case Category.UnicodePo: EmitUniCat (UnicodeCategory.OtherPunctuation, negate, reverse); break;
			// symbol
			case Category.UnicodeSm: EmitUniCat (UnicodeCategory.MathSymbol, negate, reverse); break;
			case Category.UnicodeSc: EmitUniCat (UnicodeCategory.CurrencySymbol, negate, reverse); break;
			case Category.UnicodeSk: EmitUniCat (UnicodeCategory.ModifierSymbol, negate, reverse); break;
			case Category.UnicodeSo: EmitUniCat (UnicodeCategory.OtherSymbol, negate, reverse); break;
			// other
			case Category.UnicodeCc: EmitUniCat (UnicodeCategory.Control, negate, reverse); break;
			case Category.UnicodeCf: EmitUniCat (UnicodeCategory.Format, negate, reverse); break;
			case Category.UnicodeCo: EmitUniCat (UnicodeCategory.PrivateUse, negate, reverse); break;
			case Category.UnicodeCs: EmitUniCat (UnicodeCategory.Surrogate, negate, reverse); break;
			case Category.UnicodeCn: EmitUniCat (UnicodeCategory.OtherNotAssigned, negate, reverse); break; 
			// Unicode block ranges...
			case Category.UnicodeBasicLatin:
				EmitRange ('\u0000', '\u007F', negate, false, reverse); break;
			case Category.UnicodeLatin1Supplement:
				EmitRange ('\u0080', '\u00FF', negate, false, reverse); break;
			case Category.UnicodeLatinExtendedA:
				EmitRange ('\u0100', '\u017F', negate, false, reverse); break;
			case Category.UnicodeLatinExtendedB:
				EmitRange ('\u0180', '\u024F', negate, false, reverse); break;
			case Category.UnicodeIPAExtensions:
				EmitRange ('\u0250', '\u02AF', negate, false, reverse); break;
			case Category.UnicodeSpacingModifierLetters:
				EmitRange ('\u02B0', '\u02FF', negate, false, reverse); break;
			case Category.UnicodeCombiningDiacriticalMarks:
				EmitRange ('\u0300', '\u036F', negate, false, reverse); break;
			case Category.UnicodeGreek:
				EmitRange ('\u0370', '\u03FF', negate, false, reverse); break;
			case Category.UnicodeCyrillic:
				EmitRange ('\u0400', '\u04FF', negate, false, reverse); break;
			case Category.UnicodeArmenian:
				EmitRange ('\u0530', '\u058F', negate, false, reverse); break;
			case Category.UnicodeHebrew:
				EmitRange ('\u0590', '\u05FF', negate, false, reverse); break;
			case Category.UnicodeArabic:
				EmitRange ('\u0600', '\u06FF', negate, false, reverse); break;
			case Category.UnicodeSyriac:
				EmitRange ('\u0700', '\u074F', negate, false, reverse); break;
			case Category.UnicodeThaana:
				EmitRange ('\u0780', '\u07BF', negate, false, reverse); break;
			case Category.UnicodeDevanagari:
				EmitRange ('\u0900', '\u097F', negate, false, reverse); break;
			case Category.UnicodeBengali:
				EmitRange ('\u0980', '\u09FF', negate, false, reverse); break;
			case Category.UnicodeGurmukhi:
				EmitRange ('\u0A00', '\u0A7F', negate, false, reverse); break;
			case Category.UnicodeGujarati:
				EmitRange ('\u0A80', '\u0AFF', negate, false, reverse); break;
			case Category.UnicodeOriya:
				EmitRange ('\u0B00', '\u0B7F', negate, false, reverse); break;
			case Category.UnicodeTamil:
				EmitRange ('\u0B80', '\u0BFF', negate, false, reverse); break;
			case Category.UnicodeTelugu:
				EmitRange ('\u0C00', '\u0C7F', negate, false, reverse); break;
			case Category.UnicodeKannada:
				EmitRange ('\u0C80', '\u0CFF', negate, false, reverse); break;
			case Category.UnicodeMalayalam:
				EmitRange ('\u0D00', '\u0D7F', negate, false, reverse); break;
			case Category.UnicodeSinhala:
				EmitRange ('\u0D80', '\u0DFF', negate, false, reverse); break;
			case Category.UnicodeThai:
				EmitRange ('\u0E00', '\u0E7F', negate, false, reverse); break;
			case Category.UnicodeLao:
				EmitRange ('\u0E80', '\u0EFF', negate, false, reverse); break;
			case Category.UnicodeTibetan:
				EmitRange ('\u0F00', '\u0FFF', negate, false, reverse); break;
			case Category.UnicodeMyanmar:
				EmitRange ('\u1000', '\u109F', negate, false, reverse); break;
			case Category.UnicodeGeorgian:
				EmitRange ('\u10A0', '\u10FF', negate, false, reverse); break;
			case Category.UnicodeHangulJamo:
				EmitRange ('\u1100', '\u11FF', negate, false, reverse); break;
			case Category.UnicodeEthiopic:
				EmitRange ('\u1200', '\u137F', negate, false, reverse); break;
			case Category.UnicodeCherokee:
				EmitRange ('\u13A0', '\u13FF', negate, false, reverse); break;
			case Category.UnicodeUnifiedCanadianAboriginalSyllabics:
				EmitRange ('\u1400', '\u167F', negate, false, reverse); break;
			case Category.UnicodeOgham:
				EmitRange ('\u1680', '\u169F', negate, false, reverse); break;
			case Category.UnicodeRunic:
				EmitRange ('\u16A0', '\u16FF', negate, false, reverse); break;
			case Category.UnicodeKhmer:
				EmitRange ('\u1780', '\u17FF', negate, false, reverse); break;
			case Category.UnicodeMongolian:
				EmitRange ('\u1800', '\u18AF', negate, false, reverse); break;
			case Category.UnicodeLatinExtendedAdditional:
				EmitRange ('\u1E00', '\u1EFF', negate, false, reverse); break;
			case Category.UnicodeGreekExtended:
				EmitRange ('\u1F00', '\u1FFF', negate, false, reverse); break;
			case Category.UnicodeGeneralPunctuation:
				EmitRange ('\u2000', '\u206F', negate, false, reverse); break;
			case Category.UnicodeSuperscriptsandSubscripts:
				EmitRange ('\u2070', '\u209F', negate, false, reverse); break;
			case Category.UnicodeCurrencySymbols:
				EmitRange ('\u20A0', '\u20CF', negate, false, reverse); break;
			case Category.UnicodeCombiningMarksforSymbols:
				EmitRange ('\u20D0', '\u20FF', negate, false, reverse); break;
			case Category.UnicodeLetterlikeSymbols:
				EmitRange ('\u2100', '\u214F', negate, false, reverse); break;
			case Category.UnicodeNumberForms:
				EmitRange ('\u2150', '\u218F', negate, false, reverse); break;
			case Category.UnicodeArrows:
				EmitRange ('\u2190', '\u21FF', negate, false, reverse); break;
			case Category.UnicodeMathematicalOperators:
				EmitRange ('\u2200', '\u22FF', negate, false, reverse); break;
			case Category.UnicodeMiscellaneousTechnical:
				EmitRange ('\u2300', '\u23FF', negate, false, reverse); break;
			case Category.UnicodeControlPictures:
				EmitRange ('\u2400', '\u243F', negate, false, reverse); break;
			case Category.UnicodeOpticalCharacterRecognition:
				EmitRange ('\u2440', '\u245F', negate, false, reverse); break;
			case Category.UnicodeEnclosedAlphanumerics:
				EmitRange ('\u2460', '\u24FF', negate, false, reverse); break;
			case Category.UnicodeBoxDrawing:
				EmitRange ('\u2500', '\u257F', negate, false, reverse); break;
			case Category.UnicodeBlockElements:
				EmitRange ('\u2580', '\u259F', negate, false, reverse); break;
			case Category.UnicodeGeometricShapes:
				EmitRange ('\u25A0', '\u25FF', negate, false, reverse); break;
			case Category.UnicodeMiscellaneousSymbols:
				EmitRange ('\u2600', '\u26FF', negate, false, reverse); break;
			case Category.UnicodeDingbats:
				EmitRange ('\u2700', '\u27BF', negate, false, reverse); break;
			case Category.UnicodeBraillePatterns:
				EmitRange ('\u2800', '\u28FF', negate, false, reverse); break;
			case Category.UnicodeCJKRadicalsSupplement:
				EmitRange ('\u2E80', '\u2EFF', negate, false, reverse); break;
			case Category.UnicodeKangxiRadicals:
				EmitRange ('\u2F00', '\u2FDF', negate, false, reverse); break;
			case Category.UnicodeIdeographicDescriptionCharacters:
				EmitRange ('\u2FF0', '\u2FFF', negate, false, reverse); break;
			case Category.UnicodeCJKSymbolsandPunctuation:
				EmitRange ('\u3000', '\u303F', negate, false, reverse); break;
			case Category.UnicodeHiragana:
				EmitRange ('\u3040', '\u309F', negate, false, reverse); break;
			case Category.UnicodeKatakana:
				EmitRange ('\u30A0', '\u30FF', negate, false, reverse); break;
			case Category.UnicodeBopomofo:
				EmitRange ('\u3100', '\u312F', negate, false, reverse); break;
			case Category.UnicodeHangulCompatibilityJamo:
				EmitRange ('\u3130', '\u318F', negate, false, reverse); break;
			case Category.UnicodeKanbun:
				EmitRange ('\u3190', '\u319F', negate, false, reverse); break;
			case Category.UnicodeBopomofoExtended:
				EmitRange ('\u31A0', '\u31BF', negate, false, reverse); break;
			case Category.UnicodeEnclosedCJKLettersandMonths:
				EmitRange ('\u3200', '\u32FF', negate, false, reverse); break;
			case Category.UnicodeCJKCompatibility:
				EmitRange ('\u3300', '\u33FF', negate, false, reverse); break;
			case Category.UnicodeCJKUnifiedIdeographsExtensionA:
				EmitRange ('\u3400', '\u4DB5', negate, false, reverse); break;
			case Category.UnicodeCJKUnifiedIdeographs:
				EmitRange ('\u4E00', '\u9FFF', negate, false, reverse); break;
			case Category.UnicodeYiSyllables:
				EmitRange ('\uA000', '\uA48F', negate, false, reverse); break;
			case Category.UnicodeYiRadicals:
				EmitRange ('\uA490', '\uA4CF', negate, false, reverse); break;
			case Category.UnicodeHangulSyllables:
				EmitRange ('\uAC00', '\uD7A3', negate, false, reverse); break;
			case Category.UnicodeHighSurrogates:
				EmitRange ('\uD800', '\uDB7F', negate, false, reverse); break;
			case Category.UnicodeHighPrivateUseSurrogates:
				EmitRange ('\uDB80', '\uDBFF', negate, false, reverse); break;
			case Category.UnicodeLowSurrogates:
				EmitRange ('\uDC00', '\uDFFF', negate, false, reverse); break;
			case Category.UnicodePrivateUse:
				EmitRange ('\uE000', '\uF8FF', negate, false, reverse); break;
			case Category.UnicodeCJKCompatibilityIdeographs:
				EmitRange ('\uF900', '\uFAFF', negate, false, reverse); break;
			case Category.UnicodeAlphabeticPresentationForms:
				EmitRange ('\uFB00', '\uFB4F', negate, false, reverse); break;
			case Category.UnicodeArabicPresentationFormsA:
				EmitRange ('\uFB50', '\uFDFF', negate, false, reverse); break;
			case Category.UnicodeCombiningHalfMarks:
				EmitRange ('\uFE20', '\uFE2F', negate, false, reverse); break;
			case Category.UnicodeCJKCompatibilityForms:
				EmitRange ('\uFE30', '\uFE4F', negate, false, reverse); break;
			case Category.UnicodeSmallFormVariants:
				EmitRange ('\uFE50', '\uFE6F', negate, false, reverse); break;
			case Category.UnicodeArabicPresentationFormsB:
				EmitRange ('\uFE70', '\uFEFE', negate, false, reverse); break;
			case Category.UnicodeHalfwidthandFullwidthForms:
				EmitRange ('\uFF00', '\uFFEF', negate, false, reverse); break;

				// Complex categories
			case Category.UnicodeL:
			case Category.UnicodeM:
			case Category.UnicodeN:
			case Category.UnicodeZ:
			case Category.UnicodeP:
			case Category.UnicodeS:
			case Category.UnicodeC:
				EmitCatGeneral (cat, negate, reverse); break;

			default:
				throw new NotImplementedException ("Missing category: " + cat);
			}
		}

		public void EmitNotCategory (Category cat, bool negate, bool reverse)
		{
			// not sure why the compiler needed this separate interface funtion
			if (negate) {
				EmitCategory (cat, false, reverse);
			} else {
				EmitCategory (cat, true, reverse);
			}
		}

		public void EmitRange (char lo, char hi, bool negate, bool ignore, bool reverse)
		{
			if (lo < 256 && hi < 256) {
				EmitOp (RxOp.Range, negate, ignore, reverse);
				Emit ((byte)lo);
				Emit ((byte)hi);
			} else {
				EmitOp (RxOp.UnicodeRange, negate, ignore, reverse);
				Emit ((ushort)lo);
				Emit ((ushort)hi);
			}
		}

		public void EmitSet (char lo, BitArray set, bool negate, bool ignore, bool reverse)
		{
			int len = (set.Length + 0x7) >> 3;
			if (lo < 256 && len < 256) {
				EmitOp (RxOp.Bitmap, negate, ignore, reverse);
				Emit ((byte)lo);
				Emit ((byte)len);
			} else {
				EmitOp (RxOp.UnicodeBitmap, negate, ignore, reverse);
				Emit ((ushort)lo);
				Emit ((ushort)len);
			}
			// emit the bitmap bytes
			int b = 0;
			while (len-- != 0) {
				int word = 0;
				for (int i = 0; i < 8; ++ i) {
					if (b >= set.Length)
						break;
					if (set [b ++])
						word |= 1 << i;
				}
				Emit ((byte)word);
			}
		}

		public void EmitString (string str, bool ignore, bool reverse)
		{
			bool islatin1 = false;
			int i;
			int offset = 0;
			if (ignore)
				offset += 1;
			if (reverse)
				offset += 2;
			if (ignore)
				str = str.ToLower ();
			if (str.Length < 256) {
				islatin1 = true;
				for (i = 0; i < str.Length; ++i) {
					if (str [i] >= 256) {
						islatin1 = false;
						break;
					}
				}
			}
			if (islatin1) {
				EmitOpIgnoreReverse (RxOp.String, ignore, reverse);
				Emit ((byte)str.Length);
				for (i = 0; i < str.Length; ++i)
					Emit ((byte)str [i]);
			} else {
				EmitOpIgnoreReverse (RxOp.UnicodeString, ignore, reverse);
				if (str.Length > ushort.MaxValue)
					throw new NotSupportedException ();
				Emit ((ushort)str.Length);
				for (i = 0; i < str.Length; ++i)
					Emit ((ushort)str [i]);
			}
		}

		public void EmitPosition (Position pos)
		{
			switch (pos) {
			case Position.Any:
				Emit (RxOp.AnyPosition);
				break;
			case Position.Start:
				Emit (RxOp.StartOfString);
				break;
			case Position.StartOfString:
				Emit (RxOp.StartOfString);
				break;
			case Position.StartOfLine:
				Emit (RxOp.StartOfLine);
				break;
			case Position.StartOfScan:
				Emit (RxOp.StartOfScan);
				break;
			case Position.End:
				Emit (RxOp.End);
				break;
			case Position.EndOfString:
				Emit (RxOp.EndOfString);
				break;
			case Position.EndOfLine:
				Emit (RxOp.EndOfLine);
				break;
			case Position.Boundary:
				Emit (RxOp.WordBoundary);
				break;
			case Position.NonBoundary:
				Emit (RxOp.NoWordBoundary);
				break;
			default:
				throw new NotSupportedException ();
			}
		}

		public void EmitOpen (int gid)
		{
			if (gid > ushort.MaxValue)
				throw new NotSupportedException ();
			Emit (RxOp.OpenGroup);
			Emit ((ushort)gid);
		}

		public void EmitClose (int gid)
		{
			if (gid > ushort.MaxValue)
				throw new NotSupportedException ();
			Emit (RxOp.CloseGroup);
			Emit ((ushort)gid);
		}

		public void EmitBalanceStart(int gid, int balance, bool capture,  LinkRef tail)
		{
			BeginLink (tail);
			Emit (RxOp.BalanceStart);
			Emit ((ushort)gid);
			Emit ((ushort)balance);
			Emit ((byte)(capture ? 1 : 0));
			EmitLink (tail);
		}

		public void EmitBalance ()
		{
			Emit (RxOp.Balance);
		}

		public void EmitReference (int gid, bool ignore, bool reverse)
		{
			if (gid > ushort.MaxValue)
				throw new NotSupportedException ();
			EmitOpIgnoreReverse (RxOp.Reference, ignore, reverse);
			Emit ((ushort)gid);
		}

		public void EmitIfDefined (int gid, LinkRef tail)
		{
			if (gid > ushort.MaxValue)
				throw new NotSupportedException ();
			BeginLink (tail);
			Emit (RxOp.IfDefined);
			EmitLink (tail);
			Emit ((ushort)gid);
		}

		public void EmitSub (LinkRef tail)
		{
			BeginLink (tail);
			Emit (RxOp.SubExpression);
			EmitLink (tail);
		}

		public void EmitTest (LinkRef yes, LinkRef tail)
		{
			BeginLink (yes);
			BeginLink (tail);
			Emit (RxOp.Test);
			EmitLink (yes);
			EmitLink (tail);
		}

		public void EmitBranch (LinkRef next)
		{
			BeginLink (next);
			Emit (RxOp.Branch);
			EmitLink (next);
		}

		public void EmitJump (LinkRef target)
		{
			BeginLink (target);
			Emit (RxOp.Jump);
			EmitLink (target);
		}

		public void EmitIn (LinkRef tail)
		{
			// emitted for things like [\dabcfh]
			BeginLink (tail);
			Emit (RxOp.TestCharGroup);
			EmitLink (tail);
		}

		public void EmitRepeat (int min, int max, bool lazy, LinkRef until)
		{
			BeginLink (until);
			Emit (lazy ? RxOp.RepeatLazy : RxOp.Repeat);
			EmitLink (until);
			Emit (min);
			Emit (max);
		}

		public void EmitUntil (LinkRef repeat)
		{
			ResolveLink (repeat);
			Emit (RxOp.Until);
		}

		public void EmitInfo (int count, int min, int max)
		{
			Emit (RxOp.Info);
			if (count > ushort.MaxValue)
				throw new NotSupportedException ();
			Emit ((ushort)count);
			Emit (min);
			Emit (max);
		}

		public void EmitFastRepeat (int min, int max, bool lazy, LinkRef tail)
		{
			BeginLink (tail);
			Emit (lazy ? RxOp.FastRepeatLazy : RxOp.FastRepeat);
			EmitLink (tail);
			Emit (min);
			Emit (max);
		}

		public void EmitAnchor (bool reverse, int offset, LinkRef tail)
		{
			BeginLink (tail);
			if (reverse)
				Emit (RxOp.AnchorReverse);
			else
				Emit (RxOp.Anchor);
			EmitLink (tail);
			if (offset > ushort.MaxValue)
				throw new NotSupportedException ();
			Emit ((ushort)offset);
		}

		// event for the CILCompiler
		public void EmitBranchEnd ()
		{
		}

		public void EmitAlternationEnd ()
		{
		}

		public LinkRef NewLink ()
		{
			return new RxLinkRef ();
		}

		public void ResolveLink (LinkRef link)
		{
			RxLinkRef l = link as RxLinkRef;
			for (int i = 0; i < l.current; i += 2) {
				int offset = curpos - l.offsets [i];
				if (offset > ushort.MaxValue)
					throw new NotSupportedException ();
				int offsetpos = l.offsets [i + 1];
				program [offsetpos] = (byte)offset;
				program [offsetpos + 1] = (byte)(offset >> 8);
			}
		}

	}

	class RxInterpreterFactory : IMachineFactory {
		public RxInterpreterFactory (byte[] program, EvalDelegate eval_del) {
			this.program = program;
			this.eval_del = eval_del;
		}
		
		public IMachine NewInstance () {
			return new RxInterpreter (program, eval_del);
		}

		public int GroupCount {
			get { 
				return (int)program [1] | ((int)program [2] << 8);
			}
		}

		public int Gap {
			get { return gap; }
			set { gap = value; }
		}

		public IDictionary Mapping {
			get { return mapping; }
			set { mapping = value; }
		}

		public string [] NamesMapping {
			get { return namesMapping; }
			set { namesMapping = value; }
		}

		private IDictionary mapping;
		private byte[] program;
		private EvalDelegate eval_del;
		private string[] namesMapping;
		private int gap;
	}

}

