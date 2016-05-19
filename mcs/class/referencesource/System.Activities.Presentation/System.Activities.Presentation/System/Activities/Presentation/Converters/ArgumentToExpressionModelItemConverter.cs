//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Data;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;

    // This converter converts from Argument to Argument.Expression ModelItem using the ModelPropertyEntry instance.
    // on the reverse direction it creates  a new Argument and sets the expression on it with the value
    // The direction of the created argument is provided by the converter parameter.
    //1st binding is to the PropertyValue
    //2nd binding is to the ModelProprtyEntry (to get the modelItem).
    public class ArgumentToExpressionModelItemConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ModelItem convertedValue = null;
            if (values[1] != null)
            {
                ModelPropertyEntry modelPropertyEntry = values[1] as ModelPropertyEntry;
                if (modelPropertyEntry != null)
                {
                    ModelProperty property = modelPropertyEntry.FirstModelProperty;
                    if (property != null)
                    {
                        ModelItem argumentModelItem = property.Value;
                        if (argumentModelItem != null &&
                                argumentModelItem.Properties["Expression"] != null &&
                                argumentModelItem.Properties["Expression"].Value != null)
                        {
                            convertedValue = argumentModelItem.Properties["Expression"].Value;
                        }
                    }
                }
            }
            return convertedValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            Argument target = null;
            ArgumentDirection direction = ArgumentDirection.In;
            string directionString = parameter as string;
            if (!string.IsNullOrEmpty(directionString))
            {
                direction = (ArgumentDirection)Enum.Parse(typeof(ArgumentDirection), directionString);
            }

            Activity expression = null;
            ModelItem valueExpressionModelItem = value as ModelItem;
            if (valueExpressionModelItem != null && typeof(Activity).IsAssignableFrom(valueExpressionModelItem.ItemType))
            {
                expression = (Activity)valueExpressionModelItem.GetCurrentValue();
            }
            ActivityWithResult activityWithResult = expression as ActivityWithResult;
            if (expression != null && activityWithResult != null)
            {
                // In the In case the expression is of type Activity<T> so we want to create InArgument<T>
                // In Out and InOut case the expresion is Activity<Location<T>> we want to create OutArgument<T>,InOutArgument<T>
                Type argumentType;
                if (direction == ArgumentDirection.In)
                {
                    argumentType = activityWithResult.ResultType;
                }
                else
                {
                    // if expression type is Location<T> argument type is T
                    argumentType = activityWithResult.ResultType.GetGenericArguments()[0];
                }
                target = Argument.Create(argumentType, direction);
                target.Expression = activityWithResult;
            }
            return new object[] { target };
        }

    }
}
