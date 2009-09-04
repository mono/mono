using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Security;
using System.ServiceModel.Security.Tokens;
using System.Text;

namespace System.ServiceModel.Dispatcher
{
	internal class BaseRequestProcessor
	{
		HandlersChain initialize_handlers_chain = new HandlersChain();
		HandlersChain process_handlers_chain = new HandlersChain ();
		HandlersChain error_handlers_chain = new HandlersChain ();
		HandlersChain finalize_handlers_chain = new HandlersChain ();

		protected BaseRequestProcessor () { }

		protected virtual void ProcessRequest (MessageProcessingContext mrc)
		{
			initialize_handlers_chain.ProcessRequestChain (mrc);

			using (new OperationContextScope (mrc.OperationContext)) {
				try {
					process_handlers_chain.ProcessRequestChain (mrc);
				}
				catch (Exception e) {
					// FIXME: this is not really expected use of ChannelDispatcher.ErrorHandlers.
					// They are now correctly used in process_handler_chain (namely OperationInvokerHandler).
					// For this kind of "outsider" exceptions are actually left thrown
					// (and could even cause server loop crash in .NET).

					Console.WriteLine ("Exception " + e.Message + " " + e.StackTrace);
					mrc.ProcessingException = e;
					error_handlers_chain.ProcessRequestChain (mrc);
				}
				finally {
					finalize_handlers_chain.ProcessRequestChain (mrc);
				}
			}
		}

		public HandlersChain InitializeChain
		{
			get { return initialize_handlers_chain; }
		}

		public HandlersChain ProcessingChain
		{
			get { return process_handlers_chain; }
		}

		public HandlersChain ErrorChain
		{
			get { return error_handlers_chain; }
		}

		public HandlersChain FinalizationChain
		{
			get { return finalize_handlers_chain; }
		}		
	}

	internal class HandlersChain
	{
		BaseRequestProcessorHandler chain;

		public void ProcessRequestChain (MessageProcessingContext mrc)
		{
			if (chain != null)
				chain.ProcessRequestChain (mrc);
		}

		public HandlersChain AddHandler (BaseRequestProcessorHandler handler)
		{
			if (chain == null) {
				chain = handler;
			}
			else {
				BaseRequestProcessorHandler current = chain;
				while (current.Next != null)
					current = current.Next;
				current.Next = handler;
			}
			return this;
		}
	}
}
