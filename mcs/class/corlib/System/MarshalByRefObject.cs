//
// System.MarshalByRefObject.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Jaime Anguiano Olarra (jaime@gnome.org)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Remoting;
using System.Runtime.Remoting.Lifetime;

namespace System {

	[Serializable]
	public abstract class MarshalByRefObject {
		private object life_ctrl;
		private ILease ilife_ctrl;

		[MonoTODO]
		public virtual ObjRef CreateObjRef (Type type)
		{
			throw new NotImplementedException ();
		}

		// LAME SPEC??: It does not say what happens if there is not a current lifetime
		// server object. Should we return null or throw an exception?.
		public object GetLifetimeService ()
		{
			return ilife_ctrl;
		}

		[MonoTODO]
		public virtual object InitializeLifetimeService ()
		{
			life_ctrl = new object ();
			ilife_ctrl = life_ctrl as ILease;
			
			if (ilife_ctrl.CurrentState == LeaseState.Initial) 
				ilife_ctrl.InitialLeaseTime = new TimeSpan ((long) 0);
			else 
				ilife_ctrl.InitialLeaseTime = LifetimeServices.LeaseManagerPollTime;

			return ilife_ctrl;
		}

		[MonoTODO]
		protected MarshalByRefObject ()
		{
		}
	}
}
