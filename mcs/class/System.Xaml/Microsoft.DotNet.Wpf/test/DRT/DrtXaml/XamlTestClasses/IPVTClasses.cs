using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;
using System.Threading;
using System.IO;
using System.Xml;
using System.Reflection;
using System.Xaml;

namespace Test.Elements
{

    public class IPVTContainer
    {
        public string Setter { get; set; }
        public ArrayList List { get; set; }
    }

    public class TargetPropertyHolder
    {
        static Dictionary<object, string> slots = new Dictionary<object, string>();
        static object syncObj = new object();

        public static void SetSetter(object target, string value)
        {
            lock (syncObj)
            {
                string storedValue;
                if (slots.TryGetValue(target, out storedValue))
                {
                    slots[target] = value;
                }
                else
                {
                    slots.Add(target, value);
                }
            }
        }

        public static string GetSetter(object target)
        {
            string storedValue;
            lock (syncObj)
            {
                if (slots.TryGetValue(target, out storedValue))
                {
                    return storedValue;
                }
            }
            throw new InvalidOperationException();
        }
    }

    public class SetterExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget ipvt = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            if (ipvt != null)
            {
                object targetProperty = ipvt.TargetProperty;
                if (targetProperty is MethodInfo)
                {
                    return ((MethodInfo)targetProperty).Name;
                }
                else if (targetProperty is PropertyInfo)
                {
                    return ((PropertyInfo)targetProperty).Name;
                }
            }

            return "null";
        }
    }
}
