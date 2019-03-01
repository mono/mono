using System;
using System.IO;
using System.Text;
using System.Net.Http;

#if XAMCORE_2_0
using Foundation;
using ObjCRuntime;
#elif MONOMAC
using MonoMac.Foundation;
using MonoMac.ObjCRuntime;
#else
#error Unknown platform
#endif

namespace System.Net.Http {
	class RuntimeOptions
	{
		const string HttpClientHandlerValue = "HttpClientHandler";
		const string CFNetworkHandlerValue = "CFNetworkHandler";
		const string NSUrlSessionHandlerValue = "NSUrlSessionHandler";

		const string DefaultTlsProviderValue = "default";
		const string LegacyTlsProviderValue = "legacy";
		const string AppleTlsProviderValue = "appletls";

		string http_message_handler;


		internal static RuntimeOptions Read ()
		{
			// for iOS NSBundle.ResourcePath returns the path to the root of the app bundle
			// for macOS apps NSBundle.ResourcePath returns foo.app/Contents/Resources
			// for macOS frameworks NSBundle.ResourcePath returns foo.app/Versions/Current/Resources
			Class bundle_finder = new Class (typeof (NSObject.NSObject_Disposer));
			var resource_dir = NSBundle.FromClass (bundle_finder).ResourcePath;
			var plist_path = GetFileName (resource_dir);

			if (!File.Exists (plist_path))
				return null;

			using (var plist = NSDictionary.FromFile (plist_path)) {
				var options = new RuntimeOptions ();
				options.http_message_handler = (NSString) plist ["HttpMessageHandler"];
				return options;
			}
		}

#if MONOMAC
		[Preserve]
#endif
		internal static HttpMessageHandler GetHttpMessageHandler ()
		{
			RuntimeOptions options = null;
			
			try {
				options = RuntimeOptions.Read ();
			} catch (FileNotFoundException){
				// this happens on the Mono SDKs since we don't have a real Xamarin.iOS.dll so we can't resolve NSObject
			}

			if (options == null) {
#if MONOTOUCH_WATCH
				return new NSUrlSessionHandler ();
#else
				return new HttpClientHandler ();
#endif
			}

			// all types will be present as this is executed only when the linker is not enabled
			var handler_name = options.http_message_handler;
			var t = Type.GetType (handler_name, false);

			HttpMessageHandler handler = null;
			if (t != null)
				handler = Activator.CreateInstance (t) as HttpMessageHandler;
			if (handler != null)
				return handler;
#if MONOTOUCH_WATCH
			Console.WriteLine ("{0} is not a valid HttpMessageHandler, defaulting to NSUrlSessionHandler", handler_name);
			return new NSUrlSessionHandler ();
#else
			Console.WriteLine ("{0} is not a valid HttpMessageHandler, defaulting to System.Net.Http.HttpClientHandler", handler_name);
			return new HttpClientHandler ();
#endif
		}

		// Use either Create() or Read().
		RuntimeOptions ()
		{
		}

		static string GetFileName (string resource_dir)
		{
			return Path.Combine (resource_dir, "runtime-options.plist");
		}
	}
}
