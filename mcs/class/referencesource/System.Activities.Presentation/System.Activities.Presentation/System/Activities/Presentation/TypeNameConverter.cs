//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Model;
    using System.Globalization;
    using System.Windows.Data;
    using Microsoft.Activities.Presentation;

    internal sealed class TypeNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Type type = value as Type;

            if (type == null)
            {
                ModelItem modelItem = value as ModelItem;
                if (modelItem != null)
                {
                    type = modelItem.GetCurrentValue() as Type;
                }
            }

            bool fullName = bool.Parse(parameter.ToString());

            return TypeNameHelper.GetDisplayName(type, fullName);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }
    }
}
