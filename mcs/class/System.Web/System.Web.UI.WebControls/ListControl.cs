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
	[Designer("System.Web.UI.Design.WebControls.ListControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ParseChildrenAttribute (true, "Items")]
	[ControlValueProperty ("SelectedValue", null)]
	public abstract class ListControl : DataBoundControl, IEditableTextControl, ITextControl
	{

		static readonly object SelectedIndexChangedEvent = new object ();
		static readonly object TextChangedEvent = new object ();

		ListItemCollection items;
		int _selectedIndex = -2;
		string _selectedValue;

		public ListControl () : base (HtmlTextWriterTag.Select)
		{
		}

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
		
		[Themeable (false)]
		[DefaultValue(false)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public virtual bool AutoPostBack {
			get { return ViewState.GetBool ("AutoPostBack", false); }
			set { ViewState ["AutoPostBack"] = value; }
		}

		[Themeable (false)]
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Data")]
		public virtual string DataTextField {
			get { return ViewState.GetString ("DataTextField", String.Empty); }
			set { 
				ViewState ["DataTextField"] = value;
				if (Initialized)
					RequiresDataBinding = true;
			}
		}

		[Themeable (false)]
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Data")]
		public virtual string DataTextFormatString {
			get { return ViewState.GetString ("DataTextFormatString", String.Empty); }
			set { 
				ViewState ["DataTextFormatString"] = value;
				if (Initialized)
					RequiresDataBinding = true;
			}
		}

		[Themeable (false)]
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Data")]
		public virtual string DataValueField {
			get { return ViewState.GetString ("DataValueField", String.Empty); }
			set { 
				ViewState ["DataValueField"] = value;
				if (Initialized)
					RequiresDataBinding = true;
			}
		}

		[Editor ("System.Web.UI.Design.WebControls.ListItemsCollectionEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
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
		[Themeable (false)]
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

		[Bindable(true, BindingDirection.TwoWay)]
		[Themeable (false)]
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
				_selectedValue = value;
				SetSelectedValue (value);
			}
		}

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
			IEnumerable list = GetData ().ExecuteSelect (DataSourceSelectArguments.Empty);
			InternalPerformDataBinding (list);
		}

		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			Page page = Page;
			if (page != null && IsEnabled)
				page.RegisterEnabledControl (this);
		}

		protected virtual void OnTextChanged (EventArgs e)
		{
			EventHandler handler = (EventHandler) Events [TextChangedEvent];
			if (handler != null)
				handler (this, e);
		}

		protected internal override void PerformDataBinding (IEnumerable dataSource)
		{
			if (dataSource == null)
				goto setselected;
			if (!AppendDataBoundItems)
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
		}

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

		[MonoTODO ("Not implemented")]
		protected void SetPostDataSelection (int selectedIndex)
		{
			throw new NotImplementedException ();
		}

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

		protected internal virtual void VerifyMultiSelect ()
		{
			if (!MultiSelectOk ())
				throw new HttpException("Multi select is not supported");
		}

		internal virtual bool MultiSelectOk ()
		{
			return false;
		}

		[WebSysDescription ("")]
		[WebCategory ("Action")]
		public event EventHandler SelectedIndexChanged {
			add { Events.AddHandler (SelectedIndexChangedEvent, value); }
			remove { Events.RemoveHandler (SelectedIndexChangedEvent, value); }
		}

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
	}
}





