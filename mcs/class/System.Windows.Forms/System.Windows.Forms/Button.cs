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
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	[Designer ("System.Windows.Forms.Design.ButtonBaseDesigner, " + Consts.AssemblySystem_Design,
		   "System.ComponentModel.Design.IDesigner")]
	public class Button : ButtonBase, IButtonControl {
		#region Local variables
		DialogResult	dialog_result;
		#endregion	// Local variables

		#region Public Constructors
		public Button ()
		{
			dialog_result = DialogResult.None;
			SetStyle (ControlStyles.StandardDoubleClick, false);
		}
		#endregion	// Public Constructors

		#region Public Properties
		[Browsable (true)]
		[Localizable (true)]
		[DefaultValue (AutoSizeMode.GrowOnly)]
		[MWFCategory("Layout")]
		public AutoSizeMode AutoSizeMode {
			get { return base.GetAutoSizeMode (); }
			set { base.SetAutoSizeMode (value); }
		}

		[DefaultValue (DialogResult.None)]
		[MWFCategory("Behavior")]
		public virtual DialogResult DialogResult {	// IButtonControl
			get { return dialog_result; }
			set { dialog_result = value; }
		}
		#endregion	// Public Properties

		#region Protected Properties
		protected override CreateParams CreateParams {
			get { return base.CreateParams; }
		}
		#endregion	// Protected Properties

		#region Public Methods
		public virtual void NotifyDefault (bool value)	// IButtonControl
		{	
			this.IsDefault = value;
		}

		public void PerformClick ()			// IButtonControl
		{			
			if (CanSelect)
				OnClick (EventArgs.Empty);
		}

		public override string ToString ()
		{
			return base.ToString () + ", Text: " + this.Text;
		}
		#endregion	// Public Methods

		#region	Protected Methods
		protected override void OnClick (EventArgs e)
		{
			Form p = FindForm ();
			if (p != null) {
				p.dialog_result_changed = false; // manages the case where the DialogResult of the form is overriden in the button click event.
				base.OnClick (e);
				if (dialog_result != DialogResult.None && !p.dialog_result_changed) {
					p.DialogResult = dialog_result;
				}
			} else {
				base.OnClick (e);
			}
		}

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

		protected override void OnMouseUp (MouseEventArgs mevent)
		{
			base.OnMouseUp (mevent);
		}

		protected override void OnTextChanged (EventArgs e)
		{
			base.OnTextChanged (e);
		}

		protected override bool ProcessMnemonic (char charCode)
		{
			if (this.UseMnemonic && IsMnemonic (charCode, Text) == true) {
				PerformClick ();
				return true;
			}

			return base.ProcessMnemonic (charCode);
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}
		#endregion	// Protected Methods

		#region Events
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public new event EventHandler DoubleClick {
			add { base.DoubleClick += value; }
			remove { base.DoubleClick -= value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public new event MouseEventHandler MouseDoubleClick {
			add { base.MouseDoubleClick += value; }
			remove { base.MouseDoubleClick -= value; }
		}
		#endregion	// Events

		#region	Internal methods
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

			ThemeEngine.Current.CalculateButtonTextAndImageLayout (pevent.Graphics, this, out text_rectangle, out image_rectangle);

			// Draw our button
			if (this.FlatStyle == FlatStyle.Standard)
				ThemeEngine.Current.DrawButton (pevent.Graphics, this, text_rectangle, image_rectangle, pevent.ClipRectangle);
			else if (this.FlatStyle == FlatStyle.Flat)
				ThemeEngine.Current.DrawFlatButton (pevent.Graphics, this, text_rectangle, image_rectangle, pevent.ClipRectangle);
			else if (this.FlatStyle == FlatStyle.Popup)
				ThemeEngine.Current.DrawPopupButton (pevent.Graphics, this, text_rectangle, image_rectangle, pevent.ClipRectangle);
		}

		internal override Size GetPreferredSizeCore (Size proposedSize)
		{
			Size size;

			if (this.AutoSize)
				size = ThemeEngine.Current.CalculateButtonAutoSize (this);
			else
				size = base.GetPreferredSizeCore (proposedSize);

			// Button has a special legacy behavior and implements AutoSizeMode itself
			if (AutoSizeMode == AutoSizeMode.GrowOnly)
				size = new Size (Math.Max (size.Width, Width), Math.Max (size.Height, Height));

			return size;
		}
		#endregion	// Internal methods
	}
}
