using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace System.ServiceModel.Dispatcher
{
	internal class PostReceiveRequestHandler : BaseRequestProcessorHandler
	{
		protected override bool ProcessRequest (MessageProcessingContext mrc)
		{
			Message incomingMessage = mrc.IncomingMessage;
			EnsureInstanceContextOpen (mrc.InstanceContext);
			AfterReceiveRequest (ref incomingMessage, mrc);
			mrc.IncomingMessage = incomingMessage;
			return false;
		}

		void AfterReceiveRequest (ref Message message, MessageProcessingContext mrc)
		{
			mrc.EventsHandler.AfterReceiveRequest ();
		}

		void EnsureInstanceContextOpen (InstanceContext ictx)
		{
			if (ictx.State != CommunicationState.Opened)
				ictx.Open ();
		}
	}
}
