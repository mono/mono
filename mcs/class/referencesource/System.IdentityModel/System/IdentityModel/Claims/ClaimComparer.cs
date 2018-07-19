//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Claims
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;

    class ClaimComparer : IEqualityComparer<Claim>
    {
        static IEqualityComparer<Claim> defaultComparer;
        static IEqualityComparer<Claim> hashComparer;
        static IEqualityComparer<Claim> dnsComparer;
        static IEqualityComparer<Claim> rsaComparer;
        static IEqualityComparer<Claim> thumbprintComparer;
        static IEqualityComparer<Claim> upnComparer;
        static IEqualityComparer<Claim> x500DistinguishedNameComparer;
        IEqualityComparer resourceComparer;

        ClaimComparer(IEqualityComparer resourceComparer)
        {
            this.resourceComparer = resourceComparer;
        }

        public static IEqualityComparer<Claim> GetComparer(string claimType)
        {
            if (claimType == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claimType");
            if (claimType == ClaimTypes.Dns)
                return Dns;
            if (claimType == ClaimTypes.Hash)
                return Hash;
            if (claimType == ClaimTypes.Rsa)
                return Rsa;
            if (claimType == ClaimTypes.Thumbprint)
                return Thumbprint;
            if (claimType == ClaimTypes.Upn)
                return Upn;
            if (claimType == ClaimTypes.X500DistinguishedName)
                return X500DistinguishedName;
            return Default;
        }

        public static IEqualityComparer<Claim> Default
        {
            get
            {
                if (defaultComparer == null)
                {
                    defaultComparer = new ClaimComparer(new ObjectComparer());
                }
                return defaultComparer;
            }
        }

        public static IEqualityComparer<Claim> Dns
        {
            get
            {
                if (dnsComparer == null)
                {
                    dnsComparer = new ClaimComparer(StringComparer.OrdinalIgnoreCase);
                }
                return dnsComparer;
            }
        }

        public static IEqualityComparer<Claim> Hash
        {
            get
            {
                if (hashComparer == null)
                {
                    hashComparer = new ClaimComparer(new BinaryObjectComparer());
                }
                return hashComparer;
            }
        }

        public static IEqualityComparer<Claim> Rsa
        {
            get
            {
                if (rsaComparer == null)
                {
                    rsaComparer = new ClaimComparer(new RsaObjectComparer());
                }
                return rsaComparer;
            }
        }

        public static IEqualityComparer<Claim> Thumbprint
        {
            get
            {
                if (thumbprintComparer == null)
                {
                    thumbprintComparer = new ClaimComparer(new BinaryObjectComparer());
                }
                return thumbprintComparer;
            }
        }

        public static IEqualityComparer<Claim> Upn
        {
            get
            {
                if (upnComparer == null)
                {
                    upnComparer = new ClaimComparer(new UpnObjectComparer());
                }
                return upnComparer;
            }
        }

        public static IEqualityComparer<Claim> X500DistinguishedName
        {
            get
            {
                if (x500DistinguishedNameComparer == null)
                {
                    x500DistinguishedNameComparer = new ClaimComparer(new X500DistinguishedNameObjectComparer());
                }
                return x500DistinguishedNameComparer;
            }
        }

        // we still need to review how the default equals works, this is not how Doug envisioned it.
        public bool Equals(Claim claim1, Claim claim2)
        {
            if (ReferenceEquals(claim1, claim2))
                return true;

            if (claim1 == null || claim2 == null)
                return false;

            if (claim1.ClaimType != claim2.ClaimType || claim1.Right != claim2.Right)
                return false;

            return resourceComparer.Equals(claim1.Resource, claim2.Resource);
        }

        public int GetHashCode(Claim claim)
        {
            if (claim == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("claim");

            return claim.ClaimType.GetHashCode() ^ claim.Right.GetHashCode()
                ^ ( (claim.Resource == null) ? 0 : resourceComparer.GetHashCode(claim.Resource));
        }

        class ObjectComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object obj1, object obj2)
            {
                if (obj1 == null && obj2 == null)
                    return true;
                if (obj1 == null || obj2 == null)
                    return false;
                return obj1.Equals(obj2);
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                if (obj == null)
                    return 0;
                return obj.GetHashCode();
            }
        }

        class BinaryObjectComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object obj1, object obj2)
            {
                if (ReferenceEquals(obj1, obj2))
                    return true;

                byte[] bytes1 = obj1 as byte[];
                byte[] bytes2 = obj2 as byte[];
                if (bytes1 == null || bytes2 == null)
                    return false;

                if (bytes1.Length != bytes2.Length)
                    return false;

                for (int i = 0; i < bytes1.Length; ++i)
                {
                    if (bytes1[i] != bytes2[i])
                        return false;
                }

                return true;
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                byte[] bytes = obj as byte[];
                if (bytes == null)
                    return 0;

                int hashCode = 0;
                for (int i = 0; i < bytes.Length && i < 4; ++i)
                {
                    hashCode = (hashCode << 8) | bytes[i];
                }

                return hashCode ^ bytes.Length;
            }
        }

        class RsaObjectComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object obj1, object obj2)
            {
                if (ReferenceEquals(obj1, obj2))
                    return true;

                RSA rsa1 = obj1 as RSA;
                RSA rsa2 = obj2 as RSA;
                if (rsa1 == null || rsa2 == null)
                    return false;

                RSAParameters parm1 = rsa1.ExportParameters(false);
                RSAParameters parm2 = rsa2.ExportParameters(false);

                if (parm1.Modulus.Length != parm2.Modulus.Length ||
                    parm1.Exponent.Length != parm2.Exponent.Length)
                    return false;

                for (int i = 0; i < parm1.Modulus.Length; ++i)
                {
                    if (parm1.Modulus[i] != parm2.Modulus[i])
                        return false;
                }
                for (int i = 0; i < parm1.Exponent.Length; ++i)
                {
                    if (parm1.Exponent[i] != parm2.Exponent[i])
                        return false;
                }
                return true;
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                RSA rsa = obj as RSA;
                if (rsa == null)
                    return 0;

                RSAParameters parm = rsa.ExportParameters(false);
                return parm.Modulus.Length ^ parm.Exponent.Length;
            }
        }

        class X500DistinguishedNameObjectComparer : IEqualityComparer
        {
            IEqualityComparer binaryComparer;
            public X500DistinguishedNameObjectComparer()
            {
                binaryComparer = new BinaryObjectComparer();
            }

            bool IEqualityComparer.Equals(object obj1, object obj2)
            {
                if (ReferenceEquals(obj1, obj2))
                    return true;

                X500DistinguishedName dn1 = obj1 as X500DistinguishedName;
                X500DistinguishedName dn2 = obj2 as X500DistinguishedName;
                if (dn1 == null || dn2 == null)
                    return false;

                // 1) Hopefully cover most cases (perf reason).
                if (StringComparer.Ordinal.Equals(dn1.Name, dn2.Name))
                    return true;

                // 2) Raw byte compare.  Note: we assume the rawbyte is in the same order 
                // (default = X500DistinguishedNameFlags.Reversed). 
                return binaryComparer.Equals(dn1.RawData, dn2.RawData);
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                X500DistinguishedName dn = obj as X500DistinguishedName;
                if (dn == null)
                    return 0;

                return binaryComparer.GetHashCode(dn.RawData);
            }
        }

        class UpnObjectComparer : IEqualityComparer
        {
            bool IEqualityComparer.Equals(object obj1, object obj2)
            {
                if (StringComparer.OrdinalIgnoreCase.Equals(obj1, obj2))
                    return true;

                string upn1 = obj1 as string;
                string upn2 = obj2 as string;
                if (upn1 == null || upn2 == null)
                    return false;

                SecurityIdentifier sid1;
                if (!TryLookupSidFromName(upn1, out sid1))
                    return false;

                // Normalize to sid
                SecurityIdentifier sid2;
                if (!TryLookupSidFromName(upn2, out sid2))
                    return false;

                return sid1 == sid2;
            }

            int IEqualityComparer.GetHashCode(object obj)
            {
                string upn = obj as string;
                if (upn == null)
                    return 0;

                // Normalize to sid
                SecurityIdentifier sid;
                if (TryLookupSidFromName(upn, out sid))
                    return sid.GetHashCode();

                return StringComparer.OrdinalIgnoreCase.GetHashCode(upn);
            }

            bool TryLookupSidFromName(string upn, out SecurityIdentifier sid)
            {
                sid = null;
                try
                {
                    NTAccount acct = new NTAccount(upn);
                    sid = acct.Translate(typeof(SecurityIdentifier)) as SecurityIdentifier;
                }
                catch (IdentityNotMappedException e) 
                {
                    DiagnosticUtility.TraceHandledException(e, TraceEventType.Information);
                }
                return sid != null;
            }
        }
    }
}
