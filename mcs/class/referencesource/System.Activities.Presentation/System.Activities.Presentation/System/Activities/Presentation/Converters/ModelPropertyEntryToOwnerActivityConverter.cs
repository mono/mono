//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Model;
    using System.Globalization;
    using System.Runtime;
    using System.ServiceModel.Activities;
    using System.Windows.Data;

    // This type converter converts from ModelPropertyEntry to ModelItem that owns the property
    public class ModelPropertyEntryToOwnerActivityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //converter can be parametrized - by default, always return parent model item of type Activity or WorkflowService, 
            //but if false is passed in - simply return first parent
            bool returnParentWorkflowElement = true;
            if (null != parameter && parameter is bool)
            {
                returnParentWorkflowElement = (bool)parameter;
            }

            return ModelPropertyEntryToOwnerActivityConverter.Convert(value as ModelPropertyEntry, returnParentWorkflowElement);
        }

        internal static ModelItem Convert(ModelPropertyEntry modelPropertyEntry, bool returnParentActivity)
        {
            ModelItem convertedValue = null;
            if (modelPropertyEntry != null)
            {
                ModelProperty property = modelPropertyEntry.FirstModelProperty;
                if (property != null)
                {
                    convertedValue = property.Parent;
                    if (returnParentActivity)
                    {
                        while (convertedValue != null)
                        {
                            Type itemType = convertedValue.ItemType;
                            if (typeof(Activity).IsAssignableFrom(itemType) || typeof(WorkflowService).IsAssignableFrom(itemType))
                            {
                                break;
                            }

                            convertedValue = convertedValue.Parent;
                        }
                    }
                }
            }

            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Fx.Assert("this value converter only works on the forward direction");
            return null;
        }
    }
}
