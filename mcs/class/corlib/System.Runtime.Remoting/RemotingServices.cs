//
// System.Runtime.Remoting.RemotingServices.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Channels;
using System.Runtime.CompilerServices;

namespace System.Runtime.Remoting
{
	public sealed class RemotingServices {

		private RemotingServices () {}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern static object InternalExecute (MonoMethod method, Object obj,
							       Object[] parameters, out object [] out_args);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public extern static bool IsTransparentProxy (object proxy);
		
		public static IMethodReturnMessage ExecuteMessage (
		        MarshalByRefObject target, IMethodCallMessage reqMsg)
		{
			ReturnMessage result;
			
			MonoMethod method = (MonoMethod)reqMsg.MethodBase;

			try {
				object [] out_args;
				object rval = InternalExecute (method, target, reqMsg.Args, out out_args);
				result = new ReturnMessage (rval, out_args, out_args.Length,
							    reqMsg.LogicalCallContext, reqMsg);
			
			} catch (Exception e) {
				result = new ReturnMessage (e, reqMsg);
			}

			return result;
		}

		public static object Connect (Type classToProxy, string url)
		{
			IMessageSink sink = null;
			string uri;
			
			IChannel [] channels = ChannelServices.RegisteredChannels;

			foreach (IChannel c in channels) {
				IChannelSender sender = c as IChannelSender;
				
				if (sender != null) {
					sink = sender.CreateMessageSink (url, null, out uri);

					if (sink != null)
						break;
				}
			}

			if (sink == null) {
				string msg = String.Format ("Cannot create channel sink to connect to URL {0}.", url); 
				throw new RemotingException (msg);
			}

			RemotingProxy real_proxy = new RemotingProxy (classToProxy, sink);

			return real_proxy.GetTransparentProxy ();
		}
	}
}
