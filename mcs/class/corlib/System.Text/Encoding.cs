//
// System.Text.Encoding.cs
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//


namespace System.Text {

        public abstract class Encoding {
        
                private static ASCIIEncoding asciiEncoding;
                private static UnicodeEncoding bigEndianUnicode;
                private static UnicodeEncoding unicodeEncoding;
                private static UTF7Encoding utf7Encoding;
                private static UTF8Encoding utf8Encoding;

                private int codepage;
                private string bodyName;
                private string encodingName;
                private string headerName;

                protected Encoding() {
                }

                protected Encoding(int codepage) {
                        this.codepage = codepage;
                }

                public static Encoding ASCII {
                        get {
                                if (asciiEncoding == null)
                                        asciiEncoding = new ASCIIEncoding ();
                                return asciiEncoding;
                        }
                }

                public static Encoding BigEndianUnicode {
                        get {
                                if (bigEndianUnicode == null)
                                        bigEndianUnicode = new UnicodeEncoding (true, true);
                                return bigEndianUnicode;
                        }
                }

                public virtual string BodyName {
                        get {
                                return bodyName;
                        }
                }

                public virtual int CodePage {
                        get {
                                return codepage;
                        }
                }

                public static Encoding Default {
                        get {
                                return ASCII;
                        }
                }

                public virtual string EncodingName {
                        get {
                                return encodingName;
                        }
                }

                public virtual string HeaderName {
                        get {
                                return headerName;
                        }
                }

                public virtual bool IsBrowserDisplay {
                        get {
                                // FIXME
                                return false;
                        }
                }

                public virtual bool IsBrowserSave {
                        get {
                                // FIXME
                                return false;
                        }
                }

                public virtual bool IsMailNewsDisplay {
                        get {
                                // FIXME
                                return false;
                        }
                }

                public virtual bool IsMailNewsSave {
                        get {
                                // FIXME
                                return false;
                        }
                }

                public static Encoding Unicode {
                        get {
                                if (unicodeEncoding == null) {
                                        unicodeEncoding = new UnicodeEncoding();
                                }
                                return unicodeEncoding;
                        }
                }

                public static Encoding UTF7 {
                        get {
                                if (utf7Encoding == null) {
                                        utf7Encoding = new UTF7Encoding();
                                }
                                return utf7Encoding;
                        }
                }

                public static Encoding UTF8 {
                        get {
                                if (utf8Encoding == null) {
                                        utf8Encoding = new UTF8Encoding();
                                }
                                return utf8Encoding;
                        }
                }

                public virtual string WebName {
                        get {
                                // FIXME
                                return "";
                        }
                }

                public virtual int WindowsCodePage {
                        get {
                                // FIXME
                                return 0;
                        }
                }

                public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding, byte[] bytes) {
                        // FIXME
                        return null;
                }

                public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding, byte[] bytes, int index, int count) {
                        // FIXME
                        return null;
                }

                public override bool Equals(object value) {
                        // FIXME
                        return false;
                }

                public virtual int GetByteCount(char[] chars) {
                        // FIXME
                        return 0;
                }

                public virtual int GetByteCount(string s) {
                        // FIXME
                        return 0;
                }

                public abstract int GetByteCount(char[] chars, int index, int count);

                public virtual byte[] GetBytes(char[] chars) {
                        // FIXME
                        return null;
                }

                public virtual byte[] GetBytes(string s) {
                        // FIXME
                        return null;
                }

                public virtual byte[] GetBytes(char[] chars, int index, int count) {
                        // FIXME
                        return null;
                }

                public abstract int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex);

                public virtual byte[] GetBytes(string s, int charIndex, int charCount, byte[] bytes, int byteIndex) {
                        // FIXME
                        return null;
                }

                public virtual int GetCharCount(byte[] bytes) {
                        // FIXME
                        return 0;
                }

                public virtual int GetCharCount(byte[] bytes, int index, int count) {
                        // FIXME
                        return 0;
                }

                public virtual char[] GetChars(byte[] bytes) {
                        // FIXME
                        return null;
                }

                public virtual char[] GetChars(byte[] bytes, int index, int count) {
                        // FIXME
                        return null;
                }

                public abstract char[] GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex);

                public virtual Decoder GetDecoder() {
                        // FIXME
                        return null;
                }

                public virtual Encoder GetEncoder() {
                        // FIXME
                        return null;
                }

                public virtual Encoding GetEncoding(int codepage) {
                        // FIXME
                        return null;
                }

                public virtual Encoding GetEncoding(string name) {
                        return null;
                }

                public override int GetHashCode() {
                        // FIXME
                        return 0;
                }

                public abstract int GetMaxByteCount(int charCount);

                public abstract int GetMaxCharCount(int byteCount);

                public virtual byte[] GetPreamble() {
                        // FIXME
                        return null;
                }

                public virtual string GetString(byte[] bytes) {
                        // FIXME
                        return null;
                }

                public virtual string GetString(byte[] bytes, int index, int count) {
                        // FIXME
                        return null;
                }
        }
}
