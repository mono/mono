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
    using System.Web.Mvc.Resources;

    public class ControllerBuilder {

        private Func<IControllerFactory> _factoryThunk;
        private static ControllerBuilder _instance = new ControllerBuilder();
        private HashSet<string> _namespaces = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public ControllerBuilder() {
            SetControllerFactory(new DefaultControllerFactory() {
                ControllerBuilder = this
            });
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

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Calling method multiple times might return different objects.")]
        public IControllerFactory GetControllerFactory() {
            IControllerFactory controllerFactoryInstance = _factoryThunk();
            return controllerFactoryInstance;
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
                        CultureInfo.CurrentUICulture,
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
                            CultureInfo.CurrentUICulture,
                            MvcResources.ControllerBuilder_ErrorCreatingControllerFactory,
                            controllerFactoryType),
                        ex);
                }
            };
        }
    }
}
