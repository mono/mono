using System;
using System.Collections.Generic;
using System.Text;

namespace System.ServiceModel.Dispatcher
{
	class DefaultInstanceContextProvider : IInstanceContextProvider
	{		

		public InstanceContext GetExistingInstanceContext (System.ServiceModel.Channels.Message message, IContextChannel channel) {
			return null;
		}

		public void InitializeInstanceContext (InstanceContext instanceContext, System.ServiceModel.Channels.Message message, IContextChannel channel) {
			
		}

		public bool IsIdle (InstanceContext instanceContext) {
			return true;
		}

		public void NotifyIdle (InstanceContextIdleCallback callback, InstanceContext instanceContext) {
			
		}
	
	}
}
