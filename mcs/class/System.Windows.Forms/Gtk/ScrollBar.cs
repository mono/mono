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

	// <summary>
	// </summary>

	public class ScrollBar : Control {
		internal Gtk.Adjustment adj;
		private double value;
		private double lower;
		private double upper;
		private double step_increment;
		private double page_increment;
		private double page_size;

		//
		//  --- Constructor
		//

		public ScrollBar() : base()
		{
			// Defaults
			this.upper = 100;
			this.lower = 0;
			this.value = 0;
			this.step_increment = 1;
			this.page_increment = 100;
			this.page_size = 100;

			this.adj = new Gtk.Adjustment(value, lower, upper, step_increment, page_increment, page_size);

			//spec says tabstop defaults to false.
			base.TabStop = false;

			ConnectToChanged ();
		}

		//
		//  --- Public Properties
		//


		[MonoTODO]
		public int LargeChange {
			get {
				return (int)this.adj.PageIncrement;
			}
			set {
				page_increment = value;
				this.adj.SetBounds (lower, upper, step_increment, page_increment, page_size);
			}
		}

		 [MonoTODO]
		public int Maximum {
			get {
				return (int)this.adj.Upper;
			}
			set {
				upper = value;
				this.adj.SetBounds (lower, upper, step_increment, page_increment, page_size);
			}
		}

		 [MonoTODO]
		public int Minimum {
			get {
				return (int)this.adj.Lower;
			}
			set {
				lower = value;
				this.adj.SetBounds (lower, upper, step_increment, page_increment, page_size);
			}
		}

		 [MonoTODO]
		public int SmallChange {
			get {
				return (int) this.adj.StepIncrement;
			}
			set {
				step_increment = value;
				this.adj.SetBounds (lower, upper, step_increment, page_increment, page_size);
			}
		}

		 public override string Text {
			 //Can't imagen what a scroll bar would do with text, so just call base.
			 get {
				 return base.Text;
			 }
			 set {
				 base.Text = value;
			 }
		 }

		 [MonoTODO]
		public int Value {
			get {
				return (int)this.adj.Value;
			}
			set {
				this.adj.Value = value;
			}
		}

		//
		//  --- Public Methods
		//

		public override string ToString()
		{	
			 //replace with value, if implmeted as properity.
			return Value.ToString();
		}

		//
		//  --- Public Events
		//

		[MonoTODO]
		public event ScrollEventHandler Scroll;


		public event EventHandler ValueChanged;

		internal protected void changed_cb (object o, EventArgs args)
		{
			if (ValueChanged != null)
				ValueChanged (this, args);
		}
		
		internal protected void ConnectToChanged ()
		{
			this.adj.ValueChanged += new EventHandler (changed_cb);
		}

		[MonoTODO]
		protected override void OnEnabledChanged(EventArgs e)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void OnHandleCreated(EventArgs e)
		{
			throw new NotImplementedException();
		}
	 }
}
