//
// System.Windows.Forms.TextBox
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002 Ximian, Inc
//

using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	// Represents a Windows text box control.
	// </summary>

     public class TextBox : TextBoxBase {

		HorizontalAlignment textAlign;
		bool acceptsReturn;
		CharacterCasing characterCasing;
		char passwordChar;
		ScrollBars scrollbars;

		[MonoTODO]
		public TextBox()
		{
			textAlign = HorizontalAlignment.Left;
			acceptsReturn = true;
			characterCasing = CharacterCasing.Normal;
			passwordChar = (char)0;
			scrollbars = ScrollBars.None;
		}
		
		public bool AcceptsReturn  {
			get { return acceptsReturn; }
			set {
				if ( acceptsReturn != value ) {
					int oldStyle = acceptsReturn ? (int)EditControlStyles.ES_WANTRETURN : 0;
					acceptsReturn = value;
					int newStyle = acceptsReturn ? (int)EditControlStyles.ES_WANTRETURN : 0;
					if ( IsHandleCreated )
						Win32.UpdateWindowStyle ( Handle, oldStyle, newStyle );
				}
			}
		}

		public CharacterCasing CharacterCasing {
			get { return characterCasing; }
			set {
				if ( !Enum.IsDefined ( typeof(CharacterCasing), value ) )
					throw new InvalidEnumArgumentException( "CharacterCasing",
						(int)value,
						typeof(CharacterCasing));

				if ( characterCasing != value ) {
					int oldStyle = CaseStyle; 
					characterCasing = value;
					if ( IsHandleCreated )
						Win32.UpdateWindowStyle ( Handle, oldStyle, CaseStyle );
				    }
			}
		}

		public char PasswordChar {
			get { return passwordChar; }
			set {
				passwordChar = value;
				if ( IsHandleCreated )
					Win32.SendMessage ( Handle, (int) EditControlMessages.EM_SETPASSWORDCHAR, passwordChar, 0 );
			}
		}

		public ScrollBars ScrollBars {
			get { return scrollbars; }
			set {
				if ( !Enum.IsDefined ( typeof(ScrollBars), value ) )
					throw new InvalidEnumArgumentException( "ScrollBars",
						(int)value,
						typeof(ScrollBars));

				if ( scrollbars != value ) {
					int oldStyle = ScrollBarStyle; 
					scrollbars = value;
					if ( IsHandleCreated )
						Win32.UpdateWindowStyle ( Handle, oldStyle, ScrollBarStyle );
				    }
			}
		}

		public HorizontalAlignment TextAlign {
			get { return textAlign;	}
			set {
				if ( !Enum.IsDefined ( typeof(HorizontalAlignment), value ) )
					throw new InvalidEnumArgumentException( "TextAlign",
						(int)value,
						typeof(HorizontalAlignment));

				if ( textAlign != value ) {
					textAlign = value;

					OnTextAlignChanged ( EventArgs.Empty );
				}
			}
		}
		
		public event EventHandler TextAlignChanged;
        
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;

				createParams.ClassName = "EDIT";
				createParams.Style |= (int) ( WindowStyles.WS_CHILD ) | TextAlignStyle | ScrollBarStyle | CaseStyle;
				if ( AcceptsReturn )
					createParams.Style |= (int)EditControlStyles.ES_WANTRETURN;

				return createParams;
			}
		}

		 [MonoTODO]
		 protected override ImeMode DefaultImeMode {
			 get { return ImeMode.Inherit; }
		 }
		
		// --- Protected Members
		
		protected override bool IsInputKey(Keys keyData)
		{
			//FIXME:
			return base.IsInputKey(keyData);
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			//FIXME:
			base.OnHandleCreated(e);
			if ( PasswordChar != 0 )
				Win32.SendMessage ( Handle, (int) EditControlMessages.EM_SETPASSWORDCHAR, PasswordChar, 0 );
		}
		[MonoTODO]
		protected override void OnMouseUp(MouseEventArgs e)
		{
			//FIXME:
			base.OnMouseUp(e);
		}

		protected virtual void OnTextAlignChanged(EventArgs e)
		{
			if ( TextAlignChanged != null )
				TextAlignChanged ( this, EventArgs.Empty );
		}
		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			//FIXME:
			base.WndProc(ref m);
		}

		private int TextAlignStyle
		{
			get {
				int style = 0;
				switch ( TextAlign ) {
				case HorizontalAlignment.Left:
					style = (int) EditControlStyles.ES_LEFT;
				break;
				case HorizontalAlignment.Center:
					style = (int) EditControlStyles.ES_CENTER;
				break;
				case HorizontalAlignment.Right:
					style = (int) EditControlStyles.ES_RIGHT;
				break;
				}
				return style;
			}
		}

		private int ScrollBarStyle
		{
			get {
				int style = 0;
				switch ( this.ScrollBars ) {
				case ScrollBars.Vertical:
					style = (int) WindowStyles.WS_VSCROLL;
				break;
				case ScrollBars.Horizontal:
					if ( !WordWrap )
						style = (int) WindowStyles.WS_HSCROLL;
				break;
				case ScrollBars.Both:
					style = (int) WindowStyles.WS_VSCROLL;
					if ( !WordWrap )
						style = (int) WindowStyles.WS_HSCROLL;

				break;
				}
				return style;
			}
		}

	     private int CaseStyle
	     {
		     get {
				int style = 0;
				switch ( this.CharacterCasing ) {
				case CharacterCasing.Lower:
					style = (int) EditControlStyles.ES_LOWERCASE;
				break;
				case CharacterCasing.Upper:
					style = (int) EditControlStyles.ES_UPPERCASE;
				break;
				}
				return style;
		     }
	     }
	}
}
