//
// System.Runtime.Remoting.ObjRef.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Dietmar Maurer (dietmar@ximian.com)
//   Lluis Sanchez Gual (lsg@ctv.es)
//
// (C) Ximian, Inc.  http://www.ximian.com
//
//
// FIXME: This is just a skeleton for practical purposes.
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
		Type type;
		
		public ObjRef ()
		{
			// no idea why this needs to be public
		}
		
		public ObjRef (MarshalByRefObject mbr, Type type)
		{
			if (mbr == null)
				throw new ArgumentNullException ("mbr");
			
			if (type == null)
				throw new ArgumentNullException ("type");

			// The ObjRef can only be constructed if the given mbr
			// has already been marshalled using RemotingServices.Marshall

			this.uri = RemotingServices.GetObjectUri(mbr);
			this.type = type;

			channel_info = new ChannelInfoStore ();
		}

		protected ObjRef (SerializationInfo si, StreamingContext sc)
		{
			SerializationInfoEnumerator en = si.GetEnumerator();

			while (en.MoveNext ()) {
				switch (en.Name) {
				case "uri":
					uri = (string)en.Value;
					break;
				case "type":
					type = (Type)en.Value;
					break;
				case "channelInfo":
					type = (Type)en.Value;
					break;
				default:
					throw new NotSupportedException ();
				}
			}
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

		public virtual void GetObjectData (SerializationInfo si, StreamingContext sc)
		{
			si.SetType (type);

			si.AddValue ("url", uri);
			si.AddValue ("type", type, typeof (Type));
			si.AddValue ("channelInfo", channel_info, typeof(IChannelInfo));
		}

		public virtual object GetRealObject (StreamingContext sc)
		{
			return RemotingServices.GetRemoteObject(type, null, channel_info.ChannelData);
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
	}
}
