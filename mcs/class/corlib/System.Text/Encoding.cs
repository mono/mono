// -*- Mode: C; tab-width: 8; indent-tabs-mode: t; c-basic-offset: 8 -*-
//
// System.Text.Encoding.cs
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.CompilerServices;

namespace System.Text {

	[Serializable]
	public abstract class Encoding {
                private static ASCIIEncoding ascii_encoding;
                private static UnicodeEncoding big_endian_unicode;
                private static UnicodeEncoding unicode_encoding;
                private static UTF7Encoding utf7_encoding;
                private static UTF8Encoding utf8_encoding;

                private int codepage;
   
                protected string body_name;
                protected string encoding_name;
                protected string header_name;
                protected string web_name;

		protected bool is_browser_display = false;
		protected bool is_browser_save = false;
		protected bool is_mail_news_display = false;
		protected bool is_mail_news_save = false;
		
		private Encoder default_encoder = null;
		private Decoder default_decoder = null;
		
		// used for iconv 
		private string  iconv_name;
		private bool    big_endian;
		private Encoder iconv_encoder = null;
		private Decoder iconv_decoder = null;

                protected Encoding()
		{
                }

                protected Encoding (int codepage)
		{
                        this.codepage = codepage;
                }

		internal protected Encoding (string name, bool big_endian)
		{
			this.iconv_name = name;
			this.big_endian = big_endian;
			
			iconv_decoder = new IConvDecoder (iconv_name, big_endian);
			iconv_encoder = new IConvEncoder (iconv_name, big_endian);
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
                                        utf7_encoding = new UTF7Encoding();
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
                                return is_browser_display;
                        }
                }

                public virtual bool IsBrowserSave {
                        get {
				return is_browser_save;
                        }
                }

                public virtual bool IsMailNewsDisplay {
                        get {
                                return is_mail_news_display;
                        }
                }

                public virtual bool IsMailNewsSave {
                        get {
                                return is_mail_news_save;
                        }
                }

                public virtual string WebName {
                        get {
                                return web_name;
                        }
                }

		[MonoTODO]
                public virtual int WindowsCodePage {
                        get {
                                // FIXME
                                return 0;
                        }
                }

                public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding, byte[] bytes)
		{
			return dstEncoding.GetBytes (srcEncoding.GetChars (bytes));
                }

                public static byte[] Convert(Encoding srcEncoding, Encoding dstEncoding,
					     byte[] bytes, int index, int count)
		{
			return dstEncoding.GetBytes (srcEncoding.GetChars (bytes, index, count));
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

                public virtual int GetByteCount (char[] chars)
		{
			return GetByteCount (chars, 0, chars.Length);
                }

                public virtual int GetByteCount (string s)
		{
			char [] chars = s.ToCharArray ();
			
 			return GetByteCount (chars, 0, chars.Length);
                }

                public virtual int GetByteCount (char[] chars, int index, int count)
		{
			return iconv_encoder.GetByteCount (chars, index, count, false);
		}

                public virtual byte[] GetBytes(char[] chars)
		{
			return GetBytes (chars, 0, chars.Length);
                }

                public virtual byte[] GetBytes(string s)
		{
                        char [] chars = s.ToCharArray (); 
                        return GetBytes (chars, 0, chars.Length);
                }

                public virtual byte[] GetBytes(char[] chars, int index, int count)
		{
			int bc = GetByteCount (chars, index, count);
			byte [] bytes = new byte [bc];
			
			int len = GetBytes (chars, index, count, bytes, 0);
			byte [] res = new byte [len];

			Array.Copy (bytes, res, len);
			
			return res;
                }

                public virtual int GetBytes (char[] chars, int charIndex, int charCount,
					     byte[] bytes, int byteIndex)
		{
			return iconv_encoder.GetBytes (chars, charIndex, charCount, bytes, byteIndex, true);
		}

                public virtual int GetBytes(string s, int charIndex, int charCount,
					    byte[] bytes, int byteIndex)
		{
			return GetBytes (s.ToCharArray (), charIndex, charCount, bytes, byteIndex);
                }

                public virtual int GetCharCount (byte[] bytes)
		{
			return GetCharCount (bytes, 0, bytes.Length);
                }

                public virtual int GetCharCount (byte[] bytes, int index, int count)
		{
			return iconv_decoder.GetCharCount (bytes, index, count);
		}

                public virtual char[] GetChars (byte[] bytes)
		{
			return GetChars (bytes, 0, bytes.Length);
                }

                public virtual char[] GetChars (byte[] bytes, int index, int count)
		{
			int cc = GetMaxCharCount (count);
			char [] chars = new char [cc];

			int len = GetChars (bytes, index, count, chars, 0);
			char [] res = new char [len];
			
			Array.Copy (chars, res, len);

			return res;
		}

                public virtual int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			return iconv_decoder.GetChars (bytes, byteIndex, byteCount, chars, charIndex);
		}

                public virtual Decoder GetDecoder()
		{
			if (iconv_name != null)
				return new IConvDecoder (iconv_name, big_endian);
			
			if (default_decoder == null)
				default_decoder = new DefaultDecoder (this);
			
                        return default_decoder;
                }

                public virtual Encoder GetEncoder() 
		{
			if (iconv_name != null)
				return new IConvEncoder (iconv_name, big_endian);

			if (default_encoder == null)
				default_encoder = new DefaultEncoder (this);
			
                        return default_encoder;
                }

		[MonoTODO]
                public static Encoding GetEncoding (int codepage)
		{
                        // FIXME
                        return null;
                }
		
		[MonoTODO]
                public static Encoding GetEncoding (string name)
		{
                        // FIXME
                        return null;
                }

		[MonoTODO]
                public override int GetHashCode()
		{
                        // FIXME
                        return 0;
                }

                public abstract int GetMaxByteCount (int charCount);

                public abstract int GetMaxCharCount (int byteCount);

		[MonoTODO]
                public virtual byte[] GetPreamble()
		{
                        // FIXME
                        return null;
                }

                public virtual string GetString(byte[] bytes)
		{
                        return GetString (bytes, 0, bytes.Length);
                }

                public virtual string GetString(byte[] bytes, int index, int count)
		{
			char [] chars = GetChars (bytes, index, count);

                        return new String (chars);
                }

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		internal extern static IntPtr IConvNewEncoder (string name, bool big_endian);

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		internal extern static IntPtr IConvNewDecoder (string name, bool big_endian);

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		internal extern static void IConvReset (IntPtr converter);

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		internal extern static int IConvGetByteCount (IntPtr converter, char[] chars,
							      int index, int count);

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		internal extern static int IConvGetBytes (IntPtr converter, char[] chars, int charIndex,
							  int charCount, byte[] bytes, int byteIndex);
		
		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		internal extern static int IConvGetCharCount (IntPtr converter, byte[] bytes,
							      int index, int count);

		[MethodImplAttribute(System.Runtime.CompilerServices.MethodImplOptions.InternalCall)]
		internal extern static int IConvGetChars (IntPtr converter, byte[] bytes, int byteIndex,
							  int byteCount, char[] chars, int charIndex);
        }
}
