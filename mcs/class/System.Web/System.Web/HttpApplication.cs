// 
// System.Web.HttpApplication
//
// Author:
//   Patrik Torstensson (Patrik.Torstensson@labs2.com)
//   Tim Coleman (tim@timcoleman.com)
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
		IPrincipal user;

		#endregion // Fields

		#region Constructors

		[MonoTODO()]
		public HttpApplication() 
		{
			// Init HTTP context and the methods from HttpRuntime....
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
			[MonoTODO]
			get { throw new NotImplementedException (); }
		}

		bool IHttpHandler.IsReusable {
			[MonoTODO]
			get { throw new NotImplementedException(); }
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
			get { return user; }
			[MonoTODO ("This requires the ControlPrincipal flag to be set in Flags.")]
			set { user = value; }
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

		[MonoTODO("Cleanup")]
		public virtual void Dispose () 
		{
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

		#endregion // Events and Delegates
	}
}
