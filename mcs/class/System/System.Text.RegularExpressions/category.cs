//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	category.cs
//
// author:	Dan Lewis (dlewis@gmx.co.uk)
// 		(c) 2002

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
using System.Globalization;

namespace System.Text.RegularExpressions {

	enum Category : ushort {
		None,

		// canonical classes
	
		Any,			// any character except newline		.
		AnySingleline,		// any character			. (s option)
		Word,			// any word character			\w
		Digit,			// any digit character			\d
		WhiteSpace,		// any whitespace character		\s
		
		// ECMAScript classes


		EcmaAny,
		EcmaAnySingleline,
		EcmaWord,		// [a-zA-Z_0-9]
		EcmaDigit,		// [0-9]
		EcmaWhiteSpace,		// [ \f\n\r\t\v]

		// unicode categories
		
		UnicodeL,		// Letter
		UnicodeM,		// Mark
		UnicodeN,		// Number
		UnicodeZ,		// Separator
		UnicodeP,		// Punctuation
		UnicodeS,		// Symbol
		UnicodeC,		// Other

		UnicodeLu,		// UppercaseLetter
		UnicodeLl,		// LowercaseLetter
		UnicodeLt,		// TitlecaseLetter
		UnicodeLm,		// ModifierLetter
		UnicodeLo,		// OtherLetter
		UnicodeMn,		// NonspacingMark
		UnicodeMe,		// EnclosingMark
		UnicodeMc,		// SpacingMark
		UnicodeNd,		// DecimalNumber
		UnicodeNl,		// LetterNumber
		UnicodeNo,		// OtherNumber
		UnicodeZs,		// SpaceSeparator
		UnicodeZl,		// LineSeparator
		UnicodeZp,		// ParagraphSeparator
		UnicodePd,		// DashPunctuation
		UnicodePs,		// OpenPunctuation
		UnicodePi,		// InitialPunctuation
		UnicodePe,		// ClosePunctuation
		UnicodePf,		// FinalPunctuation
		UnicodePc,		// ConnectorPunctuation
		UnicodePo,		// OtherPunctuation
		UnicodeSm,		// MathSymbol
		UnicodeSc,		// CurrencySymbol
		UnicodeSk,		// ModifierSymbol
		UnicodeSo,		// OtherSymbol
		UnicodeCc,		// Control
		UnicodeCf,		// Format
		UnicodeCo,		// PrivateUse
		UnicodeCs,		// Surrogate
		UnicodeCn,		// Unassigned

		// unicode block ranges

		// notes: the categories marked with a star are valid unicode block ranges,
		// but don't seem to be accepted by the MS parser using the /p{...} format.
		// any ideas?

		UnicodeBasicLatin,
		UnicodeLatin1Supplement,			// *
		UnicodeLatinExtendedA,				// *
		UnicodeLatinExtendedB,				// *
		UnicodeIPAExtensions,
		UnicodeSpacingModifierLetters,
		UnicodeCombiningDiacriticalMarks,
		UnicodeGreek,
		UnicodeCyrillic,
		UnicodeArmenian,
		UnicodeHebrew,
		UnicodeArabic,
		UnicodeSyriac,
		UnicodeThaana,
		UnicodeDevanagari,
		UnicodeBengali,
		UnicodeGurmukhi,
		UnicodeGujarati,
		UnicodeOriya,
		UnicodeTamil,
		UnicodeTelugu,
		UnicodeKannada,
		UnicodeMalayalam,
		UnicodeSinhala,
		UnicodeThai,
		UnicodeLao,
		UnicodeTibetan,
		UnicodeMyanmar,
		UnicodeGeorgian,
		UnicodeHangulJamo,
		UnicodeEthiopic,
		UnicodeCherokee,
		UnicodeUnifiedCanadianAboriginalSyllabics,
		UnicodeOgham,
		UnicodeRunic,
		UnicodeKhmer,
		UnicodeMongolian,
		UnicodeLatinExtendedAdditional,
		UnicodeGreekExtended,
		UnicodeGeneralPunctuation,
		UnicodeSuperscriptsandSubscripts,
		UnicodeCurrencySymbols,
		UnicodeCombiningMarksforSymbols,
		UnicodeLetterlikeSymbols,
		UnicodeNumberForms,
		UnicodeArrows,
		UnicodeMathematicalOperators,
		UnicodeMiscellaneousTechnical,
		UnicodeControlPictures,
		UnicodeOpticalCharacterRecognition,
		UnicodeEnclosedAlphanumerics,
		UnicodeBoxDrawing,
		UnicodeBlockElements,
		UnicodeGeometricShapes,
		UnicodeMiscellaneousSymbols,
		UnicodeDingbats,
		UnicodeBraillePatterns,
		UnicodeCJKRadicalsSupplement,
		UnicodeKangxiRadicals,
		UnicodeIdeographicDescriptionCharacters,
		UnicodeCJKSymbolsandPunctuation,
		UnicodeHiragana,
		UnicodeKatakana,
		UnicodeBopomofo,
		UnicodeHangulCompatibilityJamo,
		UnicodeKanbun,
		UnicodeBopomofoExtended,
		UnicodeEnclosedCJKLettersandMonths,
		UnicodeCJKCompatibility,
		UnicodeCJKUnifiedIdeographsExtensionA,
		UnicodeCJKUnifiedIdeographs,
		UnicodeYiSyllables,
		UnicodeYiRadicals,
		UnicodeHangulSyllables,
		UnicodeHighSurrogates,
		UnicodeHighPrivateUseSurrogates,
		UnicodeLowSurrogates,
		UnicodePrivateUse,
		UnicodeCJKCompatibilityIdeographs,
		UnicodeAlphabeticPresentationForms,
		UnicodeArabicPresentationFormsA,		// *
		UnicodeCombiningHalfMarks,
		UnicodeCJKCompatibilityForms,
		UnicodeSmallFormVariants,
		UnicodeArabicPresentationFormsB,		// *
		UnicodeSpecials,
		UnicodeHalfwidthandFullwidthForms,
		
		UnicodeOldItalic,
		UnicodeGothic,
		UnicodeDeseret,
		UnicodeByzantineMusicalSymbols,
		UnicodeMusicalSymbols,
		UnicodeMathematicalAlphanumericSymbols,
		UnicodeCJKUnifiedIdeographsExtensionB,
		UnicodeCJKCompatibilityIdeographsSupplement,
		UnicodeTags,

		LastValue // Keep this with the higher value in the enumeration
	}

	class CategoryUtils {
		public static Category CategoryFromName (string name) {
			try {
				if (name.StartsWith ("Is"))	// remove prefix from block range
					name = name.Substring (2);

				return (Category)Enum.Parse (typeof (Category), "Unicode" + name);
			}
			catch (ArgumentException) {
				return Category.None;
			}
		}
	
		public static bool IsCategory (Category cat, char c) {
			switch (cat) {
			case Category.None:
				return false;
			
			case Category.Any:
				return c != '\n';

			case Category.AnySingleline:
				return true;

			case Category.Word:
				return
					Char.IsLetterOrDigit (c) ||
					IsCategory (UnicodeCategory.ConnectorPunctuation, c);

			case Category.Digit:
				return Char.IsDigit (c);

			case Category.WhiteSpace:
				return Char.IsWhiteSpace (c);

			// ECMA categories

			case Category.EcmaAny:
				return c != '\n';
				
			case Category.EcmaAnySingleline:
				return true;

			case Category.EcmaWord:
				return
					'a' <= c && c <= 'z' ||
					'A' <= c && c <= 'Z' ||
					'0' <= c && c <= '9' ||
					'_' == c;

			case Category.EcmaDigit:
				return
					'0' <= c && c <= '9';
			
			case Category.EcmaWhiteSpace:
				return
					c == ' '  ||
					c == '\f' ||
					c == '\n' ||
					c == '\r' ||
					c == '\t' ||
					c == '\v';

			// Unicode categories...

			// letter
			
			case Category.UnicodeLu: return IsCategory (UnicodeCategory.UppercaseLetter, c);
			case Category.UnicodeLl: return IsCategory (UnicodeCategory.LowercaseLetter, c);
			case Category.UnicodeLt: return IsCategory (UnicodeCategory.TitlecaseLetter, c);
			case Category.UnicodeLm: return IsCategory (UnicodeCategory.ModifierLetter, c);
			case Category.UnicodeLo: return IsCategory (UnicodeCategory.OtherLetter, c);

			// mark

			case Category.UnicodeMn: return IsCategory (UnicodeCategory.NonSpacingMark, c);
			case Category.UnicodeMe: return IsCategory (UnicodeCategory.EnclosingMark, c);
			case Category.UnicodeMc: return IsCategory (UnicodeCategory.SpacingCombiningMark, c);
			case Category.UnicodeNd: return IsCategory (UnicodeCategory.DecimalDigitNumber, c);

			// number

			case Category.UnicodeNl: return IsCategory (UnicodeCategory.LetterNumber, c);
			case Category.UnicodeNo: return IsCategory (UnicodeCategory.OtherNumber, c);

			// separator

			case Category.UnicodeZs: return IsCategory (UnicodeCategory.SpaceSeparator, c);
			case Category.UnicodeZl: return IsCategory (UnicodeCategory.LineSeparator, c);
			case Category.UnicodeZp: return IsCategory (UnicodeCategory.ParagraphSeparator, c);

			// punctuation

			case Category.UnicodePd: return IsCategory (UnicodeCategory.DashPunctuation, c);
			case Category.UnicodePs: return IsCategory (UnicodeCategory.OpenPunctuation, c);
			case Category.UnicodePi: return IsCategory (UnicodeCategory.InitialQuotePunctuation, c);
			case Category.UnicodePe: return IsCategory (UnicodeCategory.ClosePunctuation, c);
			case Category.UnicodePf: return IsCategory (UnicodeCategory.FinalQuotePunctuation, c);
			case Category.UnicodePc: return IsCategory (UnicodeCategory.ConnectorPunctuation, c);
			case Category.UnicodePo: return IsCategory (UnicodeCategory.OtherPunctuation, c);

			// symbol

			case Category.UnicodeSm: return IsCategory (UnicodeCategory.MathSymbol, c);
			case Category.UnicodeSc: return IsCategory (UnicodeCategory.CurrencySymbol, c);
			case Category.UnicodeSk: return IsCategory (UnicodeCategory.ModifierSymbol, c);
			case Category.UnicodeSo: return IsCategory (UnicodeCategory.OtherSymbol, c);

			// other

			case Category.UnicodeCc: return IsCategory (UnicodeCategory.Control, c);
			case Category.UnicodeCf: return IsCategory (UnicodeCategory.Format, c);
			case Category.UnicodeCo: return IsCategory (UnicodeCategory.PrivateUse, c);
			case Category.UnicodeCs: return IsCategory (UnicodeCategory.Surrogate, c);
			case Category.UnicodeCn: return IsCategory (UnicodeCategory.OtherNotAssigned, c); 

			case Category.UnicodeL:	// letter
				return
					IsCategory (UnicodeCategory.UppercaseLetter, c) ||
					IsCategory (UnicodeCategory.LowercaseLetter, c) ||
					IsCategory (UnicodeCategory.TitlecaseLetter, c) ||
					IsCategory (UnicodeCategory.ModifierLetter, c) ||
					IsCategory (UnicodeCategory.OtherLetter, c);
			
			case Category.UnicodeM:	// mark
				return
					IsCategory (UnicodeCategory.NonSpacingMark, c) ||
					IsCategory (UnicodeCategory.EnclosingMark, c) ||
					IsCategory (UnicodeCategory.SpacingCombiningMark, c);

			case Category.UnicodeN:	// number
				return
					IsCategory (UnicodeCategory.DecimalDigitNumber, c) ||
					IsCategory (UnicodeCategory.LetterNumber, c) ||
					IsCategory (UnicodeCategory.OtherNumber, c);

			case Category.UnicodeZ:	// separator
				return
					IsCategory (UnicodeCategory.SpaceSeparator, c) ||
					IsCategory (UnicodeCategory.LineSeparator, c) ||
					IsCategory (UnicodeCategory.ParagraphSeparator, c);
					
			case Category.UnicodeP:	// punctuation
				return
					IsCategory (UnicodeCategory.DashPunctuation, c) ||
					IsCategory (UnicodeCategory.OpenPunctuation, c) ||
					IsCategory (UnicodeCategory.InitialQuotePunctuation, c) ||
					IsCategory (UnicodeCategory.ClosePunctuation, c) ||
					IsCategory (UnicodeCategory.FinalQuotePunctuation, c) ||
					IsCategory (UnicodeCategory.ConnectorPunctuation, c) ||
					IsCategory (UnicodeCategory.OtherPunctuation, c);
			
			case Category.UnicodeS:	// symbol
				return
					IsCategory (UnicodeCategory.MathSymbol, c) ||
					IsCategory (UnicodeCategory.CurrencySymbol, c) ||
					IsCategory (UnicodeCategory.ModifierSymbol, c) ||
					IsCategory (UnicodeCategory.OtherSymbol, c);
			
			case Category.UnicodeC:	// other
				return
					IsCategory (UnicodeCategory.Control, c) ||
					IsCategory (UnicodeCategory.Format, c) ||
					IsCategory (UnicodeCategory.PrivateUse, c) ||
					IsCategory (UnicodeCategory.Surrogate, c) ||
					IsCategory (UnicodeCategory.OtherNotAssigned, c);

			// Unicode block ranges...

			case Category.UnicodeBasicLatin:
				return '\u0000' <= c && c <= '\u007F';

			case Category.UnicodeLatin1Supplement:
				return '\u0080' <= c && c <= '\u00FF';

			case Category.UnicodeLatinExtendedA:
				return '\u0100' <= c && c <= '\u017F';

			case Category.UnicodeLatinExtendedB:
				return '\u0180' <= c && c <= '\u024F';

			case Category.UnicodeIPAExtensions:
				return '\u0250' <= c && c <= '\u02AF';

			case Category.UnicodeSpacingModifierLetters:
				return '\u02B0' <= c && c <= '\u02FF';

			case Category.UnicodeCombiningDiacriticalMarks:
				return '\u0300' <= c && c <= '\u036F';

			case Category.UnicodeGreek:
				return '\u0370' <= c && c <= '\u03FF';

			case Category.UnicodeCyrillic:
				return '\u0400' <= c && c <= '\u04FF';

			case Category.UnicodeArmenian:
				return '\u0530' <= c && c <= '\u058F';

			case Category.UnicodeHebrew:
				return '\u0590' <= c && c <= '\u05FF';

			case Category.UnicodeArabic:
				return '\u0600' <= c && c <= '\u06FF';

			case Category.UnicodeSyriac:
				return '\u0700' <= c && c <= '\u074F';

			case Category.UnicodeThaana:
				return '\u0780' <= c && c <= '\u07BF';

			case Category.UnicodeDevanagari:
				return '\u0900' <= c && c <= '\u097F';

			case Category.UnicodeBengali:
				return '\u0980' <= c && c <= '\u09FF';

			case Category.UnicodeGurmukhi:
				return '\u0A00' <= c && c <= '\u0A7F';

			case Category.UnicodeGujarati:
				return '\u0A80' <= c && c <= '\u0AFF';

			case Category.UnicodeOriya:
				return '\u0B00' <= c && c <= '\u0B7F';

			case Category.UnicodeTamil:
				return '\u0B80' <= c && c <= '\u0BFF';

			case Category.UnicodeTelugu:
				return '\u0C00' <= c && c <= '\u0C7F';

			case Category.UnicodeKannada:
				return '\u0C80' <= c && c <= '\u0CFF';

			case Category.UnicodeMalayalam:
				return '\u0D00' <= c && c <= '\u0D7F';

			case Category.UnicodeSinhala:
				return '\u0D80' <= c && c <= '\u0DFF';

			case Category.UnicodeThai:
				return '\u0E00' <= c && c <= '\u0E7F';

			case Category.UnicodeLao:
				return '\u0E80' <= c && c <= '\u0EFF';

			case Category.UnicodeTibetan:
				return '\u0F00' <= c && c <= '\u0FFF';

			case Category.UnicodeMyanmar:
				return '\u1000' <= c && c <= '\u109F';

			case Category.UnicodeGeorgian:
				return '\u10A0' <= c && c <= '\u10FF';

			case Category.UnicodeHangulJamo:
				return '\u1100' <= c && c <= '\u11FF';

			case Category.UnicodeEthiopic:
				return '\u1200' <= c && c <= '\u137F';

			case Category.UnicodeCherokee:
				return '\u13A0' <= c && c <= '\u13FF';

			case Category.UnicodeUnifiedCanadianAboriginalSyllabics:
				return '\u1400' <= c && c <= '\u167F';

			case Category.UnicodeOgham:
				return '\u1680' <= c && c <= '\u169F';

			case Category.UnicodeRunic:
				return '\u16A0' <= c && c <= '\u16FF';

			case Category.UnicodeKhmer:
				return '\u1780' <= c && c <= '\u17FF';

			case Category.UnicodeMongolian:
				return '\u1800' <= c && c <= '\u18AF';

			case Category.UnicodeLatinExtendedAdditional:
				return '\u1E00' <= c && c <= '\u1EFF';

			case Category.UnicodeGreekExtended:
				return '\u1F00' <= c && c <= '\u1FFF';

			case Category.UnicodeGeneralPunctuation:
				return '\u2000' <= c && c <= '\u206F';

			case Category.UnicodeSuperscriptsandSubscripts:
				return '\u2070' <= c && c <= '\u209F';

			case Category.UnicodeCurrencySymbols:
				return '\u20A0' <= c && c <= '\u20CF';

			case Category.UnicodeCombiningMarksforSymbols:
				return '\u20D0' <= c && c <= '\u20FF';

			case Category.UnicodeLetterlikeSymbols:
				return '\u2100' <= c && c <= '\u214F';

			case Category.UnicodeNumberForms:
				return '\u2150' <= c && c <= '\u218F';

			case Category.UnicodeArrows:
				return '\u2190' <= c && c <= '\u21FF';

			case Category.UnicodeMathematicalOperators:
				return '\u2200' <= c && c <= '\u22FF';

			case Category.UnicodeMiscellaneousTechnical:
				return '\u2300' <= c && c <= '\u23FF';

			case Category.UnicodeControlPictures:
				return '\u2400' <= c && c <= '\u243F';

			case Category.UnicodeOpticalCharacterRecognition:
				return '\u2440' <= c && c <= '\u245F';

			case Category.UnicodeEnclosedAlphanumerics:
				return '\u2460' <= c && c <= '\u24FF';

			case Category.UnicodeBoxDrawing:
				return '\u2500' <= c && c <= '\u257F';

			case Category.UnicodeBlockElements:
				return '\u2580' <= c && c <= '\u259F';

			case Category.UnicodeGeometricShapes:
				return '\u25A0' <= c && c <= '\u25FF';

			case Category.UnicodeMiscellaneousSymbols:
				return '\u2600' <= c && c <= '\u26FF';

			case Category.UnicodeDingbats:
				return '\u2700' <= c && c <= '\u27BF';

			case Category.UnicodeBraillePatterns:
				return '\u2800' <= c && c <= '\u28FF';

			case Category.UnicodeCJKRadicalsSupplement:
				return '\u2E80' <= c && c <= '\u2EFF';

			case Category.UnicodeKangxiRadicals:
				return '\u2F00' <= c && c <= '\u2FDF';

			case Category.UnicodeIdeographicDescriptionCharacters:
				return '\u2FF0' <= c && c <= '\u2FFF';

			case Category.UnicodeCJKSymbolsandPunctuation:
				return '\u3000' <= c && c <= '\u303F';

			case Category.UnicodeHiragana:
				return '\u3040' <= c && c <= '\u309F';

			case Category.UnicodeKatakana:
				return '\u30A0' <= c && c <= '\u30FF';

			case Category.UnicodeBopomofo:
				return '\u3100' <= c && c <= '\u312F';

			case Category.UnicodeHangulCompatibilityJamo:
				return '\u3130' <= c && c <= '\u318F';

			case Category.UnicodeKanbun:
				return '\u3190' <= c && c <= '\u319F';

			case Category.UnicodeBopomofoExtended:
				return '\u31A0' <= c && c <= '\u31BF';

			case Category.UnicodeEnclosedCJKLettersandMonths:
				return '\u3200' <= c && c <= '\u32FF';

			case Category.UnicodeCJKCompatibility:
				return '\u3300' <= c && c <= '\u33FF';

			case Category.UnicodeCJKUnifiedIdeographsExtensionA:
				return '\u3400' <= c && c <= '\u4DB5';

			case Category.UnicodeCJKUnifiedIdeographs:
				return '\u4E00' <= c && c <= '\u9FFF';

			case Category.UnicodeYiSyllables:
				return '\uA000' <= c && c <= '\uA48F';

			case Category.UnicodeYiRadicals:
				return '\uA490' <= c && c <= '\uA4CF';

			case Category.UnicodeHangulSyllables:
				return '\uAC00' <= c && c <= '\uD7A3';

			case Category.UnicodeHighSurrogates:
				return '\uD800' <= c && c <= '\uDB7F';

			case Category.UnicodeHighPrivateUseSurrogates:
				return '\uDB80' <= c && c <= '\uDBFF';

			case Category.UnicodeLowSurrogates:
				return '\uDC00' <= c && c <= '\uDFFF';

			case Category.UnicodePrivateUse:
				return '\uE000' <= c && c <= '\uF8FF';

			case Category.UnicodeCJKCompatibilityIdeographs:
				return '\uF900' <= c && c <= '\uFAFF';

			case Category.UnicodeAlphabeticPresentationForms:
				return '\uFB00' <= c && c <= '\uFB4F';

			case Category.UnicodeArabicPresentationFormsA:
				return '\uFB50' <= c && c <= '\uFDFF';

			case Category.UnicodeCombiningHalfMarks:
				return '\uFE20' <= c && c <= '\uFE2F';

			case Category.UnicodeCJKCompatibilityForms:
				return '\uFE30' <= c && c <= '\uFE4F';

			case Category.UnicodeSmallFormVariants:
				return '\uFE50' <= c && c <= '\uFE6F';

			case Category.UnicodeArabicPresentationFormsB:
				return '\uFE70' <= c && c <= '\uFEFE';

			case Category.UnicodeHalfwidthandFullwidthForms:
				return '\uFF00' <= c && c <= '\uFFEF';

			case Category.UnicodeSpecials:
				return
					'\uFEFF' <= c && c <= '\uFEFF' ||
					'\uFFF0' <= c && c <= '\uFFFD';

			// these block ranges begin above 0x10000

			case Category.UnicodeOldItalic:
			case Category.UnicodeGothic:
			case Category.UnicodeDeseret:
			case Category.UnicodeByzantineMusicalSymbols:
			case Category.UnicodeMusicalSymbols:
			case Category.UnicodeMathematicalAlphanumericSymbols:
			case Category.UnicodeCJKUnifiedIdeographsExtensionB:
			case Category.UnicodeCJKCompatibilityIdeographsSupplement:
			case Category.UnicodeTags:
				return false;

			default:
				return false;
			}
		}

		private static bool IsCategory (UnicodeCategory uc, char c) {
			if (Char.GetUnicodeCategory (c) == uc)
				return true;

			return false;
		}
	}
}
