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

    class HttpHandler : IHttpHandler, IRequiresSessionState
    {
        public bool IsReusable
        {
            [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called outside PermitOnly context.")]
            get
            {
                return true;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Entry-point from asp.net, called outside PermitOnly context.")]
        [SecurityCritical]
        public void ProcessRequest(HttpContext context)
        {
            ServiceHostingEnvironment.SafeEnsureInitialized();

            HostedHttpRequestAsyncResult.ExecuteSynchronous(context.ApplicationInstance, true, false);
        }
    }
}
