// 
// System.Web.HttpContext
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//
using System;
using System.Collections;
using System.Security.Principal;
using System.Threading;
using System.Web.Caching;
using System.Web.SessionState;

namespace System.Web {
   [MonoTODO("HttpContext - Should also keep the script timeout info")]
   public sealed class HttpContext : IServiceProvider {
      private Exception []	_arrExceptions;

      private HttpResponse	_oResponse;
      private HttpRequest _oRequest;
      private HttpServerUtility _Server;
      private HttpApplication _oApplication;
      private IHttpHandler _Handler;
      private IPrincipal _User;
      
      private Hashtable		_oItems;
      private DateTime		_oTimestamp;

      public HttpContext(HttpRequest Request, HttpResponse Response) {
         Context = this;

         _arrExceptions = null;
         _oItems = null;
         _oTimestamp = DateTime.Now;
         _oRequest = Request;
         _oResponse = Response;
      }

      public HttpContext(HttpWorkerRequest WorkerRequest) {
         Context = this;

         _arrExceptions = null;
         _oItems = null;
         _oTimestamp = DateTime.Now;
         _oRequest = new HttpRequest(WorkerRequest, this);
         _oResponse = new HttpResponse(WorkerRequest, this);
      }
      
      [MonoTODO("Context - Use System.Remoting.Messaging.CallContext instead of Thread storage")]
      internal static HttpContext Context {
         get {
            return (HttpContext) Thread.GetData(Thread.GetNamedDataSlot("Context"));
         }

         set {
            Thread.SetData(Thread.GetNamedDataSlot("Context"), value);
         }
      }

      public Exception [] AllErrors {
         get {
            return _arrExceptions;
         }
      }

      [MonoTODO("HttpApplicationState Application")]
      public HttpApplicationState Application {
         get {
            // Should get the state from a app factory (or the app it self) static method?
            throw new NotImplementedException();
         }
      }

      public HttpApplication ApplicationInstance {
         get {
            return _oApplication;
         }
         set {
            _oApplication = value;
         }
      }

      [MonoTODO("HttpCache Cache")]
      public Cache Cache {
         get {
            // Get the cache from the runtime
            throw new NotImplementedException();
         }
      }

      public static HttpContext Current {
         get {
            return Context;
         }
      }

      public Exception Error {
         get {
            if (_arrExceptions == null) {
               return null;
            } 
            else {
               return _arrExceptions[0];
            }
         }
      }

      public IHttpHandler Handler {
         get {    
            return _Handler;
         }
         
         set {
            _Handler = value;
         }
      }

      [MonoTODO("bool IsCustomErrorEnabled")]
      public bool IsCustomErrorEnabled {
         get {
            throw new NotImplementedException();
         }
      }

      [MonoTODO("bool IsDebuggingEnabled")]
      public bool IsDebuggingEnabled {
         get {
            throw new NotImplementedException();
         }
      }

      public IDictionary Items {
         get {
            if (_oItems == null) {
               _oItems = new Hashtable();
            }
				
            return _oItems;
         }
      }

      public HttpRequest Request {
         get {
            return _oRequest;
         }
      }

      public HttpResponse Response {
         get {
            return _oResponse;
         }
      }

      public HttpServerUtility Server {
         get {
            if (null == _Server) {
               _Server = new HttpServerUtility(this);
            }
            
            return _Server; 
         }
      }
      
      [MonoTODO("HttpSessionState Session")]
      public HttpSessionState Session {
         get {
            throw new NotImplementedException();
         }
      }

      [MonoTODO("bool SkipAuthorization")]
      public bool SkipAuthorization {
         get {
            throw new NotImplementedException();
         }

         set {
            throw new NotImplementedException();
         }
      }

      public DateTime Timestamp {
         get {
            return _oTimestamp;
         }
      }
      
      [MonoTODO("TraceContext Trace")]
      public TraceContext Trace {
         get {
            // TODO: Should be initialized in the constructor (holds current trace)
            throw new NotImplementedException();
         }
      }
      
      public IPrincipal User {
         get {
            return _User;
         }
         set {
            // TODO: Should check security (ControlPrincipal flag)
            _User = value;
         }
      }
		
      public void AddError(Exception errorInfo) {
         int iSize;

         if (_arrExceptions == null) {
            iSize = 1;
         } 
         else {
            iSize = _arrExceptions.Length + 1;
         }

         Exception [] arrNew = new Exception[iSize];

         _arrExceptions.CopyTo(arrNew, 0);
         _arrExceptions = arrNew;

         _arrExceptions[iSize - 1] = errorInfo;
      }

      public void ClearError() {
         _arrExceptions = null;
      }

      [MonoTODO("GetConfig(string name)")]
      public object GetConfig(string name) {
         throw new NotImplementedException();
      }

      [MonoTODO("GetAppConfig(string name)")]
      public static object GetAppConfig(string name) {
         throw new NotImplementedException();
      }
      
      [MonoTODO("IServiceProvider.GetService(Type service)")]
      object IServiceProvider.GetService(Type service) {
         throw new NotImplementedException();
      }

      [MonoTODO("void RewritePath(string path)")]
      public void RewritePath(string Path) {
         throw new NotImplementedException();
      }
   }
}
