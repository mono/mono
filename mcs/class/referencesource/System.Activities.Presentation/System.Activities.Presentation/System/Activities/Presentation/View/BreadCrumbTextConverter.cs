//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Windows.Data;
    using System.Activities.Presentation.Model;

    internal class BreadCrumbTextConverter : IMultiValueConverter
    {
        const int MaxDisplayNameLength = 20;
        double pixelsPerChar = 6.5;

        internal double PixelsPerChar
        {
            get { return this.pixelsPerChar; }
            set { this.pixelsPerChar = Math.Max(5.0, value); }
        }

        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int actualDisplayNameLength = MaxDisplayNameLength;
            ModelItem boundModelItem = values[0] as ModelItem;

            // default to root item's typename
            string breadCrumbText = (null != boundModelItem ? boundModelItem.ItemType.Name : "<null>");
            // if there is a display name property on root use that as the file name.
            if (values[1] is ModelItem)
            {
                ModelItem displayNameProperty = (ModelItem)values[1];
                if (typeof(string) == displayNameProperty.ItemType)
                {
                    values[1] = displayNameProperty.GetCurrentValue();
                }
            }
            if (values[1] is string)
            {
                string displayName = (string)values[1];
                if (!displayName.Equals(string.Empty))
                {
                    breadCrumbText = displayName;
                }
            }
            if (values.Length == 3 && values[2] is double)
            {
                double actualControlWidth = (double)values[2];
                actualDisplayNameLength = (int)Math.Max(MaxDisplayNameLength, actualControlWidth / pixelsPerChar);

            }
            if (breadCrumbText.Length > actualDisplayNameLength)
            {
                breadCrumbText = breadCrumbText.Substring(0, actualDisplayNameLength - 3) + "...";
            }
            return breadCrumbText;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException());
        }
    }
}
