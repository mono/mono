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
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace System.Runtime.Remoting {

	[Serializable]
	public class ObjRef : IObjectReference, ISerializable {
		MarshalByRefObject mbr;
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

			this.mbr = mbr;
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
					mbr = RemotingServices.GetServerForUri (uri);
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
			if (IsFromThisAppDomain ())
				return mbr;

			object [] channel_data = channel_info.ChannelData;
			IChannel[] channels = ChannelServices.RegisteredChannels;
			
			IMessageSink sink = null;

			foreach (object data in channel_data) {
				foreach (IChannel channel in channels) {
					IChannelSender sender = channel as IChannelSender;
					if (sender == null)
						continue;

					string object_uri;
					if ((sink = sender.CreateMessageSink (null, data, out object_uri)) != null)
						break;
				}
				if (sink != null)
					break;
			}

			if (sink == null)
				throw new RemotingException ("Cannot create channel sink");

			RemotingProxy real_proxy = new RemotingProxy (type, sink);

			return real_proxy.GetTransparentProxy ();
		}

		public bool IsFromThisAppDomain ()
		{
			return (mbr != null);
		}

		public bool IsFromThisProcess ()
		{
			// as yet we do not consider this optimization
			return false;
		}
	}
}
