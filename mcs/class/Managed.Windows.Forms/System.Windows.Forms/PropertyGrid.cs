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
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms.Design;
using System.Reflection;
using System.Windows.Forms.PropertyGridInternal;

namespace System.Windows.Forms
{	
	public class PropertyGrid : System.Windows.Forms.ContainerControl
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


		#endregion	// Private Members

		#region Contructors
		public PropertyGrid()
		{
			line_color = SystemColors.ScrollBar;
			browsable_attributes = new AttributeCollection(new Attribute[] {});
			commands_visible_if_available = false;
			property_sort = PropertySort.CategorizedAlphabetical;

			property_grid_view = new PropertyGridView(this);
			property_grid_view.Dock = DockStyle.Fill;

			splitter = new Splitter();
			splitter.Dock = DockStyle.Bottom;

			help_panel = new Panel();
			help_panel.Dock = DockStyle.Bottom;
			help_panel.Height = 50;

			help_description_label = new Label();
			help_description_label.Dock = DockStyle.Fill;
			help_description_label.Name = "help_description_label";
			help_description_label.Font = this.Font;

			help_title_label = new Label();
			help_title_label.Dock = DockStyle.Top;
			help_title_label.Name = "help_title_label";
			help_description_label.Font = new Font(this.Font,FontStyle.Bold);

			help_panel.Controls.Add(help_description_label);
			help_panel.Controls.Add(help_title_label);

			toolbar = new ToolBar();
			toolbar.Dock = DockStyle.Top;
			categorized_toolbarbutton = new ToolBarButton();
			alphabetic_toolbarbutton = new ToolBarButton();
			separator_toolbarbutton = new ToolBarButton();
			propertypages_toolbarbutton = new ToolBarButton();
			context_menu = new ContextMenu();

			//help_title_label.Dock = DockStyle.Top;
			help_title_label.Name = "help_title_label";
			help_title_label.Location = new Point(2,20);
			help_title_label.Size = new Size(20,20);

			toolbar_imagelist = new ImageList();
			toolbar_imagelist.ColorDepth = ColorDepth.Depth32Bit;
			toolbar_imagelist.ImageSize = new System.Drawing.Size(16, 16);
			toolbar_imagelist.TransparentColor = System.Drawing.Color.Transparent;

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

			categorized_toolbarbutton.Style = ToolBarButtonStyle.ToggleButton;
			categorized_toolbarbutton.ToolTipText = "Categorized";
			categorized_toolbarbutton.Text = "C";

			alphabetic_toolbarbutton.Style = ToolBarButtonStyle.ToggleButton;
			alphabetic_toolbarbutton.ToolTipText = "Alphabetic";
			alphabetic_toolbarbutton.Text = "A";

			separator_toolbarbutton.Style = ToolBarButtonStyle.Separator;

			propertypages_toolbarbutton.Enabled = false;
			propertypages_toolbarbutton.Style = ToolBarButtonStyle.ToggleButton;
			propertypages_toolbarbutton.ToolTipText = "Property Pages";
			propertypages_toolbarbutton.Text = "P";

			
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

			selected_objects = new object[1];
			grid_items = new GridItemCollection();

			has_focus = false;

			//TextChanged+=new System.EventHandler(RedrawEvent);
			//ForeColorChanged+=new EventHandler(RedrawEvent);
			//BackColorChanged+=new System.EventHandler(RedrawEvent);
			//FontChanged+=new EventHandler(RedrawEvent);
			//SizeChanged+=new EventHandler(RedrawEvent);

			
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);
		}
		#endregion	// Constructors

		#region Public Instance Properties

		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public AttributeCollection BrowsableAttributes
		{
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

		public override Color BackColor
		{
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


		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		public virtual bool CanShowCommands 
		{
			get {
				return can_show_commands;
			}
		}

		public Color CommandsBackColor 
		{
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

		public Color CommandsForeColor 
		{
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
		public virtual bool CommandsVisible 
		{
			get {
				return commands_visible;
			}
		}

		[DefaultValue(false)]
		public virtual bool CommandsVisibleIfAvailable 
		{
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
		public Point ContextMenuDefaultLocation
		{
			get {
				return context_menu_default_location;
			}
		}

		public Color HelpBackColor
		{
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

		public Color HelpForeColor
		{
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
		public virtual bool HelpVisible
		{
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

		public bool LargeButtons
		{
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

		public Color LineColor
		{
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
		public PropertySort PropertySort
		{
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
			}
		}

		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public PropertyTabCollection PropertyTabs
		{
			get {
				return property_tabs;
			}
		}

		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public GridItem SelectedGridItem
		{
			get {
				return selected_grid_item;
			}

			set {
				if (selected_grid_item == value) {
					return;
				}

				selected_grid_item = value;
				this.help_title_label.Text = selected_grid_item.PropertyDescriptor.Name;
				this.help_description_label.Text = selected_grid_item.PropertyDescriptor.Description;
				property_grid_view.Redraw();
		
			}
		}

		public object SelectedObject
		{
			get {
				return selected_objects[0];
			}

			set {
				selected_objects = new object[] {value};
				ReflectObjects();
				property_grid_view.Redraw();
			}
		}

		[BrowsableAttribute(false)]
		public object[] SelectedObjects
		{
			get {
				return selected_objects;
			}

			set {
				selected_objects = value;
				ReflectObjects();
			}
		}

		[BrowsableAttribute(false)]
		public PropertyTab SelectedTab
		{
			get {
				return selected_tab;
			}
		}

		public override ISite Site
		{
			get {
				return base.Site;
			}

			set {
				base.Site = value;
			}
		}


		[DefaultValue(true)]
		public virtual bool ToolbarVisible 
		{
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

		public Color ViewBackColor
		{
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

		public Color ViewForeColor
		{
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

		protected override Size DefaultSize
		{
			get {
				return base.DefaultSize;
			}
		}


		[MonoTODO]
		protected virtual Type DefaultTabType 
		{
			get {
				throw new NotImplementedException();
			}
		}

		protected override bool ShowFocusCues
		{
			get {
				return base.ShowFocusCues;
			}
		}

		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		[MonoTODO]
		public void CollapseAllGridItems()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void ExpandAllGridItems()
		{
			throw new NotImplementedException();
		}

		public override void Refresh()
		{
			base.Refresh ();
		}


		[MonoTODO]
		public void RefreshTabs(PropertyTabScope tabScope)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public void ResetSelectedProperty()
		{
			throw new NotImplementedException();
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		[MonoTODO]
		protected virtual PropertyTab CreatePropertyTab(Type tabType)
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

		protected override void OnHandleCreated(EventArgs e)
		{
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed(EventArgs e)
		{
			base.OnHandleDestroyed (e);
		}

		protected override void OnLostFocus(EventArgs e) 
		{
			has_focus=false;
			base.OnLostFocus(e);
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown (e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove (e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp (e);
		}

		protected override void OnPaint(PaintEventArgs pevent)
		{
			base.OnPaint (pevent);
		}

		[MonoTODO]
		protected virtual void OnPropertyTabChanged(PropertyTabChangedEventArgs e)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual void OnPropertyValueChanged(PropertyValueChangedEventArgs e)
		{
			throw new NotImplementedException();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize (e);
		}

		[MonoTODO]
		protected virtual void OnSelectedGridItemChanged(SelectedGridItemChangedEventArgs e)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual void OnSelectedObjectsChanged(EventArgs e)
		{
			throw new NotImplementedException();
		}

		protected override void OnSystemColorsChanged(EventArgs e)
		{
			base.OnSystemColorsChanged (e);
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged (e);
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			return base.ProcessDialogKey (keyData);
		}

		protected override void ScaleCore(float dx, float dy)
		{
			base.ScaleCore (dx, dy);
		}

		protected override void WndProc(ref Message m)
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
		#endregion

		#region PropertyTabCollection Class
		public class PropertyTabCollection : ICollection, IEnumerable
		{
			[MonoTODO]
			public PropertyTab this[int index]
			{
				get {
					throw new NotImplementedException();
				}
			}
		
			#region ICollection Members
			[MonoTODO]
			public bool IsSynchronized
			{
				get {
					// TODO:  Add PropertyTabCollection.IsSynchronized getter implementation
					return false;
				}
			}

			[MonoTODO]
			public void CopyTo(Array array, int index)
			{
				// TODO:  Add PropertyTabCollection.CopyTo implementation
			}

			[MonoTODO]
			public object SyncRoot
			{
				get {
					// TODO:  Add PropertyTabCollection.SyncRoot getter implementation
					return null;
				}
			}

			#endregion

			#region IEnumerable Members
			[MonoTODO]
			public IEnumerator GetEnumerator()
			{
				// TODO:  Add PropertyTabCollection.GetEnumerator implementation
				return null;
			}

			#endregion
		
			#region ICollection Members
			[MonoTODO]
			public int Count
			{
				get {
					// TODO:  Add PropertyTabCollection.Count getter implementation
					return 0;
				}
			}

			#endregion
		}
		#endregion	// PropertyTabCollection Class


		#region Private Helper Methods

		private void toolbar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
		{
			if (e.Button == alphabetic_toolbarbutton) {
				this.PropertySort = PropertySort.Alphabetical;
			} else if (e.Button == categorized_toolbarbutton) {
				this.PropertySort = PropertySort.Categorized;
			}
		}

		internal void UpdateToolBarButtons()
		{
			if (PropertySort == PropertySort.Alphabetical) {
				categorized_toolbarbutton.Pushed = false;
				alphabetic_toolbarbutton.Pushed = true;
			} else if (PropertySort == PropertySort.Categorized) {
				categorized_toolbarbutton.Pushed = true;
				alphabetic_toolbarbutton.Pushed = false;
			} else {
				categorized_toolbarbutton.Pushed = false;
				alphabetic_toolbarbutton.Pushed = false;
			}
		}

		private void OnResetPropertyClick(object sender, EventArgs e)
		{
			ResetSelectedProperty();
		}

		private void OnDescriptionClick(object sender, EventArgs e)
		{
			this.HelpVisible = !this.HelpVisible;
			description_menuitem.Checked = this.HelpVisible;

		}

		private void ReflectObjects()
		{
			grid_items = new GridItemCollection();
			foreach (object obj in selected_objects) {
				if (obj != null) {
					PopulateGridItemCollection(obj,grid_items);
				}
			}
		}

		private void PopulateGridItemCollection(object obj, GridItemCollection grid_item_coll)
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
			for (int i = 0; i < properties.Count;i++) {
				bool not_browseable = properties[i].Attributes.Contains(new Attribute[] {new BrowsableAttribute(false)});
				if (!not_browseable) {
					GridEntry grid_entry = new GridEntry(obj, properties[i]);
					//object test = grid_item_coll["Length"];
					grid_item_coll.Add(properties[i].Name,grid_entry);
					//PopulateGridItemCollection(grid_entry.Value,grid_entry.GridItems);
				}
			}
		}
		#endregion	// Private Helper Methods
	}
}
