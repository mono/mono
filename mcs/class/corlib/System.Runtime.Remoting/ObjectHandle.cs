//
// System.Runtime.Remoting.ObjectHandle.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Gonzalo Paniagua (gonzalo@ximian.com)
//   Jaime Anguiano Olarra (jaime@gnome.org)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

using System;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Lifetime;

namespace System.Runtime.Remoting {

	public class ObjectHandle : MarshalByRefObject, IObjectHandle {
		private object _wrapped;
		private object life_ctrl;
		
		public ObjectHandle (object o)
		{
			_wrapped = o;
		}

		[MonoTODO]
		public override object InitializeLifetimeService ()
		{
			life_ctrl = base.InitializeLifetimeService ();
			ILease ilife_ctrl = life_ctrl as ILease;
			
			if (ilife_ctrl != null)

				// I can't see in the .NET docs if the lifetime counter
				// must be initialized to the time the object is created
				// or if a relativistic time is enough (as both differ in
				// fact just in a constant. In the meantime I'll use 0.
			
				ilife_ctrl.InitialLeaseTime = new TimeSpan ((long) 0);
			
			return ilife_ctrl;
		}

		public object Unwrap ()
		{
			return _wrapped;
		}

	}
}
