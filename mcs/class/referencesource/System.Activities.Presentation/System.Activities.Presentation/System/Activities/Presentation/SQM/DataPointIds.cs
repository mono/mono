//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Sqm
{
    enum DataPointIds
    {
        // DO NOT CHANGE THE VALUES BELOW
        // These values are the official IDs assigned
        ActivityUsageCount = 748,
        FeatureUsageCount = 749,
        // IMPORTANT: If new values are added, please also update VSSqmService.IsWorkflowDesignerDataPoint
        //  in the file below, otherwise the data being sent will be filtered out.
        //  Microsoft.Visualstudio.Activities.Addin\Microsoft\VisualStudio\Activities\AddIn\VSSqmService.cs
    }
}
