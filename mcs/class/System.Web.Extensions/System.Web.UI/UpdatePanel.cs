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
using System.IO;

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
		ITemplate _contentTemplate;
		Control _contentTemplateContainer;
		UpdatePanelUpdateMode _updateMode = UpdatePanelUpdateMode.Always;
		bool _childrenAsTriggers = true;
		bool _requiresUpdate = false;
		UpdatePanelTriggerCollection _triggers;
		UpdatePanelRenderMode _renderMode = UpdatePanelRenderMode.Block;
		ScriptManager _scriptManager;

		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool ChildrenAsTriggers {
			get {
				return _childrenAsTriggers;
			}
			set {
				_childrenAsTriggers = value;
			}
		}

		[TemplateInstance (TemplateInstance.Single)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ITemplate ContentTemplate {
			get {
				return _contentTemplate;
			}
			set {
				_contentTemplate = value;
			}
		}

		[Browsable (false)]
		public Control ContentTemplateContainer {
			get {
				if (_contentTemplateContainer == null) {
					_contentTemplateContainer = CreateContentTemplateContainer ();
					Controls.Add (_contentTemplateContainer);
				}
				return _contentTemplateContainer;
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
				return ScriptManager.IsInPartialRendering;
			}
		}

		[Category ("Layout")]
		public UpdatePanelRenderMode RenderMode {
			get {
				return _renderMode;
			}
			set {
				_renderMode = value;
			}
		}

		protected internal virtual bool RequiresUpdate {
			get {
				return UpdateMode == UpdatePanelUpdateMode.Always || _requiresUpdate;
			}
		}

		internal ScriptManager ScriptManager {
			get {
				if (_scriptManager == null) {
					_scriptManager = ScriptManager.GetCurrent (Page);
					if (_scriptManager == null)
						throw new InvalidOperationException (String.Format ("The control with ID '{0}' requires a ScriptManager on the page. The ScriptManager must appear before any controls that need it.", ID));
				}
				return _scriptManager;
			}
		}

		[MergableProperty (false)]
		[DefaultValue ("")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Category ("Behavior")]
		public UpdatePanelTriggerCollection Triggers {
			get {
				if (_triggers == null)
					_triggers = new UpdatePanelTriggerCollection (this);
				return _triggers;
			}
		}

		[Category ("Behavior")]
		[DefaultValueAttribute (UpdatePanelUpdateMode.Always)]
		public UpdatePanelUpdateMode UpdateMode {
			get {
				return _updateMode;
			}
			set {
				_updateMode = value;
			}
		}

		protected virtual Control CreateContentTemplateContainer () {
			return new Control ();
		}

		[MonoTODO ()]
		protected override sealed ControlCollection CreateControlCollection () {
			// TODO: Because this method is protected and sealed, it is visible to classes that inherit 
			// from the UpdatePanel class, but it cannot be overridden. This method overrides 
			// the base implementation to return a specialized ControlCollection object that throws 
			// an InvalidOperationException when the Add(Control), AddAt(Int32, Control), Clear(), 
			// Remove(Control), or RemoveAt(Int32) method of the ControlCollection class is invoked. 
			// To change the content of the UpdatePanel control, modify the child controls of 
			// the ContentTemplateContainer property.

			return base.CreateControlCollection ();
		}

		protected internal virtual void Initialize () {
			if (_triggers != null) {
				for (int i = 0; i < _triggers.Count; i++) {
					_triggers [i].Initialize ();
				}
			}
		}

		protected override void OnInit (EventArgs e) {
			base.OnInit (e);

			ScriptManager.RegisterUpdatePanel (this);

			if (ContentTemplate != null)
				ContentTemplate.InstantiateIn (ContentTemplateContainer);
		}

		protected override void OnLoad (EventArgs e) {
			base.OnLoad (e);

			Initialize ();
		}

		protected override void OnPreRender (EventArgs e) {
			base.OnPreRender (e);

			if (UpdateMode == UpdatePanelUpdateMode.Always && !ChildrenAsTriggers)
				throw new InvalidOperationException (String.Format ("ChildrenAsTriggers cannot be set to false when UpdateMode is set to Always on UpdatePanel '{0}'", ID));
		}

		protected override void OnUnload (EventArgs e) {
			base.OnUnload (e);
		}

		protected override void Render (HtmlTextWriter writer) {
			writer.AddAttribute (HtmlTextWriterAttribute.Id, ClientID);
			if (RenderMode == UpdatePanelRenderMode.Block)
				writer.RenderBeginTag (HtmlTextWriterTag.Div);
			else
				writer.RenderBeginTag (HtmlTextWriterTag.Span);
			RenderChildren (writer);
			writer.RenderEndTag ();
		}

		protected override void RenderChildren (HtmlTextWriter writer) {
			if (ScriptManager.IsInAsyncPostBack){
				if (!ScriptManager.IsInPartialRendering) {
					ScriptManager.IsInPartialRendering = true;
					ScriptManager.AlternativeHtmlTextWriter altWriter = writer as ScriptManager.AlternativeHtmlTextWriter;
					if (altWriter == null)
						altWriter = writer.InnerWriter as ScriptManager.AlternativeHtmlTextWriter;
					if (altWriter == null)
						throw new InvalidOperationException ("Internal error. Invalid writer object.");
					
					HtmlTextWriter responseOutput = altWriter.ResponseOutput;
					StringBuilder sb = new StringBuilder ();
					HtmlTextWriter w = new HtmlTextWriter (new StringWriter (sb));
					base.RenderChildren (w);
					w.Flush ();
					
					ScriptManager.WriteCallbackPanel (responseOutput, this, sb);
					ScriptManager.IsInPartialRendering = false;
				}
				else {
					if (ScriptManager.IsInPartialRendering)
						ScriptManager.RegisterChildUpdatePanel (this);
					base.RenderChildren (writer);
				}
			}
			else
				base.RenderChildren (writer);
		}

		public void Update () {
			_requiresUpdate = true;
		}
	}
}
