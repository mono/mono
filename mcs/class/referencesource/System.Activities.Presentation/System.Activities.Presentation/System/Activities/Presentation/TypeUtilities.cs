//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Reflection;
    using System.Runtime;

    internal static class TypeUtilities
    {
        // returns true if the generic parameter can be subsituted by the candidate.
        // Sometime if the generic parameter is too complex, this method will return a false positive, but it's fine.
        public static bool CanSubstituteGenericParameter(Type genericParameter, Type candidate)
        {
            Fx.Assert(genericParameter != null, "genericParameter should not be null");
            Fx.Assert(genericParameter.IsGenericParameter == true, "genericParameter should be a valid generic parameter");

            if (ContainsAnyFlag(genericParameter.GenericParameterAttributes, GenericParameterAttributes.SpecialConstraintMask))
            {
                if (ContainsAnyFlag(genericParameter.GenericParameterAttributes, GenericParameterAttributes.ReferenceTypeConstraint))
                {
                    if (!candidate.IsClass && !candidate.IsInterface)
                    {
                        return false;
                    }
                }

                if (ContainsAnyFlag(genericParameter.GenericParameterAttributes, GenericParameterAttributes.DefaultConstructorConstraint))
                {
                    if (!TypeUtilities.CanCreateInstanceUsingDefaultConstructor(candidate))
                    {
                        return false;
                    }
                }

                if (ContainsAnyFlag(genericParameter.GenericParameterAttributes, GenericParameterAttributes.NotNullableValueTypeConstraint))
                {
                    if (!candidate.IsValueType)
                    {
                        return false;
                    }

                    if (candidate.IsGenericType && candidate.GetGenericTypeDefinition() != typeof(Nullable<>))
                    {
                        return false;
                    }
                }
            }

            foreach (Type constraint in genericParameter.GetGenericParameterConstraints())
            {
                if (constraint.ContainsGenericParameters)
                {
                    // return true for all types because we don't have a good way to find out if the candidate is good or not.
                    // The caller will try to create closed generic type which will tell if the candidate is really good or not.
                    continue;
                }

                if (!constraint.IsAssignableFrom(candidate))
                {
                    return false;
                }
            }

            return true;
        }

        public static bool CanCreateInstanceUsingDefaultConstructor(Type type)
        {
            Fx.Assert(type != null, "type could not be null");

            return type.IsValueType || (!type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null);
        }

        public static bool IsTypeCompatible(Type childObjectType, Type parentObjectType)
        {
            if (!parentObjectType.IsGenericTypeDefinition)
            {
                return parentObjectType.IsAssignableFrom(childObjectType);
            }
            else if (parentObjectType.IsInterface)
            {
                Type[] interfaceTypes = childObjectType.GetInterfaces();
                foreach (Type interfaceType in interfaceTypes)
                {
                    if (interfaceType.IsGenericType)
                    {
                        if (interfaceType.GetGenericTypeDefinition() == parentObjectType)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            else
            {
                Type current = childObjectType;
                while (current != null)
                {
                    if (current.IsGenericType)
                    {
                        if (current.GetGenericTypeDefinition() == parentObjectType)
                        {
                            return true;
                        }
                    }

                    current = current.BaseType;
                }

                return false;
            }
        }

        private static bool ContainsAnyFlag(GenericParameterAttributes attributes, GenericParameterAttributes flags)
        {
            return (attributes & flags) != GenericParameterAttributes.None;
        }
    }
}
