//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Description
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;

    public sealed class ServiceSecurityAuditBehavior : IServiceBehavior
    {
        internal const AuditLogLocation defaultAuditLogLocation = AuditLogLocation.Default;
        internal const bool defaultSuppressAuditFailure = true;
        internal const AuditLevel defaultServiceAuthorizationAuditLevel = AuditLevel.None;
        internal const AuditLevel defaultMessageAuthenticationAuditLevel = AuditLevel.None;

        AuditLogLocation auditLogLocation;
        bool suppressAuditFailure;
        AuditLevel serviceAuthorizationAuditLevel;
        AuditLevel messageAuthenticationAuditLevel;

        public ServiceSecurityAuditBehavior()
        {
            this.auditLogLocation = ServiceSecurityAuditBehavior.defaultAuditLogLocation;
            this.suppressAuditFailure = ServiceSecurityAuditBehavior.defaultSuppressAuditFailure;
            this.serviceAuthorizationAuditLevel = ServiceSecurityAuditBehavior.defaultServiceAuthorizationAuditLevel;
            this.messageAuthenticationAuditLevel = ServiceSecurityAuditBehavior.defaultMessageAuthenticationAuditLevel;
        }

        ServiceSecurityAuditBehavior(ServiceSecurityAuditBehavior behavior)
        {
            this.auditLogLocation = behavior.auditLogLocation;
            this.suppressAuditFailure = behavior.suppressAuditFailure;
            this.serviceAuthorizationAuditLevel = behavior.serviceAuthorizationAuditLevel;
            this.messageAuthenticationAuditLevel = behavior.messageAuthenticationAuditLevel;
        }

        public AuditLogLocation AuditLogLocation
        {
            get
            {
                return this.auditLogLocation;
            }
            set
            {
                if (!AuditLogLocationHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));

                this.auditLogLocation = value;
            }
        }

        public bool SuppressAuditFailure
        {
            get
            {
                return this.suppressAuditFailure;
            }
            set
            {
                this.suppressAuditFailure = value;
            }
        }

        public AuditLevel ServiceAuthorizationAuditLevel
        {
            get
            {
                return this.serviceAuthorizationAuditLevel;
            }
            set
            {
                if (!AuditLevelHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));

                this.serviceAuthorizationAuditLevel = value;
            }
        }

        public AuditLevel MessageAuthenticationAuditLevel
        {
            get
            {
                return this.messageAuthenticationAuditLevel;
            }
            set
            {
                if (!AuditLevelHelper.IsDefined(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));

                this.messageAuthenticationAuditLevel = value;
            }
        }

        internal ServiceSecurityAuditBehavior Clone()
        {
            return new ServiceSecurityAuditBehavior(this);
        }

        void IServiceBehavior.Validate(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
        }

        void IServiceBehavior.AddBindingParameters(ServiceDescription description, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection parameters)
        {
            if (parameters == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("parameters"));

            parameters.Add(this);
        }

        void IServiceBehavior.ApplyDispatchBehavior(ServiceDescription description, ServiceHostBase serviceHostBase)
        {
            if (description == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("description"));
            if (serviceHostBase == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("serviceHostBase"));

            for (int i = 0; i < serviceHostBase.ChannelDispatchers.Count; i++)
            {
                ChannelDispatcher channelDispatcher = serviceHostBase.ChannelDispatchers[i] as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints)
                    {
                        if (!endpointDispatcher.IsSystemEndpoint)
                        {
                            DispatchRuntime behavior = endpointDispatcher.DispatchRuntime;
                            behavior.SecurityAuditLogLocation = this.auditLogLocation;
                            behavior.SuppressAuditFailure = this.suppressAuditFailure;
                            behavior.ServiceAuthorizationAuditLevel = this.serviceAuthorizationAuditLevel;
                            behavior.MessageAuthenticationAuditLevel = this.messageAuthenticationAuditLevel;
                        }
                    }
                }
            }
        }
    }
}
