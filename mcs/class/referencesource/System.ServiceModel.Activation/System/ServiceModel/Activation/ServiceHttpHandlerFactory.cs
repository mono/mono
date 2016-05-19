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

    class ServiceHttpHandlerFactory : IHttpHandlerFactory
    {
        IHttpHandler handler;

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called outside PermitOnly context.")]
        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            if (this.handler == null)
            {
                this.handler = new ServiceHttpHandler();
            }
            return this.handler;
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called outside PermitOnly context.")]
        public void ReleaseHandler(IHttpHandler handler)
        {
            Fx.Assert(handler is ServiceHttpHandler, "ASP.NET asked to release the wrong handler.");
        }

        class ServiceHttpHandler : IHttpAsyncHandler, IRequiresSessionState
        {
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

                HostedHttpRequestAsyncResult.ExecuteSynchronous(context.ApplicationInstance, true, false);
            }

            [Fx.Tag.SecurityNote(Critical = "Entry-point from asp.net, called outside PermitOnly context. ASP.NET calls are critical." +
                "ExecuteSynchronous is critical because it captures HostedImpersonationContext (and makes it available later) " +
                "so caller must ensure that this is called in the right place.")]
            [SecurityCritical]
            public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback callback, object extraData)
            {
                ServiceHostingEnvironment.SafeEnsureInitialized();

                return new HostedHttpRequestAsyncResult(context.ApplicationInstance, true, false, callback, extraData);
            }

            [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called outside PermitOnly context.")]
            public void EndProcessRequest(IAsyncResult result)
            {
                HostedHttpRequestAsyncResult.End(result);
            }
        }
    }
}

