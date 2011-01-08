//
// UpdateProgress.cs
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

namespace System.Web.UI
{
	[PersistChildren (false)]
	[ParseChildren (true)]
	[DefaultProperty ("AssociatedUpdatePanelID")]
	[Designer ("System.Web.UI.Design.UpdateProgressDesigner, System.Web.Extensions.Design, Version=1.0.61025.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
	public class UpdateProgress : Control, IScriptControl
	{
		ITemplate _progressTemplate;
		ScriptManager _scriptManager;

		[Category ("Behavior")]
		[DefaultValue ("")]
		[IDReferenceProperty (typeof (UpdatePanel))]
		public string AssociatedUpdatePanelID {
			get {
				return (string) ViewState ["AssociatedUpdatePanelID"] ?? String.Empty;
			}
			set {
				ViewState ["AssociatedUpdatePanelID"] = value;
			}
		}

		[Category ("Behavior")]
		[DefaultValue (500)]
		public int DisplayAfter {
			get {
				object o = ViewState ["DisplayAfter"];
				if (o == null)
					return 500;
				return (int) o;
			}
			set {
				ViewState ["DisplayAfter"] = value;
			}
		}

		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool DynamicLayout {
			get {
				object o = ViewState ["DynamicLayout"];
				if (o == null)
					return true;
				return (bool) o;
			}
			set {
				ViewState ["DynamicLayout"] = value;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ITemplate ProgressTemplate {
			get {
				return _progressTemplate;
			}
			set {
				_progressTemplate = value;
			}
		}

		ScriptManager ScriptManager {
			get {
				if (_scriptManager == null) {
					_scriptManager = ScriptManager.GetCurrent (Page);
					if (_scriptManager == null)
						throw new InvalidOperationException (String.Format ("The control with ID '{0}' requires a ScriptManager on the page. The ScriptManager must appear before any controls that need it.", ID));
				}
				return _scriptManager;
			}
		}

		protected virtual IEnumerable<ScriptDescriptor> GetScriptDescriptors () {
			string updatePanelClientId;
			if (String.IsNullOrEmpty (AssociatedUpdatePanelID))
				updatePanelClientId = null;
			else {
				UpdatePanel updatePanel = FindControl (AssociatedUpdatePanelID) as UpdatePanel;
				if (updatePanel == null)
					throw new InvalidOperationException ("No UpdatePanel found for AssociatedUpdatePanelID '" + AssociatedUpdatePanelID + "'.");
				updatePanelClientId = updatePanel.ClientID;
			}
			ScriptControlDescriptor descriptor = new ScriptControlDescriptor ("Sys.UI._UpdateProgress", this.ClientID);
			descriptor.AddProperty ("associatedUpdatePanelId", updatePanelClientId);
			descriptor.AddProperty ("displayAfter", DisplayAfter);
			descriptor.AddProperty ("dynamicLayout", DynamicLayout);
			yield return descriptor;
		}

		protected virtual IEnumerable<ScriptReference> GetScriptReferences () {
			yield break;
		}

		protected internal override void OnPreRender (EventArgs e) {
			base.OnPreRender (e);
			ScriptManager.RegisterScriptControl (this);

			if (_progressTemplate == null)
				throw new InvalidOperationException (String.Format ("A ProgressTemplate must be specified on UpdateProgress control with ID '{0}'.", ID));

			Control container = new Control ();
			_progressTemplate.InstantiateIn (container);
			Controls.Add (container);
		}

		protected internal override void Render (HtmlTextWriter writer) {
			if (DynamicLayout)
				writer.AddStyleAttribute (HtmlTextWriterStyle.Display, "none");
			else {
				writer.AddStyleAttribute (HtmlTextWriterStyle.Display, "block");
				writer.AddStyleAttribute (HtmlTextWriterStyle.Visibility, "hidden");
			}
			writer.AddAttribute (HtmlTextWriterAttribute.Id, ClientID);
			writer.RenderBeginTag (HtmlTextWriterTag.Div);
			base.Render (writer);
			writer.RenderEndTag ();

			ScriptManager.RegisterScriptDescriptors (this);
		}

		#region IScriptControl Members

		IEnumerable<ScriptDescriptor> IScriptControl.GetScriptDescriptors () {
			return GetScriptDescriptors ();
		}

		IEnumerable<ScriptReference> IScriptControl.GetScriptReferences () {
			return GetScriptReferences ();
		}

		#endregion
	}
}
