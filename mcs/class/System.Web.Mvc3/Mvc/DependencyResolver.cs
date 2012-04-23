namespace System.Web.Mvc {
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Web.Mvc.Resources;

    public class DependencyResolver {
        // Static accessors

        private static DependencyResolver _instance = new DependencyResolver();

        public static IDependencyResolver Current {
            get {
                return _instance.InnerCurrent;
            }
        }

        public static void SetResolver(IDependencyResolver resolver) {
            _instance.InnerSetResolver(resolver);
        }

        public static void SetResolver(object commonServiceLocator) {
            _instance.InnerSetResolver(commonServiceLocator);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types.")]
        public static void SetResolver(Func<Type, object> getService, Func<Type, IEnumerable<object>> getServices) {
            _instance.InnerSetResolver(getService, getServices);
        }

        // Instance implementation (for testing purposes)

        private IDependencyResolver _current = new DefaultDependencyResolver();

        public IDependencyResolver InnerCurrent {
            get {
                return _current;
            }
        }

        public void InnerSetResolver(IDependencyResolver resolver) {
            if (resolver == null) {
                throw new ArgumentNullException("resolver");
            }

            _current = resolver;
        }

        public void InnerSetResolver(object commonServiceLocator) {
            if (commonServiceLocator == null) {
                throw new ArgumentNullException("commonServiceLocator");
            }

            Type locatorType = commonServiceLocator.GetType();
            MethodInfo getInstance = locatorType.GetMethod("GetInstance", new[] { typeof(Type) });
            MethodInfo getInstances = locatorType.GetMethod("GetAllInstances", new[] { typeof(Type) });

            if (getInstance == null ||
                getInstance.ReturnType != typeof(object) ||
                getInstances == null ||
                getInstances.ReturnType != typeof(IEnumerable<object>)) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.DependencyResolver_DoesNotImplementICommonServiceLocator,
                        locatorType.FullName
                    ),
                    "commonServiceLocator"
                );
            }

            var getService = (Func<Type, object>)Delegate.CreateDelegate(typeof(Func<Type, object>), commonServiceLocator, getInstance);
            var getServices = (Func<Type, IEnumerable<object>>)Delegate.CreateDelegate(typeof(Func<Type, IEnumerable<object>>), commonServiceLocator, getInstances);

            _current = new DelegateBasedDependencyResolver(getService, getServices);
        }

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "This is an appropriate nesting of generic types.")]
        public void InnerSetResolver(Func<Type, object> getService, Func<Type, IEnumerable<object>> getServices) {
            if (getService == null) {
                throw new ArgumentNullException("getService");
            }
            if (getServices == null) {
                throw new ArgumentNullException("getServices");
            }

            _current = new DelegateBasedDependencyResolver(getService, getServices);
        }

        // Helper classes

        private class DelegateBasedDependencyResolver : IDependencyResolver {
            Func<Type, object> _getService;
            Func<Type, IEnumerable<object>> _getServices;

            public DelegateBasedDependencyResolver(Func<Type, object> getService, Func<Type, IEnumerable<object>> getServices) {
                _getService = getService;
                _getServices = getServices;
            }

            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method might throw exceptions whose type we cannot strongly link against; namely, ActivationException from common service locator")]
            public object GetService(Type type) {
                try {
                    return _getService.Invoke(type);
                }
                catch {
                    return null;
                }
            }

            public IEnumerable<object> GetServices(Type type) {
                return _getServices(type);
            }
        }

        private class DefaultDependencyResolver : IDependencyResolver {
            [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "This method might throw exceptions whose type we cannot strongly link against; namely, ActivationException from common service locator")]
            public object GetService(Type serviceType) {
                try {
                    return Activator.CreateInstance(serviceType);
                }
                catch {
                    return null;
                }
            }

            public IEnumerable<object> GetServices(Type serviceType) {
                return Enumerable.Empty<object>();
            }
        }
    }
}
