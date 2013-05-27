//
// System.Web.Configuration.WebConfigurationManager.cs
//
// Authors:
// 	Lluis Sanchez Gual (lluis@novell.com)
// 	Chris Toshok (toshok@ximian.com)
//      Marek Habersack <mhabersack@novell.com>
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
// Copyright (C) 2005-2009 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
#if MONOWEB_DEP
using Mono.Web.Util;
#endif
using System.Xml;
using System.Configuration;
using System.Configuration.Internal;
using _Configuration = System.Configuration.Configuration;
using System.Web.Util;
using System.Threading;
using System.Web.Hosting;

namespace System.Web.Configuration {

	public static class WebConfigurationManager
	{
		sealed class ConfigPath 
		{
			public string Path;
			public bool InAnotherApp;

			public ConfigPath (string path, bool inAnotherApp)
			{
				this.Path = path;
				this.InAnotherApp = inAnotherApp;
			}
		}
		
		const int SAVE_LOCATIONS_CHECK_INTERVAL = 6000; // milliseconds
		const int SECTION_CACHE_LOCK_TIMEOUT = 200; // milliseconds

		static readonly char[] pathTrimChars = { '/' };
		static readonly object suppressAppReloadLock = new object ();
		static readonly object saveLocationsCacheLock = new object ();
		
		// See comment for the cacheLock field at top of System.Web.Caching/Cache.cs
		static readonly ReaderWriterLockSlim sectionCacheLock;

#if !TARGET_J2EE
		static IInternalConfigConfigurationFactory configFactory;
		static Hashtable configurations = Hashtable.Synchronized (new Hashtable ());
		static Hashtable configPaths = Hashtable.Synchronized (new Hashtable ());
		static bool suppressAppReload;
#else
		const string AppSettingsKey = "WebConfigurationManager.AppSettings";
		
		static internal IInternalConfigConfigurationFactory configFactory
		{
			get{
				IInternalConfigConfigurationFactory factory = (IInternalConfigConfigurationFactory)AppDomain.CurrentDomain.GetData("WebConfigurationManager.configFactory");
				if (factory == null){
					lock (AppDomain.CurrentDomain){
						object initialized = AppDomain.CurrentDomain.GetData("WebConfigurationManager.configFactory.initialized");
						if (initialized == null){
							PropertyInfo prop = typeof(ConfigurationManager).GetProperty("ConfigurationFactory", BindingFlags.Static | BindingFlags.NonPublic);
							if (prop != null){
								factory = prop.GetValue(null, null) as IInternalConfigConfigurationFactory;
								configFactory = factory;
							}
						}
					}
				}
				return factory != null ? factory : configFactory;
			}
			set{
				AppDomain.CurrentDomain.SetData("WebConfigurationManager.configFactory", value);
				AppDomain.CurrentDomain.SetData("WebConfigurationManager.configFactory.initialized", true);
			}
		}

		static internal Hashtable configurations
		{
			get{
				Hashtable table = (Hashtable)AppDomain.CurrentDomain.GetData("WebConfigurationManager.configurations");
				if (table == null){
					lock (AppDomain.CurrentDomain){
						object initialized = AppDomain.CurrentDomain.GetData("WebConfigurationManager.configurations.initialized");
						if (initialized == null){
							table = Hashtable.Synchronized (new Hashtable (StringComparer.OrdinalIgnoreCase));
							configurations = table;
						}
					}
				}
				return table != null ? table : configurations;

			}
			set{
				AppDomain.CurrentDomain.SetData("WebConfigurationManager.configurations", value);
				AppDomain.CurrentDomain.SetData("WebConfigurationManager.configurations.initialized", true);
			}
		}

		static Dictionary <int, object> sectionCache
		{
			get
			{
				Dictionary <int, object> sectionCache = AppDomain.CurrentDomain.GetData ("sectionCache") as Dictionary <int, object>;
				if (sectionCache == null) {
					sectionCache = new Dictionary <int, object> ();
					AppDomain.CurrentDomain.SetData ("sectionCache", sectionCache);
				}
				return sectionCache;
			}
			set
			{
				AppDomain.CurrentDomain.SetData ("sectionCache", value);
			}
		}

		static internal Hashtable configPaths
		{
			get{
				Hashtable table = (Hashtable)AppDomain.CurrentDomain.GetData("WebConfigurationManager.configPaths");
				if (table == null){
					lock (AppDomain.CurrentDomain){
						object initialized = AppDomain.CurrentDomain.GetData("WebConfigurationManager.configPaths.initialized");
						if (initialized == null){
							table = Hashtable.Synchronized (new Hashtable (StringComparer.OrdinalIgnoreCase));
							configPaths = table;
						}
					}
				}
				return table != null ? table : configPaths;

			}
			set{
				AppDomain.CurrentDomain.SetData("WebConfigurationManager.configPaths", value);
				AppDomain.CurrentDomain.SetData("WebConfigurationManager.configPaths.initialized", true);
			}
		}
#endif
		static Dictionary <string, DateTime> saveLocationsCache;
		static Timer saveLocationsTimer;
		
		static ArrayList extra_assemblies = null;
		static internal ArrayList ExtraAssemblies {
			get {
				if (extra_assemblies == null)
					extra_assemblies = new ArrayList();
				return extra_assemblies;
			}
		}

		static bool hasConfigErrors = false;
		static object hasConfigErrorsLock = new object ();
		static internal bool HasConfigErrors {
			get {
				lock (hasConfigErrorsLock) {
					return hasConfigErrors;
				}
			}
		}

		const int DEFAULT_SECTION_CACHE_SIZE = 100;
		const string CACHE_SIZE_OVERRIDING_KEY = "MONO_ASPNET_WEBCONFIG_CACHESIZE";
		static LruCache<int, object> sectionCache;
		
		static WebConfigurationManager ()
		{
			var section_cache_size = DEFAULT_SECTION_CACHE_SIZE;
			int section_cache_size_override;
			bool size_overriden = false;
			if (int.TryParse (Environment.GetEnvironmentVariable (CACHE_SIZE_OVERRIDING_KEY), out section_cache_size_override)) {
				section_cache_size = section_cache_size_override;
				size_overriden = true;
				Console.WriteLine ("WebConfigurationManager's LRUcache Size overriden to: {0} (via {1})", section_cache_size_override, CACHE_SIZE_OVERRIDING_KEY);
			}
			sectionCache = new LruCache<int, object> (section_cache_size);
			string eviction_warning = "WebConfigurationManager's LRUcache evictions count reached its max size";
			if (!size_overriden)
				eviction_warning += String.Format ("{0}Cache Size: {1} (overridable via {2})",
				                                   Environment.NewLine, section_cache_size, CACHE_SIZE_OVERRIDING_KEY);
			sectionCache.EvictionWarning = eviction_warning;

			configFactory = ConfigurationManager.ConfigurationFactory;
			_Configuration.SaveStart += ConfigurationSaveHandler;
			_Configuration.SaveEnd += ConfigurationSaveHandler;
			
			// Part of fix for bug #491531
			Type type = Type.GetType ("System.Configuration.CustomizableFileSettingsProvider, System", false);
			if (type != null) {
				FieldInfo fi = type.GetField ("webConfigurationFileMapType", BindingFlags.Static | BindingFlags.NonPublic);
				if (fi != null && fi.FieldType == Type.GetType ("System.Type"))
					fi.SetValue (null, typeof (ApplicationSettingsConfigurationFileMap));
			}

			sectionCacheLock = new ReaderWriterLockSlim ();
		}

		static void ReenableWatcherOnConfigLocation (object state)
		{
			string path = state as string;
			if (String.IsNullOrEmpty (path))
				return;

			DateTime lastWrite;
			lock (saveLocationsCacheLock) {
				if (!saveLocationsCache.TryGetValue (path, out lastWrite))
					lastWrite = DateTime.MinValue;
			}

			DateTime now = DateTime.Now;
			if (lastWrite == DateTime.MinValue || now.Subtract (lastWrite).TotalMilliseconds >= SAVE_LOCATIONS_CHECK_INTERVAL) {
				saveLocationsTimer.Dispose ();
				saveLocationsTimer = null;
				HttpApplicationFactory.EnableWatcher (VirtualPathUtility.RemoveTrailingSlash (HttpRuntime.AppDomainAppPath), "?eb.?onfig");
			} else
				saveLocationsTimer.Change (SAVE_LOCATIONS_CHECK_INTERVAL, SAVE_LOCATIONS_CHECK_INTERVAL);
		}
		
		static void ConfigurationSaveHandler (_Configuration sender, ConfigurationSaveEventArgs args)
		{
			try {
				sectionCacheLock.EnterWriteLock ();
				sectionCache.Clear ();
			} finally {
				sectionCacheLock.ExitWriteLock ();
			}
			
			lock (suppressAppReloadLock) {
				string rootConfigPath = WebConfigurationHost.GetWebConfigFileName (HttpRuntime.AppDomainAppPath);
				if (String.Compare (args.StreamPath, rootConfigPath, StringComparison.OrdinalIgnoreCase) == 0) {
					SuppressAppReload (args.Start);
					if (args.Start) {
						HttpApplicationFactory.DisableWatcher (VirtualPathUtility.RemoveTrailingSlash (HttpRuntime.AppDomainAppPath), "?eb.?onfig");

						lock (saveLocationsCacheLock) {
							if (saveLocationsCache == null)
								saveLocationsCache = new Dictionary <string, DateTime> (StringComparer.Ordinal);
							if (saveLocationsCache.ContainsKey (rootConfigPath))
								saveLocationsCache [rootConfigPath] = DateTime.Now;
							else
								saveLocationsCache.Add (rootConfigPath, DateTime.Now);

							if (saveLocationsTimer == null)
								saveLocationsTimer = new Timer (ReenableWatcherOnConfigLocation,
												rootConfigPath,
												SAVE_LOCATIONS_CHECK_INTERVAL,
												SAVE_LOCATIONS_CHECK_INTERVAL);
						}
					}
				}
			}
		}
		
		public static _Configuration OpenMachineConfiguration ()
		{
			return ConfigurationManager.OpenMachineConfiguration ();
		}
		
		[MonoLimitation ("locationSubPath is not handled")]
		public static _Configuration OpenMachineConfiguration (string locationSubPath)
		{
			return OpenMachineConfiguration ();
		}

		[MonoLimitation("Mono does not support remote configuration")]
		public static _Configuration OpenMachineConfiguration (string locationSubPath,
								       string server)
		{
			if (server == null)
				return OpenMachineConfiguration (locationSubPath);

			throw new NotSupportedException ("Mono doesn't support remote configuration");
		}

		[MonoLimitation("Mono does not support remote configuration")]
		public static _Configuration OpenMachineConfiguration (string locationSubPath,
								       string server,
								       IntPtr userToken)
		{
			if (server == null)
				return OpenMachineConfiguration (locationSubPath);
			throw new NotSupportedException ("Mono doesn't support remote configuration");
		}

		[MonoLimitation("Mono does not support remote configuration")]
		public static _Configuration OpenMachineConfiguration (string locationSubPath,
								       string server,
								       string userName,
								       string password)
		{
			if (server == null)
				return OpenMachineConfiguration (locationSubPath);
			throw new NotSupportedException ("Mono doesn't support remote configuration");
		}

		public static _Configuration OpenWebConfiguration (string path)
		{
			return OpenWebConfiguration (path, null, null, null, null, null);
		}
		
		public static _Configuration OpenWebConfiguration (string path, string site)
		{
			return OpenWebConfiguration (path, site, null, null, null, null);
		}
		
		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath)
		{
			return OpenWebConfiguration (path, site, locationSubPath, null, null, null);
		}

		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath, string server)
		{
			return OpenWebConfiguration (path, site, locationSubPath, server, null, null);
		}

		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath, string server, IntPtr userToken)
		{
			return OpenWebConfiguration (path, site, locationSubPath, server, null, null);
		}
		
		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath, string server, string userName, string password)
		{
			return OpenWebConfiguration (path, site, locationSubPath, server, null, null, false);
		}

		static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath, string server, string userName, string password, bool fweb)
		{
			if (String.IsNullOrEmpty (path))
				path = "/";

			bool inAnotherApp = false;
			if (!fweb && !String.IsNullOrEmpty (path))
				path = FindWebConfig (path, out inAnotherApp);

			string confKey = path + site + locationSubPath + server + userName + password;
			_Configuration conf = null;
			conf = (_Configuration) configurations [confKey];
			if (conf == null) {
				try {
					conf = ConfigurationFactory.Create (typeof (WebConfigurationHost), null, path, site, locationSubPath, server, userName, password, inAnotherApp);
					configurations [confKey] = conf;
				} catch (Exception ex) {
					lock (hasConfigErrorsLock) {
						hasConfigErrors = true;
					}
					throw ex;
				}
			}
			return conf;
		}

		public static _Configuration OpenMappedWebConfiguration (WebConfigurationFileMap fileMap, string path)
		{
			return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap, path);
		}
		
		public static _Configuration OpenMappedWebConfiguration (WebConfigurationFileMap fileMap, string path, string site)
		{
			return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap, path, site);
		}
		
		public static _Configuration OpenMappedWebConfiguration (WebConfigurationFileMap fileMap, string path, string site, string locationSubPath)
		{
			return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap, path, site, locationSubPath);
		}
		
		public static _Configuration OpenMappedMachineConfiguration (ConfigurationFileMap fileMap)
		{
			return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap);
		}

		public static _Configuration OpenMappedMachineConfiguration (ConfigurationFileMap fileMap,
									     string locationSubPath)
		{
			return OpenMappedMachineConfiguration (fileMap);
		}

		internal static object SafeGetSection (string sectionName, Type configSectionType)
		{
			try {
				return GetSection (sectionName);
			} catch (Exception) {
				if (configSectionType != null)
					return Activator.CreateInstance (configSectionType);
				return null;
			}
		}
		
		internal static object SafeGetSection (string sectionName, string path, Type configSectionType)
		{
			try {
				return GetSection (sectionName, path);
			} catch (Exception) {
				if (configSectionType != null)
					return Activator.CreateInstance (configSectionType);
				return null;
			}
		}
		
		public static object GetSection (string sectionName)
		{
			HttpContext context = HttpContext.Current;
			return GetSection (sectionName, GetCurrentPath (context), context);
		}

		public static object GetSection (string sectionName, string path)
		{
			return GetSection (sectionName, path, HttpContext.Current);
		}

		static bool LookUpLocation (string relativePath, ref _Configuration defaultConfiguration)
		{
			if (String.IsNullOrEmpty (relativePath))
				return false;

			_Configuration cnew = defaultConfiguration.FindLocationConfiguration (relativePath, defaultConfiguration);
			if (cnew == defaultConfiguration)
				return false;

			defaultConfiguration = cnew;
			return true;
		}
		
		internal static object GetSection (string sectionName, string path, HttpContext context)
		{
			if (String.IsNullOrEmpty (sectionName))
				return null;
			
			_Configuration c = OpenWebConfiguration (path, null, null, null, null, null, false);
			string configPath = c.ConfigPath;
			int baseCacheKey = 0;
			int cacheKey;
			bool pathPresent = !String.IsNullOrEmpty (path);
			string locationPath = null;

			if (pathPresent)
				locationPath = "location_" + path;
			
			baseCacheKey = sectionName.GetHashCode ();
			if (configPath != null)
				baseCacheKey ^= configPath.GetHashCode ();
			
			try {
				sectionCacheLock.EnterWriteLock ();
				
				object o;
				if (pathPresent) {
					cacheKey = baseCacheKey ^ locationPath.GetHashCode ();
					if (sectionCache.TryGetValue (cacheKey, out o))
						return o;
				
					cacheKey = baseCacheKey ^ path.GetHashCode ();
					if (sectionCache.TryGetValue (cacheKey, out o))
						return o;
				}
				
				if (sectionCache.TryGetValue (baseCacheKey, out o))
					return o;
			} finally {
				sectionCacheLock.ExitWriteLock ();
			}

			string cachePath = null;
			if (pathPresent) {
				string relPath;
				
				if (VirtualPathUtility.IsRooted (path)) {
					if (path [0] == '~')
						relPath = path.Length > 1 ? path.Substring (2) : String.Empty;
					else if (path [0] == '/')
						relPath = path.Substring (1);
					else
						relPath = path;
				} else
					relPath = path;

				HttpRequest req = context != null ? context.Request : null;
				if (req != null) {
					string vdir = VirtualPathUtility.GetDirectory (req.PathNoValidation);
					if (vdir != null) {
						vdir = vdir.TrimEnd (pathTrimChars);
						if (String.Compare (c.ConfigPath, vdir, StringComparison.Ordinal) != 0 && LookUpLocation (vdir.Trim (pathTrimChars), ref c))
							cachePath = path;
					}
				}
				
				if (LookUpLocation (relPath, ref c))
					cachePath = locationPath;
				else
					cachePath = path;
			}

			ConfigurationSection section = c.GetSection (sectionName);
			if (section == null)
				return null;

#if TARGET_J2EE
			object value = get_runtime_object.Invoke (section, new object [0]);
			if (String.CompareOrdinal ("appSettings", sectionName) == 0) {
				NameValueCollection collection;
				collection = new KeyValueMergedCollection (HttpContext.Current, (NameValueCollection) value);
				value = collection;
			}
#else
#if MONOWEB_DEP
			object value = SettingsMappingManager.MapSection (get_runtime_object.Invoke (section, new object [0]));
#else
			object value = null;
#endif
#endif
			if (cachePath != null)
				cacheKey = baseCacheKey ^ cachePath.GetHashCode ();
			else
				cacheKey = baseCacheKey;
			
			AddSectionToCache (cacheKey, value);
			return value;
		}
		
		static string MapPath (HttpRequest req, string virtualPath)
		{
			if (req != null)
				return req.MapPath (virtualPath);

			string appRoot = HttpRuntime.AppDomainAppVirtualPath;
			if (!String.IsNullOrEmpty (appRoot) && virtualPath.StartsWith (appRoot, StringComparison.Ordinal)) {
				if (String.Compare (virtualPath, appRoot, StringComparison.Ordinal) == 0)
					return HttpRuntime.AppDomainAppPath;
				return UrlUtils.Combine (HttpRuntime.AppDomainAppPath, virtualPath.Substring (appRoot.Length));
			}
			
			return null;
		}

		static string GetParentDir (string rootPath, string curPath)
		{
			int len = curPath.Length - 1;
			if (len > 0 && curPath [len] == '/')
				curPath = curPath.Substring (0, len);

			if (String.Compare (curPath, rootPath, StringComparison.Ordinal) == 0)
				return null;
			
			int idx = curPath.LastIndexOf ('/');
			if (idx == -1)
				return curPath;

			if (idx == 0)
				return "/";
			
			return curPath.Substring (0, idx);
		}

		internal static string FindWebConfig (string path)
		{
			bool dummy;

			return FindWebConfig (path, out dummy);
		}
		
		internal static string FindWebConfig (string path, out bool inAnotherApp)
		{
			inAnotherApp = false;
			
			if (String.IsNullOrEmpty (path))
				return path;
				
			if (HostingEnvironment.VirtualPathProvider != null) {
				if (HostingEnvironment.VirtualPathProvider.DirectoryExists (path))
					path = VirtualPathUtility.AppendTrailingSlash (path);
			}
				
			
			string rootPath = HttpRuntime.AppDomainAppVirtualPath;
			ConfigPath curPath;
			curPath = configPaths [path] as ConfigPath;
			if (curPath != null) {
				inAnotherApp = curPath.InAnotherApp;
				return curPath.Path;
			}
			
			HttpContext ctx = HttpContext.Current;
			HttpRequest req = ctx != null ? ctx.Request : null;
			string physPath = req != null ? VirtualPathUtility.AppendTrailingSlash (MapPath (req, path)) : null;
			string appDomainPath = HttpRuntime.AppDomainAppPath;
			
			if (physPath != null && appDomainPath != null && !physPath.StartsWith (appDomainPath, StringComparison.Ordinal))
				inAnotherApp = true;
			
			string dir;
			if (inAnotherApp || path [path.Length - 1] == '/')
				dir = path;
			else {
			 	dir = VirtualPathUtility.GetDirectory (path, false);
			 	if (dir == null)
			 		return path;
			}
			
			curPath = configPaths [dir] as ConfigPath;
			if (curPath != null) {
				inAnotherApp = curPath.InAnotherApp;
				return curPath.Path;
			}
			
			if (req == null)
				return path;

			curPath = new ConfigPath (path, inAnotherApp);
			while (String.Compare (curPath.Path, rootPath, StringComparison.Ordinal) != 0) {
				physPath = MapPath (req, curPath.Path);
				if (physPath == null) {
					curPath.Path = rootPath;
					break;
				}

				if (WebConfigurationHost.GetWebConfigFileName (physPath) != null)
					break;
				
				curPath.Path = GetParentDir (rootPath, curPath.Path);
				if (curPath.Path == null || curPath.Path == "~") {
					curPath.Path = rootPath;
					break;
				}
			}

			if (String.Compare (curPath.Path, path, StringComparison.Ordinal) != 0)
				configPaths [path] = curPath;
			else
				configPaths [dir] = curPath;
			
			return curPath.Path;
		}
		
		static string GetCurrentPath (HttpContext ctx)
		{
			HttpRequest req = ctx != null ? ctx.Request : null;
			return req != null ? req.PathNoValidation : HttpRuntime.AppDomainAppVirtualPath;
		}
		
		internal static bool SuppressAppReload (bool newValue)
		{
			bool ret;
			
			lock (suppressAppReloadLock) {
				ret = suppressAppReload;
				suppressAppReload = newValue;
			}

			return ret;
		}
		
		internal static void RemoveConfigurationFromCache (HttpContext ctx)
		{
			configurations.Remove (GetCurrentPath (ctx));
		}

#if TARGET_J2EE || MONOWEB_DEP
		readonly static MethodInfo get_runtime_object = typeof (ConfigurationSection).GetMethod ("GetRuntimeObject", BindingFlags.NonPublic | BindingFlags.Instance);
#endif		

		public static object GetWebApplicationSection (string sectionName)
		{
			HttpContext ctx = HttpContext.Current;
			HttpRequest req = ctx != null ? ctx.Request : null;
			string applicationPath = req != null ? req.ApplicationPath : null;
			return GetSection (sectionName, String.IsNullOrEmpty (applicationPath) ? String.Empty : applicationPath);
		}

		public static NameValueCollection AppSettings {
			get { return ConfigurationManager.AppSettings; }
		}

		public static ConnectionStringSettingsCollection ConnectionStrings {
			get { return ConfigurationManager.ConnectionStrings; }
		}

		internal static IInternalConfigConfigurationFactory ConfigurationFactory {
			get { return configFactory; }
		}

		static void AddSectionToCache (int key, object section)
		{
			object cachedSection;

			bool locked = false;
			try {
				if (!sectionCacheLock.TryEnterWriteLock (SECTION_CACHE_LOCK_TIMEOUT))
					return;
				locked = true;

				if (sectionCache.TryGetValue (key, out cachedSection) && cachedSection != null)
					return;

				sectionCache.Add (key, section);
			} finally {
				if (locked) {
					sectionCacheLock.ExitWriteLock ();
				}
			}
		}
		
#region stuff copied from WebConfigurationSettings
#if TARGET_J2EE
		static internal IConfigurationSystem oldConfig {
			get {
				return (IConfigurationSystem)AppDomain.CurrentDomain.GetData("WebConfigurationManager.oldConfig");
			}
			set {
				AppDomain.CurrentDomain.SetData("WebConfigurationManager.oldConfig", value);
			}
		}

		static Web20DefaultConfig config {
			get {
				return (Web20DefaultConfig) AppDomain.CurrentDomain.GetData ("Web20DefaultConfig.config");
			}
			set {
				AppDomain.CurrentDomain.SetData ("Web20DefaultConfig.config", value);
			}
		}

		static IInternalConfigSystem configSystem {
			get {
				return (IInternalConfigSystem) AppDomain.CurrentDomain.GetData ("IInternalConfigSystem.configSystem");
			}
			set {
				AppDomain.CurrentDomain.SetData ("IInternalConfigSystem.configSystem", value);
			}
		}
#else
		static internal IConfigurationSystem oldConfig;
		static Web20DefaultConfig config;
		//static IInternalConfigSystem configSystem;
#endif
		const BindingFlags privStatic = BindingFlags.NonPublic | BindingFlags.Static;
		static readonly object lockobj = new object ();

		internal static void Init ()
		{
			lock (lockobj) {
				if (config != null)
					return;

				/* deal with the ConfigurationSettings stuff */
				{
					Web20DefaultConfig settings = Web20DefaultConfig.GetInstance ();
					Type t = typeof (ConfigurationSettings);
					MethodInfo changeConfig = t.GetMethod ("ChangeConfigurationSystem",
									       privStatic);

					if (changeConfig == null)
						throw new ConfigurationException ("Cannot find method CCS");

					object [] args = new object [] {settings};
					oldConfig = (IConfigurationSystem)changeConfig.Invoke (null, args);
					config = settings;

					config.Init ();
				}

				/* deal with the ConfigurationManager stuff */
				{
					HttpConfigurationSystem system = new HttpConfigurationSystem ();
					Type t = typeof (ConfigurationManager);
					MethodInfo changeConfig = t.GetMethod ("ChangeConfigurationSystem",
									       privStatic);

					if (changeConfig == null)
						throw new ConfigurationException ("Cannot find method CCS");

					object [] args = new object [] {system};
					changeConfig.Invoke (null, args);
					//configSystem = system;
				}
			}
		}
	}

	class Web20DefaultConfig : IConfigurationSystem
	{
#if TARGET_J2EE
		static Web20DefaultConfig instance {
			get {
				Web20DefaultConfig val = (Web20DefaultConfig)AppDomain.CurrentDomain.GetData("Web20DefaultConfig.instance");
				if (val == null) {
					val = new Web20DefaultConfig();
					AppDomain.CurrentDomain.SetData("Web20DefaultConfig.instance", val);
				}
				return val;
			}
			set {
				AppDomain.CurrentDomain.SetData("Web20DefaultConfig.instance", value);
			}
		}
#else
		static Web20DefaultConfig instance;
#endif

		static Web20DefaultConfig ()
		{
			instance = new Web20DefaultConfig ();
		}

		public static Web20DefaultConfig GetInstance ()
		{
			return instance;
		}

		public object GetConfig (string sectionName)
		{
			object o = WebConfigurationManager.GetWebApplicationSection (sectionName);

			if (o == null || o is IgnoreSection) {
				/* this can happen when the section
				 * handler doesn't subclass from
				 * ConfigurationSection.  let's be
				 * nice and try to load it using the
				 * 1.x style routines in case there's
				 * a 1.x section handler registered
				 * for it.
				 */
				object o1 = WebConfigurationManager.oldConfig.GetConfig (sectionName);
				if (o1 != null)
					return o1;
			}

			return o;
		}

		public void Init ()
		{
			// nothing. We need a context.
		}
	}
#endregion
}

#endif
