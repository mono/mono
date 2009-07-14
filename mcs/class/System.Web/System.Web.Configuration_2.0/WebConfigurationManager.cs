//
// System.Web.Configuration.WebConfigurationManager.cs
//
// Authors:
// 	Lluis Sanchez Gual (lluis@novell.com)
// 	Chris Toshok (toshok@ximian.com)
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
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.IO;
using System.Collections;
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

namespace System.Web.Configuration {

	public static class WebConfigurationManager
	{
#if !TARGET_J2EE
		static readonly object suppressAppReloadLock = new object ();
		
		static IInternalConfigConfigurationFactory configFactory;
		static Hashtable configurations = Hashtable.Synchronized (new Hashtable ());
		static Hashtable sectionCache = new Hashtable ();
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

		static Hashtable sectionCache
		{
			get
			{
				Hashtable sectionCache = (Hashtable) AppDomain.CurrentDomain.GetData ("sectionCache");
				if (sectionCache == null) {
					sectionCache = new Hashtable (StringComparer.OrdinalIgnoreCase);
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
		
		static WebConfigurationManager ()
		{
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
		}

		static void ConfigurationSaveHandler (_Configuration sender, ConfigurationSaveEventArgs args)
		{
			string rootConfigPath = WebConfigurationHost.GetWebConfigFileName (HttpRuntime.AppDomainAppPath);
			if (String.Compare (args.StreamPath, rootConfigPath, StringComparison.OrdinalIgnoreCase) == 0)
				SuppressAppReload (args.Start);
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

			if (!fweb && !String.IsNullOrEmpty (path))
				path = FindWebConfig (path);

			string confKey = path + site + locationSubPath + server + userName + password;
			_Configuration conf = null;
			conf = (_Configuration) configurations [confKey];
			if (conf == null) {
				try {
					conf = ConfigurationFactory.Create (typeof (WebConfigurationHost), null, path, site, locationSubPath, server, userName, password);
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

		internal static object GetSection (string sectionName, string path, HttpContext context)
		{
			string config_vdir = FindWebConfig (path);
			if (String.IsNullOrEmpty (config_vdir))
				config_vdir = "/";

			object cachedSection = sectionCache [GetSectionCacheKey (sectionName, config_vdir)];
			if (cachedSection != null)
				return cachedSection;

			HttpRequest req = context != null ? context.Request : null;
			_Configuration c = OpenWebConfiguration (config_vdir, //path, /* path */
								 null, /* site */
					 			 req != null ? VirtualPathUtility.GetDirectory (req.Path) : null, /* locationSubPath */
								 null, /* server */
								 null, /* userName */
								 null, /* password */
								 true  /* path from FindWebConfig */);
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

			AddSectionToCache (GetSectionCacheKey (sectionName, config_vdir), value);
			return value;
#else
#if MONOWEB_DEP
			object value = SettingsMappingManager.MapSection (get_runtime_object.Invoke (section, new object [0]));
#else
			object value = null;
#endif
			AddSectionToCache (GetSectionCacheKey (sectionName, config_vdir), value);
			return value;
#endif
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
			if (String.IsNullOrEmpty (path))
				return path;

			string dir;
			if (path [path.Length - 1] == '/')
				dir = path;
			else {
				dir = VirtualPathUtility.GetDirectory (path, false);
				if (dir == null)
					return path;
			}
			
			string curPath = configPaths [dir] as string;
			if (curPath != null)
				return curPath;
			
			HttpContext ctx = HttpContext.Current;
			HttpRequest req = ctx != null ? ctx.Request : null;
			if (req == null)
				return path;

			curPath = path;
			string rootPath = HttpRuntime.AppDomainAppVirtualPath;
			string physPath;

			while (String.Compare (curPath, rootPath, StringComparison.Ordinal) != 0) {
				physPath = MapPath (req, curPath);
				if (physPath == null) {
					curPath = rootPath;
					break;
				}

				if (WebConfigurationHost.GetWebConfigFileName (physPath) != null)
					break;
				
				curPath = GetParentDir (rootPath, curPath);
				if (curPath == null) {
					curPath = rootPath;
					break;
				}
			}

			configPaths [dir] = curPath;
			return curPath;
		}
		
		static string GetCurrentPath (HttpContext ctx)
		{
			HttpRequest req = ctx != null ? ctx.Request : null;
			return req != null ? req.Path : HttpRuntime.AppDomainAppVirtualPath;
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
			if (sectionCache [key] != null)
				return;

			Hashtable tmpTable = (Hashtable) sectionCache.Clone ();
			if (tmpTable.Contains (key))
				return;

			tmpTable.Add (key, section);
			sectionCache = tmpTable;
		}

		static int GetSectionCacheKey (string sectionName, string path)
		{
			return (sectionName != null ? sectionName.GetHashCode () : 0) ^ ((path != null ? path.GetHashCode () : 0) + 37);
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
