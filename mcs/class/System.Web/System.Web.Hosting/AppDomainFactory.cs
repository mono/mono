//
// System.Web.Hosting.AppDomainFactory.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//   Gonzalo Paniagua (gonzalo@ximian.com)
//
// (C) Bob Smith
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;

namespace System.Web.Hosting
{
        public sealed class AppDomainFactory : IAppDomainFactory
        {
		[MonoTODO]
                public AppDomainFactory ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
                public object Create (string module,
				      string typeName,
				      string appId,
				      string appPath,
				      string strUrlOfAppOrigin,
				      int iZone)
		{
			throw new NotImplementedException ();
		}
        }
}
