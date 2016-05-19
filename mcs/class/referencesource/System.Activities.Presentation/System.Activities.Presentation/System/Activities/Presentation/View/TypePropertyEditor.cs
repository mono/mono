//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.View;
    using System.Windows;

    sealed class TypePropertyEditor : PropertyValueEditor
    {
        public const string AllowNull = "AllowNull";
        public const string BrowseTypeDirectly = "BrowseTypeDirectly";
        public const string Filter = "Filter";

        public TypePropertyEditor()
        {
            this.InlineEditorTemplate = (DataTemplate)EditorResources.GetResources()["TypeBrowser_InlineEditorTemplate"];
        }
    }
}
