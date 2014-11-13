//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.Web;
    using System.Web.SessionState;
    
    class AspNetRouteServiceHttpHandler : IHttpAsyncHandler, IRequiresSessionState
    {
        string serviceVirtualPath;

        public AspNetRouteServiceHttpHandler(string virtualPath)
        {
            this.serviceVirtualPath = virtualPath;
        }

        public bool IsReusable
        {
            [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called outside PermitOnly context.")]
            get
            {
                return true;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Entry-point from asp.net, called outside PermitOnly context. ASP.NET calls are critical." +
            "ExecuteSynchronous is critical because it captures HostedImpersonationContext (and makes it available later) " +
            "so caller must ensure that this is called in the right place.")]
        [SecurityCritical]
        public void ProcessRequest(HttpContext context)
        {
            ServiceHostingEnvironment.SafeEnsureInitialized();

            HostedHttpRequestAsyncResult.ExecuteSynchronous(context.ApplicationInstance, this.serviceVirtualPath, true, false);
        }

        [Fx.Tag.SecurityNote(Critical = "Entry-point from asp.net, called outside PermitOnly context. ASP.NET calls are critical." +
            "ExecuteSynchronous is critical because it captures HostedImpersonationContext (and makes it available later) " +
            "so caller must ensure that this is called in the right place.")]
        [SecurityCritical]
        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback callback, object extraData)
        {
            ServiceHostingEnvironment.SafeEnsureInitialized();

            return new HostedHttpRequestAsyncResult(context.ApplicationInstance, this.serviceVirtualPath, true, false, callback, extraData);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called outside PermitOnly context.")]
        public void EndProcessRequest(IAsyncResult result)
        {
            HostedHttpRequestAsyncResult.End(result);
        }
    }
}

