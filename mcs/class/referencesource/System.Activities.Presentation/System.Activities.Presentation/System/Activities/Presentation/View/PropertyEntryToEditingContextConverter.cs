namespace System.Activities.Presentation.View
{
    using System.Windows.Data;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Internal.PropertyEditing.Model;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Converters;

    class PropertyEntryToEditingContextConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            PropertyEntry propertyEntry = value as PropertyEntry;
            if (null == propertyEntry)
            {
                PropertyValue propertyValue = value as PropertyValue;
                if (null != propertyValue)
                {
                    propertyEntry = propertyValue.ParentProperty;
                }
            }
            return ModelPropertyEntryToOwnerActivityConverter.Convert(propertyEntry as ModelPropertyEntry, false).GetEditingContext();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }

        #endregion
    }
}
