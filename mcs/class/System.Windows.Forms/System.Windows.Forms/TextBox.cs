//
// System.Windows.Forms.TextBox
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//

using System.ComponentModel;

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

     public class TextBox : TextBoxBase {

		HorizontalAlignment textAlign;
		//
		//  --- Public Constructor
		//
		[MonoTODO]
		public TextBox()
		{
			textAlign = HorizontalAlignment.Left;
		}
		
		//  --- Public Properties
		
		[MonoTODO]
		public bool AcceptsReturn  {

			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
			}
		}
		[MonoTODO]
		public CharacterCasing CharacterCasing {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
			}
		}
		[MonoTODO]
		public char PasswordChar {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
			}
		}
		[MonoTODO]
		public ScrollBars ScrollBars {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
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
		
		// --- Public Events
		
		[MonoTODO]
		public event EventHandler TextAlignChanged;
        
       //  --- Protected Properties
        
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = base.CreateParams;

				createParams.ClassName = "EDIT";
				createParams.ExStyle = (int)WindowExStyles.WS_EX_CLIENTEDGE;
				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE) | TextAlignStyle;
				return createParams;
			}
		}

		 [MonoTODO]
		 protected override ImeMode DefaultImeMode {
			 get {
				 //FIXME:
				 return base.ImeMode;
			 }
		 }
		 [MonoTODO]
		 public override int SelectionLength {
			 get {
				 //FIXME:
				 return base.SelectionLength;
			 }
			 set {
				 //FIXME:
				 base.SelectionLength = value;
			 }
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
		}
		[MonoTODO]
		protected override void OnMouseUp(MouseEventArgs e)
		{
			//FIXME:
			base.OnMouseUp(e);
		}
		[MonoTODO]
		//[Lame Spec] spec says this should be virtural
		//Spec was right!
		protected virtual void OnTextAlignChanged(EventArgs e)
		{
			//FIXME:
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
	}
}
