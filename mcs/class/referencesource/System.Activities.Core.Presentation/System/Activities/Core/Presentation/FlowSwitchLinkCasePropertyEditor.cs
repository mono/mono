//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Windows;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Converters;
    using System.Activities.Presentation.View;
    using System.Activities.Core.Presentation.Themes;

    sealed class FlowSwitchLinkCasePropertyEditor : PropertyValueEditor
    {
        public FlowSwitchLinkCasePropertyEditor()
        {
            this.InlineEditorTemplate = (DataTemplate)EditorCategoryTemplateDictionary.Instance.GetCategoryTemplate("FlowSwitchLinkCase_InlineEditorTemplate");
        }
    }
}
