//
// System.Web.Hosting.AppDomainFactory.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;

namespace System.Web.Hosting
{
        public sealed class AppDomainFactory : IAppDomainFactory
        {
                public AppDomainFactory();
                public object Create(string module, string typeName, string appId, string appPath, string strUrlOfAppOrigin, int iZone);
        }
}
