/*
using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace System.ServiceModel.Dispatcher
{
	internal class ReplyHandler : BaseRequestProcessorHandler
	{
		IDuplexChannel duplex;

		public ReplyHandler (IChannel channel)
		{
			duplex = channel as IDuplexChannel;
		}

		protected override bool ProcessRequest (MessageProcessingContext mrc)
		{
			// if IsOneWay then no need to handle reply
			if (mrc.Operation.IsOneWay)
				return false;

			if (duplex != null)
				mrc.Reply (duplex, true);
			else
				mrc.Reply (true);
			return false;
		}
	}
}
*/
