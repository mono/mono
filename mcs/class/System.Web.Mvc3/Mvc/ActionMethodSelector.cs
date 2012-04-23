namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Web.Mvc.Resources;

    internal sealed class ActionMethodSelector {

        public ActionMethodSelector(Type controllerType) {
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

        private AmbiguousMatchException CreateAmbiguousMatchException(List<MethodInfo> ambiguousMethods, string actionName) {
            StringBuilder exceptionMessageBuilder = new StringBuilder();
            foreach (MethodInfo methodInfo in ambiguousMethods) {
                string controllerAction = Convert.ToString(methodInfo, CultureInfo.CurrentCulture);
                string controllerType = methodInfo.DeclaringType.FullName;
                exceptionMessageBuilder.AppendLine();
                exceptionMessageBuilder.AppendFormat(CultureInfo.CurrentCulture, MvcResources.ActionMethodSelector_AmbiguousMatchType, controllerAction, controllerType);
            }
            string message = String.Format(CultureInfo.CurrentCulture, MvcResources.ActionMethodSelector_AmbiguousMatch,
                actionName, ControllerType.Name, exceptionMessageBuilder);
            return new AmbiguousMatchException(message);
        }

        public MethodInfo FindActionMethod(ControllerContext controllerContext, string actionName) {
            List<MethodInfo> methodsMatchingName = GetMatchingAliasedMethods(controllerContext, actionName);
            methodsMatchingName.AddRange(NonAliasedMethods[actionName]);
            List<MethodInfo> finalMethods = RunSelectionFilters(controllerContext, methodsMatchingName);

            switch (finalMethods.Count) {
                case 0:
                    return null;

                case 1:
                    return finalMethods[0];

                default:
                    throw CreateAmbiguousMatchException(finalMethods, actionName);
            }
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

        private static bool IsMethodDecoratedWithAliasingAttribute(MethodInfo methodInfo) {
            return methodInfo.IsDefined(typeof(ActionNameSelectorAttribute), true /* inherit */);
        }

        private static bool IsValidActionMethod(MethodInfo methodInfo) {
            return !(methodInfo.IsSpecialName ||
                     methodInfo.GetBaseDefinition().DeclaringType.IsAssignableFrom(typeof(Controller)));
        }

        private void PopulateLookupTables() {
            MethodInfo[] allMethods = ControllerType.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public);
            MethodInfo[] actionMethods = Array.FindAll(allMethods, IsValidActionMethod);

            AliasedMethods = Array.FindAll(actionMethods, IsMethodDecoratedWithAliasingAttribute);
            NonAliasedMethods = actionMethods.Except(AliasedMethods).ToLookup(method => method.Name, StringComparer.OrdinalIgnoreCase);
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
