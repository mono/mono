//
// UpdatePanel.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
//
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

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI
{
	[DesignerAttribute ("System.Web.UI.Design.UpdatePanelDesigner, System.Web.Extensions.Design, Version=1.0.61025.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
	[DefaultPropertyAttribute ("Triggers")]
	[ParseChildrenAttribute (true)]
	[PersistChildrenAttribute (false)]
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class UpdatePanel : Control
	{
		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool ChildrenAsTriggers {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[TemplateInstance (TemplateInstance.Single)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ITemplate ContentTemplate {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Browsable (false)]
		public Control ContentTemplateContainer {
			get {
				throw new NotImplementedException ();
			}
		}

		public override sealed ControlCollection Controls {
			get {
				return base.Controls;
			}
		}

		[Browsable (false)]
		public bool IsInPartialRendering {
			get {
				throw new NotImplementedException ();
			}
		}

		[Category ("Layout")]
		public UpdatePanelRenderMode RenderMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		protected internal virtual bool RequiresUpdate {
			get {
				throw new NotImplementedException ();
			}
		}

		[MergableProperty (false)]
		[DefaultValue ("")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Category ("Behavior")]
		public UpdatePanelTriggerCollection Triggers {
			get {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
		public UpdatePanelUpdateMode UpdateMode {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		protected virtual Control CreateContentTemplateContainer ()
		{
			throw new NotImplementedException ();
		}

		protected override sealed ControlCollection CreateControlCollection ()
		{
			throw new NotImplementedException ();
		}

		protected internal virtual void Initialize ()
		{
			throw new NotImplementedException ();
		}

		protected override void OnInit (EventArgs e)
		{
			base.OnInit (e);
		}

		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
		}

		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}

		protected override void OnUnload (EventArgs e)
		{
			base.OnUnload (e);
		}

		protected override void Render (HtmlTextWriter writer)
		{
		}

		protected override void RenderChildren (HtmlTextWriter writer)
		{
		}

		public void Update ()
		{
			throw new NotImplementedException ();
		}
	}
}
