//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Internal.PropertyEditing.Editors
{
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Automation;
    using System.Runtime;
    sealed class FlagPanel : StackPanel
    {
        public static readonly DependencyProperty FlagStringProperty =
            DependencyProperty.Register("FlagString", typeof(string), typeof(FlagPanel), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty FlagTypeProperty = DependencyProperty.Register(
            "FlagType",
            typeof(Type),
            typeof(FlagPanel),
            new PropertyMetadata(null, new PropertyChangedCallback(OnFlagTypeChanged)));       

        public Type FlagType
        {
            get { return (Type)GetValue(FlagTypeProperty); }
            set { SetValue(FlagTypeProperty, value); }
        }

        public string FlagString
        {
            get { return (string)GetValue(FlagStringProperty); }
            set { SetValue(FlagStringProperty, value); }
        }

        static void OnFlagTypeChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            Type flagType = args.NewValue as Type;
            Fx.Assert(flagType == null || flagType.IsEnum, "FlagType should be null or enum");
            Fx.Assert(flagType == null || Attribute.IsDefined(flagType, typeof(FlagsAttribute)), "FlagType should be null or have flags attribute");

            if (flagType == null)
            {
                return;
            }

            int index = 0;
            FlagPanel panel = sender as FlagPanel;
            string[] flagNames = flagType.GetEnumNames();
            string zeroValueString = Enum.ToObject(flagType, 0).ToString();
            foreach (string flagName in flagNames)
            {
                if (zeroValueString.Equals("0") || !flagName.Equals(zeroValueString))
                {
                    CheckBox checkBox = new CheckBox();
                    panel.Children.Add(checkBox);
                    checkBox.Content = flagName;
                    checkBox.DataContext = panel;
                    checkBox.SetValue(AutomationProperties.AutomationIdProperty, flagName);
                    Binding binding = new Binding("FlagString");
                    binding.Mode = BindingMode.TwoWay;
                    binding.Converter = new CheckBoxStringConverter(index);
                    binding.ConverterParameter = panel;
                    checkBox.SetBinding(CheckBox.IsCheckedProperty, binding);
                    index++;
                }
            }
        }

        sealed class CheckBoxStringConverter : IValueConverter
        {
            int index;

            public CheckBoxStringConverter(int index)
            {
                this.index = index;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                string str = (value as string).ToUpperInvariant();
                FlagPanel panel = parameter as FlagPanel;
                if (str.Contains((panel.Children[this.index] as CheckBox).Content.ToString().ToUpperInvariant()))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                FlagPanel panel = parameter as FlagPanel;
                string str = string.Empty;
                for (int i = 0; i < panel.Children.Count; i++)
                {
                    if ((i != this.index && ((bool)(panel.Children[i] as CheckBox).IsChecked)) ||
                        (i == this.index && (bool)value))
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            str += ", ";
                        }
                        str += (panel.Children[i] as CheckBox).Content.ToString();
                    }
                }
                if (string.IsNullOrEmpty(str))
                {                    
                    Type flagType = panel.FlagType;
                    Fx.Assert(flagType != null && flagType.IsEnum, "FlagType should be enum");
                    Fx.Assert(Attribute.IsDefined(flagType, typeof(FlagsAttribute)), "FlagType should have flags attribute");

                    return Enum.ToObject(flagType, 0).ToString();                    
                }
                return str;
            }
        }
    }
}
