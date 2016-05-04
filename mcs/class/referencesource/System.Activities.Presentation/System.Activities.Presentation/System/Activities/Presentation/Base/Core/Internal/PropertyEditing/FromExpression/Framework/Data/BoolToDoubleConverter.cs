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
    using System.Runtime;

    // <summary>
    // Maps a bool to a FontWeight. True becomes Bold, and False is Normal.
    // </summary>

    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
    internal class BoolToDoubleConverter : IValueConverter
    {
        // Private Fields

        private double trueValue = 0;
        private double falseValue = 0;

        // Public Properties

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public double TrueValue
        {
            get
            {
                return this.trueValue;
            }
            set
            {
                this.trueValue = value;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public double FalseValue
        {
            get
            {
                return this.falseValue;
            }
            set
            {
                this.falseValue = value;
            }
        }


        // IValueConverter Implementation

        public object ConvertBack(object o, Type targetType, object parameter, CultureInfo culture)
        {
            Fx.Assert(false, "Never expecting the inverse transform to be called");
            return null;
        }

        public object Convert(object o, Type targetType, object parameter, CultureInfo culture)
        {
            Fx.Assert(o is bool, "Ensure that transformed element is a boolean");

            if ((bool)o)
            {
                return this.trueValue;
            }
            else
            {
                return this.falseValue;
            }
        }

    }
}
