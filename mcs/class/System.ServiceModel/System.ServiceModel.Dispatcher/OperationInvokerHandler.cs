using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Reflection;

namespace System.ServiceModel.Dispatcher
{
	internal class OperationInvokerHandler : BaseRequestProcessorHandler
	{
		protected override bool ProcessRequest (MessageProcessingContext mrc)
		{			
			RequestContext rc = mrc.RequestContext;
			DispatchRuntime dispatchRuntime = mrc.OperationContext.EndpointDispatcher.DispatchRuntime;
			DispatchOperation operation = GetOperation (mrc.IncomingMessage, dispatchRuntime);
			mrc.Operation = operation;
			try {				
				DoProcessRequest (mrc);
				if (!operation.Invoker.IsSynchronous)
					return true;
			} catch (TargetInvocationException ex) {
				mrc.ReplyMessage = BuildExceptionMessage (mrc.IncomingMessage, ex.InnerException, 
					dispatchRuntime.ChannelDispatcher.IncludeExceptionDetailInFaults);
			}
			return false;
		}

		void DoProcessRequest (MessageProcessingContext mrc)
		{
			DispatchOperation operation = mrc.Operation;
			Message req = mrc.IncomingMessage;
			object instance = mrc.InstanceContext.GetServiceInstance(req);
			object [] parameters;			
			BuildInvokeParams (mrc, out parameters);

			if (operation.Invoker.IsSynchronous) {
				object result = operation.Invoker.Invoke (instance, parameters);
				HandleInvokeResult (mrc, parameters, result);
			} else {// asynchronous
				InvokeAsynchronous (mrc, instance, parameters);
			}			
		}

		void InvokeAsynchronous (MessageProcessingContext mrc, object instance, object [] parameters)
		{
			DispatchOperation operation = mrc.Operation;
			DispatchRuntime dispatchRuntime = mrc.OperationContext.EndpointDispatcher.DispatchRuntime;
			operation.Invoker.InvokeBegin (instance, parameters,
					delegate (IAsyncResult res) {						
						try {
							object result;
							result = operation.Invoker.InvokeEnd (instance, out parameters, res);
							HandleInvokeResult (mrc, parameters, result);
							mrc.Reply (true);
						} catch (Exception ex) {
							mrc.ReplyMessage = BuildExceptionMessage (mrc.IncomingMessage, ex, dispatchRuntime.ChannelDispatcher.IncludeExceptionDetailInFaults);
							mrc.Reply (false);
						}				
					},
					null);			
		}

		DispatchOperation GetOperation (Message input, DispatchRuntime dispatchRuntime)
		{
			if (dispatchRuntime.OperationSelector != null) {
				string name = dispatchRuntime.OperationSelector.SelectOperation (ref input);
				foreach (DispatchOperation d in dispatchRuntime.Operations)
					if (d.Name == name)
						return d;
			} else {
				string action = input.Headers.Action;
				foreach (DispatchOperation d in dispatchRuntime.Operations)
					if (d.Action == action)
						return d;
			}
			return dispatchRuntime.UnhandledDispatchOperation;
		}

		void HandleInvokeResult (MessageProcessingContext mrc, object [] outputs, object result)
		{
			DispatchOperation operation = mrc.Operation;
			mrc.EventsHandler.AfterInvoke (operation);

			Message res = null;
			if (operation.SerializeReply)
				res = operation.Formatter.SerializeReply (
					mrc.OperationContext.EndpointDispatcher.ChannelDispatcher.MessageVersion, outputs, result);
			else
				res = (Message) result;
			res.Headers.CopyHeadersFrom (mrc.OperationContext.OutgoingMessageHeaders);
			res.Properties.CopyProperties (mrc.OperationContext.OutgoingMessageProperties);
			mrc.ReplyMessage = res;
		}

		Message CreateActionNotSupported (Message req)
		{
			FaultCode fc = new FaultCode (
				req.Version.Addressing.ActionNotSupported,
				req.Version.Addressing.Namespace);
			// FIXME: set correct namespace URI
			return Message.CreateMessage (req.Version, fc,
				String.Format ("action '{0}' is not supported in this service contract.", req.Headers.Action), String.Empty);
		}

		void BuildInvokeParams (MessageProcessingContext mrc, out object [] parameters)
		{
			DispatchOperation operation = mrc.Operation;
			EnsureValid (operation);

			if (operation.DeserializeRequest) {
				parameters = operation.Invoker.AllocateParameters ();
				operation.Formatter.DeserializeRequest (mrc.IncomingMessage, parameters);
			} else
				parameters = new object [] { mrc.IncomingMessage };

			mrc.EventsHandler.BeforeInvoke (operation);
		}

		Message BuildExceptionMessage (Message req, Exception ex, bool includeDetailsInFault)
		{			
			// FIXME: set correct name
			FaultCode fc = new FaultCode (
				"InternalServiceFault",
				req.Version.Addressing.Namespace);


			if (includeDetailsInFault) {
				return Message.CreateMessage (req.Version, fc, ex.Message, new ExceptionDetail (ex), req.Headers.Action);
			}
			// MS returns: The server was unable to process the request due to an internal error.  For more information about the error, either turn on IncludeExceptionDetailInFaults (either from ServiceBehaviorAttribute or from the &lt;serviceDebug&gt; configuration behavior) on the server in order to send the exception information back to the client, or turn on tracing as per the Microsoft .NET Framework 3.0 SDK documentation and inspect the server trace logs.";
			//
			string faultString =
				@"The server was unable to process the request due to an internal error.  The server may be able to return exception details (it depends on the server settings).";
			return Message.CreateMessage (req.Version, fc, faultString, req.Headers.Action);
		}

		void EnsureValid (DispatchOperation operation)
		{
			if (operation.Invoker == null)
				throw new InvalidOperationException ("DispatchOperation requires Invoker.");
			if ((operation.DeserializeRequest || operation.SerializeReply) && operation.Formatter == null)
				throw new InvalidOperationException ("The DispatchOperation '" + operation.Name + "' requires Formatter, since DeserializeRequest and SerializeReply are not both false.");
		}		
	}
}
