//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Collections.Generic;

    /// <summary>
    /// This class is to hold the searchable strings extracted from the modeltree. All strings 
    /// are well ordered. And StartIndex point to the cursor in the content. It depends on the
    /// current selection in the model item tree.Search implementor can start their search from
    /// the StartIndex.
    /// </summary>
    [Serializable]
    public sealed class TextImage
    {
        public int StartLineIndex { get; set; }
        public IList<string> Lines { get; internal set; }
    }
}
