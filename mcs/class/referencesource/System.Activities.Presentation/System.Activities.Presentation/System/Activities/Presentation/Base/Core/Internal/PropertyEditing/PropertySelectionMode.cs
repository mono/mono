//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing 
{
    using System;

    // <summary>
    // SelectionMode used for property selection.  'Default' means that with each object selection change,
    // we try to figure out the default property to select and we select it.  'Sticky' means that the user
    // has made some conscious decision as to what property should be selected and we try to preserve it
    // across object selection changes.
    // </summary>
    internal enum PropertySelectionMode 
    {
        Default,
        Sticky
    }
}
