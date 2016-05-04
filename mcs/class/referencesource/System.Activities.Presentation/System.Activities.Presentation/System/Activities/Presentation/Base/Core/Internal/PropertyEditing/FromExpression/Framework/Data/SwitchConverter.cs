// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Data
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Activities.Presentation;

    // <summary>
    // Transformer which maps from input values to output values, based on a list of SwitchCase children.
    // This isn't strictly a C-style 'switch' statement, since cases aren't guaranteed to be unique.
    // </summary>
    //
    [ContentProperty("Cases")]
    internal class SwitchConverter : DependencyObject, IValueConverter
    {
        static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register("DefaultValue", typeof(object), typeof(SwitchConverter));
        private List<SwitchCase> cases;

        public SwitchConverter()
        {
            this.cases = new List<SwitchCase>();
        }

        public List<SwitchCase> Cases
        {
            get { return this.cases; }
        }

        public object DefaultValue
        {
            get { return this.GetValue(SwitchConverter.DefaultValueProperty); }
            set { this.SetValue(SwitchConverter.DefaultValueProperty, value); }
        }

        // IValueConverter implementation
        public object Convert(object o, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (SwitchCase switchCase in this.Cases)
            {
                if (object.Equals(switchCase.In, o))
                {
                    return switchCase.Out;
                }
            }

            return this.DefaultValue;
        }

        public object ConvertBack(object o, Type targetType, object parameter, CultureInfo culture)
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException(ExceptionStringTable.SwitchConverterIsOneWay));
        }

    }

    // <summary>
    // Represents a mapping from an input value to an output value.
    // </summary>
    internal class SwitchCase : DependencyObject
    {
        static readonly DependencyProperty InProperty = DependencyProperty.Register("In", typeof(object), typeof(SwitchCase));
        static readonly DependencyProperty OutProperty = DependencyProperty.Register("Out", typeof(object), typeof(SwitchCase));

        public SwitchCase()
        {
        }

        // Properties
        public object In
        {
            get { return (object)this.GetValue(InProperty); }
            set { this.SetValue(InProperty, value); }
        }

        public object Out
        {
            get { return this.GetValue(OutProperty); }
            set { this.SetValue(OutProperty, value); }
        }
    }

    // <summary>
    // Convenience class for getting at a particular type.  Useful for databinding.
    // Used in XAML as: <TypeReference Type="*typeof(Foo)" />
    // </summary>
    internal sealed class TypeReference : DependencyObject
    {
        static readonly DependencyProperty TypeProperty = DependencyProperty.Register("Type", typeof(Type), typeof(TypeReference));

        public Type Type
        {
            get { return (Type)this.GetValue(TypeProperty); }
            set { this.SetValue(TypeProperty, value); }
        }
    }
}
