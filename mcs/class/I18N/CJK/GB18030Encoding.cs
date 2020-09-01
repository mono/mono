//
// GB18030Encoding.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
using System;
using System.Reflection;
using System.Text;
using I18N.Common;

#if DISABLE_UNSAFE
using MonoEncoder = I18N.Common.MonoSafeEncoder;
using MonoEncoding = I18N.Common.MonoSafeEncoding;
#endif

namespace I18N.CJK
{
	[Serializable]
	internal class ENCgb18030 : GB18030Encoding
	{
		public ENCgb18030 (): base () {}
	}

	[Serializable]
	public class CP54936 : GB18030Encoding { }

	[Serializable]
	public class GB18030Encoding : MonoEncoding
	{
		// Constructor.
		public GB18030Encoding ()
			: base (54936, 936)
		{
		}

		public override string EncodingName {
			get { return "Chinese Simplified (GB18030)"; }
		}

		public override string HeaderName {
			get { return "GB18030"; }
		}

		public override string BodyName {
			get { return "GB18030"; }
		}

		public override string WebName {
			get { return "GB18030"; }
		}

		public override bool IsMailNewsDisplay {
			get { return true; }
		}

		public override bool IsMailNewsSave {
			get { return true; }
		}

		public override bool IsBrowserDisplay {
			get { return true; }
		}

		public override bool IsBrowserSave {
			get { return true; }
		}

		public override int GetMaxByteCount (int len)
		{
			// non-GB2312 characters in \u0080 - \uFFFF
			return len * 4;
		}

		public override int GetMaxCharCount (int len)
		{
			return len;
		}

#if !DISABLE_UNSAFE
		public unsafe override int GetByteCountImpl (char* chars, int count)
		{
			return new GB18030Encoder (this).GetByteCountImpl (chars, count, true);
		}

		public unsafe override int GetBytesImpl (char* chars, int charCount, byte* bytes, int byteCount)
		{
			return new GB18030Encoder (this).GetBytesImpl (chars, charCount, bytes, byteCount, true);
		}
#else
		public override int GetByteCount (char [] chars, int index, int length)
		{
			return new GB18030Encoder (this).GetByteCount (chars, index, length, true);
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			return new GB18030Encoder (this).GetBytes (chars, charIndex, charCount, bytes, byteIndex, true);
		}
#endif

		public override int GetCharCount (byte [] bytes, int start, int len)
		{
			return new GB18030Decoder ().GetCharCount (bytes, start, len);
		}

		public override int GetChars (byte [] bytes, int byteIdx, int srclen, char [] chars, int charIdx)
		{
			return new GB18030Decoder ().GetChars (bytes, byteIdx, srclen, chars, charIdx);
		}

		public override Encoder GetEncoder ()
		{
			return new GB18030Encoder (this);
		}

		public override Decoder GetDecoder ()
		{
			return new GB18030Decoder ();
		}
	}

	class GB18030Decoder : DbcsEncoding.DbcsDecoder
	{
		static DbcsConvert gb2312 = DbcsConvert.Gb2312;
		// for now incomplete block is not supported - should we?
		// int incomplete1 = -1, incomplete2 = -1, incomplete3 = -1;

		public GB18030Decoder ()
			: base (null)
		{
		}

		public override int GetCharCount (byte [] bytes, int start, int len)
		{
			CheckRange (bytes, start, len);

			int end = start + len;
			int ret = 0;
			while (start < end) {
				if (bytes [start] < 0x80) {
					ret++;
					start++;
					continue;
				}
				else if (bytes [start] == 0x80) {
					// Euro sign - actually it is obsolete,
					// now it's just reserved but not used
					ret++;
					start++;
					continue;
				}
				else if (bytes [start] == 0xFF) {
					// invalid data - fill '?'
					ret++;
					start++;
					continue;
				}
				else if (start + 1 >= end) {
//					incomplete1 = bytes [start];
//					incomplete2 = -1;
//					incomplete3 = -1;
					ret++;
					break; // incomplete tail.
				}

				byte second = bytes [start + 1];
				if (second == 0x7F || second == 0xFF) {
					// invalid data
					ret++;
					start += 2;
					continue;
				}
				else if (0x30 <= second && second <= 0x39) {
					// UCS mapping
					if (start + 3 >= end) {
						// incomplete tail.
//						incomplete1 = bytes [start];
//						incomplete2 = bytes [start + 1];
//						if (start + 3 == end)
//							incomplete3 = bytes [start + 2];
						ret += start + 3 == end ? 3 : 2;
						break;
					}
					long value = GB18030Source.FromGBX (bytes, start);
					if (value < 0) {
						// invalid data.
						ret++;
						start -= (int) value;
					} else if (value >= 0x10000) {
						// UTF16 surrogate
						ret += 2;
						start += 4;
					} else {
						// UTF16 BMP
						ret++;
						start+= 4;
					}
				} else {
					// GB2312 mapping
					start += 2;
					ret++;
				}
			}
			return ret;
		}

		public override int GetChars (byte [] bytes, int byteIndex, int byteCount, char [] chars, int charIndex)
		{
			CheckRange (bytes, byteIndex, byteCount, chars, charIndex);

			int byteEnd = byteIndex + byteCount;
			int charStart = charIndex;

			while (byteIndex < byteEnd) {
				if (bytes [byteIndex] < 0x80) {
					chars [charIndex++] = (char) bytes [byteIndex++];
					continue;
				}
				else if (bytes [byteIndex] == 0x80) {
					// Euro sign - actually it is obsolete,
					// now it's just reserved but not used
					chars [charIndex++] = '\u20AC';
					byteIndex++;
					continue;
				}
				else if (bytes [byteIndex] == 0xFF) {
					// invalid data - fill '?'
					chars [charIndex++] = '?';
					byteIndex++;
					continue;
				}
				else if (byteIndex + 1 >= byteEnd) {
					//incomplete1 = bytes [byteIndex++];
					//incomplete2 = -1;
					//incomplete3 = -1;
					break; // incomplete tail.
				}

				byte second = bytes [byteIndex + 1];
				if (second == 0x7F || second == 0xFF) {
					// invalid data
					chars [charIndex++] = '?';
					byteIndex += 2;
				}
				else if (0x30 <= second && second <= 0x39) {
					// UCS mapping
					if (byteIndex + 3 >= byteEnd) {
						// incomplete tail.
						//incomplete1 = bytes [byteIndex];
						//incomplete2 = bytes [byteIndex + 1];
						//if (byteIndex + 3 == byteEnd)
						//	incomplete3 = bytes [byteIndex + 2];
						break;
					}
					long value = GB18030Source.FromGBX (bytes, byteIndex);
					if (value < 0) {
						// invalid data.
						chars [charIndex++] = '?';
						byteIndex -= (int) value;
					} else if (value >= 0x10000) {
						// UTF16 surrogate
						value -= 0x10000;
						chars [charIndex++] = (char) (value / 0x400 + 0xD800);
						chars [charIndex++] = (char) (value % 0x400 + 0xDC00);
						byteIndex += 4;
					} else {
						// UTF16 BMP
						chars [charIndex++] = (char) value;
						byteIndex += 4;
					}
				} else {
					byte first = bytes [byteIndex];
					int ord = ((first - 0x81) * 191 + second - 0x40) * 2;
					char c1 = ord < 0 || ord >= gb2312.n2u.Length ?
						'\0' : (char) (gb2312.n2u [ord] + gb2312.n2u [ord + 1] * 256);
					if (c1 == 0)
						chars [charIndex++] = '?';
					else
						chars [charIndex++] = c1;
					byteIndex += 2;
				}
			}

			return charIndex - charStart;
		}
	}

	class GB18030Encoder : MonoEncoder
	{
		static DbcsConvert gb2312 = DbcsConvert.Gb2312;

		public GB18030Encoder (MonoEncoding owner)
			: base (owner)
		{
		}

		char incomplete_byte_count;
		char incomplete_bytes;

#if !DISABLE_UNSAFE
		public unsafe override int GetByteCountImpl (char* chars, int count, bool refresh)
		{
			int start = 0;
			int end = count;
			int ret = 0;
			while (start < end) {
				char ch = chars [start];
				if (ch < 0x80) {
					// ASCII
					ret++;
					start++;
					continue;
				} else if (Char.IsSurrogate (ch)) {
					// Surrogate
					if (start + 1 == end) {
						incomplete_byte_count = ch;
						start++;
					} else {
						ret += 4;
						start += 2;
					}
					continue;
				}

				if (ch < 0x80 || ch == 0xFF) {
					// ASCII
					ret++;
					start++;
					continue;
				}

				byte b1 = gb2312.u2n [((int) ch) * 2 + 1];
				byte b2 = gb2312.u2n [((int) ch) * 2];
				if (b1 != 0 && b2 != 0) {
					// GB2312
					ret += 2;
					start++;
					continue;
				}

				// non-GB2312
				long value = GB18030Source.FromUCS (ch);
				if (value < 0)
					ret++; // invalid(?)
				else
					ret += 4;
				start++;
			}

			if (refresh) {
				if (incomplete_byte_count != char.MinValue)
					ret++;
				incomplete_byte_count = char.MinValue;
			}
			return ret;
		}

		public unsafe override int GetBytesImpl (char* chars, int charCount, byte* bytes, int byteCount, bool refresh)
		{
			int charIndex = 0;
			int byteIndex = 0;

			int charEnd = charIndex + charCount;
			int byteStart = byteIndex;
			char ch = incomplete_bytes;

			while (charIndex < charEnd) {
				if (incomplete_bytes == char.MinValue)
					ch = chars [charIndex++];
				else
					incomplete_bytes = char.MinValue;

				if (ch < 0x80) {
					// ASCII
					bytes [byteIndex++] = (byte) ch;
					continue;
				} else if (Char.IsSurrogate (ch)) {
					// Surrogate
					if (charIndex == charEnd) {
						incomplete_bytes = ch;
						break; // incomplete
					}
					char ch2 = chars [charIndex++];
					if (!Char.IsSurrogate (ch2)) {
						// invalid surrogate
						HandleFallback (
							chars, ref charIndex, ref charCount,
							bytes, ref byteIndex, ref byteCount, null);
						continue;
					}
					int cp = (ch - 0xD800) * 0x400 + ch2 - 0xDC00;
					GB18030Source.Unlinear (bytes + byteIndex, GB18030Source.FromUCSSurrogate (cp));
					byteIndex += 4;
					continue;
				}


				if (ch <= 0x80 || ch == 0xFF) {
					// Character maps to itself
					bytes [byteIndex++] = (byte) ch;
					continue;
				}

				byte b1 = gb2312.u2n [((int) ch) * 2 + 1];
				byte b2 = gb2312.u2n [((int) ch) * 2];
				if (b1 != 0 && b2 != 0) {
					bytes [byteIndex++] = b1;
					bytes [byteIndex++] = b2;
					continue;
				}

				long value = GB18030Source.FromUCS (ch);
				if (value < 0)
					bytes [byteIndex++] = 0x3F; // invalid(?)
				else {
					// non-GB2312
					GB18030Source.Unlinear (bytes + byteIndex, value);
					byteIndex += 4;
				}
			}

			if (refresh) {
				if (incomplete_bytes != char.MinValue)
					bytes [byteIndex++] = 0x3F; // incomplete
				incomplete_bytes = char.MinValue;
			}

			return byteIndex - byteStart;
		}
#else

		public override int GetByteCount(char[] chars, int start, int count, bool refresh)
		{
			int end = start + count;
			int ret = 0;
			while (start < end)
			{
				char ch = chars[start];
				if (ch < 0x80)
				{
					// ASCII
					ret++;
					start++;
					continue;
				}
				else if (Char.IsSurrogate(ch))
				{
					// Surrogate
					if (start + 1 == end)
					{
						incomplete_byte_count = ch;
						start++;
					}
					else
					{
						ret += 4;
						start += 2;
					}
					continue;
				}

				if (ch < 0x80 || ch == 0xFF)
				{
					// ASCII
					ret++;
					start++;
					continue;
				}

				byte b1 = gb2312.u2n[((int)ch) * 2 + 1];
				byte b2 = gb2312.u2n[((int)ch) * 2];
				if (b1 != 0 && b2 != 0)
				{
					// GB2312
					ret += 2;
					start++;
					continue;
				}

				// non-GB2312
				long value = GB18030Source.FromUCS(ch);
				if (value < 0)
					ret++; // invalid(?)
				else
					ret += 4;
				start++;
			}

			if (refresh)
			{
				if (incomplete_byte_count != char.MinValue)
					ret++;
				incomplete_byte_count = char.MinValue;
			}
			return ret;
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex, bool refresh)
		{
			int byteCount = bytes.Length;
			int charEnd = charIndex + charCount;
			int byteStart = byteIndex;
			char ch = incomplete_bytes;

			while (charIndex < charEnd)
			{
				if (incomplete_bytes == char.MinValue)
					ch = chars[charIndex++];
				else
					incomplete_bytes = char.MinValue;

				if (ch < 0x80)
				{
					// ASCII
					bytes[byteIndex++] = (byte)ch;
					continue;
				}
				else if (Char.IsSurrogate(ch))
				{
					// Surrogate
					if (charIndex == charEnd)
					{
						incomplete_bytes = ch;
						break; // incomplete
					}
					char ch2 = chars[charIndex++];
					if (!Char.IsSurrogate(ch2))
					{
						// invalid surrogate
						HandleFallback (chars, ref charIndex, ref charCount,
							bytes, ref byteIndex, ref byteCount, null);
						continue;
					}
					int cp = (ch - 0xD800) * 0x400 + ch2 - 0xDC00;
					GB18030Source.Unlinear(bytes,  byteIndex, GB18030Source.FromUCSSurrogate(cp));
					byteIndex += 4;
					continue;
				}


				if (ch <= 0x80 || ch == 0xFF)
				{
					// Character maps to itself
					bytes[byteIndex++] = (byte)ch;
					continue;
				}

				byte b1 = gb2312.u2n[((int)ch) * 2 + 1];
				byte b2 = gb2312.u2n[((int)ch) * 2];
				if (b1 != 0 && b2 != 0)
				{
					bytes[byteIndex++] = b1;
					bytes[byteIndex++] = b2;
					continue;
				}

				long value = GB18030Source.FromUCS(ch);
				if (value < 0)
					bytes[byteIndex++] = 0x3F; // invalid(?)
				else
				{
					// non-GB2312
					GB18030Source.Unlinear(bytes, byteIndex, value);
					byteIndex += 4;
				}
			}

			if (refresh)
			{
				if (incomplete_bytes != char.MinValue)
					bytes[byteIndex++] = 0x3F; // incomplete
				incomplete_bytes = char.MinValue;
			}

			return byteIndex - byteStart;
		}
#endif
	}
}
