//
// System.Net.CookieContainer
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
// (c) Copyright 2004 Ximian, Inc. (http://www.ximian.com)
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
using System.Collections;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace System.Net 
{
	[Serializable]
	[MonoTODO ("Need to remove older/unused cookies if it reaches the maximum capacity")]
	public class CookieContainer
	{
		public const int DefaultCookieLengthLimit = 4096;
		public const int DefaultCookieLimit = 300;
		public const int DefaultPerDomainCookieLimit = 20;

		int count;
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
				throw new ArgumentException ("Must be greater than zero", "capacity");

			this.capacity = capacity;
		}
		
		public CookieContainer (int capacity, int perDomainCapacity, int maxCookieSize)
			: this (capacity)
		{
			if (perDomainCapacity != Int32.MaxValue && (perDomainCapacity <= 0 || perDomainCapacity > capacity))
				throw new ArgumentException ("Invalid value", "perDomaniCapacity");

			if (maxCookieSize <= 0)
				throw new ArgumentException ("Must be greater than zero", "maxCookieSize");

			this.perDomainCapacity = perDomainCapacity;
			this.maxCookieSize = maxCookieSize;
		}

		// properties
		
		public int Count { 
			get { return count; }
		}
		
		public int Capacity {
			get { return capacity; }
			set { 
				if (value < 0 || (value < perDomainCapacity && perDomainCapacity != Int32.MaxValue))
					throw new ArgumentOutOfRangeException ("value");

				if (value < maxCookieSize)
					maxCookieSize = value;

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

			if (cookie.Domain == "")
				throw new ArgumentException ("Cookie domain not set.", "cookie");

			if (cookie.Value.Length > maxCookieSize)
				throw new CookieException ("value is larger than MaxCookieSize.");

			AddCookie (cookie);
		}

		void AddCookie (Cookie cookie)
		{
			lock (this) {
				if (cookies == null)
					cookies = new CookieCollection ();

				if (count + 1 > capacity)
					throw new CookieException ("Capacity exceeded");

				cookies.Add (cookie);
				count++;
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
			if (cookie.Name == null || cookie.Name == "")
				throw new CookieException ("Invalid cookie: name");

			if (cookie.Value == null)
				throw new CookieException ("Invalid cookie: value");

			if (uri != null && cookie.Domain == "")
				cookie.Domain = uri.Host;

			if (cookie.Path == null || cookie.Path == "") {
				if (uri != null) {
					cookie.Path = uri.AbsolutePath;
				} else {
					cookie.Path = "/";
				}
			}

			if (cookie.Port == "" && uri != null && !uri.IsDefaultPort) {
				cookie.Port = uri.Port.ToString ();
			}
		}

		public void Add (Uri uri, Cookie cookie)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			if (cookie == null)
				throw new ArgumentNullException ("cookie");

			Cook (uri, cookie);
			AddCookie (cookie);
		}

		public void Add (Uri uri, CookieCollection cookies)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			if (cookies == null)
				throw new ArgumentNullException ("cookies");

			foreach (Cookie c in cookies) {
				Cook (uri, c);
				AddCookie (c);
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
			foreach (Cookie cookie in cookies) {
				result.Append (cookie.ToString ());
				result.Append (';');
			}

			if (result.Length > 0)
				result.Length--; // remove trailing semicolon

			return result.ToString ();
		}

		static bool CheckDomain (string domain, string host)
		{
			if (domain != "" && domain [0] != '.')
				return (String.Compare (domain, host, true, CultureInfo.InvariantCulture) == 0);

			int dot = host.IndexOf ('.');
			if (dot == -1)
				return (String.Compare (host, domain, true, CultureInfo.InvariantCulture) == 0);
			
			string subdomain = host.Substring (dot);
			return (String.Compare (subdomain, domain, true, CultureInfo.InvariantCulture) == 0);
		}

		public CookieCollection GetCookies (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");
			
			CookieCollection coll = new CookieCollection ();
			if (cookies == null)
				return coll;
			
			foreach (Cookie cookie in cookies) {
				string domain = cookie.Domain;
				string host = uri.Host;
				if (!CheckDomain (domain, host))
					continue;

				if (cookie.Port != "" && cookie.Ports != null && uri.Port != -1) {
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
						    path [path.Length] != '/')
							continue;
					}
				}

				coll.Add (cookie);
			}
			
			return coll;
		}

		public void SetCookies (Uri uri, string cookieHeader)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");
			
			if (cookieHeader == null)
				throw new ArgumentNullException ("cookieHeader");
			
			ParseAndAddCookies (uri, cookieHeader);
		}

		// GetCookieValue, GetCookieName and ParseAndAddCookies copied from HttpRequest.cs
		static string GetCookieValue (string str, int length, ref int i)
		{
			if (i >= length)
				return null;

			int k = i;
			while (k < length && Char.IsWhiteSpace (str [k]))
				k++;

			int begin = k;
			while (k < length && str [k] != ';')
				k++;

			i = k;
			return str.Substring (begin, i - begin).Trim ();
		}

		static string GetCookieName (string str, int length, ref int i)
		{
			if (i >= length)
				return null;

			int k = i;
			while (k < length && Char.IsWhiteSpace (str [k]))
				k++;

			int begin = k;
			while (k < length && str [k] != ';' &&  str [k] != '=')
				k++;

			i = k + 1;
			return str.Substring (begin, k - begin).Trim ();
		}

		static string GetDir (string path)
		{
			if (path == null || path == "")
				return "/";

			int last = path.LastIndexOf ('/');
			if (last == -1)
				return "/" + path;

			return path.Substring (0, last + 1);
		}
		
		void ParseAndAddCookies (Uri uri, string header)
		{
			if (header.Length == 0)
				return;

			string [] name_values = header.Trim ().Split (';');
			int length = name_values.Length;
			Cookie cookie = null;
			int pos;
			CultureInfo inv = CultureInfo.InvariantCulture;
			bool havePath = false;
			bool haveDomain = false;

			for (int i = 0; i < length; i++) {
				pos = 0;
				string name_value = name_values [i].Trim ();
				string name = GetCookieName (name_value, name_value.Length, ref pos);
				if (name == null || name == "")
					throw new CookieException ("Name is empty.");

				string value = GetCookieValue (name_value, name_value.Length, ref pos);
				if (cookie != null) {
					if (!havePath && String.Compare (name, "$Path", true, inv) == 0 ||
					    String.Compare (name, "path", true, inv) == 0) {
					    	havePath = true;
						cookie.Path = value;
						continue;
					}
					
					if (!haveDomain && String.Compare (name, "$Domain", true, inv) == 0 ||
				            String.Compare (name, "domain", true, inv) == 0) {
						cookie.Domain = value;
					    	haveDomain = true;
						continue;
					}

					if (!havePath)
						cookie.Path = GetDir (uri.AbsolutePath);

					if (!haveDomain)
						cookie.Domain = uri.Host;

					havePath = false;
					haveDomain = false;
					Add (cookie);
					cookie = null;
				}
				cookie = new Cookie (name, value);
			}

			if (cookie != null) {
				if (!havePath)
					cookie.Path = GetDir (uri.AbsolutePath);

				if (!haveDomain)
					cookie.Domain = uri.Host;

				Add (cookie);
			}
		}

	} // CookieContainer

} // System.Net

