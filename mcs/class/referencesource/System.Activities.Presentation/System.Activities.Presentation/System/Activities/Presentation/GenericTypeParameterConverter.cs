//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Windows.Data;
    using System.Globalization;
    
    internal sealed class GenericTypeParameterConverter : IValueConverter
    {
        bool useFullName = false;

        public GenericTypeParameterConverter() : this(false)
        {
        }

        public GenericTypeParameterConverter(bool useFullName)
        {
            this.useFullName = useFullName;
        }

        static IValueConverter baseFullNameConverter =
            new System.Activities.Presentation.Core.GenericTypeParameterConverter(true);
        static IValueConverter baseShortNameConverter = 
            new System.Activities.Presentation.Core.GenericTypeParameterConverter(false);

        IValueConverter Converter
        {
            get
            {
                return this.useFullName ? 
                    GenericTypeParameterConverter.baseFullNameConverter : GenericTypeParameterConverter.baseShortNameConverter;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.Converter.Convert(value, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.Converter.ConvertBack(value, targetType, parameter, culture);
        }
    }
}
