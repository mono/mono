//
// System.Runtime.Remoting.Services.ITrackingHandler.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright, Ximian, Inc.
//

using System.Runtime.Remoting;

namespace System.Runtime.Remoting.Services {
	public interface ITrackingHandler
	{
		void DisconnectedObject (object obj);
		void MarshaledObject (object obj, ObjRef or);
		void UnmarshaledObject (object obj, ObjRef or);
	}
}

