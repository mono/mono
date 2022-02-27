//
// System.Web.Configuration.CompilationSection
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (c) Copyright 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Util;

namespace System.Web.Configuration
{
	public sealed class CompilationSection : ConfigurationSection
	{
		static ConfigurationPropertyCollection properties;
		static ConfigurationProperty compilersProp;
		static ConfigurationProperty tempDirectoryProp;
		static ConfigurationProperty debugProp;
		static ConfigurationProperty strictProp;
		static ConfigurationProperty explicitProp;
		static ConfigurationProperty batchProp;
		static ConfigurationProperty batchTimeoutProp;
		static ConfigurationProperty maxBatchSizeProp;
		static ConfigurationProperty maxBatchGeneratedFileSizeProp;
		static ConfigurationProperty numRecompilesBeforeAppRestartProp;
		static ConfigurationProperty defaultLanguageProp;
		static ConfigurationProperty assemblyPostProcessorTypeProp;
		static ConfigurationProperty buildProvidersProp;
		static ConfigurationProperty expressionBuildersProp;
		static ConfigurationProperty urlLinePragmasProp;
		static ConfigurationProperty codeSubDirectoriesProp;
		static ConfigurationProperty optimizeCompilationsProp;
		static ConfigurationProperty targetFrameworkProp;

		private static readonly ConfigurationProperty _propAssemblies =
			new ConfigurationProperty("assemblies", typeof(AssemblyCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

		private static readonly Lazy<ConcurrentDictionary<Assembly, string>> _assemblyNames =
			new Lazy<ConcurrentDictionary<Assembly, string>>();

		private bool _referenceSet;

		private bool _isRuntimeObject = false;

		static CompilationSection ()
		{
			assemblyPostProcessorTypeProp = new ConfigurationProperty ("assemblyPostProcessorType", typeof (string), "");
			batchProp = new ConfigurationProperty ("batch", typeof (bool), true);
			buildProvidersProp = new ConfigurationProperty ("buildProviders", typeof (BuildProviderCollection), null,
									null, PropertyHelper.DefaultValidator,
									ConfigurationPropertyOptions.None);
			batchTimeoutProp = new ConfigurationProperty ("batchTimeout", typeof (TimeSpan), new TimeSpan (0, 15, 0),
								      PropertyHelper.TimeSpanSecondsOrInfiniteConverter,
								      PropertyHelper.PositiveTimeSpanValidator,
								      ConfigurationPropertyOptions.None);
			codeSubDirectoriesProp = new ConfigurationProperty ("codeSubDirectories", typeof (CodeSubDirectoriesCollection), null,
									    null, PropertyHelper.DefaultValidator,
									    ConfigurationPropertyOptions.None);
			compilersProp = new ConfigurationProperty ("compilers", typeof (CompilerCollection), null,
								   null, PropertyHelper.DefaultValidator,
								   ConfigurationPropertyOptions.None);
			debugProp = new ConfigurationProperty ("debug", typeof (bool), false);
			defaultLanguageProp = new ConfigurationProperty ("defaultLanguage", typeof (string), "vb");
			expressionBuildersProp = new ConfigurationProperty ("expressionBuilders", typeof (ExpressionBuilderCollection), null,
									    null, PropertyHelper.DefaultValidator,
									    ConfigurationPropertyOptions.None);
			explicitProp = new ConfigurationProperty ("explicit", typeof (bool), true);
			maxBatchSizeProp = new ConfigurationProperty ("maxBatchSize", typeof (int), 1000);
			maxBatchGeneratedFileSizeProp = new ConfigurationProperty ("maxBatchGeneratedFileSize", typeof (int), 3000);
			numRecompilesBeforeAppRestartProp = new ConfigurationProperty ("numRecompilesBeforeAppRestart", typeof (int), 15);
			strictProp = new ConfigurationProperty ("strict", typeof (bool), false);
			tempDirectoryProp = new ConfigurationProperty ("tempDirectory", typeof (string), "");
			urlLinePragmasProp = new ConfigurationProperty ("urlLinePragmas", typeof (bool), false);

			// This is a 4.0 property but it is also supported in 3.5 with
			// this hotfix: http://support.microsoft.com/kb/961884
			optimizeCompilationsProp = new ConfigurationProperty ("optimizeCompilations", typeof (bool), false);

			// Mono ignores this as there is no way to switch the runtime version
			// dynamically while application is running
			targetFrameworkProp = new ConfigurationProperty ("targetFramework", typeof (string), null);

			properties = new ConfigurationPropertyCollection ();
			properties.Add (_propAssemblies);
			properties.Add (assemblyPostProcessorTypeProp);
			properties.Add (batchProp);
			properties.Add (buildProvidersProp);
			properties.Add (batchTimeoutProp);
			properties.Add (codeSubDirectoriesProp);
			properties.Add (compilersProp);
			properties.Add (debugProp);
			properties.Add (defaultLanguageProp);
			properties.Add (expressionBuildersProp);
			properties.Add (explicitProp);
			properties.Add (maxBatchSizeProp);
			properties.Add (maxBatchGeneratedFileSizeProp);
			properties.Add (numRecompilesBeforeAppRestartProp);
			properties.Add (strictProp);
			properties.Add (tempDirectoryProp);
			properties.Add (urlLinePragmasProp);
			properties.Add (optimizeCompilationsProp);
			properties.Add (targetFrameworkProp);
		}

		public CompilationSection ()
		{
		}

		protected override void PostDeserialize ()
		{
			base.PostDeserialize ();
		}

		protected internal override object GetRuntimeObject() {
			_isRuntimeObject = true;
			return base.GetRuntimeObject();
		}

		[ConfigurationProperty("assemblies")]
		public AssemblyCollection Assemblies {
			get {
				if (_isRuntimeObject || BuildManagerHost.InClientBuildManager) {
					EnsureReferenceSet();
				}
				return GetAssembliesCollection();
			}
		}

		[ConfigurationProperty ("assemblyPostProcessorType", DefaultValue = "")]
		public string AssemblyPostProcessorType {
			get { return (string) base[assemblyPostProcessorTypeProp]; }
			set { base[assemblyPostProcessorTypeProp] = value; }
		}

		[ConfigurationProperty ("batch", DefaultValue = "True")]
		public bool Batch {
			get { return (bool) base [batchProp]; }
			set { base [batchProp] = value; }
		}

		[TypeConverter (typeof (TimeSpanSecondsOrInfiniteConverter))]
		[TimeSpanValidator (MinValueString = "00:00:00")]
		[ConfigurationProperty ("batchTimeout", DefaultValue = "00:15:00")]
		public TimeSpan BatchTimeout {
			get { return (TimeSpan) base [batchTimeoutProp]; }
			set { base [batchTimeoutProp] = value; }
		}

		[ConfigurationProperty ("buildProviders")]
		public BuildProviderCollection BuildProviders {
			get { return (BuildProviderCollection) base [buildProvidersProp]; }
		}

		[ConfigurationProperty ("codeSubDirectories")]
		public CodeSubDirectoriesCollection CodeSubDirectories {
			get { return (CodeSubDirectoriesCollection) base [codeSubDirectoriesProp]; }
		}

		[ConfigurationProperty ("compilers")]
		public CompilerCollection Compilers {
			get { return (CompilerCollection) base [compilersProp]; }
		}

		[ConfigurationProperty ("debug", DefaultValue = "False")]
		public bool Debug {
			get { return (bool) base [debugProp]; }
			set { base [debugProp] = value; }
		}

		[ConfigurationProperty ("defaultLanguage", DefaultValue = "vb")]
		public string DefaultLanguage {
			get { return (string) base [defaultLanguageProp]; }
			set { base [defaultLanguageProp] = value; }
		}

		[ConfigurationProperty ("explicit", DefaultValue = "True")]
		public bool Explicit {
			get { return (bool) base [explicitProp]; }
			set { base [explicitProp] = value; }
		}

		[ConfigurationProperty ("expressionBuilders")]
		public ExpressionBuilderCollection ExpressionBuilders {
			get { return (ExpressionBuilderCollection) base [expressionBuildersProp]; }
		}

		[ConfigurationProperty ("maxBatchGeneratedFileSize", DefaultValue = "1000")]
		public int MaxBatchGeneratedFileSize {
			get { return (int) base [maxBatchGeneratedFileSizeProp]; }
			set { base [maxBatchGeneratedFileSizeProp] = value; }
		}

		[ConfigurationProperty ("maxBatchSize", DefaultValue = "1000")]
		public int MaxBatchSize {
			get { return (int) base [maxBatchSizeProp]; }
			set { base [maxBatchSizeProp] = value; }
		}

		[ConfigurationProperty ("numRecompilesBeforeAppRestart", DefaultValue = "15")]
		public int NumRecompilesBeforeAppRestart {
			get { return (int) base [numRecompilesBeforeAppRestartProp]; }
			set { base [numRecompilesBeforeAppRestartProp] = value; }
		}

		[ConfigurationProperty ("optimizeCompilations", DefaultValue = "False")]
		public bool OptimizeCompilations {
			get { return (bool) base [optimizeCompilationsProp]; }
			set { base [optimizeCompilationsProp] = value; }
		}
		
		[ConfigurationProperty ("strict", DefaultValue = "False")]
		public bool Strict {
			get { return (bool) base [strictProp]; }
			set { base [strictProp] = value; }
		}

		[ConfigurationProperty ("targetFramework", DefaultValue = null)]
		public string TargetFramework {
			get { return (string) base [targetFrameworkProp]; }
			set { base [targetFrameworkProp] = value; }
		}
		
		[ConfigurationProperty ("tempDirectory", DefaultValue = "")]
		public string TempDirectory {
			get { return (string) base [tempDirectoryProp]; }
			set { base [tempDirectoryProp] = value; }
		}

		[ConfigurationProperty ("urlLinePragmas", DefaultValue = "False")]
		public bool UrlLinePragmas {
			get { return (bool) base [urlLinePragmasProp]; }
			set { base [urlLinePragmasProp] = value; }
		}

		protected internal override ConfigurationPropertyCollection Properties {
			get { return properties; }
		}

		private AssemblyCollection GetAssembliesCollection() {
			return (AssemblyCollection)base[_propAssemblies];
		}

		// This will only set the section pointer
		private void EnsureReferenceSet() {
			if (!_referenceSet) {
				foreach (AssemblyInfo ai in GetAssembliesCollection()) {
					ai.SetCompilationReference(this);
				}
				_referenceSet = true;
			}
		}

		internal Assembly[] LoadAssembly(AssemblyInfo ai) {
			Assembly[] assemblies = null;
			if (ai.Assembly == "*") {
				throw new NotImplementedException();
			}
			else {
				Assembly a;
				a = LoadAssemblyHelper(ai.Assembly, false);
				if (a != null) {
					assemblies = new Assembly[1];
					assemblies[0] = a;
					RecordAssembly(ai.Assembly, a);
				}
			}
			return assemblies;
		}

		internal static void RecordAssembly(string assemblyName, Assembly a) {
			// For each Assembly that we load, keep track of its original
			// full name as specified in the config.
			if (!_assemblyNames.Value.ContainsKey(a)) {
				_assemblyNames.Value.TryAdd(a, assemblyName);
			}
		}

		internal Assembly LoadAssembly(string assemblyName, bool throwOnFail) {

			// The trust should always be set before we load any assembly (VSWhidbey 317295)
			// TrustLevel is currently not set on HttpRuntime - reenable once it is
			// System.Web.Util.Debug.Assert(HttpRuntime.TrustLevel != null);

			try {
				// First, try to just load the assembly
				Assembly a = Assembly.Load(assemblyName);
				// Record the original assembly name that was used to load this assembly.
				RecordAssembly(assemblyName, a);
				return a;
			}
			catch {
				AssemblyName asmName = new AssemblyName(assemblyName);

				// Check if it's simply named
				Byte[] publicKeyToken = asmName.GetPublicKeyToken();
				if ((publicKeyToken == null || publicKeyToken.Length == 0) && asmName.Version == null) {

					EnsureReferenceSet();

					// It is simply named.  Go through all the assemblies from
					// the <assemblies> section, and if we find one that matches
					// the simple name, return it (ASURT 100546)
					foreach (AssemblyInfo ai in GetAssembliesCollection()) {
						Assembly[] a = ai.AssemblyInternal;
						if (a != null) {
							for (int i = 0; i < a.Length; i++) {
								// use new AssemblyName(FullName).Name
								// instead of a.GetName().Name, because GetName() does not work in medium trust
								if (StringUtil.EqualsIgnoreCase(asmName.Name, new AssemblyName(a[i].FullName).Name)) {
									return a[i];
								}
							}
						}
					}
				}

				if (throwOnFail) {
					throw;
				}
			}

			return null;
		}

		private Assembly LoadAssemblyHelper(string assemblyName, bool starDirective) {
			// The trust should always be set before we load any assembly (VSWhidbey 317295)
			// TrustLevel is currently not set on HttpRuntime - reenable once it is
			// System.Web.Util.Debug.Assert(HttpRuntime.TrustLevel != null);

			Assembly retAssembly = null;
			// Load the assembly and add it to the dictionary.
			try {
				retAssembly = System.Reflection.Assembly.Load(assemblyName);
			}
			catch (Exception e) {

				// Check if this assembly came from the '*' directive
				bool ignoreException = false;

				if (starDirective) {
					int hresult = System.Runtime.InteropServices.Marshal.GetHRForException(e);

					// This is expected to fail for unmanaged DLLs that happen
					// to be in the bin dir.  Ignore them.

					// Also, if the DLL is not an assembly, ignore the exception (ASURT 93073, VSWhidbey 319486)

					// Test for COR_E_ASSEMBLYEXPECTED=0x80131018=-2146234344
					if (hresult == -2146234344) {
						ignoreException = true;
					}
				}

				/*
				// unimplemented
				if (BuildManager.IgnoreBadImageFormatException) {
					var badImageFormatException = e as BadImageFormatException;
					if (badImageFormatException != null) {
						ignoreException = true;
					}
				}
				*/

				if (!ignoreException) {
					string Message = e.Message;
					if (String.IsNullOrEmpty(Message)) {
						// try and make a better message than empty string
						if (e is FileLoadException) {
							Message = SR.GetString(SR.Config_base_file_load_exception_no_message, "assembly");
						}
						else if (e is BadImageFormatException) {
							Message = SR.GetString(SR.Config_base_bad_image_exception_no_message, assemblyName);
						}
						else {
							Message = SR.GetString(SR.Config_base_report_exception_type, e.GetType().ToString()); // at least this is better than no message
						}
					}
					// default to section if the assembly is not in the collection
					// which may happen it the assembly is being loaded from the bindir
					// and not named in configuration.
					String source = ElementInformation.Properties["assemblies"].Source;
					int lineNumber = ElementInformation.Properties["assemblies"].LineNumber;

					// If processing the * directive, look up the line information for it
					if (starDirective)
						assemblyName = "*";

					if (Assemblies[assemblyName] != null) {
						source = Assemblies[assemblyName].ElementInformation.Source;
						lineNumber = Assemblies[assemblyName].ElementInformation.LineNumber;
					}
					throw new ConfigurationErrorsException(Message, e, source, lineNumber);
				}
			}

			System.Web.Util.Debug.Trace("LoadAssembly", "Successfully loaded assembly '" + assemblyName + "'");

			return retAssembly;
		}
	}
}

