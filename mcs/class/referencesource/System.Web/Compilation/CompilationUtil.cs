//------------------------------------------------------------------------------
// <copyright file="CompilationConfiguration.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Code related to the <assemblies> config section
 *
 * Copyright (c) 1999 Microsoft Corporation
 */
namespace System.Web.Compilation {

    using System;
    using System.Web;
    using System.Configuration;
    using System.Web.UI;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Util;
    using System.Globalization;
    using System.Collections;
    using System.Collections.Generic;
    using System.CodeDom.Compiler;
    using System.Linq;
    using System.Security;
    using System.Security.Permissions;
    using System.Reflection;
    using System.Runtime.ExceptionServices;
    using System.Threading;
    using System.Threading.Tasks;
    
    internal static class CompilationUtil {

        internal const string CodeDomProviderOptionPath = "system.codedom/compilers/compiler/ProviderOption/";
        private const string CompilerDirectoryPath = "CompilerDirectoryPath";
        private static int _maxConcurrentCompilations;

        internal static bool IsDebuggingEnabled(HttpContext context) {
            CompilationSection compConfig = MTConfigUtil.GetCompilationConfig(context);
            return compConfig.Debug;
        }

        internal static bool IsBatchingEnabled(string configPath) {
            CompilationSection config = MTConfigUtil.GetCompilationConfig(configPath);
            return config.Batch;
        }

        internal static int GetRecompilationsBeforeAppRestarts() {
            CompilationSection config = MTConfigUtil.GetCompilationAppConfig();
            return config.NumRecompilesBeforeAppRestart;
        }

        internal static CompilerType GetCodeDefaultLanguageCompilerInfo() {
            return new CompilerType(typeof(Microsoft.VisualBasic.VBCodeProvider), null);
        }

        internal static CompilerType GetDefaultLanguageCompilerInfo(CompilationSection compConfig, VirtualPath configPath) {
            if (compConfig == null) {
                // Get the <compilation> config object
                compConfig = MTConfigUtil.GetCompilationConfig(configPath);
            }

            // If no default language was specified in config, use VB
            if (compConfig.DefaultLanguage == null) {
                return GetCodeDefaultLanguageCompilerInfo();
            }
            else {
                return compConfig.GetCompilerInfoFromLanguage(compConfig.DefaultLanguage);
            }
        }

        /*
         * Return a CompilerType that a file name's extension maps to.
         */
        internal static CompilerType GetCompilerInfoFromVirtualPath(VirtualPath virtualPath) {

            // Get the extension of the source file to compile
            string extension = virtualPath.Extension;

            // Make sure there is an extension
            if (extension.Length == 0) {
                throw new HttpException(
                    SR.GetString(SR.Empty_extension, virtualPath));
            }

            return GetCompilerInfoFromExtension(virtualPath, extension);
        }

        /*
         * Return a CompilerType that a extension maps to.
         */
        private static CompilerType GetCompilerInfoFromExtension(VirtualPath configPath, string extension) {
            // Get the <compilation> config object
            CompilationSection config = MTConfigUtil.GetCompilationConfig(configPath);

            return config.GetCompilerInfoFromExtension(extension, true /*throwOnFail*/);
        }

        /*
         * Return a CompilerType that a language maps to.
         */
        internal static CompilerType GetCompilerInfoFromLanguage(VirtualPath configPath, string language) {
            // Get the <compilation> config object
            CompilationSection config = MTConfigUtil.GetCompilationConfig(configPath);

            return config.GetCompilerInfoFromLanguage(language);
        }

        internal static CompilerType GetCSharpCompilerInfo(
            CompilationSection compConfig, VirtualPath configPath) {

            if (compConfig == null) {
                // Get the <compilation> config object
                compConfig = MTConfigUtil.GetCompilationConfig(configPath);
            }

            if (compConfig.DefaultLanguage == null)
                return new CompilerType(typeof(Microsoft.CSharp.CSharpCodeProvider), null);

            return compConfig.GetCompilerInfoFromLanguage("c#");
        }

        internal static CodeSubDirectoriesCollection GetCodeSubDirectories() {
            // Get the <compilation> config object
            CompilationSection config = MTConfigUtil.GetCompilationAppConfig();

            CodeSubDirectoriesCollection codeSubDirectories = config.CodeSubDirectories;

            // Make sure the config data is valid
            if (codeSubDirectories != null) {
                codeSubDirectories.EnsureRuntimeValidation();
            }

            return codeSubDirectories;
        }

        internal static long GetRecompilationHash(CompilationSection ps) {
            HashCodeCombiner recompilationHash = new HashCodeCombiner();
            AssemblyCollection assemblies;
            BuildProviderCollection builders;
            FolderLevelBuildProviderCollection buildProviders;
            CodeSubDirectoriesCollection codeSubDirs;
                
            // Combine items from Compilation section
            recompilationHash.AddObject(ps.Debug);
            recompilationHash.AddObject(ps.TargetFramework);
            recompilationHash.AddObject(ps.Strict);
            recompilationHash.AddObject(ps.Explicit);
            recompilationHash.AddObject(ps.Batch);
            recompilationHash.AddObject(ps.OptimizeCompilations);
            recompilationHash.AddObject(ps.BatchTimeout);
            recompilationHash.AddObject(ps.MaxBatchGeneratedFileSize);
            recompilationHash.AddObject(ps.MaxBatchSize);
            recompilationHash.AddObject(ps.NumRecompilesBeforeAppRestart);
            recompilationHash.AddObject(ps.DefaultLanguage);
            recompilationHash.AddObject(ps.UrlLinePragmas);
            recompilationHash.AddObject(ps.DisableObsoleteWarnings);
            if (ps.AssemblyPostProcessorTypeInternal != null) {
                recompilationHash.AddObject(ps.AssemblyPostProcessorTypeInternal.FullName);
            }
            if (!String.IsNullOrWhiteSpace(ps.ControlBuilderInterceptorType)) {
                recompilationHash.AddObject(ps.ControlBuilderInterceptorType);
            }

            // Combine items from Compilers collection
            foreach (Compiler compiler in ps.Compilers) {
                recompilationHash.AddObject(compiler.Language);
                recompilationHash.AddObject(compiler.Extension);
                recompilationHash.AddObject(compiler.Type);
                recompilationHash.AddObject(compiler.WarningLevel);
                recompilationHash.AddObject(compiler.CompilerOptions);
            }

            // Combine items from <expressionBuilders> section
            foreach (System.Web.Configuration.ExpressionBuilder eb in ps.ExpressionBuilders) {
                recompilationHash.AddObject(eb.ExpressionPrefix);
                recompilationHash.AddObject(eb.Type);
            }

            // Combine items from the Assembly collection
            assemblies = ps.Assemblies;

            if (assemblies.Count == 0) {
                recompilationHash.AddObject("__clearassemblies");
            }
            else {
                foreach (AssemblyInfo ai in assemblies) {
                    recompilationHash.AddObject(ai.Assembly);
                }
            }

            // Combine items from the Builders Collection
            builders = ps.BuildProviders;

            if (builders.Count == 0) {
                recompilationHash.AddObject("__clearbuildproviders");
            }
            else {
                foreach (System.Web.Configuration.BuildProvider bp in builders) {
                    recompilationHash.AddObject(bp.Type);
                    recompilationHash.AddObject(bp.Extension);
                }
            }

            // Combine items from the FolderLevelBuildProviderCollection
            buildProviders = ps.FolderLevelBuildProviders;

            if (buildProviders.Count == 0) {
                recompilationHash.AddObject("__clearfolderlevelbuildproviders");
            }
            else {
                foreach (System.Web.Configuration.FolderLevelBuildProvider bp in buildProviders) {
                    recompilationHash.AddObject(bp.Type);
                    recompilationHash.AddObject(bp.Name);
                }
            }

            codeSubDirs = ps.CodeSubDirectories;
            if (codeSubDirs.Count == 0) {
                recompilationHash.AddObject("__clearcodesubdirs");
            }
            else {
                foreach (CodeSubDirectory csd in codeSubDirs) {
                    recompilationHash.AddObject(csd.DirectoryName);
                }
            }

            // Make sure the <system.CodeDom> section is hashed properly.
            CompilerInfo[] compilerInfoArray = CodeDomProvider.GetAllCompilerInfo();
            if (compilerInfoArray != null) {
                CompilerInfo cppCodeProvider = CodeDomProvider.GetCompilerInfo("cpp");
                foreach (CompilerInfo info in compilerInfoArray) {
                    // Skip cpp code provider (Dev11 193323).
                    if (info == cppCodeProvider) {
                        continue;
                    }

                    // Ignore it if the type is not valid.
                    if (!info.IsCodeDomProviderTypeValid) {
                        continue;
                    }

                    CompilerParameters parameters = info.CreateDefaultCompilerParameters();
                    string option = parameters.CompilerOptions;
                    if (!String.IsNullOrEmpty(option)) {
                        Type type = info.CodeDomProviderType;
                        if (type != null) {
                            recompilationHash.AddObject(type.FullName);
                        }
                        // compilerOptions need to be hashed.
                        recompilationHash.AddObject(option);
                    }

                    // DevDiv 62998
                    // The tag providerOption needs to be added to the hash,
                    // as the user could switch between v2 and v3.5.
                    if (info.CodeDomProviderType == null)
                        continue;

                    // Add a hash for each providerOption added, specific for each codeDomProvider, so that
                    // if some codedom setting has changed, we know we have to recompile.
                    IDictionary<string, string> providerOptions = GetProviderOptions(info);
                    if (providerOptions != null && providerOptions.Count > 0) {
                        string codeDomProviderType = info.CodeDomProviderType.FullName;
                        foreach (string key in providerOptions.Keys) {
                            string value = providerOptions[key];
                            recompilationHash.AddObject(codeDomProviderType + ":" +  key + "=" + value);
                        }
                    }
                }
            }

            return recompilationHash.CombinedHash;
        }


        /*
         * Return a file provider Type that an extension maps to.
         */
        internal static Type GetBuildProviderTypeFromExtension(VirtualPath configPath, string extension,
            BuildProviderAppliesTo neededFor, bool failIfUnknown) {

            // Get the <compilation> config object
            CompilationSection config = MTConfigUtil.GetCompilationConfig(configPath);

            return GetBuildProviderTypeFromExtension(config, extension, neededFor, failIfUnknown);
        }

        internal static Type GetBuildProviderTypeFromExtension(CompilationSection config, string extension,
            BuildProviderAppliesTo neededFor, bool failIfUnknown) {

            BuildProviderInfo providerInfo = BuildProvider.GetBuildProviderInfo(config, extension);

            Type buildProviderType = null;
            // Never return an IgnoreFileBuildProvider/ForceCopyBuildProvider, since it's just a marker
            if (providerInfo != null &&
                providerInfo.Type != typeof(IgnoreFileBuildProvider) &&
                providerInfo.Type != typeof(ForceCopyBuildProvider)) {
                buildProviderType = providerInfo.Type;
            }

            // In updatable precomp mode, only aspx/ascx/master web files need processing.  Ignore the rest.
            if (neededFor == BuildProviderAppliesTo.Web &&
                BuildManager.PrecompilingForUpdatableDeployment &&
                !typeof(BaseTemplateBuildProvider).IsAssignableFrom(buildProviderType)) {
                buildProviderType = null;
            }

            if (buildProviderType != null) {
                // Only return it if it applies to what it's needed for
                if ((neededFor & providerInfo.AppliesTo) != 0)
                    return buildProviderType;
            }
            // If the extension is registered as a compiler extension, use
            // a SourceFileBuildProvider to handle it (not supported in Resources directory)
            else if (neededFor != BuildProviderAppliesTo.Resources &&
                config.GetCompilerInfoFromExtension(extension, false /*throwOnFail*/) != null) {
                return typeof(SourceFileBuildProvider);
            }

            if (failIfUnknown) {
                throw new HttpException( SR.GetString(SR.Unknown_buildprovider_extension, extension, neededFor.ToString()));
            }

            return null;
        }

        // Returns the list of buildProvider types associated to the specified appliesTo
        internal static List<Type> GetFolderLevelBuildProviderTypes(CompilationSection config,
            FolderLevelBuildProviderAppliesTo appliesTo) {
            FolderLevelBuildProviderCollection buildProviders = config.FolderLevelBuildProviders;
            return buildProviders.GetBuildProviderTypes(appliesTo);
        }

        // In partial trust, do not allow the CompilerDirectoryPath provider option in codedom settings (Dev10 
        internal static void CheckCompilerDirectoryPathAllowed(IDictionary<string, string> providerOptions) {
            if (providerOptions == null) {
                return;
            }
            if (!providerOptions.ContainsKey(CompilerDirectoryPath)) {
                return;
            }

            if (!HttpRuntime.HasUnmanagedPermission()) {
                string errorString = SR.GetString(SR.Insufficient_trust_for_attribute, CompilerDirectoryPath);
                throw new HttpException(errorString);
            }
        }

        internal static void CheckCompilerOptionsAllowed(string compilerOptions, bool config, string file, int line) {
            
            // If it's empty, we never block it
            if (String.IsNullOrEmpty(compilerOptions))
                return;

            // Only allow the use of compilerOptions when we have UnmanagedCode access (ASURT 73678)
            if (!HttpRuntime.HasUnmanagedPermission()) {
                string errorString = SR.GetString(SR.Insufficient_trust_for_attribute, "compilerOptions");

                if (config)
                    throw new ConfigurationErrorsException(errorString, file, line);
                else
                    throw new HttpException(errorString);
            }
        }

        // This is used to determine what files need to be copied, and what stub files
        // need to be created during deployment precompilation.
        // Note: createStub only applies if the method returns false.
        internal static bool NeedToCopyFile(VirtualPath virtualPath, bool updatable, out bool createStub) {

            createStub = false;

            // Get the <compilation> config object
            CompilationSection config = MTConfigUtil.GetCompilationConfig(virtualPath);

            string extension = virtualPath.Extension;

            BuildProviderInfo providerInfo = BuildProvider.GetBuildProviderInfo(config, extension);

            if (providerInfo != null) {
                // We only care about 'web' providers.  Everything else we treat as static
                if ((BuildProviderAppliesTo.Web & providerInfo.AppliesTo) == 0)
                    return true;

                // If the provider is a ForceCopyBuildProvider, treat as static
                if (providerInfo.Type == typeof(ForceCopyBuildProvider))
                    return true;

                // During updatable precomp, everything needs to be copied over.  However,
                // aspx files that use code beside will later be overwritten by modified
                // versions (see TemplateParser.CreateModifiedMainDirectiveFileIfNeeded)
                if (providerInfo.Type != typeof(IgnoreFileBuildProvider) &&
                    BuildManager.PrecompilingForUpdatableDeployment) {
                    return true;
                }

                // There is a real provider, so don't copy the file.  We also need to determine whether
                // a stub file needs to be created.

                createStub = true;

                // Skip the stub file for some non-requestable types
                if (providerInfo.Type == typeof(UserControlBuildProvider) ||
                    providerInfo.Type == typeof(MasterPageBuildProvider) ||
                    providerInfo.Type == typeof(IgnoreFileBuildProvider)) {
                    createStub = false;
                }

                return false;
            }

            // If the extension is registered as a compiler extension, don't copy
            if (config.GetCompilerInfoFromExtension(extension, false /*throwOnFail*/) != null) {
                return false;
            }

            // Skip the copying for asax and skin files, which are not static even though they
            // don't have a registered BuildProvider (but don't skip .skin files during
            // updatable precomp).
            // 
            if (StringUtil.EqualsIgnoreCase(extension, ".asax"))
                return false;
            if (!updatable && StringUtil.EqualsIgnoreCase(extension, ThemeDirectoryCompiler.skinExtension))
                return false;

            //
            // If there is no BuildProvider registered, it's a static file, and should be copied
            //

            return true;
        }

        internal static Type LoadTypeWithChecks(string typeName, Type requiredBaseType, Type requiredBaseType2, ConfigurationElement elem, string propertyName) {
            Type t = ConfigUtil.GetType(typeName, propertyName, elem);

            if (requiredBaseType2 == null) {
                ConfigUtil.CheckAssignableType(requiredBaseType, t, elem, propertyName);
            }
            else {
                ConfigUtil.CheckAssignableType(requiredBaseType, requiredBaseType2, t, elem, propertyName);
            }

            return t;
        }

        // Devdiv 



        internal static CodeDomProvider CreateCodeDomProvider(Type codeDomProviderType) {
            CodeDomProvider codeDomProvider = CreateCodeDomProviderWithPropertyOptions(codeDomProviderType);
            if (codeDomProvider != null) {
                return codeDomProvider;
            }
            return (CodeDomProvider)Activator.CreateInstance(codeDomProviderType);
        }

        internal static CodeDomProvider CreateCodeDomProviderNonPublic(Type codeDomProviderType) {
            CodeDomProvider codeDomProvider = CreateCodeDomProviderWithPropertyOptions(codeDomProviderType);
            if (codeDomProvider != null) {
                return codeDomProvider;
            }
            return (CodeDomProvider)HttpRuntime.CreateNonPublicInstance(codeDomProviderType);
        }

        [ReflectionPermission(SecurityAction.Assert, Unrestricted = true)]
        private static CodeDomProvider CreateCodeDomProviderWithPropertyOptions(Type codeDomProviderType) {
            // The following resembles the code in System.CodeDom.CompilerInfo.CreateProvider

            // Make a copy to avoid modifying the original.
            var originalProviderOptions = GetProviderOptions(codeDomProviderType);
            IDictionary<string, string> providerOptions = null;
            if (originalProviderOptions != null) {
                providerOptions = new Dictionary<string, string>(originalProviderOptions);
            } else {
                providerOptions = new Dictionary<string, string>();
            }
            
            // Block CompilerDirectoryPath if we are in partial trust
            CheckCompilerDirectoryPathAllowed(providerOptions);

            // Check whether the user supplied the compilerDirectoryPath or was it added by us
            bool addedCompilerDirectoryPath = false;

            if (MultiTargetingUtil.IsTargetFramework20) {
                // If the target framework is v2.0, there won't be any codedom settings, so we need
                // to explicitly set the compiler to be the v2.0 compiler using compilerVersion=v2.0.
                providerOptions["CompilerVersion"] = "v2.0";
            }
            else if (MultiTargetingUtil.IsTargetFramework35) {
                // We need to explicitly set to v3.5, as it is possible for the
                // user to only have specified it for one compiler but not 
                // the other.
                // Dev10 
                providerOptions["CompilerVersion"] = "v3.5";
            }
            else {
                // If we are targeting 4.0 but the compiler version is less than 4.0, set it to 4.0.
                // This can happen if a user tries to run a 2.0/3.5 web site in a 4.0 application pool without
                // upgrading it, and the codedom section still has 3.5 as the compilerVersion,
                // so we have to set the compilerVersion to 4.0 explicitly.
                string version = GetCompilerVersion(codeDomProviderType);
                Version v = GetVersionFromVString(version);
                if (v != null && v < MultiTargetingUtil.Version40) {
                    providerOptions["CompilerVersion"] = "v4.0";
                }
            }

            if (providerOptions != null && providerOptions.Count > 0) {
                Debug.Assert(codeDomProviderType != null, "codeDomProviderType should not be null");
                // Check whether the codedom provider supports a constructor that takes in providerOptions.
                // Currently only VB and C# support providerOptions for sure, while others such as JScript might not.
                ConstructorInfo ci = codeDomProviderType.GetConstructor(new Type[] { typeof(IDictionary<string, string>) });
                CodeDomProvider provider = null;
                if (ci != null) {
                    // First, obtain the language for the given codedom provider type.
                    CodeDomProvider defaultProvider = (CodeDomProvider)Activator.CreateInstance(codeDomProviderType);
                    string extension = defaultProvider.FileExtension;
                    var language = CodeDomProvider.GetLanguageFromExtension(extension);
                    // Then, use the new createProvider API to create an instance.
                    provider = CodeDomProvider.CreateProvider(language, providerOptions);
                }
                // Restore the provider options if we previously manually added the compilerDirectoryPath.
                // Otherwise, we might incorrectly invalidate the compilerDirectoryPath in medium trust (Dev10 
                if (addedCompilerDirectoryPath) {
                    providerOptions.Remove(CompilerDirectoryPath);
                }
                return provider;
            }

            return null;
        }

        [ReflectionPermission(SecurityAction.Assert, Unrestricted = true)]
        internal static IDictionary<string, string> GetProviderOptions(Type codeDomProviderType) {
            // Using reflection to get the property for the time being.
            // This could simply return CompilerInfo.PropertyOptions if it goes public in future.
            CodeDomProvider provider = (CodeDomProvider)Activator.CreateInstance(codeDomProviderType);
            string extension = provider.FileExtension;
            if (CodeDomProvider.IsDefinedExtension(extension)) {
                CompilerInfo ci = CodeDomProvider.GetCompilerInfo(CodeDomProvider.GetLanguageFromExtension(extension));
                return GetProviderOptions(ci);
            }
            return null;
        }

        [ReflectionPermission(SecurityAction.Assert, Unrestricted = true)]
        private static IDictionary<string, string> GetProviderOptions(CompilerInfo ci) {
            Debug.Assert(ci != null, "CompilerInfo ci should not be null");
            PropertyInfo pi = ci.GetType().GetProperty("ProviderOptions",
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.IgnoreCase | BindingFlags.Instance);
            if (pi != null)
                return (IDictionary<string, string>)pi.GetValue(ci, null);
            return null;
        }

        /// <summary>
        /// Returns the compilerVersion providerOption specified for the codedom provider type.
        /// Returns null if the providerOption is not found.
        /// </summary>
        internal static string GetCompilerVersion(Type codeDomProviderType) {
            return GetProviderOption(codeDomProviderType, "CompilerVersion");
        }

        /// <summary>
        /// Returns the value of the providerOption specified for the codedom provider type.
        /// Returns null if the providerOption is not found.
        /// </summary>
        internal static string GetProviderOption(Type codeDomProviderType, string providerOption) {
            IDictionary<string, string> providerOptions = CompilationUtil.GetProviderOptions(codeDomProviderType);
            if (providerOptions != null) {
                string version;
                if (providerOptions.TryGetValue(providerOption, out version)) {
                    return version;
                }
            }
            return null;
        }        

        /// <summary>
        /// Returns true if the string matches "v3.5" exactly.
        /// </summary>
        internal static bool IsCompilerVersion35(string compilerVersion) {
            if (compilerVersion == "v3.5") {
                return true;
            }
            return false;
        }

        /// <summary>
        /// This returns true only if the codedom CompilverVersion provider option is exactly v3.5.
        /// </summary>
        internal static bool IsCompilerVersion35(Type codeDomProviderType) {
            string compilerVersion = GetCompilerVersion(codeDomProviderType);
            bool result = IsCompilerVersion35(compilerVersion);
            return result;
        }

        /// <summary>
        /// Returns true if the codedom CompilerVersion provider option is at least v3.5.
        /// </summary>
        /// <param name="codeDomProviderType"></param>
        /// <returns></returns>
        internal static bool IsCompilerVersion35OrAbove(Type codeDomProviderType) {
            string compilerVersion = GetCompilerVersion(codeDomProviderType);
            if (IsCompilerVersion35(compilerVersion)) {
                return true;
            }
            // The compilerVersion provider option is known to exist only for v3.5.
            // If it does not exist, then we need to rely on the target framework version to
            // determine whether we need to use the 2.0 or 4.0 compiler.
            if (MultiTargetingUtil.IsTargetFramework20) {
                return false;
            }

            // If it isn't 2.0 or 3.5, assume it is 4.0 and above.
            return true;
        }

        /// <summary>
        /// Returns true if the codedom provider has warnAsError set to true
        /// </summary>
        internal static bool WarnAsError(Type codeDomProviderType) {
            string value = GetProviderOption(codeDomProviderType, "WarnAsError");
            bool result;
            if (value != null && bool.TryParse(value, out result)) {
                return result;
            }

            // Assume false if the value wasn't set 
            return false;
        }

        // Returns the version when given string of form "v4.0"
        internal static Version GetVersionFromVString(string version) {
            if (string.IsNullOrEmpty(version)) {
                return null;
            }
            Debug.Assert(version.Length > 1, "Version has invalid length");
            return new Version(version.Substring(1));
        }

        // Returns maximum number of concurrent compilations
        internal static int MaxConcurrentCompilations {
            get {
                if (_maxConcurrentCompilations == 0) {
                    int maxConcurrentCompilations;

                    if (AppSettings.MaxConcurrentCompilations.HasValue && AppSettings.MaxConcurrentCompilations.Value >= 0) {
                        maxConcurrentCompilations = AppSettings.MaxConcurrentCompilations.Value;
                    }
                    else {
                        CompilationSection config = MTConfigUtil.GetCompilationAppConfig();
                        maxConcurrentCompilations = config.MaxConcurrentCompilations;
                    }

                    if (maxConcurrentCompilations <= 0) {
                        maxConcurrentCompilations = Environment.ProcessorCount;
                    }

                    Interlocked.CompareExchange(ref _maxConcurrentCompilations, maxConcurrentCompilations, 0);
                }

                return _maxConcurrentCompilations;
            }
        }
    }
}
