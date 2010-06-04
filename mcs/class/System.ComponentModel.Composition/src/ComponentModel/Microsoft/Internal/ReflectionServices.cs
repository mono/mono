// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Internal
{
    internal static class ReflectionServices
    {
        public static Assembly Assembly(this MemberInfo member)
        {
            Type type = member as Type;
            if (type != null)
            {
                return type.Assembly;
            }

            return member.DeclaringType.Assembly;
        }

        public static bool IsVisible(this ConstructorInfo constructor)
        {
            return constructor.DeclaringType.IsVisible && constructor.IsPublic;
        }

        public static bool IsVisible(this FieldInfo field)
        {
            return field.DeclaringType.IsVisible && field.IsPublic;
        }

        public static bool IsVisible(this MethodInfo method)
        {
            if (!method.DeclaringType.IsVisible)
                return false;

            if (!method.IsPublic)
                return false;

            if (method.IsGenericMethod)
            {
                // Check type arguments, for example if we're passed 'Activator.CreateInstance<SomeMefInternalType>()'
                foreach (Type typeArgument in method.GetGenericArguments())
                {
                    if (!typeArgument.IsVisible)
                        return false;
                }
            }

            return true;
        }

        public static string GetDisplayName(Type declaringType, string name)
        {
            Assumes.NotNull(declaringType);

            return declaringType.GetDisplayName() + "." + name;
        }

        public static string GetDisplayName(this MemberInfo member)
        {
            Assumes.NotNull(member);
  
            switch (member.MemberType)
            {
                case MemberTypes.TypeInfo:
                case MemberTypes.NestedType:
                    return ((Type)member).FullName;
            }

            return GetDisplayName(member.DeclaringType, member.Name);            
        }

        internal static bool TryGetGenericInterfaceType(Type instanceType, Type targetOpenInterfaceType, out Type targetClosedInterfaceType)
        {
            // The interface must be open
            Assumes.IsTrue(targetOpenInterfaceType.IsInterface);
            Assumes.IsTrue(targetOpenInterfaceType.IsGenericTypeDefinition);
            Assumes.IsTrue(!instanceType.IsGenericTypeDefinition);

            // if instanceType is an interface, we must first check it directly
            if (instanceType.IsInterface &&
                instanceType.IsGenericType &&
                instanceType.GetGenericTypeDefinition() == targetOpenInterfaceType)
            {
                targetClosedInterfaceType = instanceType;
                return true;
            }

            try
            {
                // Purposefully not using FullName here because it results in a significantly
                //  more expensive implementation of GetInterface, this does mean that we're
                //  takign the chance that there aren't too many types which implement multiple
                //  interfaces by the same name...
                Type targetInterface = instanceType.GetInterface(targetOpenInterfaceType.Name, false);
                if (targetInterface != null &&
                    targetInterface.GetGenericTypeDefinition() == targetOpenInterfaceType)
                {
                    targetClosedInterfaceType = targetInterface;
                    return true;
                }
            }
            catch (AmbiguousMatchException)
            {
                // If there are multiple with the same name we should not pick any
            }

            targetClosedInterfaceType = null;
            return false;
        }

        internal static IEnumerable<PropertyInfo> GetAllProperties(this Type type)
        {
            return type.GetInterfaces().Concat(new Type[] { type }).SelectMany(itf => itf.GetProperties());
        }
    }
}
