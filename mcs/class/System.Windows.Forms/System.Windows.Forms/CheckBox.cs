//
// System.Windows.Forms.CheckBox.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows check box.
	/// </summary>

	[MonoTODO]
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
		public Appearance Appearance {
			get { return appearance; }
			set { appearance=value; }
		}
		
		public bool AutoCheck {
			get { return autoCheck; }
			set { autoCheck = value; }
		}
		
		public ContentAlignment CheckAlign {
			get { return checkAlign; }
			set {
				checkAlign=value;
				UpdateCheckbox();
			}
		}
		
		public bool Checked {
			get { return _checked; }
			set { 
				if( _checked != value) {
					CheckState = (value) ? CheckState.Checked : CheckState.Unchecked;
				}
			}
		}
		
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
				state |= ButtonState.ThreeState;
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
