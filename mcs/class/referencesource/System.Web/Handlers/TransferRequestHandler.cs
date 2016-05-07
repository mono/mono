//------------------------------------------------------------------------------
// <copyright file="TransferRequestHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Handlers {
    using System;
    using System.Web.Hosting;
    
    internal class TransferRequestHandler : IHttpHandler {
        
        public void ProcessRequest(HttpContext context) {
            IIS7WorkerRequest wr = context.WorkerRequest as IIS7WorkerRequest;
            if (wr == null) {
                throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_Integrated_Mode));
            }
            // Dev10 848405: use original unencoded URL (i.e., pass null for url so W3_REQUEST::SetUrl is not called)
            // Dev11 32511: Extensionless URL Handler should not pass parent IHttpUser to child requests
            wr.ScheduleExecuteUrl(null,
                                  null,
                                  null,
                                  true,
                                  context.Request.EntityBody,
                                  null,
                                  preserveUser: false);
            
            // force the completion of the current request so that the 
            // child execution can be performed immediately after unwind
            context.ApplicationInstance.EnsureReleaseState();

            // DevDiv Bugs 162750: IIS7 Integrated Mode:  TransferRequest performance issue
            // Instead of calling Response.End we call HttpApplication.CompleteRequest()
            context.ApplicationInstance.CompleteRequest();
        }

        public bool IsReusable {
            get {
                return true;
            }
        }
    }
}
