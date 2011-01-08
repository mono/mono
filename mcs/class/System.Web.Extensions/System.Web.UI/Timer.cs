//
// Timer.cs
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
	[DefaultProperty ("Interval")]
	[Designer ("System.Web.UI.Design.TimerDesigner, System.Web.Extensions.Design, Version=1.0.61025.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
	[NonVisualControl]
	[SupportsEventValidation]
	[DefaultEvent ("Tick")]
	public class Timer : Control, IPostBackEventHandler, IScriptControl
	{
		ScriptManager _scriptManager;

		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool Enabled {
			get {
				object o = ViewState ["Enabled"];
				if (o == null)
					return true;
				return (bool) o;
			}
			set {
				ViewState ["Enabled"] = value;
			}
		}

		[DefaultValue (60000)]
		[Category ("Behavior")]
		public int Interval {
			get {
				object o = ViewState ["Interval"];
				if (o == null)
					return 60000;
				return (int) o;
			}
			set {
				ViewState ["Interval"] = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
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
				if (_scriptManager == null) {
					_scriptManager = ScriptManager.GetCurrent (Page);
					if (_scriptManager == null)
						throw new InvalidOperationException (String.Format ("The control with ID '{0}' requires a ScriptManager on the page. The ScriptManager must appear before any controls that need it.", ID));
				}
				return _scriptManager;
			}
		}

		[Category ("Action")]
		public event EventHandler<EventArgs> Tick;

		protected virtual IEnumerable<ScriptDescriptor> GetScriptDescriptors () {
			ScriptControlDescriptor descriptor = new ScriptControlDescriptor ("Sys.UI._Timer", this.ClientID);
			descriptor.AddProperty ("enabled", Enabled);
			descriptor.AddProperty ("interval", Interval);
			descriptor.AddProperty ("uniqueID", UniqueID);
			yield return descriptor;
		}

		protected virtual IEnumerable<ScriptReference> GetScriptReferences () {
			ScriptReference script;
			script = new ScriptReference ("MicrosoftAjaxTimer.js", String.Empty);
			script.NotifyScriptLoaded = false;
			yield return script;
		}

		protected internal override void OnPreRender (EventArgs e) {
			base.OnPreRender (e);
			ScriptManager.RegisterScriptControl (this);
		}

		protected virtual void OnTick (EventArgs e) {
			if (Tick != null)
				Tick (this, e);
		}

		protected virtual void RaisePostBackEvent (string eventArgument) {
			OnTick (EventArgs.Empty);
		}

		protected internal override void Render (HtmlTextWriter writer) {

			Page.ClientScript.RegisterForEventValidation (UniqueID);

			writer.AddStyleAttribute (HtmlTextWriterStyle.Display, "none");
			writer.AddStyleAttribute (HtmlTextWriterStyle.Visibility, "hidden");
			writer.AddAttribute (HtmlTextWriterAttribute.Id, ClientID);
			writer.RenderBeginTag (HtmlTextWriterTag.Span);
			base.Render (writer);
			writer.RenderEndTag ();

			ScriptManager.RegisterScriptDescriptors (this);
		}

		#region IPostBackEventHandler Members

		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument) {
			RaisePostBackEvent (eventArgument);
		}

		#endregion

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

