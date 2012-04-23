namespace System.Web.Mvc.Async {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Web.Mvc.Resources;

    internal sealed class AsyncActionMethodSelector {

        public AsyncActionMethodSelector(Type controllerType) {
            ControllerType = controllerType;
            PopulateLookupTables();
        }

        public Type ControllerType {
            get;
            private set;
        }

        public MethodInfo[] AliasedMethods {
            get;
            private set;
        }

        public ILookup<string, MethodInfo> NonAliasedMethods {
            get;
            private set;
        }

        private AmbiguousMatchException CreateAmbiguousActionMatchException(IEnumerable<MethodInfo> ambiguousMethods, string actionName) {
            string ambiguityList = CreateAmbiguousMatchList(ambiguousMethods);
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.ActionMethodSelector_AmbiguousMatch,
                actionName, ControllerType.Name, ambiguityList);
            return new AmbiguousMatchException(message);
        }

        private AmbiguousMatchException CreateAmbiguousMethodMatchException(IEnumerable<MethodInfo> ambiguousMethods, string methodName) {
            string ambiguityList = CreateAmbiguousMatchList(ambiguousMethods);
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.AsyncActionMethodSelector_AmbiguousMethodMatch,
                methodName, ControllerType.Name, ambiguityList);
            return new AmbiguousMatchException(message);
        }

        private static string CreateAmbiguousMatchList(IEnumerable<MethodInfo> ambiguousMethods) {
            StringBuilder exceptionMessageBuilder = new StringBuilder();
            foreach (MethodInfo methodInfo in ambiguousMethods) {
                exceptionMessageBuilder.AppendLine();
                exceptionMessageBuilder.AppendFormat(CultureInfo.CurrentCulture, MvcResources.ActionMethodSelector_AmbiguousMatchType, methodInfo, methodInfo.DeclaringType.FullName);
            }

            return exceptionMessageBuilder.ToString();
        }

        public ActionDescriptorCreator FindAction(ControllerContext controllerContext, string actionName) {
            List<MethodInfo> methodsMatchingName = GetMatchingAliasedMethods(controllerContext, actionName);
            methodsMatchingName.AddRange(NonAliasedMethods[actionName]);
            List<MethodInfo> finalMethods = RunSelectionFilters(controllerContext, methodsMatchingName);

            switch (finalMethods.Count) {
                case 0:
                    return null;

                case 1:
                    MethodInfo entryMethod = finalMethods[0];
                    return GetActionDescriptorDelegate(entryMethod);

                default:
                    throw CreateAmbiguousActionMatchException(finalMethods, actionName);
            }
        }

        private ActionDescriptorCreator GetActionDescriptorDelegate(MethodInfo entryMethod) {
            // Is this the FooAsync() / FooCompleted() pattern?
            if (IsAsyncSuffixedMethod(entryMethod)) {
                string completionMethodName = entryMethod.Name.Substring(0, entryMethod.Name.Length - "Async".Length) + "Completed";
                MethodInfo completionMethod = GetMethodByName(completionMethodName);
                if (completionMethod != null) {
                    return (actionName, controllerDescriptor) => new ReflectedAsyncActionDescriptor(entryMethod, completionMethod, actionName, controllerDescriptor);
                }
                else {
                    throw Error.AsyncActionMethodSelector_CouldNotFindMethod(completionMethodName, ControllerType);
                }
            }

            // Fallback to synchronous method
            return (actionName, controllerDescriptor) => new ReflectedActionDescriptor(entryMethod, actionName, controllerDescriptor);
        }

        private static string GetCanonicalMethodName(MethodInfo methodInfo) {
            string methodName = methodInfo.Name;
            return (IsAsyncSuffixedMethod(methodInfo))
                ? methodName.Substring(0, methodName.Length - "Async".Length)
                : methodName;
        }

        internal List<MethodInfo> GetMatchingAliasedMethods(ControllerContext controllerContext, string actionName) {
            // find all aliased methods which are opting in to this request
            // to opt in, all attributes defined on the method must return true

            var methods = from methodInfo in AliasedMethods
                          let attrs = ReflectedAttributeCache.GetActionNameSelectorAttributes(methodInfo)
                          where attrs.All(attr => attr.IsValidName(controllerContext, actionName, methodInfo))
                          select methodInfo;
            return methods.ToList();
        }

        private static bool IsAsyncSuffixedMethod(MethodInfo methodInfo) {
            return methodInfo.Name.EndsWith("Async", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCompletedSuffixedMethod(MethodInfo methodInfo) {
            return methodInfo.Name.EndsWith("Completed", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsMethodDecoratedWithAliasingAttribute(MethodInfo methodInfo) {
            return methodInfo.IsDefined(typeof(ActionNameSelectorAttribute), true /* inherit */);
        }

        private MethodInfo GetMethodByName(string methodName) {
            List<MethodInfo> methods = (from MethodInfo methodInfo in ControllerType.GetMember(methodName, MemberTypes.Method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase)
                                        where IsValidActionMethod(methodInfo, false /* stripInfrastructureMethods */)
                                        select methodInfo).ToList();

            switch (methods.Count) {
                case 0:
                    return null;

                case 1:
                    return methods[0];

                default:
                    throw CreateAmbiguousMethodMatchException(methods, methodName);
            }
        }

        private static bool IsValidActionMethod(MethodInfo methodInfo) {
            return IsValidActionMethod(methodInfo, true /* stripInfrastructureMethods */);
        }

        private static bool IsValidActionMethod(MethodInfo methodInfo, bool stripInfrastructureMethods) {
            if (methodInfo.IsSpecialName) {
                // not a normal method, e.g. a constructor or an event
                return false;
            }

            if (methodInfo.GetBaseDefinition().DeclaringType.IsAssignableFrom(typeof(AsyncController))) {
                // is a method on Object, ControllerBase, Controller, or AsyncController
                return false;
            };

            if (stripInfrastructureMethods) {
                if (IsCompletedSuffixedMethod(methodInfo)) {
                    // do not match FooCompleted() methods, as these are infrastructure methods
                    return false;
                }
            }

            return true;
        }

        private void PopulateLookupTables() {
            MethodInfo[] allMethods = ControllerType.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public);
            MethodInfo[] actionMethods = Array.FindAll(allMethods, IsValidActionMethod);

            AliasedMethods = Array.FindAll(actionMethods, IsMethodDecoratedWithAliasingAttribute);
            NonAliasedMethods = actionMethods.Except(AliasedMethods).ToLookup(GetCanonicalMethodName, StringComparer.OrdinalIgnoreCase);
        }

        private static List<MethodInfo> RunSelectionFilters(ControllerContext controllerContext, List<MethodInfo> methodInfos) {
            // remove all methods which are opting out of this request
            // to opt out, at least one attribute defined on the method must return false

            List<MethodInfo> matchesWithSelectionAttributes = new List<MethodInfo>();
            List<MethodInfo> matchesWithoutSelectionAttributes = new List<MethodInfo>();

            foreach (MethodInfo methodInfo in methodInfos) {
                ICollection<ActionMethodSelectorAttribute> attrs = ReflectedAttributeCache.GetActionMethodSelectorAttributes(methodInfo);
                if (attrs.Count == 0) {
                    matchesWithoutSelectionAttributes.Add(methodInfo);
                }
                else if (attrs.All(attr => attr.IsValidForRequest(controllerContext, methodInfo))) {
                    matchesWithSelectionAttributes.Add(methodInfo);
                }
            }

            // if a matching action method had a selection attribute, consider it more specific than a matching action method
            // without a selection attribute
            return (matchesWithSelectionAttributes.Count > 0) ? matchesWithSelectionAttributes : matchesWithoutSelectionAttributes;
        }

    }
}
