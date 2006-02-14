// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Copyright (c) 2004-2005 Novell, Inc.
//
// Authors:
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//

// NOT COMPLETE

using System;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;
using System.Collections;
using System.ComponentModel.Design;
using System.Reflection;
using System.Windows.Forms.Design;
using System.Windows.Forms.PropertyGridInternal;

namespace System.Windows.Forms
{
	[Designer("System.Windows.Forms.Design.PropertyGridDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class PropertyGrid : System.Windows.Forms.ContainerControl, ComponentModel.Com2Interop.IComPropertyBrowser
	{
		#region Private Members
		
		
		private const int GRID_ITEM_HEIGHT = 16;
		private const int GRID_LEFT_COLUMN_WIDTH = 16;
		private const int DIVIDER_PADDING = 2;

		private AttributeCollection browsable_attributes = null;
		private bool can_show_commands = false;
		private Color commands_back_color;
		private Color commands_fore_color;
		private bool commands_visible;
		private bool commands_visible_if_available;
		private Point context_menu_default_location;
		private bool large_buttons;
		private Color line_color;
		private PropertySort property_sort;
		private PropertyTabCollection property_tabs;
		private GridItem selected_grid_item;
		internal GridItemCollection grid_items;
		private object[] selected_objects;
		private PropertyTab selected_tab;

		private ImageList toolbar_imagelist;
		private ToolBarButton categorized_toolbarbutton;
		private ToolBarButton alphabetic_toolbarbutton;
		private ToolBarButton separator_toolbarbutton;
		private ToolBarButton propertypages_toolbarbutton;

		internal ToolBar toolbar;
		internal PropertyGridView property_grid_view;
		internal Splitter splitter;
		internal Panel help_panel;
		internal Label help_title_label;
		internal Label help_description_label;
		private ContextMenu context_menu;
		private MenuItem reset_menuitem;
		private MenuItem description_menuitem;
		private object current_property_value;

		#endregion	// Private Members

		#region Contructors
		public PropertyGrid() {
			selected_objects = new object[1];
			grid_items = new GridItemCollection();
			property_tabs = new PropertyTabCollection();

			line_color = SystemColors.ScrollBar;
			line_color = SystemColors.ScrollBar;
			browsable_attributes = new AttributeCollection(new Attribute[] {});
			commands_visible_if_available = false;
			property_sort = PropertySort.Categorized;

			property_grid_view = new PropertyGridView(this);
			property_grid_view.Dock = DockStyle.Fill;

			splitter = new Splitter();
			splitter.Dock = DockStyle.Bottom;

			help_panel = new Panel();
			help_panel.Dock = DockStyle.Bottom;
			//help_panel.DockPadding.All = 3;
			help_panel.Height = 50;
			help_panel.BackColor = SystemColors.Control;


			help_title_label = new Label();
			help_title_label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			help_title_label.Name = "help_title_label";
			help_title_label.Font = new Font(this.Font,FontStyle.Bold);
			help_title_label.Text = "Title";
			help_title_label.Location = new Point(2,2);
			help_title_label.Height = 17;
			help_title_label.Width = help_panel.Width - 4;

			
			help_description_label = new Label();
			help_description_label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			help_description_label.Name = "help_description_label";
			help_description_label.Font = this.Font;
			help_description_label.Text = "The long important Description";
			help_description_label.Location = new Point(2,help_title_label.Top+help_title_label.Height);
			help_description_label.Width = help_panel.Width - 4;
			help_description_label.Height = 17;

			help_panel.Controls.Add(help_description_label);
			help_panel.Controls.Add(help_title_label);
			help_panel.Paint+=new PaintEventHandler(help_panel_Paint);

			toolbar = new ToolBar();
			toolbar.Dock = DockStyle.Top;
			categorized_toolbarbutton = new ToolBarButton();
			alphabetic_toolbarbutton = new ToolBarButton();
			separator_toolbarbutton = new ToolBarButton();
			propertypages_toolbarbutton = new ToolBarButton();
			context_menu = new ContextMenu();

			toolbar_imagelist = new ImageList();
			toolbar_imagelist.ColorDepth = ColorDepth.Depth32Bit;
			toolbar_imagelist.ImageSize = new System.Drawing.Size(16, 16);
			toolbar_imagelist.TransparentColor = System.Drawing.Color.Transparent;
			toolbar_imagelist.Images.Add( (Image)Locale.GetResource( "propertygrid_sort_category") );
			toolbar_imagelist.Images.Add( (Image)Locale.GetResource( "propertygrid_sort_alphabetical") );
			toolbar_imagelist.Images.Add( (Image)Locale.GetResource( "propertygrid_tab_properties") );

			toolbar.Appearance = ToolBarAppearance.Flat;
			toolbar.AutoSize = false;
			toolbar.Buttons.AddRange(new ToolBarButton[] {
									     categorized_toolbarbutton,
									     alphabetic_toolbarbutton,
									     separator_toolbarbutton,
									     propertypages_toolbarbutton});

			toolbar.ButtonSize = new System.Drawing.Size(20, 20);
			toolbar.ImageList = toolbar_imagelist;
			toolbar.Location = new System.Drawing.Point(0, 0);
			toolbar.Name = "toolbar";
			toolbar.ShowToolTips = true;
			toolbar.Size = new System.Drawing.Size(256, 27);
			toolbar.TabIndex = 0;
			toolbar.ButtonClick += new ToolBarButtonClickEventHandler(toolbar_ButtonClick);

			categorized_toolbarbutton.ImageIndex = 0;
			categorized_toolbarbutton.Style = ToolBarButtonStyle.ToggleButton;
			categorized_toolbarbutton.ToolTipText = (string)Locale.GetResource( "Categorized");

			alphabetic_toolbarbutton.ImageIndex = 1;
			alphabetic_toolbarbutton.Style = ToolBarButtonStyle.ToggleButton;
			alphabetic_toolbarbutton.ToolTipText = (string)Locale.GetResource( "Alphabetic");

			separator_toolbarbutton.Style = ToolBarButtonStyle.Separator;

			propertypages_toolbarbutton.Enabled = false;
			propertypages_toolbarbutton.ImageIndex = 2;
			propertypages_toolbarbutton.Style = ToolBarButtonStyle.ToggleButton;
			propertypages_toolbarbutton.ToolTipText = "Property Pages";

			
			reset_menuitem = context_menu.MenuItems.Add("Reset");
			reset_menuitem.Click +=new EventHandler(OnResetPropertyClick);
			context_menu.MenuItems.Add("-");
			description_menuitem = context_menu.MenuItems.Add("Description");
			description_menuitem.Click += new EventHandler(OnDescriptionClick);
			description_menuitem.Checked = this.HelpVisible;
			this.ContextMenu = context_menu;
			toolbar.ContextMenu = context_menu;

			this.Controls.Add(property_grid_view);
			this.Controls.Add(toolbar);
			this.Controls.Add(splitter);
			this.Controls.Add(help_panel);
			this.Name = "PropertyGrid";
			this.Size = new System.Drawing.Size(256, 400);

			has_focus = false;

			//TextChanged+=new System.EventHandler(RedrawEvent);
			//ForeColorChanged+=new EventHandler(RedrawEvent);
			//BackColorChanged+=new System.EventHandler(RedrawEvent);
			//FontChanged+=new EventHandler(RedrawEvent);
			//SizeChanged+=new EventHandler(RedrawEvent);

			UpdateToolBarButtons();
		}
		#endregion	// Constructors

		#region Public Instance Properties

		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public AttributeCollection BrowsableAttributes {
			get {
				return browsable_attributes;
			}

			set {
				if (browsable_attributes == value) {
					return;
				}

				browsable_attributes = value;
			}
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override bool AutoScroll {
			get {
				return base.AutoScroll;
			}
			set {
				base.AutoScroll = value;
			}
		}

		public override Color BackColor {
			get {
				return base.BackColor;
			}

			set {
				if (base.BackColor == value) {
					return;
				}
				base.BackColor = value;
			}
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Image BackgroundImage {
			get {
				return base.BackgroundImage;
			}		
			set {
				base.BackgroundImage = value;
			}
		}


		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		public virtual bool CanShowCommands {
			get {
				return can_show_commands;
			}
		}

		public Color CommandsBackColor {
			get {
				return commands_back_color;
			}

			set {
				if (commands_back_color == value) {
					return;
				}
				commands_back_color = value;
			}
		}

		public Color CommandsForeColor {
			get {
				return commands_fore_color;
			}

			set {
				if (commands_fore_color == value) {
					return;
				}
				commands_fore_color = value;
			}
		}

		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		public virtual bool CommandsVisible {
			get {
				return commands_visible;
			}
		}

		[DefaultValue(false)]
		public virtual bool CommandsVisibleIfAvailable {
			get {
				return commands_visible_if_available;
			}

			set {
				if (commands_visible_if_available == value) {
					return;
				}
				commands_visible_if_available = value;
			}
		}

		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public Point ContextMenuDefaultLocation {
			get {
				return context_menu_default_location;
			}
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Control.ControlCollection Controls {
			get {
				return base.Controls;
			}
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override Color ForeColor 
		{
			get {
				return base.ForeColor;
			}
			set {
				base.ForeColor = value;
			}
		}

		public Color HelpBackColor {
			get
			{
				return help_panel.BackColor;
			}
			set
			{
				if (help_panel.BackColor == value) {
					return;
				}

				help_panel.BackColor = value;
			}
		}

		public Color HelpForeColor {
			get {
				return help_panel.ForeColor;
			}

			set {
				if (help_panel.ForeColor == value) {
					return;
				}

				help_panel.ForeColor = value;
			}
		}

		[DefaultValue(true)]
		[Localizable(true)]
		public virtual bool HelpVisible {
			get {
				return help_panel.Visible;
			}

			set {
				if (help_panel.Visible == value) {
					return;
				}

				help_panel.Visible = value;
			}
		}

		public bool LargeButtons {
			get {
				return large_buttons;
			}

			set {
				if (large_buttons == value) {
					return;
				}

				large_buttons = value;
			}
		}

		public Color LineColor {
			get {
				return line_color;
			}

			set {
				if (line_color == value) {
					return;
				}

				line_color = value;
			}
		}

		[DefaultValue(PropertySort.CategorizedAlphabetical)]
		public PropertySort PropertySort {
			get {
				return property_sort;
			}

			set {
				if (!Enum.IsDefined (typeof (PropertySort), value)) {
					throw new InvalidEnumArgumentException (string.Format("Enum argument value '{0}' is not valid for PropertySort", value));
				}

				if (property_sort == value) {
					return;
				}

				property_sort = value;
				
				ReflectObjects();
				Console.WriteLine("PropertySort");
				property_grid_view.Refresh();
				
				if (PropertySortChanged != null) {
					PropertySortChanged(this, EventArgs.Empty);
				}
			}
		}

		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public PropertyTabCollection PropertyTabs {
			get {
				return property_tabs;
			}
		}

		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public GridItem SelectedGridItem {
			get {
				return selected_grid_item;
			}

			set {
				if (selected_grid_item == value) {
					return;
				}

				GridItem oldItem = selected_grid_item;
				selected_grid_item = value;
				this.help_title_label.Text = selected_grid_item.Label;
				if (selected_grid_item.PropertyDescriptor != null)
					this.help_description_label.Text = selected_grid_item.PropertyDescriptor.Description;
					
				Console.WriteLine("SelectedGridItem");
				current_property_value = value.Value;
				if (oldItem != null && oldItem.PropertyDescriptor != null)
					oldItem.PropertyDescriptor.RemoveValueChanged(SelectedObject, new EventHandler(HandlePropertyValueChanged));
				if (selected_grid_item.PropertyDescriptor != null)
					selected_grid_item.PropertyDescriptor.AddValueChanged(SelectedObject, new EventHandler(HandlePropertyValueChanged));
				OnSelectedGridItemChanged(new SelectedGridItemChangedEventArgs( oldItem, selected_grid_item));
				
			}
		}

		private void HandlePropertyValueChanged(object sender, EventArgs e)
		{
			OnPropertyValueChanged(new PropertyValueChangedEventArgs( selected_grid_item, current_property_value));
		}

		[DefaultValue(null)]
		[TypeConverter("System.Windows.Forms.PropertyGrid+SelectedObjectConverter, " + Consts.AssemblySystem_Windows_Forms)]
		public object SelectedObject {
			get {
				return selected_objects[0];
			}

			set {
				selected_objects = new object[] {value};
				if (this.SelectedObject == null)
					return;
				PropertyTabAttribute[] propTabs = (PropertyTabAttribute[])this.SelectedObject.GetType().GetCustomAttributes(typeof(PropertyTabAttribute),true);
				if (propTabs.Length > 0)
				{
					foreach (Type tabType in propTabs[0].TabClasses)
					{
						this.PropertyTabs.AddTabType(tabType);
					}
				}
				RefreshTabs(PropertyTabScope.Component);
				Console.WriteLine("SelectedObject");
				ReflectObjects();
				property_grid_view.Refresh();
			}
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object[] SelectedObjects {
			get {
				return selected_objects;
			}

			set {
				selected_objects = value;
				ReflectObjects();
			}
		}

		[BrowsableAttribute(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PropertyTab SelectedTab {
			get {
				return selected_tab;
			}
		}

		public override ISite Site {
			get {
				return base.Site;
			}

			set {
				base.Site = value;
			}
		}


		[DefaultValue(true)]
		public virtual bool ToolbarVisible {
			get {
				return toolbar.Visible;
			}

			set {
				if (toolbar.Visible == value) {
					return;
				}

				toolbar.Visible = value;
			}
		}

		public Color ViewBackColor {
			get {
				return property_grid_view.BackColor;
			}

			set {
				if (property_grid_view.BackColor == value) {
					return;
				}

				property_grid_view.BackColor = value;
			}
		}

		public Color ViewForeColor {
			get {
				return property_grid_view.ForeColor;
			}

			set {
				if (property_grid_view.ForeColor == value) {
					return;
				}

				property_grid_view.ForeColor = value;
			}
		}

		#endregion	// Public Instance Properties

		#region Protected Instance Properties

		protected override Size DefaultSize {
			get {
				return base.DefaultSize;
			}
		}


		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		protected virtual Type DefaultTabType {
			get {
				return typeof(PropertiesTab);
			}
		}
		
		protected bool DrawFlatToolbar {
			get {
				return (toolbar.Appearance == ToolBarAppearance.Flat);
			}			
			set {
				if (value) 
					toolbar.Appearance = ToolBarAppearance.Flat;
				else
					toolbar.Appearance = ToolBarAppearance.Normal;
			}
		}

		protected override bool ShowFocusCues {
			get {
				return base.ShowFocusCues;
			}
		}

		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		
		protected override void Dispose(bool val)
		{
			base.Dispose(val);
		}
		
		public void CollapseAllGridItems () 
		{
			foreach (GridItem item in this.grid_items)
			{
				item.Expanded = false;
			}
		}

		public void ExpandAllGridItems () 
		{
			foreach (GridItem item in this.grid_items)
			{
				item.Expanded = true;
			}
		}

		public override void Refresh () 
		{
			base.Refresh ();
		}

		public void RefreshTabs (PropertyTabScope tabScope) 
		{
			
			/*button = new ToolBarButton("C");
			button.ImageIndex = 0;
			this.toolbar.Buttons.Add(button);
			button = new ToolBarButton();
			button.ImageIndex = 0;
			button.Style = ToolBarButtonStyle.Separator;
			this.toolbar.Buttons.Add(button);
			foreach (PropertyTab tab in this.PropertyTabs)
			{

				int index = toolbar.ImageList.Images.Count;
				this.toolbar.ImageList.Images.Add(tab.Bitmap);
				button = new ToolBarButton();
				button.ImageIndex = index;
				this.toolbar.Buttons.Add(button);
			}*/
			
		}

		public void ResetSelectedProperty() 
		{
			if (selected_grid_item == null || selected_grid_item.PropertyDescriptor == null)
				return;
			
			selected_grid_item.PropertyDescriptor.ResetValue(SelectedObject);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods

		protected virtual PropertyTab CreatePropertyTab(Type tabType) 
		{
			return (PropertyTab)Activator.CreateInstance(tabType);
		}
		
		[MonoTODO]
		protected void OnComComponentNameChanged(ComponentRenameEventArgs e)
		{
			throw new NotImplementedException();
		}

		protected override void OnFontChanged(EventArgs e) 
		{
			base.OnFontChanged (e);
		}

		protected override void OnGotFocus(EventArgs e) 
		{
			has_focus=true;
			base.OnGotFocus(e);
		}

		protected override void OnHandleCreated (EventArgs e) 
		{
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed (EventArgs e) 
		{
			base.OnHandleDestroyed (e);
		}

		protected override void OnMouseDown (MouseEventArgs e) 
		{
			base.OnMouseDown (e);
		}

		protected override void OnMouseMove (MouseEventArgs e) 
		{
			base.OnMouseMove (e);
		}

		protected override void OnMouseUp (MouseEventArgs e) 
		{
			base.OnMouseUp (e);
		}
		
		[MonoTODO]
		protected void OnNotifyPropertyValueUIItemsChanged(object sender, EventArgs e)
		{
		}

		protected override void OnPaint (PaintEventArgs pevent) 
		{
			base.OnPaint (pevent);
		}

		[MonoTODO]
		protected virtual void OnPropertyTabChanged (PropertyTabChangedEventArgs e) 
		{
			throw new NotImplementedException();
		}

		protected virtual void OnPropertyValueChanged (PropertyValueChangedEventArgs e) 
		{
			if (PropertyValueChanged != null) 
			{
				PropertyValueChanged(this, e);
				current_property_value = selected_grid_item.Value;
			}
		}

		protected override void OnResize (EventArgs e) 
		{
			base.OnResize (e);
		}

		protected virtual void OnSelectedGridItemChanged (SelectedGridItemChangedEventArgs e) 
		{
			if (SelectedGridItemChanged != null) 
			{
				SelectedGridItemChanged(this, e);
			}
		}

		protected virtual void OnSelectedObjectsChanged (EventArgs e) 
		{
			if (SelectedObjectsChanged != null) 
			{
				SelectedObjectsChanged(this, e);
			}
		}

		protected override void OnSystemColorsChanged (EventArgs e) 
		{
			base.OnSystemColorsChanged (e);
		}

		protected override void OnVisibleChanged (EventArgs e) 
		{
			base.OnVisibleChanged (e);
		}

		protected override bool ProcessDialogKey (Keys keyData) 
		{
			return base.ProcessDialogKey (keyData);
		}

		protected override void ScaleCore (float dx, float dy) 
		{
			base.ScaleCore (dx, dy);
		}
		
		[MonoTODO]
		protected void ShowEventsButton(bool value)
		{
			throw new NotImplementedException();
		}

		protected override void WndProc (ref Message m) 
		{
			base.WndProc (ref m);
		}
		#endregion

		#region Events
		public event EventHandler PropertySortChanged;
		public event PropertyTabChangedEventHandler PropertyTabChanged;
		public event PropertyValueChangedEventHandler PropertyValueChanged;
		public event SelectedGridItemChangedEventHandler SelectedGridItemChanged;
		public event EventHandler SelectedObjectsChanged;
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged;

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged;
		#endregion

		#region Com2Interop.IComPropertyBrowser Interface
		[MonoTODO]
		bool ComponentModel.Com2Interop.IComPropertyBrowser.InPropertySet {
			get  {
				throw new NotImplementedException();
			}
		}

		[MonoTODO]
		void ComponentModel.Com2Interop.IComPropertyBrowser.DropDownDone() {
			throw new NotImplementedException();
		}

		[MonoTODO]
		bool ComponentModel.Com2Interop.IComPropertyBrowser.EnsurePendingChangesCommitted() {
			throw new NotImplementedException();
		}

		[MonoTODO]
		void ComponentModel.Com2Interop.IComPropertyBrowser.HandleF4() {
			throw new NotImplementedException();
		}

		[MonoTODO]
		void ComponentModel.Com2Interop.IComPropertyBrowser.LoadState(Microsoft.Win32.RegistryKey key) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		void ComponentModel.Com2Interop.IComPropertyBrowser.SaveState(Microsoft.Win32.RegistryKey key) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		private event ComponentRenameEventHandler com_component_name_changed;
		event ComponentRenameEventHandler ComponentModel.Com2Interop.IComPropertyBrowser.ComComponentNameChanged {
			add { com_component_name_changed += value; }
			remove { com_component_name_changed -= value; }
		}
		#endregion	// Com2Interop.IComPropertyBrowser Interface

		#region PropertyTabCollection Class
		public class PropertyTabCollection : ICollection, IEnumerable
		{
			System.Collections.ArrayList list;
			#region Private Constructors
			internal PropertyTabCollection() {
				list = new ArrayList();
			}

			#endregion	// Private Constructors

			public PropertyTab this[int index] {
				get {
					return (PropertyTab)list[index];
				}
			}
		
			#region ICollection Members
			bool ICollection.IsSynchronized
			{
				get {
					return list.IsSynchronized;
				}
			}

			void ICollection.CopyTo(Array array, int index)
			{
				list.CopyTo(array, index);
			}

			object ICollection.SyncRoot
			{
				get {
					return list.SyncRoot;
				}
			}

			#endregion

			#region IEnumerable Members
			public IEnumerator GetEnumerator() {
				return list.GetEnumerator();
			}

			#endregion
		
			#region ICollection Members
			public int Count {
				get {
					return list.Count;
				}
			}

			#endregion
			
			#region Public Instance Methods
			public void AddTabType(System.Type propertyTabType)
			{
				list.Add(Activator.CreateInstance(propertyTabType));
			}
			[MonoTODO]
			public void AddTabType(System.Type propertyTabType,
				System.ComponentModel.PropertyTabScope tabScope)
			{
				AddTabType(propertyTabType);
			}
			[MonoTODO]
			public void Clear(System.ComponentModel.PropertyTabScope tabScope)
			{
				throw new NotImplementedException();
			}
			[MonoTODO]
			public void RemoveTabType(System.Type propertyTabType)
			{
				throw new NotImplementedException();
			}
			#endregion
		}
		#endregion	// PropertyTabCollection Class

		#region Private Helper Methods

		private void toolbar_ButtonClick (object sender, ToolBarButtonClickEventArgs e) 
		{
			if (e.Button == alphabetic_toolbarbutton) {
				this.PropertySort = PropertySort.Alphabetical;
			}
			else if (e.Button == categorized_toolbarbutton) {
				this.PropertySort = PropertySort.Categorized;
			}
			UpdateToolBarButtons();
			ReflectObjects();
			Console.WriteLine("toolbar_ButtonClick");
			property_grid_view.Refresh();
		}

		internal void UpdateToolBarButtons () 
		{
			if (PropertySort == PropertySort.Alphabetical) {
				categorized_toolbarbutton.Pushed = false;
				alphabetic_toolbarbutton.Pushed = true;
			}
			else if (PropertySort == PropertySort.Categorized) {
				categorized_toolbarbutton.Pushed = true;
				alphabetic_toolbarbutton.Pushed = false;
			}
			else {
				categorized_toolbarbutton.Pushed = false;
				alphabetic_toolbarbutton.Pushed = false;
			}
		}

		private void OnResetPropertyClick (object sender, EventArgs e) 
		{
			ResetSelectedProperty();
		}

		private void OnDescriptionClick (object sender, EventArgs e) 
		{
			this.HelpVisible = !this.HelpVisible;
			description_menuitem.Checked = this.HelpVisible;

		}

		private void ReflectObjects () 
		{
			grid_items = new GridItemCollection();
			foreach (object obj in selected_objects) {
				if (obj != null) {
					PopulateGridItemCollection(obj,grid_items, true);
				}
			}
		}

		private void PopulateGridItemCollection (object obj, GridItemCollection grid_item_coll, bool recurse) 
		{
			//TypeConverter converter = TypeDescriptor.GetConverter(obj);
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
			foreach (PropertyDescriptor property in properties) {
				if (property.IsBrowsable) {
					GridEntry grid_entry = new GridEntry(obj, property);
					if (property_sort == PropertySort.Alphabetical || !recurse)
					{
						if (grid_item_coll[property.Name] == null)
							grid_item_coll.Add(property.Name,grid_entry);
					}
					else if (property_sort == PropertySort.Categorized || property_sort == PropertySort.CategorizedAlphabetical)
					{

						string category = property.Category;
						GridItem cat_item = grid_item_coll[category];
						if (cat_item == null) 
						{
							cat_item = new CategoryGridEntry(category);
							grid_item_coll.Add(category,cat_item);
						}
						cat_item.GridItems.Add(property.Name,grid_entry);
					}
					if (recurse)
					{
						object propObj = property.GetValue(obj);
						if (propObj != null)
							PopulateGridItemCollection(propObj,grid_entry.GridItems, false);
					}
				}
			}
		}

		#endregion	// Private Helper Methods

		private void help_panel_Paint(object sender, PaintEventArgs e)
		{
			e.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(help_panel.BackColor), help_panel.ClientRectangle );
			e.Graphics.DrawRectangle(SystemPens.ControlDark, 0,0,help_panel.Width-1,help_panel.Height-1 );
		}
#if NET_2_0

		public bool UseCompatibleTextRendering {
			get {
				return use_compatible_text_rendering;
			}

			set {
				use_compatible_text_rendering = value;
			}
		}
#endif
	}
}
