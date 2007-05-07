//
// System.Web.UI.WebControls.MenuItem.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.Collections;
using System.Text;
using System.ComponentModel;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[ParseChildrenAttribute (true, "ChildItems")]
	public sealed class MenuItem: IStateManager, ICloneable
	{
		StateBag ViewState = new StateBag ();
		MenuItemCollection items;
		bool marked;
		Menu menu;
		MenuItem parent;
		int index;
		string path;
		int depth = -1;
		
		object dataItem;
		IHierarchyData hierarchyData;
		
		bool gotBinding;
		MenuItemBinding binding;
		PropertyDescriptorCollection boundProperties;
		
		public MenuItem ()
		{
		}
		
		public MenuItem (string text)
		{
			Text = text;
		}
		
		public MenuItem (string text, string value)
		{
			Text = text;
			Value = value;
		}
		
		public MenuItem (string text, string value, string imageUrl)
		{
			Text = text;
			Value = value;
			ImageUrl = imageUrl;
		}
		
		public MenuItem (string text, string value, string imageUrl, string navigateUrl)
		{
			Text = text;
			Value = value;
			ImageUrl = imageUrl;
			NavigateUrl = navigateUrl;
		}
		
		public MenuItem (string text, string value, string imageUrl, string navigateUrl, string target)
		{
			Text = text;
			Value = value;
			ImageUrl = imageUrl;
			NavigateUrl = navigateUrl;
			Target = target;
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public int Depth {
			get {
				if (depth != -1) return depth;
				if (Parent == null) depth = 0;
				else depth = Parent.Depth + 1;
				return depth;
			}
		}
		
		void ResetPathData ()
		{
			path = null;
			depth = -1;
			gotBinding = false;
		}
		
		internal Menu Menu {
			get { return menu; }
			set {
				menu = value;
				if (items != null)
					items.SetMenu (menu);
				ResetPathData ();
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[DefaultValue (false)]
		[Browsable (false)]
		public bool DataBound {
			get { return ViewState ["DataBound"] == null ? false : (bool) ViewState ["DataBound"]; }
			private set { ViewState ["DataBound"] = value; }
		}
		
		[DefaultValue (null)]
		[Browsable (false)]
		public object DataItem {
			get {
				if (!DataBound) throw new InvalidOperationException ("MenuItem is not data bound.");
				return dataItem;
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[DefaultValue ("")]
		[Browsable (false)]
		public string DataPath {
			get {
				return ViewState ["DataPath"] == null ? String.Empty : (String) ViewState ["DataPath"];
			}
			private set {
				ViewState ["DataPath"] = value;
			}
		}
		
		[MergableProperty (false)]
		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		public MenuItemCollection ChildItems {
			get {
				if (items == null) {
					items = new MenuItemCollection (this);
						
					if (((IStateManager)this).IsTrackingViewState)
						((IStateManager)items).TrackViewState();
				}
				return items;
			}
		}
		
		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string ImageUrl {
			get {
				return ViewState ["ImageUrl"] == null ? String.Empty : (String) ViewState ["ImageUrl"];
			}
			set {
				ViewState ["ImageUrl"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.UrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string NavigateUrl {
			get {
				return ViewState ["NavigateUrl"] == null ? String.Empty : (String) ViewState ["NavigateUrl"];
			}
			set {
				ViewState ["NavigateUrl"] = value;
			}
		}

		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string PopOutImageUrl {
			get {
				return ViewState ["PopOutImageUrl"] == null ? String.Empty : (String) ViewState ["PopOutImageUrl"];
			}
			set {
				ViewState ["PopOutImageUrl"] = value;
			}
		}

		[DefaultValue ("")]
		public string Target {
			get {
				return ViewState ["Target"] == null ? String.Empty : (String) ViewState ["Target"];
			}
			set {
				ViewState ["Target"] = value;
			}
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public string Text {
			get {
				object o = ViewState ["Text"];
				if (o == null)
					o = ViewState ["Value"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}
			set {
				ViewState ["Text"] = value;
			}
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public string ToolTip {
			get {
				return ViewState ["ToolTip"] == null ? String.Empty : (String) ViewState ["ToolTip"];
			}
			set {
				ViewState ["ToolTip"] = value;
			}
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public string Value {
			get {
				object o = ViewState ["Value"];
				if (o == null)
					o = ViewState ["Text"];
				if (o != null)
					return (string) o;
				return String.Empty;
			}
			set {
				ViewState ["Value"] = value;
			}
		}
		
		[DefaultValue ("")]
		[UrlProperty]
		[Editor ("System.Web.UI.Design.ImageUrlEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		public string SeparatorImageUrl {
			get {
				return ViewState ["SeparatorImageUrl"] == null ? String.Empty : (String) ViewState ["SeparatorImageUrl"];
			}
			set {
				ViewState ["SeparatorImageUrl"] = value;
			}
		}
		
	    [BrowsableAttribute (true)]
	    [DefaultValueAttribute (true)]
		public bool Selectable {
			get {
				return ViewState ["Selectable"] == null ? true : (bool) ViewState ["Selectable"];
			}
			set {
				ViewState ["Selectable"] = value;
			}
		}
		
	    [BrowsableAttribute (true)]
	    [DefaultValueAttribute (true)]
		public bool Enabled {
			get {
				return ViewState ["Enabled"] == null ? true : (bool) ViewState ["Enabled"];
			}
			set {
				ViewState ["Enabled"] = value;
			}
		}
		
		internal bool BranchEnabled {
			get { return Enabled && (parent == null || parent.BranchEnabled); }
		}

		[DefaultValue (false)]
		[Browsable (true)]
		public bool Selected {
			get {
				if (menu != null)
					return menu.SelectedItem == this;
				else
					return false;
			}
			set {
				if (menu != null) {
					if (!value && menu.SelectedItem == this)
						menu.SetSelectedItem (null);
					else if (value)
						menu.SetSelectedItem (this);
				}
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public MenuItem Parent {
			get { return parent; }
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public string ValuePath {
			get {
				if (menu == null) return Value;
				
				StringBuilder sb = new StringBuilder (Value);
				MenuItem item = parent;
				while (item != null) {
					sb.Insert (0, menu.PathSeparator);
					sb.Insert (0, item.Value);
					item = item.Parent;
				}
				return sb.ToString ();
			}
		}
		
		internal int Index {
			get { return index; }
			set { index = value; ResetPathData (); }
		}
		
		internal void SetParent (MenuItem item) {
			parent = item;
			ResetPathData ();
		}
		
		internal string Path {
			get {
				if (path != null) return path;
				StringBuilder sb = new StringBuilder (index.ToString());
				MenuItem item = parent;
				while (item != null) {
					sb.Insert (0, '_');
					sb.Insert (0, item.Index.ToString ());
					item = item.Parent;
				}
				path = sb.ToString ();
				return path;
			}
		}
		
		internal bool HasChildData {
			get { return items != null; }
		}
		
		void IStateManager.LoadViewState (object savedState)
		{
			if (savedState == null)
				return;

			object[] states = (object[]) savedState;
			ViewState.LoadViewState (states [0]);
			
			if (states [1] != null)
				((IStateManager)ChildItems).LoadViewState (states [1]);
		}
		
		object IStateManager.SaveViewState ()
		{
			object[] states = new object[2];
			states[0] = ViewState.SaveViewState();
			states[1] = (items == null ? null : ((IStateManager)items).SaveViewState());
			
			for (int i = 0; i < states.Length; i++) {
				if (states [i] != null)
					return states;
			}
			return null;
		}
		
		void IStateManager.TrackViewState ()
		{
			if (marked) return;
			marked = true;
			ViewState.TrackViewState();

			if (items != null)
				((IStateManager)items).TrackViewState ();
		}
		
		bool IStateManager.IsTrackingViewState
		{
			get { return marked; }
		}
		
		internal void SetDirty ()
		{
			ViewState.SetDirty (true);
			if (items != null)
				items.SetDirty ();
		}
		
		object ICloneable.Clone ()
		{
			MenuItem nod = new MenuItem ();
			foreach (DictionaryEntry e in ViewState)
				nod.ViewState [(string)e.Key] = e.Value;
				
			foreach (ICloneable c in ChildItems)
				nod.ChildItems.Add ((MenuItem)c.Clone ());
				
			return nod;
		}
		
		internal void Bind (IHierarchyData hierarchyData)
		{
			this.hierarchyData = hierarchyData;
			DataBound = true;
			DataPath = hierarchyData.Path;
			dataItem = hierarchyData.Item;

			MenuItemBinding bin = GetBinding ();
			if (bin != null) {

				// Bind Enabled property

				if (bin.EnabledField != "")
					try { Enabled = Convert.ToBoolean (GetBoundPropertyValue (bin.EnabledField)); }
					catch { Enabled = bin.Enabled; }
				else
					Enabled = bin.Enabled;

				// Bind ImageUrl property

				if (bin.ImageUrlField.Length > 0) {
					ImageUrl = Convert.ToString (GetBoundPropertyValue (bin.ImageUrlField));
					if (ImageUrl.Length == 0)
						ImageUrl = bin.ImageUrl;
				}
				else if (bin.ImageUrl.Length > 0)
					ImageUrl = bin.ImageUrl;

				// Bind NavigateUrl property

				if (bin.NavigateUrlField.Length > 0) {
					NavigateUrl = Convert.ToString (GetBoundPropertyValue (bin.NavigateUrlField));
					if (NavigateUrl.Length == 0)
						NavigateUrl = bin.NavigateUrl;
				}
				else if (bin.NavigateUrl.Length > 0)
					NavigateUrl = bin.NavigateUrl;

				// Bind PopOutImageUrl property

				if (bin.PopOutImageUrlField.Length > 0) {
					PopOutImageUrl = Convert.ToString (GetBoundPropertyValue (bin.PopOutImageUrlField));
					if (PopOutImageUrl.Length == 0)
						PopOutImageUrl = bin.PopOutImageUrl;
				}
				else if (bin.PopOutImageUrl.Length > 0)
					PopOutImageUrl = bin.PopOutImageUrl;

				// Bind Selectable property

				if (bin.SelectableField != "")
					try { Selectable = Convert.ToBoolean (GetBoundPropertyValue (bin.SelectableField)); }
					catch { Selectable = bin.Selectable; }
				else
					Selectable = bin.Selectable;

				// Bind SeparatorImageUrl property

				if (bin.SeparatorImageUrlField.Length > 0) {
					SeparatorImageUrl = Convert.ToString (GetBoundPropertyValue (bin.SeparatorImageUrlField));
					if (SeparatorImageUrl.Length == 0)
						SeparatorImageUrl = bin.SeparatorImageUrl;
				}
				else if (bin.SeparatorImageUrl.Length > 0)
					SeparatorImageUrl = bin.SeparatorImageUrl;

				// Bind Target property

				if (bin.TargetField.Length > 0) {
					Target = Convert.ToString (GetBoundPropertyValue (bin.TargetField));
					if (Target.Length == 0)
						Target = bin.Target;
				}
				else if (bin.Target.Length > 0)
					Target = bin.Target;

				// Bind ToolTip property

				if (bin.ToolTipField.Length > 0) {
					ToolTip = Convert.ToString (GetBoundPropertyValue (bin.ToolTipField));
					if (ToolTip.Length == 0)
						ToolTip = bin.ToolTip;
				}
				else if (bin.ToolTip.Length > 0)
					ToolTip = bin.ToolTip;

				// Bind Value property
				string value = null;
				if (bin.ValueField.Length > 0) {
					value = Convert.ToString (GetBoundPropertyValue (bin.ValueField));
				}
				if (String.IsNullOrEmpty (value)) {
					if (bin.Value.Length > 0)
						value = bin.Value;
					else if (bin.Text.Length > 0)
						value = bin.Text;
					else
						value = String.Empty;
				}
				Value = value;

				// Bind Text property
				string text = null;
				if (bin.TextField.Length > 0) {
					text = Convert.ToString (GetBoundPropertyValue (bin.TextField));
					if (bin.FormatString.Length > 0)
						text = string.Format (bin.FormatString, text);
				}
				if (String.IsNullOrEmpty (text)) {
					if (bin.Text.Length > 0)
						text = bin.Text;
					else if (bin.Value.Length > 0)
						text = bin.Value;
					else
						text = String.Empty;
				}
				Text = text;

			}
			else {
				Text = Value = GetDefaultBoundText ();
			}

			INavigateUIData navigateUIData = hierarchyData as INavigateUIData;
			if (navigateUIData != null) {
				ToolTip = navigateUIData.Description;
				Text = navigateUIData.ToString ();
				NavigateUrl = navigateUIData.NavigateUrl;
			}
		}
		
		internal void SetDataItem (object item)
		{
			dataItem = item;
		}
		
		internal void SetDataPath (string path)
		{
			DataPath = path;
		}
		
		internal void SetDataBound (bool bound)
		{
			DataBound = bound;
		}
		
		string GetDefaultBoundText ()
		{
			if (hierarchyData != null) return hierarchyData.ToString ();
			else if (dataItem != null) return dataItem.ToString ();
			else return string.Empty;
		}
		
		string GetDataItemType ()
		{
			if (hierarchyData != null) return hierarchyData.Type;
			else if (dataItem != null) return dataItem.GetType().ToString ();
			else return string.Empty;
		}
		
		MenuItemBinding GetBinding ()
		{
			if (menu == null) return null;
			if (gotBinding) return binding;
			binding = menu.FindBindingForItem (GetDataItemType (), Depth);
			gotBinding = true;
			return binding;
		}
		
		object GetBoundPropertyValue (string name)
		{
			if (boundProperties == null) {
				if (hierarchyData != null)
					boundProperties = TypeDescriptor.GetProperties (hierarchyData);
				else
					boundProperties = TypeDescriptor.GetProperties (dataItem);
			}
			
			PropertyDescriptor prop = boundProperties.Find (name, true);
			if (prop == null)
				throw new InvalidOperationException ("Property '" + name + "' not found in data bound item");
				
			if (hierarchyData != null)
				return prop.GetValue (hierarchyData);
			else
				return prop.GetValue (dataItem);
		}
	}
}

#endif
