//
// System.Windows.Forms.Label.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	  implemented for Gtk+ by Rachel Hestilow (hestilow@ximian.com)
//
// (C) 2002 Ximian, Inc
//

using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {
	

	// <summary>
	//
	// </summary>
	
	public class Label : Control {
	
		private bool autoSize;
		private ContentAlignment textAlign = ContentAlignment.TopLeft;
		
		public Label () : base (){
			this.Text = " ";
			 AutoSize = false;
		}
		protected override void  OnTextChanged (EventArgs e){
			((Gtk.Label) Widget).Text = Text;
			
		}		
		internal override Gtk.Widget CreateWidget () {
			return new Gtk.Label (Text);
		}
		
		[MonoTODO]
		public virtual bool AutoSize{
			get { return autoSize;}
			set { autoSize = value;}
		}
		[MonoTODO]
		public virtual BorderStyle BorderStyle{
			get{ return BorderStyle.None; }
			set{
				if (!Enum.IsDefined (typeof(BorderStyle), value)){
					throw new InvalidEnumArgumentException();
				}
				//InvalidEnumArgumentException
			}
		}
		protected override Size DefaultSize{
			get{ return new Size (100,23); }
		}
		
		[MonoTODO]
		public FlatStyle FlatStyle {
			get { return FlatStyle.Standard;}
			set {}
		}
		[MonoTODO]
		public Image Image {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ContentAlignment ImageAlign {
			get {
				throw new NotImplementedException ();
			}
			set {
				//InvalidEnumArgumentException
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int ImageIndex {
			get {
				throw new NotImplementedException ();
			}
			set {
				//ArgumentException
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public ImageList ImageList {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public virtual int PreferredHeight {
			get {throw new NotImplementedException ();}
		}
		[MonoTODO]
		public virtual int PreferredWidth {
			get {throw new NotImplementedException ();}
		}
		[MonoTODO]
		public virtual bool RenderTransparent {
			get { return false; }
			set { }
		}
		
		[MonoTODO]
		public virtual ContentAlignment TextAlign {
			get {return textAlign;}
			set {
			
				if (!Enum.IsDefined(typeof(ContentAlignment), value)){
					throw new InvalidEnumArgumentException();
				}
				textAlign = value;				
			}
		}
		
		[MonoTODO]
		// default = true
		public bool UseMnemonic {
			get {throw new NotImplementedException ();}
			set {throw new NotImplementedException ();}
		}

		//
		//  --- Public Methods
		//
		//[MonoTODO]
		//public virtual bool Equals(object o);
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public static bool Equals(object o1, object o2);
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//public void Select()
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		public override string ToString(){
			return "System.Windows.Forms.Label, Text:" + this.Text;
		}

		//
		//  --- Public Events
		// 
		public event EventHandler AutoSizeChanged;
		public event EventHandler TextAlignChanged;
		

		//
		//  --- Protected Methods
		//
		[MonoTODO]
		protected  Rectangle CalcImageRenderBounds( Image image, Rectangle rect,  ContentAlignment align)
		{
			throw new NotImplementedException ();
		}
		
		//[MonoTODO]
		//protected  override void Dispose(bool val)
		//{
		//	throw new NotImplementedException ();
		//}
		[MonoTODO]
		protected  void DrawImage(  Graphics g,  Image img,  Rectangle r,  ContentAlignment align)		{
			throw new NotImplementedException ();
		}
		
		protected virtual void OnAutoSizeChanged (EventArgs e){
			if (AutoSizeChanged != null)
				AutoSizeChanged (this, e);
		}
		
		protected virtual void OnTextAlignChanged (EventArgs e){
			if (TextAlignChanged != null)
				TextAlignChanged (this, e);
		}
		
		//[MonoTODO]
		//protected override void  OnEnabledChanged (EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void  OnFontChanged (EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void  OnPaint (PaintEventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void  OnParentChanged (EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		

		//[MonoTODO]
		//protected override void  OnVisibleChanged (EventArgs e)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override bool ProcessMnemonic(char charCode)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected ContentAlignment RtlTranslateAlignment( ContentAlignment alignment)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected HorizontalAlignment RtlTranslateAlignment( HorizontalAlignment alignment)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected virtual void Select(bool val1, bool val2)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void SetBoundsCore(  int x, int y,  int width, int height  BoundsSpecified specified)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected void UpdateBounds()
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected void UpdateBounds(int b1, int b2, int b3, int b4)
		//{
		//	throw new NotImplementedException ();
		//}
		//[MonoTODO]
		//protected override void WndProc(ref Message m)
		//{
		//	throw new NotImplementedException ();
		//}

		
	}
}
