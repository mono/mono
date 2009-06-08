using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace System.ServiceModel.Dispatcher
{
	internal class ErrorProcessingHandler : BaseRequestProcessorHandler
	{
		public ErrorProcessingHandler (IChannel channel)
		{
			duplex = channel as IDuplexChannel;
		}

		IDuplexChannel duplex;

		protected override bool ProcessRequest (MessageProcessingContext mrc)
		{
			Exception ex = mrc.ProcessingException;
			DispatchRuntime dispatchRuntime = mrc.OperationContext.EndpointDispatcher.DispatchRuntime;
			
			//invoke all user handlers
			ChannelDispatcher channelDispatcher = dispatchRuntime.ChannelDispatcher;
			foreach (IErrorHandler handler in channelDispatcher.ErrorHandlers)
				if (handler.HandleError (ex))
					break;

			FaultConverter fc = FaultConverter.GetDefaultFaultConverter (dispatchRuntime.ChannelDispatcher.MessageVersion);
			Message res = null;			
			if (!fc.TryCreateFaultMessage (ex, out res))
				throw ex;
			mrc.ReplyMessage = res;
			if (duplex != null)
				mrc.Reply (duplex, true);
			else
				mrc.Reply (true);
			return false;
		}		
	}
}
