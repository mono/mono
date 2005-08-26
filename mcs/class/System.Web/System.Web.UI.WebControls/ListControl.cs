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
	[DefaultPropertyAttribute ("DataSource")]
	[Designer("System.Web.UI.Design.WebControls.ListControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#if NET_2_0
	[ControlValueProperty ("SelectedValue", null)]
	[ParseChildrenAttribute (true, "Items", ChildControlType = typeof (Control))]
#else
	[ParseChildrenAttribute (true, "Items")]
#endif	
	public abstract class ListControl :
#if NET_2_0
	DataBoundControl, IEditableTextControl, ITextControl
#else		
	WebControl
#endif
	{

		private static readonly object SelectedIndexChangedEvent = new object ();
#if NET_2_0
		private static readonly object TextChangedEvent = new object ();
#endif

		private object data_source;
		private ListItemCollection items;
		
		public ListControl () : base (HtmlTextWriterTag.Select)
		{
		}

#if NET_2_0
		[DefaultValue (false)]
		[Themeable (false)]
		[MonoTODO]
		public virtual bool AppendDataBoundItems
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
#endif		
		
#if NET_2_0
		[Themeable (false)]
#endif
		[DefaultValue(false)]
		public virtual bool AutoPostBack {
			get { return ViewState.GetBool ("AutoPostBack", false); }
			set { ViewState ["AutoPostBack"] = value; }
		}

#if ONLY_1_1
		[DefaultValue("")]
		public virtual string DataMember {
			get { return ViewState.GetString ("DataMember", String.Empty); }
			set { ViewState ["DataMember"] = value; }
		}

		[Bindable(true)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual object DataSource {
			get { return data_source; }
			set { data_source = value; }
		}
#endif		

#if NET_2_0
		[Themeable (false)]
#endif		
		[DefaultValue("")]
		public virtual string DataTextField {
			get { return ViewState.GetString ("DataTextField", String.Empty); }
			set { ViewState ["DataTextField"] = value; }
		}

#if NET_2_0
		[Themeable (false)]
#endif		
		[DefaultValue("")]
		public virtual string DataTextFormatString {
			get { return ViewState.GetString ("DataTextFormatString", String.Empty); }
			set { ViewState ["DataTextFormatString"] = value; }
		}

#if NET_2_0
		[Themeable (false)]
#endif		
		[DefaultValue("")]
		public virtual string DataValueField {
			get { return ViewState.GetString ("DataValueField", String.Empty); }
			set { ViewState ["DataValueField"] = value; }
		}

#if NET_2_0
		[Editor ("System.Web.UI.Design.WebControls.ListItemsCollectionEditor," + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
#endif		
		[DefaultValue(null)]
		[MergableProperty(false)]
		[PersistenceMode(PersistenceMode.InnerDefaultProperty)]
		public virtual ListItemCollection Items {
			get {
				if (items == null)
					items = new ListItemCollection ();
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
				ClearSelection ();
				if (value == -1 || items == null)
					return;
				if (value < 0 || value >= Items.Count)
					throw new ArgumentOutOfRangeException ("value");
				items [value].Selected = true;
			}
		}

		[Browsable(false)]
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
		public virtual string SelectedValue {
			get {
				int si = SelectedIndex;
				if (si == -1)
					return String.Empty;
				return Items [si].Value;
			}
			set {
				ClearSelection ();
				for (int i = 0; i < Items.Count; i++) {
					if (Items [i].Value == value)
						Items [i].Selected = true;
				}
			}
		}

#if NET_2_0
		[Themeable (false)]
		[DefaultValue ("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[MonoTODO]
		public virtual string Text 
		{
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		protected virtual new HtmlTextWriterTag TagKey
		{
			get {
				throw new NotImplementedException ();
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

#if NET_2_0
		[MonoTODO]
		protected internal override void LoadControlState (object savedState)
		{
			throw new NotImplementedException ();
		}
#endif		

		protected override void OnDataBinding (EventArgs e)
		{
			base.OnDataBinding (e);

			IEnumerable list = DataSourceResolver.ResolveDataSource (DataSource, DataMember);
			if (list == null)
				return;

			Items.Clear ();

			foreach (object container in list) {
				string text;
				string value;

				if (DataTextField != String.Empty) {
					text = DataBinder.Eval (container,
							DataTextField).ToString ();
				} else {
					text = String.Empty;
				}

				if (DataValueField != String.Empty) {
					value = DataBinder.Eval (container,
							DataValueField).ToString ();
				} else {
					value = text;
				}

				if (text == String.Empty) {
					if (value != String.Empty)
						text = value;
				} else if (DataTextFormatString != String.Empty) {
					// Dont apply the format string if we don't actually 
					// have a textfield
					text = String.Format (DataTextFormatString, text);
				}

				ListItem item = new ListItem (text, value);
				Items.Add (item);
			}
		}

#if NET_2_0
		[MonoTODO]
		protected internal override void OnInit (EventArgs e)
		{
			throw new NotImplementedException ();
		}
#endif		

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}

#if NET_2_0
		[MonoTODO]
		protected virtual void OnTextChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void PerformDataBinding (IEnumerable dataSource)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void PerformSelect ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override void RenderContents (HtmlTextWriter w)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected internal override object SaveControlState ()
		{
			throw new NotImplementedException ();
		}
#endif		

		protected override object SaveViewState ()
		{
			object first = null;
			object second = null;
			ArrayList selected = null;

			first = base.SaveViewState ();

			IStateManager manager = items as IStateManager;
			if (manager != null)
				second = manager.SaveViewState ();

			if (items != null) {
				selected = new ArrayList ();
				int count = Items.Count;
				for (int i = 0; i < count; i++) {
					if (items [i].Selected)
						selected.Add (i);
				}
			}

			if (first == null && second == null && selected == null)
				return null;

			return new Triplet (first, second, selected);
		}

		protected override void LoadViewState (object savedState)
		{
			object first = null;
			object second = null;
			ArrayList indices = null;

			Triplet triplet = savedState as Triplet;
			if (triplet != null) {
				first = triplet.First;
				second = triplet.Second;
				indices = triplet.Third as ArrayList;
			}

			base.LoadViewState (first);

			if (second != null) {
				IStateManager manager = Items as IStateManager;
				manager.LoadViewState (second);
			}

			if (indices != null) {
				foreach (int index in indices)
					Items [index].Selected = true;
			}
		}

#if NET_2_0
		[MonoTODO]
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
		}
#endif		

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
		
		
		[MonoTODO]
		[Themeable (false)]
		[DefaultValue (false)]
	        public virtual bool CausesValidation {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();	
			}
		}

		[MonoTODO]
		[Themeable (false)]
		[DefaultValue ("")]
		public virtual string ValidationGroup {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}

	
#endif
	}
}

