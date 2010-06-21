//
// System.Web.UI.WebControls.CheckBoxList.cs
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

using System.Globalization;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Security.Permissions;
using System.Web.Util;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class CheckBoxList : ListControl, IRepeatInfoUser, INamingContainer, IPostBackDataHandler 
	{
		CheckBox check_box;

		public CheckBoxList ()
		{
			check_box = new CheckBox ();
			Controls.Add (check_box);
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(-1)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual int CellPadding {
			get { return TableStyle.CellPadding; }
			set { TableStyle.CellPadding = value; }
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(-1)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual int CellSpacing {
			get { return TableStyle.CellSpacing; }
			set { TableStyle.CellSpacing = value; }
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(0)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual int RepeatColumns {
			get { return ViewState.GetInt ("RepeatColumns", 0); }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["RepeatColumns"] = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(RepeatDirection.Vertical)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual RepeatDirection RepeatDirection {
			get {
				return (RepeatDirection) ViewState.GetInt ("RepeatDirection",
						(int) RepeatDirection.Vertical);
			}
			set {
				if (value < RepeatDirection.Horizontal ||
						value > RepeatDirection.Vertical)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["RepeatDirection"] = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(RepeatLayout.Table)]
		[WebSysDescription ("")]
		[WebCategory ("Layout")]
		public virtual RepeatLayout RepeatLayout {
			get {
				return (RepeatLayout) ViewState.GetInt ("RepeatLayout",
						(int) RepeatLayout.Table);
			}
			set {
				if (value < RepeatLayout.Table ||
						value > RepeatLayout.Flow)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["RepeatLayout"] = value;
			}
		}

#if ONLY_1_1
		[Bindable(true)]
#endif		
		[DefaultValue(TextAlign.Right)]
		[WebSysDescription ("")]
		[WebCategory ("Appearance")]
		public virtual TextAlign TextAlign {
			get {
				return (TextAlign) ViewState.GetInt ("TextAlign",
						(int) TextAlign.Right);
			}
			set {
				if (value < TextAlign.Left || value > TextAlign.Right)
					throw new ArgumentOutOfRangeException ("value");
				ViewState ["TextAlign"] = value;
			}
		}

		TableStyle TableStyle {
			get { return (TableStyle) ControlStyle; }
		}

		protected override Style CreateControlStyle ()
		{
			return new TableStyle (ViewState);
		}

		protected override Control FindControl (string id, int pathOffset)
		{
			// Always, or in just all my tests?
			return this;
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);

			// Register all of the checked controls so we can
			// find out when they are unchecked.
			for (int i = 0; i < Items.Count; i++) {
				if (Items [i].Selected) {
					check_box.ID = i.ToString (Helpers.InvariantCulture);
					Page.RegisterRequiresPostBack (check_box);
				}
			}
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void Render (HtmlTextWriter writer)
		{
#if NET_2_0
			if (Items.Count == 0)
				return;
#endif
			RepeatInfo ri = new RepeatInfo ();
			ri.RepeatColumns = RepeatColumns;
			ri.RepeatDirection = RepeatDirection;
			ri.RepeatLayout = RepeatLayout;

			short ti = 0;
			if (TabIndex != 0) {
				check_box.TabIndex = TabIndex;
				ti = TabIndex;
				TabIndex = 0;
			}
#if NET_2_0
			string ak = AccessKey;
			check_box.AccessKey = ak;
			this.AccessKey = null;
#endif

			ri.RenderRepeater (writer, this, TableStyle, this);

			if (ti != 0)
				TabIndex = ti;
#if NET_2_0
			this.AccessKey = ak;
#endif
		}

#if NET_2_0
		protected virtual
#endif
		bool LoadPostData (string postDataKey, NameValueCollection postCollection)
		{
			if (!IsEnabled)
				return false;
#if NET_2_0
			EnsureDataBound ();
#endif
			int checkbox = -1;

			try {
				string id = postDataKey.Substring (ClientID.Length + 1);
				if (Char.IsDigit (id [0]))
					checkbox = Int32.Parse (id, Helpers.InvariantCulture);
			} catch {
				return false;
			}

			if (checkbox == -1)
				return false;

			string val = postCollection [postDataKey];
			bool ischecked = val == "on";
			ListItem item = Items [checkbox];
#if NET_2_0
			if (item.Enabled)
#endif
				if (ischecked && !item.Selected) {
					item.Selected = true;
					return true;
				} else if (!ischecked && item.Selected) {
					item.Selected = false;
					return true;
				}

			return false;
		}

#if NET_2_0
		protected virtual
#endif
		void RaisePostDataChangedEvent ()
		{
#if NET_2_0
			if (CausesValidation)
				Page.Validate (ValidationGroup);
#endif
			OnSelectedIndexChanged (EventArgs.Empty);
		}

		bool IPostBackDataHandler.LoadPostData (string postDataKey,
							NameValueCollection postCollection)
		{
			return LoadPostData (postDataKey, postCollection);
		}

		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			RaisePostDataChangedEvent ();
		}

#if NET_2_0
		protected virtual
#endif
		bool HasFooter 
		{
			get {
				return false;
			}
		}

		bool IRepeatInfoUser.HasFooter {
			get { return HasFooter; }
		}

#if NET_2_0
		protected virtual
#endif
		bool HasHeader
		{
			get {
				return false;
			}
		}

		bool IRepeatInfoUser.HasHeader {
			get { return HasHeader; }
		}


#if NET_2_0
		protected virtual
#endif
		bool HasSeparators
		{
			get {
				return false;
			}
		}

		bool IRepeatInfoUser.HasSeparators {
			get { return HasSeparators; }
		}

#if NET_2_0
		protected virtual
#endif
		int RepeatedItemCount
		{
			get {
				return Items.Count;
			}
		}

		int IRepeatInfoUser.RepeatedItemCount {
			get { return RepeatedItemCount; }
		}

#if NET_2_0
		protected virtual
#endif
		Style GetItemStyle (ListItemType itemType,
				    int repeatIndex)
		{
			return null;
		}

		Style IRepeatInfoUser.GetItemStyle (ListItemType itemType,
						    int repeatIndex)
		{
			return GetItemStyle (itemType, repeatIndex);
		}

#if NET_2_0
		protected virtual
#endif
		void RenderItem (ListItemType itemType,
				 int repeatIndex,
				 RepeatInfo repeatInfo,
				 HtmlTextWriter writer)
		{
			ListItem item = Items [repeatIndex];

			check_box.ID = repeatIndex.ToString (Helpers.InvariantCulture);
			check_box.Text = item.Text;
			check_box.AutoPostBack = AutoPostBack;
			check_box.Checked = item.Selected;
			check_box.TextAlign = TextAlign;
			if (!IsEnabled)
				check_box.Enabled = false;
			else
				check_box.Enabled = item.Enabled;
#if NET_2_0
			check_box.ValidationGroup = ValidationGroup;
			check_box.CausesValidation = CausesValidation;
			if (check_box.HasAttributes)
				check_box.Attributes.Clear ();
			if (item.HasAttributes)
				check_box.Attributes.CopyFrom (item.Attributes);
#endif
			check_box.RenderControl (writer);
		}

		void IRepeatInfoUser.RenderItem (ListItemType itemType,
						 int repeatIndex, RepeatInfo repeatInfo,
						 HtmlTextWriter writer)
		{
			RenderItem (itemType, repeatIndex, repeatInfo, writer);
		}
#if NET_2_0
        protected internal override void VerifyMultiSelect()
        {
            //by default the ListControl will throw an exception in this method,
            //therefor we should override the method if the class is supporting
            //MultiSelect option.
        }
#endif
    }
}
