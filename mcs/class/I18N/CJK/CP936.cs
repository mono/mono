//
// I18N.CJK.CP936.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// (new implementation based on CP950.)
//

using System;
using System.Text;
using I18N.Common;

namespace I18N.CJK
{
	[Serializable]
	internal class CP936 : DbcsEncoding
	{
		// Magic number used by Windows for the Gb2312 code page.
		private const int GB2312_CODE_PAGE = 936;
		
		// Constructor.
		public CP936() : base(GB2312_CODE_PAGE) {
		}

		internal override DbcsConvert GetConvert ()
		{
			return DbcsConvert.Gb2312;
		}

#if !DISABLE_UNSAFE
		// Get the bytes that result from encoding a character buffer.
		public unsafe override int GetByteCountImpl (char* chars, int count)
		{
			return GetBytesImpl(chars, count, null, 0);
		}

		// Get the bytes that result from encoding a character buffer.
		public unsafe override int GetBytesImpl (char* chars, int charCount, byte* bytes, int byteCount)
		{
			DbcsConvert gb2312 = GetConvert ();
			int charIndex = 0;
			int byteIndex = 0;
			int end = charCount;
#if NET_2_0
			EncoderFallbackBuffer buffer = null;
#endif

			int origIndex = byteIndex;
			for (int i = charIndex; i < end; i++, charCount--) {
				char c = chars[i];
				if (c <= 0x80 || c == 0xFF) { // ASCII
					int offset = byteIndex++;
					if (bytes != null) bytes[offset] = (byte)c;
					continue;
				}
				byte b1 = gb2312.u2n[((int)c) * 2 + 1];
				byte b2 = gb2312.u2n[((int)c) * 2];
				if (b1 == 0 && b2 == 0) {
#if NET_2_0
					HandleFallback (ref buffer, chars,
						ref i, ref charCount,
						bytes, ref byteIndex, ref byteCount, null);
#else
					int offset = byteIndex++;
					if (bytes != null) bytes[offset] = (byte)'?';
#endif
				} else {
					if (bytes != null)
					{
						bytes[byteIndex++] = b1;
						bytes[byteIndex++] = b2;
					}
					else
					{
						byteIndex += 2;
					}
				}
			}
			return byteIndex - origIndex;
		}
#else
		protected int GetBytesInternal(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			int origIndex = byteIndex;
			int end = charIndex + charCount;
			int byteCount = bytes != null ? bytes.Length : 0;

			DbcsConvert gb2312 = GetConvert();
#if NET_2_0
			EncoderFallbackBuffer buffer = null;
#endif
			for (int i = charIndex; i < end; i++, charCount--)
			{
				char c = chars[i];
				if (c <= 0x80 || c == 0xFF)
				{ // ASCII
					int offset = byteIndex++;
					if (bytes != null) bytes[offset] = (byte)c;
					continue;
				}
				byte b1 = gb2312.u2n[((int)c) * 2 + 1];
				byte b2 = gb2312.u2n[((int)c) * 2];
				if (b1 == 0 && b2 == 0)
				{
#if NET_2_0
					HandleFallback (ref buffer, chars, ref i, ref charCount,
						bytes, ref byteIndex, ref byteCount, null);
#else
					int offset = byteIndex++;
					if (bytes != null) bytes[] = (byte)'?';
#endif
				}
				else
				{
					if (bytes != null)
					{
						bytes[byteIndex++] = b1;
						bytes[byteIndex++] = b2;
					}
					else
					{
						byteIndex += 2;
					}
				}
			}
			return byteIndex - origIndex;
		}

		// Get the bytes that result from encoding a character buffer.
		public override int GetByteCount(char[] chars, int index, int count)
		{
			return GetBytes(chars, index, count, null, 0);
		}

		// Get the bytes that result from encoding a character buffer.
		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			return GetBytesInternal(chars, charIndex, charCount, bytes, byteIndex);
		}
#endif
		// Get the characters that result from decoding a byte buffer.
		public override int GetCharCount (byte [] bytes, int index, int count)
		{
			return GetDecoder ().GetCharCount (bytes, index, count);
		}

		// Get the characters that result from decoding a byte buffer.
		public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
					     char[] chars, int charIndex)
		{
			return GetDecoder ().GetChars (
				bytes, byteIndex, byteCount, chars, charIndex);
		}
		
		// Get a decoder that handles a rolling Gb2312 state.
		public override Decoder GetDecoder()
		{
			return new CP936Decoder(GetConvert ());
		}
		
		// Get the mail body name for this encoding.
		public override String BodyName
		{
			get { return("gb2312"); }
		}
		
		// Get the human-readable name for this encoding.
		public override String EncodingName
		{
			get { return("Chinese Simplified (GB2312)"); }
		}
		
		// Get the mail agent header name for this encoding.
		public override String HeaderName
		{
			get { return("gb2312"); }
		}
		
		// Determine if this encoding can be displayed in a Web browser.
		public override bool IsBrowserDisplay
		{
			get { return(true); }
		}
		
		// Determine if this encoding can be saved from a Web browser.
		public override bool IsBrowserSave
		{
			get { return(true); }
		}
		
		// Determine if this encoding can be displayed in a mail/news agent.
		public override bool IsMailNewsDisplay
		{
			get { return(true); }
		}
		
		// Determine if this encoding can be saved from a mail/news agent.
		public override bool IsMailNewsSave
		{
			get { return(true); }
		}
		
		// Get the IANA-preferred Web name for this encoding.
		public override String WebName
		{
			get { return("gb2312"); }
		}
	}

	// Decoder that handles a rolling Gb2312 state.
	sealed class CP936Decoder : DbcsEncoding.DbcsDecoder
	{
		// Constructor.
		public CP936Decoder (DbcsConvert convert)
			: base (convert)
		{
		}

		int last_byte_count, last_byte_bytes;

		// Get the characters that result from decoding a byte buffer.
		public override int GetCharCount (byte [] bytes, int index, int count)
		{
			return GetCharCount (bytes, index, count, false);
		}

#if NET_2_0
		public override
#endif
		int GetCharCount (byte [] bytes, int index, int count, bool refresh)
		{
			CheckRange (bytes, index, count);

			int lastByte = last_byte_count;
			last_byte_count = 0;
			int length = 0;
			while (count-- > 0) {
				int b = bytes [index++];
				if (lastByte == 0) {
					if (b <= 0x80 || b == 0xFF) { // ASCII
						length++;
						continue;
					} else {
						lastByte = b;
						continue;
					}
				}
				length++;
				lastByte = 0;
			}

			if (lastByte != 0) {
				if (refresh) {
					length++;
					last_byte_count = 0;
				}
				else
					last_byte_count = lastByte;
			}

			return length;
		}

		public override int GetChars (byte[] bytes, int byteIndex, int byteCount,
					     char[] chars, int charIndex)
		{
			return GetChars (bytes, byteIndex, byteCount, chars, charIndex, false);
		}

#if NET_2_0
		public override
#endif
		int GetChars (byte [] bytes, int byteIndex, int byteCount,
			      char [] chars, int charIndex, bool refresh)
		{
			CheckRange (bytes, byteIndex, byteCount, chars, charIndex);

			int origIndex = charIndex;
			int lastByte = last_byte_bytes;
			last_byte_bytes = 0;
			while (byteCount-- > 0) {
				int b = bytes[byteIndex++];
				if (lastByte == 0) {
					if (b <= 0x80 || b == 0xFF) { // ASCII
						chars[charIndex++] = (char)b;
						continue;
					} else if (b < 0x81 || b >= 0xFF) {
						continue;
					} else {
						lastByte = b;
						continue;
					}
				}
				int ord = ((lastByte - 0x81) * 191 + b - 0x40) * 2;
				char c1 = ord < 0 || ord >= convert.n2u.Length ?
					'\0' : (char) (convert.n2u[ord] + convert.n2u[ord + 1] * 256);
				if (c1 == 0)
					chars[charIndex++] = '?';
				else
					chars[charIndex++] = c1;
				lastByte = 0;
			}

			if (lastByte != 0) {
				if (refresh) {
					// FIXME: handle fallback
					chars [charIndex++] = '?';
					last_byte_bytes = 0;
				}
				else
					last_byte_bytes = lastByte;
			}

			return charIndex - origIndex;
		}
	}
	
	[Serializable]
	internal class ENCgb2312 : CP936
	{
		public ENCgb2312(): base () {}
	}
}
