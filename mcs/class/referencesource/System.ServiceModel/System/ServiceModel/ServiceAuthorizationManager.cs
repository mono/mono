//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System.Collections.Generic;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IdentityModel.Policy;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Security;

    public class ServiceAuthorizationManager
    {
        // This is the API called by framework to perform CheckAccess.
        // The API is responsible for ...
        // 1) Evaluate all policies (Forward\Backward)
        // 2) Optionally wire up the resulting AuthorizationContext 
        //    to ServiceSecurityContext.
        // 3) An availability of message content to make an authoritive decision. 
        // 4) Return the authoritive decision true/false (allow/deny).
        public virtual bool CheckAccess(OperationContext operationContext, ref Message message)
        {
            return CheckAccess(operationContext);
        }

        public virtual bool CheckAccess(OperationContext operationContext)
        {
            if (operationContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("operationContext");
            }

            // default to forward-chaining implementation
            // 1) Get policies that will participate in chain process.
            //    We provide a safe default policies set below.
            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = GetAuthorizationPolicies(operationContext);

            // 2) Do forward chaining and wire the new ServiceSecurityContext
            operationContext.IncomingMessageProperties.Security.ServiceSecurityContext =
                new ServiceSecurityContext(authorizationPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance);

            // 3) Call the CheckAccessCore
            return CheckAccessCore(operationContext);
        }

        // Define the set of policies taking part in chaining.  We will provide
        // the safe default set (primary token + all supporting tokens except token with
        // with SecurityTokenAttachmentMode.Signed + transport token).  Implementor
        // can override and provide different selection of policies set.
        protected virtual ReadOnlyCollection<IAuthorizationPolicy> GetAuthorizationPolicies(OperationContext operationContext)
        {
            SecurityMessageProperty security = operationContext.IncomingMessageProperties.Security;
            if (security == null)
            {
                return EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }

            ReadOnlyCollection<IAuthorizationPolicy> externalPolicies = security.ExternalAuthorizationPolicies;
            if (security.ServiceSecurityContext == null)
            {
                return externalPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
            }

            ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = security.ServiceSecurityContext.AuthorizationPolicies;
            if (externalPolicies == null || externalPolicies.Count <= 0)
            {
                return authorizationPolicies;
            }

            // Combine 
            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>(authorizationPolicies);
            policies.AddRange(externalPolicies);
            return policies.AsReadOnly();
        }

        // Implementor overrides this API to make authoritive decision.
        // The AuthorizationContext in opContext is generally the result from forward chain.
        protected virtual bool CheckAccessCore(OperationContext operationContext)
        {
            return true;
        }
    }
}

