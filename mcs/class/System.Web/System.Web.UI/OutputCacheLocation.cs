//
// System.Web.UI.OutputCacheLocation.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;
using System.Web;

namespace System.Web.UI
{
        public enum OutputCacheLocation
        {
                Any,
                Client,
                Downstream,
                Server,
                None
        }
}
