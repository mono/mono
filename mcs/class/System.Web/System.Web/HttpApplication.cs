// 
// System.Web.HttpApplication
//
// Authors:
// 	Patrik Torstensson (Patrik.Torstensson@labs2.com)
// 	Tim Coleman (tim@timcoleman.com)
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
using System;
using System.ComponentModel;
using System.Security.Principal;
using System.Web.SessionState;

namespace System.Web {
	[ToolboxItem (true)]
	[MonoTODO()]
	public class HttpApplication : IHttpAsyncHandler, IHttpHandler, IComponent, IDisposable {

		#region Fields

		bool _CompleteRequest;

		HttpContext _Context;
		HttpContext _OverrideContext;
         
		bool _InPreRequestResponseMode;

		ISite _Site;
		HttpModuleCollection _ModuleCollection;
		HttpSessionState _Session;

		object evAuthenticateRequest = new object();
		object evAuthorizeRequest = new object();
		object evBeginRequest = new object();
		object evDisposed = new object();
		object evEndRequest = new object();
		object evError = new object();
		object evPostRequestHandlerExecute = new object();
		object evPreRequestHandlerExecute = new object();
		object evPreSendRequestContent = new object();
		object evPreSendRequestHeaders = new object();
		object evReleaseRequestState = new object();
		object evResolveRequestCache = new object();
		object evUpdateRequestCache = new object();
		object evDefaultAuthentication = new object();
		object evAcquireRequestState = new object();
		EventHandlerList events;

		#endregion // Fields

		#region Constructors

		public HttpApplication() 
		{
		}

		#endregion // Constructors

		#region Properties

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpApplicationState Application {
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpContext Context {
			get {
				if (null != _OverrideContext) 
					return _OverrideContext;
				return _Context;
			}
		}

		protected EventHandlerList Events {
			get {
				if (events == null)
					events = new EventHandlerList ();
				return events;
			}
		}

		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpModuleCollection Modules {
			get {
				if (null == _ModuleCollection) 
					_ModuleCollection = new HttpModuleCollection();
            			return _ModuleCollection;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpRequest Request {
			get {
				if (null != _Context && (!_InPreRequestResponseMode)) 
					return _Context.Request;
				throw new HttpException("Cant get request object");
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpResponse Response {
			get {
				if (null != _Context && (!_InPreRequestResponseMode)) 
					return _Context.Response;
				throw new HttpException("Cant get response object");
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpServerUtility Server {
			get {
				if (null != _Context) 
					return _Context.Server;
				return new HttpServerUtility(this);
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public HttpSessionState Session {
			get {
				if (null != _Session) 
					return _Session;
				if (null != _Context && null != _Context.Session) 
					return _Context.Session;
				throw new HttpException("Failed to get session object");
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ISite Site {
			get { return _Site; }
			set { _Site = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public IPrincipal User {
			get { return _Context.User; }
		}

		#endregion Properties

		#region Methods

		[MonoTODO ("Implementation required.")]
		public void AddOnAcquireRequestStateAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implementation required.")]
		public void AddOnAuthenticateRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implementation required.")]
		public void AddOnAuthorizeRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implementation required.")]
		public void AddOnBeginRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implementation required.")]
		public void AddOnEndRequestAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implementation required.")]
		public void AddOnPostRequestHandlerExecuteAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implementation required.")]
		public void AddOnPreRequestHandlerExecuteAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implementation required.")]
		public void AddOnReleaseRequestStateAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implementation required.")]
		public void AddOnResolveRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Implementation required.")]
		public void AddOnUpdateRequestCacheAsync (BeginEventHandler bh, EndEventHandler eh)
		{
			throw new NotImplementedException ();
		}

		internal void ClearError() 
		{
			// Called from Server Utility
		}

		public void CompleteRequest () 
		{
			_CompleteRequest = true;
		}

		public virtual void Dispose () 
		{
			EventHandler eh = (EventHandler) Events [evDisposed];
			if (eh != null)
				eh.Invoke (this, EventArgs.Empty);
		}

		public virtual string GetVaryByCustomString (HttpContext context, string custom) 
		{
			if (custom.ToLower() == "browser") 
				return context.Request.Browser.Type;
			return string.Empty;
		}

		[MonoTODO()]
		IAsyncResult IHttpAsyncHandler.BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData) 
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		void IHttpAsyncHandler.EndProcessRequest(IAsyncResult result) 
		{
			throw new NotImplementedException();
		}

		[MonoTODO()]
		void IHttpHandler.ProcessRequest(HttpContext context) 
		{
			throw new NotImplementedException();
		}

		public virtual void Init() 
		{
		}

		#endregion // Methods

		#region Events and Delegates

		public event EventHandler AcquireRequestState
		{
			add {
				Events.AddHandler (evAcquireRequestState, value);
			}

			remove {
				Events.RemoveHandler (evAcquireRequestState, value);
			}
		}
		
		public event EventHandler AuthenticateRequest
		{
			add {
				Events.AddHandler (evAuthenticateRequest, value);
			}

			remove {
				Events.RemoveHandler (evAuthenticateRequest, value);
			}
		}
		public event EventHandler AuthorizeRequest
		{
			add {
				Events.AddHandler (evAuthorizeRequest, value);
			}

			remove {
				Events.RemoveHandler (evAuthorizeRequest, value);
			}
		}
		public event EventHandler BeginRequest
		{
			add {
				Events.AddHandler (evBeginRequest, value);
			}

			remove {
				Events.RemoveHandler (evBeginRequest, value);
			}
		}
		public event EventHandler Disposed
		{
			add {
				Events.AddHandler (evDisposed, value);
			}

			remove {
				Events.RemoveHandler (evDisposed, value);
			}
		}
		public event EventHandler EndRequest
		{
			add {
				Events.AddHandler (evEndRequest, value);
			}

			remove {
				Events.RemoveHandler (evEndRequest, value);
			}
		}
		public event EventHandler Error
		{
			add {
				Events.AddHandler (evError, value);
			}

			remove {
				Events.RemoveHandler (evError, value);
			}
		}
		public event EventHandler PostRequestHandlerExecute
		{
			add {
				Events.AddHandler (evPostRequestHandlerExecute, value);
			}

			remove {
				Events.RemoveHandler (evPostRequestHandlerExecute, value);
			}
		}
		public event EventHandler PreRequestHandlerExecute
		{
			add {
				Events.AddHandler (evPreRequestHandlerExecute, value);
			}

			remove {
				Events.RemoveHandler (evPreRequestHandlerExecute, value);
			}
		}
		public event EventHandler PreSendRequestContent
		{
			add {
				Events.AddHandler (evPreSendRequestContent, value);
			}

			remove {
				Events.RemoveHandler (evPreSendRequestContent, value);
			}
		}
		public event EventHandler PreSendRequestHeaders
		{
			add {
				Events.AddHandler (evPreSendRequestHeaders, value);
			}

			remove {
				Events.RemoveHandler (evPreSendRequestHeaders, value);
			}
		}
		public event EventHandler ReleaseRequestState
		{
			add {
				Events.AddHandler (evReleaseRequestState, value);
			}

			remove {
				Events.RemoveHandler (evReleaseRequestState, value);
			}
		}
		public event EventHandler ResolveRequestCache
		{
			add {
				Events.AddHandler (evResolveRequestCache, value);
			}

			remove {
				Events.RemoveHandler (evResolveRequestCache, value);
			}
		}
		public event EventHandler UpdateRequestCache
		{
			add {
				Events.AddHandler (evUpdateRequestCache, value);
			}

			remove {
				Events.RemoveHandler (evUpdateRequestCache, value);
			}
		}

		public event EventHandler DefaultAuthentication
		{
			add {
				Events.AddHandler (evDefaultAuthentication, value);
			}

			remove {
				Events.RemoveHandler (evDefaultAuthentication, value);
			}
		}

		#endregion // Events and Delegates
	}
}
