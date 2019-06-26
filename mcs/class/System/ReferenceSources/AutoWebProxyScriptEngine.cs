using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
#if !MONOTOUCH_WATCH
using Mono.Net;
#endif

namespace System.Net
{
	class AutoWebProxyScriptEngine
	{
		public AutoWebProxyScriptEngine (WebProxy proxy, bool useRegistry)
		{
		}

		public Uri AutomaticConfigurationScript { get; set; }
		public bool AutomaticallyDetectSettings { get; set; }

		public bool GetProxies (Uri destination, out IList<string> proxyList)
		{
			int i = 0;
			return GetProxies (destination, out proxyList, ref i);
		}

		public bool GetProxies(Uri destination, out IList<string> proxyList, ref int syncStatus)
		{
			proxyList = null;
			return false;
		}

		public void Close ()
		{
		}

		public void Abort (ref int syncStatus)
		{
		}

		public void CheckForChanges ()
		{
		}

#if !MOBILE
		public WebProxyData GetWebProxyData ()
		{
			WebProxyData data;

			// TODO: Could re-use some pieces from _AutoWebProxyScriptEngine.cs
			if (IsWindows ()) {
				data = InitializeRegistryGlobalProxy ();
				if (data != null)
					return data;
			}

			data = ReadEnvVariables ();
			return data ?? new WebProxyData ();
		}

		WebProxyData ReadEnvVariables ()
		{
			string address = Environment.GetEnvironmentVariable ("http_proxy") ?? Environment.GetEnvironmentVariable ("HTTP_PROXY");

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
					string bypass = Environment.GetEnvironmentVariable ("no_proxy") ?? Environment.GetEnvironmentVariable ("NO_PROXY");
					
					if (bypass != null) {
						string[] bypassList = bypass.Split (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
					
						foreach (string str in bypassList) {
							if (str != "*.local")
								al.Add (str);
							else
								bBypassOnLocal = true;
						}
					}

					return new WebProxyData {
						proxyAddress = uri,
						bypassOnLocal = bBypassOnLocal,
						bypassList = CreateBypassList (al)
					};
				} catch (UriFormatException) {
				}
			}

			return null;
		}

		static bool IsWindows ()
		{
			return (int) Environment.OSVersion.Platform < 4;
		}
				
		WebProxyData InitializeRegistryGlobalProxy ()
		{
			int iProxyEnable = (int)Microsoft.Win32.Registry.GetValue ("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", "ProxyEnable", 0);

			if (iProxyEnable > 0) {
				string strHttpProxy = "";
				bool bBypassOnLocal = false;
				ArrayList al = new ArrayList ();
				
				string strProxyServer = (string)Microsoft.Win32.Registry.GetValue ("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Internet Settings", "ProxyServer", null);
				
				if(strProxyServer == null) {
					return null;
				}

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

				return new WebProxyData {
					proxyAddress = ToUri (strHttpProxy),
					bypassOnLocal = bBypassOnLocal,
					bypassList = CreateBypassList (al)
				};
			}

			return null;
		}

		static Uri ToUri (string address)
		{
			if (address == null)
				return null;
				
			if (address.IndexOf ("://", StringComparison.Ordinal) == -1) 
				address = "http://" + address;

			return new Uri (address);
		}

		// Takes an ArrayList of fileglob-formatted strings and returns an array of Regex-formatted strings
		static ArrayList CreateBypassList (ArrayList al)
		{
			string[] result = al.ToArray (typeof (string)) as string[];
			for (int c = 0; c < result.Length; c++)
			{
				result [c] = "^" +
					Regex.Escape (result [c]).Replace (@"\*", ".*").Replace (@"\?", ".") +
					"$";
			}
			return new ArrayList (result);
		}
#endif
	}
}