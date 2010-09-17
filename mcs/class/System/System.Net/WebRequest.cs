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
using System.Globalization;
#if NET_2_0
using System.Net.Configuration;
using System.Net.Security;
using System.Net.Cache;
using System.Security.Principal;
#endif

#if NET_2_1
using ConfigurationException = System.ArgumentException;

namespace System.Net.Configuration {
	class Dummy {}
}
#endif

namespace System.Net 
{
#if MOONLIGHT
	internal abstract class WebRequest : ISerializable {
#else
	[Serializable]
	public abstract class WebRequest : MarshalByRefObject, ISerializable {
#endif
		static HybridDictionary prefixes = new HybridDictionary ();
#if NET_2_0
		static bool isDefaultWebProxySet;
		static IWebProxy defaultWebProxy;
		static RequestCachePolicy defaultCachePolicy;
#endif
		
		// Constructors
		
		static WebRequest ()
		{
#if NET_2_1
			AddPrefix ("http", typeof (HttpRequestCreator));
			AddPrefix ("https", typeof (HttpRequestCreator));
	#if MONOTOUCH
			AddPrefix ("file", typeof (FileWebRequestCreator));
			AddPrefix ("ftp", typeof (FtpRequestCreator));
	#endif
#else
	#if NET_2_0
			defaultCachePolicy = new HttpRequestCachePolicy (HttpRequestCacheLevel.NoCacheNoStore);
	#endif
	#if NET_2_0 && CONFIGURATION_DEP
			object cfg = ConfigurationManager.GetSection ("system.net/webRequestModules");
			WebRequestModulesSection s = cfg as WebRequestModulesSection;
			if (s != null) {
				foreach (WebRequestModuleElement el in
					 s.WebRequestModules)
					AddPrefix (el.Prefix, el.Type);
				return;
			}
	#endif
			ConfigurationSettings.GetConfig ("system.net/webRequestModules");
#endif
		}
		
		protected WebRequest () 
		{
		}
		
		protected WebRequest (SerializationInfo serializationInfo, StreamingContext streamingContext) 
		{
#if ONLY_1_1
			throw GetMustImplement ();
#endif
		}

		static Exception GetMustImplement ()
		{
			return new NotImplementedException ("This method must be implemented in derived classes");
		}
		
		// Properties

#if NET_2_0
		private AuthenticationLevel authentication_level = AuthenticationLevel.MutualAuthRequested;
		
		public AuthenticationLevel AuthenticationLevel
		{
			get {
				return(authentication_level);
			}
			set {
				authentication_level = value;
			}
		}

		[MonoTODO ("Implement the caching system. Currently always returns a policy with the NoCacheNoStore level")]
		public virtual RequestCachePolicy CachePolicy
		{
			get { return DefaultCachePolicy; }
			set {
			}
		}
#endif
		
		public virtual string ConnectionGroupName {
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public virtual long ContentLength { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public virtual string ContentType { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public virtual ICredentials Credentials { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}

#if NET_2_0
		public static RequestCachePolicy DefaultCachePolicy
		{
			get { return defaultCachePolicy; }
			set {
				throw GetMustImplement ();
			}
		}
#endif
		
		public virtual WebHeaderCollection Headers { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
#if NET_2_0 && !MOONLIGHT
		public TokenImpersonationLevel ImpersonationLevel {
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
#endif
		public virtual string Method { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public virtual bool PreAuthenticate { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public virtual IWebProxy Proxy { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public virtual Uri RequestUri { 
			get { throw GetMustImplement (); }
		}
		
		public virtual int Timeout { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
#if NET_2_0
		public virtual bool UseDefaultCredentials
		{
			get {
				throw GetMustImplement ();
			}
			set {
				throw GetMustImplement ();
			}
		}
		
//		volatile static IWebProxy proxy;
		static readonly object lockobj = new object ();
		
		public static IWebProxy DefaultWebProxy {
			get {
				if (!isDefaultWebProxySet) {
					lock (lockobj) {
						if (defaultWebProxy == null)
							defaultWebProxy = GetDefaultWebProxy ();
					}
				}
				return defaultWebProxy;
			}
			set {
				/* MS documentation states that a null value would cause an ArgumentNullException
				 * but that's not the way it behaves:
				 * https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=304724
				 */
				defaultWebProxy = value;
				isDefaultWebProxySet = true;
			}
		}
		
		[MonoTODO("Needs to respect Module, Proxy.AutoDetect, and Proxy.ScriptLocation config settings")]
		static IWebProxy GetDefaultWebProxy ()
		{
			WebProxy p = null;
			
#if CONFIGURATION_DEP
			DefaultProxySection sec = ConfigurationManager.GetSection ("system.net/defaultProxy") as DefaultProxySection;
			if (sec == null)
				return GetSystemWebProxy ();
			
			ProxyElement pe = sec.Proxy;
			
			if ((pe.UseSystemDefault != ProxyElement.UseSystemDefaultValues.False) && (pe.ProxyAddress == null))
				p = (WebProxy) GetSystemWebProxy ();
			else
				p = new WebProxy ();
			
			if (pe.ProxyAddress != null)
				p.Address = pe.ProxyAddress;
			
			if (pe.BypassOnLocal != ProxyElement.BypassOnLocalValues.Unspecified)
				p.BypassProxyOnLocal = (pe.BypassOnLocal == ProxyElement.BypassOnLocalValues.True);
#endif
			return p;
		}
#endif

		// Methods
		
		public virtual void Abort()
		{
			throw GetMustImplement ();
		}
		
		public virtual IAsyncResult BeginGetRequestStream (AsyncCallback callback, object state) 
		{
			throw GetMustImplement ();
		}
		
		public virtual IAsyncResult BeginGetResponse (AsyncCallback callback, object state)
		{
			throw GetMustImplement ();
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
			throw GetMustImplement ();
		}
		
		public virtual WebResponse EndGetResponse (IAsyncResult asyncResult)
		{
			throw GetMustImplement ();
		}
		
		public virtual Stream GetRequestStream()
		{
			throw GetMustImplement ();
		}
		
		public virtual WebResponse GetResponse()
		{
			throw GetMustImplement ();
		}
		
#if NET_2_0
		[MonoTODO("Look in other places for proxy config info")]
		public static IWebProxy GetSystemWebProxy ()
		{
			string address = Environment.GetEnvironmentVariable ("http_proxy");
			if (address == null)
				address = Environment.GetEnvironmentVariable ("HTTP_PROXY");

			if (address != null) {
				try {
					if (!address.StartsWith ("http://"))
						address = "http://" + address;
					Uri uri = new Uri (address);
					IPAddress ip;
					if (IPAddress.TryParse (uri.Host, out ip)) {
						if (IPAddress.Any.Equals (ip)) {
							UriBuilder builder = new UriBuilder (uri);
							builder.Host = "127.0.0.1";
							uri = builder.Uri;
						} else if (IPAddress.IPv6Any.Equals (ip)) {
							UriBuilder builder = new UriBuilder (uri);
							builder.Host = "[::1]";
							uri = builder.Uri;
						}
					}
					return new WebProxy (uri);
				} catch (UriFormatException) { }
			}
			return new WebProxy ();
		}
#endif

		void ISerializable.GetObjectData
		(SerializationInfo serializationInfo,
		   				  StreamingContext streamingContext)
		{
			throw new NotSupportedException ();
		}


#if NET_2_0
		protected virtual void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw GetMustImplement ();
		}
#endif

		public static bool RegisterPrefix (string prefix, IWebRequestCreate creator)
		{
			if (prefix == null)
				throw new ArgumentNullException ("prefix");
			if (creator == null)
				throw new ArgumentNullException ("creator");
			
			lock (prefixes.SyncRoot) {
				string lowerCasePrefix = prefix.ToLower (CultureInfo.InvariantCulture);
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

			prefix = prefix.ToLower (CultureInfo.InvariantCulture);

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
			AddPrefix (prefix, type);
		}

		internal static void AddPrefix (string prefix, Type type)
		{
			object o = Activator.CreateInstance (type, true);
			prefixes [prefix] = o;
		}
	}
}

