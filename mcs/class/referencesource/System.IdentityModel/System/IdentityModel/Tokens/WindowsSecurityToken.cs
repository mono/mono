//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Security.Principal;
    using System.IdentityModel.Claims;
    using System.IdentityModel.Policy;
    using System.IdentityModel.Tokens;

    public class WindowsSecurityToken : SecurityToken, IDisposable
    {
        string authenticationType;
        string id;
        DateTime effectiveTime;
        DateTime expirationTime;
        WindowsIdentity windowsIdentity;
        bool disposed = false;

        public WindowsSecurityToken(WindowsIdentity windowsIdentity)
            : this(windowsIdentity, SecurityUniqueId.Create().Value)
        {
        }

        public WindowsSecurityToken(WindowsIdentity windowsIdentity, string id)
            : this(windowsIdentity, id, null)
        {
        }

        public WindowsSecurityToken(WindowsIdentity windowsIdentity, string id, string authenticationType)
        {
            DateTime effectiveTime = DateTime.UtcNow;
            Initialize( id, authenticationType, effectiveTime, DateTime.UtcNow.AddHours( 10 ), windowsIdentity, true );
        }

        protected WindowsSecurityToken()
        {
        }

        protected void Initialize(string id, DateTime effectiveTime, DateTime expirationTime, WindowsIdentity windowsIdentity, bool clone)
        {
            Initialize( id, null, effectiveTime, expirationTime, windowsIdentity, clone );
        }

        protected void Initialize(string id, string authenticationType, DateTime effectiveTime, DateTime expirationTime, WindowsIdentity windowsIdentity, bool clone)
        {

            if (windowsIdentity == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("windowsIdentity");

            if (id == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("id");

            this.id = id;
            this.authenticationType = authenticationType;
            this.effectiveTime = effectiveTime;
            this.expirationTime = expirationTime;
            this.windowsIdentity = clone ? SecurityUtils.CloneWindowsIdentityIfNecessary(windowsIdentity, authenticationType) : windowsIdentity;
        }

        public override string Id
        {
            get { return this.id; }
        }

        public string AuthenticationType
        {
            get { return this.authenticationType; }
        }

        public override DateTime ValidFrom
        {
            get { return this.effectiveTime; }
        }

        public override DateTime ValidTo
        {
            get { return this.expirationTime; }
        }

        public virtual WindowsIdentity WindowsIdentity
        {
            get
            {
                ThrowIfDisposed();
                return this.windowsIdentity;
            }
        }

        public override ReadOnlyCollection<SecurityKey> SecurityKeys
        {
            get { return EmptyReadOnlyCollection<SecurityKey>.Instance; }
        }

        public virtual void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                if (this.windowsIdentity != null)
                {
                    this.windowsIdentity.Dispose();
                    this.windowsIdentity = null;
                }
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
