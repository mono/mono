//
// GlobalObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Microsoft.JScript {

	public class GlobalObject {

		public const double Infinity = Double.PositiveInfinity;
		public const double NaN = Double.NaN;
		public static readonly Empty undefined = null;

		protected ActiveXObjectConstructor originalActiveXObjectField;
		protected ArrayConstructor originalArrayField;
		protected BooleanConstructor originalBooleanField;
		protected DateConstructor originalDateField;
		protected EnumeratorConstructor originalEnumeratorField;
		protected ErrorConstructor originalErrorField;
		protected ErrorConstructor originalEvalErrorField;
		protected FunctionConstructor originalFunctionField;
		protected NumberConstructor originalNumberField;
		protected ObjectConstructor originalObjectField;
		protected ObjectPrototype originalObjectPrototypeField;
		protected ErrorConstructor originalRangeErrorField;
		protected ErrorConstructor originalReferenceErrorField;
		protected RegExpConstructor originalRegExpField;
		protected StringConstructor originalStringField;
		protected ErrorConstructor originalSyntaxErrorField;
		protected ErrorConstructor originalTypeErrorField;
		protected ErrorConstructor originalURIErrorField;
		protected VBArrayConstructor originalVBArrayField;
		
		
		public static ActiveXObjectConstructor ActiveXObject {
			get { return ActiveXObjectConstructor.Ctr; }
		}

		public static ArrayConstructor Array {
			get { return ArrayConstructor.Ctr; }
		}

		public static BooleanConstructor Boolean {
			get { return BooleanConstructor.Ctr; }
		}

		public static Type boolean {
			get { throw new NotImplementedException (); }
		}

		public static Type @byte {
			get { throw new NotImplementedException (); }
		}

		public static Type @char {
			get { throw new NotImplementedException (); }
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_CollectGarbage)]
		public static void CollectGarbage ()
		{
			GC.Collect ();
		}

		public static DateConstructor Date {
			get { return DateConstructor.Ctr; }
		}

		public static Type @decimal {
			get { throw new NotImplementedException (); }
		}

		
		//
		// ECMA 3, 15.1.3 URI handling Function Properties
		//
		// The following are implementations of the algorithms
		// given in the ECMA specification for the hidden functions
		// 'Encode' and 'Decode'.
		//
		
		static string encode (string str, bool full_uri)
		{
			byte [] utf8_buf = null;
			StringBuilder sb = null;

			for (int k = 0, length = str.Length; k != length; ++k) {
				char c = str [k];
				if (encode_unescaped (c, full_uri)) {
					if (sb != null)
						sb.Append (c);
				} else {
					if (sb == null) {
						sb = new StringBuilder (length + 3);
						sb.Append (str);
						sb.Length = k;
						utf8_buf = new byte [6];
					}
					if (0xDC00 <= c && c <= 0xDFFF)
						throw new Exception ("msg.bad.uri");
					int v;
					if (c < 0xD800 || 0xDBFF < c)
						v = c;
					else {
						k++;
						if (k == length)
							throw new Exception ("msg.bad.uri");
						char c2 = str [k];
						if (!(0xDC00 <= c2 && c2 <= 0xDFFF))
							throw new Exception ("msg.bad.uri");
						v = ((c - 0xD800) << 10) + (c2 - 0xDC00) + 0x10000;;
					}
					int L = one_ucs4_to_utf8_char (utf8_buf, v);
					for (int j = 0; j < L; j++) {
						int d = 0xff & utf8_buf [j];
						sb.Append ('%');
						sb.Append (to_hex_char (d >> 4));
						sb.Append (to_hex_char (d & 0xf));
					}
				}
			}
			return (sb == null) ? str : sb.ToString ();
		}
		
		static char to_hex_char (int i)
		{
			if (i >> 4 != 0)
				throw new Exception ("to_hex_char, code bug");
			return (char) ((i < 10) ? i + '0' : i - 10 + 'a');
		}
		
		static readonly string URI_DECODE_RESERVED = ";/?:@&=+$,#";
		
		//
		// Convert one UCS-4 char and write it into a UTF-8 buffer, which must be
		// at least 6 bytes long.  Return the number of UTF-8 bytes of data written.
		//
		static int one_ucs4_to_utf8_char (byte [] utf8_buf, int ucs4char)
		{
			int utf8_length = 1;

			if ((ucs4char & ~0x7f) == 0)
				utf8_buf [0] = (byte) ucs4char;
			else {
				int i;
				int a = ucs4char >> 11;
				utf8_length = 2;
				while (a != 0) {
					a >>= 5;
					utf8_length++;
				}
				i = utf8_length;
				while (--i > 0) {
					utf8_buf [i] = (byte) ((ucs4char & 0x3F) | 0x80);
					ucs4char >>= 6;
				}
				utf8_buf [0] = (byte) (0x100 - (1 << (8 - utf8_length)) + ucs4char);
			}
			return utf8_length;
		}

		static bool encode_unescaped (char c, bool fullUri)
		{
			if (('A' <= c && c <= 'Z') || ('a' <= c && c <= 'z') || ('0' <= c && c <= '9'))
				return true;

			if ("-_.!~*'()".IndexOf (c) >= 0)
				return true;

			if (fullUri)
				return URI_DECODE_RESERVED.IndexOf (c) >= 0;

			return false;
		}
		
		static string decode (string str, bool fullUri)
		{
			char [] buf = null;
			int buf_top = 0;

			for (int k = 0, length = str.Length; k != length; ) {
				char c = str [k];
				if (c != '%') {
					if (buf != null)
						buf [buf_top++] = c;
					++k;
				} else {
					if (buf != null) {
						// decode always compress so result can not be bigger then
						// str.length()
						buf = new char [length];
						str.CopyTo (0, buf, 0, k);
						buf_top = k;
					}
					int start = k;
					if (k + 3 > length)
						throw new Exception ("msg.bad.uri");
					int b = un_hex (str [k + 1], str [k + 2]);
					if (b < 0)
						throw new Exception ("msg.bad.uri");
					k += 3;
					if ((b & 0x80) == 0)
						c = (char) b;
					else {
						// Decode UTF-8 sequence into ucs4Char and encode it into
						// UTF-16
						int utf8_tail, ucs4char, min_ucs4_char;
						if ((b & 0xC0) == 0x80) {
							// First  UTF-8 should be ouside 0x80..0xBF
							throw new Exception ("msg.bad.uri");
						} else if ((b & 0x20) == 0) {
							utf8_tail = 1; 
							ucs4char = b & 0x1F;
							min_ucs4_char = 0x80;
						} else if ((b & 0x10) == 0) {
							utf8_tail = 2; 
							ucs4char = b & 0x0F;
							min_ucs4_char = 0x800;
						} else if ((b & 0x08) == 0) {
							utf8_tail = 3; 
							ucs4char = b & 0x07;
							min_ucs4_char = 0x10000;
						} else if ((b & 0x04) == 0) {
							utf8_tail = 4; 
							ucs4char = b & 0x03;
							min_ucs4_char = 0x200000;
						} else if ((b & 0x02) == 0) {
							utf8_tail = 5; 
							ucs4char = b & 0x01;
							min_ucs4_char = 0x4000000;
						} else {
							// First UTF-8 can not be 0xFF or 0xFE
							throw new Exception ("msg.bad.uri");
						}
						if (k + 3 * utf8_tail > length)
							throw new Exception ("msg.bad.uri");
						for (int j = 0; j != utf8_tail; j++) {
							if (str [k] != '%')
								throw new Exception ("msg.bad.uri");
							b = un_hex (str [k + 1], str [k + 2]);
							if (b < 0 || (b & 0xC0) != 0x80)
								throw new Exception ("msg.bad.uri");
							ucs4char = (ucs4char << 6) | (b & 0x3F);
							k += 3;
						}
						// Check for overlongs and other should-not-present codes
						if (ucs4char < min_ucs4_char || ucs4char == 0xFFFE || ucs4char == 0xFFFF)
							ucs4char = 0xFFFD;
						if (ucs4char >= 0x10000) {
							ucs4char -= 0x10000;
							if (ucs4char > 0xFFFFF)
								throw new Exception ("msg.bad.uri");
							char h = (char) ((ucs4char >> 10) + 0xD800);
							c = (char) ((ucs4char & 0x3FF) + 0xDC00);
							buf [buf_top++] = h;
						} else
							c = (char) ucs4char;
					}
					if (fullUri && URI_DECODE_RESERVED.IndexOf (c) >= 0)
						for (int x = start; x != k; x++)
							buf [buf_top++] = str [x];
					else
						buf [buf_top++] = c;
				}
			}
			return (buf == null) ? str : new string (buf, 0, buf_top);
		}
		
		static int un_hex (char c1, char c2)
		{
			int i1 = un_hex (c1);
			int i2 = un_hex (c2);

			if (i1 >= 0 && i2 >= 0)
				return (i1 << 4) | i2;

			return -1;
		}

		static int un_hex (char c)
		{
			if ('A' <= c && c <= 'F')
				return c - 'A' + 10;
			else if ('a' <= c && c <= 'f')
				return c - 'a' + 10;
			else if ('0' <= c && c <= '9')
				return c - '0';
			else 
				return -1;
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_decodeURI)]
		public static String decodeURI (Object encodedURI)
		{
			string encoded_uri_str = Convert.ToString (encodedURI, true);
			return decode (encoded_uri_str, true);
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_decodeURIComponent)]
		public static String decodeURIComponent (Object encodedURI)
		{
			string encoded_uri_str = Convert.ToString (encodedURI, true);
			return decode (encoded_uri_str, false);
		}

		public static Type @double {
			get { throw new NotImplementedException (); }
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_encodeURI)]
		public static String encodeURI (Object uri)
		{
			string uri_str = Convert.ToString (uri, true);
			return encode (uri_str, true);
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_encodeURIComponent)]
		public static String encodeURIComponent (Object uriComponent)
		{
			string uri_str = Convert.ToString (uriComponent, true);
			return encode (uri_str, false);
		}

		public static EnumeratorConstructor Enumerator {
			get { return EnumeratorConstructor.Ctr; }
		}

		public static ErrorConstructor Error {
			get { return ErrorConstructor.Ctr; }
		}

		internal const string no_escape_chars =
			"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@*_+-./";

		[JSFunctionAttribute (0, JSBuiltin.Global_escape)]
		public static String escape (Object @string)
		{
			string str = Convert.ToString (@string);
			StringBuilder sb = new StringBuilder ();
			int n = str.Length;

			char c;
			for (int i = 0; i < n; i++) {
				c = str [i];
				if (no_escape_chars.IndexOf (c) != -1)
					sb.Append (c);
				else if (c < 256) {
					sb.Append ("%");
					sb.Append (((int) c).ToString ("X2"));
				} else {
					sb.Append ("%u");
					sb.Append (((int) c).ToString ("X4"));
				}
			}

			return sb.ToString ();
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_eval)]
		public static Object eval (Object x)
		{
			throw new NotImplementedException ();
		}

		public static ErrorConstructor EvalError {
			get { return ErrorConstructor.EvalErrorCtr; }
		}

		public static Type @float {
			get { throw new NotImplementedException (); }
		}

		public static FunctionConstructor Function {
			get { return FunctionConstructor.Ctr; }
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_GetObject)]
		public static Object GetObject (Object moniker, Object progId)
		{
			throw new NotImplementedException ();
		}		

		public static Type @int {
			get { throw new NotImplementedException (); }
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_isNaN)]
		public static bool isNaN (Object num)
		{
			double number = Convert.ToNumber (num);
			return Double.IsNaN (number);
		}

		[JSFunctionAttribute(0, JSBuiltin.Global_isFinite)]
		public static bool isFinite (double number)
		{
			return !Double.IsInfinity (number) && !Double.IsNaN (number);
		}

		public static Type @long {
			get { throw new NotImplementedException (); }
		}

		public static MathObject Math {
			get { return MathObject.Object; }
		}

		public static NumberConstructor Number {
			get { return NumberConstructor.Ctr; }
		}

		public static ObjectConstructor Object {
			get { return ObjectConstructor.Ctr; }
		}

		internal static Regex float_re = new Regex (@"[\d+\-.eE]+");

		[JSFunctionAttribute (0, JSBuiltin.Global_parseFloat)]
		public static double parseFloat (Object @string)
		{
			string string_obj = Convert.ToString (@string).Trim ();
			if (string_obj.StartsWith ("Infinity") || string_obj.StartsWith ("+Infinity"))
				return Double.PositiveInfinity;
			else if (string_obj.StartsWith ("-Infinity"))
				return Double.NegativeInfinity;

			if (string_obj.Trim () == "")
				return 0;
			// Would return an empty string if regular expression match returned 0.
			object o = float_re.Match (string_obj).Value;
			if (o is string){
				string os = (string) o;
				if (o != "")
					string_obj = os;
			}

			double result;
			if (Double.TryParse (string_obj, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
				return result;
			else
				return Double.NaN;
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_parseInt)]
		public static double parseInt(Object @string, Object radix)
		{
			String string_obj = Convert.ToString (@string).TrimStart (null).
				ToLower (CultureInfo.InvariantCulture);

			double result = 0;
			int _radix = 0;
			short sign = +1;

			if (radix != null)
				_radix = Convert.ToInt32 (radix);

			if ((_radix != 0) && ((_radix < 2) || (_radix > 36)))
				return Double.NaN;

			if (string_obj.Length == 0)
				return Double.NaN;
			else if (string_obj.Length > 1) {
				char first = string_obj [0];
				if (first == '+' || first == '-') {
					string_obj = string_obj.Substring (1);
					if (first == '-')
						sign = -1;
				}
			}

			if ((_radix == 0 || _radix == 16) && string_obj.StartsWith ("0x")) {
				string_obj = string_obj.Substring (2);
				_radix = 16;
			}

			if (_radix == 0 && string_obj.StartsWith ("0"))
				_radix = 8;

			if (_radix == 0)
				_radix = 10;

			bool has_result = false;
			for (int i = 0; i < string_obj.Length; i++) {
				char digit = string_obj [i];

				int digit_value = System.Array.IndexOf (NumberPrototype.Digits, digit);
				if (digit_value == -1 || digit_value >= _radix)
					break;

				result = (result * _radix) + digit_value;
				has_result = true;
			}

			if (!has_result)
				return Double.NaN;
			return result * sign;
		}

		public static ErrorConstructor RangeError {
			get { return ErrorConstructor.RangeErrorCtr; }
		}

		public static ErrorConstructor ReferenceError {
			get { return ErrorConstructor.ReferenceErrorCtr; }
		}

		public static RegExpConstructor RegExp {
			get { return RegExpConstructor.Ctr; }
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_ScriptEngine)]
		public static String ScriptEngine ()
		{
			return "JScript";
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_ScriptEngineBuildVersion)]
		public static int ScriptEngineBuildVersion ()
		{
			return 0;
		}

		[JSFunctionAttribute(0, JSBuiltin.Global_ScriptEngineMajorVersion)]
		public static int ScriptEngineMajorVersion ()
		{
			return 8;
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_ScriptEngineMinorVersion)]
		public static int ScriptEngineMinorVersion ()
		{
			return 0;
		}

		public static Type @sbyte {
			get { throw new NotImplementedException (); }
		}

		public static Type @short {
			get { throw new NotImplementedException (); }
		}

		public static StringConstructor String {
			get { return StringConstructor.Ctr; }
		}

		public static ErrorConstructor SyntaxError {
			get { return ErrorConstructor.SyntaxErrorCtr; }
		}

		public static ErrorConstructor TypeError {
			get { return ErrorConstructor.TypeErrorCtr; }
		}

		[JSFunctionAttribute (0, JSBuiltin.Global_unescape)]
		public static String unescape (Object @string)
		{
			string str = Convert.ToString (@string);
			StringBuilder sb = new StringBuilder ();
			int n = str.Length;

			char c;
			string d;
			int s;
			for (int i = 0; i < n; i++) {
				c = str [i];
				if (c != '%' || i == n - 1)
					sb.Append (c);
				else {
					i++;
					if (str [i] == 'u' && i < n - 4) {
						i++;
						s = 4;
					} else if (i <= n - 2)
						s = 2;
					else
						s = 0;
					
					d = str.Substring (i, s);
					i += s - 1;

					char res = (char) parseInt (d, 16);
					if (((int) res).ToString ("X" + s) == d)
						sb.Append ((char) res);
					else {
						sb.Append ('%');
						if (s == 4)
							sb.Append ('u');
						sb.Append (d);
					}
				}
			}

			return sb.ToString ();
		}

		public static ErrorConstructor URIError {
			get { return ErrorConstructor.URIErrorCtr; }
		}

		public static VBArrayConstructor VBArray {
			get { return VBArrayConstructor.Ctr; }
		}

		public static Type @void {
			get { throw new NotImplementedException (); }
		}
		
		public static Type @uint {
			get { throw new NotImplementedException (); }
		}

		public static Type @ulong {
			get { throw new NotImplementedException (); }
		}

		public static Type @ushort {
			get { throw new NotImplementedException (); }
		}		
	}
}
