//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Web;
    using System.Web.Hosting;
    using System.IO;
    using System.ServiceModel.Web;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Activation;
    using System.Web.Compilation;
    using System.Reflection;

    public class WebServiceHostFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new WebServiceHost(serviceType, baseAddresses);
        }
    }
}
