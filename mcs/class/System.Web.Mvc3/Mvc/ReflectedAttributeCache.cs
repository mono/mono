namespace System.Web.Mvc {
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal static class ReflectedAttributeCache {
        private static readonly ConcurrentDictionary<MethodInfo, ReadOnlyCollection<ActionMethodSelectorAttribute>> _actionMethodSelectorAttributeCache = new ConcurrentDictionary<MethodInfo, ReadOnlyCollection<ActionMethodSelectorAttribute>>();
        private static readonly ConcurrentDictionary<MethodInfo, ReadOnlyCollection<ActionNameSelectorAttribute>> _actionNameSelectorAttributeCache = new ConcurrentDictionary<MethodInfo, ReadOnlyCollection<ActionNameSelectorAttribute>>();
        private static readonly ConcurrentDictionary<MethodInfo, ReadOnlyCollection<FilterAttribute>> _methodFilterAttributeCache = new ConcurrentDictionary<MethodInfo, ReadOnlyCollection<FilterAttribute>>();

        private static readonly ConcurrentDictionary<Type, ReadOnlyCollection<FilterAttribute>> _typeFilterAttributeCache = new ConcurrentDictionary<Type, ReadOnlyCollection<FilterAttribute>>();

        public static ICollection<FilterAttribute> GetTypeFilterAttributes(Type type) {
            return GetAttributes(_typeFilterAttributeCache, type);
        }

        public static ICollection<FilterAttribute> GetMethodFilterAttributes(MethodInfo methodInfo) {
            return GetAttributes(_methodFilterAttributeCache, methodInfo);
        }

        public static ICollection<ActionMethodSelectorAttribute> GetActionMethodSelectorAttributes(MethodInfo methodInfo) {
            return GetAttributes(_actionMethodSelectorAttributeCache, methodInfo);
        }

        public static ICollection<ActionNameSelectorAttribute> GetActionNameSelectorAttributes(MethodInfo methodInfo) {
            return GetAttributes(_actionNameSelectorAttributeCache, methodInfo);
        }

        private static ReadOnlyCollection<TAttribute> GetAttributes<TMemberInfo, TAttribute>(ConcurrentDictionary<TMemberInfo, ReadOnlyCollection<TAttribute>> lookup, TMemberInfo memberInfo)
            where TAttribute : Attribute
            where TMemberInfo : MemberInfo {

            Debug.Assert(memberInfo != null);
            Debug.Assert(lookup != null);
            return lookup.GetOrAdd(memberInfo, mi => new ReadOnlyCollection<TAttribute>((TAttribute[])memberInfo.GetCustomAttributes(typeof(TAttribute), inherit: true)));
        }
    }
}
