//
// System.Web.Hosting.IAppDomainFactory.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;

namespace System.Web.Hosting
{
        public interface IAppDomainFactory
        {
                object Create (string module,
			       string typeName,
			       string appId,
			       string appPath,
			       string strUrlOfAppOrigin,
			       int iZone);
        }
}

