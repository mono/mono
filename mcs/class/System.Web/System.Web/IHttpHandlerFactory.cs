//
// System.Web.IHttpHandlerFactory.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//
namespace System.Web
{
   public interface IHttpHandlerFactory
   {
      IHttpHandler GetHandler(HttpContext context,
                              string requestType,
                              string url,
                              string pathTranslated);
      void ReleaseHandler(IHttpHandler handler);
   }
}
