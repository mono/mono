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
using System.Runtime.InteropServices;
using System.Windows.Forms.Design;
using System.Windows.Forms.PropertyGridInternal;

namespace System.Windows.Forms {
	[Designer("System.Windows.Forms.Design.PropertyGridDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#if NET_2_0
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
#endif
	public class PropertyGrid : System.Windows.Forms.ContainerControl, ComponentModel.Com2Interop.IComPropertyBrowser {
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
		internal GridItem root_grid_item;
		private object[] selected_objects;
		private PropertyTab selected_tab;

		private ImageList toolbar_imagelist;
		private PropertyToolBarButton categorized_toolbarbutton;
		private PropertyToolBarButton alphabetic_toolbarbutton;
#if NET_2_0
		private ToolStripSeparator separator_toolbarbutton;
#else
		private ToolBarButton separator_toolbarbutton;
#endif
		private PropertyToolBarButton propertypages_toolbarbutton;

		internal PropertyToolBar toolbar;

		internal PropertyGridView property_grid_view;
		internal Splitter splitter;
		internal Panel help_panel;
		internal Label help_title_label;
		internal Label help_description_label;
		private MenuItem reset_menuitem;
		private MenuItem description_menuitem;
		private object current_property_value;

		private Color category_fore_color;
#if NET_2_0
		private Color commands_active_link_color;
		private Color commands_disabled_link_color;
		private Color commands_link_color;
#endif
		#endregion	// Private Members
		
		#region Contructors
		public PropertyGrid() {
			selected_objects = new object[0];
			property_tabs = new PropertyTabCollection();

			line_color = SystemColors.ScrollBar;
			category_fore_color = line_color;
			browsable_attributes = new AttributeCollection(new Attribute[] {});
			commands_visible_if_available = false;
			property_sort = PropertySort.Categorized;

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
			help_description_label.Name = "help_description_label";
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
			alphabetic_toolbarbutton = new PropertyToolBarButton ();
#if NET_2_0
			separator_toolbarbutton = new ToolStripSeparator ();
#else
			separator_toolbarbutton = new ToolBarButton ();
#endif
			propertypages_toolbarbutton = new PropertyToolBarButton ();
			ContextMenu context_menu = new ContextMenu();

			toolbar_imagelist = new ImageList();
			toolbar_imagelist.ColorDepth = ColorDepth.Depth32Bit;
			toolbar_imagelist.ImageSize = new System.Drawing.Size(16, 16);
			toolbar_imagelist.TransparentColor = System.Drawing.Color.Transparent;
			toolbar_imagelist.Images.Add( (Image)Locale.GetResource( "propertygrid_sort_category") );
			toolbar_imagelist.Images.Add( (Image)Locale.GetResource( "propertygrid_sort_alphabetical") );
			toolbar_imagelist.Images.Add( (Image)Locale.GetResource( "propertygrid_tab_properties") );

			toolbar.Appearance = ToolBarAppearance.Flat;
			toolbar.AutoSize = false;
			
			toolbar.ImageList = toolbar_imagelist;
			toolbar.Location = new System.Drawing.Point(0, 0);
			toolbar.ShowToolTips = true;
			toolbar.Size = new System.Drawing.Size(256, 27);
			toolbar.TabIndex = 0;
#if NET_2_0
			toolbar.Items.AddRange (new ToolStripItem [] {categorized_toolbarbutton,
								      alphabetic_toolbarbutton,
								      separator_toolbarbutton,
								      propertypages_toolbarbutton});
			//toolbar.ButtonSize = new System.Drawing.Size (20, 20);
			toolbar.ItemClicked += new ToolStripItemClickedEventHandler (toolbar_ButtonClick);
#else
			toolbar.Buttons.AddRange(new ToolBarButton [] {categorized_toolbarbutton,
								      alphabetic_toolbarbutton,
								      separator_toolbarbutton,
								      propertypages_toolbarbutton});
			toolbar.ButtonSize = new System.Drawing.Size(20, 20);
			toolbar.ButtonClick += new ToolBarButtonClickEventHandler(toolbar_ButtonClick);
			
			separator_toolbarbutton.Style = ToolBarButtonStyle.Separator;
#endif

			categorized_toolbarbutton.ImageIndex = 0;
			categorized_toolbarbutton.Style = ToolBarButtonStyle.ToggleButton;
			categorized_toolbarbutton.ToolTipText = Locale.GetText ("Categorized");

			alphabetic_toolbarbutton.ImageIndex = 1;
			alphabetic_toolbarbutton.Style = ToolBarButtonStyle.ToggleButton;
			alphabetic_toolbarbutton.ToolTipText = Locale.GetText ("Alphabetic");

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
			
			BorderHelperControl helper = new BorderHelperControl ();
			helper.Dock = DockStyle.Fill;
			helper.Controls.Add (property_grid_view);
			
			this.Controls.Add(helper);
			this.Controls.Add(toolbar);
			this.Controls.Add(splitter);
			this.Controls.Add(help_panel);
			this.Name = "PropertyGrid";
			this.Size = new System.Drawing.Size(256, 400);

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

#if NET_2_0
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		public override ImageLayout BackgroundImageLayout {
			get { return base.BackgroundImageLayout; }
			set { base.BackgroundImageLayout = value; }
		}
#endif

		[BrowsableAttribute(false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		public virtual bool CanShowCommands {
			get {
				return can_show_commands;
			}
		}
#if NET_2_0
		[DefaultValue(typeof(Color), "ControlText")]
		public
#else
		internal
#endif
		Color CategoryForeColor {
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
#if NET_2_0
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
#endif

		[BrowsableAttribute (false)]
		[EditorBrowsableAttribute(EditorBrowsableState.Advanced)]
		[MonoTODO ("Commands are not implemented yet.")]
		public virtual bool CommandsVisible {
			get {
				return commands_visible;
			}
		}

#if NET_2_0
		[DefaultValue (true)]
#else
		[DefaultValue (false)]
#endif
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

#if NET_2_0
		[DefaultValue ("Color [Control]")]
#endif
		public Color HelpBackColor {
			get {
				return help_panel.BackColor;
			}
			set {
				if (help_panel.BackColor == value) {
					return;
				}

				help_panel.BackColor = value;
			}
		}

#if NET_2_0
		[DefaultValue ("Color [ControlText]")]
#endif
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

#if NET_2_0
		[DefaultValue (false)]
#endif
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

#if NET_2_0
		[DefaultValue ("Color [InactiveBorder]")]
#endif
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

#if NET_2_0
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Padding Padding {
			get { return base.Padding; }
			set { base.Padding = value; }
		}
#endif
		
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

				UpdateToolBarButtons();
				ReflectObjects();
				property_grid_view.Refresh();

				EventHandler eh = (EventHandler)(Events [PropertySortChangedEvent]);
				if (eh != null)
					eh (this, EventArgs.Empty);
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
				return SelectedGridItemInternal;
			}

			set {
				if (value == null) {
					throw new ArgumentException ("GridItem specified to PropertyGrid.SelectedGridItem must be a valid GridItem.");
				}

				GridItem oldItem = selected_grid_item;
				
				SelectedGridItemInternal = value;

				OnSelectedGridItemChanged (new SelectedGridItemChangedEventArgs (oldItem, selected_grid_item));
			}
		}
		
		internal GridItem SelectedGridItemInternal {
			get
			{
				return selected_grid_item;
			}

			set
			{
				if (selected_grid_item == value) {
					return;
				}

				selected_grid_item = value;
				if (selected_grid_item == null) {
					help_title_label.Text = string.Empty;
					help_description_label.Text = string.Empty;
				} else {
					help_title_label.Text = selected_grid_item.Label;
					if (selected_grid_item.PropertyDescriptor != null)
						this.help_description_label.Text = selected_grid_item.PropertyDescriptor.Description;

					current_property_value = value.Value;
				}
			}
		}

		internal void PropertyValueChangedInternal () {
			OnPropertyValueChanged (new PropertyValueChangedEventArgs (selected_grid_item, current_property_value));
			PopulateSubGridItemsFromProperties (SelectedGridItem as GridEntry);
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
				if (value != null) {
					for (int i = 0; i < value.Length; i++) {
						if (value [i] == null)
							throw new ArgumentException (String.Format ("Item {0} in the objs array is null.", i));
					}
					selected_objects = value;
				} else {
					selected_objects = new object [0];
				}

				if (selected_objects.Length > 0) {
					PropertyTabAttribute[] propTabs = (PropertyTabAttribute[])this.SelectedObject.GetType().GetCustomAttributes(typeof(PropertyTabAttribute),true);
					if (propTabs.Length > 0) {
						foreach (Type tabType in propTabs[0].TabClasses) {
							this.PropertyTabs.AddTabType(tabType);
						}
					}
				} else {
					SelectedGridItemInternal = null;
				}

				RefreshTabs(PropertyTabScope.Component);
				ReflectObjects();
				if (root_grid_item != null) {
					/* find the first non category grid item and select it */
					SelectedGridItemInternal = FindFirstItem (root_grid_item);
				}
				property_grid_view.Refresh();
				OnSelectedObjectsChanged (EventArgs.Empty);
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

#if NET_2_0
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override string Text {
			get {
				return base.Text;
			}
			set {
				base.Text = value;
			}
		}
 #endif

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
		
#if NET_2_0
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
 
#endif

#if NET_2_0
		[DefaultValue ("Color [Window]")]
#endif
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

#if NET_2_0
		[DefaultValue ("Color [WindowText]")]
#endif
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
#if NET_2_0

		[DefaultValue (false)]
		public bool UseCompatibleTextRendering {
			get {

				return use_compatible_text_rendering;
			}
			set {
				use_compatible_text_rendering = value;
			}
		}
#endif

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
		
		protected override void Dispose(bool val) {
			base.Dispose(val);
		}

		[MonoTODO ("should this be recursive?  or just the toplevel items?")]
		public void CollapseAllGridItems () {
			if (root_grid_item != null) {
				foreach (GridItem item in root_grid_item.GridItems) {
					item.Expanded = false;
				}
			}
		}

		[MonoTODO ("should this be recursive?  or just the toplevel items?")]
		public void ExpandAllGridItems () {
			if (root_grid_item != null) {
				foreach (GridItem item in root_grid_item.GridItems) {
					item.Expanded = true;
				}
			}
		}

		public override void Refresh () {
			base.Refresh ();
		}

		public void RefreshTabs (PropertyTabScope tabScope) {
			
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

		public void ResetSelectedProperty() {
			if (selected_grid_item == null || selected_grid_item.PropertyDescriptor == null)
				return;
			
			selected_grid_item.PropertyDescriptor.ResetValue(SelectedObject);
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods

		protected virtual PropertyTab CreatePropertyTab(Type tabType) {
			return (PropertyTab)Activator.CreateInstance(tabType);
		}
		
		[MonoTODO]
		protected void OnComComponentNameChanged(ComponentRenameEventArgs e)
		{
			ComponentRenameEventHandler eh = (ComponentRenameEventHandler)(Events [ComComponentNameChangedEvent]);
			if (eh != null)
				eh (this, e);
		}

#if NET_2_0
		protected override void OnEnabledChanged (EventArgs e) {
			base.OnEnabledChanged (e);
		}

#endif
		
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

		protected override void OnMouseDown (MouseEventArgs e) {
			base.OnMouseDown (e);
		}

		protected override void OnMouseMove (MouseEventArgs e) {
			base.OnMouseMove (e);
		}

		protected override void OnMouseUp (MouseEventArgs e) {
			base.OnMouseUp (e);
		}
		
		[MonoTODO]
		protected void OnNotifyPropertyValueUIItemsChanged(object sender, EventArgs e) {
		}

		protected override void OnPaint (PaintEventArgs pevent) {
			pevent.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(BackColor), pevent.ClipRectangle);
			base.OnPaint (pevent);
		}

#if NET_2_0
		protected virtual void OnPropertySortChanged(EventArgs e) {
			EventHandler eh = (EventHandler) Events [PropertySortChangedEvent];
			if (eh != null)
				eh (this, e);
		}		
#endif
		
		[MonoTODO]
		protected virtual void OnPropertyTabChanged (PropertyTabChangedEventArgs e) {
			throw new NotImplementedException();
		}

		protected virtual void OnPropertyValueChanged (PropertyValueChangedEventArgs e) {
			PropertyValueChangedEventHandler eh = (PropertyValueChangedEventHandler)(Events [PropertyValueChangedEvent]);
			if (eh != null) {
				eh (this, e);
				current_property_value = selected_grid_item.Value;
			}
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

		protected override bool ProcessDialogKey (Keys keyData) {
			return base.ProcessDialogKey (keyData);
		}

#if NET_2_0
		[EditorBrowsable (EditorBrowsableState.Never)]
#endif
		protected override void ScaleCore (float dx, float dy) {
			base.ScaleCore (dx, dy);
		}
		
		[MonoTODO]
		protected void ShowEventsButton(bool value) {
			throw new NotImplementedException();
		}

		protected override void WndProc (ref Message m) {
			base.WndProc (ref m);
		}
		#endregion

		#region Events
		static object PropertySortChangedEvent = new object ();
		static object PropertyTabChangedEvent = new object ();
		static object PropertyValueChangedEvent = new object ();
		static object SelectedGridItemChangedEvent = new object ();
		static object SelectedObjectsChangedEvent = new object ();

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
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageChanged {
			add { base.BackgroundImageChanged += value; }
			remove { base.BackgroundImageChanged -= value; }
		}

#if NET_2_0
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler BackgroundImageLayoutChanged {
			add { base.BackgroundImageLayoutChanged += value; }
			remove { base.BackgroundImageLayoutChanged -= value; }
		}
#endif
		
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public new event EventHandler ForeColorChanged {
			add { base.ForeColorChanged += value; }
			remove { base.ForeColorChanged -= value; }
		}
#if NET_2_0
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
#endif
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

		static object ComComponentNameChangedEvent = new object ();
		event ComponentRenameEventHandler ComponentModel.Com2Interop.IComPropertyBrowser.ComComponentNameChanged {
			add { Events.AddHandler (ComComponentNameChangedEvent, value); }
			remove { Events.RemoveHandler (ComComponentNameChangedEvent, value); }
		}
		#endregion	// Com2Interop.IComPropertyBrowser Interface

		#region PropertyTabCollection Class
		public class PropertyTabCollection : ICollection, IEnumerable {
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
			bool ICollection.IsSynchronized {
				get {
					return list.IsSynchronized;
				}
			}

			void ICollection.CopyTo(Array array, int index) {
				list.CopyTo(array, index);
			}

			object ICollection.SyncRoot {
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
			public void AddTabType(System.Type propertyTabType) {
				list.Add(Activator.CreateInstance(propertyTabType));
			}
			[MonoTODO]
			public void AddTabType(System.Type propertyTabType,
				System.ComponentModel.PropertyTabScope tabScope) {
				AddTabType(propertyTabType);
			}
			[MonoTODO]
			public void Clear(System.ComponentModel.PropertyTabScope tabScope) {
				throw new NotImplementedException();
			}
			[MonoTODO]
			public void RemoveTabType(System.Type propertyTabType) {
				throw new NotImplementedException();
			}
			#endregion
		}
		#endregion	// PropertyTabCollection Class

		#region Private Helper Methods

		private GridItem FindFirstItem (GridItem root)
		{
			if (root.GridItemType == GridItemType.Property)
				return root;

			foreach (GridItem item in root.GridItems) {
				GridItem subitem = FindFirstItem (item);
				if (subitem != null)
					return subitem;
			}

			return null;
		}


#if NET_2_0
		private void toolbar_ButtonClick (object sender, ToolStripItemClickedEventArgs e)
		{
			toolbar_Clicked (e.ClickedItem as PropertyToolBarButton);
		}
#else
		private void toolbar_ButtonClick (object sender, ToolBarButtonClickEventArgs e) {
			toolbar_Clicked (e.Button as PropertyToolBarButton);
		}
#endif

		private void toolbar_Clicked (PropertyToolBarButton button)
		{
			if (button == null) 
				return;
				
			if (button == alphabetic_toolbarbutton) {
				this.PropertySort = PropertySort.Alphabetical;
			} else if (button == categorized_toolbarbutton) {
				this.PropertySort = PropertySort.Categorized;
			}
		}
		
		internal void UpdateToolBarButtons () {
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

		private void OnResetPropertyClick (object sender, EventArgs e) {
			ResetSelectedProperty();
		}

		private void OnDescriptionClick (object sender, EventArgs e) {
			this.HelpVisible = !this.HelpVisible;
			description_menuitem.Checked = this.HelpVisible;

		}

		private void ReflectObjects () {
			if (selected_objects.Length > 0) {
				root_grid_item = new RootGridEntry (property_grid_view,
								    selected_objects.Length > 1 ? selected_objects : selected_objects[0]);
									   
				PopulateMergedGridItems (selected_objects, true, root_grid_item as GridEntry);
			} else {
				root_grid_item = null;
			}
		}

		private bool IsPropertyVisible (PropertyDescriptor property, bool mergable)
		{
			if (property == null)
				return false;
				
			if (!property.IsBrowsable)
				return false;
				
			if (mergable) {
				MergablePropertyAttribute attrib = property.Attributes [typeof (MergablePropertyAttribute)] as MergablePropertyAttribute;
				if (attrib != null && !attrib.AllowMerge)
					return false;
			}
			
			EditorBrowsableAttribute browsable = property.Attributes [typeof (EditorBrowsableAttribute)] as EditorBrowsableAttribute;
			if (browsable != null && (browsable.State == EditorBrowsableState.Advanced || browsable.State == EditorBrowsableState.Never)) {
				return false;
			}
			
			return true;
		}

		private void PopulateMergedGridItems (object [] objs, bool recurse, GridEntry parent_grid_item)
		{
			ArrayList intersection = null;

			if (parent_grid_item == null)
				return;

			for (int i = 0; i < objs.Length; i ++) {
				if (objs [i] == null)
					continue;

				ArrayList new_intersection = new ArrayList ();
				Type type = objs[i].GetType();

				/* i tried using filter attributes, but there's no way to do it for EditorBrowsableAttributes,
				   since that type lacks an override for IsDefaultAttribute, and for some reason the
				   BrowsableAttribute.Yes filter wasn't working */
				PropertyDescriptorCollection properties = null;
				ICustomTypeDescriptor descriptor;

				descriptor = objs [i] as ICustomTypeDescriptor;
				if (descriptor != null) {
					properties = descriptor.GetProperties ();
				}
				if (properties == null) {
					TypeConverter cvt = TypeDescriptor.GetConverter (objs[i]);
					properties = cvt.GetProperties (objs[i]);
				}
				if (properties == null) {
					properties = TypeDescriptor.GetProperties (objs[i]);
				}

				foreach (PropertyDescriptor p in (i == 0 ? (ICollection)properties : (ICollection)intersection)) {
					PropertyDescriptor property = (i == 0 ? p : properties [p.Name]);
					
					if (!IsPropertyVisible (property, objs.Length > 1))
						continue;
						
					Type p_type = p.ComponentType;
					Type property_type = property.ComponentType;

					if (p_type.IsAssignableFrom (type))
						new_intersection.Add (p);
					else if (property_type.IsAssignableFrom (p_type))
						new_intersection.Add (property);
				}

				intersection = new_intersection;
			}

			if (intersection != null && intersection.Count > 0)
				PopulateGridItemsFromProperties (objs, intersection, recurse, parent_grid_item);
		}

		private void PopulateGridItemsFromProperties (object[] objs, ArrayList properties, bool recurse, GridEntry parent_grid_item)
		{
			
			GridItemCollection grid_item_coll = parent_grid_item.GridItems;

			foreach (PropertyDescriptor property in properties) {
				GridEntry grid_entry;
				grid_entry = grid_item_coll [property.Name] as GridEntry;
				if (grid_entry == null) {
					grid_entry = new GridEntry (property_grid_view, objs, property);
				} else {
					grid_entry.SelectedObjects = objs;
				}
					
				grid_entry.SetParent (parent_grid_item);
				
				bool categorized = false, show = true;
					
				if (property_sort == PropertySort.Alphabetical || /* XXX */property_sort == PropertySort.NoSort || !recurse) {
					categorized = false;
				} else if (property_sort == PropertySort.Categorized || property_sort == PropertySort.CategorizedAlphabetical) {
					categorized = parent_grid_item == root_grid_item;
				} else {
					show = false;
				}
				
				
				if (show) {
					if (categorized) {
						string category = property.Category;
						CategoryGridEntry cat_item = grid_item_coll [category] as CategoryGridEntry;
						if (cat_item == null) {
							cat_item = new CategoryGridEntry (property_grid_view, category);
							cat_item.SetParent (parent_grid_item);
							grid_item_coll.Add (category, cat_item);
						}
						if (cat_item.GridItems [property.Name] == null) {
							cat_item.GridItems.Add (property.Name, grid_entry);
							grid_entry.SetParent (cat_item);
						}
					} else {
						if (grid_item_coll [property.Name] == null) {
							grid_item_coll.Add (property.Name, grid_entry);
							grid_entry.SetParent (parent_grid_item);
						}
					}
				}

				if (recurse) {
					PopulateSubGridItemsFromProperties (grid_entry);
				}
				grid_entry.Expanded = false;
			}
		}

		private void PopulateSubGridItemsFromProperties (GridEntry grid_entry)
		{
			if (grid_entry == null)
				return;

			if (grid_entry.SelectedObjects == null)
				return;

			PropertyDescriptor property = grid_entry.PropertyDescriptor;

			if (property.Converter == null || !property.Converter.GetPropertiesSupported ())
				return;

			object [] objs = grid_entry.SelectedObjects;
			object [] subobjs = new object [objs.Length];
			
			for (int i = 0; i < objs.Length; i++)
				subobjs [i] = property.GetValue (objs [i]);
			
			PopulateMergedGridItems (subobjs, true, grid_entry);
		}

		private void help_panel_Paint(object sender, PaintEventArgs e) {
			e.Graphics.FillRectangle(ThemeEngine.Current.ResPool.GetSolidBrush(help_panel.BackColor), help_panel.ClientRectangle );
			e.Graphics.DrawRectangle(SystemPens.ControlDark, 0,0,help_panel.Width-1,help_panel.Height-1 );
		}

		internal object GetTarget (GridItem item, int selected_index)
		{
			object target = ((GridEntry)item).SelectedObjects[selected_index];

			while (item.Parent != null && item.Parent.GridItemType != GridItemType.Property)
				item = item.Parent;

			if (item.Parent != null && item.Parent.PropertyDescriptor != null)
				target = item.Parent.PropertyDescriptor.GetValue (((GridEntry)item.Parent).SelectedObjects[selected_index]);

			return target;
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
		
		internal class PropertyToolBarButton : 
#if NET_2_0
		ToolStripButton
#else
		ToolBarButton
#endif
		{
		
#if NET_2_0
		public bool Pushed {
			get { return base.Checked; }
			set { base.Checked = value; }
		}
		
		public ToolBarButtonStyle Style {
			get { return ToolBarButtonStyle.PushButton; }
			set { }
		}
#endif
		}
		
		// needed! this little helper makes it possible to draw a different toolbar border
		// and toolbar backcolor in ThemeWin32Classic
		internal class PropertyToolBar : 
#if NET_2_0
		ToolStrip
#else
		ToolBar 
#endif
		{
		
#if NET_2_0
			bool flat;
#endif
			public PropertyToolBar ()
			{
				SetStyle (ControlStyles.ResizeRedraw, true);
#if NET_2_0
				GripStyle = ToolStripGripStyle.Hidden;
#endif
			}
#if NET_2_0
			public bool ShowToolTips {
				get { return base.ShowItemToolTips; }
				set { base.ShowItemToolTips = value; }
			}
			
			public ToolBarAppearance Appearance {
				get { return flat ? ToolBarAppearance.Flat : ToolBarAppearance.Normal; }
				set { 
					if (value == Appearance)
						return;
						
					switch (value) {
					case ToolBarAppearance.Flat:
						Renderer = new ToolStripSystemRenderer ();
						break;
					case ToolBarAppearance.Normal:
						ProfessionalColorTable table = new ProfessionalColorTable ();
						table.UseSystemColors = true;
						Renderer = new ToolStripProfessionalRenderer (table);
						break;
					}
				}
			}
#endif
		}


		[MonoTODO ("not sure what this class does, but it's listed as a type converter for a property in this class, and this causes problems if it's not present")]
		internal class SelectedObjectConverter : TypeConverter
		{
		}
#endregion
	}
}
