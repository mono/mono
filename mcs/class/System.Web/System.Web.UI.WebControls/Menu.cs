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
		MenuItemCollection items;
		MenuItemBindingCollection dataBindings;
		MenuItem selectedItem;
		Hashtable bindings;
		
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

/*		[DefaultValue (true)]
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
*/
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
	//		OnSelectedItemChanged (new MenuItemEventArgs (selectedItem));
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
		}
		
		protected override void TrackViewState()
		{
			EnsureDataBound ();
			
			base.TrackViewState();
			if (dataBindings != null) {
				((IStateManager)dataBindings).TrackViewState ();
			}
			if (items != null) {
				((IStateManager)items).TrackViewState();;
			}
		}

		protected override object SaveViewState()
		{
			object[] states = new object [3];
			states[0] = base.SaveViewState();
			states[1] = (dataBindings == null ? null : ((IStateManager)dataBindings).SaveViewState());
			states[2] = (items == null ? null : ((IStateManager)items).SaveViewState());

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
				((IStateManager)dataBindings).LoadViewState(states[8]);
			if (states[2] != null)
				((IStateManager)Items).LoadViewState(states[9]);
		}
		
		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			
			if (!Page.ClientScript.IsClientScriptIncludeRegistered (typeof(Menu), "Menu.js")) {
				string url = Page.GetWebResourceUrl (typeof(Menu), "Menu.js");
				Page.ClientScript.RegisterClientScriptInclude (typeof(Menu), "Menu.js", url);
				
				string cmenu = ClientID + "_data";
				string script = string.Format ("var {0} = new Object ();\n", cmenu);
				script += string.Format ("{0}.disappearAfter = {1};\n", cmenu, ClientScriptManager.GetScriptLiteral (DisappearAfter));
				script += string.Format ("{0}.vertical = {1};\n", cmenu, ClientScriptManager.GetScriptLiteral (Orientation == Orientation.Vertical));
				
				Page.ClientScript.RegisterStartupScript (typeof(Menu), "", script, true);
			}

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
			
			if (Orientation == Orientation.Horizontal) {
				writer.AddAttribute ("cellpadding", "0");
				writer.AddAttribute ("cellspacing", "0");
				writer.AddStyleAttribute ("border-width", "0");
				writer.RenderBeginTag (HtmlTextWriterTag.Table);
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			}
			
			foreach (MenuItem item in Items) {
				RenderMenuItem (writer, item, dynamicMenus);
			}
			
			if (Orientation == Orientation.Horizontal) {
				writer.RenderEndTag ();	// TR
				writer.RenderEndTag ();	// TABLE
			}
			
			for (int n=0; n<dynamicMenus.Count; n++) {
				MenuItem item = (MenuItem) dynamicMenus [n];
				writer.AddStyleAttribute ("display", "none");
				writer.AddStyleAttribute ("visibility", "hidden");
				writer.AddStyleAttribute ("position", "absolute");
				writer.AddStyleAttribute ("left", "0px");
				writer.AddStyleAttribute ("top", "0px");
				writer.AddAttribute ("id", GetItemClientId (item, "s"));
				writer.RenderBeginTag (HtmlTextWriterTag.Div);
				
				foreach (MenuItem mi in item.ChildItems) {
					RenderMenuItem (writer, mi, dynamicMenus);
				}
				
				writer.RenderEndTag ();	// DIV
			}
		}
		
		void RenderMenuItem (HtmlTextWriter writer, MenuItem item, ArrayList dynamicMenus)
		{
			bool displayChildren = (item.Depth + 1 < StaticDisplayLevels + MaximumDynamicDisplayLevels);
			bool dynamicChildren = displayChildren && (item.Depth + 1 >= StaticDisplayLevels) && item.ChildItems.Count > 0;
			bool isDynamicItem = item.Depth + 1 > StaticDisplayLevels;

			if (Orientation == Orientation.Vertical) {
				writer.AddAttribute ("cellpadding", "0");
				writer.AddAttribute ("cellspacing", "0");
				writer.AddStyleAttribute ("border-width", "0");
				writer.RenderBeginTag (HtmlTextWriterTag.Table);
				writer.RenderBeginTag (HtmlTextWriterTag.Tr);
			}
			
			if (item.Depth > 0 && !isDynamicItem) {
				for (int n=0; n<item.Depth; n++) {
					writer.RenderBeginTag (HtmlTextWriterTag.Td);
					writer.AddStyleAttribute ("width", StaticSubMenuIndent.ToString ());
					writer.RenderBeginTag (HtmlTextWriterTag.Div);
					writer.RenderEndTag ();	// DIV
					writer.RenderEndTag ();	// TD
				}
			}
			
			writer.RenderBeginTag (HtmlTextWriterTag.Td);
			
			if (item.NavigateUrl != "") {
				writer.AddAttribute ("href", item.NavigateUrl);
				if (item.Target != null)
					writer.AddAttribute ("target", item.Target);
				writer.AddStyleAttribute ("text-decoration", "none");
			}
			else {
				writer.AddAttribute ("href", GetClientEvent (item, "sel"));
				writer.AddStyleAttribute ("text-decoration", "none");
			}
			
			string parentId = item.Parent != null ? "'" + item.Parent.Path + "'" : "null";
			if (dynamicChildren) {
				writer.AddAttribute ("onmouseover", "javascript:Menu_OverItem ('" + ClientID + "', '" + item.Path + "', " + parentId + ")");
				writer.AddAttribute ("onmouseout", "javascript:Menu_OutItem ('" + ClientID + "', '" + item.Path + "')");
			} else if (isDynamicItem) {
				writer.AddAttribute ("onmouseover", "javascript:Menu_OverLeafItem ('" + ClientID + "', " + parentId + ")");
				writer.AddAttribute ("onmouseout", "javascript:Menu_OutItem ('" + ClientID + "', " + parentId + ")");
			}
			
			writer.AddAttribute ("id", GetItemClientId (item, "i"));
			writer.RenderBeginTag (HtmlTextWriterTag.A);
			writer.Write (item.Text);
			writer.RenderEndTag ();	// A
			
			writer.RenderEndTag ();	// TD
			
			if (Orientation == Orientation.Vertical) {
				writer.RenderEndTag ();	// TR
				writer.RenderEndTag ();	// TABLE
			}
			
			if (displayChildren) {
				if (dynamicChildren) {
					dynamicMenus.Add (item);
				} else {
					foreach (MenuItem mi in item.ChildItems) {
						RenderMenuItem (writer, mi, dynamicMenus);
					}
				}
			}
		}
		
		string GetItemClientId (MenuItem item, string sufix)
		{
			return ClientID + "_" + item.Path + sufix;
		}
							
		string GetClientEvent (MenuItem item, string ev)
		{
			return Page.GetPostBackClientHyperlink (this, ev + "|" + item.Path);
		}
	}
}

#endif
