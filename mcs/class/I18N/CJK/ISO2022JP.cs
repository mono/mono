//
// ISO2022JP.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
using System;
using System.Text;
using I18N.Common;

#if DISABLE_UNSAFE
using MonoEncoder = I18N.Common.MonoSafeEncoder;
using MonoEncoding = I18N.Common.MonoSafeEncoding;
#endif

namespace I18N.CJK
{
	[Serializable]
	public class CP50220 : ISO2022JPEncoding
	{
		public CP50220 ()
			: base (50220, false, false)
		{
		}

		public override string EncodingName {
			get { return "Japanese (JIS)"; }
		}
	}

	[Serializable]
	public class CP50221 : ISO2022JPEncoding
	{
		public CP50221 ()
			: base (50221, true, false)
		{
		}

		public override string EncodingName {
			get { return "Japanese (JIS-Allow 1 byte Kana)"; }
		}
	}

	[Serializable]
	public class CP50222 : ISO2022JPEncoding
	{
		public CP50222 ()
			: base (50222, true, true)
		{
		}

		public override string EncodingName {
			get { return "Japanese (JIS-Allow 1 byte Kana - SO/SI)"; }
		}
	}

	[Serializable]
	public class ISO2022JPEncoding : MonoEncoding
	{
		public ISO2022JPEncoding (int codePage, bool allow1ByteKana, bool allowShiftIO)
			: base (codePage, 932)
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

#if !DISABLE_UNSAFE
		protected override unsafe int GetBytesInternal(char* chars, int charCount, byte* bytes, int byteCount, bool flush, object state)
		{
			if (state != null)
				return ((ISO2022JPEncoder)state).GetBytesImpl (chars, charCount, bytes, byteCount, true);

			return new ISO2022JPEncoder (this, allow_1byte_kana, allow_shift_io).GetBytesImpl (chars, charCount, bytes, byteCount, true);
		}

		public unsafe override int GetByteCountImpl (char* chars, int count)
		{
			return new ISO2022JPEncoder (this, allow_1byte_kana, allow_shift_io).GetByteCountImpl (chars, count, true);
		}

		public unsafe override int GetBytesImpl (char* chars, int charCount, byte* bytes, int byteCount)
		{
			return new ISO2022JPEncoder (this, allow_1byte_kana, allow_shift_io).GetBytesImpl (chars, charCount, bytes, byteCount, true);
		}
#else
		protected override int GetBytesInternal(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush, object state)
		{
			if (state != null)
				return ((ISO2022JPEncoder)state).GetBytesInternal(chars, charIndex, charCount, bytes, byteIndex, true);

			return new ISO2022JPEncoder(this, allow_1byte_kana, allow_shift_io).GetBytesInternal(chars, charIndex, charCount, bytes, byteIndex, true);
		}

		public override int GetByteCount(char[] chars, int charIndex, int charCount)
		{
			return new ISO2022JPEncoder(this, allow_1byte_kana, allow_shift_io).GetByteCount(chars, charIndex, charCount, true);
		}

		public override int  GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			return new ISO2022JPEncoder (this, allow_1byte_kana, allow_shift_io).GetBytes(chars, charIndex, charCount, bytes, byteIndex, true);
		}
#endif

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

	internal class ISO2022JPEncoder : MonoEncoder
	{
		static JISConvert convert = JISConvert.Convert;

		readonly bool allow_1byte_kana, allow_shift_io;

		ISO2022JPMode m = ISO2022JPMode.ASCII;
		bool shifted_in_count, shifted_in_conv;

		public ISO2022JPEncoder(MonoEncoding owner, bool allow1ByteKana, bool allowShiftIO)
			: base (owner)
		{
			this.allow_1byte_kana = allow1ByteKana;
			this.allow_shift_io = allowShiftIO;
		}

#if !DISABLE_UNSAFE
		public unsafe override int GetByteCountImpl (char* chars, int charCount, bool flush)
		{
			return GetBytesImpl(chars, charCount, null, 0, flush);
		}
#else
		public override int GetByteCount(char[] chars, int charIndex, int charCount, bool flush)
		{
			return GetBytesInternal (chars, charIndex, charCount, null, 0, true);
		}
#endif

#if !DISABLE_UNSAFE
		private unsafe bool IsShifted(byte *bytes)
		{
			return bytes == null ? shifted_in_count : shifted_in_conv;
		}

		private unsafe void SetShifted(byte *bytes, bool state)
		{
			if (bytes == null)
				shifted_in_count = state;
			else
				shifted_in_conv = state;
		}

		// returns false if it failed to add required ESC.
		private unsafe void SwitchMode (byte* bytes, ref int byteIndex,
			ref int byteCount, ref ISO2022JPMode cur, ISO2022JPMode next)
		{
			if (cur == next)
				return;

			// If bytes == null we are just counting chars..
			if (bytes == null) {
				byteIndex += 3;
				cur = next;
				return;
			}

			if (byteCount <= 3)
				throw new ArgumentOutOfRangeException ("Insufficient byte buffer.");

			bytes [byteIndex++] = 0x1B;
			switch (next) {
			case ISO2022JPMode.JISX0201:
				bytes [byteIndex++] = 0x28;
				bytes [byteIndex++] = 0x49;
				break;
			case ISO2022JPMode.JISX0208:
				bytes [byteIndex++] = 0x24;
				bytes [byteIndex++] = 0x42;
				break;
			default:
				bytes [byteIndex++] = 0x28;
				bytes [byteIndex++] = 0x42;
				break;
			}
			cur = next;
		}
#else
		private bool IsShifted(byte[] bytes)
		{
			return bytes == null ? shifted_in_count : shifted_in_conv;
		}

		private void SetShifted(byte[] bytes, bool state)
		{
			if (bytes == null)
				shifted_in_count = state;
			else
				shifted_in_conv = state;
		}

		private void SwitchMode(byte[] bytes, ref int byteIndex,
			ref int byteCount, ref ISO2022JPMode cur, ISO2022JPMode next)
		{
			if (cur == next)
				return;

			// If bytes == null we are just counting chars..
			if (bytes == null)
			{
				byteIndex += 3;
				cur = next;
				return;
			}

			if (byteCount <= 3)
				throw new ArgumentOutOfRangeException("Insufficient byte buffer.");

			bytes[byteIndex++] = 0x1B;
			switch (next)
			{
				case ISO2022JPMode.JISX0201:
					bytes[byteIndex++] = 0x28;
					bytes[byteIndex++] = 0x49;
					break;
				case ISO2022JPMode.JISX0208:
					bytes[byteIndex++] = 0x24;
					bytes[byteIndex++] = 0x42;
					break;
				default:
					bytes[byteIndex++] = 0x28;
					bytes[byteIndex++] = 0x42;
					break;
			}

			cur = next;
		}
#endif

		static readonly char [] full_width_map = new char [] {
			'\0', '\u3002', '\u300C', '\u300D', '\u3001', '\u30FB', // to nakaguro
			'\u30F2', '\u30A1', '\u30A3', '\u30A5', '\u30A7', '\u30A9', '\u30E3', '\u30E5', '\u30E7', '\u30C3', // to small tsu
			'\u30FC', '\u30A2', '\u30A4', '\u30A6', '\u30A8', '\u30AA', // A-O
			'\u30AB', '\u30AD', '\u30AF', '\u30B1', '\u30B3',
			'\u30B5', '\u30B7', '\u30B9', '\u30BB', '\u30BD',
			'\u30BF', '\u30C1', '\u30C4', '\u30C6', '\u30C8',
			'\u30CA', '\u30CB', '\u30CC', '\u30CD', '\u30CE',
			'\u30CF', '\u30D2', '\u30D5', '\u30D8', '\u30DB',
			'\u30DE', '\u30DF', '\u30E0', '\u30E1', '\u30E2',
			'\u30E4', '\u30E6', '\u30E8', // Ya-Yo
			'\u30E9', '\u30EA', '\u30EB', '\u30EC', '\u30ED',
			'\u30EF', '\u30F3', '\u309B', '\u309C' };

#if !DISABLE_UNSAFE
		public unsafe override int GetBytesImpl (
			char* chars, int charCount,
			byte* bytes, int byteCount, bool flush)
		{
			int charIndex = 0;
			int byteIndex = 0;

			int start = byteIndex;
			int end = charIndex + charCount;
			int value;

			for (int i = charIndex; i < end; i++, charCount--) {
				char ch = chars [i];

				// When half-kana is not allowed and it is
				// actually in the input, convert to full width
				// kana.
				if (!allow_1byte_kana &&
					ch >= 0xFF60 && ch <= 0xFFA0)
					ch = full_width_map [ch - 0xFF60];

				if (ch >= 0x2010 && ch <= 0x9FA5)
				{
					if (IsShifted(bytes)) {
						var offset = byteIndex++;
						if (bytes != null) bytes [offset] = 0x0F;
						SetShifted(bytes, false);
						byteCount--;
					}
					switch (m) {
					case ISO2022JPMode.JISX0208:
						break;
					default:
						SwitchMode (bytes, ref byteIndex, ref byteCount, ref m, ISO2022JPMode.JISX0208);
						break;
					}
					// This range contains the bulk of the CJK set.
					value = (ch - 0x2010) * 2;
					value = ((int)(convert.cjkToJis[value])) |
							(((int)(convert.cjkToJis[value + 1])) << 8);
				} else if (ch >= 0xFF01 && ch <= 0xFF60) {
					if (IsShifted(bytes)) {
						var offset = byteIndex++;
						if (bytes != null) bytes [offset] = 0x0F;
						SetShifted(bytes, false);
						byteCount--;
					}
					switch (m) {
					case ISO2022JPMode.JISX0208:
						break;
					default:
						SwitchMode (bytes, ref byteIndex, ref byteCount, ref m, ISO2022JPMode.JISX0208);
						break;
					}

					// This range contains extra characters,
					value = (ch - 0xFF01) * 2;
					value = ((int)(convert.extraToJis[value])) |
							(((int)(convert.extraToJis[value + 1])) << 8);
				} else if (ch >= 0xFF60 && ch <= 0xFFA0) {
					// disallowed half-width kana is
					// already converted to full-width kana
					// so here we don't have to consider it.

					if (allow_shift_io) {
						if (!IsShifted(bytes)) {
							var offset = byteIndex++;
							if (bytes != null) bytes [offset] = 0x0E;
							SetShifted(bytes, true);
							byteCount--;
						}
					} else {
						switch (m) {
						case ISO2022JPMode.JISX0201:
							break;
						default:
							SwitchMode (bytes, ref byteIndex, ref byteCount, ref m, ISO2022JPMode.JISX0201);
							break;
						}
					}
					value = ch - 0xFF40;
				} else if (ch < 128) {
					if (IsShifted(bytes)) {
						var offset = byteIndex++;
						if (bytes != null) bytes [offset] = 0x0F;
						SetShifted(bytes, false);
						byteCount--;
					}
					SwitchMode (bytes, ref byteIndex, ref byteCount, ref m, ISO2022JPMode.ASCII);
					value = (int) ch;
				} else {
					HandleFallback (
						chars, ref i, ref charCount,
						bytes, ref byteIndex, ref byteCount, this);
					// skip non-convertible character
					continue;
				}

//Console.WriteLine ("{0:X04} : {1:x02} {2:x02}", v, (int) v / 94 + 33, v % 94 + 33);
				if (value >= 0x100) {
					value -= 0x0100;
					if (bytes != null) {
						bytes [byteIndex++] = (byte) (value / 94 + 33);
						bytes [byteIndex++] = (byte) (value % 94 + 33);
					} else {
						byteIndex += 2;
					}
					byteCount -= 2;
				}
				else {
					var offset = byteIndex++;
					if (bytes != null) bytes [offset] = (byte) value;
					byteCount--;
				}
			}
			if (flush) {
				// must end in ASCII mode
				if (IsShifted(bytes)) {
					var offset = byteIndex++;
					if (bytes != null) bytes [offset] = 0x0F;
					SetShifted(bytes, false);
					byteCount--;
				}
				if (m != ISO2022JPMode.ASCII)
					SwitchMode (bytes, ref byteIndex, ref byteCount, ref m, ISO2022JPMode.ASCII);
			}
			return byteIndex - start;
		}
#else
		internal int GetBytesInternal(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush)
		{
			int start = byteIndex;
			int end = charIndex + charCount;
			int value;
			int byteCount = bytes != null ? bytes.Length : 0;

			for (int i = charIndex; i < end; i++, charCount--)
			{
				char ch = chars[i];

				// When half-kana is not allowed and it is
				// actually in the input, convert to full width
				// kana.
				if (!allow_1byte_kana &&
					ch >= 0xFF60 && ch <= 0xFFA0)
					ch = full_width_map[ch - 0xFF60];

				if (ch >= 0x2010 && ch <= 0x9FA5)
				{
					if (IsShifted (bytes))
					{
						var offset = byteIndex++;
						if (bytes != null) bytes[offset] = 0x0F;
						SetShifted (bytes, false);
						byteCount--;
					}
					switch (m)
					{
						case ISO2022JPMode.JISX0208:
							break;
						default:
							SwitchMode(bytes, ref byteIndex, ref byteCount, ref m, ISO2022JPMode.JISX0208);
							break;
					}
					// This range contains the bulk of the CJK set.
					value = (ch - 0x2010) * 2;
					value = ((int)(convert.cjkToJis[value])) |
							(((int)(convert.cjkToJis[value + 1])) << 8);
				}
				else if (ch >= 0xFF01 && ch <= 0xFF60)
				{
					if (IsShifted(bytes))
					{
						var offset = byteIndex++;
						if (bytes != null) bytes[offset] = 0x0F;
						SetShifted (bytes, false);
						byteCount--;
					}
					switch (m)
					{
						case ISO2022JPMode.JISX0208:
							break;
						default:
							SwitchMode(bytes, ref byteIndex, ref byteCount, ref m, ISO2022JPMode.JISX0208);
							break;
					}

					// This range contains extra characters,
					value = (ch - 0xFF01) * 2;
					value = ((int)(convert.extraToJis[value])) |
							(((int)(convert.extraToJis[value + 1])) << 8);
				}
				else if (ch >= 0xFF60 && ch <= 0xFFA0)
				{
					// disallowed half-width kana is
					// already converted to full-width kana
					// so here we don't have to consider it.

					if (allow_shift_io)
					{
						if (!IsShifted (bytes))
						{
							var offset = byteIndex++;
							if (bytes != null) bytes[offset] = 0x0E;
							SetShifted (bytes, true);
							byteCount--;
						}
					}
					else
					{
						switch (m)
						{
							case ISO2022JPMode.JISX0201:
								break;
							default:
								SwitchMode(bytes, ref byteIndex, ref byteCount, ref m, ISO2022JPMode.JISX0201);
								break;
						}
					}
					value = ch - 0xFF40;
				}
				else if (ch < 128)
				{
					if (IsShifted (bytes))
					{
						var offset = byteIndex++;
						if (bytes != null) bytes[offset] = 0x0F;
						SetShifted (bytes, false);
						byteCount--;
					}
					SwitchMode(bytes, ref byteIndex, ref byteCount, ref m, ISO2022JPMode.ASCII);
					value = (int)ch;
				}
				else
				{
					HandleFallback (chars, ref i, ref charCount,
						bytes, ref byteIndex, ref byteCount, this);
					// skip non-convertible character
					continue;
				}

				//Console.WriteLine ("{0:X04} : {1:x02} {2:x02}", v, (int) v / 94 + 33, v % 94 + 33);
				if (value >= 0x100)
				{
					value -= 0x0100;
					if (bytes != null)
					{
						bytes[byteIndex++] = (byte)(value / 94 + 33);
						bytes[byteIndex++] = (byte)(value % 94 + 33);
					}
					else
					{
						byteIndex += 2;
					}
					byteCount -= 2;
				}
				else
				{
					var offset = byteIndex++;
					if (bytes != null) bytes[offset] = (byte)value;
					byteCount--;
				}
			}
			if (flush)
			{
				// must end in ASCII mode
				if (IsShifted (bytes))
				{
					var offset = byteIndex++;
					if (bytes != null) bytes[offset] = 0x0F;
					SetShifted (bytes, false);
					byteCount--;
				}
				if (m != ISO2022JPMode.ASCII)
					SwitchMode(bytes, ref byteIndex, ref byteCount, ref m, ISO2022JPMode.ASCII);
			}

			return byteIndex - start;
		}
		
		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool flush)
		{
			return GetBytesInternal (chars, charIndex, charCount, bytes, byteIndex, flush);
		}
#endif

		public override void Reset ()
		{
			m = ISO2022JPMode.ASCII;
			shifted_in_conv = shifted_in_count = false;
		}
	}


	internal class ISO2022JPDecoder : Decoder
	{
		static JISConvert convert = JISConvert.Convert;

		readonly bool allow_shift_io;
		ISO2022JPMode m = ISO2022JPMode.ASCII;
		bool shifted_in_conv, shifted_in_count;

		public ISO2022JPDecoder (bool allow1ByteKana, bool allowShiftIO)
		{
			this.allow_shift_io = allowShiftIO;
		}

		// GetCharCount
		public override int GetCharCount (byte [] bytes, int index, int count)
		{
			int ret = 0;

			int end = index + count;
			for (int i = index; i < end; i++) {
				if (allow_shift_io) {
					switch (bytes [i]) {
					case 0x0F:
						shifted_in_count = false;
						continue;
					case 0x0E:
						shifted_in_count = true;
						continue;
					}
				}
				if (bytes [i] != 0x1B) {
					if (!shifted_in_count && m == ISO2022JPMode.JISX0208) {
						if (i + 1 == end)
							break; // incomplete head of wide char
						else
							ret++;
						i++; // 2 byte char
					}
					else
						ret++; // half-kana or ASCII
				} else {
					if (i + 2 >= end)
						break; // incomplete escape sequence
					i++;
					bool wide = false;
					if (bytes [i] == 0x24)
						wide = true;
					else if (bytes [i] == 0x28)
						wide = false;
					else {
						ret += 2;
						continue;
					}
					i++;
					if (bytes [i] == 0x42 || bytes [i] == 0x40)
						m = wide ? ISO2022JPMode.JISX0208 : ISO2022JPMode.ASCII;
					else if (bytes [i] == 0x4A) // obsoleted
						m = ISO2022JPMode.ASCII;
					else if (bytes [i] == 0x49)
						m = ISO2022JPMode.JISX0201;
					else
						ret += 3;
				}
			}
			return ret;
		}

		private int ToChar (int value)
		{
			value <<= 1;
			return value + 1 >= convert.jisx0208ToUnicode.Length  || value < 0 ?
				-1 :
				((int) (convert.jisx0208ToUnicode [value])) |
					(((int) (convert.jisx0208ToUnicode [value + 1])) << 8);
		}

		public override int GetChars (byte [] bytes, int byteIndex, int byteCount, char [] chars, int charIndex)
		{
			int start = charIndex;
			int end = byteIndex + byteCount;
			for (int i = byteIndex; i < end && charIndex < chars.Length; i++) {
				if (allow_shift_io) {
					switch (bytes [i]) {
					case 0x0F:
						shifted_in_conv = false;
						continue;
					case 0x0E:
						shifted_in_conv = true;
						continue;
					}
				}

				if (bytes [i] != 0x1B) {
					if (shifted_in_conv || m == ISO2022JPMode.JISX0201) {
						// half-kana
						if (bytes [i] < 0x60)
							chars [charIndex++] = (char) (bytes [i] + 0xFF40);
						else
							// invalid
							chars [charIndex++] = '?';
					}
					else if (m == ISO2022JPMode.JISX0208) {
						if (i + 1 == end)
							break; // incomplete head of wide char

						// am so lazy, so reusing jis2sjis
						int s1 = ((bytes [i] - 1) >> 1) + ((bytes [i] <= 0x5e) ? 0x71 : 0xb1);
						int s2 = bytes [i + 1] + (((bytes [i] & 1) != 0) ? 0x20 : 0x7e);
						int v = (s1 <= 0x9F ? (s1 - 0x81) : (s1 - 0xc1)) * 0xBC;
						v += s2 - 0x41;

						int ch = ToChar (v);
						if (ch < 0)
							chars [charIndex++] = '?';
						else
							chars [charIndex++] = (char) ch;
						i++;
					}
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
					else {
						chars [charIndex++] = '\x1B';
						chars [charIndex++] = (char) bytes [i];
						continue;
					}
					i++;
					if (bytes [i] == 0x42 || bytes [i] == 0x40)
						m = wide ? ISO2022JPMode.JISX0208 : ISO2022JPMode.ASCII;
					else if (bytes [i] == 0x4A) // obsoleted
						m = ISO2022JPMode.ASCII;
					else if (bytes [i] == 0x49)
						m = ISO2022JPMode.JISX0201;
					else {
						chars [charIndex++] = '\x1B';
						chars [charIndex++] = (char) bytes [i - 1];
						chars [charIndex++] = (char) bytes [i];
					}
				}
			}

			return charIndex - start;
		}

		public override void Reset ()
		{
			m = ISO2022JPMode.ASCII;
			shifted_in_count = shifted_in_conv = false;
		}
	}

	[Serializable]
	public class ENCiso_2022_jp : CP50220
	{
		public ENCiso_2022_jp () : base() {}

	}; // class ENCiso_2022_jp
}
