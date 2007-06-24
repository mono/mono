//
// ExtenderControl.cs
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
	[DefaultProperty ("TargetControlID")]
	[ParseChildren (true)]
	[NonVisualControl]
	[PersistChildren (false)]
	public abstract class ExtenderControl : Control, IExtenderControl
	{
		protected ExtenderControl () { }

		[DefaultValue ("")]
		[IDReferenceProperty]
		[Category ("Behavior")]
		public string TargetControlID {
			get {
				object o = ViewState ["TargetControlID"];
				if (o == null)
					return String.Empty;
				return (string) o;
			}
			set {
				ViewState ["TargetControlID"] = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool Visible {
			get {
				return base.Visible;
			}
			set {
				throw new NotImplementedException ();
			}
		}

		ScriptManager ScriptManager {
			get {
				ScriptManager manager = ScriptManager.GetCurrent (Page);
				if (manager == null)
					throw new InvalidOperationException (String.Format ("The control with ID '{0}' requires a ScriptManager on the page. The ScriptManager must appear before any controls that need it.", ID));
				return manager;
			}
		}

		protected abstract IEnumerable<ScriptDescriptor> GetScriptDescriptors (Control targetControl);

		protected abstract IEnumerable<ScriptReference> GetScriptReferences ();

		protected override void OnPreRender (EventArgs e) {
			base.OnPreRender (e);

			if (!ScriptManager.IsInAsyncPostBack) {
				ScriptManager.RegisterScriptDescriptors (this);

				if (String.IsNullOrEmpty (TargetControlID))
					throw new InvalidOperationException (String.Format ("The TargetControlID of '{0}' is not valid. The value cannot be null or empty.", ID));
				Control c = FindControl (TargetControlID);
				if (c == null)
					throw new InvalidOperationException (String.Format ("The TargetControlID of '{0}' is not valid. A control with ID '{1}' could not be found.", ID, TargetControlID));

				StringBuilder sb = null;
				foreach (ScriptDescriptor scriptDescriptor in ((IExtenderControl) this).GetScriptDescriptors (c)) {
					if (sb == null) {
						sb = new StringBuilder ();
						sb.AppendLine ();
					}
					sb.AppendLine ("Sys.Application.add_init(function() {");
					sb.AppendLine (scriptDescriptor.GetScript ());
					sb.AppendLine ("});");
				}
				if (sb != null)
					ScriptManager.RegisterStartupScript (this, typeof (ExtenderControl), "ScriptDescriptors" + ClientID, sb.ToString (), true);
			}
		}

		protected override void Render (HtmlTextWriter writer) {
		}

		#region IExtenderControl Members

		IEnumerable<ScriptDescriptor> IExtenderControl.GetScriptDescriptors (Control targetControl) {
			return GetScriptDescriptors (targetControl);
		}

		IEnumerable<ScriptReference> IExtenderControl.GetScriptReferences () {
			return GetScriptReferences ();
		}

		#endregion
	}
}
