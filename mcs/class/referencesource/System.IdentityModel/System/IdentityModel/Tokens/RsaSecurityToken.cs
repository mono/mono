//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Runtime.CompilerServices;

    public class RsaSecurityToken : SecurityToken
    {
        string id;
        DateTime effectiveTime;
        ReadOnlyCollection<SecurityKey> rsaKey;
        RSA rsa;
        CspKeyContainerInfo keyContainerInfo;
        GCHandle rsaHandle;

        public RsaSecurityToken(RSA rsa)
            : this(rsa, SecurityUniqueId.Create().Value)
        {
        }

        public RsaSecurityToken(RSA rsa, string id)
        {
            if (rsa == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rsa");
            if (id == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");
            this.rsa = rsa;
            this.id = id;
            this.effectiveTime = DateTime.UtcNow;
            GC.SuppressFinalize(this);
        }

        // This is defense-in-depth.
        // Rsa finalizer can throw and bring down the process if in finalizer context.
        // This internal ctor is used by SM's IssuedSecurityTokenProvider.
        // If ownsRsa=true, this class will take ownership of the Rsa object and provides
        // a reliable finalizing/disposing of Rsa object.  The GCHandle is used to ensure 
        // order in finalizer sequence.
        RsaSecurityToken(RSACryptoServiceProvider rsa, bool ownsRsa)
        {
            if (rsa == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("rsa");
            this.rsa = rsa;
            this.id = SecurityUniqueId.Create().Value;
            this.effectiveTime = DateTime.UtcNow;
            if (ownsRsa)
            {
                // This also key pair generation.  
                // This must be called before PersistKeyInCsp to avoid a handle to go out of scope.
                this.keyContainerInfo = rsa.CspKeyContainerInfo;
                // We will handle key file deletion
                rsa.PersistKeyInCsp = true;
                this.rsaHandle = GCHandle.Alloc(rsa);
            }
            else
            {
                GC.SuppressFinalize(this);
            }
        }

        ~RsaSecurityToken()
        {
            Dispose(false);
        }

        internal void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (this.rsaHandle.IsAllocated)
            {
                try
                {
                    // keyContainerInfo is a wrapper over a member of Rsa.
                    // this is to be safe that rsa.Dispose won't clean up that member.
                    string keyContainerName = this.keyContainerInfo.KeyContainerName;
                    string providerName = this.keyContainerInfo.ProviderName;
                    uint providerType = (uint)this.keyContainerInfo.ProviderType;

                    ((IDisposable)this.rsa).Dispose();

                    // Best effort delete key file in user context
                    SafeProvHandle provHandle;
                    if (!NativeMethods.CryptAcquireContextW(out provHandle,
                                                            keyContainerName,
                                                            providerName,
                                                            providerType,
                                                            NativeMethods.CRYPT_DELETEKEYSET))
                    {
                        int error = Marshal.GetLastWin32Error();
                        try
                        {
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                                new InvalidOperationException(SR.GetString(SR.FailedToDeleteKeyContainerFile), new Win32Exception(error)));
                        }
                        catch (InvalidOperationException ex)
                        {
                            DiagnosticUtility.TraceHandledException(ex, TraceEventType.Warning);
                        }
                    }
                    System.ServiceModel.Diagnostics.Utility.CloseInvalidOutSafeHandle(provHandle);
                }
                finally
                {
                    this.rsaHandle.Free();
                }
            }
        }

        internal static RsaSecurityToken CreateSafeRsaSecurityToken(int keySize)
        {
            RsaSecurityToken token;
            RSACryptoServiceProvider rsa = null;

            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                try
                {
                }
                finally
                {
                    rsa = new RSACryptoServiceProvider(keySize);
                }
                token = new RsaSecurityToken(rsa, true);
                rsa = null;
            }
            finally
            {
                if (rsa != null)
                {
                    ((IDisposable)rsa).Dispose();
                }
            }
            return token;
        }

        public override string Id
        {
            get { return this.id; }
        }

        public override DateTime ValidFrom
        {
            get { return this.effectiveTime; }
        }

        public override DateTime ValidTo
        {
            // Never expire
            get { return SecurityUtils.MaxUtcDateTime; }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get
            {
                if (this.rsaKey == null)
                {
                    List<SecurityKey> keys = new List<SecurityKey>(1);
                    keys.Add(new RsaSecurityKey(this.rsa));
                    this.rsaKey = keys.AsReadOnly();
                }
                return this.rsaKey;
            }
        }

        public RSA Rsa
        {
            get { return this.rsa; }
        }

        public override bool CanCreateKeyIdentifierClause<T>()
        {
            return typeof(T) == typeof(RsaKeyIdentifierClause);
        }

        public override T CreateKeyIdentifierClause<T>()
        {
            if (typeof(T) == typeof(RsaKeyIdentifierClause))
                return (T)((object)new RsaKeyIdentifierClause(this.rsa));

            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(
                SR.GetString(SR.TokenDoesNotSupportKeyIdentifierClauseCreation, GetType().Name, typeof(T).Name)));
        }

        public override bool MatchesKeyIdentifierClause(SecurityKeyIdentifierClause keyIdentifierClause)
        {
            RsaKeyIdentifierClause rsaKeyIdentifierClause = keyIdentifierClause as RsaKeyIdentifierClause;
            if (rsaKeyIdentifierClause != null)
                return rsaKeyIdentifierClause.Matches(this.rsa);

            return false;
        }
    }
}
