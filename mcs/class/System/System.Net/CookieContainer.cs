//
// System.Net.CookieContainer
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) 2003 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
using System.Runtime.Serialization;
using System.Text;

namespace System.Net 
{
	[Serializable]
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
			if (perDomainCapacity <= 0 || perDomainCapacity < capacity)
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
				if ((value <= 0) ||
				    (value < perDomainCapacity && perDomainCapacity != Int32.MaxValue))
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
				if ((value <= 0) ||
				    (value > DefaultCookieLimit && value != Int32.MaxValue))
					throw new ArgumentOutOfRangeException ("value");					
				if (value < perDomainCapacity)
					perDomainCapacity = value;
				perDomainCapacity = value;
			}
		}
		
		public void Add (Cookie cookie) 
		{
			if (cookie == null)
				throw new ArgumentNullException ("cookie");

			if (cookie.Domain == null && cookie.Domain == "")
				throw new ArgumentException ("Cookie domain not set.", "cookie");

			if (cookie.Value.ToString ().Length > maxCookieSize)
				throw new CookieException ("Cookie size too big");

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

		[MonoTODO ("Uri")]
		public void Add (Uri uri, Cookie cookie)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");
			
			Add (cookie);
		}

		[MonoTODO("Uri")]
		public void Add (Uri uri, CookieCollection cookies)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");
			
			Add (cookies);
		}		

		[MonoTODO("Uri")]
		public string GetCookieHeader (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");

			if (cookies == null)
				return "";

			StringBuilder result = new StringBuilder ();
			bool notfirst = false;
			foreach (Cookie cookie in cookies) {
				if (notfirst)
					result.Append (';');

				result.Append (cookie.ToString ());
				notfirst = true;
			}
			return result.ToString ();
		}

		[MonoTODO("Uri")]
		public CookieCollection GetCookies (Uri uri)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");
			
			CookieCollection coll = new CookieCollection ();
			if (cookies == null)
				return coll;
			
			foreach (Cookie cookie in cookies)
				coll.Add (cookie);
			
			return coll;
		}

		public void SetCookies (Uri uri, string cookieHeader)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");
			
			if (cookieHeader == null)
				throw new ArgumentNullException ("cookieHeader");
			
			ParseAndAddCookies (cookieHeader);
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


		void ParseAndAddCookies (string header)
		{
			if (header.Length == 0)
				return;

			/* RFC 2109
			 *	cookie          =       "Cookie:" cookie-version
			 *				   1*((";" | ",") cookie-value)
			 *	cookie-value    =       NAME "=" VALUE [";" path] [";" domain]
			 *	cookie-version  =       "$Version" "=" value
			 *	NAME            =       attr
			 *	VALUE           =       value
			 *	path            =       "$Path" "=" value
			 *	domain          =       "$Domain" "=" value
			 *
			 *	MS ignores $Version! 
			 *	',' as a separator produces errors.
			 */

			string [] name_values = header.Trim ().Split (';');
			int length = name_values.Length;
			Cookie cookie = null;
			int pos;
			for (int i = 0; i < length; i++) {
				pos = 0;
				string name_value = name_values [i].Trim ();
				string name = GetCookieName (name_value, name_value.Length, ref pos);
				string value = GetCookieValue (name_value, name_value.Length, ref pos);
				if (cookie != null) {
					if (name == "$Path") {
						cookie.Path = value;
						continue;
					} else if (name == "$Domain") {
						cookie.Domain = value;
						continue;
					} else {
						Add (cookie);
						cookie = null;
					}
				}
				cookie = new Cookie (name, value);
			}

			if (cookie != null)
				Add (cookie);
		}

	} // CookieContainer

} // System.Net

