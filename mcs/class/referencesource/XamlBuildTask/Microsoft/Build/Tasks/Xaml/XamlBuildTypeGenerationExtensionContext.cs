//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using Microsoft.Build.Framework;

    public sealed class XamlBuildTypeGenerationExtensionContext : BuildExtensionContext
    {
        public ITaskItem InputTaskItem
        {
            get;
            internal set;
        }
    }
}
