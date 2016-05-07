// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Data
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Diagnostics.CodeAnalysis;

    // <summary>
    // (bool-to-Visibility) Maps true to Visible and false to Hidden.
    // </summary>

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class BoolToVisibilityHiddenConverter : IValueConverter
    {
        bool invertBoolean = false;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]

        public bool InvertBoolean
        {
            get { return this.invertBoolean; }
            set { this.invertBoolean = value; }
        }

        // IValueConverter Implementation
        public object ConvertBack(object o, Type targetType, object parameter, CultureInfo culture)
        {
            Visibility visibility = (Visibility)o;
            bool result = visibility == Visibility.Visible;
            if (this.invertBoolean)
            {
                result = !result;
            }
            return result;
        }

        public object Convert(object o, Type targetType, object parameter, CultureInfo culture)
        {
            bool show = (bool)o;
            if (this.invertBoolean)
            {
                show = !show;
            }
            return show ? Visibility.Visible : Visibility.Hidden;
        }
    }

}
