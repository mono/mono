using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Routing.Configuration
{
	public sealed class RoutingExtensionElement : BehaviorExtensionElement
	{
		public override Type BehaviorType {
			get { return typeof (RoutingExtension); }
		}

		[ConfigurationProperty ("filterTableName", DefaultValue = null)]
		public string FilterTableName {
			get { return (string) base ["filterTableName"]; }
			set { base ["filterTableName"] = value; }
		}

		[ConfigurationProperty ("routeOnHeadersOnly", DefaultValue = true, Options = ConfigurationPropertyOptions.None)]
		public bool RouteOnHeadersOnly {
			get { return (bool) base ["routeOnHeadersOnly"]; }
			set { base ["routeOnHeadersOnly"] = value; }
		}

		[ConfigurationProperty ("soapProcessingEnabled", DefaultValue = true)]
		public bool SoapProcessingEnabled {
			get { return (bool) base ["soapProcessingEnabled"]; }
			set { base ["soapProcessingEnabled"] = value; }
		}

		protected internal override object CreateBehavior ()
		{
			var table = RoutingSection.CreateFilterTable (FilterTableName);

			var cfg = new RoutingConfiguration (table, RouteOnHeadersOnly) { SoapProcessingEnabled = this.SoapProcessingEnabled };
			return new RoutingBehavior (cfg);
		}
	}
}
