//------------------------------------------------------------------------------
// <copyright file="CodeDirectoryCompiler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.IO;
using System.Collections;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Globalization;
using System.Web.Configuration;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Util;
using System.Web.UI;

// The different types of directory that we treat as 'Code' (with minor differences)
internal enum CodeDirectoryType {
    MainCode,       // The main /code directory
    SubCode,        // Code subdirectories registered to be compiled separately
    AppResources,   // The /Resources directory
    LocalResources, // A /LocalResources directory (at any level)
    WebReferences   // The /WebReferences directory
}

internal class CodeDirectoryCompiler {

    private VirtualPath _virtualDir;
    private CodeDirectoryType _dirType;
    private StringSet _excludedSubdirectories;

    private BuildProvidersCompiler _bpc;
    private BuildProviderSet _buildProviders = new BuildProviderSet();

    private bool _onlyBuildLocalizedResources;

    static internal BuildResultMainCodeAssembly _mainCodeBuildResult;

    internal static bool IsResourceCodeDirectoryType(CodeDirectoryType dirType) {
        return dirType == CodeDirectoryType.AppResources || dirType == CodeDirectoryType.LocalResources;
    }

    internal static Assembly GetCodeDirectoryAssembly(VirtualPath virtualDir,
        CodeDirectoryType dirType, string assemblyName,
        StringSet excludedSubdirectories, bool isDirectoryAllowed) {

        string physicalDir = virtualDir.MapPath();

        if (!isDirectoryAllowed) {

            // The directory should never exist in a precompiled app
            if (Directory.Exists(physicalDir)) {
                throw new HttpException(SR.GetString(SR.Bar_dir_in_precompiled_app, virtualDir));
            }
        }

        bool supportLocalization = IsResourceCodeDirectoryType(dirType);

        // Determine the proper cache key based on the type of directory we're processing
        string cacheKey = assemblyName;

        // Try the cache first
        BuildResult result = BuildManager.GetBuildResultFromCache(cacheKey);
        Assembly resultAssembly = null;

        // If it's cached, just return it
        if (result != null) {

            // It should always be a BuildResultCompiledAssembly, though if there is
            // a VirtualPathProvider doing very bad things, it may not (VSWhidbey 341701)
            Debug.Assert(result is BuildResultCompiledAssembly);
            if (result is BuildResultCompiledAssembly) {

                // If it's the main code assembly, keep track of it so we can later call
                // the AppInitialize method
                if (result is BuildResultMainCodeAssembly) {
                    Debug.Assert(dirType == CodeDirectoryType.MainCode);
                    Debug.Assert(_mainCodeBuildResult == null);
                    _mainCodeBuildResult = (BuildResultMainCodeAssembly) result;
                }

                resultAssembly = ((BuildResultCompiledAssembly)result).ResultAssembly;

                if (!supportLocalization)
                    return resultAssembly;

                // We found a preserved resource assembly.  However, we may not be done,
                // as the culture specific files may have changed.

                // But don't make any further checks if the directory is not allowed (precomp secenario).
                // In that case, we should always return the assembly (VSWhidbey 533498)
                if (!isDirectoryAllowed)
                    return resultAssembly;

                BuildResultResourceAssembly buildResultResAssembly = (BuildResultResourceAssembly)result;

                string newResourcesDependenciesHash = HashCodeCombiner.GetDirectoryHash(virtualDir);

                // If the resources hash (which includes satellites) is up to date, we're done
                if (newResourcesDependenciesHash == buildResultResAssembly.ResourcesDependenciesHash)
                    return resultAssembly;
           }
        }

        // If app was precompiled, don't attempt compilation
        if (!isDirectoryAllowed)
            return null;

        // Check whether the virtual dir is mapped to a different application,
        // which we don't support (VSWhidbey 218603).  But don't do this for LocalResource (VSWhidbey 237935)
        if (dirType != CodeDirectoryType.LocalResources && !StringUtil.StringStartsWithIgnoreCase(physicalDir, HttpRuntime.AppDomainAppPathInternal)) {
            throw new HttpException(SR.GetString(SR.Virtual_codedir, virtualDir.VirtualPathString));
        }

        // If the directory doesn't exist, we may be done
        if (!Directory.Exists(physicalDir)) {

            // We're definitely done if it's not the main code dir
            if (dirType != CodeDirectoryType.MainCode)
                return null;

            // If it is the main code dir, we're only done is there is no profile to compile
            // since the profice gets built as part of the main assembly.
            if (!ProfileBuildProvider.HasCompilableProfile)
                return null;
        }


        // Otherwise, compile it

        BuildManager.ReportDirectoryCompilationProgress(virtualDir);

        DateTime utcStart = DateTime.UtcNow;

        CodeDirectoryCompiler cdc = new CodeDirectoryCompiler(virtualDir,
            dirType, excludedSubdirectories);

        string outputAssemblyName = null;

        if (resultAssembly != null) {
            // If resultAssembly is not null, we are in the case where we just need to build
            // the localized resx file in a resources dir (local or global)
            Debug.Assert(supportLocalization);
            outputAssemblyName = resultAssembly.GetName().Name;
            cdc._onlyBuildLocalizedResources = true;
        }
        else {
            outputAssemblyName = BuildManager.GenerateRandomAssemblyName(assemblyName);
        }

        BuildProvidersCompiler bpc = 
            new BuildProvidersCompiler(virtualDir, supportLocalization, outputAssemblyName);

        cdc._bpc = bpc;

        // Find all the build provider we want to compile from the code directory
        cdc.FindBuildProviders();

        // Give them to the BuildProvidersCompiler
        bpc.SetBuildProviders(cdc._buildProviders);

        // Compile them into an assembly
        CompilerResults results = bpc.PerformBuild();

        // Did we just compile something?
        if (results != null) {
            Debug.Assert(result == null);
            Debug.Assert(resultAssembly == null);

            // If there is already a loaded module with the same path, try to wait for it to be unloaded.
            // Otherwise, we would end up loading this old assembly instead of the new one (VSWhidbey 554697)
            DateTime waitLimit = DateTime.UtcNow.AddMilliseconds(3000);
            for (;;) {
                IntPtr hModule = UnsafeNativeMethods.GetModuleHandle(results.PathToAssembly);
                if (hModule == IntPtr.Zero)
                    break;

                Debug.Trace("CodeDirectoryCompiler", results.PathToAssembly + " is already loaded. Waiting a bit");

                System.Threading.Thread.Sleep(250);

                // Stop trying if the timeout was reached
                if (DateTime.UtcNow > waitLimit) {
                    Debug.Trace("CodeDirectoryCompiler", "Timeout waiting for old assembly to unload: " + results.PathToAssembly);
                    throw new HttpException(SR.GetString(SR.Assembly_already_loaded, results.PathToAssembly));
                }
            }

            resultAssembly = results.CompiledAssembly;
        }

        // It is possible that there was nothing to compile (and we're not in the
        // satellite resources case)
        if (resultAssembly == null)
            return null;

        // For the main code directory, use a special BuildResult that takes care of
        // calling AppInitialize if it finds one
        if (dirType == CodeDirectoryType.MainCode) {
            // Keep track of it so we can later call the AppInitialize method
            _mainCodeBuildResult = new BuildResultMainCodeAssembly(resultAssembly);

            result = _mainCodeBuildResult;
        }
        else if (supportLocalization) {
            result = new BuildResultResourceAssembly(resultAssembly);
        }
        else {
            result = new BuildResultCompiledAssembly(resultAssembly);
        }

        result.VirtualPath = virtualDir;

        // If compilations are optimized, we need to include the right dependencies, since we can no longer
        // rely on everything getting wiped out when something in App_Code changes.
        // But don't do this for local resources, since they have their own special way of
        // dealing with dependencies (in BuildResultResourceAssembly.ComputeSourceDependenciesHashCode).
        // It's crucial *not* to do it as it triggers a tricky infinite recursion due to the fact
        // that GetBuildResultFromCacheInternal calls EnsureFirstTimeDirectoryInitForDependencies if
        // there is at least one dependency
        if (BuildManager.OptimizeCompilations && dirType != CodeDirectoryType.LocalResources) {
            result.AddVirtualPathDependencies(new SingleObjectCollection(virtualDir.AppRelativeVirtualPathString));
        }

        // Top level assembly should not be cached to memory.  But LocalResources are *not*
        // top level files, and do benefit from memory caching
        if (dirType != CodeDirectoryType.LocalResources)
            result.CacheToMemory = false;

        // Cache it for next time
        BuildManager.CacheBuildResult(cacheKey, result, utcStart);

        return resultAssembly;
    }

    // Call the AppInitialize method in the Code assembly if there is one
    internal static void CallAppInitializeMethod() {
        if (_mainCodeBuildResult != null)
            _mainCodeBuildResult.CallAppInitializeMethod();
    }

    internal const string sourcesDirectoryPrefix = "Sources_";

    internal static void GetCodeDirectoryInformation(
        VirtualPath virtualDir, CodeDirectoryType dirType, StringSet excludedSubdirectories, int index,
        out Type codeDomProviderType, out CompilerParameters compilerParameters,
        out string generatedFilesDir) {

        // Compute the full path to the directory we'll use to generate all
        // the code files
        generatedFilesDir = HttpRuntime.CodegenDirInternal + "\\" + 
            sourcesDirectoryPrefix + virtualDir.FileName;

        bool supportLocalization = IsResourceCodeDirectoryType(dirType);

        // the index is used to retrieve the correct referenced assemblies
        BuildProvidersCompiler bpc = new BuildProvidersCompiler(virtualDir, supportLocalization,
            generatedFilesDir, index);

        CodeDirectoryCompiler cdc = new CodeDirectoryCompiler(virtualDir,
            dirType, excludedSubdirectories);
        cdc._bpc = bpc;

        // Find all the build provider we want to compile from the code directory
        cdc.FindBuildProviders();

        // Give them to the BuildProvidersCompiler
        bpc.SetBuildProviders(cdc._buildProviders);

        // Generate all the sources into the directory generatedFilesDir
        bpc.GenerateSources(out codeDomProviderType, out compilerParameters);
    }

    private CodeDirectoryCompiler(VirtualPath virtualDir, CodeDirectoryType dirType,
        StringSet excludedSubdirectories) {

        _virtualDir = virtualDir;
        _dirType = dirType;
        _excludedSubdirectories = excludedSubdirectories;
    }

    private void FindBuildProviders() {

        // If we need to build the profile, add its build provider
        if (_dirType == CodeDirectoryType.MainCode && ProfileBuildProvider.HasCompilableProfile) {
            _buildProviders.Add(ProfileBuildProvider.Create());

        }

        VirtualDirectory vdir = HostingEnvironment.VirtualPathProvider.GetDirectory(_virtualDir);
        ProcessDirectoryRecursive(vdir, true /*topLevel*/);
    }

    private void AddFolderLevelBuildProviders(VirtualDirectory vdir, FolderLevelBuildProviderAppliesTo appliesTo) {
        BuildManager.AddFolderLevelBuildProviders(_buildProviders, vdir.VirtualPathObject,
            appliesTo, _bpc.CompConfig, _bpc.ReferencedAssemblies);
    }

    private void ProcessDirectoryRecursive(VirtualDirectory vdir, bool topLevel) {

        // If it's a WebReferences directory, handle it using a single WebReferencesBuildProvider
        // instead of creating a different BuildProvider for each file.
        if (_dirType == CodeDirectoryType.WebReferences) {
            // Create a build provider for the current directory
            BuildProvider buildProvider = new WebReferencesBuildProvider(vdir);
            buildProvider.SetVirtualPath(vdir.VirtualPathObject);
            _buildProviders.Add(buildProvider);

            AddFolderLevelBuildProviders(vdir, FolderLevelBuildProviderAppliesTo.WebReferences);
        }
        else if (_dirType == CodeDirectoryType.AppResources) {
            AddFolderLevelBuildProviders(vdir, FolderLevelBuildProviderAppliesTo.GlobalResources);
        }
        else if (_dirType == CodeDirectoryType.LocalResources) {
            AddFolderLevelBuildProviders(vdir, FolderLevelBuildProviderAppliesTo.LocalResources);
        }
        else if (_dirType == CodeDirectoryType.MainCode || _dirType == CodeDirectoryType.SubCode) {
            AddFolderLevelBuildProviders(vdir, FolderLevelBuildProviderAppliesTo.Code);
        }

        // Go through all the files in the directory
        foreach (VirtualFileBase child in vdir.Children) {

            if (child.IsDirectory) {

                // If we are at the top level of this code directory, and the current
                // subdirectory is in the exclude list, skip it
                if (topLevel && _excludedSubdirectories != null &&
                    _excludedSubdirectories.Contains(child.Name)) {
                    continue;
                }

                // Exclude the special FrontPage directory (VSWhidbey 116727)
                if (child.Name == "_vti_cnf")
                    continue;

                ProcessDirectoryRecursive(child as VirtualDirectory, false /*topLevel*/);
                continue;
            }

            // Don't look at individual files for WebReferences directories
            if (_dirType == CodeDirectoryType.WebReferences)
                continue;

            // Skip neutral files if _onlyBuildLocalizedResources is true
            if (IsResourceCodeDirectoryType(_dirType)) {
                if (_onlyBuildLocalizedResources && System.Web.UI.Util.GetCultureName(child.VirtualPath) == null) {
                    continue;
                }
            }

            BuildProvider buildProvider = BuildManager.CreateBuildProvider(child.VirtualPathObject,
                (IsResourceCodeDirectoryType(_dirType)) ?
                    BuildProviderAppliesTo.Resources : BuildProviderAppliesTo.Code,
                _bpc.CompConfig,
                _bpc.ReferencedAssemblies, false /*failIfUnknown*/);

            // Non-supported file type
            if (buildProvider == null)
                continue;

            // For Page resources, don't generate a strongly typed class
            if (_dirType == CodeDirectoryType.LocalResources && buildProvider is BaseResourcesBuildProvider) {
                ((BaseResourcesBuildProvider)buildProvider).DontGenerateStronglyTypedClass();
            }

            _buildProviders.Add(buildProvider);
        }
    }
}

}
