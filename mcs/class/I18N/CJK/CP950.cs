//
// I18N.CJK.CP950
//
// Author:
//   Alan Tam Siu Lung (Tam@SiuLung.com)
//

using System;
using System.Text;
using I18N.Common;

namespace I18N.CJK
{
	public sealed class CP950 : DbcsEncoding
	{
		// Magic number used by Windows for the Big5 code page.
		private const int BIG5_CODE_PAGE = 950;
		
		// Constructor.
		public CP950() : base(BIG5_CODE_PAGE) {
			convert = Big5Convert.Convert;
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
				if (c < 0x80) { // ASCII
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
			// A1 40 - FA FF
			base.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
			int origIndex = charIndex;
			int lastByte = 0;
			while (byteCount-- > 0) {
				int b = bytes[byteIndex++];
				if (lastByte == 0) {
					if (b < 0x80) { // ASCII
						chars[charIndex++] = (char)b;
						continue;
					} else if (b < 0xA1 || b >= 0xFA) {
						continue;
					} else {
						lastByte = b;
						continue;
					}
				}
				int ord = ((lastByte - 0xA1) * 191 + b - 0x40) * 2;
				char c1 = (char)(convert.n2u[ord] + convert.n2u[ord + 1] * 256);
				if (c1 == 0)
					chars[charIndex++] = '?';
				else
					chars[charIndex++] = c1;
				lastByte = 0;
			}
			return charIndex - origIndex;
		}
		
		// Get a decoder that handles a rolling Big5 state.
		public override Decoder GetDecoder()
		{
			return new CP950Decoder(convert);
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
			
			public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
						     char[] chars, int charIndex)
			{
				base.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
				int origIndex = charIndex;
				while (byteCount-- > 0) {
					int b = bytes[byteIndex++];
					if (lastByte == 0) {
						if (b < 0x80) { // ASCII
							chars[charIndex++] = (char)b;
							continue;
						} else if (b < 0xA1 || b >= 0xFA) {
							continue;
						} else {
							lastByte = b;
							continue;
						}
					}
					int ord = ((lastByte - 0xA1) * 191 + b - 0x40) * 2;
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
	
	public class ENCbig5 : CP950
	{
		public ENCbig5() {}
	}
}
