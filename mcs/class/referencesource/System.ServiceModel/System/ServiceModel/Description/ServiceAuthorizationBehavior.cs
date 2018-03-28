//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IdentityModel.Policy;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Web.Security;

    public sealed class ServiceAuthorizationBehavior : IServiceBehavior
    {
        internal const bool DefaultImpersonateCallerForAllOperations = false;
        internal const bool DefaultImpersonateOnSerializingReply = false;
        internal const PrincipalPermissionMode DefaultPrincipalPermissionMode = PrincipalPermissionMode.UseWindowsGroups;

        bool impersonateCallerForAllOperations;
        bool impersonateOnSerializingReply;
        ReadOnlyCollection<IAuthorizationPolicy> externalAuthorizationPolicies;
        ServiceAuthorizationManager serviceAuthorizationManager;
        PrincipalPermissionMode principalPermissionMode;
        object roleProvider;
        bool isExternalPoliciesSet;
        bool isAuthorizationManagerSet;
        bool isReadOnly;
     
        public ServiceAuthorizationBehavior()
        {
            this.impersonateCallerForAllOperations = DefaultImpersonateCallerForAllOperations;
            this.impersonateOnSerializingReply = DefaultImpersonateOnSerializingReply;
            this.principalPermissionMode = DefaultPrincipalPermissionMode;
        }

        ServiceAuthorizationBehavior(ServiceAuthorizationBehavior other)
        {
            this.impersonateCallerForAllOperations = other.impersonateCallerForAllOperations;
            this.impersonateOnSerializingReply = other.impersonateOnSerializingReply;
            this.principalPermissionMode = other.principalPermissionMode;
            this.roleProvider = other.roleProvider;
            this.isExternalPoliciesSet = other.isExternalPoliciesSet;
            this.isAuthorizationManagerSet = other.isAuthorizationManagerSet;
         
            if (other.isExternalPoliciesSet || other.isAuthorizationManagerSet)
            {
                CopyAuthorizationPoliciesAndManager(other);
            }
            this.isReadOnly = other.isReadOnly;
        }

        public ReadOnlyCollection<IAuthorizationPolicy> ExternalAuthorizationPolicies
        {
            get
            {
                return this.externalAuthorizationPolicies;
            }
            set
            {
                ThrowIfImmutable();
                this.isExternalPoliciesSet = true;
                this.externalAuthorizationPolicies = value;
            }
        }

        public bool ShouldSerializeExternalAuthorizationPolicies()
        {
            return this.isExternalPoliciesSet;
        }

        public ServiceAuthorizationManager ServiceAuthorizationManager
        {
            get
            {
                return this.serviceAuthorizationManager;
            }
            set
            {
                ThrowIfImmutable();
                this.isAuthorizationManagerSet = true;
                this.serviceAuthorizationManager = value;
            }
        }

        public bool ShouldSerializeServiceAuthorizationManager()
        {
            return this.isAuthorizationManagerSet;
        }

        [DefaultValue(DefaultPrincipalPermissionMode)]
        public PrincipalPermissionMode PrincipalPermissionMode
        {
            get
            {
                return this.principalPermissionMode;
            }
            set
            {
                if (!PrincipalPermissionModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                ThrowIfImmutable();
                this.principalPermissionMode = value;
            }
        }

        [DefaultValue(null)]
        public RoleProvider RoleProvider
        {
            get
            {
                return (RoleProvider)this.roleProvider;
            }
            set
            {
                ThrowIfImmutable();
                this.roleProvider = value;
            }
        }

        [DefaultValue(DefaultImpersonateCallerForAllOperations)]
        public bool ImpersonateCallerForAllOperations
        {
            get
            {
                return this.impersonateCallerForAllOperations;
            }
            set
            {
                ThrowIfImmutable();
                this.impersonateCallerForAllOperations = value;
            }
        }


        [DefaultValue(DefaultImpersonateOnSerializingReply)]
        public bool ImpersonateOnSerializingReply
        {
            get
            {
                return this.impersonateOnSerializingReply;
            }
            set
            {
                ThrowIfImmutable();
                this.impersonateOnSerializingReply = value;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void ApplyAuthorizationPoliciesAndManager(DispatchRuntime behavior)
        {
            if (this.externalAuthorizationPolicies != null)
            {
                behavior.ExternalAuthorizationPolicies = this.externalAuthorizationPolicies;
            }
            if (this.serviceAuthorizationManager != null)
            {
                behavior.ServiceAuthorizationManager = this.serviceAuthorizationManager;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void CopyAuthorizationPoliciesAndManager(ServiceAuthorizationBehavior other)
        {
            this.externalAuthorizationPolicies = other.externalAuthorizationPolicies;
            this.serviceAuthorizationManager = other.serviceAuthorizationManager;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void ApplyRoleProvider(DispatchRuntime dispatchRuntime)
        {
            dispatchRuntime.RoleProvider = (RoleProvider)this.roleProvider;
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("description"));
            }
            if (serviceHostBase == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceHostBase"));
            }

            for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                if (channelDispatcher != null && !ServiceMetadataBehavior.IsHttpGetMetadataDispatcher(description, channelDispatcher))
                {
                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        DispatchRuntime behavior = endpointDispatcher.DispatchRuntime;
                        behavior.PrincipalPermissionMode = this.principalPermissionMode;
                        if (!endpointDispatcher.IsSystemEndpoint)
                        {
                            behavior.ImpersonateCallerForAllOperations = this.impersonateCallerForAllOperations;
                            behavior.ImpersonateOnSerializingReply = this.impersonateOnSerializingReply;
                        }
                        if (this.roleProvider != null)
                        {
                            ApplyRoleProvider(behavior);
                        }
                        if (this.isAuthorizationManagerSet || this.isExternalPoliciesSet)
                        {
                            ApplyAuthorizationPoliciesAndManager(behavior);
                        }
                    }
                }
            }
        }

        internal ServiceAuthorizationBehavior Clone()
        {
            return new ServiceAuthorizationBehavior(this);
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
        }

        void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));
            }
        }
    }
}
