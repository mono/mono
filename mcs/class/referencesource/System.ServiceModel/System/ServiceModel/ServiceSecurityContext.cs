//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Security;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Security.Principal;

    public class ServiceSecurityContext
    {
        static ServiceSecurityContext anonymous;
        ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies;
        AuthorizationContext authorizationContext;
        IIdentity primaryIdentity;
        Claim identityClaim;
        WindowsIdentity windowsIdentity;

        // Perf: delay created authorizationContext using forward chain.
        public ServiceSecurityContext(ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            if (authorizationPolicies == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authorizationPolicies");
            }
            this.authorizationContext = null;
            this.authorizationPolicies = authorizationPolicies;
        }

        public ServiceSecurityContext(AuthorizationContext authorizationContext)
            : this(authorizationContext, EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance)
        {
        }

        public ServiceSecurityContext(AuthorizationContext authorizationContext, ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies)
        {
            if (authorizationContext == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authorizationContext");
            }
            if (authorizationPolicies == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("authorizationPolicies");
            }
            this.authorizationContext = authorizationContext;
            this.authorizationPolicies = authorizationPolicies;
        }

        public static ServiceSecurityContext Anonymous
        {
            get
            {
                if (anonymous == null)
                {
                    anonymous = new ServiceSecurityContext(EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance);
                }
                return anonymous;
            }
        }

        public static ServiceSecurityContext Current
        {
            get
            {
                ServiceSecurityContext result = null;

                OperationContext operationContext = OperationContext.Current;
                if (operationContext != null)
                {
                    MessageProperties properties = operationContext.IncomingMessageProperties;
                    if (properties != null)
                    {
                        SecurityMessageProperty security = properties.Security;
                        if (security != null)
                        {
                            result = security.ServiceSecurityContext;
                        }
                    }
                }

                return result;
            }
        }

        public bool IsAnonymous
        {
            get
            {
                return this == Anonymous || this.IdentityClaim == null;
            }
        }

        internal Claim IdentityClaim
        {
            get
            {
                if (this.identityClaim == null)
                {
                    this.identityClaim = SecurityUtils.GetPrimaryIdentityClaim(this.AuthorizationContext);
                }
                return this.identityClaim;
            }
        }

        public IIdentity PrimaryIdentity
        {
            get
            {
                if (this.primaryIdentity == null)
                {
                    IIdentity primaryIdentity = null;
                    IList<IIdentity> identities = GetIdentities();
                    // Multiple Identities is treated as anonymous
                    if (identities != null && identities.Count == 1)
                    {
                        primaryIdentity = identities[0];
                    }

                    this.primaryIdentity = primaryIdentity ?? SecurityUtils.AnonymousIdentity;
                }
                return this.primaryIdentity;
            }
        }

        public WindowsIdentity WindowsIdentity
        {
            get
            {
                if (this.windowsIdentity == null)
                {
                    WindowsIdentity windowsIdentity = null;
                    IList<IIdentity> identities = GetIdentities();
                    if (identities != null)
                    {
                        for (int i = 0; i < identities.Count; ++i)
                        {
                            WindowsIdentity identity = identities[i] as WindowsIdentity;
                            if (identity != null)
                            {
                                // Multiple Identities is treated as anonymous
                                if (windowsIdentity != null)
                                {
                                    windowsIdentity = WindowsIdentity.GetAnonymous();
                                    break;
                                }
                                windowsIdentity = identity;
                            }
                        }
                    }

                    this.windowsIdentity = windowsIdentity ?? WindowsIdentity.GetAnonymous();
                }
                return this.windowsIdentity;
            }
        }

        public ReadOnlyCollection<IAuthorizationPolicy> AuthorizationPolicies
        {
            get
            {
                return this.authorizationPolicies;
            }
            set
            {
                this.authorizationPolicies = value;
            }
        }

        public AuthorizationContext AuthorizationContext
        {
            get
            {
                if (this.authorizationContext == null)
                {
                    this.authorizationContext = AuthorizationContext.CreateDefaultAuthorizationContext(this.authorizationPolicies);
                }
                return this.authorizationContext;
            }
        }

        IList<IIdentity> GetIdentities()
        {
            object identities;
            AuthorizationContext authContext = this.AuthorizationContext;
            if (authContext != null && authContext.Properties.TryGetValue(SecurityUtils.Identities, out identities))
            {
                return identities as IList<IIdentity>;
            }
            return null;
        }
    }
}
