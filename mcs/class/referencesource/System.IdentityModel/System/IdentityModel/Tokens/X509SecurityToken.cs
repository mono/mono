//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Security.Cryptography.X509Certificates;

    public class X509SecurityToken : SecurityToken, IDisposable
    {
        string id;
        X509Certificate2 certificate;
        ReadOnlyCollection<SecurityKey> securityKeys;
        DateTime effectiveTime = SecurityUtils.MaxUtcDateTime;
        DateTime expirationTime = SecurityUtils.MinUtcDateTime;
        bool disposed = false;
        bool disposable;

        public X509SecurityToken(X509Certificate2 certificate)
            : this(certificate, SecurityUniqueId.Create().Value) 
        { 
        }

        public X509SecurityToken(X509Certificate2 certificate, string id)
            : this(certificate, id, true)
        {
        }

        internal X509SecurityToken(X509Certificate2 certificate, bool clone)
            : this(certificate, SecurityUniqueId.Create().Value, clone)
        {
        }

        internal X509SecurityToken(X509Certificate2 certificate, bool clone, bool disposable)
            : this(certificate, SecurityUniqueId.Create().Value, clone, disposable)
        {
        }

        internal X509SecurityToken(X509Certificate2 certificate, string id, bool clone)
            : this(certificate, id, clone, true)
        {
        }

        internal X509SecurityToken(X509Certificate2 certificate, string id, bool clone, bool disposable)
        {
            if (certificate == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("certificate");
            if (id == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");

            this.id = id;
            this.certificate = clone ? new X509Certificate2(certificate) : certificate;
            // if the cert needs to be cloned then the token owns the clone and should dispose it
            this.disposable = clone || disposable;
        }

        public override string Id
        {
            get { return this.id; }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                ThrowIfDisposed();
                if (this.securityKeys == null)
                {
                    List<SecurityKey> temp = new List<SecurityKey>(1);
                    temp.Add(new X509AsymmetricSecurityKey(this.certificate));
                    this.securityKeys = temp.AsReadOnly();
                }
                return this.securityKeys;
            }
        }

        public override DateTime ValidFrom
        {
            get 
            {
                ThrowIfDisposed();
                if (this.effectiveTime == SecurityUtils.MaxUtcDateTime)
                    this.effectiveTime = this.certificate.NotBefore.ToUniversalTime();
                return this.effectiveTime;
            }
        }

        public override DateTime ValidTo
        {
            get 
            {
                ThrowIfDisposed();
                if (this.expirationTime == SecurityUtils.MinUtcDateTime)
                    this.expirationTime = this.certificate.NotAfter.ToUniversalTime();
                return this.expirationTime;
            }
        }

        public X509Certificate2 Certificate
        {
            get 
            {
                ThrowIfDisposed();
                return this.certificate;
            }
        }

        public override bool CanCreateKeyIdentifierClause<T>()
        {
            ThrowIfDisposed();
            if (typeof(T) == typeof(X509SubjectKeyIdentifierClause))
                return X509SubjectKeyIdentifierClause.CanCreateFrom(certificate);

            return typeof(T) == typeof(X509ThumbprintKeyIdentifierClause) ||
                   typeof(T) == typeof(X509IssuerSerialKeyIdentifierClause) ||
                   typeof(T) == typeof(X509RawDataKeyIdentifierClause) ||
                   base.CanCreateKeyIdentifierClause<T>();
        }

        public override T CreateKeyIdentifierClause<T>()
        {
            ThrowIfDisposed();
            if (typeof(T) == typeof(X509SubjectKeyIdentifierClause))
            {
                X509SubjectKeyIdentifierClause x509KeyIdentifierClause;
                if (X509SubjectKeyIdentifierClause.TryCreateFrom(certificate, out x509KeyIdentifierClause))
                    return x509KeyIdentifierClause as T;
            }
            else if (typeof(T) == typeof(X509ThumbprintKeyIdentifierClause))
            {
                return new X509ThumbprintKeyIdentifierClause(certificate) as T;
            }
            else if (typeof(T) == typeof(X509IssuerSerialKeyIdentifierClause))
            {
                return new X509IssuerSerialKeyIdentifierClause(certificate) as T;
            }
            else if (typeof(T) == typeof(X509RawDataKeyIdentifierClause))
            {
                return new X509RawDataKeyIdentifierClause(certificate) as T;
            }

            return base.CreateKeyIdentifierClause<T>();
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            ThrowIfDisposed();
            X509SubjectKeyIdentifierClause subjectKeyIdentifierClause = keyIdentifierClause as X509SubjectKeyIdentifierClause;
            if (subjectKeyIdentifierClause != null)
                return subjectKeyIdentifierClause.Matches(certificate);

            X509ThumbprintKeyIdentifierClause thumbprintKeyIdentifierClause = keyIdentifierClause as X509ThumbprintKeyIdentifierClause;
            if (thumbprintKeyIdentifierClause != null)
                return thumbprintKeyIdentifierClause.Matches(certificate);

            X509IssuerSerialKeyIdentifierClause issuerKeyIdentifierClause = keyIdentifierClause as X509IssuerSerialKeyIdentifierClause;
            if (issuerKeyIdentifierClause != null)
                return issuerKeyIdentifierClause.Matches(certificate);

            X509RawDataKeyIdentifierClause rawCertKeyIdentifierClause = keyIdentifierClause as X509RawDataKeyIdentifierClause;
            if (rawCertKeyIdentifierClause != null)
                return rawCertKeyIdentifierClause.Matches(certificate);

            return base.MatchesKeyIdentifierClause(keyIdentifierClause);
        }

        public virtual void Dispose()
        {
            if (this.disposable && !this.disposed)
            {
                this.disposed = true;
                this.certificate.Reset();
            }
        }

        protected void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(this.GetType().FullName));
            }
        }
    }
}
