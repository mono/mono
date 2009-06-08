using System;
using System.Collections.Generic;
using System.Text;

namespace System.ServiceModel.Dispatcher
{
	internal class OperationContextScopeHandler : BaseRequestProcessorHandler
	{
		public override void ProcessRequestChain (MessageProcessingContext mrc)
		{
			using (new OperationContextScope (mrc.OperationContext)) {
				base.ProcessRequestChain (mrc);
			}
		}

		protected override bool ProcessRequest (MessageProcessingContext mrc)
		{
			return false;
		}		
	}
}
