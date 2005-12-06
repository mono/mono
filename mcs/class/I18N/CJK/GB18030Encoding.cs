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

namespace I18N.CJK
{
	internal class ENCgb18030 : GB18030Encoding
	{
		public ENCgb18030 (): base () {}
	}

	public class CP54936 : GB18030Encoding { }

	public class GB18030Encoding : MonoEncoding
	{
		// Constructor.
		public GB18030Encoding ()
			: base (54936)
		{
		}

		public override string EncodingName {
			get { return "Chinese Simplified (GB18030)"; }
		}

		public override string WebName {
			get { return "GB18030"; }
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

		public override int GetByteCount (char [] chars, int index, int length)
		{
			return new GB18030Encoder (this).GetByteCount (chars, index, length, true);
		}

		public unsafe override int GetBytesImpl (char* chars, int charCount, byte* bytes, int byteCount)
		{
			return new GB18030Encoder (this).GetBytesImpl (chars, charCount, bytes, byteCount, true);
		}

		public override int GetCharCount (byte [] bytes, int start, int len)
		{
			return new GB18030Decoder ().GetCharCount (bytes, start, len);
		}

		public override int GetChars (byte [] bytes, int byteIdx, int srclen, char [] chars, int charIdx)
		{
			return new GB18030Decoder ().GetChars (bytes, byteIdx, srclen, chars, charIdx);
		}
	}

	class GB18030Decoder : Decoder
	{
		Gb2312Convert gb2312 = Gb2312Convert.Convert;
		// for now incomplete block is not supported - should we?
		// int incomplete1 = -1, incomplete2 = -1, incomplete3 = -1;

		public override int GetCharCount (byte [] bytes, int start, int len)
		{
			if (bytes == null)
				throw new ArgumentNullException ("bytes");
			if (start < 0 || start > bytes.Length)
				throw new ArgumentOutOfRangeException ("start");
			if (len < 0 || start + len > bytes.Length)
				throw new ArgumentOutOfRangeException ("len");

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
			if (bytes == null)
				throw new ArgumentNullException ("bytes");
			if (chars == null)
				throw new ArgumentNullException ("chars");
			if (byteIndex < 0 || byteIndex > bytes.Length)
				throw new ArgumentOutOfRangeException ("byteIndex");
			if (byteCount < 0 || byteIndex + byteCount > bytes.Length)
				throw new ArgumentOutOfRangeException ("byteCount");
			if (charIndex < 0 || charIndex > chars.Length)
				throw new ArgumentOutOfRangeException ("charIndex");

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
					// GB2312 mapping, or invalid.
					// ('second' is always valid here).
					int head = bytes [byteIndex];
					char c = gb2312.BytePairToChar (ref head, second);
					byteIndex += 2;
					chars [charIndex++] = c == char.MinValue ? '?' : c;
				}
			}

			return charIndex - charStart;
		}
	}

	class GB18030Encoder : MonoEncoding.MonoEncoder
	{
		public GB18030Encoder (MonoEncoding owner)
			: base (owner)
		{
		}

		Gb2312Convert gb2312 = Gb2312Convert.Convert;
		char incomplete;

		public override int GetByteCount (char [] chars, int start, int len, bool refresh)
		{
			if (refresh)
				incomplete = char.MinValue;

			if (chars == null)
				throw new ArgumentNullException ("chars");
			if (start < 0 || start > chars.Length)
				throw new ArgumentOutOfRangeException ("index");
			if (len < 0 || start + len > chars.Length)
				throw new ArgumentOutOfRangeException ("count");

			int end = start + len;
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
					if (start + 1 == end)
						break; // incomplete
					ret += 4;
					start += 2;
					continue;
				}

				if (ch < 0x80 || ch == 0xFF) {
					// ASCII
					ret++;
					start++;
					continue;
				}
				long value = gb2312.UcsToGbk (ch);
				if (value != 0) {
					// GB2312
					ret += 2;
					start++;
					continue;
				}

				// non-GB2312
				ret += 4;
				start++;
			}
			return ret;
		}

		public unsafe override int GetBytesImpl (char* chars, int charCount, byte* bytes, int byteCount, bool flush)
		{
			int charIndex = 0;
			int byteIndex = 0;
#if NET_2_0
			EncoderFallbackBuffer buffer = null;
#endif

			int charEnd = charIndex + charCount;
			int byteStart = byteIndex;
			char ch = incomplete;

			while (charIndex < charEnd) {
				if (incomplete == char.MinValue)
					ch = chars [charIndex++];
				else
					incomplete = char.MinValue;

				if (ch < 0x80) {
					// ASCII
					bytes [byteIndex++] = (byte) ch;
					continue;
				} else if (Char.IsSurrogate (ch)) {
					// Surrogate
					if (charIndex == charEnd) {
						incomplete = ch;
						break; // incomplete
					}
					char ch2 = chars [charIndex++];
					if (!Char.IsSurrogate (ch2)) {
						// invalid surrogate
#if NET_2_0
						HandleFallback (
							chars, ref charIndex, ref charCount,
							bytes, ref byteIndex, ref byteCount);
#else
						bytes [byteIndex++] = (byte) '?';
#endif
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

				long value = gb2312.UcsToGbk (ch);
				if (value != 0) {
					bytes [byteIndex++] = (byte) (value / 0x100);
					bytes [byteIndex++] = (byte) (value % 0x100);
					continue;
				}

				value = GB18030Source.FromUCS (ch);
				// non-GB2312
				GB18030Source.Unlinear (bytes + byteIndex, value);
				byteIndex += 4;
			}
			return byteIndex - byteStart;
		}
	}
}
