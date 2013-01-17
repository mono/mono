namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.Mvc.Resources;

    public class ControllerBuilder {

        private Func<IControllerFactory> _factoryThunk = () => null;
        private static ControllerBuilder _instance = new ControllerBuilder();
        private HashSet<string> _namespaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private IResolver<IControllerFactory> _serviceResolver;

        public ControllerBuilder()
            : this(null) {
        }

        internal ControllerBuilder(IResolver<IControllerFactory> serviceResolver) {
            _serviceResolver = serviceResolver ?? new SingleServiceResolver<IControllerFactory>(
                () => _factoryThunk(),
                 new DefaultControllerFactory { ControllerBuilder = this },
                "ControllerBuilder.GetControllerFactory"
            );
        }

        public static ControllerBuilder Current {
            get {
                return _instance;
            }
        }

        public HashSet<string> DefaultNamespaces {
            get {
                return _namespaces;
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Calling method multiple times might return different objects.")]
        public IControllerFactory GetControllerFactory() {
            return _serviceResolver.Current;
        }

        public void SetControllerFactory(IControllerFactory controllerFactory) {
            if (controllerFactory == null) {
                throw new ArgumentNullException("controllerFactory");
            }

            _factoryThunk = () => controllerFactory;
        }

        public void SetControllerFactory(Type controllerFactoryType) {
            if (controllerFactoryType == null) {
                throw new ArgumentNullException("controllerFactoryType");
            }
            if (!typeof(IControllerFactory).IsAssignableFrom(controllerFactoryType)) {
                throw new ArgumentException(
                    String.Format(
                        CultureInfo.CurrentCulture,
                        MvcResources.ControllerBuilder_MissingIControllerFactory,
                        controllerFactoryType),
                    "controllerFactoryType");
            }

            _factoryThunk = delegate() {
                try {
                    return (IControllerFactory)Activator.CreateInstance(controllerFactoryType);
                }
                catch (Exception ex) {
                    throw new InvalidOperationException(
                        String.Format(
                            CultureInfo.CurrentCulture,
                            MvcResources.ControllerBuilder_ErrorCreatingControllerFactory,
                            controllerFactoryType),
                        ex);
                }
            };
        }
    }
}

