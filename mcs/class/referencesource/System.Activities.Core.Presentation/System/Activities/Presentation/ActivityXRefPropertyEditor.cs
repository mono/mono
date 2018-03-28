//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Core.Presentation.Themes;

    sealed class ActivityXRefPropertyEditor : PropertyValueEditor 
    {
        public ActivityXRefPropertyEditor()
        {
            this.InlineEditorTemplate = EditorCategoryTemplateDictionary.Instance.GetCategoryTemplate("ActivityXRef_InlineEditorTemplate");
        }
    }
}
