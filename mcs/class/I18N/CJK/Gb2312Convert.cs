//
// I18N.CJK.Gb2312Convert
//
// Author:
//   Alan Tam Siu Lung (Tam@SiuLung.com)
//   Dick Porter (dick@ximian.com)
//

using System;

namespace I18N.CJK
{
	internal sealed class Gb2312Convert
	{
		public byte[] gb2312_to_unicode;
		public byte[] gb2312_from_unicode1;
		public byte[] gb2312_from_unicode2;
		public byte[] gb2312_from_unicode3;
		public byte[] gb2312_from_unicode4;
		public byte[] gb2312_from_unicode5;
		public byte[] gb2312_from_unicode6;
		public byte[] gb2312_from_unicode7;
		public byte[] gb2312_from_unicode8;
		public byte[] gb2312_from_unicode9;
		
		private Gb2312Convert() 
		{
			using (CodeTable table = new CodeTable ("gb2312.table")) {
				gb2312_to_unicode = table.GetSection (1);

				gb2312_from_unicode1 = table.GetSection (2);
				gb2312_from_unicode2 = table.GetSection (3);
				gb2312_from_unicode3 = table.GetSection (4);
				gb2312_from_unicode4 = table.GetSection (5);
				gb2312_from_unicode5 = table.GetSection (6);
				gb2312_from_unicode6 = table.GetSection (7);
				gb2312_from_unicode7 = table.GetSection (8);
				gb2312_from_unicode8 = table.GetSection (9);
				gb2312_from_unicode9 = table.GetSection (10);
			}
		}
		
		
		// The one and only GB2312 conversion object in the system.
		private static Gb2312Convert convert;
		static readonly object lockobj = new object ();
		// Get the primary GB2312 conversion object.
		public static Gb2312Convert Convert
		{
			get {
				lock (lockobj) {
					if (convert == null) {
						convert = new Gb2312Convert ();
					}
					return(convert);
				}
			}
		}

		public char BytePairToChar (ref int lastByte, int b)
		{
			if (lastByte == 0) {
				if (b < 0x80) {
					// ASCII
					return (char) b;
				} else if ((b <= 0xa0 &&
					    b != 0x8e &&
					    b != 0x8f) ||
					   b > 0xfe) {
					// Invalid first byte
					return char.MinValue;
				} else {
					// First byte in a
					// double-byte sequence
					lastByte = b;
					return char.MaxValue;
				}
			}

			try {
				// Second byte in a
				// double-byte sequence
				if (lastByte < 0x80 ||
				    (lastByte - 0x80) <= 0x20 ||
				    (lastByte - 0x80) > 0x77 ||
				    (b - 0x80) <= 0x20 ||
				    (b - 0x80) >= 0x7f) {
					// Invalid second byte
					return char.MinValue;
				} else {
					int value;
					int idx = (((lastByte - 0xa1) * 94 + (b - 0xa1)) * 2);

					if (idx > 0x3fe2) {
						value = 0;
					} else {
						value = (int)(gb2312_to_unicode[idx] | (gb2312_to_unicode[idx + 1] << 8));
					}

					if (value != 0) {
						return (char)value;
					} else {
						return char.MinValue;
					}
				}
			} finally {
				lastByte = 0;
			}
		}

		public int UcsToGbk (int ch)
		{
			byte byte1 = 0, byte2 = 0;
			int tablepos;

			if (ch >= 0xa4 && ch <= 0x101) {
				tablepos = (ch - 0xa4) * 2;
				byte1 = gb2312_from_unicode1 [tablepos];
				byte2 = gb2312_from_unicode1 [tablepos + 1];
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
				byte1 = gb2312_from_unicode2 [tablepos];
				byte2 = gb2312_from_unicode2 [tablepos + 1];
			} else if (ch >= 0x401 && ch <= 0x451) {
				tablepos = (ch - 0x401) * 2;
				byte1 = gb2312_from_unicode3 [tablepos];
				byte2 = gb2312_from_unicode3 [tablepos + 1];
			} else if (ch >= 0x2015 && ch <= 0x203b) {
				tablepos = (ch - 0x2015) * 2;
				byte1 = gb2312_from_unicode4 [tablepos];
				byte2 = gb2312_from_unicode4 [tablepos + 1];
			} else if (ch >= 0x2103 && ch <= 0x22a5) {
				tablepos = (ch - 0x2103) * 2;
				byte1 = gb2312_from_unicode5 [tablepos];
				byte2 = gb2312_from_unicode5 [tablepos + 1];
			} else if (ch == 0x2312) {
				byte1 = 0x21;
				byte2 = 0x50;
			} else if (ch >= 0x2460 && ch <= 0x249b) {
				tablepos = (ch - 0x2460) * 2;
				byte1 = gb2312_from_unicode6 [tablepos];
				byte2 = gb2312_from_unicode6 [tablepos + 1];
			} else if (ch >= 0x2500 && ch <= 0x254b) {
				byte1 = 0x29;
				byte2 = (byte) (0x24 + (ch % 0x100));
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
				byte1 = gb2312_from_unicode7 [tablepos];
				byte2 = gb2312_from_unicode7 [tablepos + 1];
			} else if (ch >= 0x3220 && ch <= 0x3229) {
				byte1 = 0x22;
				byte2 = (byte) (0x65 + (ch - 0x3220));
			} else if (ch >= 0x4e00 && ch <= 0x9fa0) {
				tablepos = (ch - 0x4e00) * 2;
				byte1 = gb2312_from_unicode8 [tablepos];
				byte2 = gb2312_from_unicode8 [tablepos + 1];
			} else if (ch >= 0xff01 && ch <= 0xff5e) {
				tablepos = (ch - 0xff01) * 2;
				byte1 = gb2312_from_unicode9 [tablepos];
				byte2 = gb2312_from_unicode9 [tablepos + 1];
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

			if (byte1 == 0 || byte2 == 0)
				return -1; // invalid
			else
				return ((byte1 + 0x80) << 8) + byte2 + 0x80;
		}
	}
}
