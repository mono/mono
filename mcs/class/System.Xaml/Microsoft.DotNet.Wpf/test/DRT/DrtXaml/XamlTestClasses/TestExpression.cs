using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.Xaml;

namespace Test.Elements
{
    public class TestExpression
    {
        public string A { get; set; }
        public string B { get; set; }
    }

    [XamlSetMarkupExtension("ReceiveMarkupExtension")]
    public class DependencyElement: Element
    {
        int _height;
        int _width;

        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public static void ReceiveMarkupExtensionDelegate(object targetObject, XamlSetMarkupExtensionEventArgs eventArgs)
        {
            eventArgs.Handled = true;
        }
    }

    public class ExprMarkupExtension: MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return 42;
        }
    }
}