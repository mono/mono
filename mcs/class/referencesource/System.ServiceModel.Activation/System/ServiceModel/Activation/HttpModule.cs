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
    using System.Web.Hosting;

    class HttpModule : IHttpModule
    {
        static bool disabled;

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - called outside PermitOnly context.")]
        public void Dispose()
        {
        }

        [Fx.Tag.SecurityNote(Critical = "Entry-point from asp.net, accesses ProcessRequest which is SecurityCritical.")]
        [SecurityCritical]
        public void Init(HttpApplication context)
        {
            context.PostAuthenticateRequest += new EventHandler(ProcessRequest);
        }

        [Fx.Tag.SecurityNote(Critical = "Entry-point from asp.net, called outside PermitOnly context. ASP calls are critical." +
            "HostedHttpRequestAsyncResult..ctor is critical because it captures HostedImpersonationContext." +
            "(and makes it available later) so caller must ensure that this is called in the right place.")]
        [SecurityCritical]
        static void ProcessRequest(object sender, EventArgs e)
        {
            if (HttpModule.disabled)
            {
                return;
            }

            try
            {
                ServiceHostingEnvironment.SafeEnsureInitialized();
            }
            catch (SecurityException exception)
            {
                HttpModule.disabled = true;

                DiagnosticUtility.TraceHandledException(exception, TraceEventType.Warning);

                // If requesting a .svc file, the HttpHandler will try to handle it.  It will call
                // SafeEnsureInitialized() again, which will fail with the same exception (it is
                // idempotent on failure).  This is the correct behavior.
                return;
            }

            HttpApplication application = (HttpApplication)sender;

            // Check to see whether the extension is supported
            string extension = application.Request.CurrentExecutionFilePathExtension;
            if (string.IsNullOrEmpty(extension))
            {
                return;
            }

            ServiceHostingEnvironment.ServiceType serviceType = ServiceHostingEnvironment.GetServiceType(extension);
            // do extension check first so that we do not need to do it in aspnetrouting/configurationbasedactivation
            if (serviceType == ServiceHostingEnvironment.ServiceType.Unknown)
            {
                return;
            }
            
            // check for AspNetcompat
            if (ServiceHostingEnvironment.AspNetCompatibilityEnabled)
            {
                // remap httphandler for xamlx in CBA, since there is No physical file and 
                // the xamlx httphandlerfactory will do file exist checking
                if (serviceType == ServiceHostingEnvironment.ServiceType.Workflow && ServiceHostingEnvironment.IsConfigurationBasedService(application)) 
                {
                    application.Context.RemapHandler(new HttpHandler());                   
                }
                return;
            }

            else if (serviceType == ServiceHostingEnvironment.ServiceType.WCF)
            {
                HostedHttpRequestAsyncResult.ExecuteSynchronous(application, false, false);
            }
            else if (serviceType == ServiceHostingEnvironment.ServiceType.Workflow)
            {
                HostedHttpRequestAsyncResult.ExecuteSynchronous(application, false, true);               
            }
        }
    }
}
