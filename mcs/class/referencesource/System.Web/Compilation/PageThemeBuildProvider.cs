//------------------------------------------------------------------------------
// <copyright file="PageThemeBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.Util;

internal class PageThemeBuildProvider: BaseTemplateBuildProvider {

    private VirtualPath _virtualDirPath;
    private IList _skinFileList;
    private ArrayList _cssFileList;

    internal PageThemeBuildProvider(VirtualPath virtualDirPath) {
        _virtualDirPath = virtualDirPath;

        // 

        SetVirtualPath(virtualDirPath);
    }

    internal virtual String AssemblyNamePrefix {
        get {
            return BuildManager.AppThemeAssemblyNamePrefix;
        }
    }

    internal void AddSkinFile(VirtualPath virtualPath) {
        if (_skinFileList == null)
            _skinFileList = new StringCollection();

        _skinFileList.Add(virtualPath.VirtualPathString);
    }

    internal void AddCssFile(VirtualPath virtualPath) {
        if (_cssFileList == null)
            _cssFileList = new ArrayList();

        _cssFileList.Add(virtualPath.AppRelativeVirtualPathString);
    }

    protected override TemplateParser CreateParser() {
        if (_cssFileList != null) {
            _cssFileList.Sort();
        }

        return new PageThemeParser(_virtualDirPath, _skinFileList, _cssFileList);
    }

    internal override BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(TemplateParser parser) {
        return new PageThemeCodeDomTreeGenerator((PageThemeParser)parser);
    }
}

internal class GlobalPageThemeBuildProvider : PageThemeBuildProvider {
    private VirtualPath _virtualDirPath;

    internal GlobalPageThemeBuildProvider(VirtualPath virtualDirPath) : base (virtualDirPath) {
        _virtualDirPath = virtualDirPath;
    }

    internal override String AssemblyNamePrefix {
        get {
            return BuildManager.GlobalThemeAssemblyNamePrefix;
        }
    }

    public override ICollection VirtualPathDependencies {
        get {
            ICollection parentDependencies = base.VirtualPathDependencies;

            string themeName = _virtualDirPath.FileName;

            // Here we add the app theme dir to the dependency list so that global theme will be invalidated
            // from cache when app theme is added.
            CaseInsensitiveStringSet sourceDependencies = new CaseInsensitiveStringSet();
            sourceDependencies.AddCollection(parentDependencies);
            string appThemesVdir = UrlPath.SimpleCombine(HttpRuntime.AppDomainAppVirtualPathString, HttpRuntime.ThemesDirectoryName);
            string appThemeVdir = appThemesVdir + '/' + themeName;
            if (HostingEnvironment.VirtualPathProvider.DirectoryExists(appThemeVdir)) {
                sourceDependencies.Add(appThemeVdir);
            }
            else {
                sourceDependencies.Add(appThemesVdir);
            }

            return sourceDependencies;
        }
    }
}

}
