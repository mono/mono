//
// System.Windows.Forms.PicureBox
//
// Author:
//   stubbed out by Hossein Safavi (hsafavi@purdue.edu)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Ximian, Inc
//

namespace System.Windows.Forms {

	//<summary>
	//	Skeleton. Nothing is implemented
	//
	//</summary>
	using System.Drawing;
	using System.ComponentModel;
	public class PictureBox : Control {

		Image image;
		PictureBoxSizeMode sizeMode;
		//
		// --- Public Constructor
		//
		[MonoTODO]
		public PictureBox () 
		{
			image = null;
			sizeMode = PictureBoxSizeMode.Normal;
		}
		//
		// --- Public Properties
		//

		[MonoTODO]
		public Image Image {
			get {
				return image;
			}
			set {
				image = value;
			}
		}

		[MonoTODO]
		public new ImeMode ImeMode {
			get {
				return base.ImeMode;
			}
			set {
				base.ImeMode = value;
			}
		}
		//
		// --- Protected Properties
		//
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "PICTUREBOX";
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
				return base.DefaultImeMode;
			}
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get {
				return new Size(100,50);
			}
		}
		//
		// --- Public Methods
		//
		[MonoTODO]
		public override string ToString()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public PictureBoxSizeMode SizeMode () {
			return sizeMode;
		}
		
		//
		// --- Protected Methods
		//

		[MonoTODO]
		protected override void OnEnabledChanged(EventArgs e) 
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override void OnPaint(PaintEventArgs pe) 
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override void OnParentChanged(EventArgs e) 
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override void OnResize(EventArgs e) 
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected virtual void OnSizeModeChanged(EventArgs e)
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override void OnVisibleChanged(EventArgs e) 
		{
			throw new NotImplementedException();
		}
		[MonoTODO]
		protected override void SetBoundsCore(int x,int y,int width,int height,BoundsSpecified specified) 
		{
			throw new NotImplementedException();
		}
		//
		// --- Public Events
		//

		[MonoTODO]
		public event EventHandler SizeModeChanged;
	}
}
