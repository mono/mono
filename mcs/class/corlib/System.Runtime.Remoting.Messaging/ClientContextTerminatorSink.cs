//
// System.Runtime.Remoting.Messaging.ClientContextTerminatorSink.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
//

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting.Messaging
{
	public class ClientContextTerminatorSink: IMessageSink
	{
		Context _context;

		public ClientContextTerminatorSink(Context ctx)
		{
			_context = ctx;
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			IMessage res;

			Context.NotifyGlobalDynamicSinks (true, msg, true, false);
			_context.NotifyDynamicSinks (true, msg, true, false);

			if (msg is IConstructionCallMessage)
			{
				res = ActivationServices.RemoteActivate ((IConstructionCallMessage)msg);
			}
			else
			{
				Identity identity = RemotingServices.GetMessageTargetIdentity (msg);
				res = identity.ChannelSink.SyncProcessMessage (msg);
			}

			Context.NotifyGlobalDynamicSinks (false, msg, true, false);
			_context.NotifyDynamicSinks (false, msg, true, false);

			return res;
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			if (_context.HasDynamicSinks || Context.HasGlobalDynamicSinks)
			{
				Context.NotifyGlobalDynamicSinks (true, msg, true, true);
				_context.NotifyDynamicSinks (true, msg, true, true);

				// replySink is null when calling a one way method
				if (replySink != null) replySink = new ClientContextReplySink (_context, replySink);
			}
			Identity identity = RemotingServices.GetMessageTargetIdentity (msg);
			IMessageCtrl res = identity.ChannelSink.AsyncProcessMessage (msg, replySink);

			if (replySink == null && (_context.HasDynamicSinks || Context.HasGlobalDynamicSinks))
			{
				Context.NotifyGlobalDynamicSinks (false, msg, true, true);
				_context.NotifyDynamicSinks (false, msg, true, true);
			}
			return res;
		}

		public IMessageSink NextSink 
		{ 
			get { return null; }
		}	
	}

	class ClientContextReplySink: IMessageSink
	{
		IMessageSink _replySink;
		Context _context;

		public ClientContextReplySink (Context ctx, IMessageSink replySink)
		{
			_replySink = replySink;
			_context = ctx;
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			Context.NotifyGlobalDynamicSinks (false, msg, true, true);
			_context.NotifyDynamicSinks (false, msg, true, true);
			return _replySink.SyncProcessMessage (msg);
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			throw new NotSupportedException ();
		}	

		public IMessageSink NextSink 
		{ 
			get { return _replySink; }
		}	
	}
}
