//
// System.Net.WebRequest
//
// Author:
//   Lawrence Pit (loz@cable.a2000.nl)
//

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.Serialization;

namespace System.Net 
{
	[Serializable]
	public abstract class WebRequest : MarshalByRefObject, ISerializable
	{
		private static HybridDictionary prefixes;
		
		static WebRequest () {
			prefixes = new HybridDictionary (3, true);
			RegisterPrefix ("file", new FileWebRequestCreator ());
			RegisterPrefix ("http", new HttpWebRequestCreator ());
			RegisterPrefix ("https", new HttpWebRequestCreator ());
		}
		
		internal class HttpWebRequestCreator : IWebRequestCreate
		{
			internal HttpWebRequestCreator () { }
			
			public WebRequest Create (Uri uri) 
			{
				return new HttpWebRequest (uri);
			}
		}

		internal class FileWebRequestCreator : IWebRequestCreate
		{
			internal FileWebRequestCreator () { }
			
			public WebRequest Create (Uri uri) 
			{
				return new FileWebRequest (uri);
			}
		}

		
		// Constructors
		
		protected WebRequest () { }		
		
		protected WebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
			throw new NotSupportedException ();
		}
		
		// Properties
		
		public virtual string ConnectionGroupName { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual long ContentLength { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual string ContentType { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual ICredentials Credentials { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual WebHeaderCollection Headers { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual string Method { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual bool PreAuthenticate { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual IWebProxy Proxy { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual Uri RequestUri { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		public virtual int Timeout { 
			get { throw new NotSupportedException (); }
			set { throw new NotSupportedException (); }
		}
		
		// Methods
		
		public virtual void Abort()
		{
			throw new NotSupportedException ();
		}
		
		public virtual IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) 
		{
			throw new NotSupportedException ();
		}
		
		public virtual IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			throw new NotSupportedException ();
		}

		public static WebRequest Create (string requestUriString) 
		{
			if (requestUriString == null)
				throw new ArgumentNullException ("requestUriString");
			return Create (new Uri (requestUriString));
		}
				
		public static WebRequest Create (Uri requestUri) 
		{
			if (requestUri == null)
				throw new ArgumentNullException ("requestUri");
			return GetCreator (requestUri.AbsoluteUri).Create (requestUri);
		}
		
		public static WebRequest CreateDefault (Uri requestUri)
		{
			if (requestUri == null)
				throw new ArgumentNullException ("requestUri");
			return GetCreator (requestUri.Scheme).Create (requestUri);
		}

		public virtual Stream EndGetRequestStream (IAsyncResult asyncResult)
		{
			throw new NotSupportedException ();
		}
		
		public virtual WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			throw new NotSupportedException ();
		}
		
		public virtual Stream GetRequestStream()
		{
			throw new NotSupportedException ();
		}
		
		public virtual WebResponse GetResponse()
		{
			throw new NotSupportedException ();
		}
		
		void ISerializable.GetObjectData (SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			throw new NotSupportedException ();
		}

		public static bool RegisterPrefix (string prefix, IWebRequestCreate creator)
		{
			if (prefix == null)
				throw new ArgumentNullException("prefix");
			if (creator == null)
				throw new ArgumentNullException("creator");			
			
			lock (prefixes.SyncRoot) {
				if (prefixes.Contains (prefix))
					return false;
				prefixes.Add (prefix.ToLower (), creator);
			}
			return true;
		}
		
		private static IWebRequestCreate GetCreator (string prefix)
		{
			int longestPrefix = -1;
			IWebRequestCreate creator = null;

			prefix = prefix.ToLower ();

			IDictionaryEnumerator e = prefixes.GetEnumerator ();
			while (e.MoveNext ()) {
				string key = e.Key as string;

				if (key.Length <= longestPrefix) 
					continue;
				
				if (!prefix.StartsWith (key))
					continue;					
					
				longestPrefix = key.Length;
				creator = (IWebRequestCreate) e.Value;
			}
			
			if (creator == null) 
				throw new NotSupportedException (prefix);
				
			return creator;
		}
	}
}