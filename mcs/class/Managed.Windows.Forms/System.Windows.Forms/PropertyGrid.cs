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
		private GridItemCollection grid_items;
		private object[] selected_objects;
		private PropertyTab selected_tab;
		private Color view_back_color;
		private Color view_fore_color;
		private Color help_back_color;
		private Color help_fore_color;
		private bool help_visible;

		private ImageList toolbar_imagelist;
		private ToolBarButton categorized_toolbarbutton;
		private ToolBarButton alphabetic_toolbarbutton;
		private ToolBarButton separator_toolbarbutton;
		private ToolBarButton propertypages_toolbarbutton;

		private ToolBar toolbar;
		private Label help_title_label;
		private Label help_label;
		private ContextMenu context_menu;
		private MenuItem reset_menuitem;
		private MenuItem description_menuitem;
		private TextBox grid_textbox;
		private int dividerY;
		private int grid_divider_X;
		internal bool			redraw;


		#endregion	// Private Members

		#region Contructors
		public PropertyGrid()
		{
			browsable_attributes = new AttributeCollection(new Attribute[] {});

			dividerY = 227;
			help_label = new Label();
			help_title_label = new Label();
			toolbar = new ToolBar();
			categorized_toolbarbutton = new ToolBarButton();
			alphabetic_toolbarbutton = new ToolBarButton();
			separator_toolbarbutton = new ToolBarButton();
			propertypages_toolbarbutton = new ToolBarButton();
			context_menu = new ContextMenu();
			grid_textbox = new TextBox();

			grid_textbox.Visible = false;
			grid_textbox.Font = new Font(this.Font,FontStyle.Bold);
			grid_textbox.BorderStyle = BorderStyle.None;
			grid_textbox.BackColor = ThemeEngine.Current.ColorWindow;
			grid_textbox.Validated += new EventHandler(grid_textbox_Validated);

			view_back_color = ThemeEngine.Current.ColorWindow;
			view_fore_color = ThemeEngine.Current.ColorWindowText;

			help_visible = true;
			help_back_color = ThemeEngine.Current.ColorButtonFace;
			help_fore_color = ThemeEngine.Current.ColorButtonText;

			//help_label.Dock = DockStyle.Fill;
			help_label.Name = "help_label";
			help_label.Location = new Point(2,2);
			help_label.Size = new Size(20,20);


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

			this.Controls.Add(toolbar);
			this.Controls.Add(help_title_label);
			this.Controls.Add(help_label);
			this.Controls.Add(grid_textbox);
			this.Name = "PropertyGrid";
			this.Size = new System.Drawing.Size(256, 400);

			grid_divider_X = (this.Width - GRID_LEFT_COLUMN_WIDTH)/2;

			this.MouseDown +=new MouseEventHandler(PropertyGrid_MouseDown);
			this.KeyDown +=new KeyEventHandler(PropertyGrid_KeyDown);

			selected_objects = new object[1];
			grid_items = new GridItemCollection();

			has_focus = false;

			//TextChanged+=new System.EventHandler(RedrawEvent);
			ForeColorChanged+=new EventHandler(RedrawEvent);
			BackColorChanged+=new System.EventHandler(RedrawEvent);
			FontChanged+=new EventHandler(RedrawEvent);
			SizeChanged+=new EventHandler(RedrawEvent);

			
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			SetStyle(ControlStyles.ResizeRedraw, true);
			SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);
		}
		#endregion	// Constructors

		#region Public Instance Properties

		[BrowsableAttribute(false)]
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
		public virtual bool CanShowCommands 
		{
			get {
				return can_show_commands;
			}
		}

		[CategoryAttribute("Appearance")]
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

		[CategoryAttribute("Appearance")]
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

		[CategoryAttribute("Appearance")]
		public virtual bool CommandsVisible 
		{
			get {
				return commands_visible;
			}
		}

		[CategoryAttribute("Appearance")]
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
		public Point ContextMenuDefaultLocation
		{
			get {
				return context_menu_default_location;
			}
		}

		[CategoryAttribute("Appearance")]
		public Color HelpBackColor
		{
			get
			{
				return help_back_color;
			}

			set
			{
				if (help_back_color == value) {
					return;
				}

				help_back_color = value;
				Redraw();
			}
		}

		[CategoryAttribute("Appearance")]
		public Color HelpForeColor
		{
			get {
				return help_fore_color;
			}

			set {
				if (help_fore_color == value) {
					return;
				}

				help_fore_color = value;
				Redraw();
			}
		}

		[CategoryAttribute("Appearance")]
		public virtual bool HelpVisible
		{
			get {
				return help_visible;
			}

			set {
				if (help_visible == value) {
					return;
				}

				help_visible = value;
				Redraw();
			}
		}

		[CategoryAttribute("Appearance")]
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

		[CategoryAttribute("Appearance")]
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

		[CategoryAttribute("Appearance")]
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
				Redraw();
			}
		}

		[BrowsableAttribute(false)]
		public PropertyTabCollection PropertyTabs
		{
			get {
				return property_tabs;
			}
		}

		[BrowsableAttribute(false)]
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
			}
		}

		[CategoryAttribute("Behavior")]
		public object SelectedObject
		{
			get {
				return selected_objects[0];
			}

			set {
				selected_objects = new object[] {value};
				ReflectObjects();
				Redraw();
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


		[CategoryAttribute("Appearance")]
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

		[CategoryAttribute("Appearance")]
		public Color ViewBackColor
		{
			get {
				return view_back_color;
			}

			set {
				if (view_back_color == value) {
					return;
				}

				view_back_color = value;
			}
		}

		[CategoryAttribute("Appearance")]
		public Color ViewForeColor
		{
			get {
				return view_fore_color;
			}

			set {
				if (view_fore_color == value) {
					return;
				}

				view_fore_color = value;
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
			Redraw();
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
			Redraw();
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
			this.grid_textbox.Visible = false;
			this.help_title_label.Visible = false;
			this.help_label.Visible = false;
			Draw(pevent);
			pevent.Graphics.DrawImage(this.ImageBuffer, pevent.ClipRectangle, pevent.ClipRectangle, GraphicsUnit.Pixel);
			this.grid_textbox.Visible = true;
			this.help_title_label.Visible = true;
			this.help_label.Visible = true;
			base.OnPaint (pevent);
		}

		// Derived classes should override Draw method and we dont want
		// to break the control signature, hence this approach.
		internal virtual void Draw (PaintEventArgs e) 
		{
			if (redraw) {
				Rectangle grid_rect = new Rectangle(0,toolbar.Height,this.Width-1,dividerY-DIVIDER_PADDING-toolbar.Height);
				Rectangle help_rect = new Rectangle(0,dividerY+DIVIDER_PADDING,this.Width-1,this.Height-1-dividerY-DIVIDER_PADDING);

				Rectangle grid_left_rect = new Rectangle(grid_rect.Left+1,grid_rect.Top+1,GRID_LEFT_COLUMN_WIDTH,GRID_ITEM_HEIGHT);
				Rectangle grid_label_rect = new Rectangle(grid_left_rect.Right,grid_rect.Top+1,grid_divider_X-GRID_LEFT_COLUMN_WIDTH,GRID_ITEM_HEIGHT);
				Rectangle grid_value_rect = new Rectangle(grid_label_rect.Right,grid_rect.Top+1,grid_rect.Right-grid_divider_X,GRID_ITEM_HEIGHT);

				// draw grid outline
				e.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(ViewBackColor),grid_rect);
				e.Graphics.DrawRectangle(ThemeEngine.Current.ResPool.GetPen(ThemeEngine.Current.ColorButtonShadow),grid_rect);

				// draw items
				Brush label_brush = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorWindowText);
				Brush control_brush = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorButtonFace);
				Pen control_pen = ThemeEngine.Current.ResPool.GetPen(ThemeEngine.Current.ColorButtonFace);
				for (int i = 0; i < grid_items.Count; i++)
				{
					GridItem grid_item = grid_items[i];
					
					label_brush = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorWindowText);
					if (grid_item == selected_grid_item)
					{
						e.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorHilight),grid_label_rect);
						label_brush = ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorHilightText);
						grid_textbox.Size = new Size(grid_value_rect.Size.Width-6,grid_value_rect.Size.Height);
						grid_textbox.Location = new Point(grid_value_rect.Location.X+4,grid_value_rect.Location.Y+1);

						// PDB - added check to prevent crash with test app
						if (grid_item.Value != null) {
							grid_textbox.Text = grid_item.Value.ToString();
						} else {
							grid_textbox.Text = string.Empty;
						}
						grid_textbox.Visible = true;

						help_title_label.Text = grid_item.Label;
						help_label.Text = grid_item.PropertyDescriptor.Description;
					}

					e.Graphics.FillRectangle(control_brush,grid_left_rect);
					e.Graphics.DrawRectangle(control_pen,grid_label_rect);
					e.Graphics.DrawRectangle(control_pen,grid_value_rect);

					e.Graphics.DrawString(grid_item.Label,this.Font,label_brush,grid_label_rect.Left + 5,grid_label_rect.Top+1);
					
					// PDB - added check to prevent crash with test app
					if (grid_item.Value != null) {
						e.Graphics.DrawString(grid_item.Value.ToString(),new Font(this.Font,FontStyle.Bold),ThemeEngine.Current.ResPool.GetSolidBrush(ThemeEngine.Current.ColorWindowText),grid_value_rect.Left + 2,grid_value_rect.Top+1);
					}
					
					// shift down for next item
					grid_left_rect.Y = grid_label_rect.Y = grid_value_rect.Y = grid_left_rect.Y + GRID_ITEM_HEIGHT;
				}

				// draw help
				if (help_visible)
				{
					e.Graphics.DrawRectangle(ThemeEngine.Current.ResPool.GetPen(ThemeEngine.Current.ColorButtonShadow),help_rect);
					help_title_label.Location = new Point(help_rect.Left+1,help_rect.Top+1);
					help_title_label.Size = new Size(help_rect.Width-1,help_title_label.Height);
					help_label.Location = new Point(help_rect.Left+1,help_title_label.Bottom);
					help_label.Size = new Size(help_rect.Width-1,help_rect.Height-help_title_label.Height-1);
				}

				UpdateToolBarButtons();
				redraw = false;
			}
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
				get
				{
					throw new NotImplementedException();
				}
			}
		
			#region ICollection Members
			[MonoTODO]
			public bool IsSynchronized
			{
				get
				{
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
				get
				{
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
				get
				{
					// TODO:  Add PropertyTabCollection.Count getter implementation
					return 0;
				}
			}

			#endregion
		}
		#endregion	// PropertyTabCollection Class


		#region Private Helper Methods

		internal void Redraw() 
		{
			redraw = true;
			Refresh ();
		}

		private void RedrawEvent(object sender, System.EventArgs e) 
		{
			Redraw();
		}

		private void toolbar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
		{
			if (e.Button == alphabetic_toolbarbutton)
			{
				this.PropertySort = PropertySort.Alphabetical;
			}
			else if (e.Button == categorized_toolbarbutton)
			{
				this.PropertySort = PropertySort.Categorized;
			}
			Redraw();
		}

		internal void UpdateToolBarButtons()
		{
			if (PropertySort == PropertySort.Alphabetical)
			{
				categorized_toolbarbutton.Pushed = false;
				alphabetic_toolbarbutton.Pushed = true;
			}
			else if (PropertySort == PropertySort.Categorized)
			{
				categorized_toolbarbutton.Pushed = true;
				alphabetic_toolbarbutton.Pushed = false;
			}
			else
			{
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
			foreach (object obj in selected_objects)
			{
				if (obj != null)
				{
					PopulateGridItemCollection(obj,grid_items);
				}
			}
		}

		private void PopulateGridItemCollection(object obj, GridItemCollection grid_item_coll)
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
			for (int i = 0; i < properties.Count;i++)
			{
				bool not_browseable = properties[i].Attributes.Contains(new Attribute[] {new BrowsableAttribute(false)});
				if (!not_browseable)
				{
					GridEntry grid_entry = new GridEntry(obj, properties[i]);
					object test = grid_item_coll["Length"];
					grid_item_coll.Add(properties[i].Name,grid_entry);
					//PopulateGridItemCollection(grid_entry.Value,grid_entry.GridItems);
				}
			}
		}

		private void grid_textbox_Validated(object sender, EventArgs e)
		{
			selected_grid_item.PropertyDescriptor.SetValue(selected_objects[0],selected_grid_item.PropertyDescriptor.Converter.ConvertTo(((TextBox)sender).Text,selected_grid_item.PropertyDescriptor.PropertyType));
		}

		private void PropertyGrid_MouseDown(object sender, MouseEventArgs e)
		{

			if (e.Button == MouseButtons.Left)
			{
				int index = (e.Y-toolbar.Height)/GRID_ITEM_HEIGHT;
				if (index < grid_items.Count)
					selected_grid_item = grid_items[index];
				Redraw();
			}
		}

		private void PropertyGrid_KeyDown(object sender, KeyEventArgs e)
		{
			if (selected_grid_item != null)
			{
				grid_textbox.Focus();
			}
		}
		#endregion	// Private Helper Methods
	}
}
