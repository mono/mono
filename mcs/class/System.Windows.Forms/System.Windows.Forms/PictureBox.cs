//
// System.Windows.Forms.PicureBox
//
// Author:
//   stubbed out by Hossein Safavi (hsafavi@purdue.edu)
//	Dennis Hayes (dennish@raytek.com)
//   Aleksey Ryabchuk (ryabchuk@yahoo.com)
//
// (C) 2002 Ximian, Inc
//
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	//<summary>
	//</summary>
	public class PictureBox : Control {

		Image image;
		PictureBoxSizeMode sizeMode;
		BorderStyle borderStyle;

		public PictureBox () 
		{
			image = null;
			sizeMode = PictureBoxSizeMode.Normal;
			borderStyle = BorderStyle.None;
			SetStyle ( ControlStyles.UserPaint, true );
			SetStyle ( ControlStyles.Selectable, false );
		}

		public BorderStyle BorderStyle {
			get {   return borderStyle; }
			set {
				if ( !Enum.IsDefined ( typeof(BorderStyle), value ) )
					throw new InvalidEnumArgumentException( "BorderStyle",
						(int)value,
						typeof(BorderStyle));
				
				if ( borderStyle != value ) {
					borderStyle = value;
					RecreateHandle ( );
				}
			}
		}


		public Image Image {
			get { return image; }
			set {
				image = value;

				if ( sizeMode == PictureBoxSizeMode.AutoSize && image != null )
					SetBounds ( 0, 0, 0, 0, BoundsSpecified.None );

				Invalidate ( );
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public new ImeMode ImeMode {
			get { return base.ImeMode; }
			set { base.ImeMode = value;}
		}

		protected override CreateParams CreateParams {
			get {
				RegisterDefaultWindowClass ( );

				CreateParams createParams = base.CreateParams;

				createParams.ClassName = Win32.DEFAULT_WINDOW_CLASS;

				createParams.Style |= (int) (
					WindowStyles.WS_CHILD | 
					WindowStyles.WS_VISIBLE |
					WindowStyles.WS_CLIPCHILDREN |
					WindowStyles.WS_CLIPSIBLINGS );

				switch ( BorderStyle ) {
					case BorderStyle.Fixed3D:
						createParams.ExStyle |= (int)WindowExStyles.WS_EX_CLIENTEDGE;
						break;
					case BorderStyle.FixedSingle:
						createParams.Style |= (int) WindowStyles.WS_BORDER;
						break;
				};
				return createParams;
			}		
		}

		protected override ImeMode DefaultImeMode {
			get { return ImeMode.Disable; }
		}

		protected override Size DefaultSize {
			get { return new Size(100,50); }
		}

		public override string ToString()
		{
			return GetType( ).FullName.ToString ( ) + ", SizeMode: " + SizeMode.ToString ( );
		}

		public PictureBoxSizeMode SizeMode {
			get { return sizeMode; }
			set {	
				if ( !Enum.IsDefined ( typeof(PictureBoxSizeMode), value ) )
					throw new InvalidEnumArgumentException( "SizeMode",
						(int)value,
						typeof( PictureBoxSizeMode ) );

				if ( sizeMode != value ) {
					sizeMode = value;

					if ( sizeMode == PictureBoxSizeMode.AutoSize )
						SetBounds ( 0, 0, 0, 0, BoundsSpecified.None );
					
					SetStyle ( ControlStyles.AllPaintingInWmPaint, sizeMode == PictureBoxSizeMode.StretchImage );

					Invalidate ( );
					OnSizeModeChanged ( EventArgs.Empty );
				}
			}
		}
		
		[MonoTODO]
		protected override void OnEnabledChanged(EventArgs e) 
		{
			//FIXME:
			base.OnEnabledChanged(e);
		}

		protected override void OnPaint(PaintEventArgs pevent) 
		{
			if ( Image != null ) {
				switch ( SizeMode ) {
				case PictureBoxSizeMode.StretchImage:
					pevent.Graphics.DrawImage ( Image, ClientRectangle );
				break;
				case PictureBoxSizeMode.CenterImage:
					int dx = (ClientRectangle.Width - Image.Width)/2;
					int dy = (ClientRectangle.Height- Image.Height)/2;
					pevent.Graphics.DrawImage ( Image, dx, dy );
				break;
				default:
					pevent.Graphics.DrawImage ( Image, 0, 0 );
				break;
				}
			}
			base.OnPaint(pevent);
		}

		protected override void OnParentChanged(EventArgs e) 
		{
			if ( Parent != null ) {
				BackColor = Parent.BackColor;
				Invalidate ( );
			}
				
			base.OnParentChanged(e);
		}

		protected override void OnResize(EventArgs e) 
		{
			if ( SizeMode == PictureBoxSizeMode.CenterImage )
				Invalidate ( );
			else if ( SizeMode == PictureBoxSizeMode.StretchImage && IsHandleCreated)
				Win32.InvalidateRect ( Handle, IntPtr.Zero, 0 );

			base.OnResize(e);
		}

		protected virtual void OnSizeModeChanged(EventArgs e)
		{
			if ( SizeModeChanged != null )
				SizeModeChanged ( this, e );
		}

		[MonoTODO]
		protected override void OnVisibleChanged(EventArgs e) 
		{
			base.OnVisibleChanged ( e );
		}

		protected override void OnPaintBackground (PaintEventArgs e) {
			if ( SizeMode != PictureBoxSizeMode.StretchImage ) 
				base.OnPaintBackground ( e );
		}

		protected override void SetBoundsCore(int x,int y,int width,int height,BoundsSpecified specified) 
		{
			if ( SizeMode == PictureBoxSizeMode.AutoSize && Image != null ) {
				width = Image.Width;
				height= Image.Height;
				specified = BoundsSpecified.Size;
			}
				
			base.SetBoundsCore(x, y, width, height, specified);
		}

		public event EventHandler SizeModeChanged;
	}
}
