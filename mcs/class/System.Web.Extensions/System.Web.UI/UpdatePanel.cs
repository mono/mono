//
// UpdatePanel.cs
//
// Authors:
//   Igor Zelmanovich <igorz@mainsoft.com>
//   Marek Habersack <grendel@twistedcode.net>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
// (C) 2007-2010 Novell, Inc (http://novell.com/)
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
		sealed class SingleChildControlCollection : ControlCollection
		{
			public SingleChildControlCollection (Control owner)
				: base (owner)
			{}

			internal void AddInternal (Control child)
			{
				base.Add (child);
			}
			
			public override void Add (Control child)
			{
				throw GetNoChildrenException ();
			}

			public override void AddAt (int index, Control child)
			{
				throw GetNoChildrenException ();
			}

			public override void Clear ()
			{
				throw GetNoChildrenException ();
			}

			public override void Remove (Control value)
			{
				throw GetNoChildrenException ();
			}

			public override void RemoveAt (int index)
			{
				throw GetNoChildrenException ();
			}
			
			InvalidOperationException GetNoChildrenException ()
			{
				return new InvalidOperationException ("The Controls property of UpdatePanel with ID '" + Owner.ID + "' cannot be modified directly. To change the contents of the UpdatePanel modify the child controls of the ContentTemplateContainer property.");
			}
		}
		
		ITemplate _contentTemplate;
		Control _contentTemplateContainer;
		UpdatePanelUpdateMode _updateMode = UpdatePanelUpdateMode.Always;
		bool _childrenAsTriggers = true;
		bool _requiresUpdate;
		bool _inPartialRendering;
		UpdatePanelTriggerCollection _triggers;
		UpdatePanelRenderMode _renderMode = UpdatePanelRenderMode.Block;
		ScriptManager _scriptManager;
		Control cachedParent;
		UpdatePanel parentPanel;
		bool parentPanelChecked;
		
		UpdatePanel ParentPanel {
			get {
				Control parent = Parent;
				if (cachedParent == parent && parentPanelChecked)
					return parentPanel;

				cachedParent = parent;
				parentPanel = FindParentPanel (parent);

				return parentPanel;
			}
		}
		
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
					((SingleChildControlCollection) Controls).AddInternal (_contentTemplateContainer);
				}
				return _contentTemplateContainer;
			}
		}

		public override sealed ControlCollection Controls {
			get { return base.Controls; }
		}

		[Browsable (false)]
		public bool IsInPartialRendering {
			get { return _inPartialRendering; }
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
				return UpdateMode == UpdatePanelUpdateMode.Always || _requiresUpdate || AnyTriggersFired ();
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

		bool AnyTriggersFired ()
		{
			if (_triggers == null || _triggers.Count == 0)
				return false;

			foreach (UpdatePanelTrigger trigger in _triggers)
				if (trigger.HasTriggered ())
					return true;

			return false;
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

		// Used by nested panels (see bug #542441)
		ScriptManager.AlternativeHtmlTextWriter RenderChildrenWriter { get; set; }
		
		protected virtual Control CreateContentTemplateContainer ()
		{
			return new Control ();
		}

		protected override sealed ControlCollection CreateControlCollection ()
		{
			return new SingleChildControlCollection (this);
		}

		protected internal virtual void Initialize ()
		{
			if (_triggers == null || _triggers.Count == 0 || !ScriptManager.SupportsPartialRendering)
				return;

			_triggers.Initialize ();
		}

		protected internal override void OnInit (EventArgs e) {
			base.OnInit (e);

			ScriptManager.RegisterUpdatePanel (this);
			if (ParentPanel != null)
				ScriptManager.RegisterChildUpdatePanel (this);
			
			if (ContentTemplate != null)
				ContentTemplate.InstantiateIn (ContentTemplateContainer);
		}

		protected internal override void OnLoad (EventArgs e) {
			base.OnLoad (e);

			Initialize ();
		}

		protected internal override void OnPreRender (EventArgs e) {
			base.OnPreRender (e);

			if (UpdateMode == UpdatePanelUpdateMode.Always && !ChildrenAsTriggers)
				throw new InvalidOperationException (String.Format ("ChildrenAsTriggers cannot be set to false when UpdateMode is set to Always on UpdatePanel '{0}'", ID));
		}

		protected internal override void OnUnload (EventArgs e) {
			base.OnUnload (e);
		}

		protected internal override void Render (HtmlTextWriter writer) {
			writer.AddAttribute (HtmlTextWriterAttribute.Id, ClientID);
			if (RenderMode == UpdatePanelRenderMode.Block)
				writer.RenderBeginTag (HtmlTextWriterTag.Div);
			else
				writer.RenderBeginTag (HtmlTextWriterTag.Span);
			RenderChildren (writer);
			writer.RenderEndTag ();
		}

		UpdatePanel FindParentPanel (Control parent)
		{
			parentPanelChecked = true;
			while (parent != null) {
				UpdatePanel panel = parent as UpdatePanel;
				if (panel != null)
					return panel;
				
				parent = parent.Parent;
			}

			return null;
		}
		
		protected internal override void RenderChildren (HtmlTextWriter writer)
		{
			RenderChildrenWriter = null;
			
			if (IsInPartialRendering) {
				ScriptManager.AlternativeHtmlTextWriter altWriter = writer as ScriptManager.AlternativeHtmlTextWriter;
				if (altWriter == null)
					altWriter = writer.InnerWriter as ScriptManager.AlternativeHtmlTextWriter;
				
				if (altWriter == null) {
					UpdatePanel parentPanel = ParentPanel;
					if (parentPanel != null)
						altWriter = parentPanel.RenderChildrenWriter;
				}

				if (altWriter == null)
					throw new InvalidOperationException ("Internal error. Invalid writer object.");

				// Used by nested panels (see bug #542441)
				RenderChildrenWriter = altWriter;
				try {
					HtmlTextWriter responseOutput = altWriter.ResponseOutput;
					StringBuilder sb = new StringBuilder ();
					HtmlTextWriter w = new HtmlTextWriter (new StringWriter (sb));
					base.RenderChildren (w);
					w.Flush ();
					UpdatePanel parent = ParentPanel;
					if (parent != null && parent.ChildrenAsTriggers)
						writer.Write (sb.ToString ());
					else
						ScriptManager.WriteCallbackPanel (responseOutput, this, sb);
				} finally {
					RenderChildrenWriter = null;
				}
			} else
				base.RenderChildren (writer);
		}

		internal void SetInPartialRendering (bool setting)
		{
			_inPartialRendering = setting;
		}
		
		public void Update ()
		{
			if (UpdateMode == UpdatePanelUpdateMode.Always)
				throw new InvalidOperationException ("The Update method can only be called on UpdatePanel with ID '" + ID + "' when UpdateMode is set to Conditional.");
			
			_requiresUpdate = true;
		}
	}
}
