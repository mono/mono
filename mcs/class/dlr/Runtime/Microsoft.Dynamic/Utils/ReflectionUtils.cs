/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the  Apache License, Version 2.0, please send an email to 
 * dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 *
 * ***************************************************************************/

#if FEATURE_METADATA_READER
using Microsoft.Scripting.Metadata;
#endif

#if !WIN8
using TypeInfo = System.Type;
#endif

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
#if FEATURE_REFEMIT
using System.Reflection.Emit;
#endif
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Runtime.InteropServices;
using System.Dynamic;
using System.Linq.Expressions;

using Microsoft.Scripting.Generation;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

#if WIN8 || WP75
namespace System.Runtime.CompilerServices {
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event)]
    public sealed class SpecialNameAttribute : Attribute {
        public SpecialNameAttribute() {
        }
    }
}
#endif

#if WIN8
namespace System {
    public enum TypeCode {
        Empty,
        Object,
        DBNull,
        Boolean,
        Char,
        SByte,
        Byte,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,
        Single,
        Double,
        Decimal,
        DateTime,
        String = 18
    }
}

namespace System.Reflection {
    [Flags]
    public enum BindingFlags {
        /// <summary>Specifies that instance members are to be included in the search.</summary>
        Instance = 4,
        /// <summary>Specifies that static members are to be included in the search.</summary>
        Static = 8,
        /// <summary>Specifies that public members are to be included in the search.</summary>
        Public = 16,
        /// <summary>Specifies that non-public members are to be included in the search.</summary>
        NonPublic = 32
    }
}
#elif !CLR45
namespace System.Reflection {
    public static class RuntimeReflectionExtensions {
        public static MethodInfo GetRuntimeBaseDefinition(this MethodInfo method) {
            return method.GetBaseDefinition();
        }

        public static IEnumerable<MethodInfo> GetRuntimeMethods(this Type type) {
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}
#endif

namespace Microsoft.Scripting.Utils {
    // CF doesn't support DefaultParameterValue attribute. Define our own, but not in System.Runtime.InteropServices namespace as that would 
    // make C# compiler emit the parameter's default value metadata not the attribute itself. The default value metadata are not accessible on CF.
#if !FEATURE_DEFAULT_PARAMETER_VALUE
    /// <summary>
    /// The Default Parameter Value Attribute.
    /// </summary>
    public sealed class DefaultParameterValueAttribute : Attribute
    {
        private readonly object _value;

        public object Value
        {
            get { return _value; }
        }

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="value">The value.</param>
        public DefaultParameterValueAttribute(object value)
        {
            _value = value;
        }
    }

#if !ANDROID
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false), ComVisible(true)]
    public sealed class OptionalAttribute : Attribute {
    }
#endif
#endif

    public static class ReflectionUtils {
        #region Accessibility

        public const BindingFlags AllMembers = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static bool IsPublic(this PropertyInfo property) {
            return property.GetGetMethod(nonPublic: false) != null
                || property.GetSetMethod(nonPublic: false) != null;
        }

        public static bool IsStatic(this PropertyInfo property) {
            var getter = property.GetGetMethod(nonPublic: true);
            var setter = property.GetSetMethod(nonPublic: true);

            return getter != null && getter.IsStatic
                || setter != null && setter.IsStatic;
        }

        public static bool IsStatic(this EventInfo evnt) {
            var add = evnt.GetAddMethod(nonPublic: true);
            var remove = evnt.GetRemoveMethod(nonPublic: true);

            return add != null && add.IsStatic
                || remove != null && remove.IsStatic;
        }

        public static bool IsPrivate(this PropertyInfo property) {
            var getter = property.GetGetMethod(nonPublic: true);
            var setter = property.GetSetMethod(nonPublic: true);

            return (getter == null || getter.IsPrivate)
                && (setter == null || setter.IsPrivate);
        }

        public static bool IsPrivate(this EventInfo evnt) {
            var add = evnt.GetAddMethod(nonPublic: true);
            var remove = evnt.GetRemoveMethod(nonPublic: true);

            return (add == null || add.IsPrivate)
                && (remove == null || remove.IsPrivate);
        }

        private static bool MatchesFlags(ConstructorInfo member, BindingFlags flags) {
            return
                ((member.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic) & flags) != 0 &&
                ((member.IsStatic ? BindingFlags.Static : BindingFlags.Instance) & flags) != 0;
        }

        private static bool MatchesFlags(MethodInfo member, BindingFlags flags) {
            return
                ((member.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic) & flags) != 0 && 
                ((member.IsStatic ? BindingFlags.Static : BindingFlags.Instance) & flags) != 0;
        }

        private static bool MatchesFlags(FieldInfo member, BindingFlags flags) {
            return
                ((member.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic) & flags) != 0 &&
                ((member.IsStatic ? BindingFlags.Static : BindingFlags.Instance) & flags) != 0;
        }

        private static bool MatchesFlags(PropertyInfo member, BindingFlags flags) {
            return
                ((member.IsPublic() ? BindingFlags.Public : BindingFlags.NonPublic) & flags) != 0 &&
                ((member.IsStatic() ? BindingFlags.Static : BindingFlags.Instance) & flags) != 0;
        }

        private static bool MatchesFlags(EventInfo member, BindingFlags flags) {
            var add = member.GetAddMethod();
            var remove = member.GetRemoveMethod();
            var raise = member.GetRaiseMethod();

            bool isPublic = add != null && add.IsPublic || remove != null && remove.IsPublic || raise != null && raise.IsPublic;
            bool isStatic = add != null && add.IsStatic || remove != null && remove.IsStatic || raise != null && raise.IsStatic;

            return
                ((isPublic ? BindingFlags.Public : BindingFlags.NonPublic) & flags) != 0 &&
                ((isStatic ? BindingFlags.Static : BindingFlags.Instance) & flags) != 0;
        }

        private static bool MatchesFlags(TypeInfo member, BindingFlags flags) {
            // Static/Instance are ignored
            return (((member.IsPublic || member.IsNestedPublic) ? BindingFlags.Public : BindingFlags.NonPublic) & flags) != 0;
        }

        private static bool MatchesFlags(MemberInfo member, BindingFlags flags) {
            ConstructorInfo ctor;
            MethodInfo method;
            FieldInfo field;
            EventInfo evnt;
            PropertyInfo property;

            if ((method = member as MethodInfo) != null) {
                return MatchesFlags(method, flags);
            }

            if ((field = member as FieldInfo) != null) {
                return MatchesFlags(field, flags);
            }

            if ((ctor = member as ConstructorInfo) != null) {
                return MatchesFlags(ctor, flags);
            }

            if ((evnt = member as EventInfo) != null) {
                return MatchesFlags(evnt, flags);
            }

            if ((property = member as PropertyInfo) != null) {
                return MatchesFlags(property, flags);
            }

            return MatchesFlags((TypeInfo)member, flags);
        }

        private static IEnumerable<T> WithBindingFlags<T>(this IEnumerable<T> members, Func<T, BindingFlags, bool> matchFlags, BindingFlags flags)
            where T : MemberInfo {
            return members.Where(member => matchFlags(member, flags));
        }

        public static IEnumerable<MemberInfo> WithBindingFlags(this IEnumerable<MemberInfo> members, BindingFlags flags) {
            return members.WithBindingFlags(MatchesFlags, flags);
        }

        public static IEnumerable<MethodInfo> WithBindingFlags(this IEnumerable<MethodInfo> members, BindingFlags flags) {
            return members.WithBindingFlags(MatchesFlags, flags);
        }

        public static IEnumerable<ConstructorInfo> WithBindingFlags(this IEnumerable<ConstructorInfo> members, BindingFlags flags) {
            return members.WithBindingFlags(MatchesFlags, flags);
        }

        public static IEnumerable<FieldInfo> WithBindingFlags(this IEnumerable<FieldInfo> members, BindingFlags flags) {
            return members.WithBindingFlags(MatchesFlags, flags);
        }

        public static IEnumerable<PropertyInfo> WithBindingFlags(this IEnumerable<PropertyInfo> members, BindingFlags flags) {
            return members.WithBindingFlags(MatchesFlags, flags);
        }

        public static IEnumerable<EventInfo> WithBindingFlags(this IEnumerable<EventInfo> members, BindingFlags flags) {
            return members.WithBindingFlags(MatchesFlags, flags);
        }

        public static IEnumerable<TypeInfo> WithBindingFlags(this IEnumerable<TypeInfo> members, BindingFlags flags) {
            return members.WithBindingFlags(MatchesFlags, flags);
        }

        public static MemberInfo WithBindingFlags(this MemberInfo member, BindingFlags flags) {
            return member != null && MatchesFlags(member, flags) ? member : null;
        }

        public static MethodInfo WithBindingFlags(this MethodInfo member, BindingFlags flags) {
            return member != null && MatchesFlags(member, flags) ? member : null;
        }

        public static ConstructorInfo WithBindingFlags(this ConstructorInfo member, BindingFlags flags) {
            return member != null && MatchesFlags(member, flags) ? member : null;
        }

        public static FieldInfo WithBindingFlags(this FieldInfo member, BindingFlags flags) {
            return member != null && MatchesFlags(member, flags) ? member : null;
        }

        public static PropertyInfo WithBindingFlags(this PropertyInfo member, BindingFlags flags) {
            return member != null && MatchesFlags(member, flags) ? member : null;
        }

        public static EventInfo WithBindingFlags(this EventInfo member, BindingFlags flags) {
            return member != null && MatchesFlags(member, flags) ? member : null;
        }

        public static TypeInfo WithBindingFlags(this TypeInfo member, BindingFlags flags) {
            return member != null && MatchesFlags(member, flags) ? member : null;
        }

        #endregion

        #region Signatures

        public static IEnumerable<MethodInfo> WithSignature(this IEnumerable<MethodInfo> members, Type[] parameterTypes) {
            return members.Where(c => {
                var ps = c.GetParameters();
                if (ps.Length != parameterTypes.Length) {
                    return false;
                }

                for (int i = 0; i < ps.Length; i++) {
                    if (parameterTypes[i] != ps[i].ParameterType) {
                        return false;
                    }
                }

                return true;
            });
        }

        public static IEnumerable<ConstructorInfo> WithSignature(this IEnumerable<ConstructorInfo> members, Type[] parameterTypes) {
            return members.Where(c => {
                var ps = c.GetParameters();
                if (ps.Length != parameterTypes.Length) {
                    return false;
                }

                for (int i = 0; i < ps.Length; i++) {
                    if (parameterTypes[i] != ps[i].ParameterType) {
                        return false;
                    }
                }

                return true;
            });
        }
        
        #endregion

        #region Member Inheritance

        // CLI specification, partition I, 8.10.4: Hiding, overriding, and layout
        // ----------------------------------------------------------------------
        // While hiding applies to all members of a type, overriding deals with object layout and is applicable only to instance fields 
        // and virtual methods. The CTS provides two forms of member overriding, new slot and expect existing slot. A member of a derived 
        // type that is marked as a new slot will always get a new slot in the object’s layout, guaranteeing that the base field or method 
        // is available in the object by using a qualified reference that combines the name of the base type with the name of the member 
        // and its type or signature. A member of a derived type that is marked as expect existing slot will re-use (i.e., share or override) 
        // a slot that corresponds to a member of the same kind (field or method), name, and type if one already exists from the base type; 
        // if no such slot exists, a new slot is allocated and used.
        //
        // The general algorithm that is used for determining the names in a type and the layout of objects of the type is roughly as follows:
        // - Flatten the inherited names (using the hide by name or hide by name-and-signature rule) ignoring accessibility rules. 
        // - For each new member that is marked “expect existing slot”, look to see if an exact match on kind (i.e., field or method), 
        //   name, and signature exists and use that slot if it is found, otherwise allocate a new slot. 
        // - After doing this for all new members, add these new member-kind/name/signatures to the list of members of this type 
        // - Finally, remove any inherited names that match the new members based on the hide by name or hide by name-and-signature rules.
        
        // NOTE: Following GetXxx only implement overriding, not hiding specified by hide-by-name or hide-by-name-and-signature flags.

        public static IEnumerable<MethodInfo> GetInheritedMethods(this Type type, string name = null, bool flattenHierarchy = false) {
            while (type.IsGenericParameter) {
                type = type.GetBaseType();
            }

            var baseDefinitions = new HashSet<MethodInfo>(ReferenceEqualityComparer<MethodInfo>.Instance);
            foreach (var ancestor in type.Ancestors()) {
                foreach (var declaredMethod in ancestor.GetDeclaredMethods(name)) {
                    if (declaredMethod != null && IncludeMethod(declaredMethod, type, baseDefinitions, flattenHierarchy)) {
                        yield return declaredMethod;
                    }
                }
            }
        }

        private static bool IncludeMethod(MethodInfo member, Type reflectedType, HashSet<MethodInfo> baseDefinitions, bool flattenHierarchy) {
            if (member.IsVirtual) {
                if (baseDefinitions.Add(RuntimeReflectionExtensions.GetRuntimeBaseDefinition(member))) {
                    return true;
                }
            } else if (member.DeclaringType == reflectedType) {
                return true;
            } else if (!member.IsPrivate && (!member.IsStatic || flattenHierarchy)) {
                return true;
            }

            return false;
        }

        public static IEnumerable<PropertyInfo> GetInheritedProperties(this Type type, string name = null, bool flattenHierarchy = false) {
            while (type.IsGenericParameter) {
                type = type.GetBaseType();
            }

            var baseDefinitions = new HashSet<MethodInfo>(ReferenceEqualityComparer<MethodInfo>.Instance);
            foreach (var ancestor in type.Ancestors()) {
                if (name != null) {
                    var declaredProperty = ancestor.GetDeclaredProperty(name);
                    if (declaredProperty != null && IncludeProperty(declaredProperty, type, baseDefinitions, flattenHierarchy)) {
                        yield return declaredProperty;
                    }
                } else {
                    foreach (var declaredProperty in ancestor.GetDeclaredProperties()) {
                        if (IncludeProperty(declaredProperty, type, baseDefinitions, flattenHierarchy)) {
                            yield return declaredProperty;
                        }
                    }
                }
            }
        }

        // CLI spec 22.34 Properties
        // -------------------------
        // [Note: The CLS (see Partition I) refers to instance, virtual, and static properties.  
        // The signature of a property (from the Type column) can be used to distinguish a static property, 
        // since instance and virtual properties will have the “HASTHIS” bit set in the signature (§23.2.1)
        // while a static property will not.  The distinction between an instance and a virtual property 
        // depends on the signature of the getter and setter methods, which the CLS requires to be either 
        // both virtual or both instance. end note]
        private static bool IncludeProperty(PropertyInfo member, Type reflectedType, HashSet<MethodInfo> baseDefinitions, bool flattenHierarchy) {
            var getter = member.GetGetMethod(nonPublic: true);
            var setter = member.GetSetMethod(nonPublic: true);

            MethodInfo virtualAccessor;
            if (getter != null && getter.IsVirtual) {
                virtualAccessor = getter;
            } else if (setter != null && setter.IsVirtual) {
                virtualAccessor = setter;
            } else {
                virtualAccessor = null;
            }

            if (virtualAccessor != null) {
                if (baseDefinitions.Add(RuntimeReflectionExtensions.GetRuntimeBaseDefinition(virtualAccessor))) {
                    return true;
                }
            } else if (member.DeclaringType == reflectedType) {
                return true;
            } else if (!member.IsPrivate() && (!member.IsStatic() || flattenHierarchy)) {
                return true;
            }

            return false;
        }

        public static IEnumerable<EventInfo> GetInheritedEvents(this Type type, string name = null, bool flattenHierarchy = false) {
            while (type.IsGenericParameter) {
                type = type.GetBaseType();
            }

            var baseDefinitions = new HashSet<MethodInfo>(ReferenceEqualityComparer<MethodInfo>.Instance);
            foreach (var ancestor in type.Ancestors()) {
                if (name != null) {
                    var declaredEvent = ancestor.GetDeclaredEvent(name);
                    if (declaredEvent != null && IncludeEvent(declaredEvent, type, baseDefinitions, flattenHierarchy)) {
                        yield return declaredEvent;
                    }
                } else {
                    foreach (var declaredEvent in ancestor.GetDeclaredEvents()) {
                        if (IncludeEvent(declaredEvent, type, baseDefinitions, flattenHierarchy)) {
                            yield return declaredEvent;
                        }
                    }
                }
            }
        }

        private static bool IncludeEvent(EventInfo member, Type reflectedType, HashSet<MethodInfo> baseDefinitions, bool flattenHierarchy) {
            var add = member.GetAddMethod(nonPublic: true);
            var remove = member.GetRemoveMethod(nonPublic: true);

            // TOOD: fire method?

            MethodInfo virtualAccessor;
            if (add != null && add.IsVirtual) {
                virtualAccessor = add;
            } else if (remove != null && remove.IsVirtual) {
                virtualAccessor = remove;
            } else {
                virtualAccessor = null;
            }

            if (virtualAccessor != null) {
                if (baseDefinitions.Add(RuntimeReflectionExtensions.GetRuntimeBaseDefinition(virtualAccessor))) {
                    return true;
                }
            } else if (member.DeclaringType == reflectedType) {
                return true;
            } else if (!member.IsPrivate() && (!member.IsStatic() || flattenHierarchy)) {
                return true;
            }

            return false;
        }

        public static IEnumerable<FieldInfo> GetInheritedFields(this Type type, string name = null, bool flattenHierarchy = false) {
            while (type.IsGenericParameter) {
                type = type.GetBaseType();
            }

            foreach (var ancestor in type.Ancestors()) {
                if (name != null) {
                    var declaredField = ancestor.GetDeclaredField(name);
                    if (declaredField != null && IncludeField(declaredField, type, flattenHierarchy)) {
                        yield return declaredField;
                    }
                } else {
                    foreach (var declaredField in ancestor.GetDeclaredFields()) {
                        if (IncludeField(declaredField, type, flattenHierarchy)) {
                            yield return declaredField;
                        }
                    }
                }
            }
        }
        
        private static bool IncludeField(FieldInfo member, Type reflectedType, bool flattenHierarchy) {
            if (member.DeclaringType == reflectedType) {
                return true;
            } else if (!member.IsPrivate && (!member.IsStatic || flattenHierarchy)) {
                return true;
            }

            return false;
        }

        public static IEnumerable<MemberInfo> GetInheritedMembers(this Type type, string name = null, bool flattenHierarchy = false) {
            var result =
                type.GetInheritedMethods(name, flattenHierarchy).Cast<MethodInfo, MemberInfo>().Concat(
                type.GetInheritedProperties(name, flattenHierarchy).Cast<PropertyInfo, MemberInfo>().Concat(
                type.GetInheritedEvents(name, flattenHierarchy).Cast<EventInfo, MemberInfo>().Concat(
                type.GetInheritedFields(name, flattenHierarchy).Cast<FieldInfo, MemberInfo>())));

            if (name == null) {
                return result.Concat<MemberInfo>(
                    type.GetDeclaredConstructors().Cast<ConstructorInfo, MemberInfo>().Concat(
                    type.GetDeclaredNestedTypes().Cast<TypeInfo, MemberInfo>()));
            }

            var nestedType = type.GetDeclaredNestedType(name);
            return (nestedType != null) ? result.Concat(new[] { nestedType }) : result;
        }

        #endregion

        #region Declared Members

        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type type) {
#if WIN8
            return type.GetTypeInfo().DeclaredConstructors;
#else
            return type.GetConstructors(BindingFlags.DeclaredOnly | AllMembers);
#endif
        }

#if WIN8
        public static ConstructorInfo GetConstructor(this Type type, Type[] parameterTypes) {
            return type.GetDeclaredConstructors().Where(ci => !ci.IsStatic && ci.IsPublic).WithSignature(parameterTypes).SingleOrDefault();
        }
#endif

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type type, string name = null) {
#if WIN8
            if (name == null) {
                return type.GetTypeInfo().DeclaredMethods;
            } else {
                return type.GetTypeInfo().GetDeclaredMethods(name);
            }
#else
            if (name == null) {
                return type.GetMethods(BindingFlags.DeclaredOnly | AllMembers);
            } else {
                return type.GetMember(name, MemberTypes.Method, BindingFlags.DeclaredOnly | AllMembers).OfType<MethodInfo>();
            }
#endif
        }

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type type) {
#if WIN8
            return type.GetTypeInfo().DeclaredProperties;
#else
            return type.GetProperties(BindingFlags.DeclaredOnly | AllMembers);
#endif
        }

        public static PropertyInfo GetDeclaredProperty(this Type type, string name) {
            Debug.Assert(name != null);
#if WIN8
            return type.GetTypeInfo().GetDeclaredProperty(name);
#else
            return type.GetProperty(name, BindingFlags.DeclaredOnly | AllMembers);
#endif
        }

        public static IEnumerable<EventInfo> GetDeclaredEvents(this Type type) {
#if WIN8
            return type.GetTypeInfo().DeclaredEvents;
#else
            return type.GetEvents(BindingFlags.DeclaredOnly | AllMembers);
#endif
        }

        public static EventInfo GetDeclaredEvent(this Type type, string name) {
            Debug.Assert(name != null);
#if WIN8
            return type.GetTypeInfo().GetDeclaredEvent(name);
#else
            return type.GetEvent(name, BindingFlags.DeclaredOnly | AllMembers);
#endif
        }

        public static IEnumerable<FieldInfo> GetDeclaredFields(this Type type) {
#if WIN8
            return type.GetTypeInfo().DeclaredFields;
#else
            return type.GetFields(BindingFlags.DeclaredOnly | AllMembers);
#endif
        }

        public static FieldInfo GetDeclaredField(this Type type, string name) {
            Debug.Assert(name != null);
#if WIN8
            return type.GetTypeInfo().GetDeclaredField(name);
#else
            return type.GetField(name, BindingFlags.DeclaredOnly | AllMembers);
#endif
        }

        public static IEnumerable<TypeInfo> GetDeclaredNestedTypes(this Type type) {
#if WIN8
            return type.GetTypeInfo().DeclaredNestedTypes;
#else
            return type.GetNestedTypes(BindingFlags.DeclaredOnly | AllMembers);
#endif
        }

        public static TypeInfo GetDeclaredNestedType(this Type type, string name) {
            Debug.Assert(name != null);
#if WIN8
            return type.GetTypeInfo().GetDeclaredNestedType(name);
#else
            return type.GetNestedType(name, BindingFlags.DeclaredOnly | AllMembers);
#endif
        }

        public static IEnumerable<MemberInfo> GetDeclaredMembers(this Type type, string name = null) {
#if WIN8
            var info = type.GetTypeInfo();
            if (name == null) {
                return info.DeclaredMembers;
            } else {
                return GetDeclaredMembersWithName(info, name);
            }
#else
            if (name == null) {
                return type.GetMembers(BindingFlags.DeclaredOnly | AllMembers);
            } else {
                return type.GetMember(name, BindingFlags.DeclaredOnly | AllMembers);
            }
#endif
        }

#if WIN8
        private static IEnumerable<MemberInfo> GetDeclaredMembersWithName(TypeInfo info, string name) {
            MemberInfo member;

            if ((member = info.GetDeclaredMethod(name)) != null) {
                yield return member;
            }

            if ((member = info.GetDeclaredField(name)) != null) {
                yield return member;
            }

            if ((member = info.GetDeclaredProperty(name)) != null) {
                yield return member;
            }

            if ((member = info.GetDeclaredEvent(name)) != null) {
                yield return member;
            }

            if ((member = info.GetDeclaredNestedType(name)) != null) {
                yield return member;
            }
        }
#endif

        #endregion

        #region Win8
#if WIN8 || CLR45
        public static TypeCode GetTypeCode(this Enum e) {
            return GetTypeCode(Enum.GetUnderlyingType(e.GetType()));
        }

        // TODO: reduce to numeric types?
        public static TypeCode GetTypeCode(this Type type) {
            if (type == typeof(int)) {
                return TypeCode.Int32;
            }
            if (type == typeof(sbyte)) {
                return TypeCode.SByte;
            }
            if (type == typeof(short)) {
                return TypeCode.Int16;
            }
            if (type == typeof(long)) {
                return TypeCode.Int64;
            }
            if (type == typeof(uint)) {
                return TypeCode.UInt32;
            }
            if (type == typeof(byte)) {
                return TypeCode.Byte;
            }
            if (type == typeof(ushort)) {
                return TypeCode.UInt16;
            }
            if (type == typeof(ulong)) {
                return TypeCode.UInt64;
            }
            if (type == typeof(bool)) {
                return TypeCode.Boolean;
            }
            if (type == typeof(char)) {
                return TypeCode.Char;
            }

            // TODO: do we need this?
            if (type == typeof(string)) {
                return TypeCode.String;
            }
            if (type == typeof(bool)) {
                return TypeCode.Boolean;
            }
            if (type == typeof(double)) {
                return TypeCode.Double;
            }
            if (type == typeof(float)) {
                return TypeCode.Single;
            }
            if (type == typeof(decimal)) {
                return TypeCode.Decimal;
            }
            if (type == typeof(DateTime)) {
                return TypeCode.DateTime;
            }
            return TypeCode.Object;
        }

        public static IEnumerable<Type> GetImplementedInterfaces(this Type type) {
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        public static MethodInfo GetGetMethod(this PropertyInfo propertyInfo, bool nonPublic = false) {
            var accessor = propertyInfo.GetMethod;
            return nonPublic || accessor == null || accessor.IsPublic ? accessor : null;
        }

        public static MethodInfo GetSetMethod(this PropertyInfo propertyInfo, bool nonPublic = false) {
            var accessor = propertyInfo.SetMethod;
            return nonPublic || accessor == null || accessor.IsPublic ? accessor : null;
        }

        public static MethodInfo GetAddMethod(this EventInfo eventInfo, bool nonPublic = false) {
            var accessor = eventInfo.AddMethod;
            return nonPublic || accessor == null || accessor.IsPublic ? accessor : null;
        }

        public static MethodInfo GetRemoveMethod(this EventInfo eventInfo, bool nonPublic = false) {
            var accessor = eventInfo.RemoveMethod;
            return nonPublic || accessor == null || accessor.IsPublic ? accessor : null;
        }

        public static MethodInfo GetRaiseMethod(this EventInfo eventInfo, bool nonPublic = false) {
            var accessor = eventInfo.RaiseMethod;
            return nonPublic || accessor == null || accessor.IsPublic ? accessor : null;
        }

        public static MethodInfo GetMethod(this Type type, string name) {
            return type.GetTypeInfo().GetDeclaredMethod(name);
        }

        // TODO: FlattenHierarchy
        // TODO: inherited!
        public static MethodInfo GetMethod(this Type type, string name, Type[] parameterTypes) {
            return type.GetTypeInfo().GetDeclaredMethods(name).WithSignature(parameterTypes).Single();
        }

        public static MethodInfo GetMethod(this Type type, string name, BindingFlags bindingFlags) {
            return type.GetMethods(name, bindingFlags).Single();
        }

        private static IEnumerable<MethodInfo> GetMethods(this Type type, string name, BindingFlags bindingFlags) {
            return type.GetTypeInfo().GetDeclaredMethods(name).WithBindingFlags(bindingFlags);
        }

        public static MethodInfo GetMethod(this Delegate d) {
            return d.GetMethodInfo();
        }

        // TODO: Callers should distinguish parameters from arguments. Stop using this method.
        public static Type[] GetGenericArguments(this Type type) {
            var info = type.GetTypeInfo();
            return info.IsGenericTypeDefinition ? info.GenericTypeParameters : info.GenericTypeArguments;
        }

        public static Type[] GetGenericTypeArguments(this Type type) {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static Type[] GetGenericTypeParameters(this Type type) {
            return type.GetTypeInfo().GenericTypeParameters;
        }

        public static bool IsAssignableFrom(this Type type, Type other) {
            return type.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
        }

        public static Type[] GetGenericParameterConstraints(this Type type) {
            return type.GetTypeInfo().GetGenericParameterConstraints();
        }

        public static bool IsSubclassOf(this Type type, Type other) {
            return type.GetTypeInfo().IsSubclassOf(other);
        }

        public static IEnumerable<Type> GetInterfaces(this Type type) {
            return type.GetTypeInfo().ImplementedInterfaces;
        }

        public static Type[] GetRequiredCustomModifiers(this ParameterInfo parameter) {
            return EmptyTypes;
        }

        public static Type[] GetOptionalCustomModifiers(this ParameterInfo parameter) {
            return EmptyTypes;
        }

        public static IEnumerable<Module> GetModules(this Assembly assembly) {
            return assembly.Modules;
        }

        private static string GetDefaultMemberName(this Type type) {
            foreach (var ancestor in type.Ancestors()) {
                var attr = ancestor.GetTypeInfo().GetCustomAttributes<DefaultMemberAttribute>().SingleOrDefault();
                if (attr != null) {
                    return attr.MemberName;
                }
            }

            return null;
        }

        public static IEnumerable<MemberInfo> GetDefaultMembers(this Type type) {
            string defaultMemberName = type.GetDefaultMemberName();
            if (defaultMemberName != null) {
                return type.GetInheritedMembers(defaultMemberName).WithBindingFlags(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);
            }

            return Enumerable.Empty<MemberInfo>();
        }
#else
        public static Type[] GetGenericTypeArguments(this Type type) {
            return type.IsGenericType && !type.IsGenericTypeDefinition ? type.GetTypeInfo().GetGenericArguments() : null;
        }

        public static Type[] GetGenericTypeParameters(this Type type) {
            return type.IsGenericTypeDefinition ? type.GetTypeInfo().GetGenericArguments() : null;
        }

        public static IEnumerable<Module> GetModules(this Assembly assembly) {
            return assembly.GetModules();
        }

        public static IEnumerable<Type> GetImplementedInterfaces(this Type type) {
            return type.GetInterfaces();
        }

        public static TypeCode GetTypeCode(this Type type) {
            return Type.GetTypeCode(type);
        }

        public static MethodInfo GetMethodInfo(this Delegate d) {
            return d.Method;
        }

        public static bool IsDefined(this Assembly assembly, Type attributeType) {
            return assembly.IsDefined(attributeType, false);
        }

        public static T GetCustomAttribute<T>(this Assembly assembly, bool inherit = false) where T : Attribute {
            return (T)Attribute.GetCustomAttribute(assembly, typeof(T), inherit);
        }

        public static T GetCustomAttribute<T>(this MemberInfo member, bool inherit = false) where T : Attribute {
            return (T)Attribute.GetCustomAttribute(member, typeof(T), inherit);
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this Assembly assembly, bool inherit = false) where T : Attribute {
            return Attribute.GetCustomAttributes(assembly, typeof(T), inherit).Cast<T>();
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo member, bool inherit = false) where T : Attribute {
            return Attribute.GetCustomAttributes(member, typeof(T), inherit).Cast<T>();
        }
#endif

        public static bool ContainsGenericParameters(this Type type) {
            return type.GetTypeInfo().ContainsGenericParameters;
        }

        public static bool IsInterface(this Type type) {
            return type.GetTypeInfo().IsInterface;
        }

        public static bool IsClass(this Type type) {
            return type.GetTypeInfo().IsClass;
        }

        public static bool IsGenericType(this Type type) {
            return type.GetTypeInfo().IsGenericType;
        }

        public static bool IsGenericTypeDefinition(this Type type) {
            return type.GetTypeInfo().IsGenericTypeDefinition;
        }

        public static bool IsSealed(this Type type) {
            return type.GetTypeInfo().IsSealed;
        }

        public static bool IsAbstract(this Type type) {
            return type.GetTypeInfo().IsAbstract;
        }

        public static bool IsPublic(this Type type) {
            return type.GetTypeInfo().IsPublic;
        }

        public static bool IsVisible(this Type type) {
            return type.GetTypeInfo().IsVisible;
        }
        
        public static Type GetBaseType(this Type type) {
            return type.GetTypeInfo().BaseType;
        }

        public static bool IsValueType(this Type type) {
            return type.GetTypeInfo().IsValueType;
        }

        public static bool IsEnum(this Type type) {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsPrimitive(this Type type) {
            return type.GetTypeInfo().IsPrimitive;
        }

        public static GenericParameterAttributes GetGenericParameterAttributes(this Type type) {
            return type.GetTypeInfo().GenericParameterAttributes;
        }
        
        public static Type[] EmptyTypes = new Type[0];

        public static object GetRawConstantValue(this FieldInfo field) {
            if (!field.IsLiteral) {
                throw new ArgumentException(field + " not a literal.");
            }

            object value = field.GetValue(null);
            return field.FieldType.IsEnum() ? UnwrapEnumValue(value) : value;
        }

        /// <summary>
        /// Converts a boxed enum value to the underlying integer value.
        /// </summary>
        public static object UnwrapEnumValue(object value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            switch (value.GetType().GetTypeCode()) {
                case TypeCode.Byte:
                    return System.Convert.ToByte(value);

                case TypeCode.Int16:
                    return System.Convert.ToInt16(value);

                case TypeCode.Int32:
                    return System.Convert.ToInt32(value);

                case TypeCode.Int64:
                    return System.Convert.ToInt64(value);

                case TypeCode.SByte:
                    return System.Convert.ToSByte(value);

                case TypeCode.UInt16:
                    return System.Convert.ToUInt16(value);

                case TypeCode.UInt32:
                    return System.Convert.ToUInt32(value);

                case TypeCode.UInt64:
                    return System.Convert.ToUInt64(value);

                default: 
                    throw new ArgumentException("Value must be a boxed enum.", "value");
            }
        }

        #endregion

#if FEATURE_REFEMIT
#if FEATURE_ASSEMBLYBUILDER_DEFINEDYNAMICASSEMBLY
        public static AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access) {
            return AssemblyBuilder.DefineDynamicAssembly(name, access);
        }
#else
        public static AssemblyBuilder DefineDynamicAssembly(AssemblyName name, AssemblyBuilderAccess access) {
            return AppDomain.CurrentDomain.DefineDynamicAssembly(name, access);
        }
#endif
#if !FEATURE_PDBEMIT
        public static ModuleBuilder DefineDynamicModule(this AssemblyBuilder assembly, string name, bool emitDebugInfo) {
            // ignore the flag
            return assembly.DefineDynamicModule(name);
        }
#endif
#endif

        #region Signature and Type Formatting

        // Generic type names have the arity (number of generic type paramters) appended at the end. 
        // For eg. the mangled name of System.List<T> is "List`1". This mangling is done to enable multiple 
        // generic types to exist as long as they have different arities.
        public const char GenericArityDelimiter = '`';

#if !WIN8
        public static StringBuilder FormatSignature(StringBuilder result, MethodBase method) {
            return FormatSignature(result, method, (t) => t.FullName);
        }

        public static StringBuilder FormatSignature(StringBuilder result, MethodBase method, Func<Type, string> nameDispenser) {
            ContractUtils.RequiresNotNull(result, "result");
            ContractUtils.RequiresNotNull(method, "method");
            ContractUtils.RequiresNotNull(nameDispenser, "nameDispenser");

            MethodInfo methodInfo = method as MethodInfo;
            if (methodInfo != null) {
                FormatTypeName(result, methodInfo.ReturnType, nameDispenser);
                result.Append(' ');
            }

#if FEATURE_REFEMIT
            MethodBuilder builder = method as MethodBuilder;
            if (builder != null) {
                result.Append(builder.Signature);
                return result;
            }

            ConstructorBuilder cb = method as ConstructorBuilder;
            if (cb != null) {
                result.Append(cb.Signature);
                return result;
            }
#endif
            FormatTypeName(result, method.DeclaringType, nameDispenser);
            result.Append("::");
            result.Append(method.Name);

            if (!method.IsConstructor) {
                FormatTypeArgs(result, method.GetGenericArguments(), nameDispenser);
            }

            result.Append("(");

            if (!method.ContainsGenericParameters) {
                ParameterInfo[] ps = method.GetParameters();
                for (int i = 0; i < ps.Length; i++) {
                    if (i > 0) result.Append(", ");
                    FormatTypeName(result, ps[i].ParameterType, nameDispenser);
                    if (!System.String.IsNullOrEmpty(ps[i].Name)) {
                        result.Append(" ");
                        result.Append(ps[i].Name);
                    }
                }
            } else {
                result.Append("?");
            }

            result.Append(")");
            return result;
        }
#endif

        public static StringBuilder FormatTypeName(StringBuilder result, Type type) {
            return FormatTypeName(result, type, (t) => t.FullName);
        }

        public static StringBuilder FormatTypeName(StringBuilder result, Type type, Func<Type, string> nameDispenser) {
            ContractUtils.RequiresNotNull(result, "result");
            ContractUtils.RequiresNotNull(type, "type");
            ContractUtils.RequiresNotNull(nameDispenser, "nameDispenser");
            
            if (type.IsGenericType()) {
                Type genType = type.GetGenericTypeDefinition();
                string genericName = nameDispenser(genType).Replace('+', '.');
                int tickIndex = genericName.IndexOf('`');
                result.Append(tickIndex != -1 ? genericName.Substring(0, tickIndex) : genericName);

                Type[] typeArgs = type.GetGenericArguments();
                if (type.IsGenericTypeDefinition()) {
                    result.Append('<');
                    result.Append(',', typeArgs.Length - 1);
                    result.Append('>');
                } else {
                    FormatTypeArgs(result, typeArgs, nameDispenser);
                }
            } else if (type.IsGenericParameter) {
                result.Append(type.Name);
            } else {
                // cut namespace off:
                result.Append(nameDispenser(type).Replace('+', '.'));
            }
            return result;
        }

        public static StringBuilder FormatTypeArgs(StringBuilder result, Type[] types) {
            return FormatTypeArgs(result, types, (t) => t.FullName);
        }

        public static StringBuilder FormatTypeArgs(StringBuilder result, Type[] types, Func<Type, string> nameDispenser) {
            ContractUtils.RequiresNotNull(result, "result");
            ContractUtils.RequiresNotNullItems(types, "types");
            ContractUtils.RequiresNotNull(nameDispenser, "nameDispenser");
            
            if (types.Length > 0) {
                result.Append("<");

                for (int i = 0; i < types.Length; i++) {
                    if (i > 0) result.Append(", ");
                    FormatTypeName(result, types[i], nameDispenser);
                }

                result.Append(">");
            }
            return result;
        }

        internal static string ToValidTypeName(string str) {
            if (String.IsNullOrEmpty(str)) {
                return "_";
            }

            StringBuilder sb = new StringBuilder(str);
            for (int i = 0; i < str.Length; i++) {
                if (str[i] == '\0' || str[i] == '.' || str[i] == '*' || str[i] == '+' || str[i] == '[' || str[i] == ']' || str[i] == '\\') {
                    sb[i] = '_';
                }
            }
            return sb.ToString();
        }

        public static string GetNormalizedTypeName(Type type) {
            string name = type.Name;
            if (type.IsGenericType()) {
                return GetNormalizedTypeName(name);
            }
            return name;
        }

        public static string GetNormalizedTypeName(string typeName) {
            Debug.Assert(typeName.IndexOf('.') == -1); // This is the simple name, not the full name
            int backtick = typeName.IndexOf(ReflectionUtils.GenericArityDelimiter);
            if (backtick != -1) return typeName.Substring(0, backtick);
            return typeName;
        }

        #endregion

        #region Delegates and Dynamic Methods

#if WP75
        /// <summary>
        /// Creates an open delegate for the given (dynamic)method.
        /// </summary>
        public static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType) {
            return CreateDelegate(methodInfo, delegateType, null);
        }

        /// <summary>
        /// Creates a closed delegate for the given (dynamic)method.
        /// </summary>
        public static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType, object target) {
            return Delegate.CreateDelegate(delegateType, target, methodInfo);
        }
#elif !WIN8
        /// <summary>
        /// Creates an open delegate for the given (dynamic)method.
        /// </summary>
        public static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType) {
            return CreateDelegate(methodInfo, delegateType, null);
        }

        /// <summary>
        /// Creates a closed delegate for the given (dynamic)method.
        /// </summary>
        public static Delegate CreateDelegate(this MethodInfo methodInfo, Type delegateType, object target) {
#if FEATURE_REFEMIT
            DynamicMethod dm = methodInfo as DynamicMethod;
            if (dm != null) {
                return dm.CreateDelegate(delegateType, target);
#endif
            return Delegate.CreateDelegate(delegateType, target, methodInfo);
        }
#endif

#if FEATURE_LCG
        public static bool IsDynamicMethod(MethodBase method) {
            return !PlatformAdaptationLayer.IsCompactFramework && IsDynamicMethodInternal(method);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsDynamicMethodInternal(MethodBase method) {
            return method is DynamicMethod;
        }
#else
        public static bool IsDynamicMethod(MethodBase method) {
            return false;
        }
#endif

        public static void GetDelegateSignature(Type delegateType, out ParameterInfo[] parameterInfos, out ParameterInfo returnInfo) {
            ContractUtils.RequiresNotNull(delegateType, "delegateType");

            MethodInfo invokeMethod = delegateType.GetMethod("Invoke");
            ContractUtils.Requires(invokeMethod != null, "delegateType", Strings.InvalidDelegate);

            parameterInfos = invokeMethod.GetParameters();
            returnInfo = invokeMethod.ReturnParameter;
        }

        /// <summary>
        /// Gets a Func of CallSite, object * paramCnt, object delegate type
        /// that's suitable for use in a non-strongly typed call site.
        /// </summary>
        public static Type GetObjectCallSiteDelegateType(int paramCnt) {
            switch (paramCnt) {
                case 0: return typeof(Func<CallSite, object, object>);
                case 1: return typeof(Func<CallSite, object, object, object>);
                case 2: return typeof(Func<CallSite, object, object, object, object>);
                case 3: return typeof(Func<CallSite, object, object, object, object, object>);
                case 4: return typeof(Func<CallSite, object, object, object, object, object, object>);
                case 5: return typeof(Func<CallSite, object, object, object, object, object, object, object>);
                case 6: return typeof(Func<CallSite, object, object, object, object, object, object, object, object>);
                case 7: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object>);
                case 8: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object>);
                case 9: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object>);
                case 10: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object>);
                case 11: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                case 12: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                case 13: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                case 14: return typeof(Func<CallSite, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object, object>);
                default:
#if FEATURE_REFEMIT
                    Type[] paramTypes = new Type[paramCnt + 2];
                    paramTypes[0] = typeof(CallSite);
                    paramTypes[1] = typeof(object);
                    for (int i = 0; i < paramCnt; i++) {
                        paramTypes[i + 2] = typeof(object);
                    }
                    return Snippets.Shared.DefineDelegate("InvokeDelegate" + paramCnt, typeof(object), paramTypes);
#else
                    throw new NotSupportedException("Signature not supported on this platform.");
#endif
            }
        }

#if FEATURE_LCG
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework")]
        internal static DynamicMethod RawCreateDynamicMethod(string name, Type returnType, Type[] parameterTypes) {
#if SILVERLIGHT // Module-hosted DynamicMethod is not available in SILVERLIGHT
            return new DynamicMethod(name, returnType, parameterTypes);
#else
            //
            // WARNING: we set restrictedSkipVisibility == true  (last parameter)
            //          setting this bit will allow accessing nonpublic members
            //          for more information see http://msdn.microsoft.com/en-us/library/bb348332.aspx
            //
            return new DynamicMethod(name, returnType, parameterTypes, true);
#endif
        }
#endif

        #endregion

        #region Methods and Parameters

        public static MethodBase[] GetMethodInfos(MemberInfo[] members) {
            return ArrayUtils.ConvertAll<MemberInfo, MethodBase>(
                members,
                delegate(MemberInfo inp) { return (MethodBase)inp; });
        }

        public static Type[] GetParameterTypes(ParameterInfo[] parameterInfos) {
            return GetParameterTypes((IList<ParameterInfo>)parameterInfos);
        }

        public static Type[] GetParameterTypes(IList<ParameterInfo> parameterInfos) {
            Type[] result = new Type[parameterInfos.Count];
            for (int i = 0; i < result.Length; i++) {
                result[i] = parameterInfos[i].ParameterType;
            }
            return result;
        }

        public static Type GetReturnType(this MethodBase mi) {
            return (mi.IsConstructor) ? mi.DeclaringType : ((MethodInfo)mi).ReturnType;
        }

        public static bool SignatureEquals(MethodInfo method, params Type[] requiredSignature) {
            ContractUtils.RequiresNotNull(method, "method");

            Type[] actualTypes = ReflectionUtils.GetParameterTypes(method.GetParameters());
            Debug.Assert(actualTypes.Length == requiredSignature.Length - 1);
            int i = 0;
            while (i < actualTypes.Length) {
                if (actualTypes[i] != requiredSignature[i]) return false;
                i++;
            }

            return method.ReturnType == requiredSignature[i];
        }

#if CLR2 && !SILVERLIGHT
        private static Type _ExtensionAttributeType;
#endif

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static bool IsExtension(this MemberInfo member) {
            var dlrExtension = typeof(ExtensionAttribute);
            if (member.IsDefined(dlrExtension, false)) {
                return true;
            }

#if CLR2 && !SILVERLIGHT
            if (_ExtensionAttributeType == null) {
                try {
                    _ExtensionAttributeType = Assembly.Load("System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")
                        .GetType("System.Runtime.CompilerServices.ExtensionAttribute");
                } catch {
                    _ExtensionAttributeType = dlrExtension;
                }
            }

            if (_ExtensionAttributeType != dlrExtension) {
                return member.IsDefined(_ExtensionAttributeType, false);
            }
#endif
            return false;
        }

        public static bool IsOutParameter(this ParameterInfo pi) {
            // not using IsIn/IsOut properties as they are not available in Silverlight:
            return pi.ParameterType.IsByRef && (pi.Attributes & (ParameterAttributes.Out | ParameterAttributes.In)) == ParameterAttributes.Out;
        }

        /// <summary>
        /// Returns <c>true</c> if the specified parameter is mandatory, i.e. is not optional and doesn't have a default value.
        /// </summary>
        public static bool IsMandatory(this ParameterInfo pi) {
            return (pi.Attributes & ParameterAttributes.Optional) == 0 && !pi.HasDefaultValue();
        }

        public static bool HasDefaultValue(this ParameterInfo pi) {
#if !FEATURE_DEFAULT_PARAMETER_VALUE
            return pi.IsDefined(typeof(DefaultParameterValueAttribute), false);
#else
            return (pi.Attributes & ParameterAttributes.HasDefault) != 0;
#endif
        }

        public static bool ProhibitsNull(this ParameterInfo parameter) {
            return parameter.IsDefined(typeof(NotNullAttribute), false);
        }

        public static bool ProhibitsNullItems(this ParameterInfo parameter) {
            return parameter.IsDefined(typeof(NotNullItemsAttribute), false);
        }

        public static bool IsParamArray(this ParameterInfo parameter) {
            return parameter.IsDefined(typeof(ParamArrayAttribute), false);
        }

        public static bool IsParamDictionary(this ParameterInfo parameter) {
            return parameter.IsDefined(typeof(ParamDictionaryAttribute), false);
        }

        public static bool IsParamsMethod(MethodBase method) {
            return IsParamsMethod(method.GetParameters());
        }

        public static bool IsParamsMethod(ParameterInfo[] pis) {
            foreach (ParameterInfo pi in pis) {
                if (pi.IsParamArray() || pi.IsParamDictionary()) return true;
            }
            return false;
        }

        public static object GetDefaultValue(this ParameterInfo info) {
#if !FEATURE_DEFAULT_PARAMETER_VALUE
            if (info.IsOptional) {
                return info.ParameterType == typeof(object) ? Missing.Value : ScriptingRuntimeHelpers.GetPrimitiveDefaultValue(info.ParameterType);
            } 

            var defaultValueAttribute = info.GetCustomAttributes(typeof(DefaultParameterValueAttribute), false);
            if (defaultValueAttribute.Length > 0) {
                return ((DefaultParameterValueAttribute)defaultValueAttribute[0]).Value;
            } 

            return null;
#else
            return info.DefaultValue;
#endif
        }

        #endregion

        #region Types

        /// <summary>
        /// Yields all ancestors of the given type including the type itself.
        /// Does not include implemented interfaces.
        /// </summary>
        public static IEnumerable<Type> Ancestors(this Type type) {
            do {
                yield return type;
                type = type.GetTypeInfo().BaseType;
            } while (type != null);
        }

        /// <summary>
        /// Like Type.GetInterfaces, but only returns the interfaces implemented by this type
        /// and not its parents.
        /// </summary>
        public static List<Type> GetDeclaredInterfaces(Type type) {
            IEnumerable<Type> baseInterfaces = (type.GetBaseType() != null) ? type.GetBaseType().GetInterfaces() : EmptyTypes;
            List<Type> interfaces = new List<Type>();
            foreach (Type iface in type.GetInterfaces()) {
                if (!baseInterfaces.Contains(iface)) {
                    interfaces.Add(iface);
                }
            }
            return interfaces;
        }

        internal static IEnumerable<TypeInfo> GetAllTypesFromAssembly(Assembly asm) {
            // TODO: WP7, SL5
#if SILVERLIGHT // ReflectionTypeLoadException
            try {
                return asm.GetTypes();
            } catch (Exception) {
                return ReflectionUtils.EmptyTypes;
            }
#elif WIN8
            return asm.DefinedTypes;
#else
            foreach (Module module in asm.GetModules()) {
                Type[] moduleTypes;
                try {
                    moduleTypes = module.GetTypes();
                } catch (ReflectionTypeLoadException e) {
                    moduleTypes = e.Types;
                }

                foreach (var type in moduleTypes) {
                    if (type != null) {
                        yield return type;
                    }
                }
            }
#endif
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static IEnumerable<TypeInfo> GetAllTypesFromAssembly(Assembly assembly, bool includePrivateTypes) {
            ContractUtils.RequiresNotNull(assembly, "assembly");

            if (includePrivateTypes) {
                return GetAllTypesFromAssembly(assembly);
            }

            try {
#if WIN8
                return assembly.ExportedTypes.Select(t => t.GetTypeInfo());
#else
                return assembly.GetExportedTypes();
#endif
            } catch (NotSupportedException) {
                // GetExportedTypes does not work with dynamic assemblies
            } catch (Exception) {
                // Some type loads may cause exceptions. Unfortunately, there is no way to ask GetExportedTypes
                // for just the list of types that we successfully loaded.
            }

            return GetAllTypesFromAssembly(assembly).Where(type => type.IsPublic);
        }

        #endregion

        #region Type Builder
#if FEATURE_REFEMIT

#if WIN8 // TODO: what is ReservedMask?
        private const MethodAttributes MethodAttributesToEraseInOveride = MethodAttributes.Abstract | (MethodAttributes)0xD000;
#else
        private const MethodAttributes MethodAttributesToEraseInOveride = MethodAttributes.Abstract | MethodAttributes.ReservedMask;
#endif

        public static MethodBuilder DefineMethodOverride(TypeBuilder tb, MethodAttributes extra, MethodInfo decl) {
            MethodAttributes finalAttrs = (decl.Attributes & ~MethodAttributesToEraseInOveride) | extra;
            if (!decl.DeclaringType.GetTypeInfo().IsInterface) {
                finalAttrs &= ~MethodAttributes.NewSlot;
            }

            if ((extra & MethodAttributes.MemberAccessMask) != 0) {
                // remove existing member access, add new member access
                finalAttrs &= ~MethodAttributes.MemberAccessMask;
                finalAttrs |= extra;
            }

            MethodBuilder impl = tb.DefineMethod(decl.Name, finalAttrs, decl.CallingConvention);
            CopyMethodSignature(decl, impl, false);
            return impl;
        }

        public static void CopyMethodSignature(MethodInfo from, MethodBuilder to, bool substituteDeclaringType) {
            ParameterInfo[] paramInfos = from.GetParameters();
            Type[] parameterTypes = new Type[paramInfos.Length];
            Type[][] parameterRequiredModifiers = null, parameterOptionalModifiers = null;
            Type[] returnRequiredModifiers = null, returnOptionalModifiers = null;

#if FEATURE_CUSTOM_MODIFIERS
            returnRequiredModifiers = from.ReturnParameter.GetRequiredCustomModifiers();
            returnOptionalModifiers = from.ReturnParameter.GetOptionalCustomModifiers();
#endif
            for (int i = 0; i < paramInfos.Length; i++) {
                if (substituteDeclaringType && paramInfos[i].ParameterType == from.DeclaringType) {
                    parameterTypes[i] = to.DeclaringType;
                } else {
                    parameterTypes[i] = paramInfos[i].ParameterType;
                }

#if FEATURE_CUSTOM_MODIFIERS
                var mods = paramInfos[i].GetRequiredCustomModifiers();
                if (mods.Length > 0) {
                    if (parameterRequiredModifiers == null) {
                        parameterRequiredModifiers = new Type[paramInfos.Length][];
                    }

                    parameterRequiredModifiers[i] = mods;
                }

                mods = paramInfos[i].GetOptionalCustomModifiers();
                if (mods.Length > 0) {
                    if (parameterOptionalModifiers == null) {
                        parameterOptionalModifiers = new Type[paramInfos.Length][];
                    }

                    parameterOptionalModifiers[i] = mods;
                }
#endif
            }

            to.SetSignature(
                from.ReturnType, returnRequiredModifiers, returnOptionalModifiers,
                parameterTypes, parameterRequiredModifiers, parameterOptionalModifiers
            );

            CopyGenericMethodAttributes(from, to);

            for (int i = 0; i < paramInfos.Length; i++) {
                to.DefineParameter(i + 1, paramInfos[i].Attributes, paramInfos[i].Name);
            }
        }

        private static void CopyGenericMethodAttributes(MethodInfo from, MethodBuilder to) {
            if (from.IsGenericMethodDefinition) {
                Type[] args = from.GetGenericArguments();
                string[] names = new string[args.Length];
                for (int i = 0; i < args.Length; i++) {
                    names[i] = args[i].Name;
                }
                var builders = to.DefineGenericParameters(names);
                for (int i = 0; i < args.Length; i++) {
                    // Copy template parameter attributes
                    builders[i].SetGenericParameterAttributes(args[i].GetGenericParameterAttributes());

                    // Copy template parameter constraints
                    Type[] constraints = args[i].GetGenericParameterConstraints();
                    List<Type> interfaces = new List<Type>(constraints.Length);
                    foreach (Type constraint in constraints) {
                        if (constraint.IsInterface()) {
                            interfaces.Add(constraint);
                        } else {
                            builders[i].SetBaseTypeConstraint(constraint);
                        }
                    }
                    if (interfaces.Count > 0) {
                        builders[i].SetInterfaceConstraints(interfaces.ToArray());
                    }
                }
            }
        }
#endif
        #endregion

        #region Extension Methods

        public static IEnumerable<MethodInfo> GetVisibleExtensionMethods(Assembly assembly) {
#if FEATURE_METADATA_READER
            if (!assembly.IsDynamic && AppDomain.CurrentDomain.IsFullyTrusted) {
                try {
                    return GetVisibleExtensionMethodsFast(assembly);
                } catch (SecurityException) {
                    // full-demand can still fail if there is a partial trust domain on the stack
                }
            }
#endif
            return GetVisibleExtensionMethodsSlow(assembly);
        }

#if FEATURE_METADATA_READER
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<MethodInfo> GetVisibleExtensionMethodsFast(Assembly assembly) {
            // Security: link demand
            return MetadataServices.GetVisibleExtensionMethodInfos(assembly);
        }
#endif

        // TODO: make internal
        // TODO: handle type load exceptions
        public static IEnumerable<MethodInfo> GetVisibleExtensionMethodsSlow(Assembly assembly) {
            var ea = typeof(ExtensionAttribute);
            if (assembly.IsDefined(ea)) {
                foreach (TypeInfo type in ReflectionUtils.GetAllTypesFromAssembly(assembly)) {
                    if ((type.IsPublic || type.IsNestedPublic) &&
                        type.IsAbstract &&
                        type.IsSealed &&
                        type.IsDefined(ea, false)) {

                        foreach (MethodInfo method in type.AsType().GetDeclaredMethods()) {
                            if (method.IsPublic && method.IsStatic && method.IsDefined(ea, false)) {
                                yield return method;
                            }
                        }
                    }
                }
            }
        }

        // Value is null if there are no extension methods in the assembly.
        private static Dictionary<Assembly, Dictionary<string, List<ExtensionMethodInfo>>> _extensionMethodsCache;

        /// <summary>
        /// Enumerates extension methods in given assembly. Groups the methods by declaring namespace.
        /// Uses a global cache if <paramref name="useCache"/> is true.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, IEnumerable<ExtensionMethodInfo>>> GetVisibleExtensionMethodGroups(Assembly/*!*/ assembly, bool useCache) {
#if !CLR2 && FEATURE_REFEMIT
            useCache &= !assembly.IsDynamic;
#endif
            if (useCache) {
                if (_extensionMethodsCache == null) {
                    _extensionMethodsCache = new Dictionary<Assembly, Dictionary<string, List<ExtensionMethodInfo>>>();
                }

                lock (_extensionMethodsCache) {
                    Dictionary<string, List<ExtensionMethodInfo>> existing;
                    if (_extensionMethodsCache.TryGetValue(assembly, out existing)) {
                        return EnumerateExtensionMethods(existing);
                    }
                }
            }

            Dictionary<string, List<ExtensionMethodInfo>> result = null;
            foreach (MethodInfo method in ReflectionUtils.GetVisibleExtensionMethodsSlow(assembly)) {
                if (method.DeclaringType == null || method.DeclaringType.IsGenericTypeDefinition()) {
                    continue;
                }

                var parameters = method.GetParameters();
                if (parameters.Length == 0) {
                    continue;
                }

                Type type = parameters[0].ParameterType;
                if (type.IsByRef || type.IsPointer) {
                    continue;
                }

                string ns = method.DeclaringType.Namespace ?? String.Empty;
                List<ExtensionMethodInfo> extensions = null;

                if (result == null) {
                    result = new Dictionary<string, List<ExtensionMethodInfo>>();
                }

                if (!result.TryGetValue(ns, out extensions)) {
                    result.Add(ns, extensions = new List<ExtensionMethodInfo>());
                }

                extensions.Add(new ExtensionMethodInfo(type, method));
            }

            if (useCache) {
                lock (_extensionMethodsCache) {
                    _extensionMethodsCache[assembly] = result;
                }
            }

            return EnumerateExtensionMethods(result);
        }

        // TODO: GetVisibleExtensionMethods(Hashset<string> namespaces, Type type, string methodName) : IEnumerable<MethodInfo> {}

        private static IEnumerable<KeyValuePair<string, IEnumerable<ExtensionMethodInfo>>> EnumerateExtensionMethods(Dictionary<string, List<ExtensionMethodInfo>> dict) {
            if (dict != null) {
                foreach (var entry in dict) {
                    yield return new KeyValuePair<string, IEnumerable<ExtensionMethodInfo>>(entry.Key, new ReadOnlyCollection<ExtensionMethodInfo>(entry.Value));
                }
            }
        }

        #endregion

        #region Generic Types

        internal static Dictionary<Type, Type> BindGenericParameters(Type/*!*/ openType, Type/*!*/ closedType, bool ignoreUnboundParameters) {
            var binding = new Dictionary<Type, Type>();
            BindGenericParameters(openType, closedType, (parameter, type) => {
                Type existing;
                if (binding.TryGetValue(parameter, out existing)) {
                    return type == existing;
                }

                binding[parameter] = type;

                return true;
            });

            return ConstraintsViolated(binding, ignoreUnboundParameters) ? null : binding;
        }

        /// <summary>
        /// Binds occurances of generic parameters in <paramref name="openType"/> against corresponding types in <paramref name="closedType"/>.
        /// Invokes <paramref name="binder"/>(parameter, type) for each such binding.
        /// Returns false if the <paramref name="openType"/> is structurally different from <paramref name="closedType"/> or if the binder returns false.
        /// </summary>
        internal static bool BindGenericParameters(Type/*!*/ openType, Type/*!*/ closedType, Func<Type, Type, bool>/*!*/ binder) {
            if (openType.IsGenericParameter) {
                return binder(openType, closedType);
            }

            if (openType.IsArray) {
                if (!closedType.IsArray) {
                    return false;
                }
                return BindGenericParameters(openType.GetElementType(), closedType.GetElementType(), binder);
            }

            if (!openType.IsGenericType() || !closedType.IsGenericType()) {
                return openType == closedType;
            }

            if (openType.GetGenericTypeDefinition() != closedType.GetGenericTypeDefinition()) {
                return false;
            }

            Type[] closedArgs = closedType.GetGenericArguments();
            Type[] openArgs = openType.GetGenericArguments();

            for (int i = 0; i < openArgs.Length; i++) {
                if (!BindGenericParameters(openArgs[i], closedArgs[i], binder)) {
                    return false;
                }
            }

            return true;
        }

        internal static bool ConstraintsViolated(Dictionary<Type, Type>/*!*/ binding, bool ignoreUnboundParameters) {
            foreach (var entry in binding) {
                if (ConstraintsViolated(entry.Key, entry.Value, binding, ignoreUnboundParameters)) {
                    return true;
                }
            }

            return false;
        }

        internal static bool ConstraintsViolated(Type/*!*/ genericParameter, Type/*!*/ closedType, Dictionary<Type, Type>/*!*/ binding, bool ignoreUnboundParameters) {
            if ((genericParameter.GetGenericParameterAttributes() & GenericParameterAttributes.ReferenceTypeConstraint) != 0 && closedType.IsValueType()) {
                // value type to parameter type constrained as class
                return true;
            }

            if ((genericParameter.GetGenericParameterAttributes() & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0 &&
                (!closedType.IsValueType() || (closedType.IsGenericType() && closedType.GetGenericTypeDefinition() == typeof(Nullable<>)))) {
                // nullable<T> or class/interface to parameter type constrained as struct
                return true;
            }

            if ((genericParameter.GetGenericParameterAttributes() & GenericParameterAttributes.DefaultConstructorConstraint) != 0 &&
                (!closedType.IsValueType() && closedType.GetConstructor(ReflectionUtils.EmptyTypes) == null)) {
                // reference type w/o a default constructor to type constrianed as new()
                return true;
            }

            Type[] constraints = genericParameter.GetGenericParameterConstraints();
            for (int i = 0; i < constraints.Length; i++) {
                Type instantiation = InstantiateConstraint(constraints[i], binding);

                if (instantiation == null) {
                    if (ignoreUnboundParameters) {
                        continue;
                    } else {
                        return true;
                    }
                }

                if (!instantiation.IsAssignableFrom(closedType)) {
                    return true;
                }
            }

            return false;
        }

        internal static Type InstantiateConstraint(Type/*!*/ constraint, Dictionary<Type, Type>/*!*/ binding) {
            Debug.Assert(!constraint.IsArray && !constraint.IsByRef && !constraint.IsGenericTypeDefinition());
            if (!constraint.ContainsGenericParameters()) {
                return constraint;
            }

            Type closedType;
            if (constraint.IsGenericParameter) {
                return binding.TryGetValue(constraint, out closedType) ? closedType : null;
            }

            Type[] args = constraint.GetGenericArguments();
            for (int i = 0; i < args.Length; i++) {
                if ((args[i] = InstantiateConstraint(args[i], binding)) == null) {
                    return null;
                }
            }

            return constraint.GetGenericTypeDefinition().MakeGenericType(args);
        }

        #endregion
    }

    public struct ExtensionMethodInfo : IEquatable<ExtensionMethodInfo> {
        private readonly Type/*!*/ _extendedType; // cached type of the first parameter
        private readonly MethodInfo/*!*/ _method;

        internal ExtensionMethodInfo(Type/*!*/ extendedType, MethodInfo/*!*/ method) {
            Assert.NotNull(extendedType, method);
            _extendedType = extendedType;
            _method = method;
        }

        public Type/*!*/ ExtendedType {
            get { return _extendedType; }
        }

        public MethodInfo/*!*/ Method {
            get { return _method; }
        }

        public override bool Equals(object obj) {
            return obj is ExtensionMethodInfo && Equals((ExtensionMethodInfo)obj);
        }

        public bool Equals(ExtensionMethodInfo other) {
            return _method.Equals(other._method);
        }

        public static bool operator ==(ExtensionMethodInfo self, ExtensionMethodInfo other) {
            return self.Equals(other);
        }

        public static bool operator !=(ExtensionMethodInfo self, ExtensionMethodInfo other) {
            return !self.Equals(other);
        }

        public override int GetHashCode() {
            return _method.GetHashCode();
        }
        
        /// <summary>
        /// Determines if a given type matches the type that the method extends. 
        /// The match might be non-trivial if the extended type is an open generic type with constraints.
        /// </summary>
        public bool IsExtensionOf(Type/*!*/ type) {
            ContractUtils.RequiresNotNull(type, "type");
#if FEATURE_TYPE_EQUIVALENCE
            if (type.IsEquivalentTo(ExtendedType)) {
                return true;
            }
#else
            if (type == _extendedType) {
                return true;
            }
#endif
            if (!_extendedType.GetTypeInfo().ContainsGenericParameters) {
                return false;
            }

            //
            // Ignores constraints that can't be instantiated given the information we have (type of the first parameter).
            //
            // For example, 
            // void Foo<S, T>(this S x, T y) where S : T;
            //
            // We make such methods available on all types. 
            // If they are not called with arguments that satisfy the constraint the overload resolver might fail.
            //
            return ReflectionUtils.BindGenericParameters(_extendedType, type, true) != null;
        }
    }
}
