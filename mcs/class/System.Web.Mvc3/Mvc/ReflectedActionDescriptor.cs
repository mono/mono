namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc.Resources;

    public class ReflectedActionDescriptor : ActionDescriptor {

        private readonly string _actionName;
        private readonly ControllerDescriptor _controllerDescriptor;
        private ParameterDescriptor[] _parametersCache;
        private readonly Lazy<string> _uniqueId;

        public ReflectedActionDescriptor(MethodInfo methodInfo, string actionName, ControllerDescriptor controllerDescriptor)
            : this(methodInfo, actionName, controllerDescriptor, true /* validateMethod */) {
        }

        internal ReflectedActionDescriptor(MethodInfo methodInfo, string actionName, ControllerDescriptor controllerDescriptor, bool validateMethod) {
            if (methodInfo == null) {
                throw new ArgumentNullException("methodInfo");
            }
            if (String.IsNullOrEmpty(actionName)) {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "actionName");
            }
            if (controllerDescriptor == null) {
                throw new ArgumentNullException("controllerDescriptor");
            }

            if (validateMethod) {
                string failedMessage = VerifyActionMethodIsCallable(methodInfo);
                if (failedMessage != null) {
                    throw new ArgumentException(failedMessage, "methodInfo");
                }
            }

            MethodInfo = methodInfo;
            _actionName = actionName;
            _controllerDescriptor = controllerDescriptor;
            _uniqueId = new Lazy<string>(CreateUniqueId);
        }

        public override string ActionName {
            get {
                return _actionName;
            }
        }

        public override ControllerDescriptor ControllerDescriptor {
            get {
                return _controllerDescriptor;
            }
        }

        public MethodInfo MethodInfo {
            get;
            private set;
        }

        public override string UniqueId {
            get {
                return _uniqueId.Value;
            }
        }

        private string CreateUniqueId() {
            return base.UniqueId + DescriptorUtil.CreateUniqueId(MethodInfo);
        }

        public override object Execute(ControllerContext controllerContext, IDictionary<string, object> parameters) {
            if (controllerContext == null) {
                throw new ArgumentNullException("controllerContext");
            }
            if (parameters == null) {
                throw new ArgumentNullException("parameters");
            }

            ParameterInfo[] parameterInfos = MethodInfo.GetParameters();
            var rawParameterValues = from parameterInfo in parameterInfos
                                     select ExtractParameterFromDictionary(parameterInfo, parameters, MethodInfo);
            object[] parametersArray = rawParameterValues.ToArray();

            ActionMethodDispatcher dispatcher = DispatcherCache.GetDispatcher(MethodInfo);
            object actionReturnValue = dispatcher.Execute(controllerContext.Controller, parametersArray);
            return actionReturnValue;
        }

        public override object[] GetCustomAttributes(bool inherit) {
            return MethodInfo.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
            return MethodInfo.GetCustomAttributes(attributeType, inherit);
        }

        internal override IEnumerable<FilterAttribute> GetFilterAttributes(bool useCache) {
            if (useCache && GetType() == typeof(ReflectedActionDescriptor)) {
                // Do not look at cache in types derived from this type because they might incorrectly implement GetCustomAttributes
                return ReflectedAttributeCache.GetMethodFilterAttributes(MethodInfo);
            }
            return base.GetFilterAttributes(useCache);
        }

        public override ParameterDescriptor[] GetParameters() {
            ParameterDescriptor[] parameters = LazilyFetchParametersCollection();

            // need to clone array so that user modifications aren't accidentally stored
            return (ParameterDescriptor[])parameters.Clone();
        }

        public override ICollection<ActionSelector> GetSelectors() {
            ActionMethodSelectorAttribute[] attrs = (ActionMethodSelectorAttribute[])MethodInfo.GetCustomAttributes(typeof(ActionMethodSelectorAttribute), true /* inherit */);
            ActionSelector[] selectors = Array.ConvertAll(attrs, attr => (ActionSelector)(controllerContext => attr.IsValidForRequest(controllerContext, MethodInfo)));
            return selectors;
        }

        public override bool IsDefined(Type attributeType, bool inherit) {
            return MethodInfo.IsDefined(attributeType, inherit);
        }

        private ParameterDescriptor[] LazilyFetchParametersCollection() {
            return DescriptorUtil.LazilyFetchOrCreateDescriptors<ParameterInfo, ParameterDescriptor>(
                ref _parametersCache /* cacheLocation */,
                MethodInfo.GetParameters /* initializer */,
                parameterInfo => new ReflectedParameterDescriptor(parameterInfo, this) /* converter */);
        }

        internal static ReflectedActionDescriptor TryCreateDescriptor(MethodInfo methodInfo, string name, ControllerDescriptor controllerDescriptor) {
            ReflectedActionDescriptor descriptor = new ReflectedActionDescriptor(methodInfo, name, controllerDescriptor, false /* validateMethod */);
            string failedMessage = VerifyActionMethodIsCallable(methodInfo);
            return (failedMessage == null) ? descriptor : null;
        }
    }
}
