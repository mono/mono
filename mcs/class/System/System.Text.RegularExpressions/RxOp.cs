
namespace System.Text.RegularExpressions {

	// for the IgnoreCase opcodes, the char data is stored lowercased
	// two-byte integers are in little endian format
	enum RxOp : byte {
		// followed by count, min, max integers
		Info,

		False,
		True,

		// position anchors 
		AnyPosition,
		StartOfString,
		StartOfLine,
		StartOfScan,
		EndOfString,
		EndOfLine,
		End,
		WordBoundary,
		NoWordBoundary,

		// latin1 strings
		// followed by single byte length and latin1 bytes
		// keep the order, see EmitString ()
		String,
		StringIgnoreCase,
		StringReverse,
		StringIgnoreCaseReverse,

		// followed by two byte length and unicode chars (two bytes per char)
		// a better setup may be to reference the chars in the patterns string
		// (offset, length) pairs, at least when the pattern contains them,
		// but this means we can't lowercase before hand: consider using a separate
		// string/array
		// keep the order, see EmitString ()
		UnicodeString,
		UnicodeStringIgnoreCase,
		UnicodeStringReverse,
		UnicodeStringIgnoreCaseReverse,

		// latin1 single char
		// followed by a latin1 byte
		// keep the order, see EmitCharacter ()
		Char,
		NoChar,
		CharIgnoreCase,
		NoCharIgnoreCase,
		CharReverse,
		NoCharReverse,
		CharIgnoreCaseReverse,
		NoCharIgnoreCaseReverse,

		// followed by latin1 min and max bytes
		// keep the order, see EmitRange ()
		Range,
		NoRange,
		RangeIgnoreCase,
		NoRangeIgnoreCase,
		RangeReverse,
		NoRangeReverse,
		RangeIgnoreCaseReverse,
		NoRangeIgnoreCaseReverse,

		// followed by lowbyte and length of the bitmap and by the bitmap
		// keep the order, see EmitSet ()
		Bitmap,
		NoBitmap,
		BitmapIgnoreCase,
		NoBitmapIgnoreCase,
		BitmapReverse,
		NoBitmapReverse,
		BitmapIgnoreCaseReverse,
		NoBitmapIgnoreCaseReverse,

		// unicode chars
		// followed by a unicode char
		// keep the order, see EmitCharacter ()
		UnicodeChar,
		NoUnicodeChar,
		UnicodeCharIgnoreCase,
		NoUnicodeCharIgnoreCase,
		UnicodeCharReverse,
		NoUnicodeCharReverse,
		UnicodeCharIgnoreCaseReverse,
		NoUnicodeCharIgnoreCaseReverse,

		// followed by unicode char min and max chars
		// keep the order, see EmitRange ()
		UnicodeRange,
		NoUnicodeRange,
		UnicodeRangeIgnoreCase,
		NoUnicodeRangeIgnoreCase,
		UnicodeRangeReverse,
		NoUnicodeRangeReverse,
		UnicodeRangeIgnoreCaseReverse,
		NoUnicodeRangeIgnoreCaseReverse,

		// followed by lowchar and length of the bitmap and by the bitmap
		UnicodeBitmap,
		NoUnicodeBitmap,
		UnicodeBitmapIgnoreCase,
		NoUnicodeBitmapIgnoreCase,
		UnicodeBitmapReverse,
		NoUnicodeBitmapReverse,
		UnicodeBitmapIgnoreCaseReverse,
		NoUnicodeBitmapIgnoreCaseReverse,

		// add reverse and negate versions of the categories
		CategoryAny,
		NoCategoryAny,
		CategoryAnyReverse,
		NoCategoryAnyReverse,
		CategoryAnySingleline,
		NoCategoryAnySingleline,
		CategoryAnySinglelineReverse,
		NoCategoryAnySinglelineReverse,
		CategoryDigit,
		NoCategoryDigit,
		CategoryDigitReverse,
		NoCategoryDigitReverse,
		CategoryWord,
		NoCategoryWord,
		CategoryWordReverse,
		NoCategoryWordReverse,
		CategoryWhiteSpace,
		NoCategoryWhiteSpace,
		CategoryWhiteSpaceReverse,
		NoCategoryWhiteSpaceReverse,
		CategoryEcmaWord,
		NoCategoryEcmaWord,
		CategoryEcmaWordReverse,
		NoCategoryEcmaWordReverse,
		CategoryEcmaWhiteSpace,
		NoCategoryEcmaWhiteSpace,
		CategoryEcmaWhiteSpaceReverse,
		NoCategoryEcmaWhiteSpaceReverse,

		// followed by a unicode category value (byte)
		CategoryUnicode,
		NoCategoryUnicode,
		CategoryUnicodeReverse,
		NoCategoryUnicodeReverse,

		CategoryUnicodeLetter,
		NoCategoryUnicodeLetter,
		CategoryUnicodeLetterReverse,
		NoCategoryUnicodeLetterReverse,
		CategoryUnicodeMark,
		NoCategoryUnicodeMark,
		CategoryUnicodeMarkReverse,
		NoCategoryUnicodeMarkReverse,
		CategoryUnicodeNumber,
		NoCategoryUnicodeNumber,
		CategoryUnicodeNumberReverse,
		NoCategoryUnicodeNumberReverse,
		CategoryUnicodeSeparator,
		NoCategoryUnicodeSeparator,
		CategoryUnicodeSeparatorReverse,
		NoCategoryUnicodeSeparatorReverse,
		CategoryUnicodePunctuation,
		NoCategoryUnicodePunctuation,
		CategoryUnicodePunctuationReverse,
		NoCategoryUnicodePunctuationReverse,
		CategoryUnicodeSymbol,
		NoCategoryUnicodeSymbol,
		CategoryUnicodeSymbolReverse,
		NoCategoryUnicodeSymbolReverse,
		CategoryUnicodeSpecials,
		NoCategoryUnicodeSpecials,
		CategoryUnicodeSpecialsReverse,
		NoCategoryUnicodeSpecialsReverse,
		CategoryUnicodeOther,
		NoCategoryUnicodeOther,
		CategoryUnicodeOtherReverse,
		NoCategoryUnicodeOtherReverse,
		// add more categories

		// followed by Category value (byte)
		CategoryGeneral,
		NoCategoryGeneral,
		CategoryGeneralReverse,
		NoCategoryGeneralReverse,

		// backreferences
		// followed by two-byte reference number
		// keep the order, see EmitReference ()
		Reference,
		ReferenceIgnoreCase,
		ReferenceReverse,
		ReferenceIgnoreCaseReverse,

		// group/capture support
		// followed by two-byte group id
		OpenGroup,
		CloseGroup,
		
		BalanceStart,
		Balance,

		// followed by offset and two-byte group id
		IfDefined,

		// skip ahead num bytes
		// followed by two-byte offset
		Jump,

		// followed by two-byte offset
		SubExpression,

		// followed by true and false two-byte offsets
		Test,

		// followed by two-byte offset
		Branch,

		// followed by two-byte offset
		TestCharGroup,

		// anchoring expression
		// followed by offset of tail and offset
		Anchor,
		AnchorReverse,

		// repetition support
		// followed by min, max ints
		Repeat,
		RepeatLazy,
		Until,
		FastRepeat,
		FastRepeatLazy,
		// followed by min byte
		RepeatInfinite,
		RepeatInfiniteLazy,
	}
}

