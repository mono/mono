//
// System.Runtime.Remoting.Lifetime.ILease.cs
//
// Author: Duncan Mak  (duncan@ximian.com)
//
// 2002 (C) Copyright. Ximian, Inc.
//

using System;
using System.Runtime.Remoting.Lifetime;

namespace System.Runtime.Remoting.Lifetime {

	public interface ILease
	{
		TimeSpan CurrentLeaseTime { get; }
		LeaseState CurrentLeaseState { get; }
		TimeSpan InitialLeaseTime { get; set; }
		TimeSpan RenewOnCallTime { get; set; }
		TimeSpan SponsorshipTimeout {get; set; }
	}
}
