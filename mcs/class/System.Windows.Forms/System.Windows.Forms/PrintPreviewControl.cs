//
// System.Windows.Forms.PrintPreviewControl
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;
using System.Drawing.Printing;
using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// The raw "preview" part of print previewing, without any dialogs or buttons. Most PrintPreviewControl objects are found on PrintPreviewDialog objects, but they do not have to be.
	///
	/// </summary>

	[MonoTODO]
	public class PrintPreviewControl : Control {

		#region Fields
		bool autoZoom;
		int columns;
		PrintDocument document;
		int rows;
		int startPage;
		bool useAntiAlias;
		double zoom;
		#endregion
		
		#region Constructors
		[MonoTODO]
		public PrintPreviewControl() 
		{
			autoZoom=true;
			columns=1;
			document=null;
			rows=0;
			startPage=0;
			zoom=1.0;
		}
		#endregion
		
		#region Properties
		public bool AutoZoom {
			get { return autoZoom; }
			set { autoZoom=value; }
		}
		
		public int Columns {
			get { return columns; }
			set { columns=value; }
		}

		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "PRINTPREVIEWCONTROL";
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

		public PrintDocument Document {
			get { return document; }
			set { document=value; }
		}
		
		public int Rows {
			get { return rows; }
			set { rows=value; }
		}
		
		public int StartPage {
			get { return startPage; }
			set { startPage=value; }
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
		
		public bool UseAntiAlias {
			get { return useAntiAlias; }
			set { useAntiAlias=value; }
		}
		
		public double Zoom {
			get { return zoom; }
			set { zoom=value; }
		}
		#endregion
		
		#region Methods
		[MonoTODO]
		public void InvalidatePreview() 
		{
			//FIXME:
		}
		
		[MonoTODO]
		protected override void OnPaint(PaintEventArgs e) 
		{
			//FIXME:
			base.OnPaint(e);
		}
		
		[MonoTODO]
		protected override void OnResize(EventArgs e) 
		{
			//FIXME:
			base.OnResize(e);
		}
		
		[MonoTODO]
		protected virtual void OnStartPageChanged(EventArgs e) 
		{
			//FIXME:
		}
		
		[MonoTODO]
		public override void ResetBackColor() 
		{
			//FIXME:
			base.ResetBackColor();
		}
		
		[MonoTODO]
		public override void ResetForeColor() 
		{
			//FIXME:
			base.ResetForeColor();
		}
		
		[MonoTODO]
		protected override void WndProc(ref Message m) 
		{
			//FIXME:
			base.WndProc(ref m);
		}
		#endregion
		
		#region Events
		[MonoTODO]
		public event EventHandler StartPageChanged;
		#endregion
	}
}
