//
// System.MarshalByRefObject.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Remoting;

namespace System {

	[Serializable]
	public abstract class MarshalByRefObject {

		public virtual ObjRef CreateObjRef (Type type)
		{
			throw new NotImplementedException ();
		}

		public object GetLifetimeService ()
		{
			return null;
		}

		public virtual object InitializeLifetimeService ()
		{
			return null;
		}

		protected MarshalByRefObject ()
		{
		}
	}
}
