//
// System.Windows.Forms.Splitter.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class Splitter : Control, IMessageFilter {

		//
		//  --- Constructor
		//
		[MonoTODO]
		public Splitter()
		{
			
		}

		//
		//  --- Public Properties
		//
		[MonoTODO]
		public override  bool AllowDrop {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override  AnchorStyles Anchor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override  Image BackgroundImage {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public BorderStyle BorderStyle {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override DockStyle Dock {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Font Font {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override Color ForeColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int MinExtra {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int MinSize {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override ISite Site {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public int SplitPosition {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		[MonoTODO]
		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
			}
		}

		
		//  --- Public Methods

		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException ();
		}

		//
		//  --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "SPLITTER";
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
		protected override Size DefaultSize {
			get {
				return new System.Drawing.Size(10,500);
			}
		}

		//
		//  --- Protected Methods
		//

		[MonoTODO]
		protected override void OnKeyDown(KeyEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnMouseDown(MouseEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnMouseMove(MouseEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnMouseUp(MouseEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void SetBoundsCore( int x, int y, int width, int height, BoundsSpecified specified)
		{
			throw new NotImplementedException ();
		}
		bool IMessageFilter.PreFilterMessage(ref Message m){
			throw new NotImplementedException ();
		}
	 }
}
