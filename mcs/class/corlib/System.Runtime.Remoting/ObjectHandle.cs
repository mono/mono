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
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting {

	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class ObjectHandle : MarshalByRefObject, IObjectHandle {
		private object _wrapped;
		
		public ObjectHandle (object o)
		{
			_wrapped = o;
		}

		public override object InitializeLifetimeService ()
		{
			return base.InitializeLifetimeService ();
		}

		public object Unwrap ()
		{
			return _wrapped;
		}
	}
}

