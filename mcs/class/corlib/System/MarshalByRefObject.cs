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
			return null;
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
