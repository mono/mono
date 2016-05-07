//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;

    internal static class ActivityDelegateUtilities
    {
        public static ActivityDelegateMetadata GetMetadata(Type type)
        {
            ActivityDelegateMetadata metadata = new ActivityDelegateMetadata();

            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo property in properties)
            {
                if (property.PropertyType.IsGenericType)
                {
                    ActivityDelegateArgumentMetadata argument = null;
                    if (property.PropertyType.GetGenericTypeDefinition() == typeof(DelegateInArgument<>))
                    {
                        argument = new ActivityDelegateArgumentMetadata();
                        argument.Direction = ActivityDelegateArgumentDirection.In;
                    }
                    else if (property.PropertyType.GetGenericTypeDefinition() == typeof(DelegateOutArgument<>))
                    {
                        argument = new ActivityDelegateArgumentMetadata();
                        argument.Direction = ActivityDelegateArgumentDirection.Out;
                    }

                    if (argument != null)
                    {
                        argument.Name = property.Name;
                        argument.Type = property.PropertyType.GetGenericArguments()[0];
                        metadata.Add(argument);
                    }
                }
            }

            return metadata;
        }

        public static void FillDelegate(ActivityDelegate activityDelegate, ActivityDelegateMetadata metadata)
        {
            foreach (ActivityDelegateArgumentMetadata argument in metadata)
            {
                activityDelegate.GetType().GetProperty(argument.Name).SetValue(activityDelegate, CreateDelegateArgument(argument), null);
            }
        }

        public static bool HasActivityDelegate(Type type)
        {
            return GetPropertiesByHeuristics(type).Count > 0;
        }

        public static List<ActivityDelegateInfo> CreateActivityDelegateInfo(ModelItem activity)
        {
            List<ActivityDelegateInfo> list = new List<ActivityDelegateInfo>();

            foreach (PropertyInfo property in GetPropertiesByHeuristics(activity.ItemType))
            {
                list.Add(new ActivityDelegateInfo(activity, property.Name));
            }

            return list;
        }

        // Heuristics:
        // A property is considered to be an ActivityDelegate when
        //   1. it is a public instance property
        //   2. it has a public getter and a public setter
        //   3. its type is derived from ActivityDelegate
        //   4. can create an isntance of its type
        private static List<PropertyInfo> GetPropertiesByHeuristics(Type activityType)
        {
            List<PropertyInfo> result = new List<PropertyInfo>();

            PropertyInfo[] properties = activityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo property in properties)
            {
                if (property.GetGetMethod() != null &&
                    property.GetSetMethod() != null &&
                    property.PropertyType.IsSubclassOf(typeof(ActivityDelegate)) &&
                    TypeUtilities.CanCreateInstanceUsingDefaultConstructor(property.PropertyType))
                {
                    result.Add(property);
                }
            }

            result.Sort(new PropertyInfoComparer());

            return result;
        }

        private static DelegateArgument CreateDelegateArgument(ActivityDelegateArgumentMetadata argument)
        {
            DelegateArgument delegateArgument = null;
            if (argument.Direction == ActivityDelegateArgumentDirection.In)
            {
                delegateArgument = Activator.CreateInstance(typeof(DelegateInArgument<>).MakeGenericType(argument.Type)) as DelegateArgument;
            }
            else
            {
                delegateArgument = Activator.CreateInstance(typeof(DelegateOutArgument<>).MakeGenericType(argument.Type)) as DelegateArgument;
            }

            delegateArgument.Name = argument.Name;

            return delegateArgument;
        }

        private class PropertyInfoComparer : IComparer<PropertyInfo>
        {
            public int Compare(PropertyInfo x, PropertyInfo y)
            {
                Fx.Assert(x != null, "x should not be null");
                Fx.Assert(y != null, "y should not be null");

                return StringComparer.Ordinal.Compare(x.Name, y.Name);
            }
        }
    }
}
