/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc.Resources;

    public abstract class ActionDescriptor : ICustomAttributeProvider {

        private readonly static AllowMultipleAttributesCache _allowMultiplAttributesCache = new AllowMultipleAttributesCache();
        private readonly static ActionMethodDispatcherCache _staticDispatcherCache = new ActionMethodDispatcherCache();
        private ActionMethodDispatcherCache _instanceDispatcherCache;

        private static readonly ActionSelector[] _emptySelectors = new ActionSelector[0];

        public abstract string ActionName {
            get;
        }

        public abstract ControllerDescriptor ControllerDescriptor {
            get;
        }

        internal ActionMethodDispatcherCache DispatcherCache {
            get {
                if (_instanceDispatcherCache == null) {
                    _instanceDispatcherCache = _staticDispatcherCache;
                }
                return _instanceDispatcherCache;
            }
            set {
                _instanceDispatcherCache = value;
            }
        }

        public abstract object Execute(ControllerContext controllerContext, IDictionary<string, object> parameters);

        internal static object ExtractParameterFromDictionary(ParameterInfo parameterInfo, IDictionary<string, object> parameters, MethodInfo methodInfo) {
            object value;

            if (!parameters.TryGetValue(parameterInfo.Name, out value)) {
                // the key should always be present, even if the parameter value is null
                string message = String.Format(CultureInfo.CurrentUICulture, MvcResources.ReflectedActionDescriptor_ParameterNotInDictionary,
                    parameterInfo.Name, parameterInfo.ParameterType, methodInfo, methodInfo.DeclaringType);
                throw new ArgumentException(message, "parameters");
            }

            if (value == null && !TypeHelpers.TypeAllowsNullValue(parameterInfo.ParameterType)) {
                // tried to pass a null value for a non-nullable parameter type
                string message = String.Format(CultureInfo.CurrentUICulture, MvcResources.ReflectedActionDescriptor_ParameterCannotBeNull,
                    parameterInfo.Name, parameterInfo.ParameterType, methodInfo, methodInfo.DeclaringType);
                throw new ArgumentException(message, "parameters");
            }

            if (value != null && !parameterInfo.ParameterType.IsInstanceOfType(value)) {
                // value was supplied but is not of the proper type
                string message = String.Format(CultureInfo.CurrentUICulture, MvcResources.ReflectedActionDescriptor_ParameterValueHasWrongType,
                    parameterInfo.Name, methodInfo, methodInfo.DeclaringType, value.GetType(), parameterInfo.ParameterType);
                throw new ArgumentException(message, "parameters");
            }

            return value;
        }

        internal static object ExtractParameterOrDefaultFromDictionary(ParameterInfo parameterInfo, IDictionary<string, object> parameters) {
            Type parameterType = parameterInfo.ParameterType;

            object value;
            parameters.TryGetValue(parameterInfo.Name, out value);

            // if wrong type, replace with default instance
            if (parameterType.IsInstanceOfType(value)) {
                return value;
            }
            else {
                object defaultValue;
                if (ParameterInfoUtil.TryGetDefaultValue(parameterInfo, out defaultValue)) {
                    return defaultValue;
                }
                else {
                    return TypeHelpers.GetDefaultValue(parameterType);
                }
            }
        }

        public virtual object[] GetCustomAttributes(bool inherit) {
            return GetCustomAttributes(typeof(object), inherit);
        }

        public virtual object[] GetCustomAttributes(Type attributeType, bool inherit) {
            if (attributeType == null) {
                throw new ArgumentNullException("attributeType");
            }

            return (object[])Array.CreateInstance(attributeType, 0);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "This method may perform non-trivial work.")]
        public virtual FilterInfo GetFilters() {
            return new FilterInfo();
        }

        internal static FilterInfo GetFilters(MethodInfo methodInfo) {
            // Enumerable.OrderBy() is a stable sort, so this method preserves scope ordering.
            FilterAttribute[] typeFilters = (FilterAttribute[])methodInfo.ReflectedType.GetCustomAttributes(typeof(FilterAttribute), true /* inherit */);
            FilterAttribute[] methodFilters = (FilterAttribute[])methodInfo.GetCustomAttributes(typeof(FilterAttribute), true /* inherit */);
            List<FilterAttribute> orderedFilters = RemoveOverriddenFilters(typeFilters.Concat(methodFilters)).OrderBy(attr => attr.Order).ToList();

            FilterInfo filterInfo = new FilterInfo();
            MergeFiltersIntoList(orderedFilters, filterInfo.ActionFilters);
            MergeFiltersIntoList(orderedFilters, filterInfo.AuthorizationFilters);
            MergeFiltersIntoList(orderedFilters, filterInfo.ExceptionFilters);
            MergeFiltersIntoList(orderedFilters, filterInfo.ResultFilters);
            return filterInfo;
        }

        public abstract ParameterDescriptor[] GetParameters();

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "This method may perform non-trivial work.")]
        public virtual ICollection<ActionSelector> GetSelectors() {
            return _emptySelectors;
        }

        public virtual bool IsDefined(Type attributeType, bool inherit) {
            if (attributeType == null) {
                throw new ArgumentNullException("attributeType");
            }

            return false;
        }

        internal static void MergeFiltersIntoList<TFilter>(IList<FilterAttribute> allFilters, IList<TFilter> destFilters) where TFilter : class {
            foreach (FilterAttribute filter in allFilters) {
                TFilter castFilter = filter as TFilter;
                if (castFilter != null) {
                    destFilters.Add(castFilter);
                }
            }
        }

        internal static IEnumerable<FilterAttribute> RemoveOverriddenFilters(IEnumerable<FilterAttribute> filters) {
            // If an attribute is declared on both the controller and on an action method and that attribute's
            // type has AllowMultiple = false (which is the default for attributes), we should ignore the attributes
            // declared on the controller. The CLR's reflection implementation follows a similar algorithm when it
            // encounters an overridden virtual method where both the base and the override contain some
            // AllowMultiple = false attribute.

            // Key = attribute type
            // Value = -1 if AllowMultiple true, last index of this attribute type if AllowMultiple false
            Dictionary<Type, int> attrsIndexes = new Dictionary<Type, int>();

            FilterAttribute[] filtersList = filters.ToArray();
            for (int i = 0; i < filtersList.Length; i++) {
                FilterAttribute filter = filtersList[i];
                Type filterType = filter.GetType();

                int lastIndex;
                if (attrsIndexes.TryGetValue(filterType, out lastIndex)) {
                    if (lastIndex >= 0) {
                        // this filter already exists and AllowMultiple = false, so clear last entry
                        filtersList[lastIndex] = null;
                        attrsIndexes[filterType] = i;
                    }
                }
                else {
                    // not found - add to dictionary
                    // exactly one AttributeUsageAttribute will always be present
                    bool allowMultiple = _allowMultiplAttributesCache.IsMultiUseAttribute(filterType);
                    attrsIndexes[filterType] = (allowMultiple) ? -1 : i;
                }
            }

            // any duplicated attributes have now been nulled out, so just return remaining attributes
            return filtersList.Where(attr => attr != null);
        }

        internal static string VerifyActionMethodIsCallable(MethodInfo methodInfo) {
            // we can't call instance methods where the 'this' parameter is a type other than ControllerBase
            if (!methodInfo.IsStatic && !typeof(ControllerBase).IsAssignableFrom(methodInfo.ReflectedType)) {
                return String.Format(CultureInfo.CurrentUICulture, MvcResources.ReflectedActionDescriptor_CannotCallInstanceMethodOnNonControllerType,
                    methodInfo, methodInfo.ReflectedType.FullName);
            }

            // we can't call methods with open generic type parameters
            if (methodInfo.ContainsGenericParameters) {
                return String.Format(CultureInfo.CurrentUICulture, MvcResources.ReflectedActionDescriptor_CannotCallOpenGenericMethods,
                    methodInfo, methodInfo.ReflectedType.FullName);
            }

            // we can't call methods with ref/out parameters
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            foreach (ParameterInfo parameterInfo in parameterInfos) {
                if (parameterInfo.IsOut || parameterInfo.ParameterType.IsByRef) {
                    return String.Format(CultureInfo.CurrentUICulture, MvcResources.ReflectedActionDescriptor_CannotCallMethodsWithOutOrRefParameters,
                        methodInfo, methodInfo.ReflectedType.FullName, parameterInfo);
                }
            }

            // we can call this method
            return null;
        }

        private sealed class AllowMultipleAttributesCache : ReaderWriterCache<Type, bool> {
            public bool IsMultiUseAttribute(Type attributeType) {
                return FetchOrCreateItem(attributeType, () => AttributeUsageAllowsMultiple(attributeType));
            }

            private static bool AttributeUsageAllowsMultiple(Type type) {
                return (((AttributeUsageAttribute[])type.GetCustomAttributes(typeof(AttributeUsageAttribute), true))[0]).AllowMultiple;
            }
        }

    }
}
