//
// System.Windows.Forms.TrackBar
//
// Author:
//   stubbed out by Jackson Harper (jackson@latitudegeo.com)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc
//
using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {

	// <summary>
	// </summary>

    public class TrackBar : Control, ISupportInitialize {

		//
		//  --- Public Constructors
		//
		[MonoTODO]
		public TrackBar()
		{
			
		}
		//
		// --- Public Properties
		//
		[MonoTODO]
		public bool AutoSize {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}

		[MonoTODO]
		public override Image BackgroundImage {
			get {
				//FIXME:
				return base.BackgroundImage;
			}
			set {
				//FIXME:
				base.BackgroundImage = value;
			}
		}
		[MonoTODO]
		public override Font Font {
			get {
				//FIXME:
				return base.Font;
			}
			set {
				//FIXME:
				base.Font = value;
			}
		}
		[MonoTODO]
		public override Color ForeColor {
			get {
				//FIXME:
				return base.ForeColor;
			}
			set {
				//FIXME:
				base.ForeColor = value;
			}
		}
		[MonoTODO]
		public int LargeChange {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public int Maximum {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public int Minimum {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public Orientation Orientation {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public int SmallChange {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public override string Text {
			get {
				//FIXME:
				return base.Text;
			}
			set {
				//FIXME:
				base.Text = value;
			}
		}
		[MonoTODO]
		public int TickFrequency {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public TickStyle TickStyle {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		[MonoTODO]
		public int Value {
			get {
				throw new NotImplementedException ();
			}
			set {
				//FIXME:
			}
		}
		
		// --- Public Methods
		
		[MonoTODO]
		public void SetRange(int minValue, int maxValue) 
		{
			//FIXME:
		}
		[MonoTODO]
		public override string ToString() 
		{
			//FIXME:
			return base.ToString();
		}
		
		// --- Public Events
		
		[MonoTODO]
		public event EventHandler Scroll;
		[MonoTODO]
		public event EventHandler ValueChanged;
        
        // --- Protected Properties
        //
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "TRACKBAR";
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
				//FIXME:
				return base.DefaultImeMode;
			}
		}
		[MonoTODO]
		protected override Size DefaultSize {
			get {
				//FIXME: replace with correct values
				return new System.Drawing.Size(300,20);
			}
		}
		//
		// --- Protected Methods
		//
		[MonoTODO]
		protected override void CreateHandle() 
		{
			//FIXME: just to get it to run
			base.CreateHandle();
		}
		[MonoTODO]
		protected override bool IsInputKey(Keys keyData) 
		{
			//FIXME:
			return IsInputKey(keyData);
		}
		[MonoTODO]
		protected override void OnBackColorChanged(EventArgs e) 
		{
			//FIXME:
			base.OnBackColorChanged(e);
		}
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			//FIXME:
			base.OnHandleCreated(e);
		}

		[MonoTODO]
		protected virtual void OnScroll(EventArgs e) 
		{
			//FIXME:
		}

		[MonoTODO]
		protected override void OnMouseWheel(MouseEventArgs e) { // .NET V1.1 Beta.
			//FIXME:
			base.OnMouseWheel(e);
		}

		[MonoTODO]
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified) 
		{
			//FIXME:
			base.SetBoundsCore(x, y, width, height, specified);
		}

		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			//FIXME:
			base.WndProc(ref m);
		}

		void ISupportInitialize.BeginInit(){
			//FIXME:
		}

		void ISupportInitialize.EndInit(){
			//FIXME:
		}
	}
}
