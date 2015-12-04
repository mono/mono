//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Core.Presentation.Themes;

    sealed class BindingPropertyValueEditor : PropertyValueEditor
    {
        public BindingPropertyValueEditor()
        {
            this.InlineEditorTemplate = EditorCategoryTemplateDictionary.Instance.GetCategoryTemplate("Binding_InlineEditorTemplate");
        }
    }
}
