//
// System.Windows.Forms.ContextMenu
// 
// 	Author:
//		Alberto Fernandez	(infjaf00@yahoo.es)
//

using System;
using System.ComponentModel;
using System.Drawing;
	
namespace System.Windows.Forms{	
	
	public class ContextMenu : Menu{
	
		private RightToLeft rightToLeft = RightToLeft.Inherit;
		private Control sourceControl;
		
		public ContextMenu(): base (null){
		}
		public ContextMenu(MenuItem[] menuItems): base (menuItems){
		}
		[MonoTODO]
		public virtual RightToLeft RightToLeft {
			//TODO: should affect the gtk item?
			get{
				if(rightToLeft != RightToLeft.Inherit){
					return rightToLeft;
				}
				else if(sourceControl != null){
					return sourceControl.RightToLeft;
				}
				else{
					return RightToLeft.No;
				}
			}
			set{
				if (!Enum.IsDefined (typeof(RightToLeft), value)){
					throw new InvalidEnumArgumentException();
				}
				if(rightToLeft != value){
					rightToLeft = value;
				}
			}			
		}
		public virtual Control SourceControl{
			get{ return sourceControl;}
		}
		protected internal virtual void OnPopup (EventArgs e){
			if (Popup != null)
				Popup (this, e);
		}
		[MonoTODO]
		public void Show (Control control, Point pos){
			//TODO: test it.
			this.sourceControl = control;
			OnPopup (EventArgs.Empty);
			((Gtk.Menu)Widget).Popup (null, null, null, IntPtr.Zero, 3, Gtk.Global.CurrentEventTime);
			
		}
		
		public event EventHandler Popup;		

		internal override Gtk.Widget CreateWidget(){
			return new Gtk.Menu();			
		}
		internal override void OnNewMenuItemAdd (MenuItem item){
			((Gtk.Menu)Widget).Append(item.Widget);
		}
		internal override void OnNewMenuItemAdd (int index, MenuItem item){
			((Gtk.Menu)Widget).Insert(item.Widget, index);
		}
		internal override void OnRemoveMenuItem (MenuItem item){
			((Gtk.Menu)Widget).Remove (item.Widget);
		}
		
		
	}
}
