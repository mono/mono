//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.IdentityModel.Selectors;
    using System.ServiceModel;
    using System.ServiceModel.Security.Tokens;

    class SupportingTokenAuthenticatorSpecification
    {
        SecurityTokenAttachmentMode tokenAttachmentMode;
        SecurityTokenAuthenticator tokenAuthenticator;
        SecurityTokenResolver tokenResolver;
        SecurityTokenParameters tokenParameters;
        bool isTokenOptional;

        public SupportingTokenAuthenticatorSpecification(SecurityTokenAuthenticator tokenAuthenticator, SecurityTokenResolver securityTokenResolver, SecurityTokenAttachmentMode attachmentMode, SecurityTokenParameters tokenParameters)
            : this(tokenAuthenticator, securityTokenResolver, attachmentMode, tokenParameters, false)
        {
        }

        internal SupportingTokenAuthenticatorSpecification(SecurityTokenAuthenticator tokenAuthenticator, SecurityTokenResolver securityTokenResolver, SecurityTokenAttachmentMode attachmentMode, SecurityTokenParameters tokenParameters, bool isTokenOptional)
        {
            if (tokenAuthenticator == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenAuthenticator");
            }
            
            SecurityTokenAttachmentModeHelper.Validate(attachmentMode);

            if (tokenParameters == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("tokenParameters");
            }
            this.tokenAuthenticator = tokenAuthenticator;
            this.tokenResolver = securityTokenResolver;
            this.tokenAttachmentMode = attachmentMode;
            this.tokenParameters = tokenParameters;
            this.isTokenOptional = isTokenOptional;
        }

        public SecurityTokenAuthenticator TokenAuthenticator
        {
            get { return this.tokenAuthenticator; }
        }

        public SecurityTokenResolver TokenResolver
        {
            get { return this.tokenResolver; }
        }

        public SecurityTokenAttachmentMode SecurityTokenAttachmentMode
        {
            get { return this.tokenAttachmentMode; }
        }

        public SecurityTokenParameters TokenParameters
        {
            get { return this.tokenParameters; }
        }

        internal bool IsTokenOptional
        {
            get { return this.isTokenOptional; }
            set { this.isTokenOptional = value; }
        }
    }
}
