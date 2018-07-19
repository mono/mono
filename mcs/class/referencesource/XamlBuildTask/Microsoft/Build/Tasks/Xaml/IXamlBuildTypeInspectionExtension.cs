//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System.Collections.Generic;

    public interface IXamlBuildTypeInspectionExtension
    {
        bool Execute(XamlBuildTypeInspectionExtensionContext buildContext);
    }
}
