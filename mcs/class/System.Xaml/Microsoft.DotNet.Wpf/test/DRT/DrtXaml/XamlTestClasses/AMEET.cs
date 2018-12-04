using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.Diagnostics;

namespace Test.Elements
{
    public class MyBinding : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            throw new Exception("ProvideValue shouldn't be called on MyBinding when used on a MySetter.");
        }
    }

    [XamlSetMarkupExtension("ReceiveMarkupExtension")]
    public class MySetter
    {
        public object Value { get; set; }

        public static void ReceiveMarkupExtension(object targetObject, XamlSetMarkupExtensionEventArgs eventArgs)
        {
            if (eventArgs.MarkupExtension.GetType() != typeof(MyBinding))
            {
                throw new Exception("Expected a markupExtension that is of type MyBinding, but got one of type:" + eventArgs.MarkupExtension.GetType());
            }

            MySetter setter = targetObject as MySetter;
            if (setter == null)
            {
                throw new Exception("Expected a MySetter but got one of type:" + targetObject.GetType());
            }

            setter.Value = eventArgs.MarkupExtension;

            eventArgs.Handled = true;
        }
    }
}
