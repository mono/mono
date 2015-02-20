//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Data;
    using System.Activities.Presentation.Model;
    using System.Runtime;
    using System.Activities.Presentation.Services;

    // This type converter converts from Argument to Argument.Expression
    // on the reverse direction it creates  a new Argument and sets the expression on it with the value
    // The direction of the created argument is provided by the converter parameter.
    public class ArgumentToExpressionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object convertedValue = null;
            if (value != null)
            {
                ModelItem argumentModelItem = value as ModelItem;
                if (argumentModelItem != null &&
                    argumentModelItem.Properties["Expression"] != null &&
                    argumentModelItem.Properties["Expression"].Value != null)
                {
                    convertedValue = argumentModelItem.Properties["Expression"].Value;
                }
            }
            return convertedValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Argument target  = null;
            ArgumentDirection direction = ArgumentDirection.In;
            string directionString = parameter as string;
            if (!string.IsNullOrEmpty(directionString))
            {
                direction = (ArgumentDirection)Enum.Parse(typeof(ArgumentDirection), directionString);
            }

            ActivityWithResult expression = value as ActivityWithResult;
            ModelItem valueExpressionModelItem = value as ModelItem;
            if (valueExpressionModelItem != null && typeof(ActivityWithResult).IsAssignableFrom(valueExpressionModelItem.ItemType))
            {
                expression = (ActivityWithResult)valueExpressionModelItem.GetCurrentValue();
            }

            if (expression != null)
            {

                // In the In case the expression is of type Activity<T> so we want to create InArgument<T>
                // In Out and InOut case the expresion is Activity<Location<T>> we want to create OutArgument<T>,InOutArgument<T>
                Type argumentType;
                if (direction == ArgumentDirection.In)
                {
                    argumentType = expression.ResultType;
                }
                else
                {
                    // if expression type is Location<T> argument type is T
                    if (expression.ResultType.IsGenericType)
                    {
                        argumentType = expression.ResultType.GetGenericArguments()[0];
                    }
                    //expression type is not a Location<T>, probably an error, but let's give it a try...
                    else
                    {
                        argumentType = expression.ResultType;
                    }
                }
                try
                {
                    target = Argument.Create(argumentType, direction);
                    target.Expression = expression;
                }
                catch (Exception err)
                {
                    Trace.WriteLine(err.ToString());
                    throw;
                }

            }
            if (targetType == typeof(ModelItem))
            {
                if (valueExpressionModelItem == null)
                {
                    return null;
                }
                else
                {
                    return ((ModelServiceImpl)valueExpressionModelItem.GetEditingContext().Services.GetService<ModelService>()).WrapAsModelItem(target);
                }
            }
            else
            {
                return target;
            }
        }
    }
}
