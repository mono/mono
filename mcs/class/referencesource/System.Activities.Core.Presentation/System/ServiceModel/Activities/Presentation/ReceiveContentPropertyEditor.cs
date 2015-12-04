//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System.Windows;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Converters;
    using System.Activities.Presentation.View;

    sealed class ReceiveContentPropertyEditor : DialogPropertyValueEditor
    {
        public ReceiveContentPropertyEditor()
        {
            this.InlineEditorTemplate = (DataTemplate)MessagingContentPropertyEditorResources.GetResources()["ReceiveContentPresenter_InlineEditorTemplate"];
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            ModelPropertyEntryToModelItemConverter converter = new ModelPropertyEntryToModelItemConverter();
            ModelPropertyEntryToModelItemConverter.Container container = (ModelPropertyEntryToModelItemConverter.Container)converter.Convert(propertyValue, null, null, null);
            ReceiveContentDialog.ShowDialog(container.ModelItem, container.Context, container.WorkflowViewElement);
        }
    }
}
