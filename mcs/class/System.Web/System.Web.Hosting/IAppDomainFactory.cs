//
// System.Web.Hosting.IAppDomainFactory.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;
using System.Runtime.InteropServices;

namespace System.Web.Hosting
{
	[Guid ("e6e21054-a7dc-4378-877d-b7f4a2d7e8ba")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
        public interface IAppDomainFactory
        {
                object Create ([In] string module,
			       [In] string typeName,
			       [In] string appId,
			       [In] string appPath,
			       [In] string strUrlOfAppOrigin,
			       [In] int iZone);
        }
}

