//
// Mono.Security.Uri
//	Adapted from System.Uri (in System.dll assembly) for its use in corlib
//
// Authors:
//    Lawrence Pit (loz@cable.a2000.nl)
//    Garrett Rooney (rooneg@electricjellyfish.net)
//    Ian MacLean (ianm@activestate.com)
//    Ben Maurer (bmaurer@users.sourceforge.net)
//    Atsushi Enomoto (atsushi@ximian.com)
//    Stephane Delcroix  <stephane@delcroix.org>
//
// (C) 2001 Garrett Rooney
// (C) 2003 Ian MacLean
// (C) 2003 Ben Maurer
// (C) 2003 Novell inc.
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

// See RFC 2396 for more info on URI's.

// TODO: optimize by parsing host string only once

namespace Mono.Security {

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	enum UriPartial {
		Scheme = 0,
		Authority = 1,
		Path = 2,
	}

#if INSIDE_CORLIB
	internal
#else
	public
#endif
	class Uri {

		// NOTES:
		// o  scheme excludes the scheme delimiter
		// o  port is -1 to indicate no port is defined
		// o  path is empty or starts with / when scheme delimiter == "://"
		// o  query is empty or starts with ? char, escaped.
		// o  fragment is empty or starts with # char, unescaped.
		// o  all class variables are in escaped format when they are escapable,
		//    except cachedToString.
		// o  UNC is supported, as starts with "\\" for windows,
		//    or "//" with unix.

		private bool isUnixFilePath = false;
		private string source;
		private string scheme = String.Empty;
		private string host = String.Empty;
		private int port = -1;
		private string path = String.Empty;
		private string query = String.Empty;
		private string fragment = String.Empty;
		private string userinfo = String.Empty;
		private bool isUnc = false;
		private bool isOpaquePart = false;

		private string [] segments;
		
		private bool userEscaped = false;
		private string cachedAbsoluteUri = null;
		private string cachedToString = null;
		private string cachedLocalPath = null;
		private int cachedHashCode = 0;
		private bool reduce = true;
		
		private static readonly string hexUpperChars = "0123456789ABCDEF";
	
		// Fields
		
		public static readonly string SchemeDelimiter = "://";
		public static readonly string UriSchemeFile = "file";
		public static readonly string UriSchemeFtp = "ftp";
		public static readonly string UriSchemeGopher = "gopher";
		public static readonly string UriSchemeHttp = "http";
		public static readonly string UriSchemeHttps = "https";
		public static readonly string UriSchemeMailto = "mailto";
		public static readonly string UriSchemeNews = "news";
		public static readonly string UriSchemeNntp = "nntp";

		// Constructors		

		public Uri (string uriString) : this (uriString, false) 
		{
		}

		public Uri (string uriString, bool dontEscape) 
		{
			userEscaped = dontEscape;
			source = uriString;
			Parse ();
		}

		public Uri (string uriString, bool dontEscape, bool reduce) 
		{
			userEscaped = dontEscape;
			source = uriString;
			this.reduce = reduce;
			Parse ();
		}

		public Uri (Uri baseUri, string relativeUri) 
			: this (baseUri, relativeUri, false) 
		{			
		}

		public Uri (Uri baseUri, string relativeUri, bool dontEscape) 
		{
			if (baseUri == null)
				throw new NullReferenceException ("baseUri");

			// See RFC 2396 Par 5.2 and Appendix C

			userEscaped = dontEscape;

			if (relativeUri == null)
				throw new NullReferenceException ("relativeUri");

			// Check Windows UNC (for // it is scheme/host separator)
			if (relativeUri.StartsWith ("\\\\")) {
				source = relativeUri;
				Parse ();
				return;
			}

			int pos = relativeUri.IndexOf (':');
			if (pos != -1) {

				int pos2 = relativeUri.IndexOfAny (new char [] {'/', '\\', '?'});

				// pos2 < 0 ... e.g. mailto
				// pos2 > pos ... to block ':' in query part
				if (pos2 > pos || pos2 < 0) {
					// equivalent to new Uri (relativeUri, dontEscape)
					source = relativeUri;
					Parse ();

					return;
				}
			}

			this.scheme = baseUri.scheme;
			this.host = baseUri.host;
			this.port = baseUri.port;
			this.userinfo = baseUri.userinfo;
			this.isUnc = baseUri.isUnc;
			this.isUnixFilePath = baseUri.isUnixFilePath;
			this.isOpaquePart = baseUri.isOpaquePart;

			if (relativeUri == String.Empty) {
				this.path = baseUri.path;
				this.query = baseUri.query;
				this.fragment = baseUri.fragment;
				return;
			}
			
			// 8 fragment
			// Note that in relative constructor, file URI cannot handle '#' as a filename character, but just regarded as a fragment identifier.
			pos = relativeUri.IndexOf ('#');
			if (pos != -1) {
				fragment = relativeUri.Substring (pos);
				// fragment is not escaped.
				relativeUri = relativeUri.Substring (0, pos);
			}

			// 6 query
			pos = relativeUri.IndexOf ('?');
			if (pos != -1) {
				query = relativeUri.Substring (pos);
				if (!userEscaped)
					query = EscapeString (query);
				relativeUri = relativeUri.Substring (0, pos);
			}

			if (relativeUri.Length > 0 && relativeUri [0] == '/') {
				if (relativeUri.Length > 1 && relativeUri [1] == '/') {
					source = scheme + ':' + relativeUri;
					Parse ();
					return;
				} else {
					path = relativeUri;
					if (!userEscaped)
						path = EscapeString (path);
					return;
				}
			}
			
			// par 5.2 step 6 a)
			path = baseUri.path;
			if (relativeUri.Length > 0 || query.Length > 0) {
				pos = path.LastIndexOf ('/');
				if (pos >= 0) 
					path = path.Substring (0, pos + 1);
			}

			if(relativeUri.Length == 0)
				return;
	
			// 6 b)
			path += relativeUri;

			// 6 c)
			int startIndex = 0;
			while (true) {
				pos = path.IndexOf ("./", startIndex);
				if (pos == -1)
					break;
				if (pos == 0)
					path = path.Remove (0, 2);
				else if (path [pos - 1] != '.')
					path = path.Remove (pos, 2);
				else
					startIndex = pos + 1;
			}
			
			// 6 d)
			if (path.Length > 1 && 
			    path [path.Length - 1] == '.' &&
			    path [path.Length - 2] == '/')
				path = path.Remove (path.Length - 1, 1);
			
			// 6 e)
			startIndex = 0;
			while (true) {
				pos = path.IndexOf ("/../", startIndex);
				if (pos == -1)
					break;
				if (pos == 0) {
					startIndex = 3;
					continue;
				}
				int pos2 = path.LastIndexOf ('/', pos - 1);
				if (pos2 == -1) {
					startIndex = pos + 1;
				} else {
					if (path.Substring (pos2 + 1, pos - pos2 - 1) != "..")
						path = path.Remove (pos2 + 1, pos - pos2 + 3);
					else
						startIndex = pos + 1;
				}
			}
			
			// 6 f)
			if (path.Length > 3 && path.EndsWith ("/..")) {
				pos = path.LastIndexOf ('/', path.Length - 4);
				if (pos != -1)
					if (path.Substring (pos + 1, path.Length - pos - 4) != "..")
						path = path.Remove (pos + 1, path.Length - pos - 1);
			}
			
			if (!userEscaped)
				path = EscapeString (path);
		}		
		
		// Properties
		
		public string AbsolutePath { 
			get { return path; } 
		}

		public string AbsoluteUri { 
			get { 
				if (cachedAbsoluteUri == null) {
					cachedAbsoluteUri = GetLeftPart (UriPartial.Path) + query + fragment;
				}
				return cachedAbsoluteUri;
			} 
		}

		public string Authority { 
			get { 
				return (GetDefaultPort (scheme) == port)
				     ? host : host + ":" + port;
			} 
		}

		public string Fragment { 
			get { return fragment; } 
		}

		public string Host { 
			get { return host; } 
		}

/*		public UriHostNameType HostNameType { 
			get {
				UriHostNameType ret = CheckHostName (host);
				if (ret != UriHostNameType.Unknown)
					return ret;

				// looks it always returns Basic...
				return UriHostNameType.Basic; //.Unknown;
			} 
		}*/

		public bool IsDefaultPort { 
			get { return GetDefaultPort (scheme) == port; } 
		}

		public bool IsFile { 
			get { return (scheme == UriSchemeFile); }
		}

		public bool IsLoopback { 
			get { 
				if (host == String.Empty)
					return false;
					
				if (host == "loopback" || host == "localhost") 
					return true;
/*					
				try {
					if (IPAddress.Loopback.Equals (IPAddress.Parse (host)))
						return true;
				} catch (FormatException) {}

				try {
					return IPv6Address.IsLoopback (IPv6Address.Parse (host));
				} catch (FormatException) {}
*/
				return false;
			} 
		}

		public bool IsUnc {
			// rule: This should be true only if
			//   - uri string starts from "\\", or
			//   - uri string starts from "//" (Samba way)
			get { return isUnc; } 
		}

		public string LocalPath { 
			get {
				if (cachedLocalPath != null)
					return cachedLocalPath;
				if (!IsFile)
					return AbsolutePath;

				bool windows = (path.Length > 3 && path [1] == ':' &&
						(path [2] == '\\' || path [2] == '/'));

				if (!IsUnc) {
					string p = Unescape (path);
					if (System.IO.Path.DirectorySeparatorChar == '\\' || windows)
						cachedLocalPath = p.Replace ('/', '\\');
					else
						cachedLocalPath = p;
				} else {
					// support *nix and W32 styles
					if (path.Length > 1 && path [1] == ':')
						cachedLocalPath = Unescape (path.Replace (Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));

					// LAMESPEC: ok, now we cannot determine
					// if such URI like "file://foo/bar" is
					// Windows UNC or unix file path, so
					// they should be handled differently.
					else if (System.IO.Path.DirectorySeparatorChar == '\\')
						cachedLocalPath = "\\\\" + Unescape (host + path.Replace ('/', '\\'));
					else
						cachedLocalPath = Unescape (path);
				}
				if (cachedLocalPath == String.Empty)
					cachedLocalPath = Path.DirectorySeparatorChar.ToString ();
				return cachedLocalPath;
			} 
		}

		public string PathAndQuery { 
			get { return path + query; } 
		}

		public int Port { 
			get { return port; } 
		}

		public string Query { 
			get { return query; } 
		}

		public string Scheme { 
			get { return scheme; } 
		}

		public string [] Segments { 
			get { 
				if (segments != null)
					return segments;

				if (path.Length == 0) {
					segments = new string [0];
					return segments;
				}

				string [] parts = path.Split ('/');
				segments = parts;
				bool endSlash = path.EndsWith ("/");
				if (parts.Length > 0 && endSlash) {
					string [] newParts = new string [parts.Length - 1];
					Array.Copy (parts, 0, newParts, 0, parts.Length - 1);
					parts = newParts;
				}

				int i = 0;
				if (IsFile && path.Length > 1 && path [1] == ':') {
					string [] newParts = new string [parts.Length + 1];
					Array.Copy (parts, 1, newParts, 2, parts.Length - 1);
					parts = newParts;
					parts [0] = path.Substring (0, 2);
					parts [1] = String.Empty;
					i++;
				}
				
				int end = parts.Length;
				for (; i < end; i++) 
					if (i != end - 1 || endSlash)
						parts [i] += '/';

				segments = parts;
				return segments;
			} 
		}

		public bool UserEscaped { 
			get { return userEscaped; } 
		}

		public string UserInfo { 
			get { return userinfo; }
		}
		

		// Methods		
		
/*		public static UriHostNameType CheckHostName (string name) 
		{
			if (name == null || name.Length == 0)
				return UriHostNameType.Unknown;

			if (IsIPv4Address (name)) 
				return UriHostNameType.IPv4;
				
			if (IsDomainAddress (name))
				return UriHostNameType.Dns;				
				
			try {
				IPv6Address.Parse (name);
				return UriHostNameType.IPv6;
			} catch (FormatException) {}
			
			return UriHostNameType.Unknown;
		}*/
		
		internal static bool IsIPv4Address (string name)
		{		
			string [] captures = name.Split (new char [] {'.'});
			if (captures.Length != 4)
				return false;
			for (int i = 0; i < 4; i++) {
				try {
					int d = Int32.Parse (captures [i], CultureInfo.InvariantCulture);
					if (d < 0 || d > 255)
						return false;
				} catch (Exception) {
					return false;
				}
			}
			return true;
		}			
				
		internal static bool IsDomainAddress (string name)
		{
			int len = name.Length;
			
			if (name [len - 1] == '.')
				return false;
				
			int count = 0;
			for (int i = 0; i < len; i++) {
				char c = name [i];
				if (count == 0) {
					if (!Char.IsLetterOrDigit (c))
						return false;
				} else if (c == '.') {
					count = 0;
				} else if (!Char.IsLetterOrDigit (c) && c != '-' && c != '_') {
					return false;
				}
				if (++count == 64)
					return false;
			}
			
			return true;
		}

		public static bool CheckSchemeName (string schemeName) 
		{
			if (schemeName == null || schemeName.Length == 0)
				return false;
			
			if (!Char.IsLetter (schemeName [0]))
				return false;

			int len = schemeName.Length;
			for (int i = 1; i < len; i++) {
				char c = schemeName [i];
				if (!Char.IsLetterOrDigit (c) && c != '.' && c != '+' && c != '-')
					return false;
			}
			
			return true;
		}

		public override bool Equals (object comparant) 
		{
			if (comparant == null) 
				return false;
				
			Uri uri = comparant as Uri;
			if (uri == null) {
				string s = comparant as String;
				if (s == null)
					return false;
				uri = new Uri (s);
			}

			CultureInfo inv = CultureInfo.InvariantCulture;
			return ((this.scheme.ToLower (inv) == uri.scheme.ToLower (inv)) &&
				(this.userinfo.ToLower (inv) == uri.userinfo.ToLower (inv)) &&
				(this.host.ToLower (inv) == uri.host.ToLower (inv)) &&
				(this.port == uri.port) &&
				(this.path == uri.path) &&
				(this.query.ToLower (inv) == uri.query.ToLower (inv)));
		}		
		
		public override int GetHashCode () 
		{
			if (cachedHashCode == 0)			
				cachedHashCode = scheme.GetHashCode ()
				               + userinfo.GetHashCode ()
					       + host.GetHashCode ()
					       + port
					       + path.GetHashCode ()
					       + query.GetHashCode ();			           
			return cachedHashCode;				
		}
		
		public string GetLeftPart (UriPartial part) 
		{
			int defaultPort;
			switch (part) {				
			case UriPartial.Scheme : 
				return scheme + GetOpaqueWiseSchemeDelimiter ();
			case UriPartial.Authority :
				if (host == String.Empty ||
				    scheme == Uri.UriSchemeMailto ||
				    scheme == Uri.UriSchemeNews)
					return String.Empty;
					
				StringBuilder s = new StringBuilder ();
				s.Append (scheme);
				s.Append (GetOpaqueWiseSchemeDelimiter ());
				if (path.Length > 1 && path [1] == ':' && (Uri.UriSchemeFile == scheme)) 
					s.Append ('/');  // win32 file
				if (userinfo.Length > 0) 
					s.Append (userinfo).Append ('@');
				s.Append (host);
				defaultPort = GetDefaultPort (scheme);
				if ((port != -1) && (port != defaultPort))
					s.Append (':').Append (port);			 
				return s.ToString ();				
			case UriPartial.Path :			
				StringBuilder sb = new StringBuilder ();
				sb.Append (scheme);
				sb.Append (GetOpaqueWiseSchemeDelimiter ());
				if (path.Length > 1 && path [1] == ':' && (Uri.UriSchemeFile == scheme)) 
					sb.Append ('/');  // win32 file
				if (userinfo.Length > 0) 
					sb.Append (userinfo).Append ('@');
				sb.Append (host);
				defaultPort = GetDefaultPort (scheme);
				if ((port != -1) && (port != defaultPort))
					sb.Append (':').Append (port);			 
				sb.Append (path);
				return sb.ToString ();
			}
			return null;
		}

		public static int FromHex (char digit) 
		{
			if ('0' <= digit && digit <= '9') {
				return (int) (digit - '0');
			}
				
			if ('a' <= digit && digit <= 'f')
				return (int) (digit - 'a' + 10);

			if ('A' <= digit && digit <= 'F')
				return (int) (digit - 'A' + 10);
				
			throw new ArgumentException ("digit");
		}

		public static string HexEscape (char character) 
		{
			if (character > 255) {
				throw new ArgumentOutOfRangeException ("character");
			}
			
			return "%" + hexUpperChars [((character & 0xf0) >> 4)] 
			           + hexUpperChars [((character & 0x0f))];
		}

		public static char HexUnescape (string pattern, ref int index) 
		{
			if (pattern == null) 
				throw new ArgumentException ("pattern");
				
			if (index < 0 || index >= pattern.Length)
				throw new ArgumentOutOfRangeException ("index");

			int stage = 0;
			int c = 0;
			do {
				if (((index + 3) > pattern.Length) ||
				    (pattern [index] != '%') || 
				    !IsHexDigit (pattern [index + 1]) || 
				    !IsHexDigit (pattern [index + 2]))
				{
					if (stage == 0)
						return pattern [index++];
					break;
				}

				index++;
				int msb = FromHex (pattern [index++]);
				int lsb = FromHex (pattern [index++]);
				int b = (msb << 4) + lsb;

				if (stage == 0) {
					if (b < 0xc0)
						return (char) b;
					else if (b < 0xE0) {
						c = b - 0xc0;
						stage = 2;
					} else if (b < 0xF0) {
						c = b - 0xe0;
						stage = 3;
					} else if (b < 0xF8) {
						c = b - 0xf0;
						stage = 4;
					} else if (b < 0xFB) {
						c = b - 0xf8;
						stage = 5;
					} else if (b < 0xFE) {
						c = b - 0xfc;
						stage = 6;
					}
					c <<= (stage - 1) * 6;
				}
				else
					c += (b - 0x80) << ((stage - 1) * 6);
//Console.WriteLine ("stage {0}: {5:X04} <-- {1:X02}|{2:X01},{3:X01} {4}", new object [] {stage, b, msb, lsb, pattern.Substring (index), c});
				stage--;
			} while (stage > 0);
			
			return (char) c;
		}

		public static bool IsHexDigit (char digit) 
		{
			return (('0' <= digit && digit <= '9') ||
			        ('a' <= digit && digit <= 'f') ||
			        ('A' <= digit && digit <= 'F'));
		}

		public static bool IsHexEncoding (string pattern, int index) 
		{
			if ((index + 3) > pattern.Length)
				return false;

			return ((pattern [index++] == '%') &&
			        IsHexDigit (pattern [index++]) &&
			        IsHexDigit (pattern [index]));
		}

		public string MakeRelative (Uri toUri) 
		{
			if ((this.Scheme != toUri.Scheme) ||
			    (this.Authority != toUri.Authority))
				return toUri.ToString ();
				
			if (this.path == toUri.path)
				return String.Empty;
				
			string [] segments = this.Segments;
			string [] segments2 = toUri.Segments;
			
			int k = 0;
			int max = System.Math.Min (segments.Length, segments2.Length);
			for (; k < max; k++)
				if (segments [k] != segments2 [k]) 
					break;
			
			string result = String.Empty;
			for (int i = k + 1; i < segments.Length; i++)
				result += "../";
			for (int i = k; i < segments2.Length; i++)
				result += segments2 [i];
			
			return result;
		}

		public override string ToString () 
		{
			if (cachedToString != null) 
				return cachedToString;
			string q = query.StartsWith ("?") ? '?' + Unescape (query.Substring (1)) : Unescape (query);
			cachedToString = Unescape (GetLeftPart (UriPartial.Path), true) + q + fragment;
			return cachedToString;
		}

/*		void ISerializable.GetObjectData (SerializationInfo info, 
					  StreamingContext context)
		{
			info.AddValue ("AbsoluteUri", this.AbsoluteUri);
		}*/


		// Internal Methods		

		protected void Escape ()
		{
			path = EscapeString (path);
		}

		protected static string EscapeString (string str) 
		{
			return EscapeString (str, false, true, true);
		}
		
		internal static string EscapeString (string str, bool escapeReserved, bool escapeHex, bool escapeBrackets) 
		{
			if (str == null)
				return String.Empty;
			
			StringBuilder s = new StringBuilder ();
			int len = str.Length;	
			for (int i = 0; i < len; i++) {
				// reserved    = ";" | "/" | "?" | ":" | "@" | "&" | "=" | "+" | "$" | ","
				// mark        = "-" | "_" | "." | "!" | "~" | "*" | "'" | "(" | ")"
				// control     = <US-ASCII coded characters 00-1F and 7F hexadecimal>
				// space       = <US-ASCII coded character 20 hexadecimal>
				// delims      = "<" | ">" | "#" | "%" | <">
				// unwise      = "{" | "}" | "|" | "\" | "^" | "[" | "]" | "`"

				// check for escape code already placed in str, 
				// i.e. for encoding that follows the pattern 
				// "%hexhex" in a string, where "hex" is a digit from 0-9 
				// or a letter from A-F (case-insensitive).
				if (IsHexEncoding (str,i)) {
					// if ,yes , copy it as is
					s.Append(str.Substring (i, 3));
					i += 2;
					continue;
				}

				byte [] data = Encoding.UTF8.GetBytes (new char[] {str[i]});
				int length = data.Length;
				for (int j = 0; j < length; j++) {
					char c = (char) data [j];
					if ((c <= 0x20) || (c >= 0x7f) || 
					    ("<>%\"{}|\\^`".IndexOf (c) != -1) ||
					    (escapeHex && (c == '#')) ||
					    (escapeBrackets && (c == '[' || c == ']')) ||
					    (escapeReserved && (";/?:@&=+$,".IndexOf (c) != -1))) {
						s.Append (HexEscape (c));
						continue;
					}	
					s.Append (c);
				}
			}
			
			return s.ToString ();
		}
	
		// This method is called from .ctor(). When overriden, we can
		// avoid the "absolute uri" constraints of the .ctor() by
		// overriding with custom code.
		protected void Parse ()
		{
			Parse (source);

			if (userEscaped) 
				return;

			host = EscapeString (host, false, true, false);
			path = EscapeString (path);
		}
		
		protected string Unescape (string str)
		{
			return Unescape (str, false);
		}
		
		internal string Unescape (string str, bool excludeSharp) 
		{
			if (str == null)
				return String.Empty;
			StringBuilder s = new StringBuilder ();
			int len = str.Length;
			for (int i = 0; i < len; i++) {
				char c = str [i];
				if (c == '%') {
					char x = HexUnescape (str, ref i);
					if (excludeSharp && x == '#')
						s.Append ("%23");
					else
						s.Append (x);
					i--;
				} else
					s.Append (c);
			}
			return s.ToString ();
		}

		
		// Private Methods
		
		private void ParseAsWindowsUNC (string uriString)
		{
			scheme = UriSchemeFile;
			port = -1;
			fragment = String.Empty;
			query = String.Empty;
			isUnc = true;

			uriString = uriString.TrimStart (new char [] {'\\'});
			int pos = uriString.IndexOf ('\\');
			if (pos > 0) {
				path = uriString.Substring (pos);
				host = uriString.Substring (0, pos);
			} else { // "\\\\server"
				host = uriString;
				path = String.Empty;
			}
			path = path.Replace ("\\", "/");
		}

		private void ParseAsWindowsAbsoluteFilePath (string uriString)
		{
			if (uriString.Length > 2 && uriString [2] != '\\'
					&& uriString [2] != '/')
				throw new FormatException ("Relative file path is not allowed.");
			scheme = UriSchemeFile;
			host = String.Empty;
			port = -1;
			path = uriString.Replace ("\\", "/");
			fragment = String.Empty;
			query = String.Empty;
		}

		private void ParseAsUnixAbsoluteFilePath (string uriString)
		{
			isUnixFilePath = true;
			scheme = UriSchemeFile;
			port = -1;
			fragment = String.Empty;
			query = String.Empty;
			host = String.Empty;
			path = null;

			if (uriString.StartsWith ("//")) {
				uriString = uriString.TrimStart (new char [] {'/'});
				// Now we don't regard //foo/bar as "foo" host.
				/* 
				int pos = uriString.IndexOf ('/');
				if (pos > 0) {
					path = '/' + uriString.Substring (pos + 1);
					host = uriString.Substring (0, pos);
				} else { // "///server"
					host = uriString;
					path = String.Empty;
				}
				*/
				path = '/' + uriString;
			}
			if (path == null)
				path = uriString;
		}

		// this parse method is as relaxed as possible about the format
		// it will hardly ever throw a UriFormatException
		private void Parse (string uriString)
		{			
			//
			// From RFC 2396 :
			//
			//      ^(([^:/?#]+):)?(//([^/?#]*))?([^?#]*)(\?([^#]*))?(#(.*))?
			//       12            3  4          5       6  7        8 9
			//			
			
			if (uriString == null)
				throw new ArgumentNullException ("uriString");

			int len = uriString.Length;
			if (len <= 1) 
				throw new FormatException ();

			int pos = 0;

			// 1, 2
			// Identify Windows path, unix path, or standard URI.
			pos = uriString.IndexOf (':');
			if (pos < 0) {
				// It must be Unix file path or Windows UNC
				if (uriString [0] == '/')
					ParseAsUnixAbsoluteFilePath (uriString);
				else if (uriString.StartsWith ("\\\\"))
					ParseAsWindowsUNC (uriString);
				else
					throw new FormatException ("URI scheme was not recognized, nor input string is not recognized as an absolute file path.");
				return;
			}
			else if (pos == 1) {
				if (!Char.IsLetter (uriString [0]))
					throw new FormatException ("URI scheme must start with alphabet character.");
				// This means 'a:' == windows full path.
				ParseAsWindowsAbsoluteFilePath (uriString);
				return;
			}

			// scheme
			scheme = uriString.Substring (0, pos).ToLower (CultureInfo.InvariantCulture);
			// Check scheme name characters as specified in RFC2396.
			if (!Char.IsLetter (scheme [0]))
					throw new FormatException ("URI scheme must start with alphabet character.");
			for (int i = 1; i < scheme.Length; i++) {
				if (!Char.IsLetterOrDigit (scheme, i)) {
					switch (scheme [i]) {
					case '+':
					case '-':
					case '.':
						break;
					default:
						throw new FormatException ("URI scheme must consist of one of alphabet, digits, '+', '-' or '.' character.");
					}
				}
			}
			uriString = uriString.Substring (pos + 1);

			// 8 fragment
			pos = uriString.IndexOf ('#');
			if (!IsUnc && pos != -1) {
				fragment = uriString.Substring (pos);
				uriString = uriString.Substring (0, pos);
			}

			// 6 query
			pos = uriString.IndexOf ('?');
			if (pos != -1) {
				query = uriString.Substring (pos);
				uriString = uriString.Substring (0, pos);
				if (!userEscaped)
					query = EscapeString (query);
			}

			// 3
			bool unixAbsPath = scheme == UriSchemeFile && uriString.StartsWith ("///");
			if (uriString.StartsWith ("//")) {
				if (uriString.StartsWith ("////"))
					unixAbsPath = false;
				uriString = uriString.TrimStart (new char [] {'/'});
				if (uriString.Length > 1 && uriString [1] == ':')
					unixAbsPath = false;
			} else if (!IsPredefinedScheme (scheme)) {
				path = uriString;
				isOpaquePart = true;
				return;
			}
			
			// 5 path
			pos = uriString.IndexOfAny (new char[] {'/'});
			if (unixAbsPath)
				pos = -1;
			if (pos == -1) {
				if ((scheme != Uri.UriSchemeMailto) &&
				    (scheme != Uri.UriSchemeNews) &&
					(scheme != Uri.UriSchemeFile))
					path = "/";
			} else {
				path = uriString.Substring (pos);
				uriString = uriString.Substring (0, pos);
			}

			// 4.a user info
			pos = uriString.IndexOf ("@");
			if (pos != -1) {
				userinfo = uriString.Substring (0, pos);
				uriString = uriString.Remove (0, pos + 1);
			}

			// 4.b port
			port = -1;
			pos = uriString.LastIndexOf (":");
			if (unixAbsPath)
				pos = -1;
			if (pos != -1 && pos != (uriString.Length - 1)) {
				string portStr = uriString.Remove (0, pos + 1);
				if (portStr.Length > 1 && portStr [portStr.Length - 1] != ']') {
					try {
						port = (int) UInt32.Parse (portStr, CultureInfo.InvariantCulture);
						uriString = uriString.Substring (0, pos);
					} catch (Exception) {
						throw new FormatException ("Invalid URI: invalid port number");
					}
				}
			}
			if (port == -1) {
				port = GetDefaultPort (scheme);
			}
			
			// 4 authority
			host = uriString;
/*			if (host.Length > 1 && host [0] == '[' && host [host.Length - 1] == ']') {
				try {
					host = "[" + IPv6Address.Parse (host).ToString () + "]";
				} catch (Exception) {
					throw new FormatException ("Invalid URI: The hostname could not be parsed");
				}
			}*/

			if (unixAbsPath) {
				path = '/' + uriString;
				host = String.Empty;
			} else if (host.Length == 2 && host [1] == ':') {
				// windows filepath
				path = host + path;
				host = String.Empty;
			} else if (isUnixFilePath) {
				uriString = "//" + uriString;
				host = String.Empty;
			} else if (host.Length == 0) {
				throw new FormatException ("Invalid URI: The hostname could not be parsed");
			} else if (scheme == UriSchemeFile) {
				isUnc = true;
			}

			if ((scheme != Uri.UriSchemeMailto) &&
					(scheme != Uri.UriSchemeNews) &&
					(scheme != Uri.UriSchemeFile))

			if (reduce)
				path = Reduce (path);
		}

		private static string Reduce (string path)
		{
			path = path.Replace ('\\','/');
			string [] parts = path.Split ('/');
			var result = new List<string> ();

			int end = parts.Length;
			for (int i = 0; i < end; i++) {
				string current = parts [i];
				if (current.Length == 0 || current == "." )
					continue;

				if (current == "..") {
					if (result.Count == 0) {
						if (i == 1) // see bug 52599
							continue;
						throw new Exception ("Invalid path.");
					}

					result.RemoveAt (result.Count - 1);
					continue;
				}

				result.Add (current);
			}

			if (result.Count == 0)
				return "/";

			result.Insert (0, String.Empty);

			string res = String.Join ("/", result.ToArray ());
			if (path.EndsWith ("/"))
				res += '/';
				
			return res;
		}
				
		private struct UriScheme 
		{
			public string scheme;
			public string delimiter;
			public int defaultPort;

			public UriScheme (string s, string d, int p) 
			{
				scheme = s;
				delimiter = d;
				defaultPort = p;
			}
		};

		static UriScheme [] schemes = new UriScheme [] {
			new UriScheme (UriSchemeHttp, SchemeDelimiter, 80),
			new UriScheme (UriSchemeHttps, SchemeDelimiter, 443),
			new UriScheme (UriSchemeFtp, SchemeDelimiter, 21),
			new UriScheme (UriSchemeFile, SchemeDelimiter, -1),
			new UriScheme (UriSchemeMailto, ":", 25),
			new UriScheme (UriSchemeNews, ":", -1),
			new UriScheme (UriSchemeNntp, SchemeDelimiter, 119),
			new UriScheme (UriSchemeGopher, SchemeDelimiter, 70),
		};
				
		internal static string GetSchemeDelimiter (string scheme) 
		{
			for (int i = 0; i < schemes.Length; i++) 
				if (schemes [i].scheme == scheme)
					return schemes [i].delimiter;
			return Uri.SchemeDelimiter;
		}
		
		internal static int GetDefaultPort (string scheme)
		{
			for (int i = 0; i < schemes.Length; i++) 
				if (schemes [i].scheme == scheme)
					return schemes [i].defaultPort;
			return -1;			
		}

		private string GetOpaqueWiseSchemeDelimiter ()
		{
			if (isOpaquePart)
				return ":";
			else
				return GetSchemeDelimiter (scheme);
		}

		protected bool IsBadFileSystemCharacter (char ch)
		{
			// It does not always overlap with InvalidPathChars.
			int chInt = (int) ch;
			if (chInt < 32 || (chInt < 64 && chInt > 57))
				return true;
			switch (chInt) {
			case 0:
			case 34: // "
			case 38: // &
			case 42: // *
			case 44: // ,
			case 47: // /
			case 92: // \
			case 94: // ^
			case 124: // |
				return true;
			}

			return false;
		}

		
		protected static bool IsExcludedCharacter (char ch)
		{
			if (ch <= 32 || ch >= 127)
				return true;
			
			if (ch == '"' || ch == '#' || ch == '%' || ch == '<' ||
			    ch == '>' || ch == '[' || ch == '\\' || ch == ']' ||
			    ch == '^' || ch == '`' || ch == '{' || ch == '|' ||
			    ch == '}')
				return true;
			return false;
		}

		private static bool IsPredefinedScheme (string scheme)
		{
			switch (scheme) {
			case "http":
			case "https":
			case "file":
			case "ftp":
			case "nntp":
			case "gopher":
			case "mailto":
			case "news":
				return true;
			default:
				return false;
			}
		}

		protected bool IsReservedCharacter (char ch)
		{
			if (ch == '$' || ch == '&' || ch == '+' || ch == ',' ||
			    ch == '/' || ch == ':' || ch == ';' || ch == '=' ||
			    ch == '@')
				return true;
			return false;
		}
	}
}
