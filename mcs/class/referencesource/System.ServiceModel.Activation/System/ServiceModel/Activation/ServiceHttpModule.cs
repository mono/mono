//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.Diagnostics;
    using System.Runtime;
    using System.Security;
    using System.ServiceModel;
    using System.Web;

    class ServiceHttpModule : IHttpModule
    {
        [Fx.Tag.SecurityNote(Critical = "Holds pointer to BeginProcessRequest which is SecurityCritical." +
            "This callback is called outside the PermitOnly context.")]
        [SecurityCritical]
        static BeginEventHandler beginEventHandler;
        static CompletedAsyncResult cachedAsyncResult = new CompletedAsyncResult(null, null);

        [Fx.Tag.SecurityNote(Critical = "This callback is called outside the PermitOnly context.")]
        [SecurityCritical]
        static EndEventHandler endEventHandler;

        static bool disabled;

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called outside PermitOnly context.")]
        public void Dispose()
        {
        }

        [Fx.Tag.SecurityNote(Critical = "Entry-point from ASP.NET, accesses begin/bndProcessRequest which are SecurityCritical.")]
        [SecurityCritical]
        public void Init(HttpApplication context)
        {
            if (ServiceHttpModule.beginEventHandler == null)
            {
                ServiceHttpModule.beginEventHandler = new BeginEventHandler(BeginProcessRequest);
            }
            if (ServiceHttpModule.endEventHandler == null)
            {
                ServiceHttpModule.endEventHandler = new EndEventHandler(EndProcessRequest);
            }
            context.AddOnPostAuthenticateRequestAsync(
                ServiceHttpModule.beginEventHandler,
                ServiceHttpModule.endEventHandler);
        }

        [Fx.Tag.SecurityNote(Critical = "Entry-point from asp.net, called outside PermitOnly context. ASP.NET calls are critical." +
            "HostedHttpRequestAsyncResult..ctor is critical because it captures HostedImpersonationContext (and makes it available later) " +
            "so caller must ensure that this is called in the right place.")]
        [SecurityCritical]
        static public IAsyncResult BeginProcessRequest(object sender, EventArgs e, AsyncCallback cb, object extraData)
        {
            if (ServiceHttpModule.disabled)
            {
                return GetCompletedAsyncResult(cb, extraData);
            }
            
            try
            {
                ServiceHostingEnvironment.SafeEnsureInitialized();
            }
            catch (SecurityException exception)
            {
                ServiceHttpModule.disabled = true;

                DiagnosticUtility.TraceHandledException(exception, TraceEventType.Warning);

                // If requesting a .svc file, the HttpHandler will try to handle it.  It will call
                // SafeEnsureInitialized() again, which will fail with the same exception (it is
                // idempotent on failure).  This is the correct behavior.
                return GetCompletedAsyncResult(cb, extraData);
            }            
                        
            HttpApplication application = (HttpApplication) sender;

            // Check to see whether the extension is supported.
            string extension = application.Request.CurrentExecutionFilePathExtension;
            if (string.IsNullOrEmpty(extension))
            {
                return GetCompletedAsyncResult(cb, extraData);
            }

            ServiceHostingEnvironment.ServiceType serviceType = ServiceHostingEnvironment.GetServiceType(extension);
            // do extension check first so that we do not need to do it in aspnetrouting/configurationbasedactivation
            if (serviceType == ServiceHostingEnvironment.ServiceType.Unknown)
            {
                return GetCompletedAsyncResult(cb, extraData);
            }
            
            // check for AspNetcompat
            if (ServiceHostingEnvironment.AspNetCompatibilityEnabled)
            {                
                // remap httphandler for xamlx in CBA, since there is No physical file and 
                // the xamlx httphandlerfactory will do file exist checking
                if (serviceType == ServiceHostingEnvironment.ServiceType.Workflow && ServiceHostingEnvironment.IsConfigurationBasedService(application))
                {                    
                    IHttpHandler cbaHandler = new ServiceHttpHandlerFactory().GetHandler(
                        application.Context, application.Request.RequestType,
                        application.Request.RawUrl.ToString(), application.Request.PhysicalApplicationPath);
                    application.Context.RemapHandler(cbaHandler);
                }
                return GetCompletedAsyncResult(cb, extraData);
            }

            if (serviceType == ServiceHostingEnvironment.ServiceType.WCF)
            {
                return new HostedHttpRequestAsyncResult(application, false, false, cb, extraData);
            }
            if (serviceType == ServiceHostingEnvironment.ServiceType.Workflow)
            {
                return new HostedHttpRequestAsyncResult(application, false, true, cb, extraData);
            }
            return GetCompletedAsyncResult(cb, extraData);
        }

        private static CompletedAsyncResult GetCompletedAsyncResult(AsyncCallback cb, object state)
        {
            return (cb == null) ? cachedAsyncResult : new CompletedAsyncResult(cb, state);
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called outside PermitOnly context.")]
        static public void EndProcessRequest(IAsyncResult ar)
        {
            //No need to call CompletedAsyncResult.End as the asyncResult has already completed.
            if (ar is HostedHttpRequestAsyncResult)
            {
                HostedHttpRequestAsyncResult.End(ar);
            }
        }
    }
}

