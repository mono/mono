//
// System.UriParser abstract class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Security.Permissions;
using System.Text;

namespace System {
	public abstract class UriParser {

		static object lock_object = new object ();
		static Hashtable table;

		internal string scheme_name;
		private int default_port;

		protected UriParser ()
		{
		}

		// protected methods
		protected internal virtual string GetComponents (Uri uri, UriComponents components, UriFormat format)
		{
			if ((format < UriFormat.UriEscaped) || (format > UriFormat.SafeUnescaped))
				throw new ArgumentOutOfRangeException ("format");

			UriElements elements = UriParseComponents.ParseComponents (uri.OriginalString.Trim ());

			string scheme = scheme_name;
			int dp = default_port;

			if ((scheme == null) || (scheme == "*")) {
				scheme = elements.scheme;
				dp = Uri.GetDefaultPort (scheme);
			} else if (String.Compare (scheme, elements.scheme, true) != 0) {
				throw new SystemException ("URI Parser: scheme mismatch: " + scheme + " vs. " + elements.scheme);
			}

			// it's easier to answer some case directly (as the output isn't identical 
			// when mixed with others components, e.g. leading slash, # ...)
			switch (components) {
			case UriComponents.Scheme:
				return scheme;
			case UriComponents.UserInfo:
				return elements.user;
			case UriComponents.Host:
				return elements.host;
			case UriComponents.Port: {
				string p = elements.port;
				if (p != null && p.Length != 0 && p != dp.ToString ())
					return p;
				return String.Empty;
			}
			case UriComponents.Path:
				return Format (IgnoreFirstCharIf (elements.path, '/'), format);
			case UriComponents.Query:
				return Format (elements.query, format);
			case UriComponents.Fragment:
				return Format (elements.fragment, format);
			case UriComponents.StrongPort: {
				return elements.port.Length != 0 ? elements.port : dp.ToString ();
			}
			case UriComponents.SerializationInfoString:
				components = UriComponents.AbsoluteUri;
				break;
			}

			// now we deal with multiple flags...

			StringBuilder sb = new StringBuilder ();

			if ((components & UriComponents.Scheme) != 0) {
				sb.Append (scheme);
				sb.Append (Uri.GetSchemeDelimiter (scheme));
			}

			if ((components & UriComponents.UserInfo) != 0) {
				string userinfo = elements.user;
				if (!String.IsNullOrEmpty (userinfo)) {
					sb.Append (elements.user);
					sb.Append ('@');
				}
			}

			if ((components & UriComponents.Host) != 0)
				sb.Append (elements.host);

			// for StrongPort always show port - even if -1
			// otherwise only display if ut's not the default port
			if ((components & UriComponents.StrongPort) != 0) {
				sb.Append (":");
				if (elements.port.Length != 0) {
					sb.Append (elements.port);
				} else {
					sb.Append (dp);
				}
			}

			if ((components & UriComponents.Port) != 0) {
				string p = elements.port;
				if (p != null && p.Length != 0 && p != dp.ToString ()) {
					sb.Append (":");
					sb.Append (elements.port);
				}
			}

			if ((components & UriComponents.Path) != 0) {
				if ((components & UriComponents.PathAndQuery) != 0 &&
					(elements.path.Length == 0 || !elements.path.StartsWith ("/")))
					sb.Append ("/");
				sb.Append (elements.path);
			}

			if ((components & UriComponents.Query) != 0) {
				string query = elements.query;
				if (!String.IsNullOrEmpty (query)) {
					sb.Append ("?");
					sb.Append (elements.query);
				}
			}

			string result = Format (sb.ToString (), format);
			if ((components & UriComponents.Fragment) != 0) {
				string f = elements.fragment;
				if (!String.IsNullOrEmpty (f)) {
					result += "#" + Format (f, format);
				}
			}
			return result;
		}

		protected internal virtual void InitializeAndValidate (Uri uri, out UriFormatException parsingError)
		{
			// bad boy, it should check null arguments.
			if ((uri.Scheme != scheme_name) && (scheme_name != "*"))
				// Here .NET seems to return "The Authority/Host could not be parsed", but it does not make sense.
				parsingError = new UriFormatException ("The argument Uri's scheme does not match");
			else
				parsingError = null;
		}

		protected internal virtual bool IsBaseOf (Uri baseUri, Uri relativeUri)
		{
			// compare, not case sensitive, the scheme, host and port (+ user informations)
			if (Uri.Compare (baseUri, relativeUri, UriComponents.SchemeAndServer | UriComponents.UserInfo, UriFormat.Unescaped, StringComparison.InvariantCultureIgnoreCase) != 0)
				return false;

			string base_string = baseUri.LocalPath;
			int last_slash = base_string.LastIndexOf ('/') + 1; // keep the slash
			return (String.Compare (base_string, 0, relativeUri.LocalPath, 0, last_slash, StringComparison.InvariantCultureIgnoreCase) == 0);
		}

		protected internal virtual bool IsWellFormedOriginalString (Uri uri)
		{
			// well formed according to RFC2396 and RFC2732
			// see Uri.IsWellFormedOriginalString for some docs

			// Though this class does not seem to do anything. Even null arguments aren't checked :/
			return uri.IsWellFormedOriginalString ();
		}
		protected internal virtual UriParser OnNewUri ()
		{
			// nice time for init
			return this;
		}

		[MonoTODO]
		protected virtual void OnRegister (string schemeName, int defaultPort)
		{
			// unit tests shows that schemeName and defaultPort aren't usable from here
		}

		[MonoTODO]
		protected internal virtual string Resolve (Uri baseUri, Uri relativeUri, out UriFormatException parsingError)
		{
			// used by Uri.ctor and Uri.TryCreate
			throw new NotImplementedException ();
		}

		// internal properties

		internal string SchemeName {
			get { return scheme_name; }
			set { scheme_name = value; }
		}

		internal int DefaultPort {
			get { return default_port; }
			set { default_port = value; }
		}

		// private stuff

		private string IgnoreFirstCharIf (string s, char c)
		{
			if (s.Length == 0)
				return String.Empty;
			if (s[0] == c)
				return s.Substring (1);
			return s;
		}

		private string Format (string s, UriFormat format)
		{
			if (s.Length == 0)
				return String.Empty;

			switch (format) {
			case UriFormat.UriEscaped:
				return Uri.EscapeString (s, Uri.EscapeCommonHexBrackets);
			case UriFormat.SafeUnescaped:
				return Uri.UnescapeDataString (s, true);
			case UriFormat.Unescaped:
				return Uri.Unescape (s, false);
			default:
				throw new ArgumentOutOfRangeException ("format");
			}
		}

		// static methods

		private static void CreateDefaults ()
		{
			if (table != null)
				return;

			Hashtable newtable = new Hashtable ();
			InternalRegister (newtable, new DefaultUriParser (), Uri.UriSchemeFile, -1);
			InternalRegister (newtable, new DefaultUriParser (), Uri.UriSchemeFtp, 21);
			InternalRegister (newtable, new DefaultUriParser (), Uri.UriSchemeGopher, 70);
			InternalRegister (newtable, new DefaultUriParser (), Uri.UriSchemeHttp, 80);
			InternalRegister (newtable, new DefaultUriParser (), Uri.UriSchemeHttps, 443);
			InternalRegister (newtable, new DefaultUriParser (), Uri.UriSchemeMailto, 25);
			InternalRegister (newtable, new DefaultUriParser (), Uri.UriSchemeNetPipe, -1);
			InternalRegister (newtable, new DefaultUriParser (), Uri.UriSchemeNetTcp, -1);
			InternalRegister (newtable, new DefaultUriParser (), Uri.UriSchemeNews, -1);
			InternalRegister (newtable, new DefaultUriParser (), Uri.UriSchemeNntp, 119);
			// not defined in Uri.UriScheme* but a parser class exists
			InternalRegister (newtable, new DefaultUriParser (), "ldap", 389);
			
			lock (lock_object) {
				if (table == null)
					table = newtable;
				else
					newtable = null;
			}
		}

		public static bool IsKnownScheme (string schemeName)
		{
			if (schemeName == null)
				throw new ArgumentNullException ("schemeName");
			if (schemeName.Length == 0)
				throw new ArgumentOutOfRangeException ("schemeName");

			CreateDefaults ();
			string lc = schemeName.ToLower (CultureInfo.InvariantCulture);
			return (table [lc] != null);
		}

		// *no* check version
		private static void InternalRegister (Hashtable table, UriParser uriParser, string schemeName, int defaultPort)
		{
			uriParser.SchemeName = schemeName;
			uriParser.DefaultPort = defaultPort;

			// FIXME: MS doesn't seems to call most inherited parsers
			if (uriParser is GenericUriParser) {
				table.Add (schemeName, uriParser);
			} else {
				DefaultUriParser parser = new DefaultUriParser ();
				parser.SchemeName = schemeName;
				parser.DefaultPort = defaultPort;
				table.Add (schemeName, parser);
			}

			// note: we cannot set schemeName and defaultPort inside OnRegister
			uriParser.OnRegister (schemeName, defaultPort);
		}

		[SecurityPermission (SecurityAction.Demand, Infrastructure = true)]
		public static void Register (UriParser uriParser, string schemeName, int defaultPort)
		{
			if (uriParser == null)
				throw new ArgumentNullException ("uriParser");
			if (schemeName == null)
				throw new ArgumentNullException ("schemeName");
			if ((defaultPort < -1) || (defaultPort >= UInt16.MaxValue))
				throw new ArgumentOutOfRangeException ("defaultPort");

			CreateDefaults ();

			string lc = schemeName.ToLower (CultureInfo.InvariantCulture);
			if (table [lc] != null) {
				string msg = Locale.GetText ("Scheme '{0}' is already registred.");
				throw new InvalidOperationException (msg);
			}
			InternalRegister (table, uriParser, lc, defaultPort);
		}

		internal static UriParser GetParser (string schemeName)
		{
			if (schemeName == null)
				return null;

			CreateDefaults ();

			string lc = schemeName.ToLower (CultureInfo.InvariantCulture);
			return (UriParser) table [lc];
		}
	}
}

