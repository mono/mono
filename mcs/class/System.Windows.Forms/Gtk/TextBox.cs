//
// System.Windows.Forms.TextBox
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//	Dennis Hayes (dennish@raytek.com)
//  Remco de Jong (rdj@rdj.cg.nu)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	// <summary>
	//	This is only a template.  Nothing is implemented yet.
	//
	// </summary>

     public class TextBox : TextBoxBase {

		private Gtk.TextView textview;
		private ScrollBars scrollbars;
		private HorizontalAlignment textalign;
		private bool wordwrap;

		//
		//  --- Public Constructor
		//
		public TextBox() {
			scrollbars = ScrollBars.None;
		}

		internal override Gtk.Widget CreateWidget () {
			// needs to initialized with a textbuffer from TextBoxBase
			// we need default adjustments, but the scrolledwindow constructor does not take null as argument
			
			Gtk.ScrolledWindow window = new Gtk.ScrolledWindow (
						 new Gtk.Adjustment (0, 0, 1, .1, .1, .1),
						 new Gtk.Adjustment (0, 0, 1, .1, .1, .1));

			window.SetPolicy(Gtk.PolicyType.Never, Gtk.PolicyType.Never);
			window.AddWithViewport(TextView);
			return window;
		}

		//  --- Public Properties
		
		public override bool ReadOnly {
			get
			{
				return !TextView.Editable;
			}
			set
			{
				if (value == TextView.Editable) { // only change if value is different, correct behaviour?
					TextView.Editable = !value;
					OnReadOnlyChanged(EventArgs.Empty);
				}
			}
		}

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
/*		public CharacterCasing CharacterCasing {
			get
			{
				throw new NotImplementedException ();
			}
			set
			{
				throw new NotImplementedException ();
			}
		}
*/		[MonoTODO]
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

		public ScrollBars ScrollBars {
			get {
				return scrollbars;
			}
			set {
				scrollbars = value;

				Gtk.PolicyType vpolicy = Gtk.PolicyType.Never; // correct behaviour?
				Gtk.PolicyType hpolicy = Gtk.PolicyType.Never;
				
				if (scrollbars == ScrollBars.Both) {
					vpolicy = Gtk.PolicyType.Always;
					hpolicy = Gtk.PolicyType.Always;
				}
				else if (scrollbars == ScrollBars.Horizontal) {
					hpolicy = Gtk.PolicyType.Always;
				}
				else if (scrollbars == ScrollBars.Vertical) {
					vpolicy = Gtk.PolicyType.Always;
				}

				((Gtk.ScrolledWindow) Widget).SetPolicy(hpolicy, vpolicy);
			}
		}

		public HorizontalAlignment TextAlign {
			get
			{
				return textalign;
			}
			set
			{
				Gtk.Justification justification = Gtk.Justification.Left;
				if (value == HorizontalAlignment.Center) {
					justification = Gtk.Justification.Center;
				}
				else if (value == HorizontalAlignment.Right) {
					justification = Gtk.Justification.Right;
				}	
				
				TextView.Justification = justification;
				textalign = value;
				
				OnTextAlignChanged(EventArgs.Empty);
			}
		}
		
			public override bool WordWrap {
			get
			{
				return wordwrap;
			}
			set
			{
				Gtk.WrapMode wrapmode = Gtk.WrapMode.None;
				wordwrap = value;
				if (wordwrap)
					wrapmode = Gtk.WrapMode.Word;
				
				TextView.WrapMode = wrapmode;
			}
		}

		
		// --- Public Events
		
		public event EventHandler TextAlignChanged;
        
       //  --- Protected Properties
        
/*		[MonoTODO]
		protected override CreateParams CreateParams {
			get
			{
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		protected override ImeMode DefaultImeMode {
			get
			{
				throw new NotImplementedException ();
			}
		}
*/		
		// --- Protected Members
		
		protected Gtk.TextView TextView {
			get {
				if (textview == null) {
					textview = new Gtk.TextView(TextBuffer);
					textview.Show();
				}
				return textview;
			}
		}

/*		protected override bool IsInputKey(Keys keyData)
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
*/
		protected virtual void OnTextAlignChanged(EventArgs e)
		{
			if (TextAlignChanged != null)
			 TextAlignChanged (this, e);
		}
		
/*		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			throw new NotImplementedException ();
		}
*/	}
}
