//
// System.Text.ASCIIEncoding.cs
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Text {
        
        public class ASCIIEncoding : Encoding {
                public override int GetByteCount (char[] chars, int index, int count)
		{
                        // FIXME
                        return 0;
                }

                public override int GetBytes(char[] chars, int charIndex, int charCount,
					     byte[] bytes, int byteIndex)
		{
                        // FIXME
                        return 0;
                }

                public override int GetChars(byte[] bytes, int byteIndex, int byteCount,
					     char[] chars, int charIndex)
		{
                        // FIXME
                        return 0;
                }

                public override int GetMaxByteCount (int charCount)
		{
                        // FIXME
                        return 0;
                }

                public override int GetMaxCharCount (int byteCount)
		{
                        // FIXME
                        return 0;
                }
        }
}

