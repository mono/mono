//
// System.Web.UI.WebControls.RadioButtonList.cs
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
	[ValidationProperty("SelectedItem")]
	public class RadioButtonList : ListControl, IRepeatInfoUser, INamingContainer, IPostBackDataHandler
	{
		private bool  selectionIndexChanged;
		private short  tabIndex;

		public RadioButtonList(): base()
		{
			selectionIndexChanged = false;
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (-1), WebCategory ("Layout")]
		[WebSysDescription ("The border left within a RadioButton.")]
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
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value", "CellPadding value has to be -1 for 'not set' or > -1.");
				((TableStyle)ControlStyle).CellPadding = value;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (-1), WebCategory ("Layout")]
		[WebSysDescription ("The border left between RadioButtons.")]
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
			set {
				if (value < -1)
					throw new ArgumentOutOfRangeException ("value", "CellSpacing value has to be -1 for 'not set' or > -1.");
				((TableStyle)ControlStyle).CellSpacing = value;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (0), WebCategory ("Layout")]
		[WebSysDescription ("The number of columns that should be used to display the RadioButtons.")]
		public virtual int RepeatColumns
		{
			get
			{
				object o = ViewState["RepeatColumns"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value", "RepeatColumns value has to be 0 for 'not set' or > 0.");
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
				if(o != null)
					return (RepeatDirection)o;
				return RepeatDirection.Vertical;
			}
			set
			{
				if(!Enum.IsDefined(typeof(RepeatDirection), value))
					throw new ArgumentOutOfRangeException ("value", "Only valid enumeration members are allowed");
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
				if(o != null)
					return (RepeatLayout)o;
				return RepeatLayout.Table;
			}
			set
			{
				if(!Enum.IsDefined(typeof(RepeatLayout), value))
					throw new ArgumentOutOfRangeException ("value", "Only valid enumeration members are allowed");
				ViewState["RepeatLayout"] = value;
			}
		}

#if !NET_2_0
		[Bindable (true)]
#endif
		[DefaultValue (typeof (TextAlign), "Right"), WebCategory ("Appearance")]
		[WebSysDescription ("The alignment of the RadioButton text.")]
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
					throw new ArgumentOutOfRangeException ("value", "Only valid enumeration members are allowed");
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
			info.RepeatLayout = RepeatLayout;
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

#if NET_2_0
		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}
		
		protected virtual bool LoadPostData(string postDataKey, NameValueCollection postCollection)
#else
		bool IPostBackDataHandler.LoadPostData(string postDataKey, NameValueCollection postCollection)
#endif
		{
			string value = postCollection [postDataKey];
			int c = Items.Count;
			for (int i = 0; i < c; i++) {
				if (Items [i].Value != value)
					continue;

				if (i != SelectedIndex) {
					SelectedIndex = i;
					selectionIndexChanged = true;
				}

				return true;
			}

			return false;
		}

#if NET_2_0
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}
		
		protected virtual void RaisePostDataChangedEvent ()
		{
			if (CausesValidation)
				Page.Validate (ValidationGroup);

			if(selectionIndexChanged)
				OnSelectedIndexChanged(EventArgs.Empty);
		}
#else
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			if(selectionIndexChanged)
				OnSelectedIndexChanged(EventArgs.Empty);
		}
#endif
		

#if NET_2_0
		Style IRepeatInfoUser.GetItemStyle(System.Web.UI.WebControls.ListItemType itemType, int repeatIndex)
		{
			return GetItemStyle (itemType, repeatIndex);
		}
		protected virtual Style GetItemStyle(System.Web.UI.WebControls.ListItemType itemType, int repeatIndex)
		{
			return null;
		}
#else
		Style IRepeatInfoUser.GetItemStyle(System.Web.UI.WebControls.ListItemType itemType, int repeatIndex)
		{
			return null;
		}
#endif

#if NET_2_0
		void IRepeatInfoUser.RenderItem (System.Web.UI.WebControls.ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
		{
			RenderItem (itemType, repeatIndex, repeatInfo, writer);
		}
		
		protected virtual void RenderItem (System.Web.UI.WebControls.ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
#else
		void IRepeatInfoUser.RenderItem (System.Web.UI.WebControls.ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
#endif
		{
			/* Create a new RadioButton as if it was defined in the page and render it */
			RadioButton button = new RadioButton ();
			button.Page = Page;
			button.GroupName = UniqueID;
			button.TextAlign = TextAlign;
			button.AutoPostBack = AutoPostBack;
			button.ID = ClientID + "_" + repeatIndex.ToString (NumberFormatInfo.InvariantInfo);;
			button.TabIndex = tabIndex;
			ListItem current = Items [repeatIndex];
			button.Text = current.Text;
			button.Attributes ["value"] = current.Value;
			button.Checked = current.Selected;
			button.Enabled = Enabled;
			button.RenderControl (writer);
		}

#if NET_2_0
		bool IRepeatInfoUser.HasFooter {
			get { return HasFooter; }
		}

		protected virtual bool HasFooter {
			get { return false; }
		}
#else
		bool IRepeatInfoUser.HasFooter {
			get { return false; }
		}
#endif

#if NET_2_0
		bool IRepeatInfoUser.HasHeader {
			get { return HasHeader; }
		}

		protected virtual bool HasHeader {
			get { return false; }
		}
#else
		bool IRepeatInfoUser.HasHeader {
			get { return false; }
		}
#endif

#if NET_2_0
		bool IRepeatInfoUser.HasSeparators {
			get { return HasSeparators; }
		}

		protected virtual bool HasSeparators {
			get { return false; }
		}
#else
		bool IRepeatInfoUser.HasSeparators {
			get { return false; }
		}
#endif

#if NET_2_0
		int IRepeatInfoUser.RepeatedItemCount {
			get { return RepeatedItemCount; }
		}

		protected virtual int RepeatedItemCount {
			get { return Items.Count; }
		}
#else
		int IRepeatInfoUser.RepeatedItemCount {
			get { return Items.Count; }
		}
#endif
	}
}
