// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Text.UnicodeEncoding.cs
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Text {
        
        public class UnicodeEncoding : Encoding {
                public UnicodeEncoding() {
                }

                public UnicodeEncoding(bool bigEndian, bool byteOrderMark) {
                }

                public override int GetByteCount(char[] chars, int index, int count) {
                        // FIXME
                        return 0;
                }

                public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex) {
                        // FIXME
                        return 0;
                }

                public override char[] GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex) {
                        // FIXME
                        return null;
                }

                public override int GetMaxByteCount(int charCount) {
                        // FIXME
                        return 0;
                }

                public override int GetMaxCharCount(int byteCount) {
                        // FIXME
                        return 0;
                }
        }
}
