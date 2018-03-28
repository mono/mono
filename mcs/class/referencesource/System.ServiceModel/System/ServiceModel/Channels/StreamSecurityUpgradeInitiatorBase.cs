//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.IO;
    using System.ServiceModel;
    using System.ServiceModel.Security;

    abstract class StreamSecurityUpgradeInitiatorBase : StreamSecurityUpgradeInitiator
    {
        EndpointAddress remoteAddress;
        Uri via;
        SecurityMessageProperty remoteSecurity;
        bool securityUpgraded;
        string nextUpgrade;
        bool isOpen;

        protected StreamSecurityUpgradeInitiatorBase(string upgradeString, EndpointAddress remoteAddress, Uri via)
        {
            this.remoteAddress = remoteAddress;
            this.via = via;
            this.nextUpgrade = upgradeString;
        }

        protected EndpointAddress RemoteAddress
        {
            get
            {
                return this.remoteAddress;
            }
        }

        protected Uri Via
        {
            get
            {
                return this.via;
            }
        }

        public override IAsyncResult BeginInitiateUpgrade(Stream stream, AsyncCallback callback, object state)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }

            if (!this.isOpen)
            {
                this.Open(TimeSpan.Zero);
            }

            return this.OnBeginInitiateUpgrade(stream, callback, state);
        }

        public override Stream EndInitiateUpgrade(IAsyncResult result)
        {
            if (result == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("result");
            }
            Stream retValue = this.OnEndInitiateUpgrade(result, out this.remoteSecurity);
            this.securityUpgraded = true;
            return retValue;
        }

        public override string GetNextUpgrade()
        {
            string result = this.nextUpgrade;
            this.nextUpgrade = null;
            return result;
        }

        public override SecurityMessageProperty GetRemoteSecurity()
        {
            if (!securityUpgraded)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.OperationInvalidBeforeSecurityNegotiation)));
            }

            return this.remoteSecurity;
        }

        public override Stream InitiateUpgrade(Stream stream)
        {
            if (stream == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("stream");
            }

            if (!this.isOpen)
            {
                this.Open(TimeSpan.Zero);
            }

            Stream result = this.OnInitiateUpgrade(stream, out this.remoteSecurity);
            this.securityUpgraded = true;
            return result;
        }

        internal override void EndOpen(IAsyncResult result)
        {
            base.EndOpen(result);
            this.isOpen = true;
        }

        internal override void Open(TimeSpan timeout)
        {
            base.Open(timeout);
            this.isOpen = true;
        }

        internal override void EndClose(IAsyncResult result)
        {
            base.EndClose(result);
            this.isOpen = false;
        }

        internal override void Close(TimeSpan timeout)
        {
            base.Close(timeout);
            this.isOpen = false;
        }

        protected abstract IAsyncResult OnBeginInitiateUpgrade(Stream stream, AsyncCallback callback, object state);
        protected abstract Stream OnEndInitiateUpgrade(IAsyncResult result,
            out SecurityMessageProperty remoteSecurity);
        protected abstract Stream OnInitiateUpgrade(Stream stream, out SecurityMessageProperty remoteSecurity);
    }
}
