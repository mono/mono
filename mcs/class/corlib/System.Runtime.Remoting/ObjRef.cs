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

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
		Type _serverType;

		static int MarshalledObjectRef = 1;
		static int WellKnowObjectRef = 2;
		
		public ObjRef ()
		{
			// no idea why this needs to be public

			UpdateChannelInfo();
		}

		internal ObjRef (string typeName, string uri, IChannelInfo cinfo) 
		{
			this.uri = uri;
			channel_info = cinfo;
			typeInfo = new TypeInfo (Type.GetType (typeName));
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

			if (!type.IsAssignableFrom (mbr.GetType()))
				throw new RemotingException ("The server object type cannot be cast to the requested type " + type.FullName + ".");

			UpdateChannelInfo();
		}

		internal ObjRef (Type type, string url, object remoteChannelData)
		{
			uri = url;
			typeInfo = new TypeInfo(type);

			if (remoteChannelData != null)
				channel_info = new ChannelInfo (remoteChannelData);

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
					if (o is string)
						status = ((IConvertible) o).ToInt32(null);
					else
						status = (int) o;

					if (status == 0)
						marshalledValue = false;
					break;
				case "objrefFlags":
					flags = Convert.ToInt32 (en.Value);
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

		internal bool IsReferenceToWellKnow
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
		
		public virtual IEnvoyInfo EnvoyInfo {
			get {
				return envoyInfo;
			}
			set {
				envoyInfo = value;
			}
		}
		
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

		public bool IsFromThisProcess ()
		{
			foreach (object data in channel_info.ChannelData)
			{
				if (data is CrossAppDomainData)
				{
					string refProcId = ((CrossAppDomainData)data).ProcessID;
					return (refProcId == RemotingConfiguration.ProcessId);
				}
			}
			
			return true;
		}

		internal void UpdateChannelInfo()
		{
			channel_info = new ChannelInfo ();
		}

		internal Type ServerType
		{
			get
			{
				if (_serverType == null) _serverType = Type.GetType (typeInfo.TypeName);
				return _serverType;
			}
		}
	}
}
