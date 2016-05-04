//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Activities.Presentation.PropertyEditing;
    using System.Windows;

    sealed class HandleValueEditor : PropertyValueEditor
    {
        public HandleValueEditor()
        {
            this.InlineEditorTemplate = (DataTemplate)EditorResources.GetResources()["handleValueEditor"];
        }
    }
}
