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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//

// NOT COMPLETE

using System.Collections;
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	public sealed class MdiClient : Control {
		#region Local Variables
		#endregion	// Local Variables

		#region Public Classes
		public new class ControlCollection : Control.ControlCollection {
			MdiClient	owner;

			public ControlCollection(MdiClient owner) : base(owner) {
				this.owner = owner;
			}

			public override void Add(Control value) {
				if ((value is Form) == false || (((Form)value).IsMdiChild)) {
					throw new ArgumentException("Form must be MdiChild");
				}

				for (int i=0; i<list.Count; i++) {
					if (list[i]==value) {
						// Do we need to do anything here?
						return;
					}
				}
				list.Add(value);
				//((Form)value).owner=(Form)owner;
			}

			public override void Remove(Control value) {
				//((Form)value).owner = null;
				base.Remove (value);
			}
		}
		#endregion	// Public Classes

		#region Public Constructors
		public MdiClient() {
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public override System.Drawing.Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}
			set {
				base.BackgroundImage = value;
			}
		}

		public Form[] MdiChildren {
			get {
				Form[]	children;

				children = new Form[Controls.Count];
				Controls.CopyTo(children, 0);

				return children;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				XplatUIWin32.CLIENTCREATESTRUCT	ccs;
				CreateParams			cp;

				cp = base.CreateParams;

				if (parent != null) {
					cp.X = 0;
					cp.Y = 0;
					cp.Width = parent.Width;
					cp.Height = parent.Height;
				}


				ccs = new System.Windows.Forms.XplatUIWin32.CLIENTCREATESTRUCT();
				ccs.hWindowMenu = IntPtr.Zero;
				ccs.idFirstChild = 27577;
				cp.Param = ccs;

				cp.ClassName = "MDICLIENT";
				cp.Style |= (int)(WindowStyles.WS_CHILD | WindowStyles.WS_VISIBLE);
				cp.ExStyle |= (int)WindowStyles.WS_EX_MDICHILD;

				return cp;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public void LayoutMdi(MdiLayout value) {
			throw new NotImplementedException();
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		#endregion	// Protected Instance Methods
	}
}
