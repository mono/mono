   // 
// System.Web.HttpApplication
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.ComponentModel;
using System.Web.SessionState;

namespace System.Web {
   [MonoTODO()]
   public class HttpApplication : IHttpAsyncHandler, IHttpHandler, IComponent, IDisposable {
      private bool _CompleteRequest;

      private HttpContext _Context;
      private HttpContext _OverrideContext;
         
      private bool _InPreRequestResponseMode;

      private ISite _Site;
      private HttpModuleCollection _ModuleCollection;
      private HttpSessionState _Session;

      public event EventHandler AcquireRequestState;
      public event EventHandler AuthenticateRequest;
      public event EventHandler AuthorizeRequest;
      public event EventHandler BeginRequest;
      public event EventHandler Disposed;
      public event EventHandler EndRequest;
      public event EventHandler Error;
      public event EventHandler PostRequestHandlerExecute;
      public event EventHandler PreRequestHandlerExecute;
      public event EventHandler PreSendRequestContent;
      public event EventHandler PreSendRequestHeaders;
      public event EventHandler ReleaseRequestState;
      public event EventHandler ResolveRequestCache;
      public event EventHandler UpdateRequestCache;

      [MonoTODO()]
      public HttpApplication() {
         // Init HTTP context and the methods from HttpRuntime....
      }

      internal void ClearError() {
         // Called from Server Utility
      }

      public HttpContext Context {
         get {
            if (null != _OverrideContext) {
               return _OverrideContext;
            }

            return _Context;
         }
      }

      public HttpModuleCollection Modules {
         get {
            if (null == _ModuleCollection) {
               _ModuleCollection = new HttpModuleCollection();
            }

            return _ModuleCollection;
         }
      }

      public HttpRequest Request {
         get {
            if (null != _Context && (!_InPreRequestResponseMode)) {
               return _Context.Request;
            }

            throw new HttpException("Cant get request object");
         }
      }

      public HttpResponse Response {
         get {
            if (null != _Context && (!_InPreRequestResponseMode)) {
               return _Context.Response;
            }

            throw new HttpException("Cant get response object");
         }
      }

      public HttpServerUtility Server {
         get {
            if (null != _Context) {
               return _Context.Server;
            }

            return new HttpServerUtility(this);
         }
      }

      public HttpSessionState Session {
         get {
            if (null != _Session) {
               return _Session;
            }

            if (null != _Context && null != _Context.Session) {
               return _Context.Session;
            }

            throw new HttpException("Failed to get session object");
         }
      }

      public virtual string GetVaryByCustomString(HttpContext context, string custom) {
         if (custom.ToLower() == "browser") {
            return context.Request.Browser.Type;
         }

         return string.Empty;
      }

      [MonoTODO()]
      IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) {
         throw new NotImplementedException();
      }

      [MonoTODO()]
      void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result) {
         throw new NotImplementedException();
      }

      [MonoTODO()]
      void IHttpHandler.ProcessRequest(HttpContext context) {
         throw new NotImplementedException();
      }

      bool IHttpHandler.IsReusable {
         get {
            throw new NotImplementedException();
         }
      }

      public ISite Site {
         get {
            return _Site;
         }

         set {
            _Site = value;
         }
      }

      public void CompleteRequest() {
         _CompleteRequest = true;
      }

      [MonoTODO("Cleanup")]
      public virtual void Dispose() {
         
      }

      public virtual void Init() {
      }
   }
}
