//
// System.Net.CookieParser
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Daniel Nauck    (dna(at)mono-project(dot)de)
//
// (c) 2002 Lawrence Pit
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// (c) 2008 Daniel Nauck
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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

namespace System.Net {

	class CookieParser {
		string header;
		int pos;
		int length;

		public CookieParser (string header) : this (header, 0)
		{
		}

		public CookieParser (string header, int position)
		{
			this.header = header;
			this.pos = position;
			this.length = header.Length;
		}

		public IEnumerable<Cookie> Parse ()
		{
			while (pos < length) {
				Cookie cookie;
				try {
					cookie = DoParse ();
				} catch {
					while ((pos < length) && (header [pos] != ','))
						pos++;
					pos++;
					continue;
				}
				yield return cookie;
			}
		}

		Cookie DoParse ()
		{
			var name = GetCookieName ();
			if (pos >= length)
				return new Cookie (name, string.Empty);

			var value = string.Empty;
			if (header [pos] == '=') {
				pos++;
				value = GetCookieValue ();
			}

			var cookie = new Cookie (name, value);

			if (pos >= length) {
				return cookie;
			} else if (header [pos] == ',') {
				pos++;
				return cookie;
			} else if ((header [pos++] != ';') || (pos >= length)) {
				return cookie;
			}

			while (pos < length) {
				var argName = GetCookieName ();
				string argVal = string.Empty;
				if ((pos < length) && (header [pos] == '=')) {
					pos++;
					argVal = GetCookieValue ();
				}
				ProcessArg (cookie, argName, argVal);

				if (pos >= length)
					break;
				if (header [pos] == ',') {
					pos++;
					break;
				} else if (header [pos] != ';') {
					break;
				}

				pos++;
			}

			return cookie;
		}

		void ProcessArg (Cookie cookie, string name, string val)
		{
			if ((name == null) || (name == string.Empty))
				throw new InvalidOperationException ();

			name = name.ToUpper ();
			switch (name) {
			case "COMMENT":
				if (cookie.Comment == null)
					cookie.Comment = val;
				break;
			case "COMMENTURL":
				if (cookie.CommentUri == null)
					cookie.CommentUri = new Uri (val);
				break;
			case "DISCARD":
				cookie.Discard = true;
				break;
			case "DOMAIN":
				if (cookie.Domain == "")
					cookie.Domain = val;
				break;
			case "HTTPONLY":
				cookie.HttpOnly = true;
				break;
			case "MAX-AGE": // RFC Style Set-Cookie2
				if (cookie.Expires == DateTime.MinValue) {
					try {
					cookie.Expires = cookie.TimeStamp.AddSeconds (UInt32.Parse (val));
					} catch {}
				}
				break;
			case "EXPIRES": // Netscape Style Set-Cookie
				if (cookie.Expires != DateTime.MinValue)
					break;

				if ((pos < length) && (header [pos] == ',') && IsWeekDay (val)) {
					pos++;
					val = val + ", " + GetCookieValue ();
				}

				cookie.Expires = CookieParser.TryParseCookieExpires (val);
				break;
			case "PATH":
				cookie.Path = val;
				break;
			case "PORT":
				if (cookie.Port == null)
					cookie.Port = val;
				break;
			case "SECURE":
				cookie.Secure = true;
				break;
			case "VERSION":
				try {
					cookie.Version = (int) UInt32.Parse (val);
				} catch {}
				break;
			}
		}

		string GetCookieName ()
		{
			int k = pos;
			while (k < length && Char.IsWhiteSpace (header [k]))
				k++;

			int begin = k;
			while (k < length && header [k] != ';' && header [k] != ',' && header [k] != '=')
				k++;

			pos = k;
			return header.Substring (begin, k - begin).Trim ();
		}

		string GetCookieValue ()
		{
			if (pos >= length)
				return null;

			int k = pos;
			while (k < length && Char.IsWhiteSpace (header [k]))
				k++;

			int begin;
			if (header [k] == '"'){
				int j;
				begin = k++;

				while (k < length && header [k] != '"')
					k++;

				for (j = ++k; j < length && header [j] != ';' && header [j] != ','; j++)
					;
				pos = j;
			} else {
				begin = k;
				while (k < length && header [k] != ';' && header [k] != ',')
					k++;
				pos = k;
			}

			return header.Substring (begin, k - begin).Trim ();
		}

		static bool IsWeekDay (string value)
		{
			foreach (string day in weekDays) {
				if (value.ToLower ().Equals (day))
					return true;
			}
			return false;
		}

		static string[] weekDays =
			new string[] { "mon", "tue", "wed", "thu", "fri", "sat", "sun",
				       "monday", "tuesday", "wednesday", "thursday",
				       "friday", "saturday", "sunday" };

		static string[] cookieExpiresFormats =
			new string[] { "r",
					"ddd, dd'-'MMM'-'yyyy HH':'mm':'ss 'GMT'",
					"ddd, dd'-'MMM'-'yy HH':'mm':'ss 'GMT'" };

		static DateTime TryParseCookieExpires (string value)
		{
			if (String.IsNullOrEmpty (value))
				return DateTime.MinValue;

			for (int i = 0; i < cookieExpiresFormats.Length; i++) {
				try {
					DateTime cookieExpiresUtc = DateTime.ParseExact (value, cookieExpiresFormats [i], CultureInfo.InvariantCulture);

					//convert UTC/GMT time to local time
					cookieExpiresUtc = DateTime.SpecifyKind (cookieExpiresUtc, DateTimeKind.Utc);
					return TimeZone.CurrentTimeZone.ToLocalTime (cookieExpiresUtc);
				} catch {}
			}

			//If we can't parse Expires, use cookie as session cookie (expires is DateTime.MinValue)
			return DateTime.MinValue;
		}
	}
}

