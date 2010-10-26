//
// System.Runtime.Remoting.Messaging.ClientContextTerminatorSink.cs
//
// Author: Lluis Sanchez Gual (lluis@ideary.com)
//
// (C) 2003, Lluis Sanchez Gual
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
using System.Runtime.Remoting;
using System.Runtime.Remoting.Activation;
using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting.Messaging
{
	internal class ClientContextTerminatorSink: IMessageSink
	{
		Context _context;

		public ClientContextTerminatorSink(Context ctx)
		{
			_context = ctx;
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			IMessage res = null;

			Context.NotifyGlobalDynamicSinks (true, msg, true, false);
			_context.NotifyDynamicSinks (true, msg, true, false);

			if (msg is IConstructionCallMessage)
			{
				res = ActivationServices.RemoteActivate ((IConstructionCallMessage)msg);
			}
			#if !DISABLE_REMOTING
			else
			{
				Identity identity = RemotingServices.GetMessageTargetIdentity (msg);
				res = identity.ChannelSink.SyncProcessMessage (msg);
			}
			#endif

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
			
			#if !DISABLE_REMOTING
			Identity identity = RemotingServices.GetMessageTargetIdentity (msg);
			IMessageCtrl res = identity.ChannelSink.AsyncProcessMessage (msg, replySink);

			if (replySink == null && (_context.HasDynamicSinks || Context.HasGlobalDynamicSinks))
			{
				Context.NotifyGlobalDynamicSinks (false, msg, true, true);
				_context.NotifyDynamicSinks (false, msg, true, true);
			}
			return res;
			#else
			return null;
			#endif
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
