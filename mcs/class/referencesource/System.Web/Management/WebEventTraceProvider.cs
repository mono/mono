//------------------------------------------------------------------------------
// <copyright file="TraceWebEventProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Management {
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Collections.Specialized;
    using System.Web.Util;
    using System.Security.Permissions;

    ////////////
    // Events
    ////////////

    public sealed class TraceWebEventProvider : WebEventProvider, IInternalWebEventProvider {

        internal TraceWebEventProvider() { }

        public override void Initialize(string name, NameValueCollection config)
        {
            Debug.Trace("TraceWebEventProvider", "Initializing: name=" + name);
            base.Initialize(name, config);

            ProviderUtil.CheckUnrecognizedAttributes(config, name);
        }

        public override void ProcessEvent(WebBaseEvent eventRaised)
        {
            if (eventRaised is WebBaseErrorEvent) {
                System.Diagnostics.Trace.TraceError(eventRaised.ToString());
            }
            else {
                System.Diagnostics.Trace.TraceInformation(eventRaised.ToString());
            }
        }

        public override void Flush() {
        }

        public override void Shutdown() {
        }
    }

}

