//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Editors 
{
    using System;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;

    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;
    using System.Activities.Presentation;

    // <summary>
    // Converter that takes an object and either returns it
    // or wraps it in an Image control if the object is of type ImageSource.
    // This class gets instantiated from XAML.
    // </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class ImageSourceToImageConverter : IValueConverter 
    {
        // IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) 
        {

            ImageSource imageSource = value as ImageSource;

            if (imageSource == null)
            {
                return value;
            }

            Image image = new Image();
            image.Source = imageSource;
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) 
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException());
        }

    }
}
