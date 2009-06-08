using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace System.ServiceModel.Dispatcher 
{	
	internal abstract class BaseRequestProcessorHandler 
	{
		BaseRequestProcessorHandler next;		

		public virtual void ProcessRequestChain (MessageProcessingContext mrc)
		{
			if (!ProcessRequest (mrc) && next != null ) {				
				next.ProcessRequestChain (mrc);
			}
		}

		public BaseRequestProcessorHandler Next
		{
			get { return next; }
			set { next = value; }
		}

		protected abstract bool ProcessRequest (MessageProcessingContext mrc);
	}
}
