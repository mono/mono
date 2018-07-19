//------------------------------------------------------------------------------
// <copyright file="ThemeDirectoryCompiler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.IO;
using System.Collections;
using System.Globalization;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Security.Permissions;
using System.Web.Hosting;
using System.Web.Util;
using System.Web.UI;

internal static class ThemeDirectoryCompiler {

    internal const string skinExtension = ".skin";

    internal static VirtualPath GetAppThemeVirtualDir(string themeName) {
        return HttpRuntime.AppDomainAppVirtualPathObject.SimpleCombineWithDir(HttpRuntime.ThemesDirectoryName + "/" + themeName);
    }

    internal static VirtualPath GetGlobalThemeVirtualDir(string themeName) {
        return BuildManager.ScriptVirtualDir.SimpleCombineWithDir(HttpRuntime.GlobalThemesDirectoryName + "/" + themeName);
    }

    // We need to Assert here since there could be user code on the stack (VSWhidbey 259563)
    [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
    internal static BuildResultCompiledType GetThemeBuildResultType(HttpContext context, string themeName) {
        using (new ApplicationImpersonationContext()) {
            return GetThemeBuildResultType(themeName);
        }
    }

    private static BuildResultCompiledType GetThemeBuildResultType(string themeName) {

        string appThemeCacheKey, globalThemeCacheKey = null;

        // First, check if the application theme is cached
        appThemeCacheKey = "Theme_" + Util.MakeValidTypeNameFromString(themeName);
        BuildResultCompiledType result = (BuildResultCompiledType)
            BuildManager.GetBuildResultFromCache(appThemeCacheKey);

        if (result == null) {
            // Then, check if the global theme is cached
            globalThemeCacheKey = "GlobalTheme_" + themeName;
            result = (BuildResultCompiledType) BuildManager.GetBuildResultFromCache(
                globalThemeCacheKey);
        }

        // If we found a theme buildresulttype, return it
        if (result != null)
            return result;

        bool gotLock = false;

        try {
            // Grab the compilation mutex
            CompilationLock.GetLock(ref gotLock);

            // Check the cache again now that we have the mutex
            result = (BuildResultCompiledType)BuildManager.GetBuildResultFromCache(appThemeCacheKey);
            if (result == null) {
                result = (BuildResultCompiledType)BuildManager.GetBuildResultFromCache(
                    globalThemeCacheKey);
            }

            if (result != null)
                return result;

        // Theme was not found in the caches; check if the directory exists.
        VirtualPath appVirtualDir, globalVirtualDir = null;

        appVirtualDir = GetAppThemeVirtualDir(themeName);
        PageThemeBuildProvider themeBuildProvider = null;

        VirtualPath virtualDir = appVirtualDir;

        string cacheKey = appThemeCacheKey;
        // If the theme directories do not exist, simply throw
        if (appVirtualDir.DirectoryExists()) {
            themeBuildProvider = new PageThemeBuildProvider(appVirtualDir);
        }
        else {
            globalVirtualDir = GetGlobalThemeVirtualDir(themeName);

            if (!globalVirtualDir.DirectoryExists()) {
                throw new HttpException(SR.GetString(SR.Page_theme_not_found, themeName));
            }

            virtualDir = globalVirtualDir;
            cacheKey = globalThemeCacheKey;
            themeBuildProvider = new GlobalPageThemeBuildProvider(globalVirtualDir);
        }

        // The directory exists (either app or global), so compile it
        DateTime utcStart = DateTime.UtcNow;

        VirtualDirectory vdir = virtualDir.GetDirectory();

        // Add all the .skin files to it
        AddThemeFilesToBuildProvider(vdir, themeBuildProvider, true);

        // Use predictable fixed names for theme assemblies.
        BuildProvidersCompiler bpc = new BuildProvidersCompiler(virtualDir, 
            themeBuildProvider.AssemblyNamePrefix + BuildManager.GenerateRandomAssemblyName(themeName));

        // Add the single build provider to the BuildProvidersCompiler
        bpc.SetBuildProviders(new SingleObjectCollection(themeBuildProvider));

        // Compile it
        CompilerResults results = bpc.PerformBuild();

        // Get the Type we care about from the BuildProvider
        result = (BuildResultCompiledType) themeBuildProvider.GetBuildResult(results);

        // Cache it for next time
        BuildManager.CacheBuildResult(cacheKey, result, utcStart);

        }
        finally {
            // Always release the mutex if we had taken it
            if (gotLock) {
                CompilationLock.ReleaseLock();
            }
        }
        return result;
    }

    private static void AddThemeFilesToBuildProvider(VirtualDirectory vdir,
        PageThemeBuildProvider themeBuildProvider, bool topLevel) {

        // Go through all the files in the directory
        foreach (VirtualFileBase child in vdir.Children) {

            // Recursive into subdirectories.
            if (child.IsDirectory) {
                AddThemeFilesToBuildProvider(child as VirtualDirectory, themeBuildProvider, false);
                continue;
            }

            // We only process .skin and .css files
            string extension = Path.GetExtension(child.Name);
            if ((StringUtil.EqualsIgnoreCase(extension, skinExtension)) && topLevel) {
                themeBuildProvider.AddSkinFile(child.VirtualPathObject);
                continue;
            }

            if (StringUtil.EqualsIgnoreCase(extension, ".css")) {
                themeBuildProvider.AddCssFile(child.VirtualPathObject);
                continue;
            }
        }
    }
}

}
