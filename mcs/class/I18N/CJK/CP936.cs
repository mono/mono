//
// I18N.CJK.CP936
//
// Author:
//   Alan Tam Siu Lung (Tam@SiuLung.com)
//   Dick Porter (dick@ximian.com)
//

// This is actually EUC-CN, which is why 0x80 gets added and subtracted when
// converting.


using System;
using System.Text;
using I18N.Common;

namespace I18N.CJK
{
	internal class CP936 : Encoding
	{
		// Magic number used by Windows for the GB2312 code page.
		private const int GB2312_CODE_PAGE = 936;
		
		private Gb2312Convert convert;
		
		// Constructor.
		public CP936() : base(GB2312_CODE_PAGE) {
			convert = Gb2312Convert.Convert;
		}
		
		// Get the number of bytes needed to encode a character buffer.
		public override int GetByteCount(char[] chars, int index,
						 int count)
		{
			if (chars == null) {
				throw new ArgumentNullException("chars");
			}
			if (index < 0 || index > chars.Length) {
				throw new ArgumentOutOfRangeException("index", Strings.GetString("ArgRange_Array"));
			}
			if (count < 0 || index + count > chars.Length) {
				throw new ArgumentOutOfRangeException("count", Strings.GetString("ArgRange_Array"));
			}
			
			byte[] buffer = new byte[count * 2];
			return GetBytes(chars, index, count, buffer, 0);
		}
		
		// Get the bytes that result from encoding a character buffer.
		public override int GetBytes(char[] chars, int charIndex,
					     int charCount, byte[] bytes,
					     int byteIndex)
		{
			if (chars == null) {
				throw new ArgumentNullException("chars");
			}
			if (bytes == null) {
				throw new ArgumentNullException("bytes");
			}
			if (charIndex < 0 || charIndex > chars.Length) {
				throw new ArgumentOutOfRangeException("charIndex", Strings.GetString("ArgRange_Array"));
			}
			if (charCount < 0 || charIndex + charCount > chars.Length) {
				throw new ArgumentOutOfRangeException("charCount", Strings.GetString("ArgRange_Array"));
			}
			if (byteIndex < 0 || byteIndex > bytes.Length) {
				throw new ArgumentOutOfRangeException("byteIndex", Strings.GetString("ArgRange_Array"));
			}

			int posn = byteIndex;
			int byteLength = bytes.Length;
			int ch;
			
			while(charCount-- > 0) {
				ch = chars[charIndex++];
				
				if (posn >= byteLength) {
					throw new ArgumentException ("bytes", Strings.GetString ("Arg_InsufficientSpace"));
				}

				if (ch <= 0x80 || ch == 0xFF) {
					// Character maps to itself
					bytes[posn++] = (byte)ch;
					continue;
				}

				byte byte1=0, byte2=0;
				int tablepos;
				
				if (ch >= 0xa4 && ch <= 0x101) {
					tablepos = (ch - 0xa4) * 2;
					byte1 = convert.gb2312_from_unicode1[tablepos];
					byte2 = convert.gb2312_from_unicode1[tablepos + 1];
				} else if (ch >= 0x113 && ch <= 0x2c9) {
					switch(ch) {
					case 0x113:
						byte1 = 0x28;
						byte2 = 0x25;
						break;
					case 0x11b:
						byte1 = 0x28;
						byte2 = 0x27;
						break;
					case 0x12b:
						byte1 = 0x28;
						byte2 = 0x29;
						break;
					case 0x14d:
						byte1 = 0x28;
						byte2 = 0x2d;
						break;
					case 0x16b:
						byte1 = 0x28;
						byte2 = 0x31;
						break;
					case 0x1ce:
						byte1 = 0x28;
						byte2 = 0x23;
						break;
					case 0x1d0:
						byte1 = 0x28;
						byte2 = 0x2b;
						break;
					case 0x1d2:
						byte1 = 0x28;
						byte2 = 0x2f;
						break;
					case 0x1d4:
						byte1 = 0x28;
						byte2 = 0x33;
						break;
					case 0x1d6:
						byte1 = 0x28;
						byte2 = 0x35;
						break;
					case 0x1d8:
						byte1 = 0x28;
						byte2 = 0x36;
						break;
					case 0x1da:
						byte1 = 0x28;
						byte2 = 0x37;
						break;
					case 0x1dc:
						byte1 = 0x28;
						byte2 = 0x38;
						break;
					case 0x2c7:
						byte1 = 0x21;
						byte2 = 0x26;
						break;
					case 0x2c9:
						byte1 = 0x21;
						byte2 = 0x25;
						break;
					}
				} else if (ch >= 0x391 && ch <= 0x3c9) {
					tablepos = (ch - 0x391) * 2;
					byte1 = convert.gb2312_from_unicode2[tablepos];
					byte2 = convert.gb2312_from_unicode2[tablepos + 1];
				} else if (ch >= 0x401 && ch <= 0x451) {
					tablepos = (ch - 0x401) * 2;
					byte1 = convert.gb2312_from_unicode3[tablepos];
					byte2 = convert.gb2312_from_unicode3[tablepos + 1];
				} else if (ch >= 0x2015 && ch <= 0x203b) {
					tablepos = (ch - 0x2015) * 2;
					byte1 = convert.gb2312_from_unicode4[tablepos];
					byte2 = convert.gb2312_from_unicode4[tablepos + 1];
				} else if (ch >= 0x2103 && ch <= 0x22a5) {
					tablepos = (ch - 0x2103) * 2;
					byte1 = convert.gb2312_from_unicode5[tablepos];
					byte2 = convert.gb2312_from_unicode5[tablepos + 1];
				} else if (ch == 0x2312) {
					byte1 = 0x21;
					byte2 = 0x50;
				} else if (ch >= 0x2460 && ch <= 0x249b) {
					tablepos = (ch - 0x2460) * 2;
					byte1 = convert.gb2312_from_unicode6[tablepos];
					byte2 = convert.gb2312_from_unicode6[tablepos + 1];
				} else if (ch >= 0x2500 && ch <= 0x254b) {
					byte1 = 0x29;
					byte2 = (byte)(0x24 + (ch % 0x100));
				} else if (ch >= 0x25a0 && ch <= 0x2642) {
					switch(ch) {
					case 0x25a0:
						byte1 = 0x21;
						byte2 = 0x76;
						break;
					case 0x25a1:
						byte1 = 0x21;
						byte2 = 0x75;
						break;
					case 0x25b2:
						byte1 = 0x21;
						byte2 = 0x78;
						break;
					case 0x25b3:
						byte1 = 0x21;
						byte2 = 0x77;
						break;
					case 0x25c6:
						byte1 = 0x21;
						byte2 = 0x74;
						break;
					case 0x25c7:
						byte1 = 0x21;
						byte2 = 0x73;
						break;
					case 0x25cb:
						byte1 = 0x21;
						byte2 = 0x70;
						break;
					case 0x25ce:
						byte1 = 0x21;
						byte2 = 0x72;
						break;
					case 0x25cf:
						byte1 = 0x21;
						byte2 = 0x71;
						break;
					case 0x2605:
						byte1 = 0x21;
						byte2 = 0x6f;
						break;
					case 0x2606:
						byte1 = 0x21;
						byte2 = 0x6e;
						break;
					case 0x2640:
						byte1 = 0x21;
						byte2 = 0x62;
						break;
					case 0x2642:
						byte1 = 0x21;
						byte2 = 0x61;
						break;
					}
				} else if (ch >= 0x3000 && ch <= 0x3129) {
					tablepos = (ch - 0x3000) * 2;
					byte1 = convert.gb2312_from_unicode7[tablepos];
					byte2 = convert.gb2312_from_unicode7[tablepos + 1];
				} else if (ch >= 0x3220 && ch <= 0x3229) {
					byte1 = 0x22;
					byte2 = (byte)(0x65 + (ch - 0x3220));
				} else if (ch >= 0x4e00 && ch <= 0x9fa0) {
					tablepos = (ch - 0x4e00) * 2;
					byte1 = convert.gb2312_from_unicode8[tablepos];
					byte2 = convert.gb2312_from_unicode8[tablepos + 1];
				} else if (ch >= 0xff01 && ch <= 0xff5e) {
					tablepos = (ch - 0xff01) * 2;
					byte1 = convert.gb2312_from_unicode9[tablepos];
					byte2 = convert.gb2312_from_unicode9[tablepos + 1];
				} else if (ch == 0xffe0) {
					byte1 = 0x21;
					byte2 = 0x69;
				} else if (ch == 0xffe1) {
					byte1 = 0x21;
					byte2 = 0x6a;
				} else if (ch == 0xffe3) {
					byte1 = 0x21;
					byte2 = 0x7e;
				} else if (ch == 0xffe5) {
					byte1 = 0x21;
					byte2 = 0x24;
				}

				if (byte1 == 0 || byte2 == 0) {
					bytes[posn++] = (byte)'?';
				} else if ((posn + 1) >= byteLength) {
					throw new ArgumentException ("bytes", (Strings.GetString ("Arg_InsufficientSpace")));
				} else {
					bytes[posn++] = (byte)(byte1 + 0x80);
					bytes[posn++] = (byte)(byte2 + 0x80);
				}
			}
			
			return(posn - byteIndex);
		}
		
		// Get the number of characters needed to decode a byte buffer.
		public override int GetCharCount(byte[] bytes, int index,
						 int count)
		{
			if (bytes == null) {
				throw new ArgumentNullException("bytes");
			}
			if (index < 0 || index > bytes.Length) {
				throw new ArgumentOutOfRangeException("index", Strings.GetString("ArgRange_Array"));
			}
			if (count < 0 || index + count > bytes.Length) {
				throw new ArgumentOutOfRangeException("count", Strings.GetString("ArgRange_Array"));
			}
			
			char[] buffer = new char[count];
			return GetChars(bytes, index, count, buffer, 0);
		}
		
		// Get the characters that result from decoding a byte buffer.
		public override int GetChars(byte[] bytes, int byteIndex,
					     int byteCount, char[] chars,
					     int charIndex)
		{
			if (bytes == null) {
				throw new ArgumentNullException("bytes");
			}
			if (chars == null) {
				throw new ArgumentNullException("chars");
			}
			if (byteIndex < 0 || byteIndex > bytes.Length) {
				throw new ArgumentOutOfRangeException("byteIndex", Strings.GetString("ArgRange_Array"));
			}
			if (byteCount < 0 || byteIndex + byteCount > bytes.Length) {
				throw new ArgumentOutOfRangeException("byteCount", Strings.GetString("ArgRange_Array"));
			}
			if (charIndex < 0 || charIndex > chars.Length) {
				throw new ArgumentOutOfRangeException("charIndex", Strings.GetString("ArgRange_Array"));
			}

			int charLength = chars.Length;
			int posn = charIndex;
			int length = 0;
			int byte1, byte2, value;
			byte[] table = convert.gb2312_to_unicode;

			while(byteCount > 0) {
				byte1 = bytes[byteIndex++];
				byteCount--;
				length++;

				if (posn >= charLength) {
					throw new ArgumentException ("chars", (Strings.GetString ("Arg_InsufficientSpace")));
				}

				if (byte1 < 0x80) {
					chars[posn++] = (char)byte1;
					continue;
				}

				if ((byte1 <= 0xa0 &&
				     byte1 != 0x8e &&
				     byte1 != 0x8f) ||
				    byte1 > 0xfe) {
					value = 0;
				} else if (byteCount == 0) {
					// Missing second byte
					value = 0;
				} else {
					byte2 = bytes[byteIndex++];
					byteCount--;

					if (byte1 < 0x80 ||
					    (byte1 - 0x80) <= 0x20 ||
					    (byte1 - 0x80) > 0x77 ||
					    (byte2 - 0x80) <= 0x20 ||
					    (byte2 - 0x80) >= 0x7f) {
						value = 0;
					} else {
						int idx = ((byte1 - 0xa1) * 94 + (byte2 - 0xa1)) * 2;
						if (idx > 0x3fe2) {
							value = 0;
						} else {
							value = (int)(table[idx] | (table[idx + 1] << 8));
						}
					}
				}

				if (value != 0) {
					chars[posn++] = (char)value;
				} else {
					chars[posn++] = '?';
				}
			}
				
			return(posn - charIndex);
		}
		
		// Get the maximum number of bytes needed to encode a
		// specified number of characters.
		public override int GetMaxByteCount(int charCount)
		{
			if (charCount < 0) {
				throw new ArgumentOutOfRangeException("charCount", Strings.GetString("ArgRange_NonNegative"));
			}
			
			return(charCount * 2);
		}
		
		// Get the maximum number of characters needed to decode a
		// specified number of bytes.
		public override int GetMaxCharCount(int byteCount)
		{
			if (byteCount < 0) {
				throw new ArgumentOutOfRangeException("byteCount", Strings.GetString("ArgRange_NonNegative"));
			}
			return(byteCount);
		}
		
		// Get a decoder that handles a rolling GB2312 state.
		public override Decoder GetDecoder()
		{
			return(new CP936Decoder(convert));
		}

#if !ECMA_COMPAT		
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
		
		// Get the Windows code page represented by this object.
		public override int WindowsCodePage
		{
			get { return GB2312_CODE_PAGE; }
		}
#endif // !ECMA_COMPAT
		
		// Decoder that handles a rolling GB2312 state.
		private sealed class CP936Decoder : Decoder
		{
			private Gb2312Convert convert;
			private int lastByte;
			
			// Constructor.
			public CP936Decoder(Gb2312Convert convert) {
				this.convert = convert;
				this.lastByte = 0;
			}
			
			// Override inherited methods.
			public override int GetCharCount(byte[] bytes, int index, int count)
			{
				if (bytes == null) {
					throw new ArgumentNullException("bytes");
				}
				if (index < 0 || index > bytes.Length) {
					throw new ArgumentOutOfRangeException("index", Strings.GetString("ArgRange_Array"));
				}
				if (count < 0 || count > (bytes.Length - index)) {
					throw new ArgumentOutOfRangeException("count", Strings.GetString("ArgRange_Array"));
				}
				
				char[] buffer = new char[count * 2];
				return(GetChars(bytes, index, count, buffer, 0));
			}
			
			
			public override int GetChars(byte[] bytes,
						     int byteIndex,
						     int byteCount,
						     char[] chars,
						     int charIndex)
			{
				if (bytes == null) {
					throw new ArgumentNullException("bytes");
				}
				if (chars == null) {
					throw new ArgumentNullException("chars");
				}
				if (byteIndex < 0 || byteIndex > bytes.Length) {
					throw new ArgumentOutOfRangeException("byteIndex", Strings.GetString("ArgRange_Array"));
				}
				if (byteCount < 0 || byteIndex + byteCount > bytes.Length) {
					throw new ArgumentOutOfRangeException("byteCount", Strings.GetString("ArgRange_Array"));
				}
				if (charIndex < 0 || charIndex > chars.Length) {
					throw new ArgumentOutOfRangeException("charIndex", Strings.GetString("ArgRange_Array"));
				}

				int charLength = chars.Length;
				int posn = charIndex;
				int b, value;
				byte[] table = convert.gb2312_to_unicode;

				while(byteCount > 0) {
					b = bytes[byteIndex++];
					byteCount--;

					if (lastByte == 0) {
						if (posn >= charLength) {
							throw new ArgumentException ("chars", (Strings.GetString ("Arg_InsufficientSpace")));
						}

						if (b < 0x80) {
							// ASCII
							chars[posn++] = (char)b;
						} else if ((b <= 0xa0 &&
							    b != 0x8e &&
							    b != 0x8f) ||
							   b > 0xfe) {
							// Invalid first byte
							chars[posn++] = '?';
						} else {
							// First byte in a
							// double-byte sequence
							lastByte = b;
						}
					} else {
						// Second byte in a
						// double-byte sequence
						if (lastByte < 0x80 ||
						    (lastByte - 0x80) <= 0x20 ||
						    (lastByte - 0x80) > 0x77 ||
						    (b - 0x80) <= 0x20 ||
						    (b - 0x80) >= 0x7f) {
							// Invalid second byte
							chars[posn++] = '?';
						} else {
							int idx = ((lastByte - 0xa1) * 94 + (b - 0xa1) * 2);

							if (idx > 0x3fe2) {
								value = 0;
							} else {
								value = (int)(table[idx] | (table[idx + 1] << 8));
							}

							if (value != 0) {
								chars[posn++] = (char)value;
							} else {
								chars[posn++] = '?';
							}
						}

						lastByte = 0;
					}
				}

				return (posn - charIndex);
			}
		}
	}
	
	internal class ENCgb2312 : CP936
	{
		public ENCgb2312(): base () {}
	}
}
