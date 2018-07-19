//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.ServiceModel.Diagnostics;

    public sealed class ConfigurationChannelFactory<TChannel> : ChannelFactory<TChannel>
    {
        // TChannel provides ContractDescription, attr/config|Config object [TChannel,name] provides Binding, provide Address explicitly
        public ConfigurationChannelFactory(string endpointConfigurationName, Configuration configuration, EndpointAddress remoteAddress)
            : base(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityConstructChannelFactory, typeof(TChannel).FullName), ActivityType.Construct);
                }
                if (endpointConfigurationName == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
                }
                if (configuration == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("configuration");
                }

                this.InitializeEndpoint(endpointConfigurationName, remoteAddress, configuration);
            }
        }
    }
}
