//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Runtime;
    using System.ComponentModel;
    using System.ServiceModel.Channels;
    using Config = System.ServiceModel.Configuration;

    public sealed class NetMsmqSecurity
    {
        internal const NetMsmqSecurityMode DefaultMode = NetMsmqSecurityMode.Transport;

        NetMsmqSecurityMode mode;
        MsmqTransportSecurity transportSecurity;
        MessageSecurityOverMsmq messageSecurity;

        public NetMsmqSecurity()
            : this(DefaultMode, null, null)
        {
        }

        internal NetMsmqSecurity(NetMsmqSecurityMode mode)
            : this(mode, null, null)
        {
        }

        NetMsmqSecurity(NetMsmqSecurityMode mode, MsmqTransportSecurity transportSecurity, MessageSecurityOverMsmq messageSecurity)
        {
            Fx.Assert(NetMsmqSecurityModeHelper.IsDefined(mode), string.Format("Invalid NetMsmqSecurityMode value: {0}.", mode.ToString()));

            this.mode = mode;
            this.transportSecurity = transportSecurity == null ? new MsmqTransportSecurity() : transportSecurity;
            this.messageSecurity = messageSecurity == null ? new MessageSecurityOverMsmq() : messageSecurity;
        }

        [DefaultValue(NetMsmqSecurity.DefaultMode)]
        public NetMsmqSecurityMode Mode
        {
            get { return this.mode; }
            set
            {
                if (!NetMsmqSecurityModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.mode = value;
            }
        }

        public MsmqTransportSecurity Transport
        {
            get
            {
                if (this.transportSecurity == null)
                    this.transportSecurity = new MsmqTransportSecurity();
                return this.transportSecurity;
            }
            set { this.transportSecurity = value; }
        }

        public MessageSecurityOverMsmq Message
        {
            get
            {
                if (this.messageSecurity == null)
                    this.messageSecurity = new MessageSecurityOverMsmq();
                return this.messageSecurity;
            }
            set { this.messageSecurity = value; }
        }

        internal void ConfigureTransportSecurity(MsmqBindingElementBase msmq)
        {
            if (this.mode == NetMsmqSecurityMode.Transport || this.mode == NetMsmqSecurityMode.Both)
                msmq.MsmqTransportSecurity = this.Transport;
            else
                msmq.MsmqTransportSecurity.Disable();
        }

        internal static bool IsConfiguredTransportSecurity(MsmqTransportBindingElement msmq, out UnifiedSecurityMode mode)
        {
            if (msmq == null)
            {
                mode = UnifiedSecurityMode.None;
                return false;
            }
            if (msmq.MsmqTransportSecurity.Enabled)
            {
                mode = UnifiedSecurityMode.Transport | UnifiedSecurityMode.Both;
            }
            else
            {
                mode = UnifiedSecurityMode.None | UnifiedSecurityMode.Message;
            }
            return true;
        }

        internal SecurityBindingElement CreateMessageSecurity()
        {
            return this.Message.CreateSecurityBindingElement();
        }

        //
        internal static bool TryCreate(SecurityBindingElement sbe, NetMsmqSecurityMode mode, out NetMsmqSecurity security)
        {
            security = null;
            MessageSecurityOverMsmq messageSecurity;
            if (!MessageSecurityOverMsmq.TryCreate(sbe, out messageSecurity))
                messageSecurity = null;
            security = new NetMsmqSecurity(mode, null, messageSecurity);
            return sbe == null || Config.SecurityElement.AreBindingsMatching(security.CreateMessageSecurity(), sbe, false);
        }
    }
}


