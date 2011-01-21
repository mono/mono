using System;
using System.Collections.Generic;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Channels;
using System.Text;

namespace System.ServiceModel.Dispatcher
{
	internal class InstanceManager
	{
		DispatchRuntime dispatch_runtime;

		internal InstanceManager (DispatchRuntime runtime) {
			dispatch_runtime = runtime;
		}

		internal void Initialize (InstanceContext iCtx) {
			Message message = OperationContext.Current.IncomingMessage;
			IContextChannel channel = OperationContext.Current.Channel;
			if (dispatch_runtime.InstanceContextProvider != null) {
				dispatch_runtime.InstanceContextProvider.InitializeInstanceContext (iCtx,
													message,
													channel);
			}
			foreach (IInstanceContextInitializer init in dispatch_runtime.InstanceContextInitializers)
				init.Initialize (iCtx, message);					
		}

		internal object GetServiceInstance (InstanceContext ctx, Message m, ref bool createdByUserProvider) {
			if (dispatch_runtime.InstanceProvider != null) {
				createdByUserProvider = true;
				return dispatch_runtime.InstanceProvider.GetInstance (ctx, m);
			}
			createdByUserProvider = false;
			return Activator.CreateInstance (
				dispatch_runtime.ChannelDispatcher.Host.Description.ServiceType, true);
		}

		internal IInstanceContextProvider InstanceContextProvider {
			get {
				return dispatch_runtime.InstanceContextProvider;
			}
		}

		internal void ReleaseServiceInstance (InstanceContext ctx, object impl) 
		{
			if (ctx.IsUserProvidedInstance) {
				dispatch_runtime.InstanceProvider.ReleaseInstance (ctx, impl);
				
			}
		}
	}
}
