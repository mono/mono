//
// System.Text.UTF8Encoding.cs
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Text {
        
        public class UTF8Encoding : Encoding {
                public override int GetByteCount(char[] chars, int index, int count) {
                        // FIXME
                        return 0;
                }

                public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) {
                        if (chars == null || bytes == null)
				throw new ArgumentNullException ();
			if (charIndex < 0 || charCount < 0 || byteIndex < 0 ||
			    charIndex + charCount > chars.Length ||
			    byteIndex + charCount > bytes.Length)
				throw new ArgumentOutOfRangeException ();

			// fixme: do realy unicode conversion
			
			for (int i = 0; i < charCount; i++) {
				bytes [byteIndex + i] = (byte)chars [charIndex + i];
				
			}
			
                        return charCount;
                }

                public override char[] GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) {
                        // FIXME
                        return null;
                }

                public override int GetMaxByteCount(int charCount) {
                        return charCount*3;
                }

                public override int GetMaxCharCount(int byteCount) {
                        return byteCount;
                }
        }
}

