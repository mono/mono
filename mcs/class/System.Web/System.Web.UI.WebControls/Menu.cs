//
// System.Web.UI.WebControls.Menu.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
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

namespace System.Web.UI.WebControls
{
	public class Menu : HierarchicalDataBoundControl, IPostBackEventHandler, INamingContainer
	{
		MenuItemStyle dynamicMenuItemStyle;
		MenuItemStyle dynamicMenuStyle;
		MenuItemStyle dynamicSelectedStyle;
		MenuItemStyle staticMenuItemStyle;
		MenuItemStyle staticMenuStyle;
		MenuItemStyle staticSelectedStyle;

		MenuItemStyleCollection levelMenuItemStyles;
		MenuItemStyleCollection levelSelectedStyles;
		
		MenuItemCollection items;
		MenuItemBindingCollection dataBindings;
		MenuItem selectedItem;
		Hashtable bindings;
		
		private static readonly object MenuItemClickEvent = new object();
		
		public event MenuEventHandler MenuItemClick {
			add { Events.AddHandler (MenuItemClickEvent, value); }
			remove { Events.RemoveHandler (MenuItemClickEvent, value); }
		}
		
		protected virtual void OnMenuItemClick (MenuEventArgs e)
		{
			if (Events != null) {
				MenuEventHandler eh = (MenuEventHandler) Events [MenuItemClickEvent];
				if (eh != null) eh (this, e);
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[Editor ("System.Web.UI.Design.MenuItemBindingsEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public virtual MenuItemBindingCollection DataBindings {
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
		public virtual int DisappearAfter {
			get {
				object o = ViewState ["DisappearAfter"];
				if (o != null) return (int)o;
				return 500;
			}
			set {
				ViewState["DisappearAfter"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public virtual string DynamicBottomSeparatorImageUrl {
			get {
				object o = ViewState ["dbsiu"];
				if (o != null) return (string)o;
				return "";
			}
			set {
				ViewState["dbsiu"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[WebCategory ("Appearance")]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public virtual string DynamicTopSeparatorImageUrl {
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
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public virtual string StaticBottomSeparatorImageUrl {
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
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public virtual string StaticTopSeparatorImageUrl {
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
		public virtual Orientation Orientation {
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
		public virtual int StaticDisplayLevels {
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

		[DefaultValue ("16px")]
		public Unit StaticSubMenuIndent {
			get {
				object o = ViewState ["StaticSubMenuIndent"];
				if (o != null) return (Unit)o;
				return new Unit (16);
			}
			set {
				ViewState["StaticSubMenuIndent"] = value;
			}
		}

		[DefaultValue (3)]
		public virtual int MaximumDynamicDisplayLevels {
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
		public virtual int DynamicVerticalOffset {
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
		public virtual int DynamicHorizontalOffset {
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
		public virtual bool DynamicEnableDefaultPopOutImage {
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
		public virtual bool StaticEnableDefaultPopOutImage {
			get {
				object o = ViewState ["sedpoi"];
				if (o != null) return (bool)o;
				return true;
			}
			set {
				ViewState["sedpoi"] = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[Editor ("System.Web.UI.Design.MenuItemCollectionEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public virtual MenuItemCollection Items {
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
		public virtual char PathSeparator {
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
		public virtual bool ItemWrap {
			get {
				object o = ViewState ["ItemWrap"];
				if(o != null) return (bool)o;
				return false;
			}
			set {
				ViewState ["ItemWrap"] = value;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[NotifyParentProperty (true)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual MenuItemStyle DynamicMenuItemStyle {
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
		public virtual MenuItemStyle DynamicSelectedStyle {
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
		public virtual MenuItemStyle DynamicMenuStyle {
			get {
				if (dynamicMenuStyle == null) {
					dynamicMenuStyle = new MenuItemStyle ();
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
		public virtual MenuItemStyle StaticMenuItemStyle {
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
		public virtual MenuItemStyle StaticSelectedStyle {
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
		public virtual MenuItemStyle StaticMenuStyle {
			get {
				if (staticMenuStyle == null) {
					staticMenuStyle = new MenuItemStyle ();
					if (IsTrackingViewState)
						staticMenuStyle.TrackViewState();
				}
				return staticMenuStyle;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual MenuItemStyleCollection LevelMenuItemStyles {
			get {
				if (levelMenuItemStyles == null) {
					levelMenuItemStyles = new MenuItemStyleCollection ();
					if (IsTrackingViewState)
						((IStateManager)levelMenuItemStyles).TrackViewState();
				}
				return levelMenuItemStyles;
			}
		}

		[PersistenceMode (PersistenceMode.InnerProperty)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public virtual MenuItemStyleCollection LevelSelectedStyles {
			get {
				if (levelSelectedStyles == null) {
					levelSelectedStyles = new MenuItemStyleCollection ();
					if (IsTrackingViewState)
						((IStateManager)levelSelectedStyles).TrackViewState();
				}
				return levelSelectedStyles;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public MenuItem SelectedItem {
			get { return selectedItem; }
		}

		internal void SetSelectedItem (MenuItem item)
		{
			if (selectedItem == item) return;
			if (selectedItem != null)
				selectedItem.SelectedFlag = false;
			selectedItem = item;
			selectedItem.SelectedFlag = true;
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
			HierarchicalDataSourceView data = GetData ("");
			IHierarchicalEnumerable e = data.Select ();
			foreach (object obj in e) {
				IHierarchyData hdata = e.GetHierarchyData (obj);
				MenuItem item = new MenuItem ();
				item.Bind (hdata);
				Items.Add (item);
			}
		}
		
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			MenuItem item = FindItemByPos (eventArgument);
			if (item == null) return;
			item.Selected = true;
			OnMenuItemClick (new MenuEventArgs (item));
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
			if (levelMenuItemStyles != null)
				((IStateManager)levelMenuItemStyles).TrackViewState();
			if (levelSelectedStyles != null)
				((IStateManager)levelSelectedStyles).TrackViewState();
			if (dynamicSelectedStyle != null)
				dynamicSelectedStyle.TrackViewState();
			if (staticMenuItemStyle != null)
				staticMenuItemStyle.TrackViewState ();
			if (staticMenuStyle != null)
				staticMenuStyle.TrackViewState ();
			if (staticSelectedStyle != null)
				staticSelectedStyle.TrackViewState();
		}

		protected override object SaveViewState()
		{
			object[] states = new object [11];
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
				((IStateManager)dataBindings).LoadViewState(states[1]);
			if (states[2] != null)
				((IStateManager)Items).LoadViewState(states[2]);
			if (states[3] != null)
				dynamicMenuItemStyle.LoadViewState (states[3]);
			if (states[4] != null)
				dynamicMenuStyle.LoadViewState (states[4]);
			if (states[5] != null)
				((IStateManager)levelMenuItemStyles).LoadViewState(states[5]);
			if (states[6] != null)
				((IStateManager)levelSelectedStyles).LoadViewState(states[6]);
			if (states[7] != null)
				dynamicSelectedStyle.LoadViewState (states[7]);
			if (states[8] != null)
				staticMenuItemStyle.LoadViewState (states[8]);
			if (states[9] != null)
				staticMenuStyle.LoadViewState (states[9]);
			if (states[10] != null)
				staticSelectedStyle.LoadViewState (states[10]);
		}
		
		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			
			if (!Page.ClientScript.IsClientScriptIncludeRegistered (typeof(Menu), "Menu.js")) {
				string url = Page.GetWebResourceUrl (typeof(Menu), "Menu.js");
				Page.ClientScript.RegisterClientScriptInclude (typeof(Menu), "Menu.js", url);
			}
			
			string cmenu = ClientID + "_data";
			string script = string.Format ("var {0} = new Object ();\n", cmenu);
			script += string.Format ("{0}.disappearAfter = {1};\n", cmenu, ClientScriptManager.GetScriptLiteral (DisappearAfter));
			script += string.Format ("{0}.vertical = {1};\n", cmenu, ClientScriptManager.GetScriptLiteral (Orientation == Orientation.Vertical));
			if (DynamicHorizontalOffset != 0)
				script += string.Format ("{0}.dho = {1};\n", cmenu, ClientScriptManager.GetScriptLiteral (DynamicHorizontalOffset));
			if (DynamicVerticalOffset != 0)
				script += string.Format ("{0}.dvo = {1};\n", cmenu, ClientScriptManager.GetScriptLiteral (DynamicVerticalOffset));

			Page.ClientScript.RegisterStartupScript (typeof(Menu), ClientID, script, true);

			if (dataBindings != null && dataBindings.Count > 0) {
				bindings = new Hashtable ();
				foreach (TreeNodeBinding bin in dataBindings) {
					string key = GetBindingKey (bin.DataMember, bin.Depth);
					bindings [key] = bin;
				}
			}
			else
				bindings = null;
		}
		
		protected override void RenderContents (HtmlTextWriter writer)
		{
			ArrayList dynamicMenus = new ArrayList ();
			
			RenderMenu (writer, Items, Orientation == Orientation.Vertical, dynamicMenus, false);
			
			for (int n=0; n<dynamicMenus.Count; n++) {
				MenuItem item = (MenuItem) dynamicMenus [n];
				writer.AddStyleAttribute ("visibility", "hidden");
				writer.AddStyleAttribute ("position", "absolute");
				writer.AddStyleAttribute ("left", "0px");
				writer.AddStyleAttribute ("top", "0px");
				writer.AddAttribute ("id", GetItemClientId (item, "s"));
				writer.RenderBeginTag (HtmlTextWriterTag.Div);
				
				RenderMenu (writer, item.ChildItems, true, dynamicMenus, true);
				
				writer.RenderEndTag ();	// DIV
			}
		}
		
		void RenderMenu (HtmlTextWriter writer, ICollection items, bool vertical, ArrayList dynamicMenus, bool dynamic)
		{
//			writer.AddAttribute ("border", "1");
			writer.AddAttribute ("cellpadding", "0");
			writer.AddAttribute ("cellspacing", "0");
			writer.AddStyleAttribute ("border-width", "0");

			if (dynamic && dynamicMenuStyle != null)
				dynamicMenuStyle.AddAttributesToRender (writer);
			else if (!dynamic && staticMenuStyle != null)
				staticMenuStyle.AddAttributesToRender (writer);
				
			writer.RenderBeginTag (HtmlTextWriterTag.Table);
			if (!vertical) writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			
			foreach (MenuItem item in items) {
				RenderMenuItem (writer, item, dynamicMenus);
			}
			
			if (!vertical) writer.RenderEndTag ();	// TR
			writer.RenderEndTag ();	// TABLE
			
		}
		
		void RenderMenuItem (HtmlTextWriter writer, MenuItem item, ArrayList dynamicMenus)
		{
			bool displayChildren = (item.Depth + 1 < StaticDisplayLevels + MaximumDynamicDisplayLevels);
			bool dynamicChildren = displayChildren && (item.Depth + 1 >= StaticDisplayLevels) && item.ChildItems.Count > 0;
			bool isDynamicItem = item.Depth + 1 > StaticDisplayLevels;
			bool vertical = (Orientation == Orientation.Vertical) || isDynamicItem;

			if (vertical)
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			
			if (levelMenuItemStyles != null && item.Depth < levelMenuItemStyles.Count)
				levelMenuItemStyles [item.Depth].AddAttributesToRender (writer);
			else if (isDynamicItem) {
				if (dynamicMenuItemStyle != null)
					dynamicMenuItemStyle.AddAttributesToRender (writer);
			} else {
				if (staticMenuItemStyle != null)
					staticMenuItemStyle.AddAttributesToRender (writer);
			}
			
			if (item == SelectedItem) {
				if (levelSelectedStyles != null && item.Depth < levelSelectedStyles.Count)
					levelSelectedStyles [item.Depth].AddAttributesToRender (writer);
				else if (isDynamicItem) {
					if (dynamicSelectedStyle != null)
						dynamicSelectedStyle.AddAttributesToRender (writer);
				} else {
					if (staticSelectedStyle != null)
						staticSelectedStyle.AddAttributesToRender (writer);
				}
			}
			
			writer.RenderBeginTag (HtmlTextWriterTag.Td);

			// Bottom separator image

			if (isDynamicItem && DynamicTopSeparatorImageUrl != "") {
				writer.AddAttribute ("src", DynamicTopSeparatorImageUrl);
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
			} else  if (!isDynamicItem && StaticTopSeparatorImageUrl != "") {
				writer.AddAttribute ("src", StaticTopSeparatorImageUrl);
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
			}
			
			// Menu item box
			
			string parentId = isDynamicItem ? "'" + item.Parent.Path + "'" : "null";
			if (dynamicChildren) {
				writer.AddAttribute ("onmouseover", string.Format ("javascript:Menu_OverItem ('{0}','{1}',{2})", ClientID, item.Path, parentId));
				writer.AddAttribute ("onmouseout", string.Format ("javascript:Menu_OutItem ('{0}','{1}')", ClientID, item.Path));
			} else if (isDynamicItem) {
				writer.AddAttribute ("onmouseover", string.Format ("javascript:Menu_OverLeafItem ('{0}', {1})", ClientID, parentId));
				writer.AddAttribute ("onmouseout", string.Format ("javascript:Menu_OutItem ('{0}', {1})", ClientID, parentId));
			} else {
				writer.AddAttribute ("onmouseover", string.Format ("javascript:Menu_OverStaticLeafItem ('{0}')", ClientID));
			}
			
			writer.AddAttribute ("id", GetItemClientId (item, "i"));
			
//			writer.AddAttribute ("border", "1");
			writer.AddAttribute ("cellpadding", "0");
			writer.AddAttribute ("cellspacing", "0");
			writer.AddStyleAttribute ("border-width", "0");
			writer.AddAttribute ("width", "100%");
			writer.RenderBeginTag (HtmlTextWriterTag.Table);
			writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			
			if (item.Depth > 0 && !isDynamicItem) {
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				writer.AddStyleAttribute ("width", StaticSubMenuIndent.ToString ());
				writer.RenderBeginTag (HtmlTextWriterTag.Div);
				writer.RenderEndTag ();	// DIV
				writer.RenderEndTag ();	// TD
			}
			
			if (item.ImageUrl != "") {
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				RenderItemHref (writer, item);
				writer.RenderBeginTag (HtmlTextWriterTag.A);
				writer.AddAttribute ("src", item.ImageUrl);
				writer.AddAttribute ("border", "0");
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
				writer.RenderEndTag ();	// A
				writer.RenderEndTag ();	// TD
			}
			
			// Menu item text
			
			writer.AddAttribute ("width", "100%");
			if (!ItemWrap)
				writer.AddAttribute ("nowrap", "nowrap");
			writer.RenderBeginTag (HtmlTextWriterTag.Td);
			
			RenderItemHref (writer, item);
			writer.AddStyleAttribute ("text-decoration", "none");
			writer.RenderBeginTag (HtmlTextWriterTag.A);
			writer.Write (item.Text);
			writer.RenderEndTag ();	// A
			
			writer.RenderEndTag ();	// TD
			
			// Popup image
			
			if (dynamicChildren && ((isDynamicItem && DynamicEnableDefaultPopOutImage) || (!isDynamicItem && StaticEnableDefaultPopOutImage) || item.PopOutImageUrl != ""))
			{
				writer.RenderBeginTag (HtmlTextWriterTag.Td);

				string src;
				if (item.PopOutImageUrl != "")
					src = item.PopOutImageUrl;
				else
					src = AssemblyResourceLoader.GetResourceUrl (typeof(Menu), "arrow_plus.gif");

				writer.AddAttribute ("src", src);
				writer.AddAttribute ("border", "0");
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
				
				writer.RenderEndTag ();	// TD
			}
			
			writer.RenderEndTag ();	// TR
			writer.RenderEndTag ();	// TABLE
			
			// Bottom separator image
				
			if (isDynamicItem && DynamicBottomSeparatorImageUrl != "") {
				writer.AddAttribute ("src", DynamicBottomSeparatorImageUrl);
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
			} else  if (!isDynamicItem && StaticBottomSeparatorImageUrl != "") {
				writer.AddAttribute ("src", StaticBottomSeparatorImageUrl);
				writer.RenderBeginTag (HtmlTextWriterTag.Img);
				writer.RenderEndTag ();	// IMG
			}
			
			// Submenu
				
			if (vertical) {
				if (displayChildren) {
					if (dynamicChildren) dynamicMenus.Add (item);
					else {
						writer.AddAttribute ("width", "100%");
						RenderMenu (writer, item.ChildItems, true, dynamicMenus, false);
					}
				}
				
				writer.RenderEndTag ();	// TD
				writer.RenderEndTag ();	// TR
			} else {
				writer.RenderEndTag ();	// TD
				
				writer.RenderBeginTag (HtmlTextWriterTag.Td);
				if (displayChildren) {
					if (dynamicChildren) dynamicMenus.Add (item);
					else RenderMenu (writer, item.ChildItems, false, dynamicMenus, false);
				}
				writer.RenderEndTag ();	// TD
			}
		}
		
		void RenderItemHref (HtmlTextWriter writer, MenuItem item)
		{
			if (item.NavigateUrl != "") {
				writer.AddAttribute ("href", item.NavigateUrl);
				if (item.Target != null)
					writer.AddAttribute ("target", item.Target);
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
			return Page.GetPostBackClientHyperlink (this, item.Path);
		}
	}
}

#endif
