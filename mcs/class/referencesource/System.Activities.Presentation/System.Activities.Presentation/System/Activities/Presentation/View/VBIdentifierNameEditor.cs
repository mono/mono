//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.View;
    using System.Windows;

    internal class VBIdentifierNameEditor : PropertyValueEditor
    {
        public VBIdentifierNameEditor()
        {
            this.InlineEditorTemplate = (DataTemplate)EditorResources.GetResources()["VBIdentifierNameEditorTemplate"];
        }
    }
}
