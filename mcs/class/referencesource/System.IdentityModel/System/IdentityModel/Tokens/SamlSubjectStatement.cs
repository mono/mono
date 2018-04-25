//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Selectors;
    using System.Security.Principal;

    public abstract class SamlSubjectStatement : SamlStatement
    {
        SamlSubject subject;
        IAuthorizationPolicy policy;
        bool isReadOnly = false;

        protected SamlSubjectStatement()
        {
        }

        protected SamlSubjectStatement(SamlSubject samlSubject)
        {
            if (samlSubject == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSubject"));

            this.subject = samlSubject;
        }

        public SamlSubject SamlSubject
        {
            get { return this.subject; }
            set
            {
                if (isReadOnly)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ObjectIsReadOnly)));

                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("value"));

                this.subject = value;
            }
        }

        public override bool IsReadOnly
        {
            get { return this.isReadOnly; }
        }

        public override void MakeReadOnly()
        {
            if (!this.isReadOnly)
            {
                subject.MakeReadOnly();
                this.isReadOnly = true;
            }
        }

        public override IAuthorizationPolicy CreatePolicy(ClaimSet issuer, SamlSecurityTokenAuthenticator samlAuthenticator)
        {
            if (issuer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuer");

            // SupportingTokenAuthenticator collection can be null when the Subject does not
            // contain a key.

            if (this.policy == null)
            {
                List<ClaimSet> claimSets = new List<ClaimSet>();
                ClaimSet subjectKeyClaimset = this.subject.ExtractSubjectKeyClaimSet(samlAuthenticator);
                if (subjectKeyClaimset != null)
                    claimSets.Add(subjectKeyClaimset);

                List<Claim> claims = new List<Claim>();
                ReadOnlyCollection<Claim> subjectClaims = this.subject.ExtractClaims();
                for (int i = 0; i < subjectClaims.Count; ++i)
                {
                    claims.Add(subjectClaims[i]);
                }

                AddClaimsToList(claims);
                claimSets.Add(new DefaultClaimSet(issuer, claims));
                this.policy = new UnconditionalPolicy(this.subject.Identity, claimSets.AsReadOnly(), SecurityUtils.MaxUtcDateTime);
            }

            return this.policy;
        }

        protected void SetSubject(SamlSubject samlSubject)
        {
            if (samlSubject == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentNullException("samlSubject"));

            this.subject = samlSubject;
        }

        protected abstract void AddClaimsToList(IList<Claim> claims);
    }

}
