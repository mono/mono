//
// System.Web.IHttpModule.cs
//
// Author:
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//
namespace System.Web {
   public interface IHttpModule {
      void Dispose();
      void Init(HttpApplication context);
   }
}
