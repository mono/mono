//------------------------------------------------------------------------------
// <copyright file="RolesService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.ApplicationServices {
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Configuration;
    using System.Runtime.Serialization;
    using System.Web;
    using System.Web.Security;
    using System.Web.Configuration;
    using System.Web.Management;
    using System.Web.Resources;
    using System.Security.Principal;
    using System.Web.Hosting;
    using System.Threading;
    using System.Configuration.Provider;

    [
    AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required),
    ServiceContract(Namespace = "http://asp.net/ApplicationServices/v200"),
    ServiceBehavior(Namespace="http://asp.net/ApplicationServices/v200", InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)
    ]
    public class RoleService {
        private static object _selectingProviderEventHandlerLock = new object();
        private static EventHandler<SelectingProviderEventArgs> _selectingProvider;

        public static event EventHandler<SelectingProviderEventArgs> SelectingProvider {
            add {
                lock (_selectingProviderEventHandlerLock) {
                    _selectingProvider += value;
                }
            }
            remove {
                lock (_selectingProviderEventHandlerLock) {
                    _selectingProvider -= value;
                }
            }
        }

        private static void EnsureProviderEnabled() {
            if (!Roles.Enabled) {
                throw new ProviderException(AtlasWeb.RoleService_RolesFeatureNotEnabled);
            }
        }

        private RoleProvider GetRoleProvider(IPrincipal user) {
            string providerName = Roles.Provider.Name;
            SelectingProviderEventArgs args = new SelectingProviderEventArgs(user, providerName);
            OnSelectingProvider(args);
            providerName = args.ProviderName;
            RoleProvider provider = Roles.Providers[providerName];
            if (provider == null) {
                throw new ProviderException(AtlasWeb.RoleService_RoleProviderNotFound);
            }
            return provider;
        }

        [OperationContract]
        public string[] GetRolesForCurrentUser() {
            try {
                ApplicationServiceHelper.EnsureRoleServiceEnabled();
                EnsureProviderEnabled();

                IPrincipal user = ApplicationServiceHelper.GetCurrentUser(HttpContext.Current);
                string username = ApplicationServiceHelper.GetUserName(user);
                RoleProvider provider = GetRoleProvider(user);

                return provider.GetRolesForUser(username);
            }
            catch (Exception e) {
                LogException(e);
                throw;
            }
        }

        [OperationContract]
        public bool IsCurrentUserInRole(string role) {
            if (role == null) {
                throw new ArgumentNullException("role");
            }

            try {
                ApplicationServiceHelper.EnsureRoleServiceEnabled();
                EnsureProviderEnabled();

                IPrincipal user = ApplicationServiceHelper.GetCurrentUser(HttpContext.Current);
                string username = ApplicationServiceHelper.GetUserName(user);
                RoleProvider provider = GetRoleProvider(user);

                return provider.IsUserInRole(username, role);
            }
            catch (Exception e) {
                LogException(e);
                throw;
            }
        }

        private void LogException(Exception e) {
            WebServiceErrorEvent errorevent = new WebServiceErrorEvent(AtlasWeb.UnhandledExceptionEventLogMessage, this, e);
            errorevent.Raise();
        }

        private void OnSelectingProvider(SelectingProviderEventArgs e) {
            EventHandler<SelectingProviderEventArgs> handler = _selectingProvider;
            if (handler != null) {
                handler(this, e);
            }
        }

        public RoleService() {
        }
    }
}
