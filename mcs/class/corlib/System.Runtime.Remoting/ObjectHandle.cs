//
// System.Runtime.Remoting.ObjectHandle.cs
//
// Author:
//   Dietmar Maurer (dietmr@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting {

	public class ObjectHandle : MarshalByRefObject, IObjectReference {
		
		[MonoTODO]
		public ObjectHandle (object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object InitializeLifetimeService ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public object Unwrap ()
		{
			throw new NotImplementedException ();
		}

		public object GetRealObject (StreamingContext context)
		{
			throw new NotImplementedException ();
		}

	}
}
