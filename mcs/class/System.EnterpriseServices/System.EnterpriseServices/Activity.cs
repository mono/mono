// System.EnterpriseServices.Activity.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;
using System.Runtime.InteropServices;

namespace System.EnterpriseServices {

#if NET_1_1
	[MonoTODO]
	[ComVisible(false)]
	public sealed class Activity {

		#region Constructors

		[MonoTODO]
		public Activity (ServiceConfig cfg)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Methods

		[MonoTODO]
		public void AsynchronousCall (IServiceCall serviceCall)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void BindToCurrentThread ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SynchronousCall (IServiceCall serviceCall)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void UnbindFromThread ()
		{
			throw new NotImplementedException ();
		}

		#endregion
	}
#endif
}
