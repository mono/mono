using System;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Markup;

#if SILVERLIGHTXAML
using MS.Internal.Xaml.MS.Impl;
#else
using System.Xaml.MS.Impl;
#endif

#if SILVERLIGHTXAML
namespace MS.Internal.Xaml.Schema
#else
namespace System.Xaml.Schema
#endif 
{
    class ClrEvent : ClrProperty
    {
        public readonly EventInfo ClrBindingEventInfo;
        internal ClrEvent(string name, EventInfo ei, XamlType declaringType)
            :this(name, ei, declaringType, false)
        {
        }

        internal ClrEvent(string name, EventInfo ei, XamlType declaringType, bool isStatic)
            : base(ei.Name, declaringType)
        {
            Debug.Assert(ei != null);
            Debug.Assert(ei.Name == name);

            MethodInfo mi = ei.GetAddMethod(true);

            if (mi == null)
            {
                throw new XamlSchemaException(SR.Get(SRID.SetOnlyProperty, declaringType.Name, name));
            }

            _isPublic = mi.IsPublic;
            _isReadOnly = false;
            _isStatic = isStatic;
            _isAttachable = false;
            _isEvent = true;

            ClrBindingEventInfo = ei;
        }

        protected override XamlTextSyntax LookupTextSyntax()
        {
            return XamlTextSyntax.EventSyntax;
        }

        protected override Type LookupSystemTypeOfProperty()
        {
            return ClrBindingEventInfo.EventHandlerType;
        }

        protected override object[] LookupCustomAttributes(Type attrType)
        {
            return ClrBindingEventInfo.GetCustomAttributes(attrType, true);
        }
    }
}
