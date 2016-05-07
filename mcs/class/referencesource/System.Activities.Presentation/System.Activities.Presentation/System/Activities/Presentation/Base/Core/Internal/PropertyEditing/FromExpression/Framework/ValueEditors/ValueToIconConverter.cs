// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\ValueEditors
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.ValueEditors
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Diagnostics.CodeAnalysis;
    using System.Activities.Presentation;

    // <summary>
    // Implement this interface to provide icons to the ValueToIconProvider
    // </summary>
    internal interface IIconProvider
    {
        ImageSource GetIconAsImageSource(object key, object parameter);
    }

    // <summary>
    // Gets an Icon as a Brush from the IIconProvider passed in as the first object of a multibinding
    // using the second object as the key.
    // </summary>

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class ValueToIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2)
            {
                IIconProvider iconProvider = values[0] as IIconProvider;
                object objectToLookUp = values[1];
                if (iconProvider != null && objectToLookUp != null)
                {
                    return iconProvider.GetIconAsImageSource(objectToLookUp, parameter);
                }
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException(ExceptionStringTable.NoConvertBackForValueToIconConverter));
        }
    }
}
