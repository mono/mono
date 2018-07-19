//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.ServiceModel.Diagnostics.Application;

    public sealed class VirtualPathExtension : IExtension<ServiceHostBase>
    {
        internal VirtualPathExtension(string virtualPath, string applicationVirtualPath, string siteName)
        {
            this.VirtualPath = virtualPath;
            this.ApplicationVirtualPath = applicationVirtualPath;
            this.SiteName = siteName;
        }

        public string ApplicationVirtualPath
        {
            get;
            private set;
        }

        public string SiteName
        {
            get;
            private set;
        }

        public string VirtualPath
        {
            get;
            private set;
        }

        public void Attach(ServiceHostBase owner)
        {
        }

        public void Detach(ServiceHostBase owner)
        {
            throw FxTrace.Exception.AsError(new InvalidOperationException(SR.GetString(SR.Hosting_VirtualPathExtenstionCanNotBeDetached)));
        }
    }
}
