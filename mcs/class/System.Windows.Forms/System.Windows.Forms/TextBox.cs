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
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

     public class TextBox : TextBoxBase {

		//
		//  --- Public Constructor
		//
		[MonoTODO]
		public TextBox()
		{
			throw new NotImplementedException ();
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
			get
			{
				throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		//[Lame Spec] spec says this should be virtural
		//Spec was right!
		protected virtual void OnTextAlignChanged(EventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			throw new NotImplementedException ();
		}
	}
}
