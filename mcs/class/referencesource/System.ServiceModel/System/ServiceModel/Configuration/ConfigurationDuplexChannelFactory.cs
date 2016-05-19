//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.ServiceModel.Diagnostics;

    public sealed class ConfigurationDuplexChannelFactory<TChannel> : DuplexChannelFactory<TChannel>
    {
        // TChannel provides ContractDescription, attr/config|Config object [TChannel,name] provides Binding, provide Address explicitly
        public ConfigurationDuplexChannelFactory(object callbackObject, string endpointConfigurationName, EndpointAddress remoteAddress, Configuration configuration)
            : base(typeof(TChannel))
        {
            using (ServiceModelActivity activity = DiagnosticUtility.ShouldUseActivity ? ServiceModelActivity.CreateBoundedActivity() : null)
            {
                if (DiagnosticUtility.ShouldUseActivity)
                {
                    ServiceModelActivity.Start(activity, SR.GetString(SR.ActivityConstructChannelFactory, TraceUtility.CreateSourceString(this)), ActivityType.Construct);
                }
                if (callbackObject == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callbackObject");
                }

                if (endpointConfigurationName == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("endpointConfigurationName");
                }

                if (configuration == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("configuration");
                }

                this.CheckAndAssignCallbackInstance(callbackObject);
                this.InitializeEndpoint(endpointConfigurationName, remoteAddress, configuration);
            }
        }
    }
}
