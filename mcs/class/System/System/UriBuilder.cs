//
// System.UriBuilder
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
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
		
		public UriBuilder () : this (Uri.UriSchemeHttp, "loopback")
		{		
		}

		public UriBuilder (string uri) : this (new Uri (uri))
		{
		}
		
		public UriBuilder (Uri uri)
		{
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
					fragment = "#" + Uri.EscapeString (fragment, false, true, true).Replace ("%23", "#");
				query = String.Empty;				
				modified = true;
			}
		}

		public string Host {
			get { return host; }
			set {
				host = (value == null) ? String.Empty : value;;
				modified = true;
			}
		}

		public string Password {
			get { return password; }
			set {
				password = (value == null) ? String.Empty : value;;
				modified = true;
			}
		}
		
		public string Path {
			get { return path; }
			set {
				if (value == null || value.Length == 0) {
					if ((scheme != Uri.UriSchemeMailto) &&
					    (scheme != Uri.UriSchemeNews)) 
						path = "/";					
				} else {
					path = Uri.EscapeString (value.Replace ('\\', '/'), false, true, true);
					if ((scheme != Uri.UriSchemeMailto) &&
					    (scheme != Uri.UriSchemeNews) &&
					    path [0] != '/') 
						path = "/" + path;					
				}
				modified = true;
			}
		}
		
		public int Port {
			get { return port; }
			set {
				if (value == -1) {
					port = -1;
					return;
				}
				
				// checks whether port number is valid
				new System.Net.IPEndPoint (0, value);
				
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
				query = value;
				if (query == null)
					query = String.Empty;
				else if (query.Length > 0)
					query = "?" + Uri.EscapeString (query, false, true, true);
				fragment = String.Empty;
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

				StringBuilder s = new StringBuilder ();
				s.Append (scheme);
				s.Append (Uri.GetSchemeDelimiter (scheme));
				if (username.Length > 0) {
					string userinfo = username;
					if (password.Length > 0)
						userinfo += ":" + password;
					s.Append (userinfo).Append ('@');
				}
				s.Append (host);
				if (port != -1)
					s.Append (':').Append (port);
				s.Append (path);
				s.Append (query);
				s.Append (fragment);
				
				uri = new Uri (s.ToString (), true);
				modified = false;
				return uri;
			}
		}
		
		public string UserName {
			get { return username; }
			set {
				username = (value == null) ? String.Empty : value;;
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
			builder.Append ("://");

			if (username != String.Empty) {
				builder.Append (username);
				if (password != String.Empty)
					builder.Append (":" + password);
				builder.Append ('@');
			}

			builder.Append (host);
			builder.Append (":" + port);
			builder.Append (path);
			builder.Append (query);

			return builder.ToString ();
		}
	} 
} 

