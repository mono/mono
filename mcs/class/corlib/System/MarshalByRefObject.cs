//
// System.MarshalByRefObject.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System.Runtime.Remoting;

namespace System {

	[Serializable]
	public abstract class MarshalByRefObject 
	{
		Identity _identity;		// Holds marshalling iformation of the object

		internal Identity ObjectIdentity
		{
			get { return _identity; }
			set { _identity = value; }
		}

		public virtual ObjRef CreateObjRef (Type type)
		{
			// This method can only be called when this object has been marshalled
			if (_identity == null) throw new RemotingException ("No remoting information was found for the object");
			return _identity.CreateObjRef (type);
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
