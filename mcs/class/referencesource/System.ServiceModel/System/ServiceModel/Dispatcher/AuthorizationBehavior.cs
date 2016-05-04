//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Dispatcher
{
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IdentityModel.Policy;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Diagnostics.Application;
    using System.ServiceModel.Security;

    sealed class AuthorizationBehavior
    {
        static ServiceAuthorizationManager DefaultServiceAuthorizationManager = new ServiceAuthorizationManager();

        ReadOnlyCollection<IAuthorizationPolicy> externalAuthorizationPolicies;
        ServiceAuthorizationManager serviceAuthorizationManager;
        AuditLogLocation auditLogLocation;
        bool suppressAuditFailure;
        AuditLevel serviceAuthorizationAuditLevel;

        AuthorizationBehavior() { }

        public void Authorize(ref MessageRpc rpc)
        {

            if (TD.DispatchMessageBeforeAuthorizationIsEnabled())
            {
                TD.DispatchMessageBeforeAuthorization(rpc.EventTraceActivity);
            }

            SecurityMessageProperty security = SecurityMessageProperty.GetOrCreate(rpc.Request);
            security.ExternalAuthorizationPolicies = this.externalAuthorizationPolicies;

            ServiceAuthorizationManager serviceAuthorizationManager = this.serviceAuthorizationManager ?? DefaultServiceAuthorizationManager;
            try
            {
                if (!serviceAuthorizationManager.CheckAccess(rpc.OperationContext, ref rpc.Request))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(CreateAccessDeniedFaultException());
                }
            }
            catch (Exception ex)
            {
                if (Fx.IsFatal(ex))
                {
                    throw;
                }
                if (PerformanceCounters.PerformanceCountersEnabled)
                {
                    PerformanceCounters.AuthorizationFailed(rpc.Operation.Name);
                }
                if (AuditLevel.Failure == (this.serviceAuthorizationAuditLevel & AuditLevel.Failure))
                {
                    try
                    {
                        string primaryIdentity;
                        string authContextId = null;
                        AuthorizationContext authContext = security.ServiceSecurityContext.AuthorizationContext;
                        if (authContext != null)
                        {
                            primaryIdentity = SecurityUtils.GetIdentityNamesFromContext(authContext);
                            authContextId = authContext.Id;
                        }
                        else
                        {
                            primaryIdentity = SecurityUtils.AnonymousIdentity.Name;
                            authContextId = "<null>";
                        }

                        SecurityAuditHelper.WriteServiceAuthorizationFailureEvent(this.auditLogLocation,
                            this.suppressAuditFailure, rpc.Request, rpc.Request.Headers.To, rpc.Request.Headers.Action,
                            primaryIdentity, authContextId,
                            serviceAuthorizationManager == DefaultServiceAuthorizationManager ? "<default>" : serviceAuthorizationManager.GetType().Name,
                            ex);
                    }
#pragma warning suppress 56500
                    catch (Exception auditException)
                    {
                        if (Fx.IsFatal(auditException))
                            throw;

                        DiagnosticUtility.TraceHandledException(auditException, TraceEventType.Error);
                    }
                }
                throw;
            }

            if (AuditLevel.Success == (this.serviceAuthorizationAuditLevel & AuditLevel.Success))
            {
                string primaryIdentity;
                string authContextId;
                AuthorizationContext authContext = security.ServiceSecurityContext.AuthorizationContext;
                if (authContext != null)
                {
                    primaryIdentity = SecurityUtils.GetIdentityNamesFromContext(authContext);
                    authContextId = authContext.Id;
                }
                else
                {
                    primaryIdentity = SecurityUtils.AnonymousIdentity.Name;
                    authContextId = "<null>";
                }

                SecurityAuditHelper.WriteServiceAuthorizationSuccessEvent(this.auditLogLocation,
                    this.suppressAuditFailure, rpc.Request, rpc.Request.Headers.To, rpc.Request.Headers.Action,
                    primaryIdentity, authContextId,
                    serviceAuthorizationManager == DefaultServiceAuthorizationManager ? "<default>" : serviceAuthorizationManager.GetType().Name);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static AuthorizationBehavior CreateAuthorizationBehavior(DispatchRuntime dispatch)
        {
            AuthorizationBehavior behavior = new AuthorizationBehavior();
            behavior.externalAuthorizationPolicies = dispatch.ExternalAuthorizationPolicies;
            behavior.serviceAuthorizationManager = dispatch.ServiceAuthorizationManager;
            behavior.auditLogLocation = dispatch.SecurityAuditLogLocation;
            behavior.suppressAuditFailure = dispatch.SuppressAuditFailure;
            behavior.serviceAuthorizationAuditLevel = dispatch.ServiceAuthorizationAuditLevel;
            return behavior;
        }

        public static AuthorizationBehavior TryCreate(DispatchRuntime dispatch)
        {
            if (dispatch == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("dispatch"));

            if (!dispatch.RequiresAuthorization)
                return null;

            return CreateAuthorizationBehavior(dispatch);
        }

        internal static Exception CreateAccessDeniedFaultException()
        {
            // always use default version?
            SecurityVersion wss = SecurityVersion.Default;
            FaultCode faultCode = FaultCode.CreateSenderFaultCode(wss.FailedAuthenticationFaultCode.Value, wss.HeaderNamespace.Value);
            FaultReason faultReason = new FaultReason(SR.GetString(SR.AccessDenied), CultureInfo.CurrentCulture);
            return new FaultException(faultReason, faultCode);
        }
    }
}
