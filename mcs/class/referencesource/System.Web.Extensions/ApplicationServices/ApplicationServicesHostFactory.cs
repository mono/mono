//------------------------------------------------------------------------------
// <copyright file="ApplicationServicesHostFactory.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace System.Web.ApplicationServices {

    public class ApplicationServicesHostFactory : ServiceHostFactory {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses) {
            ServiceHost host = null;
            if (typeof(ProfileService).Equals(serviceType)) {
                host = new ServiceHost(new ProfileService(), baseAddresses);
            }
            else if (typeof(RoleService).Equals(serviceType)) {
                host = new ServiceHost(new RoleService(), baseAddresses);
            }
            else if (typeof(AuthenticationService).Equals(serviceType)) {
                host = new ServiceHost(new AuthenticationService(), baseAddresses);
            }
            else {
                host = base.CreateServiceHost(serviceType, baseAddresses);
            }
            return host;
        }
    }
}
