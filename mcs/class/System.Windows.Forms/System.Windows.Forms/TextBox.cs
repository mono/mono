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
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
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
				throw new NotImplementedException ();
			}
		}
		
		// --- Public Events
		
		[MonoTODO]
		public event EventHandler TextAlignChanged;
        
       //  --- Protected Properties
        
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "TEXTBOX";
				createParams.X = Left;
				createParams.Y = Top;
				createParams.Width = Width;
				createParams.Height = Height;
				createParams.ClassStyle = 0;
				createParams.ExStyle = 0;
				createParams.Param = 0;
				//			createParams.Parent = Parent.Handle;
				createParams.Style = (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE);
				window.CreateHandle (createParams);
				return createParams;
			}
		}
		 [MonoTODO]
		 protected override ImeMode DefaultImeMode {
			 get {
				 throw new NotImplementedException ();
			 }
		 }
		 [MonoTODO]
		 public override int SelectionLength {
			 get {
				 throw new NotImplementedException ();
			 }
			 set {
				 throw new NotImplementedException ();
			 }
		 }
		
		// --- Protected Members

		
		protected override bool IsInputKey(Keys keyData)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			//FIXME:
		}
		[MonoTODO]
		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			//FIXME:
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
		}
	}
}
