//
// System.Runtime.Remoting.ObjRef.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Dietmar Maurer (dietmar@ximian.com)
//   Lluis Sanchez Gual (lluis@ideary.com)
//   Patrik Torstensson
//
// (C) Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace System.Runtime.Remoting {

	[Serializable]
	public class ObjRef : IObjectReference, ISerializable 
	{
		IChannelInfo channel_info;
		string uri;
		IRemotingTypeInfo typeInfo;
		IEnvoyInfo envoyInfo;
		int flags;

		static int MarshalledObjectRef = 1;
		static int WellKnowObjectRef = 2;
		
		public ObjRef ()
		{
			// no idea why this needs to be public

			UpdateChannelInfo();
		}

		internal ObjRef (ObjRef o, bool unmarshalAsProxy) {
			channel_info = o.channel_info;
			uri = o.uri;
	
			typeInfo = o.typeInfo;
			envoyInfo = o.envoyInfo;
			flags = o.flags;
			if (unmarshalAsProxy) flags |= MarshalledObjectRef;
		}
		
		public ObjRef (MarshalByRefObject mbr, Type type)
		{
			if (mbr == null)
				throw new ArgumentNullException ("mbr");
			
			if (type == null)
				throw new ArgumentNullException ("type");

			// The ObjRef can only be constructed if the given mbr
			// has already been marshalled using RemotingServices.Marshall

			uri = RemotingServices.GetObjectUri(mbr);
			typeInfo = new TypeInfo(type);

			if (!typeInfo.CanCastTo(mbr.GetType(), mbr))
				throw new RemotingException ("The server object type cannot be cast to the requested type " + type.FullName + ".");

			UpdateChannelInfo();
		}

		internal ObjRef (Type type, string url, object remoteChannelData)
		{
			uri = url;
			typeInfo = new TypeInfo(type);

			if (remoteChannelData != null)
				channel_info = new ChannelInfoStore (remoteChannelData);

			flags |= WellKnowObjectRef;
		}

		protected ObjRef (SerializationInfo si, StreamingContext sc)
		{
			SerializationInfoEnumerator en = si.GetEnumerator();
			// Info to serialize: uri, objrefFlags, typeInfo, envoyInfo, channelInfo

			bool marshalledValue = true;

			while (en.MoveNext ()) {
				switch (en.Name) {
				case "uri":
					uri = (string)en.Value;
					break;
				case "typeInfo":
					typeInfo = (IRemotingTypeInfo)en.Value;
					break;
				case "channelInfo":
					channel_info = (IChannelInfo)en.Value;
					break;
				case "envoyInfo":
					envoyInfo = (IEnvoyInfo)en.Value;
					break;
				case "fIsMarshalled":
					int status;
					Object o = en.Value;
					if (o.GetType().Equals(typeof(String)))
						status = ((IConvertible) o).ToInt32(null);
					else
						status = (int) o;

					if (status == 0)
						marshalledValue = false;
					break;
				case "objrefFlags":
					flags = (int) en.Value;
					break;
				default:
					throw new NotSupportedException ();
				}
			}
			if (marshalledValue) flags |= MarshalledObjectRef;
		}

		internal bool IsPossibleToCAD () 
		{
			// we should check if this obj ref belongs to a cross app context.

			// Return false. If not, serialization of this ObjRef will not work
			// on the target AD.
			return false;
		}

		public bool IsReferenceToWellKnow
		{
			get { return (flags & WellKnowObjectRef) > 0; }
		}

		public virtual IChannelInfo ChannelInfo {

			get {
				return channel_info;
			}
			
			set {
				channel_info = value;
			}
		}
		
		[MonoTODO]
		public virtual IEnvoyInfo EnvoyInfo {
			get {
				return envoyInfo;
			}
			set {
				envoyInfo = value;
			}
		}
		
		[MonoTODO]
		public virtual IRemotingTypeInfo TypeInfo {
			get {
				return typeInfo;
			}
			set {
				typeInfo = value;
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

		public virtual void GetObjectData (SerializationInfo si, StreamingContext sc)
		{
			si.SetType (GetType());
			si.AddValue ("uri", uri);
			si.AddValue ("typeInfo", typeInfo, typeof (IRemotingTypeInfo));
			si.AddValue ("envoyInfo", envoyInfo, typeof (IEnvoyInfo));
			si.AddValue ("channelInfo", channel_info, typeof(IChannelInfo));
			si.AddValue ("objrefFlags", flags);
		}

		public virtual object GetRealObject (StreamingContext sc)
		{
			if ((flags & MarshalledObjectRef) > 0)
				return RemotingServices.Unmarshal (this);
			else
				return this;
		}

		public bool IsFromThisAppDomain ()
		{
			Identity identity = RemotingServices.GetIdentityForUri (uri);
			if (identity == null) return false;		// URI not registered in this domain

			return identity.IsFromThisAppDomain;
		}

		[MonoTODO]
		public bool IsFromThisProcess ()
		{
			// as yet we do not consider this optimization
			return false;
		}

		internal void UpdateChannelInfo()
		{
			channel_info = new ChannelInfoStore ();
		}
	}
}
