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
		[Category ("Behavior")]
		[DefaultValue (true)]
		public bool Enabled {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue (60000)]
		[Category ("Behavior")]
		public int Interval {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Never)]
		public override bool Visible {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[Category ("Action")]
		public event EventHandler<EventArgs> Tick;

		protected virtual IEnumerable<ScriptDescriptor> GetScriptDescriptors () {
			throw new NotImplementedException ();
		}

		protected virtual IEnumerable<ScriptReference> GetScriptReferences () {
			throw new NotImplementedException ();
		}

		protected override void OnPreRender (EventArgs e) {
			base.OnPreRender (e);
		}

		protected virtual void OnTick (EventArgs e) {
			if (Tick != null)
				Tick (this, e);
		}

		protected virtual void RaisePostBackEvent (string eventArgument) {
			throw new NotImplementedException ();
		}

		protected override void Render (HtmlTextWriter writer) {
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

