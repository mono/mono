//
// System.Web.UI.WebControls.ListControl.cs
//
// Authors:
//	Jackson Harper (jackson@ximian.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Drawing;
using System.Web.Util;
using System.Collections;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Specialized;

namespace System.Web.UI.WebControls {

	[DataBindingHandler("System.Web.UI.Design.WebControls.ListControlDataBindingHandler, " + Consts.AssemblySystem_Design)]
	[DefaultEventAttribute ("SelectedIndexChanged")]
#if !NET_2_0
	[DefaultPropertyAttribute ("DataSource")]
#endif
	[Designer("System.Web.UI.Design.WebControls.ListControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ParseChildrenAttribute (true, "Items")]
#if NET_2_0
	[ControlValueProperty ("SelectedValue", null)]
#endif	
	public abstract class ListControl :
#if NET_2_0
	DataBoundControl, IEditableTextControl, ITextControl
#else		
	WebControl
#endif
	{

		static readonly object SelectedIndexChangedEvent = new object ();
#if NET_2_0
		static readonly object TextChangedEvent = new object ();
#endif

		ListItemCollection items;
#if NET_2_0
		int _selectedIndex = -2;
		string _selectedValue;
#else		
		int saved_selected_index = -2;
		string saved_selected_value;
#endif

		public ListControl () : base (HtmlTextWriterTag.Select)
		{
		}

#if NET_2_0
		[DefaultValue (false)]
		[Themeable (false)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool AppendDataBoundItems
		{
			get {
				return ViewState.GetBool ("AppendDataBoundItems", false);
			}
			set {
				ViewState ["AppendDataBoundItems"] = value;
				if (Initialized)
					RequiresDataBinding = true;
			}
		}
#endif		
		
#if NET_2_0
		[Themeable (false)]
#endif
		[DefaultValue(false)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool AutoPostBack {
			get { return ViewState.GetBool ("AutoPostBack", false); }
			set { ViewState ["AutoPostBack"] = value; }
		}

#if ONLY_1_1
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Data")]
		public virtual string DataMember {
			get { return ViewState.GetString ("DataMember", String.Empty); }
			set { ViewState ["DataMember"] = value; }
		}

		object data_source;

		[Bindable(true)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Data")]
		public virtual object DataSource {
			get { return data_source; }
			set { 
				if(value == null || value is IListSource || value is IEnumerable) { 
					data_source = value;
					return;
				}
				throw new ArgumentException("Invalid DataSource Type");
			}
		}
#endif		

#if NET_2_0
		[Themeable (false)]
#endif		
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Data")]
		public virtual string DataTextField {
			get { return ViewState.GetString ("DataTextField", String.Empty); }
			set { 
				ViewState ["DataTextField"] = value;
#if NET_2_0
				if (Initialized)
					RequiresDataBinding = true;
#endif
			}
		}

#if NET_2_0
		[Themeable (false)]
#endif		
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Data")]
		public virtual string DataTextFormatString {
			get { return ViewState.GetString ("DataTextFormatString", String.Empty); }
			set { 
				ViewState ["DataTextFormatString"] = value;
#if NET_2_0
				if (Initialized)
					RequiresDataBinding = true;
#endif
			}
		}

#if NET_2_0
		[Themeable (false)]
#endif		
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Data")]
		public virtual string DataValueField {
			get { return ViewState.GetString ("DataValueField", String.Empty); }
			set { 
				ViewState ["DataValueField"] = value;
#if NET_2_0
				if (Initialized)
					RequiresDataBinding = true;
#endif
			}
		}

#if NET_2_0
		[Editor ("System.Web.UI.Design.WebControls.ListItemsCollectionEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
#endif		
		[DefaultValue(null)]
		[MergableProperty(false)]
		[PersistenceMode(PersistenceMode.InnerDefaultProperty)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual ListItemCollection Items {
			get {
				if (items == null) {
					items = new ListItemCollection ();
					if (IsTrackingViewState)
						((IStateManager) items).TrackViewState ();
				}
				return items;
			}
		}

		// I can't find this info stored in the viewstate anywhere
		// so it must be calculated on the fly.
		[Bindable(true)]
		[Browsable(false)]
		[DefaultValue(0)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#if NET_2_0
		[Themeable (false)]
#endif		
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual int SelectedIndex {
			get {
				if (items == null)
					return -1;
				for (int i = 0; i < items.Count; i++) {
					if (items [i].Selected)
						return i;
				}
				return -1;
			}
			set {
#if NET_2_0
				_selectedIndex = value;

				if (value < -1)
					throw new ArgumentOutOfRangeException ("value");

				if (value >= Items.Count) 
					return;

				ClearSelection ();
				if (value == -1)
					return;

				items [value].Selected = true;

				/* you'd think this would be called, but noooo */
				//OnSelectedIndexChanged (EventArgs.Empty);
#else
				if (items == null || items.Count == 0) {
					// This will happen when assigning this property
					// before DataBind () is called on the control.
					saved_selected_index = value;
					return;
				}

				if (value < -1 || value >= Items.Count)
					throw new ArgumentOutOfRangeException ("value");

				ClearSelection ();
				if (value == -1)
					return;

				items [value].Selected = true;

				/* you'd think this would be called, but noooo */
				//OnSelectedIndexChanged (EventArgs.Empty);
#endif
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual ListItem SelectedItem {
			get {
				int si = SelectedIndex;
				if (si == -1)
					return null;
				return Items [si];
			}
		}

#if NET_2_0
		[Bindable(true, BindingDirection.TwoWay)]
		[Themeable (false)]
#else		
		[Bindable(true)]
#endif		
		[Browsable(false)]
		[DefaultValue("")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription ("")]
		[WebCategory ("Misc")]
		public virtual string SelectedValue {
			get {
				int si = SelectedIndex;
				if (si == -1)
					return String.Empty;
				return Items [si].Value;
			}
			set {
#if NET_2_0
				_selectedValue = value;
				SetSelectedValue (value);
#else
				ClearSelection ();
				if (items == null || items.Count == 0) {
					// This will happen when assigning this property
					// before DataBind () is called on the control.
					saved_selected_value = value;
					return;
				}

				int count = Items.Count;
				ListItemCollection coll = Items;
				bool thr = true;
				for (int i = 0; i < count; i++) {
					if (coll [i].Value == value) {
						coll [i].Selected = true;
						thr = false;
					}
				}

				if (thr) {
					string msg = String.Format ("Argument value is out of range: {0}", value);
					throw new ArgumentOutOfRangeException (msg);
				}
#endif
			}
		}

#if NET_2_0
		bool SetSelectedValue (string value)
		{
			if (items != null && items.Count > 0) {
				int count = items.Count;
				ListItemCollection coll = Items;
				for (int i = 0; i < count; i++) {
					if (coll [i].Value == value) {
						ClearSelection ();
						coll [i].Selected = true;
						return true;
					}
				}
			}
			return false;
		}

		[Themeable (false)]
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual string Text {
			get {
				return SelectedValue;
			}
			set {
				SelectedValue = value;
				/* you'd think this would be called, but noooo */
				//OnTextChanged (EventArgs.Empty);
			}
		}

#if HAVE_CONTROL_ADAPTERS
		protected virtual new
#else		
		protected override
#endif
		HtmlTextWriterTag TagKey
		{
			get {
				return HtmlTextWriterTag.Select;
			}
		}

		protected override void AddAttributesToRender (HtmlTextWriter w)
		{
			base.AddAttributesToRender (w);
		}
		
#endif		

		public virtual void ClearSelection ()
		{
			if (items == null)
				return;

			int count = Items.Count;
			for (int i = 0; i<count; i++)
				items [i].Selected = false;
		}

		protected override void OnDataBinding (EventArgs e)
		{
			base.OnDataBinding (e);
#if !NET_2_0
			IEnumerable list = DataSourceResolver.ResolveDataSource (DataSource, DataMember);
			PerformDataBinding (list);
#else
			IEnumerable list = GetData ().ExecuteSelect (DataSourceSelectArguments.Empty);
			InternalPerformDataBinding (list);
#endif
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
#if NET_2_0
			Page page = Page;
			if (page != null && IsEnabled)
				page.RegisterEnabledControl (this);
#endif
		}

#if NET_2_0
		protected virtual void OnTextChanged (EventArgs e)
		{
			EventHandler handler = (EventHandler) Events [TextChangedEvent];
			if (handler != null)
				handler (this, e);
		}
#endif		

#if NET_2_0
		protected internal override
#endif
		void PerformDataBinding (IEnumerable dataSource)
		{
			if (dataSource == null)
#if NET_2_0
				goto setselected;
#else
				return;
#endif
#if NET_2_0
			if (!AppendDataBoundItems)
#endif
				Items.Clear ();

			string format = DataTextFormatString;
			if (format.Length == 0)
				format = null;

			string text_field = DataTextField;
			string value_field = DataValueField;

			if (text_field.Length == 0)
				text_field = null;
			if (value_field.Length == 0)
				value_field = null;
			
			ListItemCollection coll = Items;
			foreach (object container in dataSource) {
				string text;
				string val;

				text = val = null;
				if (text_field != null)
					text = DataBinder.GetPropertyValue (container, text_field, format);
				
				if (value_field != null)
					val = DataBinder.GetPropertyValue (container, value_field).ToString ();
				else if (text_field == null) {
					text = val = container.ToString ();
					if (format != null)
						text = String.Format (format, container);
				} else if (text != null)
					val = text;

				if (text == null)
					text = val;

				coll.Add (new ListItem (text, val));
			}

#if NET_2_0
		setselected:
			if (!String.IsNullOrEmpty (_selectedValue)) {
				if (!SetSelectedValue (_selectedValue))
					throw new ArgumentOutOfRangeException ("value", String.Format ("'{0}' has a SelectedValue which is invalid because it does not exist in the list of items.", ID));
				if (_selectedIndex >= 0 && _selectedIndex != SelectedIndex)
					throw new ArgumentException ("SelectedIndex and SelectedValue are mutually exclusive.");
			}
			else if (_selectedIndex >= 0) {
				SelectedIndex = _selectedIndex;
			}
#else
			if (saved_selected_value != null) {
				SelectedValue = saved_selected_value;
				if (saved_selected_index != -2 && saved_selected_index != SelectedIndex)
					throw new ArgumentException ("SelectedIndex and SelectedValue are mutually exclusive.");
			}
			else if (saved_selected_index != -2) {
				SelectedIndex = saved_selected_index;
				// No need to check saved_selected_value here, as it's done before.
			}
#endif
		}

#if NET_2_0
		[MonoTODO ("why override?")]
		protected override void PerformSelect ()
		{
			OnDataBinding (EventArgs.Empty);
			RequiresDataBinding = false;
			MarkAsDataBound ();
			OnDataBound (EventArgs.Empty);
		}

		protected internal override void RenderContents (HtmlTextWriter writer)
		{
			bool selected = false;
			Page page = Page;
			for (int i = 0; i < Items.Count; i++) {
				ListItem item = Items [i];
				if (page != null)
					page.ClientScript.RegisterForEventValidation (UniqueID, item.Value);
				writer.WriteBeginTag ("option");
				if (item.Selected) {
					if (selected)
						VerifyMultiSelect ();
					writer.WriteAttribute ("selected", "selected", false);
					selected = true;
				}
				writer.WriteAttribute ("value", item.Value, true);

				if (item.HasAttributes)
					item.Attributes.Render (writer);

				writer.Write (">");
				string encoded = HttpUtility.HtmlEncode (item.Text);
				writer.Write (encoded);
				writer.WriteEndTag ("option");
				writer.WriteLine ();
			}
		}

#endif		

		internal ArrayList GetSelectedIndicesInternal ()
		{
			ArrayList selected = null;
			int count;
			
			if (items != null && (count = items.Count) > 0) {
				selected = new ArrayList ();
				for (int i = 0; i < count; i++) {
					if (items [i].Selected)
						selected.Add (i);
				}
			}
			return selected;
		}

		protected override object SaveViewState ()
		{
			object baseState = null;
			object itemsState = null;

			baseState = base.SaveViewState ();

			IStateManager manager = items as IStateManager;
			if (manager != null)
				itemsState = manager.SaveViewState ();

			// .NET 2.0+ never returns null. It returns a Triplet with the Third member
			// set to an instance of ArrayList. Since we don't have a use (at least atm)
			// for this, we will just return a pair with both members null.
#if !NET_2_0
			if (baseState == null && itemsState == null)
				return null;
#endif
			return new Pair (baseState, itemsState);
		}

		protected override void LoadViewState (object savedState)
		{
			object baseState = null;
			object itemsState = null;

			Pair pair = savedState as Pair;
			if (pair != null) {
				baseState = pair.First;
				itemsState = pair.Second;
			}

			base.LoadViewState (baseState);

			if (itemsState != null) {
				IStateManager manager = Items as IStateManager;
				manager.LoadViewState (itemsState);
			}
		}
#if NET_2_0
		[MonoTODO ("Not implemented")]
		protected void SetPostDataSelection (int selectedIndex)
		{
			throw new NotImplementedException ();
		}
#endif		

		protected override void TrackViewState ()
		{
			base.TrackViewState ();
			IStateManager manager = items as IStateManager;
			if (manager != null)
				manager.TrackViewState ();
		}

		protected virtual void OnSelectedIndexChanged (EventArgs e)
		{
			EventHandler handler = (EventHandler) Events [SelectedIndexChangedEvent];
			if (handler != null)
				handler (this, e);
		}

#if NET_2_0		
		protected internal virtual void VerifyMultiSelect ()
		{
            throw new HttpException("Multi select is not supported");
		}
#endif		

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event EventHandler SelectedIndexChanged {
			add { Events.AddHandler (SelectedIndexChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedIndexChangedEvent, value); }
		}

#if NET_2_0
		/* sealed in the docs */
		public event EventHandler TextChanged {
			add {
				Events.AddHandler (TextChangedEvent, value);
			}
			remove {
				Events.RemoveHandler (TextChangedEvent, value);
			}
		}
		
		
		[Themeable (false)]
		[DefaultValue (false)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool CausesValidation {
			get {
				return ViewState.GetBool ("CausesValidation", false);
			}

			set {
				ViewState ["CausesValidation"] = value;
			}
		}

		[Themeable (false)]
		[DefaultValue ("")]
		[WebSysDescription ("")]
		[WebCategoryAttribute ("Behavior")]
		public virtual string ValidationGroup {
			get {
				return ViewState.GetString ("ValidationGroup", "");
			}
			set {
				ViewState ["ValidationGroup"] = value;
			}
		}

	
#endif
	}
}





