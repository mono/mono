//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Selection 
{
    using System.Windows;

    // <summary>
    // Interface we use to separate logic that knows how to look up and resolve a given SelectionPath
    // into an actual visual object somewhere within a given CategoryList control instance.
    // </summary>
    internal interface ISelectionPathInterpreter 
    {

        // <summary>
        // Gets the token that uniquely identifies the type of SelectionPath instances
        // that a given ISelectionPathInterpreter knows how to interpret
        // </summary>
        string PathTypeId 
        { get; }

        // <summary>
        // Resolves the specified SelectionPath into an actual visual object somewhere within
        // the given CategoryList control instance.  The implementation can assume that the
        // SelectionPath instance passed into this method matches the PathTypeId specified
        // by this interface.
        // </summary>
        // <param name="root">CategoryList control to look into</param>
        // <param name="path">SelectionPath to resolve</param>
        // <param name="pendingGeneration">Set to true if the specified UI is under generating</param>
        // <returns>Resolved visual corresponding to the given path if found, null otherise.</returns>
        DependencyObject ResolveSelectionPath(CategoryList root, SelectionPath path, out bool pendingGeneration);
    }
}
