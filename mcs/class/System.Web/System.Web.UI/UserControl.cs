//
// System.Web.UI.UserControl
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;

namespace System.Web.UI
{
	public class UserControl : TemplateControl, IAttributeAccessor
	{
		private AttributeCollection attributes;

		public UserControl ()
		{
			//??
		}

		public HttpApplication Application
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Application;
			}
		}

		public AttributeCollection Attributes
		{
			get {
				if (attributes == null)
					attributes = new AttributeCollection (new StateBag ());
				return attributes;
			}
		}

		public Cache Cache
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Cache;
			}
		}

		public bool IsPostBack
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.IsPostBack;
			}
		}

		public HttpRequest Request
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Request;
			}
		}

		public HttpResponse Response
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Response;
			}
		}

		public HttpServerUtility Server
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Server;
			}
		}

		public HttpSessionState Session
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Session;
			}
		}

		public TraceContext Trace
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Trace;
			}
		}

		[MonoTODO]
		public void DesignerInitialize ()
		{
			throw NotImplementedException ();
		}

		public void InitializeAsUserControl (Page page)
		{
			if (initialized)
				return;
			initialized = true;
			FrameworkInitialize ();
		}

		[MonoTODO]
		public string MapPath (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void LoadViewState (object savedState)
		{
			throw new NotImplementedException ();
		}

		protected override void OnInit (EventArgs e)
		{
			if (Page != null)
				InitializeAsUserControlInternal (Page);

			base.OnInit(e);
		}

		[MonoTODO]
		protected override object SaveViewState ()
		{
			throw new NotImplementedException ();
		}

		string IAttributeAccessor.GetAttribute (string name)
		{
			if (attributes == null)
				return null;
			return attributes [name];
		}
		
		string IAttributeAccessor.SetAttribute (string name, string value)
		{
			Attributes [name] = value;
		}
	}
}

