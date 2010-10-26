//
// System.Runtime.Remoting.Activation.ContextLevelActivator.cs
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
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting.Activation
{
	[Serializable]
	internal class ContextLevelActivator: IActivator
	{
		IActivator m_NextActivator;

		public ContextLevelActivator (IActivator next)
		{
			m_NextActivator = next;
		}

		public ActivatorLevel Level 
		{
			get { return ActivatorLevel.Context; }
		}

		public IActivator NextActivator 
		{
			get { return m_NextActivator; }
			set { m_NextActivator = value; }
		}

		public IConstructionReturnMessage Activate (IConstructionCallMessage ctorCall)
		{
			#if !DISABLE_REMOTING
			ServerIdentity identity = RemotingServices.CreateContextBoundObjectIdentity (ctorCall.ActivationType);
			RemotingServices.SetMessageTargetIdentity (ctorCall, identity);

			ConstructionCall call = ctorCall as ConstructionCall;
			if (call == null || !call.IsContextOk)
			{
				identity.Context = Context.CreateNewContext (ctorCall);
				Context oldContext = Context.SwitchToContext (identity.Context);

				try
				{
					return m_NextActivator.Activate (ctorCall);
				}
				finally
				{
					Context.SwitchToContext (oldContext);
				}
			}
			else
			#endif
				return m_NextActivator.Activate (ctorCall);
		}
	}
}
