//
// System.Windows.Forms.ButtonBase.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	  implemented for Gtk+ by Rachel Hestilow (hestilow@ximian.com)
//
// (C) Ximian, Inc., 2002
//

using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms
{
	/// <summary>
	/// Implements the basic functionality common to button controls.
	/// ToDo note:
	///  - no methods are implemented
	/// </summary>

	public abstract class ButtonBase : Control
	{
		// private fields
//		FlatStyle flatStyle;
//		Image image;
//		ContentAlignment imageAlign;
//		int imageIndex;
		ContentAlignment textAlign;
		internal protected Label label;

//		
//		// --- Constructor ---
		protected ButtonBase() : base() {
//			flatStyle = FlatStyle.Standard;
//			image = null;
//			imageAlign = ContentAlignment.MiddleCenter;
//			imageIndex = -1;
			textAlign = ContentAlignment.MiddleCenter;
			label = new Label ();
			label.Text = Text;
			label.Visible = true;
			ConnectToClicked ();
		}
		
		internal protected void clicked_cb (object o, EventArgs args)
		{
			OnClick (EventArgs.Empty);
		}
		
		internal protected void ConnectToClicked ()
		{
			((Gtk.Button) Widget).Clicked += new EventHandler (clicked_cb);
		}
		
		// --- Properties ---
//		[MonoTODO]
//		protected override CreateParams CreateParams {
//			get { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		protected override ImeMode DefaultImeMode {
//			get { throw new NotImplementedException (); }
//		}
//		
//		[MonoTODO]
//		protected override Size DefaultSize {
//			get { throw new NotImplementedException (); }
//		}
//		
//		public FlatStyle FlatStyle {
//			get { return flatStyle; }
//			set { flatStyle=value; }
//		}
//		
//		public Image Image {
//			get { return image; }
//			set { image=value; }
//		}
//		
//		public ContentAlignment ImageAlign {
//			get { return imageAlign; }
//			set { imageAlign=value; }
//		}
//		
//		public int ImageIndex {
//			get { return imageIndex; }
//			set { imageIndex=value; }
//		}
//		
//		[MonoTODO]
//		public new ImeMode ImeMode {
//			get { throw new NotImplementedException (); }
//			set { throw new NotImplementedException (); }
//		}
//		
//		protected bool IsDefault {
//			get { throw new NotImplementedException (); }
//			set { throw new NotImplementedException (); }
//		}
		
		public virtual ContentAlignment TextAlign {
			get { return label.TextAlign; }
			set { label.TextAlign=value; }
		}
//		
//		
//		
//		
//		
//		/// --- Methods ---
//		/// internal .NET framework supporting methods, not stubbed out:
//		/// - protected override void Dispose(bool);
//		/// - protected void ResetFlagsandPaint();
//		[MonoTODO]
//		protected override AccessibleObject CreateAccessibilityInstance() {
//			throw new NotImplementedException ();
//		}
//		
//		/// [methods for events]
//		[MonoTODO]
//		protected override void OnEnabledChanged(EventArgs e) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnGotFocus(EventArgs e) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnKeyDown(KeyEventArgs kevent) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnKeyUp(KeyEventArgs kevent) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnLostFocus(EventArgs e) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnMouseDown(MouseEventArgs mevent) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnMouseEnter(EventArgs eventargs) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnMouseLeave(EventArgs eventargs) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnMouseMove(MouseEventArgs mevent) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnMouseUp(MouseEventArgs mevent) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnPaint(PaintEventArgs pevent) {
//			throw new NotImplementedException ();
//		}
//		
//		[MonoTODO]
//		protected override void OnParentChanged(EventArgs e) {
//			throw new NotImplementedException ();
//		}
//		
		protected override void OnTextChanged(EventArgs e) {
			label.Text = Text;
		}
//		
//		[MonoTODO]
//		protected override void OnVisibleChanged(EventArgs e) {
//			throw new NotImplementedException ();
//		}
//		/// end of [methods for events]
//		
//		[MonoTODO]
//		protected override void WndProc(ref Message m) {
//			throw new NotImplementedException ();
//		}
//		
//		
//		
//		/// --- ButtonBase.ButtonBaseAccessibleObject ---
//		/// the class is not stubbed, cause it's only used for .NET framework
//		
//		
	}
}
