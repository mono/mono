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
				depth = 0;
				MenuItem nod = parent;
				while (nod != null) {
					depth++;
					nod = nod.parent;
				}
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
				if (SelectedFlag) {
					if (value != null)
						value.SetSelectedItem (this);
					else if (menu != null)
						menu.SetSelectedItem (null);
				}
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
			get { return hierarchyData != null; }
		}
		
		[DefaultValue (null)]
		[Browsable (false)]
		public object DataItem {
			get {
				if (hierarchyData == null) throw new InvalidOperationException ("MenuItem is not data bound.");
				return hierarchyData.Item;
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[DefaultValue ("")]
		[Browsable (false)]
		public string DataPath {
			get {
				if (hierarchyData == null) throw new InvalidOperationException ("MenuItem is not data bound.");
				return hierarchyData.Path;
			}
		}
		
		[MergableProperty (false)]
		[Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		public MenuItemCollection ChildItems {
			get {
				if (items == null) {
					if (DataBound)
						FillBoundChildren ();
					else
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
				object o = ViewState ["ImageUrl"];
				if (o != null) return (string)o;
				if (DataBound) {
					MenuItemBinding bin = GetBinding ();
					if (bin != null) {
						if (bin.ImageUrlField != "")
							return (string) GetBoundPropertyValue (bin.ImageUrlField);
						return bin.ImageUrl;
					}
				}
				return "";
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
				object o = ViewState ["NavigateUrl"];
				if (o != null) return (string)o;
				if (DataBound) {
					MenuItemBinding bin = GetBinding ();
					if (bin != null) {
						if (bin.NavigateUrlField != "")
							return (string) GetBoundPropertyValue (bin.NavigateUrlField);
						return bin.NavigateUrl;
					}
				}
				return "";
			}
			set {
				ViewState ["NavigateUrl"] = value;
			}
		}

		[DefaultValue ("")]
		public string Target {
			get {
				object o = ViewState ["Target"];
				if(o != null) return (string)o;
				if (DataBound) {
					MenuItemBinding bin = GetBinding ();
					if (bin != null) {
						if (bin.TargetField != "")
							return (string) GetBoundPropertyValue (bin.TargetField);
						return bin.Target;
					}
				}
				return "";
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
				if (o != null) return (string)o;
				if (DataBound) {
					MenuItemBinding bin = GetBinding ();
					if (bin != null) {
						string text;
						if (bin.TextField != "")
							text = (string) GetBoundPropertyValue (bin.TextField);
						else if (bin.Text != "")
							text = bin.Text;
						else
							text = hierarchyData.ToString ();
							
						if (bin.FormatString.Length != 0)
							text = string.Format (bin.FormatString, text);
						return text;
					}
					return hierarchyData.ToString ();
				}
				return "";
			}
			set {
				ViewState ["Text"] = value;
			}
		}

		[Localizable (true)]
		[DefaultValue ("")]
		public string ToolTip {
			get {
				object o = ViewState ["ToolTip"];
				if(o != null) return (string)o;
				if (DataBound) {
					MenuItemBinding bin = GetBinding ();
					if (bin != null) {
						if (bin.ToolTipField != "")
							return (string) GetBoundPropertyValue (bin.ToolTipField);
						return bin.ToolTip;
					}
				}
				return "";
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
				if(o != null) return (string)o;
				if (DataBound) {
					MenuItemBinding bin = GetBinding ();
					if (bin != null) {
						if (bin.ValueField != "")
							return (string) GetBoundPropertyValue (bin.ValueField);
						if (bin.Value != "")
							return bin.Value;
					}
					return hierarchyData.ToString ();
				}
				return "";
			}
			set {
				ViewState ["Value"] = value;
			}
		}
		
		[DefaultValue (false)]
		public bool Selected {
			get {
				return SelectedFlag;
			}
			set {
				if (menu != null) {
					if (!value && menu.SelectedItem == this)
						menu.SetSelectedItem (null);
					else if (value)
						menu.SetSelectedItem (this);
				}
				else
					SelectedFlag = value;
			}
		}
		
		internal bool SelectedFlag {
			get {
				object o = ViewState ["Selected"];
				if(o != null) return (bool)o;
				return false;
			}
			set {
				ViewState ["Selected"] = value;
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
			
			if (menu != null && SelectedFlag)
				menu.SetSelectedItem (this);
			
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
			ViewState.SetDirty ();
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
		}
		
		MenuItemBinding GetBinding ()
		{
			if (menu == null) return null;
			if (gotBinding) return binding;
			binding = menu.FindBindingForItem (hierarchyData.Type, Depth);
			gotBinding = true;
			return binding;
		}
		
		object GetBoundPropertyValue (string name)
		{
			if (boundProperties == null) {
				ICustomTypeDescriptor desc = hierarchyData as ICustomTypeDescriptor;
				if (desc == null)
					throw new InvalidOperationException ("Property '" + name + "' not found in data bound item");
				boundProperties = desc.GetProperties ();
			}
			
			PropertyDescriptor prop = boundProperties.Find (name, true);
			if (prop == null)
				throw new InvalidOperationException ("Property '" + name + "' not found in data bound item");
			return prop.GetValue (hierarchyData);
		}

		void FillBoundChildren ()
		{
			items = new MenuItemCollection (this);
			if (!hierarchyData.HasChildren) return;

			IHierarchicalEnumerable e = hierarchyData.GetChildren ();
			foreach (object obj in e) {
				IHierarchyData hdata = e.GetHierarchyData (obj);
				MenuItem item = new MenuItem ();
				item.Bind (hdata);
				items.Add (item);
			}
		}
	}
}

#endif
