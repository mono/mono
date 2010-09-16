//
// System.UriBuilder
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//
// Copyright (C) 2005, 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Runtime.Serialization;
using System.Text;

// See RFC 2396 for more info on URI's.

namespace System 
{
	public class UriBuilder
	{
		private string scheme;
		private string host;
		private int port;
		private string path;
		private string query;
		private string fragment;
		private string username;
		private string password;
		
		private Uri uri;
		private bool modified;
		
		
		// Constructors
		
		public UriBuilder ()
			: this (Uri.UriSchemeHttp, "localhost")
		{
		}

		public UriBuilder (string uri) : this (new Uri (uri))
		{
		}
		
		public UriBuilder (Uri uri)
		{
#if NET_4_0
			if (uri == null)
				throw new ArgumentNullException ("uri");
#endif
			scheme = uri.Scheme;
			host = uri.Host;
			port = uri.Port;
			path = uri.AbsolutePath;
			query = uri.Query;
			fragment = uri.Fragment;
			username = uri.UserInfo;
			int pos = username.IndexOf (':');
			if (pos != -1) {
				password = username.Substring (pos + 1);
				username = username.Substring (0, pos);
			} else {
				password = String.Empty;
			}
			modified = true;
		}
		
		public UriBuilder (string schemeName, string hostName) 
		{
			Scheme = schemeName;
			Host = hostName;
			port = -1;
			Path = String.Empty;   // dependent on scheme it may set path to "/"
			query = String.Empty;
			fragment = String.Empty;
			username = String.Empty;
			password = String.Empty;
			modified = true;
		}
		
		public UriBuilder (string scheme, string host, int portNumber) 
			: this (scheme, host)
		{
			Port = portNumber;
		}
		
		public UriBuilder (string scheme, string host, int port, string pathValue)
			: this (scheme, host, port)
		{
			Path = pathValue;
		}

		public UriBuilder (string scheme, string host, int port, string pathValue, string extraValue)
			: this (scheme, host, port, pathValue)
		{
			if (extraValue == null || extraValue.Length == 0)
				return;
				
			if (extraValue [0] == '#') 
				Fragment = extraValue.Remove (0, 1);
			else if (extraValue [0] == '?') 
				Query = extraValue.Remove (0, 1);
			else 
				throw new ArgumentException ("extraValue");
		}

		
		// Properties
		
		public string Fragment {
			get { return fragment; }
			set {
				fragment = value;
				if (fragment == null)
					fragment = String.Empty;
				else if (fragment.Length > 0)
					fragment = "#" + value.Replace ("%23", "#");
				modified = true;
			}
		}

		public string Host {
			get { return host; }
			set {
				host = (value == null) ? String.Empty : value;
				modified = true;
			}
		}

		public string Password {
			get { return password; }
			set {
				password = (value == null) ? String.Empty : value;
			}
		}
		
		public string Path {
			get { return path; }
			set {
				if (value == null || value.Length == 0) {
					path = "/";
				} else {
					path = Uri.EscapeString (value.Replace ('\\', '/'), Uri.EscapeCommonHexBracketsQuery);
				}
				modified = true;
			}
		}
		
		public int Port {
			get { return port; }
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value");
				// apparently it is
				port = value;
				modified = true;
			}
		}
		
		public string Query {
			get { return query; }
			set {
				// LAMESPEC: it doesn't say to always prepend a 
				// question mark to the value.. it does say this 
				// for fragment.
				if (value == null || value.Length == 0)
					query = String.Empty;
				else
					query = "?" + value;
				modified = true;
			}
		}
		
		public string Scheme {
			get { return scheme; }
			set {
				if (value == null)
					value = String.Empty;
				int colonPos = value.IndexOf (':');
				if (colonPos != -1)
					value = value.Substring (0, colonPos);
				scheme = value.ToLower ();
				modified = true;
			}
		}
		
		public Uri Uri {
			get {
				if (!modified) 
					return uri;
				uri = new Uri (ToString (), true);
				// some properties are updated once the Uri is created - see unit tests
				host = uri.Host;
				modified = false;
				return uri;
			}
		}
		
		public string UserName {
			get { return username; }
			set {
				username = (value == null) ? String.Empty : value;
				modified = true;
			}
		}

		// Methods
		
		public override bool Equals (object rparam) 
		{
			return (rparam == null) ? false : this.Uri.Equals (rparam.ToString ());
		}
		
		public override int GetHashCode ()
		{
			return this.Uri.GetHashCode ();
		}
		
		public override string ToString ()
		{
			StringBuilder builder = new StringBuilder ();

			builder.Append (scheme);
			// note: mailto and news use ':', not "://", as their delimiter
			builder.Append (Uri.GetSchemeDelimiter (scheme));

			if (username != String.Empty) {
				builder.Append (username);
				if (password != String.Empty)
					builder.Append (":" + password);
				builder.Append ('@');
			}

			if (host.Length > 0) {
				builder.Append (host);
				if (port > 0)
					builder.Append (":" + port);
			}

			if (path != String.Empty &&
			    builder [builder.Length - 1] != '/' &&
			    path.Length > 0 && path [0] != '/')
				builder.Append ('/');
			builder.Append (path);
			builder.Append (query);
			builder.Append (fragment);

			return builder.ToString ();
		}
	}
}
