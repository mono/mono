
//
// System/AppDomain.cs
//
// Authors:
//   Paolo Molaro (lupus@ximian.com)
//   Dietmar Maurer (dietmar@ximian.com)
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001, 2002 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Collections;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security.Policy;

namespace System {

	public sealed class AppDomain /* : MarshalByRefObject , _AppDomain, IEvidenceFactory */ {

		private Hashtable loaded_assemblies = new Hashtable ();
		private Hashtable data_hash = new Hashtable ();
		private AppDomainSetup adsetup;
		private string friendly_name;
		private Evidence evidence;

		private AppDomain ()
		{
			//
			// Prime the loaded assemblies with the assemblies that were loaded
			// by the runtime in our behalf
			//

			//
			// TODO: Maybe we can lazily do this, as loaded_assemblies
			// will not be used all the time, we can just compute this on
			// demand.
			//
			foreach (Assembly a in getDefaultAssemblies ())
				loaded_assemblies [a.FullName] = a;
		}
		
		public AppDomainSetup SetupInformation {

			get {
				return adsetup;
			}
		}

		public string BaseDirectory {

			get {
				return adsetup.ApplicationBase;
			}
		}

		public string RelativeSearchPath {

			get {
				return adsetup.PrivateBinPath;
			}
		}

		public string DynamicDirectory {

			get {
				// fixme: dont know what to return here
				return null;
			}
		}

		public string FriendlyName {

			get {
				return friendly_name;
			}
		}

		public Evidence Evidence {

			get {
				return evidence;
			}
		}

		
		public static AppDomain CreateDomain (string friendlyName)
		{
			return CreateDomain (friendlyName, new Evidence (), new AppDomainSetup ());
		}
		
		public static AppDomain CreateDomain (string friendlyName, Evidence securityInfo)
		{
			return CreateDomain (friendlyName, securityInfo, new AppDomainSetup ());
		}
		
		public static AppDomain CreateDomain (string friendlyName,
						      Evidence securityInfo,
						      AppDomainSetup info)
		{
			if (friendlyName == null || securityInfo == null || info == null)
				throw new System.ArgumentNullException();

			AppDomain ad = new AppDomain ();

			ad.friendly_name = friendlyName;
			ad.evidence = securityInfo;
			ad.adsetup = info;

			return ad;
		}

		public static AppDomain CreateDomain (string friendlyName, Evidence securityInfo,
						      string appBasePath, string appRelativeSearchPath,
						      bool shadowCopyFiles)
		{
			AppDomainSetup info = new AppDomainSetup ();

			info.ApplicationBase = appBasePath;
			info.PrivateBinPath = appRelativeSearchPath;

			if (shadowCopyFiles)
				info.ShadowCopyFiles = "true";
			else
				info.ShadowCopyFiles = "false";

			return CreateDomain (friendlyName, securityInfo, info);
		}
		
		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		public static extern Assembly LoadFrom (String assemblyFile, Evidence securityEvidence);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private static extern AppDomain getCurDomain ();
		
		public static AppDomain CurrentDomain
		{
			get {
				return getCurDomain ();
			}
		}

		public AssemblyBuilder DefineDynamicAssembly (AssemblyName name,
							      AssemblyBuilderAccess access)
		{
			AssemblyBuilder ab = new AssemblyBuilder (name, access);
			return ab;
		}

		public Assembly Load (AssemblyName assemblyRef)
		{
			return Load (assemblyRef, new Evidence ());
		}

		public Assembly Load (AssemblyName assemblyRef, Evidence assemblySecurity)
		{
			Assembly res;
			
			if ((res = (Assembly)loaded_assemblies [assemblyRef]) != null)
				return res;

			// fixme: we should pass the whole assemblyRef instead of the name,
			// and maybe also the adsetup
			res = LoadFrom (assemblyRef.Name, assemblySecurity);

			loaded_assemblies [assemblyRef] = res;
			
			return res;
		}

		public Assembly Load (string assemblyString)
		{
			AssemblyName an = new AssemblyName ();
			an.Name = assemblyString;
			
			return Load (an, new Evidence ());			
		}

		public Assembly Load (string assemblyString, Evidence assemblySecurity)
		{
			AssemblyName an = new AssemblyName ();
			an.Name = assemblyString;
			
			return Load (an, assemblySecurity);			
		}

		public Assembly Load (byte[] rawAssembly)
		{
			return Load (rawAssembly, null, new Evidence ());
		}

		public Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore)
		{
			return Load (rawAssembly, rawSymbolStore, new Evidence ());
		}

		public Assembly Load (byte[] rawAssembly, byte[] rawSymbolStore, Evidence securityEvidence)
		{
			throw new NotImplementedException ();
		}

		//
		// This returns a list of the assemblies that were loaded in behalf
		// of this AppDomain
		//
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		static private extern Assembly [] getDefaultAssemblies ();
			
		public Assembly[] GetAssemblies ()
		{
			int x = loaded_assemblies.Count;
			Assembly[] res = new Assembly [loaded_assemblies.Count];

			int i = 0;
			foreach (DictionaryEntry de in loaded_assemblies)
				res [i++] = (Assembly) de.Value;
				
			return res;
		}

		// fixme: how does marshalling work ?
		public object GetData (string name)
		{
			switch (name) {
			case "APPBASE":
				return adsetup.ApplicationBase;
			case "APP_CONFIG_FILE":
				return adsetup.ConfigurationFile;
			case "DYNAMIC_BASE":
				return adsetup.DynamicBase;
			case "APP_NAME":
				return adsetup.ApplicationName;
			case "CACHE_BASE":
				return adsetup.CachePath;
			case "PRIVATE_BINPATH":
				return adsetup.PrivateBinPath;
			case "BINPATH_PROBE_ONLY":
				return adsetup.PrivateBinPathProbe;
			case "SHADOW_COPY_DIRS":
				return adsetup.ShadowCopyDirectories;
			case "FORCE_CACHE_INSTALL":
				return adsetup.ShadowCopyFiles;
			}

			return data_hash [name];
		}

		// fixme: how does marshalling work ?
		public void SetData (string name, object data)
		{
			// LAMESPEC: why can't we set adsetup properties ??

			data_hash [name] = data;
		}

	}
}
