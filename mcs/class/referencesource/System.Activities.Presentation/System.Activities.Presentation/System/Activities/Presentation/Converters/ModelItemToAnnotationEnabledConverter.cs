//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.Converters
{
    using System.Activities.Presentation.Model;
    using System.Windows.Data;

    internal class ModelItemToAnnotationEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, Globalization.CultureInfo culture)
        {
            ModelItem modelItem = value as ModelItem;

            if (modelItem != null)
            {
                EditingContext editingContext = modelItem.GetEditingContext();
                if (editingContext != null)
                {
                    return editingContext.Services.GetService<DesignerConfigurationService>().AnnotationEnabled;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, Globalization.CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }
    }
}
