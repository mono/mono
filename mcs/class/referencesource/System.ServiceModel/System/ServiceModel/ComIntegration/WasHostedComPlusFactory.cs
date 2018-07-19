//----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System.ServiceModel.Activation;

    public sealed class WasHostedComPlusFactory : ServiceHostFactoryBase
    {
        public override ServiceHostBase CreateServiceHost(string constructorString, Uri[] baseAddresses)
        {
            if (!AspNetEnvironment.Enabled)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.Hosting_ProcessNotExecutingUnderHostedContext, "WasHostedComPlusFactory.CreateServiceHost")));
            }

            if (string.IsNullOrEmpty(constructorString))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.Hosting_ServiceTypeNotProvided)));
            }

            return new WebHostedComPlusServiceHost(constructorString, baseAddresses);
        }
    }
}
