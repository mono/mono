//
// System/AppDomainSetup.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System {


	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	public sealed class AppDomainSetup : IAppDomainSetup {
		string application_base;
		string application_name;
		string cache_path;
		string configuration_file;
		string dynamic_base;
		string license_file;
		string private_bin_path;
		string private_bin_path_probe;
		string shadow_copy_directories;
		string shadow_copy_files;
		bool publisher_policy;
		private bool path_changed;
		private LoaderOptimization loader_optimization;
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern AppDomainSetup InitAppDomainSetup (AppDomainSetup setup);

		public AppDomainSetup ()
		{
			InitAppDomainSetup (this);
		}
		
		
		static string GetAppBase (string appBase)
		{
			int len = appBase.Length;
			if (len >= 8 && appBase.ToLower ().StartsWith ("file://"))
				appBase = appBase.Substring (7);

			return appBase;
		}
		
		public string ApplicationBase {

			get {
				return GetAppBase (application_base);
			}

			set {
				application_base = value;
			}
		}

		public string ApplicationName {

			get {
				return application_name;
			}

			set {
				application_name = value;
			}
		}

		public string CachePath {

			get {
				return cache_path;
			}

			set {
				cache_path = value;
			}
		}

		public string ConfigurationFile {

			get {
				return configuration_file;
			}

			set {
				configuration_file = value;
			}
		}

		public bool DisallowPublisherPolicy {
			get {
				return publisher_policy;
			}

			set {
				publisher_policy = value;
			}
		}
		
		public string DynamicBase {

			get {
				return dynamic_base;
			}

			set {
				dynamic_base = value;
			}
		}

		public string LicenseFile {

			get {
				return license_file;
			}

			set {
				license_file = value;
			}
		}

		[MonoTODO("--share-code")]
		public LoaderOptimization LoaderOptimization
		{
			get {
				return loader_optimization;
			}

			set { 
				loader_optimization = value;
			}
		}

		public string PrivateBinPath {

			get {
				return private_bin_path;
			}

			set {
				private_bin_path = value;
				path_changed = true;
			}
		}

		public string PrivateBinPathProbe {

			get {
				return private_bin_path_probe;
			}

			set {
				private_bin_path_probe = value;
				path_changed = true;
			}
		}

		public string ShadowCopyDirectories {

			get {
				return shadow_copy_directories;
			}

			set {
				shadow_copy_directories = value;
			}
		}

		public string ShadowCopyFiles {

			get {
				return shadow_copy_files;
			}

			set {
				shadow_copy_files = value;
			}
		}
		
	}
}
