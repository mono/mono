//
// ClientRealProxy.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2011 Novell, Inc.  http://www.novell.com
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
#if !DISABLE_REAL_PROXY
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.MonoInternal;
using System.Threading;

namespace System.ServiceModel
{
	class ClientRealProxy : RealProxy, IRemotingTypeInfo
	{
		public ClientRealProxy (Type type, IInternalContextChannel channel, bool isDuplex)
			: base (type)
		{
			this.channel = channel;
#if NET_2_1
			context_channel_type = typeof (IClientChannel);
#else
			context_channel_type = isDuplex ? typeof (IDuplexContextChannel) : typeof (IClientChannel);
#endif
		}
		
		Type context_channel_type;
		IInternalContextChannel channel;
		Dictionary<object,object[]> saved_params = new Dictionary<object,object[]> ();

		// It is used for such case that EndProcess() gets invoked
		// before storing params is done after BeginProcess().
		ManualResetEvent wait = new ManualResetEvent (false);

		#region IRemotingTypeInfo

		public virtual string TypeName { get; set; }

		public virtual bool CanCastTo (Type t, object o)
		{
			if (t == context_channel_type || context_channel_type.GetInterfaces ().Contains (t))
				return true;
			return false;
		}
		
		#endregion
		
		public override IMessage Invoke (IMessage inputMessage)
		{
			try {
				return DoInvoke (inputMessage);
			} catch (TargetInvocationException ex) {
				if (ex.InnerException != null)
					throw ex.InnerException;
				throw;
			}
		}

		IMessage DoInvoke (IMessage inputMessage)
		{
			var inmsg = (IMethodCallMessage) inputMessage;
			var od = channel.Contract.Operations.FirstOrDefault (o => inmsg.MethodBase.Equals (o.SyncMethod) || inmsg.MethodBase.Equals (o.BeginMethod) || inmsg.MethodBase.Equals (o.EndMethod));
			if (od == null) {
				// Then IContextChannel methods.
				var ret = inmsg.MethodBase.Invoke (channel, inmsg.InArgs);
				return new ReturnMessage (ret, null, 0, null, inmsg);
			} else {
				object [] pl;
				MethodBase method = null;
				List<object> outArgs = null;
				object ret;
				if (inmsg.MethodBase.Equals (od.SyncMethod)) {
					// sync invocation
					pl = new object [inmsg.MethodBase.GetParameters ().Length];
					Array.Copy (inmsg.Args, pl, inmsg.ArgCount);
					ret = channel.Process (inmsg.MethodBase, od.Name, pl);
					method = od.SyncMethod;
				} else if (inmsg.MethodBase.Equals (od.BeginMethod)) {
					// async invocation
					pl = new object [inmsg.ArgCount - 2];
					Array.Copy (inmsg.Args, 0, pl, 0, pl.Length);

					ret = channel.BeginProcess (inmsg.MethodBase, od.Name, pl, (AsyncCallback) inmsg.Args [inmsg.ArgCount - 2], inmsg.Args [inmsg.ArgCount - 1]);
					saved_params [ret] = pl;

					wait.Set ();

				} else {
					var result = (IAsyncResult) inmsg.InArgs [0];

					wait.WaitOne ();
					pl = saved_params [result];
					wait.Reset ();
					saved_params.Remove (result);
					ret = channel.EndProcess (inmsg.MethodBase, od.Name, pl, result);
					method = od.BeginMethod;
				}
				
				if (method != null && method.GetParameters ().Any (pi => pi.IsOut))
					return new ReturnMessage (ret, pl, pl.Length, null, inmsg);
				else
					return new ReturnMessage (ret, outArgs != null ? outArgs.ToArray () : null, outArgs != null ? outArgs.Count : 0, null, inmsg);
			}
		}
	}
}
#endif
