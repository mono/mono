//
// Internal UriParseComponents class
//
// Author:
//	Vinicius Jarina  <vinicius.jarina@xamarin.com>
//
// Copyright (C) 2012 Xamarin, Inc (http://www.xamarin.com)
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

using System.IO;
using System.Net;
using System.Text;
using System.Globalization;

namespace System {
	
	internal class ParserState
	{
		public ParserState (string uri, UriKind kind)
		{
			remaining = uri;
			this.kind = kind;
			elements  = new UriElements ();
		}
		
		public string remaining;
		public UriKind kind;
		public UriElements elements;
		public string error;
	}
	
	// Parse Uri components (scheme, userinfo, host, query, fragment)
	// http://www.ietf.org/rfc/rfc3986.txt
	internal static class UriParseComponents
	{
		public static UriElements ParseComponents (string uri, UriKind kind)
		{
			UriElements elements;
			string error;

			if (!TryParseComponents (uri, kind, out elements, out error))
				throw new UriFormatException (error);

			return elements;
		}

		public static bool TryParseComponents (string uri, UriKind kind, out UriElements elements, out string error)
		{
			uri = uri.Trim ();

			ParserState state = new ParserState (uri, kind);
			elements = state.elements;
			error = null;

			if (uri.Length == 0 && (kind == UriKind.Relative || kind == UriKind.RelativeOrAbsolute)){
				state.elements.isAbsoluteUri = false;
				return true;
			}
			
			if (uri.Length <= 1 && kind == UriKind.Absolute) {
				error = "Absolute URI is too short";
				return false;
			}

			bool ok = ParseFilePath (state) &&
				ParseScheme (state);

			var scheme = state.elements.scheme;
			UriParser parser = null;
			if (!string.IsNullOrEmpty (scheme)) {
				parser = UriParser.GetParser (scheme);
				if (parser != null && !(parser is DefaultUriParser))
					return true;
			}

			ok = ok &&
				ParseAuthority (state) &&
				ParsePath (state) &&
				ParseQuery (state) &&
				ParseFragment (state);

			if (string.IsNullOrEmpty (state.elements.host) &&
				(scheme == Uri.UriSchemeHttp || scheme == Uri.UriSchemeGopher || scheme == Uri.UriSchemeNntp ||
				scheme == Uri.UriSchemeHttps || scheme == Uri.UriSchemeFtp))
				state.error = "Invalid URI: The Authority/Host could not be parsed.";

			if (!string.IsNullOrEmpty (state.elements.host) &&
				Uri.CheckHostName (state.elements.host) == UriHostNameType.Unknown)
				state.error = "Invalid URI: The hostname could not be parsed.";

			if (!string.IsNullOrEmpty (state.error)) {
				elements = null;
				error = state.error;
				return false;
			}
			
			return true;
		}

				// ALPHA
		private static bool IsAlpha (char ch)
		{
			return (('a' <= ch) && (ch <= 'z')) ||
				   (('A' <= ch) && (ch <= 'Z'));
		}

		private static bool ParseFilePath (ParserState state)
		{
			return ParseWindowsFilePath (state) &&
				ParseWindowsUNC (state) &&
				ParseUnixFilePath (state);
		}

		private static bool ParseWindowsFilePath (ParserState state)
		{
			var scheme = state.elements.scheme;

			if (!string.IsNullOrEmpty (scheme) &&
				 scheme != Uri.UriSchemeFile && UriHelper.IsKnownScheme (scheme))
				return state.remaining.Length > 0;

			string part = state.remaining;

			if (part.Length > 0 && (part [0] == '/' || part [0] == '\\'))
				part = part.Substring (1);

			if (part.Length < 2 || part [1] != ':')
				return state.remaining.Length > 0;

			if (!IsAlpha (part [0])) {
				if (state.kind == UriKind.Absolute) {
					state.error = "Invalid URI: The URI scheme is not valid.";
					return false;
				}
				state.elements.isAbsoluteUri = false;
				state.elements.path = part;
				return false;
			}

			if (part.Length > 2 && part [2] != '\\' && part [2] != '/') {
				state.error = "Relative file path is not allowed.";
				return false;
			}

			if (string.IsNullOrEmpty (scheme)) {
				state.elements.scheme = Uri.UriSchemeFile;
				state.elements.delimiter = "://";
			}

			state.elements.path = part.Replace ("\\", "/");

			return false;
		}

		private static bool ParseWindowsUNC (ParserState state)
		{
			string part = state.remaining;

			if (part.Length < 2 || part [0] != '\\' || part [1] != '\\')
				return state.remaining.Length > 0;

			state.elements.scheme = Uri.UriSchemeFile;
			state.elements.delimiter = "://";
			state.elements.isUnc = true;

			part = part.TrimStart ('\\');
			int pos = part.IndexOf ('\\');
			if (pos > 0) {
				state.elements.path = part.Substring (pos);
				state.elements.host = part.Substring (0, pos);
			} else { // "\\\\server"
				state.elements.host = part;
				state.elements.path = String.Empty;
			}
			state.elements.path = state.elements.path.Replace ("\\", "/");

			return false;
		}

		private static bool ParseUnixFilePath (ParserState state)
		{
			string part = state.remaining;

			if (part.Length < 1 || part [0] != '/' || Path.DirectorySeparatorChar != '/')
				return state.remaining.Length > 0;

			state.elements.scheme = Uri.UriSchemeFile;
			state.elements.delimiter = "://";
			state.elements.isUnixFilePath = true;
			state.elements.isAbsoluteUri = (state.kind == UriKind.Relative)? false : true;

			if (part.Length >= 2 && part [0] == '/' && part [1] == '/') {
				part = part.TrimStart (new char [] {'/'});
				state.elements.path = '/' + part;
			} else
				state.elements.path = part;

			return false;
		}
		
		// 3.1) scheme      = ALPHA *( ALPHA / DIGIT / "+" / "-" / "." )
		private static bool ParseScheme (ParserState state)
		{
			string part = state.remaining;
			
			StringBuilder sb = new StringBuilder ();
			sb.Append (part [0]);
			
			int index;
			for (index = 1; index < part.Length; index++ ) {
				char ch = part [index];
				if (ch != '.' && ch != '-' && ch != '+' && !IsAlpha (ch) && !Char.IsDigit (ch))
					break;
				
				sb.Append (ch);
			}
			
			if (index == 0 || index >= part.Length) {
				if (state.kind == UriKind.Absolute) {
					state.error = "Invalid URI: The format of the URI could not be determined.";
					return false;
				}

				state.elements.isAbsoluteUri = false;
				return state.remaining.Length > 0;
			}

			if (part [index] != ':') {
				if (state.kind == UriKind.Absolute) {
					state.error = "Invalid URI: The URI scheme is not valid.";
					return false;
				}

				state.elements.isAbsoluteUri = false;
				return state.remaining.Length > 0;
			}

			state.elements.scheme = sb.ToString ().ToLowerInvariant ();
			state.remaining = part.Substring (index);

			// Check scheme name characters as specified in RFC2396.
			// Note: different checks in 1.x and 2.0
			if (!Uri.CheckSchemeName (state.elements.scheme)) {
				if (state.kind == UriKind.Absolute) {
					state.error = "Invalid URI: The URI scheme is not valid.";
					return false;
				}

				state.elements.isAbsoluteUri = false;
				return state.remaining.Length > 0;
			}

			if (state.elements.scheme == Uri.UriSchemeFile) {
				// under Windows all file:// URI are considered UNC, which is not the case other MacOS (e.g. Silverlight)
#if BOOTSTRAP_BASIC
				state.elements.isUnc = (Path.DirectorySeparatorChar == '\\');
#else
				state.elements.isUnc = Environment.IsRunningOnWindows;
#endif
			}

			return ParseDelimiter (state);
		}

		private static bool ParseDelimiter (ParserState state)
		{
			var delimiter = Uri.GetSchemeDelimiter (state.elements.scheme);

			if (!state.remaining.StartsWith (delimiter, StringComparison.Ordinal)) {
				if (UriHelper.IsKnownScheme (state.elements.scheme)) {
					state.error = "Invalid URI: The Authority/Host could not be parsed.";
					return false;
				}

				delimiter = ":";
			}
				
			state.elements.delimiter = delimiter;

			state.remaining = state.remaining.Substring (delimiter.Length);

			return state.remaining.Length > 0;
		}
		
		private static bool ParseAuthority (ParserState state)
		{
			if (state.elements.delimiter != Uri.SchemeDelimiter && state.elements.scheme != Uri.UriSchemeMailto)
				return state.remaining.Length > 0;
			
			return ParseUser (state) &&
				ParseHost (state) &&
				ParsePort (state);
		}

		static bool IsUnreserved (char ch)
		{
			return ch == '-' || ch == '.' || ch == '_' || ch == '~';
		}


		static bool IsSubDelim (char ch)
		{
			return ch == '!' || ch == '$' || ch == '&' || ch == '\'' || ch == '(' || ch == ')' ||
				ch == '*' || ch == '+' || ch == ',' || ch == ';' || ch == '=';
		}
		
		// userinfo    = *( unreserved / pct-encoded / sub-delims / ":" )
		private static bool ParseUser (ParserState state)
		{
			string part = state.remaining;
			StringBuilder sb = null;

			int index;
			for (index = 0; index < part.Length; index++) {
				char ch = part [index];

				if (ch == '%'){
					if (!Uri.IsHexEncoding (part, index))
						return false;
					ch = Uri.HexUnescape (part, ref index);
				}

				if (Char.IsLetterOrDigit (ch) || IsUnreserved (ch) || IsSubDelim (ch) || ch == ':'){
					if (sb == null)
					        sb = new StringBuilder ();
					sb.Append (ch);
				} else
					break;
			}

			if (index + 1 <= part.Length && part [index] == '@') {
				if (state.elements.scheme == Uri.UriSchemeFile) {
					state.error = "Invalid URI: The hostname could not be parsed.";
					return false;
				}

				state.elements.user = sb == null ? "" : sb.ToString ();
				state.remaining = state.remaining.Substring (index + 1);
			}
				
			return state.remaining.Length > 0;
		}
		
		// host        = IP-literal / IPv4address / reg-name
		private static bool ParseHost (ParserState state)
		{
			string part = state.remaining;

			if (state.elements.scheme == Uri.UriSchemeFile && part.Length >= 2 &&
				(part [0] == '\\' || part [0] == '/') && part [1] == part [0]) {
				part = part.TrimStart (part [0]);
				state.remaining = part;
			}

			if (!ParseWindowsFilePath (state))
				return false;

			StringBuilder sb = new StringBuilder ();
			
			var tmpHost = "";

			var possibleIpv6 = false;

			int index;
			for (index = 0; index < part.Length; index++) {	
				
				char ch = part [index];
				
				if (ch == '/' || ch == '#' || ch == '?')
					break;

				// Possible IPv6
				if (string.IsNullOrEmpty (tmpHost) && ch == ':') {
					tmpHost = sb.ToString ();
					possibleIpv6 = true;
				}
				
				sb.Append (ch);

				if (possibleIpv6 && ch == ']')
					break;
			}
			
			if (possibleIpv6) {
				IPv6Address ipv6addr;
				if (IPv6Address.TryParse (sb.ToString (), out ipv6addr)) {
#if NET_4_5
					var ipStr = ipv6addr.ToString (false);
#else
					var ipStr = ipv6addr.ToString (true);
#endif
					//remove scope
					ipStr = ipStr.Split ('%') [0];

					state.elements.host = "[" + ipStr + "]";
					state.elements.scopeId = ipv6addr.ScopeId;

					state.remaining = part.Substring (sb.Length);
					return state.remaining.Length > 0;
				}
				state.elements.host = tmpHost;
			} else
				state.elements.host = sb.ToString ();

			state.elements.host = state.elements.host.ToLowerInvariant ();

			state.remaining = part.Substring (state.elements.host.Length);
				
			return state.remaining.Length > 0;
		}
		
		// port          = *DIGIT
		private static bool ParsePort (ParserState state)
		{
			string part = state.remaining;
			if (part.Length == 0 || part [0] != ':')
				return part.Length > 0;
			
			StringBuilder sb = new StringBuilder ();
			
			int index;
			for (index = 1; index < part.Length; index++ ) {
				char ch = part [index];
				
				if (!char.IsDigit (ch)) {
					if (ch == '/' || ch == '#' || ch == '?')
						break;

					state.error = "Invalid URI: Invalid port specified.";
					return false;
				}
				
				sb.Append (ch);
			}

			if (index <= part.Length)
				state.remaining = part.Substring (index);

			if (sb.Length == 0)
				return state.remaining.Length > 0;
			
			int port;
			if (!Int32.TryParse (sb.ToString (), NumberStyles.None, CultureInfo.InvariantCulture, out port) ||
				port < 0 || port > UInt16.MaxValue) {
				state.error = "Invalid URI: Invalid port number";
				return false;
			}

			state.elements.port = port;
				
			return state.remaining.Length > 0;
		}
		
		private static bool ParsePath (ParserState state)
		{
			string part = state.remaining;
			StringBuilder sb = new StringBuilder ();
			
			int index;
			for (index = 0; index < part.Length; index++) {
				
				char ch = part [index];
				
				var supportsQuery = UriHelper.SupportsQuery (state.elements.scheme);

				if (ch == '#' || (supportsQuery && ch == '?'))
					break;
				
				sb.Append (ch);
			}
			
			if (index <= part.Length)
				state.remaining = part.Substring (index);
			
			state.elements.path  = sb.ToString ();
				
			return state.remaining.Length > 0;
		}
		
		private static bool ParseQuery (ParserState state)
		{
			string part = state.remaining;

			if (!UriHelper.SupportsQuery (state.elements.scheme))
				return part.Length > 0;
			
			if (part.Length == 0 || part [0] != '?')
				return part.Length > 0;
			
			StringBuilder sb = new StringBuilder ();
			
			int index;
			for (index = 1; index < part.Length; index++) {
				
				char ch = part [index];
				
				if (ch == '#')
					break;
				
				sb.Append (ch);
			}
			
			if (index <= part.Length)
				state.remaining = part.Substring (index);
			
			state.elements.query  = sb.ToString ();
				
			return state.remaining.Length > 0;
		}
		
		private static bool ParseFragment (ParserState state)
		{
			string part = state.remaining;
			
			if (part.Length == 0 || part [0] != '#')
				return part.Length > 0;
			
			StringBuilder sb = new StringBuilder ();
			
			int index;
			for (index = 1; index < part.Length; index++) {	
				
				char ch = part [index];
				
				sb.Append (ch);
			}
			
			state.elements.fragment = sb.ToString ();
			
			return false;
		}
	}
}
