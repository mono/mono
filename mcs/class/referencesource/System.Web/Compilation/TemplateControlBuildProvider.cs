//------------------------------------------------------------------------------
// <copyright file="TemplateControlBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.IO;
using System.Collections;
using System.CodeDom.Compiler;
using System.Web.Util;
using System.Web.UI;

internal abstract class TemplateControlBuildProvider: BaseTemplateBuildProvider {

    internal virtual DependencyParser CreateDependencyParser() { return null; }

    internal override ICollection GetBuildResultVirtualPathDependencies() {
        DependencyParser parser = CreateDependencyParser();
        if (parser == null) return null;

        parser.Init(VirtualPathObject);
        return parser.GetVirtualPathDependencies();
    }

    internal override BuildResult CreateBuildResult(CompilerResults results) {

        // If the page is compiled, use the default base class logic
        if (Parser.RequiresCompilation)
            return base.CreateBuildResult(results);

        return CreateNoCompileBuildResult();
    }

    public override Type GetGeneratedType(CompilerResults results) {
        // Use the DelayLoadType for templates, so that we can avoid 
        // loading assemblies during compilation where possible.
        return GetGeneratedType(results, useDelayLoadTypeIfEnabled: true);
    }

    internal abstract BuildResultNoCompileTemplateControl CreateNoCompileBuildResult();
}

}
