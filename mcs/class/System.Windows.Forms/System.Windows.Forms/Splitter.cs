//
// System.Windows.Forms.Splitter.cs
//
// Author:
//   stubbed out by Daniel Carrera (dcarrera@math.toronto.edu)
//	Dennis Hayes (dennish@raytek.com)
//
// (C) 2002/3 Ximian, Inc
//
using System.Drawing;
using System.ComponentModel;
namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class Splitter : Control, IMessageFilter {
		BorderStyle borderStyle;
		int minSize;
		int minExtra;
		//
		//  --- Constructor
		//
		[MonoTODO]
		public Splitter()
		{
			SetStyle ( ControlStyles.Selectable, false );
			borderStyle = BorderStyle.None;
			Dock = DockStyle.Left;
			minSize = 25;
			minExtra = 25;

			Application.AddMessageFilter ( this );
		}

		~Splitter ( ) 
		{
			Application.RemoveMessageFilter ( this );
		}
		//
		//  --- Public Properties
		//
		[MonoTODO]
		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override  bool AllowDrop {
			get { return base.AllowDrop;  }
			set { base.AllowDrop = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override  AnchorStyles Anchor {
			get { return base.Anchor;  }
			set { base.Anchor = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override  Image BackgroundImage {
			get { return base.BackgroundImage;  }
			set { base.BackgroundImage = value; }
		}

		public BorderStyle BorderStyle {
			get {   return borderStyle; }
			set {
				if ( !Enum.IsDefined ( typeof(BorderStyle), value ) )
					throw new InvalidEnumArgumentException( "BorderStyle",
						(int)value,
						typeof(BorderStyle));
				
				if ( borderStyle != value ) {
					int oldStyle = getBorderStyle ( borderStyle );
					int oldExStyle = getBorderExStyle ( borderStyle );
					borderStyle = value;

					if ( IsHandleCreated ) {
						Win32.UpdateWindowStyle ( Handle, oldStyle, getBorderStyle ( borderStyle ) );
						Win32.UpdateWindowExStyle ( Handle, oldExStyle, getBorderExStyle ( borderStyle ) );
					}
				}
			}
		}

		public override DockStyle Dock {
			get { return base.Dock; }
			set {
				if ( value == DockStyle.None || value == DockStyle.Fill )
					throw new ArgumentException ( "A splitter control must be docked left, right, top or bottom.", "value" );

				base.Dock = value;

				Cursor = Vertical ? Cursors.VSplit : Cursors.HSplit;
			}
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Font Font {
			get { return base.Font;  }
			set { base.Font = value; }
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public override Color ForeColor {
			get { return base.ForeColor;  }
			set { base.ForeColor = value; }
		}
		[MonoTODO]
		public int MinExtra {
			get { return minExtra; }
			set {
				minExtra = value;
				if ( minExtra < 0 )
					minExtra = 0;
			}
		}
		[MonoTODO]
		public int MinSize {
			get { return minSize; }
			set {
				minSize = value;
				if ( minSize < 0 )
					minSize = 0;
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

		[EditorBrowsable (EditorBrowsableState.Never)]	 
		public override string Text {
			get { return base.Text;  }
			set { base.Text = value; }
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
				CreateParams createParams = base.CreateParams;

				createParams.ClassName = Win32.DEFAULT_WINDOW_CLASS;
				createParams.Style |= (int) WindowStyles.WS_CHILD;

				createParams.Style   |= getBorderStyle   ( BorderStyle );
				createParams.ExStyle |= getBorderExStyle ( BorderStyle );

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
				return new System.Drawing.Size(3, 3);
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
			Control ctrl = getDockedControl ( );
			if ( ctrl != null ) {
				switch ( Dock ) {
				case DockStyle.Left:
					x = ctrl.Right;
					height = ctrl.Height;
					specified |=  ( BoundsSpecified.X | BoundsSpecified.Height );
				break;
				case DockStyle.Right:
				break;
				case DockStyle.Top:
				break;
				case DockStyle.Bottom:
				break;
				}
			}
			base.SetBoundsCore ( x, y, width, height, specified );
		}
		bool IMessageFilter.PreFilterMessage(ref Message m){
			return false;
		}

		private int getBorderStyle ( BorderStyle style )
		{
			if ( style == BorderStyle.FixedSingle )
				return (int) WindowStyles.WS_BORDER;

			return 0;
		}

		private int getBorderExStyle ( BorderStyle style )
		{
			if ( style == BorderStyle.Fixed3D )
				return (int) (int)WindowExStyles.WS_EX_CLIENTEDGE;

			return 0;
		}

		Control getDockedControl ( ) {
			if ( Parent != null ) {
				int index = Parent.Controls.GetChildIndex ( this, false );
				if ( index != - 1 ) {
					for ( int i = index + 1; i < Parent.Controls.Count; i++ ) {
						Control ctrl = Parent.Controls [ i ];
						if ( ctrl.Dock == this.Dock )
							return ctrl;
					}
				}
				return Parent;
			}
			return null;
		}

		private bool Vertical {
			get { return ( Dock == DockStyle.Left || Dock == DockStyle.Right ); }
		}
	 }
}
