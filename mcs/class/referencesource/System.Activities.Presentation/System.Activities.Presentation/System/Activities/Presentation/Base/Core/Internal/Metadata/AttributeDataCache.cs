//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.Metadata 
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Activities.Presentation.Metadata;
    using System.Reflection;
    using System.Diagnostics;
    using System.Collections;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    // <summary>
    // Helper class that knows how to look up the base implementation of a given MemberInfo,
    // as well as custom attributes from the MetadataStore or the CLR.  However, it does not
    // actually cache those attributes.  We can add this functionality in the future if needed.
    // On the other hand, this class does cache the map between attribute types and our internal
    // AttributeData data structures that contain AttributeUsageAttributes so we don't have to keep
    // looking them up via reflection.
    // </summary>
    internal static class AttributeDataCache 
    {

        // BindingFlags used for all GetInfo() types of calls
        private static readonly BindingFlags _getInfoBindingFlags =
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic |
            BindingFlags.Public | BindingFlags.Static;

        // Note: we use Hashtables instead of Dictionaries because they are thread safe for
        // read operations without the need for explicit locking.

        // Hashtable of MemberInfos to their base MemberInfos, or null if there is no base MemberInfo
        private static Hashtable _baseMemberMap = new Hashtable();

        // Hashtable of attribute Types to their corresponding AttributeData classes
        private static Hashtable _attributeDataCache = new Hashtable();

        // Indicator for no MemberInfo in _baseMemberMap:  Null value means the base MemberInfo wasn't
        // looked up yet, _noMemberInfo value means it was looked up but it doesn't exist.
        private static object _noMemberInfo = new object();

        // Used for thread safety
        private static object _syncObject = new object();

        // This table gets populated once at initialization, so there is no need for a Hashtable here
        private static Dictionary<MemberTypes, GetBaseMemberCallback> _baseMemberFinders;

        // Static Ctor to populate the lookup table for helper methods that know how
        // to look up the base MemberInfo of a particular MemberType (ctor, method, event, ...)
        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.InitializeReferenceTypeStaticFieldsInline)]
        static AttributeDataCache() 
        {
            _baseMemberFinders = new Dictionary<MemberTypes, GetBaseMemberCallback>();
            _baseMemberFinders[MemberTypes.Constructor] = new GetBaseMemberCallback(GetBaseConstructorInfo);
            _baseMemberFinders[MemberTypes.Method] = new GetBaseMemberCallback(GetBaseMethodInfo);
            _baseMemberFinders[MemberTypes.Property] = new GetBaseMemberCallback(GetBasePropertyInfo);
            _baseMemberFinders[MemberTypes.Event] = new GetBaseMemberCallback(GetBaseEventInfo);
        }

        // <summary>
        // Gets the base MemberInfo for the specified MemberInfo.  For types,
        // the method returns the base type, if any.  For methods, events, and properties
        // the method returns the base method, event, or property, if they exists, null
        // otherwise.
        // </summary>
        // <param name="member">MemberInfo to look up in the base class</param>
        // <returns>Specified MemberInfo in the base class if it exists, null otherwise.</returns>
        internal static MemberInfo GetBaseMemberInfo(MemberInfo member) 
        {
            object baseMember = _baseMemberMap[member];
            if (baseMember == _noMemberInfo)
            {
                return null;
            }

            if (baseMember == null) 
            {
                baseMember = CalculateBaseMemberInfo(member);

                // With Hashtable we only need to lock on writes
                lock (_syncObject) 
                {
                    _baseMemberMap[member] = baseMember ?? _noMemberInfo;
                }
            }

            return (MemberInfo)baseMember;
        }

        // <summary>
        // Looks up the specified MemberInfo in the custom MetadataStore AttributeTables
        // and returns any attributes associated with it as an enumeration.  This method
        // does not return any inherited attributes.
        // </summary>
        // <param name="type">Type to look up</param>
        // <param name="memberName">Member name to look up.  If null, attributes associated
        // with the type itself will be returned.</param>
        // <param name="tables">AttributeTables to look in</param>
        // <returns>Attributes in the AttributeTables associated with the specified
        // Type and member name.</returns>
        internal static IEnumerable<object> GetMetadataStoreAttributes(Type type, string memberName, AttributeTable[] tables) {
            if (tables == null || tables.Length == 0)
            {
                yield break;
            }

            foreach (AttributeTable table in tables) 
            {
                if (table.ContainsAttributes(type)) 
                {
                    IEnumerable attrEnum;
                    if (memberName == null)
                    {
                        attrEnum = table.GetCustomAttributes(type);
                    }
                    else
                    {
                        attrEnum = table.GetCustomAttributes(type, memberName);
                    }

                    foreach (object attr in attrEnum) 
                    {
                        yield return attr;
                    }
                }
            }
        }

        // <summary>
        // Looks up custom attributes for the specified MemberInfo in CLR via reflection
        // and returns them as an enumeration.  This method does not return any
        // inherited attributes.
        // </summary>
        // <param name="member">MemberInfo to look up</param>
        // <returns>Custom Attributes associated with the specified
        // MemberInfo in the CLR.</returns>
        internal static IEnumerable<object> GetClrAttributes(MemberInfo member) 
        {
            object[] attrs = member.GetCustomAttributes(false);
            Fx.Assert(attrs != null, "It looks like GetCustomAttributes() CAN return null.  Protect for it.");
            return attrs;
        }

        // <summary>
        // Gets an existing instance of AttributeData associated with the
        // specified attribute Type, or creates a new one and caches it for
        // later.  AttributeData is used as a cache for AttributeUsageAttributes
        // so don't have to keep using reflection to get them.
        // </summary>
        // <param name="attributeType">Attribute type to look up</param>
        // <returns>Instance of AttributeData associated with the specified
        // attribute type.</returns>
        internal static AttributeData GetAttributeData(Type attributeType) 
        {
            AttributeData attrData = _attributeDataCache[attributeType] as AttributeData;

            if (attrData == null) 
            {
                attrData = new AttributeData(attributeType);

                // With Hashtable we only need to lock on writes
                lock (_syncObject) 
                {
                    _attributeDataCache[attributeType] = attrData;
                }
            }

            return attrData;
        }

        //
        // Tries to get the base MemberInfo associated with the specified
        // member info, if any.
        //
        private static MemberInfo CalculateBaseMemberInfo(MemberInfo member) 
        {
            Fx.Assert(member != null, "member parameter should not be null");

            // Type is a special case that covers the majority of cases
            Type type = member as Type;
            if (type != null)
            {
                return type.BaseType;
            }

            Type targetType = member.DeclaringType.BaseType;

            Fx.Assert(
                _baseMemberFinders.ContainsKey(member.MemberType),
                string.Format(
                CultureInfo.CurrentCulture,
                "Didn't know how to look up the base MemberInfo for member type {0}. " +
                "Please update the list of known GetBaseInfoCallbacks in AttributeDataCache.",
                member.MemberType));

            MemberInfo baseMemberInfo = null;

            while (targetType != null && baseMemberInfo == null) 
            {
                baseMemberInfo = _baseMemberFinders[member.MemberType](member, targetType);
                targetType = targetType.BaseType;
            }

            return baseMemberInfo;
        }

        //
        // Helper method that knows how to look up the base constructor of a class.  However,
        // since constructors can't derive from one another, this method always returns
        // null.
        // 
        private static MemberInfo GetBaseConstructorInfo(MemberInfo info, Type targetType) 
        {
            return null;
        }

        //
        // Helper method that knows how to look up the base implementation of a virtual method.
        //
        private static MemberInfo GetBaseMethodInfo(MemberInfo info, Type targetType) 
        {
            MethodInfo methodInfo = info as MethodInfo;
            Fx.Assert(methodInfo != null, "It looks like MemberType did not match the type of MemberInfo: " + info.GetType().Name);
            return targetType.GetMethod(methodInfo.Name, _getInfoBindingFlags, null, ToTypeArray(methodInfo.GetParameters()), null);
        }

        //
        // Helper method that knows how to look up the base implementation of a virtual property.
        //
        private static MemberInfo GetBasePropertyInfo(MemberInfo info, Type targetType) 
        {
            PropertyInfo propInfo = info as PropertyInfo;
            Fx.Assert(propInfo != null, "It looks like MemberType did not match the type of MemberInfo: " + info.GetType().Name);
            return targetType.GetProperty(propInfo.Name, _getInfoBindingFlags, null, propInfo.PropertyType, ToTypeArray(propInfo.GetIndexParameters()), null);
        }

        //
        // Helper method that knows how to look up the base implementation of a virtual event.
        //
        private static MemberInfo GetBaseEventInfo(MemberInfo info, Type targetType) 
        {
            EventInfo eventInfo = info as EventInfo;
            Fx.Assert(eventInfo != null, "It looks like MemberType did not match the type of MemberInfo: " + info.GetType().Name);
            return targetType.GetEvent(eventInfo.Name, _getInfoBindingFlags);
        }

        //
        // Helper that converts ParamenterInfo[] into Type[]
        //
        private static Type[] ToTypeArray(ParameterInfo[] parameterInfo) {
            if (parameterInfo == null)
            {
                return null;
            }

            Type[] parameterTypes = new Type[parameterInfo.Length];
            for (int i = 0; i < parameterInfo.Length; i++)
            {
                parameterTypes[i] = parameterInfo[i].ParameterType;
            }

            return parameterTypes;
        }

        // Delegate used to call specific methods to get a base MemberInfo from the given MemberInfo
        private delegate MemberInfo GetBaseMemberCallback(MemberInfo member, Type targetType);
    }
}
