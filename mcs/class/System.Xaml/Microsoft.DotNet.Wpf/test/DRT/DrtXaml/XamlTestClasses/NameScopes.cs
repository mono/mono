using System;
using System.Collections.Generic;
using System.Windows.Markup;

namespace Test.Elements
{
    [NameScopeProperty("TheNameScope")]
    public class ElementWithNSProperty : ElementListHolder
    {
        public INameScope TheNameScope { get; set; }
    }

    [NameScopeProperty("AttachableNameScope", typeof(AttachableNameScope))]
    public class ElementWithAttachedNSProperty : ElementListHolder
    {
    }

    public class AttachableNameScope
    {
        private static Dictionary<object, object> _values;

        static AttachableNameScope()
        {
            _values = new Dictionary<object, object>();
        }

        public static void SetAttachableNameScope(object obj, INameScope value)
        {
            _values.Add(obj, value);
        }

        public static INameScope GetAttachableNameScope(object obj)
        {
            object value=null;
            _values.TryGetValue(obj, out value);
            return (INameScope)value;
        }
    }
}
