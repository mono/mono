//------------------------------------------------------------------------------
// <copyright file="CompilationSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Concurrent;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Web.Compilation;
    using System.Reflection;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.CodeDom.Compiler;
    using System.Web.Util;
    using System.Threading;
    using System.ComponentModel;
    using System.Security.Permissions;

    /*
        <!-- compilation Attributes:
          tempDirectory="directory"
          debug="[true|false]"      // Default: false
          strict="[true|false]"     // Default: false
          explicit="[true|false]"   // Default: true   !!! This follow how it was defined in Machine.config
          batch="[true|false]"      // Default: true
          batchTimeout="timeout in seconds"     // Default: 15 seconds
          maxBatchSize="max number of pages per batched compilation"    // Default: 1000 classes
          maxBatchGeneratedFileSize="max combined size (in KB) of the generated source files per batched compilation"   // Default: 3000KB
          numRecompilesBeforeAppRestart="max number of recompilations before appdomain is cycled"    // Default: 15 recomplations
          defaultLanguage="name of a language as specified in a <compiler/> tag below"      // Default: VB
          urlLinePragmas="[true|false]"     // Default: false, meaning pragmas use physical paths
          targetFramework="target framework moniker" eg ".NET Framework,Version=v4.0" // Default: null
          controlBuilderInterceptorType="type name of ControlBuilderInterceptor implementation"  //Default: ""
          disableObsoleteWarnings="[true|false]" // Default: true
          maxConcurrentCompilations="max number of concurrent assemblyBuilder compilations" // Default: 1
        -->
        <compilation debug="false" explicit="true" defaultLanguage="vb">

            <!-- codeSubDirectories example:
            Note that this section is only valid in web.config in the application root.
            <codeSubDirectories>
                <add directoryName="CodeSubDir1" />
                <add directoryName="CodeSubDir2" />
            </codeSubDirectories>
            -->

            <buildProviders>
                <add extension=".aspx" type="System.Web.Compilation.PageBuildProvider" />
                <add extension=".ascx" type="System.Web.Compilation.UserControlBuildProvider" />
                <add extension=".master" type="System.Web.Compilation.MasterPageBuildProvider" />
                <add extension=".asix" type="System.Web.Compilation.ImageGeneratorBuildProvider" />
                <add extension=".asmx" type="System.Web.Compilation.WebServiceBuildProvider" />
                <add extension=".ashx" type="System.Web.Compilation.WebHandlerBuildProvider" />
                <add extension=".soap" type="System.Web.Compilation.WebServiceBuildProvider" />
                <add extension=".resx" type="System.Web.Compilation.ResXBuildProvider" />
                <add extension=".resources" type="System.Web.Compilation.ResourcesBuildProvider" />
                <add extension=".wsdl" type="System.Web.Compilation.WsdlBuildProvider" />
                <add extension=".xsd" type="System.Web.Compilation.XsdBuildProvider" />
            </buildProviders>

            <!-- folderLevelBuildProviders example:
            <folderLevelBuildProviders>
                <add type="MyBuildProvider, MyAssembly,Version=1.0.0.0,PublicKeyToken=TOKEN" />
            </folderLevelBuildProviders>
            -->

            <assemblies>
                <add assembly="mscorlib" />
                <add assembly="System, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%ECMA_PUBLICKEY%" />
                <add assembly="System.Configuration, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%MICROSOFT_PUBLICKEY%" />
                <add assembly="System.Web, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%MICROSOFT_PUBLICKEY%" />
                <add assembly="System.Data, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%ECMA_PUBLICKEY%" />
                <add assembly="System.Web.Services, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%MICROSOFT_PUBLICKEY%" />
                <add assembly="System.Xml, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%ECMA_PUBLICKEY%" />
                <add assembly="System.Drawing, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%MICROSOFT_PUBLICKEY%" />
                <add assembly="System.EnterpriseServices, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%MICROSOFT_PUBLICKEY%" />
                <add assembly="System.Web.Mobile, Version=%ASSEMBLY_VERSION%, Culture=neutral, PublicKeyToken=%MICROSOFT_PUBLICKEY%" />
                <add assembly="*" />
            </assemblies>

            <expressionBuilders>
                <add expressionPrefix="Resources" type="System.Web.Compilation.ResourceExpressionBuilder" />
                <add expressionPrefix="ConnectionStrings" type="System.Web.Compilation.ConnectionStringsExpressionBuilder" />
                <add expressionPrefix="AppSettings" type="System.Web.Compilation.AppSettingsExpressionBuilder" />
            </expressionBuilders>

        </compilation>
*/

    public sealed class CompilationSection : ConfigurationSection {
        private const string tempDirectoryAttributeName = "tempDirectory";
        private const string assemblyPostProcessorTypeAttributeName = "assemblyPostProcessorType";
        private const string controlBuilderInterceptorTypeAttributeName = "controlBuilderInterceptorType";

        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propTempDirectory =
            new ConfigurationProperty(tempDirectoryAttributeName, typeof(string), 
                                        String.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDebug =
            new ConfigurationProperty("debug", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propStrict =
            new ConfigurationProperty("strict", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propExplicit =
            new ConfigurationProperty("explicit", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propBatch =
            new ConfigurationProperty("batch", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propOptimizeCompilations =
            new ConfigurationProperty("optimizeCompilations", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propBatchTimeout =
            new ConfigurationProperty("batchTimeout",
                                        typeof(TimeSpan),
                                        TimeSpan.FromMinutes(15.0),
                                        StdValidatorsAndConverters.TimeSpanSecondsOrInfiniteConverter,
                                        StdValidatorsAndConverters.PositiveTimeSpanValidator,
                                        ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxBatchSize =
            new ConfigurationProperty("maxBatchSize", typeof(int), 1000, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxBatchGeneratedFileSize =
            new ConfigurationProperty("maxBatchGeneratedFileSize", typeof(int), 1000, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propNumRecompilesBeforeAppRestart =
            new ConfigurationProperty("numRecompilesBeforeAppRestart", typeof(int), 15, ConfigurationPropertyOptions.None);
#if !FEATURE_PAL // FEATURE_PAL does not support VisualBasic
        private static readonly ConfigurationProperty _propDefaultLanguage =
            new ConfigurationProperty("defaultLanguage", typeof(string), "vb", ConfigurationPropertyOptions.None);
#else // !FEATURE_PAL
        private static readonly ConfigurationProperty _propDefaultLanguage =
            new ConfigurationProperty("defaultLanguage", typeof(string),"c#",ConfigurationPropertyFlags.None);
#endif // !FEATURE_PAL
        private static readonly ConfigurationProperty _propTargetFramework =
            new ConfigurationProperty("targetFramework", typeof(string), null, ConfigurationPropertyOptions.None);

        private static readonly ConfigurationProperty _propCompilers =
            new ConfigurationProperty("compilers", typeof(CompilerCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propAssemblies =
            new ConfigurationProperty("assemblies", typeof(AssemblyCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propBuildProviders =
            new ConfigurationProperty("buildProviders", typeof(BuildProviderCollection),
                                        null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propFolderLevelBuildProviders =
            new ConfigurationProperty("folderLevelBuildProviders", typeof(FolderLevelBuildProviderCollection),
                                        null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propExpressionBuilders =
            new ConfigurationProperty("expressionBuilders", typeof(ExpressionBuilderCollection),
                                        null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propUrlLinePragmas =
            new ConfigurationProperty("urlLinePragmas", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propCodeSubDirs =
            new ConfigurationProperty("codeSubDirectories", typeof(CodeSubDirectoriesCollection),
                                        null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static readonly ConfigurationProperty _propAssemblyPreprocessorType =
            new ConfigurationProperty(assemblyPostProcessorTypeAttributeName, typeof(string), 
                                        String.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propEnablePrefetchOptimization =
            new ConfigurationProperty("enablePrefetchOptimization", typeof(bool), false, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propProfileGuidedOptimizations =
            new ConfigurationProperty("profileGuidedOptimizations", typeof(ProfileGuidedOptimizationsFlags), 
                                        ProfileGuidedOptimizationsFlags.All, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propControlBuilderInterceptorType =
            new ConfigurationProperty(controlBuilderInterceptorTypeAttributeName, typeof(string),
                                        String.Empty, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propDisableObsoleteWarnings =
            new ConfigurationProperty("disableObsoleteWarnings", typeof(bool),
                                        true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propMaxConcurrentCompilations =
            new ConfigurationProperty("maxConcurrentCompilations", typeof(int), 1, ConfigurationPropertyOptions.None);

        const char fieldSeparator = ';';

        private bool _referenceSet;

        // _compilerLanguages : Hashtable <string, CompilerType>
        // NOTE: This hashtable may contain either Compiler objects or CompilerType objects.
        // It'll contain the later if the data has been read from config, but the particular
        // language compiler type hasn't been resolved yet.  Otherwise, it'll contain CompilerType objects.
        private Hashtable _compilerLanguages;

        // _compilerExtensions : Hashtable <string, CompilerType>
        // NOTE: This hashtable may contain either Compiler objects or CompilerType objects.
        // It'll contain the later if the data has been read from config, but the particular
        // language compiler type hasn't been resolved yet.  Otherwise, it'll contain CompilerType objects.
        private Hashtable _compilerExtensions;

        private long _recompilationHash = -1;

        private bool _isRuntimeObject = false;

        private Type _assemblyPostProcessorType;
        private Type _controlBuilderInterceptorType;

        private static readonly Lazy<ConcurrentDictionary<Assembly, string>> _assemblyNames =
            new Lazy<ConcurrentDictionary<Assembly, string>>();

        static CompilationSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propTempDirectory);
            _properties.Add(_propDebug);
            _properties.Add(_propStrict);
            _properties.Add(_propExplicit);
            _properties.Add(_propBatch);
            _properties.Add(_propOptimizeCompilations);
            _properties.Add(_propBatchTimeout);
            _properties.Add(_propMaxBatchSize);
            _properties.Add(_propMaxBatchGeneratedFileSize);
            _properties.Add(_propNumRecompilesBeforeAppRestart);
            _properties.Add(_propDefaultLanguage);
            _properties.Add(_propTargetFramework);
            _properties.Add(_propCompilers);
            _properties.Add(_propAssemblies);
            _properties.Add(_propBuildProviders);
            _properties.Add(_propFolderLevelBuildProviders);
            _properties.Add(_propExpressionBuilders);
            _properties.Add(_propUrlLinePragmas);
            _properties.Add(_propCodeSubDirs);
            _properties.Add(_propAssemblyPreprocessorType);
            _properties.Add(_propEnablePrefetchOptimization);
            _properties.Add(_propProfileGuidedOptimizations);
            _properties.Add(_propControlBuilderInterceptorType);
            _properties.Add(_propDisableObsoleteWarnings);
            _properties.Add(_propMaxConcurrentCompilations);
        }

        public CompilationSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        protected override object GetRuntimeObject() {
            _isRuntimeObject = true;
            return base.GetRuntimeObject();
        }

        [ConfigurationProperty(tempDirectoryAttributeName, DefaultValue = "")]
        public string TempDirectory {
            get {
                return (string)base[_propTempDirectory];
            }
            set {
                base[_propTempDirectory] = value;
            }
        }

        // Used for error handling when there is a problem withe the temp dir
        internal void GetTempDirectoryErrorInfo(out string tempDirAttribName,
            out string configFileName, out int configLineNumber) {
            tempDirAttribName = tempDirectoryAttributeName;
            configFileName = ElementInformation.Properties[tempDirectoryAttributeName].Source;
            configLineNumber = ElementInformation.Properties[tempDirectoryAttributeName].LineNumber;
        }

        [ConfigurationProperty("debug", DefaultValue = false)]
        public bool Debug {
            get {
                return (bool)base[_propDebug];
            }
            set {
                base[_propDebug] = value;
            }
        }

        [ConfigurationProperty("strict", DefaultValue = false)]
        public bool Strict {
            get {
                return (bool)base[_propStrict];
            }
            set {
                base[_propStrict] = value;
            }
        }

        [ConfigurationProperty("explicit", DefaultValue = true)]
        public bool Explicit {
            get {
                return (bool)base[_propExplicit];
            }
            set {
                base[_propExplicit] = value;
            }
        }

        [ConfigurationProperty("batch", DefaultValue = true)]
        public bool Batch {
            get {
                return (bool)base[_propBatch];
            }
            set {
                base[_propBatch] = value;
            }
        }

        [ConfigurationProperty("optimizeCompilations", DefaultValue = false)]
        public bool OptimizeCompilations {
            get {
                return (bool)base[_propOptimizeCompilations];
            }
            set {
                base[_propOptimizeCompilations] = value;
            }
        }

        [ConfigurationProperty("urlLinePragmas", DefaultValue = false)]
        public bool UrlLinePragmas {
            get {
                return (bool)base[_propUrlLinePragmas];
            }
            set {
                base[_propUrlLinePragmas] = value;
            }
        }

        [ConfigurationProperty("batchTimeout", DefaultValue = "00:15:00")]
        [TimeSpanValidator(MinValueString="00:00:00", MaxValueString=TimeSpanValidatorAttribute.TimeSpanMaxValue)]
        [TypeConverter(typeof(TimeSpanSecondsOrInfiniteConverter))]
        public TimeSpan BatchTimeout {
            get {
                return (TimeSpan)base[_propBatchTimeout];
            }
            set {
                base[_propBatchTimeout] = value;
            }
        }

        [ConfigurationProperty("maxBatchSize", DefaultValue = 1000)]
        public int MaxBatchSize {
            get {
                return (int)base[_propMaxBatchSize];
            }
            set {
                base[_propMaxBatchSize] = value;
            }
        }

        [ConfigurationProperty("maxBatchGeneratedFileSize", DefaultValue = 1000)]
        public int MaxBatchGeneratedFileSize {
            get {
                return (int)base[_propMaxBatchGeneratedFileSize];
            }
            set {
                base[_propMaxBatchGeneratedFileSize] = value;
            }
        }

        [ConfigurationProperty("numRecompilesBeforeAppRestart", DefaultValue = 15)]
        public int NumRecompilesBeforeAppRestart {
            get {
                return (int)base[_propNumRecompilesBeforeAppRestart];
            }
            set {
                base[_propNumRecompilesBeforeAppRestart] = value;
            }
        }

        [ConfigurationProperty("defaultLanguage", DefaultValue = "vb")]
        public string DefaultLanguage {
            get {
                return (string)base[_propDefaultLanguage];
            }
            set {
                base[_propDefaultLanguage] = value;
            }
        }

        [ConfigurationProperty("targetFramework", DefaultValue = null)]
        public string TargetFramework {
            get {
                return (string)base[_propTargetFramework];
            }
            set {
                base[_propTargetFramework] = value;
            }
        }

        [ConfigurationProperty("compilers")]
        public CompilerCollection Compilers {
            get {
                return (CompilerCollection)base[_propCompilers];
            }
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

        private AssemblyCollection GetAssembliesCollection() {
            return (AssemblyCollection)base[_propAssemblies];
        }

        [ConfigurationProperty("buildProviders")]
        public BuildProviderCollection BuildProviders {
            get {
                return (BuildProviderCollection)base[_propBuildProviders];
            }
        }

        private FolderLevelBuildProviderCollection GetFolderLevelBuildProviders() {
            return (FolderLevelBuildProviderCollection)base[_propFolderLevelBuildProviders];
        }

        [ConfigurationProperty("folderLevelBuildProviders")]
        public FolderLevelBuildProviderCollection FolderLevelBuildProviders {
            get {
                return GetFolderLevelBuildProviders();
            }
        }

        [ConfigurationProperty("expressionBuilders")]
        public ExpressionBuilderCollection ExpressionBuilders {
            get {
                return (ExpressionBuilderCollection)base[_propExpressionBuilders];
            }

        }

        [ConfigurationProperty(assemblyPostProcessorTypeAttributeName, DefaultValue = "")]
        public string AssemblyPostProcessorType {
            get {
                return (string)base[_propAssemblyPreprocessorType];
            }
            set {
                base[_propAssemblyPreprocessorType] = value;
            }
        }

        internal Type AssemblyPostProcessorTypeInternal {
            get {
                if (_assemblyPostProcessorType == null && !String.IsNullOrEmpty(AssemblyPostProcessorType)) {
                    lock (this) {
                        if (_assemblyPostProcessorType == null) {

                            // Only allow this in full trust
                            if (!HttpRuntime.HasUnmanagedPermission()) {
                                throw new ConfigurationErrorsException(SR.GetString(SR.Insufficient_trust_for_attribute, assemblyPostProcessorTypeAttributeName),
                                    ElementInformation.Properties[assemblyPostProcessorTypeAttributeName].Source,
                                    ElementInformation.Properties[assemblyPostProcessorTypeAttributeName].LineNumber);
                            }

                            Type assemblyPostProcessorType = ConfigUtil.GetType(AssemblyPostProcessorType, assemblyPostProcessorTypeAttributeName, this);
                            ConfigUtil.CheckBaseType(typeof(System.Web.Compilation.IAssemblyPostProcessor),
                                assemblyPostProcessorType, "assemblyPostProcessorType", this);
                            _assemblyPostProcessorType = assemblyPostProcessorType;
                        }
                    }
                }

                return _assemblyPostProcessorType;
            }
        }

        [ConfigurationProperty("codeSubDirectories")]
        public CodeSubDirectoriesCollection CodeSubDirectories {
            get {
                return (CodeSubDirectoriesCollection)base[_propCodeSubDirs];
            }

        }

        [ConfigurationProperty("enablePrefetchOptimization", DefaultValue = false)]
        public bool EnablePrefetchOptimization {
            get {
                return (bool)base[_propEnablePrefetchOptimization];
            }
            set {
                base[_propEnablePrefetchOptimization] = value;
            }
        }

        [ConfigurationProperty("profileGuidedOptimizations", DefaultValue = ProfileGuidedOptimizationsFlags.All)]
        public ProfileGuidedOptimizationsFlags ProfileGuidedOptimizations {
            get {
                return (ProfileGuidedOptimizationsFlags)base[_propProfileGuidedOptimizations];
            }
            set {
                base[_propProfileGuidedOptimizations] = value;
            }
        }

        [ConfigurationProperty(controlBuilderInterceptorTypeAttributeName, DefaultValue = "")]
        public string ControlBuilderInterceptorType {
            get {
                return (string)base[_propControlBuilderInterceptorType];
            }
            set {
                base[_propControlBuilderInterceptorType] = value;
            }
        }

        [ConfigurationProperty("disableObsoleteWarnings", DefaultValue = true)]
        public bool DisableObsoleteWarnings {
            get {
                return (bool)base[_propDisableObsoleteWarnings];
            }
            set {
                base[_propDisableObsoleteWarnings] = value;
            }
        }

        [ConfigurationProperty("maxConcurrentCompilations", DefaultValue = 1)]
        public int MaxConcurrentCompilations {
            get {
                return (int)base[_propMaxConcurrentCompilations];
            }
            set {
                base[_propMaxConcurrentCompilations] = value;
            }
        }

        private void EnsureCompilerCacheInit() {
            if (_compilerLanguages == null) {
                lock (this) {
                    if (_compilerLanguages == null) {
                        Hashtable compilerLanguages = new Hashtable(StringComparer.OrdinalIgnoreCase);
                        _compilerExtensions = new Hashtable(StringComparer.OrdinalIgnoreCase);

                        foreach (Compiler compiler in Compilers) {
                            // Parse the semicolon separated lists
                            string[] languageList = compiler.Language.Split(fieldSeparator);
                            string[] extensionList = compiler.Extension.Split(fieldSeparator);

                            foreach (string language in languageList) {
                                compilerLanguages[language] = compiler;
                            }

                            foreach (string extension in extensionList) {
                                _compilerExtensions[extension] = compiler;
                            }
                        }

                        // Only assign it at the end to make sure everything was successful
                        _compilerLanguages = compilerLanguages;
                    }
                }
            }
        }

        /*
         * Return a CompilerType that a extension maps to.
         */
        internal CompilerType GetCompilerInfoFromExtension(string extension, bool throwOnFail) {
            EnsureCompilerCacheInit();

            // First, try the cache (i.e. old <compilers> section)
            CompilerType compilerType;
            object obj = _compilerExtensions[extension];
            Compiler compiler = obj as Compiler;

            if (compiler != null) {
                compilerType = compiler.CompilerTypeInternal;
                _compilerExtensions[extension] = compilerType;
            }
            else {
                compilerType = obj as CompilerType;
            }

            if (compilerType == null) {

                // If not, try the <codedom> section

                if (CodeDomProvider.IsDefinedExtension(extension)) {
                    string language = CodeDomProvider.GetLanguageFromExtension(extension);

                    CompilerInfo ci = CodeDomProvider.GetCompilerInfo(language);

                    compilerType = new CompilerType(
                        ci.CodeDomProviderType, ci.CreateDefaultCompilerParameters());

                    // Cache it
                    _compilerExtensions[extension] = compilerType;
                }
            }

            if (compilerType == null) {
                if (!throwOnFail) return null;

                // Unsupported extension: throw an exception
                throw new HttpException(SR.GetString(SR.Invalid_lang_extension, extension));
            }

            // Clone it so the original is not modified
            compilerType = compilerType.Clone();

            // Set the value of the debug flag in the copy
            compilerType.CompilerParameters.IncludeDebugInformation = Debug;

            return compilerType;
        }

        /*
         * Return a CompilerType that a language maps to.
         */
        internal CompilerType GetCompilerInfoFromLanguage(string language) {
            EnsureCompilerCacheInit();

            // First, try the cache (i.e. old <compilers> section)
            CompilerType compilerType;
            object obj = _compilerLanguages[language];
            Compiler compiler = obj as Compiler;

            if (compiler != null) {
                compilerType = compiler.CompilerTypeInternal;
                _compilerLanguages[language] = compilerType;
            }
            else {
                compilerType = obj as CompilerType;
            }

            if (compilerType == null) {

                // Try the <codedom> section

                if (CodeDomProvider.IsDefinedLanguage(language)) {
                    CompilerInfo ci = CodeDomProvider.GetCompilerInfo(language);

                    compilerType = new CompilerType(ci.CodeDomProviderType,
                        ci.CreateDefaultCompilerParameters());

                    // Cache it
                    _compilerLanguages[language] = compilerType;
                }
            }

            if (compilerType == null) {

                // Unsupported language: throw an exception
                throw new HttpException(SR.GetString(SR.Invalid_lang, language));
            }

            // Only allow the use of compilerOptions when we have UnmanagedCode access (ASURT 73678)
            CompilationUtil.CheckCompilerOptionsAllowed(compilerType.CompilerParameters.CompilerOptions,
                true /*config*/, null, 0);

            // Clone it so the original is not modified
            compilerType = compilerType.Clone();

            // Set the value of the debug flag in the copy
            compilerType.CompilerParameters.IncludeDebugInformation = Debug;

            return compilerType;
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

        /// <summary>
        /// Returns the original assembly name associated with the loaded assembly.
        /// Returns Assembly.FullName if not found.
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        internal static string GetOriginalAssemblyName(Assembly a) {
            string name = null;
            if (!_assemblyNames.Value.TryGetValue(a, out name)) {
                // If the assembly is not found in the dictionary, just
                // return Assembly.FullName.
                name = a.FullName;
            }
            return name;
        }

        internal Assembly[] LoadAssembly(AssemblyInfo ai) {
            Assembly[] assemblies = null;
            if (ai.Assembly == "*") {
                assemblies = LoadAllAssembliesFromAppDomainBinDirectory();
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

        internal static Assembly LoadAndRecordAssembly(AssemblyName name) {
            Assembly a = Assembly.Load(name);
            RecordAssembly(name.FullName, a);
            return a;
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
            System.Web.Util.Debug.Assert(HttpRuntime.TrustLevel != null);

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
            System.Web.Util.Debug.Assert(HttpRuntime.TrustLevel != null);

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

                if (BuildManager.IgnoreBadImageFormatException) {
                    var badImageFormatException = e as BadImageFormatException;
                    if (badImageFormatException != null) {
                        ignoreException = true;
                    }
                }

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

        internal Assembly[] LoadAllAssembliesFromAppDomainBinDirectory() {
            // Get the path to the bin directory
            string binPath = HttpRuntime.BinDirectoryInternal;
            FileInfo[] binDlls;
            Assembly assembly = null;
            Assembly[] assemblies = null;
            ArrayList list;

            if (!FileUtil.DirectoryExists(binPath)) {
                // This is expected to fail if there is no 'bin' dir
                System.Web.Util.Debug.Trace("Template", "Failed to access bin dir \"" + binPath + "\"");
            }
            else {
                DirectoryInfo binPathDirectory = new DirectoryInfo(binPath);
                // Get a list of all the DLL's in the bin directory
                binDlls = binPathDirectory.GetFiles("*.dll");

                if (binDlls.Length > 0) {
                    list = new ArrayList(binDlls.Length);

                    for (int i = 0; i < binDlls.Length; i++) {
                        string assemblyName = Util.GetAssemblyNameFromFileName(binDlls[i].Name);

                        // Don't autoload generated assemblies in bin (VSWhidbey 467936)
                        if (assemblyName.StartsWith(BuildManager.WebAssemblyNamePrefix, StringComparison.Ordinal))
                            continue;

                        if (!GetAssembliesCollection().IsRemoved(assemblyName)) {
                            assembly = LoadAssemblyHelper(assemblyName, true);
                        }
                        if (assembly != null) {
                            list.Add(assembly);
                        }
                    }
                    assemblies = (System.Reflection.Assembly[])list.ToArray(typeof(System.Reflection.Assembly));
                }
            }

            if (assemblies == null) {
                // If there were no assemblies loaded, return a zero-length array
                assemblies = new Assembly[0];
            }

            return assemblies;
        }

        internal long RecompilationHash {
            get {
                if (_recompilationHash == -1) {
                    lock (this) {
                        if (_recompilationHash == -1) {
                            _recompilationHash = CompilationUtil.GetRecompilationHash(this);
                        }
                    }
                }

                return _recompilationHash;
            }
        }

        protected override void PostDeserialize() {
            // check to see if the _propCodeSubDirs was defined below the app level
            WebContext context = EvaluationContext.HostingContext as WebContext;
            if (context != null) {
                if (context.ApplicationLevel == WebApplicationLevel.BelowApplication) {
                    if (CodeSubDirectories.ElementInformation.IsPresent) {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.Config_element_below_app_illegal,
                                         _propCodeSubDirs.Name), CodeSubDirectories.ElementInformation.Source, CodeSubDirectories.ElementInformation.LineNumber);
                    }
                    if (BuildProviders.ElementInformation.IsPresent) {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.Config_element_below_app_illegal,
                                         _propBuildProviders.Name), BuildProviders.ElementInformation.Source, BuildProviders.ElementInformation.LineNumber);
                    }

                    if (FolderLevelBuildProviders.ElementInformation.IsPresent) {
                        throw new ConfigurationErrorsException(
                            SR.GetString(SR.Config_element_below_app_illegal,
                                         _propFolderLevelBuildProviders.Name), FolderLevelBuildProviders.ElementInformation.Source, FolderLevelBuildProviders.ElementInformation.LineNumber);
                    }
                }

            }
        }

        internal Type ControlBuilderInterceptorTypeInternal {
            get {
                if (_controlBuilderInterceptorType == null && !String.IsNullOrWhiteSpace(ControlBuilderInterceptorType)) {
                    lock (this) {
                        if (_controlBuilderInterceptorType == null) {
                            _controlBuilderInterceptorType = CompilationUtil.LoadTypeWithChecks(ControlBuilderInterceptorType, typeof(ControlBuilderInterceptor), null, this, controlBuilderInterceptorTypeAttributeName);
                        }
                    }
                }
                return _controlBuilderInterceptorType;
            }
        }

        // This is called as the last step of the deserialization process before the newly created section is seen by the consumer.
        // We can use it to change defaults on-the-fly.
        protected override void SetReadOnly() {
            // Unless overridden, set <compilation targetFramework="4.5" />
            ConfigUtil.SetFX45DefaultValue(this, _propTargetFramework, BinaryCompatibility.Current.TargetFramework.ToString());

            base.SetReadOnly();
        }
    }
}
