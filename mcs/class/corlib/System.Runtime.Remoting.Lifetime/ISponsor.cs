//
// System.Runtime.Remoting.Lifetime.ISponsor.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;
using System.Runtime.Remoting.Lifetime;

namespace System.Runtime.Remoting.Lifetime {

	public interface ISponsor
	{
		TimeSpan Renewal (ILease lease);
	}
}
