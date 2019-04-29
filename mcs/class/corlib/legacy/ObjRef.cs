using System;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting {

	[Serializable]
	public class ObjRef : IObjectReference, ISerializable
	{
		public ObjRef ()
		{
			throw new PlatformNotSupportedException ();
		}

		internal ObjRef DeserializeInTheCurrentDomain (int domainId, byte[] tInfo)
		{
			throw new PlatformNotSupportedException ();
		}

		internal byte[] SerializeType ()
		{
			throw new PlatformNotSupportedException ();
		}

		internal ObjRef (ObjRef o, bool unmarshalAsProxy)
		{
			throw new PlatformNotSupportedException ();
		}

		public ObjRef (MarshalByRefObject o, Type requestedType)
		{
			throw new PlatformNotSupportedException ();
		}

		internal ObjRef (Type type, string url, object remoteChannelData)
		{
			throw new PlatformNotSupportedException ();
		}

		protected ObjRef (SerializationInfo info, StreamingContext context)
		{
			throw new PlatformNotSupportedException ();
		}

		internal bool IsPossibleToCAD ()
		{
			throw new PlatformNotSupportedException ();
		}

		internal bool IsReferenceToWellKnow => throw new PlatformNotSupportedException ();

		public virtual IRemotingTypeInfo TypeInfo {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public virtual string URI {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public virtual void GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new PlatformNotSupportedException ();
		}

		public virtual object GetRealObject (StreamingContext context)
		{
			throw new PlatformNotSupportedException ();
		}

		public bool IsFromThisAppDomain ()
		{
			throw new PlatformNotSupportedException ();
		}

		public bool IsFromThisProcess ()
		{
			throw new PlatformNotSupportedException ();
		}

		internal void UpdateChannelInfo()
		{
			throw new PlatformNotSupportedException ();
		}

		internal Type ServerType => throw new PlatformNotSupportedException ();

		internal void SetDomainID (int id)
		{
			throw new PlatformNotSupportedException ();
		}
	}
}
