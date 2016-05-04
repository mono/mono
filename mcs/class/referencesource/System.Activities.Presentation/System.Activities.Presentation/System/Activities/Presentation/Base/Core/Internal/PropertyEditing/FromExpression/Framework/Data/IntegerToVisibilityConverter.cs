// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Data
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    // <summary>
    // Maps an Integer to Visilbity. 0 becomes Hidden, non-zero becomes Visible.
    // </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class IntegerToVisibilityConverter : IValueConverter
    {
        private Visibility zeroValue = Visibility.Collapsed;

        private Visibility nonzeroValue = Visibility.Visible;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public Visibility ZeroValue
        {
            get { return this.zeroValue; }
            set { this.zeroValue = value; }
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public Visibility NonzeroValue
        {
            get { return this.nonzeroValue; }
            set { this.nonzeroValue = value; }
        }

        // IValueConverter Implementation

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Fx.Assert(false, "Never expecting the inverse transform to be called");
            return null;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Fx.Assert(value is int, "Ensure that transformed value is an integer");

            if ((value is int) && (int)value == 0)
            {
                return this.zeroValue;
            }
            else
            {
                return this.nonzeroValue;
            }
        }
    }
}
