//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.ServiceModel.Dispatcher;

    [Fx.Tag.XamlVisible(false)]
    public sealed class RoutingConfiguration
    {
        internal const bool DefaultRouteOnHeadersOnly = true;
        internal const bool DefaultSoapProcessingEnabled = true;
        internal const bool DefaultEnsureOrderedDispatch = false;
        bool configured;
        MessageFilterTable<IEnumerable<ServiceEndpoint>> filterTable;

        public RoutingConfiguration()
            : this(new MessageFilterTable<IEnumerable<ServiceEndpoint>>(), DefaultRouteOnHeadersOnly)
        {
            this.configured = false;
        }

        public RoutingConfiguration (MessageFilterTable<IEnumerable<ServiceEndpoint>> filterTable, bool routeOnHeadersOnly)
        {
            if (filterTable == null)
            {
                throw FxTrace.Exception.ArgumentNull("filterTable");
            }
            this.configured = true; //User handed us the FilterTable, assume it's valid/configured
            this.filterTable = filterTable;
            this.RouteOnHeadersOnly = routeOnHeadersOnly;
            this.SoapProcessingEnabled = DefaultSoapProcessingEnabled;
            this.EnsureOrderedDispatch = DefaultEnsureOrderedDispatch;
        }

        public MessageFilterTable<IEnumerable<ServiceEndpoint>> FilterTable
        {
            get
            {
                this.configured = true;
                return this.filterTable;
            }
        }

        internal MessageFilterTable<IEnumerable<ServiceEndpoint>> InternalFilterTable
        {
            get
            {
                return this.filterTable;
            }
        }
        
        public bool RouteOnHeadersOnly
        {
            get;
            set;
        }

        public bool SoapProcessingEnabled
        {
            get;
            set;
        }

        public bool EnsureOrderedDispatch
        {
            get;
            set;
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "This gets called in RoutingService..ctor")]
        internal void VerifyConfigured()
        {
            if (!this.configured)
            {
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR.RoutingTableNotConfigured));
            }
        }
    }
}
