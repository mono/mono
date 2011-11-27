//
// I18N.CJK.CP950
//
// Author:
//   Alan Tam Siu Lung (Tam@SiuLung.com)
//   Atsushi Enomoto  <atsushi@ximian.com>
//

using System;
using System.Text;
using I18N.Common;

namespace I18N.CJK
{
	[Serializable]
	internal class CP950 : DbcsEncoding
	{
		// Magic number used by Windows for the Big5 code page.
		private const int BIG5_CODE_PAGE = 950;
		
		// Constructor.
		public CP950() : base(BIG5_CODE_PAGE) {
		}

		internal override DbcsConvert GetConvert ()
		{
			return DbcsConvert.Big5;
		}

#if !DISABLE_UNSAFE
		// Get the bytes that result from encoding a character buffer.
		public unsafe override int GetByteCountImpl (char* chars, int count)
		{
			DbcsConvert convert = GetConvert ();
			int index = 0;
			int length = 0;

			while (count-- > 0) {
				char c = chars[index++];
				if (c <= 0x80 || c == 0xFF) { // ASCII
					length++;
					continue;
				}
				byte b1 = convert.u2n[((int)c) * 2 + 1];
				byte b2 = convert.u2n[((int)c) * 2];
				if (b1 == 0 && b2 == 0) {
#if NET_2_0
					// FIXME: handle fallback for GetByteCountImpl().
					length++;
#else
					length++;
#endif
				}
				else
					length += 2;
			}
			return length;
		}

		// Get the bytes that result from encoding a character buffer.
		public unsafe override int GetBytesImpl (char* chars, int charCount,
					     byte* bytes, int byteCount)
		{
			DbcsConvert convert = GetConvert ();
			int charIndex = 0;
			int byteIndex = 0;
			int end = charCount;
#if NET_2_0
			EncoderFallbackBuffer buffer = null;
#endif

			int origIndex = byteIndex;
			for (int i = charIndex; i < end; i++, charCount--) 
			{
				char c = chars[i];
				if (c <= 0x80 || c == 0xFF) { // ASCII
					bytes[byteIndex++] = (byte)c;
					continue;
				}
				byte b1 = convert.u2n[((int)c) * 2 + 1];
				byte b2 = convert.u2n[((int)c) * 2];
				if (b1 == 0 && b2 == 0) {
#if NET_2_0
					HandleFallback (ref buffer, chars,
						ref i, ref charCount,
						bytes, ref byteIndex, ref byteCount, null);
#else
					bytes[byteIndex++] = (byte)'?';
#endif
				} else {
					bytes[byteIndex++] = b1;
					bytes[byteIndex++] = b2;
				}
			}
			return byteIndex - origIndex;
		}
#else
		// Get the bytes that result from encoding a character buffer.
		public override int GetByteCount(char[] chars, int index, int count)
		{
			DbcsConvert convert = GetConvert();
			int length = 0;

			while (count-- > 0)
			{
				char c = chars[index++];
				if (c <= 0x80 || c == 0xFF)
				{ // ASCII
					length++;
					continue;
				}
				byte b1 = convert.u2n[((int)c) * 2 + 1];
				byte b2 = convert.u2n[((int)c) * 2];
				if (b1 == 0 && b2 == 0)
				{
#if NET_2_0
					// FIXME: handle fallback for GetByteCountImpl().
					length++;
#else
					length++;
#endif
				}
				else
					length += 2;
			}
			return length;
		}

		// Get the bytes that result from encoding a character buffer.
		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			int byteCount = bytes.Length;
			int end = charIndex + charCount;

			DbcsConvert convert = GetConvert();
#if NET_2_0
			EncoderFallbackBuffer buffer = null;
#endif

			int origIndex = byteIndex;
			for (int i = charIndex; i < end; i++, charCount--)
			{
				char c = chars[i];
				if (c <= 0x80 || c == 0xFF)
				{ // ASCII
					bytes[byteIndex++] = (byte)c;
					continue;
				}
				byte b1 = convert.u2n[((int)c) * 2 + 1];
				byte b2 = convert.u2n[((int)c) * 2];
				if (b1 == 0 && b2 == 0)
				{
#if NET_2_0
					HandleFallback (ref buffer, chars, ref i, ref charCount,
						bytes, ref byteIndex, ref byteCount, null);
#else
					bytes[byteIndex++] = (byte)'?';
#endif
				}
				else
				{
					bytes[byteIndex++] = b1;
					bytes[byteIndex++] = b2;
				}
			}
			return byteIndex - origIndex;
		}
#endif
		// Get the characters that result from decoding a byte buffer.
		public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
					     char[] chars, int charIndex)
		{
			/*
			DbcsConvert convert = GetConvert ();
			// A1 40 - FA FF
			base.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
			int origIndex = charIndex;
			int lastByte = 0;
			while (byteCount-- > 0) {
				int b = bytes[byteIndex++];
				if (lastByte == 0) {
					if (b <= 0x80 || b == 0xFF) { // ASCII
						chars[charIndex++] = (char)b;
					} else if (b < 0xA1 || b >= 0xFA) {
						// incorrect first byte.
						chars[charIndex++] = '?';
						byteCount--; // cut one more byte.
					} else {
						lastByte = b;
					}
					continue;
				}
				int ord = ((lastByte - 0xA1) * 191 + b - 0x40) * 2;
				char c1 = ord < 0 || ord > convert.n2u.Length ?
					'\0' :
					(char)(convert.n2u[ord] + convert.n2u[ord + 1] * 256);
				if (c1 == 0)
					chars[charIndex++] = '?';
				else
					chars[charIndex++] = c1;
				lastByte = 0;
			}
			if (lastByte != 0)
				chars[charIndex++] = '?';

			return charIndex - origIndex;
			*/

			return GetDecoder ().GetChars (bytes, byteIndex, byteCount, chars, charIndex);
		}
		
		// Get a decoder that handles a rolling Big5 state.
		public override Decoder GetDecoder()
		{
			return new CP950Decoder(GetConvert ());
		}
		
		// Get the mail body name for this encoding.
		public override String BodyName
		{
			get { return "big5"; }
		}
		
		// Get the human-readable name for this encoding.
		public override String EncodingName
		{
			get { return "Chinese Traditional (Big5)"; }
		}
		
		// Get the mail agent header name for this encoding.
		public override String HeaderName
		{
			get { return "big5"; }
		}
		
		// Get the IANA-preferred Web name for this encoding.
		public override String WebName
		{
			get { return "big5"; }
		}
		
		/*
		// Get the Windows code page represented by this object.
		public override int WindowsCodePage
		{
			get { return BIG5_PAGE; }
		}
		*/
		
		// Decoder that handles a rolling Big5 state.
		private sealed class CP950Decoder : DbcsDecoder
		{
			// Constructor.
			public CP950Decoder(DbcsConvert convert) : base(convert) {}
			int last_byte_count, last_byte_conv;

			public override int GetCharCount (byte[] bytes, int index, int count)
			{
				return GetCharCount (bytes, index, count, false);
			}

#if NET_2_0
			public override
#endif
			int GetCharCount (byte[] bytes, int index, int count, bool refresh)
			{
				CheckRange (bytes, index, count);

				int lastByte = last_byte_count;
				last_byte_count = 0;
				int length = 0;
				while (count-- > 0) {
					int b = bytes[index++];
					if (lastByte == 0) {
						if (b <= 0x80 || b == 0xFF) { // ASCII
							length++;
						} else if (b < 0xA1 || b >= 0xFA) {
							// incorrect first byte.
							length++;
							count--; // cut one more byte.
						} else {
							lastByte = b;
						}
						continue;
					}
					int ord = ((lastByte - 0xA1) * 191 + b - 0x40) * 2;
					char c1 = ord < 0 || ord > convert.n2u.Length ?
						'\0' :
						(char)(convert.n2u[ord] + convert.n2u[ord + 1] * 256);
					if (c1 == 0)
						// FIXME: fallback
						length++;
					else
						length++;
					lastByte = 0;
				}

				if (lastByte != 0) {
					if (refresh)
						// FIXME: fallback
						length++;
					else
						last_byte_count = lastByte;
				}
				return length;
			}

			public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
						     char[] chars, int charIndex)
			{
				return GetChars (bytes, byteIndex, byteCount, chars, charIndex, false);
			}

#if NET_2_0
			public override
#endif
			int GetChars(byte[] bytes, int byteIndex, int byteCount,
						     char[] chars, int charIndex, bool refresh)
			{
				CheckRange (bytes, byteIndex, byteCount, chars, charIndex);

				int origIndex = charIndex;
				int lastByte = last_byte_conv;
				last_byte_conv = 0;
				while (byteCount-- > 0) {
					int b = bytes[byteIndex++];
					if (lastByte == 0) {
						if (b <= 0x80 || b == 0xFF) { // ASCII
							chars[charIndex++] = (char)b;
						} else if (b < 0xA1 || b >= 0xFA) {
							// incorrect first byte.
							chars[charIndex++] = '?';
							byteCount--; // cut one more byte.
						} else {
							lastByte = b;
						}
						continue;
					}
					int ord = ((lastByte - 0xA1) * 191 + b - 0x40) * 2;
					char c1 = ord < 0 || ord > convert.n2u.Length ?
						'\0' :
						(char)(convert.n2u[ord] + convert.n2u[ord + 1] * 256);
					if (c1 == 0)
						chars[charIndex++] = '?';
					else
						chars[charIndex++] = c1;
					lastByte = 0;
				}

				if (lastByte != 0) {
					if (refresh)
						chars [charIndex++] = '?';
					else
						last_byte_conv = lastByte;
				}
				return charIndex - origIndex;
			}
		}
	}
	
	[Serializable]
	internal class ENCbig5 : CP950
	{
		public ENCbig5() {}
	}
}
