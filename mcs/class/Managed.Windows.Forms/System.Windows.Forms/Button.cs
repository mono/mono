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
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
#if NET_2_0
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[Designer ("System.Windows.Forms.Design.ButtonBaseDesigner, " + Consts.AssemblySystem_Design,
		   "System.ComponentModel.Design.IDesigner")]
#endif
	public class Button : ButtonBase, IButtonControl {
		#region Local variables
		DialogResult	dialog_result;
		#endregion	// Local variables

		#region Public Constructors
		public Button() {
			dialog_result = DialogResult.None;
			SetStyle(ControlStyles.StandardDoubleClick, false);
		}
		#endregion	// Public Constructors

		#region	Internal methods
		internal override void HaveDoubleClick() {
			EventHandler eh = (EventHandler)(Events [DoubleClickEvent]);
			if (eh != null)
				eh (this, EventArgs.Empty);
		}
		
#if NET_2_0
		internal override void Draw (PaintEventArgs pevent)
		{
			// System style does not use any of the new 2.0 stuff
			if (this.FlatStyle == FlatStyle.System) {
				base.Draw (pevent);
				return;
			}

			// FIXME: This should be called every time something that can affect it
			// is changed, not every paint.  Can only change so many things at a time.

			// Figure out where our text and image should go
			Rectangle text_rectangle;
			Rectangle image_rectangle;

			ThemeEngine.Current.CalculateButtonTextAndImageLayout (this, out text_rectangle, out image_rectangle);

			// Draw our button
			if (this.FlatStyle == FlatStyle.Standard)
				ThemeEngine.Current.DrawButton (pevent.Graphics, this, text_rectangle, image_rectangle, pevent.ClipRectangle);
			else if (this.FlatStyle == FlatStyle.Flat)
				ThemeEngine.Current.DrawFlatButton (pevent.Graphics, this, text_rectangle, image_rectangle, pevent.ClipRectangle);
			else if (this.FlatStyle == FlatStyle.Popup)
				ThemeEngine.Current.DrawPopupButton (pevent.Graphics, this, text_rectangle, image_rectangle, pevent.ClipRectangle);
		}
#endif
		#endregion	// Internal methods

		#region Public Instance Properties
		[DefaultValue(DialogResult.None)]
		public virtual DialogResult DialogResult {		// IButtonControl
			get {
				return dialog_result;
			}
			set {
				dialog_result = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public virtual void NotifyDefault(bool value) {	// IButtonControl
			this.IsDefault = value;
		}

		public void PerformClick() {			// IButtonControl
			if (CanSelect)
				OnClick(EventArgs.Empty);
		}

		public override string ToString() {
			return base.ToString() + ", Text: " + this.Text;
		}
		#endregion	// Public Instance Methods

		#region	Protected Instance Methods
		protected override void OnClick(EventArgs e) {
			if (dialog_result != DialogResult.None) {
				Form p = FindForm ();

				if (p != null) {
					p.DialogResult = dialog_result;
				}
			}
			base.OnClick(e);
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp (e);
		}

		protected override bool ProcessMnemonic(char charCode) {
			if (this.UseMnemonic && IsMnemonic(charCode, Text) == true) {
				PerformClick();
				return true;
			}
			
			return base.ProcessMnemonic(charCode);
		}

		protected override void WndProc(ref Message m) {
			base.WndProc (ref m);
		}
		#endregion	// Protected Instance Methods

		#region Events
		static object DoubleClickEvent = new object ();

		// XXX this and HaveDoubleClick above need to be
		// looked at.. it's far more likely that this just
		// uses base.DoubleClick, and HaveDoubleClick goes
		// away in favor of calls to OnDoubleClick.
		[Browsable(false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public new event EventHandler DoubleClick {
			add { Events.AddHandler (DoubleClickEvent, value); }
			remove { Events.RemoveHandler (DoubleClickEvent, value); }
		}
		
#if NET_2_0
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public new event MouseEventHandler MouseDoubleClick {
			add { base.MouseDoubleClick += value; }
			remove { base.MouseDoubleClick -= value; }
		}
#endif
		#endregion	// Events
		
#if NET_2_0
		protected override void OnFontChanged (EventArgs e)
		{
			base.OnFontChanged (e);
		}
		
		protected override void OnMouseEnter (EventArgs e)
		{
			base.OnMouseEnter (e);
		}
		
		protected override void OnMouseLeave (EventArgs e)
		{
			base.OnMouseLeave (e);
		}

		protected override void OnTextChanged (EventArgs e)
		{
			base.OnTextChanged (e);
		}
#endif
	}
}
