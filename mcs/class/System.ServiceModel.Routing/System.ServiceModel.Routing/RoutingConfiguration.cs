using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Routing
{
	public sealed class RoutingConfiguration
	{
		public RoutingConfiguration ()
			: this (new MessageFilterTable<IEnumerable<ServiceEndpoint>> (), true)
		{
			// probably init from configuration
		}

		public RoutingConfiguration (MessageFilterTable<IEnumerable<ServiceEndpoint>> filterTable, bool routeOnHeadersOnly)
		{
			FilterTable = filterTable;
			RouteOnHeadersOnly = routeOnHeadersOnly;
			SoapProcessingEnabled = true;
		}

		public MessageFilterTable<IEnumerable<ServiceEndpoint>> FilterTable { get; private set; }
		public bool RouteOnHeadersOnly { get; set; }
		public bool SoapProcessingEnabled { get; set; }
	}
}
