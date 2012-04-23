namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc.Resources;

    public class ReflectedControllerDescriptor : ControllerDescriptor {

        private ActionDescriptor[] _canonicalActionsCache;
        private readonly Type _controllerType;
        private readonly ActionMethodSelector _selector;

        public ReflectedControllerDescriptor(Type controllerType) {
            if (controllerType == null) {
                throw new ArgumentNullException("controllerType");
            }

            _controllerType = controllerType;
            _selector = new ActionMethodSelector(_controllerType);
        }

        public sealed override Type ControllerType {
            get {
                return _controllerType;
            }
        }

        public override ActionDescriptor FindAction(ControllerContext controllerContext, string actionName) {
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }
            if (String.IsNullOrEmpty(actionName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "actionName");
            }

            MethodInfo matched = _selector.FindActionMethod(controllerContext, actionName);
            if (matched == null) {
                return null;
            }

            return new ReflectedActionDescriptor(matched, actionName, this);
        }

        private MethodInfo[] GetAllActionMethodsFromSelector() {
            List<MethodInfo> allValidMethods = new List<MethodInfo>();
            allValidMethods.AddRange(_selector.AliasedMethods);
            allValidMethods.AddRange(_selector.NonAliasedMethods.SelectMany(g => g));
            return allValidMethods.ToArray();
        }

        public override ActionDescriptor[] GetCanonicalActions() {
            ActionDescriptor[] actions = LazilyFetchCanonicalActionsCollection();

            // need to clone array so that user modifications aren't accidentally stored
            return (ActionDescriptor[])actions.Clone();
        }

        public override object[] GetCustomAttributes(bool inherit) {
            return ControllerType.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
            return ControllerType.GetCustomAttributes(attributeType, inherit);
        }

        internal override IEnumerable<FilterAttribute> GetFilterAttributes(bool useCache) {
            if (useCache && GetType() == typeof(ReflectedControllerDescriptor)) {
                // Do not look at cache in types derived from this type because they might incorrectly implement GetCustomAttributes
                return ReflectedAttributeCache.GetTypeFilterAttributes(ControllerType);
            }
            return base.GetFilterAttributes(useCache);
        }

        public override bool IsDefined(Type attributeType, bool inherit) {
            return ControllerType.IsDefined(attributeType, inherit);
        }

        private ActionDescriptor[] LazilyFetchCanonicalActionsCollection() {
            return DescriptorUtil.LazilyFetchOrCreateDescriptors<MethodInfo, ActionDescriptor>(
                ref _canonicalActionsCache /* cacheLocation */,
                GetAllActionMethodsFromSelector /* initializer */,
                methodInfo => ReflectedActionDescriptor.TryCreateDescriptor(methodInfo, methodInfo.Name, this) /* converter */);
        }

    }
}
