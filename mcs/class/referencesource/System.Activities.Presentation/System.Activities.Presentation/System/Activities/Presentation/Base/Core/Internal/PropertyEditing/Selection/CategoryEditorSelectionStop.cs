//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Selection 
{
    using System;
    using System.Globalization;
    using System.Activities.Presentation;

    // <summary>
    // Helper class used to manage the selection stop behavior of a given CategoryContainer's basic
    // and advanced sections.  It deals with both expanding and collapsing of the specified section
    // as well as knowing how to get a SelectionPath leading to its heading.
    // </summary>
    internal class CategoryEditorSelectionStop : ISelectionStop 
    {

        private string _editorTypeName;
        private SelectionPath _selectionPath;

        // <summary>
        // Creates a new CategoryEditorSelectionStop that wraps around the
        // specified category editor.
        // </summary>
        // <param name="editorTypeName">Contained category editor type name</param>
        public CategoryEditorSelectionStop(string editorTypeName) 
        {
            if (string.IsNullOrEmpty(editorTypeName)) 
            {
                throw FxTrace.Exception.ArgumentNull("editorTypeName");
            }

            _editorTypeName = editorTypeName;
        }

        // <summary>
        // Gets true, throws on set
        // </summary>
        public bool IsExpanded 
        {
            get { return true; }
            set { throw FxTrace.Exception.AsError(new InvalidOperationException()); }
        }

        // <summary>
        // Gets false
        // </summary>
        public bool IsExpandable 
        {
            get { return false; }
        }

        // <summary>
        // Gets a SelectionPath that leads to the contained category editor
        // </summary>
        public SelectionPath Path 
        {
            get {
                if (_selectionPath == null)
                {
                    _selectionPath = CategoryEditorSelectionPathInterpreter.Instance.ConstructSelectionPath(_editorTypeName);
                }

                return _selectionPath;
            }
        }

        // <summary>
        // Gets a description of the contained category editor
        // to expose through automation
        // </summary>
        public string Description 
        {
            get {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Properties.Resources.PropertyEditing_SelectionStatus_CategoryEditor,
                    _editorTypeName);
            }
        }
    }
}
