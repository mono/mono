//------------------------------------------------------------------------------
// <copyright file="BuildProvidersCompiler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.IO;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Reflection;
using System.Globalization;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Util;
using System.Web.Caching;
using System.Web.UI;
using System.Web.Configuration;

internal class BuildProvidersCompiler {
    private ICollection _buildProviders;
    private VirtualPath _configPath;
    private bool _supportLocalization;

    // The set of assemblies that we need to reference when compiling
    private ICollection _referencedAssemblies;
    internal ICollection ReferencedAssemblies { get { return _referencedAssemblies; } }

    private AssemblyBuilder _assemblyBuilder;

    // Key: CultureName string, Value: AssemblyBuilder
    private IDictionary _satelliteAssemblyBuilders = null;

    // If this is set, we only generate the source files into this directory
    // without compiling them.
    // This is used to implement ClientBuildManager.GetCodeDirectoryInformation
    private string _generatedFilesDir;

    internal BuildProvidersCompiler(VirtualPath configPath, string outputAssemblyName) : 
        this(configPath, false, outputAssemblyName) { }

    internal BuildProvidersCompiler(VirtualPath configPath, bool supportLocalization, 
        string outputAssemblyName) {
        _configPath = configPath;
        _supportLocalization = supportLocalization;
        _compConfig = MTConfigUtil.GetCompilationConfig(_configPath);
        _referencedAssemblies = BuildManager.GetReferencedAssemblies(CompConfig);
        _outputAssemblyName = outputAssemblyName;
    }

    internal BuildProvidersCompiler(VirtualPath configPath, bool supportLocalization,
        string generatedFilesDir, int index) {
        _configPath = configPath;
        _supportLocalization = supportLocalization;
        _compConfig = MTConfigUtil.GetCompilationConfig(_configPath);
        _referencedAssemblies = BuildManager.GetReferencedAssemblies(CompConfig, index);
        _generatedFilesDir = generatedFilesDir;
    }

    // The <compilation> config section for the set of build providers that we handle
    private CompilationSection _compConfig;
    internal CompilationSection CompConfig { get { return _compConfig; } }

    private string _outputAssemblyName;
    internal string OutputAssemblyName {
        get { return _outputAssemblyName; }
    }

    private bool CbmGenerateOnlyMode {
        get { return _generatedFilesDir != null; }
    }

    internal void SetBuildProviders(ICollection buildProviders) {
        _buildProviders = buildProviders;
    }

    private void ProcessBuildProviders() {

        CompilerType compilerType = null;
        BuildProvider firstLanguageBuildProvider = null;

        // First, delete all the existing satellite assemblies of the assembly
        // we're about to build (VSWhidbey 87022) (only if it has a fixed name)
        if (OutputAssemblyName != null) {
            Debug.Assert(!CbmGenerateOnlyMode);
            StandardDiskBuildResultCache.RemoveSatelliteAssemblies(OutputAssemblyName);
        }

        // List of BuildProvider's that don't ask for a specific language
        ArrayList languageFreeBuildProviders = null;

        foreach (BuildProvider buildProvider in _buildProviders) {

            // If it's an InternalBuildProvider, give it the assembly references early on
            buildProvider.SetReferencedAssemblies(_referencedAssemblies);

            // Instruct the internal build providers to continue processing for more parse errors.
            if (!BuildManager.ThrowOnFirstParseError) {
                InternalBuildProvider provider = buildProvider as InternalBuildProvider;
                if (provider != null) {
                    provider.ThrowOnFirstParseError = false;
                }
            }

            // Get the language and culture
            CompilerType ctwp = BuildProvider.GetCompilerTypeFromBuildProvider(buildProvider);

            // Only look for a culture if we're supposed to (basically, in the resources directories)
            string cultureName = null;
            if (_supportLocalization)
                cultureName = buildProvider.GetCultureName();

            // Is it asking for a specific language?
            if (ctwp != null) {

                // If it specifies a language, it can't also have a culture
                if (cultureName != null) {
                    throw new HttpException(SR.GetString(SR.Both_culture_and_language, BuildProvider.GetDisplayName(buildProvider)));
                }

                // Do we already know the language we'll be using
                if (compilerType != null) {

                    // If it's different from the current one, fail
                    if (!ctwp.Equals(compilerType)) {
                        throw new HttpException(SR.GetString(SR.Inconsistent_language,
                            BuildProvider.GetDisplayName(buildProvider),
                            BuildProvider.GetDisplayName(firstLanguageBuildProvider)));
                    }
                }
                else {
                    // Keep track of the build provider of error handling purpose
                    firstLanguageBuildProvider = buildProvider;

                    // Keep track of the language
                    compilerType = ctwp;
                    _assemblyBuilder = compilerType.CreateAssemblyBuilder(
                        CompConfig, _referencedAssemblies, _generatedFilesDir, OutputAssemblyName);
                }
            }
            else {
                if (cultureName != null) {
                    // Ignore the culture files in generate-only mode
                    if (CbmGenerateOnlyMode)
                        continue;

                    if (_satelliteAssemblyBuilders == null) {
                        _satelliteAssemblyBuilders = new Hashtable(
                            StringComparer.OrdinalIgnoreCase);
                    }

                    // Check if we already have an assembly builder for this culture
                    AssemblyBuilder satelliteAssemblyBuilder =
                        (AssemblyBuilder) _satelliteAssemblyBuilders[cultureName];

                    // If not, create one and store it in the hashtable
                    if (satelliteAssemblyBuilder == null) {
                        satelliteAssemblyBuilder = CompilerType.GetDefaultAssemblyBuilder(
                            CompConfig, _referencedAssemblies, _configPath, OutputAssemblyName);
                        satelliteAssemblyBuilder.CultureName = cultureName;
                        _satelliteAssemblyBuilders[cultureName] = satelliteAssemblyBuilder;
                    }

                    satelliteAssemblyBuilder.AddBuildProvider(buildProvider);
                    continue;
                }

                if (_assemblyBuilder == null) {
                    // If this provider doesn't need a specific language, and we don't know
                    // the language yet, just keep track of it
                    if (languageFreeBuildProviders == null)
                        languageFreeBuildProviders = new ArrayList();
                    languageFreeBuildProviders.Add(buildProvider);
                    continue;
                }
            }

            _assemblyBuilder.AddBuildProvider(buildProvider);
        }

        // If we didn't get an AssemblyBuilder, use a default
        if (_assemblyBuilder == null && languageFreeBuildProviders != null) {
            _assemblyBuilder = CompilerType.GetDefaultAssemblyBuilder(
                CompConfig, _referencedAssemblies, _configPath,
                _generatedFilesDir, OutputAssemblyName);
        }

        // Add all the language free providers (if any) to the AssemblyBuilder
        if (_assemblyBuilder != null && languageFreeBuildProviders != null) {
            foreach (BuildProvider languageFreeBuildProvider in languageFreeBuildProviders) {
                _assemblyBuilder.AddBuildProvider(languageFreeBuildProvider);
            }
        }
    }

    internal CompilerResults PerformBuild() {

        ProcessBuildProviders();
                
        // Build all the satellite assemblies
        if (_satelliteAssemblyBuilders != null) {
            int maxConcurrent = Math.Min(_satelliteAssemblyBuilders.Count, CompilationUtil.MaxConcurrentCompilations);
            try {
                Parallel.ForEach(_satelliteAssemblyBuilders.Values.Cast<AssemblyBuilder>(),
                    new ParallelOptions { MaxDegreeOfParallelism = maxConcurrent },
                    assemblyBuilder =>
                    {
                        assemblyBuilder.Compile();
                    });
            }
            catch (AggregateException ae) {
                ExceptionDispatchInfo.Capture(ae.GetBaseException()).Throw();
            }
        }

        // Build the main assembly
        if (_assemblyBuilder != null)
            return _assemblyBuilder.Compile();

        return null;
    }

    internal void GenerateSources(out Type codeDomProviderType,
        out CompilerParameters compilerParameters) {

        ProcessBuildProviders();

        // If we didn't get an AssemblyBuilder (happens when there was nothing to build),
        // get a default one.
        if (_assemblyBuilder == null) {
            _assemblyBuilder = CompilerType.GetDefaultAssemblyBuilder(
                CompConfig, _referencedAssemblies, _configPath, 
                _generatedFilesDir, null /*outputAssemblyName*/);
        }

        codeDomProviderType = _assemblyBuilder.CodeDomProviderType;
        compilerParameters = _assemblyBuilder.GetCompilerParameters();
    }
}


/*
 * This class handles the batch compilation of one directory.  It may
 * produce several assemblies out of them, based on dependencies and language
 * differences.  All the BuildProvider's are expected to share the same
 * configuration (i.e. they live in the same directory).
 */
internal class WebDirectoryBatchCompiler {

    private DateTime _utcStart;

    // The set of assemblies that we will link with
    private ICollection _referencedAssemblies;

    // The <compilation> config section for the set of build providers that we handle
    private CompilationSection _compConfig;

    // [VirtualPathString,InternalBuildProvider]
    private IDictionary _buildProviders = new Hashtable(
        StringComparer.OrdinalIgnoreCase);

    private VirtualDirectory _vdir;
    private ArrayList[] _nonDependentBuckets;

    private bool _ignoreProvidersWithErrors;

    // The set of parser errors detected during parsing.
    private ParserErrorCollection _parserErrors;

    // The first parse exceptions thrown during parsing.
    private HttpParseException _firstException;

    internal WebDirectoryBatchCompiler(VirtualDirectory vdir) {
        _vdir = vdir;

        _utcStart = DateTime.UtcNow;

        _compConfig = MTConfigUtil.GetCompilationConfig(_vdir.VirtualPath);

        _referencedAssemblies = BuildManager.GetReferencedAssemblies(_compConfig);
    }

    internal void SetIgnoreErrors() {
        _ignoreProvidersWithErrors = true;
    }

    internal void Process() {

        AddBuildProviders(true /*retryIfDeletionHappens*/);

        // If there are no BuildProvider's, we're done
        if (_buildProviders.Count == 0)
            return;

        BuildManager.ReportDirectoryCompilationProgress(_vdir.VirtualPathObject);

        GetBuildResultDependencies();
        ProcessDependencies();

        foreach (ICollection buildProviders in _nonDependentBuckets) {
            if (!CompileNonDependentBuildProviders(buildProviders))
                break;
        }

        // Report all parse exceptions
        if (_parserErrors != null && _parserErrors.Count > 0) {
            Debug.Assert(!_ignoreProvidersWithErrors);

            // Throw the first exception as inner exception along with the parse errors.
            HttpParseException newException = 
                new HttpParseException(_firstException.Message, _firstException, _firstException.VirtualPath,
                    _firstException.Source, _firstException.Line);

            // Add the rest of the parser errors to the exception.
            // The first one is already added.
            for (int i = 1; i < _parserErrors.Count; i++) {
                newException.ParserErrors.Add(_parserErrors[i]);
            }

            // rethrow the new exception
            throw newException;
        }
    }

    private void AddBuildProviders(bool retryIfDeletionHappens) {

        DiskBuildResultCache.ResetAssemblyDeleted();

        foreach (VirtualFile vfile in _vdir.Files) {

            // If it's already built and up to date, skip it
            BuildResult result = null;
            try {
                result = BuildManager.GetVPathBuildResultFromCache(vfile.VirtualPathObject);
            }
            catch {
                // Ignore the cached error in batch compilation mode, since we want to compile
                // as many files as possible.
                // But don't ignore it in CBM or precompile cases, since we always want to try
                // to compile everything that had failed before.
                if (!BuildManager.PerformingPrecompilation) {
                    // Skip it if an exception occurs (e.g. if a compile error was cached for it)
                    continue;
                }
            }

            if (result != null)
                continue;
            
            BuildProvider buildProvider = BuildManager.CreateBuildProvider(vfile.VirtualPathObject,
                _compConfig, _referencedAssemblies, false /*failIfUnknown*/);

            // Non-supported file type
            if (buildProvider == null)
                continue;

            // IgnoreFileBuildProvider's should never be created
            Debug.Assert(!(buildProvider is IgnoreFileBuildProvider));

            _buildProviders[vfile.VirtualPath] = buildProvider;
        }

        // If an assembly had to be deleted/renamed as a result of calling GetVPathBuildResultFromCache,
        // me way need to run the AddBuildProviders logic again.  The reason is that as a result of
        // deleting the assembly, we may have invalidated other BuildResult that we had earlier found
        // to be up to date (VSWhidbey 269297)
        if (DiskBuildResultCache.InUseAssemblyWasDeleted) {
            Debug.Assert(retryIfDeletionHappens);

            // Only retry if we're doing precompilation.  For standard batching, we can live
            // with the fact that not everything will be built after we're done (and we want to
            // be done as quickly as possible since the user is waiting).
            if (retryIfDeletionHappens && BuildManager.PerformingPrecompilation) {
                Debug.Trace("WebDirectoryBatchCompiler", "Rerunning AddBuildProviders for '" +
                    _vdir.VirtualPath + "' because an assembly was out of date.");

                // Pass false for retryIfDeletionHappens to make sure we don't get in an
                // infinite recursion.
                AddBuildProviders(false /*retryIfDeletionHappens*/);
            }
        }
    }

    private void CacheAssemblyResults(AssemblyBuilder assemblyBuilder, CompilerResults results) {

        foreach (BuildProvider buildProvider in assemblyBuilder.BuildProviders) {

            BuildResult result = buildProvider.GetBuildResult(results);

            // If the provider didn't produce anything, ignore it
            if (result == null)
                continue;

            // If CacheVPathBuildResult returns false, something was found to be invalidated
            // and we need to abort the caching (VSWhidbey 578372)
            if (!BuildManager.CacheVPathBuildResult(buildProvider.VirtualPathObject, result, _utcStart))
                break;

#if DBG
            if (results != null) {
                if (DelayLoadType.Enabled) {
                    Debug.Trace("BuildManager", buildProvider.VirtualPath + " Delay Load Assembly");
                } else {
                    Debug.Trace("BuildManager", buildProvider.VirtualPath + results.CompiledAssembly.EscapedCodeBase);
                }
            }
            else {
                Debug.Trace("BuildManager", buildProvider.VirtualPath + ": no assembly");
            }
#endif
        }
    }

    // Cache the various compile errors found during batching
    private void CacheCompileErrors(AssemblyBuilder assemblyBuilder, CompilerResults results) {

        BuildProvider previous = null;

        // Go through all the compile errors
        foreach (CompilerError error in results.Errors) {

            // Skip warnings
            if (error.IsWarning)
                continue;

            // Try to map the error back to a BuildProvider.  If we can't, skip the error.
            BuildProvider buildProvider = assemblyBuilder.GetBuildProviderFromLinePragma(error.FileName);
            if (buildProvider == null)
                continue;

            // Only cache the error for template controls.  Otherwise, for file types like
            // asmx/ashx, it's too likely that two of them define the same class.
            if (!(buildProvider is BaseTemplateBuildProvider))
                continue;

            // If the error is for the same page as the previous one, ignore it
            if (buildProvider == previous)
                continue;
            previous = buildProvider;

            // Create a new CompilerResults for this error
            CompilerResults newResults = new CompilerResults(null /*tempFiles*/);

            // Copy all the output to the new result.  Note that this will include all the
            // error lines, not just the ones for this BuildProvider.  But that's not a big deal,
            // and we can't easily filter the output here.
            foreach (string s in results.Output)
                newResults.Output.Add(s);

            // Copy various other fields to the new CompilerResults object
            newResults.PathToAssembly = results.PathToAssembly;
            newResults.NativeCompilerReturnValue = results.NativeCompilerReturnValue;

            // Add this error.  It will be the only one in the CompilerResults object.
            newResults.Errors.Add(error);

            // Create a new HttpCompileException & BuildResultCompileError to wrap this error
            HttpCompileException e = new HttpCompileException(newResults,
                assemblyBuilder.GetGeneratedSourceFromBuildProvider(buildProvider));
            BuildResult result = new BuildResultCompileError(buildProvider.VirtualPathObject, e);

            // Add the dependencies to the compile error build provider, so that
            // we will retry compilation when a dependency changes
            buildProvider.SetBuildResultDependencies(result);

            // Cache it
            BuildManager.CacheVPathBuildResult(buildProvider.VirtualPathObject, result, _utcStart);
        }

    }

    private void GetBuildResultDependencies() {
        foreach (BuildProvider buildProvider in _buildProviders.Values) {
            ICollection virtualPathDependencies = buildProvider.GetBuildResultVirtualPathDependencies();
            if (virtualPathDependencies == null)
                continue;

            foreach (string virtualPathDependency in virtualPathDependencies) {
                BuildProvider dependentBuildProvider = (BuildProvider) _buildProviders[virtualPathDependency];

                if (dependentBuildProvider != null)
                    buildProvider.AddBuildProviderDependency(dependentBuildProvider);
            }
        }
    }

    // Split the providers into non dependent buckets
    private void ProcessDependencies() {
        // First phase: compute levels in the dependency tree

        int totaldepth = 0;
        Hashtable depth = new Hashtable();
        Stack stack = new Stack();

        // compute depths
        foreach (BuildProvider buildProvider in _buildProviders.Values) {
            stack.Push(buildProvider);

            while (stack.Count > 0) {
                BuildProvider curnode = (BuildProvider)stack.Peek();

                bool recurse = false;
                int maxdepth = 0;

                if (curnode.BuildProviderDependencies != null) {
                    foreach (BuildProvider child in curnode.BuildProviderDependencies) {

                        if (depth.ContainsKey(child)) {
                            if (maxdepth <= (int)depth[child])
                                maxdepth = (int)depth[child] + 1;
                            else if ((int)depth[child] == -1)
                                throw new HttpException(SR.GetString(SR.File_Circular_Reference, child.VirtualPath));
                        }
                        else {
                            recurse = true;
                            stack.Push(child);
                        }
                    }
                }

                if (recurse)
                    depth[curnode] = -1; // being computed;
                else {
                    stack.Pop();
                    depth[curnode] = maxdepth;
                    if (totaldepth <= maxdepth)
                        totaldepth = maxdepth + 1;
                }
            }
        }

        // drop into buckets by depth
        _nonDependentBuckets = new ArrayList[totaldepth];

        for (IDictionaryEnumerator en = (IDictionaryEnumerator)depth.GetEnumerator(); en.MoveNext();) {
            int level = (int)en.Value;

            if (_nonDependentBuckets[level] == null)
                _nonDependentBuckets[level] = new ArrayList();

            _nonDependentBuckets[level].Add(en.Key);
        }

#if DBG
        int i = 0;
        foreach (ICollection buildProviders in _nonDependentBuckets) {
            Debug.Trace("BuildManager", String.Empty);
            Debug.Trace("BuildManager", "Bucket " + i + " contains " + buildProviders.Count + " files");

            foreach (BuildProvider buildProvider in buildProviders)
                Debug.Trace("BuildManager", buildProvider.VirtualPath);
            i++;
        }
#endif

    }

    private bool IsBuildProviderSkipable(BuildProvider buildProvider) {

        // If another build provider depends on it, we should not skip it
        if (buildProvider.IsDependedOn) return false;

        // No one depends on it (at least in this directory)

        // If it's a source file, skip it.  We need to do this for v1 compatibility,
        // since v1 VS projects contain many source files which have already been
        // precompiled into bin, and that should not be compiled dynamically
        if (buildProvider is SourceFileBuildProvider)
            return true;

        // For the same reason, skip resources
        if (buildProvider is ResXBuildProvider)
            return true;

        return false;
    }

    private bool CompileNonDependentBuildProviders(ICollection buildProviders) {

        // Key: CompilerType, Value: AssemblyBuilder
        IDictionary assemblyBuilders = new Hashtable();

        // List of InternalBuildProvider's that don't ask for a specific language
        ArrayList languageFreeBuildProviders = null;

        // AssemblyBuilder used for providers that don't need a specific language
        AssemblyBuilder defaultAssemblyBuilder = null;

        bool hasParserErrors = false;

        foreach (BuildProvider buildProvider in buildProviders) {

            if (IsBuildProviderSkipable(buildProvider))
                continue;

            // Instruct the internal build providers to continue processing for more parse errors.
            if (!BuildManager.ThrowOnFirstParseError) {
                InternalBuildProvider provider = buildProvider as InternalBuildProvider;
                if (provider != null) {
                    provider.ThrowOnFirstParseError = false;
                }
            }

            CompilerType compilerType = null;

            // Get the language
            try {
                compilerType = BuildProvider.GetCompilerTypeFromBuildProvider(
                    buildProvider);
            }
            catch (HttpParseException ex) {
                // Ignore the error if we are in that mode.
                if (_ignoreProvidersWithErrors) {
                    continue;
                }

                hasParserErrors = true;

                // Remember the first parse exception
                if (_firstException == null) {
                    _firstException = ex;
                }

                if (_parserErrors == null) {
                    _parserErrors = new ParserErrorCollection();
                }

                _parserErrors.AddRange(ex.ParserErrors);

                continue;
            }
            catch {
                // Ignore the error if we are in that mode.
                if (_ignoreProvidersWithErrors) {
                    continue;
                }

                throw;
            }

            AssemblyBuilder assemblyBuilder = defaultAssemblyBuilder;
            ICollection typeNames = buildProvider.GetGeneratedTypeNames();

            // Is it asking for a specific language?
            if (compilerType == null) {
                // If this provider doesn't need a specific language, and we haven't yet created
                // a default builder that is capable of building this, just keep track of it                
                if (defaultAssemblyBuilder == null || defaultAssemblyBuilder.IsBatchFull ||
                    defaultAssemblyBuilder.ContainsTypeNames(typeNames)) {
                    if (languageFreeBuildProviders == null) {
                        languageFreeBuildProviders = new ArrayList();
                    }

                    languageFreeBuildProviders.Add(buildProvider);
                    continue;
                }
            }
            else {
                // Check if we already have an assembly builder of the right type
                assemblyBuilder = (AssemblyBuilder)assemblyBuilders[compilerType];
            }

            // Starts a new assemblyBuilder if the old one already contains another buildprovider 
            // that uses the same type name
            if (assemblyBuilder == null || assemblyBuilder.IsBatchFull ||
                assemblyBuilder.ContainsTypeNames(typeNames)) {

                // If the assemblyBuilder is full, compile it.
                if (assemblyBuilder != null) {
                    CompileAssemblyBuilder(assemblyBuilder);
                }

                AssemblyBuilder newBuilder = compilerType.CreateAssemblyBuilder(
                    _compConfig, _referencedAssemblies);

                assemblyBuilders[compilerType] = newBuilder;

                // Remember it as the default if we don't already have one,
                // or if the default is already full, switch the default to the new one.
                if (defaultAssemblyBuilder == null ||
                    defaultAssemblyBuilder == assemblyBuilder) {

                    defaultAssemblyBuilder = newBuilder;
                }

                assemblyBuilder = newBuilder;
            }

            assemblyBuilder.AddTypeNames(typeNames);
            assemblyBuilder.AddBuildProvider(buildProvider);
        }

        // Don't try to compile providers, otherwise compile exceptions will be bubbled up,
        // and we lose the parse errors.
        if (hasParserErrors) {
            return false;
        }

        // Handle all the left over language free providers
        if (languageFreeBuildProviders != null) {

            // Indicates whether the default assembly builder is not a language specific builder.
            bool newDefaultAssemblyBuilder = (defaultAssemblyBuilder == null);

            // Add language independent providers to the default assembly builder.
            foreach (BuildProvider languageFreeBuildProvider in languageFreeBuildProviders) {

                ICollection typeNames = languageFreeBuildProvider.GetGeneratedTypeNames();

                // If we don't have a default language assembly builder, get one or
                // starts a new assemblyBuilder if the old one already contains another buildprovider 
                // that uses the same type name
                if (defaultAssemblyBuilder == null || defaultAssemblyBuilder.IsBatchFull ||
                    defaultAssemblyBuilder.ContainsTypeNames(typeNames)) {

                    // If the default assemblyBuilder is full, compile it.
                    if (defaultAssemblyBuilder != null) {
                        CompileAssemblyBuilder(defaultAssemblyBuilder);
                    }

                    defaultAssemblyBuilder = CompilerType.GetDefaultAssemblyBuilder(
                        _compConfig, _referencedAssemblies, _vdir.VirtualPathObject /*configPath*/, 
                        null /*outputAssemblyName*/);

                    // the default assembly builder needs to be compiled separately.
                    newDefaultAssemblyBuilder = true;
                }

                defaultAssemblyBuilder.AddTypeNames(typeNames);
                defaultAssemblyBuilder.AddBuildProvider(languageFreeBuildProvider);
            }

            // Only compile the default assembly builder if it's not part of language specific
            // assembly builder (which will be compiled separately)
            if (newDefaultAssemblyBuilder) {
                // Compile the default assembly builder.
                CompileAssemblyBuilder(defaultAssemblyBuilder);
            }
        }

        CompileAssemblyBuilderParallel(assemblyBuilders.Values);
        
        return true;
    }

    private void CompileAssemblyBuilderParallel(ICollection assemblyBuilders) {

        int maxConcurrent = Math.Min(assemblyBuilders.Count, CompilationUtil.MaxConcurrentCompilations);

        if (maxConcurrent < 2) {
            // Not using Parallel.ForEach to avoid performance penalty
            foreach (AssemblyBuilder assemblyBuilder in assemblyBuilders) {
                CompileAssemblyBuilder(assemblyBuilder);
            }
        }

        else {
            // devdiv 



            ConcurrentDictionary<AssemblyBuilder, CompilerResults> buildResults = new ConcurrentDictionary<AssemblyBuilder, CompilerResults>();
            ConcurrentDictionary<AssemblyBuilder, CompilerResults> buildErrors = new ConcurrentDictionary<AssemblyBuilder, CompilerResults>();
            
            try {
                Parallel.ForEach(assemblyBuilders.Cast<AssemblyBuilder>(),
                    new ParallelOptions { MaxDegreeOfParallelism = maxConcurrent },
                    builder =>
                    {
                        CompilerResults results;
                        try {
                            results = builder.Compile();
                        }
                        catch (HttpCompileException e) { 
                            buildErrors[builder] = e.Results;                            
                            throw;
                        }                        
                        buildResults[builder] = results;                        
                    });
            }
            catch (AggregateException e) {
                ExceptionDispatchInfo.Capture(e.GetBaseException()).Throw();
            }
            finally {
                // Before throwing the aggregated compilation exception, cache the build results first
                // This follows the execution order for the single thread case
                foreach (var pair in buildErrors) {
                    CacheCompileErrors(pair.Key, pair.Value);
                }
                foreach (var pair in buildResults) {
                    CacheAssemblyResults(pair.Key, pair.Value);
                }                
            }
        }
    }

    private void CompileAssemblyBuilder(AssemblyBuilder builder) {

        CompilerResults results;

        try {
            results = builder.Compile();
        }
        catch (HttpCompileException e) {
            CacheCompileErrors(builder, e.Results);
            throw;
        }

        CacheAssemblyResults(builder, results);
    }
}

}
