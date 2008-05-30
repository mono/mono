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

#if NET_2_0

using System.Collections;
using System.Globalization;
using System.Security.Permissions;
using System.Text;

namespace System {

	public abstract class UriParser {

		static object lock_object = new object ();
		static Hashtable table;

		private string scheme_name;
		private int default_port;


		protected UriParser ()
		{
		}

		// protected methods

		protected internal virtual string GetComponents (Uri uri, UriComponents components, UriFormat format)
		{
			if ((format < UriFormat.UriEscaped) || (format > UriFormat.SafeUnescaped))
				throw new ArgumentOutOfRangeException ("format");

			// it's easier to answer some case directly (as the output isn't identical 
			// when mixed with others components, e.g. leading slash, # ...)
			switch (components) {
			case UriComponents.Scheme:
				return uri.Scheme;
			case UriComponents.UserInfo:
				return uri.UserInfo;
			case UriComponents.Port:
				if (uri.IsDefaultPort)
					return String.Empty;
				return uri.Port.ToString ();
			case UriComponents.Path:
				return Format (IgnoreFirstCharIf (uri.LocalPath, '/'), format);
			case UriComponents.Fragment:
				return Format (IgnoreFirstCharIf (uri.Fragment, '#'), format);
			case UriComponents.StrongPort:
				return uri.Port.ToString ();
			case UriComponents.SerializationInfoString:
				components = UriComponents.AbsoluteUri;
				break;
			}

			// now we deal with multiple flags...

			StringBuilder sb = new StringBuilder ();
			if ((components & UriComponents.Scheme) != 0) {
				sb.Append (uri.Scheme);
				sb.Append (Uri.GetSchemeDelimiter (uri.Scheme));
			}

			if ((components & UriComponents.UserInfo) != 0) {
				string s = uri.UserInfo;
				if (s.Length > 0) {
					sb.Append (s);
					sb.Append ("@");
				}
			}

			if ((components & UriComponents.Host) != 0)
				sb.Append (uri.Host);

			// for StrongPort always show port - even if -1
			// otherwise only display if ut's not the default port
			if (((components & UriComponents.StrongPort) != 0) ||
				((components & UriComponents.Port) != 0) && !uri.IsDefaultPort) {
				sb.Append (":");
				sb.Append (uri.Port);
			}

			if ((components & UriComponents.Path) != 0) {
				sb.Append (uri.LocalPath);
			}

			if ((components & UriComponents.Query) != 0)
				sb.Append (uri.Query);

			if ((components & UriComponents.Fragment) != 0) {
				sb.Append (uri.Fragment);
			}

			return Format (sb.ToString (), format);
		}

		protected internal virtual void InitializeAndValidate (Uri uri, out UriFormatException parsingError)
		{
			// bad boy, it should check null arguments.
			if (uri.Scheme != scheme_name)
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
				return Uri.EscapeString (s, false, true, true);
			case UriFormat.SafeUnescaped:
				// TODO subset of escape rules
				s = Uri.Unescape (s, false);
				return s; //Uri.EscapeString (s, false, true, true);
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
			InternalRegister (newtable, new DefaultUriParser (), Uri.UriSchemeNews, 119);
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

#endif
