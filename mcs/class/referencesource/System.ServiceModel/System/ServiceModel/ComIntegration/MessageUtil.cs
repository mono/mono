//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.Security.Permissions;
    using System.Security.Principal;    
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.Transactions;
    using System.ServiceModel.Transactions;
    using System.ServiceModel.Diagnostics;
    using System.EnterpriseServices;

    static class MessageUtil
    {
        public static WindowsIdentity GetMessageIdentity(Message message)
        {
            WindowsIdentity callerIdentity = null;

            SecurityMessageProperty securityProp;
            securityProp = message.Properties.Security;
            if (securityProp != null)
            {
                ServiceSecurityContext context;
                context = securityProp.ServiceSecurityContext;
                if (context != null)
                {
                    if (context.WindowsIdentity == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.RequiresWindowsSecurity());
                    }

                    callerIdentity = context.WindowsIdentity;
                }
            }

            if ((callerIdentity == null) || (callerIdentity.IsAnonymous))
            {
                // No security, no identity, must be anonymous.
                callerIdentity = SecurityUtils.GetAnonymousIdentity();
            }

            return callerIdentity;
        }

        // The demand is not added now (in 4.5), to avoid a breaking change. To be considered in the next version.
        /*
        [PermissionSet(SecurityAction.Demand, Unrestricted = true)] // because we call code from a non-APTCA assembly; transactions are not supported in partial trust, so customers should not be broken by this demand
        */
        public static Transaction GetMessageTransaction(Message message)
        {
            ServiceConfig serviceConfig = new ServiceConfig();
            serviceConfig.Transaction = TransactionOption.Disabled;
            ServiceDomain.Enter( serviceConfig );
            try
            {
                return TransactionMessageProperty.TryGetTransaction(message);
            }
            finally
            {
                ServiceDomain.Leave();
            }
        }
    }
}
