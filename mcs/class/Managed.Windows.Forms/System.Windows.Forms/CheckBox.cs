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
//	Dennis Hayes	dennish@raytek.com
//	Peter Bartok	pbartok@novell.com
//
//
// $Revision: 1.2 $
// $Modtime: $
// $Log: CheckBox.cs,v $
// Revision 1.2  2004/08/30 15:44:20  pbartok
// - Updated to fix broken build. Not complete yet.
//
// Revision 1.1  2004/07/09 05:21:25  pbartok
// - Initial check-in
//
//

// NOT COMPLETE

using System;
using System.Drawing;

namespace System.Windows.Forms {
	public class CheckBox : ButtonBase {
		private Appearance		appearance;
		private bool			auto_check;
		private ContentAlignment	check_alignment;
		private ContentAlignment	text_alignment;
		private CheckState		check_state;
		private bool			three_state;

		#region Public Constructors
		public CheckBox() {
			appearance = Appearance.Normal;
			auto_check = true;
			check_alignment = ContentAlignment.MiddleLeft;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public Appearance Appearance {
			get {
				return appearance;
			}

			set {
				if (value != appearance) {
					value = appearance;
					if (AppearanceChanged != null) {
						AppearanceChanged(this, EventArgs.Empty);
					}
				}
			}
		}

		public bool AutoCheck {
			get {
				return auto_check;
			}

			set {
				auto_check = value;
			}
		}

		public ContentAlignment CheckAlign {
			get {
				return check_alignment;
			}

			set {
				if (value != check_alignment) {
					check_alignment = value;

					Redraw();
				}
			}
		}

		public bool Checked {
			get {
				if (check_state != CheckState.Unchecked) {
					return true;
				}
				return false;
			}

			set {
				if (value && (check_state != CheckState.Checked)) {
					check_state = CheckState.Checked;
					Redraw();
					OnCheckedChanged(EventArgs.Empty);
				} else if (!value && (check_state != CheckState.Unchecked)) {
					check_state = CheckState.Unchecked;
					Redraw();
					OnCheckedChanged(EventArgs.Empty);
				}
			}
		}

		public CheckState CheckState {
			get {
				return check_state;
			}

			set {
				if (value != check_state) {
					bool	was_checked = (check_state != CheckState.Unchecked);

					check_state = value;

					if (was_checked != (check_state != CheckState.Unchecked)) {
						OnCheckedChanged(EventArgs.Empty);
					}

					OnCheckStateChanged(EventArgs.Empty);
					Redraw();
				}
			}
		}

		public override ContentAlignment TextAlign {
			get {
				return text_alignment;
			}

			set {
				if (value != text_alignment) {
					text_alignment = value;
					Redraw();
				}
			}
		}


		public bool ThreeState {
			get {
				return three_state;
			}

			set {
				three_state = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		protected override CreateParams CreateParams {
			get {
				return base.CreateParams;
			}
		}

		protected override Size DefaultSize {
			get {
				return new Size(104, 24);
			}
		}
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		public override string ToString() {
			if (CheckState == CheckState.Unchecked) {
				return "CheckBox (unchecked)";
			} else if (CheckState == CheckState.Checked) {
				return "CheckBox (checked)";
			} else {
				return "CheckBox (state indeterminate)";
			}
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected override AccessibleObject CreateAccessibilityInstance() {
			return base.CreateAccessibilityInstance ();
		}

		protected virtual void OnAppearanceChanged(EventArgs e) {
			if (AppearanceChanged != null) {
				AppearanceChanged(this, e);
			}
		}

		protected virtual void OnCheckedChanged(EventArgs e) {
Console.WriteLine("Checked changed");
			if (CheckedChanged != null) {
				CheckedChanged(this, e);
			}
		}

		protected virtual void OnCheckStateChanged(EventArgs e) {
Console.WriteLine("CheckState changed");
			if (CheckStateChanged != null) {
				CheckStateChanged(this, e);
			}
		}

		protected override void OnClick(EventArgs e) {
Console.WriteLine("Got click");
			base.OnClick (e);
		}

		protected override void OnHandleCreated(EventArgs e) {
			base.OnHandleCreated (e);
		}

		protected override void OnMouseUp(MouseEventArgs e) {
			base.OnMouseUp (e);
		}

		protected override bool ProcessMnemonic(char charCode) {
			return base.ProcessMnemonic (charCode);
		}

		#endregion	// Protected Instance Methods

		#region Events
		public event EventHandler	AppearanceChanged;
		public event EventHandler	CheckedChanged;
		public event EventHandler	CheckStateChanged;
		#endregion	// Events
	}
}
#if not
	public class CheckBox : ButtonBase {

		// private fields
		Appearance			appearance;
		bool					autoCheck;
		ContentAlignment	checkAlign;
		bool					_checked;
		CheckState			checkState;
		bool					threeState;
		ContentAlignment	textAlign;
		Rectangle			textRect;
		Rectangle			checkRect;
		StringFormat		textFormat;
		int					checkMarkSize=13;		// Keep it configurable for accessability
		Graphics				canvasDC;
		Bitmap				canvasBmp;
		
		// --- Constructor ---
		public CheckBox() : base() 
		{

			appearance = Appearance.Normal;
			autoCheck = true;
			checkAlign = ContentAlignment.MiddleLeft;
			_checked = false;
			checkState = CheckState.Unchecked;
			threeState = false;

			canvasBmp = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
			canvasDC = Graphics.FromImage(canvasBmp);

			/* Set defaults for drawing text... */
			textAlign = ContentAlignment.MiddleLeft;
			textFormat = new StringFormat();
			textFormat.Alignment=StringAlignment.Near;
			textFormat.LineAlignment=StringAlignment.Center;
			textRect = ClientRectangle;
			textRect.X+=checkMarkSize;
			textRect.Width-=checkMarkSize;

			/* ... and for drawing our checkbox */
			checkRect.X=ClientRectangle.Left;
			checkRect.Y=(ClientRectangle.Bottom-ClientRectangle.Top)/2-checkMarkSize/2;
			checkRect.Width=checkMarkSize;
			checkRect.Height=checkMarkSize;

			SizeChanged+=new System.EventHandler(CheckboxSizeChanged);
			GotFocus+=new System.EventHandler(CheckboxUpdate);
			LostFocus+=new System.EventHandler(CheckboxUpdate);
			TextChanged+=new System.EventHandler(CheckboxUpdate);

			SubClassWndProc_ = true;
			SetStyle (ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint, true);
			callWinControlProcMask &= ~(CallWinControlProcMask.MOUSE_MESSAGES | CallWinControlProcMask.KEYBOARD_MESSAGES);
		}
		
		// --- CheckBox Properties ---
	
		public CheckState CheckState {
			get { return checkState; }
			set { 
				if( checkState != value) {

					checkState = value; 
					bool oldChecked = _checked;

					if( checkState == CheckState.Unchecked) {
						_checked = false;
					}
					else {
						_checked = true;
					}

					if( oldChecked != _checked) { 
						OnCheckedChanged(new EventArgs());
					}

					OnCheckStateChanged(new EventArgs());
				}
			}
		}
		
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;
	
				createParams.ClassName = "BUTTON";

				createParams.Style = (int) (
					(int)WindowStyles.WS_CHILD | 
					(int)WindowStyles.WS_VISIBLE | 
					(int)ButtonStyles.BS_CHECKBOX |
					(int)ButtonStyles.BS_NOTIFY |
					(int)WindowStyles.WS_CLIPSIBLINGS |
					(int)WindowStyles.WS_CLIPCHILDREN |
					(int)WindowStyles.WS_TABSTOP |
					(int)SS_Static_Control_Types.SS_LEFT );

				if (autoCheck) {
					createParams.Style |= (int)ButtonStyles.BS_AUTOCHECKBOX;
				}

				/* We need this, we draw ourselves */
				createParams.Style |= (int) ButtonStyles.BS_OWNERDRAW;

				return createParams;
			}
		}
		
		[MonoTODO]
		protected override Size DefaultSize {
			get { return new Size(100,checkMarkSize); }
		}
		
		[MonoTODO]
		public override ContentAlignment TextAlign {
			get {
				return textAlign;
			}
			set {
				textAlign = value;
				UpdateCheckbox();
			}
		}
		
		public bool ThreeState {
			get {
				return threeState;
			}

			set {
				threeState = value;
			}
		}	
		
		// --- CheckBox methods ---

		protected override AccessibleObject CreateAccessibilityInstance() 
		{
			throw new NotImplementedException ();
		}
		
		
		// [event methods]
		[MonoTODO]
		protected virtual void OnAppearanceChanged(EventArgs e) 
		{
			if (AppearanceChanged != null) {
				AppearanceChanged(this, e);
			}
		}
		
		[MonoTODO]
		protected virtual void OnCheckedChanged(EventArgs e) 
		{
			//FIXME:
			if(CheckedChanged != null) {
				CheckedChanged( this, e);
			}
		}
		
		[MonoTODO]
		protected virtual void OnCheckStateChanged(EventArgs e) 
		{
			//FIXME:
			if(CheckStateChanged != null) {
				CheckStateChanged( this, e);
			}
		}
		
		[MonoTODO]
		protected override void OnClick(EventArgs e) 
		{
			CheckState = (CheckState)Win32.SendMessage(Handle, (int)ButtonMessages.BM_GETCHECK, 0, 0);
			base.OnClick(e);
		}
		
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			//FIXME:
			base.OnHandleCreated(e);
			Win32.SendMessage(Handle, (int)ButtonMessages.BM_SETCHECK, (int)checkState, 0);
		}
		
//		protected override void OnMouseDown (MouseEventArgs e) 
//		{
//			base.OnMouseDown (e);
//		}
		
		[MonoTODO]
		protected override void OnMouseUp(MouseEventArgs mevent) 
		{
			if (ThreeState) {
				switch (CheckState) {
					case CheckState.Unchecked: {
						CheckState = CheckState.Checked;
						break;
					}

					case CheckState.Indeterminate: {
						CheckState = CheckState.Unchecked;
						break;
					}

					case CheckState.Checked: {
						CheckState = CheckState.Indeterminate;
						break;
					}
				}
			} else {
				Checked = Checked ? false : true;
			}
			CheckboxRedraw();
			Invalidate();
			base.OnMouseUp(mevent);
		}
		// end of [event methods]
		
		
		[MonoTODO]
		protected override bool ProcessMnemonic(char charCode) 
		{
			//FIXME:
			return base.ProcessMnemonic(charCode);
		}
		
		[MonoTODO]
		//FIXME: do a better tostring
		public override string ToString() 
		{
			if (Checked) {
				return "CheckBox" + " Checked";
			} else {
				return "CheckBox" +  " Unchecked";
			}
		}
		
		internal override void ButtonPaint (PaintEventArgs e) 
		{
			if (canvasBmp!=null) {
				e.Graphics.DrawImage(canvasBmp, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
			}
		}
		
		/// --- CheckBox events ---
		public event EventHandler AppearanceChanged;
		public event EventHandler CheckedChanged;
		public event EventHandler CheckStateChanged;
		
		/// --- public class CheckBox.CheckBoxAccessibleObject : AccessibleObject ---
		/// the class is only used for .NET framework
		/// 
		public class CheckBoxAccessibleObject : AccessibleObject {
		}

		private void UpdateCheckbox() {
			/* Calculate the position of text and checkbox rectangle */
			if (appearance!=Appearance.Button) {
				switch(checkAlign) {
					case ContentAlignment.BottomCenter: {
						if (ClientRectangle.Height<checkMarkSize*2) {
							ClientSize=new Size(ClientRectangle.Width, checkMarkSize*2);
						}
						checkRect.X=(ClientRectangle.Right-ClientRectangle.Left)/2-checkMarkSize/2;
						checkRect.Y=ClientRectangle.Bottom-checkMarkSize;
						textRect.X=ClientRectangle.X;
						textRect.Width=ClientRectangle.Width;
						break;
					}

					case ContentAlignment.BottomLeft: {
						checkRect.X=ClientRectangle.Left;
						checkRect.Y=ClientRectangle.Bottom-checkMarkSize;
						textRect.X=ClientRectangle.X+checkMarkSize;
						textRect.Width=ClientRectangle.Width-checkMarkSize;
						break;
					}

					case ContentAlignment.BottomRight: {
						checkRect.X=ClientRectangle.Right-checkMarkSize;
						checkRect.Y=ClientRectangle.Bottom-checkMarkSize;
						textRect.X=ClientRectangle.X;
						textRect.Width=ClientRectangle.Width-checkMarkSize;
						break;
					}

					case ContentAlignment.MiddleCenter: {
						checkRect.X=(ClientRectangle.Right-ClientRectangle.Left)/2-checkMarkSize/2;
						checkRect.Y=(ClientRectangle.Bottom-ClientRectangle.Top)/2-checkMarkSize/2;
						textRect.X=ClientRectangle.X;
						textRect.Width=ClientRectangle.Width;
						break;
					}

					default:
					case ContentAlignment.MiddleLeft: {
						checkRect.X=ClientRectangle.Left;
						checkRect.Y=(ClientRectangle.Bottom-ClientRectangle.Top)/2-checkMarkSize/2;
						textRect.X=ClientRectangle.X+checkMarkSize;
						textRect.Width=ClientRectangle.Width-checkMarkSize;
						break;
					}

					case ContentAlignment.MiddleRight: {
						checkRect.X=ClientRectangle.Right-checkMarkSize;
						checkRect.Y=(ClientRectangle.Bottom-ClientRectangle.Top)/2-checkMarkSize/2;
						textRect.X=ClientRectangle.X;
						textRect.Width=ClientRectangle.Width-checkMarkSize;
						break;
					}

					case ContentAlignment.TopCenter: {
						if (ClientRectangle.Height<checkMarkSize*2) {
							ClientSize=new Size(ClientRectangle.Width, checkMarkSize*2);
						}
						checkRect.X=(ClientRectangle.Right-ClientRectangle.Left)/2-checkMarkSize/2;
						checkRect.Y=ClientRectangle.Top;
						textRect.X=ClientRectangle.X;
						textRect.Y=checkMarkSize;
						textRect.Width=ClientRectangle.Width;
						textRect.Height=ClientRectangle.Height-checkMarkSize;
						break;
					}

					case ContentAlignment.TopLeft: {
						checkRect.X=ClientRectangle.Left;
						checkRect.Y=ClientRectangle.Top;
						textRect.X=ClientRectangle.X+checkMarkSize;
						textRect.Width=ClientRectangle.Width-checkMarkSize;
						break;
					}

					case ContentAlignment.TopRight: {
						checkRect.X=ClientRectangle.Right-checkMarkSize;
						checkRect.Y=ClientRectangle.Top;
						textRect.X=ClientRectangle.X;
						textRect.Width=ClientRectangle.Width-checkMarkSize;
						break;
					}
				}
			} else {
				textRect.X=ClientRectangle.X;
				textRect.Width=ClientRectangle.Width;
			}

			/* Set the horizontal alignment of our text */
			switch(textAlign) {
				case ContentAlignment.BottomLeft:
				case ContentAlignment.MiddleLeft:
				case ContentAlignment.TopLeft: {
					textFormat.Alignment=StringAlignment.Near;
					break;
				}

				case ContentAlignment.BottomCenter:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.TopCenter: {
					textFormat.Alignment=StringAlignment.Center;
					break;
				}

				case ContentAlignment.BottomRight:
				case ContentAlignment.MiddleRight:
				case ContentAlignment.TopRight: {
					textFormat.Alignment=StringAlignment.Far;
					break;
				}
			}

			/* Set the vertical alignment of our text */
			switch(textAlign) {
				case ContentAlignment.TopLeft: 
				case ContentAlignment.TopCenter: 
				case ContentAlignment.TopRight: {
					textFormat.LineAlignment=StringAlignment.Near;
					break;
				}

				case ContentAlignment.BottomLeft:
				case ContentAlignment.BottomCenter:
				case ContentAlignment.BottomRight: {
					textFormat.LineAlignment=StringAlignment.Far;
					break;
				}

				case ContentAlignment.MiddleLeft:
				case ContentAlignment.MiddleCenter:
				case ContentAlignment.MiddleRight: {
					textFormat.LineAlignment=StringAlignment.Center;
					break;
				}
			}
			CheckboxRedraw();
			Invalidate();
		}

		private void CheckboxRedraw() {
			SolidBrush	sb;

			/* Build the image representing the control */

			if (canvasDC!=null) {
				canvasDC.Dispose();
			}
			if (canvasBmp!=null) {
				canvasBmp.Dispose();
			}

			if (ClientRectangle.Width<1 || ClientRectangle.Height<1) {
				return;
			}
			canvasBmp = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
			canvasDC = Graphics.FromImage(canvasBmp);

			sb=new SolidBrush(BackColor);
			canvasDC.FillRectangle(sb, ClientRectangle);
			sb.Dispose();

			ButtonState state = ButtonState.Normal;
			if (FlatStyle == FlatStyle.Flat) {
				state |= ButtonState.Flat;
			}
			
			if (Checked) {
				state |= ButtonState.Checked;
			}
			
			if (ThreeState && (CheckState == CheckState.Indeterminate)) {
				state |= ButtonState.Checked;
				state |= ButtonState.Pushed;
			}

			if (appearance!=Appearance.Button) {
				ControlPaint.DrawCheckBox(canvasDC, checkRect, state);
			} else {
				ControlPaint.DrawButton(canvasDC, textRect, state);
			}

			/* Place the text; to be compatible with Windows place it after the checkbox has been drawn */
			sb=new SolidBrush(base.foreColor);
			canvasDC.DrawString(Text, Font, sb, textRect, textFormat);
			sb.Dispose();

			if (Focused) {
				ControlPaint.DrawFocusRectangle(canvasDC, textRect);
			}
		}

		private void CheckboxUpdate(object sender, System.EventArgs e) {
			/* Force recalculation of text & checkbox rectangles */
			UpdateCheckbox();
		}

		private void CheckboxSizeChanged(object sender, System.EventArgs e)
		{
			/* Force recalculation of text & checkbox rectangles */
			textRect.Y=ClientRectangle.Y;
			textRect.Height=ClientRectangle.Height;
			UpdateCheckbox();
		}
	}


}
#endif
