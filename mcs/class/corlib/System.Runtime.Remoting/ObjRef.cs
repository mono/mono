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
using System.Runtime.Remoting.Channels;

namespace System.Runtime.Remoting {

	[Serializable]
	public class ObjRef : IObjectReference, ISerializable {
		MarshalByRefObject mbr;
		IChannelInfo chnl_info;
		string uri;
		Type type;

		
		public ObjRef ()
		{
			// no idea why this needs to be public
		}
		
		public ObjRef (MarshalByRefObject mbr, Type type)
		{
			this.mbr = mbr;
			this.type = type;

			chnl_info = new ChannelInfoStore ();
		}

		[MonoTODO]
		protected ObjRef (SerializationInfo si, StreamingContext sc)
		{
			// FIXME: Implement.
			//
			// This encarnates the object from serialized data.
		}

		public virtual IChannelInfo ChannelInfo {

			get {
				return chnl_info;
			}
			
			set {
				chnl_info = value;
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

		public virtual object GetRealObject (StreamingContext sc)
		{
			if (IsFromThisAppDomain ())
				return mbr;
			
			// FIXME:
			return null;
		}

		public bool IsFromThisAppDomain ()
		{
			return (mbr != null);
		}

		[MonoTODO]
		public bool IsFromThisProcess ()
		{
			// FIXME:
			
			return true;
		}
	}
}
