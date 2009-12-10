using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Dispatcher
{
	public class StrictAndMessageFilter : MessageFilter
	{
		MessageFilter filter1, filter2;

		public StrictAndMessageFilter (MessageFilter filter1, MessageFilter filter2)
		{
			if (filter1 == null)
				throw new ArgumentNullException ("filter1");
			if (filter2 == null)
				throw new ArgumentNullException ("filter2");

			this.filter1 = filter1;
			this.filter2 = filter2;
		}

		public override bool Match (Message message)
		{
			return filter1.Match (message) && filter2.Match (message);
		}

		public override bool Match (MessageBuffer buffer)
		{
			return filter1.Match (buffer) && filter2.Match (buffer);
		}
	}
}
