//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Collections;
    using System.Collections.ObjectModel;

    class ServiceDeploymentInfo
    {
        public ServiceDeploymentInfo(string virtualPath,  ServiceHostFactoryBase serviceHostFactory, string serviceType)
        {
            this.VirtualPath = virtualPath;
            this.ServiceHostFactory = serviceHostFactory;
            this.ServiceType = serviceType;
            this.MessageHandledByRoute = false;
        }

        public bool MessageHandledByRoute
        {
            get;
            set;
        }

        public string VirtualPath
        {
            get;
            private set;
        }

        public string ServiceType 
        {
            get;
            private set; 
        }

        public ServiceHostFactoryBase ServiceHostFactory
        {
            get;
            private set;
        }        
    }
}
