//
// System.Web.Configuration.WebConfigurationManager.cs
//
// Authors:
// 	Lluis Sanchez Gual (lluis@novell.com)
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
using System.Xml;
using System.Configuration;
using System.Configuration.Internal;
using _Configuration = System.Configuration.Configuration;

namespace System.Web.Configuration {

	public abstract class WebConfigurationManager
	{
		static IInternalConfigConfigurationFactory configFactory;
		static Hashtable configurations = new Hashtable ();
		
		static WebConfigurationManager ()
		{
			PropertyInfo prop = typeof(ConfigurationManager).GetProperty ("ConfigurationFactory", BindingFlags.Static | BindingFlags.NonPublic);
			if (prop != null)
				configFactory = prop.GetValue (null, null) as IInternalConfigConfigurationFactory;
		}
		
		WebConfigurationManager ()
		{
		}
		
		public static _Configuration OpenWebConfiguration (string path)
		{
			return OpenWebConfiguration (path, null, null, null, IntPtr.Zero, null);
		}
		
		public static _Configuration OpenWebConfiguration (string path, string site)
		{
			return OpenWebConfiguration (path, site, null, null, IntPtr.Zero, null);
		}
		
		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath)
		{
			return OpenWebConfiguration (path, site, locationSubPath, null, IntPtr.Zero, null);
		}
		
		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath, string server, IntPtr userToken)
		{
			return OpenWebConfiguration (path, site, locationSubPath, server, userToken, null);
		}
		
		[MonoTODO ("Do something with the extra parameters")]
		public static _Configuration OpenWebConfiguration (string path, string site, string locationSubPath, string server, IntPtr userToken, string password)
		{
			string basePath = GetBasePath (path);
			_Configuration conf;
			
			lock (configurations) {
				conf = (_Configuration) configurations [basePath];
				if (conf == null) {
					conf = ConfigurationFactory.Create (typeof(WebConfigurationHost), null, path, site, locationSubPath, server, userToken, password);
					configurations [basePath] = conf;
				}
			}
			if (basePath.Length < path.Length) {
			
				// If the path has a file name, look for a location specific configuration
				
				int dif = path.Length - basePath.Length;
				string file = path.Substring (path.Length - dif);
				int i=0;
				while (i < file.Length && file [i] == '/')
					i++;
				if (i != 0)
					file = file.Substring (i);

				if (file.Length != 0) {
					foreach (ConfigurationLocation loc in conf.Locations) {
						if (loc.Path == file)
							return loc.OpenConfiguration ();
					}
				}
			}
			return conf;
		}
		
		public static _Configuration OpenMappedWebConfiguration (WebConfigurationFileMap fileMap, string path)
		{
			return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap, path);
		}
		
		[MonoTODO ("Do something with the extra parameters")]
		public static _Configuration OpenMappedWebConfiguration (WebConfigurationFileMap fileMap, string path, string site)
		{
			return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap, path, site);
		}
		
		[MonoTODO ("Do something with the extra parameters")]
		public static _Configuration OpenMappedWebConfiguration (WebConfigurationFileMap fileMap, string path, string site, string locationSubPath)
		{
			return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap, path, site, locationSubPath);
		}
		
		public static _Configuration OpenMappedMachineConfiguration (ConfigurationFileMap fileMap)
		{
			return ConfigurationFactory.Create (typeof(WebConfigurationHost), fileMap);
		}
		
		internal static IInternalConfigConfigurationFactory ConfigurationFactory {
			get { return configFactory; }
		}
		
		static string GetBasePath (string path)
		{
			if (path == "/")
				return path;
			
			string pd = HttpContext.Current.Request.MapPath (path);

			if (!Directory.Exists (pd)) {
				int i = path.LastIndexOf ('/');
				path = path.Substring (0, i);
			} 
			
			while (path [path.Length - 1] == '/')
				path = path.Substring (0, path.Length - 1);
			return path;
		}
	}
}

#endif
