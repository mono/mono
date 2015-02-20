//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Activities.Presentation.Hosting;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Xaml;

    internal static class MultiTargetingTypeResolver
    {
        public static ResolverResult Resolve(MultiTargetingSupportService multiTargetingService, Type type)
        {
            SharedFx.Assert(multiTargetingService != null, "multiTargetingService should not be null");
            SharedFx.Assert(type != null, "type should not be null");

            if (!multiTargetingService.IsSupportedType(type))
            {
                return ResolverResult.Unknown;
            }

            ResolverResult result;

            Type reflectionType = multiTargetingService.GetReflectionType(type);

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            PropertyInfo[] targetProperties = reflectionType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);

            List<string> newProperties = new List<string>();

            // Assume we don't remove properties in newer framework
            // We only compare property name here
            if (properties.Length > targetProperties.Length)
            {
                foreach (PropertyInfo propertyInfo in properties)
                {
                    bool found = false;
                    foreach (PropertyInfo targetProperty in targetProperties)
                    {
                        if (targetProperty.Name == propertyInfo.Name)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        newProperties.Add(propertyInfo.Name);
                    }
                }

                result = new ResolverResult(newProperties);
            }
            else
            {
                result = ResolverResult.FullySupported;
            }

            return result;
        }

        public static XamlType GetXamlType(ResolverResult resolverResult, XamlType oldXamlType)
        {
            SharedFx.Assert(oldXamlType != null, "oldXamlType should not be null");

            switch (resolverResult.Kind)
            {
                case XamlTypeKind.FullySupported:
                    return oldXamlType;

                case XamlTypeKind.PartialSupported:
                    return new XamlTypeWithExtraPropertiesRemoved(oldXamlType.UnderlyingType, oldXamlType.SchemaContext, resolverResult.NewProperties);

                default:
                    SharedFx.Assert(resolverResult.Kind == XamlTypeKind.Unknown, "resolverResult.Kind should be XamlTypeKind.Unknown.");
                    return null;
            }
        }
    }
}
