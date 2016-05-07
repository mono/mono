//------------------------------------------------------------------------------
// <copyright file="UserControlBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------



namespace System.Web.Compilation {

using System;
using System.Web.UI;
using System.Web.Configuration;

[BuildProviderAppliesTo(BuildProviderAppliesTo.Code | BuildProviderAppliesTo.Web)]
internal class UserControlBuildProvider: TemplateControlBuildProvider {
    internal override DependencyParser CreateDependencyParser() {
        return new UserControlDependencyParser();
    }

    protected override TemplateParser CreateParser() {
        return new UserControlParser();
    }

    internal override BaseCodeDomTreeGenerator CreateCodeDomTreeGenerator(TemplateParser parser) {
        return new UserControlCodeDomTreeGenerator((UserControlParser)parser);
    }

    internal override BuildResultNoCompileTemplateControl CreateNoCompileBuildResult() {
        return new BuildResultNoCompileUserControl(Parser.BaseType, Parser);
    }
}

}
