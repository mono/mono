//
// System.Web.UI.WebControls.CheckBoxList.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.Collections.Specialized;
using System.ComponentModel;
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
			Controls.Add (checkBoxRepeater);
			isChangeNotified = false;
		}

		[DefaultValue (-1), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The border left within a CheckBox.")]
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

		[DefaultValue (-1), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The border left between CheckBoxes.")]
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

		[DefaultValue (0), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The number of columns that should be used to display the CheckBoxes.")]
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

		[DefaultValue (typeof (RepeatDirection), "Vertical"), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The direction that is followed when doing the layout.")]
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

		[DefaultValue (typeof (RepeatLayout), "Table"), Bindable (true), WebCategory ("Layout")]
		[WebSysDescription ("The method used to create the layout.")]
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

		[DefaultValue (typeof (TextAlign), "Right"), Bindable (true), WebCategory ("Appearance")]
		[WebSysDescription ("The alignment of the CheckBox text.")]
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
						checkBoxRepeater.ID = i.ToString(NumberFormatInfo.InvariantInfo);
						Page.RegisterRequiresPostBack(checkBoxRepeater);
					}
				}
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			RepeatInfo ri = new RepeatInfo();
			checkBoxRepeater.TabIndex = TabIndex;
			bool dirtyFlag = false;
			short  tTabIndex = TabIndex;
			Style s = (ControlStyleCreated ? ControlStyle : null);
			if(TabIndex > 0)
			{
				if(!ViewState.IsItemDirty("TabIndex"))
					dirtyFlag = true;
				TabIndex = 0;
			}
			ri.RepeatColumns = RepeatColumns;
			ri.RepeatLayout  = RepeatLayout;
			ri.RepeatDirection = RepeatDirection;
			ri.RenderRepeater(writer, this, s, this);
			if(tTabIndex > 0)
			{
				TabIndex = tTabIndex;
			}
			if(dirtyFlag)
			{
				ViewState.SetItemDirty("TabIndex", false);
			}
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

		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
			OnSelectedIndexChanged(EventArgs.Empty);
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
