//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.Toolbox
{
    using System;
    using System.Drawing;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Microsoft.Tools.Common;

    //This class is responsible for converting 'old' bitmap style, contained 
    //in ToolboxItem objects to WPF compatible ImageSource object

    [ValueConversion(typeof(Bitmap), typeof(ImageSource))]
    sealed class ToolboxItemImageConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Bitmap source = value as Bitmap;
            if (targetType == typeof(ImageSource) && null != source)
            {
                IntPtr hBitmap = source.GetHbitmap();
                try
                {
                    BitmapSizeOptions sizeOptions = BitmapSizeOptions.FromEmptyOptions();
                    return Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, sizeOptions);
                }
                finally
                {
                    Win32Interop.DeleteObject(hBitmap);
                }
            }
            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new NotSupportedException());
        }
    }
}
