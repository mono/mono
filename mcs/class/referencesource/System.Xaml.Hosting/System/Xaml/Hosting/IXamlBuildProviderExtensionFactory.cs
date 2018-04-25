//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Xaml.Hosting
{
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Web.Compilation;

    internal interface IXamlBuildProviderExtensionFactory
    {
        IXamlBuildProviderExtension GetXamlBuildProviderExtension();
    }
}
