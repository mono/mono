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

using System.Text;


namespace System {
	
	internal class ParserState
	{
		public ParserState (string uri)
		{
			remaining = uri;
			elements  = new UriElements ();
		}
		
		public string remaining;
		public UriElements elements;
	}
	
	// Parse Uri components (scheme, userinfo, host, query, fragment)
	// http://www.ietf.org/rfc/rfc3986.txt
	internal static class UriParseComponents
	{
		public static UriElements ParseComponents (string uri)
		{
			ParserState state = new ParserState (uri);
			
			bool ok = ParseScheme (ref state);
			if (ok)
			    ok = ParseAuthority (ref state);
			if (ok)
			    ok = ParsePath (ref state);
			if (ok)
			    ok = ParseQuery (ref state);
			if (ok)
			    ParseFragment (ref state);
			
			return state.elements;
		}
				// ALPHA
		private static bool IsAlpha (char ch)
		{
			return (('a' <= ch) && (ch <= 'z')) ||
				   (('A' <= ch) && (ch <= 'Z'));
		}
		
		// 3.1) scheme      = ALPHA *( ALPHA / DIGIT / "+" / "-" / "." )
		private static bool ParseScheme (ref ParserState state) 
		{
			string part = state.remaining;
			
			if (!IsAlpha (part [0]))
				return part.Length > 0;
			
			StringBuilder sb = new StringBuilder ();
			sb.Append (part [0]);
			
			int index;
			for (index = 1; index < part.Length; index++ ) {
				char ch = part [index];
				if (ch != '.' && ch != '-' && ch != '+' && !IsAlpha (ch))
					break;
				
				sb.Append (ch);
			}
			
			if (index + 1 <= part.Length && part [index] == ':') {
				state.elements.scheme = sb.ToString ();
				state.remaining = part.Substring (index + 1);
			}
				
			return state.remaining.Length > 0;
		}
		
		private static bool ParseAuthority (ref ParserState state)
		{
			string part = state.remaining;
			
			if (part.Length < 2 || part [0] != '/' || part [1] != '/')
				return part.Length > 0;
			
			state.remaining = part.Substring (2);
			
			bool ok = ParseUser (ref state);
			if (ok)
				ok = ParseHost (ref state);
			if (ok)
				ok = ParsePort (ref state);
			return ok;
		}
		
		// userinfo    = *( unreserved / pct-encoded / sub-delims / ":" )
		private static bool ParseUser (ref ParserState state)
		{
			string part = state.remaining;
			StringBuilder sb = new StringBuilder ();
			
			int index;
			for (index = 0; index < part.Length; index++) {
				
				char ch = part [index];
				
				if (ch == '@' || ch == '/' && ch == '#' && ch == '?')
					break;
				
				sb.Append (ch);
			}
			
			if (index + 1 <= part.Length && part [index] == '@') {
				
				state.elements.user = sb.ToString ();
				state.remaining = state.remaining.Substring (index + 1);
			}
				
			return state.remaining.Length > 0;
		}
		
		// host        = IP-literal / IPv4address / reg-name
		private static bool ParseHost (ref ParserState state)
		{
			string part = state.remaining;
			StringBuilder sb = new StringBuilder ();
			
			int index;
			for (index = 0; index < part.Length; index++) {	
				
				char ch = part [index];
				
				if (ch == '/' || ch == ':' || ch == '#' || ch == '?')
					break;
				
				sb.Append (ch);
			}
			
			if (index  <= part.Length)
				state.remaining = part.Substring (index);
			
			state.elements.host = sb.ToString();
				
			return state.remaining.Length > 0;
		}
		
		// port          = *DIGIT
		private static bool ParsePort (ref ParserState state)
		{
			string part = state.remaining;
			if (part.Length == 0 || part [0] != ':')
				return part.Length > 0;
			
			StringBuilder sb = new StringBuilder ();
			
			int index;
			for (index = 1; index < part.Length; index++ ) {
				char ch = part [index];
				
				if (!char.IsDigit (ch))
					break;
				
				sb.Append (ch);
			}
			
			if (index <= part.Length)
				state.remaining = part.Substring (index);
			
			state.elements.port = sb.ToString();
				
			return state.remaining.Length > 0;
		}
		
		private static bool ParsePath (ref ParserState state)
		{
			string part = state.remaining;
			StringBuilder sb = new StringBuilder ();
			
			int index;
			for (index = 0; index < part.Length; index++) {
				
				char ch = part [index];
				
				if (ch == '#' || ch == '?')
					break;
				
				sb.Append (ch);
			}
			
			if (index <= part.Length)
				state.remaining = part.Substring (index);
			
			state.elements.path  = sb.ToString ();
				
			return state.remaining.Length > 0;
		}
		
		private static bool ParseQuery (ref ParserState state)
		{
			string part = state.remaining;
			
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
		
		private static bool ParseFragment (ref ParserState state)
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
