//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Policy;
    using System.ServiceModel;
    using System.IdentityModel.Tokens;
    using System.Runtime.Serialization;
    using System.Collections.ObjectModel;

    public sealed class SecureConversationServiceCredential
    {
        static readonly SecurityStateEncoder defaultSecurityStateEncoder = new DataProtectionSecurityStateEncoder();
        SecurityStateEncoder securityStateEncoder;
        Collection<Type> securityContextClaimTypes;
        bool isReadOnly;

        internal SecureConversationServiceCredential()
        {
            this.securityStateEncoder = defaultSecurityStateEncoder;
            securityContextClaimTypes = new Collection<Type>();
            SamlAssertion.AddSamlClaimTypes(securityContextClaimTypes);
        }

        internal SecureConversationServiceCredential(SecureConversationServiceCredential other)
        {
            this.securityStateEncoder = other.securityStateEncoder;
            this.securityContextClaimTypes = new Collection<Type>();
            for (int i = 0; i < other.securityContextClaimTypes.Count; ++i)
            {
                this.securityContextClaimTypes.Add(other.securityContextClaimTypes[i]);
            }
            this.isReadOnly = other.isReadOnly;
        }

        public SecurityStateEncoder SecurityStateEncoder
        {
            get 
            { 
                return this.securityStateEncoder; 
            }
            set 
            {
                ThrowIfImmutable();
                this.securityStateEncoder = value; 
            }
        }

        public Collection<Type> SecurityContextClaimTypes
        {
            get { return this.securityContextClaimTypes; }
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
