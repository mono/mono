//
// System.Net.WebRequest
//
// Authors:
//  Lawrence Pit (loz@cable.a2000.nl)
//	Marek Safar (marek.safar@gmail.com)
//
// Copyright 2011 Xamarin Inc.
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
using System.Reflection;
using System.Runtime.Serialization;
using System.Globalization;
using System.Net.Configuration;
using System.Net.Security;
using System.Net.Cache;
using System.Security.Principal;
#if NET_4_5
using System.Threading.Tasks;
#endif

#if NET_2_1
using ConfigurationException = System.ArgumentException;

namespace System.Net.Configuration {
	class Dummy {}
}
#endif

namespace System.Net 
{
	[Serializable]
	public abstract class WebRequest : MarshalByRefObject, ISerializable {
		static HybridDictionary prefixes = new HybridDictionary ();
		static bool isDefaultWebProxySet;
		static IWebProxy defaultWebProxy;
		static RequestCachePolicy defaultCachePolicy;

		static WebRequest ()
		{
#if MOBILE
			IWebRequestCreate http = new HttpRequestCreator ();
			RegisterPrefix ("http", http);
			RegisterPrefix ("https", http);
			RegisterPrefix ("file", new FileWebRequestCreator ());
			RegisterPrefix ("ftp", new FtpRequestCreator ());
#else
	#if CONFIGURATION_DEP
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
		}

		static Exception GetMustImplement ()
		{
			return new NotImplementedException ("This method must be implemented in derived classes");
		}
		
		// Properties

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

		[MonoTODO ("Implement the caching system. Currently always returns a policy with the NoCacheNoStore level")]
		public virtual RequestCachePolicy CachePolicy
		{
			get { return DefaultCachePolicy; }
			set {
			}
		}
		
		public static RequestCachePolicy DefaultCachePolicy {
			get {
				return defaultCachePolicy ?? (defaultCachePolicy = new HttpRequestCachePolicy (HttpRequestCacheLevel.NoCacheNoStore));
			}
			set {
				throw GetMustImplement ();
			}
		}
		
		public virtual WebHeaderCollection Headers { 
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}
		
		public TokenImpersonationLevel ImpersonationLevel {
			get { throw GetMustImplement (); }
			set { throw GetMustImplement (); }
		}

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
#if CONFIGURATION_DEP
			DefaultProxySection sec = ConfigurationManager.GetSection ("system.net/defaultProxy") as DefaultProxySection;
			WebProxy p;
			
			if (sec == null)
				return GetSystemWebProxy ();
			
			ProxyElement pe = sec.Proxy;
			
			if ((pe.UseSystemDefault != ProxyElement.UseSystemDefaultValues.False) && (pe.ProxyAddress == null)) {
				IWebProxy proxy = GetSystemWebProxy ();
				
				if (!(proxy is WebProxy))
					return proxy;
				
				p = (WebProxy) proxy;
			} else
				p = new WebProxy ();
			
			if (pe.ProxyAddress != null)
				p.Address = pe.ProxyAddress;
			
			if (pe.BypassOnLocal != ProxyElement.BypassOnLocalValues.Unspecified)
				p.BypassProxyOnLocal = (pe.BypassOnLocal == ProxyElement.BypassOnLocalValues.True);
				
			foreach(BypassElement elem in sec.BypassList)
				p.BypassArrayList.Add(elem.Address);
			
			return p;
#else
			return GetSystemWebProxy ();
#endif
		}

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
#if NET_4_0
		[MonoTODO ("for portable library support")]
		public static HttpWebRequest CreateHttp (string requestUriString)
		{
			throw new NotImplementedException ();
		}
			
		[MonoTODO ("for portable library support")]
		public static HttpWebRequest CreateHttp (Uri requestUri)
		{
			throw new NotImplementedException ();
		}
#endif
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
		
		[MonoTODO("Look in other places for proxy config info")]
		public static IWebProxy GetSystemWebProxy ()
		{
#if MONOTOUCH
			return CFNetwork.GetDefaultProxy ();
#else
#if MONODROID
			// Return the system web proxy.  This only works for ICS+.
			var androidProxy = AndroidPlatform.GetDefaultProxy ();
			if (androidProxy != null)
				return androidProxy;
#endif
#if !NET_2_1
			if (IsWindows ()) {
				int iProxyEnable = (int)Microsoft.Win32.Registry.GetValue ("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", "ProxyEnable", 0);

				if (iProxyEnable > 0) {
					string strHttpProxy = "";					
					bool bBypassOnLocal = false;
					ArrayList al = new ArrayList ();
					
					string strProxyServer = (string)Microsoft.Win32.Registry.GetValue ("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", "ProxyServer", null);
					string strProxyOverrride = (string)Microsoft.Win32.Registry.GetValue ("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", "ProxyOverride", null);
					
					if (strProxyServer.Contains ("=")) {
						foreach (string strEntry in strProxyServer.Split (new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
							if (strEntry.StartsWith ("http=")) {
								strHttpProxy = strEntry.Substring (5);
								break;
							}
					} else strHttpProxy = strProxyServer;
					
					if (strProxyOverrride != null) {						
						string[] bypassList = strProxyOverrride.Split (new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
					
						foreach (string str in bypassList) {
							if (str != "<local>")
								al.Add (str);
							else
								bBypassOnLocal = true;
						}
					}
					
					return new WebProxy (strHttpProxy, bBypassOnLocal, al.ToArray (typeof(string)) as string[]);
				}
			} else {
#endif
				if (Platform.IsMacOS)
					return CFNetwork.GetDefaultProxy ();
				
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
						
						bool bBypassOnLocal = false;						
						ArrayList al = new ArrayList ();
						string bypass = Environment.GetEnvironmentVariable ("no_proxy");
						
						if (bypass == null)
							bypass = Environment.GetEnvironmentVariable ("NO_PROXY");
						
						if (bypass != null) {
							string[] bypassList = bypass.Split (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
						
							foreach (string str in bypassList) {
								if (str != "*.local")
									al.Add (str);
								else
									bBypassOnLocal = true;
							}
						}
						
						return new WebProxy (uri, bBypassOnLocal, al.ToArray (typeof(string)) as string[]);
					} catch (UriFormatException) {
					}
				}
#if !NET_2_1
			}
#endif
			
			return new WebProxy ();
#endif // MONOTOUCH
		}

		void ISerializable.GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw new NotSupportedException ();
		}

		protected virtual void GetObjectData (SerializationInfo serializationInfo, StreamingContext streamingContext)
		{
			throw GetMustImplement ();
		}

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
		
		internal static bool IsWindows ()
		{
			return (int) Environment.OSVersion.Platform < 4;
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

#if NET_4_5
		public virtual Task<Stream> GetRequestStreamAsync ()
		{
			return Task<Stream>.Factory.FromAsync (BeginGetRequestStream, EndGetRequestStream, null);
		}

		public virtual Task<WebResponse> GetResponseAsync ()
		{
			return Task<WebResponse>.Factory.FromAsync (BeginGetResponse, EndGetResponse, null);
		}
#endif

	}
}
