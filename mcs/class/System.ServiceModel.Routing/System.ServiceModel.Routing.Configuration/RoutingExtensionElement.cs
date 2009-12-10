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
		public string FilterTableName { get; set; }

		[ConfigurationProperty ("routeOnHeadersOnly", DefaultValue = true, Options = ConfigurationPropertyOptions.None)]
		public bool RouteOnHeadersOnly { get; set; }

		[ConfigurationProperty ("soapProcessingEnabled", DefaultValue = true)]
		public bool SoapProcessingEnabled { get; set; }

		protected override object CreateBehavior ()
		{
			var sec = (RoutingSection) ConfigurationManager.GetSection ("system.serviceModel/routing");
			var re = (RoutingExtension) Activator.CreateInstance (typeof (RoutingExtension), new object [0]);

			var filters = new List<MessageFilter> ();
			var table = RoutingSection.CreateFilterTable (FilterTableName);

			re.ApplyConfiguration (new RoutingConfiguration (table, RouteOnHeadersOnly) {
				SoapProcessingEnabled = this.SoapProcessingEnabled });
			return re;
		}
	}
}
