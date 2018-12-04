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
    class ClrAttachedProperty : ClrProperty
    {
        public readonly MethodInfo ClrBindingGetterMethodInfo;
        public readonly MethodInfo ClrBindingSetterMethodInfo;
        private Type _systemTypeOfProperty = null;
        internal ClrAttachedProperty(string name, MethodInfo getter, MethodInfo setter, XamlType declaringType)
            : base(name, declaringType)
        {
            Debug.Assert(getter != null || setter != null);

            if (getter == null && setter == null)
            {
                throw new XamlSchemaException(SR.Get(SRID.SetOnlyProperty, declaringType.Name, name));
            }

            _isPublic = (getter != null) ? getter.IsPublic : setter.IsPublic;
            _isReadOnly = (setter == null);
            _isStatic = false;
            _isAttachable = true;
            _isEvent = false;

            ClrBindingGetterMethodInfo = getter;
            ClrBindingSetterMethodInfo = setter;
        }

        protected override Type LookupSystemTypeOfProperty()
        {
            if (_systemTypeOfProperty == null)
            {
                _systemTypeOfProperty = PrivateLookupSystemTypeOfProperty;
            }
            return _systemTypeOfProperty;
        }

        private Type PrivateLookupSystemTypeOfProperty
        {
            get 
            {
                if (ClrBindingGetterMethodInfo != null)
                {
                    return ClrBindingGetterMethodInfo.ReturnType;
                }
                else
                {
                    ParameterInfo[] pis = ClrBindingSetterMethodInfo.GetParameters();
                    if (pis.Length > 1)
                    {
                        return ClrBindingSetterMethodInfo.GetParameters()[1].ParameterType;
                    }
                    else
                    {
                        throw new XamlSchemaException(SR.Get(SRID.IncorrectSetterParamNum, ClrBindingSetterMethodInfo.Name, 2));
                    }
                }
            }
        }

        protected override object[] LookupCustomAttributes(Type attrType)
        {
            if (ClrBindingGetterMethodInfo != null)
            {
                return ClrBindingGetterMethodInfo.GetCustomAttributes(attrType, true);
            }
            return ClrBindingSetterMethodInfo.GetCustomAttributes(attrType, true);
        }

        protected override XamlType LookupTargetType()
        {
            MethodInfo mi = (ClrBindingGetterMethodInfo != null)
                ? ClrBindingGetterMethodInfo 
                : ClrBindingSetterMethodInfo;
            ParameterInfo[] parameters = mi.GetParameters();
            Type paramType = parameters[0].ParameterType;
            XamlType targetType = SchemaContext.GetXamlType(paramType);
            return targetType;
        }
    }
}
