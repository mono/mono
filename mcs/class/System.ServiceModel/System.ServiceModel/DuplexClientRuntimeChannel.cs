//
// DuplexClientRuntimeChannel.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Security;
using System.Threading;
using System.Xml;

namespace System.ServiceModel.MonoInternal
{
#if DISABLE_REAL_PROXY
	// FIXME: This is a quick workaround for bug #571907
	public
#endif
	class DuplexClientRuntimeChannel
		: ClientRuntimeChannel, IDuplexContextChannel
	{
		public DuplexClientRuntimeChannel (ServiceEndpoint endpoint,
			ChannelFactory factory, EndpointAddress remoteAddress, Uri via)
			: base (endpoint, factory, remoteAddress, via)
		{
			var ed = new EndpointDispatcher (remoteAddress, endpoint.Contract.Name, endpoint.Contract.Namespace);
			ed.InitializeServiceEndpoint (true, null, endpoint);
			Runtime.CallbackDispatchRuntime = ed.DispatchRuntime;
		}

		public bool AutomaticInputSessionShutdown {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		InstanceContext callback_instance;

		public InstanceContext CallbackInstance {
			get { return callback_instance; }
			set {
				callback_instance = value;
				Runtime.CallbackDispatchRuntime.InstanceContextProvider = new CallbackInstanceContextProvider (callback_instance);
			}
		}

		Action<TimeSpan> session_shutdown_delegate;

		public void CloseOutputSession (TimeSpan timeout)
		{
			throw new NotImplementedException ();
		}

		public IAsyncResult BeginCloseOutputSession (TimeSpan timeout, AsyncCallback callback, object state)
		{
			if (session_shutdown_delegate == null)
				session_shutdown_delegate = new Action<TimeSpan> (CloseOutputSession);
			return session_shutdown_delegate.BeginInvoke (timeout, callback, state);
		}

		public void EndCloseOutputSession (IAsyncResult result)
		{
			session_shutdown_delegate.EndInvoke (result);
		}

		// listener loop manager

		bool loop;

		TimeSpan receive_timeout;
		bool receive_synchronously = true; // FIXME

		IAsyncResult loop_result;
		AutoResetEvent loop_handle = new AutoResetEvent (false);
		AutoResetEvent finish_handle = new AutoResetEvent (false);
		AutoResetEvent receive_reply_handle = new AutoResetEvent (false);

		protected override void OnOpen (TimeSpan timeout)
		{
			loop = true;
			base.OnOpen (timeout);
			receive_timeout = TimeSpan.FromSeconds (10);
		}

		protected override void OnOpened ()
		{
			base.OnOpened ();
			loop_result = new Action<IInputChannel> (ProcessRequestOrInput).BeginInvoke (DuplexChannel, null, null);
		}

		protected override void OnClose (TimeSpan timeout)
		{
			DateTime start = DateTime.UtcNow;
			base.OnClose (timeout);
			loop = false;
			if (!loop_handle.WaitOne (timeout - (DateTime.UtcNow - start)))
				throw new TimeoutException ();
			if (!finish_handle.WaitOne (timeout - (DateTime.UtcNow - start)))
				throw new TimeoutException ();
		}

		void ProcessRequestOrInput (IInputChannel input)
		{
			while (true) {
				if (!loop)
					return;

				if (receive_synchronously) {
					Message msg;
					if (input.TryReceive (receive_timeout, out msg))
						ProcessInput (input, msg);
				} else {
					input.BeginTryReceive (receive_timeout, TryReceiveDone, input);
					loop_handle.WaitOne (receive_timeout);
				}
			}
		}

		void TryReceiveDone (IAsyncResult result)
		{
			try {
				Message msg;
				var input = (IInputChannel) result.AsyncState;
				if (input.EndTryReceive (result, out msg)) {
					loop_handle.Set ();
					ProcessInput (input, msg);
				}
			} catch (Exception ex) {
				// FIXME: rather log it
				Console.WriteLine ("Error at duplex client receiver side");
				Console.WriteLine (ex);
				loop = false;
			}
		}

		void ProcessInputCore (IInputChannel input, Message message)
		{
				bool isReply = message != null && Contract.Operations.Any (od => (od.DeclaringContract.CallbackContractType == od.DeclaringContract.ContractType || !od.InCallbackContract) && od.Messages.Any (md => md.Action == message.Headers.Action));
				if (isReply) {
					if (ReplyHandlerQueue.Count > 0) {
						if (isReply) {
							var h = ReplyHandlerQueue.Dequeue ();
							h (message);
							return;
						}
					}
				}
				
				if (message.IsFault) {
					Exception ex;
					var mf = MessageFault.CreateFault (message, 0x10000);
					if (FaultConverter.GetDefaultFaultConverter (message.Version).TryCreateException (message, mf, out ex)) // FIXME: get maxMessageSize somehow
						throw ex;
					else
						throw new FaultException (mf);
				}
				
				if (!MessageMatchesEndpointDispatcher (message, Runtime.CallbackDispatchRuntime.EndpointDispatcher))
					throw new EndpointNotFoundException (String.Format ("The request message has the target '{0}' with action '{1}' which is not reachable in this service contract", message.Headers.To, message.Headers.Action));
				new InputOrReplyRequestProcessor (Runtime.CallbackDispatchRuntime, input).ProcessInput (message);
		}

		void ProcessInput (IInputChannel input, Message message)
		{
			try {
				ProcessInputCore (input, message);
			} catch (Exception ex) {
				// FIXME: log it.
				Console.WriteLine (ex);
			}
		}

		bool MessageMatchesEndpointDispatcher (Message req, EndpointDispatcher endpoint)
		{
			// FIXME: no need to filter address? It'd be mostly anonymous URI though.

			return endpoint.ContractFilter.Match (req);
		}
		
		internal override Message RequestCorrelated (Message msg, TimeSpan timeout, IOutputChannel channel)
		{
			DateTime startTime = DateTime.UtcNow;
			Message ret = null;
			ManualResetEvent wait = new ManualResetEvent (false);
			Action<Message> handler = delegate (Message reply) {
				ret = reply;
				wait.Set ();
			};
			ReplyHandlerQueue.Enqueue (handler);
			channel.Send (msg, timeout);
			if (ret == null && !wait.WaitOne (timeout - (DateTime.UtcNow - startTime)))
				throw new TimeoutException ();
			return ret;
		}
		
		internal Queue<Action<Message>> ReplyHandlerQueue = new Queue<Action<Message>> ();
	}
}
