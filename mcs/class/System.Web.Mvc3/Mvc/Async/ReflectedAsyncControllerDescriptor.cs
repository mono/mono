namespace System.Web.Mvc.Async {
    using System;
    using System.Collections.Generic;

    public class ReflectedAsyncControllerDescriptor : ControllerDescriptor {

        private static readonly ActionDescriptor[] _emptyCanonicalActions = new ActionDescriptor[0];

        private readonly Type _controllerType;
        private readonly AsyncActionMethodSelector _selector;

        public ReflectedAsyncControllerDescriptor(Type controllerType) {
            if (controllerType == null) {
                throw new ArgumentNullException("controllerType");
            }

            _controllerType = controllerType;
            _selector = new AsyncActionMethodSelector(_controllerType);
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
                throw Error.ParameterCannotBeNullOrEmpty("actionName");
            }

            ActionDescriptorCreator creator = _selector.FindAction(controllerContext, actionName);
            if (creator == null) {
                return null;
            }

            return creator(actionName, this);
        }

        public override ActionDescriptor[] GetCanonicalActions() {
            // everything is looked up dymanically, so there are no 'canonical' actions
            return _emptyCanonicalActions;
        }

        public override object[] GetCustomAttributes(bool inherit) {
            return ControllerType.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
            return ControllerType.GetCustomAttributes(attributeType, inherit);
        }

        internal override IEnumerable<FilterAttribute> GetFilterAttributes(bool useCache) {
            if (useCache && GetType() == typeof(ReflectedAsyncControllerDescriptor)) {
                // Do not look at cache in types derived from this type because they might incorrectly implement GetCustomAttributes
                return ReflectedAttributeCache.GetTypeFilterAttributes(ControllerType);
            }
            return base.GetFilterAttributes(useCache);
        }

        public override bool IsDefined(Type attributeType, bool inherit) {
            return ControllerType.IsDefined(attributeType, inherit);
        }

    }
}
