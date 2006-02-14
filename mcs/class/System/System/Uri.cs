//
// System.Uri
//
// Authors:
//    Lawrence Pit (loz@cable.a2000.nl)
//    Garrett Rooney (rooneg@electricjellyfish.net)
//    Ian MacLean (ianm@activestate.com)
//    Ben Maurer (bmaurer@users.sourceforge.net)
//    Atsushi Enomoto (atsushi@ximian.com)
//    Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Garrett Rooney
// (C) 2003 Ian MacLean
// (C) 2003 Ben Maurer
// Copyright (C) 2003,2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Collections;
using System.Globalization;

// See RFC 2396 for more info on URI's.

// TODO: optimize by parsing host string only once

namespace System {

	[Serializable]
#if NET_2_0
	[TypeConverter (typeof (UriTypeConverter))]
	public class Uri : ISerializable {
#else
	public class Uri : MarshalByRefObject, ISerializable {
#endif
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

		private bool isUnixFilePath;
		private string source;
		private string scheme = String.Empty;
		private string host = String.Empty;
		private int port = -1;
		private string path = String.Empty;
		private string query = String.Empty;
		private string fragment = String.Empty;
		private string userinfo = String.Empty;
		private bool isUnc;
		private bool isOpaquePart;
		private bool isAbsoluteUri = true;

		private string [] segments;
		
		private bool userEscaped;
		private string cachedAbsoluteUri;
		private string cachedToString;
		private string cachedLocalPath;
		private int cachedHashCode;
		
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
#if NET_2_0
		public static readonly string UriSchemeNetPipe = "net.pipe";
		public static readonly string UriSchemeNetTcp = "net.tcp";
#endif

		// Constructors		

		public Uri (string uriString) : this (uriString, false) 
		{
		}

		protected Uri (SerializationInfo serializationInfo, 
			       StreamingContext streamingContext) :
			this (serializationInfo.GetString ("AbsoluteUri"), true)
		{
		}

#if NET_2_0
		public Uri (string uriString, UriKind uriKind)
		{
			source = uriString;
			ParseUri ();

			switch (uriKind) {
			case UriKind.Absolute:
				if (!IsAbsoluteUri)
					throw new UriFormatException("Invalid URI: The format of the URI could not be "
						+ "determined.");
				break;
			case UriKind.Relative:
				if (IsAbsoluteUri)
					throw new UriFormatException("Invalid URI: The format of the URI could not be "
						+ "determined because the parameter 'uriString' represents an absolute URI.");
				break;
			case UriKind.RelativeOrAbsolute:
				break;
			default:
				string msg = Locale.GetText ("Invalid UriKind value '{0}'.", uriKind);
				throw new ArgumentException ("uriKind", msg);
			}
		}

		public Uri (Uri baseUri, Uri relativeUri)
			: this (baseUri, relativeUri.OriginalString, false)
		{
			// FIXME: this should call UriParser.Resolve
		}

		// note: doc says that dontEscape is always false but tests show otherwise
		[Obsolete]
		public Uri (string uriString, bool dontEscape) 
		{
			userEscaped = dontEscape;
			source = uriString;
			ParseUri ();
			if (!isAbsoluteUri)
				throw new UriFormatException("Invalid URI: The format of the URI could not be "
					+ "determined.");
		}
#else
		public Uri (string uriString, bool dontEscape) 
		{
			userEscaped = dontEscape;
			source = uriString;
			Parse ();
			if (!isAbsoluteUri)
				throw new UriFormatException("Invalid URI: The format of the URI could not be "
					+ "determined.");
		}
#endif

		public Uri (Uri baseUri, string relativeUri) 
			: this (baseUri, relativeUri, false) 
		{
			// FIXME: this should call UriParser.Resolve
		}

#if NET_2_0
		[Obsolete ("dontEscape is always false")]
#endif
		public Uri (Uri baseUri, string relativeUri, bool dontEscape) 
		{
#if NET_2_0
			if (baseUri == null)
				throw new ArgumentNullException ("baseUri");
			if (relativeUri == null)
				relativeUri = String.Empty;
#else
			if (baseUri == null)
				throw new NullReferenceException ("baseUri");
#endif
			// See RFC 2396 Par 5.2 and Appendix C

			userEscaped = dontEscape;

			// Check Windows UNC (for // it is scheme/host separator)
			if (relativeUri.StartsWith ("\\\\")) {
				source = relativeUri;
#if NET_2_0
				ParseUri ();
#else
				Parse ();
#endif
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
#if NET_2_0
					ParseUri ();
#else
					Parse ();
#endif
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
				if (userEscaped)
					fragment = relativeUri.Substring (pos);
				else
					fragment = "#" + EscapeString (relativeUri.Substring (pos+1));
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
#if NET_2_0
					ParseUri ();
#else
					Parse ();
#endif
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
			get {
#if NET_2_0
				switch (Scheme) {
				case "mailto":
				case "file":
					// faster (mailto) and special (file) cases
					return path;
				default:
					if (path.Length == 0) {
						string start = Scheme + SchemeDelimiter;
						if (path.StartsWith (start))
							return "/";
						else
							return String.Empty;
					}
					return path;
				}
#else
				return path;
#endif
			}
		}

		public string AbsoluteUri { 
			get { 
				EnsureAbsoluteUri ();
				if (cachedAbsoluteUri == null) {
					cachedAbsoluteUri = GetLeftPart (UriPartial.Path);
					if (query.Length > 0)
						cachedAbsoluteUri += query;
					if (fragment.Length > 0)
						cachedAbsoluteUri += fragment;
				}
				return cachedAbsoluteUri;
			} 
		}

		public string Authority { 
			get { 
				return (GetDefaultPort (Scheme) == port)
				     ? host : host + ":" + port;
			} 
		}

		public string Fragment { 
			get { 
				EnsureAbsoluteUri ();
				return fragment; 
			} 
		}

		public string Host { 
			get { 
				EnsureAbsoluteUri ();
				return host; 
			} 
		}

		public UriHostNameType HostNameType { 
			get {
				UriHostNameType ret = CheckHostName (Host);
				if (ret != UriHostNameType.Unknown)
					return ret;
#if NET_2_0
				switch (Scheme) {
				case "mailto":
					return UriHostNameType.Basic;
				default:
					return (IsFile) ? UriHostNameType.Basic : ret;
				}
#else
				// looks it always returns Basic...
				return UriHostNameType.Basic; //.Unknown;
#endif
			} 
		}

		public bool IsDefaultPort { 
			get { return GetDefaultPort (Scheme) == port; }
		}

		public bool IsFile { 
			get { return (Scheme == UriSchemeFile); }
		}

		public bool IsLoopback { 
			get {
				if (Host.Length == 0) {
#if NET_2_0
					return IsFile;
#else
					return false;
#endif
				}

				if (host == "loopback" || host == "localhost") 
					return true;
					
				try {
					if (IPAddress.Loopback.Equals (IPAddress.Parse (host)))
						return true;
				} catch (FormatException) {}

				try {
					return IPv6Address.IsLoopback (IPv6Address.Parse (host));
				} catch (FormatException) {}
				
				return false;
			} 
		}

		public bool IsUnc {
			// rule: This should be true only if
			//   - uri string starts from "\\", or
			//   - uri string starts from "//" (Samba way)
			get { 
				EnsureAbsoluteUri ();
				return isUnc; 
			} 
		}

		public string LocalPath { 
			get {
				EnsureAbsoluteUri ();
				if (cachedLocalPath != null)
					return cachedLocalPath;
				if (!IsFile)
					return AbsolutePath;

				bool windows = (path.Length > 3 && path [1] == ':' &&
						(path [2] == '\\' || path [2] == '/'));

				if (!IsUnc) {
					string p = Unescape (path);
					bool replace = windows;
#if ONLY_1_1
					replace |= (System.IO.Path.DirectorySeparatorChar == '\\');
#endif
					if (replace)
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
					else if (System.IO.Path.DirectorySeparatorChar == '\\') {
						string h = host;
						if (path.Length > 0) {
#if NET_2_0
							if ((path.Length > 1) || (path[0] != '/')) {
								h += path.Replace ('/', '\\');
							}
#else
							h += path.Replace ('/', '\\');
#endif
						}
						cachedLocalPath = "\\\\" + Unescape (h);
					}  else
						cachedLocalPath = Unescape (path);
				}
				if (cachedLocalPath.Length == 0)
					cachedLocalPath = Path.DirectorySeparatorChar.ToString ();
				return cachedLocalPath;
			} 
		}

		public string PathAndQuery { 
			get { return path + Query; } 
		}

		public int Port { 
			get { 
				EnsureAbsoluteUri ();
				return port; 
			} 
		}

		public string Query { 
			get { 
				EnsureAbsoluteUri ();
				return query; 
			}
		}

		public string Scheme { 
			get { 
				EnsureAbsoluteUri ();
				return scheme; 
			} 
		}

		public string [] Segments { 
			get { 
				EnsureAbsoluteUri ();
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
					parts [1] = "";
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
			get { 
				EnsureAbsoluteUri ();
				return userinfo; 
			}
		}
		
#if NET_2_0
		[MonoTODO ("add support for IPv6 address")]
		public string DnsSafeHost {
			get { return Unescape (Host); }
		}

		public bool IsAbsoluteUri {
			get { return isAbsoluteUri; }
		}

		public string OriginalString {
			get { return source; }
		}
#endif

		// Methods		

		private void EnsureAbsoluteUri ()
		{
#if NET_2_0
			if (!IsAbsoluteUri)
				throw new InvalidOperationException ("This operation is not supported for a relative URI.");
#endif
		}
		
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

		[MonoTODO ("Find out what this should do")]
#if NET_2_0
		[Obsolete]
#endif
		protected virtual void Canonicalize ()
		{
		}

		// defined in RFC3986 as = ALPHA *( ALPHA / DIGIT / "+" / "-" / ".")
		public static bool CheckSchemeName (string schemeName) 
		{
			if (schemeName == null || schemeName.Length == 0)
				return false;
			
			if (!IsAlpha (schemeName [0]))
				return false;

			int len = schemeName.Length;
			for (int i = 1; i < len; i++) {
				char c = schemeName [i];
				if (!Char.IsDigit (c) && !IsAlpha (c) && c != '.' && c != '+' && c != '-')
					return false;
			}
			
			return true;
		}

		private static bool IsAlpha (char c)
		{
#if NET_2_0
			// as defined in rfc2234
			// %x41-5A / %x61-7A (A-Z / a-z)
			int i = (int) c;
			return (((i >= 0x41) && (i <= 0x5A)) || ((i >= 0x61) && (i <= 0x7A)));
#else
			// Fx 1.x got this too large
			return Char.IsLetter (c);
#endif
		}

		[MonoTODO ("Find out what this should do")]
#if NET_2_0
		[Obsolete]
#endif
		protected virtual void CheckSecurity ()
		{
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
				if ((scheme == Uri.UriSchemeMailto) || (scheme == Uri.UriSchemeNews))
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

				if (path.Length > 0) {
#if NET_2_0
					switch (Scheme) {
					case "mailto":
					case "news":
						sb.Append (path);
						break;
					default:
						sb.Append (Reduce (path));
						break;
					}
#else
					sb.Append (path);
#endif
				}
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

			if (!IsHexEncoding (pattern, index))
				return pattern [index++];

			index++;
			int msb = FromHex (pattern [index++]);
			int lsb = FromHex (pattern [index++]);
			return (char) ((msb << 4) | lsb);
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

#if NET_2_0
		[Obsolete]
#endif
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
			int max = Math.Min (segments.Length, segments2.Length);
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

			cachedToString = Unescape (GetLeftPart (UriPartial.Path), true);
			if (query.Length > 0) {
				string q = query.StartsWith ("?") ? '?' + Unescape (query.Substring (1), true) : Unescape (query, true);
				cachedToString += q;
			}
			if (fragment.Length > 0)
				cachedToString += fragment;
			return cachedToString;
		}

#if NET_2_0
		protected void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("AbsoluteUri", this.AbsoluteUri);
		}
#endif

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			info.AddValue ("AbsoluteUri", this.AbsoluteUri);
		}


		// Internal Methods		

#if NET_2_0
		[Obsolete]
#endif
		protected virtual void Escape ()
		{
			path = EscapeString (path);
		}

#if NET_2_0
		[Obsolete]
#endif
		protected static string EscapeString (string str) 
		{
			return EscapeString (str, false, true, true);
		}
		
		internal static string EscapeString (string str, bool escapeReserved, bool escapeHex, bool escapeBrackets) 
		{
			if (str == null)
				return String.Empty;
			
			byte [] data = Encoding.UTF8.GetBytes (str);
			StringBuilder s = new StringBuilder ();
			int len = data.Length;	
			for (int i = 0; i < len; i++) {
				char c = (char) data [i];
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
				if('%' == c && IsHexEncoding(str,i))
				{
					// if ,yes , copy it as is
					s.Append(c);
					s.Append(str[++i]);
					s.Append(str[++i]);
					continue;
				}

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

		// On .NET 1.x, this method is called from .ctor(). When overriden, we 
		// can avoid the "absolute uri" constraints of the .ctor() by
		// overriding with custom code.
#if NET_2_0
		[Obsolete("The method has been deprecated. It is not used by the system.")]
#endif
		protected virtual void Parse ()
		{
#if !NET_2_0
			ParseUri ();
#endif
		}

		private void ParseUri ()
		{
			Parse (source);

			if (userEscaped)
				return;

			host = EscapeString (host, false, true, false);
			if (host.Length > 1 && host [0] != '[' && host [host.Length - 1] != ']') {
				// host name present (but not an IPv6 address)
				host = host.ToLower (CultureInfo.InvariantCulture);
			}

			if (path.Length > 0) {
				path = EscapeString (path);
			}
		}

#if NET_2_0
		[Obsolete]
#endif
		protected virtual string Unescape (string str)
		{
			return Unescape (str, false);
		}
		
		internal static string Unescape (string str, bool excludeSpecial) 
		{
			if (str == null)
				return String.Empty;
			StringBuilder s = new StringBuilder ();
			int len = str.Length;
			for (int i = 0; i < len; i++) {
				char c = str [i];
				if (c == '%') {
					char surrogate;
					char x = HexUnescapeMultiByte (str, ref i, out surrogate);
					if (excludeSpecial && x == '#')
						s.Append ("%23");
					else if (excludeSpecial && x == '%')
						s.Append ("%25");
					else if (excludeSpecial && x == '?')
						s.Append ("%3F");
					else {
						s.Append (x);
						if (surrogate != char.MinValue)
							s.Append (surrogate);
					}
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
				throw new UriFormatException ("Relative file path is not allowed.");
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
				throw new UriFormatException ();

			int pos = 0;

			// 1, 2
			// Identify Windows path, unix path, or standard URI.
			pos = uriString.IndexOf (':');
			if (pos == 0) {
				throw new UriFormatException("Invalid URI: "
					+ "The format of the URI could not be "
					+ "determined.");
			} else if (pos < 0) {
				// It must be Unix file path or Windows UNC
				if (uriString [0] == '/')
					ParseAsUnixAbsoluteFilePath (uriString);
				else if (uriString.StartsWith ("\\\\"))
					ParseAsWindowsUNC (uriString);
				else
					/* Relative path */
					isAbsoluteUri = false;
				return;
			} 
			else if (pos == 1) {
				if (!IsAlpha (uriString [0]))
					throw new UriFormatException ("URI scheme must start with a letter.");
				// This means 'a:' == windows full path.
				ParseAsWindowsAbsoluteFilePath (uriString);
				return;
			}

			// scheme
			scheme = uriString.Substring (0, pos).ToLower (CultureInfo.InvariantCulture);
			// Check scheme name characters as specified in RFC2396.
			// Note: different checks in 1.x and 2.0
			if (!CheckSchemeName (scheme)) {
				string msg = Locale.GetText ("URI scheme must start with a letter and must consist of one of alphabet, digits, '+', '-' or '.' character.");
				throw new UriFormatException (msg);
			}

			uriString = uriString.Substring (pos + 1);

			// 8 fragment
			pos = uriString.IndexOf ('#');
			if (!IsUnc && pos != -1) {
				if (userEscaped)
					fragment = uriString.Substring (pos);
				else
					fragment = "#" + EscapeString (uriString.Substring (pos+1));

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
			if (IsPredefinedScheme (scheme) && scheme != UriSchemeMailto && scheme != UriSchemeNews && (
				(uriString.Length < 2) ||
				(uriString.Length >= 2 && uriString [0] == '/' && uriString [1] != '/')))				
				throw new UriFormatException ("Invalid URI: The Authority/Host could not be parsed.");
			
			
			bool unixAbsPath = scheme == UriSchemeFile && (uriString.StartsWith ("///") || uriString == "//");
			if (uriString.StartsWith ("//")) {
				if (scheme != UriSchemeMailto && scheme != UriSchemeNews)
					uriString = uriString.Substring (2);

				if (scheme == UriSchemeFile) {
					int num_leading_slash = 2;
					for (int i = 0; i < uriString.Length; i++) {
						if (uriString [i] != '/')
							break;
						num_leading_slash++;
					}
					if (num_leading_slash >= 4) {
						unixAbsPath = false;
						uriString = uriString.TrimStart ('/');
					} else if (num_leading_slash >= 3) {
						uriString = uriString.Substring (1);
					}
				}
				
				if (uriString.Length > 1 && uriString [1] == ':')
					unixAbsPath = false;

			} else if (!IsPredefinedScheme (scheme)) {
				path = uriString;
				isOpaquePart = true;
				return;
			}

			// 5 path
			pos = uriString.IndexOf ('/');
			if (unixAbsPath)
				pos = -1;
			if (pos == -1) {
				if ((scheme != Uri.UriSchemeMailto) &&
#if ONLY_1_1
				    (scheme != Uri.UriSchemeFile) &&
#endif
				    (scheme != Uri.UriSchemeNews))
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
				if (portStr.Length > 1 && portStr[portStr.Length - 1] != ']') {
					try {
#if NET_2_0
						port = (int) UInt16.Parse (portStr, CultureInfo.InvariantCulture);
#else
						port = (int) UInt32.Parse (portStr, CultureInfo.InvariantCulture);
#endif
						uriString = uriString.Substring (0, pos);
					} catch (Exception) {
						throw new UriFormatException ("Invalid URI: Invalid port number");
					}
				} else {
					if (port == -1) {
						port = GetDefaultPort (scheme);
					}
				}
			} else {
				if (port == -1) {
					port = GetDefaultPort (scheme);
				}
			}
			
			// 4 authority
			host = uriString;

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
			} else if (scheme == UriSchemeFile) {
				isUnc = true;
			} else if (scheme == UriSchemeNews) {
				// no host for 'news', misinterpreted path
				if (host.Length > 0) {
					path = host;
					host = String.Empty;
				}
			} else if (host.Length == 0 &&
				(scheme == UriSchemeHttp || scheme == UriSchemeGopher || scheme == UriSchemeNntp
				 || scheme == UriSchemeHttps || scheme == UriSchemeFtp)) {
				throw new UriFormatException ("Invalid URI: The hostname could not be parsed");
			}

			bool badhost = ((host.Length > 0) && (CheckHostName (host) == UriHostNameType.Unknown));
			if (!badhost && (host.Length > 1) && (host[0] == '[') && (host[host.Length - 1] == ']')) {
				try {
					host = "[" + IPv6Address.Parse (host).ToString (true) + "]";
				}
				catch (Exception) {
					badhost = true;
				}
			}
			if (badhost && (scheme != "monodoc")) {
				string msg = Locale.GetText ("Invalid URI: The hostname could not be parsed.");
				throw new UriFormatException (msg);
			}

			if ((scheme != Uri.UriSchemeMailto) &&
					(scheme != Uri.UriSchemeNews) &&
					(scheme != Uri.UriSchemeFile)) {
				path = Reduce (path);
			}
		}

		private static string Reduce (string path)
		{
			path = path.Replace ('\\','/');
			string [] parts = path.Split ('/');
			ArrayList result = new ArrayList ();

			int end = parts.Length;
			for (int i = 0; i < end; i++) {
				string current = parts [i];
				if (current == "" || current == "." )
					continue;

				if (current == "..") {
					int resultCount = result.Count;
#if NET_2_0
					// in 2.0 profile, skip leading ".." parts
					if (resultCount == 0) {
						continue;
					}

					result.RemoveAt (resultCount - 1);
					continue;
#else
					// in 1.x profile, retain leading ".." parts, and only reduce
					// URI is previous part is not ".."
					if (resultCount > 0) {
						if ((string) result[resultCount - 1] != "..") {
							result.RemoveAt (resultCount - 1);
							continue;
						}
					}
#endif
				}

				result.Add (current);
			}

			if (result.Count == 0)
				return "/";

			if (path [0] == '/')
				result.Insert (0, "");

			string res = String.Join ("/", (string []) result.ToArray (typeof (string)));
			if (path.EndsWith ("/"))
				res += '/';
				
			return res;
		}

		// A variant of HexUnescape() which can decode multi-byte escaped
		// sequences such as (e.g.) %E3%81%8B into a single character
		private static char HexUnescapeMultiByte (string pattern, ref int index, out char surrogate) 
		{
			surrogate = char.MinValue;

			if (pattern == null) 
				throw new ArgumentException ("pattern");
				
			if (index < 0 || index >= pattern.Length)
				throw new ArgumentOutOfRangeException ("index");

			if (!IsHexEncoding (pattern, index))
				return pattern [index++];

			int orig_index = index++;
			int msb = FromHex (pattern [index++]);
			int lsb = FromHex (pattern [index++]);

			// We might be dealing with a multi-byte character:
			// The number of ones at the top-end of the first byte will tell us
			// how many bytes will make up this character.
			int msb_copy = msb;
			int num_bytes = 0;
			while ((msb_copy & 0x8) == 0x8) {
				num_bytes++;
				msb_copy <<= 1;
			}

			// We might be dealing with a single-byte character:
			// If there was only 0 or 1 leading ones then we're not dealing
			// with a multi-byte character.
			if (num_bytes <= 1)
				return (char) ((msb << 4) | lsb);

			// Now that we know how many bytes *should* follow, we'll check them
			// to ensure we are dealing with a valid multi-byte character.
			byte [] chars = new byte [num_bytes];
			bool all_invalid = false;
			chars[0] = (byte) ((msb << 4) | lsb);

			for (int i = 1; i < num_bytes; i++) {
				if (!IsHexEncoding (pattern, index++)) {
					all_invalid = true;
					break;
				}

				// All following bytes must be in the form 10xxxxxx
				int cur_msb = FromHex (pattern [index++]);
				if ((cur_msb & 0xc) != 0x8) {
					all_invalid = true;
					break;
				}

				int cur_lsb = FromHex (pattern [index++]);
				chars[i] = (byte) ((cur_msb << 4) | cur_lsb);
			}

			// If what looked like a multi-byte character is invalid, then we'll
			// just return the first byte as a single byte character.
			if (all_invalid) {
				index = orig_index + 3;
				return (char) chars[0];
			}

			// Otherwise, we're dealing with a valid multi-byte character.
			// We need to ignore the leading ones from the first byte:
			byte mask = (byte) 0xFF;
			mask >>= (num_bytes + 1);
			int result = chars[0] & mask;

			// The result will now be built up from the following bytes.
			for (int i = 1; i < num_bytes; i++) {
				// Ignore upper two bits
				result <<= 6;
				result |= (chars[i] & 0x3F);
			}

			if (result <= 0xFFFF) {
				return (char) result;
			} else {
				// We need to handle this as a UTF16 surrogate (i.e. return
				// two characters)
				result -= 0x10000;
				surrogate = (char) ((result & 0x3FF) | 0xDC00);
				return (char) ((result >> 10) | 0xD800);
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
			new UriScheme (UriSchemeHttps, SchemeDelimiter, 443),
			new UriScheme (UriSchemeFtp, SchemeDelimiter, 21),
			new UriScheme (UriSchemeFile, SchemeDelimiter, -1),
			new UriScheme (UriSchemeMailto, ":", 25),
			new UriScheme (UriSchemeNews, ":", 119),
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
#if NET_2_0
			UriParser parser = UriParser.GetParser (scheme);
			if (parser == null)
				return -1;
			return parser.DefaultPort;
#else
			for (int i = 0; i < schemes.Length; i++) 
				if (schemes [i].scheme == scheme)
					return schemes [i].defaultPort;
			return -1;
#endif
		}

		private string GetOpaqueWiseSchemeDelimiter ()
		{
			if (isOpaquePart)
				return ":";
			else
				return GetSchemeDelimiter (scheme);
		}

#if NET_2_0
		[Obsolete]
#endif
		protected virtual bool IsBadFileSystemCharacter (char ch)
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

#if NET_2_0
		[Obsolete]
#endif
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
#if NET_2_0
			case "net.pipe":
			case "net.tcp":
#endif
				return true;
			default:
				return false;
			}
		}

#if NET_2_0
		[Obsolete]
#endif
		protected virtual bool IsReservedCharacter (char ch)
		{
			if (ch == '$' || ch == '&' || ch == '+' || ch == ',' ||
			    ch == '/' || ch == ':' || ch == ';' || ch == '=' ||
			    ch == '@')
				return true;
			return false;
		}
#if NET_2_0
		private UriParser parser;

		private UriParser Parser {
			get {
				if (parser == null)
					parser = UriParser.GetParser (Scheme);
				return parser;
			}
			set { parser = value; }
		}

		public string GetComponents (UriComponents components, UriFormat format)
		{
			return Parser.GetComponents (this, components, format);
		}

		public bool IsBaseOf (Uri uri)
		{
			return Parser.IsBaseOf (this, uri);
		}

		public bool IsWellFormedOriginalString ()
		{
			return Parser.IsWellFormedOriginalString (this);
		}

		[MonoTODO]
		public Uri MakeRelativeUri (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			throw new NotImplementedException ();
		}

		// static methods

		private const int MaxUriLength = 32766;

		public static int Compare (Uri uri1, Uri uri2, UriComponents partsToCompare, UriFormat compareFormat, StringComparison comparisonType)
		{
			if ((comparisonType < StringComparison.CurrentCulture) || (comparisonType > StringComparison.OrdinalIgnoreCase)) {
				string msg = Locale.GetText ("Invalid StringComparison value '{0}'", comparisonType);
				throw new ArgumentException ("comparisonType", msg);
			}

			if ((uri1 == null) && (uri2 == null))
				return 0;

			string s1 = uri1.GetComponents (partsToCompare, compareFormat);
			string s2 = uri2.GetComponents (partsToCompare, compareFormat);
			return String.Compare (s1, s2, comparisonType);
		}

		[MonoTODO]
		public static string EscapeDataString (string stringToEscape)
		{
			if (stringToEscape == null)
				throw new ArgumentNullException ("stringToEscape");

			if (stringToEscape.Length > MaxUriLength) {
				string msg = Locale.GetText ("Uri is longer than the maximum {0} characters.");
				throw new UriFormatException (msg);
			}

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string EscapeUriString (string stringToEscape)
		{
			if (stringToEscape == null)
				throw new ArgumentNullException ("stringToEscape");

			if (stringToEscape.Length > MaxUriLength) {
				string msg = Locale.GetText ("Uri is longer than the maximum {0} characters.");
				throw new UriFormatException (msg);
			}

			throw new NotImplementedException ();
		}

		public static bool IsWellFormedUriString (string uriString, UriKind uriKind)
		{
			if (uriString == null)
				return false;
			Uri uri = new Uri (uriString, uriKind);
			return uri.IsWellFormedOriginalString ();
		}

		[MonoTODO ("rework code to avoid exception catching")]
		public static bool TryCreate (string uriString, UriKind uriKind, out Uri result)
		{
			try {
				result = new Uri (uriString, uriKind);
				return true;
			}
			catch (UriFormatException) {
				result = null;
				return false;
			}
		}

		[MonoTODO ("rework code to avoid exception catching")]
		public static bool TryCreate (Uri baseUri, string relativeUri, out Uri result)
		{
			try {
				// FIXME: this should call UriParser.Resolve
				result = new Uri (baseUri, relativeUri);
				return true;
			}
			catch (UriFormatException) {
				result = null;
				return false;
			}
		}

		[MonoTODO ("rework code to avoid exception catching")]
		public static bool TryCreate (Uri baseUri, Uri relativeUri, out Uri result)
		{
			try {
				// FIXME: this should call UriParser.Resolve
				result = new Uri (baseUri, relativeUri);
				return true;
			}
			catch (UriFormatException) {
				result = null;
				return false;
			}
		}

		[MonoTODO]
		public static string UnescapeDataString (string stringToUnescape)
		{
			if (stringToUnescape == null)
				throw new ArgumentNullException ("stringToUnescape");

			throw new NotImplementedException ();
		}
#endif
	}
}
