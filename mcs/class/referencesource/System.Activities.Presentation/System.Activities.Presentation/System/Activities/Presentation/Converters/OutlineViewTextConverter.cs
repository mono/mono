//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System.Activities.Presentation.Model;
    using System.Globalization;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Data;
    using Microsoft.Activities.Presentation;

    internal class OutlineViewTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string displayName = string.Empty;
            if (values[0] != DependencyProperty.UnsetValue)
            {
                displayName = values[0] as string;
            }

            if (string.IsNullOrEmpty(displayName) && values[1] != DependencyProperty.UnsetValue)
            {
                ModelItem modelItem = values[1] as ModelItem;
                if (modelItem != null)
                {
                    ModelProperty nameProperty = modelItem.Properties["Name"];
                    if (nameProperty != null && nameProperty.Value != null)
                    {
                        displayName = nameProperty.Value.GetCurrentValue() as string;
                    }

                    if (string.IsNullOrEmpty(displayName))
                    {
                        Fx.Assert(modelItem.ItemType != null && modelItem.ItemType.Name != null, "ModelItem should always have a name");
                        displayName = TypeNameHelper.GetDisplayName(modelItem.ItemType, false);
                    }
                }
            }

            return displayName;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException());
        }
    }
}
