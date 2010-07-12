using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel.Channels.Security;
using System.ServiceModel;
using System.ServiceModel.Security.Tokens;

namespace System.ServiceModel.Dispatcher
{
	internal class SecurityHandler : BaseRequestProcessorHandler
	{
		protected override bool ProcessRequest (MessageProcessingContext mrc)
		{
			DispatchRuntime dispatch_runtime = mrc.OperationContext.EndpointDispatcher.DispatchRuntime;

			// FIXME: I doubt this should be done at this "handler" 
			// layer, especially considering about non-ServiceHost
			// use of SecurityBindingElement + listener.
			//
			// For example there is no way to handle it in duplex
			// dispatch callbacks.
			if (dispatch_runtime.ChannelDispatcher == null)
				return false;

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
