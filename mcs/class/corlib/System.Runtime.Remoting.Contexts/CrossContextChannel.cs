//
// System.Runtime.Remoting.Contexts.CrossContextChannel.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002, Lluis Sanchez Gual
//

using System;
using System.Threading;
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

			Context oldContext = null;
			IMessage response;

			if (Threading.Thread.CurrentContext != identity.Context)
				oldContext = Context.SwitchToContext (identity.Context);

			try
			{
				Context.NotifyGlobalDynamicSinks (true, msg, false, false);
				Thread.CurrentContext.NotifyDynamicSinks (true, msg, false, false);

				response = identity.Context.GetServerContextSinkChain().SyncProcessMessage (msg);

				Context.NotifyGlobalDynamicSinks (false, msg, false, false);
				Thread.CurrentContext.NotifyDynamicSinks (false, msg, false, false);
			}
			catch (Exception ex)
			{
				response = new ReturnMessage (ex, (IMethodCallMessage)msg);
			}
			finally
			{
				if (oldContext != null)
					Context.SwitchToContext (oldContext);
			}
			
			return response;
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			ServerIdentity identity = (ServerIdentity) RemotingServices.GetMessageTargetIdentity (msg);
			
			Context oldContext = null;
			if (Threading.Thread.CurrentContext != identity.Context)
				oldContext = Context.SwitchToContext (identity.Context);

			try
			{
				Context.NotifyGlobalDynamicSinks (true, msg, false, true);
				Thread.CurrentContext.NotifyDynamicSinks (true, msg, false, false);
				if (replySink != null) replySink = new ContextRestoreSink (replySink, oldContext, msg);

				IMessageCtrl res = identity.AsyncObjectProcessMessage (msg, replySink);

				if (replySink == null)
				{
					Context.NotifyGlobalDynamicSinks (false, msg, false, false);
					Thread.CurrentContext.NotifyDynamicSinks (false, msg, false, false);
				}

				return res;
			}
			catch (Exception ex)
			{
				if (replySink != null)
					replySink.SyncProcessMessage (new ReturnMessage (ex, (IMethodCallMessage)msg));
				return null;
			}
			finally
			{
				if (oldContext != null)
					Context.SwitchToContext (oldContext);
			}
		}

		public IMessageSink NextSink 
		{ 
			get { return null; }
		}

		class ContextRestoreSink: IMessageSink
		{
			IMessageSink _next;
			Context _context;
			IMessage _call;

			public ContextRestoreSink (IMessageSink next, Context context, IMessage call)
			{
				_next = next;
				_context = context;
				_call = call;
			}

			public IMessage SyncProcessMessage (IMessage msg)
			{
				try
				{
					Context.NotifyGlobalDynamicSinks (false, msg, false, false);
					Thread.CurrentContext.NotifyDynamicSinks (false, msg, false, false);
					return _next.SyncProcessMessage (msg);
				}
				catch (Exception ex)
				{
					return new ReturnMessage (ex, (IMethodCallMessage)_call);
				}
				finally
				{
					if (_context != null)
						Context.SwitchToContext (_context);
				}		
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
