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

namespace System.Web.UI.WebControls
{
	public class ListControl: WebControl
	{
		private static readonly object SelectedIndexChangedEvent = new object();

		private object dataSource;
		private ListItemCollection items;
		
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
		
		internal void Select(ArrayList whatToSelect)
		{
			ClearSelection();
			int i = 0;
			while(i < whatToSelect.Count)
			{
				int index = (int)whatToSelect[i++];				
				if(index > 0)
				{
					Items[index].Selected = true;
				}
			}
		}
		
		public virtual void ClearSelection()
		{
			//TODO: Found it - an undocumented method
		}
		
		protected override void LoadViewState(object savedState)
		{
			//TODO: Implement me
			throw new NotImplementedException();
		}
	}
}
