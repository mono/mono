/**
 * Namespace: System.Web.UI.WebControls
 * Class:     RadioButtonList
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  95%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[ValidationProperty("SelectedItem")]
	public class RadioButtonList : ListControl, IRepeatInfoUser, INamingContainer, IPostBackDataHandler
	{
		private bool  selectionIndexChanged;
		private short  tabIndex;

		public RadioButtonList(): base()
		{
			selectionIndexChanged = false;
		}

		public virtual int CellPadding
		{
			get
			{
				if(ControlStyleCreated)
				{
					return (int)(((TableStyle)ControlStyle).CellPadding);
				}
				return -1;
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
				if(ControlStyleCreated)
				{
					return (int)(((TableStyle)ControlStyle).CellSpacing);
				}
				return -1;
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
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				if(value < 0)
					throw new ArgumentOutOfRangeException("value");
				ViewState["RepeatColumns"] = value;
			}
		}

		public virtual RepeatDirection RepeatDirection
		{
			get
			{
				object o = ViewState["RepeatDirection"];
				if(o != null)
					return (RepeatDirection)o;
				return RepeatDirection.Vertical;
			}
			set
			{
				if(!Enum.IsDefined(typeof(RepeatDirection), value))
					throw new ArgumentException();
				ViewState["RepeatDirection"] = value;
			}
		}

		public virtual RepeatLayout RepeatLayout
		{
			get
			{
				object o = ViewState["RepeatLayout"];
				if(o != null)
					return (RepeatLayout)o;
				return RepeatLayout.Table;
			}
			set
			{
				if(!Enum.IsDefined(typeof(RepeatLayout), value))
					throw new ArgumentException();
				ViewState["RepeatLayout"] = value;
			}
		}

		public virtual TextAlign TextAlign
		{
			get
			{
				object o = ViewState["TextAlign"];
				if(o != null)
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

		protected override void Render(HtmlTextWriter writer)
		{
			RepeatInfo info = new RepeatInfo();
			Style cStyle = (ControlStyleCreated ? ControlStyle : null);
			bool dirty = false;
			tabIndex = TabIndex;
			if(tabIndex != 0)
			{
				dirty = !ViewState.IsItemDirty("TabIndex");
				TabIndex = 0;
			}
			info.RepeatColumns = RepeatColumns;
			info.RepeatDirection = RepeatDirection;
			info.RenderRepeater(writer, this, cStyle, this);
			if(tabIndex != 0)
			{
				TabIndex = tabIndex;
			}
			if(dirty)
			{
				ViewState.SetItemDirty("TabIndex", false);
			}
		}

		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			string value = postCollection[postDataKey];
			for(int i=0; i < Items.Count; i++)
			{
				if(Items[i].Value == value)
				{
					if(i != SelectedIndex)
					{
						SelectedIndex = i;
					}
					return true;
				}
			}
			return false;
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
			if(selectionIndexChanged)
				OnSelectedIndexChanged(EventArgs.Empty);
		}

		Style IRepeatInfoUser.GetItemStyle(System.Web.UI.WebControls.ListItemType itemType, int repeatIndex)
		{
			return null;
		}

		void IRepeatInfoUser.RenderItem (System.Web.UI.WebControls.ListItemType itemType,
						 int repeatIndex,
						 RepeatInfo repeatInfo,
						 HtmlTextWriter writer)
		{
			/* Create a new RadioButton as if it was defined in the page and render it */
			RadioButton button = new RadioButton ();
			button.GroupName = UniqueID;
			button.TextAlign = TextAlign;
			button.AutoPostBack = AutoPostBack;
			button.ID = ClientID + "_" + repeatIndex.ToString (NumberFormatInfo.InvariantInfo);;
			object view_state = ViewState ["TabIndex"];
			if (view_state != null)
				button.TabIndex = (short) view_state;
			ListItem current = Items [repeatIndex];
			button.Text = current.Text;
			button.Attributes ["value"] = current.Value;
			button.Checked = current.Selected;
			button.RenderControl (writer);
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
	}
}
