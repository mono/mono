//
// System.Windows.Forms.TextBox
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//
	// </summary>

     public class TextBox : TextBoxBase {

		//
		//  --- Public Constructor
		//
		[MonoTODO]
		public TextBox()
		{
			
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
		[MonoTODO]
		public HorizontalAlignment TextAlign {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				//FIXME:
			}
		}
		
		// --- Public Events
		
		[MonoTODO]
		public event EventHandler TextAlignChanged;
        
       //  --- Protected Properties
        
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				if( Parent != null) {
					CreateParams createParams = new CreateParams ();
					if (window == null) {
						window = new ControlNativeWindow (this);
					}

					createParams.Caption = Text;
					createParams.ClassName = "EDIT";
					createParams.X = Left;
					createParams.Y = Top;
					createParams.Width = Width;
					createParams.Height = Height;
					createParams.ClassStyle = 0;
					createParams.ExStyle = (int)WindowExStyles.WS_EX_CLIENTEDGE;
					createParams.Param = 0;
					createParams.Parent = Parent.Handle;
					createParams.Style = (int) (
						WindowStyles.WS_CHILD | 
						WindowStyles.WS_VISIBLE);
					return createParams;
				}
				return null;
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
	}
}
