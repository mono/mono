//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Claims
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = XsiConstants.Namespace)]
    public class DefaultClaimSet : ClaimSet
    {
        [DataMember(Name = "Issuer")]
        ClaimSet issuer;
        [DataMember(Name = "Claims")]
        IList<Claim> claims;

        public DefaultClaimSet(params Claim[] claims)
        {
            Initialize(this, claims);
        }

        public DefaultClaimSet(IList<Claim> claims)
        {
            Initialize(this, claims);
        }

        public DefaultClaimSet(ClaimSet issuer, params Claim[] claims)
        {
            Initialize(issuer, claims);
        }

        public DefaultClaimSet(ClaimSet issuer, IList<Claim> claims)
        {
            Initialize(issuer, claims);
        }

        public override Claim this[int index] 
        {
            get { return this.claims[index]; }
        }

        public override int Count
        {
            get { return this.claims.Count; }
        }

        public override ClaimSet Issuer
        {
            get { return this.issuer; }
        }

        public override bool ContainsClaim(Claim claim)
        {
            if (claim == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");

            for (int i = 0; i < this.claims.Count; ++i)
            {
                if (claim.Equals(this.claims[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public override IEnumerable<Claim> FindClaims(string claimType, string right)
        {
            bool anyClaimType = (claimType == null);
            bool anyRight = (right == null);

            for (int i = 0; i < this.claims.Count; ++i)
            {
                Claim claim = this.claims[i];
                if ((claim != null) &&
                    (anyClaimType || claimType == claim.ClaimType) &&
                    (anyRight || right == claim.Right))
                {
                    yield return claim;
                }
            }
        }

        public override IEnumerator<Claim> GetEnumerator()
        {
            return this.claims.GetEnumerator();
        }

        protected void Initialize(ClaimSet issuer, IList<Claim> claims)
        {
            if (issuer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("issuer");
            if (claims == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claims");

            this.issuer = issuer;
            this.claims = claims;
        }

        public override string ToString()
        {
            return SecurityUtils.ClaimSetToString(this);
        }
    }
}
