//
// System.Windows.Forms.UpDownBase
//
// Author:
//	 stubbed out by Stefan Warnke (StefanW@POBox.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Gianandrea Terzi (gianandrea.terzi@lario.com)
//
// (C) Ximian, Inc., 2002
//
using System;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
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

		/// --- Constructor ---
		public UpDownBase()	
		{
			throw new NotImplementedException ();
		}

		/// --- Destructor ---
		~UpDownBase() {
			throw new NotImplementedException ();
		}

		/// --- Public Properties ---
		#region Public Properties
		// Gets or sets the background color for the control
		public override Color BackColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		// Gets or sets the background image displayed in the control
		public override Image BackgroundImage {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		// Gets or sets the border style for the up-down control
		public BorderStyle BorderStyle {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		// Gets or sets the shortcut menu associated with the control
		public override ContextMenu ContextMenu {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		// Gets a value indicating whether the control has input focus
		public override bool Focused {
			get {
				throw new NotImplementedException ();
			}
		}
		
		// Gets or sets the foreground color of the control
		public override Color ForeColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		// Gets or sets a value indicating whether the user can use the 
		// UP ARROW and DOWN ARROW keys to select values
		public bool InterceptArrowKeys {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
		}

		// Gets or sets the site of the control.
		public override ISite Site {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		// Gets or sets the text displayed in the up-down control
		public override string Text {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		// Gets or sets the alignment of the text in the up-down control
		public HorizontalAlignment TextAlign {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		// Gets or sets the alignment of the up and down buttons on the 
		// up-down control
		public LeftRightAlignment UpDownAlign {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
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
			throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
		}

		// Gets the required creation parameters when the control handle is created
		protected override CreateParams CreateParams {
			get {
				throw new NotImplementedException ();
			}
		}

		// Gets the default size of the control.
		protected override Size DefaultSize {
			get {
				throw new NotImplementedException ();
			}
		}

		// Gets or sets a value indicating whether a value has been entered by the user
		protected bool UserEdit {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		#endregion // Protected Properties

	
		/// --- Protected Methods ---
		#region Protected Methods

		// Raises the FontChanged event
		protected override void OnFontChanged(EventArgs e) 
		{
			throw new NotImplementedException ();
		}

		// Raises the HandleCreated event
		protected override void OnHandleCreated(EventArgs e) 
		{
			throw new NotImplementedException ();
		}

		// Raises the Layout event
		protected override void OnLayout(LayoutEventArgs e) 
		{
			throw new NotImplementedException ();
		}
	
		// Raises the MouseWheel event
		protected override void OnMouseWheel(MouseEventArgs e) 
		{
			throw new NotImplementedException ();
		}

		// Raises the KeyDown event
		protected virtual void OnTextBoxKeyDown(object source, KeyEventArgs e) 
		{
			throw new NotImplementedException ();
		}

		// Raises the KeyPress event
		protected virtual void OnTextBoxKeyPress(object source, KeyPressEventArgs e) 
		{
			throw new NotImplementedException ();
		}

		// Raises the LostFocus event
		protected virtual void OnTextBoxLostFocus(object source, EventArgs e) 
		{
			throw new NotImplementedException ();
		}

		// Raises the Resize event
		protected virtual void OnTextBoxResize(object source, EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		// Raises the TextChanged event.
		protected virtual void OnTextBoxTextChanged(object source, EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		// This member overrides Control.SetBoundsCore.
		protected override void SetBoundsCore(int x, int y, int width, 
			int height, BoundsSpecified specified)  {
			throw new NotImplementedException ();
		}

		// When overridden in a derived class, updates the text displayed in the
		// up-down control
		protected abstract void UpdateEditText();

		// When overridden in a derived class, validates the text displayed in the
		// up-down control
		protected virtual void ValidateEditText() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]

			//FIXME shoould this be (ref message m)??
		protected virtual void WndProc(Message m) { // .NET V1.1 Beta
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void Dispose(bool Disposing) { // .NET V1.1 Beta
			throw new NotImplementedException ();
		}
		
		#endregion // Protected Methods

	}
}
