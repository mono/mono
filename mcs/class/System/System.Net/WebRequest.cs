//
// System.Net.WebRequest
//
// Authors:
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
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization;

namespace System.Net 
{
	[Serializable]
	public abstract class WebRequest : MarshalByRefObject, ISerializable
	{
		static HybridDictionary prefixes = new HybridDictionary ();
		
		// Constructors
		
		static WebRequest ()
		{
			ConfigurationSettings.GetConfig ("system.net/webRequestModules");
		}
		
		protected WebRequest () 
		{
		}
		
		protected WebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
		}
		
		// Properties
		
		public virtual string ConnectionGroupName { 
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public virtual long ContentLength { 
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public virtual string ContentType { 
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public virtual ICredentials Credentials { 
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public virtual WebHeaderCollection Headers { 
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public virtual string Method { 
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public virtual bool PreAuthenticate { 
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public virtual IWebProxy Proxy { 
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		public virtual Uri RequestUri { 
			get { throw new NotImplementedException (); }
		}
		
		public virtual int Timeout { 
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		// Methods
		
		public virtual void Abort()
		{
			throw new NotImplementedException ();
		}
		
		public virtual IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) 
		{
			throw new NotImplementedException ();
		}
		
		public virtual IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}
		
		public virtual WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}
		
		public virtual Stream GetRequestStream()
		{
			throw new NotImplementedException ();
		}
		
		public virtual WebResponse GetResponse()
		{
			throw new NotImplementedException ();
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
				string lowerCasePrefix = prefix.ToLower ();
				if (prefixes.Contains (lowerCasePrefix))
					return false;
				prefixes.Add (lowerCasePrefix, creator);
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

		internal static void ClearPrefixes ()
		{
			prefixes.Clear ();
		}

		internal static void RemovePrefix (string prefix)
		{
			prefixes.Remove (prefix);
		}

		internal static void AddPrefix (string prefix, string typeName)
		{
			Type type = Type.GetType (typeName);
			if (type == null)
				throw new ConfigurationException (String.Format ("Type {0} not found", typeName));

			object o = Activator.CreateInstance (type, true);
			prefixes [prefix] = o;
		}
	}
}

