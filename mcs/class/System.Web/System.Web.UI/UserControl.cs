//
// System.Web.UI.UserControl
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//
using System;
using System.Web.Caching;
using System.Web.SessionState;

namespace System.Web.UI
{
	public class UserControl : TemplateControl, IAttributeAccessor
	{
		private bool initialized;
		private AttributeCollection attributes;
		private StateBag attrBag;

		public UserControl ()
		{
			//??
		}

		public HttpApplicationState Application
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Application;
			}
		}

		private void EnsureAttributes ()
		{
			if (attributes == null) {
				attrBag = new StateBag (true);
				if (IsTrackingViewState)
					attrBag.TrackViewState ();
				attributes = new AttributeCollection (attrBag);
			}
		}

		public AttributeCollection Attributes
		{
			get {
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
					return false;
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
			throw new NotImplementedException ();
		}

		public void InitializeAsUserControl (Page page)
		{
			if (initialized)
				return;
			initialized = true;
			WireupAutomaticEvents ();
			FrameworkInitialize ();
		}

		[MonoTODO]
		public string MapPath (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState != null) {
				Pair p = (Pair) savedState;
				base.LoadViewState (p.First);
				if (p.Second != null) {
					EnsureAttributes ();
					attrBag.LoadViewState (p.Second);
				}
			}

		}

		protected override void OnInit (EventArgs e)
		{
			if (Page != null)
				InitializeAsUserControl (Page);

			base.OnInit(e);
		}

		protected override object SaveViewState ()
		{
			object baseState = base.SaveViewState();
			object attrState = null;
			if (attributes != null)
				attrState = attrBag.SaveViewState ();
			if (baseState == null && attrState == null)
				return null;
			return new Pair (baseState, attrState);
		}

		string IAttributeAccessor.GetAttribute (string name)
		{
			if (attributes == null)
				return null;
			return attributes [name];
		}
		
		void IAttributeAccessor.SetAttribute (string name, string value)
		{
			EnsureAttributes ();
			Attributes [name] = value;
		}
	}
}

