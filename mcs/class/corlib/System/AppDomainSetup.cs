//
// System.AppDomainSetup.cs
//
// Author:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
	[Serializable]
	[ClassInterface (ClassInterfaceType.None)]
	[MonoTODO ("Fix serialization compatibility with MS.NET")]
	public sealed class AppDomainSetup : IAppDomainSetup
	{
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
		bool disallow_binding_redirects;
		bool disallow_code_downloads;

		public AppDomainSetup ()
		{
		}

		internal AppDomainSetup (AppDomainSetup setup)
		{
			application_base = setup.application_base;
			application_name = setup.application_name;
			cache_path = setup.cache_path;
			configuration_file = setup.configuration_file;
			dynamic_base = setup.dynamic_base;
			license_file = setup.license_file;
			private_bin_path = setup.private_bin_path;
			private_bin_path_probe = setup.private_bin_path_probe;
			shadow_copy_directories = setup.shadow_copy_directories;
			shadow_copy_files = setup.shadow_copy_files;
			publisher_policy = setup.publisher_policy;
			path_changed = setup.path_changed;
			loader_optimization = setup.loader_optimization;
			disallow_binding_redirects = setup.disallow_binding_redirects;
			disallow_code_downloads = setup.disallow_code_downloads;
		}

		static string GetAppBase (string appBase)
		{
			if (appBase == null) return null;
			int len = appBase.Length;
			if (len >= 8 && appBase.ToLower ().StartsWith ("file://")) {
				appBase = appBase.Substring (7);
				if (Path.DirectorySeparatorChar != '/')
					appBase = appBase.Replace ('/', Path.DirectorySeparatorChar);

			} else if (appBase.IndexOf (':') == -1) {
				appBase = Path.GetFullPath (appBase);
			}

			return appBase;
		}
		
		public string ApplicationBase {
			get {
				return application_base;
			}
			set {
				application_base = GetAppBase (value);
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
				if (dynamic_base == null)
					return null;

				if (Path.IsPathRooted (dynamic_base))
					return dynamic_base;

				if (ApplicationBase == null)
					throw new MemberAccessException ("The ApplicationBase must be set before retrieving this property.");
				
				return Path.Combine (ApplicationBase, dynamic_base);
			}
			set {
				if (application_name == null)
					throw new MemberAccessException ("ApplicationName must be set before the DynamicBase can be set.");
				uint id = (uint) application_name.GetHashCode ();
				dynamic_base = Path.Combine (value, id.ToString("x"));
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

		[MonoTODO ("--share-code")]
		public LoaderOptimization LoaderOptimization {
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

#if NET_1_1
		public bool DisallowBindingRedirects {
			get {
				return disallow_binding_redirects;
			}
			set {
				disallow_binding_redirects = value;
			}
		}

		public bool DisallowCodeDownload {
			get {
				return disallow_code_downloads;
			}
			set {
				disallow_code_downloads = value;
			}
		}
#endif
	}
}
