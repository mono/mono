using System;
using System.Text;

namespace I18N.CJK
{
	// FIXME:
	// find out what is the difference between 50220, 50221 and 50222.

	public class CP50220 : ISO2022JPEncoding
	{
		public CP50220 ()
			: base (true, true)
		{
		}

		public override int CodePage {
			get { return 50220; }
		}

		public override string EncodingName {
			get { return "Japanese (JIS)"; }
		}
	}

	public class CP50221 : ISO2022JPEncoding
	{
		public CP50221 ()
			: base (false, true)
		{
		}

		public override int CodePage {
			get { return 50221; }
		}

		public override string EncodingName {
			get { return "Japanese (JIS-Allow 1 byte Kana)"; }
		}
	}

	public class CP50222 : ISO2022JPEncoding
	{
		public CP50222 ()
			: base (true, true)
		{
		}

		public override int CodePage {
			get { return 50222; }
		}

		public override string EncodingName {
			get { return "Japanese (JIS-Allow 1 byte Kana - SO/SI)"; }
		}
	}

	public class ISO2022JPEncoding : Encoding
	{
		static JISConvert convert = JISConvert.Convert;

		public ISO2022JPEncoding (bool allow1ByteKana, bool allowShiftIO)
		{
			this.allow_1byte_kana = allow1ByteKana;
			this.allow_shift_io = allowShiftIO;
		}

		readonly bool allow_1byte_kana, allow_shift_io;

		public override string BodyName {
			get { return "iso-2022-jp"; }
		}

		public override string HeaderName {
			get { return "iso-2022-jp"; }
		}

		public override string WebName {
			get { return "csISO2022JP"; }
		}

		public override int GetMaxByteCount (int charCount)
		{
			// ESC w ESC s ESC w ... (even number) ESC s
			return charCount / 2 * 5 + 4;
		}

		public override int GetMaxCharCount (int byteCount)
		{
			// no escape sequence
			return byteCount;
		}

		public override int GetByteCount (char [] chars, int charIndex, int charCount)
		{
			return new ISO2022JPEncoder (allow_1byte_kana, allow_shift_io).GetByteCount (chars, charIndex, charCount, true);
		}

		public override int GetBytes (char [] chars, int charIndex, int charCount, byte [] bytes, int byteIndex)
		{
			return new ISO2022JPEncoder (allow_1byte_kana, allow_shift_io).GetBytes (chars, charIndex, charCount, bytes, byteIndex, true);
		}

		public override int GetCharCount (byte [] bytes, int index, int count)
		{
			return new ISO2022JPDecoder (allow_1byte_kana, allow_shift_io).GetCharCount (bytes, index, count);
		}

		public override int GetChars (byte [] bytes, int byteIndex, int byteCount, char [] chars, int charIndex)
		{
			return new ISO2022JPDecoder (allow_1byte_kana, allow_shift_io).GetChars (bytes, byteIndex, byteCount, chars, charIndex);
		}
	}

	internal enum ISO2022JPMode {
		ASCII,
		JISX0208,
		JISX0201
	}

	internal class ISO2022JPEncoder : Encoder
	{
		static JISConvert convert = JISConvert.Convert;

		readonly bool allow_1byte_kana, allow_shift_io;

		ISO2022JPMode m = ISO2022JPMode.ASCII;

		public ISO2022JPEncoder (bool allow1ByteKana, bool allowShiftIO)
		{
			this.allow_1byte_kana = allow1ByteKana;
			this.allow_shift_io = allowShiftIO;
		}

		public override int GetByteCount (char [] chars, int charIndex, int charCount, bool flush)
		{
			int end = charIndex + charCount;
			int value;
			int byteCount = 0;

			for (int i = charIndex; i < end; i++) {
				char ch = chars [i];
				if (ch >= 0x2010 && ch <= 0x9FA5)
				{
					if (m != ISO2022JPMode.JISX0208)
						byteCount += 3;
					m = ISO2022JPMode.JISX0208;
					// This range contains the bulk of the CJK set.
					value = (ch - 0x2010) * 2;
					value = ((int)(convert.cjkToJis[value])) |
							(((int)(convert.cjkToJis[value + 1])) << 8);
				} else if (ch >= 0xFF01 && ch <= 0xFF60) {
					if (m != ISO2022JPMode.JISX0208)
						byteCount += 3;
					m = ISO2022JPMode.JISX0208;

					// This range contains extra characters,
					value = (ch - 0xFF01) * 2;
					value = ((int)(convert.extraToJis[value])) |
							(((int)(convert.extraToJis[value + 1])) << 8);
				} else if(ch >= 0xFF60 && ch <= 0xFFA0) {
					if (m != ISO2022JPMode.JISX0201)
						byteCount += 3;
					m = ISO2022JPMode.JISX0201;
					value = ch - 0xFF60 + 0xA0;
				} else if (ch < 128) {
					if (m != ISO2022JPMode.ASCII)
						byteCount += 3;
					m = ISO2022JPMode.ASCII;
					value = (int) ch;
				} else
					// skip non-convertible character
					continue;

				if (value > 0x100)
					byteCount += 2;
				else
					byteCount++;
			}
			// must end in ASCII mode
			if (flush && m != ISO2022JPMode.ASCII) {
				byteCount += 3;
				m = ISO2022JPMode.ASCII;
			}
			return byteCount;
		}

		// returns false if it failed to add required ESC.
		private bool SwitchMode (byte [] bytes, ref int byteIndex,
			ISO2022JPMode cur, ISO2022JPMode next)
		{
			if (cur == next)
				return true;
			if (bytes.Length <= byteIndex + 3)
				return false;
			bytes [byteIndex++] = 0x1B;
			bytes [byteIndex++] = (byte) (next == ISO2022JPMode.JISX0208 ? 0x24 : 0x28);
			bytes [byteIndex++] = (byte) (next == ISO2022JPMode.JISX0201 ? 0x49 : 0x42);
			return true;
		}

		public override int GetBytes (char [] chars, int charIndex, int charCount, byte [] bytes, int byteIndex, bool flush)
		{
			bool wide = false;
			int start = byteIndex;

			int end = charIndex + charCount;
			int value;

			for (int i = charIndex; i < end &&
				byteIndex < bytes.Length + (wide ? 1 : 0); i++) {
				char ch = chars [i];
				if (ch >= 0x2010 && ch <= 0x9FA5)
				{
					if (!SwitchMode (bytes, ref byteIndex, m, ISO2022JPMode.JISX0208))
						break;
					m = ISO2022JPMode.JISX0208;
					// This range contains the bulk of the CJK set.
					value = (ch - 0x2010) * 2;
					value = ((int)(convert.cjkToJis[value])) |
							(((int)(convert.cjkToJis[value + 1])) << 8);
				} else if (ch >= 0xFF01 && ch <= 0xFF60) {
					if (!SwitchMode (bytes, ref byteIndex, m, ISO2022JPMode.JISX0208))
						break;
					m = ISO2022JPMode.JISX0208;

					// This range contains extra characters,
					value = (ch - 0xFF01) * 2;
					value = ((int)(convert.extraToJis[value])) |
							(((int)(convert.extraToJis[value + 1])) << 8);
				} else if(ch >= 0xFF60 && ch <= 0xFFA0) {
					if (!SwitchMode (bytes, ref byteIndex, m, ISO2022JPMode.JISX0201))
						break;
					m = ISO2022JPMode.JISX0201;
					value = ch - 0xFF60 + 0xA0;
				} else if (ch < 128) {
					if (!SwitchMode (bytes, ref byteIndex, m, ISO2022JPMode.ASCII))
						break;
					m = ISO2022JPMode.ASCII;
					value = (int) ch;
				} else
					// skip non-convertible character
					continue;

//Console.WriteLine ("{0:X04} : {1:x02} {2:x02}", v, (int) v / 94 + 33, v % 94 + 33);
				if (value > 0x100) {
					value -= 0x0100;
					bytes [byteIndex++] = (byte) (value / 94 + 33);
					bytes [byteIndex++] = (byte) (value % 94 + 33);
				}
				else
					bytes [byteIndex++] = (byte) value;
			}
			if (flush) {
				// must end in ASCII mode
				SwitchMode (bytes, ref byteIndex, m, ISO2022JPMode.ASCII);
				m = ISO2022JPMode.ASCII;
			}
			return byteIndex - start;
		}
	}

	internal class ISO2022JPDecoder : Decoder
	{
		static JISConvert convert = JISConvert.Convert;

		readonly bool allow_1byte_kana, allow_shift_io;

		public ISO2022JPDecoder (bool allow1ByteKana, bool allowShiftIO)
		{
			this.allow_1byte_kana = allow1ByteKana;
			this.allow_shift_io = allowShiftIO;
		}

		// GetCharCount
		public override int GetCharCount (byte [] bytes, int index, int count)
		{
			int ret = 0;

			int end = index + count;
			for (int i = index; i < end; i++) {
				if (bytes [i] != 0x1B) {
					ret++;
					continue;
				} else {
					if (i + 2 >= end)
						break; // incomplete escape sequence
					i++;
					if (bytes [i] != 0x24 &&
						bytes [i] != 0x28)
						throw new ArgumentException ("Unexpected ISO-2022-JP escape sequence.");
					i++;
					if (bytes [i] != 0x42)
						throw new ArgumentException ("Unexpected ISO-2022-JP escape sequence.");
				}
			}

			return ret;
		}

		private char ToChar (int value)
		{
			value <<= 1;
			return value >= convert.jisx0208ToUnicode.Length ? '?' :
				(char) (((int) (convert.jisx0208ToUnicode [value])) |
					(((int) (convert.jisx0208ToUnicode [value + 1])) << 8));
		}

		public override int GetChars (byte [] bytes, int byteIndex, int byteCount, char [] chars, int charIndex)
		{
			ISO2022JPMode m = ISO2022JPMode.ASCII;
			int start = charIndex;
			int end = byteIndex + byteCount;
			for (int i = byteIndex; i < end && charIndex < chars.Length; i++) {
				if (bytes [i] != 0x1B) {
					if (m == ISO2022JPMode.JISX0208) {
						if (i + 1 == end)
							break; // incomplete head of wide char

						// am so lazy, so reusing jis2sjis and 
						int s1 = ((bytes [i] - 1) >> 1) + ((bytes [i] <= 0x5e) ? 0x71 : 0xb1);
						int s2 = bytes [i + 1] + (((bytes [i] & 1) != 0) ? 0x20 : 0x7e);
						int v = (s1 - 0x81) * 0xBC;
						v += s2 - 0x41;

						chars [charIndex++] = ToChar (v);
						i++;
					}
					else if (m == ISO2022JPMode.JISX0201)
						chars [charIndex++] = (char) (bytes [i] + 0xFF40);
					// LAMESPEC: actually this should not
					// be allowed when 1byte-kana is not
					// allowed, but MS.NET seems to allow
					// it in any mode.
					else if (bytes [i] > 0xA0 && bytes [i] < 0xE0) // half-width Katakana
						chars [charIndex++] = (char) (bytes [i] - 0xA0 + 0xFF60);
					else
						chars [charIndex++] = (char) bytes [i];
					continue;
				} else {
					if (i + 2 >= end)
						break; // incomplete escape sequence
					i++;
					bool wide = false;
					if (bytes [i] == 0x24)
						wide = true;
					else if (bytes [i] == 0x28)
						wide = false;
					else
						throw new ArgumentException ("Unexpected ISO-2022-JP escape sequence.");
					i++;
					if (bytes [i] == 0x42)
						m = wide ? ISO2022JPMode.JISX0208 : ISO2022JPMode.ASCII;
					else if (bytes [i] == 0x49)
						m = ISO2022JPMode.JISX0201;
					else
						throw new ArgumentException (String.Format ("Unexpected ISO-2022-JP escape sequence. Ended with 0x{0:X04}", bytes [i]));
				}
			}

			return charIndex - start;
		}
	}

	public class ENCiso_2022_jp : CP50220
	{
		public ENCiso_2022_jp () : base() {}

	}; // class ENCiso_2022_jp
}
