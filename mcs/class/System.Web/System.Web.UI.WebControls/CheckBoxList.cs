/**
* Namespace: System.Web.UI.WebControls
* Class:     CheckBoxList
*
* Author:  Gaurav Vaish
* Maintainer: gvaish@iitk.ac.in
* Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
* Implementation: yes
* Status:  80%
*
* (C) Gaurav Vaish (2001)
*/

using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class CheckBoxList: ListControl, IRepeatInfoUser, INamingContainer, IPostBackDataHandler
	{
		CheckBox checkBoxRepeater;
		bool     isChangeNotified;
		
		public CheckBoxList()
		{
			checkBoxRepeater = new CheckBox();
			checkBoxRepeater.ID = "0";
			checkBoxRepeater.EnableViewState = false;
			checkBoxRepeater.Controls.Add(this);
			isChangeNotified = false;
		}
		
		public virtual int CellPadding
		{
			get
			{
				return (ControlStyleCreated ? ((TableStyle)ControlStyle).CellPadding : -1);
			}
			set
			{
				((TableStyle)ControlStyle).CellPadding = value;
			}
		}
		
		public virtual int CellSpacing
		{
			get
			{
				return (ControlStyleCreated ? ((TableStyle)ControlStyle).CellSpacing : -1);
			}
			set
			{
				((TableStyle)ControlStyle).CellSpacing = value;
			}
		}
		
		public virtual int RepeatColumns
		{
			get
			{
				object o = ViewState["RepeatColumns"];
				if(o!=null)
					return (int)o;
				return 0;
			}
			set
			{
				if(value < 0)
					throw new ArgumentOutOfRangeException();
				ViewState["RepeatColumns"] = value;
			}
		}
		
		public virtual RepeatDirection RepeatDirection
		{
			get
			{
				object o = ViewState["RepeatDirection"];
				if(o!=null)
					return (RepeatDirection)o;
				return RepeatDirection.Vertical;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(RepeatDirection),value))
					throw new ArgumentException();
				ViewState["RepeatDirection"] = value;
			}
		}
		
		public virtual RepeatLayout RepeatLayout
		{
			get
			{
				object o = ViewState["RepeatLayout"];
				if(o!=null)
					return (RepeatLayout)o;
				return RepeatLayout.Table;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(RepeatLayout), value))
					throw new ArgumentException();
				ViewState["RepeatLayout"] = value;
			}
		}
		
		public virtual TextAlign TextAlign
		{
			get
			{
				object o = ViewState["TextAlign"];
				if(o!=null)
					return (TextAlign)o;
				return TextAlign.Right;
			}
			set
			{
				if(!Enum.IsDefined(typeof(TextAlign), value))
					throw new ArgumentException();
				ViewState["TextAlign"] = value;
			}
		}
		
		protected override Style CreateControlStyle()
		{
			// I have to return a TableStyle
			return new TableStyle(ViewState);
		}
		
		protected override Control FindControl(string id, int pathOffset)
		{
			return this;
		}
		
		protected override void OnPreRender(EventArgs e)
		{
			checkBoxRepeater.AutoPostBack = AutoPostBack;
			if(Page!=null)
			{
				for(int i=0; i < Items.Count; i++)
				{
					if(Items[i].Selected)
					{
						// register each selected with the ID
						checkBoxRepeater.ID = i.ToString(NumberFormatInfo.InvariantInfo);
						Page.RegisterRequiresPostBack(checkBoxRepeater);
					}
				}
			}
		}
		
		[MonoTODO]
		protected override void Render(HtmlTextWriter writer)
		{
			throw new NotImplementedException();
		}
		
		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			int index = Int32.Parse(postDataKey.Substring(UniqueID.Length + 1));
			if(index >= 0 && index < Items.Count)
			{
				bool exists = (postCollection[postDataKey]!=null);
				if(Items[index].Selected != exists)
				{
					Items[index].Selected = exists;
					if(!isChangeNotified)
					{
						isChangeNotified = true;
						return true;
					}
				}
			}
			return false;
		}
		
		[MonoTODO]
		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
			throw new NotImplementedException();
		}
		
		bool IRepeatInfoUser.HasFooter
		{
			get
			{
				return false;
			}
		}
		
		bool IRepeatInfoUser.HasHeader
		{
			get
			{
				return false;
			}
		}
		
		bool IRepeatInfoUser.HasSeparators
		{
			get
			{
				return false;
			}
		}
		
		int IRepeatInfoUser.RepeatedItemCount
		{
			get
			{
				return Items.Count;
			}
		}
		
		// I don't need this
		Style IRepeatInfoUser.GetItemStyle(ListItemType itemType, int repeatIndex)
		{
			return null;
		}
		
		void IRepeatInfoUser.RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
		{
			checkBoxRepeater.ID = repeatIndex.ToString(NumberFormatInfo.InvariantInfo);
			checkBoxRepeater.Text = Items[repeatIndex].Text;
			checkBoxRepeater.TextAlign = TextAlign;
			checkBoxRepeater.Checked = Items[repeatIndex].Selected;
			checkBoxRepeater.RenderControl(writer);
		}
	}
}
