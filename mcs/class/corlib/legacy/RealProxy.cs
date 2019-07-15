
using System;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Contexts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting.Proxies
{
	[StructLayout (LayoutKind.Sequential)]
	internal class TransparentProxy
	{
#region Keep this code, it is used by the runtime
#pragma warning disable 169, 649
		public RealProxy _rp;
		Mono.RuntimeRemoteClassHandle _class;
		bool _custom_type_info;

		internal object LoadRemoteFieldNew (IntPtr classPtr, IntPtr fieldPtr)
		{
			throw new PlatformNotSupportedException ();
		}

		internal void StoreRemoteField (IntPtr classPtr, IntPtr fieldPtr, object arg)
		{
			throw new PlatformNotSupportedException ();
		}
#pragma warning restore 169, 649
#endregion
	}

	[StructLayout (LayoutKind.Sequential)]
	public abstract class RealProxy
	{
#region Keep this code, it is used by the runtime
#pragma warning disable 169, 649
		Type class_to_proxy;
		internal Context _targetContext;
		internal MarshalByRefObject _server;
		int _targetDomainId = -1;
		internal string _targetUri;
		internal Object _objectIdentity;
		Object _objTP;
		object _stubData;

		internal object GetAppDomainTarget ()
		{
			throw new PlatformNotSupportedException ();
		}

		internal static object PrivateInvoke (RealProxy rp, IMessage msg, out Exception exc,
						      out object [] out_args)
		{
			throw new PlatformNotSupportedException ();
		}

#pragma warning disable 169, 649
#endregion

		RealProxy ()
		{
			throw new PlatformNotSupportedException ();
		}
	}
}
