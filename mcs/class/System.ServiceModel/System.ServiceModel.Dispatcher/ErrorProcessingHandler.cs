using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace System.ServiceModel.Dispatcher
{
	internal class ErrorProcessingHandler : BaseRequestProcessorHandler
	{
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
			mrc.Reply (true);
			return false;
		}		
	}
}
