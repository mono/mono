//
// System.Windows.Forms.UpDownBase
//
// Author:
//	 stubbed out by Stefan Warnke (StefanW@POBox.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//	 Alexandre Pigolkine (pigolkine@gxm.de)
//
// (C) Ximian, Inc., 2002/3
//
using System;
using System.Drawing;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace System.Windows.Forms {
	internal class SpinnerControl : Control {
		
		public SpinnerControl() {
			SubClassWndProc_ = true;
		}
		
		protected override CreateParams CreateParams {
			get {
				if( Parent != null) {
					CreateParams createParams = new CreateParams ();
		
					createParams.Caption = Text;
					createParams.ClassName = "msctls_updown32";
					createParams.X = Left;
					createParams.Y = Top;
					createParams.Width = Width;
					createParams.Height = Height;
					createParams.ClassStyle = 0;
					createParams.ExStyle = 0;
					createParams.Param = 0;
					createParams.Parent = Parent.Handle;
					createParams.Style = (int) (
						WindowStyles.WS_CHILD | 
						WindowStyles.WS_VISIBLE);
					createParams.Style |= (int)(UpDownControlStyles.UDS_ALIGNRIGHT |
					                            UpDownControlStyles.UDS_AUTOBUDDY );
					return createParams;
				}
				return null;
			}
		}
		
		protected override void WndProc(ref Message m) { 
			switch( m.Msg) 
			{
			case Msg.WM_NOTIFY:
				NM_UPDOWN nmupdown = (NM_UPDOWN)Marshal.PtrToStructure ( m.LParam, typeof ( NM_UPDOWN ) );
				// With default setup 
				// NM_UPDOWN.iDelta < 0, then Up button pressed
				// NM_UPDOWN.iDelta > 0, then Down button pressed
				// CHECKME: do we need to call Up/Down Abs(delta) times ?
				UpDownBase parentUpDown = Parent as UpDownBase;
				if( parentUpDown != null) {
					if( nmupdown.iDelta < 0) {
						parentUpDown.UpButton();
					}
					else {
						parentUpDown.DownButton();
					}
				}
				else {
					CallControlWndProc(ref m);
				}
				break;
			case Msg.WM_HSCROLL:
			case Msg.WM_VSCROLL:
				CallControlWndProc(ref m);
				break;
			default:
				base.WndProc(ref m);
				break;
			}
		}
	}

	// <summary>
	// </summary>


	/// <summary>
	/// The up-down control consists of a text box and a small vertical scroll 
	/// bar, commonly referred to as a spinner control.
	/// </summary>
	public abstract class UpDownBase : ContainerControl {

		//UpDownBase+ButtonID
		enum ButtonID 
		{
			Down = 2, 
			None = 0, 
			Up = 1
		}

		internal TextBox			EditBox_;
		internal SpinnerControl		Spinner_;
		private bool				UserEdit_;
		/// --- Constructor ---
		public UpDownBase()	
		{
			UserEdit_ = false;
			Win32.InitCommonControls();
			EditBox_ = new TextBox();
			EditBox_.Location = new System.Drawing.Point(0, 0);
			Spinner_ = new SpinnerControl();
			this.Controls.Add(EditBox_);
			this.Controls.Add(Spinner_);
		}

		/// --- Destructor ---

		/// --- Public Properties ---
		#region Public Properties
		// Gets or sets the background color for the control
		public override Color BackColor {
			get {
				return base.BackColor;
				//FIXME:
			}
			set {
				//FIXME:
				base.BackColor = value;
			}
		}

		// Gets or sets the background image displayed in the control
		public override Image BackgroundImage {
			get {
				//FIXME:
				return base.BackgroundImage;
			}
			set {
				//FIXME:
				base.BackgroundImage = value;
			}
		}

		// Gets or sets the border style for the up-down control
		public BorderStyle BorderStyle {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		// Gets or sets the shortcut menu associated with the control
		public override ContextMenu ContextMenu {
			get {
				//FIXME:
				return base.ContextMenu;
			}
			set {
				//FIXME:
				base.ContextMenu = value;
			}
		}

		// Gets a value indicating whether the control has input focus
		public override bool Focused {
			get {
				//FIXME:
				return base.Focused;
			}
		}
		
		// Gets or sets the foreground color of the control
		public override Color ForeColor {
			get {
				//FIXME:
				return base.ForeColor;
			}
			set {
				//FIXME:
				base.ForeColor = value;
			}
		}

		// Gets or sets a value indicating whether the user can use the 
		// UP ARROW and DOWN ARROW keys to select values
		public bool InterceptArrowKeys {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		// Gets the height of the up-down control
		public int PreferredHeight {
			get {
				throw new NotImplementedException ();
			}
		}

		// Gets or sets a value indicating whether the text may be 
		// changed by the use of the up or down buttons only
		public bool ReadOnly {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		// Gets or sets the site of the control.
		public override ISite Site {
			get {
				//FIXME:
				return base.Site;
			}
			set {
				//FIXME:
				base.Site = value;
			}
		}

		// Gets or sets the text displayed in the up-down control
		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
				EditBox_.Text = value;
			}
		}

		// Gets or sets the alignment of the text in the up-down control
		public HorizontalAlignment TextAlign {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		// Gets or sets the alignment of the up and down buttons on the 
		// up-down control
		public LeftRightAlignment UpDownAlign {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		#endregion // Public Properties


		/// --- Public Methods ---
		#region Public Methods

		// When overridden in a derived class, handles the pressing of the down 
		// button on the up-down control. 
		public abstract void DownButton();
	
		// Selects a range of text in the up-down control specifying the 
		// starting position and number of characters to select.
		public void Select(int start,int length) 
		{
			//FIXME:
		}
		
		// When overridden in a derived class, handles the pressing of 
		// the up button on the up-down control
		public abstract void UpButton();

		#endregion // Public Methods


		/// --- Protected Properties ---
		#region Protected Properties

		// Gets or sets a value indicating whether the text property is being 
		// changed internally by its parent class
		protected bool ChangingText {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		// Gets the required creation parameters when the control handle is created
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		// Gets the default size of the control.
		protected override Size DefaultSize {
			get {
				return new System.Drawing.Size(100,20);
			}
		}

		// Gets or sets a value indicating whether a value has been entered by the user
		protected bool UserEdit {
			get {
				return UserEdit_;
			}
			set {
				UserEdit_ = true;
			}
		}
		#endregion // Protected Properties

	
		/// --- Protected Methods ---
		#region Protected Methods

		// Raises the FontChanged event
		protected override void OnFontChanged(EventArgs e) 
		{
			//FIXME:
			base.OnFontChanged(e);
		}

		// Raises the HandleCreated event
		protected override void OnHandleCreated(EventArgs e) 
		{
			//FIXME:
			base.OnHandleCreated(e);
			EditBox_.Text = this.Text;
		}

		// Raises the Layout event
		protected override void OnLayout(LayoutEventArgs e) 
		{
			//FIXME:
			base.OnLayout(e);
		}
	
		// Raises the MouseWheel event
		protected override void OnMouseWheel(MouseEventArgs e) 
		{
			//FIXME:
		}

		// Raises the KeyDown event
		protected virtual void OnTextBoxKeyDown(object source, KeyEventArgs e) 
		{
			//FIXME:
		}

		// Raises the KeyPress event
		protected virtual void OnTextBoxKeyPress(object source, KeyPressEventArgs e) 
		{
			//FIXME:
		}

		// Raises the LostFocus event
		protected virtual void OnTextBoxLostFocus(object source, EventArgs e) 
		{
			//FIXME:
		}

		// Raises the Resize event
		protected virtual void OnTextBoxResize(object source, EventArgs e) 
		{
			//FIXME:
		}
		
		// Raises the TextChanged event.
		protected virtual void OnTextBoxTextChanged(object source, EventArgs e) 
		{
			//FIXME:
		}
		
		// This member overrides Control.SetBoundsCore.
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)  {
			//FIXME: use PreferredHeight ?
			base.SetBoundsCore(x,y,width,height,specified);
			EditBox_.Size = new System.Drawing.Size(width, height);
		}

		// When overridden in a derived class, updates the text displayed in the
		// up-down control
		protected abstract void UpdateEditText();

		// When overridden in a derived class, validates the text displayed in the
		// up-down control
		protected virtual void ValidateEditText() 
		{
			//FIXME:
		}

		[MonoTODO]

			//FIXME shoould this be (ref message m)??
		protected override void WndProc(ref Message m) { // .NET V1.1 Beta
			//FIXME:
			base.WndProc(ref m);
		}
		
		[MonoTODO]
		protected override void Dispose(bool Disposing) { // .NET V1.1 Beta
			base.Dispose(Disposing);
		}
		
		#endregion // Protected Methods

	}
}
