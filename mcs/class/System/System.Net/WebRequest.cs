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
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Mono.Net;

#if NET_2_1
using ConfigurationException = System.ArgumentException;

namespace System.Net.Configuration {
	class Dummy {}
}
#endif

namespace System.Net 
{
	public abstract partial class WebRequest : MarshalByRefObject, ISerializable {
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
				foreach (WebRequestModuleElement el in s.WebRequestModules)
					RegisterPrefix (el.Prefix, (IWebRequestCreate) Activator.CreateInstance (el.Type, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, null));
				return;
			}
	#endif
			ConfigurationSettings.GetConfig ("system.net/webRequestModules");
#endif
		}

		// Properties

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
				throw new NotImplementedException ("This method must be implemented in derived classes");
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

		internal static IWebProxy InternalDefaultWebProxy {
			get {
				return DefaultWebProxy;
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

		// Takes an ArrayList of fileglob-formatted strings and returns an array of Regex-formatted strings
		private static string[] CreateBypassList (ArrayList al)
		{
			string[] result = al.ToArray (typeof (string)) as string[];
			for (int c = 0; c < result.Length; c++)
			{
				result [c] = "^" +
					Regex.Escape (result [c]).Replace (@"\*", ".*").Replace (@"\?", ".") +
					"$";
			}
			return result;
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
			if ((int) Environment.OSVersion.Platform < 4 /* IsWindows */) {
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
					
					return new WebProxy (strHttpProxy, bBypassOnLocal, CreateBypassList (al));
				}
			} else
#endif
			{
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
						
						return new WebProxy (uri, bBypassOnLocal, CreateBypassList (al));
					} catch (UriFormatException) {
					}
				}
			}
			
			return new WebProxy ();
#endif // MONOTOUCH
		}

		internal static ArrayList PrefixList
		{
			get {
				if (s_PrefixList == null) {
					lock (InternalSyncObject) {
						if (s_PrefixList == null)
							s_PrefixList = new ArrayList ();
					}
				}

				return s_PrefixList;
			}
			set {
				s_PrefixList = value;
			}
		}

	}
}
