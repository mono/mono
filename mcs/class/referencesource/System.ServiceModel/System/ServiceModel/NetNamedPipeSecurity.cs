//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel
{
    using System.Net.Security;
    using System.ServiceModel.Channels;
    using System.ComponentModel;

    public sealed class NetNamedPipeSecurity
    {
        internal const NetNamedPipeSecurityMode DefaultMode = NetNamedPipeSecurityMode.Transport;
        NetNamedPipeSecurityMode mode;
        NamedPipeTransportSecurity transport = new NamedPipeTransportSecurity();

        public NetNamedPipeSecurity()
        {
            this.mode = DefaultMode;
        }

        NetNamedPipeSecurity(NetNamedPipeSecurityMode mode, NamedPipeTransportSecurity transport)
        {
            this.mode = mode;
            this.transport = transport == null ? new NamedPipeTransportSecurity() : transport;
        }

        [DefaultValue(DefaultMode)]
        public NetNamedPipeSecurityMode Mode
        {
            get { return this.mode; }
            set
            {
                if (!NetNamedPipeSecurityModeHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.mode = value;
            }
        }

        public NamedPipeTransportSecurity Transport
        {
            get { return this.transport; }
            set
            {
                if (value == null)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                this.transport = value;
            }
        }

        internal WindowsStreamSecurityBindingElement CreateTransportSecurity()
        {
            if (mode == NetNamedPipeSecurityMode.Transport)
            {
                return this.transport.CreateTransportProtectionAndAuthentication();
            }
            else
            {
                return null;
            }
        }

        internal static bool TryCreate(WindowsStreamSecurityBindingElement wssbe, NetNamedPipeSecurityMode mode, out NetNamedPipeSecurity security)
        {
            security = null;
            NamedPipeTransportSecurity transportSecurity = new NamedPipeTransportSecurity();
            if (mode == NetNamedPipeSecurityMode.Transport)
            {
                if (!NamedPipeTransportSecurity.IsTransportProtectionAndAuthentication(wssbe, transportSecurity))
                    return false;
            }
            security = new NetNamedPipeSecurity(mode, transportSecurity);
            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeTransport()
        {
            if (this.transport.ProtectionLevel == ConnectionOrientedTransportDefaults.ProtectionLevel)
            {
                return false;
            }
            return true;
        }
    }
}


