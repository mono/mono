//------------------------------------------------------------------------------
// <copyright file="IServiceObjectContainer.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */
namespace System.ComponentModel.Design {
    using System;

    /// <devdoc>
    ///     This interface provides a container for services.  A service container
    ///     is, by definition, a service provider.  In addition to providing services
    ///     it also provides a mechanism for adding and removing services.
    /// </devdoc>
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface IServiceContainer : IServiceProvider {

        /// <devdoc>
        ///     Adds the given service to the service container.
        /// </devdoc>
        void AddService(Type serviceType, object serviceInstance);

        /// <devdoc>
        ///     Adds the given service to the service container.
        /// </devdoc>
        void AddService(Type serviceType, object serviceInstance, bool promote);

        /// <devdoc>
        ///     Adds the given service to the service container.
        /// </devdoc>
        void AddService(Type serviceType, ServiceCreatorCallback callback);

        /// <devdoc>
        ///     Adds the given service to the service container.
        /// </devdoc>
        void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote);

        /// <devdoc>
        ///     Removes the given service type from the service container.
        /// </devdoc>
        void RemoveService(Type serviceType);

        /// <devdoc>
        ///     Removes the given service type from the service container.
        /// </devdoc>
        void RemoveService(Type serviceType, bool promote);
   }
}

