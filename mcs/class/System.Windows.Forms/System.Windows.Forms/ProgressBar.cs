//
// System.Windows.Forms.ProgressBar
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
	/// Represents a Windows progress bar control.
	///
	/// </summary>

	[MonoTODO]
	public sealed class ProgressBar : Control {

		#region Fields
		int maximum;
		int minimum;
		int step;
		int value;
		#endregion
		
		#region Constructor
		[MonoTODO]
		public ProgressBar() 
		{
			maximum=100;
			minimum=0;
			step=10;
			value=0;
		}
		#endregion
		
		#region Properties
		[MonoTODO]
		public override bool AllowDrop {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public override Color BackColor {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		[MonoTODO]
		public override Image BackgroundImage {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException (); 
			}
		}

		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// public new bool CausesValidation {get; set;}
		
		[MonoTODO]
		protected override CreateParams CreateParams {
			get {
				CreateParams createParams = new CreateParams ();
				window = new ControlNativeWindow (this);

				createParams.Caption = Text;
				createParams.ClassName = "PROGRESSBAR";
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
		public override Color ForeColor  {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException (); 
			}
		}
		
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// public new ImeMode ImeMode {get; set;}
		
		public int Maximum {
			get {
				return maximum;
			}
			set {
				maximum=value;
			}
		}
		
		public int Minimum {
			get {
				return minimum;
			}
			set {
				minimum=value;
			}
		}
		
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// public new bool TabStop {get; set;}
		[MonoTODO]
		public override RightToLeft RightToLeft {
			get { 
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
		
		public int Step {
			get { 
				return step;
			}
			set {
				step=value;
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
		
		public int Value {
			get {
				return value;
			}
			set {
				value=value; 
			}
		}
		#endregion
		
		#region Methods
		[MonoTODO]
		protected override void CreateHandle() 	{
			base.CreateHandle();
		}
		
		[MonoTODO]
		public void Increment(int value) 	{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public void PerformStep() 
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public override string ToString() 
		{
			throw new NotImplementedException ();
		}
		#endregion
		
		#region Events
		/*
		 * This member supports the .NET Framework infrastructure and is not intended to be used directly from your code:
		 public new event EventHandler DoubleClick;
		 public new event EventHandler Enter;
		 public new event KeyEventHandler KeyDown;
		 public new event KeyPressEventHandler KeyPress;
		 public new event KeyEventHandler KeyUp;
		 public new event EventHandler Leave;
		 public new event PaintEventHandler Paint;
		*/
		#endregion
	}
}
