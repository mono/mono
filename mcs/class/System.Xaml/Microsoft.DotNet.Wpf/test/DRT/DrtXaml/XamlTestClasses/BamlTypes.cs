using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace XamlTestClasses
{
    public class BamlTestType : DependencyObject
    {
        public static readonly DependencyProperty DpWithNoClrPropertyProperty =
            DependencyProperty.Register("DpWithNoClrProperty", typeof(BamlTestType),
              typeof(BamlTestType), new UIPropertyMetadata(null));

        public object TypeMismatch
        {
            get { return GetValue(TypeMismatchProperty); }
            set { SetValue(TypeMismatchProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TypeMismatch.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeMismatchProperty =
            DependencyProperty.Register("TypeMismatch", typeof(int), typeof(BamlTestType), new UIPropertyMetadata(0));


        public static readonly RoutedEvent RoutedEventWithNoClrEventEvent =
            EventManager.RegisterRoutedEvent("RoutedEventWithNoClrEvent", RoutingStrategy.Direct,
            typeof(RoutedEventHandler), typeof(BamlTestType));
    }
}
