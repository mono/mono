//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.Activation
{
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;

    sealed class HostingMessageProperty : IAspNetMessageProperty
    {
        const string name = "webhost";

        [Fx.Tag.SecurityNote(Critical = "Keeps track of impersonated user, caller must use with care and call Dispose at the appropriate time.")]
        [SecurityCritical]
        HostedImpersonationContext impersonationContext;

        [Fx.Tag.SecurityNote(Critical = "Stores a SecurityCritical helper class that controls HttpContext.Current with an elevation." +
            "Need to ensure that HostedThreadData is constructed and used properly.")]
        [SecurityCritical]
        HostedThreadData currentThreadData;

        [Fx.Tag.SecurityNote(Critical = "Sets impersonation context from an arbitrary source, caller must guard.")]
        [SecurityCritical]
        internal HostingMessageProperty(HostedHttpRequestAsyncResult result)
        {
            Fx.Assert(ServiceHostingEnvironment.IsHosted, "should only be called in the hosted path");

            if (ServiceHostingEnvironment.AspNetCompatibilityEnabled)
            {
                if (result.ImpersonationContext != null && result.ImpersonationContext.IsImpersonated)
                {
                    this.impersonationContext = result.ImpersonationContext;
                    this.impersonationContext.AddRef();
                }

                currentThreadData = result.HostedThreadData;
            }

            this.OriginalRequestUri = result.OriginalRequestUri;
        }

        public Uri OriginalRequestUri
        {
            get;
            private set;
        }

        static internal string Name
        {
            get
            {
                return name;
            }
        }

        HostedImpersonationContext ImpersonationContext
        {
            [Fx.Tag.SecurityNote(Critical = "Keeps track of impersonated user, caller must use with care.",
                Safe = "Safe for Get, individual members of HostedImpersonationContext are protected.")]
            [SecuritySafeCritical]
            get
            {
                return impersonationContext;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Delegates to a SecurityCritical method in HostedThreadData." +
            "Caller must ensure that function is called appropriately and result is guarded and Dispose()'d correctly.")]
        [SecurityCritical]
        public IDisposable ApplyIntegrationContext()
        {
            if (ServiceHostingEnvironment.AspNetCompatibilityEnabled)
            {
                return currentThreadData.CreateContext();
            }

            return null;
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical method HostedImpersonationContext.Impersonate." +
            "Caller should use with care, must take responsibility for reverting impersonation.")]
        [SecurityCritical]
        public IDisposable Impersonate()
        {
            if (this.ImpersonationContext != null)
            {
                return this.ImpersonationContext.Impersonate();
            }
            else
            {
                return null;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Cleans up impersonationContext, which is critical.", Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        public void Close()
        {
            if (impersonationContext != null)
            {
                impersonationContext.Release();
                impersonationContext = null;
            }
        }
    }
}
