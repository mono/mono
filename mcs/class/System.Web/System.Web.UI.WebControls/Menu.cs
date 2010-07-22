//
// System.Web.UI.WebControls.Menu.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//	Igor Zelmanovich (igorz@mainsoft.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
//

//
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

#if NET_2_0

using System;
using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Web.UI;
using System.Web.Handlers;
using System.Collections.Specialized;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

namespace System.Web.UI.WebControls
{
	[DefaultEvent ("MenuItemClick")]
	[ControlValueProperty ("SelectedValue")]
	[Designer ("System.Web.UI.Design.WebControls.MenuDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[SupportsEventValidation]
	public class Menu : HierarchicalDataBoundControl, IPostBackEventHandler, INamingContainer
	{
		MenuItemStyle dynamicMenuItemStyle;
		SubMenuStyle dynamicMenuStyle;
		MenuItemStyle dynamicSelectedStyle;
		MenuItemStyle staticMenuItemStyle;
		SubMenuStyle staticMenuStyle;
		MenuItemStyle staticSelectedStyle;
		Style staticHoverStyle;
		Style dynamicHoverStyle;

		MenuItemStyleCollection levelMenuItemStyles;
		MenuItemStyleCollection levelSelectedStyles;
		SubMenuStyleCollection levelSubMenuStyles;
		ITemplate staticItemTemplate;
		ITemplate dynamicItemTemplate;
		
		MenuItemCollection items;
		MenuItemBindingCollection dataBindings;
		MenuItem selectedItem;
		string selectedItemPath;
		Hashtable bindings;

		Hashtable _menuItemControls;
		bool _requiresChildControlsDataBinding;
		SiteMapNode _currSiteMapNode;
		int registeredStylesCounter = -1;
		List<Style> levelSelectedLinkStyles;
		List<Style> levelMenuItemLinkStyles;
		Style popOutBoxStyle;
		Style controlLinkStyle;
		Style dynamicMenuItemLinkStyle;
		Style staticMenuItemLinkStyle;
		Style dynamicSelectedLinkStyle;
		Style staticSelectedLinkStyle;
		Style dynamicHoverLinkStyle;
		Style staticHoverLinkStyle;
#if NET_4_0
		bool includeStyleBlock = true;
		MenuRenderingMode renderingMode = MenuRenderingMode.Default;
#endif
		static readonly object MenuItemClickEvent = new object();
		static readonly object MenuItemDataBoundEvent = new object();
		
		public static readonly string MenuItemClickCommandName = "Click";
		
		public event MenuEventHandler MenuItemClick {
			add { Events.AddHandler (MenuItemClickEvent, value); }
			remove { Events.RemoveHandler (MenuItemClickEvent, value); }
		}
		
		public event MenuEventHandler MenuItemDataBound {
			add { Events.AddHandler (MenuItemDataBoundEvent, value); }
			remove { Events.RemoveHandler (MenuItemDataBoundEvent, value); }
		}
		
		protected virtual void OnMenuItemClick (MenuEventArgs e)
		{
			if (Events != null) {
				MenuEventHandler eh = (MenuEventHandler) Events [MenuItemClickEvent];
				if (eh != null) eh (this, e);
			}
		}
		
		protected virtual void OnMenuItemDataBound (MenuEventArgs e)
		{
			if (Events != null) {
				MenuEventHandler eh = (MenuEventHandler) Events [MenuItemDataBoundEvent];
				if (eh != null) eh (this, e);
			}
		}
#if NET_4_0
		[DefaultValue (true)]
		[Description ("Determines whether or not to render the inline style block (only used in standards compliance mode)")]
		public bool IncludeStyleBlock {
			get { return includeStyleBlock; }
			set { includeStyleBlock = value; }
		}

		[DefaultValue (MenuRenderingMode.Default)]
		public MenuRenderingMode RenderingMode {
			get { return renderingMode; }
			set {
				if (value < MenuRenderingMode.Default || value > MenuRenderingMode.List)
					throw new ArgumentOutOfRangeException ("value");

				renderingMode = value;
			}
		}
#endif
		[DefaultValueAttribute (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[EditorAttribute ("System.Web.UI.Design.WebControls.MenuBindingsEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[MergablePropertyAttribute (false)]
		public MenuItemBindingCollection DataBindings {
			get {
				if (dataBindings == null) {
					dataBindings = new MenuItemBindingCollection ();
					if (IsTrackingViewState)
						((IStateManager)dataBindings).TrackViewState();
				}
				return dataBindings;
			}
		}

		[DefaultValue (500)]
		[ThemeableAttribute (false)]
		public int DisappearAfter {
			get {
				object o = ViewState ["DisappearAfter"];
				if (o != null) return (int)o;
				return 500;
			}
			set {
				ViewState["DisappearAfter"] = value;
			}
		}

		[ThemeableAttribute (true)]
		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string DynamicBottomSeparatorImageUrl {
			get {
				object o = ViewState ["dbsiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["dbsiu"] = value;
			}
		}

		[DefaultValueAttribute ("")]
		public string DynamicItemFormatString {
			get {
				object o = ViewState ["DynamicItemFormatString"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["DynamicItemFormatString"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string DynamicTopSeparatorImageUrl {
			get {
				object o = ViewState ["dtsiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["dtsiu"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string StaticBottomSeparatorImageUrl {
			get {
				object o = ViewState ["sbsiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["sbsiu"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string StaticTopSeparatorImageUrl {
			get {
				object o = ViewState ["stsiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["stsiu"] = value;
			}
		}

		[DefaultValue (Orientation.Vertical)]
		public Orientation Orientation {
			get {
				object o = ViewState ["Orientation"];
				if (o != null) return (Orientation) o;
				return Orientation.Vertical;
			}
			set {
				ViewState["Orientation"] = value;
			}
		}

		[DefaultValue (1)]
		[ThemeableAttribute (true)]
		public int StaticDisplayLevels {
			get {
				object o = ViewState ["StaticDisplayLevels"];
				if (o != null) return (int)o;
				return 1;
			}
			set {
				if (value < 1) throw new ArgumentOutOfRangeException ();
				ViewState["StaticDisplayLevels"] = value;
			}
		}

		[DefaultValueAttribute ("")]
		public string StaticItemFormatString {
			get {
				object o = ViewState ["StaticItemFormatString"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["StaticItemFormatString"] = value;
			}
		}

		[DefaultValue (typeof (Unit), "16px")]
		[ThemeableAttribute (true)]
		public Unit StaticSubMenuIndent {
			get {
				object o = ViewState ["StaticSubMenuIndent"];
				if (o != null)
					return (Unit)o;
				// LAMESPEC: on 4.0 it returns Unit.Empty and on 3.5 16px
#if NET_4_0
				return Unit.Empty;
#else
				return new Unit (16);
#endif
			}
			set {
				ViewState["StaticSubMenuIndent"] = value;
			}
		}

		[ThemeableAttribute (true)]
		[DefaultValue (3)]
		public int MaximumDynamicDisplayLevels {
			get {
				object o = ViewState ["MaximumDynamicDisplayLevels"];
				if (o != null) return (int)o;
				return 3;
			}
			set {
				if (value < 0) throw new ArgumentOutOfRangeException ();
				ViewState["MaximumDynamicDisplayLevels"] = value;
			}
		}

		[DefaultValue (0)]
		public int DynamicVerticalOffset {
			get {
				object o = ViewState ["DynamicVerticalOffset"];
				if (o != null) return (int)o;
				return 0;
			}
			set {
				ViewState["DynamicVerticalOffset"] = value;
			}
		}

		[DefaultValue (0)]
		public int DynamicHorizontalOffset {
			get {
				object o = ViewState ["DynamicHorizontalOffset"];
				if (o != null) return (int)o;
				return 0;
			}
			set {
				ViewState["DynamicHorizontalOffset"] = value;
			}
		}

		[DefaultValue (true)]
		public bool DynamicEnableDefaultPopOutImage {
			get {
				object o = ViewState ["dedpoi"];
				if (o != null) return (bool)o;
				return true;
			}
			set {
				ViewState["dedpoi"] = value;
			}
		}

		[DefaultValue (true)]
		public bool StaticEnableDefaultPopOutImage {
			get {
				object o = ViewState ["sedpoi"];
				if (o != null) return (bool)o;
				return true;
			}
			set {
				ViewState["sedpoi"] = value;
			}
		}

		[DefaultValueAttribute (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Editor ("System.Web.UI.Design.MenuItemCollectionEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[MergablePropertyAttribute (false)]
		public MenuItemCollection Items {
			get {
				if (items == null) {
					items = new MenuItemCollection (this);
					if (IsTrackingViewState)
						((IStateManager)items).TrackViewState();
				}
				return items;
			}
		}

		[DefaultValue ('/')]
		public char PathSeparator {
			get {
				object o = ViewState ["PathSeparator"];
				if(o != null) return (char)o;
				return '/';
			}
			set {
				ViewState ["PathSeparator"] = value;
			}
		}

		[DefaultValue (false)]
		public bool ItemWrap {
			get {
				object o = ViewState ["ItemWrap"];
				if(o != null) return (bool)o;
				return false;
			}
			set {
				ViewState ["ItemWrap"] = value;
			}
		}

		Style PopOutBoxStyle {
			get {
				if (popOutBoxStyle == null) {
					popOutBoxStyle = new Style ();
					popOutBoxStyle.BackColor = Color.White;
				}
				return popOutBoxStyle;
			}
		}

		Style ControlLinkStyle {
			get {
				if (controlLinkStyle == null) {
					controlLinkStyle = new Style ();
					controlLinkStyle.AlwaysRenderTextDecoration = true;
				}
				return controlLinkStyle;
			}
		}

		Style DynamicMenuItemLinkStyle {
			get {
				if (dynamicMenuItemLinkStyle == null) {
					dynamicMenuItemLinkStyle = new Style ();
				}
				return dynamicMenuItemLinkStyle;
			}
		}

		Style StaticMenuItemLinkStyle {
			get {
				if (staticMenuItemLinkStyle == null) {
					staticMenuItemLinkStyle = new Style ();
				}
				return staticMenuItemLinkStyle;
			}
		}

		Style DynamicSelectedLinkStyle {
			get {
				if (dynamicSelectedLinkStyle == null) {
					dynamicSelectedLinkStyle = new Style ();
				}
				return dynamicSelectedLinkStyle;
			}
		}

		Style StaticSelectedLinkStyle {
			get {
				if (staticSelectedLinkStyle == null) {
					staticSelectedLinkStyle = new Style ();
				}
				return staticSelectedLinkStyle;
			}
		}

		Style DynamicHoverLinkStyle {
			get {
				if (dynamicHoverLinkStyle == null) {
					dynamicHoverLinkStyle = new Style ();
				}
				return dynamicHoverLinkStyle;
			}
		}

		Style StaticHoverLinkStyle {
			get {
				if (staticHoverLinkStyle == null) {
					staticHoverLinkStyle = new Style ();
				}
				return staticHoverLinkStyle;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public MenuItemStyle DynamicMenuItemStyle {
			get {
				if (dynamicMenuItemStyle == null) {
					dynamicMenuItemStyle = new MenuItemStyle ();
					if (IsTrackingViewState)
						dynamicMenuItemStyle.TrackViewState();
				}
				return dynamicMenuItemStyle;
			}
		}
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public MenuItemStyle DynamicSelectedStyle {
			get {
				if (dynamicSelectedStyle == null) {
					dynamicSelectedStyle = new MenuItemStyle ();
					if (IsTrackingViewState)
						dynamicSelectedStyle.TrackViewState();
				}
				return dynamicSelectedStyle;
			}
		}
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public SubMenuStyle DynamicMenuStyle {
			get {
				if (dynamicMenuStyle == null) {
					dynamicMenuStyle = new SubMenuStyle ();
					if (IsTrackingViewState)
						dynamicMenuStyle.TrackViewState();
				}
				return dynamicMenuStyle;
			}
		}
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public MenuItemStyle StaticMenuItemStyle {
			get {
				if (staticMenuItemStyle == null) {
					staticMenuItemStyle = new MenuItemStyle ();
					if (IsTrackingViewState)
						staticMenuItemStyle.TrackViewState();
				}
				return staticMenuItemStyle;
			}
		}
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public MenuItemStyle StaticSelectedStyle {
			get {
				if (staticSelectedStyle == null) {
					staticSelectedStyle = new MenuItemStyle ();
					if (IsTrackingViewState)
						staticSelectedStyle.TrackViewState();
				}
				return staticSelectedStyle;
			}
		}
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public SubMenuStyle StaticMenuStyle {
			get {
				if (staticMenuStyle == null) {
					staticMenuStyle = new SubMenuStyle ();
					if (IsTrackingViewState)
						staticMenuStyle.TrackViewState();
				}
				return staticMenuStyle;
			}
		}

		[DefaultValue (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Editor ("System.Web.UI.Design.WebControls.MenuItemStyleCollectionEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public MenuItemStyleCollection LevelMenuItemStyles {
			get {
				if (levelMenuItemStyles == null) {
					levelMenuItemStyles = new MenuItemStyleCollection ();
					if (IsTrackingViewState)
						((IStateManager)levelMenuItemStyles).TrackViewState();
				}
				return levelMenuItemStyles;
			}
		}

		[DefaultValue (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Editor ("System.Web.UI.Design.WebControls.MenuItemStyleCollectionEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public MenuItemStyleCollection LevelSelectedStyles {
			get {
				if (levelSelectedStyles == null) {
					levelSelectedStyles = new MenuItemStyleCollection ();
					if (IsTrackingViewState)
						((IStateManager)levelSelectedStyles).TrackViewState();
				}
				return levelSelectedStyles;
			}
		}

		[DefaultValue (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Editor ("System.Web.UI.Design.WebControls.SubMenuStyleCollectionEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public SubMenuStyleCollection LevelSubMenuStyles {
			get {
				if (levelSubMenuStyles == null) {
					levelSubMenuStyles = new SubMenuStyleCollection ();
					if (IsTrackingViewState)
						((IStateManager)levelSubMenuStyles).TrackViewState();
				}
				return levelSubMenuStyles;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public Style DynamicHoverStyle {
			get {
				if (dynamicHoverStyle == null) {
					dynamicHoverStyle = new Style ();
					if (IsTrackingViewState)
						dynamicHoverStyle.TrackViewState();
				}
				return dynamicHoverStyle;
			}
		}
		
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public Style StaticHoverStyle {
			get {
				if (staticHoverStyle == null) {
					staticHoverStyle = new Style ();
					if (IsTrackingViewState)
						staticHoverStyle.TrackViewState();
				}
				return staticHoverStyle;
			}
		}
		
		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string ScrollDownImageUrl {
			get {
				object o = ViewState ["sdiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["sdiu"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string ScrollUpImageUrl {
			get {
				object o = ViewState ["suiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["suiu"] = value;
			}
		}

		[Localizable (true)]
		public string ScrollDownText {
			get {
				object o = ViewState ["ScrollDownText"];
				if (o != null) return (string) o;
				return Locale.GetText ("Scroll down");
			}
			set {
				ViewState["ScrollDownText"] = value;
			}
		}

		[Localizable (true)]
		public string ScrollUpText {
			get {
				object o = ViewState ["ScrollUpText"];
				if (o != null) return (string) o;
				return Locale.GetText ("Scroll up");
			}
			set {
				ViewState["ScrollUpText"] = value;
			}
		}

		public string DynamicPopOutImageTextFormatString 
		{
			get
			{
				object o = ViewState ["dpoitf"];
				if (o != null) return (string) o;
				return Locale.GetText ("Expand {0}");
			}
			set
			{
				ViewState ["dpoitf"] = value;
			}
		}
		

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string DynamicPopOutImageUrl {
			get {
				object o = ViewState ["dpoiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["dpoiu"] = value;
			}
		}

		public string StaticPopOutImageTextFormatString
		{
			get
			{
				object o = ViewState ["spoitf"];
				if (o != null) return (string) o;
				return Locale.GetText ("Expand {0}");
			}
			set
			{
				ViewState ["spoitf"] = value;
			}
		}


		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string StaticPopOutImageUrl {
			get {
				object o = ViewState ["spoiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["spoiu"] = value;
			}
		}

		[DefaultValue ("")]
		public string Target {
			get {
				object o = ViewState ["Target"];
				if (o != null) return (string) o;
				return "";
			}
			set {
				ViewState["Target"] = value;
			}
		}

		[DefaultValue (null)]
		[TemplateContainer (typeof(MenuItemTemplateContainer), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ITemplate StaticItemTemplate {
			get { return staticItemTemplate; }
			set { staticItemTemplate = value; }
		}
		
		[DefaultValue (null)]
		[TemplateContainer (typeof(MenuItemTemplateContainer), BindingDirection.OneWay)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Browsable (false)]
		public ITemplate DynamicItemTemplate {
			get { return dynamicItemTemplate; }
			set { dynamicItemTemplate = value; }
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public MenuItem SelectedItem {
			get {
				if (selectedItem == null && selectedItemPath != null) {
					selectedItem = FindItemByPos (selectedItemPath);
				}
				
				return selectedItem;
			}
		}

		[Browsable (false)]
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public string SelectedValue {
			get { return selectedItem != null ? selectedItem.Value : ""; }
		}

		[Localizable (true)]
		public string SkipLinkText 
		{
			get {
				object o = ViewState ["SkipLinkText"];
				if (o != null)
					return (string) o;
				return "Skip Navigation Links";
			}
			set {
				ViewState ["SkipLinkText"] = value;
			}
		}
		

		internal void SetSelectedItem (MenuItem item)
		{
			if (selectedItem == item) return;
			selectedItem = item;
			selectedItemPath = item.Path;
		}
		
		public MenuItem FindItem (string valuePath)
		{
			if (valuePath == null) throw new ArgumentNullException ("valuePath");
			string[] path = valuePath.Split (PathSeparator);
			int n = 0;
			MenuItemCollection col = Items;
			bool foundBranch = true;
			while (col.Count > 0 && foundBranch) {
				foundBranch = false;
				foreach (MenuItem item in col) {
					if (item.Value == path [n]) {
						if (++n == path.Length) return item;
						col = item.ChildItems;
						foundBranch = true;
						break;
					}
				}
			}
			return null;
		}
		
		string GetBindingKey (string dataMember, int depth)
		{
			return dataMember + " " + depth;
		}
		
		internal MenuItemBinding FindBindingForItem (string type, int depth)
		{
			if (bindings == null) return null;

			MenuItemBinding bin = (MenuItemBinding) bindings [GetBindingKey (type, depth)];
			if (bin != null) return bin;
			
			bin = (MenuItemBinding) bindings [GetBindingKey (type, -1)];
			if (bin != null) return bin;
			
			bin = (MenuItemBinding) bindings [GetBindingKey ("", depth)];
			if (bin != null) return bin;
			
			bin = (MenuItemBinding) bindings [GetBindingKey ("", -1)];
			return bin;
		}
		
		protected internal override void PerformDataBinding ()
		{
			base.PerformDataBinding ();

			// Do not attempt to bind data if there is no
			// data source set.
			if (!IsBoundUsingDataSourceID && (DataSource == null)) {
				EnsureChildControlsDataBound ();
				return;
			}

			InitializeDataBindings ();

			HierarchicalDataSourceView data = GetData ("");

			if (data == null) {
				throw new InvalidOperationException ("No view returned by data source control.");
			}
			Items.Clear ();
			IHierarchicalEnumerable e = data.Select ();
			FillBoundChildrenRecursive (e, Items);

			CreateChildControlsForItems ();
			ChildControlsCreated = true;

			EnsureChildControlsDataBound ();
		}

		void FillBoundChildrenRecursive (IHierarchicalEnumerable hEnumerable, MenuItemCollection itemCollection)
		{
			if (hEnumerable == null)
				return;
			foreach (object obj in hEnumerable) {
				IHierarchyData hdata = hEnumerable.GetHierarchyData (obj);
				MenuItem item = new MenuItem ();
				itemCollection.Add (item);
				item.Bind (hdata);

				SiteMapNode siteMapNode = hdata as SiteMapNode;
				if (siteMapNode != null) {
					if (_currSiteMapNode == null)
						_currSiteMapNode = siteMapNode.Provider.CurrentNode;
					if (siteMapNode == _currSiteMapNode)
						item.Selected = true;
				}
				
				OnMenuItemDataBound (new MenuEventArgs (item));

				if (hdata == null || !hdata.HasChildren)
					continue;

				IHierarchicalEnumerable e = hdata.GetChildren ();
				FillBoundChildrenRecursive (e, item.ChildItems);
			}
		}
		
		protected void SetItemDataBound (MenuItem node, bool dataBound)
		{
			node.SetDataBound (dataBound);
		}
		
		protected void SetItemDataPath (MenuItem node, string dataPath)
		{
			node.SetDataPath (dataPath);
		}
		
		protected void SetItemDataItem (MenuItem node, object dataItem)
		{
			node.SetDataItem (dataItem);
		}
		
		protected internal virtual void RaisePostBackEvent (string eventArgument)
		{
			ValidateEvent (UniqueID, eventArgument);
			if (!IsEnabled)
				return;

			EnsureChildControls();
			MenuItem item = FindItemByPos (eventArgument);
			if (item == null) return;
			item.Selected = true;
			OnMenuItemClick (new MenuEventArgs (item));
		}

		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			RaisePostBackEvent (eventArgument);
		}
		
		MenuItem FindItemByPos (string path)
		{
			string[] indexes = path.Split ('_');
			MenuItem item = null;
			
			foreach (string index in indexes) {
				int i = int.Parse (index);
				if (item == null) {
					if (i >= Items.Count) return null;
					item = Items [i];
				} else {
					if (i >= item.ChildItems.Count) return null;
					item = item.ChildItems [i];
				}
			}
			return item;
		}
		
		protected override HtmlTextWriterTag TagKey {
			get { return HtmlTextWriterTag.Table; }
		}
		
		protected override void TrackViewState()
		{
			EnsureDataBound ();
			
			base.TrackViewState();
			if (dataBindings != null) {
				((IStateManager)dataBindings).TrackViewState ();
			}
			if (items != null) {
				((IStateManager)items).TrackViewState();
			}
			if (dynamicMenuItemStyle != null)
				dynamicMenuItemStyle.TrackViewState ();
			if (dynamicMenuStyle != null)
				dynamicMenuStyle.TrackViewState ();
			if (levelMenuItemStyles != null && levelMenuItemStyles.Count > 0)
				((IStateManager)levelMenuItemStyles).TrackViewState();
			if (levelSelectedStyles != null && levelMenuItemStyles.Count > 0)
				((IStateManager)levelSelectedStyles).TrackViewState();
			if (levelSubMenuStyles != null && levelSubMenuStyles.Count > 0)
				((IStateManager)levelSubMenuStyles).TrackViewState();
			if (dynamicSelectedStyle != null)
				dynamicSelectedStyle.TrackViewState();
			if (staticMenuItemStyle != null)
				staticMenuItemStyle.TrackViewState ();
			if (staticMenuStyle != null)
				staticMenuStyle.TrackViewState ();
			if (staticSelectedStyle != null)
				staticSelectedStyle.TrackViewState();
			if (staticHoverStyle != null)
				staticHoverStyle.TrackViewState();
			if (dynamicHoverStyle != null)
				dynamicHoverStyle.TrackViewState();
		}

		protected override object SaveViewState()
		{
			object[] states = new object [14];
			states[0] = base.SaveViewState ();
			states[1] = dataBindings == null ? null : ((IStateManager)dataBindings).SaveViewState();
			states[2] = items == null ? null : ((IStateManager)items).SaveViewState();
			states[3] = dynamicMenuItemStyle == null ? null : dynamicMenuItemStyle.SaveViewState();
			states[4] = dynamicMenuStyle == null ? null : dynamicMenuStyle.SaveViewState();
			states[5] = levelMenuItemStyles == null ? null : ((IStateManager)levelMenuItemStyles).SaveViewState();
			states[6] = levelSelectedStyles == null ? null : ((IStateManager)levelSelectedStyles).SaveViewState();
			states[7] = dynamicSelectedStyle == null ? null : dynamicSelectedStyle.SaveViewState();
			states[8] = (staticMenuItemStyle == null ? null : staticMenuItemStyle.SaveViewState());
			states[9] = staticMenuStyle == null ? null : staticMenuStyle.SaveViewState();
			states[10] = staticSelectedStyle == null ? null : staticSelectedStyle.SaveViewState();
			states[11] = staticHoverStyle == null ? null : staticHoverStyle.SaveViewState();
			states[12] = dynamicHoverStyle == null ? null : dynamicHoverStyle.SaveViewState();
			states[13] = levelSubMenuStyles == null ? null : ((IStateManager)levelSubMenuStyles).SaveViewState();

			for (int i = states.Length - 1; i >= 0; i--) {
				if (states [i] != null)
					return states;
			}

			return null;
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState == null)
				return;

			object [] states = (object []) savedState;
			base.LoadViewState (states[0]);
			
			if (states[1] != null)
				((IStateManager)DataBindings).LoadViewState(states[1]);
			if (states[2] != null)
				((IStateManager)Items).LoadViewState(states[2]);
			if (states[3] != null)
				DynamicMenuItemStyle.LoadViewState (states[3]);
			if (states[4] != null)
				DynamicMenuStyle.LoadViewState (states[4]);
			if (states[5] != null)
				((IStateManager)LevelMenuItemStyles).LoadViewState(states[5]);
			if (states[6] != null)
				((IStateManager)LevelSelectedStyles).LoadViewState(states[6]);
			if (states[7] != null)
				DynamicSelectedStyle.LoadViewState (states[7]);
			if (states[8] != null)
				StaticMenuItemStyle.LoadViewState (states[8]);
			if (states[9] != null)
				StaticMenuStyle.LoadViewState (states[9]);
			if (states[10] != null)
				StaticSelectedStyle.LoadViewState (states[10]);
			if (states[11] != null)
				StaticHoverStyle.LoadViewState (states[11]);
			if (states[12] != null)
				DynamicHoverStyle.LoadViewState (states[12]);
			if (states[13] != null)
				((IStateManager)LevelSubMenuStyles).LoadViewState(states[13]);
		}
		
		protected internal override void OnInit (EventArgs e)
		{
			Page.RegisterRequiresControlState (this);
			base.OnInit (e);
		}
		
		protected internal override void LoadControlState (object ob)
		{
			if (ob == null) return;
			object[] state = (object[]) ob;
			base.LoadControlState (state[0]);
			selectedItemPath = state[1] as string;
		}
		
		protected internal override object SaveControlState ()
		{
			object bstate = base.SaveControlState ();
			object mstate = selectedItemPath;
			
			if (bstate != null || mstate != null)
				return new object[] { bstate, mstate };
			else
				return null;
		}
		
		protected internal override void CreateChildControls ()
		{
			if (!IsBoundUsingDataSourceID && (DataSource == null)) {
				CreateChildControlsForItems ();
			}
			else {
				EnsureDataBound ();
			}
		}

		void CreateChildControlsForItems () {
			Controls.Clear ();
			// Check for HasChildViewState to avoid unnecessary calls to ClearChildViewState.
			if (HasChildViewState)
				ClearChildViewState ();
			_menuItemControls = new Hashtable ();
			CreateChildControlsForItems (Items);
			_requiresChildControlsDataBinding = true;
		}

		void CreateChildControlsForItems (MenuItemCollection items ) {
			foreach (MenuItem item in items) {
				bool isDynamicItem = IsDynamicItem (item);
				if (isDynamicItem && dynamicItemTemplate != null) {
					MenuItemTemplateContainer cter = new MenuItemTemplateContainer (item.Index, item);
					dynamicItemTemplate.InstantiateIn (cter);
					_menuItemControls [item] = cter;
					Controls.Add (cter);
				}
				else if (!isDynamicItem && staticItemTemplate != null) {
					MenuItemTemplateContainer cter = new MenuItemTemplateContainer (item.Index, item);
					staticItemTemplate.InstantiateIn (cter);
					_menuItemControls [item] = cter;
					Controls.Add (cter);
				}
				if (item.HasChildData)
					CreateChildControlsForItems (item.ChildItems);
			}
		}

		protected override void EnsureDataBound ()
		{
			base.EnsureDataBound ();
			
			EnsureChildControlsDataBound ();
		}

		void EnsureChildControlsDataBound () {
			if (!_requiresChildControlsDataBinding)
				return;
			DataBindChildren ();
			_requiresChildControlsDataBinding = false;
		}

		[MonoTODO ("Not implemented")]
		protected override IDictionary GetDesignModeState ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		protected override void SetDesignModeState (IDictionary data)
		{
			throw new NotImplementedException ();
		}
				
		public override ControlCollection Controls {
			get { return base.Controls; }
		}
		
		public sealed override void DataBind ()
		{
			base.DataBind ();
		}
		
		protected override bool OnBubbleEvent (object source, EventArgs args)
		{
			if (!(args is CommandEventArgs))
				return false;

			MenuEventArgs menuArgs = args as MenuEventArgs;
			if (menuArgs != null && string.Equals (menuArgs.CommandName, MenuItemClickCommandName))
				OnMenuItemClick (menuArgs);
			return true;
		}

		protected override void OnDataBinding (EventArgs e)
		{
			EnsureChildControls ();
			base.OnDataBinding (e);
		}

		const string onPreRenderScript = "var {0} = new Object ();\n{0}.webForm = {1};\n{0}.disappearAfter = {2};\n{0}.vertical = {3};";
		
		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			
			if (!Page.ClientScript.IsClientScriptIncludeRegistered (typeof(Menu), "Menu.js")) {
				string url = Page.ClientScript.GetWebResourceUrl (typeof(Menu), "Menu.js");
				Page.ClientScript.RegisterClientScriptInclude (typeof(Menu), "Menu.js", url);
			}
			
			string cmenu = ClientID + "_data";
			string script = String.Format (onPreRenderScript,
						       cmenu,
						       Page.IsMultiForm ? Page.theForm : "window",
						       ClientScriptManager.GetScriptLiteral (DisappearAfter),
						       ClientScriptManager.GetScriptLiteral (Orientation == Orientation.Vertical));			

			if (DynamicHorizontalOffset != 0)
				script += String.Concat (cmenu, ".dho = ", ClientScriptManager.GetScriptLiteral (DynamicHorizontalOffset), ";\n");
			if (DynamicVerticalOffset != 0)
				script += String.Concat (cmenu, ".dvo = ", ClientScriptManager.GetScriptLiteral (DynamicVerticalOffset), ";\n");
			
			// The order in which styles are defined matters when more than one class
			// is assigned to an element
			RegisterStyle (PopOutBoxStyle);
			RegisterStyle (ControlStyle, ControlLinkStyle);
			
			if (staticMenuItemStyle != null)
				RegisterStyle (StaticMenuItemStyle, StaticMenuItemLinkStyle);

			if (staticMenuStyle != null)
				RegisterStyle (StaticMenuStyle);
			
			if (dynamicMenuItemStyle != null)
				RegisterStyle (DynamicMenuItemStyle, DynamicMenuItemLinkStyle);

			if (dynamicMenuStyle != null)
				RegisterStyle (DynamicMenuStyle);

			if (levelMenuItemStyles != null && levelMenuItemStyles.Count > 0) {
				levelMenuItemLinkStyles = new List<Style> (levelMenuItemStyles.Count);
				foreach (Style style in levelMenuItemStyles) {
					Style linkStyle = new Style ();
					levelMenuItemLinkStyles.Add (linkStyle);
					RegisterStyle (style, linkStyle);
				}
			}
		
			if (levelSubMenuStyles != null)
				foreach (Style style in levelSubMenuStyles)
					RegisterStyle (style);

			if (staticSelectedStyle != null)
				RegisterStyle (staticSelectedStyle, StaticSelectedLinkStyle);
			
			if (dynamicSelectedStyle != null)
				RegisterStyle (dynamicSelectedStyle, DynamicSelectedLinkStyle);

			if (levelSelectedStyles != null && levelSelectedStyles.Count > 0) {
				levelSelectedLinkStyles = new List<Style> (levelSelectedStyles.Count);
				foreach (Style style in levelSelectedStyles) {
					Style linkStyle = new Style ();
					levelSelectedLinkStyles.Add (linkStyle);
					RegisterStyle (style, linkStyle);
				}
			}
			
			if (staticHoverStyle != null) {
				if (Page.Header == null)
					throw new InvalidOperationException ("Using Menu.StaticHoverStyle requires Page.Header to be non-null (e.g. <head runat=\"server\" />).");
				RegisterStyle (staticHoverStyle, StaticHoverLinkStyle);
				script += string.Concat (cmenu, ".staticHover = ", ClientScriptManager.GetScriptLiteral (staticHoverStyle.RegisteredCssClass), ";\n");
				script += string.Concat (cmenu, ".staticLinkHover = ", ClientScriptManager.GetScriptLiteral (StaticHoverLinkStyle.RegisteredCssClass), ";\n");
			}
			
			if (dynamicHoverStyle != null) {
				if (Page.Header == null)
					throw new InvalidOperationException ("Using Menu.DynamicHoverStyle requires Page.Header to be non-null (e.g. <head runat=\"server\" />).");
				RegisterStyle (dynamicHoverStyle, DynamicHoverLinkStyle);
				script += string.Concat (cmenu, ".dynamicHover = ", ClientScriptManager.GetScriptLiteral (dynamicHoverStyle.RegisteredCssClass), ";\n");
				script += string.Concat (cmenu, ".dynamicLinkHover = ", ClientScriptManager.GetScriptLiteral (DynamicHoverLinkStyle.RegisteredCssClass), ";\n");
			}

			Page.ClientScript.RegisterWebFormClientScript ();
			Page.ClientScript.RegisterStartupScript (typeof(Menu), ClientID, script, true);

		}

		void InitializeDataBindings () {
			if (dataBindings != null && dataBindings.Count > 0) {
				bindings = new Hashtable ();
				foreach (MenuItemBinding bin in dataBindings) {
					string key = GetBindingKey (bin.DataMember, bin.Depth);
					bindings [key] = bin;
				}
			}
			else
				bindings = null;
		}

		string IncrementStyleClassName () {
			registeredStylesCounter++;
			return ClientID + "_" + registeredStylesCounter;
		}

		void RegisterStyle (Style baseStyle, Style linkStyle) {
			linkStyle.CopyTextStylesFrom (baseStyle);
			linkStyle.BorderStyle = BorderStyle.None;
			RegisterStyle (linkStyle);
			RegisterStyle (baseStyle);
		}

		void RegisterStyle (Style baseStyle)
		{
			if (Page.Header == null)
				return;
			string className = IncrementStyleClassName ();
			baseStyle.SetRegisteredCssClass (className);
			Page.Header.StyleSheet.CreateStyleRule (baseStyle, this, "." + className);
		}
		
		protected internal override void Render (HtmlTextWriter writer)
		{
			if (Items.Count > 0)
				base.Render (writer);
		}
		
		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			writer.AddAttribute ("cellpadding", "0", false);
			writer.AddAttribute ("cellspacing", "0", false);
			writer.AddAttribute ("border", "0", false);
			if (Page.Header != null) {
				// styles are registered
				if (staticMenuStyle != null) {
					AddCssClass (ControlStyle, staticMenuStyle.CssClass);
					AddCssClass (ControlStyle, staticMenuStyle.RegisteredCssClass);
				}
				if (levelSubMenuStyles != null && levelSubMenuStyles.Count > 0) {
					AddCssClass (ControlStyle, levelSubMenuStyles [0].CssClass);
					AddCssClass (ControlStyle, levelSubMenuStyles [0].RegisteredCssClass);
				}
			}
			else {
				// styles are not registered
				if (staticMenuStyle != null){
					ControlStyle.CopyFrom (staticMenuStyle);
				}
				if (levelSubMenuStyles != null && levelSubMenuStyles.Count > 0) {
					ControlStyle.CopyFrom (levelSubMenuStyles [0]);
				}
			}
			base.AddAttributesToRender (writer);
		}

		void AddCssClass (Style style, string cssClass) {
			style.AddCssClass (cssClass);
		}
		
		public override void RenderBeginTag (HtmlTextWriter writer)
		{
			string skipLinkText = SkipLinkText;
			if (!String.IsNullOrEmpty (skipLinkText)) {
				// <a href="#ID_SkipLink">
				writer.AddAttribute (HtmlTextWriterAttribute.Href, "#" + ClientID + "_SkipLink");
				writer.RenderBeginTag (HtmlTextWriterTag.A);
				
				// <img alt="" height="0" width="0" src="" style="border-width:0px;"/>
				writer.AddAttribute (HtmlTextWriterAttribute.Alt, skipLinkText);
				writer.AddAttribute (HtmlTextWriterAttribute.Height, "0");
				writer.AddAttribute (HtmlTextWriterAttribute.Width, "0");
				
				Page page = Page;
				ClientScriptManager csm;
				
				if (page != null)
					csm = page.ClientScript;
				else
					csm = new ClientScriptManager (null);
				writer.AddAttribute (HtmlTextWriterAttribute.Src, csm.GetWebResourceUrl (typeof (SiteMapPath), "transparent.gif"));
				writer.AddStyleAttribute (HtmlTextWriterStyle.BorderWidth, "0px");
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();
				
				writer.RenderEndTag (); // </a>
			}
			base.RenderBeginTag (writer);
		}
		
		public override void RenderEndTag (HtmlTextWriter writer)
		{
			base.RenderEndTag (writer);

			if (StaticDisplayLevels == 1 && MaximumDynamicDisplayLevels > 0)
				RenderDynamicMenu (writer, Items);

			string skipLinkText = SkipLinkText;
			if (!String.IsNullOrEmpty (skipLinkText)) {
				writer.AddAttribute (HtmlTextWriterAttribute.Id, "SkipLink");
				writer.RenderBeginTag (HtmlTextWriterTag.A);
				writer.RenderEndTag ();
			}
		}
		
		protected internal override void RenderContents (HtmlTextWriter writer)
		{
			RenderMenuBody (writer, Items, Orientation == Orientation.Vertical, false, false);
		}

		void RenderDynamicMenu (HtmlTextWriter writer, MenuItemCollection items) {
			for (int n = 0; n < items.Count; n++) {
				if (DisplayChildren (items [n])) {
					RenderDynamicMenu (writer, items [n]);
					RenderDynamicMenu (writer, items [n].ChildItems);
				}
			}
		}
		
		MenuRenderHtmlTemplate _dynamicTemplate;
		MenuRenderHtmlTemplate GetDynamicMenuTemplate (MenuItem item)
		{
			if (_dynamicTemplate != null) 
				return _dynamicTemplate;

			_dynamicTemplate = new MenuRenderHtmlTemplate ();
			HtmlTextWriter writer = _dynamicTemplate.GetMenuTemplateWriter ();

			if (Page.Header != null) {
				writer.AddAttribute (HtmlTextWriterAttribute.Class, MenuRenderHtmlTemplate.GetMarker (0));
			}
			else {
				writer.AddAttribute (HtmlTextWriterAttribute.Style, MenuRenderHtmlTemplate.GetMarker (0));
			}

			writer.AddStyleAttribute ("visibility", "hidden");
			writer.AddStyleAttribute ("position", "absolute");
			writer.AddStyleAttribute ("z-index", "1");
			writer.AddStyleAttribute ("left", "0px");
			writer.AddStyleAttribute ("top", "0px");
			writer.AddAttribute ("id", MenuRenderHtmlTemplate.GetMarker (1));
			writer.RenderBeginTag (HtmlTextWriterTag.Div);

			// Up button
			writer.AddAttribute ("id", MenuRenderHtmlTemplate.GetMarker (2));
			writer.AddStyleAttribute ("display", "block");
			writer.AddStyleAttribute ("text-align", "center");
			writer.AddAttribute ("onmouseover", string.Concat ("Menu_OverScrollBtn ('", ClientID, "','", MenuRenderHtmlTemplate.GetMarker (3), "','u')"));
			writer.AddAttribute ("onmouseout", string.Concat ("Menu_OutScrollBtn ('", ClientID, "','", MenuRenderHtmlTemplate.GetMarker (4), "','u')")); 
			writer.RenderBeginTag (HtmlTextWriterTag.Div);
			
			writer.AddAttribute ("src", MenuRenderHtmlTemplate.GetMarker (5)); //src
			writer.AddAttribute ("alt", MenuRenderHtmlTemplate.GetMarker (6)); //ScrollUpText
			writer.RenderBeginTag (HtmlTextWriterTag.Img);
			writer.RenderEndTag ();	// IMG
			
			writer.RenderEndTag ();	// DIV scroll button
		
			writer.AddAttribute ("id", MenuRenderHtmlTemplate.GetMarker (7));
			writer.RenderBeginTag (HtmlTextWriterTag.Div);
			writer.AddAttribute ("id", MenuRenderHtmlTemplate.GetMarker (8));
			writer.RenderBeginTag (HtmlTextWriterTag.Div);
			
			// call of RenderMenu
			writer.Write (MenuRenderHtmlTemplate.GetMarker (9));
			
			writer.RenderEndTag ();	// DIV Content
			writer.RenderEndTag ();	// DIV Scroll container

			// Down button
			writer.AddAttribute ("id", MenuRenderHtmlTemplate.GetMarker (0));
			writer.AddStyleAttribute ("display", "block");
			writer.AddStyleAttribute ("text-align", "center");
			writer.AddAttribute ("onmouseover", string.Concat ("Menu_OverScrollBtn ('", ClientID, "','", MenuRenderHtmlTemplate.GetMarker (1), "','d')"));
			writer.AddAttribute ("onmouseout", string.Concat ("Menu_OutScrollBtn ('", ClientID, "','", MenuRenderHtmlTemplate.GetMarker (2), "','d')")); 
			writer.RenderBeginTag (HtmlTextWriterTag.Div);
			
			writer.AddAttribute ("src", MenuRenderHtmlTemplate.GetMarker (3)); //src
			writer.AddAttribute ("alt", MenuRenderHtmlTemplate.GetMarker (4)); //ScrollDownText
			writer.RenderBeginTag (HtmlTextWriterTag.Img);
			writer.RenderEndTag ();	// IMG
			
			writer.RenderEndTag ();	// DIV scroll button
			
			writer.RenderEndTag ();	// DIV menu

			_dynamicTemplate.Parse ();
			return _dynamicTemplate;
		}

		void RenderDynamicMenu (HtmlTextWriter writer, MenuItem item)
		{
			_dynamicTemplate = GetDynamicMenuTemplate (item);

			string idPrefix = ClientID + "_" + item.Path;
			string [] param = new string [9];
			param [0] = GetCssMenuStyle (true, item.Depth + 1);
			param [1] = idPrefix + "s";
			param [2] = idPrefix + "cu";
			param [3] = item.Path;
			param [4] = item.Path;
			param [5] = ScrollUpImageUrl != "" ? ScrollUpImageUrl : Page.ClientScript.GetWebResourceUrl (typeof (Menu), "arrow_up.gif");
			param [6] = ScrollUpText;
			param [7] = idPrefix + "cb";
			param [8] = idPrefix + "cc";

			_dynamicTemplate.RenderTemplate (writer, param, 0, param.Length);

			RenderMenu (writer, item.ChildItems, true, true, item.Depth + 1, false);

			string [] param2 = new string [5];
			param2 [0] = idPrefix + "cd";
			param2 [1] = item.Path;
			param2 [2] = item.Path;
			param2 [3] = ScrollDownImageUrl != "" ? ScrollDownImageUrl : Page.ClientScript.GetWebResourceUrl (typeof (Menu), "arrow_down.gif");
			param2 [4] = ScrollDownText;

			_dynamicTemplate.RenderTemplate (writer, param2, param.Length + 1, param2.Length);

		}

		string GetCssMenuStyle (bool dynamic, int menuLevel)
		{
			if (Page.Header != null) {
				// styles are registered
				StringBuilder sb = new StringBuilder ();

				if (!dynamic && staticMenuStyle != null) {
					sb.Append (staticMenuStyle.CssClass);
					sb.Append (' ');
					sb.Append (staticMenuStyle.RegisteredCssClass);
				}
				if (dynamic && dynamicMenuStyle != null) {
					sb.Append (PopOutBoxStyle.RegisteredCssClass);
					sb.Append (' ');
					sb.Append (dynamicMenuStyle.CssClass);
					sb.Append (' ');
					sb.Append (dynamicMenuStyle.RegisteredCssClass);
				}
				if (levelSubMenuStyles != null && levelSubMenuStyles.Count > menuLevel) {
					sb.Append (levelSubMenuStyles [menuLevel].CssClass);
					sb.Append (' ');
					sb.Append (levelSubMenuStyles [menuLevel].RegisteredCssClass); 
				}
				return sb.ToString ();
			}
			else {
				// styles are not registered
				SubMenuStyle style = new SubMenuStyle ();

				if (!dynamic && staticMenuStyle != null) {
					style.CopyFrom (staticMenuStyle);
				}
				if (dynamic && dynamicMenuStyle != null) {
					style.CopyFrom (PopOutBoxStyle);
					style.CopyFrom (dynamicMenuStyle);
				}
				if (levelSubMenuStyles != null && levelSubMenuStyles.Count > menuLevel) {
					style.CopyFrom (levelSubMenuStyles [menuLevel]);
				}
				return style.GetStyleAttributes (null).Value;
			}
		}

		void RenderMenuBeginTagAttributes (HtmlTextWriter writer, bool dynamic, int menuLevel) {
			writer.AddAttribute ("cellpadding", "0", false);
			writer.AddAttribute ("cellspacing", "0", false);
			writer.AddAttribute ("border", "0", false);

			if (!dynamic) {
				SubMenuStyle style = new SubMenuStyle ();
				FillMenuStyle (dynamic, menuLevel, style);
				style.AddAttributesToRender (writer);
			}
		}

		void FillMenuStyle (bool dynamic, int menuLevel, SubMenuStyle style) {
			if (Page.Header != null) {
				// styles are registered
				if (!dynamic && staticMenuStyle != null) {
					AddCssClass (style, staticMenuStyle.CssClass);
					AddCssClass (style, staticMenuStyle.RegisteredCssClass);
				}
				if (dynamic && dynamicMenuStyle != null) {
					AddCssClass (style, dynamicMenuStyle.CssClass);
					AddCssClass (style, dynamicMenuStyle.RegisteredCssClass);
				}
				if (levelSubMenuStyles != null && levelSubMenuStyles.Count > menuLevel) {
					AddCssClass (style, levelSubMenuStyles [menuLevel].CssClass);
					AddCssClass (style, levelSubMenuStyles [menuLevel].RegisteredCssClass);
				}
			}
			else {
				// styles are not registered
				if (!dynamic && staticMenuStyle != null) {
					style.CopyFrom (staticMenuStyle);
				}
				if (dynamic && dynamicMenuStyle != null) {
					style.CopyFrom (dynamicMenuStyle);
				}
				if (levelSubMenuStyles != null && levelSubMenuStyles.Count > menuLevel) {
					style.CopyFrom (levelSubMenuStyles [menuLevel]);
				}
			}
		}

		void RenderMenu (HtmlTextWriter writer, MenuItemCollection items, bool vertical, bool dynamic, int menuLevel, bool notLast)
		{
			RenderMenuBeginTag (writer, dynamic, menuLevel);
			RenderMenuBody (writer, items, vertical, dynamic, notLast);
			RenderMenuEndTag (writer);
		}
		
		void RenderMenuBeginTag (HtmlTextWriter writer, bool dynamic, int menuLevel)
		{
			RenderMenuBeginTagAttributes (writer, dynamic, menuLevel);
			writer.RenderBeginTag (HtmlTextWriterTag.Table);
		}
		
		void RenderMenuEndTag (HtmlTextWriter writer)
		{
			writer.RenderEndTag ();
		}

		void RenderMenuBody (HtmlTextWriter writer, MenuItemCollection items, bool vertical, bool dynamic, bool notLast) {
			if (!vertical)
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);

			int count = items.Count;
			for (int n = 0; n < count; n++) {
				MenuItem item = items [n];
				Adapters.MenuAdapter adapter = Adapter as Adapters.MenuAdapter;
				if (adapter != null)
					adapter.RenderItem (writer, item, n);
				else
					RenderMenuItem (writer, item, (n + 1 == count) ? notLast : true, n == 0);
			}

			if (!vertical)
				writer.RenderEndTag ();	// TR
		}

		void RenderMenuItemSpacing (HtmlTextWriter writer, Unit itemSpacing, bool vertical) {
			if (vertical) {
				writer.AddStyleAttribute ("height", itemSpacing.ToString ());
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				writer.RenderEndTag ();
				writer.RenderEndTag ();
			}
			else {
				writer.AddStyleAttribute ("width", itemSpacing.ToString ());
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				writer.RenderEndTag ();
			}
		}
		
		bool IsDynamicItem (MenuItem item) {
			return item.Depth + 1 > StaticDisplayLevels;
		}

		bool DisplayChildren (MenuItem item) {
			return (item.Depth + 1 < StaticDisplayLevels + MaximumDynamicDisplayLevels) && item.ChildItems.Count > 0;
		}
		
		internal void RenderItem (HtmlTextWriter writer, MenuItem item, int position) {
			// notLast should be true if item or any of its ancestors is not a
			// last child.
			bool notLast = false;
			MenuItem parent;
			MenuItem child = item;			
			while (null != (parent = child.Parent)) {
				if (child.Index != parent.ChildItems.Count - 1) {
					notLast = true;
					break;
				}
				child = parent;
			}
			
			RenderMenuItem (writer, item, notLast, position == 0);
		}
		
		void RenderMenuItem (HtmlTextWriter writer, MenuItem item, bool notLast, bool isFirst) {
			bool displayChildren = DisplayChildren (item);
			bool dynamicChildren = displayChildren && (item.Depth + 1 >= StaticDisplayLevels);
			bool isDynamicItem = IsDynamicItem (item);
			bool vertical = (Orientation == Orientation.Vertical) || isDynamicItem;
			
			Unit itemSpacing = GetItemSpacing (item, isDynamicItem);

			if (itemSpacing != Unit.Empty && (item.Depth > 0 || !isFirst))
				RenderMenuItemSpacing (writer, itemSpacing, vertical);

			if(!String.IsNullOrEmpty(item.ToolTip))
				writer.AddAttribute (HtmlTextWriterAttribute.Title, item.ToolTip);
			if (vertical)
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);

			string parentId = isDynamicItem ? "'" + item.Parent.Path + "'" : "null";
			if (dynamicChildren) {
				writer.AddAttribute ("onmouseover",
						     "javascript:Menu_OverItem ('" + ClientID + "','" + item.Path + "'," + parentId + ")");
				writer.AddAttribute ("onmouseout",
						     "javascript:Menu_OutItem ('" + ClientID + "','" + item.Path + "')");
			} else if (isDynamicItem) {
				writer.AddAttribute ("onmouseover",
						     "javascript:Menu_OverDynamicLeafItem ('" + ClientID + "','" + item.Path + "'," + parentId + ")");
				writer.AddAttribute ("onmouseout",
						     "javascript:Menu_OutItem ('" + ClientID + "','" + item.Path + "'," + parentId + ")");
			} else {
				writer.AddAttribute ("onmouseover",
						     "javascript:Menu_OverStaticLeafItem ('" + ClientID + "','" + item.Path + "')");
				writer.AddAttribute ("onmouseout",
						     "javascript:Menu_OutItem ('" + ClientID + "','" + item.Path + "')");
			}

			writer.RenderBeginTag (HtmlTextWriterTag.Td);

			// Top separator image

			if (isDynamicItem && DynamicTopSeparatorImageUrl != "") {
				writer.AddAttribute ("src", ResolveClientUrl (DynamicTopSeparatorImageUrl));
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
			}
			else if (!isDynamicItem && StaticTopSeparatorImageUrl != "") {
				writer.AddAttribute ("src", ResolveClientUrl (StaticTopSeparatorImageUrl));
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
			}

			// Menu item box
			
			MenuItemStyle style = new MenuItemStyle ();
			if (Page.Header != null) {
				// styles are registered
				if (!isDynamicItem && staticMenuItemStyle != null) {
					AddCssClass (style, staticMenuItemStyle.CssClass);
					AddCssClass (style, staticMenuItemStyle.RegisteredCssClass);
				}
				if (isDynamicItem && dynamicMenuItemStyle != null) {
					AddCssClass (style, dynamicMenuItemStyle.CssClass);
					AddCssClass (style, dynamicMenuItemStyle.RegisteredCssClass);
				}
				if (levelMenuItemStyles != null && levelMenuItemStyles.Count > item.Depth) {
					AddCssClass (style, levelMenuItemStyles [item.Depth].CssClass);
					AddCssClass (style, levelMenuItemStyles [item.Depth].RegisteredCssClass);
				}
				if (item == SelectedItem) {
					if (!isDynamicItem && staticSelectedStyle != null) {
						AddCssClass (style, staticSelectedStyle.CssClass);
						AddCssClass (style, staticSelectedStyle.RegisteredCssClass);
					}
					if (isDynamicItem && dynamicSelectedStyle != null) {
						AddCssClass (style, dynamicSelectedStyle.CssClass);
						AddCssClass (style, dynamicSelectedStyle.RegisteredCssClass);
					}
					if (levelSelectedStyles != null && levelSelectedStyles.Count > item.Depth) {
						AddCssClass (style, levelSelectedStyles [item.Depth].CssClass);
						AddCssClass (style, levelSelectedStyles [item.Depth].RegisteredCssClass);
					}
				}
			}
			else {
				// styles are not registered
				if (!isDynamicItem && staticMenuItemStyle != null) {
					style.CopyFrom (staticMenuItemStyle);
				}
				if (isDynamicItem && dynamicMenuItemStyle != null) {
					style.CopyFrom (dynamicMenuItemStyle);
				}
				if (levelMenuItemStyles != null && levelMenuItemStyles.Count > item.Depth) {
					style.CopyFrom (levelMenuItemStyles [item.Depth]);
				}
				if (item == SelectedItem) {
					if (!isDynamicItem && staticSelectedStyle != null) {
						style.CopyFrom (staticSelectedStyle);
					}
					if (isDynamicItem && dynamicSelectedStyle != null) {
						style.CopyFrom (dynamicSelectedStyle);
					}
					if (levelSelectedStyles != null && levelSelectedStyles.Count > item.Depth) {
						style.CopyFrom (levelSelectedStyles [item.Depth]);
					}
				}
			}
			style.AddAttributesToRender (writer);

			writer.AddAttribute ("id", GetItemClientId (item, "i"));

			writer.AddAttribute ("cellpadding", "0", false);
			writer.AddAttribute ("cellspacing", "0", false);
			writer.AddAttribute ("border", "0", false);
			writer.AddAttribute ("width", "100%", false);
			writer.RenderBeginTag (HtmlTextWriterTag.Table);
			writer.RenderBeginTag (HtmlTextWriterTag.Tr);

			// Menu item text

			if (vertical)
				writer.AddStyleAttribute (HtmlTextWriterStyle.Width, "100%");
			if (!ItemWrap)
				writer.AddStyleAttribute ("white-space", "nowrap");
			writer.RenderBeginTag (HtmlTextWriterTag.Td);

			RenderItemHref (writer, item);
			
			Style linkStyle = new Style ();
			if (Page.Header != null) {
				// styles are registered
				AddCssClass (linkStyle, ControlLinkStyle.RegisteredCssClass);

				if (!isDynamicItem && staticMenuItemStyle != null) {
					AddCssClass (linkStyle, staticMenuItemStyle.CssClass);
					AddCssClass (linkStyle, staticMenuItemLinkStyle.RegisteredCssClass);
				}
				if (isDynamicItem && dynamicMenuItemStyle != null) {
					AddCssClass (linkStyle, dynamicMenuItemStyle.CssClass);
					AddCssClass (linkStyle, dynamicMenuItemLinkStyle.RegisteredCssClass);
				}
				if (levelMenuItemStyles != null && levelMenuItemStyles.Count > item.Depth) {
					AddCssClass (linkStyle, levelMenuItemStyles [item.Depth].CssClass);
					AddCssClass (linkStyle, levelMenuItemLinkStyles [item.Depth].RegisteredCssClass);
				}
				if (item == SelectedItem) {
					if (!isDynamicItem && staticSelectedStyle != null) {
						AddCssClass (linkStyle, staticSelectedStyle.CssClass);
						AddCssClass (linkStyle, staticSelectedLinkStyle.RegisteredCssClass);
					}
					if (isDynamicItem && dynamicSelectedStyle != null) {
						AddCssClass (linkStyle, dynamicSelectedStyle.CssClass);
						AddCssClass (linkStyle, dynamicSelectedLinkStyle.RegisteredCssClass);
					}
					if (levelSelectedStyles != null && levelSelectedStyles.Count > item.Depth) {
						AddCssClass (linkStyle, levelSelectedStyles [item.Depth].CssClass);
						AddCssClass (linkStyle, levelSelectedLinkStyles [item.Depth].RegisteredCssClass);
					}
				}
			}
			else {
				// styles are not registered
				linkStyle.CopyFrom (ControlLinkStyle);

				if (!isDynamicItem && staticMenuItemStyle != null) {
					linkStyle.CopyFrom (staticMenuItemLinkStyle);
				}
				if (isDynamicItem && dynamicMenuItemStyle != null) {
					linkStyle.CopyFrom (dynamicMenuItemLinkStyle);
				}
				if (levelMenuItemStyles != null && levelMenuItemStyles.Count > item.Depth) {
					linkStyle.CopyFrom (levelMenuItemLinkStyles [item.Depth]);
				}
				if (item == SelectedItem) {
					if (!isDynamicItem && staticSelectedStyle != null) {
						linkStyle.CopyFrom (staticSelectedLinkStyle);
					}
					if (isDynamicItem && dynamicSelectedStyle != null) {
						linkStyle.CopyFrom (dynamicSelectedLinkStyle);
					}
					if (levelSelectedStyles != null && levelSelectedStyles.Count > item.Depth) {
						linkStyle.CopyFrom (levelSelectedLinkStyles [item.Depth]);
					}
				}

				linkStyle.AlwaysRenderTextDecoration = true;
			}
			linkStyle.AddAttributesToRender (writer);

			writer.AddAttribute ("id", GetItemClientId (item, "l"));
			
			if (item.Depth > 0 && !isDynamicItem) {
				double value;
#if NET_4_0
				Unit unit = StaticSubMenuIndent;
				if (unit == Unit.Empty)
					value = 16;
				else
					value = unit.Value;
#else
				value = StaticSubMenuIndent.Value;
#endif
				Unit indent = new Unit (value * item.Depth, StaticSubMenuIndent.Type);
				writer.AddStyleAttribute (HtmlTextWriterStyle.MarginLeft, indent.ToString ());
			}
			writer.RenderBeginTag (HtmlTextWriterTag.A);
			RenderItemContent (writer, item, isDynamicItem);
			writer.RenderEndTag ();	// A

			writer.RenderEndTag ();	// TD

			// Popup image

			if (dynamicChildren) {
				string popOutImage = GetPopOutImage (item, isDynamicItem);
				if (popOutImage != null) {
					writer.RenderBeginTag (HtmlTextWriterTag.Td);
					writer.AddAttribute ("src", ResolveClientUrl (popOutImage));
					writer.AddAttribute ("border", "0");
					string toolTip = String.Format (isDynamicItem ? DynamicPopOutImageTextFormatString : StaticPopOutImageTextFormatString, item.Text);
					writer.AddAttribute (HtmlTextWriterAttribute.Alt, toolTip);
					writer.RenderBeginTag (HtmlTextWriterTag.Img);
					writer.RenderEndTag ();	// IMG
					writer.RenderEndTag ();	// TD
				}
			}

			writer.RenderEndTag ();	// TR
			writer.RenderEndTag ();	// TABLE
			
			writer.RenderEndTag ();	// TD

			if (!vertical && itemSpacing == Unit.Empty && (notLast || (displayChildren && !dynamicChildren))) {
				writer.AddStyleAttribute ("width", "3px");
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				writer.RenderEndTag ();
			}
			
			// Bottom separator image
			string separatorImg = item.SeparatorImageUrl;
			if (separatorImg.Length == 0) {
				if (isDynamicItem)
					separatorImg = DynamicBottomSeparatorImageUrl;
				else
					separatorImg = StaticBottomSeparatorImageUrl;
			}
			if (separatorImg.Length > 0) {
				if (!vertical)
					writer.RenderBeginTag (HtmlTextWriterTag.Td);
				writer.AddAttribute ("src", ResolveClientUrl (separatorImg));
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
				if (!vertical)
					writer.RenderEndTag (); // TD
			}

			if (vertical)
				writer.RenderEndTag ();	// TR

			if (itemSpacing != Unit.Empty)
				RenderMenuItemSpacing (writer, itemSpacing, vertical);

			// Submenu

			if (displayChildren && !dynamicChildren) {
				if (vertical)
					writer.RenderBeginTag (HtmlTextWriterTag.Tr);
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				writer.AddAttribute ("width", "100%");
				RenderMenu (writer, item.ChildItems, Orientation == Orientation.Vertical, false, item.Depth + 1, notLast);
				if (item.Depth + 2 == StaticDisplayLevels)
					RenderDynamicMenu (writer, item.ChildItems);
				writer.RenderEndTag ();	// TD
				if (vertical)
					writer.RenderEndTag ();	// TR
			}

		}

		void RenderItemContent (HtmlTextWriter writer, MenuItem item, bool isDynamicItem) {
			if (_menuItemControls!=null && _menuItemControls [item] != null) {
				((Control) _menuItemControls [item]).Render (writer);
			}
			else {

				if (!String.IsNullOrEmpty (item.ImageUrl)) {
					writer.AddAttribute (HtmlTextWriterAttribute.Src, ResolveClientUrl (item.ImageUrl));
					writer.AddAttribute (HtmlTextWriterAttribute.Alt, item.ToolTip);
					writer.AddStyleAttribute (HtmlTextWriterStyle.BorderStyle, "none");
					writer.AddStyleAttribute (HtmlTextWriterStyle.VerticalAlign, "middle");
					writer.RenderBeginTag (HtmlTextWriterTag.Img);
					writer.RenderEndTag ();	// IMG
				}

				if (isDynamicItem && DynamicItemFormatString.Length > 0) {
					writer.Write (String.Format (DynamicItemFormatString, item.Text));
				}
				else if (!isDynamicItem && StaticItemFormatString.Length > 0) {
					writer.Write (String.Format (StaticItemFormatString, item.Text));
				}
				else {
					writer.Write (item.Text);
				}
			}
		}
			
		Unit GetItemSpacing (MenuItem item, bool dynamic)
		{
			Unit itemSpacing = Unit.Empty;
			
			if (item.Selected) {
				if (levelSelectedStyles != null && item.Depth < levelSelectedStyles.Count) {
					itemSpacing = levelSelectedStyles [item.Depth].ItemSpacing;
					if (itemSpacing != Unit.Empty) return itemSpacing;
				}

				if (dynamic && dynamicSelectedStyle != null)
					itemSpacing = dynamicSelectedStyle.ItemSpacing;
				else if (!dynamic && staticSelectedStyle != null)
					itemSpacing = staticSelectedStyle.ItemSpacing;
				if (itemSpacing != Unit.Empty)
					return itemSpacing;
			}
			
			if (levelMenuItemStyles != null && item.Depth < levelMenuItemStyles.Count) {
				itemSpacing = levelMenuItemStyles [item.Depth].ItemSpacing;
				if (itemSpacing != Unit.Empty) return itemSpacing;
			}

			if (dynamic && dynamicMenuItemStyle != null)
				return dynamicMenuItemStyle.ItemSpacing;
			else if (!dynamic && staticMenuItemStyle != null)
				return staticMenuItemStyle.ItemSpacing;
			else
				return Unit.Empty;
		}
		
		string GetPopOutImage (MenuItem item, bool isDynamicItem)
		{
			if (item.PopOutImageUrl != "")
				return item.PopOutImageUrl;

			if (isDynamicItem) {
				if (DynamicPopOutImageUrl != "")
					return DynamicPopOutImageUrl;
				if (DynamicEnableDefaultPopOutImage)
					return Page.ClientScript.GetWebResourceUrl (typeof (Menu), "arrow_plus.gif");
			} else {
				if (StaticPopOutImageUrl != "")
					return StaticPopOutImageUrl;
				if (StaticEnableDefaultPopOutImage)
					return Page.ClientScript.GetWebResourceUrl (typeof (Menu), "arrow_plus.gif");
			}
			return null;
		}
			
		void RenderItemHref (HtmlTextWriter writer, MenuItem item)
		{
			if (!item.BranchEnabled) {
				writer.AddAttribute ("disabled", "true", false);
			}
			else if (!item.Selectable) {
				writer.AddAttribute ("href", "#", false);
				writer.AddStyleAttribute ("cursor", "text");
			}
			else if (item.NavigateUrl != "") {
				string target = item.Target != "" ? item.Target : Target;
#if TARGET_J2EE
				string navUrl = ResolveClientUrl (item.NavigateUrl, String.Compare (target, "_blank", StringComparison.InvariantCultureIgnoreCase) != 0);
#else
				string navUrl = ResolveClientUrl (item.NavigateUrl);
#endif
				writer.AddAttribute ("href", navUrl);
				if (target != "")
					writer.AddAttribute ("target", target);
			}
			else {
				writer.AddAttribute ("href", GetClientEvent (item));
			}

		}
		
		string GetItemClientId (MenuItem item, string sufix)
		{
			return ClientID + "_" + item.Path + sufix;
		}
		
		string GetClientEvent (MenuItem item)
		{
			return Page.ClientScript.GetPostBackClientHyperlink (this, item.Path, true);
		}

		class MenuTemplateWriter : TextWriter
		{
			char [] _buffer;
			int _ptr = 0;
			
			public MenuTemplateWriter (char [] buffer)
			{
				_buffer = buffer;
			}

			public override Encoding Encoding
			{
				get { return Encoding.Unicode; }
			}

			public override void Write (char value)
			{
				if (_ptr == _buffer.Length)
					EnsureCapacity ();
				
				_buffer [_ptr++] = value;
			}

			public override void Write (string value)
			{
				if (value == null)
					return;

				if (_ptr + value.Length >= _buffer.Length)
					EnsureCapacity ();

				for (int i = 0; i < value.Length; i++)
					_buffer [_ptr++] = value [i];
			}

			void EnsureCapacity ()
			{
				char [] tmpBuffer = new char [_buffer.Length * 2];
				Array.Copy (_buffer, tmpBuffer, _buffer.Length);

				_buffer = tmpBuffer;
			}
		}

		class MenuRenderHtmlTemplate
		{
			public const string Marker = "\u093a\u093b\u0971";
			char [] _templateHtml;

			MenuTemplateWriter _templateWriter;
			ArrayList idxs = new ArrayList (32);

			public MenuRenderHtmlTemplate ()
			{
				_templateHtml = new char [1024];
				_templateWriter = new MenuTemplateWriter (_templateHtml);
			}

			public static string GetMarker (int num)
			{
				char charNum = (char) ((int) '\u0971' + num);
				return string.Concat (Marker, charNum);
			}

			public HtmlTextWriter GetMenuTemplateWriter()
			{
				return new HtmlTextWriter (_templateWriter);
			}

			public void Parse ()
			{
				int mpos = 0;
				for (int i = 0; i < _templateHtml.Length; i++) {
					if (_templateHtml [i] == '\0') {
						idxs.Add (i);
						break;
					}

					if (_templateHtml [i] != Marker [mpos]) {
						mpos = 0;
						continue;
					}

					mpos++;
					if (mpos == Marker.Length) {
						mpos = 0;
						idxs.Add (i - Marker.Length + 1);
					}
				}
			}

			public void RenderTemplate (HtmlTextWriter writer, string [] dynamicParts, int start, int count)
			{
				if (idxs.Count == 0)
					return;

				int partStart = 0;
				int partEnd = (start == 0) ? -Marker.Length - 1 : (int) idxs [start - 1];
				int di = 0;

				int i = start;
				int total = start + count;
				for (; i < total; i++) {

					partStart = partEnd + Marker.Length + 1;
					partEnd = (int) idxs [i];
					
					// write static part
					writer.Write (_templateHtml, partStart, partEnd - partStart);

					// write synamic part
					di = (int) _templateHtml [partEnd + Marker.Length] - 0x971;
					writer.Write (dynamicParts [di]);
				}

				partStart = partEnd + Marker.Length + 1;
				partEnd = (int) idxs [i];

				writer.Write (_templateHtml, partStart, partEnd - partStart);
			}
		
		}
	}
}

#endif
