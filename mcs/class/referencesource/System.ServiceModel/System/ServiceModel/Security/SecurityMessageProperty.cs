//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;
    using System.ServiceModel.Security.Tokens;
    using System.Net.Security;

    public class SecurityMessageProperty : IMessageProperty, IDisposable
    {
        // This is the list of outgoing supporting tokens
        Collection<SupportingTokenSpecification> outgoingSupportingTokens;
        Collection<SupportingTokenSpecification> incomingSupportingTokens;
        SecurityTokenSpecification transportToken;
        SecurityTokenSpecification protectionToken;
        SecurityTokenSpecification initiatorToken;
        SecurityTokenSpecification recipientToken;

        ServiceSecurityContext securityContext;
        ReadOnlyCollection<IAuthorizationPolicy> externalAuthorizationPolicies;
        string senderIdPrefix = "_";
        bool disposed = false;

        public SecurityMessageProperty()
        {
            this.securityContext = ServiceSecurityContext.Anonymous;
        }

        public ServiceSecurityContext ServiceSecurityContext
        {
            get
            {
                ThrowIfDisposed();
                return this.securityContext;
            }
            set
            {
                ThrowIfDisposed();
                this.securityContext = value;
            }
        }

        public ReadOnlyCollection<IAuthorizationPolicy> ExternalAuthorizationPolicies
        {
            get
            {
                return this.externalAuthorizationPolicies;
            }
            set
            {
                this.externalAuthorizationPolicies = value;
            }
        }

        public SecurityTokenSpecification ProtectionToken
        {
            get
            {
                ThrowIfDisposed();
                return this.protectionToken;
            }
            set
            {
                ThrowIfDisposed();
                this.protectionToken = value;
            }
        }

        public SecurityTokenSpecification InitiatorToken
        {
            get
            {
                ThrowIfDisposed();
                return this.initiatorToken;
            }
            set
            {
                ThrowIfDisposed();
                this.initiatorToken = value;
            }
        }

        public SecurityTokenSpecification RecipientToken
        {
            get
            {
                ThrowIfDisposed();
                return this.recipientToken;
            }
            set
            {
                ThrowIfDisposed();
                this.recipientToken = value;
            }
        }

        public SecurityTokenSpecification TransportToken
        {
            get
            {
                ThrowIfDisposed();
                return this.transportToken;
            }
            set
            {
                ThrowIfDisposed();
                this.transportToken = value;
            }
        }


        public string SenderIdPrefix
        {
            get
            {
                return this.senderIdPrefix;
            }
            set
            {
                XmlHelper.ValidateIdPrefix(value);
                this.senderIdPrefix = value;
            }
        }

        public bool HasIncomingSupportingTokens
        {
            get
            {
                ThrowIfDisposed();
                return ((this.incomingSupportingTokens != null) && (this.incomingSupportingTokens.Count > 0));
            }
        }

        public Collection<SupportingTokenSpecification> IncomingSupportingTokens
        {
            get
            {
                ThrowIfDisposed();
                if (this.incomingSupportingTokens == null)
                {
                    this.incomingSupportingTokens = new Collection<SupportingTokenSpecification>();
                }
                return this.incomingSupportingTokens;
            }
        }

        public Collection<SupportingTokenSpecification> OutgoingSupportingTokens
        {
            get
            {
                if (this.outgoingSupportingTokens == null)
                {
                    this.outgoingSupportingTokens = new Collection<SupportingTokenSpecification>();
                }
                return this.outgoingSupportingTokens;
            }
        }

        internal bool HasOutgoingSupportingTokens
        {
            get
            {
                return ((this.outgoingSupportingTokens != null) && (this.outgoingSupportingTokens.Count > 0));
            }
        }

        public IMessageProperty CreateCopy()
        {
            ThrowIfDisposed();
            SecurityMessageProperty result = new SecurityMessageProperty();

            if (this.HasOutgoingSupportingTokens)
            {
                for (int i = 0; i < this.outgoingSupportingTokens.Count; ++i)
                {
                    result.OutgoingSupportingTokens.Add(this.outgoingSupportingTokens[i]);
                }
            }

            if (this.HasIncomingSupportingTokens)
            {
                for (int i = 0; i < this.incomingSupportingTokens.Count; ++i)
                {
                    result.IncomingSupportingTokens.Add(this.incomingSupportingTokens[i]);
                }
            }

            result.securityContext = this.securityContext;
            result.externalAuthorizationPolicies = this.externalAuthorizationPolicies;
            result.senderIdPrefix = this.senderIdPrefix;

            result.protectionToken = this.protectionToken;
            result.initiatorToken = this.initiatorToken;
            result.recipientToken = this.recipientToken;
            result.transportToken = this.transportToken;

            return result;
        }

        public static SecurityMessageProperty GetOrCreate(Message message)
        {
            if (message == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");

            SecurityMessageProperty result = null;
            if (message.Properties != null)
                result = message.Properties.Security;

            if (result == null)
            {
                result = new SecurityMessageProperty();
                message.Properties.Security = result;
            }

            return result;
        }

        void AddAuthorizationPolicies(SecurityTokenSpecification spec, Collection<IAuthorizationPolicy> policies)
        {
            if (spec != null && spec.SecurityTokenPolicies != null && spec.SecurityTokenPolicies.Count > 0)
            {
                for (int i = 0; i < spec.SecurityTokenPolicies.Count; ++i)
                {
                    policies.Add(spec.SecurityTokenPolicies[i]);
                }
            }
        }

        internal ReadOnlyCollection<IAuthorizationPolicy> GetInitiatorTokenAuthorizationPolicies()
        {
            return GetInitiatorTokenAuthorizationPolicies(true);
        }

        internal ReadOnlyCollection<IAuthorizationPolicy> GetInitiatorTokenAuthorizationPolicies(bool includeTransportToken)
        {
            return GetInitiatorTokenAuthorizationPolicies(includeTransportToken, null);
        }
        
        internal ReadOnlyCollection<IAuthorizationPolicy> GetInitiatorTokenAuthorizationPolicies(bool includeTransportToken, SecurityContextSecurityToken supportingSessionTokenToExclude)
        {
            // fast path
            if (!this.HasIncomingSupportingTokens)
            {
                if (this.transportToken != null && this.initiatorToken == null && this.protectionToken == null)
                {
                    if (includeTransportToken && this.transportToken.SecurityTokenPolicies != null)
                    {
                        return this.transportToken.SecurityTokenPolicies;
                    }
                    else
                    {
                        return EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
                    }
                }
                else if (this.transportToken == null && this.initiatorToken != null && this.protectionToken == null)
                {
                    return this.initiatorToken.SecurityTokenPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
                }
                else if (this.transportToken == null && this.initiatorToken == null && this.protectionToken != null)
                {
                    return this.protectionToken.SecurityTokenPolicies ?? EmptyReadOnlyCollection<IAuthorizationPolicy>.Instance;
                }
            }

            Collection<IAuthorizationPolicy> policies = new Collection<IAuthorizationPolicy>();
            if (includeTransportToken)
            {
                AddAuthorizationPolicies(this.transportToken, policies);
            }
            AddAuthorizationPolicies(this.initiatorToken, policies);
            AddAuthorizationPolicies(this.protectionToken, policies);
            if (this.HasIncomingSupportingTokens)
            {
                for (int i = 0; i < this.incomingSupportingTokens.Count; ++i)
                {
                    if (supportingSessionTokenToExclude != null)
                    {
                        SecurityContextSecurityToken sct = this.incomingSupportingTokens[i].SecurityToken as SecurityContextSecurityToken;
                        if (sct != null && sct.ContextId == supportingSessionTokenToExclude.ContextId)
                        {
                            continue;
                        }
                    }
                    SecurityTokenAttachmentMode attachmentMode = this.incomingSupportingTokens[i].SecurityTokenAttachmentMode;
                    // a safety net in case more attachment modes get added to the product without 
                    // reviewing this code.
                    if (attachmentMode == SecurityTokenAttachmentMode.Endorsing
                        || attachmentMode == SecurityTokenAttachmentMode.Signed
                        || attachmentMode == SecurityTokenAttachmentMode.SignedEncrypted
                        || attachmentMode == SecurityTokenAttachmentMode.SignedEndorsing)
                    {
                        AddAuthorizationPolicies(this.incomingSupportingTokens[i], policies);
                    }
                }
            }
            return new ReadOnlyCollection<IAuthorizationPolicy>(policies);
        }

        public void Dispose()
        {
            // do no-op for future V2
            if (!this.disposed)
            {
                this.disposed = true;
            }
        }

        void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().FullName));
            }
        }
    }
}
