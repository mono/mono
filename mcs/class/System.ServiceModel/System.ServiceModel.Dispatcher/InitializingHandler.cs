using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace System.ServiceModel.Dispatcher
{
	internal class InitializingHandler : BaseRequestProcessorHandler
	{
		protected override bool ProcessRequest (MessageProcessingContext mrc)
		{
			InstanceContext ictx = CreateInstanceContext (mrc);
			mrc.InstanceContext = ictx;
			mrc.OperationContext.InstanceContext = ictx;
			return false;
		}

		InstanceContext CreateInstanceContext (MessageProcessingContext mrc)
		{
			InstanceContext iCtx = null;
			DispatchRuntime dispatchRuntime = mrc.OperationContext.EndpointDispatcher.DispatchRuntime;
			IInstanceContextProvider p = dispatchRuntime.InstanceContextProvider;

			if (p != null) {
				iCtx = p.GetExistingInstanceContext (mrc.IncomingMessage, mrc.OperationContext.Channel);
			}
			if (iCtx == null) {
				ServiceHostBase host = dispatchRuntime.ChannelDispatcher.Host;
				iCtx = new InstanceContext (dispatchRuntime.ChannelDispatcher.Host, null, false);
				// FIXME: could be easier way to identify session channel
				if ((mrc.Channel is ISessionChannel<IInputSession> || mrc.Channel is ISessionChannel<IDuplexSession>) && host.Description.Behaviors.Find<ServiceBehaviorAttribute> ().InstanceContextMode == InstanceContextMode.PerSession)
					mrc.Channel.Closed += delegate { iCtx.Close (); };
			}

			iCtx.InstanceManager = new InstanceManager (dispatchRuntime);
			return iCtx;
		}		
	}
}
