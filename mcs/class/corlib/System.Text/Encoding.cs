// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
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
                private static ASCIIEncoding ascii_encoding;
                private static UnicodeEncoding big_endian_unicode;
                private static UnicodeEncoding unicode_encoding;
                private static Utf7_Encoding utf7_encoding;
                private static UTF8Encoding utf8_encoding;

                private int codepage;
                protected string body_name;
                protected string encoding_name;
                protected string header_name;

                protected Encoding()
		{
                }

                protected Encoding (int codepage)
		{
                        this.codepage = codepage;
                }

                public static Encoding ASCII {
                        get {
                                if (ascii_encoding == null)
                                        ascii_encoding = new ASCIIEncoding ();
                                return ascii_encoding;
                        }
                }

                public static Encoding BigEndianUnicode {
                        get {
                                if (big_endian_unicode == null)
                                        big_endian_unicode = new UnicodeEncoding (true, true);
                                return big_endian_unicode;
                        }
                }

                public virtual string BodyName {
                        get {
                                return body_name;
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
                                return encoding_name;
                        }
                }

                public virtual string HeaderName {
                        get {
                                return header_name;
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
                                if (unicode_encoding == null) {
                                        unicode_encoding = new UnicodeEncoding();
                                }
                                return unicode_encoding;
                        }
                }

                public static Encoding UTF7 {
                        get {
                                if (utf7_encoding == null) {
                                        utf7_encoding = new Utf7_Encoding();
                                }
                                return utf7_encoding;
                        }
                }

                public static Encoding UTF8 {
                        get {
                                if (utf8_encoding == null) {
                                        utf8_encoding = new UTF8Encoding();
                                }
                                return utf8_encoding;
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

                public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding, byte[] bytes)
		{
                        // FIXME
                        return null;
                }

                public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding,
					     byte[] bytes, int index, int count)
		{
                        // FIXME
                        return null;
                }

                public override bool Equals (object value)
		{
			if (!(value is Encoding))
				return false;

			Encoding e = (Encoding) value;

			if (e.codepage != codepage)
				return false;

			if (e.body_name != body_name)
				return false;

			if (e.encoding_name != encoding_name)
				return false;

			if (e.header_name != header_name)
				return false;
			
                        return true;
                }

                public virtual int GetByteCount(char[] chars)
		{
                        // FIXME
                        return 0;
                }

                public virtual int GetByteCount(string s)
		{
                        // FIXME
                        return 0;
                }

                public abstract int GetByteCount (char[] chars, int index, int count);

                public virtual byte[] GetBytes(char[] chars)
		{
                        // FIXME
                        return null;
                }

                public virtual byte[] GetBytes(string s)
		{
                        // FIXME
                        return null;
                }

                public virtual byte[] GetBytes(char[] chars, int index, int count)
		{
                        // FIXME
                        return null;
                }

                public abstract int GetBytes (char[] chars, int charIndex, int charCount,
					      byte[] bytes, int byteIndex);

                public virtual int GetBytes(string s, int charIndex, int charCount,
					    byte[] bytes, int byteIndex)
		{
                        // FIXME
                        return 0;
                }

                public virtual int GetCharCount (byte[] bytes)
		{
                        // FIXME
                        return 0;
                }

                public virtual int GetCharCount (byte[] bytes, int index, int count)
		{
                        // FIXME
                        return 0;
                }

                public virtual char[] GetChars (byte[] bytes)
		{
                        // FIXME
                        return null;
                }

                public virtual char[] GetChars (byte[] bytes, int index, int count)
		{
                        // FIXME
                        return null;
                }

                public abstract int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex);

                public virtual Decoder GetDecoder()
		{
                        // FIXME
                        return null;
                }

                public virtual Encoder GetEncoder() 
		{
                        // FIXME
                        return null;
                }

                public virtual Encoding GetEncoding (int codepage)
		{
                        // FIXME
                        return null;
                }

                public virtual Encoding GetEncoding (string name)
		{
                        return null;
                }

                public override int GetHashCode()
		{
                        // FIXME
                        return 0;
                }

                public abstract int GetMaxByteCount (int charCount);

                public abstract int GetMaxCharCount (int byteCount);

                public virtual byte[] GetPreamble()
		{
                        // FIXME
                        return null;
                }

                public virtual string GetString(byte[] bytes)
		{
                        // FIXME
                        return null;
                }

                public virtual string GetString(byte[] bytes, int index, int count)
		{
                        // FIXME
                        return null;
                }
        }
}
