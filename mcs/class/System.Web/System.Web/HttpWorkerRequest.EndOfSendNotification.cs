//
// System.Web.HttpWorkerRequest.EndOfSendNotification.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

namespace System.Web
{
        public delegate void HttpWorkerRequest.EndOfSendNotification(
                                HttpWorkerRequest wr,
                                object extraData);
}
