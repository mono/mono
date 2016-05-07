//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{

    class SearchableEntry
    {
        internal int LineNumber { get; set; }
        internal SearchableEntryOption SearchableEntryType { get; set; }
        internal ModelItem ModelItem { get; set; }
        internal ModelProperty ModelProperty { get; set; }
        internal string Text { get; set; }
        internal string PropertyPath { get; set; }
    }
}
