using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Reflection;
using System.Threading;

namespace System.ServiceModel.Dispatcher
{
	internal class OperationInvokerHandler : BaseRequestProcessorHandler
	{
		IDuplexChannel duplex;

		public OperationInvokerHandler (IChannel channel)
		{
			duplex = channel as IDuplexChannel;
		}

		protected override bool ProcessRequest (MessageProcessingContext mrc)
		{			
			RequestContext rc = mrc.RequestContext;
			DispatchRuntime dispatchRuntime = mrc.OperationContext.EndpointDispatcher.DispatchRuntime;
			DispatchOperation operation = GetOperation (mrc.IncomingMessage, dispatchRuntime);
			mrc.Operation = operation;
			try {				
				DoProcessRequest (mrc);
				if (!mrc.Operation.IsOneWay)
					Reply (mrc, true);
			} catch (TargetInvocationException ex) {
				mrc.ReplyMessage = BuildExceptionMessage (mrc, ex.InnerException, 
					dispatchRuntime.ChannelDispatcher.IncludeExceptionDetailInFaults);
				if (!mrc.Operation.IsOneWay)
					Reply (mrc, true);
				ProcessCustomErrorHandlers (mrc, ex);
			}
			return false;
		}

		void DoProcessRequest (MessageProcessingContext mrc)
		{
			DispatchOperation operation = mrc.Operation;
			Message req = mrc.IncomingMessage;
			object instance = mrc.InstanceContext.GetServiceInstance(req);
			object [] parameters, outParams;
			BuildInvokeParams (mrc, out parameters);

			if (operation.Invoker.IsSynchronous) {
				object result = operation.Invoker.Invoke (instance, parameters, out outParams);
				HandleInvokeResult (mrc, outParams, result);
			} else {
				AsyncCallback callback = delegate {};
				// FIXME: the original code passed null callback
				// and null state, which is very wrong :(
				// It is still wrong to pass dummy callback, but
				// wrong code without obvious issues is better
				// than code with an obvious issue.
				var ar = operation.Invoker.InvokeBegin (instance, parameters, callback, null);
				object result = operation.Invoker.InvokeEnd (instance, out outParams, ar);
				HandleInvokeResult (mrc, outParams, result);
			}
		}

		void Reply (MessageProcessingContext mrc, bool useTimeout)
		{
			if (duplex != null)
				mrc.Reply (duplex, useTimeout);
			else
				mrc.Reply (useTimeout);
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

			if (operation.IsOneWay)
				return;

			Message res = null;
			if (operation.SerializeReply)
				res = operation.Formatter.SerializeReply (
					mrc.OperationContext.IncomingMessageVersion, outputs, result);
			else
				res = (Message) result;
			res.Headers.CopyHeadersFrom (mrc.OperationContext.OutgoingMessageHeaders);
			res.Properties.CopyProperties (mrc.OperationContext.OutgoingMessageProperties);
			if (res.Headers.RelatesTo == null)
				 res.Headers.RelatesTo = mrc.OperationContext.IncomingMessageHeaders.MessageId;
			mrc.ReplyMessage = res;
		}

		void BuildInvokeParams (MessageProcessingContext mrc, out object [] parameters)
		{
			DispatchOperation operation = mrc.Operation;
			EnsureValid (operation);

			if (operation.DeserializeRequest) {
				parameters = operation.Invoker.AllocateInputs ();
				operation.Formatter.DeserializeRequest (mrc.IncomingMessage, parameters);
			} else
				parameters = new object [] { mrc.IncomingMessage };

			mrc.EventsHandler.BeforeInvoke (operation);
		}

		void ProcessCustomErrorHandlers (MessageProcessingContext mrc, Exception ex)
		{
			var dr = mrc.OperationContext.EndpointDispatcher.DispatchRuntime;
			bool shutdown = false;
			if (dr.ChannelDispatcher != null) // non-callback channel
				foreach (var eh in dr.ChannelDispatcher.ErrorHandlers)
					shutdown |= eh.HandleError (ex);
			if (shutdown)
				ProcessSessionErrorShutdown (mrc);
		}

		void ProcessSessionErrorShutdown (MessageProcessingContext mrc)
		{
			var dr = mrc.OperationContext.EndpointDispatcher.DispatchRuntime;
			var session = mrc.OperationContext.Channel.InputSession;
			var dcc = mrc.OperationContext.Channel as IDuplexContextChannel;
			if (session == null || dcc == null)
				return;
			foreach (var h in dr.InputSessionShutdownHandlers)
				h.ChannelFaulted (dcc);
		}

		bool IsGenericFaultException (Type type, out Type arg)
		{
			for (; type != null; type = type.BaseType) {
				if (!type.IsGenericType)
					continue;
				var tdef = type.GetGenericTypeDefinition ();
				if (!tdef.Equals (typeof (FaultException<>)))
					continue;
				arg = type.GetGenericArguments () [0];
				return true;
			}

			arg = null;
			return false;
		}

		Message BuildExceptionMessage (MessageProcessingContext mrc, Exception ex, bool includeDetailsInFault)
		{
			var dr = mrc.OperationContext.EndpointDispatcher.DispatchRuntime;
			var cd = dr.ChannelDispatcher;
			Message msg = null;
			if (cd != null) // non-callback channel
				foreach (var eh in cd.ErrorHandlers)
					eh.ProvideFault (ex, cd.MessageVersion, ref msg);
			if (msg != null)
				return msg;

			var req = mrc.IncomingMessage;

			Type gft;
			var fe = ex as FaultException;
			if (fe != null && IsGenericFaultException (fe.GetType (), out gft)) {
				foreach (var fci in mrc.Operation.FaultContractInfos) {
					if (fci.Detail == gft)
						return Message.CreateMessage (req.Version, fe.CreateMessageFault (), fci.Action);
				}
			}

			// FIXME: set correct name
			FaultCode fc = new FaultCode (
				"InternalServiceFault",
				req.Version.Addressing.Namespace);


			if (includeDetailsInFault) {
				return Message.CreateMessage (req.Version, fc, ex.Message, new ExceptionDetail (ex), req.Headers.Action);
			}

			string faultString =
				@"The server was unable to process the request due to an internal error.  The server may be able to return exception details (it depends on the server settings).";
			return Message.CreateMessage (req.Version, fc, faultString, req.Headers.Action);
		}

		void EnsureValid (DispatchOperation operation)
		{
			if (operation.Invoker == null)
				throw new InvalidOperationException (String.Format ("DispatchOperation '{0}' for contract '{1}' requires Invoker.", operation.Name, operation.Parent.EndpointDispatcher.ContractName));
			if ((operation.DeserializeRequest || operation.SerializeReply) && operation.Formatter == null)
				throw new InvalidOperationException ("The DispatchOperation '" + operation.Name + "' requires Formatter, since DeserializeRequest and SerializeReply are not both false.");
		}		
	}
}
