//
// System.Web.IHttpAsyncHandler.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

namespace System.Web
{
        public interface IHttpAsyncHandler : IHttpHandler
        {
                IAsyncResult BeginProcessRequest(HttpContext context,
                                                 AsyncCallback cb,
                                                 object extraData);
                void EndProcessRequest(IAsyncResult result);
        }
}
