//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------


namespace Microsoft.Build.Tasks.Xaml
{
    using System.CodeDom;

    public interface IXamlBuildTypeGenerationExtension
    {
        bool Execute(ClassData classData, XamlBuildTypeGenerationExtensionContext buildContext);
    }
}
