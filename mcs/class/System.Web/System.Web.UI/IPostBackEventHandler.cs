//
// System.Web.UI.IPostBackEventHandler.cs
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
        public interface IPostBackEventHandler
        {
                void RaisePostBackEvent(string eventArgument);
        }
}
