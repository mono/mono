using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;

namespace System.ServiceModel.Dispatcher
{
	internal class FinalizeProcessingHandler : BaseRequestProcessorHandler
	{
		protected override bool ProcessRequest (MessageProcessingContext mrc)
		{
			FinishRequest (mrc);
			return false;
		}

		void FinishRequest (MessageProcessingContext mrc)
		{
			if (mrc.Operation != null &&  mrc.Operation.IsTerminating && mrc.OperationContext.Channel.InputSession != null)
				mrc.OperationContext.Channel.Close (); // FIXME: timeout?
			if (mrc.Operation != null &&  mrc.Operation.ReleaseInstanceAfterCall)
				mrc.InstanceContext.ReleaseServiceInstance ();
			mrc.InstanceContext.CloseIfIdle ();			
		}
	}
}
