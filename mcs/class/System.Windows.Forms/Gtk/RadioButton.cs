//              
//		System.Windows.Forms.Frame
//
//		Author: 
//			Joel Basson			(jstrike@mweb.co.za)
//			Alberto Fernandez	(infjaf00@yahoo.es)
//
//

using System;
using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms{


	/// <summary>
	/// Represents a Windows RadioButton control.
	///
	/// </summary>


	public class RadioButton : ButtonBase {

		private bool autocheck = true;
			
		[MonoTODO]
		public RadioButton (){
		}
		public Appearance Appearance{
			get{ 
				if ( (Widget as Gtk.RadioButton).DrawIndicator)
					return Appearance.Normal;
				return Appearance.Button;
			}
			set{
				if (!Enum.IsDefined (typeof(Appearance), value) )
					throw new InvalidEnumArgumentException();
				if (value != Appearance){
					if (value == Appearance.Normal)
						(Widget as Gtk.RadioButton).DrawIndicator = true;
					else
						(Widget as Gtk.RadioButton).DrawIndicator = false;
					OnAppearanceChanged (EventArgs.Empty);				
				}
			}
		}
		[MonoTODO]
		public bool AutoCheck {
			get { return autocheck;  }
			set { autocheck = value; }
		}
		[MonoTODO]
		public ContentAlignment CheckAlign {
			get { return ContentAlignment.MiddleLeft; }
			set {
				if (!Enum.IsDefined (typeof (ContentAlignment), value))
					throw new InvalidEnumArgumentException ("CheckAlign");
			}
		}
		public bool Checked {
			get { return (Widget as Gtk.RadioButton).Active; }
			set { (Widget as Gtk.RadioButton).Active = value; }
		}
		protected override Size DefaultSize {
			get { return new Size (104,24); }
		}		
		public override string Text{
			get{ return ((Gtk.RadioButton) Widget).Label; }
			set{ ((Gtk.RadioButton) Widget).Label = value; }
		}
		// Not in docs?	
		protected virtual void OnAppearanceChanged( EventArgs e){
			if (AppearanceChanged != null)
				AppearanceChanged (this, e);
		}
		protected virtual void OnCheckedChanged( EventArgs e){
			if (CheckedChanged != null)
				CheckedChanged (this, e);
		}		
		public void PerformClick(){
			(Widget as Gtk.RadioButton).Click();
		}
		public override string ToString(){
			return "System.Windows.Forms.RadioButton, Checked: " + Checked;
		}
		
		public event EventHandler AppearanceChanged;
		public event EventHandler CheckedChanged;
		public new event EventHandler DoubleClick;

		internal override Gtk.Widget CreateWidget (){
			Gtk.RadioButton rb = new Gtk.RadioButton ("");
			rb.Toggled += new EventHandler (GtkToggled);
			return rb;
		}
		private void GtkToggled (object sender, EventArgs args){
			OnCheckedChanged(args);
		}
	}
}
