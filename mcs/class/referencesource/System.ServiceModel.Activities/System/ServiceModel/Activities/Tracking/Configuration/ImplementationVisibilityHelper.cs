//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Activities.Tracking.Configuration
{

    using System.Activities.Tracking;

    static class ImplementationVisibilityHelper
    {
        public static bool IsDefined(ImplementationVisibility value)
        {
            return value == ImplementationVisibility.All || value == ImplementationVisibility.RootScope;
        }
    }
}
