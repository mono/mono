//
// System.Runtime.Remoting.ObjRef.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//
// FIXME: This is just a skeleton for practical purposes.
//

using System;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting {

	[Serializable]
	public class ObjRef : IObjectReference, ISerializable {
		MarshalByRefObject mbr;
		SerializationInfo si;
		string uri;
		Type type;
		
		public ObjRef ()
		{
		}
		
		public ObjRef (MarshalByRefObject mbr, Type type)
		{
			this.mbr = mbr;
			this.type = type;
		}

		[MonoTODO]
		protected ObjRef (SerializationInfo si, StreamingContext sc)
		{
			// FIXME: Implement.
			//
			// This encarnates the object from serialized data.
		}

		[MonoTODO]
		public virtual IChannelInfo ChannelInfo {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public virtual IEnvoyInfo EnvoyInfo {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public virtual IRemotingTypeInfo TypeInfo {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public virtual string URI {
			get {
				return uri;
			}
			set {
				uri = value;
			}
		}

		[MonoTODO]
		public virtual void GetObjectData (SerializationInfo si, StreamingContext sc)
		{
			// FIXME:
		}

		[MonoTODO]
		public virtual object GetRealObject (StreamingContext sc)
		{
			// FIXME:
			
			return null;
		}

		[MonoTODO]
		public bool IsFromThisAppDomain ()
		{
			// FIXME:
			
			return true;
		}

		[MonoTODO]
		public bool IsFromThisProcess ()
		{
			// FIXME:
			
			return true;
		}
	}
}
