//
// System.Runtime.Remoting.Contexts.CrossContextChannel.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting.Contexts
{
	internal class CrossContextChannel: IMessageSink
	{
		public IMessage SyncProcessMessage (IMessage msg)
		{
			ServerIdentity identity = (ServerIdentity) RemotingServices.GetMessageTargetIdentity (msg);

			if (Threading.Thread.CurrentContext != identity.Context)
			{
				// Context switch needed

				Context oldContext = Context.SwitchToContext (identity.Context);
				IMessage response = identity.Context.GetServerContextSinkChain().SyncProcessMessage (msg);
				Context.SwitchToContext (oldContext);
				return response;
			}
			else
				return identity.Context.GetServerContextSinkChain().SyncProcessMessage (msg);
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			ServerIdentity identity = (ServerIdentity) RemotingServices.GetMessageTargetIdentity (msg);
			
			if (Threading.Thread.CurrentContext != identity.Context)
			{
				Context oldContext = Context.SwitchToContext (identity.Context);
				replySink = new ContextRestoreSink (replySink, oldContext);
			}
			
			return identity.AsyncObjectProcessMessage (msg, replySink);
		}

		public IMessageSink NextSink 
		{ 
			get { return null; }
		}

		class ContextRestoreSink: IMessageSink
		{
			IMessageSink _next;
			Context _context;

			public ContextRestoreSink (IMessageSink next, Context context)
			{
				_next = next;
				_context = context;
			}

			public IMessage SyncProcessMessage (IMessage msg)
			{
				Context.SwitchToContext (_context);
				return _next.SyncProcessMessage (msg);
			}

			public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
			{
				throw new NotSupportedException();	// Not needed
			}

			public IMessageSink NextSink 
			{ 
				get { return _next; }
			}		
		}

	}
}
