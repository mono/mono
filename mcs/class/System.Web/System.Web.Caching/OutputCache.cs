//
// Authors:
//   Marek Habersack <mhabersack@novell.com>
//
// (C) 2010 Novell, Inc (http://novell.com/)
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

using System.Configuration;
using System.Configuration.Provider;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Permissions;
using System.Web;
using System.Web.Configuration;

namespace System.Web.Caching
{
	public static class OutputCache
	{
		internal const string DEFAULT_PROVIDER_NAME = "AspNetInternalProvider";
		
		static readonly object initLock = new object ();
		static readonly object defaultProviderInitLock = new object();
		
		static bool initialized;
		static string defaultProviderName;
		static OutputCacheProviderCollection providers;
		static OutputCacheProvider defaultProvider;
		
		public static string DefaultProviderName {
			get {
				Init ();
				if (String.IsNullOrEmpty (defaultProviderName))
					return DEFAULT_PROVIDER_NAME;
				
				return defaultProviderName;
			}
		}

		internal static OutputCacheProvider DefaultProvider {
			get {
				if (defaultProvider == null) {
					lock (defaultProviderInitLock) {
						if (defaultProvider == null)
							defaultProvider = new InMemoryOutputCacheProvider ();
					}
				}

				return defaultProvider;
			}
		}
		
		public static OutputCacheProviderCollection Providers {
			get {
				Init ();
				return providers;
			}
		}
		
		[SecurityPermission (SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public static object Deserialize (Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");

			object o = new BinaryFormatter ().Deserialize (stream);
			if (o == null || IsInvalidType (o))
				throw new ArgumentException ("The provided parameter is not of a supported type for serialization and/or deserialization.");

			return o;
		}

		[SecurityPermission (SecurityAction.Assert, Flags = SecurityPermissionFlag.SerializationFormatter)]
		public static void Serialize (Stream stream, object data)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");			

			// LAMESPEC: data == null doesn't throw ArgumentNullException
			if (data == null || IsInvalidType (data))
				throw new ArgumentException ("The provided parameter is not of a supported type for serialization and/or deserialization.");
			
			new BinaryFormatter ().Serialize (stream, data);
		}

		internal static OutputCacheProvider GetProvider (string providerName)
		{
			if (String.IsNullOrEmpty (providerName))
				return null;

			if (String.Compare (providerName, DEFAULT_PROVIDER_NAME, StringComparison.Ordinal) == 0)
				return DefaultProvider;

			OutputCacheProviderCollection providers = OutputCache.Providers;
			return (providers != null ? providers [providerName] : null);
		}
		
		static bool IsInvalidType (object data)
		{
			return !(data is MemoryResponseElement) &&
				!(data is FileResponseElement) &&
				!(data is SubstitutionResponseElement);
		}
		
		static void Init ()
		{
			if (initialized)
				return;

			lock (initLock) {
				if (initialized)
					return;
				
				var cfg = WebConfigurationManager.GetWebApplicationSection ("system.web/caching/outputCache") as OutputCacheSection;
				ProviderSettingsCollection cfgProviders = cfg.Providers;

				defaultProviderName = cfg.DefaultProviderName;
				if (cfgProviders != null && cfgProviders.Count > 0) {
					var coll = new OutputCacheProviderCollection ();

					foreach (ProviderSettings ps in cfgProviders)
						coll.Add (LoadProvider (ps));

					coll.SetReadOnly ();
					providers = coll;
				}

				initialized = true;
			}
		}

		static OutputCacheProvider LoadProvider (ProviderSettings ps)
		{
			Type type = HttpApplication.LoadType (ps.Type, false);
			if (type == null)
				throw new ConfigurationErrorsException (String.Format ("Could not load type '{0}'.", ps.Type));
			
			var ret = Activator.CreateInstance (type) as OutputCacheProvider;
			ret.Initialize (ps.Name, ps.Parameters);

			return ret;
		}

		internal static void RemoveFromProvider (string key, string providerName)
		{
			if (providerName == null)
				return;

			OutputCacheProviderCollection providers = Providers;
			OutputCacheProvider provider;
			
			if (providers == null || providers.Count == 0)
				provider = null;
			else
				provider = providers [providerName];

			if (provider == null)
				throw new ProviderException ("Provider '" + providerName + "' was not found.");

			provider.Remove (key);
		}
	}
}
