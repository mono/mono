//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.MsmqIntegration
{
    using System.ComponentModel;
    using System.Net.Security;
    using System.ServiceModel.Channels;

    public sealed class MsmqIntegrationSecurity
    {
        internal const MsmqIntegrationSecurityMode DefaultMode = MsmqIntegrationSecurityMode.Transport;

        MsmqIntegrationSecurityMode mode;
        MsmqTransportSecurity transportSecurity;

        public MsmqIntegrationSecurity()
        {
            this.mode = DefaultMode;
            this.transportSecurity = new MsmqTransportSecurity();
        }

        [DefaultValue(MsmqIntegrationSecurity.DefaultMode)]
        public MsmqIntegrationSecurityMode Mode
        {
            get { return this.mode; }
            set
            {
                if (!MsmqIntegrationSecurityModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.mode = value;
            }
        }

        public MsmqTransportSecurity Transport
        {
            get { return this.transportSecurity; }
            set { this.transportSecurity = value; }
        }

        internal void ConfigureTransportSecurity(MsmqBindingElementBase msmq)
        {
            if (this.mode == MsmqIntegrationSecurityMode.Transport)
                msmq.MsmqTransportSecurity = this.Transport;
            else
                msmq.MsmqTransportSecurity.Disable();
        }
    }
}


