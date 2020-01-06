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
// Copyright (c) 2004-2008 Novell, Inc.
//
// Authors:
//	Jonathan Chambers (jonathan.chambers@ansys.com)
//      Ivan N. Zlatev	  (contact@i-nz.net)
//

// NOT COMPLETE

using System;
using System.IO;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;
using System.Collections;
using System.ComponentModel.Design;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;
using System.Windows.Forms.PropertyGridInternal;

namespace System.Windows.Forms 
{
	[Designer("System.Windows.Forms.Design.PropertyGridDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class PropertyGrid : System.Windows.Forms.ContainerControl, ComponentModel.Com2Interop.IComPropertyBrowser 
	{
		#region Private Members
		
		
		private const string UNCATEGORIZED_CATEGORY_LABEL = "Misc";
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
		private GridEntry selected_grid_item;
		private GridEntry root_grid_item;
		private object[] selected_objects;
		private PropertyTab properties_tab;
		private PropertyTab selected_tab;

		private ImageList toolbar_imagelist;
		private Image categorized_image;
		private Image alphabetical_image;
		private Image propertypages_image;
		private PropertyToolBarButton categorized_toolbarbutton;
		private PropertyToolBarButton alphabetic_toolbarbutton;
		private PropertyToolBarButton propertypages_toolbarbutton;
		private PropertyToolBarSeparator separator_toolbarbutton;
		private bool events_tab_visible;

		private PropertyToolBar toolbar;

		private PropertyGridView property_grid_view;
		private Splitter splitter;
		private Panel help_panel;
		private Label help_title_label;
		private Label help_description_label;
		private MenuItem reset_menuitem;
		private MenuItem description_menuitem;

		private Color category_fore_color;
		private Color commands_active_link_color;
		private Color commands_disabled_link_color;
		private Color commands_link_color;
		#endregion	// Private Members
		
		#region Contructors
		public PropertyGrid ()
		{
			selected_objects = new object[0];
			property_tabs = new PropertyTabCollection(this);

			line_color = SystemColors.ScrollBar;
			category_fore_color = line_color;
			commands_visible = false;
			commands_visible_if_available = false;
			property_sort = PropertySort.CategorizedAlphabetical;
			property_grid_view = new PropertyGridView(this);

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
			help_title_label.Location = new Point(2,2);
			help_title_label.Height = 17;
			help_title_label.Width = help_panel.Width - 4;

			
			help_description_label = new Label();
			help_description_label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
			help_description_label.AutoEllipsis = true;
			help_description_label.AutoSize = false;
			help_description_label.Font = this.Font;
			help_description_label.Location = new Point(2,help_title_label.Top+help_title_label.Height);
			help_description_label.Width = help_panel.Width - 4;
			help_description_label.Height = help_panel.Height - help_description_label.Top - 2;

			help_panel.Controls.Add(help_description_label);
			help_panel.Controls.Add(help_title_label);
			help_panel.Paint+=new PaintEventHandler(help_panel_Paint);

			toolbar = new PropertyToolBar();
			toolbar.Dock = DockStyle.Top;
			categorized_toolbarbutton = new PropertyToolBarButton ();
			categorized_toolbarbutton.Pushed = true;
			alphabetic_toolbarbutton = new PropertyToolBarButton ();
			propertypages_toolbarbutton = new PropertyToolBarButton ();
			separator_toolbarbutton = new PropertyToolBarSeparator ();
			ContextMenu context_menu = new ContextMenu();
			context_menu_default_location = Point.Empty;

			categorized_image = new Bitmap (typeof (PropertyGrid), "propertygrid-categorized.png");
			alphabetical_image = new Bitmap (typeof (PropertyGrid), "propertygrid-alphabetical.png");
			propertypages_image = new Bitmap (typeof (PropertyGrid), "propertygrid-propertypages.png");

			toolbar_imagelist = new ImageList();
			toolbar_imagelist.ColorDepth = ColorDepth.Depth32Bit;
			toolbar_imagelist.ImageSize = new System.Drawing.Size(16, 16);
			toolbar_imagelist.TransparentColor = System.Drawing.Color.Transparent;

			toolbar.Appearance = ToolBarAppearance.Flat;
			toolbar.AutoSize = false;
			
			toolbar.ImageList = toolbar_imagelist;
			toolbar.Location = new System.Drawing.Point(0, 0);
			toolbar.ShowToolTips = true;
			toolbar.Size = new System.Drawing.Size(256, 27);
			toolbar.TabIndex = 0;

			toolbar.Items.AddRange (new ToolStripItem [] {categorized_toolbarbutton,
								      alphabetic_toolbarbutton,
								      new PropertyToolBarSeparator (),
								      propertypages_toolbarbutton});
			//toolbar.ButtonSize = new System.Drawing.Size (20, 20);
			categorized_toolbarbutton.Click += new EventHandler (toolbarbutton_clicked);
			alphabetic_toolbarbutton.Click += new EventHandler (toolbarbutton_clicked);
			propertypages_toolbarbutton.Click += new EventHandler (toolbarbutton_clicked);

			categorized_toolbarbutton.Style = ToolBarButtonStyle.ToggleButton;
			categorized_toolbarbutton.ToolTipText = Locale.GetText ("Categorized");

			alphabetic_toolbarbutton.Style = ToolBarButtonStyle.ToggleButton;
			alphabetic_toolbarbutton.ToolTipText = Locale.GetText ("Alphabetic");

			propertypages_toolbarbutton.Enabled = false;
			propertypages_toolbarbutton.Style = ToolBarButtonStyle.ToggleButton;
			propertypages_toolbarbutton.ToolTipText = "Property Pages";

			properties_tab = CreatePropertyTab (this.DefaultTabType);
			selected_tab = properties_tab;
			RefreshToolbar (property_tabs);
			
			reset_menuitem = context_menu.MenuItems.Add("Reset");
			reset_menuitem.Click +=new EventHandler(OnResetPropertyClick);
			context_menu.MenuItems.Add("-");
			description_menuitem = context_menu.MenuItems.Add("Description");
			description_menuitem.Click += new EventHandler(OnDescriptionClick);
			description_menuitem.Checked = this.HelpVisible;
			this.ContextMenu = context_menu;
			toolbar.ContextMenu = context_menu;
			
			BorderHelperControl helper = new BorderHelperControl ();
			helper.Dock = DockStyle.Fill;
			helper.Controls.Add (property_grid_view);
			
			this.Controls.Add(helper);
			this.Controls.Add(toolbar);
			this.Controls.Add(splitter);
			this.Controls.Add(help_panel);
			this.Name = "PropertyGrid";
			this.Size = new System.Drawing.Size(256, 400);
		}
		#endregion	// Constructors

		#region Public Instance Properties

		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public AttributeCollection BrowsableAttributes {
			get {
				if (browsable_attributes == null) {
					browsable_attributes = new AttributeCollection (new Attribute[] { 
						BrowsableAttribute.Yes });
				}
				return browsable_attributes;
			}
			set {
				if (browsable_attributes == value)
					return;

				if (browsable_attributes == null || browsable_attributes.Count == 0)
					browsable_attributes = null;
				else
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
				base.BackColor = value;
				toolbar.BackColor = value;
				Refresh ();
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}

		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		public virtual bool CanShowCommands {
			get {
				return can_show_commands;
			}
		}

		[DefaultValue(typeof(Color), "ControlText")]
		public Color CategoryForeColor {
			get {
				return category_fore_color;
			}
			set {
				if (category_fore_color != value) {
					category_fore_color = value;
					Invalidate ();
				}
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

		public Color CommandsActiveLinkColor {
			get {
				return commands_active_link_color;
			}
			set {
				commands_active_link_color = value;
			}
		}
		
		public Color CommandsDisabledLinkColor {
			get {
				return commands_disabled_link_color;
			}
			set {
				commands_disabled_link_color = value;
			}
		}

		public Color CommandsLinkColor {
			get {
				return commands_link_color;
			}
			set {
				commands_link_color = value;
			}
		}

		[BrowsableAttribute (false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		[MonoTODO ("Commands are not implemented yet.")]
		public virtual bool CommandsVisible {
			get {
				return commands_visible;
			}
		}

		[DefaultValue (true)]
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
		public override Color ForeColor {
			get {
				return base.ForeColor;
			}
			set {
				base.ForeColor = value;
			}
		}

		[DefaultValue ("Color [Control]")]
		public Color HelpBackColor {
			get {
				return help_panel.BackColor;
			}
			set {
				help_panel.BackColor = value;
			}
		}

		[DefaultValue ("Color [ControlText]")]
		public Color HelpForeColor {
			get {
				return help_panel.ForeColor;
			}

			set {
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
				splitter.Visible = value;
				help_panel.Visible = value;
			}
		}

		[DefaultValue (false)]
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

		[DefaultValue ("Color [InactiveBorder]")]
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

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Padding Padding {
			get { return base.Padding; }
			set { base.Padding = value; }
		}
		
		[DefaultValue(PropertySort.CategorizedAlphabetical)]
		public PropertySort PropertySort {
			get {
				return property_sort;
			}

			set {
				if (!Enum.IsDefined (typeof (PropertySort), value))
					throw new InvalidEnumArgumentException ("value", (int) value, typeof (PropertySort));
				if (property_sort == value)
					return;

				// we do not need to update the the grid items and fire
				// a PropertySortChanged event when switching between
				// Categorized and CateogizedAlphabetical
				bool needUpdate = (property_sort & PropertySort.Categorized) == 0 ||
					(value & PropertySort.Categorized) == 0;
				property_sort = value;
				if (needUpdate) {
					UpdateSortLayout (root_grid_item);
					// update selection
					if (selected_grid_item != null) {
						if (selected_grid_item.GridItemType == GridItemType.Category && 
						    (value == PropertySort.Alphabetical || value == PropertySort.NoSort))
							SelectItemCore (null, null);
						else
							SelectItemCore (null, selected_grid_item);
					}
					property_grid_view.UpdateView ();

					EventHandler eh = (EventHandler)(Events [PropertySortChangedEvent]);
					if (eh != null)
						eh (this, EventArgs.Empty);
				}
				UpdatePropertySortButtonsState ();
			}
		}

		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public PropertyTabCollection PropertyTabs {
			get { return property_tabs; }
		}

		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		public GridItem SelectedGridItem {
			get { return selected_grid_item; }
			set {
				if (value == null)
					throw new ArgumentException ("GridItem specified to PropertyGrid.SelectedGridItem must be a valid GridItem.");
				if (value != selected_grid_item) {
					GridEntry oldItem = selected_grid_item;
					SelectItemCore (oldItem, (GridEntry)value);
					OnSelectedGridItemChanged (new SelectedGridItemChangedEventArgs (oldItem, value));
				}
			}
		}

		internal GridItem RootGridItem {
			get { return root_grid_item; }
		}

		private void UpdateHelp (GridItem item)
		{
			if (item == null) {
				help_title_label.Text = string.Empty;
				help_description_label.Text = string.Empty;
			} else {
				help_title_label.Text = item.Label;
				if (item.PropertyDescriptor != null)
					this.help_description_label.Text = item.PropertyDescriptor.Description;
			}
		}

		private void SelectItemCore (GridEntry oldItem, GridEntry item)
		{
			UpdateHelp (item);
			selected_grid_item = item;
			property_grid_view.SelectItem (oldItem, item);
		}

		internal void OnPropertyValueChangedInternal (GridItem item, object property_value) 
		{
			property_grid_view.UpdateView ();
			OnPropertyValueChanged (new PropertyValueChangedEventArgs (item, property_value));
		}

		internal void OnExpandItem (GridEntry item)
		{
			property_grid_view.ExpandItem (item);
			OnExpandedItemChanged (EventArgs.Empty);
		}

		internal void OnCollapseItem (GridEntry item)
		{
			property_grid_view.CollapseItem (item);
			OnExpandedItemChanged (EventArgs.Empty);
		}

		internal DialogResult ShowError (string text)
		{
			return this.ShowError (text, MessageBoxButtons.OK);
		}

		internal DialogResult ShowError (string text, MessageBoxButtons buttons)
		{
			if (text == null)
				throw new ArgumentNullException ("text");
			return MessageBox.Show (this, text, "Properties Window", buttons, MessageBoxIcon.Exclamation);
		}

		[DefaultValue(null)]
		[TypeConverter("System.Windows.Forms.PropertyGrid+SelectedObjectConverter, " + Consts.AssemblySystem_Windows_Forms)]
		public object SelectedObject {
			get {
				if (selected_objects.Length > 0)
					return selected_objects[0];
				return null;
			}

			set {
				if (selected_objects != null && selected_objects.Length == 1 && selected_objects[0] == value)
					return;
				if (value == null)
					SelectedObjects = new object[0];
				else
					SelectedObjects = new object[] {value};

			}
		}

		[BrowsableAttribute(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public object[] SelectedObjects {
			get {
				return selected_objects;
			}

			set {
				root_grid_item = null;
				SelectItemCore (null, null); // unselect current item in the view
				if (value != null) {
					for (int i = 0; i < value.Length; i++) {
						if (value [i] == null)
							throw new ArgumentException (String.Format ("Item {0} in the objs array is null.", i));
					}
					selected_objects = value;
				} else {
					selected_objects = new object [0];
				}

				ShowEventsButton (false);
				PopulateGrid (selected_objects);
				RefreshTabs(PropertyTabScope.Component);
				if (root_grid_item != null)
					SelectItemCore (null, GetDefaultPropertyItem (root_grid_item, selected_tab));
				property_grid_view.UpdateView ();
				OnSelectedObjectsChanged (EventArgs.Empty);
			}
		}

		[BrowsableAttribute(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public PropertyTab SelectedTab {
			get { return selected_tab; }
		}

		public override ISite Site {
			get { return base.Site; }
			set { base.Site = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override string Text {
			get { return base.Text; }
			set { base.Text = value; }
		}

		[DefaultValue(true)]
		public virtual bool ToolbarVisible {
			get { return toolbar.Visible; }
			set {
				if (toolbar.Visible == value) {
					return;
				}

				toolbar.Visible = value;
			}
		}
		
		protected ToolStripRenderer ToolStripRenderer {
			get {
				if (toolbar != null) {
					return toolbar.Renderer;
				}
				return null;
			}
			set {
				if (toolbar != null) {
					toolbar.Renderer = value;
				}
			}
		}

		[DefaultValue ("Color [Window]")]
		public Color ViewBackColor {
			get { return property_grid_view.BackColor; }
			set {
				if (property_grid_view.BackColor == value) {
					return;
				}

				property_grid_view.BackColor = value;
			}
		}

		[DefaultValue ("Color [WindowText]")]
		public Color ViewForeColor {
			get { return property_grid_view.ForeColor; }
			set {
				if (property_grid_view.ForeColor == value) {
					return;
				}

				property_grid_view.ForeColor = value;
			}
		}

		[DefaultValue (false)]
		public bool UseCompatibleTextRendering {
			get { return use_compatible_text_rendering; }
			set {
				if (use_compatible_text_rendering != value) {
					use_compatible_text_rendering = value;
					if (Parent != null)
						Parent.PerformLayout (this, "UseCompatibleTextRendering");
					Invalidate ();
				}
			}
		}

		#endregion	// Public Instance Properties

		#region Protected Instance Properties

		protected override Size DefaultSize {
			get { return base.DefaultSize; }
		}


		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
		protected virtual Type DefaultTabType {
			get { return typeof(PropertiesTab); }
		}
		
		protected bool DrawFlatToolbar {
			get { return (toolbar.Appearance == ToolBarAppearance.Flat); }			
			set {
				if (value) 
					toolbar.Appearance = ToolBarAppearance.Flat;
				else
					toolbar.Appearance = ToolBarAppearance.Normal;
			}
		}

		protected internal override bool ShowFocusCues {
			get { return base.ShowFocusCues; }
		}

		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		
		protected override void Dispose(bool disposing) {
			base.Dispose(disposing);
		}

		public void CollapseAllGridItems () 
		{
			GridEntry category = FindCategoryItem (selected_grid_item);
			if (category != null)
				SelectedGridItem = category;
			CollapseItemRecursive (root_grid_item);
			property_grid_view.UpdateView ();
		}

		private void CollapseItemRecursive (GridItem item)
		{
			if (item == null)
				return;

			foreach (GridItem child in item.GridItems) {
				CollapseItemRecursive (child);
				if (child.Expandable)
					child.Expanded = false;
			}
		}

		private GridEntry FindCategoryItem (GridEntry entry)
		{
			if (entry == null || (property_sort != PropertySort.Categorized && 
			    property_sort != PropertySort.CategorizedAlphabetical))
				return null;

			if (entry.GridItemType == GridItemType.Category)
				return entry;

			GridEntry category = null;
			GridItem current = (GridItem)entry;
			while (category == null) {
				if (current.Parent != null && current.Parent.GridItemType == GridItemType.Category)
					category = (GridEntry) current.Parent;
				current = current.Parent;
				if (current == null)
					break;
			}
			return (GridEntry) category;
		}

		public void ExpandAllGridItems () 
		{
			ExpandItemRecursive (root_grid_item);
			property_grid_view.UpdateView ();
		}

		private void ExpandItemRecursive (GridItem item)
		{
			if (item == null)
				return;

			foreach (GridItem child in item.GridItems) {
				ExpandItemRecursive (child);
				if (child.Expandable)
					child.Expanded = true;
			}
		}

		public override void Refresh () 
		{
			base.Refresh ();
			// force a full reload here
			SelectedObjects = SelectedObjects;
		}

		private void toolbar_Clicked (PropertyToolBarButton button)
		{
			if (button == null) 
				return;

			if (button == alphabetic_toolbarbutton) {
				this.PropertySort = PropertySort.Alphabetical;
				alphabetic_toolbarbutton.Pushed = true;
				categorized_toolbarbutton.Pushed = false;
			} else if (button == categorized_toolbarbutton) {
				this.PropertySort = PropertySort.CategorizedAlphabetical;
				categorized_toolbarbutton.Pushed = true;
				alphabetic_toolbarbutton.Pushed = false;
			} else {
				if (button.Enabled)
					SelectPropertyTab (button.PropertyTab);
			}
		}

		private void toolbarbutton_clicked (object o, EventArgs args)
		{
			toolbar_Clicked (o as PropertyToolBarButton);
		}

		private void SelectPropertyTab (PropertyTab propertyTab)
		{
			if (propertyTab != null && selected_tab != propertyTab) {
				foreach (object toolbarItem in toolbar.Items) {
					PropertyToolBarButton button = toolbarItem as PropertyToolBarButton;
					if (button != null && button.PropertyTab != null) {
						if (button.PropertyTab == selected_tab)
							button.Pushed = false;
						else if (button.PropertyTab == propertyTab)
							button.Pushed = true;
					}
				}
				selected_tab = propertyTab;
				PopulateGrid (selected_objects);
				SelectItemCore (null, GetDefaultPropertyItem (root_grid_item, selected_tab));
				property_grid_view.UpdateView ();
			}
		}

		private void UpdatePropertySortButtonsState ()
		{
			if (property_sort == PropertySort.NoSort) {
				alphabetic_toolbarbutton.Pushed = false;
				categorized_toolbarbutton.Pushed = false;
			} else if (property_sort == PropertySort.Alphabetical) {
				alphabetic_toolbarbutton.Pushed = true;
				categorized_toolbarbutton.Pushed = false;
			} else if (property_sort == PropertySort.Categorized || 
				   property_sort == PropertySort.CategorizedAlphabetical) {
				alphabetic_toolbarbutton.Pushed = false;
				categorized_toolbarbutton.Pushed = true;
			}
		}
		
		protected void ShowEventsButton (bool value) 
		{
			if (value && property_tabs.Contains (typeof (EventsTab)))
				events_tab_visible = true;
			else
				events_tab_visible = false;
			RefreshTabs (PropertyTabScope.Component);
		}

		public void RefreshTabs (PropertyTabScope tabScope) 
		{
			property_tabs.Clear (tabScope);
			if (selected_objects != null) {
				Type[] tabTypes = null;
				PropertyTabScope[] tabScopes = null;

				if (events_tab_visible && property_tabs.Contains (typeof (EventsTab)))
					property_tabs.InsertTab (0, properties_tab, PropertyTabScope.Component);

				GetMergedPropertyTabs (selected_objects, out tabTypes, out tabScopes);
				if (tabTypes != null && tabScopes != null && tabTypes.Length > 0) {
					bool selectedTabPreserved = false;
					for (int i=0; i < tabTypes.Length; i++) {
						property_tabs.AddTabType (tabTypes[i], tabScopes[i]);
						if (tabTypes[i] == selected_tab.GetType ())
							selectedTabPreserved = true;
					}
					if (!selectedTabPreserved)
						SelectPropertyTab (properties_tab);
				}
			} else {
				SelectPropertyTab (properties_tab);
			}
			RefreshToolbar (property_tabs);
		}

		private void RefreshToolbar (PropertyTabCollection tabs)
		{
			EnsurePropertiesTab ();

			toolbar.SuspendLayout ();
			toolbar.Items.Clear ();
			toolbar_imagelist.Images.Clear ();

			int imageIndex = 0;
			toolbar.Items.Add (categorized_toolbarbutton);
			toolbar_imagelist.Images.Add (categorized_image);
			categorized_toolbarbutton.ImageIndex = imageIndex;
			imageIndex++;
			toolbar.Items.Add (alphabetic_toolbarbutton);
			toolbar_imagelist.Images.Add (alphabetical_image);
			alphabetic_toolbarbutton.ImageIndex = imageIndex;
			imageIndex++;
			toolbar.Items.Add (separator_toolbarbutton);
			if (tabs != null && tabs.Count > 0) {
				foreach (PropertyTab tab in tabs) {
					PropertyToolBarButton button = new PropertyToolBarButton (tab);
					button.Click += new EventHandler (toolbarbutton_clicked);
					toolbar.Items.Add (button);
					if (tab.Bitmap != null) {
						tab.Bitmap.MakeTransparent ();
						toolbar_imagelist.Images.Add (tab.Bitmap);
						button.ImageIndex = imageIndex;
						imageIndex++;
					}
					if (tab == selected_tab)
						button.Pushed = true;
				}
				toolbar.Items.Add (new PropertyToolBarSeparator ());
			}

			toolbar.Items.Add (propertypages_toolbarbutton);
			toolbar_imagelist.Images.Add (propertypages_image);
			propertypages_toolbarbutton.ImageIndex = imageIndex;
			
			toolbar.ResumeLayout ();
		}

		private void EnsurePropertiesTab ()
		{
			if (property_tabs == null)
				return;

			if (property_tabs.Count > 0 && !property_tabs.Contains (this.DefaultTabType))
				property_tabs.InsertTab (0, properties_tab, PropertyTabScope.Component);
		}

		private void GetMergedPropertyTabs (object[] objects, out Type[] tabTypes, out PropertyTabScope[] tabScopes)
		{
			tabTypes = null;
			tabScopes = null;
			if (objects == null || objects.Length == 0)
				return;

			ArrayList intersection = null;
			ArrayList scopes = new ArrayList ();
			for (int i=0; i < objects.Length; i++) {
				if (objects[i] == null)
					continue;
				PropertyTabAttribute tabAttribute = (PropertyTabAttribute)TypeDescriptor.GetAttributes (objects[i])[typeof (PropertyTabAttribute)];
				if (tabAttribute == null || tabAttribute.TabClasses == null || tabAttribute.TabClasses.Length == 0)
					return;

				ArrayList new_intersection = new ArrayList ();
				scopes.Clear ();
				IList currentIntersection = (i == 0 ? (IList)tabAttribute.TabClasses : (IList)intersection);
				for (int j=0; j < currentIntersection.Count; j++) {
					if ((Type)currentIntersection[j] == tabAttribute.TabClasses[j]) {
						new_intersection.Add (tabAttribute.TabClasses[j]);
						scopes.Add (tabAttribute.TabScopes[j]);
					}
				}
				intersection = new_intersection;
			}

			tabTypes = new Type[intersection.Count];
			intersection.CopyTo (tabTypes);
			tabScopes = new PropertyTabScope[tabTypes.Length];
			scopes.CopyTo (tabScopes);
		}

		public void ResetSelectedProperty() 
		{
			if (selected_grid_item == null)
				return;
			selected_grid_item.ResetValue ();
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods

		protected virtual PropertyTab CreatePropertyTab (Type tabType) 
		{
			if (!typeof(PropertyTab).IsAssignableFrom (tabType))
				return null;

			PropertyTab tab = null;

			ConstructorInfo ctor = tabType.GetConstructor (new Type[] { typeof (IServiceProvider) });
			if (ctor != null)
				tab = (PropertyTab)ctor.Invoke (new object[] { this.Site });
			else
				tab = (PropertyTab)Activator.CreateInstance (tabType);
			return tab;
		}
		
		[MonoTODO ("Never called")]
		protected void OnComComponentNameChanged(ComponentRenameEventArgs e)
		{
			ComponentRenameEventHandler eh = (ComponentRenameEventHandler)(Events [ComComponentNameChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnEnabledChanged (EventArgs e) {
			base.OnEnabledChanged (e);
		}
		
		protected override void OnFontChanged(EventArgs e) {
			base.OnFontChanged (e);
		}

		protected override void OnGotFocus(EventArgs e) {
			base.OnGotFocus(e);
		}

		protected override void OnHandleCreated (EventArgs e) {
			base.OnHandleCreated (e);
		}

		protected override void OnHandleDestroyed (EventArgs e) {
			base.OnHandleDestroyed (e);
		}

		protected override void OnMouseDown (MouseEventArgs me) {
			base.OnMouseDown (me);
		}

		protected override void OnMouseMove (MouseEventArgs me) {
			base.OnMouseMove (me);
		}

		protected override void OnMouseUp (MouseEventArgs me) {
			base.OnMouseUp (me);
		}
		
		protected void OnNotifyPropertyValueUIItemsChanged(object sender, EventArgs e) 
		{
			property_grid_view.UpdateView ();
		}

		protected override void OnPaint (PaintEventArgs pevent) {
			pevent.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(BackColor), pevent.ClipRectangle);
			base.OnPaint (pevent);
		}

		protected virtual void OnPropertySortChanged(EventArgs e) {
			EventHandler eh = (EventHandler) Events [PropertySortChangedEvent];
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnPropertyTabChanged (PropertyTabChangedEventArgs e) 
		{
			PropertyTabChangedEventHandler eh = (PropertyTabChangedEventHandler)(Events [PropertyTabChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnPropertyValueChanged (PropertyValueChangedEventArgs e) {
			PropertyValueChangedEventHandler eh = (PropertyValueChangedEventHandler)(Events [PropertyValueChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnResize (EventArgs e) {
			base.OnResize (e);
		}

		protected virtual void OnSelectedGridItemChanged (SelectedGridItemChangedEventArgs e) {
			SelectedGridItemChangedEventHandler eh = (SelectedGridItemChangedEventHandler)(Events [SelectedGridItemChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnSelectedObjectsChanged (EventArgs e) {
			EventHandler eh = (EventHandler)(Events [SelectedObjectsChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override void OnSystemColorsChanged (EventArgs e) {
			base.OnSystemColorsChanged (e);
		}

		protected override void OnVisibleChanged (EventArgs e) {
			base.OnVisibleChanged (e);
		}

		protected void OnExpandedItemChanged (EventArgs e)
		{
			EventHandler eh = (EventHandler)(Events [ExpandedItemChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

		protected override bool ProcessDialogKey (Keys keyData) {
			return base.ProcessDialogKey (keyData);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		protected override void ScaleCore (float dx, float dy) {
			base.ScaleCore (dx, dy);
		}
		
		protected override void WndProc (ref Message m) 
		{
			base.WndProc (ref m);
		}
		#endregion

		#region Events
		static object PropertySortChangedEvent = new object ();
		static object PropertyTabChangedEvent = new object ();
		static object PropertyValueChangedEvent = new object ();
		static object SelectedGridItemChangedEvent = new object ();
		static object SelectedObjectsChangedEvent = new object ();
		static object ExpandedItemChangedEvent = new object ();

		public event EventHandler PropertySortChanged {
			add { Events.AddHandler (PropertySortChangedEvent, value); }
			remove { Events.RemoveHandler (PropertySortChangedEvent, value); }
		}

		public event PropertyTabChangedEventHandler PropertyTabChanged {
			add { Events.AddHandler (PropertyTabChangedEvent, value); }
			remove { Events.RemoveHandler (PropertyTabChangedEvent, value); }
		}

		public event PropertyValueChangedEventHandler PropertyValueChanged {
			add { Events.AddHandler (PropertyValueChangedEvent, value); }
			remove { Events.RemoveHandler (PropertyValueChangedEvent, value); }
		}

		public event SelectedGridItemChangedEventHandler SelectedGridItemChanged {
			add { Events.AddHandler (SelectedGridItemChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedGridItemChangedEvent, value); }
		}

		public event EventHandler SelectedObjectsChanged {
			add { Events.AddHandler (SelectedObjectsChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedObjectsChangedEvent, value); }
		}

		// UIA Framework Note: Used to track changes of expanded state of grid items
		internal event EventHandler ExpandedItemChanged {
			add { Events.AddHandler (ExpandedItemChangedEvent, value); }
			remove { Events.RemoveHandler (ExpandedItemChangedEvent, value); }
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public new event KeyEventHandler KeyDown {
			add { base.KeyDown += value; }
			remove { base.KeyDown -= value; }
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event KeyPressEventHandler KeyPress {
			add { base.KeyPress += value; }
			remove { base.KeyPress -= value; }
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public new event KeyEventHandler KeyUp {
			add { base.KeyUp += value; }
			remove { base.KeyUp -= value; }
		}

		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public new event MouseEventHandler MouseDown {
			add { base.MouseDown += value; }
			remove { base.MouseDown -= value; }
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public new event EventHandler MouseEnter {
			add { base.MouseEnter += value; }
			remove { base.MouseEnter -= value; }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public new event EventHandler MouseLeave {
			add { base.MouseLeave += value; }
			remove { base.MouseLeave -= value; }
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public new event MouseEventHandler MouseMove {
			add { base.MouseMove += value; }
			remove { base.MouseMove -= value; }
		}
		
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		[Browsable(false)]
		public new event MouseEventHandler MouseUp {
			add { base.MouseUp += value; }
			remove { base.MouseUp -= value; }
		}
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler PaddingChanged {
			add { base.PaddingChanged += value; }
			remove { base.PaddingChanged -= value; }
		}
		
		[Browsable(false)]
		public new event EventHandler TextChanged {
			add { base.TextChanged += value; }
			remove { base.TextChanged -= value; }
		}
		#endregion

		#region Com2Interop.IComPropertyBrowser Interface
		[MonoTODO ("Not implemented, will throw NotImplementedException")]
		bool ComponentModel.Com2Interop.IComPropertyBrowser.InPropertySet {
			get {
				throw new NotImplementedException();
			}
		}

		[MonoTODO ("Stub, does nothing")]
		void ComponentModel.Com2Interop.IComPropertyBrowser.DropDownDone ()
		{
		}

		[MonoTODO ("Not implemented, will throw NotImplementedException")]
		bool ComponentModel.Com2Interop.IComPropertyBrowser.EnsurePendingChangesCommitted ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Stub, does nothing")]
		void ComponentModel.Com2Interop.IComPropertyBrowser.HandleF4 ()
		{
		}

		[MonoTODO ("Stub, does nothing")]
		void ComponentModel.Com2Interop.IComPropertyBrowser.LoadState (Microsoft.Win32.RegistryKey optRoot)
		{
		}

		[MonoTODO ("Stub, does nothing")]
		void ComponentModel.Com2Interop.IComPropertyBrowser.SaveState (Microsoft.Win32.RegistryKey optRoot)
		{
		}

		static object ComComponentNameChangedEvent = new object ();
		event ComponentRenameEventHandler ComponentModel.Com2Interop.IComPropertyBrowser.ComComponentNameChanged {
			add { Events.AddHandler (ComComponentNameChangedEvent, value); }
			remove { Events.RemoveHandler (ComComponentNameChangedEvent, value); }
		}
		#endregion	// Com2Interop.IComPropertyBrowser Interface

		#region PropertyTabCollection Class
		public class PropertyTabCollection : ICollection, IEnumerable 
		{
			ArrayList property_tabs;
			ArrayList property_tabs_scopes;
			PropertyGrid property_grid;

			internal PropertyTabCollection (PropertyGrid propertyGrid) 
			{
				property_grid = propertyGrid;
				property_tabs = new ArrayList ();
				property_tabs_scopes = new ArrayList ();
			}

			public PropertyTab this[int index] {
				get { return (PropertyTab)property_tabs[index]; }
			}
		
			bool ICollection.IsSynchronized {
				get { return property_tabs.IsSynchronized; }
			}

			void ICollection.CopyTo (Array dest, int index)
			{
				property_tabs.CopyTo (dest, index);
			}

			object ICollection.SyncRoot {
				get { return property_tabs.SyncRoot; }
			}

			public IEnumerator GetEnumerator ()
			{
				return property_tabs.GetEnumerator ();
			}

			public int Count {
				get { return property_tabs.Count; }
			}

			public void AddTabType (Type propertyTabType)
			{
				AddTabType (propertyTabType, PropertyTabScope.Global);
			}

			public void AddTabType (Type propertyTabType, PropertyTabScope tabScope)
			{
				if (propertyTabType == null)
					throw new ArgumentNullException ("propertyTabType");

				// Avoid duplicates
				if (this.Contains (propertyTabType))
					return;
				PropertyTab tab = property_grid.CreatePropertyTab (propertyTabType);
				if (tab != null) {
					property_tabs.Add (tab);
					property_tabs_scopes.Add (tabScope);
				}
				property_grid.RefreshToolbar (this);
			}

			internal PropertyTabScope GetTabScope (PropertyTab tab)
			{
				if (tab == null)
					throw new ArgumentNullException ("tab");

				int index = property_tabs.IndexOf (tab);
				if (index != -1)
					return (PropertyTabScope)property_tabs_scopes[index];
				return PropertyTabScope.Global;
			}

			internal void InsertTab (int index, PropertyTab propertyTab, PropertyTabScope tabScope)
			{
				if (propertyTab == null)
					throw new ArgumentNullException ("propertyTab");
				
				if (!this.Contains (propertyTab.GetType ())) {
					property_tabs.Insert (index, propertyTab);
					property_tabs_scopes.Insert (index, tabScope);
				}
			}

			internal bool Contains (Type propertyType)
			{
				if (propertyType == null)
					throw new ArgumentNullException ("propertyType");

				foreach (PropertyTab t in property_tabs) {
					if (t.GetType () == propertyType)
						return true;
				}
				return false;
			}

			internal PropertyTab this[Type tabType] {
				get {
					foreach (PropertyTab tab in property_tabs) {
						if (tabType == tab.GetType ())
							return tab;
					}
					return null;
				}
			}

			public void Clear (PropertyTabScope tabScope)
			{
				ArrayList toRemove = new ArrayList ();
				for (int i=0; i < property_tabs_scopes.Count; i++) {
					if ((PropertyTabScope)property_tabs_scopes[i] == tabScope)
						toRemove.Add (i);
				}
				foreach (int indexToRemove in toRemove) {
					if (property_tabs.Count > indexToRemove)
						property_tabs.RemoveAt (indexToRemove);
					if (property_tabs_scopes.Count > indexToRemove)	
						property_tabs_scopes.RemoveAt (indexToRemove);
				}
				property_grid.RefreshToolbar (this);
			}

			public void RemoveTabType (Type propertyTabType)
			{
				if (propertyTabType == null)
					throw new ArgumentNullException ("propertyTabType");

				ArrayList toRemove = new ArrayList ();
				for (int i=0; i < property_tabs.Count; i++) {
					if (property_tabs[i].GetType () == propertyTabType)
						toRemove.Add (i);
				}
				foreach (int indexToRemove in toRemove) {
					property_tabs.RemoveAt (indexToRemove);
					property_tabs_scopes.RemoveAt (indexToRemove);
				}
				property_grid.RefreshToolbar (this);
			}
		}
		#endregion	// PropertyTabCollection Class

		#region Private Helper Methods

		private GridItem FindFirstPropertyItem (GridItem root)
		{
			if (root.GridItemType == GridItemType.Property)
				return root;

			foreach (GridItem item in root.GridItems) {
				GridItem subitem = FindFirstPropertyItem (item);
				if (subitem != null)
					return subitem;
			}

			return null;
		}

		private GridEntry GetDefaultPropertyItem (GridEntry rootItem, PropertyTab propertyTab)
		{
			if (rootItem == null || rootItem.GridItems.Count == 0 || propertyTab == null)
				return null;
			object[] propertyOwners = rootItem.Values;
			if (propertyOwners == null || propertyOwners.Length == 0 || propertyOwners[0] == null)
				return null;

			GridItem defaultSelected = null;
			if (propertyOwners.Length > 1)
				defaultSelected = rootItem.GridItems[0];
			else {
				PropertyDescriptor defaultProperty = propertyTab.GetDefaultProperty (propertyOwners[0]);
				if (defaultProperty != null)
					defaultSelected = FindItem (defaultProperty.Name, rootItem);
				if (defaultSelected == null)
					defaultSelected = FindFirstPropertyItem (rootItem);
			}

			return defaultSelected as GridEntry;
		}

		private GridEntry FindItem (string name, GridEntry rootItem)
		{
			if (rootItem == null || name == null)
				return null;

			if (property_sort == PropertySort.Alphabetical || property_sort == PropertySort.NoSort) {
				foreach (GridItem item in rootItem.GridItems) {
					if (item.Label == name) {
						return (GridEntry)item;
					}
				}
			} else if (property_sort == PropertySort.Categorized || 
				   property_sort == PropertySort.CategorizedAlphabetical) {
				foreach (GridItem categoryItem in rootItem.GridItems) {
					foreach (GridItem item in categoryItem.GridItems) {
						if (item.Label == name) {
							return (GridEntry)item;
						}
					}
				}
			}

			return null;
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

		private void PopulateGrid (object[] objects) 
		{
			if (objects.Length > 0) {
				root_grid_item = new RootGridEntry (this, objects);
				root_grid_item.Expanded = true;
				UpdateSortLayout (root_grid_item);
			} else {
				root_grid_item = null;
			}
		}

		private void UpdateSortLayout (GridEntry rootItem)
		{
			if (rootItem == null)
				return;

			GridItemCollection reordered = new GridItemCollection ();

			if (property_sort == PropertySort.Alphabetical || property_sort == PropertySort.NoSort) {
				alphabetic_toolbarbutton.Pushed = true;
				categorized_toolbarbutton.Pushed = false;
				foreach (GridItem item in rootItem.GridItems) {
					if (item.GridItemType == GridItemType.Category) {
						foreach (GridItem categoryChild in item.GridItems) {
							reordered.Add (categoryChild);
							((GridEntry)categoryChild).SetParent (rootItem);
						}
					} else {
						reordered.Add (item);
					}
				}
			} else if (property_sort == PropertySort.Categorized || 
				   property_sort == PropertySort.CategorizedAlphabetical) {
				alphabetic_toolbarbutton.Pushed = false;
				categorized_toolbarbutton.Pushed = true;
				GridItemCollection categories = new GridItemCollection ();

				foreach (GridItem item in rootItem.GridItems) {
					if (item.GridItemType == GridItemType.Category) {
						categories.Add (item);
						continue;
					}

					string categoryName = item.PropertyDescriptor.Category;
					if (categoryName == null)
						categoryName = UNCATEGORIZED_CATEGORY_LABEL;
					GridItem category_item = rootItem.GridItems [categoryName];
					if (category_item == null)
						category_item = categories [categoryName];

					if (category_item == null) {
						// Create category grid items if they already don't
						category_item = new CategoryGridEntry (this, categoryName, rootItem);
						category_item.Expanded = true;
						categories.Add (category_item);
					}

					category_item.GridItems.Add (item);
					((GridEntry)item).SetParent (category_item);
				}

				reordered.AddRange (categories);
			}

			rootItem.GridItems.Clear ();
			rootItem.GridItems.AddRange (reordered);
		}

		private void help_panel_Paint(object sender, PaintEventArgs e) {
			e.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(help_panel.BackColor), help_panel.ClientRectangle );
			e.Graphics.DrawRectangle(SystemPens.ControlDark, 0,0,help_panel.Width-1,help_panel.Height-1 );
		}

		#endregion	// Private Helper Methods

#region Internal helper classes
		// as we can not change the color for BorderStyle.FixedSingle and we need the correct
		// ClientRectangle so that the ScrollBar doesn't draw over the border we need this class
		internal class BorderHelperControl : Control {

			public BorderHelperControl ()
			{
				BackColor = ThemeEngine.Current.ColorWindow;
			}

			protected override void OnPaint (PaintEventArgs e)
			{
				e.Graphics.DrawRectangle (SystemPens.ControlDark, 0 , 0 , Width - 1, Height - 1);
				base.OnPaint (e);
			}
			
			protected override void OnSizeChanged (EventArgs e)
			{
				if (Controls.Count == 1) {
					Control control = Controls [0];
					
					if (control.Location.X != 1 || control.Location.Y != 1)
						control.Location = new Point (1, 1);
					
					control.Width = ClientRectangle.Width - 2;
					control.Height = ClientRectangle.Height - 2;
				}
				base.OnSizeChanged (e);
			}
		}

		private class PropertyToolBarSeparator : ToolStripSeparator
		{
			public PropertyToolBarSeparator ()
			{
			}
		}

		private class PropertyToolBarButton : ToolStripButton
		{
			private PropertyTab property_tab;

			public PropertyToolBarButton ()
			{
			}

			public PropertyToolBarButton (PropertyTab propertyTab)
			{
				if (propertyTab == null)
					throw new ArgumentNullException ("propertyTab");
				property_tab = propertyTab;
			}

			public PropertyTab PropertyTab {
				get { return property_tab; }
			}

			public bool Pushed {
				get { return base.Checked; }
				set { base.Checked = value; }
			}
			
			public ToolBarButtonStyle Style {
				get { return ToolBarButtonStyle.PushButton; }
				set { }
			}
		}
		
		// needed! this little helper makes it possible to draw a different toolbar border
		// and toolbar backcolor in ThemeWin32Classic
		internal class PropertyToolBar : ToolStrip
		{
			ToolBarAppearance appearance;

			public PropertyToolBar ()
			{
				SetStyle (ControlStyles.ResizeRedraw, true);
				GripStyle = ToolStripGripStyle.Hidden;
				appearance = ToolBarAppearance.Normal;
			}

			public bool ShowToolTips {
				get { return base.ShowItemToolTips; }
				set { base.ShowItemToolTips = value; }
			}
			
			public ToolBarAppearance Appearance {
				get { return appearance; }
				set { 
					if (value == Appearance)
						return;
						
					switch (value) {
					case ToolBarAppearance.Flat:
						Renderer = new ToolStripSystemRenderer ();
						appearance = ToolBarAppearance.Flat;
						break;
					case ToolBarAppearance.Normal:
						ProfessionalColorTable table = new ProfessionalColorTable ();
						table.UseSystemColors = true;
						Renderer = new ToolStripProfessionalRenderer (table);
						appearance = ToolBarAppearance.Normal;
						break;
					}
				}
			}
		}


		[MonoInternalNote ("not sure what this class does, but it's listed as a type converter for a property in this class, and this causes problems if it's not present")]
		private class SelectedObjectConverter : TypeConverter
		{
		}
#endregion
	}
}
