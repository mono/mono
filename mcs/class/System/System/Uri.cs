//
// System.Uri
//
// Author:
//    Garrett Rooney (rooneg@electricjellyfish.net)
//
// (C) 2001 Garrett Rooney
//

using System.Runtime.Serialization;

namespace System {

	[Serializable]
	public class Uri : MarshalByRefObject, ISerializable {

		private string path = "";
		private string host = "";
		private string fragment = "";
		private string scheme = "";
		private string port = "";
		private string query = "";
		
		// FIXME: is this correct?
		private bool userEscaped = false;

		public static readonly string SchemeDelimiter = "://";
		public static readonly string UriSchemeFile = "file";
		public static readonly string UriSchemeFtp = "ftp";
		public static readonly string UriSchemeGopher = "gopher";
		public static readonly string UriSchemeHttp = "http";
		public static readonly string UriSchemeHttps = "https";
		public static readonly string UriSchemeMailto = "mailto";
		public static readonly string UriSchemeNntp = "nntp";

		// the details table holds random info about the types of uri's
		private struct detail {
			public string scheme;
			public string port;
			public string delimiter;

			public detail(string s, string p, string d) {
				scheme = s;
				port = p;
				delimiter = d;
			}
		};

		static detail[] details = new detail[] {
			new detail(UriSchemeFile, "-1", SchemeDelimiter),
			new detail(UriSchemeFtp, "23", SchemeDelimiter),
			new detail(UriSchemeGopher, "70", SchemeDelimiter),
			new detail(UriSchemeHttp, "80", SchemeDelimiter),
			new detail(UriSchemeHttps, "223", SchemeDelimiter),
			new detail(UriSchemeMailto, "25", ":"),
			new detail(UriSchemeNntp, "119", SchemeDelimiter)
		};

		public static UriHostNameType CheckHostName(string name) {
			throw new NotImplementedException();
		}

		public static bool CheckSchemeName(string schemeName) {
			throw new NotImplementedException();
		}

		public static int FromHex(char digit) {
			throw new NotImplementedException();
		}

		public static string HexEscape(char character) {
			throw new NotImplementedException();
		}

		public static char HexUnescape(string pattern, ref int index) {
			throw new NotImplementedException();
		}

		public static bool IsHexDigit(char character) {
			throw new NotImplementedException();
		}

		public static bool IsHexEncoding(string pattern, int index) {
			throw new NotImplementedException();
		}

		private void Parse(string uri) {
			int i;

			// figure out the scheme
			int colon = uri.IndexOf(':');
			if (colon == -1 || colon == uri.Length) {
				throw new UriFormatException();
			} else {
				string s = uri.Substring(0, colon).ToLower();
				uri = uri.Remove(0, colon);
				for (i = 0; i < details.Length; i++) {
					if (details[i].scheme == s) {
						scheme = details[i].scheme;

						// assume default port.  if 
						// they specify one it'll get 
						// set later on.
						port = details[i].port;
						break;
					}
				}
				if (i == details.Length) {
					throw new UriFormatException();
				}
			}

			// get rid of the delimiter
			uri = uri.Remove(0, details[i].delimiter.Length);

			parseHost(uri);
		}

		[MonoTODO]
		private void parseHost(string uri) {

			// FIXME: this doesn't handle IPv6 addresses correctly
			for (int i = 0; i < uri.Length; i++) {
				switch (uri[i]) {
					case ':':
						host = uri.Substring(0, i);
						parsePort(uri.Remove(0, i + 1));
						return;
					case '/':
						host = uri.Substring(0, i);
						parsePath(uri.Remove(0, i + 1));
						return;
					case '?':
					case '#':
						throw new UriFormatException();
					default:
						break;
				}
			}

			host = uri;
		}

		[MonoTODO]
		private void parsePort(string uri) {

			for (int i = 0; i < uri.Length; i++) {
				switch (uri[i]) {
					case '/':
						port = uri.Substring(0, i);
						parsePath(uri.Remove(0, i + 1));
						return;
					case '?':
					case '#':
						throw new UriFormatException();
					default:
						// FIXME: should this check if 
						// uri[i] is a number?
						break;
				}
			}

			port = uri;
		}

		private void parsePath(string uri) {
			
			for (int i = 0; i < uri.Length; i++) {
				switch (uri[i]) {
					case '#':
						path = uri.Substring(0, i);
						fragment = uri.Remove(0, i + 1);
						return;
					case '?':
						path = uri.Substring(0, i);
						query = uri.Remove(0, i + 1);
						return;
					default:
						break;
				}
			}

			path = uri;
		}

		public Uri(string uri) {
			Parse(uri);
		}

		protected Uri(SerializationInfo serializationInfo, 
			      StreamingContext streamingContext) {
			throw new NotImplementedException();
		}

		public Uri(string uri, bool dontEscape) {
			userEscaped = dontEscape;
			Parse(uri);
		}

		public Uri(Uri baseUri, string relativeUri) {
			throw new NotImplementedException();
		}

		public Uri(Uri baseUri, string relativeUri, bool dontEscape) {
			userEscaped = dontEscape;

			throw new NotImplementedException();
		}

		public string AbsolutePath { get { return path; } }

		public string AbsoluteUri { 
			get { throw new NotImplementedException(); } 
		}

		public string Authority { get { return host + ":" + port; } }

		public string Fragment { get { return fragment; } }

		public string Host { get { return host; } }

		public UriHostNameType HostNameType { 
			get { throw new NotImplementedException(); } 
		}

		public bool IsDefaultPort { 
			get { 
				for (int i = 0; i < details.Length; i++) {
					if (details[i].scheme == scheme) {
						if (details[i].port == port) {
							return true;
						}
					}
				}
				
				return false;			
			} 
		}

		public bool IsFile { 
			get {
				if (scheme == UriSchemeFile)
					return true;
				else
					return false;
			}
		}

		[MonoTODO ("Should check IPv6")]
		public bool IsLoopback { 
			get { 
				if (host == "localhost" || host == "127.0.0.1")
					return true;
				else
					return false;
			} 
		}

		[MonoTODO]
		public bool IsUnc { 
			get { throw new NotImplementedException(); } 
		}

		[MonoTODO]
		public string LocalPath { 
			get { throw new NotImplementedException(); } 
		}

		public string PathAndQuery { 
			get { return path + "?" + query; } 
		}

		public string Port { get { return port; } }

		public string Query { get { return query; } }

		public string Scheme { get { return scheme; } }

		// FIXME: what the hell are segments?
		[MonoTODO]
		public string[] Segments { 
			get { throw new NotImplementedException(); } 
		}

		public bool UserEscaped { get { return userEscaped; } }

		[MonoTODO]
		public string UserInfo { 
			get { throw new NotImplementedException(); } 
		}

		[MonoTODO]
		public override bool Equals(object compared) {
			throw new NotImplementedException();	
		}

		[MonoTODO]
		public override int GetHashCode() {
			throw new NotImplementedException();	
		}

		[MonoTODO]
		public string GetLeftPart(UriPartial part) {
			throw new NotImplementedException();	
		}

		[MonoTODO]
		public string MakeRelative(Uri toUri) {
			throw new NotImplementedException();	
		}

		[MonoTODO]
		public override string ToString() {
			throw new NotImplementedException();	
		}

		[MonoTODO]
		public void GetObjectData(SerializationInfo info, 
					  StreamingContext context)
		{
			// FIXME: Implement me.  yes, it is public because it implements ISerializable
		}

		[MonoTODO]
		protected static string EscapeString(string str) {
			throw new NotImplementedException();	
		}
	}
}
