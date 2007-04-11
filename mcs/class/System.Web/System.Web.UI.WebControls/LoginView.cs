//
// System.Web.UI.WebControls.LoginView class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Konstantin Triger  <kostat@mainsoft.com>
//
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

#if NET_2_0

using System.Collections;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultEvent ("ViewChanged")]
	[DefaultProperty ("CurrentView")]
	[Designer ("System.Web.UI.Design.WebControls.LoginViewDesigner," + Consts.AssemblySystem_Design)]
	[ParseChildren (true)]
	[PersistChildren (false)]
	[Themeable (true)]
	public class LoginView : Control, INamingContainer {

		private static readonly object viewChangedEvent = new object ();
		private static readonly object viewChangingEvent = new object ();

		private ITemplate anonymousTemplate;
		private ITemplate loggedInTemplate;
		private bool isAuthenticated;
		private bool theming;
		private RoleGroupCollection coll;


		public LoginView ()
		{
			theming = true;
		}


		[Browsable (false)]
		[DefaultValue (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (LoginView))]
		public virtual ITemplate AnonymousTemplate {
			get { return anonymousTemplate; }
			set { anonymousTemplate = value; }
		}

		public override ControlCollection Controls {
			get {
				EnsureChildControls();
				return base.Controls;
			}
		}

		[Browsable (true)]
		public override bool EnableTheming {
			get { return theming; }
			set { theming = value; }
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (LoginView))]
		public virtual ITemplate LoggedInTemplate {
			get { return loggedInTemplate; }
			set { loggedInTemplate = value; }
		}

		[Filterable (false)]
		[MergableProperty (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Themeable (false)]
		public RoleGroupCollection RoleGroups {
			get {
				if (coll == null)
					coll = new RoleGroupCollection ();
				return coll;
			}
		}

		[Browsable (true)]
		public override string SkinID {
			get { return base.SkinID; }
			set { base.SkinID = value; }
		}

		bool IsAuthenticated {
			get {
				return isAuthenticated;
			}
			set {
				if (value == isAuthenticated)
					return;
				
				isAuthenticated = value;

				OnViewChanging (EventArgs.Empty);
				ChildControlsCreated = false;
				OnViewChanged (EventArgs.Empty);
			}
		}

		[MonoTODO ("Handle RoleGroups")]
		protected internal override void CreateChildControls ()
		{
			Controls.Clear ();
			Control c = new Control ();
			if (Page != null && Page.Request.IsAuthenticated) {
				isAuthenticated = true;
				if (LoggedInTemplate != null)
					LoggedInTemplate.InstantiateIn (c);
			}
			else {
				isAuthenticated = false;
				if (AnonymousTemplate != null)
					AnonymousTemplate.InstantiateIn (c);
			}

			Controls.Add (c);
		}

		public override void DataBind ()
		{
			EventArgs args = EventArgs.Empty;
			OnDataBinding (args);
			EnsureChildControls ();
			DataBindChildren ();
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override void Focus ()
		{
			// LAMESPEC: throw new InvalidOperationException ();
			throw new NotSupportedException ();
		}

		protected internal override void LoadControlState (object savedState)
		{
			if (savedState == null)
				return;

			isAuthenticated = (bool) savedState;
		}

		protected internal override void OnInit (EventArgs e)
		{
			base.OnInit (e);
			if (Page != null)
				Page.RegisterRequiresControlState(this);
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			if (Page != null)
				IsAuthenticated = Page.Request.IsAuthenticated;
		}

		protected virtual void OnViewChanged (EventArgs e)
		{
			EventHandler h = (EventHandler)Events [viewChangedEvent];
			if (h != null)
				h (this, e);
		}

		protected virtual void OnViewChanging (EventArgs e)
		{
			EventHandler h = (EventHandler)Events [viewChangingEvent];
			if (h != null)
				h (this, e);
		}

		protected internal override void Render(HtmlTextWriter writer) {
			EnsureChildControls();
			base.Render (writer);
		}

		protected internal override object SaveControlState ()
		{
			if (isAuthenticated)
				return isAuthenticated;

			return null;
		}

		[MonoTODO ("for design-time usage - no more details available")]
		protected override void SetDesignModeState (IDictionary data)
		{
			base.SetDesignModeState (data);
		}


		// events

		public event EventHandler ViewChanged {
			add { Events.AddHandler (viewChangedEvent, value); }
			remove { Events.RemoveHandler (viewChangedEvent, value); }
		}

		public event EventHandler ViewChanging {
			add { Events.AddHandler (viewChangingEvent, value); }
			remove { Events.RemoveHandler (viewChangingEvent, value); }
		}
	}
}

#endif
