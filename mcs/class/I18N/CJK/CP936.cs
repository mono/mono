//
// I18N.CJK.CP936
//
// Author:
//   Alan Tam Siu Lung (Tam@SiuLung.com)
//

using System;
using System.Text;
using I18N.Common;

namespace I18N.CJK
{
	internal class CP936 : DbcsEncoding
	{
		// Magic number used by Windows for the GB2312 code page.
		private const int GB2312_CODE_PAGE = 936;
		
		// Constructor.
		public CP936() : base(GB2312_CODE_PAGE) {
			convert = Gb2312Convert.Convert;
		}
		
		// Get the bytes that result from encoding a character buffer.
		public override int GetBytes(char[] chars, int charIndex, int charCount,
					     byte[] bytes, int byteIndex)
		{
			// 00 00 - FF FF
			base.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
			int origIndex = byteIndex;
			while (charCount-- > 0) {
				char c = chars[charIndex++];
				if (c <= 0x80 || c == 0xFF) { // ASCII
					bytes[byteIndex++] = (byte)c;
					continue;
				}
				byte b1 = convert.u2n[((int)c) * 2 + 1];
				byte b2 = convert.u2n[((int)c) * 2];
				if (b1 == 0 && b2 == 0) {
					bytes[byteIndex++] = (byte)'?';
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
			// A1 A1 - FA F7
			base.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
			int origIndex = charIndex;
			int lastByte = 0;
			while (byteCount-- > 0) {
				int b = bytes[byteIndex++];
				if (lastByte == 0) {
					if (b <= 0x80 || b == 0xFF) { // ASCII
						chars[charIndex++] = (char)b;
						continue;
					} else if (b < 0xA1 || b >= 0xF8) {
						continue;
					} else {
						lastByte = b;
						continue;
					}
				}
				int ord = ((lastByte - 0xA1) * 94 + b - 0xA1) * 2;
				char c1 = (char)(convert.n2u[ord] + convert.n2u[ord + 1] * 256);
				if (c1 == 0)
					chars[charIndex++] = '?';
				else
					chars[charIndex++] = c1;
				lastByte = 0;
			}
			return charIndex - origIndex;
		}
		
		// Get a decoder that handles a rolling GB2312 state.
		public override Decoder GetDecoder()
		{
			return new CP936Decoder(convert);
		}
		
		// Get the mail body name for this encoding.
		public override String BodyName
		{
			get { return "gb2312"; }
		}
		
		// Get the human-readable name for this encoding.
		public override String EncodingName
		{
			get { return "Chinese Simplified (GB2312)"; }
		}
		
		// Get the mail agent header name for this encoding.
		public override String HeaderName
		{
			get { return "gb2312"; }
		}
		
		// Get the IANA-preferred Web name for this encoding.
		public override String WebName
		{
			get { return "gb2312"; }
		}
		
		/*
		// Get the Windows code page represented by this object.
		public override int WindowsCodePage
		{
			get { return GB2312_PAGE; }
		}
		*/
		
		// Decoder that handles a rolling GB2312 state.
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
						} else if (b < 0xA1 || b >= 0xF8) {
							continue;
						} else {
							lastByte = b;
							continue;
						}
					}
					int ord = ((lastByte - 0xA1) * 94 + b - 0xA1) * 2;
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
		public ENCgb2312() {}
	}
}
