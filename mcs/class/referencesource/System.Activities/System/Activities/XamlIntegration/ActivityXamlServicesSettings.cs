//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.XamlIntegration
{
    using System;

    public class ActivityXamlServicesSettings
    {
        public bool CompileExpressions
        {
            get;
            set;
        }

        public LocationReferenceEnvironment LocationReferenceEnvironment
        {
            get;
            set;
        }
    }
}
