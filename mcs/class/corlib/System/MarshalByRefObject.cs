//
// System.MarshalByRefObject.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System {

	public abstract class MarshalByRefObject {

		public virtual ObjRef CreateObjRef (Type type)
		{
			return null;
		}

		public object GetLifeTimeService ()
		{
			return null;
		}

		public virtual object InitializeLifeTimeService ()
		{
			return null;
		}
	}
}
