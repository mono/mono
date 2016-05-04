//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Sqm
{
    internal enum WorkflowDesignerFeatureId
    {
        None = 0,
        ResetZoom = 1,
        FitToScreen = 2,
        Minimap = 3,        // The number of times minimap is opened
        Breadcrumb = 4,
        ExpandAll = 5,
        CollapseAll = 6,
        Restore = 7,
        OpenChild = 8,      // Context menu only
        ViewParent = 9,     // Context menu only
        CopyAsImage = 10,   // Context menu + shortcut key
        SaveAsImage = 11,   // Context menu + shortcut key
    }

    static class FeatureUsageCounter
    {
        internal static void ReportUsage(IVSSqmService sqmService, WorkflowDesignerFeatureId featureId)
        {
            if (sqmService != null)
            {
                uint[] data = new uint[1];
                data[0] = (uint)featureId;
                sqmService.AddArrayToStream((int)DataPointIds.FeatureUsageCount, data, data.Length);
            }
        }
    }
}
