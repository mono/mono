//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Converters;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Core.Presentation.Themes;
    using System.Windows;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Model;
    using System.Collections.ObjectModel;
    using System.Windows.Controls;

    sealed class ArgumentCollectionPropertyEditor : DialogPropertyValueEditor 
    {
        public ArgumentCollectionPropertyEditor()
        {
            this.InlineEditorTemplate = EditorCategoryTemplateDictionary.Instance.GetCategoryTemplate("ArgumentCollection_InlineTemplate");
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            ModelPropertyEntryToOwnerActivityConverter propertyEntryConverter =
                new ModelPropertyEntryToOwnerActivityConverter();

            ModelItem activityModelItem =
                (ModelItem)propertyEntryConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null);

            ModelItem parentModelItem =
                (ModelItem)propertyEntryConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), true, null);

            EditingContext context = ((IModelTreeItem)activityModelItem).ModelTreeManager.Context;

            var inputData = parentModelItem.Properties[propertyValue.ParentProperty.PropertyName].Collection;            

            DynamicArgumentDesignerOptions options = new DynamicArgumentDesignerOptions
            { 
                Title = propertyValue.ParentProperty.DisplayName, 
            };

            using (EditingScope scope = context.Services.GetRequiredService<ModelTreeManager>().CreateEditingScope(StringResourceDictionary.Instance.GetString("InvokeMethodParameterEditing"), true))
            {
                if (DynamicArgumentDialog.ShowDialog(activityModelItem, inputData, context, activityModelItem.View, options))
                {
                    scope.Complete();
                }
            }
        }
    }
}
