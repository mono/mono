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

		[MonoTODO]
		protected internal virtual string GetComponents (Uri uri, UriComponents components, UriFormat format)
		{
			if ((format < UriFormat.UriEscaped) || (format > UriFormat.SafeUnescaped))
				throw new ArgumentOutOfRangeException ("format");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual void InitializeAndValidate (Uri uri, out UriFormatException parsingError)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual bool IsBaseOf (Uri baseUri, Uri relativeUri)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal virtual bool IsWellFormedOriginalString (Uri uri)
		{
			// well formed according to RFC2396 and RFC2732
			// see Uri.IsWellFormedOriginalString for some docs
			throw new NotImplementedException ();
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

		// static methods

		private static void CreateDefaults ()
		{
			lock (lock_object) {
				if (table == null) {
					table = new Hashtable ();

					InternalRegister (new FileStyleUriParser (), Uri.UriSchemeFile, -1);
					InternalRegister (new FtpStyleUriParser (), Uri.UriSchemeFtp, 21);
					InternalRegister (new GopherStyleUriParser (), Uri.UriSchemeGopher, 70);
					InternalRegister (new HttpStyleUriParser (), Uri.UriSchemeHttp, 80);
					InternalRegister (new HttpStyleUriParser (), Uri.UriSchemeHttps, 443);
					// FIXME ??? no MailToUriParser
					InternalRegister (new GenericUriParser (GenericUriParserOptions.Default),
						Uri.UriSchemeMailto, 25);
					InternalRegister (new NetPipeStyleUriParser (), Uri.UriSchemeNetPipe, -1);
					InternalRegister (new NetTcpStyleUriParser (), Uri.UriSchemeNetTcp, -1);
					InternalRegister (new NewsStyleUriParser (), Uri.UriSchemeNews, 119);
					InternalRegister (new NewsStyleUriParser (), Uri.UriSchemeNntp, 119);
					// not defined in Uri.UriScheme* but a parser class exists
					InternalRegister (new LdapStyleUriParser (), "ldap", 389);
				}
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
		private static void InternalRegister (UriParser uriParser, string schemeName, int defaultPort)
		{
			uriParser.SchemeName = schemeName;
			uriParser.DefaultPort = defaultPort;
			table.Add (schemeName, uriParser);
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
			InternalRegister (uriParser, lc, defaultPort);
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
