//
// System.Windows.Forms.ProgressBar
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//  Remco de Jong (rdj@rdj.cg.nu)
//  Alberto Fernandez (infjaf00@yahoo.es)
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
		
		public ProgressBar(){
			maximum=100;
			minimum=0;
			step=10;
			val=0;
			this.Size = this.DefaultSize;
		}
		#endregion
	
		
/*		#region Properties
		[MonoTODO]
		public override bool AllowDrop {
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
		public override Font Font {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
*/
		protected override Size DefaultSize {
			get { return new System.Drawing.Size (100, 23); }		
		}
		
		/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
		/// public new ImeMode ImeMode {get; set;}
		
		public int Maximum {
			get { return maximum;}
			set { 
				if (value < 0){
					String st = String.Format (
						"'{0}' is not a valid value for Maximum."+
						"It should be >= 0", value
					);
					throw new ArgumentException (st);
				}
				maximum=value; 
				if (value < minimum)
					minimum = value;
				if (value < val)
					val = value;
				UpdateGtkProgressBar();
			}
		}
		
		public int Minimum {
			get { return minimum; }
			set { 
				if (value < 0){
					String st = String.Format (
						"'{0}' is not a valid value for Minimum."+
						"It should be >= 0", value
					);
					throw new ArgumentException (st);
				}
				minimum=value; 
				if (value > maximum)
					maximum = value;
				if (value > val)
					val = value;
				UpdateGtkProgressBar();
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
			get { return step; }
			set { step=value; }
		}
		
		
		
		public int Value {
			get { 
				return val; 
			}
			set { 
				if ((value > maximum) || (value < minimum)){
					String st = String.Format (
					"'{0}' is not a valid value for Value."+
					" It should be betwen Minimum and Maximum", value);
					
					throw new ArgumentException (st);
				}

				val=value; 
				UpdateGtkProgressBar();
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
		public void Increment(int value) {
			int tmp = Value + value;
			tmp = (tmp < Minimum) ? Minimum : tmp;
			tmp = (tmp > Maximum) ? Maximum : tmp;
			Value = tmp;
		}
		
/*		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e) 
		{
			throw new NotImplementedException ();
		}
*/		
		public void PerformStep(){
			this.Increment (this.Step);			
		}
		
		public override string ToString(){
			String ret = String.Format (
				"System.Windows.Forms.ProgressBar, " +
				"Minimum: {0}, Maximum: {1}, Value: {2}",
				this.Minimum,
				this.Maximum,
				this.Value);
			return ret;
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
		internal override Gtk.Widget CreateWidget () {
			Gtk.ProgressBar pbar = new Gtk.ProgressBar ();
			return pbar;
		}	
		protected override void  OnTextChanged(EventArgs e){
			((Gtk.ProgressBar)Widget).Text = Text;
		}
		internal void UpdateGtkProgressBar(){
			float fraction = ((float) val / (float) maximum);
			(Widget as Gtk.ProgressBar).Fraction = fraction;
		}
	}
}
