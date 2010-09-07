//
// System.Runtime.Remoting.StackBuilderSink.cs
//
// Author: Lluis Sanchez Gual (lsg@ctv.es)
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
using System.Reflection;
using System.Runtime.Remoting.Proxies;

namespace System.Runtime.Remoting.Messaging
{
	// Sink that calls the real method of the object

	internal class StackBuilderSink: IMessageSink
	{
		MarshalByRefObject _target;
		RealProxy _rp;

		public StackBuilderSink (MarshalByRefObject obj, bool forceInternalExecute)
		{
			_target = obj;
			if (!forceInternalExecute && RemotingServices.IsTransparentProxy (obj))
				_rp = RemotingServices.GetRealProxy (obj);
		}

		public IMessage SyncProcessMessage (IMessage msg)
		{
			CheckParameters (msg);

			// Makes the real call to the object
			if (_rp != null) return _rp.Invoke (msg);
			else return RemotingServices.InternalExecuteMessage (_target, (IMethodCallMessage)msg);
		}

		public IMessageCtrl AsyncProcessMessage (IMessage msg, IMessageSink replySink)
		{
			object[] parms = new object[] {msg, replySink};
			ThreadPool.QueueUserWorkItem (new WaitCallback ((data) => {
				try {
					ExecuteAsyncMessage (data);
				} catch {}
				}
				), parms);
			return null;
		}
		
		void ExecuteAsyncMessage (object ob)
		{
			object[] parms = (object[]) ob;
			IMethodCallMessage msg = (IMethodCallMessage) parms[0];
			IMessageSink replySink = (IMessageSink)parms[1];
			
			CheckParameters (msg);
			
			IMessage res;
			if (_rp != null) res = _rp.Invoke (msg);
			else res = RemotingServices.InternalExecuteMessage (_target, msg);
			
			replySink.SyncProcessMessage (res);
		}

		public IMessageSink NextSink 
		{ 
			get { return null; }
		}

		void CheckParameters (IMessage msg)
		{
			IMethodCallMessage mcm = (IMethodCallMessage) msg;
			
			ParameterInfo[] parameters = mcm.MethodBase.GetParameters();
			int narg = 0;

			foreach (ParameterInfo pi in parameters)
			{
				object pval = mcm.GetArg (narg++);
				Type pt = pi.ParameterType;
				if (pt.IsByRef) pt = pt.GetElementType ();
				
				if (pval != null && !pt.IsInstanceOfType (pval))
					throw new RemotingException ("Cannot cast argument " + pi.Position + " of type '" + pval.GetType().AssemblyQualifiedName +
						"' to type '" + pt.AssemblyQualifiedName + "'");
			}
		}
	}
}
