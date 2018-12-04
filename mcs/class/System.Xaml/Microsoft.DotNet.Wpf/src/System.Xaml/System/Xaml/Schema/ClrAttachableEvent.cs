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
    [DebuggerDisplay("{Name}")]
    class ClrAttachableEvent : ClrProperty
    {
        public readonly MethodInfo AddHandler, RemoveHandler;
        internal ClrAttachableEvent(string name, MethodInfo addHandler, MethodInfo removeHandler, XamlType declaringType) : 
            base(name, declaringType)
        {
            Debug.Assert(addHandler != null);
            Debug.Assert(removeHandler != null);
            
            AddHandler = addHandler;
            RemoveHandler = removeHandler;

            _isPublic = true;
            _isReadOnly = false;
            _isStatic = true;
            _isAttachable = true;
            _isEvent = true;
        }

        protected override Type LookupSystemTypeOfProperty()
        {
            ParameterInfo[] ps = AddHandler.GetParameters();
            Debug.Assert(ps.Length == 2);   // inst, handler
            return ps[1].ParameterType;
        }

        protected override object[] LookupCustomAttributes(Type attrType)
        {
            return AddHandler.GetCustomAttributes(attrType, true);
        }

        protected override XamlTextSyntax LookupTextSyntax()
        {
            return XamlTextSyntax.EventSyntax;
        }
    }
}
