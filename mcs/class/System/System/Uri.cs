//
// System.Uri
//
// Author:
//    Lawrence Pit (loz@cable.a2000.nl)
//
// Author:
//    Garrett Rooney (rooneg@electricjellyfish.net)
//
// (C) 2001 Garrett Rooney
//

using System.Net;
using System.Runtime.Serialization;
using System.Text;

// See RFC 2396 for more info on URI's.

// TODO: optimize by parsing host string only once

namespace System 
{
	[Serializable]
	public class Uri : MarshalByRefObject, ISerializable 
	{
		// NOTES:
		// o  scheme excludes the scheme delimiter
		// o  port is -1 to indicate no port is defined
		// o  path is empty or starts with / when scheme delimiter == "://"
		// o  query is empty or starts with ? char
		// o  fragment is empty or starts with # char
		// o  all class variables are in escaped format when they are escapable,
		//    except cachedToString.
		
		private string scheme = String.Empty;
		private string host = String.Empty;
		private int port = -1;
		private string path = String.Empty;
		private string query = String.Empty;
		private string fragment = String.Empty;
		private string userinfo = String.Empty;
		
		private bool userEscaped = false;
		private string cachedAbsoluteUri = null;
		private string cachedToString = null;
		private int cachedHashCode = 0;
		
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

		protected Uri (SerializationInfo serializationInfo, 
			       StreamingContext streamingContext) :
			this (serializationInfo.GetString ("AbsoluteUri"), true)
		{
		}

		public Uri (string uriString, bool dontEscape) 
		{			
			userEscaped = dontEscape;
			Parse (uriString);
			
			if (userEscaped) 
				return;
			
			host = EscapeString (host, false, true, false);
			path = EscapeString (path);
			query = EscapeString (query);
			fragment = EscapeString (fragment, false, false, true);
		}

		public Uri (Uri baseUri, string relativeUri) 
			: this (baseUri, relativeUri, false) 
		{			
		}

		public Uri (Uri baseUri, string relativeUri, bool dontEscape) 
		{
			// See RFC 2396 Par 5.2 and Appendix C

			userEscaped = dontEscape;

			this.scheme = baseUri.scheme;
			this.host = baseUri.host;
			this.port = baseUri.port;
			this.userinfo = baseUri.userinfo;
			
			if (relativeUri == null)
				throw new NullReferenceException ("relativeUri");

			if (relativeUri == String.Empty) {
				this.path = baseUri.path;
				this.query = baseUri.query;
				this.fragment = baseUri.fragment;
				return;
			}
			
			int pos = relativeUri.IndexOf (':');
			if (pos != -1) {
				int pos2 = relativeUri.IndexOfAny (new char [] {'/', '\\'});
				if (pos2 > pos) {
					// equivalent to new Uri (relativeUri, dontEscape)
					Parse (relativeUri);

					if (userEscaped) 
						return;

					host = EscapeString (host, false, true, false);
					path = EscapeString (path);
					query = EscapeString (query);
					fragment = EscapeString (fragment, false, false, true);
					return;
				}
			}
						
			// 8 fragment
			pos = relativeUri.IndexOf ('#');
			if (pos != -1) {
				fragment = relativeUri.Substring (pos);
				if (!userEscaped)
					fragment = EscapeString (fragment, false, false, true);
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

			if (relativeUri[0] == '/') {
				path = relativeUri;
				if (!userEscaped)
					path = EscapeString (path);
				return;
			}
			
			// par 5.2 step 6 a)
			path = baseUri.path;
			pos = path.LastIndexOf ('/');
			if (pos > 0) 
				path = path.Substring (0, pos + 1);
				
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
				Console.WriteLine ("6f " + pos);
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
				if (cachedAbsoluteUri == null)
					cachedAbsoluteUri = GetLeftPart (UriPartial.Path) + query + fragment;
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

		public UriHostNameType HostNameType { 
			get { return CheckHostName (host); } 
		}

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
					
				try {
					return IPAddress.IsLoopback (IPAddress.Parse (host));
				} catch (FormatException) {}

				try {
					return IPv6Address.IsLoopback (IPv6Address.Parse (host));
				} catch (FormatException) {}
				
				return false;
			} 
		}

		public bool IsUnc { 
			get { return (scheme == Uri.UriSchemeFile); } 
		}

		public string LocalPath { 
			get { 
				if (!IsUnc)
					return path;				
				
				// support *nix and W32 styles
				if (path.Length > 1 && path [1] == ':')
					return Unescape (path.Replace ('/', '\\'));
					
				if (System.IO.Path.DirectorySeparatorChar == '\\')
					return "\\\\" + Unescape (host + path.Replace ('/', '\\'));
				else 
					return "/" + Unescape (host + path);
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
				string p = path.EndsWith ("/") 
				         ? path.Remove (path.Length - 1, 1)
				         : path;
				string [] segments = p.Split ('/');
				int len = segments.Length - 1;
				for (int i = 0; i < len; i++) 
					segments [i] += '/';
				if (path.EndsWith ("/"))
					segments [len] += '/';
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
		
		public static UriHostNameType CheckHostName (string name) 
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
		}
		
		internal static bool IsIPv4Address (string name)
		{		
			string [] captures = name.Split (new char [] {'.'});
			if (captures.Length != 4)
				return false;
			for (int i = 0; i < 4; i++) {
				try {
					int d = Int32.Parse (captures [i]);
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
			
			return ((this.scheme == uri.scheme) &&
			        (this.userinfo == uri.userinfo) &&
			        (this.host == uri.host) &&
			        (this.port == uri.port) &&
			        (this.path == uri.path) &&
			        (this.query == uri.query));
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
				return scheme + GetSchemeDelimiter (scheme); 				
			case UriPartial.Authority :
				if (host == String.Empty ||
				    scheme == Uri.UriSchemeMailto ||
				    scheme == Uri.UriSchemeNews)
					return String.Empty;
					
				StringBuilder s = new StringBuilder ();
				s.Append (scheme);
				s.Append (GetSchemeDelimiter (scheme));
				if (path.Length > 1 && path [1] == ':' && "file".Equals (scheme)) 
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
				sb.Append (GetSchemeDelimiter (scheme));
				if (path.Length > 1 && path [1] == ':' && "file".Equals (scheme)) 
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
				
			if (((index + 3) > pattern.Length) ||
			    (pattern [index] != '%') || 
			    !IsHexDigit (pattern [index + 1]) || 
			    !IsHexDigit (pattern [index + 2]))
			{
				return pattern[index++];
			}
			
			index++;
			return (char) ((FromHex (pattern [index++]) << 4) + FromHex (pattern [index++]));
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
			int max = Math.Max (segments.Length, segments2.Length);
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
				
			cachedToString = Unescape (AbsoluteUri);
			
			return cachedToString;
		}

		public void GetObjectData (SerializationInfo info, 
					  StreamingContext context)
		{
			info.AddValue ("AbsoluteUri", this.AbsoluteUri);
		}


		// Internal Methods		

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
				char c = str [i];
				// reserved    = ";" | "/" | "?" | ":" | "@" | "&" | "=" | "+" | "$" | ","
				// mark        = "-" | "_" | "." | "!" | "~" | "*" | "'" | "(" | ")"
				// control     = <US-ASCII coded characters 00-1F and 7F hexadecimal>
				// space       = <US-ASCII coded character 20 hexadecimal>
				// delims      = "<" | ">" | "#" | "%" | <">
				// unwise      = "{" | "}" | "|" | "\" | "^" | "[" | "]" | "`"

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
			
			return s.ToString ();
		}		
		
		protected virtual string Unescape (string str) 
		{
			if (str == null)
				return String.Empty;
			StringBuilder s = new StringBuilder ();
			int len = str.Length;
			for (int i = 0; i < len; i++) {
				char c = str [i];
				if (c == '%') {
					s.Append (HexUnescape (str, ref i));
					i--;
				} else
					s.Append (c);					
			}
			return s.ToString ();
		}

		
		// Private Methods
		
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
				throw new UriFormatException ();

			// 1
			char c = 'x';
			int pos = 0;
			for (; pos < len; pos++) {
				c = uriString [pos];
				if ((c == ':') || (c == '/') || (c == '\\') || (c == '?') || (c == '#')) 
					break;
			}

			// 2 scheme
			if (c == ':') {				
				if (pos == 1) {
					// a windows filepath
					scheme = "file";
					path = uriString.Replace ('\\', '/');
					return;
				}
					
				scheme = uriString.Substring (0, pos).ToLower ();
				uriString = uriString.Remove (0, pos + 1);
			} else 
				scheme = "file";			
						
			// 3
			if ((uriString.Length >= 2) && 
			    ((uriString [0] == '/') || (uriString [0] == '\\')) &&
			    ((uriString [1] == '/') || (uriString [1] == '\\'))) 
				if ("file".Equals (scheme)) 
					uriString = uriString.TrimStart (new char [] {'/', '\\'});			    	
				else
				    	uriString = uriString.Remove (0, 2);
			    	
			// 8 fragment
			pos = uriString.IndexOf ('#');
			if (pos != -1) {
				fragment = uriString.Substring (pos);
				uriString = uriString.Substring (0, pos);
			}

			// 6 query
			pos = uriString.IndexOf ('?');
			if (pos != -1) {
				query = uriString.Substring (pos);
				uriString = uriString.Substring (0, pos);
			}
			
			// 5 path
			pos = uriString.IndexOfAny (new char[] {'/', '\\'});
			if (pos == -1) {
				if ((scheme != Uri.UriSchemeMailto) &&
				    (scheme != Uri.UriSchemeNews))
					path = "/";
			} else {
				path = uriString.Substring (pos).Replace ('\\', '/');
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
			if (pos != -1 && pos != (uriString.Length - 1)) {
				string portStr = uriString.Remove (0, pos + 1);
				if (portStr.Length > 1 && portStr [portStr.Length - 1] != ']') {
					try {
						port = Int32.Parse (portStr);
						new System.Net.IPEndPoint (0, port);  // test validity port
						uriString = uriString.Substring (0, pos);
					} catch (Exception) {
						throw new UriFormatException ("Invalid URI: invalid port number");
					}
				}
			}
			if (port == -1) {
				port = GetDefaultPort (scheme);
			}
			
			// 4 authority
			host = uriString;
			if (host.Length > 1 && host [0] == '[' && host [host.Length - 1] == ']') 
				try {
					host = "[" + IPv6Address.Parse (host).ToString () + "]";
				} catch (Exception) {
					throw new UriFormatException ("Invalid URI: The hostname could not be parsed");
				}
			if (host.Length == 2 && host [1] == ':') {
				// windows filepath
				path = host + path;
				host = String.Empty;
			}
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
			new UriScheme (UriSchemeHttps, SchemeDelimiter, 223),
			new UriScheme (UriSchemeFtp, SchemeDelimiter, 23),
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

		protected virtual bool IsBadFileSystemCharacter (char ch)
		{
			foreach (char c in System.IO.Path.InvalidPathChars)
				if (c == ch)
					return true;
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

		protected virtual bool IsReservedCharacter (char ch)
		{
			if (ch == '$' || ch == '&' || ch == '+' || ch == ',' ||
			    ch == '/' || ch == ':' || ch == ';' || ch == '=' ||
			    ch == '@')
				return true;
			return false;
		}
	}
}
