//------------------------------------------------------------------------------
// <copyright file="IisTraceWebEventProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Management {
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Collections.Specialized;
    using System.Web.Util;
    using System.Web.Hosting;
    using System.Security.Permissions;

    ////////////
    // Events
    ////////////

    public sealed class IisTraceWebEventProvider  : WebEventProvider {

        public IisTraceWebEventProvider() {
            // only supported on IIS version 7 and later
            HttpContext context = HttpContext.Current;
            if (context != null) {
                if (!HttpRuntime.UseIntegratedPipeline && !(context.WorkerRequest is ISAPIWorkerRequestInProcForIIS7)) {
                    throw new PlatformNotSupportedException(SR.GetString(SR.Requires_Iis_7));
                }
            }
        }

        public override void Initialize(string name, NameValueCollection config)
        {
            Debug.Trace("IisTraceWebEventProvider", "Initializing: name=" + name);
            base.Initialize(name, config);

            ProviderUtil.CheckUnrecognizedAttributes(config, name);
        }


        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            HttpContext context = HttpContext.Current;
            if (context != null) {
                context.WorkerRequest.RaiseTraceEvent(eventRaised);
            }
        }
        

        public override void Flush() {
        }


        public override void Shutdown() {
        }
    }

}

