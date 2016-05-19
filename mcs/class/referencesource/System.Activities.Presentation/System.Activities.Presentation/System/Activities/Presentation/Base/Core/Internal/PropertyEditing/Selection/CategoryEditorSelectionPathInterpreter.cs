//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Selection 
{
    using System;
    using System.Diagnostics;
    using System.Windows;

    using System.Activities.Presentation;
    using System.Activities.Presentation.PropertyEditing;

    using System.Activities.Presentation.Internal.PropertyEditing.Model;

    // <summary>
    // Helper class that knows how to construct and interpret SelectionPaths leading
    // to category editors.
    // </summary>
    internal class CategoryEditorSelectionPathInterpreter : ISelectionPathInterpreter 
    {
        private static CategoryEditorSelectionPathInterpreter _instance = new CategoryEditorSelectionPathInterpreter();
        private CategoryEditorSelectionPathInterpreter() 
        {
        }
        public static CategoryEditorSelectionPathInterpreter Instance 
        { get { return _instance; } }

        public string PathTypeId 
        { get { return "Cider_CategoryEditorPath"; } }

        // <summary>
        // Creates an instance of SelectionPath to the specified category editor that
        // this class knows how to interpret.
        // </summary>
        // <param name="editorTypeName">Editor type name to create the path to</param>
        // <returns>A new instance of SelectionPath to the specified category editor</returns>
        public SelectionPath ConstructSelectionPath(string editorTypeName) 
        {
            if (string.IsNullOrEmpty(editorTypeName)) 
            {
                throw FxTrace.Exception.ArgumentNull("editorTypeName");
            }
            return new SelectionPath(PathTypeId, editorTypeName);
        }

        // ISelectionPathInterpreter Members

        public DependencyObject ResolveSelectionPath(CategoryList root, SelectionPath path, out bool pendingGeneration) 
        {
            pendingGeneration = false;
            if (path == null || !string.Equals(PathTypeId, path.PathTypeId)) 
            {
                Debug.Fail("Invalid SelectionPath specified.");
                return null;
            }

            if (root == null) 
            {
                Debug.Fail("No CategoryList specified.");
                return null;
            }

            string editorTypeName = path.Path;

            if (string.IsNullOrEmpty(editorTypeName)) 
            {
                Debug.Fail("Invalid SelectionPath specified.");
                return null;
            }

            ModelCategoryEntry category;
            CategoryEditor editor = root.FindCategoryEditor(editorTypeName, out category);
            if (editor == null || category == null) 
            {
                return null;
            }

            return root.FindCategoryEditorVisual(editor, category, out pendingGeneration);
        }
    }
}
