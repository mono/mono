using System;
using System.Collections.Generic;
using System.Reflection;

using System.Windows.Markup;

#if SILVERLIGHTXAML
using MS.Internal.Xaml.Schema;
using MS.Internal.Xaml;
#else
using System.Xaml.Schema;
using System.Xaml;
#endif

#if SILVERLIGHTXAML
namespace MS.Internal.Xaml.MS.Impl
#else
namespace System.Xaml.MS.Impl
#endif   
{
    enum XamlImplMemberKind { None, Property, Attachable, Event };

    class XamlMemberInfoKey
    {
        XamlImplMemberKind _kind;

        MemberInfo _memberInfo1;
        MemberInfo _memberInfo2;
        int _hashCode = 0;

        public XamlMemberInfoKey(PropertyInfo pi)
        {
            _kind = XamlImplMemberKind.Property;
            _memberInfo1 = pi;
        }

        public XamlMemberInfoKey(MethodInfo miGetter, MethodInfo miSetter)
        {
            _kind = XamlImplMemberKind.Attachable;
            _memberInfo1 = miGetter;
            _memberInfo2 = miSetter;
        }

        public XamlMemberInfoKey(EventInfo ei)
        {
            _kind = XamlImplMemberKind.Event;
            _memberInfo1 = ei;
        }

        public XamlImplMemberKind Kind { get { return _kind; } }
        public PropertyInfo PropertyInfo { get { return (PropertyInfo)_memberInfo1; } }
        public EventInfo EventInfo { get { return (EventInfo)_memberInfo1; } }
        public MethodInfo GetterMethodInfo { get { return (MethodInfo)_memberInfo1; } }
        public MethodInfo SetterMethodInfo { get { return (MethodInfo)_memberInfo2; } }

        public override int GetHashCode()
        {
            if (_hashCode != 0)
            {
                return _hashCode;
            }
            _hashCode = (_memberInfo1 != null)
                ? HashCode(_memberInfo1)
                : HashCode(_memberInfo2);

            if (_hashCode == 0)
            {
                _hashCode = 1;
            }
            return _hashCode;
        }

        private static int HashCode(MemberInfo mi)
        {
            if (mi == null)
                return 0;
            return mi.Name.GetHashCode() + mi.DeclaringType.Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            XamlMemberInfoKey other = obj as XamlMemberInfoKey;
            if (Object.ReferenceEquals(other, null))
            {
                return false;
            }
            return (this._memberInfo1.DeclaringType == this._memberInfo1.DeclaringType)
                && (this._memberInfo1.Name == other._memberInfo1.Name);
        }

        public static bool operator ==(XamlMemberInfoKey xipi0, XamlMemberInfoKey xipi1)
        {
            return xipi0.Equals(xipi1);
        }

        public static bool operator !=(XamlMemberInfoKey xipi0, XamlMemberInfoKey xipi1)
        {
            return !(xipi0 == xipi1);
        }
    }
}
        
