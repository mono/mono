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
	public class SoapProcessingExtensionElement : BehaviorExtensionElement
	{
		public override Type BehaviorType {
			get { return typeof (SoapProcessingBehavior); }
		}

		[ConfigurationProperty ("processMessages", DefaultValue = true, Options = ConfigurationPropertyOptions.None)]
		public bool ProcessMessages {
			get { return (bool) base ["processMessages"]; }
			set { base ["processMessages"] = value; }
		}

		protected override object CreateBehavior ()
		{
			return new SoapProcessingBehavior () { ProcessMessages = this.ProcessMessages };
		}
	}
}
