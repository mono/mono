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

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (-1), WebCategory ("Layout")]
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

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (-1), WebCategory ("Layout")]
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

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (0), WebCategory ("Layout")]
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

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof (RepeatDirection), "Vertical"), WebCategory ("Layout")]
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

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof (RepeatLayout), "Table"), WebCategory ("Layout")]
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

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof (TextAlign), "Right"), WebCategory ("Appearance")]
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
			if(TabIndex != 0)
			{
				if(!ViewState.IsItemDirty("TabIndex"))
					dirtyFlag = true;
				TabIndex = 0;
			}
			ri.RepeatColumns = RepeatColumns;
			ri.RepeatLayout  = RepeatLayout;
			ri.RepeatDirection = RepeatDirection;
			ri.RenderRepeater(writer, this, s, this);
			if(tTabIndex != 0)
			{
				TabIndex = tTabIndex;
			}
			if(dirtyFlag)
			{
				ViewState.SetItemDirty("TabIndex", false);
			}
		}

#if NET_2_0
		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			LoadPostData (postDataKey, postCollection);
		}
		
		protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
#else
		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
#endif
		{
			if (!Enabled)
				return false;

			int index = Int32.Parse(postDataKey.Substring(UniqueID.Length + 1));
			if(index >= 0 && index < Items.Count)
			{
				string v = postCollection [postDataKey];
				bool exists = (v != null);
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

#if NET_2_0
		void IPostBackDataHandler.RaisePostDataChangedEvent()
		{
			RaisePostDataChangedEvent ();
		}
		
		protected virtual void RaisePostDataChangedEvent()
#else
		void IPostBackDataHandler.RaisePostDataChangedEvent()
#endif
		{
			OnSelectedIndexChanged(EventArgs.Empty);
		}


#if NET_2_0
		bool IRepeatInfoUser.HasFooter {
			get { return HasFooter; }
		}
		
		protected virtual bool HasFooter
#else
		bool IRepeatInfoUser.HasFooter
#endif
		{
			get
			{
				return false;
			}
		}

#if NET_2_0
		bool IRepeatInfoUser.HasHeader {
			get { return HasHeader; }
		}
		
		protected virtual bool HasHeader
#else
		bool IRepeatInfoUser.HasHeader
#endif
		{
			get
			{
				return false;
			}
		}

#if NET_2_0
		bool IRepeatInfoUser.HasSeparators {
			get { return HasSeparators; }
		}
		
		protected virtual bool HasSeparators
#else
		bool IRepeatInfoUser.HasSeparators
#endif
		{
			get
			{
				return false;
			}
		}

#if NET_2_0
		int IRepeatInfoUser.RepeatedItemCount {
			get { return RepeatedItemCount; }
		}
		
		protected virtual int RepeatedItemCount
#else
		int IRepeatInfoUser.RepeatedItemCount
#endif
		{
			get
			{
				return Items.Count;
			}
		}

#if NET_2_0
		Style IRepeatInfoUser.GetItemStyle(ListItemType itemType, int repeatIndex)
		{
			return GetItemStyle (itemType, repeatIndex);
		}
		
		protected virtual Style GetItemStyle(ListItemType itemType, int repeatIndex)
#else
		Style IRepeatInfoUser.GetItemStyle(ListItemType itemType, int repeatIndex)
#endif
		{
			return null;
		}

#if NET_2_0
		void IRepeatInfoUser.RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
		{
			RenderItem (itemType, repeatIndex, repeatInfo, writer);
		}
		
		protected virtual void RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
#else
		void IRepeatInfoUser.RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
#endif
		{
			checkBoxRepeater.ID = repeatIndex.ToString(NumberFormatInfo.InvariantInfo);
			checkBoxRepeater.Text = Items[repeatIndex].Text;
			checkBoxRepeater.TextAlign = TextAlign;
			checkBoxRepeater.Checked = Items[repeatIndex].Selected;
			checkBoxRepeater.Enabled = Enabled;
			checkBoxRepeater.RenderControl(writer);
		}
	}
}
