// System.EnterpriseServices.ServiceDomain.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {

#if NET_1_1
	[ComVisible(false)]
	public sealed class ServiceDomain {
		#region Constructors

		private ServiceDomain ()
		{
		}

		#endregion Constructors

		#region Methods

		[MonoTODO]
		public static void Enter (ServiceConfig cfg)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static TransactionStatus Leave ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
#endif
}
