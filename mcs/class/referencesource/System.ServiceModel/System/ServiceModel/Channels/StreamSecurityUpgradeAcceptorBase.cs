//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.IO;
    using System.Runtime.Diagnostics;
    using System.ServiceModel.Security;

    abstract class StreamSecurityUpgradeAcceptorBase : StreamSecurityUpgradeAcceptor
    {
        SecurityMessageProperty remoteSecurity;
        bool securityUpgraded;
        string upgradeString;
        EventTraceActivity eventTraceActivity;

        protected StreamSecurityUpgradeAcceptorBase(string upgradeString)
        {
            this.upgradeString = upgradeString;
        }

        internal EventTraceActivity EventTraceActivity
        {
            get 
            {
                if (this.eventTraceActivity == null)
                {
                    this.eventTraceActivity = EventTraceActivity.GetFromThreadOrCreate();
                }
                return this.eventTraceActivity;
            }
        }

        public override Stream AcceptUpgrade(Stream stream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }

            Stream result = this.OnAcceptUpgrade(stream, out this.remoteSecurity);
            this.securityUpgraded = true;
            return result;
        }

        public override IAsyncResult BeginAcceptUpgrade(Stream stream, AsyncCallback callback, object state)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }

            return this.OnBeginAcceptUpgrade(stream, callback, state);
        }

        public override bool CanUpgrade(string contentType)
        {
            if (this.securityUpgraded)
            {
                return false;
            }

            return (contentType == this.upgradeString);
        }

        public override Stream EndAcceptUpgrade(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            Stream retValue = this.OnEndAcceptUpgrade(result, out this.remoteSecurity);
            this.securityUpgraded = true;
            return retValue;
        }

        public override SecurityMessageProperty GetRemoteSecurity()
        {
            // this could be null if upgrade not completed.
            return this.remoteSecurity;
        }

        protected abstract Stream OnAcceptUpgrade(Stream stream, out SecurityMessageProperty remoteSecurity);
        protected abstract IAsyncResult OnBeginAcceptUpgrade(Stream stream, AsyncCallback callback, object state);
        protected abstract Stream OnEndAcceptUpgrade(IAsyncResult result,
            out SecurityMessageProperty remoteSecurity);
    }
}
