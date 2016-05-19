//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Net;
    using System.Configuration;
    using System.ServiceModel.Channels;

    internal class PeerTransportListenAddressValidator : ConfigurationValidatorBase
    {
        public PeerTransportListenAddressValidator()
        {
        }

        public override bool CanValidate(Type type)
        {
            return type == typeof(System.Net.IPAddress);
        }

        public override void Validate(object value)
        {
            PeerValidateHelper.ValidateListenIPAddress(value as IPAddress);
        }
    }
}
