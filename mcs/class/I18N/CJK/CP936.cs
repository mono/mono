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
	internal class CP936 : DbcsEncoding
	{
		static DbcsConvert gb2312 = DbcsConvert.Gb2312;

		// Magic number used by Windows for the Gb2312 code page.
		private const int GB2312_CODE_PAGE = 936;
		
		// Constructor.
		public CP936() : base(GB2312_CODE_PAGE) {
		}
		
		// Get the bytes that result from encoding a character buffer.
		public unsafe override int GetBytesImpl (char* chars, int charCount,
					     byte* bytes, int byteCount)
		{
			int charIndex = 0;
			int byteIndex = 0;
#if NET_2_0
			EncoderFallbackBuffer buffer = null;
#endif

			int origIndex = byteIndex;
			while (charCount-- > 0) {
				char c = chars[charIndex++];
				if (c <= 0x80 || c == 0xFF) { // ASCII
					bytes[byteIndex++] = (byte)c;
					continue;
				}
				byte b1 = gb2312.u2n[((int)c) * 2 + 1];
				byte b2 = gb2312.u2n[((int)c) * 2];
				if (b1 == 0 && b2 == 0) {
#if NET_2_0
					HandleFallback (ref buffer, chars,
						ref charIndex, ref charCount,
						bytes, ref byteIndex, ref byteCount);
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
		
		// Get the characters that result from decoding a byte buffer.
		public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
					     char[] chars, int charIndex)
		{
			// A1 40 - FA FF
			base.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
			int origIndex = charIndex;
			int lastByte = 0;
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
				char c1 = (char)(gb2312.n2u[ord] + gb2312.n2u[ord + 1] * 256);
				if (c1 == 0)
					chars[charIndex++] = '?';
				else
					chars[charIndex++] = c1;
				lastByte = 0;
			}
			return charIndex - origIndex;
		}
		
		// Get a decoder that handles a rolling Gb2312 state.
		public override Decoder GetDecoder()
		{
			return new CP936Decoder(gb2312);
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
		
		// Decoder that handles a rolling Gb2312 state.
		private sealed class CP936Decoder : DbcsDecoder
		{
			// Constructor.
			public CP936Decoder(DbcsConvert convert) : base(convert) {}
			
			public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
						     char[] chars, int charIndex)
			{
				base.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
				int origIndex = charIndex;
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
					char c1 = (char)(convert.n2u[ord] + convert.n2u[ord + 1] * 256);
					if (c1 == 0) {
						chars[charIndex++] = '?';
					} else {
						chars[charIndex++] = c1;
					}
					lastByte = 0;
				}
				return charIndex - origIndex;
			}
		}
	}
	
	internal class ENCgb2312 : CP936
	{
		public ENCgb2312(): base () {}
	}
}
