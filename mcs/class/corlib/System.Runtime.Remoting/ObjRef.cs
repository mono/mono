//
// System.Runtime.Remoting.ObjRef.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//
// FIXME: This is just a skeleton for practical purposes.
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting {

	public class ObjRef : IObjectReference, ISerializable {
		MarshalByRefObject mbr;
		SerializationInfo si;
		Type type;
		
		public ObjRef ()
		{
		}
		
		public ObjRef (MarshalByRefObject mbr, Type type)
		{
			this.mbr = mbr;
			this.type = type;
		}

		public ObjRef (SerializationInfo si, StreamingContext sc)
		{
			// FIXME: Implement.
			//
			// This encarnates the object from serialized data.
		}

		public virtual void GetObjectData (SerializationInfo si, StreamingContext sc)
		{
			// FIXME:
		}

		public virtual object GetRealObject (StreamingContext sc)
		{
			// FIXME:
			
			return null;
		}

		public bool IsFromThisAppDomain ()
		{
			// FIXME:
			
			return true;
		}

		public bool IsFromThisProcess ()
		{
			// FIXME:
			
			return true;
		}
	}
}
