//
// System.IHttpHandler.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

namespace System.Web
{
        public interface IHttpHandler
        {
                bool IsReusable {get;}
                void ProcessRequest(HttpContext context);
        }
}
