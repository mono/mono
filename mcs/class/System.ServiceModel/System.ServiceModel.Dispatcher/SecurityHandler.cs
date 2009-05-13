using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Dispatcher
{
	internal class SecurityHandler : BaseRequestProcessorHandler
	{
		protected override bool ProcessRequest (MessageProcessingContext mrc)
		{
			DispatchRuntime dispatch_runtime = mrc.OperationContext.EndpointDispatcher.DispatchRuntime;
			Message negoResponce = null;
			// process WS-Trust based negotiation
			MessageSecurityBindingSupport support =
				dispatch_runtime.ChannelDispatcher.Listener.GetProperty<MessageSecurityBindingSupport> ();
			if (support != null && mrc.IncomingMessage.Headers.FindHeader ("Security", Constants.WssNamespace) < 0) {
				CommunicationSecurityTokenAuthenticator nego =
					support.TokenAuthenticator as CommunicationSecurityTokenAuthenticator;
				if (nego != null)
					negoResponce = nego.Communication.ProcessNegotiation (mrc.IncomingMessage);
			}

			if (negoResponce == null)
				return false;
			
			ReplyNegoResponse (mrc, negoResponce);
			return true;

		}

		void ReplyNegoResponse (MessageProcessingContext mrc, Message negoResponse)
		{
			negoResponse.Headers.CopyHeadersFrom (mrc.OperationContext.OutgoingMessageHeaders);
			negoResponse.Properties.CopyProperties (mrc.OperationContext.OutgoingMessageProperties);			
			mrc.RequestContext.Reply (negoResponse, mrc.Operation.Parent.ChannelDispatcher.timeouts.SendTimeout);
			return;
		}
	}
}
