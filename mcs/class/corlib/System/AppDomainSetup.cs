
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

namespace System {


	public sealed class AppDomainSetup : IAppDomainSetup {

		private string application_base;
		private string application_name;
		private string cache_path;
		private string configuration_file;
		private string dynamic_base;
		private string license_file;
		private string private_bin_path;
		private string private_bin_path_probe;
		private string shadow_copy_directories;
		private string shadow_copy_files;

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern AppDomainSetup InitAppDomainSetup (AppDomainSetup setup);

		public AppDomainSetup ()
		{
			InitAppDomainSetup (this);
		}
		
		public string ApplicationBase {

			get {
				return application_base;
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

		public string PrivateBinPath {

			get {
				return private_bin_path;
			}

			set {
				private_bin_path = value;
			}
		}

		public string PrivateBinPathProbe {

			get {
				return private_bin_path_probe;
			}

			set {
				private_bin_path_probe = value;
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
