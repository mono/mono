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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//
// $Revision: 1.5 $
// $Modtime: $
// $Log: Form.cs,v $
// Revision 1.5  2004/08/19 21:30:37  pbartok
// - Added handling of WM_CLOSE
//
// Revision 1.4  2004/08/11 22:20:59  pbartok
// - Signature fixes
//
// Revision 1.3  2004/08/04 21:13:47  pbartok
// - Added AutoScale properties
//
// Revision 1.2  2004/07/13 15:31:45  jordi
// commit: new properties and fixes form size problems
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Runtime.InteropServices;
using System.Threading;

namespace System.Windows.Forms {
	public class Form : ContainerControl {
		#region Local Variables
		private static bool	autoscale;
		private static Size	autoscale_base_size;
		#endregion	// Local Variables

		#region Public Constructor & Destructor
		public Form() {
			Console.WriteLine("Form Constructor called");			
			//XplatUI.Version();
		}
		#endregion	// Public Constructor & Destructor

		#region Public Static Properties
		#endregion	// Public Static Properties

		#region Public Instance Properties
		protected override Size DefaultSize {
			get {
				return new Size (250, 250);
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		public bool AutoScale {
			get {
				return autoscale;
			}

			set {
				autoscale=value;
			}
		}

		public virtual Size AutoScaleBaseSize {
			get {
				return autoscale_base_size;
			}

			set {
				autoscale_base_size=value;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Static Methods
		#endregion	// Public Static Methods

		#region Public Instance Methods
		[MonoTODO("Need to add MDI support")]
		protected override CreateParams CreateParams {
			get {
				CreateParams create_params = new CreateParams();

				create_params.Caption = "";

				create_params.ClassName=XplatUI.DefaultClassName;
				create_params.ClassStyle = 0;
				create_params.ExStyle=0;
				create_params.Parent=IntPtr.Zero;
				create_params.Param=0;
				create_params.X = Left;
				create_params.Y = Top;
				create_params.Width = Width;
				create_params.Height = Height;
				
				create_params.Style |= (int)WindowStyles.WS_OVERLAPPEDWINDOW;
				create_params.Style |= (int)WindowStyles.WS_VISIBLE;

				return create_params;
			}
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override void WndProc(ref Message m) {
			switch((Msg)m.Msg) {
				case Msg.WM_CLOSE: {
					CancelEventArgs args = new CancelEventArgs(false);

					OnClosing(args);

					if (!args.Cancel) {
						OnClosed(EventArgs.Empty);
						base.WndProc(ref m);
						break;
					}
					break;
				}

				default: {
					base.WndProc (ref m);
					break;
				}
			}
		}

		#endregion	// Protected Instance Methods

		#region Events

		protected virtual void OnActivated(EventArgs e) {
			if (Activated != null) {
				Activated(this, e);
			}
		}

		protected virtual void OnClosed(EventArgs e) {
			if (Closed != null) {
				Closed(this, e);
			}
		}

		protected virtual void OnClosing(System.ComponentModel.CancelEventArgs e) {
			if (Closing != null) {
				Closing(this, e);
			}
		}

		protected virtual void OnDeactivate(EventArgs e) {
			if (Deactivate != null) {
				Deactivate(this, e);
			}
		}

		protected virtual void OnInputLanguageChanged(InputLanguageChangedEventArgs e) {
			if (InputLanguageChanged!=null) {
				InputLanguageChanged(this, e);
			}
		}

		protected virtual void OnInputLanguageChanging(InputLanguageChangingEventArgs e) {
			if (InputLanguageChanging!=null) {
				InputLanguageChanging(this, e);
			}
		}

		protected virtual void OnLoad(EventArgs e) {
			if (Load != null) {
				Load(this, e);
			}
		}

		protected virtual void OnMaximizedBoundsChanged(EventArgs e) {
			if (MaximizedBoundsChanged != null) {
				MaximizedBoundsChanged(this, e);
			}
		}

		protected virtual void OnMaximumSizeChanged(EventArgs e) {
			if (MaximumSizeChanged != null) {
				MaximumSizeChanged(this, e);
			}
		}

		protected virtual void OnMdiChildActivate(EventArgs e) {
			if (MdiChildActivate != null) {
				MdiChildActivate(this, e);
			}
		}

		protected virtual void OnMenuComplete(EventArgs e) {
			if (MenuComplete != null) {
				MenuComplete(this, e);
			}
		}

		protected virtual void OnMenuStart(EventArgs e) {
			if (MenuStart != null) {
				MenuStart(this, e);
			}
		}

		protected virtual void OnMinimumSizeChanged(EventArgs e) {
			if (MinimumSizeChanged != null) {
				MinimumSizeChanged(this, e);
			}
		}

		public event EventHandler Activated;
		public event EventHandler Closed;
		public event CancelEventHandler Closing;
		public event EventHandler Deactivate;
		public event InputLanguageChangedEventHandler InputLanguageChanged;
		public event InputLanguageChangingEventHandler InputLanguageChanging;
		public event EventHandler Load;
		public event EventHandler MaximizedBoundsChanged;
		public event EventHandler MaximumSizeChanged;
		public event EventHandler MdiChildActivate;
		public event EventHandler MenuComplete;
		public event EventHandler MenuStart;
		public event EventHandler MinimumSizeChanged;
		#endregion	// Events
	}
}
