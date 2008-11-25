//
// System.Web.UI.UserControl.cs
//
// Authors:
//   Gonzalo Paniagua Javier (gonzalo@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Security.Permissions;
using System.Web.Caching;
using System.Web.SessionState;

namespace System.Web.UI {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ControlBuilder (typeof (UserControlControlBuilder))]
	[DefaultEvent ("Load"), DesignerCategory ("ASPXCodeBehind")]
	[ToolboxItem (false)]
	[Designer ("System.Web.UI.Design.UserControlDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[ParseChildren (true)]
#if NET_2_0
	[Designer ("Microsoft.VisualStudio.Web.WebForms.WebFormDesigner, " + Consts.AssemblyMicrosoft_VisualStudio_Web, typeof (IRootDesigner))]
#else
	[RootDesignerSerializer ("Microsoft.VSDesigner.WebForms.RootCodeDomSerializer, " + Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.Serialization.CodeDomSerializer, " + Consts.AssemblySystem_Design, true)]
#endif
	public class UserControl : TemplateControl, IAttributeAccessor, IUserControlDesignerAccessor
#if NET_2_0
		, INamingContainer, IFilterResolutionService, INonBindingContainer
#endif
	{
#if NET_2_0
		ControlCachePolicy cachePolicy;
#endif
		bool initialized;
		AttributeCollection attributes;
		StateBag attrBag;

		public UserControl ()
		{
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public HttpApplicationState Application
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Application;
			}
		}

		void EnsureAttributes ()
		{
			if (attributes == null) {
				attrBag = new StateBag (true);
				if (IsTrackingViewState)
					attrBag.TrackViewState ();
				attributes = new AttributeCollection (attrBag);
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public AttributeCollection Attributes
		{
			get {
				EnsureAttributes ();
				return attributes;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public Cache Cache
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Cache;
			}
		}

#if NET_2_0
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public ControlCachePolicy CachePolicy 
		{
			get {
				BasePartialCachingControl bpcc = Parent as BasePartialCachingControl;

				if (bpcc != null)
					return bpcc.CachePolicy;
				
				if (cachePolicy == null)
					cachePolicy = new ControlCachePolicy ();
				return cachePolicy;
			}
		}
#endif		

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public bool IsPostBack
		{
			get {
				Page p = Page;
				if (p == null)
					return false;
				return p.IsPostBack;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public HttpRequest Request
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Request;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public HttpResponse Response
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Response;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public HttpServerUtility Server
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Server;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public HttpSessionState Session
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Session;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public TraceContext Trace
		{
			get {
				Page p = Page;
				if (p == null)
					return null;
				return p.Trace;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void DesignerInitialize ()
		{
			InitRecursive (null);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public void InitializeAsUserControl (Page page)
		{
			if (initialized)
				return;
			this.Page = page;
			InitializeAsUserControlInternal ();
		}

		internal void InitializeAsUserControlInternal ()
		{
			if (initialized)
				return;
			initialized = true;
			WireupAutomaticEvents ();
			FrameworkInitialize ();
		}

		public string MapPath (string virtualPath)
		{
			return Request.MapPath (virtualPath, TemplateSourceDirectory, true);
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

#if NET_2_0
		protected internal
#else
		protected
#endif
		override void OnInit (EventArgs e)
		{
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

		string IUserControlDesignerAccessor.InnerText
		{
			get {
				string innerText = ((string) ViewState["!DesignTimeInnerText"]);
				if (innerText == null)
					return string.Empty; 
				return innerText;
			}
			set { ViewState["!DesignTimeInnerText"] = value; }
		}

		string IUserControlDesignerAccessor.TagName
		{
			get {
				string innerTag = ((string) ViewState["!DesignTimeTagName"]);
				if (innerTag == null)
					return string.Empty; 
				return innerTag;
			}
			set { ViewState["!DesignTimeTagName"] = value; }
		}
#if NET_2_0
		[MonoTODO ("Not implemented")]
		int IFilterResolutionService.CompareFilters (string filter1, string filter2)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		bool IFilterResolutionService.EvaluateFilter (string filterName)
		{
			throw new NotImplementedException ();
		}
#endif
	}
}
