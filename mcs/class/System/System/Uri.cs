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
//    Stephane Delcroix  <stephane@delcroix.org>
//
// (C) 2001 Garrett Rooney
// (C) 2003 Ian MacLean
// (C) 2003 Ben Maurer
// Copyright (C) 2003,2009 Novell, Inc (http://www.novell.com)
// Copyright (c) 2009 Stephane Delcroix
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
// See RFC 2396 for more info on URI's.
//
// TODO: optimize by parsing host string only once
//
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;

//
// Disable warnings on Obsolete methods being used
//
#pragma warning disable 612

namespace System {

	[Serializable]
	[TypeConverter (typeof (UriTypeConverter))]
	public class Uri : ISerializable {
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

		private string source;
		private string scheme = String.Empty;
		private string host = String.Empty;
		private int port = -1;
		private string path = String.Empty;
		private string query = String.Empty;
		private string fragment = String.Empty;
		private string userinfo;
		private bool isUnc;
		private bool isAbsoluteUri = true;
		private long scope_id;

		private List<string> segments;
		
		private bool userEscaped;
		private string cachedAbsoluteUri;
		private string cachedToString;
		private string cachedLocalPath;
		private int cachedHashCode;
		
		private static bool s_IriParsing;

		internal static bool IriParsing {
			get { return s_IriParsing; }
			set { s_IriParsing = value; }
		}

#if BOOTSTRAP_BASIC
		private static readonly string hexUpperChars = "0123456789ABCDEF";
		private static readonly string [] Empty = new string [0];
		private static bool isWin32 = (Path.DirectorySeparatorChar == '\\');
#else
		static readonly char[] hexUpperChars = new [] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
#endif
	
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
		public static readonly string UriSchemeNetPipe = "net.pipe";
		public static readonly string UriSchemeNetTcp = "net.tcp";

		internal static readonly string UriSchemeTelnet = "telnet";
		internal static readonly string UriSchemeLdap = "ldap";
		internal static readonly string UriSchemeUuid = "uuid";

		private static readonly string [] knownUriSchemes =
		{
			UriSchemeFile,
			UriSchemeFtp,
			UriSchemeGopher,
			UriSchemeHttp,
			UriSchemeHttps,
			UriSchemeMailto,
			UriSchemeNews,
			UriSchemeNntp,
			UriSchemeNetPipe,
			UriSchemeNetTcp
		};

		// Constructors

		static Uri ()
		{
			IriParsing = true;

			var iriparsingVar = Environment.GetEnvironmentVariable ("MONO_URI_IRIPARSING");
			if (iriparsingVar == "true")
				IriParsing = true;
			else if (iriparsingVar == "false")
				IriParsing = false;
		}

		public Uri (string uriString) : this (uriString, false) 
		{
		}

		protected Uri (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			string uri = serializationInfo.GetString ("AbsoluteUri");
			if (uri.Length > 0) {
				source = uri;
				ParseUri (UriKind.Absolute);
			} else {
				uri = serializationInfo.GetString ("RelativeUri");
				if (uri.Length > 0) {
					source = uri;
					ParseUri (UriKind.Relative);
				} else {
					throw new ArgumentException ("Uri string was null or empty.");
				}
			}
		}

		public Uri (string uriString, UriKind uriKind)
		{
			source = uriString;
			ParseUri (uriKind);

			switch (uriKind) {
			case UriKind.Absolute:
				if (!IsAbsoluteUri)
					throw new UriFormatException ("Invalid URI: The format of the URI could not be "
						+ "determined.");
				break;
			case UriKind.Relative:
				if (IsAbsoluteUri)
					throw new UriFormatException ("Invalid URI: The format of the URI could not be "
						+ "determined because the parameter 'uriString' represents an absolute URI.");
				break;
			case UriKind.RelativeOrAbsolute:
				break;
			default:
				string msg = Locale.GetText ("Invalid UriKind value '{0}'.", uriKind);
				throw new ArgumentException (msg);
			}
		}

		//
		// An exception-less constructor, returns success
		// condition on the out parameter `success'.
		//
		Uri (string uriString, UriKind uriKind, out bool success)
		{
			if (uriString == null) {
				success = false;
				return;
			}

			if (uriKind != UriKind.RelativeOrAbsolute &&
				uriKind != UriKind.Absolute &&
				uriKind != UriKind.Relative) {
				string msg = Locale.GetText ("Invalid UriKind value '{0}'.", uriKind);
				throw new ArgumentException (msg);
			}

			source = uriString;
			if (ParseNoExceptions (uriKind, uriString) != null)
				success = false;
			else {
				success = true;
				
				switch (uriKind) {
				case UriKind.Absolute:
					if (!IsAbsoluteUri)
						success = false;
					break;
				case UriKind.Relative:
					if (IsAbsoluteUri)
						success = false;
					break;
				case UriKind.RelativeOrAbsolute:
					break;
				default:
					success = false;
					break;
				}
			}
		}

		public Uri (Uri baseUri, Uri relativeUri)
		{
			Merge (baseUri, relativeUri == null ? String.Empty : relativeUri.OriginalString);
			// FIXME: this should call UriParser.Resolve
		}

		// note: doc says that dontEscape is always false but tests show otherwise
		[Obsolete]
		public Uri (string uriString, bool dontEscape) 
		{
			userEscaped = dontEscape;
			source = uriString;
			ParseUri (UriKind.Absolute);
			if (!isAbsoluteUri)
				throw new UriFormatException ("Invalid URI: The format of the URI could not be "
					+ "determined: " + uriString);
		}

		public Uri (Uri baseUri, string relativeUri) 
		{
			Merge (baseUri, relativeUri);
			// FIXME: this should call UriParser.Resolve
		}

		[Obsolete ("dontEscape is always false")]
		public Uri (Uri baseUri, string relativeUri, bool dontEscape) 
		{
			userEscaped = dontEscape;
			Merge (baseUri, relativeUri);
		}

		private void Merge (Uri baseUri, string relativeUri)
		{
			if (baseUri == null)
				throw new ArgumentNullException ("baseUri");
			if (!baseUri.IsAbsoluteUri)
				throw new ArgumentOutOfRangeException ("baseUri");
			if (string.IsNullOrEmpty (relativeUri)) {
				source = baseUri.OriginalString;
				ParseUri (UriKind.Absolute);
				return;
			}

			string error;
			bool startsWithSlash = false;

			UriElements baseEl;
			if (!UriParseComponents.TryParseComponents (baseUri.OriginalString, UriKind.Absolute, out baseEl, out error))
				throw new UriFormatException (error);

			if (relativeUri.StartsWith (baseEl.scheme + ":", StringComparison.Ordinal))
				relativeUri = relativeUri.Substring (baseEl.scheme.Length + 1);

			if (relativeUri.Length >= 1 && relativeUri [0] == '/') {
				if (relativeUri.Length >= 2 && relativeUri [1] == '/') {
					source = baseEl.scheme + ":" + relativeUri;
					ParseUri (UriKind.Absolute);
					return;
				}

				relativeUri = relativeUri.Substring (1);
				startsWithSlash = true;
			}

			UriElements relativeEl;
			if (!UriParseComponents.TryParseComponents (relativeUri, UriKind.RelativeOrAbsolute, out relativeEl, out error))
				throw new UriFormatException (error);

			if (relativeEl.isAbsoluteUri) {
				source = relativeUri;
				ParseUri (UriKind.Absolute);
				return;
			}

			source = baseEl.scheme + baseEl.delimiter;

			if (baseEl.user != null)
				source += baseEl.user + "@";

			source += baseEl.host;

			if (baseEl.port >= 0)
				source += ":" + baseEl.port.ToString (CultureInfo.InvariantCulture);

			var canUseBase = true;

			string path;
			if (!string.IsNullOrEmpty (relativeEl.path) || startsWithSlash) {
				canUseBase = false;
				path = relativeEl.path;
				if (startsWithSlash)
					path = relativeEl.path;
				else {
					var pathEnd = baseEl.path.LastIndexOf ('/');
					path = (pathEnd > 0)? baseEl.path.Substring (0, pathEnd+1) : "";
					path += relativeEl.path;
				}
			} else {
				path = baseEl.path;
			}

			if ((path.Length == 0 || path [0] != '/') && baseEl.delimiter == SchemeDelimiter)
				path = "/" + path;

			source += UriHelper.Reduce (path, true);

			if (relativeEl.query != null) {
				canUseBase = false;
				source += "?" + relativeEl.query;
			} else if (canUseBase && baseEl.query != null)
				source += "?" + baseEl.query;

			if (relativeEl.fragment != null)
				source += "#" + relativeEl.fragment;
			else if (canUseBase && baseEl.fragment != null)
				source += "#" + baseEl.fragment;

			ParseUri (UriKind.Absolute);

			return;
		}
		
		// Properties
		
		public string AbsolutePath { 
			get {
				EnsureAbsoluteUri ();
				if (scheme == "mailto" || scheme == "file")
					// faster (mailto) and special (file) cases
					return path;
				
				if (path.Length == 0) {
					string start = scheme + SchemeDelimiter;
					if (path.StartsWith (start, StringComparison.Ordinal))
						return "/";
					else
						return String.Empty;
				}
				return path;
			}
		}

		public string AbsoluteUri { 
			get { 
				EnsureAbsoluteUri ();

				if (cachedAbsoluteUri == null)
					cachedAbsoluteUri = GetComponents (UriComponents.AbsoluteUri, UriFormat.UriEscaped);

				return cachedAbsoluteUri;
			} 
		}

		public string Authority { 
			get { 
				EnsureAbsoluteUri ();
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
				EnsureAbsoluteUri ();
				UriHostNameType ret = CheckHostName (Host);
				if (ret != UriHostNameType.Unknown)
					return ret;

				if (scheme == "mailto")
					return UriHostNameType.Basic;
				return (IsFile) ? UriHostNameType.Basic : ret;
			} 
		}

		public bool IsDefaultPort { 
			get {
				EnsureAbsoluteUri ();
				return GetDefaultPort (Scheme) == port;
			}
		}

		public bool IsFile { 
			get {
				EnsureAbsoluteUri ();
				return (Scheme == UriSchemeFile);
			}
		}

		public bool IsLoopback { 
			get {
				EnsureAbsoluteUri ();
				
				if (Host.Length == 0) {
					return IsFile;
				}

				if (host == "loopback" || host == "localhost") 
					return true;

				IPAddress result;
				if (IPAddress.TryParse (host, out result))
					if (IPAddress.Loopback.Equals (result))
						return true;

				IPv6Address result6;
				if (IPv6Address.TryParse (host, out result6)){
					if (IPv6Address.IsLoopback (result6))
						return true;
				}

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

		private bool IsLocalIdenticalToAbsolutePath ()
		{
			if (IsFile)
				return false;

			if ((scheme == Uri.UriSchemeNews) || (scheme == Uri.UriSchemeNntp) || (scheme == Uri.UriSchemeFtp))
				return false;

			return IsWellFormedOriginalString ();
		}

		public string LocalPath { 
			get {
				EnsureAbsoluteUri ();
				if (cachedLocalPath != null)
					return cachedLocalPath;

				var formatFlags = UriHelper.FormatFlags.NoSlashReplace;

				if (userEscaped)
					formatFlags |= UriHelper.FormatFlags.UserEscaped;

				string unescapedPath = UriHelper.FormatAbsolute (path, scheme,
					UriComponents.Path, UriFormat.Unescaped, formatFlags);

				if (path.StartsWith ("/", StringComparison.Ordinal) &&
					!unescapedPath.StartsWith ("/", StringComparison.Ordinal))
					unescapedPath = "/" + unescapedPath;

				if (IsLocalIdenticalToAbsolutePath ()) {
					cachedLocalPath = unescapedPath;
					return cachedLocalPath;
				}

				if (!IsUnc) {
					bool windows = (path.Length > 3 && path [1] == ':' &&
						(path [2] == '\\' || path [2] == '/'));

					if (windows)
						cachedLocalPath = unescapedPath.Replace ('/', '\\');
					else
						cachedLocalPath = unescapedPath;
				} else {
					// support *nix and W32 styles
					if (path.Length > 1 && path [1] == ':')
						cachedLocalPath = unescapedPath.Replace (Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

					// LAMESPEC: ok, now we cannot determine
					// if such URI like "file://foo/bar" is
					// Windows UNC or unix file path, so
					// they should be handled differently.
					else if (System.IO.Path.DirectorySeparatorChar == '\\') {
						string h = host;
						if (path.Length > 0) {
							if ((path.Length > 1) || (path[0] != '/')) {
								h += unescapedPath.Replace ('/', '\\');
							}
						}
						cachedLocalPath = "\\\\" + h;
					}  else
						cachedLocalPath = unescapedPath;
				}
				if (cachedLocalPath.Length == 0)
					cachedLocalPath = Path.DirectorySeparatorChar.ToString ();
				return cachedLocalPath;
			} 
		}

		public string PathAndQuery { 
			get {
				EnsureAbsoluteUri ();
				return path + Query;
			} 
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

				// return a (pre-allocated) empty array
				if (path.Length == 0)
#if BOOTSTRAP_BASIC
					return Empty;
#else
					return EmptyArray<string>.Value;
#endif
				// do not return the original array (since items can be changed)
				if (segments != null)
					return segments.ToArray ();

				List<string> list = new List<string> ();
				StringBuilder current = new StringBuilder ();
				for (int i = 0; i < path.Length; i++) {
					switch (path [i]) {
					case '/':
					case '\\':
						current.Append (path [i]);
						list.Add (current.ToString ());
						current.Length = 0;
						break;
					case '%':
						if ((i < path.Length - 2) && (path [i + 1] == '5' && path [i + 2] == 'C')) {
							current.Append ("%5C");
							list.Add (current.ToString ());
							current.Length = 0;
							i += 2;
						} else {
							current.Append ('%');
						}
						break;
					default:
						current.Append (path [i]);
						break;
					}
				}

				if (current.Length > 0)
					list.Add (current.ToString ());

				if (IsFile && (list.Count > 0)) {
					string first = list [0];
					if ((first.Length > 1) && (first [1] == ':')) {
						list.Insert (0, "/");
					}
				}
				segments = list;
				return segments.ToArray ();
			} 
		}

		public bool UserEscaped { 
			get { return userEscaped; } 
		}

		public string UserInfo { 
			get { 
				EnsureAbsoluteUri ();
				return userinfo == null ? String.Empty : userinfo;
			}
		}
		
		public string DnsSafeHost {
			get {
				EnsureAbsoluteUri ();
				string host = Host;
				if (HostNameType == UriHostNameType.IPv6) {
					host = Host.Substring (1, Host.Length - 2);
					if (scope_id != 0)
						host += "%" + scope_id.ToString ();
				}
				return Unescape (host);
			}
		}

		public bool IsAbsoluteUri {
			get { return isAbsoluteUri; }
		}

		public string OriginalString {
			get { return source; }
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
				
			IPv6Address addr;
			if (IPv6Address.TryParse (name, out addr))
				return UriHostNameType.IPv6;
			
			return UriHostNameType.Unknown;
		}
		
		internal static bool IsIPv4Address (string name)
		{
			string [] captures = name.Split (new char [] {'.'});
			if (captures.Length != 4)
				return false;

			for (int i = 0; i < 4; i++) {
				int length;

				length = captures [i].Length;
				if (length == 0)
					return false;
				uint number;
				if (!UInt32.TryParse (captures [i], out number))
					return false;
				if (number > 255)
					return false;
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
					// www..host.com is bad
					if (i + 1 < len && name [i + 1] == '.')
						return false;
					count = 0;
					continue;
				} else if (!Char.IsLetterOrDigit (c) && c != '-' && c != '_') {
					return false;
				}
				if (++count == 64)
					return false;
			}
			
			return true;
		}
#if !NET_2_1

		[Obsolete ("This method does nothing, it has been obsoleted")]
		protected virtual void Canonicalize ()
		{
			//
			// This is flagged in the Microsoft documentation as used
			// internally, no longer in use, and Obsolete.
			//
		}

		[MonoTODO ("Find out what this should do")]
		[Obsolete]
		protected virtual void CheckSecurity ()
		{
		}

#endif // NET_2_1

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
			// as defined in rfc2234
			// %x41-5A / %x61-7A (A-Z / a-z)
			int i = (int) c;
			return (((i >= 0x41) && (i <= 0x5A)) || ((i >= 0x61) && (i <= 0x7A)));
		}

		public override bool Equals (object comparand) 
		{
			if (comparand == null) 
				return false;

			Uri uri = comparand as Uri;
			if ((object) uri == null) {
				string s = comparand as String;
				if (s == null)
					return false;

				if (!TryCreate (s, UriKind.RelativeOrAbsolute, out uri))
					return false;
			}

			return InternalEquals (uri);
		}

		// Assumes: uri != null
		bool InternalEquals (Uri uri)
		{
			if (this.isAbsoluteUri != uri.isAbsoluteUri)
				return false;
			if (!this.isAbsoluteUri)
				return this.source == uri.source;

			CultureInfo inv = CultureInfo.InvariantCulture;
			return this.scheme.ToLower (inv) == uri.scheme.ToLower (inv)
				&& this.host.ToLower (inv) == uri.host.ToLower (inv)
				&& this.port == uri.port
				&& this.query == uri.query
				&& this.path == uri.path;
		}

		public static bool operator == (Uri uri1, Uri uri2)
		{
			return object.Equals (uri1, uri2);
		}

		public static bool operator != (Uri uri1, Uri uri2)
		{
			return !(uri1 == uri2);
		}

		public override int GetHashCode () 
		{
			if (cachedHashCode == 0) {
				CultureInfo inv = CultureInfo.InvariantCulture;
				if (isAbsoluteUri) {
					cachedHashCode = scheme.ToLower (inv).GetHashCode ()
						^ host.ToLower (inv).GetHashCode ()
						^ port
						^ query.GetHashCode ()
						^ path.GetHashCode ();
				}
				else {
					cachedHashCode = source.GetHashCode ();
				}
			}
			return cachedHashCode;
		}
		
		public string GetLeftPart (UriPartial part) 
		{
			EnsureAbsoluteUri ();
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
				if (userinfo != null) 
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
				if (userinfo != null) 
					sb.Append (userinfo).Append ('@');
				sb.Append (host);
				defaultPort = GetDefaultPort (scheme);
				if ((port != -1) && (port != defaultPort))
					sb.Append (':').Append (port);

				if (path.Length > 0) {
					if (scheme == "mailto" || scheme == "news")
						sb.Append (path);
					else 
						sb.Append (Reduce (path, CompactEscaped (scheme)));
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

		public static bool IsHexDigit (char character) 
		{
			return (('0' <= character && character <= '9') ||
			        ('a' <= character && character <= 'f') ||
			        ('A' <= character && character <= 'F'));
		}

		public static bool IsHexEncoding (string pattern, int index) 
		{
			if ((index + 3) > pattern.Length)
				return false;

			return ((pattern [index++] == '%') &&
			        IsHexDigit (pattern [index++]) &&
			        IsHexDigit (pattern [index]));
		}

		//
		// Implemented by copying most of the MakeRelative code
		//
		public Uri MakeRelativeUri (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");
			if (Host != uri.Host || Scheme != uri.Scheme)
				return uri;

			string result = String.Empty;
			if (this.path != uri.path){
				string [] segments = this.Segments;
				string [] segments2 = uri.Segments;
				
				int k = 0;
				int max = Math.Min (segments.Length, segments2.Length);
				for (; k < max; k++)
					if (segments [k] != segments2 [k]) 
						break;
				
				for (int i = k; i < segments.Length && segments [i].EndsWith ("/", StringComparison.Ordinal); i++)
					result += "../";
				for (int i = k; i < segments2.Length; i++)
					result += segments2 [i];
				
				if (result == string.Empty)
					result = "./";
			}
			uri.AppendQueryAndFragment (ref result);

			return new Uri (result, UriKind.Relative);
		}

		[Obsolete ("Use MakeRelativeUri(Uri uri) instead.")]
		public string MakeRelative (Uri toUri) 
		{
			if ((this.Scheme != toUri.Scheme) ||
			    (this.Authority != toUri.Authority))
				return toUri.ToString ();

			string result = String.Empty;
			if (this.path != toUri.path){
				string [] segments = this.Segments;
				string [] segments2 = toUri.Segments;
				int k = 0;
				int max = Math.Min (segments.Length, segments2.Length);
				for (; k < max; k++)
					if (segments [k] != segments2 [k]) 
						break;
				
				for (int i = k + 1; i < segments.Length; i++)
					result += "../";
				for (int i = k; i < segments2.Length; i++)
					result += segments2 [i];
			}

			// Important: MakeRelative does not append fragment or query.

			return result;
		}

		void AppendQueryAndFragment (ref string result)
		{
			if (query.Length > 0) {
				string q = query [0] == '?' ? '?' + Unescape (query.Substring (1), true, false) : Unescape (query, false);
				result += q;
			}
			if (fragment.Length > 0)
				result += Unescape (fragment, true, false);
		}
		
		public override string ToString () 
		{
			if (cachedToString != null) 
				return cachedToString;

			if (isAbsoluteUri) {
				if (Parser is DefaultUriParser)
					cachedToString = Parser.GetComponentsHelper (this, UriComponents.AbsoluteUri, UriHelper.ToStringUnescape);
				else
					cachedToString = Parser.GetComponents (this, UriComponents.AbsoluteUri, UriFormat.SafeUnescaped);
			} else
				cachedToString = UriHelper.FormatRelative (source, scheme, UriHelper.ToStringUnescape);

			return cachedToString;
		}

		protected void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			if (this.isAbsoluteUri) {
				serializationInfo.AddValue ("AbsoluteUri", this.AbsoluteUri);
			} else {
				serializationInfo.AddValue ("AbsoluteUri", String.Empty);
				serializationInfo.AddValue ("RelativeUri", this.OriginalString);
			}
		}

		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			GetObjectData (info, context);
		}


		// Internal Methods		

		[Obsolete]
		protected virtual void Escape ()
		{
			path = EscapeString (path);
		}

		[Obsolete]
		protected static string EscapeString (string str) 
		{
			return EscapeString (str, Uri.EscapeCommonHexBrackets);
		}

		private const string EscapeCommon = "<>%\"{}|\\^`";
		private const string EscapeReserved = ";/?:@&=+$,";
		private const string EscapeFragment = "#";
		private const string EscapeBrackets = "[]";

		private const string EscapeNews = EscapeCommon + EscapeBrackets + "?";
		private const string EscapeCommonHex = EscapeCommon + EscapeFragment;
		private const string EscapeCommonBrackets = EscapeCommon + EscapeBrackets;
		internal const string EscapeCommonHexBrackets = EscapeCommon + EscapeFragment + EscapeBrackets;
		internal const string EscapeCommonHexBracketsQuery = EscapeCommonHexBrackets + "?";

		internal static string EscapeString (string str, string escape)
		{
			return EscapeString (str, escape, true);
		}

		internal static string EscapeString (string str, string escape, bool nonAsciiEscape) 
		{
			if (String.IsNullOrEmpty (str))
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
					s.Append (str.Substring (i, 3));
					i += 2;
					continue;
				}

				char c = str [i];
				bool outside_limited_ascii = ((c <= 0x20) || (c >= 0x7f));
				bool needs_escape = (escape.IndexOf (c) != -1);
				if (nonAsciiEscape && outside_limited_ascii) {
					byte [] data = Encoding.UTF8.GetBytes (new char [] { c });
					int length = data.Length;
					for (int j = 0; j < length; j++) {
						c = (char) data [j];
						if (needs_escape || nonAsciiEscape)
							s.Append (HexEscape (c));
						else
							s.Append (c);
					}
				} else if (needs_escape) {
					s.Append (HexEscape (c));
				} else {
					s.Append (c);
				}
			}
			
			return s.ToString ();
		}

		// On .NET 1.x, this method is called from .ctor(). When overriden, we 
		// can avoid the "absolute uri" constraints of the .ctor() by
		// overriding with custom code.
		[Obsolete("The method has been deprecated. It is not used by the system.")]
		protected virtual void Parse ()
		{
		}

		private void ParseUri (UriKind kind)
		{
			Parse (kind, source);

			if (userEscaped)
				return;

			if (host.Length > 1 && host [0] != '[' && host [host.Length - 1] != ']') {
				// host name present (but not an IPv6 address)
				host = host.ToLower (CultureInfo.InvariantCulture);
			}
		}

		[Obsolete]
		protected virtual string Unescape (string path)
		{
			var formatFlags = UriHelper.FormatFlags.NoSlashReplace | UriHelper.FormatFlags.NoReduce;
			return UriHelper.FormatAbsolute (path, scheme, UriComponents.Path, UriFormat.Unescaped, formatFlags);
		}

		internal static string Unescape (string str, bool excludeSpecial)
		{
			return Unescape (str, excludeSpecial, excludeSpecial);
		}
		
		internal static string Unescape (string str, bool excludeSpecial, bool excludeBackslash) 
		{
			if (String.IsNullOrEmpty (str))
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
					else if (excludeBackslash && x == '\\')
						s.Append ("%5C");
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

		//
		// Returns null on success, string with error on failure
		//
		private string ParseAsWindowsAbsoluteFilePath (string uriString)
		{
			if (uriString.Length > 2 && uriString [2] != '\\' && uriString [2] != '/')
				return "Relative file path is not allowed.";
			scheme = UriSchemeFile;
			host = String.Empty;
			port = -1;
			path = uriString.Replace ("\\", "/");
			fragment = String.Empty;
			query = String.Empty;

			return null;
		}

		private void ParseAsUnixAbsoluteFilePath (string uriString)
		{
			scheme = UriSchemeFile;
			port = -1;
			fragment = String.Empty;
			query = String.Empty;
			host = String.Empty;
			path = null;

			if (uriString.Length >= 2 && uriString [0] == '/' && uriString [1] == '/') {
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

		//
		// This parse method will throw exceptions on failure
		//  
		private void Parse (UriKind kind, string uriString)
		{			
			if (uriString == null)
				throw new ArgumentNullException ("uriString");

			string s = ParseNoExceptions (kind, uriString);
			if (s != null)
				throw new UriFormatException (s);
		}

		private bool SupportsQuery ()
		{
			return UriHelper.SupportsQuery (scheme);
		}


		private string ParseNoExceptions (UriKind kind, string uriString)
		{
			UriElements elements;
			string error;
			if (!UriParseComponents.TryParseComponents (source, kind, out elements, out error))
				return error;

			scheme = elements.scheme;
			var parser = UriParser.GetParser (scheme);
			if (parser != null && !(parser is DefaultUriParser)) {
				userinfo = Parser.GetComponents (this, UriComponents.UserInfo, UriFormat.UriEscaped);
				host = Parser.GetComponents (this, UriComponents.Host, UriFormat.UriEscaped);

				var portStr = Parser.GetComponents (this, UriComponents.StrongPort, UriFormat.UriEscaped);
				if (!string.IsNullOrEmpty (portStr))
					port = int.Parse (portStr);

				path = Parser.GetComponents (this, UriComponents.Path | UriComponents.KeepDelimiter, UriFormat.UriEscaped);
				query = Parser.GetComponents (this, UriComponents.Query, UriFormat.UriEscaped);
				fragment = Parser.GetComponents (this, UriComponents.StrongPort, UriFormat.UriEscaped);

				return null;
			}

			var formatFlags = UriHelper.FormatFlags.None;
			if (UriHelper.HasCharactersToNormalize (uriString))
				formatFlags |= UriHelper.FormatFlags.HasUriCharactersToNormalize;

			if (userEscaped)
				formatFlags |= UriHelper.FormatFlags.UserEscaped;

			if (elements.host != null)
				formatFlags |= UriHelper.FormatFlags.HasHost;

			userinfo = elements.user;

			if (elements.host != null) {
				host = UriHelper.FormatAbsolute (elements.host, scheme,
					UriComponents.Host, UriFormat.UriEscaped, formatFlags);
			}

			port = elements.port;

			if (port == -1)
				port = GetDefaultPort (scheme);

			if (elements.path != null) {
				path = UriHelper.FormatAbsolute (elements.path, scheme,
					UriComponents.Path, UriFormat.UriEscaped, formatFlags);
				if (elements.delimiter == SchemeDelimiter && string.IsNullOrEmpty (path))
					path = "/";
			}

			if (elements.query != null) {
				query = "?" + UriHelper.FormatAbsolute (elements.query, scheme,
					UriComponents.Query, UriFormat.UriEscaped, formatFlags);
			}

			if (elements.fragment != null) {
				fragment = "#" + UriHelper.FormatAbsolute (elements.fragment, scheme,
					UriComponents.Fragment, UriFormat.UriEscaped, formatFlags);
			}

			isAbsoluteUri = elements.isAbsoluteUri;
			isUnc = elements.isUnc;
			scope_id = elements.scopeId;

			return null;
		}
		
		private static string TryGetKnownUriSchemeInstance (string scheme)
		{
			foreach (string knownScheme in knownUriSchemes) {
				if (knownScheme == scheme)
					return knownScheme;
			}
			
			return scheme;
		}
	
		private static bool CompactEscaped (string scheme)
		{
			if (scheme == null || scheme.Length < 4)
				return false;

			char first = scheme [0];
			if (first == 'h'){
				return scheme == "http" || scheme == "https";
			} else if (first == 'f' && scheme == "file"){
				return true;
			} else if (first == 'n')
				return scheme == "net.pipe" || scheme == "net.tcp";

			return false;
		}

		// replace '\', %5C ('\') and %2f ('/') into '/'
		// replace %2e ('.') into '.'
		private static string NormalizePath (string path)
		{
			StringBuilder res = new StringBuilder ();
			for (int i = 0; i < path.Length; i++) {
				char c = path [i];
				switch (c) {
				case '\\':
					c = '/';
					break;
				case '%':
					if (i < path.Length - 2) {
						char c1 = path [i + 1];
						char c2 = Char.ToUpper (path [i + 2]);
						if ((c1 == '2') && (c2 == 'E')) {
							c = '.';
							i += 2;
						} else if (((c1 == '2') && (c2 == 'F')) || ((c1 == '5') && (c2 == 'C'))) {
							c = '/';
							i += 2;
						}
					}
					break;
				}
				res.Append (c);
			}
			return res.ToString ();
		}

		// This is called "compacting" in the MSDN documentation
		private static string Reduce (string path, bool compact_escaped)
		{
			// quick out, allocation-free, for a common case
			if (path == "/")
				return path;

			if (compact_escaped && (path.IndexOf ('%') != -1)) {
				// replace '\', %2f, %5c with '/' and replace %2e with '.'
				// other escaped values seems to survive this step
				path = NormalizePath (path);
			} else {
				// (always) replace '\' with '/'
				path = path.Replace ('\\', '/');
			}

			List<string> result = new List<string> ();

			bool begin = true;
			for (int startpos = 0; startpos < path.Length; ) {
				int endpos = path.IndexOf ('/', startpos);
				if (endpos == -1)
					endpos = path.Length;
				string current = path.Substring (startpos, endpos-startpos);
				startpos = endpos + 1;
				if ((begin && current.Length == 0) || current == "." ) {
					begin = false;
					continue;
				}

				begin = false;
				if (current == "..") {
					int resultCount = result.Count;
					// in 2.0 profile, skip leading ".." parts
					if (resultCount == 0) {
						continue;
					}

					result.RemoveAt (resultCount - 1);
					continue;
				}

				result.Add (current);
			}

			if (result.Count == 0)
				return "/";

			StringBuilder res = new StringBuilder ();

			if (path [0] == '/')
				res.Append ('/');

			bool first = true;
			foreach (string part in result) {
				if (first) {
					first = false;
				} else {
					res.Append ('/');
				}
				res.Append (part);
			}

			if (path [path.Length - 1] == '/')
				res.Append ('/');
				
			return res.ToString ();
		}

		// A variant of HexUnescape() which can decode multi-byte escaped
		// sequences such as (e.g.) %E3%81%8B into a single character
		internal static char HexUnescapeMultiByte (string pattern, ref int index, out char surrogate) 
		{
			bool invalidEscape;
			return HexUnescapeMultiByte (pattern, ref index, out surrogate, out invalidEscape);
		}

		internal static char HexUnescapeMultiByte (string pattern, ref int index, out char surrogate, out bool invalidEscape)
		{
			surrogate = char.MinValue;
			invalidEscape = false;

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
			if (num_bytes <= 1) {
				var c = (char) ((msb << 4) | lsb);
				invalidEscape = c > 0x7F;
				return c;
			}

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
				invalidEscape = true;
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
			new UriScheme (UriSchemeUuid, ":", -1),
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
			UriParser parser = UriParser.GetParser (scheme);
			if (parser == null)
				return -1;
			return parser.DefaultPort;
		}

		private string GetOpaqueWiseSchemeDelimiter ()
		{
			return GetSchemeDelimiter (scheme);
		}

		[Obsolete]
		protected virtual bool IsBadFileSystemCharacter (char character)
		{
			// It does not always overlap with InvalidPathChars.
			int chInt = (int) character;
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

		[Obsolete]
		protected static bool IsExcludedCharacter (char character)
		{
			if (character <= 32 || character >= 127)
				return true;
			
			if (character == '"' || character == '#' || character == '%' || character == '<' ||
			    character == '>' || character == '[' || character == '\\' || character == ']' ||
			    character == '^' || character == '`' || character == '{' || character == '|' ||
			    character == '}')
				return true;
			return false;
		}

		internal static bool MaybeUri (string s)
		{
			int p = s.IndexOf (':');
			if (p == -1)
				return false;

			if (p >= 10)
				return false;

			return IsPredefinedScheme (s.Substring (0, p));
		}
		
		//
		// Using a simple block of if's is twice as slow as the compiler generated
		// switch statement.   But using this tuned code is faster than the
		// compiler generated code, with a million loops on x86-64:
		//
		// With "http": .10 vs .51 (first check)
		// with "https": .16 vs .51 (second check)
		// with "foo": .22 vs .31 (never found)
		// with "mailto": .12 vs .51  (last check)
		//
		//
		private static bool IsPredefinedScheme (string scheme)
		{
			if (scheme == null || scheme.Length < 3)
				return false;
			
			char c = scheme [0];
			if (c == 'h')
				return (scheme == "http" || scheme == "https");
			if (c == 'f')
				return (scheme == "file" || scheme == "ftp");
				
			if (c == 'n'){
				c = scheme [1];
				if (c == 'e')
					return (scheme == "news" || scheme == "net.pipe" || scheme == "net.tcp");
				if (scheme == "nntp")
					return true;
				return false;
			}
			if ((c == 'g' && scheme == "gopher") || (c == 'm' && scheme == "mailto"))
				return true;

			return false;
		}

		[Obsolete]
		protected virtual bool IsReservedCharacter (char character)
		{
			if (character == '$' || character == '&' || character == '+' || character == ',' ||
			    character == '/' || character == ':' || character == ';' || character == '=' ||
			    character == '@')
				return true;
			return false;
		}

		[NonSerialized]
		private UriParser parser;

		private UriParser Parser {
			get {
				if (parser == null) {
					parser = UriParser.GetParser (scheme);
					// no specific parser ? then use a default one
					if (parser == null)
						parser = new DefaultUriParser ("*");
				}
				return parser;
			}
			set { parser = value; }
		}

		public string GetComponents (UriComponents components, UriFormat format)
		{
			if ((components & UriComponents.SerializationInfoString) == 0)
				EnsureAbsoluteUri ();

			return Parser.GetComponents (this, components, format);
		}

		public bool IsBaseOf (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");
			return Parser.IsBaseOf (this, uri);
		}

		public bool IsWellFormedOriginalString ()
		{
			// funny, but it does not use the Parser's IsWellFormedOriginalString().
			// Also, it seems we need to *not* escape hex.
			return EscapeString (OriginalString, EscapeCommonBrackets) == OriginalString;
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
			if (uri1 == null)
				return -1;
			if (uri2 == null)
				return 1;

			string s1 = uri1.GetComponents (partsToCompare, compareFormat);
			string s2 = uri2.GetComponents (partsToCompare, compareFormat);
			return String.Compare (s1, s2, comparisonType);
		}

		//
		// The rules for EscapeDataString
		//
		static bool NeedToEscapeDataChar (char b)
		{
			if ((b >= 'A' && b <= 'Z') ||
				(b >= 'a' && b <= 'z') ||
				(b >= '0' && b <= '9'))
				return false;

			switch (b) {
			case '-':
			case '.':
			case '_':
			case '~':
				return false;
			}


			return true;
		}
		
		public static string EscapeDataString (string stringToEscape)
		{
			if (stringToEscape == null)
				throw new ArgumentNullException ("stringToEscape");

			if (stringToEscape.Length > MaxUriLength) {
				throw new UriFormatException (string.Format ("Uri is longer than the maximum {0} characters.", MaxUriLength));
			}

			bool escape = false;
			foreach (char c in stringToEscape){
				if (NeedToEscapeDataChar (c)){
					escape = true;
					break;
				}
			}
			if (!escape){
				return stringToEscape;
			}
			
			StringBuilder sb = new StringBuilder ();
			byte [] bytes = Encoding.UTF8.GetBytes (stringToEscape);
			foreach (byte b in bytes){
				if (NeedToEscapeDataChar ((char) b))
					sb.Append (HexEscape ((char) b));
				else
					sb.Append ((char) b);
			}
			return sb.ToString ();
		}

		//
		// The rules for EscapeUriString
		//
		static bool NeedToEscapeUriChar (char b)
		{
			if ((b >= 'A' && b <= 'Z') || (b >= 'a' && b <= 'z') || (b >= '&' && b <= ';'))
				return false;

			switch (b) {
			case '!':
			case '#':
			case '$':
			case '=':
			case '?':
			case '@':
			case '_':
			case '~':
				return false;
			case '[':
			case ']':
				return false;
			}

			return true;
		}
		
		public static string EscapeUriString (string stringToEscape)
		{
			if (stringToEscape == null)
				throw new ArgumentNullException ("stringToEscape");

			if (stringToEscape.Length > MaxUriLength) {
				throw new UriFormatException (string.Format ("Uri is longer than the maximum {0} characters.", MaxUriLength));
			}

			bool escape = false;
			foreach (char c in stringToEscape){
				if (NeedToEscapeUriChar (c)){
					escape = true;
					break;
				}
			}
			if (!escape)
				return stringToEscape;

			StringBuilder sb = new StringBuilder ();
			byte [] bytes = Encoding.UTF8.GetBytes (stringToEscape);
			foreach (byte b in bytes){
				if (NeedToEscapeUriChar ((char) b))
					sb.Append (HexEscape ((char) b));
				else
					sb.Append ((char) b);
			}
			return sb.ToString ();
		}

		public static bool IsWellFormedUriString (string uriString, UriKind uriKind)
		{
			if (uriString == null)
				return false;

			Uri uri;
			if (Uri.TryCreate (uriString, uriKind, out uri))
				return uri.IsWellFormedOriginalString ();
			return false;
		}

		public static bool TryCreate (string uriString, UriKind uriKind, out Uri result)
		{
			bool success;

			Uri r = new Uri (uriString, uriKind, out success);
			if (success) {
				result = r;
				return true;
			}
			result = null;
			return false;
		}

		// [MonoTODO ("rework code to avoid exception catching")]
		public static bool TryCreate (Uri baseUri, string relativeUri, out Uri result)
		{
			result = null;
			if (relativeUri == null)
				return false;

			try {
				Uri relative = new Uri (relativeUri, UriKind.RelativeOrAbsolute);
				if ((baseUri != null) && baseUri.IsAbsoluteUri) {
					// FIXME: this should call UriParser.Resolve
					result = new Uri (baseUri, relative);
				} else if (relative.IsAbsoluteUri) {
					// special case - see unit tests
					result = relative;
				}
				return (result != null);
			} catch (UriFormatException) {
				return false;
			}
		}

		//[MonoTODO ("rework code to avoid exception catching")]
		public static bool TryCreate (Uri baseUri, Uri relativeUri, out Uri result)
		{
			result = null;
			if ((baseUri == null) || !baseUri.IsAbsoluteUri)
				return false;
			if (relativeUri == null)
				return false;
			try {
				// FIXME: this should call UriParser.Resolve
				result = new Uri (baseUri, relativeUri.OriginalString);
				return true;
			} catch (UriFormatException) {
				return false;
			}
		}

		public static string UnescapeDataString (string stringToUnescape)
		{
			return UnescapeDataString (stringToUnescape, false);
		}

		internal static string UnescapeDataString (string stringToUnescape, bool safe)
		{
			if (stringToUnescape == null)
				throw new ArgumentNullException ("stringToUnescape");

			if (stringToUnescape.IndexOf ('%') == -1 && stringToUnescape.IndexOf ('+') == -1)
				return stringToUnescape;

			StringBuilder output = new StringBuilder ();
			long len = stringToUnescape.Length;
			MemoryStream bytes = new MemoryStream ();
			int xchar;

			for (int i = 0; i < len; i++) {
				if (stringToUnescape [i] == '%' && i + 2 < len && stringToUnescape [i + 1] != '%') {
					if (stringToUnescape [i + 1] == 'u' && i + 5 < len) {
						if (bytes.Length > 0) {
							output.Append (GetChars (bytes, Encoding.UTF8));
							bytes.SetLength (0);
						}

						xchar = GetChar (stringToUnescape, i + 2, 4, safe);
						if (xchar != -1) {
							output.Append ((char) xchar);
							i += 5;
						}
						else {
							output.Append ('%');
						}
					}
					else if ((xchar = GetChar (stringToUnescape, i + 1, 2, safe)) != -1) {
						bytes.WriteByte ((byte) xchar);
						i += 2;
					}
					else {
						output.Append ('%');
					}
					continue;
				}

				if (bytes.Length > 0) {
					output.Append (GetChars (bytes, Encoding.UTF8));
					bytes.SetLength (0);
				}

				output.Append (stringToUnescape [i]);
			}

			if (bytes.Length > 0) {
				output.Append (GetChars (bytes, Encoding.UTF8));
			}

			bytes = null;
			return output.ToString ();
		}

		private static int GetInt (byte b)
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

		private static int GetChar (string str, int offset, int length, bool safe)
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

			if (!safe)
				return val;

			switch ((char) val) {
			case '%':
			case '#':
			case '?':
			case '/':
			case '\\':
			case '@':
			case '&': // not documented
				return -1;
			default:
				return val;
			}
		}

		private static char [] GetChars (MemoryStream b, Encoding e)
		{
			return e.GetChars (b.GetBuffer (), 0, (int) b.Length);
		}
		

		private void EnsureAbsoluteUri ()
		{
			if (!IsAbsoluteUri)
				throw new InvalidOperationException ("This operation is not supported for a relative URI.");
		}
	}
}
