//
// System.Runtime.Remoting.Contexts.CrossContextChannel.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2002, Lluis Sanchez Gual
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
