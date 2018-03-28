//------------------------------------------------------------------------------
// <copyright file="ApplicationBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.IO;
using System.Collections;
using System.CodeDom.Compiler;
using System.Web.Hosting;
using System.Web.Util;
using System.Web.UI;

internal class ApplicationBuildProvider: BaseTemplateBuildProvider {

    internal static BuildResultCompiledGlobalAsaxType GetGlobalAsaxBuildResult(bool isPrecompiledApp) {

        string cacheKey = BuildManager.GlobalAsaxAssemblyName;
        
        // Try the cache first, and if it's not there, compile it
        BuildResultCompiledGlobalAsaxType result = BuildManager.GetBuildResultFromCache(cacheKey) as
            BuildResultCompiledGlobalAsaxType;
        if (result != null)
            return result;

        // If this is a precompiled app don't attempt to compile it
        if (isPrecompiledApp)
            return null;

        VirtualPath virtualPath = BuildManager.GlobalAsaxVirtualPath;

        // If global.asax doesn't exist, just ignore it
        if (!virtualPath.FileExists())
            return null;

        // Compile global.asax
        ApplicationBuildProvider buildProvider = new ApplicationBuildProvider();
        buildProvider.SetVirtualPath(virtualPath);

        DateTime utcStart = DateTime.UtcNow;

        BuildProvidersCompiler bpc = new BuildProvidersCompiler(virtualPath /*configPath*/, 
            BuildManager.GenerateRandomAssemblyName(BuildManager.GlobalAsaxAssemblyName));

        // Set the BuildProvider using a single item collection
        bpc.SetBuildProviders(new SingleObjectCollection(buildProvider));

        CompilerResults results = bpc.PerformBuild();

        result = (BuildResultCompiledGlobalAsaxType) buildProvider.GetBuildResult(results);

        // Top level assembliy should not be cached to memory.
        result.CacheToMemory = false;

        // Cache it for next time
        BuildManager.CacheBuildResult(cacheKey, result, utcStart);

        // Return the compiled type
        return result;
    }

    protected override TemplateParser CreateParser() {
        return new ApplicationFileParser();
    }

    internal override BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(TemplateParser parser) {
        return new ApplicationFileCodeDomTreeGenerator((ApplicationFileParser)parser);
    }

    internal override BuildResultCompiledType CreateBuildResult(Type t) {
        BuildResultCompiledGlobalAsaxType result = new BuildResultCompiledGlobalAsaxType(t);

        // If global.asax contains <object> tags, set a flag to avoid doing useless work
        // later on in HttpApplicationFactory (VSWhidbey 453101)
        if (Parser.ApplicationObjects != null || Parser.SessionObjects != null)
            result.HasAppOrSessionObjects = true;

        return result;
    }
}

}
