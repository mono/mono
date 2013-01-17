//
// System.Net.DigestClient.cs
//
// Authors:
//	Greg Reinacker (gregr@rassoc.com)
//	Sebastien Pouliot (spouliot@motus.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com
//
// Copyright 2002-2003 Greg Reinacker, Reinacker & Associates, Inc. All rights reserved.
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// (c) 2003 Novell, Inc. (http://www.novell.com)
//
// Original (server-side) source code available at
// http://www.rassoc.com/gregr/weblog/stories/2002/07/09/webServicesSecurityHttpDigestAuthenticationWithoutActiveDirectory.html
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
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace System.Net
{
	//
	// This works with apache mod_digest
	//TODO:
	//	MD5-sess
	//	qop (auth-int)
	//
	//	See RFC 2617 for details.
	//


	class DigestHeaderParser
	{
		string header;
		int length;
		int pos;
		static string [] keywords = { "realm", "opaque", "nonce", "algorithm", "qop" };
		string [] values = new string [keywords.Length];

		public DigestHeaderParser (string header)
		{
			this.header = header.Trim ();
		}

		public string Realm {
			get { return values [0]; }
		}

		public string Opaque {
			get { return values [1]; }
		}

		public string Nonce {
			get { return values [2]; }
		}
		
		public string Algorithm {
			get { return values [3]; }
		}
		
		public string QOP {
			get { return values [4]; }
		}

		public bool Parse ()
		{
			if (!header.ToLower ().StartsWith ("digest "))
				return false;

			pos = 6;
			length = this.header.Length;
			while (pos < length) {
				string key, value;
				if (!GetKeywordAndValue (out key, out value))
					return false;

				SkipWhitespace ();
				if (pos < length && header [pos] == ',')
					pos++;

				int idx = Array.IndexOf (keywords, (key));
				if (idx == -1)
					continue;

				if (values [idx] != null)
					return false;

				values [idx] = value;
			}

			if (Realm == null || Nonce == null)
				return false;

			return true;
		}

		void SkipWhitespace ()
		{
			char c = ' ';
			while (pos < length && (c == ' ' || c == '\t' || c == '\r' || c == '\n')) {
				c = header [pos++];
			}
			pos--;
		}
		
		string GetKey ()
		{
			SkipWhitespace ();
			int begin = pos;
			while (pos < length && header [pos] != '=') {
				pos++;
			}
			
			string key = header.Substring (begin, pos - begin).Trim ().ToLower ();
			return key;
		}

		bool GetKeywordAndValue (out string key, out string value)
		{
			key = null;
			value = null;
			key = GetKey ();
			if (pos >= length)
				return false;

			SkipWhitespace ();
			if (pos + 1 >= length || header [pos++] != '=')
				return false;

			SkipWhitespace ();
			// note: Apache doesn't use " in all case (like algorithm)
			if (pos + 1 >= length)
				return false;

			bool useQuote = false;
			if (header [pos] == '"') {
				pos++;
				useQuote = true;
			}

			int beginQ = pos;
			if (useQuote) {
				pos = header.IndexOf ('"', pos);
				if (pos == -1)
					return false;
			} else {
				do {
					char c = header [pos];
					if (c == ',' || c == ' ' || c == '\t' || c == '\r' || c == '\n')
						break;
				} while (++pos < length);

				if (pos >= length && beginQ == pos)
					return false;
			}

			value = header.Substring (beginQ, pos - beginQ);
			pos += 2;
			return true;
		}
	}

	class DigestSession
	{
		static RandomNumberGenerator rng;
		DateTime lastUse;
		
		static DigestSession () 
		{
			rng = RandomNumberGenerator.Create ();
		}

		private int _nc;
		private HashAlgorithm hash;
		private DigestHeaderParser parser;
		private string _cnonce;

		public DigestSession () 
		{
			_nc = 1;
			lastUse = DateTime.Now;
		}

		public string Algorithm {
			get { return parser.Algorithm; }
		}

		public string Realm {
			get { return parser.Realm; }
		}

		public string Nonce {
			get { return parser.Nonce; }
		}

		public string Opaque {
			get { return parser.Opaque; }
		}

		public string QOP {
			get { return parser.QOP; }
		}

		public string CNonce {
			get { 
				if (_cnonce == null) {
					// 15 is a multiple of 3 which is better for base64 because it
					// wont end with '=' and risk messing up the server parsing
					byte[] bincnonce = new byte [15];
					rng.GetBytes (bincnonce);
					_cnonce = Convert.ToBase64String (bincnonce);
					Array.Clear (bincnonce, 0, bincnonce.Length);
				}
				return _cnonce;
			}
		}

		public bool Parse (string challenge) 
		{
			parser = new DigestHeaderParser (challenge);
			if (!parser.Parse ()) {
				return false;
			}

			// build the hash object (only MD5 is defined in RFC2617)
			if ((parser.Algorithm == null) || (parser.Algorithm.ToUpper ().StartsWith ("MD5")))
				hash = HashAlgorithm.Create ("MD5");

			return true;
		}

		private string HashToHexString (string toBeHashed) 
		{
			if (hash == null)
				return null;

			hash.Initialize ();
			byte[] result = hash.ComputeHash (Encoding.ASCII.GetBytes (toBeHashed));

			StringBuilder sb = new StringBuilder ();
			foreach (byte b in result)
				sb.Append (b.ToString ("x2"));
			return sb.ToString ();
		}

		private string HA1 (string username, string password) 
		{
			string ha1 = String.Format ("{0}:{1}:{2}", username, Realm, password);
			if (Algorithm != null && Algorithm.ToLower () == "md5-sess")
				ha1 = String.Format ("{0}:{1}:{2}", HashToHexString (ha1), Nonce, CNonce);
			return HashToHexString (ha1);
		}

		private string HA2 (HttpWebRequest webRequest) 
		{
			string ha2 = String.Format ("{0}:{1}", webRequest.Method, webRequest.RequestUri.PathAndQuery);
			if (QOP == "auth-int") {
				// TODO
				// ha2 += String.Format (":{0}", hentity);
			}		
			return HashToHexString (ha2);
		}

		private string Response (string username, string password, HttpWebRequest webRequest) 
		{
			string response = String.Format ("{0}:{1}:", HA1 (username, password), Nonce);
			if (QOP != null)
				response += String.Format ("{0}:{1}:{2}:", _nc.ToString ("X8"), CNonce, QOP);
			response += HA2 (webRequest);
			return HashToHexString (response);
		}

		public Authorization Authenticate (WebRequest webRequest, ICredentials credentials) 
		{
			if (parser == null)
				throw new InvalidOperationException ();

			HttpWebRequest request = webRequest as HttpWebRequest;
			if (request == null)
				return null;
	
			lastUse = DateTime.Now;
			NetworkCredential cred = credentials.GetCredential (request.RequestUri, "digest");
			if (cred == null)
				return null;

			string userName = cred.UserName;
			if (userName == null || userName == "")
				return null;

			string password = cred.Password;
	
			StringBuilder auth = new StringBuilder ();
			auth.AppendFormat ("Digest username=\"{0}\", ", userName);
			auth.AppendFormat ("realm=\"{0}\", ", Realm);
			auth.AppendFormat ("nonce=\"{0}\", ", Nonce);
			auth.AppendFormat ("uri=\"{0}\", ", request.Address.PathAndQuery);

			if (Algorithm != null) { // hash algorithm (only MD5 in RFC2617)
				auth.AppendFormat ("algorithm=\"{0}\", ", Algorithm);
			}

			auth.AppendFormat ("response=\"{0}\", ", Response (userName, password, request));

			if (QOP != null) { // quality of protection (server decision)
				auth.AppendFormat ("qop=\"{0}\", ", QOP);
			}

			lock (this) {
				// _nc MUST NOT change from here...
				// number of request using this nonce
				if (QOP != null) {
					auth.AppendFormat ("nc={0:X8}, ", _nc);
					_nc++;
				}
				// until here, now _nc can change
			}

			if (CNonce != null) // opaque value from the client
				auth.AppendFormat ("cnonce=\"{0}\", ", CNonce);

			if (Opaque != null) // exact same opaque value as received from server
				auth.AppendFormat ("opaque=\"{0}\", ", Opaque);

			auth.Length -= 2; // remove ", "
			return new Authorization (auth.ToString ());
		}

		public DateTime LastUse {
			get { return lastUse; }
		}
	}

	class DigestClient : IAuthenticationModule
	{

		static readonly Hashtable cache = Hashtable.Synchronized (new Hashtable ());
		
		static Hashtable Cache {
			get {
				lock (cache.SyncRoot) {
					CheckExpired (cache.Count);
				}
				
				return cache;
			}
		}

		static void CheckExpired (int count)
		{
			if (count < 10)
				return;

			DateTime t = DateTime.MaxValue;
			DateTime now = DateTime.Now;
			ArrayList list = null;
			foreach (int key in cache.Keys) {
				DigestSession elem = (DigestSession) cache [key];
				if (elem.LastUse < t &&
				    (elem.LastUse - now).Ticks > TimeSpan.TicksPerMinute * 10) {
					t = elem.LastUse;
					if (list == null)
						list = new ArrayList ();

					list.Add (key);
				}
			}

			if (list != null) {
				foreach (int k in list)
					cache.Remove (k);
			}
		}
		
		// IAuthenticationModule
	
		public Authorization Authenticate (string challenge, WebRequest webRequest, ICredentials credentials) 
		{
			if (credentials == null || challenge == null)
				return null;
	
			string header = challenge.Trim ();
			if (header.ToLower ().IndexOf ("digest") == -1)
				return null;

			HttpWebRequest request = webRequest as HttpWebRequest;
			if (request == null)
				return null;

			DigestSession currDS = new DigestSession();
			if (!currDS.Parse (challenge))
				return null;

			int hashcode = request.Address.GetHashCode () ^ credentials.GetHashCode () ^ currDS.Nonce.GetHashCode ();
			DigestSession ds = (DigestSession) Cache [hashcode];
			bool addDS = (ds == null);
			if (addDS)
				ds = currDS;
			else if (!ds.Parse (challenge))
				return null;

			if (addDS)
				Cache.Add (hashcode, ds);

			return ds.Authenticate (webRequest, credentials);
		}

		public Authorization PreAuthenticate (WebRequest webRequest, ICredentials credentials) 
		{
			HttpWebRequest request = webRequest as HttpWebRequest;
			if (request == null)
				return null;

			if (credentials == null)
				return null;

			int hashcode = request.Address.GetHashCode () ^ credentials.GetHashCode ();
			DigestSession ds = (DigestSession) Cache [hashcode];
			if (ds == null)
				return null;

			return ds.Authenticate (webRequest, credentials);
		}
	
		public string AuthenticationType { 
			get { return "Digest"; }
		}
	
		public bool CanPreAuthenticate { 
			get { return true; }
		}
	}
}

