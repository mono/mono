//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    
    class DllHostedComPlusServiceHost : ComPlusServiceHost
    {
        public DllHostedComPlusServiceHost (Guid clsid,
                                            ServiceElement service,
                                            ComCatalogObject applicationObject,
                                            ComCatalogObject classObject)
        {
            Initialize (clsid,
                        service,
                        applicationObject,
                        classObject,
                        HostingMode.ComPlus);
        }
    }
}
