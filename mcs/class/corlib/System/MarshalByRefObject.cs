//
// System.MarshalByRefObject.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Lluis Sanchez Gual (lsg@ctv.es)
//   Patrik Torstensson (totte_mono@yahoo.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
using System.Threading;
using System.Runtime.Remoting;

namespace System {

	[Serializable]
	public abstract class MarshalByRefObject {
		private Identity _identity;		// Holds marshalling iformation of the object

		internal Identity GetObjectIdentity(MarshalByRefObject obj, out bool IsClient) {
			IsClient = false;
			Identity objId = null;

			if (RemotingServices.IsTransparentProxy(obj)) 
			{
				objId = RemotingServices.GetRealProxy(obj).ObjectIdentity;
				IsClient = true;
			} else 
			{
				objId = obj.ObjectIdentity;
			}

			return objId;
		}

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
