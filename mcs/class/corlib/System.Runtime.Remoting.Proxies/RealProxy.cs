//
// System.Runtime.Remoting.Proxies.RealProxy.cs
//
// Authors:
//   Dietmar Maurer (dietmar@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.Remoting.Messaging;

namespace System.Runtime.Remoting.Proxies
{

	public abstract class RealProxy {

		RealProxy () {
			throw new NotImplementedException ();
		}

		RealProxy (Type classToProxy) {
			throw new NotImplementedException ();
		}

		RealProxy (Type classToProxy, IntPtr stub, object stubData) {
			throw new NotImplementedException ();
		}

		public abstract IMessage Invoke (IMessage msg);

		public virtual object GetTransparentProxy () {
			throw new NotImplementedException ();
		}
	}

}
