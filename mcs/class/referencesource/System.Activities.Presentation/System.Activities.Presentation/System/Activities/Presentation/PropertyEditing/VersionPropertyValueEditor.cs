//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.PropertyEditing
{
    using System.Activities.Presentation.View;
    using System.Windows;

    internal sealed class VersionPropertyValueEditor : PropertyValueEditor
    {
        public VersionPropertyValueEditor()
        {
            this.InlineEditorTemplate = EditorResources.GetResources()["VersionPropertyValueEditorTemplate"] as DataTemplate;
        }
    }
}
