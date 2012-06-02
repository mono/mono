//
// System.Net.CredentialCache.cs
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
using System.Collections;
using System.Runtime.Serialization;

namespace System.Net {
	public class CredentialCache : ICredentials, IEnumerable, ICredentialsByHost
	{
		static NetworkCredential empty = new NetworkCredential (String.Empty, String.Empty, String.Empty);
		Hashtable cache;
		Hashtable cacheForHost;

		public CredentialCache () 
		{
			cache = new Hashtable ();
			cacheForHost = new Hashtable ();
		}
		
		[MonoTODO ("Need EnvironmentPermission implementation first")]
		public static ICredentials DefaultCredentials {
			get {
				// This is used for NTLM, Kerberos and Negotiate under MS
				return empty;
			}
		}

		// MS does might return a special ICredentials which does not allow getting the
		// username/password information out of it for non-internal classes.
		public static NetworkCredential DefaultNetworkCredentials {
			get { return empty; }
		}

		public NetworkCredential GetCredential (Uri uriPrefix, string authType)
		{
			int longestPrefix = -1;
			NetworkCredential result = null;
			
			if (uriPrefix == null || authType == null)
				return null;
				
			string absPath = uriPrefix.AbsolutePath;
			absPath = absPath.Substring (0, absPath.LastIndexOf ('/'));
			
			IDictionaryEnumerator e = cache.GetEnumerator ();
			while (e.MoveNext ()) {
				CredentialCacheKey key = e.Key as CredentialCacheKey;
				
				if (key.Length <= longestPrefix) 
					continue;
				
				if (String.Compare (key.AuthType, authType, true) != 0)
					continue;
				
				Uri cachedUri = key.UriPrefix;
				
				if (cachedUri.Scheme != uriPrefix.Scheme)
					continue;
					
				if (cachedUri.Port != uriPrefix.Port)
					continue;
					
				if (cachedUri.Host != uriPrefix.Host)
					continue;
								
				if (!absPath.StartsWith (key.AbsPath))
					continue;
					
				longestPrefix = key.Length;
				result = (NetworkCredential) e.Value;
			}
			
			return result;
		}

		public IEnumerator GetEnumerator ()
		{
			return cache.Values.GetEnumerator ();
		}		
		
		public void Add (Uri uriPrefix, string authType, NetworkCredential cred)
		{
			if (uriPrefix == null) 
				throw new ArgumentNullException ("uriPrefix");

			if (authType == null) 
				throw new ArgumentNullException ("authType");
			
			// throws ArgumentException when same key already exists.
			cache.Add (new CredentialCacheKey (uriPrefix, authType), cred);
		}
		
		public void Remove (Uri uriPrefix, string authType)
		{
			if (uriPrefix == null) 
				throw new ArgumentNullException ("uriPrefix");

			if (authType == null) 
				throw new ArgumentNullException ("authType");

			cache.Remove (new CredentialCacheKey (uriPrefix, authType));
		}
		
		public NetworkCredential GetCredential (string host, int port, string authenticationType)
		{
			NetworkCredential result = null;
			
			if (host == null || port < 0 || authenticationType == null)
				return null;

			IDictionaryEnumerator e = cacheForHost.GetEnumerator ();
			while (e.MoveNext ()) {
				CredentialCacheForHostKey key = e.Key as CredentialCacheForHostKey;
				
				if (String.Compare (key.AuthType, authenticationType, true) != 0)
					continue;
				
				if (key.Host != host)
					continue;
				
				if (key.Port != port)
					continue;
				
				result = (NetworkCredential) e.Value;
			}
			
			return result;
		}

		public void Add (string host, int port, string authenticationType, NetworkCredential credential)
		{
			if (host == null)
				throw new ArgumentNullException("host");

			if (port < 0)
				throw new ArgumentOutOfRangeException("port");

			if (authenticationType == null)
				throw new ArgumentOutOfRangeException("authenticationType");

			cacheForHost.Add (new CredentialCacheForHostKey (host, port, authenticationType), credential);
		}

		public void Remove (string host, int port, string authenticationType)
		{
			if (host == null)
				return;

			if (authenticationType == null)
				return;

			cacheForHost.Remove (new CredentialCacheForHostKey (host, port, authenticationType));
		}

		class CredentialCacheKey {
			Uri uriPrefix;
			string authType;
			string absPath;
			int len;
			int hash;
			
			internal CredentialCacheKey (Uri uriPrefix, string authType)
			{
				this.uriPrefix = uriPrefix;
				this.authType = authType;
				
				this.absPath = uriPrefix.AbsolutePath;
				this.absPath = absPath.Substring (0, absPath.LastIndexOf ('/'));				

				this.len = uriPrefix.AbsoluteUri.Length;
				this.hash = uriPrefix.GetHashCode () 
				          + authType.GetHashCode ();
			}
			
			public int Length {
				get { return len; }
			}			
			
			public string AbsPath {
				get { return absPath; }
			}
			
			public Uri UriPrefix {
				get { return uriPrefix; }
			}
			
			public string AuthType {
				get { return authType; }
			}
			
			public override int GetHashCode ()
			{
				return hash;
			}
			
			public override bool Equals (object obj)
			{
				CredentialCacheKey key = obj as CredentialCacheKey;
				return ((key != null) && (this.hash == key.hash));
			}
			
			public override string ToString ()
			{
				return absPath + " : " + authType + " : len=" + len;
			}
		}

		class CredentialCacheForHostKey {
			string host;
			int port;
			string authType;
			int hash;

			internal CredentialCacheForHostKey (string host, int port, string authType)
			{
				this.host = host;
				this.port = port;
				this.authType = authType;

				this.hash = host.GetHashCode ()
					+ port.GetHashCode ()
					+ authType.GetHashCode ();
			}

			public string Host {
				get { return host; }
			}

			public int Port {
				get { return port; }
			}

			public string AuthType {
				get { return authType; }
			}

		public override int GetHashCode ()
		{
			return hash;
		}

		public override bool Equals (object obj)
		{
			CredentialCacheForHostKey key = obj as CredentialCacheForHostKey;
			return ((key != null) && (this.hash == key.hash));
		}

		public override string ToString ()
		{
			return host + " : " + authType;
		}
	}
}
}


