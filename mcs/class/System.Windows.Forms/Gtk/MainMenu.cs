//
// System.Windows.Forms.MainMenu
// 
// 	Author:
//		Alberto Fernandez	(infjaf00@yahoo.es)
//

using System;
using System.ComponentModel;

namespace System.Windows.Forms{

	public class MainMenu:Menu{
		private RightToLeft rightToLeft = RightToLeft.Inherit;
		private Form ownerForm;

		public MainMenu ():base (null){ 
		}
		public MainMenu (MenuItem[]items):base (items){
		}
		
		[MonoTODO]
		public virtual RightToLeft RightToLeft {
			get{
				if(rightToLeft != RightToLeft.Inherit){
					return rightToLeft;
				}
				else if(ownerForm != null){
					return ownerForm.RightToLeft;
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
		[MonoTODO]
		public virtual MainMenu CloneMenu (){	
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected virtual IntPtr CreateMenuHandle (){
			return IntPtr.Zero;
		}
		[MonoTODO]
		protected override void Dispose (bool disposing){
			base.Dispose (disposing);			
		}
		public virtual Form GetForm (){
			return ownerForm;
		}
		[MonoTODO]
		public override String ToString (){
			if (ownerForm != null){
				return base.ToString () + ", GetForm: " +
					ownerForm.ToString ();
			}
			return base.ToString (); 
		}
		
		
		
		[MonoTODO]
		internal void AddToForm (Form form){
			ownerForm = form;
		}
		[MonoTODO]
		internal void RemoveFromForm (){
			ownerForm = null;
		}
		
		internal override Gtk.Widget CreateWidget(){
			Gtk.MenuBar mb = new Gtk.MenuBar();
			return mb;			
		}
		
		internal override void OnNewMenuItemAdd (MenuItem item){
			((Gtk.MenuBar)Widget).Append (item.Widget);
		}
		internal override void OnRemoveMenuItem (MenuItem item){
			((Gtk.MenuBar)Widget).Remove (item.Widget);
		}		
	}
}
