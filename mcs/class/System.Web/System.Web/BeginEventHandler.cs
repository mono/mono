//
// System.Web.BeginEventHandler.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

namespace System.Web
{
        public delegate IAsyncResult BeginEventHandler(object sender,
                                                       EventArgs e,
                                                       AsyncCallback cb,
                                                       object extraData);
}
