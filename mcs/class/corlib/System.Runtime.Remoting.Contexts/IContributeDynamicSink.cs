//
// System.Runtime.Remoting.Contexts.IContributeDynamicSink.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting.Contexts {

	public interface IContributeDynamicSink
	{
		IDynamicMessageSink GetDynamicSink ();
	}
}
