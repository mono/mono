// 
// System.Web.HttpUtility
//
// Authors:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Wictor Wilén (decode/encode functions) (wictor@ibizkit.se)
//   Tim Coleman (tim@timcoleman.com)
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Web.Util;

namespace System.Web {

#if !MOBILE
	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#endif
	public sealed class HttpUtility
	{
		sealed class HttpQSCollection : NameValueCollection
		{
			public override string ToString ()
			{
				int count = Count;
				if (count == 0)
					return "";
				StringBuilder sb = new StringBuilder ();
				string [] keys = AllKeys;
				for (int i = 0; i < count; i++) {
					sb.AppendFormat ("{0}={1}&", keys [i], this [keys [i]]);
				}
				if (sb.Length > 0)
					sb.Length--;
				return sb.ToString ();
			}
		}
		
		#region Constructors

		public HttpUtility () 
		{
		}
	
		#endregion // Constructors
	
		#region Methods
	
		public static void HtmlAttributeEncode (string s, TextWriter output) 
		{
			if (output == null) {
#if NET_4_0
				throw new ArgumentNullException ("output");
#else
				throw new NullReferenceException (".NET emulation");
#endif
			}
#if NET_4_0
			HttpEncoder.Current.HtmlAttributeEncode (s, output);
#else
			output.Write (HttpEncoder.HtmlAttributeEncode (s));
#endif
		}
	
		public static string HtmlAttributeEncode (string s) 
		{
#if NET_4_0
			if (s == null)
				return null;
			
			using (var sw = new StringWriter ()) {
				HttpEncoder.Current.HtmlAttributeEncode (s, sw);
				return sw.ToString ();
			}
#else
			return HttpEncoder.HtmlAttributeEncode (s);
#endif
		}
	
		public static string UrlDecode (string str) 
		{
			return UrlDecode(str, Encoding.UTF8);
		}
	
		static char [] GetChars (MemoryStream b, Encoding e)
		{
			return e.GetChars (b.GetBuffer (), 0, (int) b.Length);
		}

		static void WriteCharBytes (IList buf, char ch, Encoding e)
		{
			if (ch > 255) {
				foreach (byte b in e.GetBytes (new char[] { ch }))
					buf.Add (b);
			} else
				buf.Add ((byte)ch);
		}
		
		public static string UrlDecode (string s, Encoding e)
		{
			if (null == s) 
				return null;

			if (s.IndexOf ('%') == -1 && s.IndexOf ('+') == -1)
				return s;
			
			if (e == null)
				e = Encoding.UTF8;

			long len = s.Length;
			var bytes = new List <byte> ();
			int xchar;
			char ch;
			
			for (int i = 0; i < len; i++) {
				ch = s [i];
				if (ch == '%' && i + 2 < len && s [i + 1] != '%') {
					if (s [i + 1] == 'u' && i + 5 < len) {
						// unicode hex sequence
						xchar = GetChar (s, i + 2, 4);
						if (xchar != -1) {
							WriteCharBytes (bytes, (char)xchar, e);
							i += 5;
						} else
							WriteCharBytes (bytes, '%', e);
					} else if ((xchar = GetChar (s, i + 1, 2)) != -1) {
						WriteCharBytes (bytes, (char)xchar, e);
						i += 2;
					} else {
						WriteCharBytes (bytes, '%', e);
					}
					continue;
				}

				if (ch == '+')
					WriteCharBytes (bytes, ' ', e);
				else
					WriteCharBytes (bytes, ch, e);
			}
			
			byte[] buf = bytes.ToArray ();
			bytes = null;
			return e.GetString (buf);
			
		}
	
		public static string UrlDecode (byte [] bytes, Encoding e)
		{
			if (bytes == null)
				return null;

			return UrlDecode (bytes, 0, bytes.Length, e);
		}

		static int GetInt (byte b)
		{
			char c = (char) b;
			if (c >= '0' && c <= '9')
				return c - '0';

			if (c >= 'a' && c <= 'f')
				return c - 'a' + 10;

			if (c >= 'A' && c <= 'F')
				return c - 'A' + 10;

			return -1;
		}

		static int GetChar (byte [] bytes, int offset, int length)
		{
			int value = 0;
			int end = length + offset;
			for (int i = offset; i < end; i++) {
				int current = GetInt (bytes [i]);
				if (current == -1)
					return -1;
				value = (value << 4) + current;
			}

			return value;
		}

		static int GetChar (string str, int offset, int length)
		{
			int val = 0;
			int end = length + offset;
			for (int i = offset; i < end; i++) {
				char c = str [i];
				if (c > 127)
					return -1;

				int current = GetInt ((byte) c);
				if (current == -1)
					return -1;
				val = (val << 4) + current;
			}

			return val;
		}
		
		public static string UrlDecode (byte [] bytes, int offset, int count, Encoding e)
		{
			if (bytes == null)
				return null;
			if (count == 0)
				return String.Empty;

			if (bytes == null)
				throw new ArgumentNullException ("bytes");

			if (offset < 0 || offset > bytes.Length)
				throw new ArgumentOutOfRangeException ("offset");

			if (count < 0 || offset + count > bytes.Length)
				throw new ArgumentOutOfRangeException ("count");

			StringBuilder output = new StringBuilder ();
			MemoryStream acc = new MemoryStream ();

			int end = count + offset;
			int xchar;
			for (int i = offset; i < end; i++) {
				if (bytes [i] == '%' && i + 2 < count && bytes [i + 1] != '%') {
					if (bytes [i + 1] == (byte) 'u' && i + 5 < end) {
						if (acc.Length > 0) {
							output.Append (GetChars (acc, e));
							acc.SetLength (0);
						}
						xchar = GetChar (bytes, i + 2, 4);
						if (xchar != -1) {
							output.Append ((char) xchar);
							i += 5;
							continue;
						}
					} else if ((xchar = GetChar (bytes, i + 1, 2)) != -1) {
						acc.WriteByte ((byte) xchar);
						i += 2;
						continue;
					}
				}

				if (acc.Length > 0) {
					output.Append (GetChars (acc, e));
					acc.SetLength (0);
				}

				if (bytes [i] == '+') {
					output.Append (' ');
				} else {
					output.Append ((char) bytes [i]);
				}
			}

			if (acc.Length > 0) {
				output.Append (GetChars (acc, e));
			}
			
			acc = null;
			return output.ToString ();
		}
	
		public static byte [] UrlDecodeToBytes (byte [] bytes)
		{
			if (bytes == null)
				return null;

			return UrlDecodeToBytes (bytes, 0, bytes.Length);
		}

		public static byte [] UrlDecodeToBytes (string str)
		{
			return UrlDecodeToBytes (str, Encoding.UTF8);
		}

		public static byte [] UrlDecodeToBytes (string str, Encoding e)
		{
			if (str == null)
				return null;

			if (e == null)
				throw new ArgumentNullException ("e");

			return UrlDecodeToBytes (e.GetBytes (str));
		}

		public static byte [] UrlDecodeToBytes (byte [] bytes, int offset, int count)
		{
			if (bytes == null)
				return null;
			if (count == 0)
				return new byte [0];

			int len = bytes.Length;
			if (offset < 0 || offset >= len)
				throw new ArgumentOutOfRangeException("offset");

			if (count < 0 || offset > len - count)
				throw new ArgumentOutOfRangeException("count");

			MemoryStream result = new MemoryStream ();
			int end = offset + count;
			for (int i = offset; i < end; i++){
				char c = (char) bytes [i];
				if (c == '+') {
					c = ' ';
				} else if (c == '%' && i < end - 2) {
					int xchar = GetChar (bytes, i + 1, 2);
					if (xchar != -1) {
						c = (char) xchar;
						i += 2;
					}
				}
				result.WriteByte ((byte) c);
			}

			return result.ToArray ();
		}

		public static string UrlEncode(string str) 
		{
			return UrlEncode(str, Encoding.UTF8);
		}
	
		public static string UrlEncode (string s, Encoding Enc) 
		{
			if (s == null)
				return null;

			if (s == String.Empty)
				return String.Empty;

			bool needEncode = false;
			int len = s.Length;
			for (int i = 0; i < len; i++) {
				char c = s [i];
				if ((c < '0') || (c < 'A' && c > '9') || (c > 'Z' && c < 'a') || (c > 'z')) {
					if (HttpEncoder.NotEncoded (c))
						continue;

					needEncode = true;
					break;
				}
			}

			if (!needEncode)
				return s;

			// avoided GetByteCount call
			byte [] bytes = new byte[Enc.GetMaxByteCount(s.Length)];
			int realLen = Enc.GetBytes (s, 0, s.Length, bytes, 0);
			return Encoding.ASCII.GetString (UrlEncodeToBytes (bytes, 0, realLen));
		}
	  
		public static string UrlEncode (byte [] bytes)
		{
			if (bytes == null)
				return null;

			if (bytes.Length == 0)
				return String.Empty;

			return Encoding.ASCII.GetString (UrlEncodeToBytes (bytes, 0, bytes.Length));
		}

		public static string UrlEncode (byte [] bytes, int offset, int count)
		{
			if (bytes == null)
				return null;

			if (bytes.Length == 0)
				return String.Empty;

			return Encoding.ASCII.GetString (UrlEncodeToBytes (bytes, offset, count));
		}

		public static byte [] UrlEncodeToBytes (string str)
		{
			return UrlEncodeToBytes (str, Encoding.UTF8);
		}

		public static byte [] UrlEncodeToBytes (string str, Encoding e)
		{
			if (str == null)
				return null;

			if (str.Length == 0)
				return new byte [0];

			byte [] bytes = e.GetBytes (str);
			return UrlEncodeToBytes (bytes, 0, bytes.Length);
		}

		public static byte [] UrlEncodeToBytes (byte [] bytes)
		{
			if (bytes == null)
				return null;

			if (bytes.Length == 0)
				return new byte [0];

			return UrlEncodeToBytes (bytes, 0, bytes.Length);
		}

		public static byte [] UrlEncodeToBytes (byte [] bytes, int offset, int count)
		{
			if (bytes == null)
				return null;
#if NET_4_0
			return HttpEncoder.Current.UrlEncode (bytes, offset, count);
#else
			return HttpEncoder.UrlEncodeToBytes (bytes, offset, count);
#endif
		}

		public static string UrlEncodeUnicode (string str)
		{
			if (str == null)
				return null;

			return Encoding.ASCII.GetString (UrlEncodeUnicodeToBytes (str));
		}

		public static byte [] UrlEncodeUnicodeToBytes (string str)
		{
			if (str == null)
				return null;

			if (str.Length == 0)
				return new byte [0];

			MemoryStream result = new MemoryStream (str.Length);
			foreach (char c in str){
				HttpEncoder.UrlEncodeChar (c, result, true);
			}
			return result.ToArray ();
		}

		/// <summary>
		/// Decodes an HTML-encoded string and returns the decoded string.
		/// </summary>
		/// <param name="s">The HTML string to decode. </param>
		/// <returns>The decoded text.</returns>
		public static string HtmlDecode (string s) 
		{
#if NET_4_0
			if (s == null)
				return null;
			
			using (var sw = new StringWriter ()) {
				HttpEncoder.Current.HtmlDecode (s, sw);
				return sw.ToString ();
			}
#else
			return HttpEncoder.HtmlDecode (s);
#endif
		}
	
		/// <summary>
		/// Decodes an HTML-encoded string and sends the resulting output to a TextWriter output stream.
		/// </summary>
		/// <param name="s">The HTML string to decode</param>
		/// <param name="output">The TextWriter output stream containing the decoded string. </param>
		public static void HtmlDecode(string s, TextWriter output) 
		{
			if (output == null) {
#if NET_4_0
				throw new ArgumentNullException ("output");
#else
				throw new NullReferenceException (".NET emulation");
#endif
			}
				
			if (!String.IsNullOrEmpty (s)) {
#if NET_4_0
				HttpEncoder.Current.HtmlDecode (s, output);
#else
				output.Write (HttpEncoder.HtmlDecode (s));
#endif
			}
		}

		public static string HtmlEncode (string s)
		{
#if NET_4_0
			if (s == null)
				return null;
			
			using (var sw = new StringWriter ()) {
				HttpEncoder.Current.HtmlEncode (s, sw);
				return sw.ToString ();
			}
#else
			return HttpEncoder.HtmlEncode (s);
#endif
		}
		
		/// <summary>
		/// HTML-encodes a string and sends the resulting output to a TextWriter output stream.
		/// </summary>
		/// <param name="s">The string to encode. </param>
		/// <param name="output">The TextWriter output stream containing the encoded string. </param>
		public static void HtmlEncode(string s, TextWriter output) 
		{
			if (output == null) {
#if NET_4_0
				throw new ArgumentNullException ("output");
#else
				throw new NullReferenceException (".NET emulation");
#endif
			}
				
			if (!String.IsNullOrEmpty (s)) {
#if NET_4_0
				HttpEncoder.Current.HtmlEncode (s, output);
#else
				output.Write (HttpEncoder.HtmlEncode (s));
#endif
			}
		}
#if NET_4_0
		public static string HtmlEncode (object value)
		{
			if (value == null)
				return null;

#if !MOBILE
			IHtmlString htmlString = value as IHtmlString;
			if (htmlString != null)
				return htmlString.ToHtmlString ();
#endif

			return HtmlEncode (value.ToString ());
		}

		public static string JavaScriptStringEncode (string value)
		{
			return JavaScriptStringEncode (value, false);
		}

		public static string JavaScriptStringEncode (string value, bool addDoubleQuotes)
		{
			if (String.IsNullOrEmpty (value))
				return addDoubleQuotes ? "\"\"" : String.Empty;

			int len = value.Length;
			bool needEncode = false;
			char c;
			for (int i = 0; i < len; i++) {
				c = value [i];

				if (c >= 0 && c <= 31 || c == 34 || c == 39 || c == 60 || c == 62 || c == 92) {
					needEncode = true;
					break;
				}
			}

			if (!needEncode)
				return addDoubleQuotes ? "\"" + value + "\"" : value;

			var sb = new StringBuilder ();
			if (addDoubleQuotes)
				sb.Append ('"');

			for (int i = 0; i < len; i++) {
				c = value [i];
				if (c >= 0 && c <= 7 || c == 11 || c >= 14 && c <= 31 || c == 39 || c == 60 || c == 62)
					sb.AppendFormat ("\\u{0:x4}", (int)c);
				else switch ((int)c) {
						case 8:
							sb.Append ("\\b");
							break;

						case 9:
							sb.Append ("\\t");
							break;

						case 10:
							sb.Append ("\\n");
							break;

						case 12:
							sb.Append ("\\f");
							break;

						case 13:
							sb.Append ("\\r");
							break;

						case 34:
							sb.Append ("\\\"");
							break;

						case 92:
							sb.Append ("\\\\");
							break;

						default:
							sb.Append (c);
							break;
					}
			}

			if (addDoubleQuotes)
				sb.Append ('"');

			return sb.ToString ();
		}
#endif
		public static string UrlPathEncode (string s)
		{
#if NET_4_0
			return HttpEncoder.Current.UrlPathEncode (s);
#else
			return HttpEncoder.UrlPathEncode (s);
#endif
		}

		public static NameValueCollection ParseQueryString (string query)
		{
			return ParseQueryString (query, Encoding.UTF8);
		}

		public static NameValueCollection ParseQueryString (string query, Encoding encoding)
		{
			if (query == null)
				throw new ArgumentNullException ("query");
			if (encoding == null)
				throw new ArgumentNullException ("encoding");
			if (query.Length == 0 || (query.Length == 1 && query[0] == '?'))
				return new HttpQSCollection ();
			if (query[0] == '?')
				query = query.Substring (1);
				
			NameValueCollection result = new HttpQSCollection ();
			ParseQueryString (query, encoding, result);
			return result;
		}

		internal static void ParseQueryString (string query, Encoding encoding, NameValueCollection result)
		{
			if (query.Length == 0)
				return;

			string decoded = HtmlDecode (query);
			int decodedLength = decoded.Length;
			int namePos = 0;
			bool first = true;
			while (namePos <= decodedLength) {
				int valuePos = -1, valueEnd = -1;
				for (int q = namePos; q < decodedLength; q++) {
					if (valuePos == -1 && decoded [q] == '=') {
						valuePos = q + 1;
					} else if (decoded [q] == '&') {
						valueEnd = q;
						break;
					}
				}

				if (first) {
					first = false;
					if (decoded [namePos] == '?')
						namePos++;
				}
				
				string name, value;
				if (valuePos == -1) {
					name = null;
					valuePos = namePos;
				} else {
					name = UrlDecode (decoded.Substring (namePos, valuePos - namePos - 1), encoding);
				}
				if (valueEnd < 0) {
					namePos = -1;
					valueEnd = decoded.Length;
				} else {
					namePos = valueEnd + 1;
				}
				value = UrlDecode (decoded.Substring (valuePos, valueEnd - valuePos), encoding);

				result.Add (name, value);
				if (namePos == -1)
					break;
			}
		}
		#endregion // Methods
	}
}

