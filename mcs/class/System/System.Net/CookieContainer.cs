//
// System.Net.CookieContainer
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// (c) Copyright 2004 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)

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
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;

namespace System.Net 
{
	[Serializable]
#if MOONLIGHT
	#if INSIDE_SYSTEM
	internal sealed class CookieContainer {
	#else 
	public sealed class CookieContainer {
	#endif
#else
	public class CookieContainer {
#endif
		public const int DefaultCookieLengthLimit = 4096;
		public const int DefaultCookieLimit = 300;
		public const int DefaultPerDomainCookieLimit = 20;

		int capacity = DefaultCookieLimit;
		int perDomainCapacity = DefaultPerDomainCookieLimit;
		int maxCookieSize = DefaultCookieLengthLimit;
		CookieCollection cookies;
				
		// ctors
		public CookieContainer ()
		{ 
		} 
	
		public CookieContainer (int capacity)
		{
			if (capacity <= 0)
				throw new ArgumentException ("Must be greater than zero", "Capacity");

			this.capacity = capacity;
		}
		
		public CookieContainer (int capacity, int perDomainCapacity, int maxCookieSize)
			: this (capacity)
		{
			if (perDomainCapacity != Int32.MaxValue && (perDomainCapacity <= 0 || perDomainCapacity > capacity))
				throw new ArgumentOutOfRangeException ("perDomainCapacity",
					string.Format ("PerDomainCapacity must be " +
					"greater than {0} and less than {1}.", 0,
					capacity));

			if (maxCookieSize <= 0)
				throw new ArgumentException ("Must be greater than zero", "MaxCookieSize");

			this.perDomainCapacity = perDomainCapacity;
			this.maxCookieSize = maxCookieSize;
		}

		// properties
		
		public int Count { 
			get { return (cookies == null) ? 0 : cookies.Count; }
		}
		
		public int Capacity {
			get { return capacity; }
			set { 
				if (value < 0 || (value < perDomainCapacity && perDomainCapacity != Int32.MaxValue))
					throw new ArgumentOutOfRangeException ("value",
						string.Format ("Capacity must be greater " +
						"than {0} and less than {1}.", 0,
						perDomainCapacity));
				capacity = value;
			}
		}
		
		public int MaxCookieSize {
			get { return maxCookieSize; }
			set {
				if (value <= 0)
					throw new ArgumentOutOfRangeException ("value");
				maxCookieSize = value;
			}
		}
		
		public int PerDomainCapacity {
			get { return perDomainCapacity; }
			set {
				if (value != Int32.MaxValue && (value <= 0 || value > capacity))
					throw new ArgumentOutOfRangeException ("value");
				perDomainCapacity = value;
			}
		}
		
		public void Add (Cookie cookie) 
		{
			if (cookie == null)
				throw new ArgumentNullException ("cookie");

			if (cookie.Domain.Length == 0)
				throw new ArgumentException ("Cookie domain not set.", "cookie.Domain");

			if (cookie.Value.Length > maxCookieSize)
				throw new CookieException ("value is larger than MaxCookieSize.");

			// .NET's Add (Cookie) is fundamentally broken and does not copy properties
			// like Secure, HttpOnly and Expires so we clone the parts that .NET
			// does keep before calling AddCookie
			Cookie c = new Cookie (cookie.Name, cookie.Value);
			c.Path = (cookie.Path.Length == 0) ? "/" : cookie.Path;
			c.Domain = cookie.Domain;
			c.ExactDomain = cookie.ExactDomain;
			c.Version = cookie.Version;
			
			AddCookie (c);
		}

		void AddCookie (Cookie cookie)
		{
			if (cookies == null)
				cookies = new CookieCollection ();

			if (cookies.Count >= capacity)
				RemoveOldest (null);

			// try to avoid counting per-domain
			if (cookies.Count >= perDomainCapacity) {
				if (CountDomain (cookie.Domain) >= perDomainCapacity)
					RemoveOldest (cookie.Domain);
			}

			// clone the important parts of the cookie
			Cookie c = new Cookie (cookie.Name, cookie.Value);
			c.Path = (cookie.Path.Length == 0) ? "/" : cookie.Path;
			c.Domain = cookie.Domain;
			c.ExactDomain = cookie.ExactDomain;
			c.Version = cookie.Version;
			c.Expires = cookie.Expires;
			c.CommentUri = cookie.CommentUri;
			c.Comment = cookie.Comment;
			c.Discard = cookie.Discard;
			c.HttpOnly = cookie.HttpOnly;
			c.Secure = cookie.Secure;

			cookies.Add (c);
			CheckExpiration ();

		}

		int CountDomain (string domain)
		{
			int count = 0;
			foreach (Cookie c in cookies) {
				if (CheckDomain (domain, c.Domain, true))
					count++;
			}
			return count;
		}

		void RemoveOldest (string domain)
		{
			int n = 0;
			DateTime oldest = DateTime.MaxValue;
			for (int i = 0; i < cookies.Count; i++) {
				Cookie c = cookies [i];
				if ((c.TimeStamp < oldest) && ((domain == null) || (domain == c.Domain))) {
					oldest = c.TimeStamp;
					n = i;
				}
			}
			cookies.List.RemoveAt (n);
		}

		// Only needs to be called from AddCookie (Cookie) and GetCookies (Uri)
		void CheckExpiration ()
		{
			if (cookies == null)
				return;

			for (int i = cookies.Count - 1; i >= 0; i--) {
				Cookie cookie = cookies [i];
				if (cookie.Expired)
					cookies.List.RemoveAt (i);
			}
		}

		public void Add (CookieCollection cookies)
		{
			if (cookies == null)
				throw new ArgumentNullException ("cookies");

			foreach (Cookie cookie in cookies)
				Add (cookie);
		}

		void Cook (Uri uri, Cookie cookie)
		{
			if (String.IsNullOrEmpty (cookie.Name))
				throw new CookieException ("Invalid cookie: name");

			if (cookie.Value == null)
				throw new CookieException ("Invalid cookie: value");

			if (uri != null && cookie.Domain.Length == 0)
				cookie.Domain = uri.Host;

			if (cookie.Version == 0 && String.IsNullOrEmpty (cookie.Path)) {
				if (uri != null) {
					cookie.Path = uri.AbsolutePath;
				} else {
					cookie.Path = "/";
				}
			}

			if (cookie.Port.Length == 0 && uri != null && !uri.IsDefaultPort) {
				cookie.Port = "\"" + uri.Port.ToString () + "\"";
			}
		}

		public void Add (Uri uri, Cookie cookie)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			if (cookie == null)
				throw new ArgumentNullException ("cookie");

			if (!cookie.Expired) {
				Cook (uri, cookie);
				AddCookie (cookie);
			}
		}

		public void Add (Uri uri, CookieCollection cookies)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			if (cookies == null)
				throw new ArgumentNullException ("cookies");

			foreach (Cookie cookie in cookies) {
				if (!cookie.Expired) {
					Cook (uri, cookie);
					AddCookie (cookie);
				}
			}
		}		

		public string GetCookieHeader (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			CookieCollection coll = GetCookies (uri);
			if (coll.Count == 0)
				return "";

			StringBuilder result = new StringBuilder ();
			foreach (Cookie cookie in coll) {
				// don't include the domain since it can be infered from the URI
				// include empty path as '/'
				result.Append (cookie.ToString (uri));
				result.Append ("; ");
			}

			if (result.Length > 0)
				result.Length -= 2; // remove trailing semicolon and space

			return result.ToString ();
		}

		static bool CheckDomain (string domain, string host, bool exact)
		{
			if (domain.Length == 0)
				return false;

			if (exact)
				return (String.Compare (host, domain, StringComparison.InvariantCultureIgnoreCase) == 0);

			// check for allowed sub-domains - without string allocations
			if (!host.EndsWith (domain, StringComparison.InvariantCultureIgnoreCase))
				return false;
			// mono.com -> www.mono.com is OK but supermono.com NOT OK
			if (domain [0] == '.')
				return true;
			int p = host.Length - domain.Length - 1;
			if (p < 0)
				return false;
			return (host [p] == '.');
		}

		public CookieCollection GetCookies (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			CheckExpiration ();
			CookieCollection coll = new CookieCollection ();
			if (cookies == null)
				return coll;

			foreach (Cookie cookie in cookies) {
				string domain = cookie.Domain;
				if (!CheckDomain (domain, uri.Host, cookie.ExactDomain))
					continue;

				if (cookie.Port.Length > 0 && cookie.Ports != null && uri.Port != -1) {
					if (Array.IndexOf (cookie.Ports, uri.Port) == -1)
						continue;
				}

				string path = cookie.Path;
				string uripath = uri.AbsolutePath;
				if (path != "" && path != "/") {
					if (uripath != path) {
						if (!uripath.StartsWith (path))
							continue;

						if (path [path.Length - 1] != '/' && uripath.Length > path.Length &&
						    uripath [path.Length] != '/')
							continue;
					}
				}

				if (cookie.Secure && uri.Scheme != "https")
					continue;

				coll.Add (cookie);
			}

			coll.Sort ();
			return coll;
		}

		public void SetCookies (Uri uri, string cookieHeader)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");
			
			if (cookieHeader == null)
				throw new ArgumentNullException ("cookieHeader");			
			
			if (cookieHeader.Length == 0)
				return;
			
			// Cookies must be separated by ',' (like documented on MSDN)
			// but expires uses DAY, DD-MMM-YYYY HH:MM:SS GMT, so simple ',' search is wrong.
			// See http://msdn.microsoft.com/en-us/library/aa384321%28VS.85%29.aspx
			string [] jar = cookieHeader.Split (',');
			string tmpCookie;
			for (int i = 0; i < jar.Length; i++) {
				tmpCookie = jar [i];

				if (jar.Length > i + 1
					&& Regex.IsMatch (jar[i],
						@".*expires\s*=\s*(Mon|Tue|Wed|Thu|Fri|Sat|Sun)",
						RegexOptions.IgnoreCase) 
					&& Regex.IsMatch (jar[i+1],
						@"\s\d{2}-(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)-\d{4} \d{2}:\d{2}:\d{2} GMT",
						RegexOptions.IgnoreCase)) {
					tmpCookie = new StringBuilder (tmpCookie).Append (",").Append (jar [++i]).ToString ();
				}

				try {
					Cookie c = Parse (tmpCookie);

					// add default values from URI if missing from the string
					if (c.Path.Length == 0) {
						c.Path = uri.AbsolutePath;
					} else if (!uri.AbsolutePath.StartsWith (c.Path)) {
						string msg = String.Format ("'Path'='{0}' is invalid with URI", c.Path);
						throw new CookieException (msg);
					}

					if (c.Domain.Length == 0) {
						c.Domain = uri.Host;
						// don't consider domain "a.b.com" as ".a.b.com"
						c.ExactDomain = true;
					}

					AddCookie (c);
				}
				catch (Exception e) {
					string msg = String.Format ("Could not parse cookies for '{0}'.", uri);
					throw new CookieException (msg, e);
				}
			}
		}

		static Cookie Parse (string s)
		{
			string [] parts = s.Split (';');
			Cookie c = new Cookie ();
			for (int i = 0; i < parts.Length; i++) {
				string key, value;
				int sep = parts[i].IndexOf ('=');
				if (sep == -1) {
					key = parts [i].Trim ();
					value = String.Empty;
				} else {
					key = parts [i].Substring (0, sep).Trim ();
					value = parts [i].Substring (sep + 1).Trim ();
				}

				switch (key.ToLowerInvariant ()) {
				case "path":
				case "$path":
					if (c.Path.Length == 0)
						c.Path = value;
					break;
				case "domain":
				case "$domain":
					if (c.Domain.Length == 0) {
						c.Domain = value;
						// here mono.com means "*.mono.com"
						c.ExactDomain = false;
					}
					break;
				case "expires":
				case "$expires":
					if (c.Expires == DateTime.MinValue)
						c.Expires = DateTime.SpecifyKind (DateTime.ParseExact (value,
							@"ddd, dd-MMM-yyyy HH:mm:ss G\MT", CultureInfo.InvariantCulture), DateTimeKind.Utc);
						break;
				case "httponly":
					c.HttpOnly = true;
					break;
				case "secure":
					c.Secure = true;
					break;
				default:
					if (c.Name.Length == 0) {
						c.Name = key;
						c.Value = value;
					}
					break;
				}
			}
			return c;
		}
	}
}

