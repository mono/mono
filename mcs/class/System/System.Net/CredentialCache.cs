//
// System.Net.CredentialCache.cs
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Collections;
using System.Runtime.Serialization;

namespace System.Net 
{
	public class CredentialCache : ICredentials, IEnumerable
	{
		// Fields
		private Hashtable cache;
		
		// Constructors		
		public CredentialCache () 
		{
			cache = new Hashtable ();
		}
		
		// Properties
		
		[MonoTODO ("Need EnvironmentPermission implementation first")]
		public static ICredentials DefaultCredentials {
			get {
				throw new NotImplementedException ();
			}
		}
		

		// ICredentials

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

		// IEnumerable

		public IEnumerator GetEnumerator ()
		{
			return cache.Values.GetEnumerator ();
		}		
		
		// Methods
		
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
		
		// Inner Classes
		
		internal class CredentialCacheKey
		{
			private Uri uriPrefix;
			private string authType;
			
			private string absPath;
			private int len;
			private int hash;
			
			internal CredentialCacheKey (Uri uriPrefix, string authType)
			{
				this.uriPrefix = uriPrefix;
				this.authType = authType;
				
				this.absPath = uriPrefix.AbsolutePath;
				this.absPath = absPath.Substring (0, absPath.LastIndexOf ('/'));				

				this.len = uriPrefix.AbsoluteUri.Length;
				this.hash = uriPrefix.GetHashCode () 
				          + authType.ToString ().GetHashCode ();
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
	} 
} 

