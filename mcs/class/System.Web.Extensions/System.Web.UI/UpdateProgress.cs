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
		[Category ("Behavior")]
		[DefaultValue ("")]
		[IDReferenceProperty (typeof (UpdatePanel))]
		public string AssociatedUpdatePanelID {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
		[DefaultValue (500)]
		public int DisplayAfter {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool DynamicLayout {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ITemplate ProgressTemplate {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		protected virtual IEnumerable<ScriptDescriptor> GetScriptDescriptors () {
			throw new NotImplementedException ();
		}

		protected virtual IEnumerable<ScriptReference> GetScriptReferences () {
			throw new NotImplementedException ();
		}

		protected override void OnPreRender (EventArgs e) {
			base.OnPreRender (e);
		}

		protected override void Render (HtmlTextWriter writer) {
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
