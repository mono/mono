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
                object Create(in string module, in string typeName, in string appId, in string appPath, in string strUrlOfAppOrigin, in int iZone);
        }
}
