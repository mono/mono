//
// System.Web.UI.WebControls.Menu.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//	Igor Zelmanovich (igorz@mainsoft.com)
//
// (C) 2004-2010 Novell, Inc (http://www.novell.com)
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


using System;
using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
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
		IMenuRenderer renderer;
		
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
		Style popOutBoxStyle;
		Style controlLinkStyle;
		Style dynamicMenuItemLinkStyle;
		Style staticMenuItemLinkStyle;
		Style dynamicSelectedLinkStyle;
		Style staticSelectedLinkStyle;
		Style dynamicHoverLinkStyle;
		Style staticHoverLinkStyle;
		bool? renderList;
		bool includeStyleBlock = true;
		MenuRenderingMode renderingMode = MenuRenderingMode.Default;
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

		IMenuRenderer Renderer {
			get {
				if (renderer == null)
					renderer = CreateRenderer (null);
				
				return renderer;
			}
		}
		bool RenderList {
			get {
				if (renderList == null) {
					switch (RenderingMode) {
						case MenuRenderingMode.List:
							renderList = true;
							break;

						case MenuRenderingMode.Table:
							renderList = false;
							break;

						default:
							if (RenderingCompatibilityLessThan40)
								renderList = false;
							else
								renderList = true;
							break;
					}
				}

				return renderList.Value;
			}
		}
		
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
				renderer = CreateRenderer (renderer);
			}
		}
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
				if (o != null)
					return (string)o;
				return String.Empty;
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
				return Unit.Empty;
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

		internal Style PopOutBoxStyle {
			get {
				if (popOutBoxStyle == null) {
					popOutBoxStyle = new Style ();
					popOutBoxStyle.BackColor = Color.White;
				}
				return popOutBoxStyle;
			}
		}

		internal Style ControlLinkStyle {
			get {
				if (controlLinkStyle == null) {
					controlLinkStyle = new Style ();
					controlLinkStyle.AlwaysRenderTextDecoration = true;
				}
				return controlLinkStyle;
			}
		}

		internal Style DynamicMenuItemLinkStyle {
			get {
				if (dynamicMenuItemLinkStyle == null) {
					dynamicMenuItemLinkStyle = new Style ();
				}
				return dynamicMenuItemLinkStyle;
			}
		}

		internal Style StaticMenuItemLinkStyle {
			get {
				if (staticMenuItemLinkStyle == null) {
					staticMenuItemLinkStyle = new Style ();
				}
				return staticMenuItemLinkStyle;
			}
		}

		internal Style DynamicSelectedLinkStyle {
			get {
				if (dynamicSelectedLinkStyle == null) {
					dynamicSelectedLinkStyle = new Style ();
				}
				return dynamicSelectedLinkStyle;
			}
		}

		internal Style StaticSelectedLinkStyle {
			get {
				if (staticSelectedLinkStyle == null) {
					staticSelectedLinkStyle = new Style ();
				}
				return staticSelectedLinkStyle;
			}
		}

		internal Style DynamicHoverLinkStyle {
			get {
				if (dynamicHoverLinkStyle == null) {
					dynamicHoverLinkStyle = new Style ();
				}
				return dynamicHoverLinkStyle;
			}
		}

		internal Style StaticHoverLinkStyle {
			get {
				if (staticHoverLinkStyle == null) {
					staticHoverLinkStyle = new Style ();
				}
				return staticHoverLinkStyle;
			}
		}

		internal MenuItemStyle StaticMenuItemStyleInternal {
			get { return staticMenuItemStyle; }
		}

		internal SubMenuStyle StaticMenuStyleInternal {
			get { return staticMenuStyle; }
		}

		internal MenuItemStyle DynamicMenuItemStyleInternal {
			get { return dynamicMenuItemStyle; }
		}

		internal SubMenuStyle DynamicMenuStyleInternal {
			get { return dynamicMenuStyle; }
		}

		internal MenuItemStyleCollection LevelMenuItemStylesInternal {
			get { return levelMenuItemStyles; }
		}

		internal List<Style> LevelMenuItemLinkStyles {
			get { return null; }
		}

		internal SubMenuStyleCollection LevelSubMenuStylesInternal {
			get { return levelSubMenuStyles; }
		}

		internal MenuItemStyle StaticSelectedStyleInternal {
			get { return staticSelectedStyle; }
		}

		internal MenuItemStyle DynamicSelectedStyleInternal {
			get { return dynamicSelectedStyle; }
		}

		internal MenuItemStyleCollection LevelSelectedStylesInternal {
			get { return levelSelectedStyles; }
		}

		internal List<Style> LevelSelectedLinkStyles {
			get { return null; }
		}

		internal Style StaticHoverStyleInternal {
			get { return staticHoverStyle; }
		}

		internal Style DynamicHoverStyleInternal {
			get { return dynamicHoverStyle; }
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
		
		IMenuRenderer CreateRenderer (IMenuRenderer current)
		{
			Type newType = null;
			
			switch (RenderingMode) {
				case MenuRenderingMode.Default:
					if (RenderingCompatibilityLessThan40)
						newType = typeof (MenuTableRenderer);
					else
						newType = typeof (MenuListRenderer);
					break;
					
				case MenuRenderingMode.Table:
					newType = typeof (MenuTableRenderer);
					break;

				case MenuRenderingMode.List:
					newType = typeof (MenuListRenderer);
					break;
			}

			if (newType == null)
				return null;

			if (current == null || current.GetType () != newType)
				return Activator.CreateInstance (newType, this) as IMenuRenderer;
			return current;
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
			get { return Renderer.Tag; }
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

		protected override void LoadViewState (object state)
		{
			if (state == null)
				return;

			object [] states = (object []) state;
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
		
		protected internal override void LoadControlState (object savedState)
		{
			if (savedState == null) return;
			object[] state = (object[]) savedState;
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

		void CreateChildControlsForItems (MenuItemCollection items )
		{
			IMenuRenderer renderer = Renderer;
			foreach (MenuItem item in items) {
				bool isDynamicItem = renderer.IsDynamicItem (this, item);
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
		
		protected override bool OnBubbleEvent (object source, EventArgs e)
		{
			if (!(e is CommandEventArgs))
				return false;

			MenuEventArgs menuArgs = e as MenuEventArgs;
			if (menuArgs != null && string.Equals (menuArgs.CommandName, MenuItemClickCommandName))
				OnMenuItemClick (menuArgs);
			return true;
		}

		protected override void OnDataBinding (EventArgs e)
		{
			EnsureChildControls ();
			base.OnDataBinding (e);
		}
		
		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);

			string cmenu = ClientID + "_data";
			StringBuilder script = new StringBuilder ();
			Page page = Page;
			HtmlHead header;
			ClientScriptManager csm;

			if (page != null) {
				header = page.Header;
				csm = page.ClientScript;
			} else {
				header = null;
				csm = null;
			}
			
			Renderer.PreRender (page, header, csm, cmenu, script);

			if (csm != null) {
				csm.RegisterWebFormClientScript ();
				csm.RegisterStartupScript (typeof(Menu), ClientID, script.ToString (), true);
			}
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
		
		protected internal override void Render (HtmlTextWriter writer)
		{
			if (Items.Count > 0)
				base.Render (writer);
		}
		
		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			Renderer.AddAttributesToRender (writer);
			base.AddAttributesToRender (writer);
		}
		
		public override void RenderBeginTag (HtmlTextWriter writer)
		{
			string skipLinkText = SkipLinkText;
			if (!String.IsNullOrEmpty (skipLinkText))
				Renderer.RenderBeginTag (writer, skipLinkText);
			base.RenderBeginTag (writer);
		}
		
		public override void RenderEndTag (HtmlTextWriter writer)
		{
			base.RenderEndTag (writer);

			Renderer.RenderEndTag (writer);
			
			string skipLinkText = SkipLinkText;
			if (!String.IsNullOrEmpty (skipLinkText)) {
				writer.AddAttribute (HtmlTextWriterAttribute.Id, ClientID + "_SkipLink");
				writer.RenderBeginTag (HtmlTextWriterTag.A);
				writer.RenderEndTag ();
			}
		}
		
		protected internal override void RenderContents (HtmlTextWriter writer)
		{
			Renderer.RenderContents (writer);
		}

		internal void RenderDynamicMenu (HtmlTextWriter writer, MenuItemCollection items)
		{
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

		internal void RenderMenu (HtmlTextWriter writer, MenuItemCollection items, bool vertical, bool dynamic, int menuLevel, bool notLast)
		{
			IMenuRenderer renderer = Renderer;
			
			renderer.RenderMenuBeginTag (writer, dynamic, menuLevel);
			renderer.RenderMenuBody (writer, items, vertical, dynamic, notLast);
			renderer.RenderMenuEndTag (writer, dynamic, menuLevel);
		}

		internal bool DisplayChildren (MenuItem item)
		{
			return (item.Depth + 1 < StaticDisplayLevels + MaximumDynamicDisplayLevels) && item.ChildItems.Count > 0;
		}
		
		internal void RenderItem (HtmlTextWriter writer, MenuItem item, int position)
		{
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
			Renderer.RenderMenuItem (writer, item, notLast, position == 0);
		}

		internal void RenderItemContent (HtmlTextWriter writer, MenuItem item, bool isDynamicItem)
		{
			if (_menuItemControls!=null && _menuItemControls [item] != null)
				((Control) _menuItemControls [item]).Render (writer);

			Renderer.RenderItemContent (writer, item, isDynamicItem);
		}
			
		internal Unit GetItemSpacing (MenuItem item, bool dynamic)
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

