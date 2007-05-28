//
// ScriptManagerProxy.cs
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
	[DefaultProperty ("Scripts")]
	[NonVisualControl]
	public class ScriptManagerProxy : Control
	{
		[DefaultValue ("")]
		[Category ("Behavior")]
		[MergableProperty (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public AuthenticationServiceManager AuthenticationService {
			get {
				throw new NotImplementedException ();
			}
		}

		[MergableProperty (false)]
		[Category ("Behavior")]
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		public ProfileServiceManager ProfileService {
			get {
				throw new NotImplementedException ();
			}
		}

		[DefaultValue ("")]
		[Category ("Behavior")]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[MergableProperty (false)]
		public ScriptReferenceCollection Scripts {
			get {
				throw new NotImplementedException ();
			}
		}

		[MergableProperty (false)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Category ("Behavior")]
		[DefaultValue ("")]
		public ServiceReferenceCollection Services {
			get {
				throw new NotImplementedException ();
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public override bool Visible {
			get {
				return base.Visible;
			}
			set {
				throw new NotImplementedException ();
			}
		}

		protected override void OnInit (EventArgs e) {
			base.OnInit (e);
		}
	}
}
