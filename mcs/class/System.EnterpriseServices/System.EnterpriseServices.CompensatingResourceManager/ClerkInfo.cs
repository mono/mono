// System.EnterpriseServices.CompensatingResourceManager.ClerkInfo.cs
//
// Author:  Mike Kestner (mkestner@ximian.com)
//
// Copyright (C) 2004 Novell, Inc.
//

using System;

namespace System.EnterpriseServices.CompensatingResourceManager {

	public sealed class ClerkInfo
	{
		[MonoTODO]
		~ClerkInfo ()
		{
			throw new NotImplementedException ();
		}

		#region Constructors

		// FIXME we should actually have this constructor 
		// internal ClerkInfo(System.EnterpriseServices.CompensatingResourceManager.CrmMonitor monitor,
		//		System.EnterpriseServices.CompensatingResourceManager._IMonitorClerks clerks)
		// but we currently don't have these types
		internal ClerkInfo ()
		{
		}

		#endregion Constructors

		#region Properties

		[MonoTODO]
		public string ActivityId {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public Clerk Clerk {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Compensator {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string Description {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string InstanceId {
			get {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string TransactionUOW {
			get {
				throw new NotImplementedException ();
			}
		}

		#endregion

	}
}
