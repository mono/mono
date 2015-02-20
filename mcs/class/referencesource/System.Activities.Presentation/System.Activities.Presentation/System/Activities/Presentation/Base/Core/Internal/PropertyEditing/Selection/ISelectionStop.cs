//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Selection 
{
    using System.ComponentModel;

    // <summary>
    // Interface we use to mark objects in the visual tree as ones
    // that can be selected and, potentially, expanded and collapsed.
    // </summary>
    internal interface ISelectionStop 
    {

        // <summary>
        // Gets or sets the IsExpanded flag on the selected object.
        // Setting this flag on an object that doesn't return true
        // from IsExpandable is undefined.
        // </summary>
        bool IsExpanded 
        { get; set; }

        // <summary>
        // Gets a flag indicating whether this object can be expanded
        // and collapsed using the IsExpanded flag.
        // </summary>
        bool IsExpandable 
        { get; }

        // <summary>
        // Gets the SelectionPath to this object that can be used to
        // restore selection to it when the PropertyInspector reloads
        // (such as when we recycle the app domains).
        // </summary>
        SelectionPath Path 
        { get; }

        // <summary>
        // Gets the description to show through the Automation API
        // defining what item is selected.
        // </summary>
        string Description 
        { get; }
    }
}
