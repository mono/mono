
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
		// a better setup may be to have all the unicode chars in a separate
		// char array and reference them from here with (offset, length) pairs
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
		UnicodeNoRange,
		UnicodeRangeIgnoreCase,
		UnicodeNoRangeIgnoreCase,
		UnicodeRangeReverse,
		UnicodeNoRangeReverse,
		UnicodeRangeIgnoreCaseReverse,
		UnicodeNoRangeIgnoreCaseReverse,

		// followed by lowchar and length of the bitmap and by the bitmap
		UnicodeBitmap,
		UnicodeNoBitmap,
		UnicodeBitmapIgnoreCase,
		UnicodeNoBitmapIgnoreCase,
		UnicodeBitmapReverse,
		UnicodeNoBitmapReverse,
		UnicodeBitmapIgnoreCaseReverse,
		UnicodeNoBitmapIgnoreCaseReverse,

		// add reverse and negate versions of the categories
		CategoryAny,
		CategoryDigit,
		CategoryWord,
		CategoryWhiteSpace,
		CategoryEcmaWord,
		CategoryEcmaWhiteSpace,

		// followed by a unicode category value (byte)
		CategoryUnicode,
		// add more categories

		// backreferences
		// followed by two-byte reference number
		// keep the order, see EmitReference ()
		Reference,
		ReversenceIgnoreCase,
		ReferenceReverse,
		ReversenceIgnoreCaseReverse,

		// group/capture support
		// followed by two-byte group id
		OpenGroup,
		CloseGroup,

		// skip ahead num bytes
		// followed by two-byte offset
		Jump,

		// followed by two-byte offset
		Branch,

		// anchoring expression
		// followed by offset of tail and offset
		Anchor,
		AnchorReverse,

		// repetition support
		// followed by min, max ints
		Repeat,
		RepeatLazy,
		// followed by min byte
		RepeatInfinite,
		RepeatInfiniteLazy,
	}
}

