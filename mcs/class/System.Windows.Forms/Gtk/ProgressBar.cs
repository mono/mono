//
// System.Windows.Forms.ProgressBar
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//  Remco de Jong (rdj@rdj.cg.nu)
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
		private int maximum;
		private int minimum;
		private int step;
		private int val;
		#endregion

		#region Constructor
		[MonoTODO]
		public ProgressBar() 
		{
			maximum=100;
			minimum=0;
			step=10;
			val=0;
		}
		#endregion

		internal override Gtk.Widget CreateWidget () {
			Gtk.ProgressBar pbar = new Gtk.ProgressBar ();
			return pbar;
		}		
		
/*		#region Properties
		[MonoTODO]
		public override bool AllowDrop {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override Color BackColor {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		public override Color ForeColor  {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public override Image BackgroundImage {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
*/
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// public new bool CausesValidation {get; set;}
		
/*		[MonoTODO]
		protected override CreateParams CreateParams {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected override ImeMode DefaultImeMode {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected override Size DefaultSize {
			get { throw new NotImplementedException (); }		
		}
		
		[MonoTODO]
		public override Font Font {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
*/
		
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
/*		[MonoTODO]
		public override RightToLeft RightToLeft {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
*/		
		public int Step {
			get { 
				return step; 
			}
			set { 
				step=value; 
			}
		}
		
		protected override void  OnTextChanged(EventArgs e)
		{
			((Gtk.ProgressBar)Widget).Text = Text;
		}
		
		public int Value {
			get { 
				return val; 
			}
			set { 
				if (val <= maximum) {
					val=value; 
					float fraction = ((float) val / (float) maximum);
					((Gtk.ProgressBar)Widget).Fraction = fraction;
				}
			}
		}
//		#endregion
		
		
		
		
/*		#region Methods
		[MonoTODO]
		protected override void CreateHandle() 
		{
			throw new NotImplementedException ();
		}
*/		
		[MonoTODO]
		public void Increment(int value) 
		{
			Value += value;
		}
		
/*		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
*/		
		[MonoTODO]
		public void PerformStep() 
		{
			Value += step;
		}
		
		[MonoTODO]
		public override string ToString() 
		{
			throw new NotImplementedException ();
		}
//		#endregion
		
		
		
		
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
