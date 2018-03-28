//------------------------------------------------------------------------------
// <copyright file="MasterPageBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.Web.Configuration;
using System.Web.UI;

[BuildProviderAppliesTo(BuildProviderAppliesTo.Code | BuildProviderAppliesTo.Web)]
internal class MasterPageBuildProvider: UserControlBuildProvider {
    internal override DependencyParser CreateDependencyParser() {
        return new MasterPageDependencyParser();
    }

    protected override TemplateParser CreateParser() {
        return new MasterPageParser();
    }

    internal override BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(TemplateParser parser) {
        return new MasterPageCodeDomTreeGenerator((MasterPageParser)parser);
    }

    internal override BuildResultNoCompileTemplateControl CreateNoCompileBuildResult() {
        return new BuildResultNoCompileMasterPage(Parser.BaseType, Parser);
    }
}
}
