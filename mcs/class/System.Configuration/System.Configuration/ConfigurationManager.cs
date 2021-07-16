//
// System.Configuration.ConfigurationManager.cs
//
// Authors:
//	Duncan Mak (duncan@ximian.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Reflection;
using System.Xml;
using System.IO;
using System.Text;
using System.Configuration.Internal;
using System.Security.Cryptography;

namespace System.Configuration {

	/*roaming user config path: C:\Documents and Settings\toshok\Application Data\domain-System.Configurati_Url_py3nlovv3wxe21qgacxc3n2b1mph2log\1.0.0.0\user.config */

	public static class ConfigurationManager
	{
//		static bool systemWebInUse;
		static InternalConfigurationFactory configFactory = new InternalConfigurationFactory ();
		static IInternalConfigSystem configSystem = new ClientConfigurationSystem ();
		static object lockobj = new object ();
		
		static string GetAssemblyInfo (Assembly a)
		{
			string company_name;
			string product_name;
			string version;
			object[] attrs;
			byte[] hash;
			byte[] pkt;

			pkt = a.GetName ().GetPublicKeyToken ();

			// Keep this in sync with System.Configuration.CustomizableFileSettingsProvider.GetCompanyName
			attrs = a.GetCustomAttributes (typeof (AssemblyCompanyAttribute), true);
			if ((attrs != null) && attrs.Length > 0) {
				company_name = ((AssemblyCompanyAttribute)attrs[0]).Company;
			} else {
				MethodInfo entryPoint = a.EntryPoint;
				Type entryType = entryPoint != null ? entryPoint.DeclaringType : null;
				if (entryType != null && !String.IsNullOrEmpty (entryType.Namespace)) {
					int end = entryType.Namespace.IndexOf ('.');
					company_name = end < 0 ? entryType.Namespace : entryType.Namespace.Substring (0, end);
				}
				else
					company_name = "Program";
			}

			// Keep this in sync with System.Configuration.CustomizableFileSettingsProvider.GetEvidenceHash
			hash = SHA1.Create ().ComputeHash (pkt != null && pkt.Length > 0 ? pkt : Encoding.UTF8.GetBytes (a.EscapedCodeBase));
			System.Text.StringBuilder evidence_string = new System.Text.StringBuilder();
			foreach (byte b in hash)
				evidence_string.AppendFormat("{0:x2}",b);

			// Keep this in sync with System.Configuration.CustomizableFileSettingsProvider.GetProductName
			attrs = a.GetCustomAttributes (typeof (AssemblyProductAttribute), false);
			product_name = String.Format ("{0}_{1}_{2}",
				(attrs != null && attrs.Length > 0) ? ((AssemblyProductAttribute)attrs[0]).Product : AppDomain.CurrentDomain.FriendlyName,
				pkt != null && pkt.Length > 0 ? "StrongName" : "Url",
				evidence_string.ToString ());

			// Keep this in sync with System.Configuration.CustomizableFileSettingsProvider.GetProductVersion
			attrs = a.GetCustomAttributes (typeof (AssemblyInformationalVersionAttribute), false);
			if (attrs != null && attrs.Length > 0) {
				version = ((AssemblyInformationalVersionAttribute)attrs[0]).InformationalVersion;
			} else {
				attrs = a.GetCustomAttributes (typeof (AssemblyFileVersionAttribute), false);
				version = (attrs != null && attrs.Length > 0) ? ((AssemblyFileVersionAttribute)attrs[0]).Version : a.GetName ().Version.ToString ();
			}

			return Path.Combine (company_name, product_name, version);
		}

		internal static Configuration OpenExeConfigurationInternal (ConfigurationUserLevel userLevel, Assembly calling_assembly, string exePath)
		{
			ExeConfigurationFileMap map = new ExeConfigurationFileMap ();

			/* Roaming and RoamingAndLocal should be different

			On windows,
			  PerUserRoaming = \Documents and Settings\<username>\Application Data\...
			  PerUserRoamingAndLocal = \Documents and Settings\<username>\Local Settings\Application Data\...
			*/

			switch (userLevel) {
			case ConfigurationUserLevel.None:
				if (exePath == null || exePath.Length == 0) {
					map.ExeConfigFilename = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
				} else {
					if (!Path.IsPathRooted (exePath))
						exePath = Path.GetFullPath (exePath);
					if (!File.Exists (exePath)) {
						Exception cause = new ArgumentException ("The specified path does not exist.", "exePath");
						throw new ConfigurationErrorsException ("Error Initializing the configuration system:", cause);
					}
					map.ExeConfigFilename = exePath + ".config";
				}
				break;
			case ConfigurationUserLevel.PerUserRoaming:
				map.RoamingUserConfigFilename = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.ApplicationData), GetAssemblyInfo(calling_assembly));
				map.RoamingUserConfigFilename = Path.Combine (map.RoamingUserConfigFilename, "user.config");
				goto case ConfigurationUserLevel.None;

			case ConfigurationUserLevel.PerUserRoamingAndLocal:
				map.LocalUserConfigFilename = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), GetAssemblyInfo(calling_assembly));
				map.LocalUserConfigFilename = Path.Combine (map.LocalUserConfigFilename, "user.config");
				goto case ConfigurationUserLevel.PerUserRoaming;
			}

			return ConfigurationFactory.Create (typeof(ExeConfigurationHost), map, userLevel);
		}

		public static Configuration OpenExeConfiguration (ConfigurationUserLevel userLevel)
		{
			return OpenExeConfigurationInternal (userLevel, Assembly.GetEntryAssembly () ?? Assembly.GetCallingAssembly (), null);
		}
		
		public static Configuration OpenExeConfiguration (string exePath)
		{
			return OpenExeConfigurationInternal (ConfigurationUserLevel.None, Assembly.GetEntryAssembly () ?? Assembly.GetCallingAssembly (), exePath);
		}

		[MonoLimitation("ConfigurationUserLevel parameter is not supported.")]
		public static Configuration OpenMappedExeConfiguration (ExeConfigurationFileMap fileMap, ConfigurationUserLevel userLevel)
		{
			return ConfigurationFactory.Create (typeof(ExeConfigurationHost), fileMap, userLevel);
		}

		public static Configuration OpenMachineConfiguration ()
		{
			ConfigurationFileMap map = new ConfigurationFileMap ();
			return ConfigurationFactory.Create (typeof(MachineConfigurationHost), map);
		}
		
		public static Configuration OpenMappedMachineConfiguration (ConfigurationFileMap fileMap)
		{
			return ConfigurationFactory.Create (typeof(MachineConfigurationHost), fileMap);
		}
		
		internal static IInternalConfigConfigurationFactory ConfigurationFactory {
			get { return configFactory; }
		}

		internal static IInternalConfigSystem ConfigurationSystem {
			get { return configSystem; }
		}

		public static object GetSection (string sectionName)
		{
			object o = ConfigurationSystem.GetSection (sectionName);
			if (o is ConfigurationSection)
				return ((ConfigurationSection) o).GetRuntimeObject ();
			else
				return o;
		}

		public static void RefreshSection (string sectionName)
		{
			ConfigurationSystem.RefreshConfig (sectionName);
		}

		public static NameValueCollection AppSettings {
			get {
				return (NameValueCollection) GetSection ("appSettings");
			}
		}

		public static ConnectionStringSettingsCollection ConnectionStrings {
			get {
				ConnectionStringsSection connectionStrings = (ConnectionStringsSection) GetSection ("connectionStrings");
				return connectionStrings.ConnectionStrings;
			}
		}

		/* invoked from System.Web */
		internal static IInternalConfigSystem ChangeConfigurationSystem (IInternalConfigSystem newSystem)
		{
			if (newSystem == null)
				throw new ArgumentNullException ("newSystem");

			lock (lockobj) {
				// KLUDGE!! We need that when an assembly loaded inside an ASP.NET
				// domain does OpenExeConfiguration ("") - we must return the path
				// to web.config in that instance.
				/*
				string t = newSystem.GetType ().ToString ();
				if (String.Compare (t, "System.Web.Configuration.HttpConfigurationSystem", StringComparison.OrdinalIgnoreCase) == 0)
					systemWebInUse = true;
				else
					systemWebInUse = false;
				*/
				IInternalConfigSystem old = configSystem;
				configSystem = newSystem;
				return old;
			}
		}
	}
}
