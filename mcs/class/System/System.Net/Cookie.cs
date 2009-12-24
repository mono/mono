//
// System.Net.Cookie.cs
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Daniel Nauck    (dna(at)mono-project(dot)de)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004,2009 Novell, Inc (http://www.novell.com)
//

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
using System.Text;
using System.Globalization;
using System.Collections;

namespace System.Net {

	// Supported cookie formats are:
	// Netscape: http://home.netscape.com/newsref/std/cookie_spec.html
	// RFC 2109: http://www.ietf.org/rfc/rfc2109.txt
	// RFC 2965: http://www.ietf.org/rfc/rfc2965.txt
	[Serializable]
	public sealed class Cookie 
	{
		string comment;
		Uri commentUri;
		bool discard;
		string domain;
		DateTime expires;
		bool httpOnly;
		string name;
		string path;
		string port;
		int [] ports;
		bool secure;
		DateTime timestamp;
		string val;
		int version;
		
		static char [] reservedCharsName = new char [] {' ', '=', ';', ',', '\n', '\r', '\t'};
		static char [] portSeparators = new char [] {'"', ','};
                static string tspecials = "()<>@,;:\\\"/[]?={} \t";   // from RFC 2965, 2068

		public Cookie ()
		{
			expires = DateTime.MinValue;
			timestamp = DateTime.Now;
			domain = String.Empty;
			name = String.Empty;
			val = String.Empty;
			comment = String.Empty;
			port = String.Empty;
		}

		public Cookie (string name, string value)
			: this ()
		{
			Name = name;
			Value = value;
		}

		public Cookie (string name, string value, string path) 
			: this (name, value) 
		{
			Path = path;
		}

		public Cookie (string name, string value, string path, string domain)
			: this (name, value, path)
		{
			Domain = domain;
		}

		public string Comment {
			get { return comment; }
			set { comment = value == null ? String.Empty : value; }
		}

		public Uri CommentUri {
			get { return commentUri; }
			set { commentUri = value; }
		}

		public bool Discard {
			get { return discard; }
			set { discard = value; }
		}

		public string Domain {
			get { return domain; }
			set {
				if (String.IsNullOrEmpty (value)) {
					domain = String.Empty;
					ExactDomain = true;
				} else {
					domain = value;
					ExactDomain = (value [0] != '.');
				}
			}
		}

		internal bool ExactDomain { get; set; }

		public bool Expired {
			get { 
				return expires <= DateTime.Now && 
				       expires != DateTime.MinValue;
			}
			set {  
				if (value)
					expires = DateTime.Now;
			}
		}

		public DateTime Expires {
			get { return expires; }
			set { expires = value; }
		}

		public bool HttpOnly {
			get { return httpOnly; }
			set { httpOnly = value; }
		}

		public string Name {
			get { return name; }
			set { 
				if (String.IsNullOrEmpty (value))
					throw new CookieException ("Name cannot be empty");
				
				if (value [0] == '$' || value.IndexOfAny (reservedCharsName) != -1) {
					// see CookieTest, according to MS implementation
					// the name value changes even though it's incorrect
					name = String.Empty;
					throw new CookieException ("Name contains invalid characters");
				}
					
				name = value; 
			}
		}

		public string Path {
			get { return (path == null) ? String.Empty : path; }
			set { path = (value == null) ? String.Empty : value; }
		}

		public string Port {
			get { return port; }
			set { 
				if (String.IsNullOrEmpty (value)) {
					port = String.Empty;
					return;
				}
				if (value [0] != '"' || value [value.Length - 1] != '"') {
					throw new CookieException("The 'Port'='" + value + "' part of the cookie is invalid. Port must be enclosed by double quotes.");
				}
				port = value; 
				string [] values = port.Split (portSeparators);
				ports = new int[values.Length];
				for (int i = 0; i < ports.Length; i++) {
					ports [i] = Int32.MinValue;
					if (values [i].Length == 0)
						continue;
					try {						
						ports [i] = Int32.Parse (values [i]);
					} catch (Exception e) {
						throw new CookieException("The 'Port'='" + value + "' part of the cookie is invalid. Invalid value: " + values [i], e);
					}
				}
				Version = 1;
			}
		}

		internal int [] Ports {
			get { return ports; }
		}

		public bool Secure {
			get { return secure; }
			set { secure = value; }
		}

		public DateTime TimeStamp {
			get { return timestamp; }
		}

		public string Value {
			get { return val; }
			set { 
				if (value == null) {
					val = String.Empty;
					return;
				}
				
				// LAMESPEC: According to .Net specs the Value property should not accept 
				// the semicolon and comma characters, yet it does. For now we'll follow
				// the behaviour of MS.Net instead of the specs.
				/*
				if (value.IndexOfAny(reservedCharsValue) != -1)
					throw new CookieException("Invalid value. Value cannot contain semicolon or comma characters.");
				*/
				
				val = value; 
			}
		}
		
		public int Version {
			get { return version; }
			set { 
				if ((value < 0) || (value > 10)) 
					version = 0;
				else 
					version = value; 
			}
		}

		public override bool Equals (Object obj) 
		{
			System.Net.Cookie c = obj as System.Net.Cookie;			
			
			return c != null &&
			       String.Compare (this.name, c.name, true, CultureInfo.InvariantCulture) == 0 &&
			       String.Compare (this.val, c.val, false, CultureInfo.InvariantCulture) == 0 &&
			       String.Compare (this.Path, c.Path, false, CultureInfo.InvariantCulture) == 0 &&
			       String.Compare (this.domain, c.domain, true, CultureInfo.InvariantCulture) == 0 &&
			       this.version == c.version;
		}

		public override int GetHashCode ()
		{
			return hash(CaseInsensitiveHashCodeProvider.DefaultInvariant.GetHashCode(name),
			            val.GetHashCode (),
			            Path.GetHashCode (),
	                            CaseInsensitiveHashCodeProvider.DefaultInvariant.GetHashCode (domain),
			            version);
		}

		private static int hash (int i, int j, int k, int l, int m) 
		{
			return i ^ (j << 13 | j >> 19) ^ (k << 26 | k >> 6) ^ (l << 7 | l >> 25) ^ (m << 20 | m >> 12);
		}

		// returns a string that can be used to send a cookie to an Origin Server
		// i.e., only used for clients
		// see para 4.2.2 of RFC 2109 and para 3.3.4 of RFC 2965
		// see also bug #316017
		public override string ToString () 
		{
			return ToString (null);
		}

		internal string ToString (Uri uri)
		{
			if (name.Length == 0) 
				return String.Empty;

			StringBuilder result = new StringBuilder (64);
	
			if (version > 0)
				result.Append ("$Version=").Append (version).Append ("; ");		
				
			result.Append (name).Append ("=").Append (val);
			
			if (version == 0)
				return result.ToString ();

			if (!String.IsNullOrEmpty (path))
				result.Append ("; $Path=").Append (path);
			else if (uri != null)
				result.Append ("; $Path=/").Append (path);

			bool append_domain = (uri == null) || (uri.Host != domain);
			if (append_domain && !String.IsNullOrEmpty (domain))
				result.Append ("; $Domain=").Append (domain);			
	
			if (port != null && port.Length != 0)
				result.Append ("; $Port=").Append (port);	
						
			return result.ToString ();
		}

		internal string ToClientString () 
		{
			if (name.Length == 0) 
				return String.Empty;

			StringBuilder result = new StringBuilder (64);
	
			if (version > 0) 
				result.Append ("Version=").Append (version).Append (";");
				
			result.Append (name).Append ("=").Append (val);

			if (path != null && path.Length != 0)
				result.Append (";Path=").Append (QuotedString (path));
				
			if (domain != null && domain.Length != 0)
				result.Append (";Domain=").Append (QuotedString (domain));			
	
			if (port != null && port.Length != 0)
				result.Append (";Port=").Append (port);	
						
			return result.ToString ();
		}

		// See par 3.6 of RFC 2616
  	    	string QuotedString (string value)
	    	{
			if (version == 0 || IsToken (value))
				return value;
			else 
				return "\"" + value.Replace("\"", "\\\"") + "\"";
	    	}			    	    

	    	bool IsToken (string value) 
	    	{
			int len = value.Length;
			for (int i = 0; i < len; i++) {
			    	char c = value [i];
				if (c < 0x20 || c >= 0x7f || tspecials.IndexOf (c) != -1)
			      		return false;
			}
			return true;
	    	}	    
	}
}

