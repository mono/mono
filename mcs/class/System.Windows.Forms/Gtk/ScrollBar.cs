//
// System.Windows.Forms.Button.cs
//
// Author:
//   stubbed out by Jaak Simm (jaaksimm@firm.ee)
//	  implemented for Gtk+ by Philip Van Hoof (me@freax.org)
//
// (C) Ximian, Inc., 2002
//

using System.Drawing;

namespace System.Windows.Forms {

	/// <summary>
	/// </summary>

	public class ScrollBar : Control {
		internal Gtk.Adjustment adj;
		private double val;
		private double lower;
		private double upper;
		private double step_increment;
		private double page_increment;
		private double page_size;

		//
		//  --- Constructor
		//

		public ScrollBar() : base(){
			// Defaults
			this.upper = 100;
			this.lower = 0;
			this.val = 0;
			this.step_increment = 1;
			this.page_increment = 10;
			this.page_size = 10;
			this.adj = new Gtk.Adjustment(val, lower, upper, step_increment, page_increment, page_size);
			//spec says tabstop defaults to false.
			base.TabStop = false;
			ConnectToChanged();
		}

		//
		//  --- Public Properties
		//


		public int LargeChange {
			get {return (int)page_increment;}
			set {
				if (value < 0){
					String st = String.Format (
						"'{0}' is not a valid value for LargeChange."+
						"It should be >= 0", value
					);
					throw new ArgumentException (st);
				}
				page_increment = value;
				UpdateGtkScroll();
			}
		}

		public int Maximum {
			get {return (int) upper;}
			set {
				upper = value;
				if (value < lower)
					lower = value;
				if (value < this.Value)
					this.Value = value;
					
				UpdateGtkScroll();
			}
		}

		public int Minimum {
			get {return (int) lower;}
			set {
				lower = value;
				if (value > upper)
					upper = value;
				if (value > val)
					val = value;
				UpdateGtkScroll();
			}
		}
		public int SmallChange {
			get { return (int) step_increment;}
			set {
				if (value < 0){
					String st = String.Format (
						"'{0}' is not a valid value for SmallChange."+
						"It should be >= 0", value
					);
					throw new ArgumentException (st);
				}
				step_increment = value;
				UpdateGtkScroll();
			}
		}

		public int Value {
			get {return (int) val; }
				
			set {
				if ((value > upper) || (value < lower)){
					String st = String.Format (
					"'{0}' is not a valid value for Minimum."+
					" It should be betwen Minimum and Maximum", value);
					
					throw new ArgumentException (st);
				}
				val = value; 
				UpdateGtkScroll();
			}
		}

		//
		//  --- Public Events
		//

		public event ScrollEventHandler Scroll;
		public event EventHandler ValueChanged;

		protected virtual void OnValueChanged (EventArgs args){
			if (ValueChanged != null)
				ValueChanged (this, args);
		}
		protected virtual void OnScroll (ScrollEventArgs args){
			if (Scroll != null)
				Scroll (this, args);
		}
		[MonoTODO]
		protected override void OnEnabledChanged(EventArgs e){
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e){
			throw new NotImplementedException();
		}	
		
		// FIXME: 
		
		internal protected void changed_cb (object o, EventArgs args){
			val = this.adj.Value;
			OnValueChanged(args);
			OnScroll (new ScrollEventArgs(ScrollEventType.ThumbPosition ,(int) val));
		}
		
		internal protected void ConnectToChanged (){
			this.adj.ValueChanged += new EventHandler (changed_cb);
		}
		
		internal protected void UpdateGtkScroll(){
			this.adj.SetBounds (lower, upper, step_increment, page_increment, page_size);
			if ((int) val != this.adj.Value)
				this.adj.Value = (int) val;
		}
	 }
}
