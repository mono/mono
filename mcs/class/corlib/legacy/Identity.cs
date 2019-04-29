using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.Remoting.Contexts;

namespace System.Runtime.Remoting
{
	internal abstract class Identity
	{
#region Still need to remove this

		protected string _objectUri;
		protected IMessageSink _channelSink = null;
		protected IMessageSink _envoySink = null;
		protected ObjRef _objRef;

#endregion

		public abstract ObjRef CreateObjRef (Type requestedType);

		public bool IsFromThisAppDomain => throw new PlatformNotSupportedException ();

		public IMessageSink ChannelSink {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public IMessageSink EnvoySink => throw new PlatformNotSupportedException ();

		public string ObjectUri {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}

		public bool IsConnected => throw new PlatformNotSupportedException ();

		public bool Disposed {
			get => throw new PlatformNotSupportedException ();
			set => throw new PlatformNotSupportedException ();
		}
	}
}
