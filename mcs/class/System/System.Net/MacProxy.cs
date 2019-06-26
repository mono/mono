// 
// MacProxy.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2012-2014 Xamarin Inc.
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
using System.Net;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using ObjCRuntimeInternal;

namespace Mono.Net
{
	internal struct CFStreamClientContext {
		public IntPtr Version;
		public IntPtr Info;
		public IntPtr Retain;
		public IntPtr Release;
		public IntPtr CopyDescription;
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

	internal class CFRunLoop : CFObject
	{
		[DllImport (CFObject.CoreFoundationLibrary)]
		static extern void CFRunLoopAddSource (IntPtr rl, IntPtr source, IntPtr mode);

		[DllImport (CFObject.CoreFoundationLibrary)]
		static extern void CFRunLoopRemoveSource (IntPtr rl, IntPtr source, IntPtr mode);

		[DllImport (CFObject.CoreFoundationLibrary)]
		static extern int CFRunLoopRunInMode (IntPtr mode, double seconds, bool returnAfterSourceHandled);

		[DllImport (CFObject.CoreFoundationLibrary)]
		static extern IntPtr CFRunLoopGetCurrent ();

		[DllImport (CFObject.CoreFoundationLibrary)]
		static extern void CFRunLoopStop (IntPtr rl);

		public CFRunLoop (IntPtr handle, bool own): base (handle, own)
		{
		}

		public static CFRunLoop CurrentRunLoop {
			get { return new CFRunLoop (CFRunLoopGetCurrent (), false); }
		}

		public void AddSource (IntPtr source, CFString mode)
		{
			CFRunLoopAddSource (Handle, source, mode.Handle);
		}

		public void RemoveSource (IntPtr source, CFString mode)
		{
			CFRunLoopRemoveSource (Handle, source, mode.Handle);
		}

		public int RunInMode (CFString mode, double seconds, bool returnAfterSourceHandled)
		{
			return CFRunLoopRunInMode (mode.Handle, seconds, returnAfterSourceHandled);
		}

		public void Stop ()
		{
			CFRunLoopStop (Handle);
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
			
			//in OSX 10.13 pointer comparison didn't work for kCFProxyTypeAutoConfigurationURL
			if (CFString.Compare (type, kCFProxyTypeAutoConfigurationJavaScript) == 0)
				return CFProxyType.AutoConfigurationJavaScript;

			if (CFString.Compare (type, kCFProxyTypeAutoConfigurationURL) == 0)
				return CFProxyType.AutoConfigurationUrl;

			if (CFString.Compare (type, kCFProxyTypeFTP) == 0)
				return CFProxyType.FTP;

			if (CFString.Compare (type, kCFProxyTypeHTTP) == 0)
				return CFProxyType.HTTP;

			if (CFString.Compare (type, kCFProxyTypeHTTPS) == 0)
				return CFProxyType.HTTPS;

			if (CFString.Compare (type, kCFProxyTypeSOCKS) == 0)
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
		
		[DllImport (CFNetworkLibrary, EntryPoint = "CFNetworkCopyProxiesForAutoConfigurationScript")]
		// CFArrayRef CFNetworkCopyProxiesForAutoConfigurationScript (CFStringRef proxyAutoConfigurationScript, CFURLRef targetURL, CFErrorRef* error);
		extern static IntPtr CFNetworkCopyProxiesForAutoConfigurationScriptSequential (IntPtr proxyAutoConfigurationScript, IntPtr targetURL, out IntPtr error);

		[DllImport (CFNetworkLibrary)]
		extern static IntPtr CFNetworkExecuteProxyAutoConfigurationURL (IntPtr proxyAutoConfigURL, IntPtr targetURL, CFProxyAutoConfigurationResultCallback cb, ref CFStreamClientContext clientContext);


		class GetProxyData : IDisposable {
			public IntPtr script;
			public IntPtr targetUri;
			public IntPtr error;
			public IntPtr result;
			public ManualResetEvent evt = new ManualResetEvent (false);

			public void Dispose ()
			{
				evt.Close ();
			}
		}

		static object lock_obj = new object ();
		static Queue<GetProxyData> get_proxy_queue;
		static AutoResetEvent proxy_event;

		static void CFNetworkCopyProxiesForAutoConfigurationScriptThread ()
		{
			GetProxyData data;
			var data_left = true;

			while (true) {
				proxy_event.WaitOne ();

				do {
					lock (lock_obj) {
						if (get_proxy_queue.Count == 0)
							break;
						data = get_proxy_queue.Dequeue ();
						data_left = get_proxy_queue.Count > 0;
					}

					data.result = CFNetworkCopyProxiesForAutoConfigurationScriptSequential (data.script, data.targetUri, out data.error);
					data.evt.Set ();
				} while (data_left);
			}
		}

		static IntPtr CFNetworkCopyProxiesForAutoConfigurationScript (IntPtr proxyAutoConfigurationScript, IntPtr targetURL, out IntPtr error)
		{
			// This method must only be called on only one thread during an application's life time.
			// Note that it's not enough to use a lock to make calls sequential across different threads,
			// it has to be one thread. Also note that that thread can't be the main thread, because the
			// main thread might be blocking waiting for this network request to finish.
			// Another possibility would be to use JavaScriptCore to execute this piece of
			// javascript ourselves, but unfortunately it's not available before iOS7.
			// See bug #7923 comment #21+.

			using (var data = new GetProxyData ()) {
				data.script = proxyAutoConfigurationScript;
				data.targetUri = targetURL;

				lock (lock_obj) {
					if (get_proxy_queue == null) {
						get_proxy_queue = new Queue<GetProxyData> ();
						proxy_event = new AutoResetEvent (false);
						new Thread (CFNetworkCopyProxiesForAutoConfigurationScriptThread) {
							IsBackground = true,
						}.Start ();
					}
					get_proxy_queue.Enqueue (data);
					proxy_event.Set ();
				}

				data.evt.WaitOne ();

				error = data.error;

				return data.result;
			}
		}

		static CFArray CopyProxiesForAutoConfigurationScript (IntPtr proxyAutoConfigurationScript, CFUrl targetURL)
		{
			IntPtr err = IntPtr.Zero;
			IntPtr native = CFNetworkCopyProxiesForAutoConfigurationScript (proxyAutoConfigurationScript, targetURL.Handle, out err);
			
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

		delegate void CFProxyAutoConfigurationResultCallback (IntPtr client, IntPtr proxyList, IntPtr error);

		public static CFProxy[] ExecuteProxyAutoConfigurationURL (IntPtr proxyAutoConfigURL, Uri targetURL)
		{
			CFUrl url = CFUrl.Create (targetURL.AbsoluteUri);
			if (url == null)
				return null;

			CFProxy[] proxies = null;

			var runLoop = CFRunLoop.CurrentRunLoop;

			// Callback that will be called after executing the configuration script
			CFProxyAutoConfigurationResultCallback cb = delegate (IntPtr client, IntPtr proxyList, IntPtr error) {
				if (proxyList != IntPtr.Zero) {
					var array = new CFArray (proxyList, false);
					proxies = new CFProxy [array.Count];
					for (int i = 0; i < proxies.Length; i++) {
						CFDictionary dict = new CFDictionary (array[i], false);
						proxies[i] = new CFProxy (dict);
					}
					array.Dispose ();
				}
				runLoop.Stop ();
			};

			var clientContext = new CFStreamClientContext ();
			var loopSource = CFNetworkExecuteProxyAutoConfigurationURL (proxyAutoConfigURL, url.Handle, cb, ref clientContext);

			// Create a private mode
			var mode = CFString.Create ("Mono.MacProxy");

			runLoop.AddSource (loopSource, mode);
			runLoop.RunInMode (mode, double.MaxValue, false);
			runLoop.RemoveSource (loopSource, mode);

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
				return SelectProxy (proxies, targetUri, out credentials);
			}

			static Uri ExecuteProxyAutoConfigurationURL (IntPtr proxyAutoConfigURL, Uri targetUri, out NetworkCredential credentials)
			{
				CFProxy[] proxies = CFNetwork.ExecuteProxyAutoConfigurationURL (proxyAutoConfigURL, targetUri);
				return SelectProxy (proxies, targetUri, out credentials);
			}


			static Uri SelectProxy (CFProxy[] proxies, Uri targetUri, out NetworkCredential credentials)
			{
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
								proxy = ExecuteProxyAutoConfigurationURL (proxies[i].AutoConfigurationUrl, targetUri, out credentials);
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
