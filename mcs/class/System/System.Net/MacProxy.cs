// 
// MacProxy.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 

using System;
using System.Runtime.InteropServices;

namespace System.Net
{
	internal class CFObject : IDisposable
	{
		public const string CoreFoundationLibrary = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";
		const string SystemLibrary = "/usr/lib/libSystem.dylib";

		[DllImport (SystemLibrary)]
		public static extern IntPtr dlopen (string path, int mode);

		[DllImport (SystemLibrary)]
		public static extern IntPtr dlsym (IntPtr handle, string symbol);

		[DllImport (SystemLibrary)]
		public static extern void dlclose (IntPtr handle);

		public static IntPtr GetIndirect (IntPtr handle, string symbol)
		{
			return dlsym (handle, symbol);
		}

		public static IntPtr GetCFObjectHandle (IntPtr handle, string symbol)
		{
			var indirect = dlsym (handle, symbol);
			if (indirect == IntPtr.Zero)
				return IntPtr.Zero;

			return Marshal.ReadIntPtr (indirect);
		}

		public CFObject (IntPtr handle, bool own)
		{
			Handle = handle;

			if (!own)
				Retain ();
		}

		~CFObject ()
		{
			Dispose (false);
		}

		public IntPtr Handle { get; private set; }

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFRetain (IntPtr handle);

		void Retain ()
		{
			CFRetain (Handle);
		}

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFRelease (IntPtr handle);

		void Release ()
		{
			CFRelease (Handle);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				Release ();
				Handle = IntPtr.Zero;
			}
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
	}

	internal class CFArray : CFObject
	{
		public CFArray (IntPtr handle, bool own) : base (handle, own) { }

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFArrayCreate (IntPtr allocator, IntPtr values, int numValues, IntPtr callbacks);
		static readonly IntPtr kCFTypeArrayCallbacks;

		static CFArray ()
		{
			var handle = dlopen (CoreFoundationLibrary, 0);
			if (handle == IntPtr.Zero)
				return;

			try {
				kCFTypeArrayCallbacks = GetIndirect (handle, "kCFTypeArrayCallBacks");
			} finally {
				dlclose (handle);
			}
		}

		static unsafe CFArray Create (params IntPtr[] values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");

			fixed (IntPtr *pv = values) {
				IntPtr handle = CFArrayCreate (IntPtr.Zero, (IntPtr) pv, values.Length, kCFTypeArrayCallbacks);

				return new CFArray (handle, false);
			}
		}

		public static CFArray Create (params CFObject[] values)
		{
			if (values == null)
				throw new ArgumentNullException ("values");

			IntPtr[] _values = new IntPtr [values.Length];
			for (int i = 0; i < _values.Length; i++)
				_values[i] = values[i].Handle;

			return Create (_values);
		}

		[DllImport (CoreFoundationLibrary)]
		extern static int CFArrayGetCount (IntPtr handle);

		public int Count {
			get { return CFArrayGetCount (Handle); }
		}

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFArrayGetValueAtIndex (IntPtr handle, int index);

		public IntPtr this[int index] {
			get {
				return CFArrayGetValueAtIndex (Handle, index);
			}
		}
	}

	internal class CFNumber : CFObject
	{
		public CFNumber (IntPtr handle, bool own) : base (handle, own) { }

		[DllImport (CoreFoundationLibrary)]
		extern static bool CFNumberGetValue (IntPtr handle, int type, out bool value);

		public static bool AsBool (IntPtr handle)
		{
			bool value;

			if (handle == IntPtr.Zero)
				return false;

			CFNumberGetValue (handle, 1, out value);

			return value;
		}

		public static implicit operator bool (CFNumber number)
		{
			return AsBool (number.Handle);
		}

		[DllImport (CoreFoundationLibrary)]
		extern static bool CFNumberGetValue (IntPtr handle, int type, out int value);

		public static int AsInt32 (IntPtr handle)
		{
			int value;

			if (handle == IntPtr.Zero)
				return 0;

			CFNumberGetValue (handle, 9, out value);

			return value;
		}

		public static implicit operator int (CFNumber number)
		{
			return AsInt32 (number.Handle);
		}
	}

	internal struct CFRange {
		public int Location, Length;
		
		public CFRange (int loc, int len)
		{
			Location = loc;
			Length = len;
		}
	}

	internal class CFString : CFObject
	{
		string str;

		public CFString (IntPtr handle, bool own) : base (handle, own) { }

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFStringCreateWithCharacters (IntPtr alloc, IntPtr chars, int length);

		public static CFString Create (string value)
		{
			IntPtr handle;

			unsafe {
				fixed (char *ptr = value) {
					handle = CFStringCreateWithCharacters (IntPtr.Zero, (IntPtr) ptr, value.Length);
				}
			}

			if (handle == IntPtr.Zero)
				return null;

			return new CFString (handle, true);
		}

		[DllImport (CoreFoundationLibrary)]
		extern static int CFStringGetLength (IntPtr handle);

		public int Length {
			get {
				if (str != null)
					return str.Length;

				return CFStringGetLength (Handle);
			}
		}

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFStringGetCharactersPtr (IntPtr handle);

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFStringGetCharacters (IntPtr handle, CFRange range, IntPtr buffer);

		public static string AsString (IntPtr handle)
		{
			if (handle == IntPtr.Zero)
				return null;
			
			int len = CFStringGetLength (handle);
			
			if (len == 0)
				return string.Empty;
			
			IntPtr chars = CFStringGetCharactersPtr (handle);
			IntPtr buffer = IntPtr.Zero;
			
			if (chars == IntPtr.Zero) {
				CFRange range = new CFRange (0, len);
				buffer = Marshal.AllocHGlobal (len * 2);
				CFStringGetCharacters (handle, range, buffer);
				chars = buffer;
			}

			string str;

			unsafe {
				str = new string ((char *) chars, 0, len);
			}
			
			if (buffer != IntPtr.Zero)
				Marshal.FreeHGlobal (buffer);

			return str;
		}

		public override string ToString ()
		{
			if (str == null)
				str = AsString (Handle);

			return str;
		}

		public static implicit operator string (CFString str)
		{
			return str.ToString ();
		}

		public static implicit operator CFString (string str)
		{
			return Create (str);
		}
	}

	internal class CFDictionary : CFObject
	{
		public CFDictionary (IntPtr handle, bool own) : base (handle, own) { }

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFDictionaryGetValue (IntPtr handle, IntPtr key);

		public IntPtr GetValue (IntPtr key)
		{
			return CFDictionaryGetValue (Handle, key);
		}

		public IntPtr this[IntPtr key] {
			get {
				return GetValue (key);
			}
		}
	}

	internal class CFUrl : CFObject
	{
		public CFUrl (IntPtr handle, bool own) : base (handle, own) { }

		[DllImport (CoreFoundationLibrary)]
		extern static IntPtr CFURLCreateWithString (IntPtr allocator, IntPtr str, IntPtr baseURL);

		public static CFUrl Create (string absolute)
		{
			if (string.IsNullOrEmpty (absolute))
				return null;

			CFString str = CFString.Create (absolute);
			IntPtr handle = CFURLCreateWithString (IntPtr.Zero, str.Handle, IntPtr.Zero);
			str.Dispose ();

			if (handle == IntPtr.Zero)
				return null;

			return new CFUrl (handle, true);
		}
	}

	internal enum CFProxyType {
		None,
		AutoConfigurationUrl,
		AutoConfigurationJavaScript,
		FTP,
		HTTP,
		HTTPS,
		SOCKS
	}
	
	internal class CFProxy {
		//static IntPtr kCFProxyAutoConfigurationHTTPResponseKey;
		static IntPtr kCFProxyAutoConfigurationJavaScriptKey;
		static IntPtr kCFProxyAutoConfigurationURLKey;
		static IntPtr kCFProxyHostNameKey;
		static IntPtr kCFProxyPasswordKey;
		static IntPtr kCFProxyPortNumberKey;
		static IntPtr kCFProxyTypeKey;
		static IntPtr kCFProxyUsernameKey;

		//static IntPtr kCFProxyTypeNone;
		static IntPtr kCFProxyTypeAutoConfigurationURL;
		static IntPtr kCFProxyTypeAutoConfigurationJavaScript;
		static IntPtr kCFProxyTypeFTP;
		static IntPtr kCFProxyTypeHTTP;
		static IntPtr kCFProxyTypeHTTPS;
		static IntPtr kCFProxyTypeSOCKS;

		static CFProxy ()
		{
			IntPtr handle = CFObject.dlopen (CFNetwork.CFNetworkLibrary, 0);

			//kCFProxyAutoConfigurationHTTPResponseKey = CFObject.GetCFObjectHandle (handle, "kCFProxyAutoConfigurationHTTPResponseKey");
			kCFProxyAutoConfigurationJavaScriptKey = CFObject.GetCFObjectHandle (handle, "kCFProxyAutoConfigurationJavaScriptKey");
			kCFProxyAutoConfigurationURLKey = CFObject.GetCFObjectHandle (handle, "kCFProxyAutoConfigurationURLKey");
			kCFProxyHostNameKey = CFObject.GetCFObjectHandle (handle, "kCFProxyHostNameKey");
			kCFProxyPasswordKey = CFObject.GetCFObjectHandle (handle, "kCFProxyPasswordKey");
			kCFProxyPortNumberKey = CFObject.GetCFObjectHandle (handle, "kCFProxyPortNumberKey");
			kCFProxyTypeKey = CFObject.GetCFObjectHandle (handle, "kCFProxyTypeKey");
			kCFProxyUsernameKey = CFObject.GetCFObjectHandle (handle, "kCFProxyUsernameKey");

			//kCFProxyTypeNone = CFObject.GetCFObjectHandle (handle, "kCFProxyTypeNone");
			kCFProxyTypeAutoConfigurationURL = CFObject.GetCFObjectHandle (handle, "kCFProxyTypeAutoConfigurationURL");
			kCFProxyTypeAutoConfigurationJavaScript = CFObject.GetCFObjectHandle (handle, "kCFProxyTypeAutoConfigurationJavaScript");
			kCFProxyTypeFTP = CFObject.GetCFObjectHandle (handle, "kCFProxyTypeFTP");
			kCFProxyTypeHTTP = CFObject.GetCFObjectHandle (handle, "kCFProxyTypeHTTP");
			kCFProxyTypeHTTPS = CFObject.GetCFObjectHandle (handle, "kCFProxyTypeHTTPS");
			kCFProxyTypeSOCKS = CFObject.GetCFObjectHandle (handle, "kCFProxyTypeSOCKS");

			CFObject.dlclose (handle);
		}

		CFDictionary settings;
		
		internal CFProxy (CFDictionary settings)
		{
			this.settings = settings;
		}
		
		static CFProxyType CFProxyTypeToEnum (IntPtr type)
		{
			if (type == kCFProxyTypeAutoConfigurationJavaScript)
				return CFProxyType.AutoConfigurationJavaScript;

			if (type == kCFProxyTypeAutoConfigurationURL)
				return CFProxyType.AutoConfigurationUrl;

			if (type == kCFProxyTypeFTP)
				return CFProxyType.FTP;

			if (type == kCFProxyTypeHTTP)
				return CFProxyType.HTTP;

			if (type == kCFProxyTypeHTTPS)
				return CFProxyType.HTTPS;

			if (type == kCFProxyTypeSOCKS)
				return CFProxyType.SOCKS;
			
			return CFProxyType.None;
		}
		
#if false
		// AFAICT these get used with CFNetworkExecuteProxyAutoConfiguration*()
		
		// TODO: bind CFHTTPMessage so we can return the proper type here.
		public IntPtr AutoConfigurationHTTPResponse {
			get { return settings[kCFProxyAutoConfigurationHTTPResponseKey]; }
		}
#endif

		public IntPtr AutoConfigurationJavaScript {
			get {
				if (kCFProxyAutoConfigurationJavaScriptKey == IntPtr.Zero)
					return IntPtr.Zero;
				
				return settings[kCFProxyAutoConfigurationJavaScriptKey];
			}
		}
		
		public IntPtr AutoConfigurationUrl {
			get {
				if (kCFProxyAutoConfigurationURLKey == IntPtr.Zero)
					return IntPtr.Zero;
				
				return settings[kCFProxyAutoConfigurationURLKey];
			}
		}
		
		public string HostName {
			get {
				if (kCFProxyHostNameKey == IntPtr.Zero)
					return null;
				
				return CFString.AsString (settings[kCFProxyHostNameKey]);
			}
		}
		
		public string Password {
			get {
				if (kCFProxyPasswordKey == IntPtr.Zero)
					return null;

				return CFString.AsString (settings[kCFProxyPasswordKey]);
			}
		}
		
		public int Port {
			get {
				if (kCFProxyPortNumberKey == IntPtr.Zero)
					return 0;
				
				return CFNumber.AsInt32 (settings[kCFProxyPortNumberKey]);
			}
		}
		
		public CFProxyType ProxyType {
			get {
				if (kCFProxyTypeKey == IntPtr.Zero)
					return CFProxyType.None;
				
				return CFProxyTypeToEnum (settings[kCFProxyTypeKey]);
			}
		}
		
		public string Username {
			get {
				if (kCFProxyUsernameKey == IntPtr.Zero)
					return null;

				return CFString.AsString (settings[kCFProxyUsernameKey]);
			}
		}
	}
	
	internal class CFProxySettings {
		static IntPtr kCFNetworkProxiesHTTPEnable;
		static IntPtr kCFNetworkProxiesHTTPPort;
		static IntPtr kCFNetworkProxiesHTTPProxy;
		static IntPtr kCFNetworkProxiesProxyAutoConfigEnable;
		static IntPtr kCFNetworkProxiesProxyAutoConfigJavaScript;
		static IntPtr kCFNetworkProxiesProxyAutoConfigURLString;

		static CFProxySettings ()
		{
			IntPtr handle = CFObject.dlopen (CFNetwork.CFNetworkLibrary, 0);

			kCFNetworkProxiesHTTPEnable = CFObject.GetCFObjectHandle (handle, "kCFNetworkProxiesHTTPEnable");
			kCFNetworkProxiesHTTPPort = CFObject.GetCFObjectHandle (handle, "kCFNetworkProxiesHTTPPort");
			kCFNetworkProxiesHTTPProxy = CFObject.GetCFObjectHandle (handle, "kCFNetworkProxiesHTTPProxy");
			kCFNetworkProxiesProxyAutoConfigEnable = CFObject.GetCFObjectHandle (handle, "kCFNetworkProxiesProxyAutoConfigEnable");
			kCFNetworkProxiesProxyAutoConfigJavaScript = CFObject.GetCFObjectHandle (handle, "kCFNetworkProxiesProxyAutoConfigJavaScript");
			kCFNetworkProxiesProxyAutoConfigURLString = CFObject.GetCFObjectHandle (handle, "kCFNetworkProxiesProxyAutoConfigURLString");

			CFObject.dlclose (handle);
		}

		CFDictionary settings;
		
		public CFProxySettings (CFDictionary settings)
		{
			this.settings = settings;
		}
		
		public CFDictionary Dictionary {
			get { return settings; }
		}
		
		public bool HTTPEnable {
			get {
				if (kCFNetworkProxiesHTTPEnable == IntPtr.Zero)
					return false;

				return CFNumber.AsBool (settings[kCFNetworkProxiesHTTPEnable]);
			}
		}
		
		public int HTTPPort {
			get {
				if (kCFNetworkProxiesHTTPPort == IntPtr.Zero)
					return 0;
				
				return CFNumber.AsInt32 (settings[kCFNetworkProxiesHTTPPort]);
			}
		}
		
		public string HTTPProxy {
			get {
				if (kCFNetworkProxiesHTTPProxy == IntPtr.Zero)
					return null;
				
				return CFString.AsString (settings[kCFNetworkProxiesHTTPProxy]);
			}
		}
		
		public bool ProxyAutoConfigEnable {
			get {
				if (kCFNetworkProxiesProxyAutoConfigEnable == IntPtr.Zero)
					return false;
				
				return CFNumber.AsBool (settings[kCFNetworkProxiesProxyAutoConfigEnable]);
			}
		}
		
		public string ProxyAutoConfigJavaScript {
			get {
				if (kCFNetworkProxiesProxyAutoConfigJavaScript == IntPtr.Zero)
					return null;
				
				return CFString.AsString (settings[kCFNetworkProxiesProxyAutoConfigJavaScript]);
			}
		}
		
		public string ProxyAutoConfigURLString {
			get {
				if (kCFNetworkProxiesProxyAutoConfigURLString == IntPtr.Zero)
					return null;
				
				return CFString.AsString (settings[kCFNetworkProxiesProxyAutoConfigURLString]);
			}
		}
	}
	
	internal static class CFNetwork {
#if !MONOTOUCH
		public const string CFNetworkLibrary = "/System/Library/Frameworks/CoreServices.framework/Frameworks/CFNetwork.framework/CFNetwork";
#else
		public const string CFNetworkLibrary = "/System/Library/Frameworks/CFNetwork.framework/CFNetwork";
#endif
		
		[DllImport (CFNetworkLibrary)]
		// CFArrayRef CFNetworkCopyProxiesForAutoConfigurationScript (CFStringRef proxyAutoConfigurationScript, CFURLRef targetURL);
		extern static IntPtr CFNetworkCopyProxiesForAutoConfigurationScript (IntPtr proxyAutoConfigurationScript, IntPtr targetURL);
		
		static CFArray CopyProxiesForAutoConfigurationScript (IntPtr proxyAutoConfigurationScript, CFUrl targetURL)
		{
			IntPtr native = CFNetworkCopyProxiesForAutoConfigurationScript (proxyAutoConfigurationScript, targetURL.Handle);
			
			if (native == IntPtr.Zero)
				return null;
			
			return new CFArray (native, true);
		}
		
		public static CFProxy[] GetProxiesForAutoConfigurationScript (IntPtr proxyAutoConfigurationScript, CFUrl targetURL)
		{
			if (proxyAutoConfigurationScript == IntPtr.Zero)
				throw new ArgumentNullException ("proxyAutoConfigurationScript");
			
			if (targetURL == null)
				throw new ArgumentNullException ("targetURL");
			
			CFArray array = CopyProxiesForAutoConfigurationScript (proxyAutoConfigurationScript, targetURL);
			
			if (array == null)
				return null;
			
			CFProxy[] proxies = new CFProxy [array.Count];
			for (int i = 0; i < proxies.Length; i++) {
				CFDictionary dict = new CFDictionary (array[i], false);
				proxies[i] = new CFProxy (dict);
			}

			array.Dispose ();
			
			return proxies;
		}
		
		public static CFProxy[] GetProxiesForAutoConfigurationScript (IntPtr proxyAutoConfigurationScript, Uri targetUri)
		{
			if (proxyAutoConfigurationScript == IntPtr.Zero)
				throw new ArgumentNullException ("proxyAutoConfigurationScript");
			
			if (targetUri == null)
				throw new ArgumentNullException ("targetUri");
			
			CFUrl targetURL = CFUrl.Create (targetUri.AbsoluteUri);
			CFProxy[] proxies = GetProxiesForAutoConfigurationScript (proxyAutoConfigurationScript, targetURL);
			targetURL.Dispose ();
			
			return proxies;
		}
		
		[DllImport (CFNetworkLibrary)]
		// CFArrayRef CFNetworkCopyProxiesForURL (CFURLRef url, CFDictionaryRef proxySettings);
		extern static IntPtr CFNetworkCopyProxiesForURL (IntPtr url, IntPtr proxySettings);
		
		static CFArray CopyProxiesForURL (CFUrl url, CFDictionary proxySettings)
		{
			IntPtr native = CFNetworkCopyProxiesForURL (url.Handle, proxySettings != null ? proxySettings.Handle : IntPtr.Zero);
			
			if (native == IntPtr.Zero)
				return null;
			
			return new CFArray (native, true);
		}
		
		public static CFProxy[] GetProxiesForURL (CFUrl url, CFProxySettings proxySettings)
		{
			if (url == null || url.Handle == IntPtr.Zero)
				throw new ArgumentNullException ("url");
			
			if (proxySettings == null)
				proxySettings = GetSystemProxySettings ();
			
			CFArray array = CopyProxiesForURL (url, proxySettings.Dictionary);
			
			if (array == null)
				return null;

			CFProxy[] proxies = new CFProxy [array.Count];
			for (int i = 0; i < proxies.Length; i++) {
				CFDictionary dict = new CFDictionary (array[i], false);
				proxies[i] = new CFProxy (dict);
			}

			array.Dispose ();
			
			return proxies;
		}
		
		public static CFProxy[] GetProxiesForUri (Uri uri, CFProxySettings proxySettings)
		{
			if (uri == null)
				throw new ArgumentNullException ("uri");
			
			CFUrl url = CFUrl.Create (uri.AbsoluteUri);
			if (url == null)
				return null;
			
			CFProxy[] proxies = GetProxiesForURL (url, proxySettings);
			url.Dispose ();
			
			return proxies;
		}
		
		[DllImport (CFNetworkLibrary)]
		// CFDictionaryRef CFNetworkCopySystemProxySettings (void);
		extern static IntPtr CFNetworkCopySystemProxySettings ();
		
		public static CFProxySettings GetSystemProxySettings ()
		{
			IntPtr native = CFNetworkCopySystemProxySettings ();
			
			if (native == IntPtr.Zero)
				return null;
			
			var dict = new CFDictionary (native, true);

			return new CFProxySettings (dict);
		}
		
		class CFWebProxy : IWebProxy {
			ICredentials credentials;
			bool userSpecified;
			
			public CFWebProxy ()
			{
			}
			
			public ICredentials Credentials {
				get { return credentials; }
				set {
					userSpecified = true;
					credentials = value;
				}
			}
			
			static Uri GetProxyUri (CFProxy proxy, out NetworkCredential credentials)
			{
				string protocol;
				
				switch (proxy.ProxyType) {
				case CFProxyType.FTP:
					protocol = "ftp://";
					break;
				case CFProxyType.HTTP:
				case CFProxyType.HTTPS:
					protocol = "http://";
					break;
				default:
					credentials = null;
					return null;
				}
				
				string username = proxy.Username;
				string password = proxy.Password;
				string hostname = proxy.HostName;
				int port = proxy.Port;
				string uri;
				
				if (username != null)
					credentials = new NetworkCredential (username, password);
				else
					credentials = null;
				
				uri = protocol + hostname + (port != 0 ? ':' + port.ToString () : string.Empty);
				
				return new Uri (uri, UriKind.Absolute);
			}
			
			static Uri GetProxyUriFromScript (IntPtr script, Uri targetUri, out NetworkCredential credentials)
			{
				CFProxy[] proxies = CFNetwork.GetProxiesForAutoConfigurationScript (script, targetUri);
				
				if (proxies == null) {
					credentials = null;
					return targetUri;
				}
				
				for (int i = 0; i < proxies.Length; i++) {
					switch (proxies[i].ProxyType) {
					case CFProxyType.HTTPS:
					case CFProxyType.HTTP:
					case CFProxyType.FTP:
						// create a Uri based on the hostname/port/etc info
						return GetProxyUri (proxies[i], out credentials);
					case CFProxyType.SOCKS:
					default:
						// unsupported proxy type, try the next one
						break;
					case CFProxyType.None:
						// no proxy should be used
						credentials = null;
						return targetUri;
					}
				}
				
				credentials = null;
				
				return null;
			}
			
			public Uri GetProxy (Uri targetUri)
			{
				NetworkCredential credentials = null;
				Uri proxy = null;
				
				if (targetUri == null)
					throw new ArgumentNullException ("targetUri");
				
				try {
					CFProxySettings settings = CFNetwork.GetSystemProxySettings ();
					CFProxy[] proxies = CFNetwork.GetProxiesForUri (targetUri, settings);
					
					if (proxies != null) {
						for (int i = 0; i < proxies.Length && proxy == null; i++) {
							switch (proxies[i].ProxyType) {
							case CFProxyType.AutoConfigurationJavaScript:
								proxy = GetProxyUriFromScript (proxies[i].AutoConfigurationJavaScript, targetUri, out credentials);
								break;
							case CFProxyType.AutoConfigurationUrl:
								// unsupported proxy type (requires fetching script from remote url)
								break;
							case CFProxyType.HTTPS:
							case CFProxyType.HTTP:
							case CFProxyType.FTP:
								// create a Uri based on the hostname/port/etc info
								proxy = GetProxyUri (proxies[i], out credentials);
								break;
							case CFProxyType.SOCKS:
								// unsupported proxy type, try the next one
								break;
							case CFProxyType.None:
								// no proxy should be used
								proxy = targetUri;
								break;
							}
						}
						
						if (proxy == null) {
							// no supported proxies for this Uri, fall back to trying to connect to targetUri directly
							proxy = targetUri;
						}
					} else {
						proxy = targetUri;
					}
				} catch {
					// ignore errors while retrieving proxy data
					proxy = targetUri;
				}
				
				if (!userSpecified)
					this.credentials = credentials;
				
				return proxy;
			}
			
			public bool IsBypassed (Uri targetUri)
			{
				if (targetUri == null)
					throw new ArgumentNullException ("targetUri");
				
				return GetProxy (targetUri) == targetUri;
			}
		}
		
		public static IWebProxy GetDefaultProxy ()
		{
			return new CFWebProxy ();
		}
	}
}
