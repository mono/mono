//
// System.Web.UI.WebControls.ListControl.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Web;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.UI.WebControls
{
#if NET_2_0
	[ControlValuePropertyAttribute ("SelectedValue")]
#else
	[DefaultProperty("DataSource")]
#endif
	[DefaultEvent("SelectedIndexChanged")]
	[Designer ("System.Web.UI.Design.WebControls.ListControlDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
	[DataBindingHandler("System.Web.UI.Design.ListControlDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[ParseChildren(true, "Items")]
	public abstract class ListControl : 
		#if NET_2_0
			DataBoundControl
		#else
			WebControl
		#endif
	{
		private static readonly object SelectedIndexChangedEvent = new object();

		#if !NET_2_0
		private object dataSource;
		#endif
		
		private ListItemCollection items;

		private int cachedSelectedIndex = -1;
		private string cachedSelectedValue;

		#if !NET_2_0
		public ListControl(): base(HtmlTextWriterTag.Select)
		{
		}
		#else
		
		ArrayList selectedIndices;
		
		protected override HtmlTextWriterTag TagKey {
			get { return HtmlTextWriterTag.Select; }
		}
		#endif

		[WebCategory ("Action")]
		[WebSysDescription ("Raised when the selected index entry has changed.")]
		public event EventHandler SelectedIndexChanged
		{
			add
			{
				Events.AddHandler(SelectedIndexChangedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(SelectedIndexChangedEvent, value);
			}
		}

#if NET_2_0
	    [ThemeableAttribute (false)]
#endif
		[DefaultValue (false), WebCategory ("Behavior")]
		[WebSysDescription ("The control automatically posts back after changing the text.")]
		public virtual bool AutoPostBack
		{
			get
			{
				object o = ViewState["AutoPostBack"];
				if(o!=null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["AutoPostBack"] = value;
			}
		}

		#if !NET_2_0
		[DefaultValue (""), WebCategory ("Data")]
		[WebSysDescription ("The name of the table that is used for binding when a DataSource is specified.")]
		public virtual string DataMember
		{
			get
			{
				object o = ViewState["DataMember"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["DataMember"] = value;
			}
		}


		[DefaultValue (null), Bindable (true), WebCategory ("Data")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("The DataSource that is used for data-binding.")]
		public virtual object DataSource
		{
			get
			{
				return dataSource;
			}
			set
			{
				if(value == null || value is IListSource || value is IEnumerable) {
					dataSource = value;
					return;
				}
				throw new ArgumentException(HttpRuntime.FormatResourceString(ID, "Invalid DataSource Type"));
			}
		}
		#endif

#if NET_2_0
	    [ThemeableAttribute (false)]
#endif
		[DefaultValue (""), WebCategory ("Data")]
		[WebSysDescription ("The field in the datatable that provides the text entry.")]
		public virtual string DataTextField
		{
			get
			{
				object o = ViewState["DataTextField"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["DataTextField"] = value;
			}
		}

#if NET_2_0
	    [ThemeableAttribute (false)]
#endif
		[DefaultValue (""), WebCategory ("Data")]
		[WebSysDescription ("Specifies a formatting rule for the texts that are returned.")]
		public virtual string DataTextFormatString
		{
			get
			{
				object o = ViewState["DataTextFormatString"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["DataTextFormatString"] = value;
			}
		}

#if NET_2_0
	    [ThemeableAttribute (false)]
#endif
		[DefaultValue (""), WebCategory ("Data")]
		[WebSysDescription ("The field in the datatable that provides the entry value.")]
		public virtual string DataValueField
		{
			get
			{
				object o = ViewState["DataValueField"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["DataValueField"] = value;
			}
		}

#if NET_2_0
	    [Editor ("System.Web.UI.Design.WebControls.ListItemsCollectionEditor,System.Design, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
#endif
		[DefaultValue (null), MergableProperty (false), WebCategory ("Misc")]
		[PersistenceMode (PersistenceMode.InnerDefaultProperty)]
		[WebSysDescription ("A collection of all items contained in this list.")]
		public virtual ListItemCollection Items
		{
			get
			{
				if(items==null)
				{
					items = new ListItemCollection();
					
#if NET_2_0
					if (selectedIndices != null) {
						Select (selectedIndices);
						selectedIndices = null;
					}
#endif
					
					if(IsTrackingViewState)
					{
						items.TrackViewState();
					}
				}
				return items;
			}
		}

#if NET_2_0
	    [ThemeableAttribute (false)]
#endif
		[DefaultValue (0), Bindable (true), WebCategory ("Misc")]
		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("The index number of the currently selected ListItem.")]
		public virtual int SelectedIndex
		{
			get {
				ListItemCollection items = Items;
				int last = items.Count;
				for (int i = 0; i < last; i++) {
					if (items [i].Selected)
						return i;
				}
				return -1;
			}
			set {
				if (Items.Count == 0)
				{
					cachedSelectedIndex = value;
					return;
				}
				if ((value < -1) || (value >= Items.Count))
					throw new ArgumentOutOfRangeException ();

				ClearSelection ();
				if (value != -1)
					Items [value].Selected = true;
			}
		}

		[DefaultValue (null), WebCategory ("Misc")]
		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("The currently selected ListItem.")]
		public virtual ListItem SelectedItem
		{
			get
			{
				int idx = SelectedIndex;
				if (idx < 0)
					return null;

				return Items [idx];
			}
		}

#if NET_1_1
		#if NET_2_0
	    [ThemeableAttribute (false)]
		#endif
		[DefaultValue (""), Bindable (true), WebCategory ("Misc")]
		[Browsable (false), DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("The value of the currently selected ListItem.")]
		public virtual string SelectedValue {
			get {
				int idx = SelectedIndex;
				if (idx == -1)
					return "";

				return Items [idx].Value;
			}

			set {
				ListItem item = null;

				if (value != null) {
					if (Items.Count > 0) {
						item = Items.FindByValue (value);
						if (item == null)
							throw new ArgumentOutOfRangeException ("value");
					} else {
						cachedSelectedValue = value;
						return;
					}
				}

				ClearSelection ();
				if (item != null)
					item.Selected = true;
			}
		}
#endif

#if NET_2_0
    	[ThemeableAttribute (false)]
		[DefaultValue (false), WebCategory ("Behavior")]
		[WebSysDescription ("Determines if validation is performed when clicked.")]
		public bool CausesValidation
		{
			get
			{
				Object cv = ViewState["CausesValidation"];
				if(cv!=null)
					return (Boolean)cv;
				return true;
			}
			set
			{
				ViewState["CausesValidation"] = value;
			}
		}

		[DefaultValueAttribute ("")]
		[ThemeableAttribute (false)]
		[WebCategoryAttribute ("Behavior")]
		public string ValidationGroup {
			get {
				string text = (string)ViewState["ValidationGroup"];
				if (text!=null) return text;
				return String.Empty;
			}
			set {
				ViewState["ValidationGroup"] = value;
			}
		}
		
		[ThemeableAttribute (false)]
		[WebCategory ("Behavior")]
		[DefaultValueAttribute (false)]
		public bool AppendDataBoundItems {
			get {
				Object cv = ViewState["AppendDataBoundItems"];
				if (cv != null) return (bool) cv;
				return false;
			}
			set {
				ViewState["AppendDataBoundItems"] = value;
			}
		}
#endif
		
		internal virtual ArrayList SelectedIndices
		{
			get
			{
				ArrayList si = new ArrayList();
				for(int i=0; i < Items.Count; i++)
				{
					if(Items[i].Selected)
						si.Add(i);
				}
				return si;
			}
		}

		internal void Select(ArrayList indices)
		{
			ClearSelection();
			foreach(object intObj in indices)
			{
				int index = (int)intObj;
				if(index >= 0 && index < Items.Count)
					Items[index].Selected = true;
			}
		}

		public virtual void ClearSelection()
		{
			for(int i=0; i < Items.Count; i++)
			{
				Items[i].Selected = false;
			}
		}

#if !NET_2_0
		protected override void LoadViewState(object savedState)
		{
			//Order: BaseClass, Items (Collection), Indices
			if(savedState != null && savedState is Triplet)
			{
				Triplet state = (Triplet)savedState;
				base.LoadViewState(state.First);
				Items.LoadViewState(state.Second);
				object indices = state.Third;
				if(indices != null)
				{
					Select((ArrayList)indices);
				}
			}
		}
#else
		protected override void LoadViewState(object savedState)
		{
			//Order: BaseClass, Items (Collection)
			if(savedState != null && savedState is Pair)
			{
				Pair state = (Pair) savedState;
				base.LoadViewState(state.First);
				Items.LoadViewState(state.Second);
			}
		}
#endif

#if NET_2_0
		protected override void PerformSelect ()
		{
			base.PerformSelect ();
		}

		protected override void PerformDataBinding (IEnumerable ds)
		{
			base.PerformDataBinding (ds);
			if (!AppendDataBoundItems)
				Items.Clear();
			FillItems (ds);
		}
		
#else
		protected override void OnDataBinding(EventArgs e)
		{
			base.OnDataBinding(e);
			IEnumerable ds = DataSourceHelper.GetResolvedDataSource (DataSource, DataMember);
			if (ds != null)
				Items.Clear ();
			FillItems (ds);
		}
#endif

		void FillItems (IEnumerable ds)
		{
			if(ds != null) {
				string dtf = DataTextField;
				string dvf = DataValueField;
				string dtfs = DataTextFormatString;
				if (dtfs.Length == 0)
					dtfs = "{0}";

				bool dontUseProperties = (dtf.Length == 0 && dvf.Length == 0);

				foreach (object current in ds) {
					ListItem li = new ListItem();
					if (dontUseProperties){
						li.Text  = String.Format (dtfs, current);
						li.Value = current.ToString ();
						Items.Add (li);
						continue;
					}

					object o;
					if (dtf.Length > 0) {
						o = DataBinder.GetPropertyValue (current, dtf, dtfs);
						li.Text = o.ToString ();
					}

					if (dvf.Length > 0) {
						o = DataBinder.GetPropertyValue (current, dvf, null);
						li.Value = o.ToString ();
					}

					Items.Add(li);
				}
			}

			if (cachedSelectedValue != null) {
				int index = Items.FindByValueInternal (cachedSelectedValue);
				if (index == -1)
					throw new ArgumentOutOfRangeException("value");

				if (cachedSelectedIndex != -1 && cachedSelectedIndex != index)
					throw new ArgumentException(HttpRuntime.FormatResourceString(
						"Attributes_mutually_exclusive", "Selected Index", "Selected Value"));

				SelectedIndex = index;
				cachedSelectedIndex = -1;
				cachedSelectedValue = null;
				return;
			}

			if (cachedSelectedIndex != -1) {
				SelectedIndex = cachedSelectedIndex;
				cachedSelectedIndex = -1;
			}
		}

		protected virtual void OnSelectedIndexChanged(EventArgs e)
		{
			if(Events!=null)
				{
					EventHandler eh = (EventHandler)(Events[SelectedIndexChangedEvent]);
					if(eh!=null)
						eh(this, e);
				}
		}

		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender(e);
		}

#if !NET_2_0
		protected override object SaveViewState()
		{
			//Order: BaseClass, Items (Collection), Indices
			object vs = base.SaveViewState();
			object itemSvs = Items.SaveViewState();
			
			object indices = null;
			if (SaveSelectedIndicesViewState)
				indices = SelectedIndices;

			if (vs != null || itemSvs != null || indices != null)
				return new Triplet(vs, itemSvs, indices);

			return null;
		}
#else
		protected override object SaveViewState()
		{
			//Order: BaseClass, Items (Collection), Indices
			object vs = base.SaveViewState();
			object itemSvs = Items.SaveViewState();
			if (vs != null || itemSvs != null)
				return new Pair (vs, itemSvs);

			return null;
		}
#endif

		protected override void TrackViewState()
		{
			base.TrackViewState();
			Items.TrackViewState();
		}

		private bool SaveSelectedIndicesViewState {
			get {
				if (Events[SelectedIndexChangedEvent] == null && Enabled && Visible) {
					Type t = GetType();
					// If I am a derivative, let it take of storing the selected indices.
					if (t == typeof(DropDownList) || t == typeof(ListBox) ||
					    t == typeof(CheckBoxList) || t == typeof(RadioButtonList))
						return false;
				}
				return true;
			}
		}
		
#if NET_2_0

		protected override void OnInit (EventArgs e)
		{
			Page.RegisterRequiresControlState (this);
			base.OnInit (e);
		}
		
		protected internal override void LoadControlState (object ob)
		{
			if (ob == null) return;
			object[] state = (object[]) ob;
			base.LoadControlState (state[0]);
			selectedIndices = state[1] as ArrayList;
		}
		
		protected internal override object SaveControlState ()
		{
			object bstate = base.SaveControlState ();
			ArrayList mstate = SelectedIndices;
			if (mstate.Count == 0) mstate = null;
			
			if (bstate != null || mstate != null)
				return new object[] { bstate, mstate };
			else
				return null;
		}
		
		protected internal virtual void VerifyMultiSelect ()
		{
			object o = ViewState ["SelectionMode"];
			if (o != null && o.ToString () == "Single")
				throw new HttpException ("Cannot_MultiSelect_In_Single_Mode");
		}
		
		protected override void RenderContents (HtmlTextWriter writer)
		{
			bool selMade = false;
			foreach (ListItem current in Items){
				writer.WriteBeginTag ("option");
				if (current.Selected){
					if (selMade) VerifyMultiSelect ();
					selMade = true;
					writer.WriteAttribute ("selected", "selected");
				}
				writer.WriteAttribute ("value", current.Value, true);
				writer.Write ('>');
				writer.Write (HttpUtility.HtmlEncode (current.Text));
				writer.WriteEndTag ("option");
				writer.WriteLine ();
			}
		}
		
#endif
	}
}
