//------------------------------------------------------------------------------
// <copyright file="TransferRequestHandler.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.Handlers {
    using System;
    using System.Threading.Tasks;
    using System.Web.Hosting;

    internal class TransferRequestHandler : IHttpAsyncHandler {
        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            return TaskAsyncHelper.BeginTask(() => ProcessRequestAsync(context), cb, extraData);
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            TaskAsyncHelper.EndTask(result);
        }

        private Task ProcessRequestAsync(HttpContext context) {
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
            var releaseStateTask = context.ApplicationInstance.EnsureReleaseStateAsync();

            // DevDiv Bugs 162750: IIS7 Integrated Mode:  TransferRequest performance issue
            // Instead of calling Response.End we call HttpApplication.CompleteRequest()
            if (releaseStateTask.IsCompleted) {
                context.ApplicationInstance.CompleteRequest();
                return TaskAsyncHelper.CompletedTask;
            }
            else {
                return releaseStateTask.ContinueWith((_) => context.ApplicationInstance.CompleteRequest());
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            string errorMessage = SR.GetString(SR.HttpTaskAsyncHandler_CannotExecuteSynchronously, GetType());
            throw new NotSupportedException(errorMessage);
        }

        public bool IsReusable {
            get {
                return true;
            }
        }
    }
}
