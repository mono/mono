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
	public class HttpWorkerRequest {
        	public delegate void EndOfSendNotification(
                                HttpWorkerRequest wr,
                                object extraData);
	}
}
