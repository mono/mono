//
// System.Runtime.Remoting.Contexts.IContextPropertyActivator..cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Runtime.Remoting.Activation;

namespace System.Runtime.Remoting.Contexts {

	public interface IContextPropertyActivator
	{
		void CollectFromClientContext (IConstructionCallMessage msg);
		void CollectFromServerContext (IConstructionReturnMessage msg);
		bool DeliverClientContextToServerContext (IConstructionCallMessage msg);
		bool DeliverServerContextToClientContext (IConstructionReturnMessage msg);
		bool IsOKToActivate (IConstructionCallMessage msg);
	}
}
