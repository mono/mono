//		
//			System.Windows.Forms.CheckBox
//
//			Author: 
//					Joel Basson			(jstrike@mweb.co.za)
//					Alberto Fernandez	(infjaf00@yahoo.es)
//
//

using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	/// <summary>
	/// Represents a Windows CheckBox control.
	///
	/// </summary>

	public class CheckBox: ButtonBase{

		private bool autocheck = false;
		private bool threeState = false;
		private CheckState checkState;

		public CheckBox () : base (){
			AutoCheck = true;
			Checked = false;
			ThreeState = false;
		}
		public Appearance Appearance{
			get{ 
				if ( (Widget as Gtk.CheckButton).DrawIndicator)
					return Appearance.Normal;
				return Appearance.Button;
			}
			set{
				if (!Enum.IsDefined (typeof(Appearance), value) )
					throw new InvalidEnumArgumentException();
				if (value != Appearance){
					if (value == Appearance.Normal)
						(Widget as Gtk.CheckButton).DrawIndicator = true;
					else
						(Widget as Gtk.CheckButton).DrawIndicator = false;
					OnAppearanceChanged (EventArgs.Empty);				
				}
			}
		}
		public bool AutoCheck {
			get { return autocheck;  }
			set { autocheck = value; }
		}
		[MonoTODO]
		public ContentAlignment CheckAlign {
			get { return ContentAlignment.MiddleLeft; }
			set {
				if (!Enum.IsDefined (typeof (ContentAlignment), value)){
					throw new InvalidEnumArgumentException ("CheckAlign");
				}
			}
		}
		public bool Checked {
			get { return (this.CheckState != CheckState.Unchecked); }
			set { this.CheckState = (value) ? CheckState.Checked : CheckState.Unchecked;}
		}
		public CheckState CheckState{
			get{ return checkState; }
			set{
				if (! Enum.IsDefined ( typeof(CheckState), value) )
					throw new InvalidEnumArgumentException();
				checkState = value;
				switch (value){
					case CheckState.Indeterminate:
						(Widget as Gtk.CheckButton).Inconsistent = true;
						break;
					case CheckState.Checked:
						(Widget as Gtk.CheckButton).Inconsistent = false;
						(Widget as Gtk.CheckButton).Active = true;
						break;
					case CheckState.Unchecked:
						(Widget as Gtk.CheckButton).Inconsistent = false;
						(Widget as Gtk.CheckButton).Active = false;
						break;
				}
				
			}
		}
		protected override Size DefaultSize {
			get{ return new Size(104, 24);}
		}
		public bool ThreeState {
			get { return threeState; } 
			set { threeState = value; }
		}
		protected virtual void OnAppearanceChanged( EventArgs e){
			if (AppearanceChanged != null)
				AppearanceChanged (this, e);
		}
		protected virtual void OnCheckedChanged( EventArgs e){
			if (CheckedChanged != null)
				CheckedChanged (this, e);
		}
		protected virtual void OnCheckStateChanged(EventArgs e){
			if (CheckStateChanged != null)
				CheckStateChanged (this, e);
		}
		public override string ToString(){
			return "System.Windows.Forms.CheckBox, CheckState: " +
				(int) this.CheckState;
		}
		
		public event EventHandler AppearanceChanged;
		public event EventHandler CheckedChanged;
		public event EventHandler CheckStateChanged;
		public new event EventHandler DoubleClick;
		
		
		
		internal override Gtk.Widget CreateWidget () {
			Gtk.CheckButton cbox = new Gtk.CheckButton();
			cbox.Add (label.Widget);
			return cbox;	
		}
		internal override void ConnectEvents(){
			(Widget as Gtk.CheckButton).Toggled += new EventHandler (GtkToggled);
		}
		
		// Lock variable.
		private bool started = false;
		private void GtkToggled (object o, EventArgs e){
			if (started)
				return;
			started = true;
			if (AutoCheck){
				if (this.CheckState == CheckState.Indeterminate){
					this.CheckState = CheckState.Unchecked;
					OnCheckedChanged(e);
					OnCheckStateChanged(e);					
				}
				else if ((this.CheckState == CheckState.Checked) && (this.ThreeState)) {
					this.CheckState = CheckState.Indeterminate;
					OnCheckStateChanged (e);
				}
				else{
					this.Checked = !this.Checked;
					OnCheckedChanged(e);
					OnCheckStateChanged(e);
				}
					
			}
			else{
				this.CheckState = checkState;
			}
			started = false;
			
		}		
	}
}
