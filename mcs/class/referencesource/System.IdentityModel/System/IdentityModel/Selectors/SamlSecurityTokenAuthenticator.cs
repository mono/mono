//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Selectors
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Tokens;
    using System.IdentityModel.Policy;
    using System.Security.Principal;
    using System.Security;

    public class SamlSecurityTokenAuthenticator : SecurityTokenAuthenticator
    {
        List<SecurityTokenAuthenticator> supportingAuthenticators;
        Collection<string> allowedAudienceUris;
        AudienceUriMode audienceUriMode;
        TimeSpan maxClockSkew;

        public SamlSecurityTokenAuthenticator(IList<SecurityTokenAuthenticator> supportingAuthenticators)
            : this(supportingAuthenticators, TimeSpan.Zero)
        { }

        public SamlSecurityTokenAuthenticator(IList<SecurityTokenAuthenticator> supportingAuthenticators, TimeSpan maxClockSkew)
        {

            if (supportingAuthenticators == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("supportingAuthenticators");

            this.supportingAuthenticators = new List<SecurityTokenAuthenticator>(supportingAuthenticators.Count);
            for (int i = 0; i < supportingAuthenticators.Count; ++i)
            {
                this.supportingAuthenticators.Add(supportingAuthenticators[i]);
            }

            this.maxClockSkew = maxClockSkew;
            this.audienceUriMode = AudienceUriMode.Always;
            this.allowedAudienceUris = new Collection<string>();
        }

        public AudienceUriMode AudienceUriMode
        {
            get { return this.audienceUriMode; }
            set
            {
                AudienceUriModeValidationHelper.Validate(audienceUriMode);
                this.audienceUriMode = value;
            }
        }

        public IList<string> AllowedAudienceUris
        {
            get { return this.allowedAudienceUris; }
        }

        protected override bool CanValidateTokenCore(SecurityToken token)
        {
            return token is SamlSecurityToken;
        }

        protected override ReadOnlyCollection<IAuthorizationPolicy> ValidateTokenCore(SecurityToken token)
        {
            if (token == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");

            SamlSecurityToken samlToken = token as SamlSecurityToken;

            if (samlToken == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SamlTokenAuthenticatorCanOnlyProcessSamlTokens, token.GetType().ToString())));

            if (samlToken.Assertion.Signature == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SamlTokenMissingSignature)));

            if (!this.IsCurrentlyTimeEffective(samlToken))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLTokenTimeInvalid, DateTime.UtcNow.ToUniversalTime(), samlToken.ValidFrom.ToString(), samlToken.ValidTo.ToString())));

            if (samlToken.Assertion.SigningToken == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SamlSigningTokenMissing)));

            // Build the Issuer ClaimSet for this Saml token.
            ClaimSet issuer = null;
            bool canBeValidated = false;
            for (int i = 0; i < this.supportingAuthenticators.Count; ++i)
            {
                canBeValidated = this.supportingAuthenticators[i].CanValidateToken(samlToken.Assertion.SigningToken);
                if (canBeValidated)
                    break;
            }
            if (!canBeValidated)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SamlInvalidSigningToken)));
            issuer = ResolveClaimSet(samlToken.Assertion.SigningToken) ?? ClaimSet.Anonymous;

            List<IAuthorizationPolicy> policies = new List<IAuthorizationPolicy>();
            for (int i = 0; i < samlToken.Assertion.Statements.Count; ++i)
            {
                policies.Add(samlToken.Assertion.Statements[i].CreatePolicy(issuer, this));
            }


            // Check AudienceUri if required
            // AudienceUriMode != Never - don't need to check can only be one of three
            // AudienceUriMode == Always
            // AudienceUriMode == BearerKey and there are no proof keys
            //
            if ((this.audienceUriMode == AudienceUriMode.Always)
             || (this.audienceUriMode == AudienceUriMode.BearerKeyOnly) && (samlToken.SecurityKeys.Count < 1))
            {
                // throws if not found.
                bool foundAudienceCondition = false;
                if (this.allowedAudienceUris == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAudienceUrisNotFound)));
                }

                for (int i = 0; i < samlToken.Assertion.Conditions.Conditions.Count; i++)
                {

                    SamlAudienceRestrictionCondition audienceCondition = samlToken.Assertion.Conditions.Conditions[i] as SamlAudienceRestrictionCondition;
                    if (audienceCondition == null)
                        continue;

                    foundAudienceCondition = true;
                    if (!ValidateAudienceRestriction(audienceCondition))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAudienceUriValidationFailed)));
                    }
                }

                if (!foundAudienceCondition)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new SecurityTokenException(SR.GetString(SR.SAMLAudienceUriValidationFailed)));
            }

            return policies.AsReadOnly();
        }

        protected virtual bool ValidateAudienceRestriction(SamlAudienceRestrictionCondition audienceRestrictionCondition)
        {
            for (int i = 0; i < audienceRestrictionCondition.Audiences.Count; i++)
            {
                if (audienceRestrictionCondition.Audiences[i] == null)
                    continue;

                for (int j = 0; j < this.allowedAudienceUris.Count; j++)
                {
                    if (StringComparer.Ordinal.Compare(audienceRestrictionCondition.Audiences[i].AbsoluteUri, this.allowedAudienceUris[j]) == 0)
                        return true;
                    else if (Uri.IsWellFormedUriString(this.allowedAudienceUris[j], UriKind.Absolute))
                    {
                        Uri uri = new Uri(this.allowedAudienceUris[j]);
                        if (audienceRestrictionCondition.Audiences[i].Equals(uri))
                            return true;
                    }
                }
            }

            return false;
        }

        public virtual ClaimSet ResolveClaimSet(SecurityToken token)
        {
            if (token == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");

            for (int i = 0; i < this.supportingAuthenticators.Count; ++i)
            {
                if (this.supportingAuthenticators[i].CanValidateToken(token))
                {
                    ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = this.supportingAuthenticators[i].ValidateToken(token);
                    AuthorizationContext authContext = AuthorizationContext.CreateDefaultAuthorizationContext(authorizationPolicies);
                    if (authContext.ClaimSets.Count > 0)
                    {
                        return authContext.ClaimSets[0];
                    }
                }
            }
            return null;
        }

        public virtual ClaimSet ResolveClaimSet(SecurityKeyIdentifier keyIdentifier)
        {
            if (keyIdentifier == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");

            RsaKeyIdentifierClause rsaKeyIdentifierClause;
            EncryptedKeyIdentifierClause encryptedKeyIdentifierClause;
            if (keyIdentifier.TryFind<RsaKeyIdentifierClause>(out rsaKeyIdentifierClause))
            {
                return new DefaultClaimSet(new Claim(ClaimTypes.Rsa, rsaKeyIdentifierClause.Rsa, Rights.PossessProperty));
            }
            else if (keyIdentifier.TryFind<EncryptedKeyIdentifierClause>(out encryptedKeyIdentifierClause))
            {
                return new DefaultClaimSet(Claim.CreateHashClaim(encryptedKeyIdentifierClause.GetBuffer()));
            }

            return null;
        }

        public virtual IIdentity ResolveIdentity(SecurityToken token)
        {
            if (token == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("token");

            for (int i = 0; i < this.supportingAuthenticators.Count; ++i)
            {
                if (this.supportingAuthenticators[i].CanValidateToken(token))
                {
                    ReadOnlyCollection<IAuthorizationPolicy> authorizationPolicies = this.supportingAuthenticators[i].ValidateToken(token);
                    if (authorizationPolicies != null && authorizationPolicies.Count != 0)
                    {
                        for (int j = 0; j < authorizationPolicies.Count; ++j)
                        {
                            IAuthorizationPolicy policy = authorizationPolicies[j];
                            if (policy is UnconditionalPolicy)
                            {
                                return ((UnconditionalPolicy)policy).PrimaryIdentity;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public virtual IIdentity ResolveIdentity(SecurityKeyIdentifier keyIdentifier)
        {
            if (keyIdentifier == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("keyIdentifier");

            RsaKeyIdentifierClause rsaKeyIdentifierClause;
            if (keyIdentifier.TryFind<RsaKeyIdentifierClause>(out rsaKeyIdentifierClause))
            {
                return SecurityUtils.CreateIdentity(rsaKeyIdentifierClause.Rsa.ToXmlString(false), this.GetType().Name);
            }

            return null;
        }

        bool IsCurrentlyTimeEffective(SamlSecurityToken token)
        {
            if (token.Assertion.Conditions != null)
            {
                return SecurityUtils.IsCurrentlyTimeEffective(token.Assertion.Conditions.NotBefore, token.Assertion.Conditions.NotOnOrAfter, this.maxClockSkew);
            }

            // If SAML Condition is not present then the assertion is valid at any given time.
            return true;
        }
    }

}
