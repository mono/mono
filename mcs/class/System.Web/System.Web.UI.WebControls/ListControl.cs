/**
 * Namespace: System.Web.UI.WebControls
 * Class:     ListControl
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  10%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Web.Utils;

namespace System.Web.UI.WebControls
{
	public class ListControl: WebControl
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
		
		protected virtual void OnSelectedIndexChanged(EventArgs e)
		{
			if(Events!=null)
				{
					EventHandler eh = (EventHandler)(Events[SelectedIndexChangedEvent]);
					if(eh!=null)
						eh(this, e);
				}
		}
		
		protected override void OnDataBinding(EventArgs e)
		{
			base.OnDataBinding(e);
			IEnumerable resolvedData = DataSourceHelper.GetResolvedDataSource(DataSource, DataMember);
			if(resolvedData != null)
			{
				string dataTextField = DataTextField;
				string dataValueField = DataValueField;
				Items.Clear();
				ICollection rdsCollection = resolvedDataSource as ICollection;
				if(rdsCollection != null)
				{
					Items.Capacity = rdsCollection.Count;
				}
				bool valid = ( (dataTextField.Length >= 0) && (dataValueField.Length >=0) );
				foreach(IEnumerable current in resolvedDataSource.GetEnumerator())
				{
					ListItem li = new ListItem();
					if(valid)
					{
						if(dataTextField.Length >= 0)
						{
							li.Text = DataBinder.GetPropertyValue(current, dataTextField, null);
						}
						if(dataValueField.Length >= 0)
						{
							li.Value = DataBinder.GetPropertyValue(current, dataValueField, null);
						}
					} else
					{
						li.Text  = dataTextField.ToString();
						li.Value = dataValueField.ToString();
					}
					Items.Add(li);
				}
			}
			if(cachedSelectedIndex != -1)
			{
				SelectedIndex = cachedSelectedIndex;
				cachedSelectedIndex = -1;
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
				throw new ArgumentException(/*HttpRuntime.FormatResourceString(ID, "Invalid DataSource Type")*/);
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
						//items.TrackViewState();
					}
				}
				return items;
			}
		}
		
		public virtual int SelectedIndex
		{
			get
			{
				object o = ViewState["SelectedIndex"];
				if(o!=null)
					return (int)o;
				return -1;
			}
			set
			{
				ViewState["SelectedIndex"] = value;
			}
		}
		
		internal virtual int[] SelectedIndices
		{
			get
			{
				ArrayList si = new ArrayList();
				for(int i=0; i < Items.Count; i++)
				{
					if(Items[i].Selected)
						ArrayList.Add(i);
				}
				int[] indices = (int[])si.ToArray();
			}
		}
		
		public virtual ListItem SelectedItem
		{
			get
			{
				if(SelectedIndex > 0)
				{
					return Items[SelectedIndex];
				}
				return null;
			}
		}
		
		internal virtual ArrayList SelectedIndexes
		{
			get
			{
				ArrayList retVal = new ArrayList();
				int index = 0;
				while(index < Items.Count)
				{
					retVal.Add(Items[index++]);
				}
				return retVal;
			}
		}
		
		internal void Select(int[] indices)
		{
			ClearSelection();
			foreach(int index in indices)
			{
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
			//TODO: Implement me
			throw new NotImplementedException();
		}
		
		protected override object SaveViewState()
		{
			object vs = base.SaveViewState();
			object indices = null;
			if( Events[SelectedIndexChangedEvent] != null && Enabled && Visible)
				indices = SelectedIndices;
			if(indices != null)
		}
	}
}
