//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Core.Presentation.Themes;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Converters;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;

    internal class DelegateArgumentsValueEditor : DialogPropertyValueEditor
    {
        public DelegateArgumentsValueEditor()
        {
            this.InlineEditorTemplate = EditorCategoryTemplateDictionary.Instance.GetCategoryTemplate("DelegateArguments_InlineTemplate");
        }

        public override void ShowDialog(PropertyValue propertyValue, Windows.IInputElement commandSource)
        {
            ModelPropertyEntryToOwnerActivityConverter propertyEntryConverter = new ModelPropertyEntryToOwnerActivityConverter();
            ModelItem parentModelItem = (ModelItem)propertyEntryConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), true, null);
            EditingContext context = ((IModelTreeItem)parentModelItem).ModelTreeManager.Context;
            ModelItemDictionary inputData = parentModelItem.Properties[propertyValue.ParentProperty.PropertyName].Dictionary;
            DynamicArgumentDesignerOptions options = new DynamicArgumentDesignerOptions();
            options.Title = propertyValue.ParentProperty.DisplayName;

            DynamicArgumentDialog.ShowDialog(parentModelItem, inputData, context, parentModelItem.View, options);
        }
    }
}
