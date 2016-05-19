//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Claims
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Security.Principal;
    using System.Text;

    [DataContract(Namespace = XsiConstants.Namespace)]
    public abstract class ClaimSet : IEnumerable<Claim>
    {
        static ClaimSet system;
        static ClaimSet windows;
        static ClaimSet anonymous;

        public static ClaimSet System 
        { 
            get 
            {
                if (system == null)
                {
                    List<Claim> claims = new List<Claim>(2);
                    claims.Add(Claim.System);
                    claims.Add(new Claim(ClaimTypes.System, XsiConstants.System, Rights.PossessProperty));
                    system = new DefaultClaimSet(claims);
                }
                return system; 
            } 
        }

        public static ClaimSet Windows
        {
            get
            {
                if (windows == null)
                {
                    List<Claim> claims = new List<Claim>(2);
                    SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.NTAuthoritySid, null);
                    claims.Add(new Claim(ClaimTypes.Sid, sid, Rights.Identity));
                    claims.Add(Claim.CreateWindowsSidClaim(sid));
                    windows = new DefaultClaimSet(claims);
                }
                return windows;
            }
        }

        internal static ClaimSet Anonymous
        {
            get
            {
                if (anonymous == null)
                    anonymous = new DefaultClaimSet();

                return anonymous;
            }
        }

        static internal bool SupportedRight(string right)
        {
            return right == null ||
                Rights.Identity.Equals(right) ||
                Rights.PossessProperty.Equals(right);
        }


        public virtual bool ContainsClaim(Claim claim, IEqualityComparer<Claim> comparer)
        {
            if (claim == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");
            if (comparer == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("comparer");

            IEnumerable<Claim> claims = FindClaims(null, null);
            if (claims != null)
            {
                foreach (Claim matchingClaim in claims)
                {
                    if (comparer.Equals(claim, matchingClaim))
                        return true;
                }
            }
            return false;
        }

        public virtual bool ContainsClaim(Claim claim)
        {
            if (claim == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");

            IEnumerable<Claim> claims = FindClaims(claim.ClaimType, claim.Right);
            if (claims != null)
            {
                foreach (Claim matchingClaim in claims)
                {
                    if (claim.Equals(matchingClaim))
                        return true;
                }
            }
            return false;
        }

        public abstract Claim this[int index] { get; }
        public abstract int Count { get; }
        public abstract ClaimSet Issuer { get; }
        // Note: null string represents any.
        public abstract IEnumerable<Claim> FindClaims(string claimType, string right);
        public abstract IEnumerator<Claim> GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() { return this.GetEnumerator(); }

    }
}
