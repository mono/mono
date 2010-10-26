//
// System.AppDomainSetup.cs
//
// Authors:
//	Dietmar Maurer (dietmar@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//
// Known Problems:
//    	Fix serialization compatibility with MS.NET.
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
using System.Security;

#if NET_2_0 && (!NET_2_1 || MONOTOUCH) && !MICRO_LIB
using System.Runtime.Hosting;
using System.Security.Policy;
#endif

namespace System
{
	[Serializable]
	[ClassInterface (ClassInterfaceType.None)]
#if NET_2_0
	[ComVisible (true)]
#endif
#if NET_2_1
	public sealed class AppDomainSetup
#else
	public sealed class AppDomainSetup : IAppDomainSetup
#endif
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

		// those fields also exist in the runtime, so we need dummies in 1.x profile too.
#if NET_2_0 && (!NET_2_1 || MONOTOUCH) && !MICRO_LIB
		private ActivationArguments _activationArguments;
		AppDomainInitializer domain_initializer;
		[NonSerialized]
		ApplicationTrust application_trust;
		string [] domain_initializer_args;
		#if !DISABLE_SECURITY
		SecurityElement application_trust_xml;
		#endif
#else
		object _activationArguments;
		object domain_initializer; // always null
		object application_trust;
		object domain_initializer_args;
		object application_trust_xml;
#endif
		bool disallow_appbase_probe;
		byte [] configuration_bytes;

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
//#if NET_2_0
			_activationArguments = setup._activationArguments;
			domain_initializer = setup.domain_initializer;
			domain_initializer_args = setup.domain_initializer_args;
			#if !DISABLE_SECURITY
			application_trust_xml = setup.application_trust_xml;
			#endif
			disallow_appbase_probe = setup.disallow_appbase_probe;
			configuration_bytes = setup.configuration_bytes;
//#endif
		}

#if NET_2_0 && (!NET_2_1 || MONOTOUCH) && !MICRO_LIB
		public AppDomainSetup (ActivationArguments activationArguments)
		{
			_activationArguments = activationArguments;
		}

		public AppDomainSetup (ActivationContext activationContext)
		{
			_activationArguments = new ActivationArguments (activationContext);
		}
#endif

		static string GetAppBase (string appBase)
		{
			if (appBase == null)
				return null;

			int len = appBase.Length;
			if (len >= 8 && appBase.ToLower ().StartsWith ("file://")) {
				appBase = appBase.Substring (7);
				if (Path.DirectorySeparatorChar != '/')
					appBase = appBase.Replace ('/', Path.DirectorySeparatorChar);
				if (Environment.IsRunningOnWindows) {
					// Under Windows prepend "//" to indicate it's a local file
					appBase = "//" + appBase;
				}
#if NET_2_0
			} else {
#else
			// under 1.x the ":" gets a special treatment - but it doesn't make sense outside Windows
			} else if (!Environment.IsRunningOnWindows || (appBase.IndexOf (':') == -1)) {
#endif
				appBase = Path.GetFullPath (appBase);
			}

			return appBase;
		}

		public string ApplicationBase {
			get { return GetAppBase (application_base); }
			set { application_base = value; } 
		}

		public string ApplicationName {
			get {
				return application_name;
			}
			set {
				application_name = value;
			}
		}
#if !NET_2_1 || MONOTOUCH
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
				if (configuration_file == null)
					return null;
				if (Path.IsPathRooted(configuration_file))
					return configuration_file;
				if (ApplicationBase == null)
					throw new MemberAccessException("The ApplicationBase must be set before retrieving this property.");
				return Path.Combine(ApplicationBase, configuration_file);
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
#endif
		[MonoLimitation ("In Mono this is controlled by the --share-code flag")]
		public LoaderOptimization LoaderOptimization {
			get {
				return loader_optimization;
			}
			set {
				loader_optimization = value;
			}
		}
#if !NET_2_1 || MONOTOUCH
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

#if NET_2_0 && !MICRO_LIB
		public ActivationArguments ActivationArguments {
			get { return _activationArguments; }
			set { _activationArguments = value; }
		}

		[MonoLimitation ("it needs to be invoked within the created domain")]
		public AppDomainInitializer AppDomainInitializer {
			get { return domain_initializer; }
			set { domain_initializer = value; }
		}

		[MonoLimitation ("it needs to be used to invoke the initializer within the created domain")]
		public string [] AppDomainInitializerArguments {
			get { return domain_initializer_args; }
			set { domain_initializer_args = value; }
		}

		[MonoNotSupported ("This property exists but not considered.")]
		public ApplicationTrust ApplicationTrust {
			get {
				if (application_trust_xml == null)
					return null;
				if (application_trust == null)
					application_trust = new ApplicationTrust ();
				return application_trust;
			}
			set {
				application_trust = value;
				if (value != null) {
					application_trust_xml = value.ToXml ();
					application_trust.FromXml (application_trust_xml);
				}
				else
					application_trust_xml = null;
			}
		}

		[MonoNotSupported ("This property exists but not considered.")]
		public bool DisallowApplicationBaseProbing {
			get { return disallow_appbase_probe; }
			set { disallow_appbase_probe = value; }
		}

		[MonoNotSupported ("This method exists but not considered.")]
		public byte [] GetConfigurationBytes ()
		{
			return configuration_bytes != null ? configuration_bytes.Clone () as byte [] : null;
		}

		[MonoNotSupported ("This method exists but not considered.")]
		public void SetConfigurationBytes (byte [] value)
		{
			configuration_bytes = value;
		}
#endif
#endif // !NET_2_1
	}
}
