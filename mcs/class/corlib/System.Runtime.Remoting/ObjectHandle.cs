//
// System.Runtime.Remoting.ObjectHandle.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//   Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting {

	public class ObjectHandle : MarshalByRefObject, IObjectHandle {
		private object _wrapped;
		
		public ObjectHandle (object o)
		{
			_wrapped = o;
		}

		[MonoTODO]
		public override object InitializeLifetimeService ()
		{
			throw new NotImplementedException ();
		}

		public object Unwrap ()
		{
			return _wrapped;
		}

	}
}
