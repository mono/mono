//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Claims
{
    using System.Collections.Generic;
    using System.IdentityModel.Policy;
    using System.Net.Mail;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Principal;

    public class X509CertificateClaimSet : ClaimSet, IIdentityInfo, IDisposable
    {
        X509Certificate2 certificate;
        DateTime expirationTime = SecurityUtils.MinUtcDateTime;
        ClaimSet issuer;
        X509Identity identity;
        X509ChainElementCollection elements;
        IList<Claim> claims;
        int index;
        bool disposed = false;

        public X509CertificateClaimSet(X509Certificate2 certificate)
            : this(certificate, true)
        {
        }

        internal X509CertificateClaimSet(X509Certificate2 certificate, bool clone)
        {
            if (certificate == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            this.certificate = clone ? new X509Certificate2(certificate) : certificate;
        }

        X509CertificateClaimSet(X509CertificateClaimSet from)
            : this(from.X509Certificate, true)
        {
        }

        X509CertificateClaimSet(X509ChainElementCollection elements, int index)
        {
            this.elements = elements;
            this.index = index;
            this.certificate = elements[index].Certificate;
        }

        public override Claim this[int index]
        {
            get
            {
                ThrowIfDisposed();
                EnsureClaims();
                return this.claims[index];
            }
        }

        public override int Count
        {
            get
            {
                ThrowIfDisposed();
                EnsureClaims();
                return this.claims.Count;
            }
        }

        IIdentity IIdentityInfo.Identity
        {
            get
            {
                ThrowIfDisposed();
                if (this.identity == null)
                    this.identity = new X509Identity(this.certificate, false, false);
                return this.identity;
            }
        }

        public DateTime ExpirationTime
        {
            get
            {
                ThrowIfDisposed();
                if (this.expirationTime == SecurityUtils.MinUtcDateTime)
                    this.expirationTime = this.certificate.NotAfter.ToUniversalTime();
                return this.expirationTime;
            }
        }

        public override ClaimSet Issuer
        {
            get
            {
                ThrowIfDisposed();
                if (this.issuer == null)
                {
                    if (this.elements == null)
                    {
                        X509Chain chain = new X509Chain();
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                        chain.Build(certificate);
                        this.index = 0;
                        this.elements = chain.ChainElements;
                    }

                    if (this.index + 1 < this.elements.Count)
                    {
                        this.issuer = new X509CertificateClaimSet(this.elements, this.index + 1);
                        this.elements = null;
                    }
                    // SelfSigned?
                    else if (StringComparer.OrdinalIgnoreCase.Equals(this.certificate.SubjectName.Name, this.certificate.IssuerName.Name))
                        this.issuer = this;
                    else
                        this.issuer = new X500DistinguishedNameClaimSet(this.certificate.IssuerName);

                }
                return this.issuer;
            }
        }

        public X509Certificate2 X509Certificate
        {
            get
            {
                ThrowIfDisposed();
                return this.certificate;
            }
        }

        internal X509CertificateClaimSet Clone()
        {
            ThrowIfDisposed();
            return new X509CertificateClaimSet(this);
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                SecurityUtils.DisposeIfNecessary(this.identity);
                if (this.issuer != null)
                {
                    if (this.issuer != this)
                    {
                        SecurityUtils.DisposeIfNecessary(this.issuer as IDisposable);
                    }
                }
                if (this.elements != null)
                {
                    for (int i = this.index + 1; i < this.elements.Count; ++i)
                    {
                        SecurityUtils.ResetCertificate(this.elements[i].Certificate);
                    }
                }
                SecurityUtils.ResetCertificate(this.certificate);
            }
        }

        IList<Claim> InitializeClaimsCore()
        {
            List<Claim> claims = new List<Claim>();
            byte[] thumbprint = this.certificate.GetCertHash();
            claims.Add(new Claim(ClaimTypes.Thumbprint, thumbprint, Rights.Identity));
            claims.Add(new Claim(ClaimTypes.Thumbprint, thumbprint, Rights.PossessProperty));

            // Ordering SubjectName, Dns, SimpleName, Email, Upn
            string value = this.certificate.SubjectName.Name;
            if (!string.IsNullOrEmpty(value))
                claims.Add(Claim.CreateX500DistinguishedNameClaim(this.certificate.SubjectName));

            value = this.certificate.GetNameInfo(X509NameType.DnsName, false);
            if (!string.IsNullOrEmpty(value))
                claims.Add(Claim.CreateDnsClaim(value));

            value = this.certificate.GetNameInfo(X509NameType.SimpleName, false);
            if (!string.IsNullOrEmpty(value))
                claims.Add(Claim.CreateNameClaim(value));

            value = this.certificate.GetNameInfo(X509NameType.EmailName, false);
            if (!string.IsNullOrEmpty(value))
                claims.Add(Claim.CreateMailAddressClaim(new MailAddress(value)));

            value = this.certificate.GetNameInfo(X509NameType.UpnName, false);
            if (!string.IsNullOrEmpty(value))
                claims.Add(Claim.CreateUpnClaim(value));

            value = this.certificate.GetNameInfo(X509NameType.UrlName, false);
            if (!string.IsNullOrEmpty(value))
                claims.Add(Claim.CreateUriClaim(new Uri(value)));

            RSA rsa = this.certificate.PublicKey.Key as RSA;
            if (rsa != null)
                claims.Add(Claim.CreateRsaClaim(rsa));

            return claims;
        }

        void EnsureClaims()
        {
            if (this.claims != null)
                return;

            this.claims = InitializeClaimsCore();
        }

        static bool SupportedClaimType(string claimType)
        {
            return claimType == null ||
                ClaimTypes.Thumbprint.Equals(claimType) ||
                ClaimTypes.X500DistinguishedName.Equals(claimType) ||
                ClaimTypes.Dns.Equals(claimType) ||
                ClaimTypes.Name.Equals(claimType) ||
                ClaimTypes.Email.Equals(claimType) ||
                ClaimTypes.Upn.Equals(claimType) ||
                ClaimTypes.Uri.Equals(claimType) ||
                ClaimTypes.Rsa.Equals(claimType);
        }

        // Note: null string represents any.
        public override IEnumerable<Claim> FindClaims(string claimType, string right)
        {
            ThrowIfDisposed();
            if (!SupportedClaimType(claimType) || !ClaimSet.SupportedRight(right))
            {
                yield break;
            }
            else if (this.claims == null && ClaimTypes.Thumbprint.Equals(claimType))
            {
                if (right == null || Rights.Identity.Equals(right))
                {
                    yield return new Claim(ClaimTypes.Thumbprint, this.certificate.GetCertHash(), Rights.Identity);
                }
                if (right == null || Rights.PossessProperty.Equals(right))
                {
                    yield return new Claim(ClaimTypes.Thumbprint, this.certificate.GetCertHash(), Rights.PossessProperty);
                }
            }
            else if (this.claims == null && ClaimTypes.Dns.Equals(claimType))
            {
                if (right == null || Rights.PossessProperty.Equals(right))
                {
                    string value = this.certificate.GetNameInfo(X509NameType.DnsName, false);
                    if (!string.IsNullOrEmpty(value))
                    {
                        yield return Claim.CreateDnsClaim(value);
                    }
                }
            }
            else
            {
                EnsureClaims();

                bool anyClaimType = (claimType == null);
                bool anyRight = (right == null);

                for (int i = 0; i < this.claims.Count; ++i)
                {
                    Claim claim = this.claims[i];
                    if ((claim != null) &&
                        (anyClaimType || claimType.Equals(claim.ClaimType)) &&
                        (anyRight || right.Equals(claim.Right)))
                    {
                        yield return claim;
                    }
                }
            }
        }

        public override IEnumerator<Claim> GetEnumerator()
        {
            ThrowIfDisposed();
            EnsureClaims();
            return this.claims.GetEnumerator();
        }

        public override string ToString()
        {
            return this.disposed ? base.ToString() : SecurityUtils.ClaimSetToString(this);
        }

        void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().FullName));
            }
        }

        class X500DistinguishedNameClaimSet : DefaultClaimSet, IIdentityInfo
        {
            IIdentity identity;

            public X500DistinguishedNameClaimSet(X500DistinguishedName x500DistinguishedName)
            {
                if (x500DistinguishedName == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("x500DistinguishedName");

                this.identity = new X509Identity(x500DistinguishedName);
                List<Claim> claims = new List<Claim>(2);
                claims.Add(new Claim(ClaimTypes.X500DistinguishedName, x500DistinguishedName, Rights.Identity));
                claims.Add(Claim.CreateX500DistinguishedNameClaim(x500DistinguishedName));
                Initialize(ClaimSet.Anonymous, claims);
            }

            public IIdentity Identity
            {
                get { return this.identity; }
            }
        }
    }

    class X509Identity : GenericIdentity, IDisposable
    {
        const string X509 = "X509";
        const string Thumbprint = "; ";
        X500DistinguishedName x500DistinguishedName;
        X509Certificate2 certificate;
        string name;
        bool disposed = false;
        bool disposable = true;

        public X509Identity(X509Certificate2 certificate)
            : this(certificate, true, true)
        {
        }

        public X509Identity(X500DistinguishedName x500DistinguishedName)
            : base(X509, X509)
        {
            this.x500DistinguishedName = x500DistinguishedName;
        }

        internal X509Identity(X509Certificate2 certificate, bool clone, bool disposable)
            : base(X509, X509)
        {
            this.certificate = clone ? new X509Certificate2(certificate) : certificate;
            this.disposable = clone || disposable;
        }

        public override string Name
        {
            get
            {
                ThrowIfDisposed();
                if (this.name == null)
                {   
                    //
                    // DCR 48092: PrincipalPermission authorization using certificates could cause Elevation of Privilege.
                    // because there could be duplicate subject name.  In order to be more unique, we use SubjectName + Thumbprint
                    // instead
                    //
                    this.name = GetName() + Thumbprint + this.certificate.Thumbprint;
                }
                return this.name;
            }
        }

        string GetName()
        {
            if (this.x500DistinguishedName != null)
                return this.x500DistinguishedName.Name;

            string value = this.certificate.SubjectName.Name;
            if (!string.IsNullOrEmpty(value))
                return value;

            value = this.certificate.GetNameInfo(X509NameType.DnsName, false);
            if (!string.IsNullOrEmpty(value))
                return value;

            value = this.certificate.GetNameInfo(X509NameType.SimpleName, false);
            if (!string.IsNullOrEmpty(value))
                return value;

            value = this.certificate.GetNameInfo(X509NameType.EmailName, false);
            if (!string.IsNullOrEmpty(value))
                return value;

            value = this.certificate.GetNameInfo(X509NameType.UpnName, false);
            if (!string.IsNullOrEmpty(value))
                return value;

            return String.Empty;
        }

        public override ClaimsIdentity Clone()
        {
            return this.certificate != null ? new X509Identity(this.certificate) : new X509Identity(this.x500DistinguishedName);
        }

        public void Dispose()
        {
            if (this.disposable && !this.disposed)
            {
                this.disposed = true;
                if (this.certificate != null)
                {
                    this.certificate.Reset();
                }
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
