//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Description
{
    using System;
    using System.Globalization;
    using System.ServiceModel;
    using System.ServiceModel.Dispatcher;
    using System.Transactions;
    using System.ServiceModel.Channels;
    using System.Collections.Generic;
    using System.Diagnostics;

    [DebuggerDisplay("Address={address}")]
    [DebuggerDisplay("Name={name}")]
    public class ServiceMetadataEndpoint : ServiceEndpoint
    {
        public ServiceMetadataEndpoint()
            : this(MetadataExchangeBindings.CreateMexHttpBinding(), null /*address*/)
        {
        }

        public ServiceMetadataEndpoint(EndpointAddress address)
            : this(MetadataExchangeBindings.CreateMexHttpBinding(), address)
        {
        }

        public ServiceMetadataEndpoint(Binding binding, EndpointAddress address)
            : base(ServiceMetadataBehavior.MexContract, binding, address)
        {
            this.IsSystemEndpoint = true;
        }
    }
}

