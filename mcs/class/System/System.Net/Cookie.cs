//
// System.Net.Cookie.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
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

namespace System.Net {

	// Supported cookie formats are:
	// Netscape: http://home.netscape.com/newsref/std/cookie_spec.html
	// RFC 2109: http://www.ietf.org/rfc/rfc2109.txt
	// RFC 2965: http://www.ietf.org/rfc/rfc2965.txt
	[Serializable]
	public sealed class Cookie 
	{
		private string comment;
		private Uri commentUri;
		private bool discard;
		private string domain;
		private bool expired;
		private DateTime expires;
		private string name;
		private string path;
		private string port;
		private int [] ports;
		private bool secure;
		private DateTime timestamp;
		private string val;
		private int version;
		
		private static char [] reservedCharsName = new char [] {' ', '=', ';', ',', '\n', '\r', '\t'};
		private static char [] reservedCharsValue = new char [] {';', ','};
		private static char [] portSeparators = new char [] {'"', ','};
                private static string tspecials = "()<>@,;:\\\"/[]?={} \t";   // from RFC 2965, 2068

		public Cookie ()
		{
			expires = DateTime.MinValue;
			timestamp = DateTime.Now;
			domain = "";
			name = "";
			val = "";
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
			set { domain = value == null ? String.Empty : value; }
		}

		public bool Expired {
			get { 
				return expires <= DateTime.Now && 
				       expires != DateTime.MinValue;
			}
			set { 
				expired = value; 
				if (expired) {
					expires = DateTime.Now;
				}
			}
		}

		public DateTime Expires {
			get { return expires; }
			set { expires = value; }
		}

		public string Name {
			get { return name; }
			set { 
				if (value == null || value.Length == 0) {
					throw new CookieException ("Name cannot be empty");
				}			
				
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
			get { return (path == null) ? "/" : path; }
			set { path = (value == null) ? String.Empty : value; }
		}

		public string Port {
			get { return port; }
			set { 
				if (value == null || value.Length == 0) {
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
			}
		}
		
		int[] Ports {
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
			       String.Compare (this.name, c.name, true) == 0 &&
			       String.Compare (this.val, c.val, false) == 0 &&
			       String.Compare (this.path, c.path, false) == 0 &&
			       String.Compare (this.domain, c.domain, true) == 0 &&
			       this.version == c.version;
		}
		
		public override int GetHashCode ()
		{
			return hash(name.ToLower ().GetHashCode (),
			            val.GetHashCode (),
			            path.GetHashCode (),
			            domain.ToLower ().GetHashCode (),
			            version);
		}
		
		private static int hash (int i, int j, int k, int l, int m) 
		{
			return i ^ (j << 13 | j >> 19) ^ (k << 26 | k >> 6) ^ (l << 7 | l >> 25) ^ (m << 20 | m >> 12);
		}
		
		// returns a string that can be used to send a cookie to an Origin Server
		// i.e., only used for clients
		// see also para 3.3.4 of RFC 1965
		public override string ToString () 
		{
			if (name.Length == 0) 
				return String.Empty;

			StringBuilder result = new StringBuilder (64);
	
			if (version > 0) {
				result.Append ("$Version=").Append (version).Append (";");
            		}				
				
			result.Append (name).Append ("=").Append (val);

			// in the MS.Net implementation path and domain don't show up in
			// the result, I guess that's a bug in their implementation...
			if (path != null && path.Length != 0)
				result.Append (";$Path=").Append (QuotedString (path));
				
			if (domain != null && domain.Length != 0)
				result.Append (";$Domain=").Append (QuotedString (domain));			
	
			if (port != null && port.Length != 0)
				result.Append (";$Port=").Append (port);	
						
			return result.ToString ();
		}
				
		// See par 3.6 of RFC 2616
  	    	private string QuotedString (string value)
	    	{
			if (version == 0 || IsToken (value))
				return value;
			else 
				return "\"" + value.Replace("\"", "\\\"") + "\"";
	    	}			    	    

	    	private bool IsToken (string value) 
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

