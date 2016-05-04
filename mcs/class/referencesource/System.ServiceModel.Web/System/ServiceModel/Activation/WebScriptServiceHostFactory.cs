//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Web;
    using System.Web.Hosting;
    using System.IO;
    using System.ServiceModel.Diagnostics;
    using System.Web.Compilation;
    using System.Reflection;

    public class WebScriptServiceHostFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new WebScriptServiceHost(serviceType, baseAddresses);
        }
    }
}
