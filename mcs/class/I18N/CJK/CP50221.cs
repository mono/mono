using System;
using System.Text;

namespace I18N.CJK
{
	public class CP50221 : Encoding
	{
		static JISConvert convert = JISConvert.Convert;

		public override string BodyName {
			get { return "iso-2022-jp"; }
		}

		public override string HeaderName {
			get { return "iso-2022-jp"; }
		}

		public override string WebName {
			get { return "csISO2022JP"; }
		}

		public override string EncodingName {
			get { return "Japanese (JIS-Allow 1 byte Kana)"; }
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
			return new CP50221Encoder ().GetByteCount (chars, charIndex, charCount, true);
		}

		public override int GetBytes (char [] chars, int charIndex, int charCount, byte [] bytes, int byteIndex)
		{
			return new CP50221Encoder ().GetBytes (chars, charIndex, charCount, bytes, byteIndex, true);
		}

		public override int GetCharCount (byte [] bytes, int index, int count)
		{
			return new CP50221Decoder ().GetCharCount (bytes, index, count);
		}

		public override int GetChars (byte [] bytes, int byteIndex, int byteCount, char [] chars, int charIndex)
		{
			return new CP50221Decoder ().GetChars (bytes, byteIndex, byteCount, chars, charIndex);
		}
	}

	internal enum CP50221Mode {
		ASCII,
		JISX0208,
		JISX0201
	}

	internal class CP50221Encoder : Encoder
	{
		static JISConvert convert = JISConvert.Convert;

		CP50221Mode m = CP50221Mode.ASCII;

		public override int GetByteCount (char [] chars, int charIndex, int charCount, bool flush)
		{
			int end = charIndex + charCount;
			int value;
			int byteCount = 0;

			for (int i = charIndex; i < end; i++) {
				char ch = chars [i];
				if (ch >= 0x2010 && ch <= 0x9FA5)
				{
					if (m != CP50221Mode.JISX0208)
						byteCount += 3;
					m = CP50221Mode.JISX0208;
					// This range contains the bulk of the CJK set.
					value = (ch - 0x2010) * 2;
					value = ((int)(convert.cjkToJis[value])) |
							(((int)(convert.cjkToJis[value + 1])) << 8);
				} else if (ch >= 0xFF01 && ch <= 0xFF60) {
					if (m != CP50221Mode.JISX0208)
						byteCount += 3;
					m = CP50221Mode.JISX0208;

					// This range contains extra characters,
					value = (ch - 0xFF01) * 2;
					value = ((int)(convert.extraToJis[value])) |
							(((int)(convert.extraToJis[value + 1])) << 8);
				} else if(ch >= 0xFF60 && ch <= 0xFFA0) {
					if (m != CP50221Mode.JISX0201)
						byteCount += 3;
					m = CP50221Mode.JISX0201;
					value = ch - 0xFF60 + 0xA0;
				} else if (ch < 128) {
					if (m != CP50221Mode.ASCII)
						byteCount += 3;
					m = CP50221Mode.ASCII;
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
			if (flush && m != CP50221Mode.ASCII) {
				byteCount += 3;
				m = CP50221Mode.ASCII;
			}
			return byteCount;
		}

		// returns false if it failed to add required ESC.
		private bool SwitchMode (byte [] bytes, ref int byteIndex,
			CP50221Mode cur, CP50221Mode next)
		{
			if (cur == next)
				return true;
			if (bytes.Length <= byteIndex + 3)
				return false;
			bytes [byteIndex++] = 0x1B;
			bytes [byteIndex++] = (byte) (next == CP50221Mode.JISX0208 ? 0x24 : 0x28);
			bytes [byteIndex++] = (byte) (next == CP50221Mode.JISX0201 ? 0x49 : 0x42);
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
					if (!SwitchMode (bytes, ref byteIndex, m, CP50221Mode.JISX0208))
						break;
					m = CP50221Mode.JISX0208;
					// This range contains the bulk of the CJK set.
					value = (ch - 0x2010) * 2;
					value = ((int)(convert.cjkToJis[value])) |
							(((int)(convert.cjkToJis[value + 1])) << 8);
				} else if (ch >= 0xFF01 && ch <= 0xFF60) {
					if (!SwitchMode (bytes, ref byteIndex, m, CP50221Mode.JISX0208))
						break;
					m = CP50221Mode.JISX0208;

					// This range contains extra characters,
					value = (ch - 0xFF01) * 2;
					value = ((int)(convert.extraToJis[value])) |
							(((int)(convert.extraToJis[value + 1])) << 8);
				} else if(ch >= 0xFF60 && ch <= 0xFFA0) {
					if (!SwitchMode (bytes, ref byteIndex, m, CP50221Mode.JISX0201))
						break;
					m = CP50221Mode.JISX0201;
					value = ch - 0xFF60 + 0xA0;
				} else if (ch < 128) {
					if (!SwitchMode (bytes, ref byteIndex, m, CP50221Mode.ASCII))
						break;
					m = CP50221Mode.ASCII;
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
				SwitchMode (bytes, ref byteIndex, m, CP50221Mode.ASCII);
				m = CP50221Mode.ASCII;
			}
			return byteIndex - start;
		}
	}

	internal class CP50221Decoder : Decoder
	{
		static JISConvert convert = JISConvert.Convert;

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
			CP50221Mode m = CP50221Mode.ASCII;
			int start = charIndex;
			int end = byteIndex + byteCount;
			for (int i = byteIndex; i < end && charIndex < chars.Length; i++) {
				if (bytes [i] != 0x1B) {
					if (m == CP50221Mode.JISX0208) {
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
					else if (m == CP50221Mode.JISX0201)
						chars [charIndex++] = (char) (bytes [i] + 0xFF40);
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
						m = wide ? CP50221Mode.JISX0208 : CP50221Mode.ASCII;
					else if (bytes [i] == 0x49)
						m = CP50221Mode.JISX0201;
					else
						throw new ArgumentException (String.Format ("Unexpected ISO-2022-JP escape sequence. Ended with 0x{0:X04}", bytes [i]));
				}
			}

			return charIndex - start;
		}
	}

	public class ENCiso_2022_jp : CP50221
	{
		public ENCiso_2022_jp () : base() {}

	}; // class ENCiso_2022_jp
}
