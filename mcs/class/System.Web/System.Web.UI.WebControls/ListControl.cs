/**
 * Namespace: System.Web.UI.WebControls
 * Class:     ListControl
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.UI.WebControls
{
	[DefaultEvent("SelectedIndexChanged")]
	[DefaultProperty("DataSource")]
	//[Designer("??")]
	//[DataBindingHandler("??")]
	[ParseChildren(true, "Items")]
	public abstract class ListControl: WebControl
	{
		private static readonly object SelectedIndexChangedEvent = new object();

		private object             dataSource;
		private ListItemCollection items;

		private int cachedSelectedIndex = -1;

		public ListControl(): base(HtmlTextWriterTag.Select)
		{
		}

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

		public virtual object DataSource
		{
			get
			{
				return dataSource;
			}
			set
			{
				if(value != null)
				{
					if(value is IListSource || value is IEnumerable)
					{
						dataSource = value;
						return;
					}
				}
				throw new ArgumentException(HttpRuntime.FormatResourceString(ID, "Invalid DataSource Type"));
			}
		}

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

		public virtual ListItemCollection Items
		{
			get
			{
				if(items==null)
				{
					items = new ListItemCollection();
					if(IsTrackingViewState)
					{
						items.TrackViewState();
					}
				}
				return items;
			}
		}

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
				if (value < -1 || value > Items.Count)
					throw new ArgumentOutOfRangeException ();

				ClearSelection ();
				if (value != -1)
					Items [value].Selected = true;
			}
		}

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

		protected override void OnDataBinding(EventArgs e)
		{
			base.OnDataBinding(e);
			IEnumerable ds = DataSourceHelper.GetResolvedDataSource (DataSource, DataMember);

			if(ds != null) {
				string dtf = DataTextField;
				string dvf = DataValueField;
				string dtfs = DataTextFormatString;
				if (dtfs.Length == 0)
					dtfs = "{0}";

				Items.Clear();

				bool useProperties = (dtf.Length > 0 && dvf.Length > 0);

				foreach (object current in ds) {
					ListItem li = new ListItem();
					if (!useProperties){
						li.Text  = String.Format (dtfs, current);
						li.Value = current.ToString ();
						Items.Add (li);
						continue;
					}

					object o;
					if (dtf.Length > 0) {
						o = DataBinder.GetPropertyValue (current, dtf, null);
						li.Text = o.ToString ();
					}

					if (dvf.Length > 0) {
						o = DataBinder.GetPropertyValue (current, dvf, null);
						li.Value = o.ToString ();
					}

					Items.Add(li);
				}
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
	}
}
