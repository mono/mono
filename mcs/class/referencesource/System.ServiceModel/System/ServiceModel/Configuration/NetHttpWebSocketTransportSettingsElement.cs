// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel.Channels;

    /// <summary>
    /// NetHttpWebSocketTransportSettingsElement for WebSocketTransportSettings
    /// </summary>
    public sealed partial class NetHttpWebSocketTransportSettingsElement : WebSocketTransportSettingsElement
    {
        [ConfigurationProperty(ConfigurationStrings.TransportUsage, DefaultValue = NetHttpBindingDefaults.TransportUsage)]
        [ServiceModelEnumValidator(typeof(WebSocketTransportUsageHelper))]
        public override WebSocketTransportUsage TransportUsage
        {
            get { return base.TransportUsage; }
            set { base.TransportUsage = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.SubProtocol, DefaultValue = WebSocketTransportSettings.SoapSubProtocol)]
        [StringValidator(MinLength = 0)]
        public override string SubProtocol
        {
            get { return base.SubProtocol; }
            set { base.SubProtocol = value; }
        }
    }
}
