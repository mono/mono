//
// System.Windows.Forms.MenuItem
// 
// 	Author:
//		Alberto Fernandez	(infjaf00@yahoo.es)
//

using System;

namespace System.Windows.Forms{

	public class MenuItem : Menu{
	
		private Menu parent;
		private int index;		
		
		public MenuItem ():
			this (MenuMerge.Add, 0, Shortcut.None,null, null, null, null, null){
		}

		public MenuItem (String text):
			this (MenuMerge.Add, 0,Shortcut.None, text, null,null, null, null){
		}

		public MenuItem (String text, EventHandler onClick):
			this (MenuMerge.Add, 0, Shortcut.None,text, onClick,null, null, null){
		}

		public MenuItem (String text, MenuItem[]items):
			this (MenuMerge.Add, 0,	Shortcut.None, text,null, null, null,items){
		}

		public MenuItem (String text, EventHandler onClick,Shortcut shortcut):
			this (MenuMerge.Add, 0, shortcut, text, onClick, null, null, null){
		}

		public MenuItem (
			MenuMerge mergeType, 
			int mergeOrder, 
			Shortcut shortcut,
			String text,
			EventHandler onClick, 
			EventHandler onPopup,
			EventHandler onSelect,
			MenuItem[]items):
			base (items){
				this.MergeType = mergeType;
				this.MergeOrder = mergeOrder;
				this.Shortcut = shortcut;
				this.Text = text;
				if (onClick != null){
					Click += onClick;
				}
				if (onPopup != null){
					Popup += onPopup;
				}
				if (onSelect != null){
					Select += onSelect;
				}
		}
		[MonoTODO]
		public virtual bool BarBreak{
			get{return false;}
			set{}

		}

		[MonoTODO]
		public virtual bool Break{
			get { return false; }
			set {}

		}

		public virtual bool Checked{
			get{return ((GtkMyMenuItem)Widget).Checked;}
			set{
				if (!this.IsParent){
					((GtkMyMenuItem)Widget).Checked = value;
				}
			}

		}

		public virtual bool DefaultItem{
			get{ return ((GtkMyMenuItem)Widget).DefaultItem; }
			set{
				// Only one default item?
				if (value && (Parent != null)){
					foreach (MenuItem it in Parent.MenuItems){
						it.DefaultItem = false;
					}					
				}
				((GtkMyMenuItem)Widget).DefaultItem = value;
			}
		}
		public virtual bool Enabled{
			get{return ((GtkMyMenuItem)Widget).Enabled;}
			set{((GtkMyMenuItem)Widget).Enabled = value;}
		}
		public virtual int Index{
			get{return index;}
			set{index = value;}
		}		
		[MonoTODO]
		public bool MdiList{
			get{throw new NotImplementedException ();}
			set{}
		}
		[MonoTODO]
		public int MenuID{
			get{throw new NotImplementedException ();}
		}

		[MonoTODO]
		public int MergeOrder{
			get{throw new NotImplementedException ();}				
			set{}
		}
		[MonoTODO]
		public MenuMerge MergeType{
			get{throw new NotImplementedException ();}
			set{}
		}
		[MonoTODO]
		public virtual char Mnemonic{
			get{throw new NotImplementedException ();}
		}

		[MonoTODO]
		public virtual bool OwnerDraw{
			get{return false;}
			set{}
		}

		public virtual Menu Parent{
			get{return parent;}
		}
		public virtual bool RadioCheck{
			get { return ((GtkMyMenuItem)Widget).RadioCheck; }
			set { ((GtkMyMenuItem)Widget).RadioCheck = value; }
		}

		public virtual Shortcut Shortcut{
			get{return ((GtkMyMenuItem)Widget).Shortcut;}
			set{((GtkMyMenuItem)Widget).Shortcut = value;}
		}
		public virtual bool ShowShortcut{
			get{return ((GtkMyMenuItem)Widget).ShowShortcut;}
			set{((GtkMyMenuItem)Widget).ShowShortcut = value;}
		}
		public virtual String Text{
			get { return ((GtkMyMenuItem)Widget).Text; }
			set { ((GtkMyMenuItem)Widget).Text = value; }
		}
		public virtual bool Visible{
			get{return Widget.Visible;}
			set{Widget.Visible = value;}
		}

		public virtual MenuItem CloneMenu (){
			MenuItem it = new MenuItem ();
			it.CloneMenu(this);
			return (it);
		}
		
		[MonoTODO]
		protected virtual void CloneMenu (MenuItem itemSrc){

		}
		[MonoTODO]
		protected override void Dispose (bool disposing){
			base.Dispose (disposing);
		}
		
		[MonoTODO]
		public virtual MenuItem MergeMenu (){			
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public virtual void MergeMenu (MenuItem itemSrc){
		}

		protected virtual void OnClick (EventArgs e){
			if (Click != null){
				Click (this, e);
			}
		}
		protected virtual void OnDrawItem (DrawItemEventArgs e){
			if (DrawItem != null){
				DrawItem (this, e);
			}
		}

		// Don't use
		protected virtual void OnInitMenuPopup (EventArgs e){
		}
		protected virtual void OnMeasureItem (MeasureItemEventArgs e){
			if (MeasureItem != null){
				MeasureItem (this,e);
			}
		}
		protected virtual void OnPopup (EventArgs e){
			if (Popup != null){
				Popup(this, e);
			}
		}
		protected virtual void OnSelect (EventArgs e){
			if (Select != null){
				Select (this, e);
			}
		}
		public virtual void PerformClick (EventArgs e){
			OnClick (e);
		}
		public virtual void PerformSelect (){
			OnSelect (EventArgs.Empty);
		}
		[MonoTODO]
		public override String ToString (){
			return base.ToString();
		}
		public event EventHandler Click;

		public event DrawItemEventHandler DrawItem;

		public event MeasureItemEventHandler MeasureItem;

		public event EventHandler Popup;

		
		public event EventHandler Select;
		
		[MonoTODO]
		internal void SetParent (Menu parent){
			this.parent = parent;
		}
		
		internal Gtk.Menu gtkMenu;
		
		internal Gtk.Menu GtkMenu{
			get{
				if (gtkMenu == null){
					gtkMenu = new Gtk.Menu();
				}
				return gtkMenu;
			}
		}

		internal override Gtk.Widget CreateWidget(){
			widget = new GtkMyMenuItem();
			ConnectEvents();
			return widget;
		}
		[MonoTODO]
		internal void ConnectEvents(){
			GtkMyMenuItem w = (GtkMyMenuItem)Widget;
			w.GtkActivated += new EventHandler (OnGtkActivated);
			w.Selected += new EventHandler (OnGtkSelected);
			// TODO: Connect Events:
			// DrawItem
			// MeasureItem
			// Popup		
		}
		internal void OnGtkActivated (object sender, EventArgs args){
			OnClick(args);
		}
		internal void OnGtkSelected (object sender, EventArgs args){
			OnSelect (args);
		}
		internal override void OnNewMenuItemAdd (MenuItem item){				
			GtkMenu.Add (item.Widget);
			((Gtk.MenuItem)Widget).Submenu = GtkMenu;
		}
		internal override void OnNewMenuItemAdd (int index, MenuItem item){
			GtkMenu.Insert(item.Widget, index);
			((Gtk.MenuItem)Widget).Submenu = GtkMenu;
		}
		internal override void OnRemoveMenuItem (MenuItem item){
			GtkMenu.Remove (item.Widget);
		}
		internal override void OnLastSubItemRemoved (){
			((Gtk.MenuItem)Widget).Submenu = null;
		}		
	}
	
	
	
	
	
	
	
	
	
	//-----
	
	
	
	
	public class GtkMyMenuItem : Gtk.CheckMenuItem{
		
		public GtkMyMenuItem () : this (""){
		}
		public GtkMyMenuItem (string text): base (text){
			this.Text = text;
			Toggled += new EventHandler (this.OnToggled);
		}
		private bool checkd = false;
		private bool radioCheck = false;
		private string text = "";
		private bool enabled = true;
		private bool dfault = false;
		private bool _showShortcut = true;
		private Shortcut _shortcut;
		
		internal bool separator = false;
		
		
		[MonoTODO]
		public virtual bool ShowShortcut{
			get {return _showShortcut;}
			set {
				_showShortcut = value;
				OnShowShortcutChange (value);				
			}
		}
		[MonoTODO]
		public virtual Shortcut Shortcut{
			get { return _shortcut;}
			set { 
				_shortcut = value;
				OnShortcutChange (value);
			}
		}
			
		public bool Checked {
			get{ return checkd;}
			set{ 
				checkd = value;
				if (!separator){
					Active = value;
				}
			}
		}
		public bool DefaultItem{
			get{ return dfault; }
			set{ 
				dfault = value;
				OnDefaultChange (value);
			}
		}	
			
		public bool RadioCheck{
			get{ return radioCheck; }
			set{ radioCheck = value; }
		}
		
		public string Text{
			get{ return text; }
			set{ 
				if (value == null){
					value ="";
				}
				text = value;
				OnTextChange (value);
			}			
		}
		public bool Enabled{
			get{ return enabled; }
			set{ 
				if (!separator){
					this.Sensitive = value;
				}
				enabled = value;
			}
		}
		public event EventHandler GtkActivated;
		
		
		private void OnGtkActivated(EventArgs args){
			if (GtkActivated != null)
				GtkActivated (this, args);
		}
		// Lock variable
		private bool tog = false;
		private void OnToggled (object sender, EventArgs args){
			if (tog)
				return;
			tog = true;
			this.Active = Checked;
			OnGtkActivated (args);
			tog = false;
		}
		
		internal void OnShowShortcutChange (bool value){
			if (separator)
				return;
			
			// Hack to hide the shortcut.
			foreach (Gtk.AccelLabel w in Children){
				if (value){
					//Show: Assign this menuItem as AccelWidget.
					w.AccelWidget = this;
				}
				else{		
					// Hide: Assign the label itself as AccelWidget
					w.AccelWidget = w;
				}				
			}			
		}
		internal void OnShortcutChange (Shortcut value){
			if (separator)
				return;
			ShortcutHelper.AddShortcutToWidget(this, new Gtk.AccelGroup(), value, "activate");
			this.ShowShortcut = this.ShowShortcut;
		}
				
		internal void OnDefaultChange (bool value){
			if (separator)
				return;
			foreach (Gtk.AccelLabel w in Children){
				Pango.FontDescription fnt = new Pango.FontDescription();
				fnt.Weight = value ? Pango.Weight.Bold :  Pango.Weight.Normal;
				w.ModifyFont (fnt);				
			}
			
		}
		[MonoTODO]
		internal void OnTextChange (string t){
			if ((t== "-") && separator){
				return;
			}
			if (t == "-") {
				foreach (Gtk.Widget w in Children){
					this.Remove (w);
				}
				this.Add(new Gtk.HSeparator());
				this.Sensitive = false;
				this.Active = false;
				separator = true;
				return;
			}
			if (separator){
				foreach (Gtk.Widget w in Children){
					this.Remove (w);
				}
				this.Add (new Gtk.AccelLabel (""));
				this.Sensitive = Enabled;
				this.Active = checkd;
				separator = false;
			}
			foreach (Gtk.Widget w in Children){			
				((Gtk.AccelLabel)w).TextWithMnemonic = SWFGtkConv.AccelString(t);
			}			
		}			
	}
}
